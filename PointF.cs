using System;
#if NETSTANDARD1_0
namespace wContour
{
    /// <summary>
    /// A point in space (replaces System.Drawing.PointF)
    /// </summary>
    public struct PointF
    {
        /// <summary>
        /// The X-coordinate
        /// </summary>
        public float X { get; }

        /// <summary>
        /// The Y-coordinate
        /// </summary>
        public float Y { get; }

        /// <summary>
        /// Create a PointF
        /// </summary>
        public PointF(float x, float y)
        {
            X = x;
            Y = y;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(X)}: {X}, {nameof(Y)}: {Y}";
        }
    }
}
#endif