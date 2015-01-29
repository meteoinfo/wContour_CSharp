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
    /// Polyline
    /// </summary>
    public class PolyLine
    {
        /// <summary>
        /// Value
        /// </summary>
        public double Value;
        /// <summary>
        /// Type
        /// </summary>
        public string Type;
        /// <summary>
        /// Border index
        /// </summary>
        public int BorderIdx;
        /// <summary>
        /// Point list
        /// </summary>
        public List<PointD> PointList;        
    }
}
