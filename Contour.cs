//******************************
// Copyright 2012 Yaqiang Wang,
// yaqiang.wang@gmail.com
//******************************

using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Drawing;

namespace wContour
{
    /// <summary>
    /// Contour
    /// </summary>
    public static class Contour
    {
        private static List<EndPoint> _endPointList = new List<EndPoint>();

        #region Public Contour Methods

        /// <summary>
        /// Tracing contour lines from the grid data with undefine data
        /// </summary>
        /// <param name="S0">input grid data</param>
        /// <param name="X">X coordinate array</param>
        /// <param name="Y">Y coordinate array</param>
        /// <param name="nc">number of contour values</param>
        /// <param name="contour">contour value array</param>
        /// <param name="undefData">Undefine data</param>
        /// <param name="borders">borders</param>
        /// <param name="S1">data flag array</param>
        /// <returns>Contour line list</returns>
        public static List<PolyLine> TracingContourLines(double[,] S0, double[] X, double[] Y,
            int nc, double[] contour, double undefData, List<Border> borders, int[,] S1)
        {            
            double dx = X[1] - X[0];
            double dy = Y[1] - Y[0];
            List<PolyLine> contourLines = CreateContourLines_UndefData(S0, X, Y, nc, contour, dx, dy, S1, undefData, borders);

            return contourLines;
        }

        /// <summary>
        /// Tracing contour borders of the grid data with undefined data.
        /// Grid data are from left to right and from bottom to top.
        /// Grid data array: first dimention is Y, second dimention is X.
        /// </summary>
        /// <param name="S0">grid data</param>
        /// <param name="X">x coordinate</param>
        /// <param name="Y">y coordinate</param>
        /// <param name="S1"></param>
        /// <param name="undefData">undefine data</param>
        /// <returns>borderline list</returns>
        public static List<Border> TracingBorders(double[,] S0, double[] X, double[] Y, ref int[,] S1, double undefData)
        {
            List<BorderLine> borderLines = new List<BorderLine>();

            int m, n, i, j;
            m = S0.GetLength(0);    //Y
            n = S0.GetLength(1);    //X

            S1 = new int[m, n];    //---- New array (0 with undefine data, 1 with data)
            for (i = 0; i < m; i++)
            {
                for (j = 0; j < n; j++)
                {
                    if (DoubleEquals(S0[i, j], undefData))    //Undefine data
                        S1[i, j] = 0;
                    else
                        S1[i, j] = 1;
                }
            }

            //---- Border points are 1, undefine points are 0, inside data points are 2
            //l - Left; r - Right; b - Bottom; t - Top; lb - LeftBottom; rb - RightBottom; lt - LeftTop; rt - RightTop
            int l, r, b, t, lb, rb, lt, rt;
            for (i = 1; i < m - 1; i++)
            {
                for (j = 1; j < n - 1; j++)
                {
                    if (S1[i, j] == 1)    //data point
                    {
                        l = S1[i, j - 1];
                        r = S1[i, j + 1];
                        b = S1[i - 1, j];
                        t = S1[i + 1, j];
                        lb = S1[i - 1, j - 1];
                        rb = S1[i - 1, j + 1];
                        lt = S1[i + 1, j - 1];
                        rt = S1[i + 1, j + 1];

                        if (l > 0 && r > 0 && b > 0 && t > 0 && lb > 0 && rb > 0 && lt > 0 && rt > 0)
                            S1[i, j] = 2;    //Inside data point

                        if (l + r + b + t + lb + rb + lt + rt <= 2)
                            S1[i, j] = 0;    //Data point, but not more than 3 continued data points together.
                        //So they can't be traced as a border (at least 4 points together).

                    }
                }
            }

            //---- Remove isolated data points (up, down, left and right points are all undefine data).
            bool isContinue = false;
            while (true)
            {
                isContinue = false;
                for (i = 1; i < m - 1; i++)
                {
                    for (j = 1; j < n - 1; j++)
                    {
                        if (S1[i, j] == 1)    //data point
                        {
                            l = S1[i, j - 1];
                            r = S1[i, j + 1];
                            b = S1[i - 1, j];
                            t = S1[i + 1, j];
                            lb = S1[i - 1, j - 1];
                            rb = S1[i - 1, j + 1];
                            lt = S1[i + 1, j - 1];
                            rt = S1[i + 1, j + 1];
                            if ((l == 0 && r == 0) || (b == 0 && t == 0))    //Up, down, left and right points are all undefine data
                            {
                                S1[i, j] = 0;
                                isContinue = true;
                            }
                            if ((lt == 0 && r == 0 && b == 0) || (rt == 0 && l == 0 && b == 0) ||
                                (lb == 0 && r == 0 && t == 0) || (rb == 0 && l == 0 && t == 0))
                            {
                                S1[i, j] = 0;
                                isContinue = true;
                            }
                        }
                    }
                }
                if (!isContinue)    //untile no more isolated data point.
                    break;
            }
            //Deal with grid data border points
            for (j = 0; j < n; j++)    //Top and bottom border points
            {
                if (S1[0, j] == 1)
                {
                    if (S1[1, j] == 0)    //up point is undefine
                        S1[0, j] = 0;
                    else
                    {
                        if (j == 0)
                        {
                            if (S1[0, j + 1] == 0)
                                S1[0, j] = 0;
                        }
                        else if (j == n - 1)
                        {
                            if (S1[0, n - 2] == 0)
                                S1[0, j] = 0;
                        }
                        else
                        {
                            if (S1[0, j - 1] == 0 && S1[0, j + 1] == 0)
                                S1[0, j] = 0;
                        }
                    }
                }
                if (S1[m - 1, j] == 1)
                {
                    if (S1[m - 2, j] == 0)    //down point is undefine
                        S1[m - 1, j] = 0;
                    else
                    {
                        if (j == 0)
                        {
                            if (S1[m - 1, j + 1] == 0)
                                S1[m - 1, j] = 0;
                        }
                        else if (j == n - 1)
                        {
                            if (S1[m - 1, n - 2] == 0)
                                S1[m - 1, j] = 0;
                        }
                        else
                        {
                            if (S1[m - 1, j - 1] == 0 && S1[m - 1, j + 1] == 0)
                                S1[m - 1, j] = 0;
                        }
                    }
                }
            }
            for (i = 0; i < m; i++)    //Left and right border points
            {
                if (S1[i, 0] == 1)
                {
                    if (S1[i, 1] == 0)    //right point is undefine
                        S1[i, 0] = 0;
                    else
                    {
                        if (i == 0)
                        {
                            if (S1[i + 1, 0] == 0)
                                S1[i, 0] = 0;
                        }
                        else if (i == m - 1)
                        {
                            if (S1[m - 2, 0] == 0)
                                S1[i, 0] = 0;
                        }
                        else
                        {
                            if (S1[i - 1, 0] == 0 && S1[i + 1, 0] == 0)
                                S1[i, 0] = 0;
                        }
                    }
                }
                if (S1[i, n - 1] == 1)
                {
                    if (S1[i, n - 2] == 0)    //left point is undefine
                        S1[i, n - 1] = 0;
                    else
                    {
                        if (i == 0)
                        {
                            if (S1[i + 1, n - 1] == 0)
                                S1[i, n - 1] = 0;
                        }
                        else if (i == m - 1)
                        {
                            if (S1[m - 2, n - 1] == 0)
                                S1[i, n - 1] = 0;
                        }
                        else
                        {
                            if (S1[i - 1, n - 1] == 0 && S1[i + 1, n - 1] == 0)
                                S1[i, n - 1] = 0;
                        }
                    }
                }
            }

            //---- Generate S2 array from S1, add border to S2 with undefine data.
            int[,] S2 = new int[m + 2, n + 2];
            for (i = 0; i < m + 2; i++)
            {
                for (j = 0; j < n + 2; j++)
                {
                    if (i == 0 || i == m + 1)    //bottom or top border
                        S2[i, j] = 0;
                    else if (j == 0 || j == n + 1)    //left or right border
                        S2[i, j] = 0;
                    else
                        S2[i, j] = S1[i - 1, j - 1];
                }
            }

            //---- Using times number of each point during chacing process.
            int[,] UNum = new int[m + 2, n + 2];
            for (i = 0; i < m + 2; i++)
            {
                for (j = 0; j < n + 2; j++)
                {
                    if (S2[i, j] == 1)
                    {
                        l = S2[i, j - 1];
                        r = S2[i, j + 1];
                        b = S2[i - 1, j];
                        t = S2[i + 1, j];
                        lb = S2[i - 1, j - 1];
                        rb = S2[i - 1, j + 1];
                        lt = S2[i + 1, j - 1];
                        rt = S2[i + 1, j + 1];
                        //---- Cross point with two boder lines, will be used twice.
                        if (l == 1 && r == 1 && b == 1 && t == 1 && ((lb == 0 && rt == 0) || (rb == 0 && lt == 0)))
                            UNum[i, j] = 2;
                        else
                            UNum[i, j] = 1;
                    }
                    else
                        UNum[i, j] = 0;
                }
            }

            //---- Tracing borderlines
            PointD aPoint;
            IJPoint aijPoint;
            BorderLine aBLine = new BorderLine();
            List<PointD> pointList = new List<PointD>();
            List<IJPoint> ijPList = new List<IJPoint>();
            int sI, sJ, i1, j1, i2, j2, i3 = 0, j3 = 0;
            for (i = 1; i < m + 1; i++)
            {
                for (j = 1; j < n + 1; j++)
                {
                    if (S2[i, j] == 1)    //Tracing border from any border point
                    {
                        pointList = new List<PointD>();
                        ijPList = new List<IJPoint>();
                        aPoint = new PointD();
                        aPoint.X = X[j - 1];
                        aPoint.Y = Y[i - 1];
                        aijPoint = new IJPoint();
                        aijPoint.I = i - 1;
                        aijPoint.J = j - 1;
                        pointList.Add(aPoint);
                        ijPList.Add(aijPoint);
                        sI = i;
                        sJ = j;
                        i2 = i;
                        j2 = j;
                        i1 = i2;
                        j1 = -1;    //Trace from left firstly                        

                        while (true)
                        {
                            if (TraceBorder(S2, i1, i2, j1, j2, ref i3, ref j3))
                            {
                                i1 = i2;
                                j1 = j2;
                                i2 = i3;
                                j2 = j3;
                                UNum[i3, j3] = UNum[i3, j3] - 1;
                                if (UNum[i3, j3] == 0)
                                    S2[i3, j3] = 3;    //Used border point
                            }
                            else
                                break;

                            aPoint = new PointD();
                            aPoint.X = X[j3 - 1];
                            aPoint.Y = Y[i3 - 1];
                            aijPoint = new IJPoint();
                            aijPoint.I = i3 - 1;
                            aijPoint.J = j3 - 1;
                            pointList.Add(aPoint);
                            ijPList.Add(aijPoint);
                            if (i3 == sI && j3 == sJ)
                                break;
                        }
                        UNum[i, j] = UNum[i, j] - 1;
                        if (UNum[i, j] == 0)
                            S2[i, j] = 3;    //Used border point
                        //UNum[i, j] = UNum[i, j] - 1;
                        if (pointList.Count > 1)
                        {
                            aBLine = new BorderLine();
                            aBLine.area = GetExtentAndArea(pointList, ref aBLine.extent);
                            aBLine.isOutLine = true;
                            aBLine.isClockwise = true;
                            aBLine.pointList = pointList;
                            aBLine.ijPointList = ijPList;
                            borderLines.Add(aBLine);
                        }
                    }
                }
            }

            //---- Form borders
            List<Border> borders = new List<Border>();
            Border aBorder = new Border();
            BorderLine aLine, bLine;
            //---- Sort borderlines with area from small to big.
            //For inside border line analysis
            for (i = 1; i < borderLines.Count; i++)    
            {
                aLine = (BorderLine)borderLines[i];
                for (j = 0; j < i; j++)
                {
                    bLine = (BorderLine)borderLines[j];
                    if (aLine.area > bLine.area)
                    {
                        borderLines.RemoveAt(i);
                        borderLines.Insert(j, aLine);
                        break;
                    }
                }
            }
            List<BorderLine> lineList;
            if (borderLines.Count == 1)    //Only one boder line
            {
                aLine = (BorderLine)borderLines[0];
                if (!IsClockwise(aLine.pointList))
                {
                    aLine.pointList.Reverse();
                    aLine.ijPointList.Reverse();
                }
                aLine.isClockwise = true;
                lineList = new List<BorderLine>();
                lineList.Add(aLine);
                aBorder = new Border();
                aBorder.LineList = lineList;
                borders.Add(aBorder);
            }
            else    //muti border lines
            {
                for (i = 0; i < borderLines.Count; i++)
                {
                    if (i == borderLines.Count)
                        break;

                    aLine = borderLines[i];
                    if (!IsClockwise(aLine.pointList))
                    {
                        aLine.pointList.Reverse();
                        aLine.ijPointList.Reverse();
                    }
                    aLine.isClockwise = true;
                    lineList = new List<BorderLine>();
                    lineList.Add(aLine);
                    //Try to find the boder lines are inside of aLine.
                    for (j = i + 1; j < borderLines.Count; j++)
                    {
                        if (j == borderLines.Count)
                            break;

                        bLine = borderLines[j];
                        if (bLine.extent.xMin > aLine.extent.xMin && bLine.extent.xMax < aLine.extent.xMax &&
                          bLine.extent.yMin > aLine.extent.yMin && bLine.extent.yMax < aLine.extent.yMax)
                        {
                            aPoint = bLine.pointList[0];
                            if (PointInPolygon(aLine.pointList, aPoint))    //bLine is inside of aLine
                            {                                
                                bLine.isOutLine = false;
                                if (IsClockwise(bLine.pointList))
                                {
                                    pointList.Reverse();
                                    bLine.ijPointList.Reverse();
                                }
                                bLine.isClockwise = false;
                                lineList.Add(bLine);
                                borderLines.RemoveAt(j);
                                j = j - 1;
                            }
                        }
                    }
                    aBorder = new Border();
                    aBorder.LineList = lineList;
                    borders.Add(aBorder);
                }
            }

            return borders;
        }        

        /// <summary>
        /// Create contour lines from the grid data with undefine data
        /// </summary>
        /// <param name="S0">input grid data</param>
        /// <param name="X">X coordinate array</param>
        /// <param name="Y">Y coordinate array</param>
        /// <param name="nc">number of contour values</param>
        /// <param name="contour">contour value array</param>
        /// <param name="nx">interval of X coordinate</param>
        /// <param name="ny">interval of Y coordinate</param>
        /// <param name="S1"></param>
        /// <param name="undefData">Undefine data</param>
        /// <param name="borders">Border line list</param>
        /// <returns>Contour line list</returns>
        private static List<PolyLine> CreateContourLines_UndefData(double[,] S0, double[] X, double[] Y,
            int nc, double[] contour, double nx, double ny, int[,] S1, double undefData, List<Border> borders)
        {
            List<PolyLine> contourLineList = new List<PolyLine>();
            List<PolyLine> cLineList = new List<PolyLine>();
            int m, n, i, j;
            m = S0.GetLength(0);    //---- Y
            n = S0.GetLength(1);    //---- X

            //---- Add a small value to aviod the contour point as same as data point
            double dShift;
            dShift = getAbsMinValue(contour) * 0.0000001;
            //dShift = contour[0] * 0.0000001;
            if (dShift == 0)
                dShift = 0.0000001;
            for (i = 0; i < m; i++)
            {
                for (j = 0; j < n; j++)
                {
                    if (!(DoubleEquals(S0[i, j], undefData)))
                        //S0[i, j] = S0[i, j] + (contour[1] - contour[0]) * 0.0001;
                        S0[i, j] = S0[i, j] + dShift;
                }
            }

            //---- Define if H S are border
            int[, ,] SB = new int[2, m, n - 1], HB = new int[2, m - 1, n];   //---- Which border and trace direction
            for (i = 0; i < m; i++)
            {
                for (j = 0; j < n; j++)
                {
                    if (j < n - 1)
                    {
                        SB[0, i, j] = -1;
                        SB[1, i, j] = -1;
                    }
                    if (i < m - 1)
                    {
                        HB[0, i, j] = -1;
                        HB[1, i, j] = -1;
                    }
                }
            }
            Border aBorder;
            BorderLine aBLine;
            List<IJPoint> ijPList = new List<IJPoint>();
            int k, si, sj;
            IJPoint aijP, bijP;
            for (i = 0; i < borders.Count; i++)
            {
                aBorder = borders[i];
                for (j = 0; j < aBorder.LineNum; j++)
                {
                    aBLine = aBorder.LineList[j];
                    ijPList = aBLine.ijPointList;
                    for (k = 0; k < ijPList.Count - 1; k++)
                    {
                        aijP = ijPList[k];
                        bijP = ijPList[k + 1];
                        if (aijP.I == bijP.I)
                        {
                            si = aijP.I;
                            sj = Math.Min(aijP.J, bijP.J);
                            SB[0, si, sj] = i;
                            if (bijP.J > aijP.J)    //---- Trace from top
                                SB[1, si, sj] = 1;
                            else
                                SB[1, si, sj] = 0;    //----- Trace from bottom
                        }
                        else
                        {
                            sj = aijP.J;
                            si = Math.Min(aijP.I, bijP.I);
                            HB[0, si, sj] = i;
                            if (bijP.I > aijP.I)    //---- Trace from left
                                HB[1, si, sj] = 0;
                            else
                                HB[1, si, sj] = 1;    //---- Trace from right

                        }
                    }
                }
            }

            //---- Define horizontal and vertical arrays with the position of the tracing value, -2 means no tracing point. 
            double[,] S = new double[m, n - 1], H = new double[m - 1, n];
            double w;    //---- Tracing value
            int c;
            //ArrayList _endPointList = new ArrayList();    //---- Contour line end points for insert to border
            for (c = 0; c < nc; c++)
            {
                w = contour[c];
                for (i = 0; i < m; i++)
                {
                    for (j = 0; j < n; j++)
                    {
                        if (j < n - 1)
                        {
                            if (S1[i, j] != 0 && S1[i, j + 1] != 0)
                            {
                                if ((S0[i, j] - w) * (S0[i, j + 1] - w) < 0)    //---- Has tracing value
                                    S[i, j] = (w - S0[i, j]) / (S0[i, j + 1] - S0[i, j]);
                                else
                                    S[i, j] = -2;
                            }
                            else
                                S[i, j] = -2;
                        }
                        if (i < m - 1)
                        {
                            if (S1[i, j] != 0 && S1[i + 1, j] != 0)
                            {
                                if ((S0[i, j] - w) * (S0[i + 1, j] - w) < 0)    //---- Has tracing value
                                    H[i, j] = (w - S0[i, j]) / (S0[i + 1, j] - S0[i, j]);
                                else
                                    H[i, j] = -2;
                            }
                            else
                                H[i, j] = -2;
                        }
                    }
                }

                cLineList = Isoline_UndefData(S0, X, Y, w, nx, ny, ref S, ref H, SB, HB, contourLineList.Count);
                contourLineList.AddRange(cLineList);
            }

            //---- Set border index for close contours
            PolyLine aLine;
            //ArrayList pList = new ArrayList();
            PointD aPoint;
            for (i = 0; i < borders.Count; i++)
            {
                aBorder = borders[i];
                aBLine = aBorder.LineList[0];
                for (j = 0; j < contourLineList.Count; j++)
                {
                    aLine = contourLineList[j];
                    if (aLine.Type == "Close")
                    {
                        aPoint = aLine.PointList[0];
                        if (PointInPolygon(aBLine.pointList, aPoint))
                            aLine.BorderIdx = i;
                    }
                    contourLineList.RemoveAt(j);
                    contourLineList.Insert(j, aLine);
                }
            }

            return contourLineList;
        }       

        /// <summary>
        /// Create contour lines
        /// </summary>
        /// <param name="S0">input grid data array</param>
        /// <param name="X">X coordinate array</param>
        /// <param name="Y">Y coordinate array</param>
        /// <param name="nc">number of contour values</param>
        /// <param name="contour">contour value array</param>
        /// <param name="nx">Interval of X coordinate</param>
        /// <param name="ny">Interval of Y coordinate</param>
        /// <returns></returns>
        private static List<PolyLine> CreateContourLines(double[,] S0, double[] X, double[] Y, int nc, double[] contour, double nx, double ny)
        {
            List<PolyLine> contourLineList = new List<PolyLine>(), bLineList = new List<PolyLine>(), lLineList = new List<PolyLine>(),
                tLineList = new List<PolyLine>(), rLineList = new List<PolyLine>(), cLineList = new List<PolyLine>();
            int m, n, i, j;
            m = S0.GetLength(0);    //---- Y
            n = S0.GetLength(1);    //---- X

            //---- Define horizontal and vertical arrays with the position of the tracing value, -2 means no tracing point. 
            double[,] S = new double[m, n - 1], H = new double[m - 1, n];
            double dShift;
            dShift = contour[0] * 0.00001;
            if (dShift == 0)
                dShift = 0.00001;
            for (i = 0; i < m; i++)
            {
                for (j = 0; j < n; j++)
                    S0[i, j] = S0[i, j] + dShift;
            }

            double w;    //---- Tracing value
            int c;
            for (c = 0; c < nc; c++)
            {
                w = contour[c];
                for (i = 0; i < m; i++)
                {
                    for (j = 0; j < n; j++)
                    {
                        if (j < n - 1)
                        {
                            if ((S0[i, j] - w) * (S0[i, j + 1] - w) < 0)    //---- Has tracing value
                                S[i, j] = (w - S0[i, j]) / (S0[i, j + 1] - S0[i, j]);
                            else
                                S[i, j] = -2;
                        }
                        if (i < m - 1)
                        {
                            if ((S0[i, j] - w) * (S0[i + 1, j] - w) < 0)    //---- Has tracing value
                                H[i, j] = (w - S0[i, j]) / (S0[i + 1, j] - S0[i, j]);
                            else
                                H[i, j] = -2;
                        }
                    }
                }

                bLineList = Isoline_Bottom(S0, X, Y, w, nx, ny, ref S, ref H);
                lLineList = Isoline_Left(S0, X, Y, w, nx, ny, ref S, ref H);
                tLineList = Isoline_Top(S0, X, Y, w, nx, ny, ref S, ref H);
                rLineList = Isoline_Right(S0, X, Y, w, nx, ny, ref S, ref H);
                cLineList = Isoline_Close(S0, X, Y, w, nx, ny, ref S, ref H);
                contourLineList.AddRange(bLineList);
                contourLineList.AddRange(lLineList);
                contourLineList.AddRange(tLineList);
                contourLineList.AddRange(rLineList);
                contourLineList.AddRange(cLineList);
            }

            return contourLineList;
        }

        /// <summary>
        /// Cut contour lines with a polygon. Return the polylines inside of the polygon
        /// </summary>
        /// <param name="alinelist">polyline list</param>
        /// <param name="polyList">border points of the cut polygon</param>
        /// <returns>Inside Polylines after cut</returns>
        private static List<PolyLine> CutContourWithPolygon(List<PolyLine> alinelist, List<PointD> polyList)
        {
            List<PolyLine> newLineList = new List<PolyLine>();
            int i, j, k;
            PolyLine aLine = new PolyLine(), bLine = new PolyLine();
            List<PointD> aPList = new List<PointD>();
            double aValue;
            string aType;
            bool ifInPolygon;
            PointD q1, q2, p1, p2, IPoint;
            Line lineA, lineB;
            EndPoint aEndPoint = new EndPoint();

            _endPointList = new List<EndPoint>();
            if (!IsClockwise(polyList))    //---- Make cut polygon clockwise
                polyList.Reverse();

            for (i = 0; i < alinelist.Count; i++)
            {
                aLine = alinelist[i];
                aValue = aLine.Value;
                aType = aLine.Type;                
                aPList = new List<PointD>(aLine.PointList);
                ifInPolygon = false;
                List<PointD> newPlist = new List<PointD>();
                //---- For "Close" type contour,the start point must be outside of the cut polygon.
                if (aType == "Close" && PointInPolygon(polyList, aPList[0]))
                {
                    bool isAllIn = true;
                    int notInIdx = 0;
                    for (j = 0; j < aPList.Count; j++)
                    {
                        if (!PointInPolygon(polyList, aPList[j]))
                        {
                            notInIdx = j;
                            isAllIn = false;
                            break;
                        }
                    }
                    if (!isAllIn)
                    {
                        List<PointD> bPList = new List<PointD>();
                        for (j = notInIdx; j < aPList.Count; j++)
                            bPList.Add(aPList[j]);

                        for (j = 1; j < notInIdx; j++)
                            bPList.Add(aPList[j]);

                        bPList.Add(bPList[0]);
                        aPList = bPList;
                    }
                }
                p1 = new PointD();
                for (j = 0; j < aPList.Count; j++)
                {
                    p2 = aPList[j];
                    if (PointInPolygon(polyList, p2))
                    {
                        if (!ifInPolygon && j > 0)
                        {
                            lineA = new Line();
                            lineA.P1 = p1;
                            lineA.P2 = p2;
                            q1 = polyList[polyList.Count - 1];
                            IPoint = new PointD();
                            for (k = 0; k < polyList.Count; k++)
                            {
                                q2 = polyList[k];
                                lineB = new Line();
                                lineB.P1 = q1;
                                lineB.P2 = q2;
                                if (IsLineSegmentCross(lineA, lineB))
                                {
                                    IPoint = GetCrossPoint(lineA, lineB);
                                    aEndPoint.sPoint = q1;
                                    aEndPoint.Point = IPoint;
                                    aEndPoint.Index = newLineList.Count;
                                    _endPointList.Add(aEndPoint);    //---- Generate _endPointList for border insert
                                    break;
                                }
                                q1 = q2;
                            }
                            newPlist.Add(IPoint);
                            aType = "Border";
                        }
                        newPlist.Add(aPList[j]);
                        ifInPolygon = true;
                    }
                    else
                    {
                        if (ifInPolygon)
                        {
                            lineA = new Line();
                            lineA.P1 = p1;
                            lineA.P2 = p2;
                            q1 = polyList[polyList.Count - 1];
                            IPoint = new PointD();
                            for (k = 0; k < polyList.Count; k++)
                            {
                                q2 = polyList[k];
                                lineB = new Line();
                                lineB.P1 = q1;
                                lineB.P2 = q2;
                                if (IsLineSegmentCross(lineA, lineB))
                                {
                                    IPoint = GetCrossPoint(lineA, lineB);
                                    aEndPoint.sPoint = q1;
                                    aEndPoint.Point = IPoint;
                                    aEndPoint.Index = newLineList.Count;
                                    _endPointList.Add(aEndPoint);
                                    break;
                                }
                                q1 = q2;
                            }
                            newPlist.Add(IPoint);

                            bLine.Value = aValue;
                            bLine.Type = aType;
                            bLine.PointList = newPlist;
                            newLineList.Add(bLine);
                            ifInPolygon = false;
                            newPlist = new List<PointD>();
                            aType = "Border";
                        }
                    }
                    p1 = p2;
                }
                if (ifInPolygon && newPlist.Count > 1)
                {
                    bLine.Value = aValue;
                    bLine.Type = aType;
                    bLine.PointList = newPlist;
                    newLineList.Add(bLine);
                }
            }

            return newLineList;
        }

        /// <summary>
        /// Cut contour lines with a polygon. Return the polylines inside of the polygon
        /// </summary>
        /// <param name="alinelist">polyline list</param>
        /// <param name="aBorder">border for cutting</param>
        /// <returns>Inside Polylines after cut</returns>
        private static List<PolyLine> CutContourLines(List<PolyLine> alinelist, Border aBorder)
        {
            List<PointD> pointList = aBorder.LineList[0].pointList;
            List<PolyLine> newLineList = new List<PolyLine>();
            int i, j, k;
            PolyLine aLine, bLine;
            List<PointD> aPList = new List<PointD>();
            double aValue;
            string aType;
            bool ifInPolygon;
            PointD q1, q2, p1, p2, IPoint;
            Line lineA, lineB;
            EndPoint aEndPoint = new EndPoint();

            _endPointList = new List<EndPoint>();
            if (!IsClockwise(pointList))    //---- Make cut polygon clockwise
                pointList.Reverse();

            for (i = 0; i < alinelist.Count; i++)
            {
                aLine = alinelist[i];
                aValue = aLine.Value;
                aType = aLine.Type;
                aPList = new List<PointD>(aLine.PointList);
                ifInPolygon = false;
                List<PointD> newPlist = new List<PointD>();
                //---- For "Close" type contour,the start point must be outside of the cut polygon.
                if (aType == "Close" && PointInPolygon(pointList, (PointD)aPList[0]))
                {
                    bool isAllIn = true;
                    int notInIdx = 0;
                    for (j = 0; j < aPList.Count; j++)
                    {
                        if (!PointInPolygon(pointList, (PointD)aPList[j]))
                        {
                            notInIdx = j;
                            isAllIn = false;
                            break;
                        }
                    }
                    if (!isAllIn)
                    {
                        List<PointD> bPList = new List<PointD>();
                        for (j = notInIdx; j < aPList.Count; j++)
                            bPList.Add(aPList[j]);

                        for (j = 1; j < notInIdx; j++)
                            bPList.Add(aPList[j]);

                        bPList.Add(bPList[0]);
                        aPList = bPList;
                    }
                }

                p1 = new PointD();
                for (j = 0; j < aPList.Count; j++)
                {
                    p2 = aPList[j];
                    if (PointInPolygon(pointList, p2))
                    {
                        if (!ifInPolygon && j > 0)
                        {
                            lineA = new Line();
                            lineA.P1 = p1;
                            lineA.P2 = p2;
                            q1 = pointList[pointList.Count - 1];
                            IPoint = new PointD();
                            for (k = 0; k < pointList.Count; k++)
                            {
                                q2 = pointList[k];
                                lineB = new Line();
                                lineB.P1 = q1;
                                lineB.P2 = q2;
                                if (IsLineSegmentCross(lineA, lineB))
                                {
                                    IPoint = GetCrossPoint(lineA, lineB);
                                    aEndPoint.sPoint = q1;
                                    aEndPoint.Point = IPoint;
                                    aEndPoint.Index = newLineList.Count;
                                    _endPointList.Add(aEndPoint);    //---- Generate _endPointList for border insert
                                    break;
                                }
                                q1 = q2;
                            }
                            newPlist.Add(IPoint);
                            aType = "Border";
                        }
                        newPlist.Add(aPList[j]);
                        ifInPolygon = true;
                    }
                    else
                    {
                        if (ifInPolygon)
                        {
                            lineA = new Line();
                            lineA.P1 = p1;
                            lineA.P2 = p2;
                            q1 = pointList[pointList.Count - 1];
                            IPoint = new PointD();
                            for (k = 0; k < pointList.Count; k++)
                            {
                                q2 = pointList[k];
                                lineB = new Line();
                                lineB.P1 = q1;
                                lineB.P2 = q2;
                                if (IsLineSegmentCross(lineA, lineB))
                                {
                                    IPoint = GetCrossPoint(lineA, lineB);
                                    aEndPoint.sPoint = q1;
                                    aEndPoint.Point = IPoint;
                                    aEndPoint.Index = newLineList.Count;
                                    _endPointList.Add(aEndPoint);
                                    break;
                                }
                                q1 = q2;
                            }
                            newPlist.Add(IPoint);

                            bLine = new PolyLine();
                            bLine.Value = aValue;
                            bLine.Type = aType;
                            bLine.PointList = newPlist;
                            newLineList.Add(bLine);
                            ifInPolygon = false;
                            newPlist = new List<PointD>();
                            aType = "Border";
                        }
                    }
                    p1 = p2;
                }
                if (ifInPolygon && newPlist.Count > 1)
                {
                    bLine = new PolyLine();
                    bLine.Value = aValue;
                    bLine.Type = aType;
                    bLine.PointList = newPlist;
                    newLineList.Add(bLine);
                }
            }

            return newLineList;
        }

        /// <summary>
        /// Smooth Polylines
        /// </summary>
        /// <param name="aLineList">Polyline list</param>
        /// <returns>Polyline list after smoothing</returns>
        public static List<PolyLine> SmoothLines(List<PolyLine> aLineList)
        {
            return SmoothLines(aLineList, 0.05f);
        }

        /// <summary>
        /// Smooth Polylines
        /// </summary>
        /// <param name="aLineList">Polyline list</param>
        /// <param name="step">B-Spline scan step (0 - 1)</param>
        /// <returns>Polyline list after smoothing</returns>
        public static List<PolyLine> SmoothLines(List<PolyLine> aLineList, float step)
        {
            List<PolyLine> newLineList = new List<PolyLine>();
            int i;
            PolyLine aline;
            List<PointD> newPList = new List<PointD>();
            double aValue;
            string aType;
            bool isClose;

            for (i = 0; i < aLineList.Count; i++)
            {
                aline = aLineList[i];
                aValue = aline.Value;
                aType = aline.Type;
                isClose = aType == "Close";
                newPList = new List<PointD>(aline.PointList);
                if (newPList.Count <= 1)
                    continue;

                if (newPList.Count == 2)
                {
                    PointD bP = new PointD();
                    PointD aP = newPList[0];
                    PointD cP = newPList[1];
                    bP.X = (cP.X - aP.X) / 4 + aP.X;
                    bP.Y = (cP.Y - aP.Y) / 4 + aP.Y;
                    newPList.Insert(1, bP);
                    bP = new PointD();
                    bP.X = (cP.X - aP.X) / 4 * 3 + aP.X;
                    bP.Y = (cP.Y - aP.Y) / 4 * 3 + aP.Y;
                    newPList.Insert(2, bP);
                }
                if (newPList.Count == 3)
                {
                    PointD bP = new PointD();
                    PointD aP = newPList[0];
                    PointD cP = newPList[1];
                    bP.X = (cP.X - aP.X) / 2 + aP.X;
                    bP.Y = (cP.Y - aP.Y) / 2 + aP.Y;
                    newPList.Insert(1, bP);
                }
                newPList = BSplineScanning(newPList, isClose, step);
                aline.PointList = newPList;
                newLineList.Add(aline);
            }

            return newLineList;
        }

        /// <summary>
        /// Smooth points
        /// </summary>
        /// <param name="pointList">point list</param>
        /// <returns>new points</returns>
        public static List<PointD> SmoothPoints(List<PointD> pointList)
        {
            return BSplineScanning(pointList);
        }

