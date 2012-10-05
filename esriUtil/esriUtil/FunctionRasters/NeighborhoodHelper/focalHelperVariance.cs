﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.esriSystem;

namespace esriUtil.FunctionRasters.NeighborhoodHelper
{
    class focalHelperVariance : focalFunctionDatasetBase
    {
        public override object getTransformedValue(System.Array bigArr, int startClm, int startRw)
        {
            double s2 = 0;
            double s = 0;
            int cntSub = 0;
            foreach (int[] xy in iter)
            {
                int bWc = xy[0] + startClm;
                int bRc = xy[1] + startRw;
                double vl = System.Convert.ToDouble(bigArr.GetValue(bWc, bRc));
                if (rasterUtil.isNullData(vl, noDataValue))
                {
                    cntSub++;
                }
                else
                {
                    s += vl;
                    s2 += vl * vl;
                }
            }
            int n = iter.Count - cntSub;
            return (s2 - ((s*s) / n)) / n;
        }
    }
}