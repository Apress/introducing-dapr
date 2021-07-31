using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ObjectRecognition.DataStructures
{
    internal class ModelPrediction
    {
        // https://github.com/hunglc007/tensorflow-yolov4-tflite/blob/master/data/anchors/yolov4_anchors.txt
        static readonly float[][][] ANCHORS = new float[][][]
        {
            new float[][] { new float[] { 12, 16 }, new float[] { 19, 36 }, new float[] { 40, 28 } },
            new float[][] { new float[] { 36, 75 }, new float[] { 76, 55 }, new float[] { 72, 146 } },
            new float[][] { new float[] { 142, 110 }, new float[] { 192, 243 }, new float[] { 459, 401 } }
        };
        static readonly float[] STRIDES = new float[] { 8, 16, 32 };
        static readonly float[] XYSCALE = new float[] { 1.2f, 1.1f, 1.05f };
        static readonly int[] shapes = new int[] { 52, 26, 13 };

        const int anchorsCount = 3;

        [VectorType(1, 52, 52, 3, 85)]
        [ColumnName("Identity:0")]
        public float[] Identity { get; set; }

        [VectorType(1, 26, 26, 3, 85)]
        [ColumnName("Identity_1:0")]
        public float[] Identity1 { get; set; }

        [VectorType(1, 13, 13, 3, 85)]
        [ColumnName("Identity_2:0")]
        public float[] Identity2 { get; set; }

        [ColumnName("width")]
        public float ImageWidth { get; set; }

        [ColumnName("height")]
        public float ImageHeight { get; set; }

        public IReadOnlyList<ModelResult> ParseResults(string[] categories, float scoreThreshold = 0.5f, float iouThres = 0.5f)
        {
            List<float[]> postProcesssedResults = new List<float[]>();
            int classesCount = categories.Length;
            var results = new[] { Identity, Identity1, Identity2 };

            for (int i = 0; i < results.Length; i++)
            {
                var pred = results[i];
                var outputSize = shapes[i];

                for (int boxY = 0; boxY < outputSize; boxY++)
                {
                    for (int boxX = 0; boxX < outputSize; boxX++)
                    {
                        for (int a = 0; a < anchorsCount; a++)
                        {
                            var offset = (boxY * outputSize * (classesCount + 5) * anchorsCount) + (boxX * (classesCount + 5) * anchorsCount) + a * (classesCount + 5);
                            var predictedBoundingBox = pred.Skip(offset).Take(classesCount + 5).ToArray();

                            var predXywh = predictedBoundingBox.Take(4).ToArray();
                            var predConf = predictedBoundingBox[4];
                            var predProb = predictedBoundingBox.Skip(5).ToArray();

                            var rawDx = predXywh[0];
                            var rawDy = predXywh[1];
                            var rawDw = predXywh[2];
                            var rawDh = predXywh[3];

                            float predX = ((Sigmoid(rawDx) * XYSCALE[i]) - 0.5f * (XYSCALE[i] - 1) + boxX) * STRIDES[i];
                            float predY = ((Sigmoid(rawDy) * XYSCALE[i]) - 0.5f * (XYSCALE[i] - 1) + boxY) * STRIDES[i];
                            float predW = (float)Math.Exp(rawDw) * ANCHORS[i][a][0];
                            float predH = (float)Math.Exp(rawDh) * ANCHORS[i][a][1];

                            float predX1 = predX - predW * 0.5f;
                            float predY1 = predY - predH * 0.5f;
                            float predX2 = predX + predW * 0.5f;
                            float predY2 = predY + predH * 0.5f;

                            float org_h = ImageHeight;
                            float org_w = ImageWidth;

                            float inputSize = 416f;
                            float resizeRatio = Math.Min(inputSize / org_w, inputSize / org_h);
                            float dw = (inputSize - resizeRatio * org_w) / 2f;
                            float dh = (inputSize - resizeRatio * org_h) / 2f;

                            var orgX1 = 1f * (predX1 - dw) / resizeRatio; // left
                            var orgX2 = 1f * (predX2 - dw) / resizeRatio; // right
                            var orgY1 = 1f * (predY1 - dh) / resizeRatio; // top
                            var orgY2 = 1f * (predY2 - dh) / resizeRatio; // bottom

                            // clip some boxes that are out of range
                            orgX1 = Math.Max(orgX1, 0);
                            orgY1 = Math.Max(orgY1, 0);
                            orgX2 = Math.Min(orgX2, org_w - 1);
                            orgY2 = Math.Min(orgY2, org_h - 1);

                            if (orgX1 > orgX2 || orgY1 > orgY2) continue; // invalid_mask

                            // discard some of the boxes with low scores
                            var scores = predProb.Select(p => p * predConf).ToList();

                            float scoreMaxCat = scores.Max();
                            if (scoreMaxCat > scoreThreshold)
                            {
                                postProcesssedResults.Add(new float[] { orgX1, orgY1, orgX2, orgY2, scoreMaxCat, scores.IndexOf(scoreMaxCat) });
                            }
                        }
                    }
                }
            }

            // Apply Non-maximum Suppression to filter the results
            postProcesssedResults = postProcesssedResults.OrderByDescending(x => x[4]).ToList(); // sort by confidence
            List<ModelResult> resultsNms = new List<ModelResult>();
            int index = 0;
            while (index < postProcesssedResults.Count)
            {
                var res = postProcesssedResults[index];
                if (res == null)
                {
                    index++;
                    continue;
                }

                var confidence = res[4];
                var label = categories[(int)res[5]];

                resultsNms.Add(new ModelResult(res.Take(4).ToArray(), label, confidence));
                postProcesssedResults[index] = null;

                var iou = postProcesssedResults.Select(bbox => bbox == null ? float.NaN : BoxIoU(res, bbox)).ToList();
                for (int i = 0; i < iou.Count; i++)
                {
                    if (float.IsNaN(iou[i])) continue;
                    if (iou[i] > iouThres)
                    {
                        postProcesssedResults[i] = null;
                    }
                }
                index++;
            }

            return resultsNms;
        }

        private static float Sigmoid(float x)
        {
            return 1f / (1f + (float)Math.Exp(-x));
        }

        /// <summary>
        /// Return intersection-over-union (Jaccard index) of boxes.
        /// <para>Both sets of boxes are expected to be in (x1, y1, x2, y2) format.</para>
        /// </summary>
        private static float BoxIoU(float[] boxes1, float[] boxes2)
        {
            static float box_area(float[] box)
            {
                return (box[2] - box[0]) * (box[3] - box[1]);
            }

            var area1 = box_area(boxes1);
            var area2 = box_area(boxes2);

            Debug.Assert(area1 >= 0);
            Debug.Assert(area2 >= 0);

            var dx = Math.Max(0, Math.Min(boxes1[2], boxes2[2]) - Math.Max(boxes1[0], boxes2[0]));
            var dy = Math.Max(0, Math.Min(boxes1[3], boxes2[3]) - Math.Max(boxes1[1], boxes2[1]));
            var inter = dx * dy;

            return inter / (area1 + area2 - inter);
        }
    }
}
