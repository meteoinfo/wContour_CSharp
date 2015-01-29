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
    /// End point
    /// </summary>
    public class EndPoint
    {
        /// <summary>
        /// Start point
        /// </summary>
        public PointD sPoint = new PointD();
        /// <summary>
        /// Point
        /// </summary>
        public PointD Point = new PointD();
        /// <summary>
        /// Index
        /// </summary>
        public int Index;
        /// <summary>
        /// Border Index
        /// </summary>
        public int BorderIdx;

        /// <summary>
        /// Constructor
        /// </summary>
        public EndPoint()
        {

        }
    }
}
