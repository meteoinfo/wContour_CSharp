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
    /// Legend class
    /// </summary>
    public class Legend
    {
        /// <summary>
        /// Legend parameter
        /// </summary>
        public struct legendPara
        {
            /// <summary>
            /// If is vertical
            /// </summary>
            public bool isVertical;
            /// <summary>
            /// Start point
            /// </summary>
            public PointD startPoint;
            /// <summary>
            /// Length
            /// </summary>
            public double length;
            /// <summary>
            /// Width
            /// </summary>
            public double width;
            /// <summary>
            /// Contour values
            /// </summary>
            public double[] contourValues;
            /// <summary>
            /// If is triangle
            /// </summary>
            public bool isTriangle;
        }

        /// <summary>
        /// Legend polygon
        /// </summary>
        public struct lPolygon
        {
            /// <summary>
            /// Value
            /// </summary>
            public double value;
            /// <summary>
            /// If is first
            /// </summary>
            public bool isFirst;
            /// <summary>
            /// Point list
            /// </summary>
            public List<PointD> pointList;
        }

        /// <summary>
        /// Create legend polygons
        /// </summary>
        /// <param name="aLegendPara"> legend parameters</param>
        /// <returns>legend polygons</returns>
        public static List<lPolygon> CreateLegend(legendPara aLegendPara)
        {
            List<lPolygon> polygonList = new List<lPolygon>();
            List<PointD> pList = new List<PointD>();
            lPolygon aLPolygon;
            PointD aPoint;
            int i, pNum;
            double aLength;
            bool ifRectangle;

            pNum = aLegendPara.contourValues.Length + 1;
            aLength = aLegendPara.length / pNum;
            if (aLegendPara.isVertical)
            {
                for (i = 0; i < pNum; i++)
                {
                    pList = new List<PointD>();
                    ifRectangle = true;
                    if (i == 0)
                    {
                        aLPolygon.value = aLegendPara.contourValues[0];
                        aLPolygon.isFirst = true;
                        if (aLegendPara.isTriangle)
                        {
                            aPoint = new PointD();
                            aPoint.X = aLegendPara.startPoint.X + aLegendPara.width / 2;
                            aPoint.Y = aLegendPara.startPoint.Y;
                            pList.Add(aPoint);
                            aPoint = new PointD();
                            aPoint.X = aLegendPara.startPoint.X + aLegendPara.width;
                            aPoint.Y = aLegendPara.startPoint.Y + aLength;
                            pList.Add(aPoint);
                            aPoint = new PointD();
                            aPoint.X = aLegendPara.startPoint.X;
                            aPoint.Y = aLegendPara.startPoint.Y + aLength;
                            pList.Add(aPoint);
                            ifRectangle = false;
                        }
                    }
                    else
                    {
                        aLPolygon.value = aLegendPara.contourValues[i - 1];
                        aLPolygon.isFirst = false;
                        if (i == pNum - 1)
                        {
                            if (aLegendPara.isTriangle)
                            {
                                aPoint = new PointD();
                                aPoint.X = aLegendPara.startPoint.X;
                                aPoint.Y = aLegendPara.startPoint.Y + i * aLength;
                                pList.Add(aPoint);
                                aPoint = new PointD();
                                aPoint.X = aLegendPara.startPoint.X + aLegendPara.width;
                                aPoint.Y = aLegendPara.startPoint.Y + i * aLength;
                                pList.Add(aPoint);
                                aPoint = new PointD();
                                aPoint.X = aLegendPara.startPoint.X + aLegendPara.width / 2;
                                aPoint.Y = aLegendPara.startPoint.Y + (i + 1) * aLength;
                                pList.Add(aPoint);
                                ifRectangle = false;
                            }
                        }
                    }

                    if (ifRectangle)
                    {
                        aPoint = new PointD();
                        aPoint.X = aLegendPara.startPoint.X;
                        aPoint.Y = aLegendPara.startPoint.Y + i * aLength;
                        pList.Add(aPoint);
                        aPoint = new PointD();
                        aPoint.X = aLegendPara.startPoint.X + aLegendPara.width;
                        aPoint.Y = aLegendPara.startPoint.Y + i * aLength;
                        pList.Add(aPoint);
                        aPoint = new PointD();
                        aPoint.X = aLegendPara.startPoint.X + aLegendPara.width;
                        aPoint.Y = aLegendPara.startPoint.Y + (i + 1) * aLength;
                        pList.Add(aPoint);
                        aPoint = new PointD();
                        aPoint.X = aLegendPara.startPoint.X;
                        aPoint.Y = aLegendPara.startPoint.Y + (i + 1) * aLength;
                        pList.Add(aPoint);
                    }

                    pList.Add(pList[0]);
                    aLPolygon.pointList = pList;

                    polygonList.Add(aLPolygon);
                }
            }
            else
            {
                for (i = 0; i < pNum; i++)
                {
                    pList = new List<PointD>();
                    ifRectangle = true;
                    if (i == 0)
                    {
                        aLPolygon.value = aLegendPara.contourValues[0];
                        aLPolygon.isFirst = true;
                        if (aLegendPara.isTriangle)
                        {
                            aPoint = new PointD();
                            aPoint.X = aLegendPara.startPoint.X;
                            aPoint.Y = aLegendPara.startPoint.Y + aLegendPara.width / 2;
                            pList.Add(aPoint);
                            aPoint = new PointD();
                            aPoint.X = aLegendPara.startPoint.X + aLength;
                            aPoint.Y = aLegendPara.startPoint.Y;
                            pList.Add(aPoint);
                            aPoint = new PointD();
                            aPoint.X = aLegendPara.startPoint.X + aLength;
                            aPoint.Y = aLegendPara.startPoint.Y + aLegendPara.width;
                            pList.Add(aPoint);
                            ifRectangle = false;
                        }
                    }
                    else
                    {
                        aLPolygon.value = aLegendPara.contourValues[i - 1];
                        aLPolygon.isFirst = false;
                        if (i == pNum - 1)
                        {
                            if (aLegendPara.isTriangle)
                            {
                                aPoint = new PointD();
                                aPoint.X = aLegendPara.startPoint.X + i * aLength;
                                aPoint.Y = aLegendPara.startPoint.Y;
                                pList.Add(aPoint);
                                aPoint = new PointD();
                                aPoint.X = aLegendPara.startPoint.X + (i + 1) * aLength;
                                aPoint.Y = aLegendPara.startPoint.Y + aLegendPara.width / 2;
                                pList.Add(aPoint);
                                aPoint = new PointD();
                                aPoint.X = aLegendPara.startPoint.X + i * aLength;
                                aPoint.Y = aLegendPara.startPoint.Y + aLegendPara.width;
                                pList.Add(aPoint);
                                ifRectangle = false;
                            }
                        }
                    }

                    if (ifRectangle)
                    {
                        aPoint = new PointD();
                        aPoint.X = aLegendPara.startPoint.X + i * aLength;
                        aPoint.Y = aLegendPara.startPoint.Y;
                        pList.Add(aPoint);
                        aPoint = new PointD();
                        aPoint.X = aLegendPara.startPoint.X + (i + 1) * aLength;
                        aPoint.Y = aLegendPara.startPoint.Y;
                        pList.Add(aPoint);
                        aPoint = new PointD();
                        aPoint.X = aLegendPara.startPoint.X + (i + 1) * aLength;
                        aPoint.Y = aLegendPara.startPoint.Y + aLegendPara.width;
                        pList.Add(aPoint);
                        aPoint = new PointD();
                        aPoint.X = aLegendPara.startPoint.X + i * aLength;
                        aPoint.Y = aLegendPara.startPoint.Y + aLegendPara.width;
                        pList.Add(aPoint);
                    }

                    pList.Add(pList[0]);
                    aLPolygon.pointList = pList;

                    polygonList.Add(aLPolygon);
                }
            }

            return polygonList;
        }
    }
}
