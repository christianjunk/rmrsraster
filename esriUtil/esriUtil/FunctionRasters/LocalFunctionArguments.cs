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
     public class LocalFunctionArguments 
     {
         public LocalFunctionArguments()
         {
             rsUtil = new rasterUtil();
         }
         public LocalFunctionArguments(rasterUtil rasterUtility)
         {
             rsUtil = rasterUtility;
         }
         private IRaster inrs = null;
         private rasterUtil rsUtil = null;
         public ESRI.ArcGIS.Geodatabase.IRaster InRaster 
         { 
             get 
             { 
                 return inrs; 
             } 
             set 
             {
                 IRaster temp = value;
                 inrs = rsUtil.returnRaster(temp,rstPixelType.PT_FLOAT);
                 IRaster rs = rsUtil.getBand(inrs, 0);
                 otrs = rsUtil.constantRasterFunction(rs, 0);
             } 
         }
         private IRaster otrs = null;
         public ESRI.ArcGIS.Geodatabase.IRaster outRaster { get { return otrs; }}
     }
}