        /// <summary>
        /// Tracing polygons from contour lines and borders
        /// </summary>
        /// <param name="S0">input grid data</param>
        /// <param name="cLineList">contour lines</param>
        /// <param name="borderList">borders</param>
        /// <param name="contour">contour values</param>
        /// <returns>traced contour polygons</returns>
        public static List<Polygon> TracingPolygons(double[,] S0, List<PolyLine> cLineList, List<Border> borderList, double[] contour)
        {
            List<Polygon> aPolygonList = new List<Polygon>(), newPolygonList = new List<Polygon>();
            List<BorderPoint> newBPList = new List<BorderPoint>();
            List<BorderPoint> bPList = new List<BorderPoint>();
            List<PointD> PList = new List<PointD>();
            Border aBorder;
            BorderLine aBLine;
            PointD aPoint;
            BorderPoint aBPoint;
            int i, j;
            List<PolyLine> lineList = new List<PolyLine>();
            List<BorderPoint> aBorderList = new List<BorderPoint>();
            PolyLine aLine;
            Polygon aPolygon;
            IJPoint aijP;
            double aValue = 0;
            int[] pNums;            

            //Borders loop
            for (i = 0; i < borderList.Count; i++)
            {
                aBorderList.Clear();
                bPList.Clear();
                lineList.Clear();
                aPolygonList.Clear();
                aBorder = borderList[i];

                aBLine = aBorder.LineList[0];
                PList = aBLine.pointList;
                if (!IsClockwise(PList))    //Make sure the point list is clockwise
                    PList.Reverse();                

                if (aBorder.LineNum == 1)    //The border has just one line
                {
                    //Construct border point list
                    for (j = 0; j < PList.Count; j++)
                    {
                        aPoint = PList[j];
                        aBPoint = new BorderPoint();
                        aBPoint.Id = -1;
                        aBPoint.Point = aPoint;
                        aBPoint.Value = S0[aBLine.ijPointList[j].I, aBLine.ijPointList[j].J];
                        aBorderList.Add(aBPoint);
                    }

                    //Find the contour lines of this border
                    for (j = 0; j < cLineList.Count; j++)
                    {
                        aLine = cLineList[j];
                        if (aLine.BorderIdx == i)
                        {
                            lineList.Add(aLine);    //Construct contour line list
                            //Construct border point list of the contour line
                            if (aLine.Type == "Border")    //The contour line with the start/end point on the border
                            {
                                aPoint = aLine.PointList[0];
                                aBPoint = new BorderPoint();
                                aBPoint.Id = lineList.Count - 1;
                                aBPoint.Point = aPoint;
                                aBPoint.Value = aLine.Value;
                                bPList.Add(aBPoint);
                                aPoint = aLine.PointList[aLine.PointList.Count - 1];
                                aBPoint = new BorderPoint();
                                aBPoint.Id = lineList.Count - 1;
                                aBPoint.Point = aPoint;
                                aBPoint.Value = aLine.Value;
                                bPList.Add(aBPoint);
                            }
                        }
                    }

                    if (lineList.Count == 0)    //No contour lines in this border, the polygon is the border
                    {
                        //Judge the value of the polygon
                        aijP = aBLine.ijPointList[0];
                        aPolygon = new Polygon();
                        if (S0[aijP.I, aijP.J] < contour[0])
                        {
                            aValue = contour[0];
                            aPolygon.IsHighCenter = false;
                        }
                        else
                        {
                            for (j = contour.Length - 1; j >= 0; j--)
                            {
                                if (S0[aijP.I, aijP.J] > contour[j])
                                {
                                    aValue = contour[j];
                                    break;
                                }
                            }
                            aPolygon.IsHighCenter = true;
                        }
                        if (PList.Count > 0)
                        {
                            aPolygon.IsBorder = true;
                            aPolygon.HighValue = aValue;
                            aPolygon.LowValue = aValue;
                            aPolygon.Extent = new Extent();
                            aPolygon.Area = GetExtentAndArea(PList, ref aPolygon.Extent);
                            aPolygon.StartPointIdx = 0;
                            aPolygon.IsClockWise = true;
                            aPolygon.OutLine.Type = "Border";
                            aPolygon.OutLine.Value = aValue;
                            aPolygon.OutLine.BorderIdx = i;
                            aPolygon.OutLine.PointList = PList;
                            aPolygon.HoleLines = new List<PolyLine>();
                            aPolygonList.Add(aPolygon);
                        }
                    }
                    else    //Has contour lines in this border
                    {
                        //Insert the border points of the contour lines to the border point list of the border
                        newBPList = InsertPoint2Border(bPList, aBorderList);
                        //aPolygonList = TracingPolygons(lineList, newBPList, aBound, contour);
                        aPolygonList = TracingPolygons(lineList, newBPList);
                    }
                    aPolygonList = AddPolygonHoles(aPolygonList);
                }
                else    //---- The border has holes
                {
                    aBLine = aBorder.LineList[0];
                    //Find the contour lines of this border
                    for (j = 0; j < cLineList.Count; j++)
                    {
                        aLine = cLineList[j];
                        if (aLine.BorderIdx == i)
                        {
                            lineList.Add(aLine);
                            if (aLine.Type == "Border")
                            {
                                aPoint = aLine.PointList[0];
                                aBPoint = new BorderPoint();
                                aBPoint.Id = lineList.Count - 1;
                                aBPoint.Point = aPoint;
                                aBPoint.Value = aLine.Value;
                                bPList.Add(aBPoint);
                                aPoint = aLine.PointList[aLine.PointList.Count - 1];
                                aBPoint = new BorderPoint();
                                aBPoint.Id = lineList.Count - 1;
                                aBPoint.Point = aPoint;
                                aBPoint.Value = aLine.Value;
                                bPList.Add(aBPoint);
                            }
                        }
                    }
                    if (lineList.Count == 0)  //No contour lines in this border, the polygon is the border and the holes
                    {
                        aijP = aBLine.ijPointList[0];
                        aPolygon = new Polygon();
                        if (S0[aijP.I, aijP.J] < contour[0])
                        {
                            aValue = contour[0];
                            aPolygon.IsHighCenter = false;
                        }
                        else
                        {
                            for (j = contour.Length - 1; j >= 0; j--)
                            {
                                if (S0[aijP.I, aijP.J] > contour[j])
                                {
                                    aValue = contour[j];
                                    break;
                                }
                            }
                            aPolygon.IsHighCenter = true;
                        }
                        if (PList.Count > 0)
                        {
                            aPolygon.IsBorder = true;
                            aPolygon.HighValue = aValue;
                            aPolygon.LowValue = aValue;
                            aPolygon.Area = GetExtentAndArea(PList, ref aPolygon.Extent);
                            aPolygon.StartPointIdx = 0;
                            aPolygon.IsClockWise = true;
                            aPolygon.OutLine.Type = "Border";
                            aPolygon.OutLine.Value = aValue;
                            aPolygon.OutLine.BorderIdx = i;
                            aPolygon.OutLine.PointList = PList;
                            aPolygon.HoleLines = new List<PolyLine>();
                            aPolygonList.Add(aPolygon);
                        }
                    }
                    else
                    {
                        pNums = new int[aBorder.LineNum];
                        newBPList = InsertPoint2Border_Ring(S0, bPList, aBorder, ref pNums);
                        
                        aPolygonList = TracingPolygons_Ring(lineList, newBPList, aBorder, contour, pNums);
                        //aPolygonList = TracingPolygons(lineList, newBPList, contour);

                        //Sort polygons by area
                        List<Polygon> sortList = new List<Polygon>();
                        while (aPolygonList.Count > 0)
                        {
                            Boolean isInsert = false;
                            for (j = 0; j < sortList.Count; j++)
                            {
                                if (aPolygonList[0].Area > sortList[j].Area)
                                {
                                    sortList.Add(aPolygonList[0]);
                                    isInsert = true;
                                    break;
                                }
                            }
                            if (!isInsert)
                            {
                                sortList.Add(aPolygonList[0]);
                            }
                            aPolygonList.RemoveAt(0);
                        }
                        aPolygonList = sortList;
                    }

                    List<List<PointD>> holeList = new List<List<PointD>>();
                    for (j = 0; j < aBorder.LineNum; j++)
                    {
                        //if (aBorder.LineList[j].pointList.Count == pNums[j])
                        //{
                        //    holeList.Add(aBorder.LineList[j].pointList);
                        //}
                        holeList.Add(aBorder.LineList[j].pointList);
                    }

                    if (holeList.Count > 0)
                    {
                        AddHoles_Ring(ref aPolygonList, holeList);
                    }
                    aPolygonList = AddPolygonHoles_Ring(aPolygonList);
                }
                newPolygonList.AddRange(aPolygonList);
            }

            //newPolygonList = AddPolygonHoles(newPolygonList);
            foreach (Polygon nPolygon in newPolygonList)
            {
                if (!IsClockwise(nPolygon.OutLine.PointList))
                    nPolygon.OutLine.PointList.Reverse();
            }

            return newPolygonList;
        }

        /// <summary>
        /// Create contour polygons
        /// </summary>
        /// <param name="LineList">contour lines</param>
        /// <param name="aBound">gid data extent</param>
        /// <param name="contour">contour values</param>
        /// <returns>contour polygons</returns>
        private static List<Polygon> CreateContourPolygons(List<PolyLine> LineList, Extent aBound, double[] contour)
        {
            List<Polygon> aPolygonList = new List<Polygon>();
            List<BorderPoint> newBorderList = new List<BorderPoint>();            

            //---- Insert points to border list
            newBorderList = InsertPoint2RectangleBorder(LineList, aBound);

            //---- Tracing polygons
            aPolygonList = TracingPolygons(LineList, newBorderList, aBound, contour);

            return aPolygonList;
        }

        /// <summary>
        /// Create polygons from cutted contour lines
        /// </summary>
        /// <param name="LineList">polylines</param>
        /// <param name="polyList">Border point list</param>
        /// <param name="aBound">extent</param>
        /// <param name="contour">contour values</param>
        /// <returns>contour polygons</returns>
        private static List<Polygon> CreateCutContourPolygons(List<PolyLine> LineList, List<PointD> polyList, Extent aBound, double[] contour)
        {
            List<Polygon> aPolygonList = new List<Polygon>();
            List<BorderPoint> newBorderList = new List<BorderPoint>();
            List<BorderPoint> borderList = new List<BorderPoint>();
            PointD aPoint;
            BorderPoint aBPoint;
            int i;

            //---- Get border point list
            if (!IsClockwise(polyList))
                polyList.Reverse();

            for (i = 0; i < polyList.Count; i++)
            {
                aPoint = polyList[i];
                aBPoint = new BorderPoint();
                aBPoint.Id = -1;
                aBPoint.Point = aPoint;
                borderList.Add(aBPoint);
            }

            //---- Insert points to border list
            newBorderList = InsertEndPoint2Border(_endPointList, borderList);

            //---- Tracing polygons
            aPolygonList = TracingPolygons(LineList, newBorderList, aBound, contour);

            return aPolygonList;
        }        

        /// <summary>
        /// Create contour polygons from borders
        /// </summary>
        /// <param name="S0">Input grid data array</param>
        /// <param name="cLineList">Contour lines</param>
        /// <param name="borderList">borders</param>
        /// <param name="aBound">extent</param>
        /// <param name="contour">contour value</param>
        /// <returns>contour polygons</returns>
        private static List<Polygon> CreateBorderContourPolygons(double[,] S0, List<PolyLine> cLineList, List<Border> borderList, Extent aBound,                double[] contour)
        {
            List<Polygon> aPolygonList = new List<Polygon>(), newPolygonList = new List<Polygon>();
            List<BorderPoint> newBPList = new List<BorderPoint>();
            List<BorderPoint> bPList = new List<BorderPoint>();
            List<PointD> PList = new List<PointD>();
            Border aBorder;
            BorderLine aBLine;
            PointD aPoint;
            BorderPoint aBPoint;
            int i, j;
            List<PolyLine> lineList = new List<PolyLine>();
            List<BorderPoint> aBorderList = new List<BorderPoint>();
            PolyLine aLine;
            Polygon aPolygon;
            IJPoint aijP;
            double aValue = 0;
            int[] pNums;

            //Borders loop
            for (i = 0; i < borderList.Count; i++)
            {
                aBorderList.Clear();
                bPList.Clear();
                lineList.Clear();
                aPolygonList.Clear();
                aBorder = borderList[i];
                if (aBorder.LineNum == 1)    //The border has just one line
                {
                    aBLine = aBorder.LineList[0];
                    PList = aBLine.pointList;
                    if (!IsClockwise(PList))    //Make sure the point list is clockwise
                        PList.Reverse();

                    //Construct border point list
                    for (j = 0; j < PList.Count; j++)
                    {
                        aPoint = PList[j];
                        aBPoint = new BorderPoint();
                        aBPoint.Id = -1;
                        aBPoint.Point = aPoint;
                        aBPoint.Value = S0[aBLine.ijPointList[j].I, aBLine.ijPointList[j].J];
                        aBorderList.Add(aBPoint);
                    }

                    //Find the contour lines of this border
                    for (j = 0; j < cLineList.Count; j++)
                    {
                        aLine = cLineList[j];
                        if (aLine.BorderIdx == i)
                        {
                            lineList.Add(aLine);    //Construct contour line list
                            //Construct border point list of the contour line
                            if (aLine.Type == "Border")    //The contour line with the start/end point on the border
                            {
                                aPoint = aLine.PointList[0];
                                aBPoint = new BorderPoint();
                                aBPoint.Id = lineList.Count - 1;
                                aBPoint.Point = aPoint;
                                aBPoint.Value = aLine.Value;
                                bPList.Add(aBPoint);
                                aPoint = aLine.PointList[aLine.PointList.Count - 1];
                                aBPoint = new BorderPoint();
                                aBPoint.Id = lineList.Count - 1;
                                aBPoint.Point = aPoint;
                                aBPoint.Value = aLine.Value;
                                bPList.Add(aBPoint);
                            }
                        }
                    }
                    
                    if (lineList.Count == 0)    //No contour lines in this border, the polygon is the border
                    {
                        //Judge the value of the polygon
                        aijP = aBLine.ijPointList[0];
                        aPolygon = new Polygon();
                        if (S0[aijP.I, aijP.J] < contour[0])
                        {
                            aValue = contour[0];
                            aPolygon.IsHighCenter = false;
                        }
                        else
                        {
                            for (j = contour.Length - 1; j >= 0; j--)
                            {
                                if (S0[aijP.I, aijP.J] > contour[j])
                                {
                                    aValue = contour[j];
                                    break;
                                }
                            }
                            aPolygon.IsHighCenter = true;
                        }
                        if (PList.Count > 0)
                        {
                            aPolygon.HighValue = aValue;
                            aPolygon.LowValue = aValue;
                            aPolygon.Extent = new Extent();
                            aPolygon.Area = GetExtentAndArea(PList, ref aPolygon.Extent);
                            aPolygon.StartPointIdx = 0;
                            aPolygon.IsClockWise = true;
                            aPolygon.OutLine.Type = "Border";
                            aPolygon.OutLine.Value = aValue;
                            aPolygon.OutLine.BorderIdx = i;
                            aPolygon.OutLine.PointList = PList;
                            aPolygonList.Add(aPolygon);
                        }
                    }
                    else    //Has contour lines in this border
                    {
                        //Insert the border points of the contour lines to the border point list of the border
                        newBPList = InsertPoint2Border(bPList, aBorderList);
                        //aPolygonList = TracingPolygons(lineList, newBPList, aBound, contour);
                        aPolygonList = TracingPolygons(lineList, newBPList);
                    }
                }
                else    //---- The border has holes
                {
                    aBLine = aBorder.LineList[0];
                    //Find the contour lines of this border
                    for (j = 0; j < cLineList.Count; j++)
                    {
                        aLine = cLineList[j];
                        if (aLine.BorderIdx == i)
                        {
                            lineList.Add(aLine);
                            if (aLine.Type == "Border")
                            {
                                aPoint = aLine.PointList[0];
                                aBPoint = new BorderPoint();
                                aBPoint.Id = lineList.Count - 1;
                                aBPoint.Point = aPoint;
                                aBPoint.Value = aLine.Value;
                                bPList.Add(aBPoint);
                                aPoint = aLine.PointList[aLine.PointList.Count - 1];
                                aBPoint = new BorderPoint();
                                aBPoint.Id = lineList.Count - 1;
                                aBPoint.Point = aPoint;
                                aBPoint.Value = aLine.Value;
                                bPList.Add(aBPoint);
                            }
                        }
                    }
                    if (lineList.Count == 0)  //No contour lines in this border, the polygon is the border and the holes
                    {
                        aPolygon = new Polygon();
                        aijP = aBLine.ijPointList[0];
                        if (S0[aijP.I, aijP.J] < contour[0])
                        {
                            aValue = contour[0];
                            aPolygon.IsHighCenter = false;
                        }
                        else
                        {
                            for (j = contour.Length - 1; j >= 0; j--)
                            {
                                if (S0[aijP.I, aijP.J] > contour[j])
                                {
                                    aValue = contour[j];
                                    break;
                                }
                            }
                            aPolygon.IsHighCenter = true;
                        }
                        if (PList.Count > 0)
                        {
                            aPolygon.HighValue = aValue;
                            aPolygon.LowValue = aValue;
                            aPolygon.Area = GetExtentAndArea(PList, ref aPolygon.Extent);
                            aPolygon.StartPointIdx = 0;
                            aPolygon.IsClockWise = true;
                            aPolygon.OutLine.Type = "Border";
                            aPolygon.OutLine.Value = aValue;
                            aPolygon.OutLine.BorderIdx = i;
                            aPolygon.OutLine.PointList = PList;
                            aPolygonList.Add(aPolygon);
                        }
                    }
                    else
                    {
                        pNums = new int[aBorder.LineNum];
                        newBPList = InsertPoint2Border_Ring(S0, bPList, aBorder, ref pNums);
                        aPolygonList = TracingPolygons_Ring(lineList, newBPList, aBorder, contour, pNums);
                        //aPolygonList = TracingPolygons(lineList, newBPList, contour);
                    }
                }
                newPolygonList.AddRange(aPolygonList);
            }

            return newPolygonList;
        }

        /// <summary>
        /// Judge if a point is in a polygon
        /// </summary>
        /// <param name="poly">polygon border</param>
        /// <param name="aPoint">point</param>
        /// <returns>If the point is in the polygon</returns>
        public static bool PointInPolygon(List<PointD> poly, PointD aPoint)
        {
            double xNew, yNew, xOld, yOld;
            double x1, y1, x2, y2;
            int i;
            bool inside = false;
            int nPoints = poly.Count;

            if (nPoints < 3)
                return false;

            xOld = poly[nPoints - 1].X;
            yOld = poly[nPoints - 1].Y;
            for (i = 0; i < nPoints; i++)
            {
                xNew = poly[i].X;
                yNew = poly[i].Y;
                if (xNew > xOld)
                {
                    x1 = xOld;
                    x2 = xNew;
                    y1 = yOld;
                    y2 = yNew;
                }
                else
                {
                    x1 = xNew;
                    x2 = xOld;
                    y1 = yNew;
                    y2 = yOld;
                }

                //---- edge "open" at left end
                if ((xNew < aPoint.X) == (aPoint.X <= xOld) &&
                   (aPoint.Y - y1) * (x2 - x1) < (y2 - y1) * (aPoint.X - x1))
                    inside = !inside;

                xOld = xNew;
                yOld = yNew;
            }

            return inside;
        }

        /// <summary>
        /// Judge if a point is in a polygon
        /// </summary>
        /// <param name="aPolygon">polygon</param>
        /// <param name="aPoint">point</param>
        /// <returns>is in</returns>
        public static bool PointInPolygon(Polygon aPolygon, PointD aPoint)
        {
            if (aPolygon.HasHoles)
            {
                bool isIn = PointInPolygon(aPolygon.OutLine.PointList, aPoint);
                if (isIn)
                {
                    foreach (PolyLine aLine in aPolygon.HoleLines)
                    {
                        if (PointInPolygon(aLine.PointList, aPoint))
                        {
                            isIn = false;
                            break;
                        }
                    }
                }

                return isIn;
            }
            else
                return PointInPolygon(aPolygon.OutLine.PointList, aPoint);
        }

        /// <summary>
        /// Clip polylines with a border polygon
        /// </summary>
        /// <param name="polylines">polyline list</param>
        /// <param name="clipPList">cutting border point list</param>
        /// <returns>cutted polylines</returns>
        public static List<PolyLine> ClipPolylines(List<PolyLine> polylines, List<PointD> clipPList)
        {
            List<PolyLine> newPolylines = new List<PolyLine>();
            foreach (PolyLine aPolyline in polylines)
            {
                newPolylines.AddRange(CutPolyline(aPolyline, clipPList));
            }

            return newPolylines;
        }

        /// <summary>
        /// Clip polygons with a border polygon
        /// </summary>
        /// <param name="polygons">polygon list</param>
        /// <param name="clipPList">cutting border point list</param>
        /// <returns>cutted polygons</returns>
        public static List<Polygon> ClipPolygons(List<Polygon> polygons, List<PointD> clipPList)
        {
            List<Polygon> newPolygons = new List<Polygon>();
            for (int i = 0; i < polygons.Count; i++)
            {
                Polygon aPolygon = polygons[i];
                if (aPolygon.HasHoles)
                    newPolygons.AddRange(CutPolygon_Hole(aPolygon, clipPList));
                else
                    newPolygons.AddRange(CutPolygon(aPolygon, clipPList));
            }

            //Sort polygons with bording rectangle area
            List<Polygon> outPolygons = new List<Polygon>();
            bool isInserted = false;
            for (int i = 0; i < newPolygons.Count; i++)
            {
                Polygon aPolygon = newPolygons[i];
                isInserted = false;
                for (int j = 0; j < outPolygons.Count; j++)
                {
                    if (aPolygon.Area > outPolygons[j].Area)
                    {
                        outPolygons.Insert(j, aPolygon);
                        isInserted = true;
                        break;
                    }
                }

                if (!isInserted)
                    outPolygons.Add(aPolygon);
            }

            return outPolygons;
        }

        #endregion

        #region Streamline
        /// <summary>
        /// Tracing stream lines
        /// </summary>
        /// <param name="U">U component array</param>
        /// <param name="V">V component array</param>
        /// <param name="X">X coordinate array</param>
        /// <param name="Y">Y coordinate array</param>
        /// <param name="UNDEF">undefine data</param>
        /// <param name="density">stream line density</param>
        /// <returns>streamlines</returns>
        public static List<PolyLine> TracingStreamline(double[,] U, double[,] V, double[] X, double[] Y, double UNDEF, int density)
        {
            List<PolyLine> streamLines = new List<PolyLine>();
            int xNum = U.GetLength(1);
            int yNum = U.GetLength(0);
            double[,] Dx = new double[yNum, xNum];
            double[,] Dy = new double[yNum, xNum];
            double deltX = X[1] - X[0];
            double deltY = Y[1] - Y[0];            
            if (density == 0)
                density = 1;
            double radius = deltX / (density * density);                     
            int i, j;

            //Normalize wind components
            for (i = 0; i < yNum; i++)
            {
                for (j = 0; j < xNum; j++)
                {
                    if (Math.Abs(U[i, j] / UNDEF - 1) < 0.01)
                    {
                        Dx[i, j] = 0.1;
                        Dy[i, j] = 0.1;
                    }
                    else
                    {
                        double WS = Math.Sqrt(U[i, j] * U[i, j] + V[i, j] * V[i, j]);
                        if (WS == 0)
                            WS = 1;
                        Dx[i, j] = (U[i, j] / WS) * deltX / density;
                        Dy[i, j] = (V[i, j] / WS) * deltY / density;
                    }
                }
            }

            //Flag the grid boxes
            List<PointD>[,] SPoints = new List<PointD>[yNum - 1, xNum - 1];
            int[,] flags = new int[yNum - 1, xNum - 1];
            for (i = 0; i < yNum - 1; i++)
            {
                for (j = 0; j < xNum - 1; j++)
                {
                    if (i % 2 == 0 && j % 2 == 0)
                        flags[i, j] = 0;
                    else
                        flags[i, j] = 1;

                    SPoints[i, j] = new List<PointD>();
                }
            }

            //Tracing streamline            
            for (i = 0; i < yNum - 1; i++)
            {
                for (j = 0; j < xNum - 1; j++)
                {
                    if (flags[i, j] == 0)    //No streamline started form this grid box, a new streamline started
                    {
                        List<PointD> pList = new List<PointD>();
                        PointD aPoint = new PointD();                       
                        int ii, jj;
                        int loopN;
                        PolyLine aPL = new PolyLine();

                        //Start point - the center of the grid box
                        aPoint.X = X[j] + deltX / 2;
                        aPoint.Y = Y[i] + deltY / 2;
                        pList.Add((PointD)aPoint.Clone());
                        SPoints[i, j].Add((PointD)aPoint.Clone());
                        flags[i, j] = 1;    //Flag the grid box and no streamline will start from this box again
                        ii = i;
                        jj = j;
                        int loopLimit = 500;

                        //Tracing forward
                        loopN = 0;
                        while (loopN < loopLimit)
                        {
                            //Trace next streamline point
                            bool isInDomain = TracingStreamlinePoint(ref aPoint, Dx, Dy, X, Y, ref ii, ref jj, true);

                            //Terminating the streamline
                            if (isInDomain)
                            {
                                if (Math.Abs(U[ii, jj] / UNDEF - 1) < 0.01 || Math.Abs(U[ii, jj + 1] / UNDEF - 1) < 0.01 ||
                                    Math.Abs(U[ii + 1, jj] / UNDEF - 1) < 0.01 || Math.Abs(U[ii + 1, jj + 1] / UNDEF - 1) < 0.01)
                                    break;
                                else
                                {
                                    bool isTerminating = false;
                                    foreach (PointD sPoint in SPoints[ii, jj])
                                    {
                                        if (Math.Sqrt((aPoint.X - sPoint.X) * (aPoint.X - sPoint.X) +
                                            (aPoint.Y - sPoint.Y) * (aPoint.Y - sPoint.Y)) < radius)
                                        {
                                            isTerminating = true;
                                            break;
                                        }
                                    }
                                    if (!isTerminating)
                                    {
                                        pList.Add((PointD)aPoint.Clone());
                                        SPoints[ii, jj].Add((PointD)aPoint.Clone());
                                        flags[ii, jj] = 1;
                                    }
                                    else
                                        break;
                                }
                            }
                            else
                            {
                                break;
                            }

                            loopN += 1;
                        }                        

                        //Tracing backword
                        aPoint.X = X[j] + deltX / 2;
                        aPoint.Y = Y[i] + deltY / 2;
                        ii = i;
                        jj = j;
                        loopN = 0;
                        while (loopN < loopLimit)
                        {
                            //Trace next streamline point
                            bool isInDomain = TracingStreamlinePoint(ref aPoint, Dx, Dy, X, Y, ref ii, ref jj, false);

                            //Terminating the streamline
                            if (isInDomain)
                            {
                                if (Math.Abs(U[ii, jj] / UNDEF - 1) < 0.01 || Math.Abs(U[ii, jj + 1] / UNDEF - 1) < 0.01 ||
                                    Math.Abs(U[ii + 1, jj] / UNDEF - 1) < 0.01 || Math.Abs(U[ii + 1, jj + 1] / UNDEF - 1) < 0.01)
                                    break;
                                else
                                {
                                    bool isTerminating = false;
                                    foreach (PointD sPoint in SPoints[ii, jj])
                                    {
                                        if (Math.Sqrt((aPoint.X - sPoint.X) * (aPoint.X - sPoint.X) +
                                            (aPoint.Y - sPoint.Y) * (aPoint.Y - sPoint.Y)) < radius)
                                        {
                                            isTerminating = true;
                                            break;
                                        }
                                    }
                                    if (!isTerminating)
                                    {
                                        pList.Insert(0, (PointD)aPoint.Clone());
                                        SPoints[ii, jj].Add((PointD)aPoint.Clone());
                                        flags[ii, jj] = 1;
                                    }
                                    else
                                        break;
                                }
                            }
                            else
                            {
                                break;
                            }

                            loopN += 1;
                        }
                        if (pList.Count > 1)
                        {                            
                            aPL.PointList = pList;
                            streamLines.Add(aPL);                            
                        }

                    }
                }
            }

            //Return
            return streamLines;
        }

