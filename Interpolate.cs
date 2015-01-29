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
    /// Interpolate
    /// </summary>
    public static class Interpolate
    {
        private const double pi = 14159265358;
        private const double twopi = 2.0 * 3.14159265358;

        #region IDW
        /// <summary>
        /// Create grid x/y coordinate arrays with x/y delt
        /// </summary>
        /// <param name="Xlb">X of left-bottom</param>
        /// <param name="Ylb">Y of left-bottom</param>
        /// <param name="Xrt">X of right-top</param>
        /// <param name="Yrt">Y of right-top</param>
        /// <param name="XDelt">X delt</param>
        /// <param name="YDelt">Y delt</param>
        /// <param name="X">Output X array</param>
        /// <param name="Y">Output Y array</param>
        public static void CreateGridXY_Delt(double Xlb, double Ylb, double Xrt, double Yrt,
            double XDelt, double YDelt, ref double[] X, ref double[] Y)
        {
            int i, Xnum, Ynum;
            Xnum = (int)((Xrt - Xlb) / XDelt + 1);
            Ynum = (int)((Yrt - Ylb) / YDelt + 1);
            X = new double[Xnum];
            Y = new double[Ynum];
            for (i = 0; i < Xnum; i++)
            {
                X[i] = Xlb + i * XDelt;
            }
            for (i = 0; i < Ynum; i++)
            {
                Y[i] = Ylb + i * YDelt;
            }
        }

        /// <summary>
        /// Create grid x/y coordinate arrays with x/y number
        /// </summary>
        /// <param name="Xlb">X of left-bottom</param>
        /// <param name="Ylb">Y of left-bottom</param>
        /// <param name="Xrt">X of right-top</param>
        /// <param name="Yrt">Y of right-top</param>
        /// <param name="Xnum">X number</param>
        /// <param name="Ynum">Y number</param>
        /// <param name="X">Output X array</param>
        /// <param name="Y">Output Y array</param>
        public static void CreateGridXY_Num(double Xlb, double Ylb, double Xrt, double Yrt,
            int Xnum, int Ynum, ref double[] X, ref double[] Y)
        {
            int i;
            double XDelt, YDelt;
            X = new double[Xnum];
            Y = new double[Ynum];
            XDelt = (Xrt - Xlb) / Xnum;
            YDelt = (Yrt - Ylb) / Ynum;
            for (i = 0; i < Xnum; i++)
            {
                X[i] = Xlb + i * XDelt;
            }
            for (i = 0; i < Ynum; i++)
            {
                Y[i] = Ylb + i * YDelt;
            }
        }
              
        /// <summary>
        /// Interpolation with IDW neighbor method
        /// </summary>
        /// <param name="SCoords">Discrete data array</param>
        /// <param name="X">Grid X array</param>
        /// <param name="Y">Grid Y array</param>
        /// <param name="NumberOfNearestNeighbors">Number of nearest neighbors</param>
        /// <returns>Interpolated grid Data</returns>
        public static double[,] Interpolation_IDW_Neighbor(double[,] SCoords, double[] X, double[] Y,
            int NumberOfNearestNeighbors)
        {
            int rowNum, colNum, pNum;
            colNum = X.Length;
            rowNum = Y.Length;
            pNum = SCoords.GetLength(1);
            double[,] GCoords = new double[rowNum, colNum];
            int i, j, p, l, aP;
            double w, SV, SW, aMin;
            int points;
            points = NumberOfNearestNeighbors;
            object[,] NW = new object[2, points];

            //---- Do interpolation
            for (i = 0; i < rowNum; i++)
            {
                for (j = 0; j < colNum; j++)
                {
                    GCoords[i, j] = -999.0;
                    SV = 0;
                    SW = 0;
                    for (p = 0; p < points; p++)
                    {
                        if (Math.Pow(X[j] - SCoords[0, p], 2) + Math.Pow(Y[i] - SCoords[1, p], 2) == 0)
                        {
                            GCoords[i, j] = SCoords[2, p];
                            break;
                        }
                        else
                        {
                            w = 1 / (Math.Pow(X[j] - SCoords[0, p], 2) + Math.Pow(Y[i] - SCoords[1, p], 2));
                            NW[0, p] = w;
                            NW[1, p] = p;
                        }
                    }
                    if (GCoords[i, j] == -999.0)
                    {
                        for (p = points; p < pNum; p++)
                        {
                            if (Math.Pow(X[j] - SCoords[0, p], 2) + Math.Pow(Y[i] - SCoords[1, p], 2) == 0)
                            {
                                GCoords[i, j] = SCoords[2, p];
                                break;
                            }
                            else
                            {
                                w = 1 / (Math.Pow(X[j] - SCoords[0, p], 2) + Math.Pow(Y[i] - SCoords[1, p], 2));
                                aMin = (double)NW[0, 0];
                                aP = 0;
                                for (l = 1; l < points; l++)
                                {
                                    if ((double)NW[0, l] < aMin)
                                    {
                                        aMin = (double)NW[0, l];
                                        aP = l;
                                    }
                                }
                                if (w > aMin)
                                {
                                    NW[0, aP] = w;
                                    NW[1, aP] = p;
                                }
                            }
                        }
                        if (GCoords[i, j] == -999.0)
                        {
                            for (p = 0; p < points; p++)
                            {
                                SV += (double)NW[0, p] * SCoords[2, (int)NW[1, p]];
                                SW += (double)NW[0, p];
                            }
                            GCoords[i, j] = SV / SW;
                        }
                    }
                }
            }

            //---- Smooth with 5 points
            double s = 0.5;
            for (i = 1; i < rowNum - 1; i++)
            {
                for (j = 1; j < colNum - 1; j++)
                {
                    GCoords[i, j] = GCoords[i, j] + s / 4 * (GCoords[i + 1, j] + GCoords[i - 1, j] +
                        GCoords[i, j + 1] + GCoords[i, j - 1] - 4 * GCoords[i, j]);
                }
            }

            return GCoords;
        }

        /// <summary>
        /// Interpolation with IDW neighbor method
        /// </summary>
        /// <param name="SCoords">Discrete data array</param>
        /// <param name="X">Grid X array</param>
        /// <param name="Y">Grid Y array</param>
        /// <param name="NumberOfNearestNeighbors">Number of nearest neighbors</param>
        /// <param name="unDefData">Undefine data value</param>
        /// <returns>Interpolated grid data</returns>
        public static double[,] Interpolation_IDW_Neighbor(double[,] SCoords, double[] X, double[] Y,
          int NumberOfNearestNeighbors, double unDefData)
        {
            int rowNum, colNum, pNum;
            colNum = X.Length;
            rowNum = Y.Length;
            pNum = SCoords.GetLength(1);
            double[,] GCoords = new double[rowNum, colNum];
            int i, j, p, l, aP;
            double w, SV, SW, aMin;
            int points;
            points = NumberOfNearestNeighbors;
            double[] AllWeights = new double[pNum];
            object[,] NW = new object[2, points];
            int NWIdx;

            //---- Do interpolation with IDW method 
            for (i = 0; i < rowNum; i++)
            {
                for (j = 0; j < colNum; j++)
                {
                    GCoords[i, j] = unDefData;
                    SV = 0;
                    SW = 0;
                    NWIdx = 0;
                    for (p = 0; p < pNum; p++)
                    {
                        if (SCoords[2, p] == unDefData)
                        {
                            AllWeights[p] = -1;
                            continue;
                        }
                        if (Math.Pow(X[j] - SCoords[0, p], 2) + Math.Pow(Y[i] - SCoords[1, p], 2) == 0)
                        {
                            GCoords[i, j] = SCoords[2, p];
                            break;
                        }
                        else
                        {
                            w = 1 / (Math.Pow(X[j] - SCoords[0, p], 2) + Math.Pow(Y[i] - SCoords[1, p], 2));
                            AllWeights[p] = w;
                            if (NWIdx < points)
                            {
                                NW[0, NWIdx] = w;
                                NW[1, NWIdx] = p;
                            }
                            NWIdx += 1;
                        }
                    }

                    if (GCoords[i, j] == unDefData)
                    {
                        for (p = 0; p < pNum; p++)
                        {
                            w = AllWeights[p];
                            if (w == -1)
                                continue;

                            aMin = (double)NW[0, 0];
                            aP = 0;
                            for (l = 1; l < points; l++)
                            {
                                if ((double)NW[0, l] < aMin)
                                {
                                    aMin = (double)NW[0, l];
                                    aP = l;
                                }
                            }
                            if (w > aMin)
                            {
                                NW[0, aP] = w;
                                NW[1, aP] = p;
                            }
                        }
                        for (p = 0; p < points; p++)
                        {
                            SV += (double)NW[0, p] * SCoords[2, (int)NW[1, p]];
                            SW += (double)NW[0, p];
                        }
                        GCoords[i, j] = SV / SW;
                    }
                }
            }

            //---- Smooth with 5 points
            double s = 0.5;
            for (i = 1; i < rowNum - 1; i++)
            {
                for (j = 1; j < colNum - 1; j++)
                    GCoords[i, j] = GCoords[i, j] + s / 4 * (GCoords[i + 1, j] + GCoords[i - 1, j] + GCoords[i, j + 1] +
                      GCoords[i, j - 1] - 4 * GCoords[i, j]);

            }

            return GCoords;
        }

        /// <summary>
        /// Interpolation with IDW neighbor method
        /// </summary>
        /// <param name="SCoords">Discrete data array</param>
        /// <param name="X">Grid X array</param>
        /// <param name="Y">Grid Y array</param>
        /// <param name="NeededPointNum">Needed at leat point number</param>
        /// <param name="radius">Search radius</param>
        /// <param name="unDefData">Undefine data</param>
        /// <returns>Interpolated grid data</returns>
        public static double[,] Interpolation_IDW_Radius(double[,] SCoords, double[] X, double[] Y,
            int NeededPointNum, double radius, double unDefData)
        {
            int rowNum, colNum, pNum;
            colNum = X.Length;
            rowNum = Y.Length;
            pNum = SCoords.GetLength(1);
            double[,] GCoords = new double[rowNum, colNum];
            int i, j, p, vNum;
            double w, SV, SW;
            bool ifPointGrid;

            //---- Do interpolation
            for (i = 0; i < rowNum; i++)
            {
                for (j = 0; j < colNum; j++)
                {
                    GCoords[i, j] = unDefData;
                    ifPointGrid = false;
                    SV = 0;
                    SW = 0;
                    vNum = 0;
                    for (p = 0; p < pNum; p++)
                    {
                        if (SCoords[2, p] == unDefData)
                            continue;

                        if (SCoords[0, p] < X[j] - radius || SCoords[0, p] > X[j] + radius || SCoords[1, p] < Y[i] - radius ||
                            SCoords[1, p] > Y[i] + radius)
                            continue;

                        if (Math.Pow(X[j] - SCoords[0, p], 2) + Math.Pow(Y[i] - SCoords[1, p], 2) == 0)
                        {
                            GCoords[i, j] = SCoords[2, p];
                            ifPointGrid = true;
                            break;
                        }
                        else if (Math.Sqrt(Math.Pow(X[j] - SCoords[0, p], 2) + 
                            Math.Pow(Y[i] - SCoords[1, p], 2)) <= radius)
                        {
                            w = 1 / (Math.Pow(X[j] - SCoords[0, p], 2) + Math.Pow(Y[i] - SCoords[1, p], 2));
                            SW = SW + w;
                            SV = SV + SCoords[2, p] * w;
                            vNum += 1;
                        }            
                    }

                    if (!ifPointGrid)
                    {
                        if (vNum >= NeededPointNum)
                        {
                            GCoords[i, j] = SV / SW;
                        }
                    }
                }
            }

            //---- Smooth with 5 points
            double s = 0.5;
            for (i = 1; i < rowNum - 1; i++)
            {
                for (j = 1; j < colNum - 2; j++)
                {
                    if (GCoords[i, j] == unDefData || GCoords[i + 1, j] == unDefData || GCoords[i - 1, j] ==
                      unDefData || GCoords[i, j + 1] == unDefData || GCoords[i, j - 1] == unDefData)
                    {
                        continue;
                    }
                    GCoords[i, j] = GCoords[i, j] + s / 4 * (GCoords[i + 1, j] + GCoords[i - 1, j] + GCoords[i, j + 1] +
                      GCoords[i, j - 1] - 4 * GCoords[i, j]);
                }
            }
            
            return GCoords;
        }

        /// <summary>
        /// Interpolate from grid data using bi-linear method
        /// </summary>
        /// <param name="GridData">input grid data</param>
        /// <param name="X">input x coordinates</param>
        /// <param name="Y">input y coordinates</param>
        /// <param name="unDefData">Undefine data</param>
        /// <param name="nX">output x coordinates</param>
        /// <param name="nY">output y coordinates</param>
        /// <returns>Output grid data</returns>
        public static double[,] Interpolation_Grid(double[,] GridData, double[] X, double[] Y, double unDefData,
            ref double[] nX, ref double[] nY)
        {
            int xNum = X.Length;
            int yNum = Y.Length;
            int nxNum = X.Length * 2 - 1;
            int nyNum = Y.Length * 2 - 1;
            nX = new double[nxNum];
            nY = new double[nyNum];
            double[,] nGridData = new double[nyNum, nxNum];
            int i, j;
            double a, b, c, d;
            List<double> dList = new List<double>();
            for (i = 0; i < nxNum; i++)
            {
                if (i % 2 == 0)
                    nX[i] = X[i / 2];
                else
                    nX[i] = (X[(i - 1) / 2] + X[(i - 1) / 2 + 1]) / 2;
            }
            for (i = 0; i < nyNum; i++)
            {
                if (i % 2 == 0)
                    nY[i] = Y[i / 2];
                else
                    nY[i] = (Y[(i - 1) / 2] + Y[(i - 1) / 2 + 1]) / 2;
                for (j = 0; j < nxNum; j++)
                {
                    if (i % 2 == 0 && j % 2 == 0)                                         
                        nGridData[i, j] = GridData[i / 2, j / 2];
                    else if (i % 2 == 0 && j % 2 != 0)
                    {
                        a = GridData[i / 2, (j - 1) / 2];
                        b = GridData[i / 2, (j - 1) / 2 + 1];
                        dList = new List<double>();
                        if (a != unDefData)
                            dList.Add(a);
                        if (b != unDefData)
                            dList.Add(b);

                        if (dList.Count == 0)
                            nGridData[i, j] = unDefData;
                        else if (dList.Count == 1)
                            nGridData[i, j] = dList[0];
                        else
                            nGridData[i, j] = (a + b) / 2;
                    }
                    else if (i % 2 != 0 && j % 2 == 0)
                    {
                        a = GridData[(i - 1) / 2, j / 2];
                        b = GridData[(i - 1) / 2 + 1, j / 2];
                        dList = new List<double>();
                        if (a != unDefData)
                            dList.Add(a);
                        if (b != unDefData)
                            dList.Add(b);

                        if (dList.Count == 0)
                            nGridData[i, j] = unDefData;
                        else if (dList.Count == 1)
                            nGridData[i, j] = dList[0];
                        else
                            nGridData[i, j] = (a + b) / 2;
                    }
                    else
                    {
                        a = GridData[(i - 1) / 2, (j - 1) / 2];
                        b = GridData[(i - 1) / 2, (j - 1) / 2 + 1];
                        c = GridData[(i - 1) / 2 + 1, (j - 1) / 2 + 1];
                        d = GridData[(i - 1) / 2 + 1, (j - 1) / 2];
                        dList = new List<double>();
                        if (a != unDefData)
                            dList.Add(a);
                        if (b != unDefData)
                            dList.Add(b);
                        if (c != unDefData)
                            dList.Add(c);
                        if (d != unDefData)
                            dList.Add(d);

                        if (dList.Count == 0)
                            nGridData[i, j] = unDefData;
                        else if (dList.Count == 1)
                            nGridData[i, j] = dList[0];
                        else
                        {
                            double aSum = 0;
                            foreach (double dd in dList)
                                aSum += dd;
                            nGridData[i, j] = aSum / dList.Count;
                        }
                    }
                }
            }

            return nGridData;
        }

        #endregion

        #region Cressman
         /// <summary>
        /// Cressman analysis with default radius of 10, 7, 4, 2, 1
        /// </summary>
        /// <param name="stationData">station data array - x,y,value</param>
        /// <param name="X">X array</param>
        /// <param name="Y">Y array</param>
        /// <param name="unDefData">undefine data</param>        
        /// <returns>result grid data</returns>
        public static double[,] Cressman(double[,] stationData, double[] X, double[] Y, double unDefData)
        {
            List<double> radList = new List<double>();
            radList.AddRange(new double[] { 10, 7, 4, 2, 1 });

            return Cressman(stationData, X, Y, unDefData, radList);
        }

        /// <summary>
        /// Cressman analysis
        /// </summary>
        /// <param name="stationData">station data array - x,y,value</param>
        /// <param name="X">X array</param>
        /// <param name="Y">Y array</param>
        /// <param name="unDefData">undefine data</param>
        /// <param name="radList">radius list</param>
        /// <returns>result grid data</returns>
        public static double[,] Cressman(double[,] stationData, double[] X, double[] Y, double unDefData, List<double> radList)
        {
            double[,] stData = (double[,])stationData.Clone();
            int xNum = X.Length;
            int yNum = Y.Length;
            int pNum = stData.GetLength(1);
            double[,] gridData = new double[yNum, xNum];
            int irad = radList.Count;
            int i, j;
            
            //Loop through each stn report and convert stn lat/lon to grid coordinates
            double xMin = X[0];
            double xMax = X[X.Length - 1];
            double yMin = Y[0];
            double yMax = Y[Y.Length - 1];
            double xDelt = X[1] - X[0];
            double yDelt = Y[1] - Y[0];
            double x, y;
            double sum = 0, total = 0;
            int stNum = 0;
            for (i = 0; i < pNum; i++)
            {
                x = stData[0, i];
                y = stData[1, i];
                stData[0, i] = (x - xMin) / xDelt;
                stData[1, i] = (y - yMin) / yDelt;
                if (stData[2, i] != unDefData)
                {
                    total += stData[2, i];
                    stNum += 1;
                }
            }
            total = total / stNum;

            ////Initial grid values are average of station reports
            //for (i = 0; i < yNum; i++)
            //{
            //    for (j = 0; j < xNum; j++)
            //    {
            //        gridData[i, j] = sum;
            //    }
            //}

            //Initial the arrays
            double HITOP = -999900000000000000000.0;
            double HIBOT = 999900000000000000000.0;
            double[,] TOP = new double[yNum, xNum];
            double[,] BOT = new double[yNum, xNum];
            //double[,] GRID = new double[yNum, xNum];
            //int[,] NG = new int[yNum, xNum];
            for (i = 0; i < yNum; i++)
            {
                for (j = 0; j < xNum; j++)
                {
                    TOP[i, j] = HITOP;
                    BOT[i, j] = HIBOT;
                    //GRID[i, j] = 0;
                    //NG[i, j] = 0;
                }
            }

            //Initial grid values are average of station reports within the first radius
            double rad;
            if (radList.Count > 0)
                rad = radList[0];
            else
                rad = 4;
            for (i = 0; i < yNum; i++)
            {
                y = (double)i;
                yMin = y - rad;
                yMax = y + rad;
                for (j = 0; j < xNum; j++)
                {
                    x = (double)j;
                    xMin = x - rad;
                    xMax = x + rad;
                    stNum = 0;
                    sum = 0;
                    for (int s = 0; s < pNum; s++)
                    {
                        double val = stData[2, s];
                        double sx = stData[0, s];
                        double sy = stData[1, s];
                        if (sx < 0 || sx >= xNum - 1 || sy < 0 || sy >= yNum - 1)
                            continue;

                        if (val == unDefData || sx < xMin || sx > xMax || sy < yMin || sy > yMax)
                            continue;

                        double dis = Math.Sqrt(Math.Pow(sx - x, 2) + Math.Pow(sy - y, 2));
                        if (dis > rad)
                            continue;

                        sum += val;
                        stNum += 1;
                        if (TOP[i, j] < val)
                            TOP[i, j] = val;
                        if (BOT[i, j] > val)
                            BOT[i, j] = val;
                    }
                    if (stNum == 0)
                    {
                        gridData[i, j] = unDefData;
                        //gridData[i, j] = total;
                    }
                    else
                        gridData[i, j] = sum / stNum;
                }
            }

            //Perform the objective analysis
            for (int p = 0; p < irad; p++)
            {
                rad = radList[p];
                for (i = 0; i < yNum; i++)
                {
                    y = (double)i;
                    yMin = y - rad;
                    yMax = y + rad;
                    for (j = 0; j < xNum; j++)
                    {
                        if (gridData[i, j] == unDefData)
                            continue;

                        x = (double)j;
                        xMin = x - rad;
                        xMax = x + rad;                        
                        sum = 0;
                        double wSum = 0;
                        for (int s = 0; s < pNum; s++)
                        {
                            double val = stData[2, s];
                            double sx = stData[0, s];
                            double sy = stData[1, s];
                            if (sx < 0 || sx >= xNum - 1 || sy < 0 || sy >= yNum - 1)
                                continue;

                            if (val == unDefData || sx < xMin || sx > xMax || sy < yMin || sy > yMax)
                                continue;

                            double dis = Math.Sqrt(Math.Pow(sx - x, 2) + Math.Pow(sy - y, 2));
                            if (dis > rad)
                                continue;

                            int i1 = (int)sy;
                            int j1 = (int)sx;
                            int i2 = i1 + 1;
                            int j2 = j1 + 1;
                            double a = gridData[i1, j1];
                            double b = gridData[i1, j2];
                            double c = gridData[i2, j1];
                            double d = gridData[i2, j2];
                            List<double> dList = new List<double>();
                            if (a != unDefData)
                                dList.Add(a);
                            if (b != unDefData)
                                dList.Add(b);
                            if (c != unDefData)
                                dList.Add(c);
                            if (d != unDefData)
                                dList.Add(d);

                            double calVal;
                            if (dList.Count == 0)
                                continue;
                            else if (dList.Count == 1)
                                calVal = dList[0];
                            else if (dList.Count <= 3)
                            {
                                double aSum = 0;
                                foreach (double dd in dList)
                                    aSum += dd;
                                calVal = aSum / dList.Count;
                            }
                            else
                            {
                                double x1val = a + (c - a) * (sy - i1);
                                double x2val = b + (d - b) * (sy - i1);
                                calVal = x1val + (x2val - x1val) * (sx - j1);
                            }
                            double eVal = val - calVal;
                            double w = (rad * rad - dis * dis) / (rad * rad + dis * dis);
                            sum += eVal * w;
                            wSum += w;
                        }
                        if (wSum < 0.000001)
                        {
                            gridData[i, j] = unDefData;
                        }
                        else
                        {
                            double aData = gridData[i, j] + sum / wSum;
                            gridData[i, j] = Math.Max(BOT[i, j], Math.Min(TOP[i, j], aData));
                        }
                    }
                }
            }

            //Return
            return gridData;
        }

        ///// <summary>
        ///// Cressman analysis
        ///// </summary>
        ///// <param name="stationData">station data array - x,y,value</param>
        ///// <param name="GX">X array</param>
        ///// <param name="GY">Y array</param>
        ///// <param name="unDefData">undefine data</param>
        ///// <param name="radList">radii list</param>
        ///// <returns>result grid data</returns>
        //public static double[,] CressmanR(double[,] stationData, double[] GX, double[] GY, double unDefData, List<double> radList)
        //{
        //    int MXI = GY.Length;
        //    int MXJ = GX.Length;
        //    int II = MXI;
        //    int JJ = MXJ;

        //    double[,] FIELD = new double[MXI, MXJ];
        //    int[,] NG = new int[MXI, MXJ];
        //    double[,] TOP = new double[MXI, MXJ];
        //    double[,] BOT = new double[MXI, MXJ];
        //    double[,] GRID = new double[MXI, MXJ];
        //    double[,] SW = new double[MXI, MXJ];
        //    double[,] WS = new double[MXI, MXJ];
        //    double TOTAL, ASTA;

        //    int MXSTA = stationData.GetLength(1);
        //    int NSTA = MXSTA;
        //    double[] STDATA = new double[MXSTA];
        //    double[] STAX = new double[MXSTA];
        //    double[] STAY = new double[MXSTA];
        //    int I, J;
        //    double xMin = GX[0];
        //    double xMax = GX[GX.Length - 1];
        //    double yMin = GY[0];
        //    double yMax = GY[GY.Length - 1];
        //    double xDelt = GX[1] - GX[0];
        //    double yDelt = GY[1] - GY[0];
        //    for (I = 0; I < MXSTA; I++)
        //    {
        //        STDATA[I] = stationData[2, I];
        //        //STAX[I] = stationData[0, I];
        //        //STAY[I] = stationData[1, I];
        //        STAX[I] = (stationData[0, I] - xMin) / xDelt;
        //        STAY[I] = (stationData[1, I] - yMin) / yDelt;
        //    }
        //    double HITOP = -999900000000000000000.0;
        //    double HIBOT = 999900000000000000000.0;
        //    TOTAL = 0.0;
        //    ASTA = 0.0;                       
        //    for (J = 0; J < NSTA; J++)
        //    {
        //        if (Math.Abs(STDATA[J]) != unDefData)
        //        {
        //            TOTAL = TOTAL + STDATA[J];
        //            ASTA = ASTA + 1.0;
        //        }
        //    }
        //    if (ASTA == 0.0)
        //    {
        //         //No effective station data
        //        return null;
        //    }
        //    TOTAL = TOTAL / ASTA;
        //    for (J = 0; J < JJ; J++)
        //    {
        //        for (I = 0; I < II; I++)
        //            FIELD[I, J] = TOTAL;
        //    }
        //    //C                                                                       
        //    //C        NOW DO THE CRESSMAN ANALYSIS                                   
        //    //C                                                                       
        //    for (J = 0; J < JJ; J++)
        //    {
        //        for (I = 0; I < II; I++)
        //        {
        //            //C                                                                       
        //            //C       INITIALIZE THE ARRAYS                                           
        //            //C                                                                       
        //            TOP[I, J] = HITOP;
        //            BOT[I, J] = HIBOT;
        //            GRID[I, J] = 0.0;
        //            NG[I, J] = 0;
        //        }
        //    }

        //    double RADM = radList[0];
        //    double RADL = RADM - 1.0;
        //    int JL, JM, IL, IM;
        //    for (int N = 0; N < NSTA; N++)
        //    {
        //        if (STDATA[N] == unDefData)
        //            continue;

        //        //C                                                                       
        //        //C        FOR EACH STATION, CHECK THE X/Y COORDINATES                    
        //        //C        INITIALIZE THE GRID PT ARRAY USING AVAILABLE DATA WITHIN A     
        //        //C        RADIUS OF RADM GRID POINTS OF THE GRID POINT OF INTEREST.      
        //        //C                                                                       
        //        JL = (int)Math.Max(1.0, STAY[N] - RADL);
        //        JM = (int)Math.Min((double)JJ, STAY[N] + RADM);
        //        IL = (int)Math.Max(1.0, STAX[N] - RADL);
        //        IM = (int)Math.Min((double)II, STAX[N] + RADM);
        //        for (J = JL - 1; J < JM; J++)
        //        {
        //            for (I = IL - 1; I < IM; I++)
        //            {
        //                if (STDATA[N] > TOP[I, J])
        //                    TOP[I, J] = STDATA[N];
        //                if (STDATA[N] < BOT[I, J])
        //                    BOT[I, J] = STDATA[N];
        //                GRID[I, J] = GRID[I, J] + STDATA[N];
        //                NG[I, J] = NG[I, J] + 1;
        //            }
        //        }
        //    }
        //    //C                                                                       
        //    //C        FORM 1ST GUESS FIELD BY COMPUTING AN AVERAGE VALUE FIELD       
        //    //C                                                                       
        //    for (J = 0; J < JJ; J++)
        //    {
        //        for (I = 0; I < II; I++)
        //        {
        //            //C                                                                       
        //            //C         GRID=-9999 IF NO STA VALUES INFLUENCE A PARTICULAR GRID POINT 
        //            //C                                                                       
        //            if (NG[I, J] > 0)
        //                GRID[I, J] = GRID[I, J] / (double)NG[I, J];
        //            else
        //                //C           GRID(I,J)=-9999.                                            
        //                GRID[I, J] = TOTAL;

        //            //C                                                                       
        //            //C        THE 1ST GUESS IS THE AVERAGE VALUE OF ALL STA OBS SURROUNDING  
        //            //C        THE PARTICULAR GRID POINT.                                     
        //            //C                                                                       
        //        }
        //    }

        //    //C                                                                       
        //    //C        DO NPASS ITERATIONS OR PASSES FOR THE ANALYSIS                 
        //    //C 
        //    int NPASS = radList.Count;
        //    for (int ITER = 0; ITER < NPASS; ITER++)
        //    {
        //        RADM = radList[ITER];
        //        RADL = RADM - 1.0;
        //        double RSQ = RADM * RADM;
        //        for (J = 0; J < JJ; J++)
        //        {
        //            for (I = 0; I < II; I++)
        //            {
        //                WS[I, J] = 0.0;
        //                SW[I, J] = 0.0;
        //            }
        //        }
        //        //C                                                                       
        //        //C        ON EACH PASS, USE ALL OF THE STATION VALUES                    
        //        //C                                                                       
        //        for (int N = 0; N < NSTA; N++)
        //        {
        //            if (STDATA[N] == unDefData)
        //                continue;

        //            if (STAX[N] < 0 || STAX[N] >= JJ - 1 || STAY[N] < 0 || STAY[N] >= II - 1)
        //                continue;

        //            double X = STAX[N];
        //            double Y = STAY[N];
        //            int IX = (int)X;
        //            int JY = (int)Y;
        //            double DX = X - (double)IX;
        //            double DY = Y - (double)JY;
        //            //C                                                                       
        //            //C        FROM THE GUESS FIELD INTERPOLATE TO EACH STATION               
        //            //C                                                                       
        //            double GT = GRID[IX, JY] + (GRID[IX + 1, JY] - GRID[IX, JY]) * DX
        //               + (GRID[IX, JY + 1] - GRID[IX, JY]) * DY
        //               + (GRID[IX, JY] - GRID[IX + 1, JY] -
        //               GRID[IX, JY + 1] + GRID[IX + 1, JY + 1]) * DX * DY;
        //            //C                                                                       
        //            //C        DIFFERENCE BETWEEN THE STA VALUE(OBS) AND THE INTERP. VALUE    
        //            //C                                                                       
        //            double DV = STDATA[N] - GT;
        //            //C                                                                       
        //            //C        SET UP THE LIMITS OF THE SPHERE OF INFLUENCE                   
        //            //C                                                                       
        //            JL = (int)Math.Max(1.0, Y - RADL);
        //            double YL = JL;
        //            JM = (int)Math.Min((double)JJ, Y + RADM);
        //            IL = (int)Math.Max(1.0, X - RADL);
        //            double XL = IL;
        //            IM = (int)Math.Min((double)II, X + RADM);
        //            double YJ = YL - Y - 1.0;
        //            for (J = JL - 1; J < JM; J++)
        //            {
        //                YJ = YJ + 1.0;
        //                double DD = YJ * YJ;
        //                double XI = XL - X - 1.0;
        //                for (I = IL - 1; I < IM; I++)
        //                {
        //                    XI = XI + 1.0;
        //                    //C                                                                       
        //                    //C        CALCULATE THE STA DIST FROM THE GRID PT                        
        //                    //C                                                                       
        //                    double D2 = DD + XI * XI;
        //                    //C                                                                       
        //                    //C        CHECK IF THE STA IS CLOSE TO THE GRID PT TO INFLUENCE IT       
        //                    //C                                                                       
        //                    if (D2 <= RSQ)
        //                    {
        //                        //C                                                                       
        //                        //C        CALCULATE THE CORRECTION FACTOR                                
        //                        //C                                                                       
        //                        double CCF = (RSQ - D2) / (RSQ + D2);
        //                        SW[I, J] = SW[I, J] + CCF;
        //                        WS[I, J] = WS[I, J] + CCF * DV;
        //                    }
        //                }
        //            }
        //        }
        //        for (J = 0; J < JJ; J++)
        //        {
        //            for (I = 0; I < II; I++)
        //            {
        //                if (SW[I, J] >= 0.00001)
        //                {
        //                    //C                                                                       
        //                    //C        GRID PT VALUE IS CORRECTED BY AN AVERAGE CORRECTION VALUE      
        //                    //C                                                                       
        //                    double PP = GRID[I, J] + WS[I, J] / SW[I, J];
        //                    GRID[I, J] = Math.Max(BOT[I, J], Math.Min(TOP[I, J], PP));
        //                }
        //            }
        //        }
        //    }

        //    for (J = 0; J < JJ; J++)
        //    {
        //        for (I = 0; I < II; I++)
        //        {
        //            GRID[I, J] = Math.Min(HIBOT, Math.Max(HITOP, GRID[I, J]));
        //            FIELD[I, J] = GRID[I, J];
        //        }
        //    }
        //    return FIELD;
        //}
       
        #endregion

        #region Others
        /// <summary>
        /// Assign point value to grid value
        /// </summary>
        /// <param name="SCoords">point value array</param>
        /// <param name="X">X coordinate</param>
        /// <param name="Y">Y coordinate</param>
        /// <param name="unDefData">undefined value</param>
        /// <returns>grid data</returns>
        public static double[,] AssignPointToGrid(double[,] SCoords, double[] X, double[] Y,
            double unDefData)
        {
            int rowNum, colNum, pNum;
            colNum = X.Length;
            rowNum = Y.Length;
            pNum = SCoords.GetLength(1);
            double[,] GCoords = new double[rowNum, colNum];
            double dX = X[1] - X[0];
            double dY = Y[1] - Y[0];
            int[,] pNums = new int[rowNum, colNum];            

            for (int i = 0; i < rowNum; i++)
            {
                for (int j = 0; j < colNum; j++)
                {
                    pNums[i, j] = 0;
                    GCoords[i, j] = 0.0;
                }
            }

            for (int p = 0; p < pNum; p++)
            {
                if (DoubleEquals(SCoords[2, p], unDefData))
                    continue;

                double x = SCoords[0, p];
                double y = SCoords[1, p];
                if (x < X[0] || x > X[colNum - 1])
                    continue;
                if (y < Y[0] || y > Y[rowNum - 1])
                    continue;

                int j = (int)((x - X[0]) / dX);
                int i = (int)((y - Y[0]) / dY);
                pNums[i, j] += 1;
                GCoords[i, j] += SCoords[2, p];
            }

            for (int i = 0; i < rowNum; i++)
            {
                for (int j = 0; j < colNum; j++)
                {
                    if (pNums[i, j] == 0)
                        GCoords[i, j] = unDefData;
                    else
                        GCoords[i, j] = GCoords[i, j] / pNums[i, j];
                }
            }

            return GCoords;
        }

        private static bool DoubleEquals(double a, double b)
        {
            //if (Math.Abs(a - b) < 0.000001)
            if (Math.Abs(a / b - 1) < 0.00000000001)
                return true;
            else
                return false;
        }

        #endregion

    }
}
