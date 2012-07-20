﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.esriSystem;

namespace esriUtil.FunctionRasters.NeighborhoodHelper
{
    class focalHelperSum: focalFunctionDatasetBase
    {
        public override object  getTransformedValue(System.Array bigArr, int startClm, int startRw)
        {
            //Console.WriteLine("Start CR = " + startClm.ToString()+":"+ startRw.ToString());
            double db = 0;
            foreach (int[] xy in iter)
            {
                int bWc = xy[0] + startClm;
                int bRc = xy[1] + startRw;
                
                double vl = System.Convert.ToDouble(bigArr.GetValue(bWc, bRc));
                //Console.WriteLine("\t"+vl.ToString());
                if(vl==noDataValue)
                {
                    continue;
                }
                else
                {
                    db += vl;
                }
            }
            return db;
        }
       
    }
}