        private static bool TracingStreamlinePoint(ref PointD aPoint, double[,] Dx, double[,] Dy, double[] X, double[] Y,
            ref int ii, ref int jj, bool isForward)
        {
            double a, b, c, d, val1, val2;
            double dx, dy;
            int xNum = X.Length;
            int yNum = Y.Length;
            double deltX = X[1] - X[0];
            double deltY = Y[1] - Y[0];
            
            //Interpolation the U/V displacement components to the point
            a = Dx[ii, jj];
            b = Dx[ii, jj + 1];
            c = Dx[ii + 1, jj];
            d = Dx[ii + 1, jj + 1];
            val1 = a + (c - a) * ((aPoint.Y - Y[ii]) / deltY);
            val2 = b + (d - b) * ((aPoint.Y - Y[ii]) / deltY);
            dx = val1 + (val2 - val1) * ((aPoint.X - X[jj]) / deltX);
            a = Dy[ii, jj];
            b = Dy[ii, jj + 1];
            c = Dy[ii + 1, jj];
            d = Dy[ii + 1, jj + 1];
            val1 = a + (c - a) * ((aPoint.Y - Y[ii]) / deltY);
            val2 = b + (d - b) * ((aPoint.Y - Y[ii]) / deltY);
            dy = val1 + (val2 - val1) * ((aPoint.X - X[jj]) / deltX);

            //Tracing forward by U/V displacement components            
            if (isForward)
            {
                aPoint.X += dx;
                aPoint.Y += dy;
            }
            else
            {
                aPoint.X -= dx;
                aPoint.Y -= dy;
            }            

            //Find the grid box that the point is located 
            if (!(aPoint.X >= X[jj] && aPoint.X <= X[jj + 1] && aPoint.Y >= Y[ii] && aPoint.Y <= Y[ii + 1]))
            {
                if (aPoint.X < X[0] || aPoint.X > X[X.Length - 1] || aPoint.Y < Y[0] || aPoint.Y > Y[Y.Length - 1])
                {
                    return false;                    
                }

                //Get the grid box of the point located
                for (int ti = ii - 2; ti < ii + 3; ti++)
                {
                    if (ti >= 0 && ti < yNum)
                    {
                        if (aPoint.Y >= Y[ti] && aPoint.Y <= Y[ti + 1])
                        {
                            ii = ti;
                            for (int tj = jj - 2; tj < jj + 3; tj++)
                            {
                                if (tj >= 0 && tj < xNum)
                                {
                                    if (aPoint.X >= X[tj] && aPoint.X <= X[tj + 1])
                                    {
                                        jj = tj;
                                        break;
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
            }

            return true;
        }
       
        #endregion

        #region Private contour methods

        private static bool TraceBorder(int[,] S1, int i1, int i2, int j1, int j2, ref int i3, ref int j3)
        {
            bool canTrace = true;
            int a, b, c, d;
            if (i1 < i2)    //---- Trace from bottom
            {
                if (S1[i2, j2 - 1] == 1 && S1[i2, j2 + 1] == 1)
                {
                    a = S1[i2 - 1, j2 - 1];
                    b = S1[i2 + 1, j2];
                    c = S1[i2 + 1, j2 - 1];
                    if ((a != 0 && b == 0) || (a == 0 && b != 0 && c != 0))
                    {
                        i3 = i2;
                        j3 = j2 - 1;
                    }
                    else
                    {
                        i3 = i2;
                        j3 = j2 + 1;
                    }
                }
                else if (S1[i2, j2 - 1] == 1 && S1[i2 + 1, j2] == 1)
                {
                    a = S1[i2 + 1, j2 - 1];
                    b = S1[i2 + 1, j2 + 1];
                    c = S1[i2, j2 - 1];
                    d = S1[i2, j2 + 1];
                    if (a == 0 || b == 0 || c == 0 || d == 0)
                    {
                        if ((a == 0 && d == 0) || (b == 0 && c == 0))
                        {
                            i3 = i2;
                            j3 = j2 - 1;
                        }
                        else
                        {
                            i3 = i2 + 1;
                            j3 = j2;
                        }
                    }
                    else
                    {
                        i3 = i2;
                        j3 = j2 - 1;
                    }
                }
                else if (S1[i2, j2 + 1] == 1 && S1[i2 + 1, j2] == 1)
                {
                    a = S1[i2 + 1, j2 - 1];
                    b = S1[i2 + 1, j2 + 1];
                    c = S1[i2, j2 - 1];
                    d = S1[i2, j2 + 1];
                    if (a == 0 || b == 0 || c == 0 || d == 0)
                    {
                        if ((a == 0 && d == 0) || (b == 0 && c == 0))
                        {
                            i3 = i2;
                            j3 = j2 + 1;
                        }
                        else
                        {
                            i3 = i2 + 1;
                            j3 = j2;
                        }
                    }
                    else
                    {
                        i3 = i2;
                        j3 = j2 + 1;
                    }
                }
                else if (S1[i2, j2 - 1] == 1)
                {
                    i3 = i2;
                    j3 = j2 - 1;
                }
                else if (S1[i2, j2 + 1] == 1)
                {
                    i3 = i2;
                    j3 = j2 + 1;
                }
                else if (S1[i2 + 1, j2] == 1)
                {
                    i3 = i2 + 1;
                    j3 = j2;
                }
                else
                {
                    canTrace = false;
                }
            }
            else if (j1 < j2)    //---- Trace from left
            {
                if (S1[i2 + 1, j2] == 1 && S1[i2 - 1, j2] == 1)
                {
                    a = S1[i2 + 1, j2 - 1];
                    b = S1[i2, j2 + 1];
                    c = S1[i2 + 1, j2 + 1];
                    if ((a != 0 && b == 0) || (a == 0 && b != 0 && c != 0))
                    {
                        i3 = i2 + 1;
                        j3 = j2;
                    }
                    else
                    {
                        i3 = i2 - 1;
                        j3 = j2;
                    }
                }
                else if (S1[i2 + 1, j2] == 1 && S1[i2, j2 + 1] == 1)
                {
                    c = S1[i2 - 1, j2];
                    d = S1[i2 + 1, j2];
                    a = S1[i2 - 1, j2 + 1];
                    b = S1[i2 + 1, j2 + 1];
                    if (a == 0 || b == 0 || c == 0 || d == 0)
                    {
                        if ((a == 0 && d == 0) || (b == 0 && c == 0))
                        {
                            i3 = i2 + 1;
                            j3 = j2;
                        }
                        else
                        {
                            i3 = i2;
                            j3 = j2 + 1;
                        }
                    }
                    else
                    {
                        i3 = i2 + 1;
                        j3 = j2;
                    }
                }
                else if (S1[i2 - 1, j2] == 1 && S1[i2, j2 + 1] == 1)
                {
                    c = S1[i2 - 1, j2];
                    d = S1[i2 + 1, j2];
                    a = S1[i2 - 1, j2 + 1];
                    b = S1[i2 + 1, j2 + 1];
                    if (a == 0 || b == 0 || c == 0 || d == 0)
                    {
                        if ((a == 0 && d == 0) || (b == 0 && c == 0))
                        {
                            i3 = i2 - 1;
                            j3 = j2;
                        }
                        else
                        {
                            i3 = i2;
                            j3 = j2 + 1;
                        }
                    }
                    else
                    {
                        i3 = i2 - 1;
                        j3 = j2;
                    }
                }
                else if (S1[i2 + 1, j2] == 1)
                {
                    i3 = i2 + 1;
                    j3 = j2;
                }
                else if (S1[i2 - 1, j2] == 1)
                {
                    i3 = i2 - 1;
                    j3 = j2;
                }
                else if (S1[i2, j2 + 1] == 1)
                {
                    i3 = i2;
                    j3 = j2 + 1;
                }
                else
                {
                    canTrace = false;
                }
            }
            else if (i1 > i2)    //---- Trace from top
            {
                if (S1[i2, j2 - 1] == 1 && S1[i2, j2 + 1] == 1)
                {
                    a = S1[i2 + 1, j2 - 1];
                    b = S1[i2 - 1, j2];
                    c = S1[i2 - 1, j2 + 1];
                    if ((a != 0 && b == 0) || (a == 0 && b != 0 && c != 0))
                    {
                        i3 = i2;
                        j3 = j2 - 1;
                    }
                    else
                    {
                        i3 = i2;
                        j3 = j2 + 1;
                    }
                }
                else if (S1[i2, j2 - 1] == 1 && S1[i2 - 1, j2] == 1)
                {
                    a = S1[i2 - 1, j2 - 1];
                    b = S1[i2 - 1, j2 + 1];
                    c = S1[i2, j2 - 1];
                    d = S1[i2, j2 + 1];
                    if (a == 0 || b == 0 || c == 0 || d == 0)
                    {
                        if ((a == 0 && d == 0) || (b == 0 && c == 0))
                        {
                            i3 = i2;
                            j3 = j2 - 1;
                        }
                        else
                        {
                            i3 = i2 - 1;
                            j3 = j2;
                        }
                    }
                    else
                    {
                        i3 = i2;
                        j3 = j2 - 1;
                    }
                }
                else if (S1[i2, j2 + 1] == 1 && S1[i2 - 1, j2] == 1)
                {
                    a = S1[i2 - 1, j2 - 1];
                    b = S1[i2 - 1, j2 + 1];
                    c = S1[i2, j2 - 1];
                    d = S1[i2, j2 + 1];
                    if (a == 0 || b == 0 || c == 0 || d == 0)
                    {
                        if ((a == 0 && d == 0) || (b == 0 && c == 0))
                        {
                            i3 = i2;
                            j3 = j2 + 1;
                        }
                        else
                        {
                            i3 = i2 - 1;
                            j3 = j2;
                        }
                    }
                    else
                    {
                        i3 = i2;
                        j3 = j2 + 1;
                    }
                }
                else if (S1[i2, j2 - 1] == 1)
                {
                    i3 = i2;
                    j3 = j2 - 1;
                }
                else if (S1[i2, j2 + 1] == 1)
                {
                    i3 = i2;
                    j3 = j2 + 1;
                }
                else if (S1[i2 - 1, j2] == 1)
                {
                    i3 = i2 - 1;
                    j3 = j2;
                }
                else
                {
                    canTrace = false;
                }
            }
            else if (j1 > j2)    //---- Trace from right
            {
                if (S1[i2 + 1, j2] == 1 && S1[i2 - 1, j2] == 1)
                {
                    a = S1[i2 + 1, j2 + 1];
                    b = S1[i2, j2 - 1];
                    c = S1[i2 - 1, j2 - 1];
                    if ((a != 0 && b == 0) || (a == 0 && b != 0 && c != 0))
                    {
                        i3 = i2 + 1;
                        j3 = j2;
                    }
                    else
                    {
                        i3 = i2 - 1;
                        j3 = j2;
                    }
                }
                else if (S1[i2 + 1, j2] == 1 && S1[i2, j2 - 1] == 1)
                {
                    c = S1[i2 - 1, j2];
                    d = S1[i2 + 1, j2];
                    a = S1[i2 - 1, j2 - 1];
                    b = S1[i2 + 1, j2 - 1];
                    if (a == 0 || b == 0 || c == 0 || d == 0)
                    {
                        if ((a == 0 && d == 0) || (b == 0 && c == 0))
                        {
                            i3 = i2 + 1;
                            j3 = j2;
                        }
                        else
                        {
                            i3 = i2;
                            j3 = j2 - 1;
                        }
                    }
                    else
                    {
                        i3 = i2 + 1;
                        j3 = j2;
                    }
                }
                else if (S1[i2 - 1, j2] == 1 && S1[i2, j2 - 1] == 1)
                {
                    c = S1[i2 - 1, j2];
                    d = S1[i2 + 1, j2];
                    a = S1[i2 - 1, j2 - 1];
                    b = S1[i2 + 1, j2 - 1];
                    if (a == 0 || b == 0 || c == 0 || d == 0)
                    {
                        if ((a == 0 && d == 0) || (b == 0 && c == 0))
                        {
                            i3 = i2 - 1;
                            j3 = j2;
                        }
                        else
                        {
                            i3 = i2;
                            j3 = j2 - 1;
                        }
                    }
                    else
                    {
                        i3 = i2 - 1;
                        j3 = j2;
                    }
                }
                else if (S1[i2 + 1, j2] == 1)
                {
                    i3 = i2 + 1;
                    j3 = j2;
                }
                else if (S1[i2 - 1, j2] == 1)
                {
                    i3 = i2 - 1;
                    j3 = j2;
                }
                else if (S1[i2, j2 - 1] == 1)
                {
                    i3 = i2;
                    j3 = j2 - 1;
                }
                else
                {
                    canTrace = false;
                }
            }

            return canTrace;
        }

        private static List<PolyLine> Isoline_UndefData(double[,] S0, double[] X, double[] Y,
            double W, double nx, double ny, 
            ref double[,] S, ref double[,] H, int[, ,] SB, int[, ,] HB, int lineNum)
        {

            List<PolyLine> cLineList = new List<PolyLine>();
            int m, n, i, j;
            m = S0.GetLength(0);
            n = S0.GetLength(1);

            int i1, i2, j1, j2, i3 = 0, j3 = 0;
            double a2x, a2y, a3x = 0, a3y = 0, sx, sy;
            PointD aPoint;
            PolyLine aLine;
            List<PointD> pList;
            bool isS = true;
            EndPoint aEndPoint = new EndPoint();
            //---- Tracing from border
            for (i = 0; i < m; i++)
            {
                for (j = 0; j < n; j++)
                {
                    if (j < n - 1)
                    {
                        if (SB[0, i, j] > -1)    //---- Border
                        {
                            if (S[i, j] != -2)
                            {
                                pList = new List<PointD>();
                                i2 = i;
                                j2 = j;
                                a2x = X[j2] + S[i2, j2] * nx;    //---- x of first point
                                a2y = Y[i2];                   //---- y of first point
                                if (SB[1, i, j] == 0)    //---- Bottom border
                                {
                                    i1 = -1;
                                    aEndPoint.sPoint.X = X[j + 1];
                                    aEndPoint.sPoint.Y = Y[i];
                                }
                                else
                                {
                                    i1 = i2;
                                    aEndPoint.sPoint.X = X[j];
                                    aEndPoint.sPoint.Y = Y[i];
                                }
                                j1 = j2;
                                aPoint = new PointD();
                                aPoint.X = a2x;
                                aPoint.Y = a2y;
                                pList.Add(aPoint);

                                aEndPoint.Index = lineNum + cLineList.Count;
                                aEndPoint.Point = aPoint;
                                aEndPoint.BorderIdx = SB[0, i, j];
                                _endPointList.Add(aEndPoint);

                                aLine = new PolyLine();
                                aLine.Type = "Border";
                                aLine.BorderIdx = SB[0, i, j];
                                while (true)
                                {
                                    if (TraceIsoline_UndefData(i1, i2, ref H, ref S, j1, j2, X, Y, nx, ny, a2x, ref i3, ref j3, ref a3x,                                                    ref a3y, ref isS))
                                    {
                                        aPoint = new PointD();
                                        aPoint.X = a3x;
                                        aPoint.Y = a3y;
                                        pList.Add(aPoint);
                                        if (isS)
                                        {
                                            if (SB[0, i3, j3] > -1)
                                            {
                                                if (SB[1, i3, j3] == 0)
                                                {
                                                    aEndPoint.sPoint.X = X[j3 + 1];
                                                    aEndPoint.sPoint.Y = Y[i3];
                                                }
                                                else
                                                {
                                                    aEndPoint.sPoint.X = X[j3];
                                                    aEndPoint.sPoint.Y = Y[i3];
                                                }
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (HB[0, i3, j3] > -1)
                                            {
                                                if (HB[1, i3, j3] == 0)
                                                {
                                                    aEndPoint.sPoint.X = X[j3];
                                                    aEndPoint.sPoint.Y = Y[i3];
                                                }
                                                else
                                                {
                                                    aEndPoint.sPoint.X = X[j3];
                                                    aEndPoint.sPoint.Y = Y[i3 + 1];
                                                }
                                                break;
                                            }
                                        }
                                        a2x = a3x;
                                        a2y = a3y;
                                        i1 = i2;
                                        j1 = j2;
                                        i2 = i3;
                                        j2 = j3;
                                    }
                                    else
                                    {
                                        aLine.Type = "Error";
                                        break;
                                    }
                                }
                                S[i, j] = -2;
                                if (pList.Count > 1 && aLine.Type != "Error")
                                {
                                    aEndPoint.Point = aPoint;
                                    _endPointList.Add(aEndPoint);

                                    aLine.Value = W;
                                    aLine.PointList = pList;
                                    cLineList.Add(aLine);
                                }
                                else
                                    _endPointList.RemoveAt(_endPointList.Count - 1);

                            }
                        }
                    }
                    if (i < m - 1)
                    {
                        if (HB[0, i, j] > -1)    //---- Border
                        {
                            if (H[i, j] != -2)
                            {
                                pList = new List<PointD>();
                                i2 = i;
                                j2 = j;
                                a2x = X[j2];
                                a2y = Y[i2] + H[i2, j2] * ny;
                                i1 = i2;
                                if (HB[1, i, j] == 0)
                                {
                                    j1 = -1;
                                    aEndPoint.sPoint.X = X[j];
                                    aEndPoint.sPoint.Y = Y[i];
                                }
                                else
                                {
                                    j1 = j2;
                                    aEndPoint.sPoint.X = X[j];
                                    aEndPoint.sPoint.Y = Y[i + 1];
                                }
                                aPoint = new PointD();
                                aPoint.X = a2x;
                                aPoint.Y = a2y;
                                pList.Add(aPoint);

                                aEndPoint.Index = lineNum + cLineList.Count;
                                aEndPoint.Point = aPoint;
                                aEndPoint.BorderIdx = HB[0, i, j];
                                _endPointList.Add(aEndPoint);

                                aLine = new PolyLine();
                                aLine.Type = "Border";
                                aLine.BorderIdx = HB[0, i, j];
                                while (true)
                                {
                                    if (TraceIsoline_UndefData(i1, i2, ref H, ref S, j1, j2, X, Y, nx, ny, a2x, ref i3, ref j3, ref a3x,                                                    ref a3y, ref isS))
                                    {
                                        aPoint = new PointD();
                                        aPoint.X = a3x;
                                        aPoint.Y = a3y;
                                        pList.Add(aPoint);
                                        if (isS)
                                        {
                                            if (SB[0, i3, j3] > -1)
                                            {
                                                if (SB[1, i3, j3] == 0)
                                                {
                                                    aEndPoint.sPoint.X = X[j3 + 1];
                                                    aEndPoint.sPoint.Y = Y[i3];
                                                }
                                                else
                                                {
                                                    aEndPoint.sPoint.X = X[j3];
                                                    aEndPoint.sPoint.Y = Y[i3];
                                                }
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (HB[0, i3, j3] > -1)
                                            {
                                                if (HB[1, i3, j3] == 0)
                                                {
                                                    aEndPoint.sPoint.X = X[j3];
                                                    aEndPoint.sPoint.Y = Y[i3];
                                                }
                                                else
                                                {
                                                    aEndPoint.sPoint.X = X[j3];
                                                    aEndPoint.sPoint.Y = Y[i3 + 1];
                                                }
                                                break;
                                            }
                                        }
                                        a2x = a3x;
                                        a2y = a3y;
                                        i1 = i2;
                                        j1 = j2;
                                        i2 = i3;
                                        j2 = j3;
                                    }
                                    else
                                    {
                                        aLine.Type = "Error";
                                        break;
                                    }
                                }
                                H[i, j] = -2;
                                if (pList.Count > 1 && aLine.Type != "Error")
                                {
                                    aEndPoint.Point = aPoint;
                                    _endPointList.Add(aEndPoint);

                                    aLine.Value = W;
                                    aLine.PointList = pList;
                                    cLineList.Add(aLine);
                                }
                                else
                                    _endPointList.RemoveAt(_endPointList.Count - 1);

                            }
                        }
                    }
                }
            }

            //---- Clear border points
            for (j = 0; j < n - 1; j++)
            {
                if (S[0, j] != -2)
                    S[0, j] = -2;
                if (S[m - 1, j] != -2)
                    S[m - 1, j] = -2;
            }

            for (i = 0; i < m - 1; i++)
            {
                if (H[i, 0] != -2)
                    H[i, 0] = -2;
                if (H[i, n - 1] != -2)
                    H[i, n - 1] = -2;
            }

            //---- Tracing close lines
            for (i = 1; i < m - 2; i++)
            {
                for (j = 1; j < n - 1; j++)
                {
                    if (H[i, j] != -2)
                    {
                        List<PointD> pointList = new List<PointD>();
                        i2 = i;
                        j2 = j;
                        a2x = X[j2];
                        a2y = Y[i] + H[i, j2] * ny;
                        j1 = -1;
                        i1 = i2;
                        sx = a2x;
                        sy = a2y;
                        aPoint = new PointD();
                        aPoint.X = a2x;
                        aPoint.Y = a2y;
                        pointList.Add(aPoint);
                        aLine = new PolyLine();
                        aLine.Type = "Close";

                        while (true)
                        {
                            if (TraceIsoline_UndefData(i1, i2, ref H, ref S, j1, j2, X, Y, nx, ny, a2x, ref i3, ref j3, ref a3x, ref a3y, ref isS))
                            {
                                aPoint = new PointD();
                                aPoint.X = a3x;
                                aPoint.Y = a3y;
                                pointList.Add(aPoint);
                                if (Math.Abs(a3y - sy) < 0.000001 && Math.Abs(a3x - sx) < 0.000001)
                                    break;

                                a2x = a3x;
                                a2y = a3y;
                                i1 = i2;
                                j1 = j2;
                                i2 = i3;
                                j2 = j3;
                                //If X[j2] < a2x && i2 = 0 )
                                //    aLine.type = "Error"
                                //    Exit Do
                                //End If
                            }
                            else
                            {
                                aLine.Type = "Error";
                                break;
                            }
                        }
                        H[i, j] = -2;
                        if (pointList.Count > 1 && aLine.Type != "Error")
                        {
                            aLine.Value = W;
                            aLine.PointList = pointList;
                            cLineList.Add(aLine);
                        }
                    }
                }
            }

            for (i = 1; i < m - 1; i++)
            {
                for (j = 1; j < n - 2; j++)
                {
                    if (S[i, j] != -2)
                    {
                        List<PointD> pointList = new List<PointD>();
                        i2 = i;
                        j2 = j;
                        a2x = X[j2] + S[i, j] * nx;
                        a2y = Y[i];
                        j1 = j2;
                        i1 = -1;
                        sx = a2x;
                        sy = a2y;
                        aPoint = new PointD();
                        aPoint.X = a2x;
                        aPoint.Y = a2y;
                        pointList.Add(aPoint);
                        aLine = new PolyLine();
                        aLine.Type = "Close";

                        while (true)
                        {
                            if (TraceIsoline_UndefData(i1, i2, ref H, ref S, j1, j2, X, Y, nx, ny, a2x, ref i3, ref j3, ref a3x, ref a3y, ref isS))
                            {
                                aPoint = new PointD();
                                aPoint.X = a3x;
                                aPoint.Y = a3y;
                                pointList.Add(aPoint);
                                if (Math.Abs(a3y - sy) < 0.000001 && Math.Abs(a3x - sx) < 0.000001)
                                    break;

                                a2x = a3x;
                                a2y = a3y;
                                i1 = i2;
                                j1 = j2;
                                i2 = i3;
                                j2 = j3;
                            }
                            else
                            {
                                aLine.Type = "Error";
                                break;
                            }
                        }
                        S[i, j] = -2;
                        if (pointList.Count > 1 && aLine.Type != "Error")
                        {
                            aLine.Value = W;
                            aLine.PointList = pointList;
                            cLineList.Add(aLine);
                        }
                    }
                }
            }

            return cLineList;
        }

        private static bool TraceIsoline_UndefData(int i1, int i2, ref double[,] H, ref double[,] S, int j1, int j2, double[] X,
            double[] Y, double nx, double ny, double a2x, ref int i3,
            ref int j3, ref double a3x, ref double a3y, ref bool isS)
        {
            bool canTrace = true;
            if (i1 < i2)    //---- Trace from bottom
            {
                if (H[i2, j2] != -2 && H[i2, j2 + 1] != -2)
                {
                    if (H[i2, j2] < H[i2, j2 + 1])
                    {
                        a3x = X[j2];
                        a3y = Y[i2] + H[i2, j2] * ny;
                        i3 = i2;
                        j3 = j2;
                        H[i3, j3] = -2;
                        isS = false;
                    }
                    else
                    {
                        a3x = X[j2 + 1];
                        a3y = Y[i2] + H[i2, j2 + 1] * ny;
                        i3 = i2;
                        j3 = j2 + 1;
                        H[i3, j3] = -2;
                        isS = false;
                    }
                }
                else if (H[i2, j2] != -2 && H[i2, j2 + 1] == -2)
                {
                    a3x = X[j2];
                    a3y = Y[i2] + H[i2, j2] * ny;
                    i3 = i2;
                    j3 = j2;
                    H[i3, j3] = -2;
                    isS = false;
                }
                else if (H[i2, j2] == -2 && H[i2, j2 + 1] != -2)
                {
                    a3x = X[j2 + 1];
                    a3y = Y[i2] + H[i2, j2 + 1] * ny;
                    i3 = i2;
                    j3 = j2 + 1;
                    H[i3, j3] = -2;
                    isS = false;
                }
                else if (S[i2 + 1, j2] != -2)
                {
                    a3x = X[j2] + S[i2 + 1, j2] * nx;
                    a3y = Y[i2 + 1];
                    i3 = i2 + 1;
                    j3 = j2;
                    S[i3, j3] = -2;
                    isS = true;
                }
                else
                    canTrace = false;
            }
            else if (j1 < j2)    //---- Trace from left
            {
                if (S[i2, j2] != -2 && S[i2 + 1, j2] != -2)
                {
                    if (S[i2, j2] < S[i2 + 1, j2])
                    {
                        a3x = X[j2] + S[i2, j2] * nx;
                        a3y = Y[i2];
                        i3 = i2;
                        j3 = j2;
                        S[i3, j3] = -2;
                        isS = true;
                    }
                    else
                    {
                        a3x = X[j2] + S[i2 + 1, j2] * nx;
                        a3y = Y[i2 + 1];
                        i3 = i2 + 1;
                        j3 = j2;
                        S[i3, j3] = -2;
                        isS = true;
                    }
                }
                else if (S[i2, j2] != -2 && S[i2 + 1, j2] == -2)
                {
                    a3x = X[j2] + S[i2, j2] * nx;
                    a3y = Y[i2];
                    i3 = i2;
                    j3 = j2;
                    S[i3, j3] = -2;
                    isS = true;
                }
                else if (S[i2, j2] == -2 && S[i2 + 1, j2] != -2)
                {
                    a3x = X[j2] + S[i2 + 1, j2] * nx;
                    a3y = Y[i2 + 1];
                    i3 = i2 + 1;
                    j3 = j2;
                    S[i3, j3] = -2;
                    isS = true;
                }
                else if (H[i2, j2 + 1] != -2)
                {
                    a3x = X[j2 + 1];
                    a3y = Y[i2] + H[i2, j2 + 1] * ny;
                    i3 = i2;
                    j3 = j2 + 1;
                    H[i3, j3] = -2;
                    isS = false;
                }
                else
                    canTrace = false;

            }
            else if (X[j2] < a2x)    //---- Trace from top
            {
                if (H[i2 - 1, j2] != -2 && H[i2 - 1, j2 + 1] != -2)
                {
                    if (H[i2 - 1, j2] > H[i2 - 1, j2 + 1])    //---- < changed to >
                    {
                        a3x = X[j2];
                        a3y = Y[i2 - 1] + H[i2 - 1, j2] * ny;
                        i3 = i2 - 1;
                        j3 = j2;
                        H[i3, j3] = -2;
                        isS = false;
                    }
                    else
                    {
                        a3x = X[j2 + 1];
                        a3y = Y[i2 - 1] + H[i2 - 1, j2 + 1] * ny;
                        i3 = i2 - 1;
                        j3 = j2 + 1;
                        H[i3, j3] = -2;
                        isS = false;
                    }
                }
                else if (H[i2 - 1, j2] != -2 && H[i2 - 1, j2 + 1] == -2)
                {
                    a3x = X[j2];
                    a3y = Y[i2 - 1] + H[i2 - 1, j2] * ny;
                    i3 = i2 - 1;
                    j3 = j2;
                    H[i3, j3] = -2;
                    isS = false;
                }
                else if (H[i2 - 1, j2] == -2 && H[i2 - 1, j2 + 1] != -2)
                {
                    a3x = X[j2 + 1];
                    a3y = Y[i2 - 1] + H[i2 - 1, j2 + 1] * ny;
                    i3 = i2 - 1;
                    j3 = j2 + 1;
                    H[i3, j3] = -2;
                    isS = false;
                }
                else if (S[i2 - 1, j2] != -2)
                {
                    a3x = X[j2] + S[i2 - 1, j2] * nx;
                    a3y = Y[i2 - 1];
                    i3 = i2 - 1;
                    j3 = j2;
                    S[i3, j3] = -2;
                    isS = true;
                }
                else
                    canTrace = false;
            }
            else    //---- Trace from right
            {
                if (S[i2 + 1, j2 - 1] != -2 && S[i2, j2 - 1] != -2)
                {
                    if (S[i2 + 1, j2 - 1] > S[i2, j2 - 1])    //---- < changed to >
                    {
                        a3x = X[j2 - 1] + S[i2 + 1, j2 - 1] * nx;
                        a3y = Y[i2 + 1];
                        i3 = i2 + 1;
                        j3 = j2 - 1;
                        S[i3, j3] = -2;
                        isS = true;
                    }
                    else
                    {
                        a3x = X[j2 - 1] + S[i2, j2 - 1] * nx;
                        a3y = Y[i2];
                        i3 = i2;
                        j3 = j2 - 1;
                        S[i3, j3] = -2;
                        isS = true;
                    }
                }
                else if (S[i2 + 1, j2 - 1] != -2 && S[i2, j2 - 1] == -2)
                {
                    a3x = X[j2 - 1] + S[i2 + 1, j2 - 1] * nx;
                    a3y = Y[i2 + 1];
                    i3 = i2 + 1;
                    j3 = j2 - 1;
                    S[i3, j3] = -2;
                    isS = true;
                }
                else if (S[i2 + 1, j2 - 1] == -2 && S[i2, j2 - 1] != -2)
                {
                    a3x = X[j2 - 1] + S[i2, j2 - 1] * nx;
                    a3y = Y[i2];
                    i3 = i2;
                    j3 = j2 - 1;
                    S[i3, j3] = -2;
                    isS = true;
                }
                else if (H[i2, j2 - 1] != -2)
                {
                    a3x = X[j2 - 1];
                    a3y = Y[i2] + H[i2, j2 - 1] * ny;
                    i3 = i2;
                    j3 = j2 - 1;
                    H[i3, j3] = -2;
                    isS = false;
                }
                else
                    canTrace = false;
            }

            return canTrace;
        }

        private static object[] TraceIsoline(int i1, int i2, ref double[,] H, ref double[,] S, int j1, int j2, double[] X,
            double[] Y, double nx, double ny, double a2x)
        {
            int i3, j3;
            double a3x, a3y;
            if (i1 < i2)    //---- Trace from bottom
            {
                if (H[i2, j2] != -2 && H[i2, j2 + 1] != -2)
                {
                    if (H[i2, j2] < H[i2, j2 + 1])
                    {
                        a3x = X[j2];
                        a3y = Y[i2] + H[i2, j2] * ny;
                        i3 = i2;
                        j3 = j2;
                        H[i3, j3] = -2;
                    }
                    else
                    {
                        a3x = X[j2 + 1];
                        a3y = Y[i2] + H[i2, j2 + 1] * ny;
                        i3 = i2;
                        j3 = j2 + 1;
                        H[i3, j3] = -2;
                    }
                }
                else if (H[i2, j2] != -2 && H[i2, j2 + 1] == -2)
                {
                    a3x = X[j2];
                    a3y = Y[i2] + H[i2, j2] * ny;
                    i3 = i2;
                    j3 = j2;
                    H[i3, j3] = -2;
                }
                else if (H[i2, j2] == -2 && H[i2, j2 + 1] != -2)
                {
                    a3x = X[j2 + 1];
                    a3y = Y[i2] + H[i2, j2 + 1] * ny;
                    i3 = i2;
                    j3 = j2 + 1;
                    H[i3, j3] = -2;
                }
                else
                {
                    a3x = X[j2] + S[i2 + 1, j2] * nx;
                    a3y = Y[i2 + 1];
                    i3 = i2 + 1;
                    j3 = j2;
                    S[i3, j3] = -2;
                }
            }
            else if (j1 < j2)    //---- Trace from left
            {
                if (S[i2, j2] != -2 && S[i2 + 1, j2] != -2)
                {
                    if (S[i2, j2] < S[i2 + 1, j2])
                    {
                        a3x = X[j2] + S[i2, j2] * nx;
                        a3y = Y[i2];
                        i3 = i2;
                        j3 = j2;
                        S[i3, j3] = -2;
                    }
                    else
                    {
                        a3x = X[j2] + S[i2 + 1, j2] * nx;
                        a3y = Y[i2 + 1];
                        i3 = i2 + 1;
                        j3 = j2;
                        S[i3, j3] = -2;
                    }
                }
                else if (S[i2, j2] != -2 && S[i2 + 1, j2] == -2)
                {
                    a3x = X[j2] + S[i2, j2] * nx;
                    a3y = Y[i2];
                    i3 = i2;
                    j3 = j2;
                    S[i3, j3] = -2;
                }
                else if (S[i2, j2] == -2 && S[i2 + 1, j2] != -2)
                {
                    a3x = X[j2] + S[i2 + 1, j2] * nx;
                    a3y = Y[i2 + 1];
                    i3 = i2 + 1;
                    j3 = j2;
                    S[i3, j3] = -2;
                }
                else
                {
                    a3x = X[j2 + 1];
                    a3y = Y[i2] + H[i2, j2 + 1] * ny;
                    i3 = i2;
                    j3 = j2 + 1;
                    H[i3, j3] = -2;
                }
            }
            else if (X[j2] < a2x)    //---- Trace from top
            {
                if (H[i2 - 1, j2] != -2 && H[i2 - 1, j2 + 1] != -2)
                {
                    if (H[i2 - 1, j2] > H[i2 - 1, j2 + 1])    //---- < changed to >
                    {
                        a3x = X[j2];
                        a3y = Y[i2 - 1] + H[i2 - 1, j2] * ny;
                        i3 = i2 - 1;
                        j3 = j2;
                        H[i3, j3] = -2;
                    }
                    else
                    {
                        a3x = X[j2 + 1];
                        a3y = Y[i2 - 1] + H[i2 - 1, j2 + 1] * ny;
                        i3 = i2 - 1;
                        j3 = j2 + 1;
                        H[i3, j3] = -2;
                    }
                }
                else if (H[i2 - 1, j2] != -2 && H[i2 - 1, j2 + 1] == -2)
                {
                    a3x = X[j2];
                    a3y = Y[i2 - 1] + H[i2 - 1, j2] * ny;
                    i3 = i2 - 1;
                    j3 = j2;
                    H[i3, j3] = -2;
                }
                else if (H[i2 - 1, j2] == -2 && H[i2 - 1, j2 + 1] != -2)
                {
                    a3x = X[j2 + 1];
                    a3y = Y[i2 - 1] + H[i2 - 1, j2 + 1] * ny;
                    i3 = i2 - 1;
                    j3 = j2 + 1;
                    H[i3, j3] = -2;
                }
                else
                {
                    a3x = X[j2] + S[i2 - 1, j2] * nx;
                    a3y = Y[i2 - 1];
                    i3 = i2 - 1;
                    j3 = j2;
                    S[i3, j3] = -2;
                }
            }
            else    //---- Trace from right
            {
                if (S[i2 + 1, j2 - 1] != -2 && S[i2, j2 - 1] != -2)
                {
                    if (S[i2 + 1, j2 - 1] > S[i2, j2 - 1])    //---- < changed to >
                    {
                        a3x = X[j2 - 1] + S[i2 + 1, j2 - 1] * nx;
                        a3y = Y[i2 + 1];
                        i3 = i2 + 1;
                        j3 = j2 - 1;
                        S[i3, j3] = -2;
                    }
                    else
                    {
                        a3x = X[j2 - 1] + S[i2, j2 - 1] * nx;
                        a3y = Y[i2];
                        i3 = i2;
                        j3 = j2 - 1;
                        S[i3, j3] = -2;
                    }
                }
                else if (S[i2 + 1, j2 - 1] != -2 && S[i2, j2 - 1] == -2)
                {
                    a3x = X[j2 - 1] + S[i2 + 1, j2 - 1] * nx;
                    a3y = Y[i2 + 1];
                    i3 = i2 + 1;
                    j3 = j2 - 1;
                    S[i3, j3] = -2;
                }
                else if (S[i2 + 1, j2 - 1] == -2 && S[i2, j2 - 1] != -2)
                {
                    a3x = X[j2 - 1] + S[i2, j2 - 1] * nx;
                    a3y = Y[i2];
                    i3 = i2;
                    j3 = j2 - 1;
                    S[i3, j3] = -2;
                }
                else
                {
                    a3x = X[j2 - 1];
                    a3y = Y[i2] + H[i2, j2 - 1] * ny;
                    i3 = i2;
                    j3 = j2 - 1;
                    H[i3, j3] = -2;
                }
            }

            return new Object[] { i3, j3, a3x, a3y };
        }

        private static List<PolyLine> Isoline_Bottom(double[,] S0, double[] X, double[] Y, double W, double nx, double ny,
            ref double[,] S, ref double[,] H)
        {
            List<PolyLine> bLineList = new List<PolyLine>();
            int m, n, j;
            m = S0.GetLength(0);
            n = S0.GetLength(1);

            int i1, i2, j1 = 0, j2, i3, j3;
            double a2x, a2y, a3x, a3y;
            object[] returnVal;
            PointD aPoint;
            PolyLine aLine;
            for (j = 0; j < n - 1; j++)    //---- Trace isoline from bottom
            {
                if (S[0, j] != -2)    //---- Has tracing value
                {
                    List<PointD> pointList = new List<PointD>();
                    i2 = 0;
                    j2 = j;
                    a2x = X[j] + S[0, j] * nx;    //---- x of first point
                    a2y = Y[0];                   //---- y of first point
                    i1 = -1;
                    aPoint = new PointD();
                    aPoint.X = a2x;
                    aPoint.Y = a2y;
                    pointList.Add(aPoint);
                    while (true)
                    {
                        returnVal = TraceIsoline(i1, i2, ref H, ref S, j1, j2, X, Y, nx, ny, a2x);
                        i3 = (int)returnVal[0];
                        j3 = (int)returnVal[1];
                        a3x = (double)returnVal[2];
                        a3y = (double)returnVal[3];
                        aPoint = new PointD();
                        aPoint.X = a3x;
                        aPoint.Y = a3y;
                        pointList.Add(aPoint);
                        if (i3 == m - 1 || j3 == n - 1 || a3y == Y[0] || a3x == X[0])
                            break;

                        a2x = a3x;
                        a2y = a3y;
                        i1 = i2;
                        j1 = j2;
                        i2 = i3;
                        j2 = j3;
                    }
                    S[0, j] = -2;
                    if (pointList.Count > 4)
                    {
                        aLine = new PolyLine();
                        aLine.Value = W;
                        aLine.Type = "Bottom";
                        aLine.PointList = pointList;
                        //m_LineList.Add(aLine);
                        bLineList.Add(aLine);
                    }
                }
            }

            return bLineList;
        }

        private static List<PolyLine> Isoline_Left(double[,] S0, double[] X, double[] Y, double W, double nx, double ny,
            ref double[,] S, ref double[,] H)
        {
            List<PolyLine> lLineList = new List<PolyLine>();
            int m, n, i;
            m = S0.GetLength(0);
            n = S0.GetLength(1);

            int i1, i2, j1, j2, i3, j3;
            double a2x, a2y, a3x, a3y;
            object[] returnVal;
            PointD aPoint;
            PolyLine aLine;
            for (i = 0; i < m - 1; i++)    //---- Trace isoline from Left
            {
                if (H[i, 0] != -2)
                {
                    List<PointD> pointList = new List<PointD>();
                    i2 = i;
                    j2 = 0;
                    a2x = X[0];
                    a2y = Y[i] + H[i, 0] * ny;
                    j1 = -1;
                    i1 = i2;
                    aPoint = new PointD();
                    aPoint.X = a2x;
                    aPoint.Y = a2y;
                    pointList.Add(aPoint);
                    while (true)
                    {
                        returnVal = TraceIsoline(i1, i2, ref H, ref S, j1, j2, X, Y, nx, ny, a2x);
                        i3 = (int)returnVal[0];
                        j3 = (int)returnVal[1];
                        a3x = (double)returnVal[2];
                        a3y = (double)returnVal[3];
                        aPoint = new PointD();
                        aPoint.X = a3x;
                        aPoint.Y = a3y;
                        pointList.Add(aPoint);
                        if (i3 == m - 1 || j3 == n - 1 || a3y == Y[0] || a3x == X[0])
                            break;

                        a2x = a3x;
                        a2y = a3y;
                        i1 = i2;
                        j1 = j2;
                        i2 = i3;
                        j2 = j3;
                    }
                    if (pointList.Count > 4)
                    {
                        aLine = new PolyLine();
                        aLine.Value = W;
                        aLine.Type = "Left";
                        aLine.PointList = pointList;
                        //m_LineList.Add(aLine);
                        lLineList.Add(aLine);
                    }
                }
            }

            return lLineList;
        }

        private static List<PolyLine> Isoline_Top(double[,] S0, double[] X, double[] Y, double W, double nx, double ny,
                ref double[,] S, ref double[,] H)
        {
            List<PolyLine> tLineList = new List<PolyLine>();
            int m, n, j;
            m = S0.GetLength(0);
            n = S0.GetLength(1);

            int i1, i2, j1, j2, i3, j3;
            double a2x, a2y, a3x, a3y;
            object[] returnVal;
            PointD aPoint;
            PolyLine aLine;
            for (j = 0; j < n - 1; j++)
            {
                if (S[m - 1, j] != -2)
                {
                    List<PointD> pointList = new List<PointD>();
                    i2 = m - 1;
                    j2 = j;
                    a2x = X[j] + S[i2, j] * nx;
                    a2y = Y[i2];
                    i1 = i2;
                    j1 = j2;
                    aPoint = new PointD();
                    aPoint.X = a2x;
                    aPoint.Y = a2y;
                    pointList.Add(aPoint);
                    while (true)
                    {
                        returnVal = TraceIsoline(i1, i2, ref H, ref S, j1, j2, X, Y, nx, ny, a2x);
                        i3 = (int)returnVal[0];
                        j3 = (int)returnVal[1];
                        a3x = (double)returnVal[2];
                        a3y = (double)returnVal[3];
                        aPoint = new PointD();
                        aPoint.X = a3x;
                        aPoint.Y = a3y;
                        pointList.Add(aPoint);
                        if (i3 == m - 1 || j3 == n - 1 || a3y == Y[0] || a3x == X[0])
                            break;

                        a2x = a3x;
                        a2y = a3y;
                        i1 = i2;
                        j1 = j2;
                        i2 = i3;
                        j2 = j3;
                    }
                    S[m - 1, j] = -2;
                    if (pointList.Count > 4)
                    {
                        aLine = new PolyLine();
                        aLine.Value = W;
                        aLine.Type = "Top";
                        aLine.PointList = pointList;
                        //m_LineList.Add(aLine);
                        tLineList.Add(aLine);
                    }
                }
            }

            return tLineList;
        }

        private static List<PolyLine> Isoline_Right(double[,] S0, double[] X, double[] Y, double W, double nx, double ny,
                ref double[,] S, ref double[,] H)
        {
            List<PolyLine> rLineList = new List<PolyLine>();
            int m, n, i;
            m = S0.GetLength(0);
            n = S0.GetLength(1);

            int i1, i2, j1, j2, i3, j3;
            double a2x, a2y, a3x, a3y;
            object[] returnVal;
            PointD aPoint;
            PolyLine aLine;
            for (i = 0; i < m - 1; i++)
            {
                if (H[i, n - 1] != -2)
                {
                    List<PointD> pointList = new List<PointD>();
                    i2 = i;
                    j2 = n - 1;
                    a2x = X[j2];
                    a2y = Y[i] + H[i, j2] * ny;
                    j1 = j2;
                    i1 = i2;
                    aPoint = new PointD();
                    aPoint.X = a2x;
                    aPoint.Y = a2y;
                    pointList.Add(aPoint);
                    while (true)
                    {
                        returnVal = TraceIsoline(i1, i2, ref H, ref S, j1, j2, X, Y, nx, ny, a2x);
                        i3 = (int)returnVal[0];
                        j3 = (int)returnVal[1];
                        a3x = (double)returnVal[2];
                        a3y = (double)returnVal[3];
                        aPoint = new PointD();
                        aPoint.X = a3x;
                        aPoint.Y = a3y;
                        pointList.Add(aPoint);
                        if (i3 == m - 1 || j3 == n - 1 || a3y == Y[0] || a3x == X[0])
                            break;

                        a2x = a3x;
                        a2y = a3y;
                        i1 = i2;
                        j1 = j2;
                        i2 = i3;
                        j2 = j3;
                    }
                    if (pointList.Count > 4)
                    {
                        aLine = new PolyLine();
                        aLine.Value = W;
                        aLine.Type = "Right";
                        aLine.PointList = pointList;
                        //m_LineList.Add(aLine)
                        rLineList.Add(aLine);
                    }
                }
            }

            return rLineList;
        }

        private static List<PolyLine> Isoline_Close(double[,] S0, double[] X, double[] Y, double W, double nx, double ny,
                ref double[,] S, ref double[,] H)
        {
            List<PolyLine> cLineList = new List<PolyLine>();
            int m, n, i, j;
            m = S0.GetLength(0);
            n = S0.GetLength(1);

            int i1, i2, j1, j2, i3, j3;
            double a2x, a2y, a3x, a3y, sx, sy;
            object[] returnVal;
            PointD aPoint;
            PolyLine aLine;
            for (i = 1; i < m - 2; i++)
            {
                for (j = 1; j < n - 1; j++)
                {
                    if (H[i, j] != -2)
                    {
                        List<PointD> pointList = new List<PointD>();
                        i2 = i;
                        j2 = j;
                        a2x = X[j2];
                        a2y = Y[i] + H[i, j2] * ny;
                        j1 = 0;
                        i1 = i2;
                        sx = a2x;
                        sy = a2y;
                        aPoint = new PointD();
                        aPoint.X = a2x;
                        aPoint.Y = a2y;
                        pointList.Add(aPoint);
                        while (true)
                        {
                            returnVal = TraceIsoline(i1, i2, ref H, ref S, j1, j2, X, Y, nx, ny, a2x);
                            i3 = (int)returnVal[0];
                            j3 = (int)returnVal[1];
                            a3x = (double)returnVal[2];
                            a3y = (double)returnVal[3];
                            if (i3 == 0 && j3 == 0)
                                break;

                            aPoint = new PointD();
                            aPoint.X = a3x;
                            aPoint.Y = a3y;
                            pointList.Add(aPoint);
                            if (Math.Abs(a3y - sy) < 0.000001 && Math.Abs(a3x - sx) < 0.000001)
                                break;

                            a2x = a3x;
                            a2y = a3y;
                            i1 = i2;
                            j1 = j2;
                            i2 = i3;
                            j2 = j3;
                            if (i2 == m - 1 || j2 == n - 1)
                                break;

                        }
                        H[i, j] = -2;
                        if (pointList.Count > 4)
                        {
                            aLine = new PolyLine();
                            aLine.Value = W;
                            aLine.Type = "Close";
                            aLine.PointList = pointList;
                            //m_LineList.Add(aLine)
                            cLineList.Add(aLine);
                        }
                    }
                }
            }

            for (i = 1; i < m - 1; i++)
            {
                for (j = 1; j < n - 2; j++)
                {
                    if (S[i, j] != -2)
                    {
                        List<PointD> pointList = new List<PointD>();
                        i2 = i;
                        j2 = j;
                        a2x = X[j2] + S[i, j] * nx;
                        a2y = Y[i];
                        j1 = j2;
                        i1 = 0;
                        sx = a2x;
                        sy = a2y;
                        aPoint = new PointD();
                        aPoint.X = a2x;
                        aPoint.Y = a2y;
                        pointList.Add(aPoint);
                        while (true)
                        {
                            returnVal = TraceIsoline(i1, i2, ref H, ref S, j1, j2, X, Y, nx, ny, a2x);
                            i3 = (int)returnVal[0];
                            j3 = (int)returnVal[1];
                            a3x = (double)returnVal[2];
                            a3y = (double)returnVal[3];
                            aPoint = new PointD();
                            aPoint.X = a3x;
                            aPoint.Y = a3y;
                            pointList.Add(aPoint);
                            if (Math.Abs(a3y - sy) < 0.000001 && Math.Abs(a3x - sx) < 0.000001)
                                break;

                            a2x = a3x;
                            a2y = a3y;
                            i1 = i2;
                            j1 = j2;
                            i2 = i3;
                            j2 = j3;
                            if (i2 == m - 1 || j2 == n - 1)
                                break;
                        }
                        S[i, j] = -2;
                        if (pointList.Count > 4)
                        {
                            aLine = new PolyLine();
                            aLine.Value = W;
                            aLine.Type = "Close";
                            aLine.PointList = pointList;
                            //m_LineList.Add(aLine)
                            cLineList.Add(aLine);
                        }
                    }
                }
            }

            return cLineList;
        }

        private static List<Polygon> TracingPolygons(List<PolyLine> LineList, List<BorderPoint> borderList, Extent bBound, double[] contour)
        {
            if (LineList.Count == 0)
                return new List<Polygon>();

            List<Polygon> aPolygonList = new List<Polygon>();
            //List<Polygon> newPolygonlist = new List<Polygon>();
            List<PolyLine> aLineList = new List<PolyLine>();
            PolyLine aLine;
            PointD aPoint;
            Polygon aPolygon;
            Extent aBound;
            int i, j;

            aLineList = new List<PolyLine>(LineList);

            //---- Tracing border polygon
            List<PointD> aPList;
            List<PointD> newPList = new List<PointD>();
            BorderPoint bP;
            int[] timesArray = new int[borderList.Count - 1];
            for (i = 0; i < timesArray.Length; i++)
                timesArray[i] = 0;

            int pIdx, pNum, vNum;
            double aValue = 0, bValue = 0;
            List<BorderPoint> lineBorderList = new List<BorderPoint>();

            pNum = borderList.Count - 1;
            for (i = 0; i < pNum; i++)
            {
                if ((borderList[i]).Id == -1)
                    continue;

                pIdx = i;
                aPList = new List<PointD>();
                lineBorderList.Add(borderList[i]);

                //---- Clockwise traceing
                if (timesArray[pIdx] < 2)
                {
                    aPList.Add((borderList[pIdx]).Point);
                    pIdx += 1;
                    if (pIdx == pNum)
                        pIdx = 0;

                    vNum = 0;
                    while (true)
                    {
                        bP = borderList[pIdx];
                        if (bP.Id == -1)    //---- Not endpoint of contour
                        {
                            if (timesArray[pIdx] == 1)
                                break;

                            aPList.Add(bP.Point);
                            timesArray[pIdx] += +1;
                        }
                        else    //---- endpoint of contour
                        {
                            if (timesArray[pIdx] == 2)
                                break;

                            timesArray[pIdx] += +1;
                            aLine = aLineList[bP.Id];
                            if (vNum == 0)
                            {
                                aValue = aLine.Value;
                                bValue = aLine.Value;
                                vNum += 1;
                            }
                            else
                            {
                                if (aValue == bValue)
                                {
                                    if (aLine.Value > aValue)
                                        bValue = aLine.Value;
                                    else if (aLine.Value < aValue)
                                        aValue = aLine.Value;

                                    vNum += 1;
                                }
                            }
                            newPList = new List<PointD>(aLine.PointList);
                            aPoint = newPList[0];
                            //If Not (Math.Abs(bP.point.X - aPoint.X) < 0.000001 And _
                            //  Math.Abs(bP.point.Y - aPoint.Y) < 0.000001) Then    '---- Start point
                            if (!(bP.Point.X == aPoint.X && bP.Point.Y == aPoint.Y))    //---- Start point
                                newPList.Reverse();

                            aPList.AddRange(newPList);
                            for (j = 0; j < borderList.Count - 1; j++)
                            {
                                if (j != pIdx)
                                {
                                    if ((borderList[j]).Id == bP.Id)
                                    {
                                        pIdx = j;
                                        timesArray[pIdx] += +1;
                                        break;
                                    }
                                }
                            }
                        }

                        if (pIdx == i)
                        {
                            if (aPList.Count > 0)
                            {
                                aPolygon = new Polygon();
                                aPolygon.LowValue = aValue;
                                aPolygon.HighValue = bValue;
                                aBound = new Extent();
                                aPolygon.Area = GetExtentAndArea(aPList, ref aBound);
                                aPolygon.IsClockWise = true;
                                aPolygon.StartPointIdx = lineBorderList.Count - 1;
                                aPolygon.Extent = aBound;
                                aPolygon.OutLine.PointList = aPList;
                                aPolygon.OutLine.Value = aValue;
                                aPolygon.IsHighCenter = true;
                                aPolygon.OutLine.Type = "Border";
                                aPolygonList.Add(aPolygon);
                            }
                            break;
                        }
                        pIdx += 1;
                        if (pIdx == pNum)
                            pIdx = 0;

                    }
                }


                //---- Anticlockwise traceing
                pIdx = i;
                if (timesArray[pIdx] < 2)
                {
                    aPList = new List<PointD>();
                    aPList.Add((borderList[pIdx]).Point);
                    pIdx += -1;
                    if (pIdx == -1)
                        pIdx = pNum - 1;

                    vNum = 0;
                    while (true)
                    {
                        bP = borderList[pIdx];
                        if (bP.Id == -1)    //---- Not endpoint of contour
                        {
                            if (timesArray[pIdx] == 1)
                                break;
                            aPList.Add(bP.Point);
                            timesArray[pIdx] += +1;
                        }
                        else    //---- endpoint of contour
                        {
                            if (timesArray[pIdx] == 2)
                                break;

                            timesArray[pIdx] += +1;
                            aLine = aLineList[bP.Id];
                            if (vNum == 0)
                            {
                                aValue = aLine.Value;
                                bValue = aLine.Value;
                                vNum += 1;
                            }
                            else
                            {
                                if (aValue == bValue)
                                {
                                    if (aLine.Value > aValue)
                                        bValue = aLine.Value;
                                    else if (aLine.Value < aValue)
                                        aValue = aLine.Value;

                                    vNum += 1;
                                }
                            }
                            newPList = new List<PointD>(aLine.PointList);
                            aPoint = newPList[0];
                            //If Not (Math.Abs(bP.point.X - aPoint.X) < 0.000001 And _
                            //  Math.Abs(bP.point.Y - aPoint.Y) < 0.000001) Then    '---- Start point
                            if (!(bP.Point.X == aPoint.X && bP.Point.Y == aPoint.Y))    //---- Start point
                                newPList.Reverse();

                            aPList.AddRange(newPList);
                            for (j = 0; j < borderList.Count - 1; j++)
                            {
                                if (j != pIdx)
                                {
                                    if ((borderList[j]).Id == bP.Id)
                                    {
                                        pIdx = j;
                                        timesArray[pIdx] += +1;
                                        break;
                                    }
                                }
                            }
                        }

                        if (pIdx == i)
                        {
                            if (aPList.Count > 0)
                            {
                                aPolygon = new Polygon();
                                aPolygon.LowValue = aValue;
                                aPolygon.HighValue = bValue;
                                aBound = new Extent();
                                aPolygon.Area = GetExtentAndArea(aPList, ref aBound);
                                aPolygon.IsClockWise = false;
                                aPolygon.StartPointIdx = lineBorderList.Count - 1;
                                aPolygon.Extent = aBound;
                                aPolygon.OutLine.PointList = aPList;
                                aPolygon.OutLine.Value = aValue;
                                aPolygon.IsHighCenter = true;
                                aPolygon.OutLine.Type = "Border";
                                aPolygonList.Add(aPolygon);
                            }
                            break;
                        }
                        pIdx += -1;
                        if (pIdx == -1)
                            pIdx = pNum - 1;

                    }
                }
            }

            //---- tracing close polygons
            List<Polygon> cPolygonlist = new List<Polygon>();
            bool isInserted;
            for (i = 0; i < aLineList.Count; i++)
            {
                aLine = aLineList[i];
                if (aLine.Type == "Close" && aLine.PointList.Count > 0)
                {
                    aPolygon = new Polygon();
                    aPolygon.LowValue = aLine.Value;
                    aPolygon.HighValue = aLine.Value;
                    aBound = new Extent();
                    aPolygon.Area = GetExtentAndArea(aLine.PointList, ref aBound);
                    aPolygon.IsClockWise = IsClockwise(aLine.PointList);
                    aPolygon.Extent = aBound;
                    aPolygon.OutLine = aLine;
                    aPolygon.IsHighCenter = true;

                    //---- Sort from big to small
                    isInserted = false;
                    for (j = 0; j < cPolygonlist.Count; j++)
                    {
                        if (aPolygon.Area > (cPolygonlist[j]).Area)
                        {
                            cPolygonlist.Insert(j, aPolygon);
                            isInserted = true;
                            break;
                        }
                    }
                    if (!isInserted)
                        cPolygonlist.Add(aPolygon);

                }
            }

            //---- Juge isHighCenter for border polygons
            Extent cBound1, cBound2;
            if (aPolygonList.Count > 0)
            {
                int outPIdx;
                bool IsSides = true;
                bool IfSameValue = false;    //---- If all boder polygon lines have same value
                aPolygon = aPolygonList[0];
                if (aPolygon.LowValue == aPolygon.HighValue)
                {
                    IsSides = false;
                    outPIdx = aPolygon.StartPointIdx;
                    while (true)
                    {
                        if (aPolygon.IsClockWise)
                        {
                            outPIdx = outPIdx - 1;
                            if (outPIdx == -1)
                                outPIdx = lineBorderList.Count - 1;

                        }
                        else
                        {
                            outPIdx = outPIdx + 1;
                            if (outPIdx == lineBorderList.Count)
                                outPIdx = 0;

                        }
                        bP = lineBorderList[outPIdx];
                        aLine = aLineList[bP.Id];
                        if (aLine.Value == aPolygon.LowValue)
                        {
                            if (outPIdx == aPolygon.StartPointIdx)
                            {
                                IfSameValue = true;
                                break;
                            }
                            else
                                continue;

                        }
                        else
                        {
                            IfSameValue = false;
                            break;
                        }
                    }
                }

                if (IfSameValue)
                {
                    if (cPolygonlist.Count > 0)
                    {
                        Polygon cPolygon;
                        cPolygon = cPolygonlist[0];
                        cBound1 = cPolygon.Extent;
                        for (i = 0; i < aPolygonList.Count; i++)
                        {
                            aPolygon = aPolygonList[i];
                            cBound2 = aPolygon.Extent;
                            if (cBound1.xMin > cBound2.xMin && cBound1.yMin > cBound2.yMin &&
                              cBound1.xMax < cBound2.xMax && cBound1.yMax < cBound2.yMax)
                            {
                                aPolygon.IsHighCenter = false;                                
                            }
                            else
                            {
                                aPolygon.IsHighCenter = true;                                
                            }
                            //aPolygonList[i] = aPolygon;
                        }
                    }
                    else
                    {
                        bool tf = true;    //---- Temperal solution, not finished
                        for (i = 0; i < aPolygonList.Count; i++)
                        {
                            aPolygon = aPolygonList[i];
                            tf = !tf;
                            aPolygon.IsHighCenter = tf;
                            //aPolygonList[i] = aPolygon;                            
                        }
                    }
                }
                else
                {
                    for (i = 0; i < aPolygonList.Count; i++)
                    {
                        aPolygon = aPolygonList[i];
                        if (aPolygon.LowValue == aPolygon.HighValue)
                        {
                            IsSides = false;
                            outPIdx = aPolygon.StartPointIdx;
                            while (true)
                            {
                                if (aPolygon.IsClockWise)
                                {
                                    outPIdx = outPIdx - 1;
                                    if (outPIdx == -1)
                                        outPIdx = lineBorderList.Count - 1;
                                }
                                else
                                {
                                    outPIdx = outPIdx + 1;
                                    if (outPIdx == lineBorderList.Count)
                                        outPIdx = 0;

                                }
                                bP = lineBorderList[outPIdx];
                                aLine = aLineList[bP.Id];
                                if (aLine.Value == aPolygon.LowValue)
                                {
                                    if (outPIdx == aPolygon.StartPointIdx)
                                        break;
                                    else
                                    {
                                        IsSides = !IsSides;
                                        continue;
                                    }
                                }
                                else
                                {
                                    if (IsSides)
                                    {
                                        if (aLine.Value < aPolygon.LowValue)
                                        {
                                            aPolygon.IsHighCenter = false;
                                            //aPolygonList.Insert(i, aPolygon);
                                            //aPolygonList.RemoveAt(i + 1);
                                        }
                                    }
                                    else
                                    {
                                        if (aLine.Value > aPolygon.LowValue)
                                        {
                                            aPolygon.IsHighCenter = false;
                                            //aPolygonList.Insert(i, aPolygon);
                                            //aPolygonList.RemoveAt(i + 1);
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            else    //Add border polygon
            {
                //Get max & min contour values
                double max = aLineList[0].Value, min = aLineList[0].Value;
                foreach (PolyLine aPLine in aLineList)
                {
                    if (aPLine.Value > max)
                        max = aPLine.Value;
                    if (aPLine.Value < min)
                        min = aPLine.Value;
                }
                aPolygon = new Polygon();
                aLine = new PolyLine();
                aLine.Type = "Border";
                aLine.Value = contour[0];
                aPolygon.IsHighCenter = false;
                if (cPolygonlist.Count > 0)
                {
                    if ((cPolygonlist[0].LowValue == max))
                    {
                        aLine.Value = contour[contour.Length - 1];
                        aPolygon.IsHighCenter = true;
                    }
                }
                newPList.Clear();
                aPoint = new PointD();
                aPoint.X = bBound.xMin;
                aPoint.Y = bBound.yMin;
                newPList.Add(aPoint);
                aPoint = new PointD();
                aPoint.X = bBound.xMin;
                aPoint.Y = bBound.yMax;
                newPList.Add(aPoint);
                aPoint = new PointD();
                aPoint.X = bBound.xMax;
                aPoint.Y = bBound.yMax;
                newPList.Add(aPoint);
                aPoint = new PointD();
                aPoint.X = bBound.xMax;
                aPoint.Y = bBound.yMin;
                newPList.Add(aPoint);
                newPList.Add(newPList[0]);
                aLine.PointList = new List<PointD>(newPList);

                if (aLine.PointList.Count > 0)
                {
                    aPolygon.LowValue = aLine.Value;
                    aPolygon.HighValue = aLine.Value;
                    aBound = new Extent();
                    aPolygon.Area = GetExtentAndArea(aLine.PointList, ref aBound);
                    aPolygon.IsClockWise = IsClockwise(aLine.PointList);
                    aPolygon.Extent = aBound;
                    aPolygon.OutLine = aLine;
                    //aPolygon.IsHighCenter = false;
                    aPolygonList.Add(aPolygon);
                }
            }

            //---- Add close polygons to form total polygons list
            aPolygonList.AddRange(cPolygonlist);

            //---- Juge IsHighCenter for close polygons
            int polygonNum = aPolygonList.Count;
            Polygon bPolygon;
            for (i = polygonNum - 1; i >= 0; i--)
            {
                aPolygon = aPolygonList[i];
                if (aPolygon.OutLine.Type == "Close")
                {
                    cBound1 = aPolygon.Extent;
                    aValue = aPolygon.LowValue;
                    aPoint = aPolygon.OutLine.PointList[0];
                    for (j = i - 1; j >= 0; j--)
                    {
                        bPolygon = aPolygonList[j];
                        cBound2 = bPolygon.Extent;
                        bValue = bPolygon.LowValue;
                        newPList = new List<PointD>(bPolygon.OutLine.PointList);
                        if (PointInPolygon(newPList, aPoint))
                        {
                            if (cBound1.xMin > cBound2.xMin && cBound1.yMin > cBound2.yMin &&
                              cBound1.xMax < cBound2.xMax && cBound1.yMax < cBound2.yMax)
                            {
                                if (aValue < bValue)
                                {
                                    aPolygon.IsHighCenter = false;
                                    //aPolygonList[i] = aPolygon;                                    
                                }
                                else if (aValue == bValue)
                                {
                                    if (bPolygon.IsHighCenter)
                                    {
                                        aPolygon.IsHighCenter = false;
                                        //aPolygonList[i] = aPolygon;
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }

            return aPolygonList;
        }        

        private static List<Polygon> TracingPolygons(List<PolyLine> LineList, List<BorderPoint> borderList)
        {
            if (LineList.Count == 0)
                return new List<Polygon>();

            List<Polygon> aPolygonList = new List<Polygon>();
            List<PolyLine> aLineList = new List<PolyLine>();
            PolyLine aLine;
            PointD aPoint;
            Polygon aPolygon;
            Extent aBound;
            int i, j;

            aLineList = new List<PolyLine>(LineList);

            //---- Tracing border polygon
            List<PointD> aPList;
            List<PointD> newPList = new List<PointD>();
            BorderPoint bP;
            int[] timesArray = new int[borderList.Count - 1];
            for (i = 0; i < timesArray.Length; i++)
                timesArray[i] = 0;

            int pIdx, pNum, vNum, cvNum;
            double aValue = 0, bValue = 0, cValue = 0;
            List<BorderPoint> lineBorderList = new List<BorderPoint>();

            pNum = borderList.Count - 1;
            for (i = 0; i < pNum; i++)
            {
                if ((borderList[i]).Id == -1)
                    continue;

                pIdx = i;
                aPList = new List<PointD>();
                lineBorderList.Add(borderList[i]);

                //---- Clockwise traceing
                if (timesArray[pIdx] < 2)
                {
                    aPList.Add((borderList[pIdx]).Point);
                    pIdx += 1;
                    if (pIdx == pNum)
                        pIdx = 0;

                    vNum = 0;
                    cvNum = 0;
                    while (true)
                    {
                        bP = borderList[pIdx];
                        if (bP.Id == -1)    //---- Not endpoint of contour
                        {
                            if (timesArray[pIdx] == 1)
                                break;

                            if (cvNum < 5)
                                cValue = bP.Value;
                            cvNum += 1;
                            aPList.Add(bP.Point);
                            timesArray[pIdx] += +1;
                        }
                        else    //---- endpoint of contour
                        {
                            if (timesArray[pIdx] == 2)
                                break;

                            timesArray[pIdx] += +1;
                            aLine = aLineList[bP.Id];
                            if (vNum == 0)
                            {
                                aValue = aLine.Value;
                                bValue = aLine.Value;
                                vNum += 1;
                            }
                            else
                            {
                                if (aValue == bValue)
                                {
                                    if (aLine.Value > aValue)
                                        bValue = aLine.Value;
                                    else if (aLine.Value < aValue)
                                        aValue = aLine.Value;

                                    vNum += 1;
                                }
                            }
                            newPList = new List<PointD>(aLine.PointList);
                            aPoint = newPList[0];
                            //If Not (Math.Abs(bP.point.X - aPoint.X) < 0.000001 And _
                            //  Math.Abs(bP.point.Y - aPoint.Y) < 0.000001) Then    '---- Start point
                            if (!(bP.Point.X == aPoint.X && bP.Point.Y == aPoint.Y))    //---- Start point
                                newPList.Reverse();

                            aPList.AddRange(newPList);
                            for (j = 0; j < borderList.Count - 1; j++)
                            {
                                if (j != pIdx)
                                {
                                    if ((borderList[j]).Id == bP.Id)
                                    {
                                        pIdx = j;
                                        timesArray[pIdx] += +1;
                                        break;
                                    }
                                }
                            }
                        }

                        if (pIdx == i)
                        {
                            if (aPList.Count > 0)
                            {
                                aPolygon = new Polygon();
                                aPolygon.IsBorder = true;
                                aPolygon.LowValue = aValue;
                                aPolygon.HighValue = bValue;
                                aBound = new Extent();
                                aPolygon.Area = GetExtentAndArea(aPList, ref aBound);
                                aPolygon.IsClockWise = true;
                                aPolygon.StartPointIdx = lineBorderList.Count - 1;
                                aPolygon.Extent = aBound;
                                aPolygon.OutLine.PointList = aPList;
                                aPolygon.OutLine.Value = aValue;
                                aPolygon.IsHighCenter = true;
                                aPolygon.HoleLines = new List<PolyLine>();
                                if (aValue == bValue)
                                {
                                    if (cValue < aValue)
                                        aPolygon.IsHighCenter = false;
                                }
                                aPolygon.OutLine.Type = "Border";
                                aPolygonList.Add(aPolygon);
                            }
                            break;
                        }
                        pIdx += 1;
                        if (pIdx == pNum)
                            pIdx = 0;

                    }
                }


                //---- Anticlockwise traceing
                pIdx = i;
                if (timesArray[pIdx] < 2)
                {
                    aPList = new List<PointD>();
                    aPList.Add((borderList[pIdx]).Point);
                    pIdx += -1;
                    if (pIdx == -1)
                        pIdx = pNum - 1;

                    vNum = 0;
                    cvNum = 0;
                    while (true)
                    {
                        bP = borderList[pIdx];
                        if (bP.Id == -1)    //---- Not endpoint of contour
                        {
                            if (timesArray[pIdx] == 1)
                                break;

                            if (cvNum < 5)
                                cValue = bP.Value;
                            cvNum += 1;
                            aPList.Add(bP.Point);
                            timesArray[pIdx] += +1;
                        }
                        else    //---- endpoint of contour
                        {
                            if (timesArray[pIdx] == 2)
                                break;

                            timesArray[pIdx] += +1;
                            aLine = aLineList[bP.Id];
                            if (vNum == 0)
                            {
                                aValue = aLine.Value;
                                bValue = aLine.Value;
                                vNum += 1;
                            }
                            else
                            {
                                if (aValue == bValue)
                                {
                                    if (aLine.Value > aValue)
                                        bValue = aLine.Value;
                                    else if (aLine.Value < aValue)
                                        aValue = aLine.Value;

                                    vNum += 1;
                                }
                            }
                            newPList = new List<PointD>(aLine.PointList);
                            aPoint = newPList[0];
                            //If Not (Math.Abs(bP.point.X - aPoint.X) < 0.000001 And _
                            //  Math.Abs(bP.point.Y - aPoint.Y) < 0.000001) Then    '---- Start point
                            if (!(bP.Point.X == aPoint.X && bP.Point.Y == aPoint.Y))    //---- Start point
                                newPList.Reverse();

                            aPList.AddRange(newPList);
                            for (j = 0; j < borderList.Count - 1; j++)
                            {
                                if (j != pIdx)
                                {
                                    if ((borderList[j]).Id == bP.Id)
                                    {
                                        pIdx = j;
                                        timesArray[pIdx] += +1;
                                        break;
                                    }
                                }
                            }
                        }

                        if (pIdx == i)
                        {
                            if (aPList.Count > 0)
                            {
                                aPolygon = new Polygon();
                                aPolygon.IsBorder = true;
                                aPolygon.LowValue = aValue;
                                aPolygon.HighValue = bValue;
                                aBound = new Extent();
                                aPolygon.Area = GetExtentAndArea(aPList, ref aBound);
                                aPolygon.IsClockWise = false;
                                aPolygon.StartPointIdx = lineBorderList.Count - 1;
                                aPolygon.Extent = aBound;
                                aPolygon.OutLine.PointList = aPList;
                                aPolygon.OutLine.Value = aValue;
                                aPolygon.IsHighCenter = true;
                                aPolygon.HoleLines = new List<PolyLine>();
                                if (aValue == bValue)
                                {
                                    if (cValue < aValue)
                                        aPolygon.IsHighCenter = false;
                                }
                                aPolygon.OutLine.Type = "Border";
                                aPolygonList.Add(aPolygon);
                            }
                            break;
                        }
                        pIdx += -1;
                        if (pIdx == -1)
                            pIdx = pNum - 1;

                    }
                }
            }

            //---- tracing close polygons
            List<Polygon> cPolygonlist = new List<Polygon>();
            bool isInserted;
            for (i = 0; i < aLineList.Count; i++)
            {
                aLine = aLineList[i];
                if (aLine.Type == "Close" && aLine.PointList.Count > 0)
                {
                    aPolygon = new Polygon();
                    aPolygon.IsBorder = false;
                    aPolygon.LowValue = aLine.Value;
                    aPolygon.HighValue = aLine.Value;
                    aBound = new Extent();
                    aPolygon.Area = GetExtentAndArea(aLine.PointList, ref aBound);
                    aPolygon.IsClockWise = IsClockwise(aLine.PointList);
                    aPolygon.Extent = aBound;
                    aPolygon.OutLine = aLine;
                    aPolygon.IsHighCenter = true;
                    aPolygon.HoleLines = new List<PolyLine>();

                    //---- Sort from big to small
                    isInserted = false;
                    for (j = 0; j < cPolygonlist.Count; j++)
                    {
                        if (aPolygon.Area > (cPolygonlist[j]).Area)
                        {
                            cPolygonlist.Insert(j, aPolygon);
                            isInserted = true;
                            break;
                        }
                    }
                    if (!isInserted)
                        cPolygonlist.Add(aPolygon);

                }
            }

            //---- Juge isHighCenter for border polygons
            aPolygonList = JudgePolygonHighCenter(aPolygonList, cPolygonlist, aLineList, borderList);

            return aPolygonList;
        }

        private static List<Polygon> TracingClipPolygons(Polygon inPolygon, List<PolyLine> LineList, List<BorderPoint> borderList)
        {
            if (LineList.Count == 0)
                return new List<Polygon>();

            List<Polygon> aPolygonList = new List<Polygon>();
            List<PolyLine> aLineList = new List<PolyLine>();
            PolyLine aLine;
            PointD aPoint;
            Polygon aPolygon;
            Extent aBound;
            int i, j;

            aLineList = new List<PolyLine>(LineList);

            //---- Tracing border polygon
            List<PointD> aPList;
            List<PointD> newPList = new List<PointD>();
            BorderPoint bP;
            int[] timesArray = new int[borderList.Count - 1];
            for (i = 0; i < timesArray.Length; i++)
                timesArray[i] = 0;

            int pIdx, pNum;
            List<BorderPoint> lineBorderList = new List<BorderPoint>();

            pNum = borderList.Count - 1;
            PointD bPoint, b1Point;
            for (i = 0; i < pNum; i++)
            {
                if ((borderList[i]).Id == -1)
                    continue;

                pIdx = i;                
                lineBorderList.Add(borderList[i]);
                bP = borderList[pIdx];
                b1Point = borderList[pIdx].Point;                    

                //---- Clockwise traceing
                if (timesArray[pIdx] < 1)
                {
                    aPList = new List<PointD>();
                    aPList.Add((borderList[pIdx]).Point);
                    pIdx += 1;
                    if (pIdx == pNum)
                        pIdx = 0;

                    bPoint = (PointD)borderList[pIdx].Point.Clone();
                    if (borderList[pIdx].Id == -1)
                    {
                        int aIdx = pIdx + 10;
                        for (int o = 1; o <= 10; o++)
                        {
                            if (borderList[pIdx + o].Id > -1)
                            {
                                aIdx = pIdx + o - 1;
                                break;
                            }
                        }
                        bPoint = (PointD)borderList[aIdx].Point.Clone();
                    }
                    else
                    {
                        bPoint.X = (bPoint.X + b1Point.X) / 2;
                        bPoint.Y = (bPoint.Y + b1Point.Y) / 2;
                    }
                    if (PointInPolygon(inPolygon, bPoint))
                    {
                        while (true)
                        {
                            bP = borderList[pIdx];
                            if (bP.Id == -1)    //---- Not endpoint of contour
                            {
                                if (timesArray[pIdx] == 1)
                                    break;

                                aPList.Add(bP.Point);
                                timesArray[pIdx] += 1;                                
                            }
                            else    //---- endpoint of contour
                            {
                                if (timesArray[pIdx] == 1)
                                    break;

                                timesArray[pIdx] += 1;                                
                                aLine = aLineList[bP.Id];

                                newPList = new List<PointD>(aLine.PointList);
                                aPoint = newPList[0];

                                if (!(DoubleEquals(bP.Point.X, aPoint.X) && DoubleEquals(bP.Point.Y, aPoint.Y)))    //---- Start point
                                    //if (!IsClockwise(newPList))
                                    newPList.Reverse();

                                aPList.AddRange(newPList);
                                for (j = 0; j < borderList.Count - 1; j++)
                                {
                                    if (j != pIdx)
                                    {
                                        if ((borderList[j]).Id == bP.Id)
                                        {
                                            pIdx = j;
                                            timesArray[pIdx] += 1;                                            
                                            break;
                                        }
                                    }
                                }
                            }

                            if (pIdx == i)
                            {
                                if (aPList.Count > 0)
                                {
                                    aPolygon = new Polygon();
                                    aPolygon.IsBorder = true;
                                    aPolygon.LowValue = inPolygon.LowValue;
                                    aPolygon.HighValue = inPolygon.HighValue;
                                    aBound = new Extent();
                                    aPolygon.Area = GetExtentAndArea(aPList, ref aBound);
                                    aPolygon.IsClockWise = true;
                                    aPolygon.StartPointIdx = lineBorderList.Count - 1;
                                    aPolygon.Extent = aBound;
                                    aPolygon.OutLine.PointList = aPList;
                                    aPolygon.OutLine.Value = inPolygon.LowValue;
                                    aPolygon.IsHighCenter = inPolygon.IsHighCenter;
                                    aPolygon.OutLine.Type = "Border";
                                    aPolygon.HoleLines = new List<PolyLine>();
                                    aPolygonList.Add(aPolygon);                                    
                                }
                                break;
                            }
                            pIdx += 1;
                            if (pIdx == pNum)
                                pIdx = 0;                            
                        }
                    }
                }                

                //---- Anticlockwise traceing
                pIdx = i;
                if (timesArray[pIdx] < 1)
                {
                    aPList = new List<PointD>();
                    aPList.Add((borderList[pIdx]).Point);
                    pIdx += -1;
                    if (pIdx == -1)
                        pIdx = pNum - 1;

                    bPoint = (PointD)borderList[pIdx].Point.Clone();
                    if (borderList[pIdx].Id == -1)
                    {
                        int aIdx = pIdx + 10;
                        for (int o = 1; o <= 10; o++)
                        {
                            if (borderList[pIdx + o].Id > -1)
                            {
                                aIdx = pIdx + o - 1;
                                break;
                            }
                        }
                        bPoint = (PointD)borderList[aIdx].Point.Clone();
                    }
                    else
                    {
                        bPoint.X = (bPoint.X + b1Point.X) / 2;
                        bPoint.Y = (bPoint.Y + b1Point.Y) / 2;
                    }
                    if (PointInPolygon(inPolygon, bPoint))
                    {
                        while (true)
                        {
                            bP = borderList[pIdx];
                            if (bP.Id == -1)    //---- Not endpoint of contour
                            {
                                if (timesArray[pIdx] == 1)
                                    break;

                                aPList.Add(bP.Point);
                                timesArray[pIdx] += 1;
                            }
                            else    //---- endpoint of contour
                            {
                                if (timesArray[pIdx] == 1)
                                    break;

                                timesArray[pIdx] += 1;
                                aLine = aLineList[bP.Id];

                                newPList = new List<PointD>(aLine.PointList);
                                aPoint = newPList[0];

                                if (!(DoubleEquals(bP.Point.X, aPoint.X) && DoubleEquals(bP.Point.Y, aPoint.Y)))    //---- Start point
                                    //if (IsClockwise(newPList))
                                    newPList.Reverse();

                                aPList.AddRange(newPList);
                                for (j = 0; j < borderList.Count - 1; j++)
                                {
                                    if (j != pIdx)
                                    {
                                        if ((borderList[j]).Id == bP.Id)
                                        {
                                            pIdx = j;
                                            timesArray[pIdx] += 1;
                                            break;
                                        }
                                    }
                                }
                            }

                            if (pIdx == i)
                            {
                                if (aPList.Count > 0)
                                {
                                    aPolygon = new Polygon();
                                    aPolygon.IsBorder = true;
                                    aPolygon.LowValue = inPolygon.LowValue;
                                    aPolygon.HighValue = inPolygon.HighValue;
                                    aBound = new Extent();
                                    aPolygon.Area = GetExtentAndArea(aPList, ref aBound);
                                    aPolygon.IsClockWise = false;
                                    aPolygon.StartPointIdx = lineBorderList.Count - 1;
                                    aPolygon.Extent = aBound;
                                    aPolygon.OutLine.PointList = aPList;
                                    aPolygon.OutLine.Value = inPolygon.LowValue;
                                    aPolygon.IsHighCenter = inPolygon.IsHighCenter;
                                    aPolygon.OutLine.Type = "Border";
                                    aPolygon.HoleLines = new List<PolyLine>();
                                    aPolygonList.Add(aPolygon);
                                }
                                break;
                            }
                            pIdx += -1;
                            if (pIdx == -1)
                                pIdx = pNum - 1;

                        }
                    }
                }
            }

            return aPolygonList;
        }

        private static List<Polygon> JudgePolygonHighCenter(List<Polygon> borderPolygons, List<Polygon> closedPolygons, 
            List<PolyLine> aLineList, List<BorderPoint> borderList)
        {
            int i, j;
            Polygon aPolygon;
            PolyLine aLine;
            List<PointD> newPList = new List<PointD>();
            Extent aBound;
            double aValue = 0;
            double bValue = 0;
            PointD aPoint;

            if (borderPolygons.Count == 0)    //Add border polygon
            {

                //Get max & min contour values
                double max = aLineList[0].Value, min = aLineList[0].Value;
                foreach (PolyLine aPLine in aLineList)
                {
                    if (aPLine.Value > max)
                        max = aPLine.Value;
                    if (aPLine.Value < min)
                        min = aPLine.Value;
                }
                aPolygon = new Polygon();
                aValue = borderList[0].Value;
                if (aValue < min)
                {
                    max = min;
                    min = aValue;
                    aPolygon.IsHighCenter = true;
                }
                else if (aValue > max)
                {
                    min = max;
                    max = aValue;
                    aPolygon.IsHighCenter = false;
                }  
                aLine = new PolyLine();
                aLine.Type = "Border";
                aLine.Value = aValue;
                newPList.Clear();
                foreach (BorderPoint aP in borderList)
                {
                    newPList.Add(aP.Point);
                }
                aLine.PointList = new List<PointD>(newPList);
                if (aLine.PointList.Count > 0)
                {
                    aPolygon.IsBorder = true;
                    aPolygon.LowValue = min;
                    aPolygon.HighValue = max;                                      
                    aBound = new Extent();
                    aPolygon.Area = GetExtentAndArea(aLine.PointList, ref aBound);
                    aPolygon.IsClockWise = IsClockwise(aLine.PointList);
                    aPolygon.Extent = aBound;
                    aPolygon.OutLine = aLine;
                    aPolygon.HoleLines = new List<PolyLine>();                    
                    borderPolygons.Add(aPolygon);
                }                                                 
            }

            //---- Add close polygons to form total polygons list
            borderPolygons.AddRange(closedPolygons);

            //---- Juge IsHighCenter for close polygons
            Extent cBound1, cBound2;
            int polygonNum = borderPolygons.Count;
            Polygon bPolygon;
            for (i = 1; i < polygonNum; i++)
            {
                aPolygon = borderPolygons[i];
                if (aPolygon.OutLine.Type == "Close")
                {
                    cBound1 = aPolygon.Extent;
                    aValue = aPolygon.LowValue;
                    aPoint = aPolygon.OutLine.PointList[0];
                    for (j = i - 1; j >= 0; j--)
                    {
                        bPolygon = borderPolygons[j];
                        cBound2 = bPolygon.Extent;
                        bValue = bPolygon.LowValue;
                        newPList = new List<PointD>(bPolygon.OutLine.PointList);
                        if (PointInPolygon(newPList, aPoint))
                        {
                            if (cBound1.xMin > cBound2.xMin && cBound1.yMin > cBound2.yMin &&
                              cBound1.xMax < cBound2.xMax && cBound1.yMax < cBound2.yMax)
                            {
                                if (aValue < bValue)
                                {
                                    aPolygon.IsHighCenter = false;
                                    //borderPolygons[i] = aPolygon;
                                }
                                else if (aValue == bValue)
                                {
                                    if (bPolygon.IsHighCenter)
                                    {
                                        aPolygon.IsHighCenter = false;
                                        //borderPolygons[i] = aPolygon;
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }

            return borderPolygons;
        }

        private static List<Polygon> JudgePolygonHighCenter_old(List<Polygon> borderPolygons, List<Polygon> closedPolygons,
            List<PolyLine> aLineList, List<BorderPoint> borderList)
        {
            int i, j;
            Polygon aPolygon;
            PolyLine aLine;
            List<PointD> newPList = new List<PointD>();
            Extent aBound;
            double aValue = 0;
            double bValue = 0;
            PointD aPoint;

            if (borderPolygons.Count == 0)    //Add border polygon
            {

                //Get max & min contour values
                double max = aLineList[0].Value, min = aLineList[0].Value;
                foreach (PolyLine aPLine in aLineList)
                {
                    if (aPLine.Value > max)
                        max = aPLine.Value;
                    if (aPLine.Value < min)
                        min = aPLine.Value;
                }
                aPolygon = new Polygon();
                aLine = new PolyLine();
                aLine.Type = "Border";
                aLine.Value = min;
                aPolygon.IsHighCenter = false;
                if (closedPolygons.Count > 0)
                {
                    if (borderList[0].Value >= closedPolygons[0].LowValue)
                    {
                        aLine.Value = max;
                        aPolygon.IsHighCenter = true;
                    }
                }
                newPList.Clear();
                foreach (BorderPoint aP in borderList)
                {
                    newPList.Add(aP.Point);
                }
                aLine.PointList = new List<PointD>(newPList);

                if (aLine.PointList.Count > 0)
                {
                    aPolygon.IsBorder = true;
                    aPolygon.LowValue = aLine.Value;
                    aPolygon.HighValue = aLine.Value;
                    aBound = new Extent();
                    aPolygon.Area = GetExtentAndArea(aLine.PointList, ref aBound);
                    aPolygon.IsClockWise = IsClockwise(aLine.PointList);
                    aPolygon.Extent = aBound;
                    aPolygon.OutLine = aLine;
                    aPolygon.HoleLines = new List<PolyLine>();
                    //aPolygon.IsHighCenter = false;
                    borderPolygons.Add(aPolygon);
                }
            }

            //---- Add close polygons to form total polygons list
            borderPolygons.AddRange(closedPolygons);

            //---- Juge IsHighCenter for close polygons
            Extent cBound1, cBound2;
            int polygonNum = borderPolygons.Count;
            Polygon bPolygon;
            for (i = 1; i < polygonNum; i++)
            {
                aPolygon = borderPolygons[i];
                if (aPolygon.OutLine.Type == "Close")
                {
                    cBound1 = aPolygon.Extent;
                    aValue = aPolygon.LowValue;
                    aPoint = aPolygon.OutLine.PointList[0];
                    for (j = i - 1; j >= 0; j--)
                    {
                        bPolygon = borderPolygons[j];
                        cBound2 = bPolygon.Extent;
                        bValue = bPolygon.LowValue;
                        newPList = new List<PointD>(bPolygon.OutLine.PointList);
                        if (PointInPolygon(newPList, aPoint))
                        {
                            if (cBound1.xMin > cBound2.xMin && cBound1.yMin > cBound2.yMin &&
                              cBound1.xMax < cBound2.xMax && cBound1.yMax < cBound2.yMax)
                            {
                                if (aValue < bValue)
                                {
                                    aPolygon.IsHighCenter = false;
                                    //borderPolygons[i] = aPolygon;
                                }
                                else if (aValue == bValue)
                                {
                                    if (bPolygon.IsHighCenter)
                                    {
                                        aPolygon.IsHighCenter = false;
                                        //borderPolygons[i] = aPolygon;
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }

            return borderPolygons;
        }

        //private static List<Polygon> TracingPolygons_Ring_Back(List<PolyLine> LineList, List<BorderPoint> borderList, Extent bBound,
        //    double[] contour, int[] pNums)
        //{
        //    List<Polygon> aPolygonList = new List<Polygon>();
        //    List<Polygon> newPolygonlist = new List<Polygon>();
        //    List<PolyLine> aLineList = new List<PolyLine>();
        //    PolyLine aLine = new PolyLine();
        //    PointD aPoint;
        //    Polygon aPolygon = new Polygon();
        //    Extent aBound = new Extent();
        //    int i = 0;
        //    int j = 0;

        //    aLineList = new List<PolyLine>(LineList);

        //    //---- Tracing border polygon
        //    List<PointD> aPList = new List<PointD>();
        //    List<PointD> newPList = new List<PointD>();
        //    BorderPoint bP;
        //    BorderPoint bP1;
        //    int[] timesArray = new int[borderList.Count];
        //    for (i = 0; i <= timesArray.Length - 1; i++)
        //    {
        //        timesArray[i] = 0;
        //    }
        //    int pIdx = 0;
        //    int pNum = 0;
        //    int vNum = 0;
        //    double aValue = 0;
        //    double bValue = 0;
        //    ArrayList lineBorderList = new ArrayList();
        //    int borderIdx1 = 0;
        //    int borderIdx2 = 0;
        //    int innerIdx = 0;

        //    pNum = borderList.Count;
        //    for (i = 0; i <= pNum - 1; i++)
        //    {
        //        if (((BorderPoint)borderList[i]).Id == -1)
        //        {
        //            continue;
        //        }
        //        pIdx = i;
        //        aPList.Clear();
        //        lineBorderList.Add(borderList[i]);

        //        //---- Clockwise traceing
        //        if (timesArray[pIdx] < 2)
        //        {
        //            bP = (BorderPoint)borderList[pIdx];
        //            innerIdx = bP.BInnerIdx;
        //            aPList.Add(bP.Point);
        //            borderIdx1 = bP.BorderIdx;
        //            borderIdx2 = borderIdx1;
        //            pIdx += 1;
        //            innerIdx += 1;
        //            //If pIdx = pNum Then
        //            //    pIdx = 0
        //            //End If
        //            if (innerIdx == pNums[borderIdx1] - 1)
        //            {
        //                pIdx = pIdx - (pNums[borderIdx1] - 1);
        //            }
        //            vNum = 0;
        //            do
        //            {
        //                bP = (BorderPoint)borderList[pIdx];
        //                //---- Not endpoint of contour
        //                if (bP.Id == -1)
        //                {
        //                    if (timesArray[pIdx] == 1)
        //                    {
        //                        break; // TODO: might not be correct. Was : Exit Do
        //                    }
        //                    aPList.Add(bP.Point);
        //                    timesArray[pIdx] += +1;
        //                    //---- endpoint of contour
        //                }
        //                else
        //                {
        //                    if (timesArray[pIdx] == 2)
        //                    {
        //                        break; // TODO: might not be correct. Was : Exit Do
        //                    }
        //                    timesArray[pIdx] += +1;
        //                    aLine = (PolyLine)aLineList[bP.Id];
        //                    //---- Set high and low value of the polygon
        //                    if (vNum == 0)
        //                    {
        //                        aValue = aLine.Value;
        //                        bValue = aLine.Value;
        //                        vNum += 1;
        //                    }
        //                    else
        //                    {
        //                        if (aValue == bValue)
        //                        {
        //                            if (aLine.Value > aValue)
        //                            {
        //                                bValue = aLine.Value;
        //                            }
        //                            else if (aLine.Value < aValue)
        //                            {
        //                                aValue = aLine.Value;
        //                            }
        //                            vNum += 1;
        //                        }
        //                    }
        //                    newPList = new List<PointD>(aLine.PointList);
        //                    aPoint = (PointD)newPList[0];
        //                    //If Not (Math.Abs(bP.point.x - aPoint.x) < 0.000001 And _
        //                    //  Math.Abs(bP.point.y - aPoint.y) < 0.000001) Then    '---- Not start point
        //                    //---- Not start point
        //                    if (!(bP.Point.X == aPoint.X & bP.Point.Y == aPoint.Y))
        //                    {
        //                        newPList.Reverse();
        //                    }
        //                    aPList.AddRange(newPList);
        //                    //---- Find corresponding border point
        //                    for (j = 0; j <= borderList.Count - 1; j++)
        //                    {
        //                        if (j != pIdx)
        //                        {
        //                            bP1 = (BorderPoint)borderList[j];
        //                            if (bP1.Id == bP.Id)
        //                            {
        //                                pIdx = j;
        //                                innerIdx = bP1.BInnerIdx;
        //                                timesArray[pIdx] += +1;
        //                                borderIdx2 = bP1.BorderIdx;
        //                                break; // TODO: might not be correct. Was : Exit For
        //                            }
        //                        }
        //                    }
        //                    //
        //                }

        //                //---- Return to start point, tracing finish
        //                if (pIdx == i)
        //                {
        //                    if (aPList.Count > 0)
        //                    {
        //                        aPolygon.LowValue = aValue;
        //                        aPolygon.HighValue = bValue;
        //                        aPolygon.Area = GetExtentAndArea(aPList, ref aBound);
        //                        aPolygon.IsClockWise = true;
        //                        aPolygon.StartPointIdx = lineBorderList.Count - 1;
        //                        aPolygon.Extent = aBound;
        //                        aPolygon.OutLine.PointList = new List<PointD>(aPList);
        //                        aPolygon.OutLine.Value = aValue;
        //                        aPolygon.IsHighCenter = true;
        //                        aPolygon.OutLine.Type = "Border";
        //                        aPolygonList.Add(aPolygon);
        //                    }
        //                    break; // TODO: might not be correct. Was : Exit Do
        //                }
        //                pIdx += 1;
        //                innerIdx += 1;
        //                if (borderIdx1 != borderIdx2)
        //                {
        //                    borderIdx1 = borderIdx2;
        //                }
        //                //If pIdx = pNum Then
        //                //    pIdx = 0
        //                //End If
        //                if (innerIdx == pNums[borderIdx1] - 1)
        //                {
        //                    pIdx = pIdx - (pNums[borderIdx1] - 1);
        //                    innerIdx = 0;
        //                }
        //            } while (true);
        //        }


        //        //---- Anticlockwise traceing
        //        pIdx = i;
        //        if (timesArray[pIdx] < 2)
        //        {
        //            aPList.Clear();
        //            bP = (BorderPoint)borderList[pIdx];
        //            innerIdx = bP.BInnerIdx;
        //            aPList.Add(bP.Point);
        //            borderIdx1 = bP.BorderIdx;
        //            borderIdx2 = borderIdx1;
        //            pIdx += -1;
        //            innerIdx += -1;
        //            //If pIdx = -1 Then
        //            //    pIdx = pNum - 1
        //            //End If
        //            if (innerIdx == -1)
        //            {
        //                pIdx = pIdx + (pNums[borderIdx1] - 1);
        //            }
        //            vNum = 0;
        //            do
        //            {
        //                bP = (BorderPoint)borderList[pIdx];
        //                //---- Not endpoint of contour
        //                if (bP.Id == -1)
        //                {
        //                    if (timesArray[pIdx] == 1)
        //                    {
        //                        break; // TODO: might not be correct. Was : Exit Do
        //                    }
        //                    aPList.Add(bP.Point);
        //                    timesArray[pIdx] += +1;
        //                    //---- endpoint of contour
        //                }
        //                else
        //                {
        //                    if (timesArray[pIdx] == 2)
        //                    {
        //                        break; // TODO: might not be correct. Was : Exit Do
        //                    }
        //                    timesArray[pIdx] += +1;
        //                    aLine = (PolyLine)aLineList[bP.Id];
        //                    if (vNum == 0)
        //                    {
        //                        aValue = aLine.Value;
        //                        bValue = aLine.Value;
        //                        vNum += 1;
        //                    }
        //                    else
        //                    {
        //                        if (aValue == bValue)
        //                        {
        //                            if (aLine.Value > aValue)
        //                            {
        //                                bValue = aLine.Value;
        //                            }
        //                            else if (aLine.Value < aValue)
        //                            {
        //                                aValue = aLine.Value;
        //                            }
        //                            vNum += 1;
        //                        }
        //                    }
        //                    newPList = new List<PointD>(aLine.PointList);
        //                    aPoint = (PointD)newPList[0];
        //                    //If Not (Math.Abs(bP.point.x - aPoint.x) < 0.000001 And _
        //                    //  Math.Abs(bP.point.y - aPoint.y) < 0.000001) Then    '---- Start point
        //                    //---- Start point
        //                    if (!(bP.Point.X == aPoint.X & bP.Point.Y == aPoint.Y))
        //                    {
        //                        newPList.Reverse();
        //                    }
        //                    aPList.AddRange(newPList);
        //                    for (j = 0; j <= borderList.Count - 1; j++)
        //                    {
        //                        if (j != pIdx)
        //                        {
        //                            bP1 = (BorderPoint)borderList[j];
        //                            if (bP1.Id == bP.Id)
        //                            {
        //                                pIdx = j;
        //                                innerIdx = bP1.BInnerIdx;
        //                                timesArray[pIdx] += +1;
        //                                borderIdx2 = bP1.BorderIdx;
        //                                break; // TODO: might not be correct. Was : Exit For
        //                            }
        //                        }
        //                    }
        //                }

        //                if (pIdx == i)
        //                {
        //                    if (aPList.Count > 0)
        //                    {
        //                        aPolygon.LowValue = aValue;
        //                        aPolygon.HighValue = bValue;
        //                        aPolygon.Area = GetExtentAndArea(aPList, ref aBound);
        //                        aPolygon.IsClockWise = false;
        //                        aPolygon.StartPointIdx = lineBorderList.Count - 1;
        //                        aPolygon.Extent = aBound;
        //                        aPolygon.OutLine.PointList = new List<PointD>(aPList);
        //                        aPolygon.OutLine.Value = aValue;
        //                        aPolygon.IsHighCenter = true;
        //                        aPolygon.OutLine.Type = "Border";
        //                        aPolygonList.Add(aPolygon);
        //                    }
        //                    break; // TODO: might not be correct. Was : Exit Do
        //                }
        //                pIdx += -1;
        //                innerIdx += -1;
        //                if (borderIdx1 != borderIdx2)
        //                {
        //                    borderIdx1 = borderIdx2;
        //                }
        //                //If pIdx = -1 Then
        //                //    pIdx = pNum - 1
        //                //End If
        //                if (innerIdx == -1)
        //                {
        //                    pIdx = pIdx + pNums[borderIdx1];
        //                    innerIdx = pNums[borderIdx1] - 1;
        //                }
        //            } while (true);
        //        }
        //    }

        //    //---- tracing close polygons
        //    List<Polygon> cPolygonlist = new List<Polygon>();
        //    bool isInserted = false;
        //    for (i = 0; i <= aLineList.Count - 1; i++)
        //    {
        //        aLine = (PolyLine)aLineList[i];
        //        if (aLine.Type == "Close")
        //        {
        //            aPolygon.LowValue = aLine.Value;
        //            aPolygon.HighValue = aLine.Value;
        //            aPolygon.Area = GetExtentAndArea(aLine.PointList, ref aBound);
        //            aPolygon.IsClockWise = IsClockwise(aLine.PointList);
        //            aPolygon.Extent = aBound;
        //            aPolygon.OutLine = aLine;
        //            aPolygon.IsHighCenter = true;

        //            //---- Sort from big to small
        //            isInserted = false;
        //            for (j = 0; j <= cPolygonlist.Count - 1; j++)
        //            {
        //                if (aPolygon.Area > ((Polygon)cPolygonlist[j]).Area)
        //                {
        //                    cPolygonlist.Insert(j, aPolygon);
        //                    isInserted = true;
        //                    break; // TODO: might not be correct. Was : Exit For
        //                }
        //            }
        //            if (!isInserted)
        //            {
        //                cPolygonlist.Add(aPolygon);
        //            }
        //        }
        //    }

        //    //---- Juge isHighCenter for border polygons
        //    aPolygonList = JudgePolygonHighCenter(aPolygonList, cPolygonlist, aLineList, borderList);

        //    newPolygonlist = new List<Polygon>(aPolygonList);

        //    return newPolygonlist;
        //}        

        //private static List<Polygon> TracingPolygons_Ring_Old(List<PolyLine> LineList, List<BorderPoint> borderList, Border aBorder,
        //    double[] contour, int[] pNums)
        //{
        //    List<Polygon> aPolygonList = new List<Polygon>();
        //    List<Polygon> newPolygonlist = new List<Polygon>();
        //    List<PolyLine> aLineList = new List<PolyLine>();
        //    PolyLine aLine = new PolyLine();
        //    PointD aPoint;
        //    Polygon aPolygon = new Polygon();
        //    Extent aBound = new Extent();
        //    int i = 0;
        //    int j = 0;

        //    aLineList = new List<PolyLine>(LineList);

        //    //---- Tracing border polygon
        //    List<PointD> aPList = new List<PointD>();
        //    List<PointD> newPList = new List<PointD>();
        //    BorderPoint bP;
        //    BorderPoint bP1;
        //    int[] timesArray = new int[borderList.Count];
        //    for (i = 0; i <= timesArray.Length - 1; i++)
        //    {
        //        timesArray[i] = 0;
        //    }
        //    int pIdx = 0;
        //    int pNum = 0;
        //    int vNum = 0;
        //    double aValue = 0;
        //    double bValue = 0;
        //    double cValue = 0;
        //    ArrayList lineBorderList = new ArrayList();
        //    int borderIdx1 = 0;
        //    int borderIdx2 = 0;
        //    int innerIdx = 0;

        //    pNum = borderList.Count;
        //    for (i = 0; i <= pNum - 1; i++)
        //    {
        //        if (((BorderPoint)borderList[i]).Id == -1)
        //        {
        //            continue;
        //        }
        //        pIdx = i;
        //        aPList.Clear();
        //        lineBorderList.Add(borderList[i]);

        //        //---- Clockwise traceing
        //        if (timesArray[pIdx] < 2)
        //        {
        //            bP = (BorderPoint)borderList[pIdx];
        //            innerIdx = bP.BInnerIdx;
        //            aPList.Add(bP.Point);
        //            borderIdx1 = bP.BorderIdx;
        //            borderIdx2 = borderIdx1;
        //            pIdx += 1;
        //            innerIdx += 1;
        //            //If pIdx = pNum Then
        //            //    pIdx = 0
        //            //End If
        //            if (innerIdx == pNums[borderIdx1] - 1)
        //            {
        //                pIdx = pIdx - (pNums[borderIdx1] - 1);
        //            }
        //            vNum = 0;
        //            do
        //            {
        //                bP = (BorderPoint)borderList[pIdx];
        //                //---- Not endpoint of contour
        //                if (bP.Id == -1)
        //                {
        //                    if (timesArray[pIdx] == 1)
        //                    {
        //                        break; // TODO: might not be correct. Was : Exit Do
        //                    }
        //                    cValue = bP.Value;
        //                    aPList.Add(bP.Point);
        //                    timesArray[pIdx] += +1;
        //                    //---- endpoint of contour
        //                }
        //                else
        //                {
        //                    if (timesArray[pIdx] == 2)
        //                    {
        //                        break; // TODO: might not be correct. Was : Exit Do
        //                    }
        //                    timesArray[pIdx] += +1;
        //                    aLine = (PolyLine)aLineList[bP.Id];
        //                    //---- Set high and low value of the polygon
        //                    if (vNum == 0)
        //                    {
        //                        aValue = aLine.Value;
        //                        bValue = aLine.Value;
        //                        vNum += 1;
        //                    }
        //                    else
        //                    {
        //                        if (aValue == bValue)
        //                        {
        //                            if (aLine.Value > aValue)
        //                            {
        //                                bValue = aLine.Value;
        //                            }
        //                            else if (aLine.Value < aValue)
        //                            {
        //                                aValue = aLine.Value;
        //                            }
        //                            vNum += 1;
        //                        }
        //                    }
        //                    newPList = new List<PointD>(aLine.PointList);
        //                    aPoint = (PointD)newPList[0];
        //                    //If Not (Math.Abs(bP.point.x - aPoint.x) < 0.000001 And _
        //                    //  Math.Abs(bP.point.y - aPoint.y) < 0.000001) Then    '---- Not start point
        //                    //---- Not start point
        //                    if (!(bP.Point.X == aPoint.X & bP.Point.Y == aPoint.Y))
        //                    {
        //                        newPList.Reverse();
        //                    }
        //                    aPList.AddRange(newPList);
        //                    //---- Find corresponding border point
        //                    for (j = 0; j <= borderList.Count - 1; j++)
        //                    {
        //                        if (j != pIdx)
        //                        {
        //                            bP1 = (BorderPoint)borderList[j];
        //                            if (bP1.Id == bP.Id)
        //                            {
        //                                pIdx = j;
        //                                innerIdx = bP1.BInnerIdx;
        //                                timesArray[pIdx] += +1;
        //                                borderIdx2 = bP1.BorderIdx;
        //                                break; // TODO: might not be correct. Was : Exit For
        //                            }
        //                        }
        //                    }
        //                    //
        //                }

        //                //---- Return to start point, tracing finish
        //                if (pIdx == i)
        //                {
        //                    if (aPList.Count > 0)
        //                    {
        //                        aPolygon.IsBorder = true;
        //                        aPolygon.LowValue = aValue;
        //                        aPolygon.HighValue = bValue;
        //                        aPolygon.Area = GetExtentAndArea(aPList, ref aBound);
        //                        aPolygon.IsClockWise = true;
        //                        aPolygon.StartPointIdx = lineBorderList.Count - 1;
        //                        aPolygon.Extent = aBound;
        //                        aPolygon.OutLine.PointList = new List<PointD>(aPList);
        //                        aPolygon.OutLine.Value = aValue;
        //                        aPolygon.IsHighCenter = true;
        //                        if (aValue == bValue)
        //                        {
        //                            if (cValue < aValue)
        //                                aPolygon.IsHighCenter = false;
        //                        }
        //                        aPolygon.OutLine.Type = "Border";
        //                        aPolygon.HoleLines = new List<PolyLine>();
        //                        aPolygonList.Add(aPolygon);
        //                    }
        //                    break; // TODO: might not be correct. Was : Exit Do
        //                }
        //                pIdx += 1;
        //                innerIdx += 1;
        //                if (borderIdx1 != borderIdx2)
        //                {
        //                    borderIdx1 = borderIdx2;
        //                }
        //                //If pIdx = pNum Then
        //                //    pIdx = 0
        //                //End If
        //                if (innerIdx == pNums[borderIdx1] - 1)
        //                {
        //                    pIdx = pIdx - (pNums[borderIdx1] - 1);
        //                    innerIdx = 0;
        //                }
        //            } while (true);
        //        }


        //        //---- Anticlockwise traceing
        //        pIdx = i;
        //        if (timesArray[pIdx] < 2)
        //        {
        //            aPList.Clear();
        //            bP = (BorderPoint)borderList[pIdx];
        //            innerIdx = bP.BInnerIdx;
        //            aPList.Add(bP.Point);
        //            borderIdx1 = bP.BorderIdx;
        //            borderIdx2 = borderIdx1;
        //            pIdx += -1;
        //            innerIdx += -1;
        //            //If pIdx = -1 Then
        //            //    pIdx = pNum - 1
        //            //End If
        //            if (innerIdx == -1)
        //            {
        //                pIdx = pIdx + (pNums[borderIdx1] - 1);
        //            }
        //            vNum = 0;
        //            do
        //            {
        //                bP = (BorderPoint)borderList[pIdx];
        //                //---- Not endpoint of contour
        //                if (bP.Id == -1)
        //                {
        //                    if (timesArray[pIdx] == 1)
        //                    {
        //                        break; // TODO: might not be correct. Was : Exit Do
        //                    }
        //                    cValue = bP.Value;
        //                    aPList.Add(bP.Point);
        //                    timesArray[pIdx] += +1;
        //                    //---- endpoint of contour
        //                }
        //                else
        //                {
        //                    if (timesArray[pIdx] == 2)
        //                    {
        //                        break; // TODO: might not be correct. Was : Exit Do
        //                    }
        //                    timesArray[pIdx] += +1;
        //                    aLine = (PolyLine)aLineList[bP.Id];
        //                    if (vNum == 0)
        //                    {
        //                        aValue = aLine.Value;
        //                        bValue = aLine.Value;
        //                        vNum += 1;
        //                    }
        //                    else
        //                    {
        //                        if (aValue == bValue)
        //                        {
        //                            if (aLine.Value > aValue)
        //                            {
        //                                bValue = aLine.Value;
        //                            }
        //                            else if (aLine.Value < aValue)
        //                            {
        //                                aValue = aLine.Value;
        //                            }
        //                            vNum += 1;
        //                        }
        //                    }
        //                    newPList = new List<PointD>(aLine.PointList);
        //                    aPoint = (PointD)newPList[0];
        //                    //If Not (Math.Abs(bP.point.x - aPoint.x) < 0.000001 And _
        //                    //  Math.Abs(bP.point.y - aPoint.y) < 0.000001) Then    '---- Start point
        //                    //---- Start point
        //                    if (!(bP.Point.X == aPoint.X & bP.Point.Y == aPoint.Y))
        //                    {
        //                        newPList.Reverse();
        //                    }
        //                    aPList.AddRange(newPList);
        //                    for (j = 0; j <= borderList.Count - 1; j++)
        //                    {
        //                        if (j != pIdx)
        //                        {
        //                            bP1 = (BorderPoint)borderList[j];
        //                            if (bP1.Id == bP.Id)
        //                            {
        //                                pIdx = j;
        //                                innerIdx = bP1.BInnerIdx;
        //                                timesArray[pIdx] += +1;
        //                                borderIdx2 = bP1.BorderIdx;
        //                                break; // TODO: might not be correct. Was : Exit For
        //                            }
        //                        }
        //                    }
        //                }

        //                if (pIdx == i)
        //                {
        //                    if (aPList.Count > 0)
        //                    {
        //                        aPolygon.IsBorder = true;
        //                        aPolygon.LowValue = aValue;
        //                        aPolygon.HighValue = bValue;
        //                        aPolygon.Area = GetExtentAndArea(aPList, ref aBound);
        //                        aPolygon.IsClockWise = false;
        //                        aPolygon.StartPointIdx = lineBorderList.Count - 1;
        //                        aPolygon.Extent = aBound;
        //                        aPolygon.OutLine.PointList = new List<PointD>(aPList);
        //                        aPolygon.OutLine.Value = aValue;
        //                        aPolygon.IsHighCenter = true;
        //                        if (aValue == bValue)
        //                        {
        //                            if (cValue < aValue)
        //                                aPolygon.IsHighCenter = false;
        //                        }
        //                        aPolygon.OutLine.Type = "Border";
        //                        aPolygon.HoleLines = new List<PolyLine>();
        //                        aPolygonList.Add(aPolygon);
        //                    }
        //                    break; 
        //                }
        //                pIdx += -1;
        //                innerIdx += -1;
        //                if (borderIdx1 != borderIdx2)
        //                {
        //                    borderIdx1 = borderIdx2;
        //                }
        //                //If pIdx = -1 Then
        //                //    pIdx = pNum - 1
        //                //End If
        //                if (innerIdx == -1)
        //                {
        //                    pIdx = pIdx + pNums[borderIdx1];
        //                    innerIdx = pNums[borderIdx1] - 1;
        //                }
        //            } while (true);
        //        }
        //    }

        //    //---- tracing close polygons
        //    List<Polygon> cPolygonlist = new List<Polygon>();
        //    bool isInserted = false;
        //    for (i = 0; i <= aLineList.Count - 1; i++)
        //    {
        //        aLine = (PolyLine)aLineList[i];
        //        if (aLine.Type == "Close")
        //        {
        //            aPolygon.IsBorder = false;
        //            aPolygon.LowValue = aLine.Value;
        //            aPolygon.HighValue = aLine.Value;
        //            aPolygon.Area = GetExtentAndArea(aLine.PointList, ref aBound);
        //            aPolygon.IsClockWise = IsClockwise(aLine.PointList);
        //            aPolygon.Extent = aBound;
        //            aPolygon.OutLine = aLine;
        //            aPolygon.IsHighCenter = true;
        //            aPolygon.HoleLines = new List<PolyLine>();

        //            //---- Sort from big to small
        //            isInserted = false;
        //            for (j = 0; j <= cPolygonlist.Count - 1; j++)
        //            {
        //                if (aPolygon.Area > ((Polygon)cPolygonlist[j]).Area)
        //                {
        //                    cPolygonlist.Insert(j, aPolygon);
        //                    isInserted = true;
        //                    break; 
        //                }
        //            }
        //            if (!isInserted)
        //            {
        //                cPolygonlist.Add(aPolygon);
        //            }
        //        }
        //    }

        //    //---- Juge isHighCenter for border polygons
        //    Extent cBound1 = new Extent();
        //    Extent cBound2 = new Extent();
        //    if (aPolygonList.Count > 0)
        //    {
        //        int outPIdx = 0;
        //        bool IsSides = false;
        //        bool IfSameValue = false;
        //        //---- If all boder polygon lines have same value
        //        aPolygon = (Polygon)aPolygonList[0];
        //        if (aPolygon.LowValue == aPolygon.HighValue)
        //        {
        //            IsSides = false;
        //            outPIdx = aPolygon.StartPointIdx;
        //            do
        //            {
        //                if (aPolygon.IsClockWise)
        //                {
        //                    outPIdx = outPIdx - 1;
        //                    if (outPIdx == -1)
        //                    {
        //                        outPIdx = lineBorderList.Count - 1;
        //                    }
        //                }
        //                else
        //                {
        //                    outPIdx = outPIdx + 1;
        //                    if (outPIdx == lineBorderList.Count)
        //                    {
        //                        outPIdx = 0;
        //                    }
        //                }
        //                bP = (BorderPoint)lineBorderList[outPIdx];
        //                aLine = (PolyLine) aLineList[bP.Id];
        //                if (aLine.Value == aPolygon.LowValue)
        //                {
        //                    if (outPIdx == aPolygon.StartPointIdx)
        //                    {
        //                        IfSameValue = true;
        //                        break; 
        //                    }
        //                    else
        //                    {
        //                        continue;
        //                    }
        //                }
        //                else
        //                {
        //                    IfSameValue = false;
        //                    break; 
        //                }
        //            } while (true);
        //        }

        //        if (IfSameValue)
        //        {
        //            if (cPolygonlist.Count > 0)
        //            {
        //                Polygon cPolygon = new Polygon();
        //                cPolygon = (Polygon)cPolygonlist[0];
        //                cBound1 = cPolygon.Extent;
        //                for (i = 0; i <= aPolygonList.Count - 1; i++)
        //                {
        //                    aPolygon = (Polygon)aPolygonList[i];
        //                    cBound2 = aPolygon.Extent;
        //                    if (cBound1.xMin > cBound2.xMin & cBound1.yMin > cBound2.yMin & cBound1.xMax < cBound2.xMax & cBound1.yMax < cBound2.yMax)
        //                    {
        //                        aPolygon.IsHighCenter = false;
        //                        aPolygonList.Insert(i, aPolygon);
        //                        aPolygonList.RemoveAt(i + 1);
        //                    }
        //                    else
        //                    {
        //                        aPolygon.IsHighCenter = true;
        //                        aPolygonList.Insert(i, aPolygon);
        //                        aPolygonList.RemoveAt(i + 1);
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                bool tf = true;
        //                //---- Temperal solution, not finished
        //                for (i = 0; i <= aPolygonList.Count - 1; i++)
        //                {
        //                    aPolygon = (Polygon)aPolygonList[i];
        //                    tf = !tf;
        //                    aPolygon.IsHighCenter = tf;
        //                    aPolygonList.Insert(i, aPolygon);
        //                    aPolygonList.RemoveAt(i + 1);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            for (i = 0; i <= aPolygonList.Count - 1; i++)
        //            {
        //                aPolygon = (Polygon)aPolygonList[i];
        //                if (aPolygon.LowValue == aPolygon.HighValue)
        //                {
        //                    IsSides = false;
        //                    outPIdx = aPolygon.StartPointIdx;
        //                    do
        //                    {
        //                        if (aPolygon.IsClockWise)
        //                        {
        //                            outPIdx = outPIdx - 1;
        //                            if (outPIdx == -1)
        //                            {
        //                                outPIdx = lineBorderList.Count - 1;
        //                            }
        //                        }
        //                        else
        //                        {
        //                            outPIdx = outPIdx + 1;
        //                            if (outPIdx == lineBorderList.Count)
        //                            {
        //                                outPIdx = 0;
        //                            }
        //                        }
        //                        bP = (BorderPoint)lineBorderList[outPIdx];
        //                        aLine = (PolyLine)aLineList[bP.Id];
        //                        if (aLine.Value == aPolygon.LowValue)
        //                        {
        //                            if (outPIdx == aPolygon.StartPointIdx)
        //                            {
        //                                break; 
        //                            }
        //                            else
        //                            {
        //                                IsSides = !IsSides;
        //                                continue;
        //                            }
        //                        }
        //                        else
        //                        {
        //                            if (IsSides)
        //                            {
        //                                if (aLine.Value < aPolygon.LowValue)
        //                                {
        //                                    aPolygon.IsHighCenter = false;
        //                                    aPolygonList.Insert(i, aPolygon);
        //                                    aPolygonList.RemoveAt(i + 1);
        //                                }
        //                            }
        //                            else
        //                            {
        //                                if (aLine.Value > aPolygon.LowValue)
        //                                {
        //                                    aPolygon.IsHighCenter = false;
        //                                    aPolygonList.Insert(i, aPolygon);
        //                                    aPolygonList.RemoveAt(i + 1);
        //                                }
        //                            }
        //                            break; 
        //                        }
        //                    } while (true);
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        aLine.Type = "Border";
        //        aLine.Value = contour[0];
        //        //newPList.Clear();
        //        //aPoint.X = bBound.xMin;
        //        //aPoint.Y = bBound.yMin;
        //        //newPList.Add(aPoint);
        //        //aPoint.X = bBound.xMin;
        //        //aPoint.Y = bBound.yMax;
        //        //newPList.Add(aPoint);
        //        //aPoint.X = bBound.xMax;
        //        //aPoint.Y = bBound.yMax;
        //        //newPList.Add(aPoint);
        //        //aPoint.X = bBound.xMax;
        //        //aPoint.Y = bBound.yMin;
        //        //newPList.Add(aPoint);
        //        //newPList.Add(newPList[0]);
        //        aLine.PointList = new List<PointD>(aBorder.LineList[0].pointList);

        //        if (aLine.PointList.Count > 0)
        //        {
        //            aPolygon.LowValue = aLine.Value;
        //            aPolygon.HighValue = aLine.Value;
        //            aPolygon.Area = GetExtentAndArea(aLine.PointList, ref aBound);
        //            aPolygon.IsClockWise = IsClockwise(aLine.PointList);
        //            aPolygon.Extent = aBound;
        //            aPolygon.OutLine = aLine;
        //            aPolygon.IsHighCenter = false;
        //            aPolygonList.Add(aPolygon);
        //        }
        //    }

        //    //---- Add close polygons to form total polygons list
        //    aPolygonList.AddRange(cPolygonlist);

        //    //---- Juge siHighCenter for close polygons
        //    int polygonNum = aPolygonList.Count;
        //    Polygon bPolygon = default(Polygon);
        //    for (i = polygonNum - 1; i >= 0; i += -1)
        //    {
        //        aPolygon = (Polygon)aPolygonList[i];
        //        if (aPolygon.OutLine.Type == "Close")
        //        {
        //            cBound1 = aPolygon.Extent;
        //            aValue = aPolygon.LowValue;
        //            aPoint = (PointD)aPolygon.OutLine.PointList[0];
        //            for (j = i - 1; j >= 0; j += -1)
        //            {
        //                bPolygon = (Polygon)aPolygonList[j];
        //                cBound2 = bPolygon.Extent;
        //                bValue = bPolygon.LowValue;
        //                newPList = new List<PointD>(bPolygon.OutLine.PointList);
        //                if (PointInPolygon(newPList, aPoint))
        //                {
        //                    if (cBound1.xMin > cBound2.xMin & cBound1.yMin > cBound2.yMin & cBound1.xMax < cBound2.xMax & cBound1.yMax < cBound2.yMax)
        //                    {
        //                        if (aValue < bValue)
        //                        {
        //                            aPolygon.IsHighCenter = false;
        //                            aPolygonList.Insert(i, aPolygon);
        //                            aPolygonList.RemoveAt(i + 1);
        //                        }
        //                        else if (aValue == bValue)
        //                        {
        //                            if (bPolygon.IsHighCenter)
        //                            {
        //                                aPolygon.IsHighCenter = false;
        //                                aPolygonList.Insert(i, aPolygon);
        //                                aPolygonList.RemoveAt(i + 1);
        //                            }
        //                        }
        //                        break; 
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    newPolygonlist = new List<Polygon>(aPolygonList);

        //    return newPolygonlist;
        //}

        private static List<Polygon> TracingPolygons_Ring(List<PolyLine> LineList, List<BorderPoint> borderList, Border aBorder,
            double[] contour, int[] pNums)
        {
            List<Polygon> aPolygonList = new List<Polygon>();            
            List<PolyLine> aLineList = new List<PolyLine>();
            PolyLine aLine;
            PointD aPoint;
            Polygon aPolygon;
            Extent aBound;
            int i = 0;
            int j = 0;

            aLineList = new List<PolyLine>(LineList);

            //---- Tracing border polygon
            List<PointD> aPList;
            List<PointD> newPList = new List<PointD>();
            BorderPoint bP;
            BorderPoint bP1;
            int[] timesArray = new int[borderList.Count];
            for (i = 0; i < timesArray.Length; i++)
            {
                timesArray[i] = 0;
            }
            int pIdx = 0;
            int pNum = 0;
            int vNum = 0;
            double aValue = 0;
            double bValue = 0;
            double cValue = 0;
            List<BorderPoint> lineBorderList = new List<BorderPoint>();
            int borderIdx1 = 0;
            int borderIdx2 = 0;
            int innerIdx = 0;

            pNum = borderList.Count;
            for (i = 0; i < pNum; i++)
            {
                if ((borderList[i]).Id == -1)
                {
                    continue;
                }
                pIdx = i;                
                lineBorderList.Add(borderList[i]);

                Boolean sameBorderIdx = false;    //The two end points of the contour line are on same inner border
                //---- Clockwise traceing
                if (timesArray[pIdx] < 2)
                {
                    bP = borderList[pIdx];
                    innerIdx = bP.BInnerIdx;
                    aPList = new List<PointD>();
                    List<int> bIdxList = new List<int>();
                    aPList.Add(bP.Point);
                    bIdxList.Add(pIdx);
                    borderIdx1 = bP.BorderIdx;
                    borderIdx2 = borderIdx1;
                    pIdx += 1;
                    innerIdx += 1;
                    //If pIdx = pNum Then
                    //    pIdx = 0
                    //End If
                    if (innerIdx == pNums[borderIdx1] - 1)
                    {
                        pIdx = pIdx - (pNums[borderIdx1] - 1);
                    }
                    vNum = 0;
                    do
                    {
                        bP = borderList[pIdx];
                        //---- Not endpoint of contour
                        if (bP.Id == -1)
                        {
                            if (timesArray[pIdx] == 1)
                            {
                                break;
                            }
                            cValue = bP.Value;
                            aPList.Add(bP.Point);
                            timesArray[pIdx] += 1;
                            bIdxList.Add(pIdx);
                            //---- endpoint of contour
                        }
                        else
                        {
                            if (timesArray[pIdx] == 2)
                            {
                                break;
                            }
                            timesArray[pIdx] += 1;
                            bIdxList.Add(pIdx);
                            aLine = aLineList[bP.Id];
                            //---- Set high and low value of the polygon
                            if (vNum == 0)
                            {
                                aValue = aLine.Value;
                                bValue = aLine.Value;
                                vNum += 1;
                            }
                            else
                            {
                                if (aValue == bValue)
                                {
                                    if (aLine.Value > aValue)
                                    {
                                        bValue = aLine.Value;
                                    }
                                    else if (aLine.Value < aValue)
                                    {
                                        aValue = aLine.Value;
                                    }
                                    vNum += 1;
                                }
                            }
                            newPList = new List<PointD>(aLine.PointList);
                            aPoint = newPList[0];
                            //If Not (Math.Abs(bP.point.x - aPoint.x) < 0.000001 And _
                            //  Math.Abs(bP.point.y - aPoint.y) < 0.000001) Then    '---- Not start point
                            //---- Not start point
                            if (!(bP.Point.X == aPoint.X && bP.Point.Y == aPoint.Y))
                            {
                                newPList.Reverse();
                            }
                            aPList.AddRange(newPList);
                            //---- Find corresponding border point
                            for (j = 0; j < borderList.Count; j++)
                            {
                                if (j != pIdx)
                                {
                                    bP1 = borderList[j];
                                    if (bP1.Id == bP.Id)
                                    {
                                        pIdx = j;
                                        innerIdx = bP1.BInnerIdx;
                                        timesArray[pIdx] += 1;
                                        bIdxList.Add(pIdx);
                                        borderIdx2 = bP1.BorderIdx;
                                        if (bP.BorderIdx > 0 && bP.BorderIdx == bP1.BorderIdx)
                                        {
                                            sameBorderIdx = true;
                                        }
                                        break; 
                                    }
                                }
                            }
                        }

                        //---- Return to start point, tracing finish
                        if (pIdx == i)
                        {
                            if (aPList.Count > 0)
                            {
                                if (sameBorderIdx)
                                {
                                    Boolean isTooBig = false;
                                    int baseNum = 0;
                                    for (int idx = 0; idx < bP.BorderIdx; idx++)
                                    {
                                        baseNum += pNums[idx];
                                    }
                                    int sIdx = baseNum;
                                    int eIdx = baseNum + pNums[bP.BorderIdx];
                                    int theIdx = sIdx;
                                    for (int idx = sIdx; idx < eIdx; idx++)
                                    {
                                        if (!bIdxList.Contains(idx))
                                        {
                                            theIdx = idx;
                                            break;
                                        }
                                    }
                                    if (PointInPolygon(aPList, borderList[theIdx].Point))
                                    {
                                        isTooBig = true;
                                    }

                                    if (isTooBig)
                                    {
                                        break;
                                    }
                                }

                                aPolygon = new Polygon();
                                aPolygon.IsBorder = true;
                                aPolygon.IsInnerBorder = sameBorderIdx;
                                aPolygon.LowValue = aValue;
                                aPolygon.HighValue = bValue;
                                aBound = new Extent();
                                aPolygon.Area = GetExtentAndArea(aPList, ref aBound);
                                aPolygon.IsClockWise = true;
                                aPolygon.StartPointIdx = lineBorderList.Count - 1;
                                aPolygon.Extent = aBound;
                                aPolygon.OutLine.PointList = aPList;
                                aPolygon.OutLine.Value = aValue;
                                aPolygon.IsHighCenter = true;
                                if (aValue == bValue)
                                {
                                    if (cValue < aValue)
                                        aPolygon.IsHighCenter = false;
                                }
                                aPolygon.OutLine.Type = "Border";
                                aPolygon.HoleLines = new List<PolyLine>();
                                aPolygonList.Add(aPolygon);
                            }
                            break;
                        }
                        pIdx += 1;
                        innerIdx += 1;
                        if (borderIdx1 != borderIdx2)
                        {
                            borderIdx1 = borderIdx2;
                        }

                        //if (pIdx == pNum)
                        //    pIdx = 0;

                        if (innerIdx == pNums[borderIdx1] - 1)
                        {
                            pIdx = pIdx - (pNums[borderIdx1] - 1);
                            innerIdx = 0;
                        }
                    } while (true);
                }

                sameBorderIdx = false;
                //---- Anticlockwise traceing
                pIdx = i;
                if (timesArray[pIdx] < 2)
                {
                    aPList = new List<PointD>();
                    List<int> bIdxList = new List<int>();
                    bP = borderList[pIdx];
                    innerIdx = bP.BInnerIdx;
                    aPList.Add(bP.Point);
                    bIdxList.Add(pIdx);
                    borderIdx1 = bP.BorderIdx;
                    borderIdx2 = borderIdx1;
                    pIdx += -1;
                    innerIdx += -1;
                    //If pIdx = -1 Then
                    //    pIdx = pNum - 1
                    //End If
                    if (innerIdx == -1)
                    {
                        pIdx = pIdx + (pNums[borderIdx1] - 1);
                    }
                    vNum = 0;
                    do
                    {
                        bP = borderList[pIdx];
                        //---- Not endpoint of contour
                        if (bP.Id == -1)
                        {
                            if (timesArray[pIdx] == 1)
                            {
                                break;
                            }
                            cValue = bP.Value;
                            aPList.Add(bP.Point);
                            timesArray[pIdx] += 1;
                            bIdxList.Add(pIdx);
                            //---- endpoint of contour
                        }
                        else
                        {
                            if (timesArray[pIdx] == 2)
                            {
                                break;
                            }
                            timesArray[pIdx] += 1;
                            bIdxList.Add(pIdx);
                            aLine = aLineList[bP.Id];
                            if (vNum == 0)
                            {
                                aValue = aLine.Value;
                                bValue = aLine.Value;
                                vNum += 1;
                            }
                            else
                            {
                                if (aValue == bValue)
                                {
                                    if (aLine.Value > aValue)
                                    {
                                        bValue = aLine.Value;
                                    }
                                    else if (aLine.Value < aValue)
                                    {
                                        aValue = aLine.Value;
                                    }
                                    vNum += 1;
                                }
                            }
                            newPList = new List<PointD>(aLine.PointList);
                            aPoint = newPList[0];
                            //If Not (Math.Abs(bP.point.x - aPoint.x) < 0.000001 And _
                            //  Math.Abs(bP.point.y - aPoint.y) < 0.000001) Then    '---- Start point
                            //---- Start point
                            if (!(bP.Point.X == aPoint.X && bP.Point.Y == aPoint.Y))
                            {
                                newPList.Reverse();
                            }
                            aPList.AddRange(newPList);
                            for (j = 0; j < borderList.Count; j++)
                            {
                                if (j != pIdx)
                                {
                                    bP1 = borderList[j];
                                    if (bP1.Id == bP.Id)
                                    {
                                        pIdx = j;
                                        innerIdx = bP1.BInnerIdx;
                                        timesArray[pIdx] += 1;
                                        bIdxList.Add(pIdx);
                                        borderIdx2 = bP1.BorderIdx;
                                        if (bP.BorderIdx > 0 && bP.BorderIdx == bP1.BorderIdx)
                                        {
                                            sameBorderIdx = true;
                                        }
                                        break; 
                                    }
                                }
                            }
                        }

                        if (pIdx == i)
                        {
                            if (aPList.Count > 0)
                            {
                                if (sameBorderIdx)
                                {
                                    Boolean isTooBig = false;
                                    int baseNum = 0;
                                    for (int idx = 0; idx < bP.BorderIdx; idx++)
                                    {
                                        baseNum += pNums[idx];
                                    }
                                    int sIdx = baseNum;
                                    int eIdx = baseNum + pNums[bP.BorderIdx];
                                    int theIdx = sIdx;
                                    for (int idx = sIdx; idx < eIdx; idx++)
                                    {
                                        if (!bIdxList.Contains(idx))
                                        {
                                            theIdx = idx;
                                            break;
                                        }
                                    }
                                    if (PointInPolygon(aPList, borderList[theIdx].Point))
                                    {
                                        isTooBig = true;
                                    }

                                    if (isTooBig)
                                    {
                                        break;
                                    }
                                }

                                aPolygon = new Polygon();
                                aPolygon.IsBorder = true;
                                aPolygon.IsInnerBorder = sameBorderIdx;
                                aPolygon.LowValue = aValue;
                                aPolygon.HighValue = bValue;
                                aBound = new Extent();
                                aPolygon.Area = GetExtentAndArea(aPList, ref aBound);
                                aPolygon.IsClockWise = false;
                                aPolygon.StartPointIdx = lineBorderList.Count - 1;
                                aPolygon.Extent = aBound;
                                aPolygon.OutLine.PointList = aPList;
                                aPolygon.OutLine.Value = aValue;
                                aPolygon.IsHighCenter = true;
                                if (aValue == bValue)
                                {
                                    if (cValue < aValue)
                                        aPolygon.IsHighCenter = false;
                                }
                                aPolygon.OutLine.Type = "Border";
                                aPolygon.HoleLines = new List<PolyLine>();
                                aPolygonList.Add(aPolygon);
                            }
                            break;
                        }
                        pIdx += -1;
                        innerIdx += -1;
                        if (borderIdx1 != borderIdx2)
                        {
                            borderIdx1 = borderIdx2;
                        }
                        //If pIdx = -1 Then
                        //    pIdx = pNum - 1
                        //End If
                        if (innerIdx == -1)
                        {
                            pIdx = pIdx + pNums[borderIdx1];
                            innerIdx = pNums[borderIdx1] - 1;
                        }
                    } while (true);
                }
            }

            //---- tracing close polygons
            List<Polygon> cPolygonlist = new List<Polygon>();
            bool isInserted = false;
            for (i = 0; i < aLineList.Count; i++)
            {
                aLine = aLineList[i];
                if (aLine.Type == "Close")
                {
                    aPolygon = new Polygon();
                    aPolygon.IsBorder = false;
                    aPolygon.LowValue = aLine.Value;
                    aPolygon.HighValue = aLine.Value;
                    aBound = new Extent();
                    aPolygon.Area = GetExtentAndArea(aLine.PointList, ref aBound);
                    aPolygon.IsClockWise = IsClockwise(aLine.PointList);
                    aPolygon.Extent = aBound;
                    aPolygon.OutLine = aLine;
                    aPolygon.IsHighCenter = true;
                    aPolygon.HoleLines = new List<PolyLine>();

                    //---- Sort from big to small
                    isInserted = false;
                    for (j = 0; j < cPolygonlist.Count; j++)
                    {
                        if (aPolygon.Area > (cPolygonlist[j]).Area)
                        {
                            cPolygonlist.Insert(j, aPolygon);
                            isInserted = true;
                            break;
                        }
                    }
                    if (!isInserted)
                    {
                        cPolygonlist.Add(aPolygon);
                    }
                }
            }

            //---- Juge isHighCenter for border polygons
            if (aPolygonList .Count == 0)
            {
                aLine = new PolyLine();
                aLine.Type = "Border";
                aLine.Value = contour[0];                
                aLine.PointList = new List<PointD>(aBorder.LineList[0].pointList);

                if (aLine.PointList.Count > 0)
                {
                    aPolygon = new Polygon();
                    aPolygon.LowValue = aLine.Value;
                    aPolygon.HighValue = aLine.Value;
                    aBound = new Extent();
                    aPolygon.Area = GetExtentAndArea(aLine.PointList, ref aBound);
                    aPolygon.IsClockWise = IsClockwise(aLine.PointList);
                    aPolygon.Extent = aBound;
                    aPolygon.OutLine = aLine;
                    aPolygon.IsHighCenter = false;
                    aPolygonList.Add(aPolygon);
                }
            }            

            //---- Add close polygons to form total polygons list
            aPolygonList.AddRange(cPolygonlist);

            //---- Juge siHighCenter for close polygons
            Extent cBound1;
            Extent cBound2;
            int polygonNum = aPolygonList.Count;
            Polygon bPolygon;
            for (i = polygonNum - 1; i >= 0; i += -1)
            {
                aPolygon = aPolygonList[i];
                if (aPolygon.OutLine.Type == "Close")
                {
                    cBound1 = aPolygon.Extent;
                    aValue = aPolygon.LowValue;
                    aPoint = aPolygon.OutLine.PointList[0];
                    for (j = i - 1; j >= 0; j += -1)
                    {
                        bPolygon = aPolygonList[j];
                        cBound2 = bPolygon.Extent;
                        bValue = bPolygon.LowValue;
                        newPList = new List<PointD>(bPolygon.OutLine.PointList);
                        if (PointInPolygon(newPList, aPoint))
                        {
                            if (cBound1.xMin > cBound2.xMin & cBound1.yMin > cBound2.yMin & cBound1.xMax < cBound2.xMax & cBound1.yMax < cBound2.yMax)
                            {
                                if (aValue < bValue)
                                {
                                    aPolygon.IsHighCenter = false;
                                    //aPolygonList.Insert(i, aPolygon);
                                    //aPolygonList.RemoveAt(i + 1);
                                }
                                else if (aValue == bValue)
                                {
                                    if (bPolygon.IsHighCenter)
                                    {
                                        aPolygon.IsHighCenter = false;
                                        //aPolygonList.Insert(i, aPolygon);
                                        //aPolygonList.RemoveAt(i + 1);
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }

            return aPolygonList;
        }

        private static List<Polygon> AddPolygonHoles(List<Polygon> polygonList)
        {                        
            List<Polygon> holePolygons = new List<Polygon>();
            int i, j;
            for (i = 0; i < polygonList.Count; i++)
            {
                Polygon aPolygon = polygonList[i];
                if (!aPolygon.IsBorder)
                {
                    aPolygon.HoleIndex = 1;
                    holePolygons.Add(aPolygon);
                }
            }

            if (holePolygons.Count == 0)
                return polygonList;
            else
            {
                List<Polygon> newPolygons = new List<Polygon>();
                for (i = 1; i < holePolygons.Count; i++)
                {
                    Polygon aPolygon = holePolygons[i];                  
                    for (j = i - 1; j >= 0; j--)
                    {
                        Polygon bPolygon = holePolygons[j];
                        if (bPolygon.Extent.Include(aPolygon.Extent))
                        {
                            if (PointInPolygon(bPolygon.OutLine.PointList, aPolygon.OutLine.PointList[0]))
                            {
                                aPolygon.HoleIndex = bPolygon.HoleIndex + 1;
                                bPolygon.AddHole(aPolygon);
                                //holePolygons[i] = aPolygon;
                                //holePolygons[j] = bPolygon;
                                break;
                            }
                        }
                    }                    
                }
                List<Polygon> hole1Polygons = new List<Polygon>();
                for (i = 0; i < holePolygons.Count; i++)
                {
                    if (holePolygons[i].HoleIndex == 1)
                        hole1Polygons.Add(holePolygons[i]);
                }

                for (i = 0; i < polygonList.Count; i++)
                {
                    Polygon aPolygon = polygonList[i];
                    if (aPolygon.IsBorder == true)
                    {
                        for (j = 0; j < hole1Polygons.Count; j++)
                        {
                            Polygon bPolygon = hole1Polygons[j];
                            if (aPolygon.Extent.Include(bPolygon.Extent))
                            {
                                if (PointInPolygon(aPolygon.OutLine.PointList, bPolygon.OutLine.PointList[0]))
                                {
                                    aPolygon.AddHole(bPolygon);
                                }
                            }
                        }
                        newPolygons.Add(aPolygon);
                    }
                }
                newPolygons.AddRange(holePolygons);

                return newPolygons;
            }
        }

        private static List<Polygon> AddPolygonHoles_Ring(List<Polygon> polygonList)
        {
            List<Polygon> holePolygons = new List<Polygon>();
            int i, j;
            for (i = 0; i < polygonList.Count; i++)
            {
                Polygon aPolygon = polygonList[i];
                if (!aPolygon.IsBorder || aPolygon.IsInnerBorder)
                {
                    aPolygon.HoleIndex = 1;
                    holePolygons.Add(aPolygon);
                }
            }

            if (holePolygons.Count == 0)
                return polygonList;
            else
            {
                List<Polygon> newPolygons = new List<Polygon>();
                for (i = 1; i < holePolygons.Count; i++)
                {
                    Polygon aPolygon = holePolygons[i];
                    for (j = i - 1; j >= 0; j--)
                    {
                        Polygon bPolygon = holePolygons[j];
                        if (bPolygon.Extent.Include(aPolygon.Extent))
                        {
                            if (PointInPolygon(bPolygon.OutLine.PointList, aPolygon.OutLine.PointList[0]))
                            {
                                aPolygon.HoleIndex = bPolygon.HoleIndex + 1;
                                bPolygon.AddHole(aPolygon);
                                //holePolygons[i] = aPolygon;
                                //holePolygons[j] = bPolygon;
                                break;
                            }
                        }
                    }
                }
                List<Polygon> hole1Polygons = new List<Polygon>();
                for (i = 0; i < holePolygons.Count; i++)
                {
                    if (holePolygons[i].HoleIndex == 1)
                        hole1Polygons.Add(holePolygons[i]);
                }

                for (i = 0; i < polygonList.Count; i++)
                {
                    Polygon aPolygon = polygonList[i];
                    if (aPolygon.IsBorder && !aPolygon.IsInnerBorder)
                    {
                        for (j = 0; j < hole1Polygons.Count; j++)
                        {
                            Polygon bPolygon = hole1Polygons[j];
                            if (aPolygon.Extent.Include(bPolygon.Extent))
                            {
                                if (PointInPolygon(aPolygon.OutLine.PointList, bPolygon.OutLine.PointList[0]))
                                {
                                    aPolygon.AddHole(bPolygon);
                                }
                            }
                        }
                        newPolygons.Add(aPolygon);
                    }
                }
                newPolygons.AddRange(holePolygons);

                return newPolygons;
            }
        }

        private static void AddHoles_Ring(ref List<Polygon> polygonList, List<List<PointD>> holeList)
        {
            int i, j;
            for (i = 0; i < holeList.Count; i++)
            {
                List<PointD> holePs = holeList[i];
                Extent aExtent = GetExtent(holePs);
                for (j = 0; j < polygonList.Count; j++)
                {
                    Polygon aPolygon = polygonList[j];
                    if (aPolygon.Extent.Include(aExtent))
                    {
                        bool isHole = true;
                        foreach (PointD aP in holePs)
                        {
                            if (!PointInPolygon(aPolygon.OutLine.PointList, aP))
                            {
                                isHole = false;
                                break;
                            }
                        }
                        if (isHole)
                        {
                            aPolygon.AddHole(holePs);
                            //polygonList[j] = aPolygon;
                            break;
                        }
                    }
                }
            }
        }

        #endregion

        #region Clipping
        private static List<PolyLine> CutPolyline(PolyLine inPolyline, List<PointD> clipPList)
        {
            List<PolyLine> newPolylines = new List<PolyLine>();
            List<PointD> aPList = inPolyline.PointList;
            Extent plExtent = GetExtent(aPList);
            Extent cutExtent = GetExtent(clipPList);

            if (!IsExtentCross(plExtent, cutExtent))
                return newPolylines;

            int i, j;

            if (!IsClockwise(clipPList))    //---- Make cut polygon clockwise
                clipPList.Reverse();

            //Judge if all points of the polyline are in the cut polygon
            if (PointInPolygon(clipPList, aPList[0]))
            {
                bool isAllIn = true;
                int notInIdx = 0;
                for (i = 0; i < aPList.Count; i++)
                {
                    if (!PointInPolygon(clipPList, aPList[i]))
                    {
                        notInIdx = i;
                        isAllIn = false;
                        break;
                    }
                }
                //if (!isAllIn && inPolyline.Type == "Close")   //Put start point outside of the cut polygon
                if (!isAllIn)
                {
                    if (inPolyline.Type == "Close")
                    {
                        List<PointD> bPList = new List<PointD>();
                        bPList.AddRange(aPList.GetRange(notInIdx, aPList.Count - notInIdx));
                        bPList.AddRange(aPList.GetRange(1, notInIdx - 1));
                        //for (i = notInIdx; i < aPList.Count; i++)
                        //    bPList.Add(aPList[i]);

                        //for (i = 1; i < notInIdx; i++)
                        //    bPList.Add(aPList[i]);

                        bPList.Add(bPList[0]);
                        aPList = new List<PointD>(bPList);
                    }
                    else
                    {
                        aPList.Reverse();
                    }
                }
                else    //the input polygon is inside the cut polygon
                {
                    newPolylines.Add(inPolyline);
                    return newPolylines;
                }
            }

            //Cutting            
            bool isInPolygon = PointInPolygon(clipPList, aPList[0]);
            PointD q1, q2, p1, p2, IPoint;
            Line lineA, lineB;
            List<PointD> newPlist = new List<PointD>();
            PolyLine bLine;            
            p1 = aPList[0];
            for (i = 1; i < aPList.Count; i++)
            {
                p2 = aPList[i];
                if (PointInPolygon(clipPList, p2))
                {
                    if (!isInPolygon)
                    {
                        IPoint = new PointD();
                        lineA = new Line();
                        lineA.P1 = p1;
                        lineA.P2 = p2;
                        q1 = clipPList[clipPList.Count - 1];
                        for (j = 0; j < clipPList.Count; j++)
                        {
                            q2 = clipPList[j];
                            lineB = new Line();
                            lineB.P1 = q1;
                            lineB.P2 = q2;
                            if (IsLineSegmentCross(lineA, lineB))
                            {
                                IPoint = GetCrossPoint(lineA, lineB);
                                break;
                            }
                            q1 = q2;
                        }
                        newPlist.Add(IPoint);
                        //aType = "Border";
                    }
                    newPlist.Add(aPList[i]);
                    isInPolygon = true;
                }
                else
                {
                    if (isInPolygon)
                    {
                        IPoint = new PointD();
                        lineA = new Line();
                        lineA.P1 = p1;
                        lineA.P2 = p2;
                        q1 = clipPList[clipPList.Count - 1];
                        for (j = 0; j < clipPList.Count; j++)
                        {
                            q2 = clipPList[j];
                            lineB = new Line();
                            lineB.P1 = q1;
                            lineB.P2 = q2;
                            if (IsLineSegmentCross(lineA, lineB))
                            {
                                IPoint = GetCrossPoint(lineA, lineB);
                                break;
                            }
                            q1 = q2;
                        }
                        newPlist.Add(IPoint);

                        bLine = new PolyLine();
                        bLine.Value = inPolyline.Value;
                        bLine.Type = inPolyline.Type;
                        bLine.PointList = newPlist;
                        newPolylines.Add(bLine);
                        isInPolygon = false;
                        newPlist = new List<PointD>();
                        //aType = "Border";
                    }
                }
                p1 = p2;
            }

            if (isInPolygon && newPlist.Count > 1)
            {
                bLine = new PolyLine();
                bLine.Value = inPolyline.Value;
                bLine.Type = inPolyline.Type;
                bLine.PointList = newPlist;
                newPolylines.Add(bLine);
            }

            return newPolylines;
        }

        //private static List<Polygon> CutPolygon_Hole_Old(Polygon inPolygon, List<PointD> clipPList)
        //{
        //    List<Polygon> newPolygons = new List<Polygon>();
        //    List<PolyLine> newPolylines = new List<PolyLine>();
        //    List<PointD> aPList = inPolygon.OutLine.PointList;
        //    Extent plExtent = GetExtent(aPList);
        //    Extent cutExtent = GetExtent(clipPList);

        //    if (!IsExtentCross(plExtent, cutExtent))
        //        return newPolygons;

        //    int i, j;

        //    if (!IsClockwise(clipPList))    //---- Make cut polygon clockwise
        //        clipPList.Reverse();

        //    //Judge if all points of the polyline are in the cut polygon - outline   
        //    List<List<PointD>> newLines = new List<List<PointD>>();
        //    if (PointInPolygon(clipPList, aPList[0]))
        //    {
        //        bool isAllIn = true;
        //        int notInIdx = 0;
        //        for (i = 0; i < aPList.Count; i++)
        //        {
        //            if (!PointInPolygon(clipPList, aPList[i]))
        //            {
        //                notInIdx = i;
        //                isAllIn = false;
        //                break;
        //            }
        //        }
        //        if (!isAllIn)   //Put start point outside of the cut polygon
        //        {
        //            List<PointD> bPList = new List<PointD>();
        //            bPList.AddRange(aPList.GetRange(notInIdx, aPList.Count - notInIdx));
        //            bPList.AddRange(aPList.GetRange(1, notInIdx - 1));
        //            //for (i = notInIdx; i < aPList.Count; i++)
        //            //    bPList.Add(aPList[i]);

        //            //for (i = 1; i < notInIdx; i++)
        //            //    bPList.Add(aPList[i]);

        //            bPList.Add(bPList[0]);
        //            //if (!IsClockwise(bPList))
        //            //    bPList.Reverse();
        //            newLines.Add(bPList);
        //        }
        //        else    //the input polygon is inside the cut polygon
        //        {
        //            newPolygons.Add(inPolygon);
        //            return newPolygons;
        //        }
        //    }
        //    else
        //    {
        //        newLines.Add(aPList);
        //    }

        //    //Holes
        //    List<List<PointD>> holeLines = new List<List<PointD>>();
        //    for (int h = 0; h < inPolygon.HoleLines.Count; h++)
        //    {
        //        List<PointD> holePList = inPolygon.HoleLines[h].PointList;
        //        plExtent = GetExtent(holePList);
        //        if (!IsExtentCross(plExtent, cutExtent))
        //            continue;

        //        if (PointInPolygon(clipPList, holePList[0]))
        //        {
        //            bool isAllIn = true;
        //            int notInIdx = 0;
        //            for (i = 0; i < holePList.Count; i++)
        //            {
        //                if (!PointInPolygon(clipPList, holePList[i]))
        //                {
        //                    notInIdx = i;
        //                    isAllIn = false;
        //                    break;
        //                }
        //            }
        //            if (!isAllIn)   //Put start point outside of the cut polygon
        //            {
        //                List<PointD> bPList = new List<PointD>();
        //                bPList.AddRange(holePList.GetRange(notInIdx, holePList.Count - notInIdx));
        //                bPList.AddRange(holePList.GetRange(1, notInIdx - 1));
        //                //for (i = notInIdx; i < aPList.Count; i++)
        //                //    bPList.Add(aPList[i]);

        //                //for (i = 1; i < notInIdx; i++)
        //                //    bPList.Add(aPList[i]);

        //                bPList.Add(bPList[0]);
        //                newLines.Add(bPList);
        //            }
        //            else    //the hole is inside the cut polygon
        //            {
        //                holeLines.Add(inPolygon.HoleLines[h].PointList);
        //            }
        //        }
        //        else
        //            newLines.Add(holePList);
        //    }

        //    //Prepare border point list
        //    List<BorderPoint> borderList = new List<BorderPoint>();
        //    BorderPoint aBP = new BorderPoint();
        //    foreach (PointD aP in clipPList)
        //    {
        //        aBP = new BorderPoint();
        //        aBP.Point = aP;
        //        aBP.Id = -1;
        //        borderList.Add(aBP);
        //    }

        //    //Cutting         
        //    List<BorderPoint> borderPList = new List<BorderPoint>();
        //    for (int l = 0; l < newLines.Count; l++)
        //    {
        //        aPList = newLines[l];
        //        bool isInPolygon = false;
        //        PointD q1, q2, p1, p2, IPoint = new PointD();
        //        Line lineA, lineB;
        //        List<PointD> newPlist = new List<PointD>();
        //        PolyLine bLine = new PolyLine();
        //        p1 = aPList[0];
        //        int lastJ = 0;
        //        for (i = 1; i < aPList.Count; i++)
        //        {
        //            p2 = aPList[i];
        //            if (PointInPolygon(clipPList, p2))
        //            {
        //                if (!isInPolygon)
        //                {
        //                    lineA.P1 = p1;
        //                    lineA.P2 = p2;
        //                    q1 = (PointD)clipPList[clipPList.Count - 1];
        //                    for (j = 0; j < clipPList.Count; j++)
        //                    {
        //                        q2 = clipPList[j];
        //                        lineB.P1 = q1;
        //                        lineB.P2 = q2;
        //                        if (IsLineSegmentCross(lineA, lineB))
        //                        {
        //                            //if (lastJ == j)
        //                            //{
        //                            //    j += 1;
        //                            //    q1 = q2;
        //                            //    q2 = clipPList[j];
        //                            //    lineB.P1 = q1;
        //                            //    lineB.P2 = q2;
        //                            //}
        //                            IPoint = GetCrossPoint(lineA, lineB);
        //                            aBP = new BorderPoint();
        //                            aBP.Id = newPolylines.Count;
        //                            aBP.Point = IPoint;
        //                            borderPList.Add(aBP);
        //                            lastJ = j;
        //                            break;
        //                        }
        //                        q1 = q2;
        //                    }
        //                    newPlist.Add(IPoint);
        //                }
        //                newPlist.Add(aPList[i]);
        //                isInPolygon = true;
        //            }
        //            else
        //            {
        //                if (isInPolygon)
        //                {
        //                    lineA.P1 = p1;
        //                    lineA.P2 = p2;
        //                    q1 = clipPList[clipPList.Count - 1];
        //                    for (j = 0; j < clipPList.Count; j++)
        //                    {
        //                        q2 = clipPList[j];
        //                        lineB.P1 = q1;
        //                        lineB.P2 = q2;
        //                        if (IsLineSegmentCross(lineA, lineB))
        //                        {
        //                            //if (lastJ == j)
        //                            //{
        //                            //    j += 1;
        //                            //    q1 = q2;
        //                            //    q2 = clipPList[j];
        //                            //    lineB.P1 = q1;
        //                            //    lineB.P2 = q2;
        //                            //}
        //                            IPoint = GetCrossPoint(lineA, lineB);
        //                            aBP = new BorderPoint();
        //                            aBP.Id = newPolylines.Count;
        //                            aBP.Point = IPoint;
        //                            borderPList.Add(aBP);
        //                            lastJ = j;
        //                            break;
        //                        }
        //                        q1 = q2;
        //                    }
        //                    newPlist.Add(IPoint);

        //                    bLine = new PolyLine();
        //                    bLine.Value = inPolygon.OutLine.Value;
        //                    bLine.Type = inPolygon.OutLine.Type;
        //                    bLine.PointList = new List<PointD>(newPlist);
        //                    newPolylines.Add(bLine);

        //                    isInPolygon = false;
        //                    newPlist = new List<PointD>();
        //                }
        //            }
        //            p1 = p2;
        //        }
        //    }

        //    if (newPolylines.Count > 0)
        //    {
        //        borderList = InsertPoint2Border(borderPList, borderList);

        //        //Tracing polygons
        //        newPolygons = TracingClipPolygons(inPolygon, newPolylines, borderList);
        //    }
        //    else
        //    {
        //        if (PointInPolygon(aPList, clipPList[0]))
        //        {
        //            Extent aBound = new Extent();
        //            Polygon aPolygon = new Polygon();
        //            aPolygon.IsBorder = true;
        //            aPolygon.LowValue = inPolygon.LowValue;
        //            aPolygon.HighValue = inPolygon.HighValue;
        //            aPolygon.Area = GetExtentAndArea(clipPList, ref aBound);
        //            aPolygon.IsClockWise = true;
        //            //aPolygon.StartPointIdx = lineBorderList.Count - 1;
        //            aPolygon.Extent = aBound;
        //            aPolygon.OutLine.PointList = new List<PointD>(clipPList);
        //            aPolygon.OutLine.Value = inPolygon.LowValue;
        //            aPolygon.IsHighCenter = inPolygon.IsHighCenter;
        //            aPolygon.OutLine.Type = "Border";
        //            aPolygon.HoleLines = new List<PolyLine>();

        //            newPolygons.Add(aPolygon);
        //        }
        //    }

        //    if (holeLines.Count > 0)
        //    {
        //        AddHoles_Ring(ref newPolygons, holeLines);
        //    }

        //    return newPolygons;
        //}

        private static List<Polygon> CutPolygon_Hole(Polygon inPolygon, List<PointD> clipPList)
        {
            List<Polygon> newPolygons = new List<Polygon>();
            List<PolyLine> newPolylines = new List<PolyLine>();
            List<PointD> aPList = inPolygon.OutLine.PointList;
            Extent plExtent = GetExtent(aPList);
            Extent cutExtent = GetExtent(clipPList);

            if (!IsExtentCross(plExtent, cutExtent))
                return newPolygons;

            int i, j;

            if (!IsClockwise(clipPList))    //---- Make cut polygon clockwise
                clipPList.Reverse();

            //Judge if all points of the polyline are in the cut polygon - outline   
            List<List<PointD>> newLines = new List<List<PointD>>();
            if (PointInPolygon(clipPList, aPList[0]))
            {
                bool isAllIn = true;
                int notInIdx = 0;
                for (i = 0; i < aPList.Count; i++)
                {
                    if (!PointInPolygon(clipPList, aPList[i]))
                    {
                        notInIdx = i;
                        isAllIn = false;
                        break;
                    }
                }
                if (!isAllIn)   //Put start point outside of the cut polygon
                {
                    List<PointD> bPList = new List<PointD>();
                    bPList.AddRange(aPList.GetRange(notInIdx, aPList.Count - notInIdx));
                    bPList.AddRange(aPList.GetRange(1, notInIdx - 1));
                    //for (i = notInIdx; i < aPList.Count; i++)
                    //    bPList.Add(aPList[i]);

                    //for (i = 1; i < notInIdx; i++)
                    //    bPList.Add(aPList[i]);

                    bPList.Add(bPList[0]);
                    //if (!IsClockwise(bPList))
                    //    bPList.Reverse();
                    newLines.Add(bPList);
                }
                else    //the input polygon is inside the cut polygon
                {
                    newPolygons.Add(inPolygon);
                    return newPolygons;
                }
            }
            else
            {
                newLines.Add(aPList);
            }

            //Holes
            List<List<PointD>> holeLines = new List<List<PointD>>();
            for (int h = 0; h < inPolygon.HoleLines.Count; h++)
            {
                List<PointD> holePList = inPolygon.HoleLines[h].PointList;
                plExtent = GetExtent(holePList);
                if (!IsExtentCross(plExtent, cutExtent))
                    continue;

                if (PointInPolygon(clipPList, holePList[0]))
                {
                    bool isAllIn = true;
                    int notInIdx = 0;
                    for (i = 0; i < holePList.Count; i++)
                    {
                        if (!PointInPolygon(clipPList, holePList[i]))
                        {
                            notInIdx = i;
                            isAllIn = false;
                            break;
                        }
                    }
                    if (!isAllIn)   //Put start point outside of the cut polygon
                    {
                        List<PointD> bPList = new List<PointD>();
                        bPList.AddRange(holePList.GetRange(notInIdx, holePList.Count - notInIdx));
                        bPList.AddRange(holePList.GetRange(1, notInIdx - 1));
                        //for (i = notInIdx; i < aPList.Count; i++)
                        //    bPList.Add(aPList[i]);

                        //for (i = 1; i < notInIdx; i++)
                        //    bPList.Add(aPList[i]);

                        bPList.Add(bPList[0]);
                        newLines.Add(bPList);
                    }
                    else    //the hole is inside the cut polygon
                    {
                        holeLines.Add(holePList);
                    }
                }
                else
                    newLines.Add(holePList);
            }

            //Prepare border point list
            List<BorderPoint> borderList = new List<BorderPoint>();
            BorderPoint aBP = new BorderPoint();
            foreach (PointD aP in clipPList)
            {
                aBP = new BorderPoint();
                aBP.Point = aP;
                aBP.Id = -1;
                borderList.Add(aBP);
            }

            //Cutting                     
            for (int l = 0; l < newLines.Count; l++)
            {
                aPList = newLines[l];
                bool isInPolygon = false;
                PointD q1, q2, p1, p2, IPoint;
                Line lineA, lineB;
                List<PointD> newPlist = new List<PointD>();
                PolyLine bLine;
                p1 = aPList[0];
                int inIdx = -1, outIdx = -1;                
                bool newLine = true;
                int a1 = 0;
                for (i = 1; i < aPList.Count; i++)
                {
                    p2 = aPList[i];
                    if (PointInPolygon(clipPList, p2))
                    {
                        if (!isInPolygon)
                        {
                            lineA = new Line();
                            lineA.P1 = p1;
                            lineA.P2 = p2;
                            q1 = borderList[borderList.Count - 1].Point;
                            IPoint = new PointD();                            
                            for (j = 0; j < borderList.Count; j++)
                            {
                                q2 = borderList[j].Point;
                                lineB = new Line();
                                lineB.P1 = q1;
                                lineB.P2 = q2;
                                if (IsLineSegmentCross(lineA, lineB))
                                {                                    
                                    IPoint = GetCrossPoint(lineA, lineB);
                                    aBP = new BorderPoint();
                                    aBP.Id = newPolylines.Count;
                                    aBP.Point = IPoint;
                                    borderList.Insert(j, aBP);
                                    inIdx = j;
                                    break;
                                }
                                q1 = q2;
                            }
                            newPlist.Add(IPoint);
                        }
                        newPlist.Add(aPList[i]);
                        isInPolygon = true;
                    }
                    else
                    {
                        if (isInPolygon)
                        {
                            lineA = new Line();
                            lineA.P1 = p1;
                            lineA.P2 = p2;
                            q1 = borderList[borderList.Count - 1].Point;
                            IPoint = new PointD();
                            for (j = 0; j < borderList.Count; j++)
                            {
                                q2 = borderList[j].Point;
                                lineB = new Line();
                                lineB.P1 = q1;
                                lineB.P2 = q2;
                                if (IsLineSegmentCross(lineA, lineB))
                                {                                    
                                    if (!newLine)
                                    {
                                        if (inIdx - outIdx >= 1 && inIdx - outIdx <= 10)
                                        {
                                            if (!TwoPointsInside(a1, outIdx, inIdx, j))
                                            {
                                                borderList.RemoveAt(inIdx);
                                                borderList.Insert(outIdx, aBP);
                                            }
                                        }
                                        else if (inIdx - outIdx <= -1 && inIdx - outIdx >= -10)
                                        {
                                            if (!TwoPointsInside(a1, outIdx, inIdx, j))
                                            {
                                                borderList.RemoveAt(inIdx);
                                                borderList.Insert(outIdx + 1, aBP);
                                            }
                                        }
                                        else if (inIdx == outIdx)
                                        {
                                            if (!TwoPointsInside(a1, outIdx, inIdx, j))
                                            {
                                                borderList.RemoveAt(inIdx);
                                                borderList.Insert(inIdx + 1, aBP);
                                            }
                                        }
                                    }
                                    IPoint = GetCrossPoint(lineA, lineB);
                                    aBP = new BorderPoint();
                                    aBP.Id = newPolylines.Count;
                                    aBP.Point = IPoint;
                                    borderList.Insert(j, aBP);
                                    outIdx = j;
                                    a1 = inIdx;                                    

                                    newLine = false;
                                    break;                                    
                                }
                                q1 = q2;
                            }
                            newPlist.Add(IPoint);

                            bLine = new PolyLine();
                            bLine.Value = inPolygon.OutLine.Value;
                            bLine.Type = inPolygon.OutLine.Type;
                            bLine.PointList = newPlist;
                            newPolylines.Add(bLine);

                            isInPolygon = false;
                            newPlist = new List<PointD>();
                        }
                    }
                    p1 = p2;
                }
            }

            if (newPolylines.Count > 0)
            {                
                //Tracing polygons
                newPolygons = TracingClipPolygons(inPolygon, newPolylines, borderList);
            }
            else
            {
                if (PointInPolygon(aPList, clipPList[0]))
                {
                    Extent aBound = new Extent();
                    Polygon aPolygon = new Polygon();
                    aPolygon.IsBorder = true;
                    aPolygon.LowValue = inPolygon.LowValue;
                    aPolygon.HighValue = inPolygon.HighValue;
                    aPolygon.Area = GetExtentAndArea(clipPList, ref aBound);
                    aPolygon.IsClockWise = true;
                    //aPolygon.StartPointIdx = lineBorderList.Count - 1;
                    aPolygon.Extent = aBound;
                    aPolygon.OutLine.PointList = clipPList;
                    aPolygon.OutLine.Value = inPolygon.LowValue;
                    aPolygon.IsHighCenter = inPolygon.IsHighCenter;
                    aPolygon.OutLine.Type = "Border";
                    aPolygon.HoleLines = new List<PolyLine>();

                    newPolygons.Add(aPolygon);
                }
            }

            if (holeLines.Count > 0)
            {
                AddHoles_Ring(ref newPolygons, holeLines);
            }

            return newPolygons;
        }

        private static List<Polygon> CutPolygon(Polygon inPolygon, List<PointD> clipPList)
        {
            List<Polygon> newPolygons = new List<Polygon>();
            List<PolyLine> newPolylines = new List<PolyLine>();
            List<PointD> aPList = inPolygon.OutLine.PointList;
            Extent plExtent = GetExtent(aPList);
            Extent cutExtent = GetExtent(clipPList);

            if (!IsExtentCross(plExtent, cutExtent))
                return newPolygons;

            int i, j;

            if (!IsClockwise(clipPList))    //---- Make cut polygon clockwise
                clipPList.Reverse();

            //Judge if all points of the polyline are in the cut polygon            
            if (PointInPolygon(clipPList, aPList[0]))
            {
                bool isAllIn = true;
                int notInIdx = 0;
                for (i = 0; i < aPList.Count; i++)
                {
                    if (!PointInPolygon(clipPList, aPList[i]))
                    {
                        notInIdx = i;
                        isAllIn = false;
                        break;
                    }
                }
                if (!isAllIn)   //Put start point outside of the cut polygon
                {
                    List<PointD> bPList = new List<PointD>();
                    bPList.AddRange(aPList.GetRange(notInIdx, aPList.Count - notInIdx));
                    bPList.AddRange(aPList.GetRange(1, notInIdx - 1));
                    //for (i = notInIdx; i < aPList.Count; i++)
                    //    bPList.Add(aPList[i]);

                    //for (i = 1; i < notInIdx; i++)
                    //    bPList.Add(aPList[i]);

                    bPList.Add(bPList[0]);
                    aPList = new List<PointD>(bPList);                    
                }
                else    //the input polygon is inside the cut polygon
                {
                    newPolygons.Add(inPolygon);
                    return newPolygons;                  
                }
            }

            //Prepare border point list
            List<BorderPoint> borderList = new List<BorderPoint>();
            BorderPoint aBP = new BorderPoint();
            foreach (PointD aP in clipPList)
            {
                aBP = new BorderPoint();
                aBP.Point = aP;
                aBP.Id = -1;
                borderList.Add(aBP);
            }

            //Cutting            
            bool isInPolygon = false;
            PointD q1, q2, p1, p2, IPoint;
            Line lineA, lineB;
            List<PointD> newPlist = new List<PointD>();
            PolyLine bLine;            
            p1 = aPList[0];
            int inIdx = -1, outIdx = -1;
            int a1 = 0;
            bool isNewLine = true;
            for (i = 1; i < aPList.Count; i++)
            {
                p2 = (PointD)aPList[i];
                if (PointInPolygon(clipPList, p2))
                {
                    if (!isInPolygon)
                    {
                        lineA = new Line();
                        lineA.P1 = p1;
                        lineA.P2 = p2;
                        q1 = borderList[borderList.Count - 1].Point;
                        IPoint = new PointD();
                        for (j = 0; j < borderList.Count; j++)
                        {
                            q2 = borderList[j].Point;
                            lineB = new Line();
                            lineB.P1 = q1;
                            lineB.P2 = q2;
                            if (IsLineSegmentCross(lineA, lineB))
                            {                                
                                IPoint = GetCrossPoint(lineA, lineB);
                                aBP = new BorderPoint();
                                aBP.Id = newPolylines.Count;
                                aBP.Point = IPoint;
                                borderList.Insert(j, aBP);
                                inIdx = j;               
                                break;
                            }
                            q1 = q2;
                        }
                        newPlist.Add(IPoint);                        
                    }
                    newPlist.Add(aPList[i]);                    
                    isInPolygon = true;
                }
                else
                {
                    if (isInPolygon)
                    {
                        lineA = new Line();
                        lineA.P1 = p1;
                        lineA.P2 = p2;
                        q1 = borderList[borderList.Count - 1].Point;
                        IPoint = new PointD();
                        for (j = 0; j < borderList.Count; j++)
                        {
                            q2 = borderList[j].Point;
                            lineB = new Line();
                            lineB.P1 = q1;
                            lineB.P2 = q2;
                            if (IsLineSegmentCross(lineA, lineB))
                            {                                
                                if (!isNewLine)
                                {
                                    if (inIdx - outIdx >= 1 && inIdx - outIdx <= 10)
                                    {
                                        if (!TwoPointsInside(a1, outIdx, inIdx, j))
                                        {
                                            borderList.RemoveAt(inIdx);
                                            borderList.Insert(outIdx, aBP);
                                        }
                                    }
                                    else if (inIdx - outIdx <= -1 && inIdx - outIdx >= -10)
                                    {
                                        if (!TwoPointsInside(a1, outIdx, inIdx, j))
                                        {
                                            borderList.RemoveAt(inIdx);
                                            borderList.Insert(outIdx + 1, aBP);
                                        }
                                    }
                                    else if (inIdx == outIdx)
                                    {
                                        if (!TwoPointsInside(a1, outIdx, inIdx, j))
                                        {
                                            borderList.RemoveAt(inIdx);
                                            borderList.Insert(inIdx + 1, aBP);
                                        }
                                    }
                                }
                                IPoint = GetCrossPoint(lineA, lineB);
                                aBP = new BorderPoint();
                                aBP.Id = newPolylines.Count;
                                aBP.Point = IPoint;
                                borderList.Insert(j, aBP);
                                outIdx = j;
                                a1 = inIdx;
                                isNewLine = false;
                                break;
                            }
                            q1 = q2;
                        }
                        newPlist.Add(IPoint);

                        bLine = new PolyLine();
                        bLine.Value = inPolygon.OutLine.Value;
                        bLine.Type = inPolygon.OutLine.Type;
                        bLine.PointList = newPlist;
                        newPolylines.Add(bLine);                        

                        isInPolygon = false;
                        newPlist = new List<PointD>();                        
                    }
                }
                p1 = p2;
            }
            
            if (newPolylines.Count > 0)
            {                                                              
                //Tracing polygons
                newPolygons = TracingClipPolygons(inPolygon, newPolylines, borderList);
            }
            else
            {
                if (PointInPolygon(aPList, clipPList[0]))
                {
                    Extent aBound = new Extent();
                    Polygon aPolygon = new Polygon();
                    aPolygon.IsBorder = true;
                    aPolygon.LowValue = inPolygon.LowValue;
                    aPolygon.HighValue = inPolygon.HighValue;
                    aPolygon.Area = GetExtentAndArea(clipPList, ref aBound);
                    aPolygon.IsClockWise = true;
                    //aPolygon.StartPointIdx = lineBorderList.Count - 1;
                    aPolygon.Extent = aBound;
                    aPolygon.OutLine.PointList = clipPList;
                    aPolygon.OutLine.Value = inPolygon.LowValue;
                    aPolygon.IsHighCenter = inPolygon.IsHighCenter;
                    aPolygon.OutLine.Type = "Border";
                    aPolygon.HoleLines = new List<PolyLine>();

                    newPolygons.Add(aPolygon);
                }
            }

            return newPolygons;
        }

        //private static List<Polygon> CutPolygon_Old1(Polygon inPolygon, List<PointD> clipPList)
        //{
        //    List<Polygon> newPolygons = new List<Polygon>();
        //    List<PolyLine> newPolylines = new List<PolyLine>();
        //    List<PointD> aPList = inPolygon.OutLine.PointList;
        //    Extent plExtent = GetExtent(aPList);
        //    Extent cutExtent = GetExtent(clipPList);

        //    if (!IsExtentCross(plExtent, cutExtent))
        //        return newPolygons;

        //    int i, j;

        //    if (!IsClockwise(clipPList))    //---- Make cut polygon clockwise
        //        clipPList.Reverse();

        //    //Judge if all points of the polyline are in the cut polygon            
        //    if (PointInPolygon(clipPList, aPList[0]))
        //    {
        //        bool isAllIn = true;
        //        int notInIdx = 0;
        //        for (i = 0; i < aPList.Count; i++)
        //        {
        //            if (!PointInPolygon(clipPList, aPList[i]))
        //            {
        //                notInIdx = i;
        //                isAllIn = false;
        //                break;
        //            }
        //        }
        //        if (!isAllIn)   //Put start point outside of the cut polygon
        //        {
        //            List<PointD> bPList = new List<PointD>();
        //            bPList.AddRange(aPList.GetRange(notInIdx, aPList.Count - notInIdx));
        //            bPList.AddRange(aPList.GetRange(1, notInIdx - 1));
        //            //for (i = notInIdx; i < aPList.Count; i++)
        //            //    bPList.Add(aPList[i]);

        //            //for (i = 1; i < notInIdx; i++)
        //            //    bPList.Add(aPList[i]);

        //            bPList.Add(bPList[0]);
        //            aPList = new List<PointD>(bPList);
        //        }
        //        else    //the input polygon is inside the cut polygon
        //        {
        //            newPolygons.Add(inPolygon);
        //            return newPolygons;
        //        }
        //    }

        //    //Prepare border point list
        //    List<BorderPoint> borderList = new List<BorderPoint>();
        //    BorderPoint aBP = new BorderPoint();
        //    foreach (PointD aP in clipPList)
        //    {
        //        aBP = new BorderPoint();
        //        aBP.Point = aP;
        //        aBP.Id = -1;
        //        borderList.Add(aBP);
        //    }

        //    //Cutting            
        //    bool isInPolygon = false;
        //    PointD q1, q2, p1, p2, IPoint = new PointD();
        //    Line lineA, lineB;
        //    List<PointD> newPlist = new List<PointD>();
        //    PolyLine bLine = new PolyLine();
        //    p1 = aPList[0];
        //    List<BorderPoint> borderPList = new List<BorderPoint>();
        //    for (i = 1; i < aPList.Count; i++)
        //    {
        //        p2 = (PointD)aPList[i];
        //        if (PointInPolygon(clipPList, p2))
        //        {
        //            if (!isInPolygon)
        //            {
        //                lineA.P1 = p1;
        //                lineA.P2 = p2;
        //                q1 = (PointD)clipPList[clipPList.Count - 1];
        //                for (j = 0; j < clipPList.Count; j++)
        //                {
        //                    q2 = clipPList[j];
        //                    lineB.P1 = q1;
        //                    lineB.P2 = q2;
        //                    if (IsLineSegmentCross(lineA, lineB))
        //                    {
        //                        IPoint = GetCrossPoint(lineA, lineB);
        //                        aBP = new BorderPoint();
        //                        aBP.Id = newPolylines.Count;
        //                        aBP.Point = IPoint;
        //                        borderPList.Add(aBP);
        //                        break;
        //                    }
        //                    q1 = q2;
        //                }
        //                newPlist.Add(IPoint);
        //            }
        //            newPlist.Add(aPList[i]);
        //            isInPolygon = true;
        //        }
        //        else
        //        {
        //            if (isInPolygon)
        //            {
        //                lineA.P1 = p1;
        //                lineA.P2 = p2;
        //                q1 = (PointD)clipPList[clipPList.Count - 1];
        //                for (j = 0; j < clipPList.Count; j++)
        //                {
        //                    q2 = clipPList[j];
        //                    lineB.P1 = q1;
        //                    lineB.P2 = q2;
        //                    if (IsLineSegmentCross(lineA, lineB))
        //                    {
        //                        IPoint = GetCrossPoint(lineA, lineB);
        //                        aBP = new BorderPoint();
        //                        aBP.Id = newPolylines.Count;
        //                        aBP.Point = IPoint;
        //                        borderPList.Add(aBP);
        //                        break;
        //                    }
        //                    q1 = q2;
        //                }
        //                newPlist.Add(IPoint);

        //                bLine = new PolyLine();
        //                bLine.Value = inPolygon.OutLine.Value;
        //                bLine.Type = inPolygon.OutLine.Type;
        //                bLine.PointList = new List<PointD>(newPlist);
        //                newPolylines.Add(bLine);

        //                isInPolygon = false;
        //                newPlist = new List<PointD>();
        //            }
        //        }
        //        p1 = p2;
        //    }

        //    if (newPolylines.Count > 0)
        //    {
        //        borderList = InsertPoint2Border(borderPList, borderList);

        //        //Tracing polygons
        //        newPolygons = TracingClipPolygons(inPolygon, newPolylines, borderList);
        //    }
        //    else
        //    {
        //        if (PointInPolygon(aPList, clipPList[0]))
        //        {
        //            Extent aBound = new Extent();
        //            Polygon aPolygon = new Polygon();
        //            aPolygon.IsBorder = true;
        //            aPolygon.LowValue = inPolygon.LowValue;
        //            aPolygon.HighValue = inPolygon.HighValue;
        //            aPolygon.Area = GetExtentAndArea(clipPList, ref aBound);
        //            aPolygon.IsClockWise = true;
        //            //aPolygon.StartPointIdx = lineBorderList.Count - 1;
        //            aPolygon.Extent = aBound;
        //            aPolygon.OutLine.PointList = new List<PointD>(clipPList);
        //            aPolygon.OutLine.Value = inPolygon.LowValue;
        //            aPolygon.IsHighCenter = inPolygon.IsHighCenter;
        //            aPolygon.OutLine.Type = "Border";
        //            aPolygon.HoleLines = new List<PolyLine>();

        //            newPolygons.Add(aPolygon);
        //        }
        //    }

        //    return newPolygons;
        //}

        //private static List<Polygon> CutPolygon_Old(Polygon inPolygon, List<PointD> clipPList)
        //{
        //    List<Polygon> newPolygons = new List<Polygon>();
        //    List<PointD> aPList = inPolygon.OutLine.PointList;
        //    Extent cutExtent = GetExtent(clipPList);

        //    if (!IsExtentCross(inPolygon.Extent, cutExtent))
        //        return newPolygons;

        //    int i, j;

        //    if (!IsClockwise(clipPList))    //---- Make cut polygon clockwise
        //        clipPList.Reverse();

        //    //Put start point outside of the cut polygon if .
        //    if (PointInPolygon(clipPList, aPList[0]))
        //    {
        //        bool isAllIn = true;
        //        int notInIdx = 0;
        //        for (i = 0; i < aPList.Count; i++)
        //        {
        //            if (!PointInPolygon(clipPList, aPList[i]))
        //            {
        //                notInIdx = i;
        //                isAllIn = false;
        //                break;
        //            }
        //        }
        //        if (!isAllIn)
        //        {
        //            List<PointD> bPList = new List<PointD>();
        //            bPList.AddRange(aPList.GetRange(notInIdx, aPList.Count - notInIdx));
        //            bPList.AddRange(aPList.GetRange(1, notInIdx - 1));
        //            //for (i = notInIdx; i < aPList.Count; i++)
        //            //    bPList.Add(aPList[i]);

        //            //for (i = 1; i < notInIdx; i++)
        //            //    bPList.Add(aPList[i]);

        //            bPList.Add(bPList[0]);
        //            aPList = new List<PointD>(bPList);
        //        }
        //        else    //the input polygon is inside the cut polygon
        //        {
        //            newPolygons.Add(inPolygon);
        //            return newPolygons;
        //        }
        //    }

        //    //Cutting            
        //    bool isInPolygon = false;
        //    PointD q1, q2, p1, p2, IPoint = new PointD();
        //    Line lineA, lineB;
        //    List<PointD> newPlist = new List<PointD>();
        //    //PolyLine aLine = new PolyLine(), bLine = new PolyLine();
        //    Polygon outPolygon;
        //    p1 = aPList[0];
        //    int crossNum = 0;
        //    int outIdx = 0, inIdx = 0;
        //    bool isClockwise = true;
        //    for (i = 1; i < aPList.Count; i++)
        //    {
        //        p2 = (PointD)aPList[i];
        //        if (PointInPolygon(clipPList, p2))
        //        {
        //            if (!isInPolygon)
        //            {
        //                lineA.P1 = p1;
        //                lineA.P2 = p2;
        //                q1 = (PointD)clipPList[clipPList.Count - 1];
        //                for (j = 0; j < clipPList.Count; j++)
        //                {
        //                    q2 = (PointD)clipPList[j];
        //                    lineB.P1 = q1;
        //                    lineB.P2 = q2;
        //                    if (IsLineSegmentCross(lineA, lineB))
        //                    {
        //                        IPoint = GetCrossPoint(lineA, lineB);
        //                        crossNum += 1;
        //                        inIdx = j;
        //                        break;
        //                    }
        //                    q1 = q2;
        //                }                        

        //                if (crossNum == 2 && Math.Abs(inIdx - outIdx) > 1)    //Cross out then cross int, add part of the cut points.
        //                {
        //                    if (isClockwise)
        //                    {
        //                        inIdx = j - 1;
        //                        if (inIdx > outIdx)
        //                        {
        //                            newPlist.AddRange(clipPList.GetRange(outIdx, inIdx - outIdx));
        //                        }
        //                        else
        //                        {
        //                            newPlist.AddRange(clipPList.GetRange(outIdx, clipPList.Count - outIdx));
        //                            newPlist.AddRange(clipPList.GetRange(1, inIdx - 1));
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if (inIdx < outIdx)
        //                        {
        //                            newPlist.AddRange(clipPList.GetRange(inIdx, outIdx - inIdx));
        //                        }
        //                        else
        //                        {
        //                            newPlist.AddRange(clipPList.GetRange(inIdx, clipPList.Count - inIdx));
        //                            newPlist.AddRange(clipPList.GetRange(1, outIdx - 1));
        //                        }
        //                    }
        //                }

        //                newPlist.Add(IPoint);
        //                crossNum = 0;
        //            }
        //            newPlist.Add(aPList[i]);
        //            isInPolygon = true;
        //        }
        //        else
        //        {
        //            if (isInPolygon)
        //            {
        //                lineA.P1 = p1;
        //                lineA.P2 = p2;
        //                q1 = (PointD)clipPList[clipPList.Count - 1];
        //                for (j = 0; j < clipPList.Count; j++)
        //                {
        //                    q2 = (PointD)clipPList[j];
        //                    lineB.P1 = q1;
        //                    lineB.P2 = q2;
        //                    if (IsLineSegmentCross(lineA, lineB))
        //                    {
        //                        IPoint = GetCrossPoint(lineA, lineB);
        //                        crossNum += 1;
        //                        isClockwise = PointInPolygon(aPList, q2);
        //                        if (isClockwise)
        //                            outIdx = j;
        //                        else
        //                            outIdx = j - 1;
                                
        //                        break;
        //                    }
        //                    q1 = q2;
        //                }
        //                newPlist.Add(IPoint);

        //                outPolygon = new Polygon();
        //                outPolygon.LowValue = inPolygon.LowValue;
        //                outPolygon.HighValue = inPolygon.HighValue;
        //                outPolygon.IsClockWise = inPolygon.IsClockWise;
        //                outPolygon.IsHighCenter = inPolygon.IsHighCenter;
        //                outPolygon.StartPointIdx = inPolygon.StartPointIdx;
        //                outPolygon.Extent = new Extent();
        //                outPolygon.Area = GetExtentAndArea(newPlist, ref outPolygon.Extent);                        
        //                outPolygon.OutLine.Type = "Border";
        //                outPolygon.OutLine.Value = inPolygon.OutLine.Value;
        //                outPolygon.OutLine.BorderIdx = inPolygon.OutLine.BorderIdx;
        //                outPolygon.OutLine.PointList = new List<PointD>(newPlist);
        //                newPolygons.Add(outPolygon);
        //                isInPolygon = false;
        //                newPlist = new List<PointD>();
        //                //aType = "Border";
        //            }
        //        }
        //        p1 = p2;
        //    }

        //    if (isInPolygon && newPlist.Count > 1)
        //    {
        //        outPolygon = new Polygon();
        //        outPolygon.LowValue = inPolygon.LowValue;
        //        outPolygon.HighValue = inPolygon.HighValue;
        //        outPolygon.IsClockWise = inPolygon.IsClockWise;
        //        outPolygon.IsHighCenter = inPolygon.IsHighCenter;
        //        outPolygon.StartPointIdx = inPolygon.StartPointIdx;
        //        outPolygon.Extent = new Extent();
        //        outPolygon.Area = GetExtentAndArea(newPlist, ref outPolygon.Extent);
        //        outPolygon.OutLine.Type = "Border";
        //        outPolygon.OutLine.Value = inPolygon.OutLine.Value;
        //        outPolygon.OutLine.BorderIdx = inPolygon.OutLine.BorderIdx;
        //        outPolygon.OutLine.PointList = new List<PointD>(newPlist);
        //        newPolygons.Add(outPolygon);
        //    }

        //    return newPolygons;
        //}

        private static bool TwoPointsInside(int a1, int a2, int b1, int b2)
        {
            if (a2 < a1)
                a1 += 1;
            if (b1 < a1)
                a1 += 1;
            if (b1 < a2)
                a2 += 1;


            if (a2 < a1)
            {
                int c = a1;
                a1 = a2;
                a2 = c;
            }

            if (b1 > a1 && b1 <= a2)
            {
                if (b2 > a1 && b2 <= a2)
                    return true;
                else
                    return false;
            }
            else
            {
                if (!(b2 > a1 && b2 <= a2))
                    return true;
                else
                    return false;
            }
        }

        #endregion

        #region Smoothing Methods
        private static List<PointD> BSplineScanning(List<PointD> pointList)
        {
            bool isClose = false;
            int n = pointList.Count;
            if (DoubleEquals(pointList[0].X, pointList[n - 1].X) && DoubleEquals(pointList[0].Y, pointList[n - 1].Y))
                isClose = true;

            return BSplineScanning(pointList, isClose, 0.05f);
        }

        private static List<PointD> BSplineScanning(List<PointD> pointList, bool isClose)
        {
            return BSplineScanning(pointList, isClose, 0.05f);
        }

        /// <summary>
        /// B-Spline interpolation
        /// </summary>
        /// <param name="pointList">Point list</param>
        /// <param name="isClose">Is closed or not</param>
        /// <param name="step">Scan step (0 - 1)</param>
        /// <returns>Interpolated points</returns>
        private static List<PointD> BSplineScanning(List<PointD> pointList, bool isClose, float step)
        {
            Single t;
            int i;
            double X = 0, Y = 0;            
            PointD aPoint;
            List<PointD> newPList = new List<PointD>();
            int sum = pointList.Count;

            if (sum < 4)
            {
                return null;
            }

            aPoint = (PointD)pointList[0];
            PointD bPoint = (PointD)pointList[sum - 1];
            //if (aPoint.X == bPoint.X && aPoint.Y == bPoint.Y)
            if (isClose)
            {
                pointList.RemoveAt(0);
                pointList.Add(pointList[0]);
                pointList.Add(pointList[1]);
                pointList.Add(pointList[2]);
                pointList.Add(pointList[3]);
                pointList.Add(pointList[4]);
                pointList.Add(pointList[5]);
                pointList.Add(pointList[6]);
                //pointList.Add(pointList[7]);
                //pointList.Add(pointList[8]);
            }

            sum = pointList.Count;
            for (i = 0; i < sum - 3; i++)
            {
                for (t = 0; t <= 1; t += step)
                {
                    BSpline(pointList, t, i, ref X, ref Y);
                    if (isClose)
                    {
                        if (i > 3)
                        {
                            aPoint = new PointD();
                            aPoint.X = X;
                            aPoint.Y = Y;
                            newPList.Add(aPoint);
                        }
                    }
                    else
                    {
                        aPoint = new PointD();
                        aPoint.X = X;
                        aPoint.Y = Y;
                        newPList.Add(aPoint);
                    }
                }
            }

            if (isClose)
                newPList.Add(newPList[0]);
            else
            {
                newPList.Insert(0, pointList[0]);
                newPList.Add(pointList[pointList.Count - 1]);
            }

            return newPList;
        }

        private static void BSpline(List<PointD> pointList, double t, int i, ref double X, ref double Y)
        {
            double[] f = new double[4];
            fb(t, ref f);
            int j;
            X = 0;
            Y = 0;
            PointD aPoint;
            for (j = 0; j < 4; j++)
            {
                aPoint = pointList[i + j];
                X = X + f[j] * aPoint.X;
                Y = Y + f[j] * aPoint.Y;
            }
        }

        private static double f0(double t)
        {
            return 1.0 / 6 * (-t + 1) * (-t + 1) * (-t + 1);
        }

        private static double f1(double t)
        {
            return 1.0 / 6 * (3 * t * t * t - 6 * t * t + 4);
        }

        private static double f2(double t)
        {
            return 1.0 / 6 * (-3 * t * t * t + 3 * t * t + 3 * t + 1);
        }

        private static double f3(double t)
        {
            return 1.0 / 6 * t * t * t;
        }

        private static void fb(double t, ref double[] fs)
        {
            fs[0] = f0(t);
            fs[1] = f1(t);
            fs[2] = f2(t);
            fs[3] = f3(t);
        }

        #endregion

        #region Other Methods
        private static Extent GetExtent(List<PointD> pList)
        {
            double  minX, minY, maxX, maxY;
            int i;
            PointD aPoint;
            aPoint = pList[0];
            minX = aPoint.X;
            maxX = aPoint.X;
            minY = aPoint.Y;
            maxY = aPoint.Y;
            for (i = 1; i < pList.Count; i++)
            {
                aPoint = pList[i];
                if (aPoint.X < minX)
                    minX = aPoint.X;

                if (aPoint.X > maxX)
                    maxX = aPoint.X;

                if (aPoint.Y < minY)
                    minY = aPoint.Y;

                if (aPoint.Y > maxY)
                    maxY = aPoint.Y;
            }

            Extent aExtent = new Extent();
            aExtent.xMin = minX;
            aExtent.yMin = minY;
            aExtent.xMax = maxX;
            aExtent.yMax = maxY;            

            return aExtent;
        }

        private static double GetExtentAndArea(List<PointD> pList, ref Extent aExtent)
        {
            double bArea, minX, minY, maxX, maxY;
            int i;
            PointD aPoint;
            aPoint = pList[0];
            minX = aPoint.X;
            maxX = aPoint.X;
            minY = aPoint.Y;
            maxY = aPoint.Y;
            for (i = 1; i < pList.Count; i++)
            {
                aPoint = pList[i];
                if (aPoint.X < minX)
                    minX = aPoint.X;

                if (aPoint.X > maxX)
                    maxX = aPoint.X;

                if (aPoint.Y < minY)
                    minY = aPoint.Y;

                if (aPoint.Y > maxY)
                    maxY = aPoint.Y;
            }

            aExtent.xMin = minX;
            aExtent.yMin = minY;
            aExtent.xMax = maxX;
            aExtent.yMax = maxY;
            bArea = (maxX - minX) * (maxY - minY);

            return bArea;
        }

        /// <summary>
        /// Determin if the point list is clockwise
        /// </summary>
        /// <param name="pointList">point list</param>
        /// <returns>clockwise or not</returns>
        public static bool IsClockwise(List<PointD> pointList)
        {
            int i;
            PointD aPoint;
            double yMax = 0;
            int yMaxIdx = 0;
            for (i = 0; i < pointList.Count - 1; i++)
            {
                aPoint = pointList[i];
                if (i == 0)
                {
                    yMax = aPoint.Y;
                    yMaxIdx = 0;
                }
                else
                {
                    if (yMax < aPoint.Y)
                    {
                        yMax = aPoint.Y;
                        yMaxIdx = i;
                    }
                }
            }
            PointD p1, p2, p3;
            int p1Idx, p2Idx, p3Idx;
            p1Idx = yMaxIdx - 1;
            p2Idx = yMaxIdx;
            p3Idx = yMaxIdx + 1;
            if (yMaxIdx == 0)
                p1Idx = pointList.Count - 2;

            p1 = pointList[p1Idx];
            p2 = pointList[p2Idx];
            p3 = pointList[p3Idx];
            if ((p3.X - p1.X) * (p2.Y - p1.Y) - (p2.X - p1.X) * (p3.Y - p1.Y) > 0)
                return true;
            else
                return false;

        }        

        private static bool IsLineSegmentCross(Line lineA, Line lineB)
        {
            Extent boundA = new Extent(), boundB = new Extent();
            List<PointD> PListA = new List<PointD>(), PListB = new List<PointD>();
            PListA.Add(lineA.P1);
            PListA.Add(lineA.P2);
            PListB.Add(lineB.P1);
            PListB.Add(lineB.P2);
            GetExtentAndArea(PListA, ref boundA);
            GetExtentAndArea(PListB, ref boundB);

            if (!IsExtentCross(boundA, boundB))
                return false;
            else
            {
                double XP1 = (lineB.P1.X - lineA.P1.X) * (lineA.P2.Y - lineA.P1.Y) -
                  (lineA.P2.X - lineA.P1.X) * (lineB.P1.Y - lineA.P1.Y);
                double XP2 = (lineB.P2.X - lineA.P1.X) * (lineA.P2.Y - lineA.P1.Y) -
                  (lineA.P2.X - lineA.P1.X) * (lineB.P2.Y - lineA.P1.Y);
                if (XP1 * XP2 > 0)
                    return false;
                else
                    return true;
            }
        }

        private static bool IsExtentCross(Extent aBound, Extent bBound)
        {
            if (aBound.xMin > bBound.xMax || aBound.xMax < bBound.xMin || aBound.yMin > bBound.yMax ||
              aBound.yMax < bBound.yMin)
                return false;
            else
                return true;

        }

        /// <summary>
        /// Get cross point of two line segments
        /// </summary>
        /// <param name="aP1">point 1 of line a</param>
        /// <param name="aP2">point 2 of line a</param>
        /// <param name="bP1">point 1 of line b</param>
        /// <param name="bP2">point 2 of line b</param>
        /// <returns>cross point</returns>
        public static PointF GetCrossPoint(PointF aP1, PointF aP2, PointF bP1, PointF bP2)
        {
            PointF IPoint = new PointF(0, 0);
            PointF p1, p2, q1, q2;
            double tempLeft, tempRight;

            double XP1 = (bP1.X - aP1.X) * (aP2.Y - aP1.Y) -
                  (aP2.X - aP1.X) * (bP1.Y - aP1.Y);
            double XP2 = (bP2.X - aP1.X) * (aP2.Y - aP1.Y) -
              (aP2.X - aP1.X) * (bP2.Y - aP1.Y);
            if (XP1 == 0)
                IPoint = bP1;
            else if (XP2 == 0)
                IPoint = bP2;
            else
            {
                p1 = aP1;
                p2 = aP2;
                q1 = bP1;
                q2 = bP2;

                tempLeft = (q2.X - q1.X) * (p1.Y - p2.Y) - (p2.X - p1.X) * (q1.Y - q2.Y);
                tempRight = (p1.Y - q1.Y) * (p2.X - p1.X) * (q2.X - q1.X) + q1.X * (q2.Y - q1.Y) * (p2.X - p1.X) - p1.X * (p2.Y - p1.Y) * (q2.X - q1.X);
                IPoint.X = (float)(tempRight / tempLeft);

                tempLeft = (p1.X - p2.X) * (q2.Y - q1.Y) - (p2.Y - p1.Y) * (q1.X - q2.X);
                tempRight = p2.Y * (p1.X - p2.X) * (q2.Y - q1.Y) + (q2.X - p2.X) * (q2.Y - q1.Y) * (p1.Y - p2.Y) - q2.Y * (q1.X - q2.X) * (p2.Y - p1.Y);
                IPoint.Y = (float)(tempRight / tempLeft);
            }

            return IPoint;
        }

        private static PointD GetCrossPoint(Line lineA, Line lineB)
        {
            PointD IPoint = new PointD();
            PointD p1, p2, q1, q2;
            double tempLeft, tempRight;

            double XP1 = (lineB.P1.X - lineA.P1.X) * (lineA.P2.Y - lineA.P1.Y) -
                  (lineA.P2.X - lineA.P1.X) * (lineB.P1.Y - lineA.P1.Y);
            double XP2 = (lineB.P2.X - lineA.P1.X) * (lineA.P2.Y - lineA.P1.Y) -
              (lineA.P2.X - lineA.P1.X) * (lineB.P2.Y - lineA.P1.Y);
            if (XP1 == 0)
                IPoint = lineB.P1;
            else if (XP2 == 0)
                IPoint = lineB.P2;
            else
            {
                p1 = lineA.P1;
                p2 = lineA.P2;
                q1 = lineB.P1;
                q2 = lineB.P2;

                tempLeft = (q2.X - q1.X) * (p1.Y - p2.Y) - (p2.X - p1.X) * (q1.Y - q2.Y);
                tempRight = (p1.Y - q1.Y) * (p2.X - p1.X) * (q2.X - q1.X) + q1.X * (q2.Y - q1.Y) * (p2.X - p1.X) - p1.X * (p2.Y - p1.Y) * (q2.X - q1.X);
                IPoint.X = tempRight / tempLeft;

                tempLeft = (p1.X - p2.X) * (q2.Y - q1.Y) - (p2.Y - p1.Y) * (q1.X - q2.X);
                tempRight = p2.Y * (p1.X - p2.X) * (q2.Y - q1.Y) + (q2.X - p2.X) * (q2.Y - q1.Y) * (p1.Y - p2.Y) - q2.Y * (q1.X - q2.X) * (p2.Y - p1.Y);
                IPoint.Y = tempRight / tempLeft;
            }

            return IPoint;
        }

        private static List<BorderPoint> InsertPoint2Border(List<BorderPoint> bPList, List<BorderPoint> aBorderList)
        {
            BorderPoint aBPoint, bP;
            int i, j;
            PointD p1, p2, p3;
            //ArrayList aEPList = new ArrayList(), temEPList = new ArrayList(), dList = new ArrayList();
            List<BorderPoint> BorderList = new List<BorderPoint>(aBorderList);

            for (i = 0; i < bPList.Count; i++)
            {
                bP = bPList[i];
                p3 = bP.Point;
                aBPoint = BorderList[0];
                p1 = aBPoint.Point;
                for (j = 1; j < BorderList.Count; j++)
                {
                    aBPoint = BorderList[j];
                    p2 = aBPoint.Point;                    
                    if ((p3.X - p1.X) * (p3.X - p2.X) <= 0)
                    {                       
                        if ((p3.Y - p1.Y) * (p3.Y - p2.Y) <= 0)
                        {
                            if ((p3.X - p1.X) * (p2.Y - p1.Y) - (p2.X - p1.X) * (p3.Y - p1.Y) <= 0.001)
                            {
                                BorderList.Insert(j, bP);
                                break;
                            }
                        }
                    }
                    p1 = p2;
                }
            }

            return BorderList;
        }

        private static List<BorderPoint> InsertPoint2Border_old(List<BorderPoint> bPList, List<BorderPoint> aBorderList)
        {
            BorderPoint aBPoint, bP;
            int i, j;
            PointD p1, p2, p3;
            //ArrayList aEPList = new ArrayList(), temEPList = new ArrayList(), dList = new ArrayList();
            List<BorderPoint> BorderList = new List<BorderPoint>(aBorderList);

            for (i = 0; i < bPList.Count; i++)
            {
                bP = bPList[i];
                p3 = bP.Point;
                aBPoint = BorderList[0];
                p1 = aBPoint.Point;
                for (j = 1; j < BorderList.Count; j++)
                {
                    aBPoint = BorderList[j];
                    p2 = aBPoint.Point;
                    if ((DoubleEquals(p3.X, p1.X) && DoubleEquals(p3.Y, p1.Y)) ||
                        (DoubleEquals(p3.X, p2.X) && DoubleEquals(p3.Y, p2.Y)))
                    {
                        BorderList.Insert(j, bP);
                        break;
                    }
                    else
                    {
                        if ((p3.X - p1.X) * (p3.X - p2.X) <= 0 || (DoubleEquals(p3.X, p1.X) || DoubleEquals(p3.X, p2.X)))
                        //if ((p3.X - p1.X) * (p3.X - p2.X) <= 0)
                        {
                            if ((p3.Y - p1.Y) * (p3.Y - p2.Y) <= 0 || (DoubleEquals(p3.Y, p1.Y) || DoubleEquals(p3.Y, p2.Y)))
                            //if ((p3.Y - p1.Y) * (p3.Y - p2.Y) <= 0)
                            {
                                if ((p3.X - p1.X) * (p2.Y - p1.Y) - (p2.X - p1.X) * (p3.Y - p1.Y) <= 0.001)
                                {
                                    BorderList.Insert(j, bP);
                                    break;
                                }
                            }
                        }
                    }
                    p1 = p2;
                }
            }

            return BorderList;
        }

        private static List<BorderPoint> InsertPoint2RectangleBorder(List<PolyLine> LineList, Extent aBound)
        {
            BorderPoint bPoint, bP;
            PolyLine aLine;
            PointD aPoint;
            int i, j, k;
            List<BorderPoint> LBPList = new List<BorderPoint>(), TBPList = new List<BorderPoint>();
            List<BorderPoint> RBPList = new List<BorderPoint>(), BBPList = new List<BorderPoint>();
            List<BorderPoint> BorderList = new List<BorderPoint>();
            List<PointD> aPointList = new List<PointD>();
            bool IsInserted;

            //---- Get four border point list
            for (i = 0; i < LineList.Count; i++)
            {
                aLine = LineList[i];
                if (aLine.Type != "Close")
                {
                    aPointList = new List<PointD>(aLine.PointList);
                    bP = new BorderPoint();
                    bP.Id = i;
                    for (k = 0; k <= 1; k++)
                    {
                        if (k == 0)
                            aPoint = aPointList[0];
                        else
                            aPoint = aPointList[aPointList.Count - 1];

                        bP.Point = aPoint;
                        IsInserted = false;
                        if (aPoint.X == aBound.xMin)
                        {
                            for (j = 0; j < LBPList.Count; j++)
                            {
                                bPoint = LBPList[j];
                                if (aPoint.Y < bPoint.Point.Y)
                                {
                                    LBPList.Insert(j, bP);
                                    IsInserted = true;
                                    break;
                                }
                            }
                            if (!IsInserted)
                                LBPList.Add(bP);

                        }
                        else if (aPoint.X == aBound.xMax)
                        {
                            for (j = 0; j < RBPList.Count; j++)
                            {
                                bPoint = RBPList[j];
                                if (aPoint.Y > bPoint.Point.Y)
                                {
                                    RBPList.Insert(j, bP);
                                    IsInserted = true;
                                    break;
                                }
                            }
                            if (!IsInserted)
                                RBPList.Add(bP);

                        }
                        else if (aPoint.Y == aBound.yMin)
                        {
                            for (j = 0; j < BBPList.Count; j++)
                            {
                                bPoint = BBPList[j];
                                if (aPoint.X > bPoint.Point.X)
                                {
                                    BBPList.Insert(j, bP);
                                    IsInserted = true;
                                    break;
                                }
                            }
                            if (!IsInserted)
                                BBPList.Add(bP);

                        }
                        else if (aPoint.Y == aBound.yMax)
                        {
                            for (j = 0; j < TBPList.Count; j++)
                            {
                                bPoint = TBPList[j];
                                if (aPoint.X < bPoint.Point.X)
                                {
                                    TBPList.Insert(j, bP);
                                    IsInserted = true;
                                    break;
                                }
                            }
                            if (!IsInserted)
                                TBPList.Add(bP);

                        }
                    }
                }
            }

            //---- Get border list
            bP = new BorderPoint();
            bP.Id = -1;

            aPoint = new PointD();
            aPoint.X = aBound.xMin;
            aPoint.Y = aBound.yMin;
            bP.Point = aPoint;
            BorderList.Add(bP);

            BorderList.AddRange(LBPList);

            bP = new BorderPoint();
            bP.Id = -1;
            aPoint = new PointD();
            aPoint.X = aBound.xMin;
            aPoint.Y = aBound.yMax;
            bP.Point = aPoint;
            BorderList.Add(bP);

            BorderList.AddRange(TBPList);

            bP = new BorderPoint();
            bP.Id = -1;
            aPoint = new PointD();
            aPoint.X = aBound.xMax;
            aPoint.Y = aBound.yMax;
            bP.Point = aPoint;
            BorderList.Add(bP);

            BorderList.AddRange(RBPList);

            bP = new BorderPoint();
            bP.Id = -1;
            aPoint = new PointD();
            aPoint.X = aBound.xMax;
            aPoint.Y = aBound.yMin;
            bP.Point = aPoint;
            BorderList.Add(bP);

            BorderList.AddRange(BBPList);

            BorderList.Add(BorderList[0]);

            return BorderList;
        }

        private static List<BorderPoint> InsertEndPoint2Border(List<EndPoint> EPList, List<BorderPoint> aBorderList)
        {
            BorderPoint aBPoint, bP;
            int i, j, k;
            PointD p1, p2;
            List<EndPoint> aEPList = new List<EndPoint>();
            List<EndPoint> temEPList = new List<EndPoint>();
            ArrayList dList = new ArrayList();
            EndPoint aEP;
            double dist;
            bool IsInsert;
            List<BorderPoint> BorderList = new List<BorderPoint>();

            aEPList = new List<EndPoint>(EPList);

            aBPoint = aBorderList[0];
            p1 = aBPoint.Point;
            BorderList.Add(aBPoint);
            for (i = 1; i < aBorderList.Count; i++)
            {
                aBPoint = aBorderList[i];
                p2 = aBPoint.Point;
                temEPList.Clear();
                for (j = 0; j < aEPList.Count; j++)
                {
                    if (j == aEPList.Count)
                        break;

                    aEP = aEPList[j];
                    if (Math.Abs(aEP.sPoint.X - p1.X) < 0.000001 && Math.Abs(aEP.sPoint.Y - p1.Y) < 0.000001)
                    {
                        temEPList.Add(aEP);
                        aEPList.RemoveAt(j);
                        j -= 1;
                    }
                }
                if (temEPList.Count > 0)
                {
                    dList.Clear();
                    if (temEPList.Count > 1)
                    {
                        for (j = 0; j < temEPList.Count; j++)
                        {
                            aEP = temEPList[j];
                            dist = Math.Pow(aEP.Point.X - p1.X, 2) + Math.Pow(aEP.Point.Y - p1.Y, 2);
                            if (j == 0)
                                dList.Add(new object[] { dist, j });
                            else
                            {
                                IsInsert = false;
                                for (k = 0; k < dList.Count; k++)
                                {
                                    if (dist < (double)((object[])dList[k])[0])
                                    {
                                        dList.Insert(k, new object[] { dist, j });
                                        IsInsert = true;
                                        break;
                                    }
                                }
                                if (!IsInsert)
                                    dList.Add(new object[] { dist, j });

                            }
                        }
                        for (j = 0; j < dList.Count; j++)
                        {
                            aEP = temEPList[(int)((object[])dList[j])[1]];
                            bP = new BorderPoint();
                            bP.Id = aEP.Index;
                            bP.Point = aEP.Point;
                            BorderList.Add(bP);
                        }
                    }
                    else
                    {
                        aEP = temEPList[0];
                        bP = new BorderPoint();
                        bP.Id = aEP.Index;
                        bP.Point = aEP.Point;
                        BorderList.Add(bP);
                    }
                }

                BorderList.Add(aBPoint);

                p1 = p2;
            }

            return BorderList;
        }

        private static List<BorderPoint> InsertPoint2Border_Ring(double[,] S0, List<BorderPoint> bPList, Border aBorder, ref int[] pNums)
        {
            BorderPoint aBPoint, bP;
            int i, j, k;
            PointD p1, p2, p3;
            //ArrayList aEPList = new ArrayList(), temEPList = new ArrayList(), dList = new ArrayList();
            BorderLine aBLine;
            List<BorderPoint> newBPList = new List<BorderPoint>(), tempBPList = new List<BorderPoint>(), tempBPList1 = new List<BorderPoint>();

            pNums = new int[aBorder.LineNum];
            for (k = 0; k < aBorder.LineNum; k++)
            {
                aBLine = aBorder.LineList[k];
                tempBPList.Clear();
                for (i = 0; i < aBLine.pointList.Count; i++)
                {
                    aBPoint = new BorderPoint();
                    aBPoint.Id = -1;
                    aBPoint.BorderIdx = k;
                    aBPoint.Point = aBLine.pointList[i];
                    aBPoint.Value = S0[aBLine.ijPointList[i].I, aBLine.ijPointList[i].J];
                    tempBPList.Add(aBPoint);
                }
                for (i = 0; i < bPList.Count; i++)
                {
                    bP = (BorderPoint)bPList[i].Clone();
                    bP.BorderIdx = k;
                    p3 = bP.Point;
                    //aBPoint = (BorderPoint)tempBPList[0];
                    p1 = (PointD)tempBPList[0].Point.Clone();
                    for (j = 1; j < tempBPList.Count; j++)
                    {
                        //aBPoint = (BorderPoint)tempBPList[j];
                        p2 = (PointD)tempBPList[j].Point.Clone();
                        if ((p3.X - p1.X) * (p3.X - p2.X) <= 0)
                        {
                            if ((p3.Y - p1.Y) * (p3.Y - p2.Y) <= 0)
                            {
                                if ((p3.X - p1.X) * (p2.Y - p1.Y) - (p2.X - p1.X) * (p3.Y - p1.Y) == 0)
                                {
                                    tempBPList.Insert(j, bP);
                                    break;
                                }
                            }
                        }
                        p1 = p2;
                    }
                }
                tempBPList1.Clear();
                for (i = 0; i < tempBPList.Count; i++)
                {
                    bP = tempBPList[i];
                    bP.BInnerIdx = i;
                    tempBPList1.Add(bP);
                }
                pNums[k] = tempBPList1.Count;
                newBPList.AddRange(tempBPList1);
            }

            return newBPList;
        }

        //private static bool DoubleEquals(double a, double b)
        //{
        //    if (b == 0)
        //        return (Math.Abs(a - b) < 0.0000001);
        //    else
        //    {
        //        if (Math.Abs(a - b) < 0.0000001)
        //        {
        //            if (Math.Abs(a / b - 1) < 0.001)
        //                return true;
        //            else
        //                return false;
        //        }
        //        else
        //            return false;
        //    }
        //}

        private static bool DoubleEquals(double a, double b)
        {
            double difference = Math.Abs(a * 0.00001);
            if (Math.Abs(a - b) < difference)
                return true;
            else
                return false;
        }

        private static double getAbsMinValue(double[,] S0)
        {
            double min = 0, v;
            int m, n, i, j, idx;
            m = S0.GetLength(0);    //---- Y
            n = S0.GetLength(1);    //---- X
            idx = 0;
            for (i = 0; i < m; i++)
            {
                for (j = 0; j < n; j++)
                {
                    v = Math.Abs(S0[i, j]);
                    if (v != 0)
                    {
                        if (idx == 0)
                            min = v;
                        else
                        {
                            if (min > v)
                                min = v;
                        }
                        idx += 1;
                    }                    
                }
            }

            return min;
        }

        private static double getAbsMinValue(double[] values)
        {
            double min = 0.0001, v;
            int n, i;
            n = values.Length;
            int idx = 0;
            for (i = 1; i < n; i++)
            {
                if (values[i] == 0.0)
                    continue;

                v = Math.Abs(values[i]);
                if (idx == 0)
                    min = v;
                else
                {
                    if (min > v)
                        min = v;
                }
            }

            return min;
        }

        #endregion
    }
}
