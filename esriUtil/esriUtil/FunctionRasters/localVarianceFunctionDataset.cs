﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace esriUtil.FunctionRasters
{
    class localVarianceFunctionDataset :localFunctionBase
    {
        public override void updateOutArr(ref System.Array outArr, ref List<System.Array> pArr)
        {
            int pBWidth = outArr.GetUpperBound(0) + 1;
            int pBHeight = outArr.GetUpperBound(1) + 1;
            for (int i = 0; i < pBHeight; i++)
            {
                for (int k = 0; k < pBWidth; k++)
                {
                    float sumVl = System.Convert.ToSingle(pArr[0].GetValue(k, i));
                    if (rasterUtil.isNullData(sumVl, System.Convert.ToSingle(noDataValueArr.GetValue(0))))
                    {
                        continue;
                    }
                    float var = 0;
                    float sumVl2 = sumVl*sumVl;
                    for (int nBand = 0; nBand < pArr.Count; nBand++)
                    {
                        float noDataValue = System.Convert.ToSingle(noDataValueArr.GetValue(nBand));
                        float pixelValue = System.Convert.ToSingle(pArr[nBand].GetValue(k, i));
                        if (rasterUtil.isNullData(pixelValue, noDataValue))
                        {
                            var = noDataVl;
                            break;
                        }
                        sumVl += pixelValue;
                        sumVl2 += pixelValue*pixelValue;
                    }
                    var = (sumVl2-((sumVl*sumVl)/pArr.Count))/pArr.Count;
                    outArr.SetValue(var, k, i);
                }
            }
        }
    }
}
