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
    /// Polygon
    /// </summary>
    public class Polygon
    {
        #region Variables
        /// <summary>
        /// If is border contour polygon
        /// </summary>
        public bool IsBorder;
        /// <summary>
        /// If there is only inner border
        /// </summary>
        public bool IsInnerBorder = false;
        /// <summary>
        /// Start value
        /// </summary>
        public double LowValue;
        /// <summary>
        /// End value
        /// </summary>
        public double HighValue;
        /// <summary>
        /// If clockwise
        /// </summary>
        public bool IsClockWise;
        /// <summary>
        /// Start point index
        /// </summary>
        public int StartPointIdx;
        /// <summary>
        /// If high center
        /// </summary>
        public bool IsHighCenter;
        /// <summary>
        /// Extent - bordering rectangle
        /// </summary>
        public Extent Extent = new Extent();
        /// <summary>
        /// Area
        /// </summary>
        public double Area;
        /// <summary>
        /// Outline
        /// </summary>
        public PolyLine OutLine = new PolyLine();
        /// <summary>
        /// Hole lines
        /// </summary>
        public List<PolyLine> HoleLines = new List<PolyLine>();
        /// <summary>
        /// Hole index
        /// </summary>
        public int HoleIndex;

        #endregion


        #region Methods
        /// <summary>
        /// Clone
        /// </summary>
        /// <returns>Cloned polygon</returns>
        public object Clone()
        {
            Polygon aPolygon = new Polygon();
            aPolygon.IsBorder = IsBorder;
            aPolygon.LowValue = LowValue;
            aPolygon.HighValue = HighValue;
            aPolygon.IsClockWise = IsClockWise;
            aPolygon.StartPointIdx = StartPointIdx;
            aPolygon.IsHighCenter = IsHighCenter;
            aPolygon.Extent = Extent;
            aPolygon.Area = Area;
            aPolygon.OutLine = OutLine;
            aPolygon.HoleLines = new List<PolyLine>(HoleLines);
            aPolygon.HoleIndex = HoleIndex;

            return aPolygon;
        }

        /// <summary>
        /// Get if has holes
        /// </summary>
        public bool HasHoles
        {
            get { return (HoleLines.Count > 0); }
        }

        /// <summary>
        /// Add a hole by a polygon
        /// </summary>
        /// <param name="aPolygon">polygon</param>
        public void AddHole(Polygon aPolygon)
        {
            HoleLines.Add(aPolygon.OutLine);
        }

        /// <summary>
        /// Add a hole by point list
        /// </summary>
        /// <param name="pList">point list</param>
        public void AddHole(List<PointD> pList)
        {
            if (Contour.IsClockwise(pList))
                pList.Reverse();

            PolyLine aLine = new PolyLine();
            aLine.PointList = pList;
            HoleLines.Add(aLine);
        }

        #endregion
    }
}
