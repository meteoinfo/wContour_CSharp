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
    /// Border
    /// </summary>
    public class Border
    {
        #region Variables
        /// <summary>
        /// Line list
        /// </summary>
        public List<BorderLine> LineList;

        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public Border()
        {
            LineList = new List<BorderLine>();
        }

        #endregion

        #region Properties
        /// <summary>
        /// Get line number
        /// </summary>
        public int LineNum
        {
            get { return LineList.Count; }
        }

        #endregion
    }
}
