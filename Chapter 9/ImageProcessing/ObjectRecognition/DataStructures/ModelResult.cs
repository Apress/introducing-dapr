namespace ObjectRecognition.DataStructures
{
    public class ModelResult
    {
        /// <summary>
        /// x1, y1, x2, y2 coordinates.
        /// </summary>
        public float[] BBox { get; }

        /// <summary>
        /// The category of the bounding box.
        /// </summary>
        public string Label { get; }

        /// <summary>
        /// Confidence level.
        /// </summary>
        public float Confidence { get; }

        public ModelResult(float[] bbox, string label, float confidence)
        {
            BBox = bbox;
            Label = label;
            Confidence = confidence;
        }
    }
}
