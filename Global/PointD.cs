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
    /// Point - double x/y
    /// </summary>
    public class PointD
    {
        /// <summary>
        /// x
        /// </summary>
        public double X;
        /// <summary>
        /// y
        /// </summary>
        public double Y;

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public PointD()
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        public PointD(double x, double y)
        {
            X = x;
            Y = y;
        }

        #endregion

        #region Methods
        /// <summary>
        /// Clone
        /// </summary>
        /// <returns>Cloned object</returns>
        public object Clone()
        {
            return new PointD(X, Y);
        }

        #endregion
    }
}
