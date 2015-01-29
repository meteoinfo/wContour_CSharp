//******************************
// Copyright 2012 Yaqiang Wang,
// yaqiang.wang@gmail.com
//******************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wContour
{
    /// <summary>
    /// Extent
    /// </summary>
    public class Extent
    {
        /// <summary>
        /// x minimum
        /// </summary>
        public double xMin;
        /// <summary>
        /// y minimum
        /// </summary>
        public double yMin;
        /// <summary>
        /// x maximum
        /// </summary>
        public double xMax;
        /// <summary>
        /// y maximum
        /// </summary>
        public double yMax;

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public Extent()
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="minX">minimum x</param>
        /// <param name="maxX">maximum x</param>
        /// <param name="minY">minimum y</param>
        /// <param name="maxY">maximum y</param>
        public Extent(double minX, double maxX, double minY, double maxY)
        {
            xMin = minX;
            xMax = maxX;
            yMin = minY;
            yMax = maxY;
        }

        #endregion

        /// <summary>
        /// Judge if this extent include another extent
        /// </summary>
        /// <param name="bExtent">extent</param>
        /// <returns>is included</returns>
        public bool Include(Extent bExtent)
        {
            if (xMin <= bExtent.xMin && xMax >= bExtent.xMax && yMin <= bExtent.yMin && yMax >= bExtent.yMax)
                return true;
            else
                return false;
        }
    }
}
