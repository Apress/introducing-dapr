using Microsoft.ML;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using ObjectRecognition.DataStructures;
using static Microsoft.ML.Transforms.Image.ImageResizingEstimator;
using System;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace ObjectRecognition
{
    public class ModelEvaluator
    {
        private string modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Model\yolov4.onnx");
        static readonly string[] classesNames = new string[] { "person", "bicycle", "car", "motorbike", "aeroplane", "bus",
            "train", "truck", "boat", "traffic light", "fire hydrant", "stop sign", "parking meter", "bench", "bird", "cat",
            "dog", "horse", "sheep", "cow", "elephant", "bear", "zebra", "giraffe", "backpack", "umbrella", "handbag", "tie",
            "suitcase", "frisbee", "skis", "snowboard", "sports ball", "kite", "baseball bat", "baseball glove", "skateboard",
            "surfboard", "tennis racket", "bottle", "wine glass", "cup", "fork", "knife", "spoon", "bowl", "banana", "apple",
            "sandwich", "orange", "broccoli", "carrot", "hot dog", "pizza", "donut", "cake", "chair", "sofa", "pottedplant",
            "bed", "diningtable", "toilet", "tvmonitor", "laptop", "mouse", "remote", "keyboard", "cell phone", "microwave",
            "oven", "toaster", "sink", "refrigerator", "book", "clock", "vase", "scissors", "teddy bear", "hair drier",
            "toothbrush" };

        private Dictionary<string, Color> colorCache = new Dictionary<string, Color>();

        public IReadOnlyList<ModelResult> Evaluate(string imagePath, string taggedImagePath)
        {
            MLContext mlContext = new MLContext();

            var pipeline = mlContext.Transforms.ResizeImages(inputColumnName: "bitmap", outputColumnName: "input_1:0", imageWidth: 416, imageHeight: 416, resizing: ResizingKind.IsoPad)
                .Append(mlContext.Transforms.ExtractPixels(outputColumnName: "input_1:0", scaleImage: 1f / 255f, interleavePixelColors: true))
                .Append(mlContext.Transforms.ApplyOnnxModel(
                    shapeDictionary: new Dictionary<string, int[]>()
                    {
                        { "input_1:0", new[] { 1, 416, 416, 3 } },
                        { "Identity:0", new[] { 1, 52, 52, 3, 85 } },
                        { "Identity_1:0", new[] { 1, 26, 26, 3, 85 } },
                        { "Identity_2:0", new[] { 1, 13, 13, 3, 85 } },
                    },
                    inputColumnNames: new[]
                    {
                        "input_1:0"
                    },
                    outputColumnNames: new[]
                    {
                        "Identity:0",
                        "Identity_1:0",
                        "Identity_2:0"
                    },
                    modelFile: modelPath));

            using (var model = pipeline.Fit(mlContext.Data.LoadFromEnumerable(new List<ImageData>())))
            using (var predictionEngine = mlContext.Model.CreatePredictionEngine<ImageData, ModelPrediction>(model))
            using (var image = Image.FromFile(imagePath))
            using (var bitmap = new Bitmap(image))
            {
                var predict = predictionEngine.Predict(new ImageData() { Image = bitmap });
                var results = predict.ParseResults(classesNames, 0.3f, 0.7f);

                using (var g = Graphics.FromImage(bitmap))
                {
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    foreach (var res in results)
                    {
                        if (!colorCache.ContainsKey(res.Label))
                        {
                            Random random = new Random();
                            KnownColor[] names = (KnownColor[])Enum.GetValues(typeof(KnownColor));
                            KnownColor randomColorName = names[random.Next(names.Length)];
                            Color randomColor = Color.FromKnownColor(randomColorName);
                            colorCache.Add(res.Label, randomColor);
                        }

                        // draw predictions
                        var x1 = res.BBox[0];
                        var y1 = res.BBox[1];
                        var x2 = res.BBox[2];
                        var y2 = res.BBox[3];
                        g.DrawRectangle(new Pen(colorCache[res.Label]), x1, y1, x2 - x1, y2 - y1);

                        using (var brushes = new SolidBrush(Color.FromArgb(65, colorCache[res.Label])))
                        {
                            g.FillRectangle(brushes, x1, y1, x2 - x1, y2 - y1);
                        }

                        g.DrawString(res.Label + " " + res.Confidence.ToString("0.00"),
                                     new Font("Arial", 12), Brushes.White, new PointF(x1, y1));
                    }
                    bitmap.Save(taggedImagePath, ImageFormat.Jpeg);
                }

                return results;
            }
        }
    }
}
