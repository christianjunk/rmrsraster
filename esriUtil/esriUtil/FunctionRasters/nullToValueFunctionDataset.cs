﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using esriUtil.FunctionRasters.NeighborhoodHelper;

namespace esriUtil.FunctionRasters
{
    public class nullToValueFunctionDataset : IRasterFunction
    {
        private IRasterInfo myRasterInfo; // Raster Info for the focal Function
        private rstPixelType myPixeltype = rstPixelType.PT_UNKNOWN; // Pixel Type of the log Function.
        private string myName = "nulltoValue Function"; // Name of the log Function.
        private string myDescription = "Converts null values to a given value"; // Description of the log Function.
        private IRaster inrs = null;
        private double newvalue = 0;
        System.Array noDataArr = null;
        private IRasterFunctionHelper myFunctionHelper = new RasterFunctionHelperClass(); // Raster Function Helper object.
        public IRasterInfo RasterInfo { get { return myRasterInfo; } }
        public rstPixelType PixelType { get { return myPixeltype; } set { myPixeltype = value; } }
        public string Name { get { return myName; } set { myName = value; } }
        public string Description { get { return myDescription; } set { myDescription = value; } }
        public bool myValidFlag = false;
        public bool Valid { get { return myValidFlag; } }
        public double noDataValue = Double.MinValue;
        public void Bind(object pArgument)
        {
            if (pArgument is nullToValueFunctionArguments)
            {
                nullToValueFunctionArguments args = (nullToValueFunctionArguments)pArgument;
                inrs = args.InRaster;
                newvalue = args.NewValue;
                noDataArr = args.NoDataArray;
                myFunctionHelper.Bind(inrs);
                myRasterInfo = myFunctionHelper.RasterInfo;
                //System.Windows.Forms.MessageBox.Show(rsProp.PixelType.ToString());
                myPixeltype = myRasterInfo.PixelType;
                //System.Windows.Forms.MessageBox.Show(myRasterInfo.PixelType.ToString());
                myValidFlag = true;
            }
            else
            {
                throw new System.Exception("Incorrect arguments object. Expected: nullToValueFunctonArguments");
            }
        }
        /// <summary>
        /// Read pixels from the input Raster and fill the PixelBlock provided with processed pixels.
        /// The RasterFunctionHelper object is used to handle pixel type conversion and resampling.
        /// The log raster is the natural log of the raster. 
        /// </summary>
        /// <param name="pTlc">Point to start the reading from in the Raster</param>
        /// <param name="pRaster">Reference Raster for the PixelBlock</param>
        /// <param name="pPixelBlock">PixelBlock to be filled in</param>
        public void Read(IPnt pTlc, IRaster pRaster, IPixelBlock pPixelBlock)
        {
            try
            {
                System.Array noDataValueArr = noDataArr;
                myFunctionHelper.Read(pTlc, null, pRaster, pPixelBlock);
                int pBHeight = pPixelBlock.Height;
                int pBWidth = pPixelBlock.Width;
                for (int nBand = 0; nBand < pPixelBlock.Planes; nBand++)
                {
                    noDataValue = System.Convert.ToDouble(noDataValueArr.GetValue(nBand));
                    System.Array pixelValues = (System.Array)(pPixelBlock.get_SafeArray(nBand));
                    for (int r = 0; r < pBHeight; r++)
                    {
                        for (int c = 0; c < pBWidth; c++)
                        {
                            double outVl = System.Convert.ToDouble(pixelValues.GetValue(c, r));
                            
                            if (rasterUtil.isNullData(outVl,noDataValue))
                            {
                                //Console.WriteLine("Orig = " + outVl.ToString());
                                outVl = newvalue;
                                //Console.WriteLine("setting values to " + outVl.ToString());
                            }
                            
                            pixelValues.SetValue(outVl, c, r);
                        }  
                    }
                    pPixelBlock.set_SafeArray(nBand, pixelValues);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void Update()
        {
            try
            {
            }
            catch (Exception exc)
            {
                System.Exception myExc = new System.Exception("Exception caught in Update method of aggregation Function", exc);
                throw myExc;
            }
        }
    }
}
