//******************************
// Copyright 2012 Yaqiang Wang,
// yaqiang.wang@gmail.com
//******************************

using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace wContour
{
    /// <summary>
    /// Border line
    /// </summary>
    public class BorderLine
    {
        /// <summary>
        /// Area
        /// </summary>
        public double area;
        /// <summary>
        /// Extent
        /// </summary>
        public Extent extent = new Extent();
        /// <summary>
        /// Is outline
        /// </summary>
        public bool isOutLine;
        /// <summary>
        /// Is clockwise
        /// </summary>
        public bool isClockwise;
        /// <summary>
        /// Point list
        /// </summary>
        public List<PointD> pointList = new List<PointD>();
        /// <summary>
        /// IJPoint list
        /// </summary>
        public List<IJPoint> ijPointList = new List<IJPoint>();
    }
}
