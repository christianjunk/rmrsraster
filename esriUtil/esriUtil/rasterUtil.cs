﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.DataSourcesNetCDF;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;

namespace esriUtil
{
    public class rasterUtil
    {
        public rasterUtil()
        {
            string mainPath = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\RmrsRasterUtilityHelp";
            string globFuncDir = mainPath + "\\func";
            string globMosaicDir = mainPath + "\\mosaic";
            string globConvDir = mainPath + "\\conv";
            System.IO.DirectoryInfo DInfo = new System.IO.DirectoryInfo(globFuncDir);
            if (!DInfo.Exists)
            {
                DInfo.Create();
            }
            if (!System.IO.Directory.Exists(globMosaicDir)) System.IO.Directory.CreateDirectory(globMosaicDir);
            if (!System.IO.Directory.Exists(globConvDir)) System.IO.Directory.CreateDirectory(globConvDir);
            mosaicDir = globMosaicDir + "\\" + newGuid;
            funcDir = globFuncDir + "\\" + newGuid;
            convDir = globConvDir + "\\" + newGuid;
            fp = newGuid.Substring(1, 3);
            System.IO.Directory.CreateDirectory(funcDir);
            System.IO.Directory.CreateDirectory(convDir);
            System.IO.Directory.CreateDirectory(mosaicDir);
        }
        //~rasterUtil()
        //{
        //    try
        //    {
        //        System.IO.Directory.Delete(funcDir, true);
        //    }
        //    catch
        //    {
        //    }
            
        //}
        public bool isNumeric(string s)
        {
            return geoUtil.isNumeric(s);
        }
        
        private string mosaicDir = "";
        public string TempMosaicDir { get { return mosaicDir; } } 
        private string funcDir = "";
        public string TempFuncDir { get { return funcDir; } }
        private string convDir = "";
        public string TempConvDir { get { return convDir; } }
        private int funcCnt = 0;
        private string newGuid = System.Guid.NewGuid().ToString();
        private string fp = "";
        private string FuncCnt
        {
            get
            {
                funcCnt++;
                return fp+funcCnt.ToString();
            }
        }
        /// <summary>
        /// The different GLCM metric types
        /// </summary>
        public enum glcmMetric { CONTRAST, DIS, HOMOG, ASM, ENERGY, MAXPROB, MINPROB, RANGE, ENTROPY, MEAN, VAR, CORR, COV }
        /// <summary>
        /// ouput raster types
        /// </summary>
        public enum rasterType { GRID, TIFF, IMAGINE, JP2, GDB, JPG, PNG, BMP, GIF,PIX, XPM, MAP, MEM, HDF4, BIL, BIP, BSQ, RST, ENV }
        /// <summary>
        /// sampling cluster types
        /// </summary>
        public enum clusterType {SUM,MEAN,MEDIAN,MODE};
        public enum zoneType { MAX, MIN, RANGE, SUM, MEAN, VAR, STD, MEDIAN, MODE, MINORITY, VARIETY, ENTROPY, ASM }
        /// <summary>
        /// focal window functions types
        /// </summary>
        public enum focalType { SUM, MIN, MAX, MEAN, STD, MODE, MEDIAN, VARIANCE, UNIQUE, ENTROPY, ASM }
        /// <summary>
        /// local type of functions
        /// </summary>
        public enum localType { MAX, MIN, MAXBAND, MINBAND, SUM, MULTIPLY, DIVIDE, SUBTRACT, POWER, MEAN, VARIANCE, STD, MODE, MEDIAN, UNIQUE, ENTROPY, ASM }
        /// <summary>
        /// logical type of functions
        /// </summary>
        public enum logicalType { GT, LT, GE, LE, EQ, AND, OR }
        /// <summary>
        /// patch values used in landscape metrics
        /// </summary>
        public enum landscapeType { AREA, EDGE, RATIO, REGION }
        /// <summary>
        /// the window neighborhood window type
        /// </summary>
        public enum windowType {CIRCLE,RECTANGLE};
        /// <summary>
        /// the log type
        /// </summary>
        public enum transType { LOG10, LN, EXP, EXP10, ABS, SIN, COS, TAN, ASIN, ACOS, ATAN, RADIANS, SQRT, SQUARED }
        public enum mergeType { FIRST, LAST, MIN, MAX, MEAN }
        public enum surfaceType { SLOPE, ASPECT, EASTING, NORTHING, FLIP }
        private geoDatabaseUtility geoUtil = new geoDatabaseUtility();
        /// <summary>
        /// Creates an in Memory Raster given a raster dataset
        /// </summary>
        /// <param name="rsDset">IRasterDataset</param>
        /// <returns>IRaster</returns>
        public IRaster createRaster(IRasterDataset rsDset)
        {
            string cNm = rsDset.Format.ToLower();
            if (cNm.EndsWith("hdf4") || cNm.EndsWith("ntif"))
            {
                IRasterBandCollection rsBc = new RasterClass();
                IRasterDatasetJukebox rsDsetJu = (IRasterDatasetJukebox)rsDset;
                int subCnt = rsDsetJu.SubdatasetCount;
                for (int i = 0; i < subCnt; i++)
                {
                    rsDsetJu.Subdataset = i;
                    IRasterDataset subDset = (IRasterDataset)rsDsetJu;
                    rsBc.AppendBand(((IRasterBandCollection)subDset).Item(0));
                }
                return (IRaster)rsBc;

            }
            else
            {
                IRasterDataset3 rDset3 = (IRasterDataset3)rsDset;
                return rDset3.CreateFullRaster();
            }
        }
        /// <summary>
        /// Opens a raster dataset given a string path
        /// </summary>
        /// <param name="rasterPath">full path to a raster dataset</param>
        /// <returns>IRasterDataset</returns>
        public IRasterDataset openRasterDataset(string rasterPath,out string bnd)
        {
            IWorkspace wks = openRasterDatasetRec(rasterPath);
            string rstDir = wks.PathName;
            string rstName = rasterPath.Replace(rstDir, "").TrimStart(new char[]{'\\'});
            string[] rstNameSplit = rstName.Split(new char[] { '\\' });
            string dataSet = "";
            string rsDset = "";
            bnd = "all";
            switch (rstNameSplit.Length)
            {
                case 1:
                    rsDset = rstNameSplit[0];
                    break;
                case 2:
                    rsDset = rstNameSplit[0];
                    bnd = rstNameSplit[1];
                    break;
                default:
                    dataSet = rstNameSplit[0];
                    rsDset = rstNameSplit[1];
                    bnd = rstNameSplit[2];
                    string[] bndsp = bnd.Split(new char[]{'_'});
                    bnd = bndsp[bndsp.Length-1];
                    break;
            }
            IRasterDataset rstDset = null;
            if (wks.Type == esriWorkspaceType.esriLocalDatabaseWorkspace || wks.Type == esriWorkspaceType.esriRemoteDatabaseWorkspace)
            {
                IRasterWorkspaceEx rsWks = (IRasterWorkspaceEx)wks;
                rstDset = rsWks.OpenRasterDataset(rsDset);
            }
            else
            {
                if (System.IO.Path.GetExtension(wks.PathName).ToLower() == ".nc")
                {
                    INetCDFWorkspace rsWks = (INetCDFWorkspace)wks;
                    IMDWorkspace mdWks = (IMDWorkspace)wks;
                    NetCDFRasterDatasetName rsDsetName = new NetCDFRasterDatasetNameClass();
                    IMDRasterDatasetView rsDsetV = (IMDRasterDatasetView)rsDsetName;
                    string xDim, yDim, bandDim;
                    getDeminsions(rsWks, out xDim, out yDim, out bandDim);
                    rsDsetV.Variable = rsDset;
                    rsDsetV.XDimension = xDim;
                    rsDsetV.YDimension = yDim;
                    rsDsetV.BandDimension = bandDim;
                    rstDset = (IRasterDataset)mdWks.CreateView(rsDset, (IMDDatasetView)rsDsetV);
                }
                else
                {
                    IRasterWorkspace rsWks = (IRasterWorkspace)wks;
                    rstDset = rsWks.OpenRasterDataset(rsDset);
                }
            }
            return rstDset;

        }

        private void getDeminsions(INetCDFWorkspace rsWks, out string xDim, out string yDim, out string bandDim)
        {
            string lonDim = "x";
            string latDim = "y";
            xDim = null;
            yDim = null;
            IStringArray sArr = rsWks.GetDimensions();
            bandDim = sArr.get_Element(2);
            for (int i = 0; i < sArr.Count; i++)
            {
                string el = sArr.get_Element(i);
                string ell = el.ToLower();
                if (ell.Contains("lon")) lonDim = el;
                else if (ell.Contains("lat")) latDim = el;
                else if (ell.Contains("x")) xDim = el;
                else if (ell.Contains("y")) yDim = el;
            }
            if (xDim == null) xDim = lonDim;
            if (yDim == null) yDim = latDim;
        }

        private IWorkspace openRasterDatasetRec(string rasterPath)
        {
            IWorkspace wks = null;
            string pD = System.IO.Path.GetDirectoryName(rasterPath);
            //Console.WriteLine(pD);
            try
            {
                wks = geoUtil.OpenRasterWorkspace(pD);
                if (wks == null)
                {
                    if (pD.Length <2)
                    {
                        return wks;
                    }
                    wks = openRasterDatasetRec(pD);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return wks;
        }
        /// <summary>
        /// gets the number of cell counts for each value within the raster
        /// </summary>
        /// <param name="rst">IRaster, IRasterDataset, or full string path</param>
        /// <returns> a dictionary of cell counts by value</returns>
        public Dictionary<string, int> getCountsbyClass(object rst)
        {
            Dictionary<string, int> cnts = new Dictionary<string, int>();
            IRaster inrst = returnRaster(rst);
            IRaster2 rst2 = (IRaster2)inrst;
            if (rst2 ==null)
            {
                return cnts;
            }
            ICursor scur = rst2.AttributeTable.Search(null, false);
            int vlIndex = scur.Fields.FindField("VALUE");
            int cntIndex = scur.Fields.FindField("COUNT");

            IRow srow = scur.NextRow();
            while (srow != null)
            {
                string vl = srow.get_Value(vlIndex).ToString();
                int cnt = System.Convert.ToInt32(srow.get_Value(cntIndex));
                cnts.Add(vl, cnt);
                srow = scur.NextRow();
            }
            return cnts;
        }
        /// <summary>
        /// Creates a random point feature class with an equal number of sample for each class within the input raster.
        /// The feature class has 3 fields class, category, and weight which are used to identify the class that the sample point originated from,
        /// the category that point represents on the ground, and the weight that each sample caries (calculated as the count of class pixels / the mean of class pixels) 
        /// </summary>
        /// <param name="wks">The workspace to store the point feature class</param>
        /// <param name="rasterPath">the full path of the categorized raster</param>
        /// <param name="sampleSizePerClass">number of samples per class</param>
        /// <param name="numImages">the number of total images (tiles) used to create the full image picture</param>
        /// <returns>IFeatureClass</returns>
        public IFeatureClass createRandomSampleLocationsByClass(IWorkspace wks, object rasterPath, int[] sampleSizePerClass, int numImages, string outName)
        {
            IFeatureClass sampFtrCls = null;
            try
            {
                IRaster rst = returnRaster(rasterPath);
                IRaster2 rst2 = (IRaster2)rst;
                string pointName = "rndSmp_" + ((IDataset)rst2.RasterDataset).BrowseName;
                if (outName != null)
                {
                    pointName = outName;
                }
                Random rndGen = new Random();
                pointName = geoUtil.getSafeOutputNameNonRaster(wks, pointName);
                IRasterProps rstProps = (IRasterProps)rst2;
                int rWidth = rstProps.Width;
                int rHeight = rstProps.Height;
                int[] spcInt = new int[sampleSizePerClass.Length];
                for (int i = 0; i < sampleSizePerClass.Length; i++)
                {
                    int s = sampleSizePerClass[i]/numImages;
                    if (s == 0) s = 1;
                    spcInt[i] = s;
                }
                IFields flds = new FieldsClass();
                IFieldsEdit fldsE = (IFieldsEdit)flds;
                IField fld = new FieldClass();
                IFieldEdit fldE = (IFieldEdit)fld;
                fldE.Name_2 = "Value";
                fldE.Type_2 = esriFieldType.esriFieldTypeDouble;
                fldsE.AddField(fld);
                fld = new FieldClass();
                fldE = (IFieldEdit)fld;
                fldE.Name_2 = "CATEGORY";
                fldE.Type_2 = esriFieldType.esriFieldTypeString;
                fldE.Length_2 = 20;
                fldsE.AddField(fld);
                fld = new FieldClass();
                fldE = (IFieldEdit)fld;
                fldE.Name_2 = "WEIGHT";
                fldE.Type_2 = esriFieldType.esriFieldTypeDouble;
                fldsE.AddField(fld);
                sampFtrCls = geoUtil.createFeatureClass((IWorkspace2)wks, pointName, flds, esriGeometryType.esriGeometryPoint, rstProps.SpatialReference);
                Dictionary<string, int> classCnts = getCountsbyClass(rst);
                Dictionary<string, List<double[]>> xyList = new Dictionary<string, List<double[]>>();
                bool lookingForSamples = true;
                List<double[]> tCoor = new List<double[]>();
                List<string> checkList = new List<string>();
                List<string> selectRowsColums = new List<string>();
                while (lookingForSamples)
                {
                    int x = rndGen.Next(rWidth); //column
                    int y = rndGen.Next(rHeight); //row
                    //Console.WriteLine("Column = " + x.ToString() + " Row = " + y.ToString());
                    object vlTobject = rst2.GetPixelValue(0, x, y);
                    if(vlTobject==null)
                    {
                        continue;
                    }
                    else
                    {
                        string vl = vlTobject.ToString();
                        int vlint = System.Convert.ToInt32(vlTobject);
                        double xC = rst2.ToMapX(x);
                        double yC = rst2.ToMapY(y);
                        string tStr = x.ToString() + ";" + y.ToString();
                        double[] xy = { xC, yC };

                        if (xyList.TryGetValue(vl, out tCoor))
                        {
                            int spc=spcInt[0];
                            if (spcInt.Contains(vlint)) spc = spcInt[vlint];
                            if (tCoor.Count < spcInt[vlint] && !selectRowsColums.Contains(tStr))
                            {
                                tCoor.Add(xy);
                                selectRowsColums.Add(tStr);
                                xyList[vl] = tCoor;
                            }
                            else
                            {
                                if (!checkList.Contains(vl))
                                {
                                    checkList.Add(vl);
                                }
                                if (checkList.Count >= classCnts.Count)
                                {
                                    lookingForSamples = false;
                                }

                            } 
                        }
                        else
                        {
                            tCoor = new List<double[]>();
                            tCoor.Add(xy);
                            selectRowsColums.Add(tStr);
                            xyList.Add(vl, tCoor);
                        }
                    }
                }
                int classIndex = sampFtrCls.FindField("Value");
                int weightIndex = sampFtrCls.FindField("WEIGHT");
                double clAv = classCnts.Values.Average();
                foreach (KeyValuePair<string, List<double[]>> kVp in xyList)
                {
                    string ky = kVp.Key;
                    List<double[]> vl = kVp.Value;
                    foreach (double[] d in vl)
                    {
                        IFeature ftr = sampFtrCls.CreateFeature();
                        IGeometry geo = ftr.Shape;
                        IPoint pnt = (IPoint)geo;
                        pnt.PutCoords(d[0], d[1]);
                        ftr.set_Value(classIndex, System.Convert.ToDouble(ky));
                        ftr.set_Value(weightIndex,(classCnts[ky] / clAv));
                        ftr.Store();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.ToString());
            }

            return sampFtrCls;
        }
        /// <summary>
        /// Creates a random point feature class across the area of the image. The feature class has 1 field called value that contains the cell value at each sample location
        /// </summary>
        /// <param name="wks">the workspace to store the point feature class</param>
        /// <param name="rasterPath">the path to the raster</param>
        /// <param name="TotalSamples">the total number of samples</param>
        /// <returns>newly created IFeatureClass</returns>
        public IFeatureClass createRandomSampleLocations(IWorkspace wks, object rasterPath, int TotalSamples, string outName)
        {
            IFeatureClass sampFtrCls = null;
            try
            {
                IRaster rst = returnRaster(rasterPath);
                IRaster2 rst2 = (IRaster2)rst;
                string pointName = "rndSmp_" + ((IDataset)((IRasterBandCollection)rst).Item(0)).BrowseName;
                if (outName != null)
                {
                    pointName = outName;
                }
                pointName = geoUtil.getSafeOutputNameNonRaster(wks, pointName);
                Random rndGen = new Random();
                IFeatureWorkspace ftrWks = (IFeatureWorkspace)wks;
                IRasterProps rstProps = (IRasterProps)rst2;
                int rWidth = rstProps.Width;
                int rHeight = rstProps.Height;
                IFields flds = new FieldsClass();
                IFieldsEdit fldsE = (IFieldsEdit)flds;
                IField fld = new FieldClass();
                IFieldEdit fldE = (IFieldEdit)fld;
                fldE.Name_2 = "VALUE";
                fldE.Type_2 = esriFieldType.esriFieldTypeDouble;
                fldsE.AddField(fld);
                sampFtrCls = geoUtil.createFeatureClass((IWorkspace2)wks, pointName, flds, esriGeometryType.esriGeometryPoint, rstProps.SpatialReference);
                int checkSampleSize = 0;
                int classIndex = sampFtrCls.FindField("Value");
                while (checkSampleSize<TotalSamples)
                {
                    int x = rndGen.Next(rWidth);
                    int y = rndGen.Next(rHeight);
                    object vlT = rst2.GetPixelValue(0, x, y);
                    if (vlT==null)
                    {
                        continue;
                    }
                    else
                    {
                        double xC = rst2.ToMapX(x);
                        double yC = rst2.ToMapY(y);
                        IFeature ftr = sampFtrCls.CreateFeature();
                        IGeometry geo = ftr.Shape;
                        IPoint pnt = (IPoint)geo;
                        pnt.PutCoords(xC, yC);
                        try
                        {
                            ftr.set_Value(classIndex, vlT);
                            ftr.Store();
                            checkSampleSize++;
                        }
                        catch (Exception e)
                        {
                            //System.Windows.Forms.MessageBox.Show("Error:" + e.ToString());
                            Console.WriteLine(e.ToString());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.ToString());
            }
            return sampFtrCls;
        }
        /// <summary>
        /// deletes an existing raster dataset
        /// </summary>
        /// <param name="fullPath">the full path name of the raster dataset</param>
        public void deleteRasterDataset(string fullPath)
        {
            if (geoUtil.ftrExists(fullPath))
            {
                string bnd = "";
                IDataset dSet = (IDataset)openRasterDataset(fullPath,out bnd);
                if (dSet.CanDelete()) dSet.Delete();
            }
        }
        /// <summary>
        /// retrieves a IRaster for a given a raster band
        /// </summary>
        /// <param name="inRaster">template raster</param>
        /// <param name="index">band index zero based</param>
        /// <returns></returns>
        public IRaster getBand(object inRaster, int index)
        {
            IRaster rst = returnRaster(inRaster);
            if (rst != null)
            {
                IRasterBandCollection rsBc = new RasterClass();
                IRasterBandCollection rsBc2 = (IRasterBandCollection)rst;
                rsBc.AppendBand(rsBc2.Item(index));
                rst = (IRaster)rsBc;
            }
            return rst;
        }
        public IRaster reSampleRasterFunction(object inRaster, double cellSize,rstResamplingTypes typeResample = rstResamplingTypes.RSP_NearestNeighbor)
        {
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc = new GeometricFunctionClass();
            IGeometricFunctionArguments args = new GeometricFunctionArgumentsClass();
            IRaster inrs = returnRaster(inRaster);
            IRasterGeometryProc geoP = new RasterGeometryProcClass();
            geoP.Resample(typeResample, cellSize, inrs);
            IRaster2 rs = (IRaster2)inrs;
            args.Raster = returnRaster(inRaster);
            args.GeodataXform = rs.GeodataXform;
            frDset.Init(rsFunc, args);
            IRaster outRs = returnRaster((IRasterDataset)frDset);
            return outRs;
        }
        public IRaster reSizeRasterCellsFunction(object inRaster,int numCells)
        {
            IRaster inrs = returnRaster(inRaster);
            IRasterProps ps = (IRasterProps)inrs;
            double cellW = ps.MeanCellSize().X * numCells;
            return reSampleRasterFunction(inrs, cellW);
        }
        /// <summary>
        /// performs a x and y shift of the input raster
        /// </summary>
        /// <param name="inRaster">IRaster, IRasterDataset, string path</param>
        /// <param name="shiftX">number of cells to shift positive number move to the east negative number move to the west</param>
        /// <param name="shiftY">number of cells to shift positive number move north negative number move south</param>
        /// <returns></returns>
        public IRaster shiftRasterFunction(object inRaster, double shiftX, double shiftY)
        {
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc = new GeometricFunctionClass();
            //IGeometricXform geoX = new GeometricXformClass();
            IRaster inRs = returnRaster(inRaster);
            IRasterProps rsProp = (IRasterProps)inRs;
            IPnt pnt = rsProp.MeanCellSize();
            double mX = pnt.X;
            double mY = pnt.Y;
            double sX = mX * shiftX;
            double sY = mY * shiftY;
            IRasterGeometryProc3 rsGeoProc3 = new RasterGeometryProcClass();
            rsGeoProc3.Shift(sX, sY, inRs);
            IRaster2 rs = (IRaster2)inRs;
            IGeometricFunctionArguments args = new GeometricFunctionArgumentsClass();
            args.Raster = returnRaster(inRaster);
            args.GeodataXform = rs.GeodataXform;
            frDset.Init(rsFunc, args);
            IRaster outRs = returnRaster((IRasterDataset)frDset);
            return outRs;
        }
        public IRaster flipRasterFunction(object inRaster)
        {
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc = new GeometricFunctionClass();
            //IGeometricXform geoX = new GeometricXformClass();
            IRaster inRs = returnRaster(inRaster);
            IRasterGeometryProc3 rsGeoProc3 = new RasterGeometryProcClass();
            rsGeoProc3.Flip(inRs);
            IRaster2 rs = (IRaster2)inRs;
            IGeometricFunctionArguments args = new GeometricFunctionArgumentsClass();
            args.Raster = returnRaster(inRaster);
            args.GeodataXform = rs.GeodataXform;
            frDset.Init(rsFunc, args);
            IRaster outRs = returnRaster((IRasterDataset)frDset);
            return outRs;
        }
        public IRaster reprojectRasterFunction(object inRaster,ISpatialReference spatialReference)
        {
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc = new GeometricFunctionClass();
            //IGeometricXform geoX = new GeometricXformClass();
            IRaster inRs = returnRaster(inRaster);
            IRasterGeometryProc3 rsGeoProc3 = new RasterGeometryProcClass();
            object cellSize = Type.Missing;
            rsGeoProc3.ProjectFast(spatialReference,rstResamplingTypes.RSP_NearestNeighbor, ref cellSize,inRs);
            IRaster2 rs = (IRaster2)inRs;
            IGeometricFunctionArguments args = new GeometricFunctionArgumentsClass();
            args.Raster = returnRaster(inRaster);
            args.GeodataXform = rs.GeodataXform;
            frDset.Init(rsFunc, args);
            IRaster outRs = returnRaster((IRasterDataset)frDset);
            return outRs;
        }
        public IRaster RotateRasterFunction(object inRaster,double rotationAngle)
        {
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc = new GeometricFunctionClass();
            //IGeometricXform geoX = new GeometricXformClass();
            IRaster inRs = returnRaster(inRaster);
            IRasterProps rsP = (IRasterProps)inRs;
            IPoint pPoint = new PointClass();
            IEnvelope env = rsP.Extent;
            double hX = (env.Width/2)+env.XMin;
            double hY = (env.Height / 2) + env.YMin;
            pPoint.X = hX;
            pPoint.Y = hY;
            IRasterGeometryProc3 rsGeoProc3 = new RasterGeometryProcClass();
            rsGeoProc3.Rotate(pPoint,rotationAngle,inRs);
            IRaster2 rs = (IRaster2)inRs;
            IGeometricFunctionArguments args = new GeometricFunctionArgumentsClass();
            args.Raster = returnRaster(inRaster);
            args.GeodataXform = rs.GeodataXform;
            frDset.Init(rsFunc, args);
            IRaster outRs = returnRaster((IRasterDataset)frDset);
            return outRs;
        }
        public IRaster fastGLCMFunction(object inRaster, int radius, bool horizontal, glcmMetric glcmType)
        {
            FunctionRasters.NeighborhoodHelper.glcmHelperBase glcmB = new FunctionRasters.NeighborhoodHelper.glcmHelperBase();
            glcmB.RasterUtility = this;
            glcmB.InRaster = returnRaster(inRaster);
            glcmB.Radius = radius;
            glcmB.Horizontal = horizontal;
            glcmB.GlCM_Metric = glcmType;
            glcmB.WindowType = windowType.CIRCLE;
            glcmB.calcGLCM();
            return glcmB.OutRaster;
        }
        public IRaster fastGLCMFunction(object inRaster, int clms, int rws, bool horizontal, glcmMetric glcmType)
        {
            FunctionRasters.NeighborhoodHelper.glcmHelperBase glcmB = new FunctionRasters.NeighborhoodHelper.glcmHelperBase();
            glcmB.RasterUtility = this;
            glcmB.InRaster = returnRaster(inRaster);
            glcmB.Columns = clms;
            glcmB.Rows = rws;
            glcmB.Horizontal = horizontal;
            glcmB.GlCM_Metric = glcmType;
            glcmB.WindowType = windowType.RECTANGLE;
            glcmB.calcGLCM();
            return glcmB.OutRaster;
        }
        /// <summary>
        /// Performs GLMC Analysis. All bands within the input raster will be transformed
        /// </summary>
        /// <param name="inRaster">raster to perform GLCM</param>
        /// <param name="clms">number of Columns within the analysis window</param>
        /// <param name="rws">number of Rows within the analysis window</param>
        /// <param name="horizontal">whether the direction of the GLCM is horizontal</param>
        /// <param name="glcmType">the type of GLCM to calculate</param>
        /// <returns>a transformed raster</returns>
        public IRaster calcGLCMFunction(object inRaster, int clms, int rws, bool horizontal, glcmMetric glcmType)
        {
            IRaster iR1 = returnRaster(inRaster);
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc = null;
            switch (glcmType)
            {
                case glcmMetric.CONTRAST:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.glcmHelperContrast();
                    break;
                case glcmMetric.DIS:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.glcmHelperDissimilarity();
                    break;
                case glcmMetric.HOMOG:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.glcmHelperHomogeneity();
                    break;
                case glcmMetric.ASM:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.glcmHelperASM();
                    break;
                case glcmMetric.ENERGY:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.glcmHelperEnergy();
                    break;
                case glcmMetric.MAXPROB:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.glcmHelperMaxProb();
                    break;
                case glcmMetric.MINPROB:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.glcmHelperMinProb();
                    break;
                case glcmMetric.RANGE:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.glcmHelperRange();
                    break;
                case glcmMetric.ENTROPY:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.glcmHelperEntropy();
                    break;
                case glcmMetric.MEAN:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.glcmHelperMean();
                    break;
                case glcmMetric.VAR:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.glcmHelperVariance();
                    break;
                case glcmMetric.CORR:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.glcmHelperCorrelation();
                    break;
                case glcmMetric.COV:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.glcmHelperCovariance();
                    break;
                default:
                    break;
            }
            FunctionRasters.glcmFunctionArguments args = new FunctionRasters.glcmFunctionArguments(this);
            args.Columns = clms;
            args.Rows = rws;
            args.InRaster = iR1;
            args.Horizontal = horizontal;
            args.GLCMMETRICS = glcmType;
            frDset.Init(rsFunc, args);
            IRaster outRs = createRaster((IRasterDataset)frDset);
            return outRs;
        }
        /// <summary>
        /// Performs GLMC Analysis. All bands within the input raster will be transformed
        /// </summary>
        /// <param name="inRaster">raster to perform GLCM</param>
        /// <param name="radius">number of Columns that define the radius of the analysis window</param>
        /// <param name="horizontal">whether the direction of the GLCM is horizontal</param>
        /// <param name="glcmType">the type of GLCM to calculate</param>
        /// <returns>a transformed raster</returns>
        public IRaster calcGLCMFunction(object inRaster, int radius, bool horizontal, glcmMetric glcmType)
        {
            IRaster iR1 = returnRaster(inRaster);
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc = null;
            switch (glcmType)
	        {
		        case glcmMetric.CONTRAST:
                 rsFunc = new FunctionRasters.NeighborhoodHelper.glcmHelperContrast();
                 break;
                case glcmMetric.DIS:
                 rsFunc = new FunctionRasters.NeighborhoodHelper.glcmHelperDissimilarity();
                 break;
                case glcmMetric.HOMOG:
                 rsFunc = new FunctionRasters.NeighborhoodHelper.glcmHelperHomogeneity();
                 break;
                case glcmMetric.ASM:
                 rsFunc = new FunctionRasters.NeighborhoodHelper.glcmHelperASM();
                 break;
                case glcmMetric.ENERGY:
                 rsFunc = new FunctionRasters.NeighborhoodHelper.glcmHelperEnergy();
                 break;
                case glcmMetric.MAXPROB:
                 rsFunc = new FunctionRasters.NeighborhoodHelper.glcmHelperMaxProb();
                 break;
                case glcmMetric.MINPROB:
                 rsFunc = new FunctionRasters.NeighborhoodHelper.glcmHelperMinProb();
                 break;
                case glcmMetric.RANGE:
                 rsFunc = new FunctionRasters.NeighborhoodHelper.glcmHelperRange();
                 break;
                case glcmMetric.ENTROPY:
                 rsFunc = new FunctionRasters.NeighborhoodHelper.glcmHelperEntropy();
                 break;
                case glcmMetric.MEAN:
                 rsFunc = new FunctionRasters.NeighborhoodHelper.glcmHelperMean();
                 break;
                case glcmMetric.VAR:
                 rsFunc = new FunctionRasters.NeighborhoodHelper.glcmHelperVariance();
                 break;
                case glcmMetric.CORR:
                 rsFunc = new FunctionRasters.NeighborhoodHelper.glcmHelperCorrelation();
                 break;
                case glcmMetric.COV:
                 rsFunc = new FunctionRasters.NeighborhoodHelper.glcmHelperCovariance();
                 break;
                default:
                 break;
	        }
            FunctionRasters.glcmFunctionArguments args = new FunctionRasters.glcmFunctionArguments(this);
            args.Radius = radius;
            args.InRaster = iR1;
            args.Horizontal = horizontal;
            args.GLCMMETRICS = glcmType;
            frDset.Init(rsFunc, args);
            IRaster outRs = createRaster((IRasterDataset)frDset);
            return outRs;
        }
        /// <summary>
        /// performs a convolution analysis for a defined kernel
        /// </summary>
        /// <param name="inRaster"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="kn"></param>
        /// <returns></returns>
        public IRaster convolutionRasterFunction(object inRaster, int width, int height, double[] kn)
        {
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc = new ConvolutionFunctionClass();
            IRaster rs = returnRaster(inRaster,rstPixelType.PT_FLOAT);
            IConvolutionFunctionArguments args = new ConvolutionFunctionArgumentsClass();
            args.Raster = rs;
            args.Rows = width;
            args.Columns = height;
            IDoubleArray dbArry = new DoubleArrayClass();
            foreach (double d in kn)
            {
                dbArry.Add(d);
            }
            args.Kernel = dbArry;
            args.Type = esriRasterFilterTypeEnum.esriRasterFilterUserDefined;
            frDset.Init(rsFunc, args);
            IRaster outRs = returnRaster((IRasterDataset)frDset,rstPixelType.PT_FLOAT);
            if (width > 3 || height > 3)
            {
                IRasterProps rsProp = (IRasterProps)outRs;
                IEnvelope env = rsProp.Extent;
                IRasterGeometryProc3 rsGeoProc3 = new RasterGeometryProcClass();
                int addY = 0;
                int addX = 0;
                if (width > 3)
                {
                    addX = (((width + 1) / 2) - 2);
                    
                }

                if (height > 3)
                {
                    addY = -1 * (((height + 1) / 2) - 2);
                    
                }
                outRs = shiftRasterFunction(outRs, addX, addY);
            }
            double cells = getNFromKernal(kn);
            //functionModel.estimateStatistics(rs, outRs, focalType.SUM, cells);
            return outRs;
            
        }

        private double getNFromKernal(double[] kn)
        {
            double n = 0;
            for (int i = 0; i < kn.Length; i++)
            {
                double vl = kn[i];
                n += vl;
            }
            return n;
        }
        /// <summary>
        /// gets the actual cell size of the input raster as opposed to the average cell size
        /// </summary>
        /// <param name="rs"></param>
        /// <returns></returns>
        public IPnt getCellSize(IRaster rs)
        {
            IRasterProps rsProps = (IRasterProps)rs;
            IEnvelope env = rsProps.Extent;
            double w = (env.XMax - env.XMin) / rsProps.Width;
            double h = (env.YMax - env.YMin) / rsProps.Height;
            IPnt pnt = new PntClass();
            pnt.SetCoords(w, h);
            return pnt;
        }
        /// <summary>
        /// Calculates a transform a raster values to a different value via tranType 
        /// </summary>
        /// <param name="inRaster"></param>
        /// <param name="typ"></param>
        /// <returns></returns>
        public IRaster calcMathRasterFunction(object inRaster, transType typ)
        {
            IRaster rRst = returnRaster(inRaster);
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc = null;
            switch (typ)
            {
                case transType.LOG10:
                    rsFunc = new FunctionRasters.log10FunctionDataset();
                    break;
                case transType.LN:
                    rsFunc = new FunctionRasters.logFunctionDataset();
                    break;
                case transType.EXP:
                    rsFunc = new FunctionRasters.expFunctionDataset();
                    break;
                case transType.EXP10:
                    rsFunc = new FunctionRasters.exp10FunctionDataset();
                    break;
                case transType.SIN:
                    rsFunc = new FunctionRasters.sinFunctionDataset();
                    break;
                case transType.COS:
                    rsFunc = new FunctionRasters.cosFunctionDataset();
                    break;
                case transType.TAN:
                    rsFunc = new FunctionRasters.tanFunctionDataset();
                    break;
                case transType.ASIN:
                    rsFunc = new FunctionRasters.asinFunctionDataset();
                    break;
                case transType.ACOS:
                    rsFunc = new FunctionRasters.acosFunctionDataset();
                    break;
                case transType.ATAN:
                    rsFunc = new FunctionRasters.atanFunctionDataset();
                    break;
                case transType.RADIANS:
                    rsFunc = new FunctionRasters.radiansFunctionDataset();
                    break;
                case transType.SQRT:
                    rsFunc = new FunctionRasters.sqrtFunctionDataset();
                    break;
                case transType.SQUARED:
                    rsFunc = new FunctionRasters.squaredFunctionDataset();
                    break;
                default:
                    rsFunc = new FunctionRasters.absFunctionDataset();
                    break;
            }
            FunctionRasters.MathFunctionArguments args = new FunctionRasters.MathFunctionArguments(this);
            args.InRaster = rRst;
            frDset.Init(rsFunc, args);
            IRaster outRs = createRaster((IRasterDataset)frDset);
            functionModel.estimateStatistics(rRst, outRs, typ);
            return outRs;
        }
        /// <summary>
        /// Creates a new raster dataset based on the template Raster. If a raster with the same outRaster name exist it will be overwritten
        /// </summary>
        /// <param name="templateRaster">a raster that has the size and shape desired</param>
        /// <param name="outWks">the output workspace</param>
        /// <param name="outRasterName">the name of the raster</param>
        /// <param name="numBands">the number of raster bands</param>
        /// <param name="pixelType">the pixel type</param>
        /// <returns></returns>
        public IRaster createNewRaster(IRaster templateRaster, IWorkspace outWks, string outRasterName, int numBands, rstPixelType pixelType)
        {
            outRasterName = getSafeOutputName(outWks, outRasterName);
            IRaster rsT = returnRaster(getBand(templateRaster,0),pixelType);
            IRaster rs = constantRasterFunction(rsT, getNoDataValue(pixelType));
            IRasterBandCollection rsbC = new RasterClass();
            for (int i = 0; i < numBands; i++)
            {
                rsbC.AppendBands((IRasterBandCollection)rs);
            }
            string outTypeStr = rasterUtil.rasterType.IMAGINE.ToString();
            if (outWks.Type != esriWorkspaceType.esriFileSystemWorkspace)
            {
                outTypeStr = rasterType.GDB.ToString();
            }
            return returnRaster(saveRasterToDataset((IRaster)rsbC, outRasterName, outWks));
        }
        /// <summary>
        /// Creates a new raster dataset based on the template Raster. If a raster with the same outRaster name exist it will be overwritten
        /// </summary>
        /// <param name="templateRaster">a raster that has the size and shape desired</param>
        /// <param name="outWks">the output workspace</param>
        /// <param name="outRasterName">the name of the raster</param>
        /// <param name="numBands">the number of raster bands</param>
        /// <param name="pixelType">the pixel type</param>
        /// <param name="env">the extent</param>
        /// <param name="meanCellSize"> the mean Cell Size of the new raster</param>
        /// <param name="spRf"> the spatial reference of the raster</param>
        /// <returns></returns>
        public IRaster createNewRaster(IEnvelope env, IPnt meanCellSize,IWorkspace outWks, string outRasterName, int numBands, rstPixelType pixelType, ISpatialReference spRf)
        {
            outRasterName = getSafeOutputName(outWks, outRasterName);
            IRasterDataset3 newRstDset = null;
            IRaster rs = null;
            if (outWks.Type == esriWorkspaceType.esriFileSystemWorkspace)
            {
                if (!outRasterName.ToLower().EndsWith(".img"))
                {
                    outRasterName = outRasterName + ".img";
                }
                double dX = meanCellSize.X;
                double dY = meanCellSize.Y;
                IRasterWorkspace2 rsWks = (IRasterWorkspace2)outWks;
                newRstDset = (IRasterDataset3)rsWks.CreateRasterDataset(outRasterName, "IMAGINE Image", env.LowerLeft, System.Convert.ToInt32(env.Width / dX), System.Convert.ToInt32(env.Height / dY), dX, dY, numBands, pixelType, spRf, true);
                rs = newRstDset.CreateFullRaster();
            }
            else
            {
                IRasterWorkspaceEx rsWks = (IRasterWorkspaceEx)outWks;
                IRasterDef rsDef = new RasterDefClass();
                IRasterStorageDef rsStDef = new RasterStorageDefClass();
                rsStDef.Origin = env.LowerLeft;
                rsStDef.TileHeight = 128;
                rsStDef.TileWidth = 128;
                rsStDef.CellSize = meanCellSize;
                rsDef.SpatialReference = spRf;
                newRstDset = (IRasterDataset3)rsWks.CreateRasterDataset(outRasterName, numBands, pixelType, rsStDef, null, rsDef, null);
                rs = newRstDset.CreateFullRaster();
                IRasterProps rsPr = (IRasterProps)rs;
                rsPr.Height = System.Convert.ToInt32(env.Height);
                rsPr.Width = System.Convert.ToInt32(env.Width);
                rsPr.Extent = env;
            }
            return rs;
        }
        /// <summary>
        /// creates a rectangle folcal window that can be used to lookup values
        /// </summary>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <returns></returns>
        public int[,] createFocalWindowRectangle(int Width, int Height, out List<int[]> iter)
        {
            return createFocalWindow(Width, Height, windowType.RECTANGLE, out iter);
        }
        /// <summary>
        /// creates a circle folcal window that can be used to lookup values
        /// </summary>
        /// <param name="Radius"></param>
        /// <returns></returns>
        public int[,] createFocalWindowCircle(int Radius, out List<int[]> iter)
        {
            int Width = ((Radius - 1) * 2) + 1;
            return createFocalWindow(Width, Width, windowType.CIRCLE, out iter);
        }
        /// <summary>
        /// creates a focal window array of 0 and 1 that can represent a circle or rectangle
        /// </summary>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="WindowType"></param>
        /// <returns></returns>
        public static int[,] createFocalWidow(int Width, int Height, windowType WindowType, out List<int[]> iter)
        {
            iter = new List<int[]>();
            int[,] xAr = new int[Width, Height];
            int x = 0;
            int y = 0;
            switch (WindowType)
            {
                case windowType.CIRCLE:
                    int radius = ((Width - 1) / 2);
                    for (y = 0; y < Height; y++)
                    {
                        for (x = 0; x < Width; x++)
                        {
                            double cD = Math.Sqrt(Math.Pow((x - radius), 2) + Math.Pow((y - radius), 2));
                            if (cD <= (radius))
                            {
                                xAr[x, y] = 1;
                                iter.Add(new int[] { x, y });
                            }
                            else
                            {
                                xAr[x, y] = 0;
                            }
                        }
                    }
                    break;
                default:
                    for (y = 0; y < Height; y++)
                    {
                        for (x = 0; x < Width; x++)
                        {
                            xAr[x, y] = 1;
                            iter.Add(new int[] { x, y });
                        }
                    }
                    break;
            }
            return xAr;
        }
        public void createFocalWindowRectangleGLCM(int Width, int Height, bool horizontal, out List<int[]> iter)
        {
            createFocalWindowGLCM(Width, Height, windowType.RECTANGLE, horizontal, out iter);
        }
        public void createFocalWindowCircleGLCM(int Radius, bool horizontal, out List<int[]> iter)
        {
            int Width = ((Radius - 1) * 2) + 1;
            createFocalWindowGLCM(Width, Width, windowType.CIRCLE, horizontal, out iter);
        }
        private void createFocalWindowGLCM(int Width, int Height, windowType WindowType, bool horizontal,out List<int[]> iter)//iter = x,y,weight,getNeightbor 1 or 0
        {

            iter = new List<int[]>();
            int x = 0;
            int y = 0;
            int h = 0;
            int w = Width;
            if (horizontal)
            {
                w = w - 1;
            }
            else
            {
                h = 1;
            }
            switch (WindowType)
            {
                case windowType.CIRCLE:
                    int radius = ((Width - 1) / 2);
                    for (y = h; y < Height; y++)
                    {
                        for (x = 0; x < w; x++)
                        {
                            double cD = Math.Sqrt(Math.Pow((x - radius), 2) + Math.Pow((y - radius), 2));
                            if (cD <= (radius))
                            {
                                int gN = 0;
                                if(horizontal)
                                {
                                    double cDn = Math.Sqrt(Math.Pow(((x+1) - radius), 2) + Math.Pow((y - radius), 2));
                                    if (cDn <= radius)
                                    {
                                        gN = 1;
                                    }

                                }
                                else
                                {
                                    double cDn = Math.Sqrt(Math.Pow((x - radius), 2) + Math.Pow(((y-1) - radius), 2));
                                    if (cDn <= radius)
                                    {
                                        gN = 1;
                                    }
                                }
                                iter.Add(new int[] { x, y, gN });
                            }
                            
                        }
                    }
                    break;
                default:
                    for (y = h; y < Height; y++)
                    {
                        for (x = 0; x < w; x++)
                        {
                            int gN = 0;
                            if (horizontal)
                            {
                                if ((x+1) <= Width-1)
                                {
                                    gN = 1;
                                }

                            }
                            else
                            {
                                if ((y-1) >= 0)
                                {
                                    gN = 1;
                                }
                            }
                            iter.Add(new int[] { x, y, gN });
                        }
                    }
                    break;
            }
            return;
        }
        private int[,] createFocalWindow(int Width, int Height, windowType WindowType,out List<int[]> iter)
        {
            iter = new List<int[]>();
            int[,] xAr = new int[Width, Height];
            int x = 0;
            int y = 0;
            switch (WindowType)
            {
                case windowType.CIRCLE:
                    int radius = ((Width - 1) / 2);
                    for (y = 0; y < Height; y++)
                    {
                        for (x = 0; x < Width; x++)
                        {
                            double cD = Math.Sqrt(Math.Pow((x - radius), 2) + Math.Pow((y - radius), 2));
                            if (cD <= (radius))
                            {
                                xAr[x, y] = 1;
                                iter.Add(new int[] { x, y });
                            }
                            else
                            {
                                xAr[x, y] = 0;
                            }
                        }
                    }
                    break;
                default:
                    for (y = 0; y < Height; y++)
                    {
                        for (x = 0; x < Width; x++)
                        {
                            xAr[x, y] = 1;
                            iter.Add(new int[] { x, y });
                        }
                    }
                    break;
            }
            return xAr;

        }
        /// <summary>
        /// Will perform an arithmeticOperation on an input raster all bands
        /// </summary>
        /// <param name="inRaster1">either IRaster, IRasterDataset, or a valid path pointing to a raster</param>
        /// <param name="inRaster2">either IRaster, IRasterDataset, a numeric value, or a valid path pointing to a raster</param>
        /// <param name="op">the type of operation</param>
        /// <returns>a IRaster that can be used for further analysis</returns>
        public IRaster calcArithmaticFunction(object inRaster1, object inRaster2, esriRasterArithmeticOperation op, rstPixelType outRasterType = rstPixelType.PT_FLOAT)
        {
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc = new ArithmeticFunctionClass();
            IArithmeticFunctionArguments args = new ArithmeticFunctionArgumentsClass();
            if(isNumeric(inRaster1.ToString())&&isNumeric(inRaster2.ToString()))
            {
                Console.WriteLine("Must have at least one raster");
                return null;
            }
            args.Operation = op;
            object iR1, iR2;
            if (isNumeric(inRaster1.ToString())&&!isNumeric(inRaster2.ToString()))
            {
                iR2 = returnRaster(inRaster2);
                IScalar sc = new ScalarClass();
                int bCnt = ((IRasterBandCollection)iR2).Count;
                float[] d = new float[bCnt];
                for (int i = 0; i < bCnt; i++)
                {
                    d[i] = System.Convert.ToSingle(inRaster1);
                }
                sc.Value = d;
                iR1 = sc;
            }
            else if (isNumeric(inRaster2.ToString()) && !isNumeric(inRaster1.ToString()))
            {
                iR1 = returnRaster(inRaster1);
                IScalar sc = new ScalarClass();
                int bCnt = ((IRasterBandCollection)iR1).Count;
                float[] d = new float[bCnt];
                for (int i = 0; i < bCnt; i++)
                {
                    d[i] = System.Convert.ToSingle(inRaster2);
                }
                sc.Value = d;
                iR2 = sc;
            }
            else
            {
                iR1 = returnRaster(inRaster1);
                iR2 = returnRaster(inRaster2);
                IRasterBandCollection rsBc1 = (IRasterBandCollection)iR1;
                IRasterBandCollection rsBc2 = (IRasterBandCollection)iR2;
                int bCnt1,bCnt2;
                bCnt1 = rsBc1.Count;
                bCnt2 = rsBc2.Count;
                if (bCnt1 != rsBc2.Count)
                {
                    int dif = bCnt1-bCnt2;
                    int absDif = Math.Abs(dif);
                    if (dif > 0)
                    {
                        for (int i = 0; i < absDif; i++)
                        {
                            IRaster rs = getBand(iR2, 0);
                            rsBc2.AppendBands((IRasterBandCollection)rs);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < absDif; i++)
                        {
                            IRaster rs = getBand(iR1, 0);
                            rsBc1.AppendBands((IRasterBandCollection)rs);
                        }
                    }
                }
            }
            args.Raster = iR1;
            args.Raster2 = iR2;
            frDset.Init(rsFunc, args);
            IRaster outRs = returnRaster((IRasterDataset)frDset,outRasterType);
            //functionModel.estimateStatistics(iR1, iR2, outRs, op);
            return outRs;
            
            
        }
        public IRasterDataset saveRasterToDataset(IRaster inRaster, string outName, IWorkspace wks,rasterType rastertype)
        {
            string rsTypeStr = rastertype.ToString();
            string ext = "";
            if (rastertype== rasterType.IMAGINE)
            {
                rsTypeStr = "IMAGINE Image";
                ext = ".img";
            }
            else if (rastertype == rasterType.HDF4)
            {
                ext = ".hdf";
            }
            else if (rastertype == rasterType.ENV)
            {
                ext = ".hdr";
            }
            else
            {
                ext = "." + rastertype.ToString().ToLower();
            }
            esriWorkspaceType tp = wks.Type;
            if (tp == esriWorkspaceType.esriLocalDatabaseWorkspace)
            {
                rsTypeStr = rasterType.GDB.ToString();
            }
            if (rastertype == rasterType.GRID || rastertype == rasterType.GDB)
            {
                outName = getSafeOutputName(wks, outName);
                if (outName.Length > 12)
                {
                    outName.Substring(12);
                }
                if ((rastertype==rasterType.GRID)&&(((IRasterProps)inRaster).PixelType == rstPixelType.PT_FLOAT))
                {
                    inRaster = convertToDifFormatFunction(inRaster, rstPixelType.PT_FLOAT);
                }
            }
            else
            {
                if (outName.IndexOf(ext) == -1)
                {
                    outName = outName + ext;
                }

            }
            if (geoUtil.ftrExists(wks, outName))
            {
                deleteRasterDataset(wks.PathName + "\\" + outName);
            }
            IRasterDataset rsDset = null;
            try
            {
                
                ISaveAs sv = (ISaveAs)inRaster;
                rsDset = (IRasterDataset)sv.SaveAs(outName, wks, rsTypeStr);
                
                IRaster2 rs2 = (IRaster2)calcStatsAndHist(rsDset);
                ITable vat = rs2.AttributeTable;
                int rwCnt = 0;
                try
                {
                    rwCnt = vat.RowCount(null);
                }
                catch
                {
                    rwCnt = 0;
                }
                if (rwCnt > 0)
                {
                    IRasterDatasetEdit2 rsDsetE = (IRasterDatasetEdit2)rsDset;
                    rsDsetE.DeleteAttributeTable();
                    if (((IRasterBandCollection)rs2).Count == 1)
                    {
                        rsDsetE.BuildAttributeTable();
                    }
                }
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.ToString());
                Console.WriteLine(e.ToString());
                rsDset = ((IRaster2)returnRaster(wks.PathName + "\\" + outName)).RasterDataset;
            }
            return rsDset;
        }
        /// <summary>
        /// Save a raster to specified dataset
        /// </summary>
        /// <param name="inRaster"></param>
        /// <param name="outName"></param>
        /// <param name="wks"></param>
        /// <returns></returns>
        /// 
        public IRasterDataset saveRasterToDataset(IRaster inRaster, string outName, IWorkspace wks)
        {
            rasterType rsType = rasterType.GDB;
            esriWorkspaceType tp = wks.Type;
            if (tp == esriWorkspaceType.esriFileSystemWorkspace)
            {
                rsType = rasterType.IMAGINE;
            }
            return saveRasterToDataset(inRaster, outName, wks, rsType);
        }
        /// <summary>
        /// calculates stats and histogram for rasters
        /// </summary>
        /// <param name="rs"></param>
        public IRaster calcStatsAndHist(IRaster rs)
        {
            return calcStatsAndHist(rs, 1);
        }
        public IRaster calcStatsAndHist(IRaster rs, int skipFactor)
        {
            IRaster outRs = null;
            try
            {
                IRaster2 rs2 = (IRaster2)rs;
                IRasterDataset rsDset = rs2.RasterDataset;
                IRasterDataset3 rsDset3 = (IRasterDataset3)rsDset;
                IRasterDatasetEdit3 rsDset3e = (IRasterDatasetEdit3)rsDset3;
                rsDset3e.DeleteStats();
                rsDset3e.ComputeStatisticsHistogram(skipFactor, skipFactor, new double[] { }, false);
                outRs = rsDset3.CreateFullRaster();
            }
            catch
            {
                try
                {
                    IRasterBandCollection rsBc = (IRasterBandCollection)rs;
                    for (int i = 0; i < rsBc.Count; i++)
                    {
                        IRasterBand rsB = rsBc.Item(i);
                        bool hasStats = true;
                        rsB.HasStatistics(out hasStats);
                        if (hasStats)
                        {
                            IRasterStatistics rsStats = rsB.Statistics;
                            rsStats.SkipFactorX = skipFactor;
                            rsStats.SkipFactorY = skipFactor;
                            rsStats.Recalculate();
                        }
                        else
                        {
                            rsB.ComputeStatsAndHist();
                        }
                    }
                    outRs = rs;
                }
                catch (Exception e)
                {
                    outRs = rs;
                    Console.WriteLine(e.ToString());
                }
            }
            return outRs;
            
        }
        public IRaster calcStatsAndHist(IRasterDataset rsDset)
        {
            IRaster rs = returnRaster(rsDset);
            rs = calcStatsAndHist(rs);
            return rs;
        }
        /// <summary>
        /// Rescales raster to 8 byte unsigned integer 0-256
        /// </summary>
        /// <param name="inRaster"></param>
        /// <returns></returns>
        public IRaster reScaleRasterFunction(object inRaster)
        {
            return reScaleRasterFunction(inRaster, rstPixelType.PT_UCHAR);
        }
        /// <summary>
        /// Rescales raster to a given raster pixel type min max value
        /// </summary>
        /// <param name="inRaster"></param>
        /// <returns></returns>
        public IRaster reScaleRasterFunction(object inRaster, rstPixelType pType)
        {
            return reScaleRasterFunction(inRaster,pType,esriRasterStretchType.esriRasterStretchMinimumMaximum);
        }
        public IRaster reScaleRasterFunction(object inRaster,rstPixelType pType,esriRasterStretchType stretchType)
        {
            IRaster iR1 = returnRaster(inRaster);
            calcStatsAndHist(iR1);
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc = new StretchFunction();
            rsFunc.PixelType = pType;
            
            //IRasterProps rsProps = (IRasterProps)iR1;
            //rsFunc.RasterInfo.Extent = rsProps.Extent;
            //rsFunc.RasterInfo.CellSize = getCellSize(iR1);
            IStretchFunctionArguments args = new StretchFunctionArgumentsClass();
            args.Raster = iR1;
            args.StretchType = stretchType;
            frDset.Init(rsFunc, args);
            //frDset.Simplify();
            IRaster rs = createRaster((IRasterDataset)frDset);
            return rs;


        }
        public IRaster calcFocalSampleFunction(object inRaster, HashSet<string> offset, focalType statType)
        {
            IRaster iR1 = returnRaster(inRaster);
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc = null;
            switch (statType)
            {
                case focalType.MIN:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.focalSampleHelperMin();
                    break;
                case focalType.SUM:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.focalSampleHelperSum();
                    break;
                case focalType.MEAN:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.focalSampleHelperMean();
                    break;
                case focalType.MODE:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.focalSampleHelperMode();
                    break;
                case focalType.MEDIAN:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.focalSampleHelperMedian();
                    break;
                case focalType.VARIANCE:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.focalSampleHelperVariance();
                    break;
                case focalType.STD:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.focalSampleHelperStd();
                    break;
                case focalType.UNIQUE:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.focalSampleHelperUnique();
                    break;
                case focalType.ENTROPY:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.focalSampleHelperEntropy();
                    break;
                case focalType.ASM:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.focalSampleHelperASM();
                    break;
                default:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.focalSampleHelperMax();
                    break;
            }
            FunctionRasters.focalSampleArguments args = new FunctionRasters.focalSampleArguments(this);
            args.OffSets = offset;
            args.Operation = statType;
            //args.WindowType = windowType.RECTANGLE;
            args.InRaster = iR1;
            frDset.Init(rsFunc, args);
            IRaster outRs = createRaster((IRasterDataset)frDset);
            return outRs;

        }
        /// <summary>
        /// Will perform a focal raster operation on an input raster all bands
        /// </summary>
        /// <param name="inRaster">either IRaster, IRasterDataset, or a valid path pointing to a raster</param>
        /// <param name="clm">number of columns (cells)</param>
        /// <param name="rws">number of rows</param>
        /// <param name="statType">the type of operation</param>
        /// <returns>a IRaster that can be used for further analysis</returns>
        public IRaster calcFocalStatisticsFunction(object inRaster, int clm, int rws, focalType statType)
        {
            IRaster iR1 = returnRaster(inRaster);
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc = null;
            switch (statType)
            {
                case focalType.MIN:
                case focalType.MAX:
                case focalType.MEAN:
                case focalType.STD:
                    return calcFocalStatisticsRectangle(iR1, clm, rws, statType);
                case focalType.SUM:
                    IRaster mRs = calcFocalStatisticsFunction(iR1, clm, rws, focalType.MEAN);
                    return calcArithmaticFunction(mRs, clm * rws, esriRasterArithmeticOperation.esriRasterMultiply);
                case focalType.MODE:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.focalHelperMode();
                    break;
                case focalType.MEDIAN:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.focalHelperMedian();
                    break;
                case focalType.VARIANCE:
                    IRaster rs = calcFocalStatisticsFunction(iR1, clm, rws,focalType.STD);
                    return calcArithmaticFunction(rs,2,esriRasterArithmeticOperation.esriRasterPower);
                case focalType.UNIQUE:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.focalHelperUnique();
                    break;
                case focalType.ENTROPY:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.focalHelperEntropy();
                    break;
                case focalType.ASM:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.focalHelperProbability();
                    break;
                default:
                    break;
            }
            FunctionRasters.FocalFunctionArguments args = new FunctionRasters.FocalFunctionArguments(this);
            args.Rows = rws;
            args.Columns = clm;
            //args.WindowType = windowType.RECTANGLE;
            args.InRaster = iR1;
            args.Operation = statType;
            frDset.Init(rsFunc, args);
            IRaster outRs = createRaster((IRasterDataset)frDset);
            double cells  = clm*rws;
            //functionModel.estimateStatistics(iR1, outRs, statType, cells);
            return outRs;
        }

        private IRaster calcFocalStatisticsRectangle(IRaster iR1, int clm, int rws, focalType statType)
        {
            //Console.WriteLine(statType);
            //Console.WriteLine((esriFocalStatisticType)statType);
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc = new StatisticsFunctionClass();
            IStatisticsFunctionArguments args = new StatisticsFunctionArgumentsClass();
            args.Raster = convertToDifFormatFunction(iR1, rstPixelType.PT_FLOAT);
            args.Columns = clm;
            args.Rows = rws; 
            args.Type = (esriFocalStatisticType)statType;
            frDset.Init(rsFunc, args);
            IRaster outRs = returnRaster((IRasterDataset)frDset,rstPixelType.PT_FLOAT);
            return outRs;
        }
        /// <summary>
        /// Will perform a focal raster operation on an input raster all bands
        /// </summary>
        /// <param name="inRaster">either IRaster, IRasterDataset, or a valid path pointing to a raster</param>
        /// <param name="radius">number of cells that make up the radius of a circle</param>
        /// <param name="statType">the type of opporation</param>
        /// <returns>a IRaster that can be used for further analysis</returns>
        public IRaster calcFocalStatisticsFunction(object inRaster, int radius, focalType statType)
        {
            IRaster iR1 = returnRaster(inRaster);
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc = null;
            List<int[]> outLst = new List<int[]>();
            int[,] crl = null;
            double[] cArr = null;
            double sumCircle = 0;
            switch (statType)
            {
                case focalType.MIN:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.focalHelperMin();
                    break;
                case focalType.SUM:
                    crl = createFocalWindowCircle(radius, out outLst);
                    cArr = (from int i in crl select System.Convert.ToDouble(i)).ToArray();
                    return convolutionRasterFunction(iR1,crl.GetUpperBound(0)+1,crl.GetUpperBound(1)+1,cArr);
                case focalType.MEAN:
                    crl = createFocalWindowCircle(radius, out outLst);
                    sumCircle = (from int i in crl select System.Convert.ToDouble(i)).Sum();
                    cArr = (from int i in crl select System.Convert.ToDouble(i)).ToArray();
                    IRaster conRsMean = convolutionRasterFunction(iR1,crl.GetUpperBound(0)+1,crl.GetUpperBound(1)+1,cArr);
                    return calcArithmaticFunction(conRsMean, sumCircle, esriRasterArithmeticOperation.esriRasterDivide);
                case focalType.MODE:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.focalHelperMode();
                    break;
                case focalType.MEDIAN:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.focalHelperMedian();
                    break;
                case focalType.VARIANCE:
                    crl = createFocalWindowCircle(radius, out outLst);
                    cArr = (from int i in crl select System.Convert.ToDouble(i)).ToArray();
                    double sumCr = cArr.Sum();
                    IRaster rs2 = calcMathRasterFunction(iR1, transType.SQUARED);
                    IRaster sumRs2 = convolutionRasterFunction(rs2,crl.GetUpperBound(0)+1,crl.GetUpperBound(1)+1,cArr);
                    IRaster sumRs2M = calcArithmaticFunction(sumRs2, sumCr, esriRasterArithmeticOperation.esriRasterDivide);
                    IRaster sumRs = convolutionRasterFunction(iR1, crl.GetUpperBound(0) + 1, crl.GetUpperBound(1) + 1, cArr);
                    IRaster sumRsSquared = calcMathRasterFunction(sumRs, transType.SQUARED);
                    IRaster difRs = calcArithmaticFunction(sumRsSquared, sumRs2, esriRasterArithmeticOperation.esriRasterMinus);
                    return calcArithmaticFunction(difRs, sumCr, esriRasterArithmeticOperation.esriRasterDivide);
                case focalType.STD:
                    IRaster var = calcFocalStatisticsFunction(iR1, radius, focalType.VARIANCE);
                    return calcMathRasterFunction(var, transType.SQRT);
                case focalType.UNIQUE:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.focalHelperUnique();
                    break;
                case focalType.ENTROPY:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.focalHelperEntropy();
                    break;
                case focalType.ASM:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.focalHelperProbability();
                    break;
                default:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.focalHelperMax();
                    break;
            }
            FunctionRasters.FocalFunctionArguments args = new FunctionRasters.FocalFunctionArguments(this);
            args.Radius = radius;
            args.InRaster = iR1;
            args.Operation = statType;
            frDset.Init(rsFunc, args);
            IRaster outRs = createRaster((IRasterDataset)frDset);
            double cells = getNFromCircle(radius);
            return outRs;
        }
        private double getNFromCircle(int radius)
        {
            int n = 0;
            List<int[]> iterLst = new List<int[]>();
            int[,] kern = createFocalWindowCircle(radius, out iterLst);
            for (int i = 0; i <= kern.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= kern.GetUpperBound(1); j++)
                {
                    int vl = kern[i, j];
                    n += vl;
                }
            }
            return n;
        }
        /// <summary>
        /// Will perform a focal raster operation on an input raster all bands
        /// </summary>
        /// <param name="inRaster">either IRaster, IRasterDataset, or a valid path pointing to a raster</param>
        /// <param name="clm">number of columns (cells)</param>
        /// <param name="rws">number of rows</param>
        /// <param name="statType">the type of operation</param>
        /// <param name="landType">the type of metric</param>
        /// <returns>a IRaster that can be used for further analysis</returns>
        public IRaster calcLandscapeFunction(object inRaster, int clm, int rws, focalType statType, landscapeType landType)
        {
            IRaster iR1 = returnRaster(inRaster);
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc = new FunctionRasters.landscapeFunctionDataset();
            FunctionRasters.LandscapeFunctionArguments args = new FunctionRasters.LandscapeFunctionArguments(this);
            args.WindowType = windowType.RECTANGLE;
            args.Rows = rws;
            args.Columns = clm;
            args.InRaster = iR1;
            args.Operation = statType;
            args.LandscapeType = landType;
            frDset.Init(rsFunc, args);
            IRaster rs = createRaster((IRasterDataset)frDset);
            return rs;
        }
        /// <summary>
        /// Will perform a focal raster operation on an input raster all bands
        /// </summary>
        /// <param name="inRaster">either IRaster, IRasterDataset, or a valid path pointing to a raster</param>
        /// <param name="radius">number of cells that make up the radius of the moving window</param>
        /// <param name="statType">the type of operation</param>
        /// <param name="landType">the type of metric</param>
        /// <returns>a IRaster that can be used for further analysis</returns>
        public IRaster calcLandscapeFunction(object inRaster, int radius, focalType statType, landscapeType landType)
        {
            IRaster iR1 = returnRaster(inRaster);
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc = new FunctionRasters.landscapeFunctionDataset();
            FunctionRasters.LandscapeFunctionArguments args = new FunctionRasters.LandscapeFunctionArguments(this);
            args.WindowType = windowType.CIRCLE;
            args.Radius = radius;
            args.InRaster = iR1;
            args.Operation = statType;
            args.LandscapeType = landType;
            frDset.Init(rsFunc, args);
            IRaster rs = createRaster((IRasterDataset)frDset);
            return rs;
        }
        /// <summary>
        /// Returns a IRaster given either a path, IRasterdataset, or IRaster
        /// </summary>
        /// <param name="inRaster"></param>
        /// <returns></returns>
        public IRaster returnRaster(object inRaster)
        {
            IRaster iR1 = null;
            try
            {
                if (inRaster is Raster)
                {
                    IRasterBandCollection rsBc = new RasterClass();
                    rsBc.AppendBands((IRasterBandCollection)((IRaster)inRaster));
                    iR1 = (IRaster)rsBc;
                }
                else if (inRaster is RasterDataset)
                {
                    iR1 = createRaster((IRasterDataset)inRaster);
                }
                else if (inRaster is String)
                {
                    string rsNm = System.IO.Path.GetFileNameWithoutExtension(inRaster.ToString());
                    string bnd = "";
                    iR1 = createRaster(openRasterDataset(inRaster.ToString(), out bnd));
                    if (bnd.ToLower()!="all")
                    {
                        if (isNumeric(bnd))
                        {
                            int bndNum = System.Convert.ToInt32(bnd)-1;
                            iR1 = getBand(iR1, bndNum);
                        }
                    }
                }
                else
                {
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return iR1;
        }
        public IRaster returnRaster(object inRaster, rstPixelType pType)
        {
            IRaster rs = returnRaster(inRaster);
            IRasterProps rsProps = (IRasterProps)rs;
            if(rsProps.PixelType!=pType)
            {
                rs = convertToDifFormatFunction(rs, pType);
            }
            return rs;
        }
        /// <summary>
        /// samples all bands of a given raster given a point feature class. Appends those values to a field named by the raster/band.
        /// </summary>
        /// <param name="inFtrCls">Point feature class that is used to sample</param>
        /// <param name="sampleRst">Raster dataset that is going to be sampled</param>
        /// <returns>a list of all the created field names</returns>
        public string[] sampleRaster(IFeatureClass inFtrCls, IRaster sampleRst, string inName)
        {
            List<string> outLst = new List<string>();
            IRaster2 sr = (IRaster2)sampleRst;
            Dictionary<int, string> lc = new Dictionary<int, string>();
            IRasterBandCollection rsBC = (IRasterBandCollection)sr;
            IEnumRasterBand rsBE = rsBC.Bands;
            IRasterBand rsB = rsBE.Next();
            string rsName = inName;
            if(rsName==null)
            {
                rsName = ((IDataset)sr.RasterDataset).Name;
            }
            int cntB = 0;
            while (rsB != null)
            {
                string fldName = rsName + "_Band" + (cntB + 1).ToString();
                //fldName = geoUtil.getSafeFieldName(inFtrCls, fldName);
                outLst.Add(fldName);
                esriFieldType fldType = esriFieldType.esriFieldTypeDouble;
                fldName = geoUtil.createField(inFtrCls, fldName, fldType);
                lc.Add(cntB,fldName);
                cntB++;
                rsB = rsBE.Next();
            }
            IGeometry geo = (IGeometry)((IRasterProps)sampleRst).Extent;
            ISpatialFilter spFlt = new SpatialFilterClass();
            spFlt.Geometry = geo;
            spFlt.GeometryField = inFtrCls.ShapeFieldName;
            spFlt.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            IFeatureCursor sCur = inFtrCls.Search(spFlt, false);
            IFeature sRow = sCur.NextFeature();
            while (sRow != null)
            {
                geo = sRow.Shape;
                IPoint pnt = (IPoint)geo;
                int x,y;
                sr.MapToPixel(pnt.X, pnt.Y, out x, out y);
                for (int i = 0; i < ((IRasterBandCollection)sr).Count; i++)
                {
                    int fldIndex = inFtrCls.FindField(lc[i]);
                    object rsVl = sr.GetPixelValue(i, x, y);
                    try
                    {
                        sRow.set_Value(fldIndex, rsVl);
                    }
                    catch
                    {
                        Console.WriteLine(rsVl.ToString());
                    }

                }
                sRow.Store();
                sRow = sCur.NextFeature();
            }
            return outLst.ToArray();

        }
        /// <summary>
        /// sample a raster using a given offset
        /// </summary>
        /// <param name="inFtrCls"></param>
        /// <param name="sampleRst"></param>
        /// <param name="inName"></param>
        /// <param name="azmithDistance"></param>
        /// <param name="typeOfCluster"></param>
        /// <returns></returns>
        public string[] sampleRaster(IFeatureClass inFtrCls, IRaster sampleRst, string inName, Dictionary<double,double> azmithDistance, clusterType typeOfCluster)
        {
            List<string> outLst = new List<string>();
            IRaster2 sr = (IRaster2)sampleRst;
            Dictionary<int, string> lc = new Dictionary<int, string>();
            IRasterBandCollection rsBC = (IRasterBandCollection)sr;
            IEnumRasterBand rsBE = rsBC.Bands;
            IRasterBand rsB = rsBE.Next();
            string rsName = inName;
            if (rsName == null)
            {
                rsName = ((IDataset)sr.RasterDataset).Name;
            }
            int cntB = 0;
            while (rsB != null)
            {
                string fldName = rsName + "_Band" + (cntB + 1).ToString();
                //fldName = geoUtil.getSafeFieldName(inFtrCls, fldName);
                outLst.Add(fldName);
                esriFieldType fldType = esriFieldType.esriFieldTypeDouble;
                fldName = geoUtil.createField(inFtrCls, fldName, fldType);
                lc.Add(cntB, fldName);
                cntB++;
                rsB = rsBE.Next();
            }
            IGeometry geo = (IGeometry)((IRasterProps)sampleRst).Extent;
            ISpatialFilter spFlt = new SpatialFilterClass();
            spFlt.Geometry = geo;
            spFlt.GeometryField = inFtrCls.ShapeFieldName;
            spFlt.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            IFeatureCursor sCur = inFtrCls.Search(spFlt, false);
            IFeature sRow = sCur.NextFeature();
            while (sRow != null)
            {
                geo = sRow.Shape;
                IPoint pnt = (IPoint)geo;
                int x, y;
                x = 0;
                y = 0;
                for (int i = 0; i < ((IRasterBandCollection)sr).Count; i++)
                {
                    double rsVl = 0;
                    List<double> rsVlLst = new List<double>();
                    try
                    {
                        sr.MapToPixel(pnt.X, pnt.Y, out x, out y);
                        int fldIndex = inFtrCls.FindField(lc[i]);
                        rsVlLst.Add(System.Convert.ToDouble(sr.GetPixelValue(i, x, y)));
                        foreach (KeyValuePair<double, double> kVp in azmithDistance)
                        {
                            double az = kVp.Key;
                            double ds = kVp.Value;
                            double nX = pnt.X + (System.Math.Sin(az * Math.PI / 180) * ds);
                            double nY = pnt.Y + (System.Math.Cos(az * Math.PI / 180) * ds);
                            sr.MapToPixel(nX, nY, out x, out y);
                            rsVlLst.Add(System.Convert.ToDouble(sr.GetPixelValue(i, x, y)));
                        }
                        switch (typeOfCluster)
                        {
                            case clusterType.SUM:
                                rsVl = rsVlLst.Sum();
                                break;
                            case clusterType.MEAN:
                                rsVl = rsVlLst.Average();
                                break;
                            case clusterType.MEDIAN:
                                rsVlLst.Sort();
                                rsVl = rsVlLst[(rsVlLst.Count-1) / 2];
                                break;
                            case clusterType.MODE:
                                Dictionary<double, int> cntDic = new Dictionary<double, int>();
                                int maxLc = 0;
                                double maxKy = rsVlLst[0];
                                foreach (double d in rsVlLst)
                                {
                                    if (cntDic.ContainsKey(d))
                                    {
                                        int cntVl = cntDic[d] + 1;
                                        if(cntVl>maxLc)
                                        {
                                            maxLc = cntVl;
                                            maxKy = d;
                                        }
                                        cntDic[d] = cntVl;
                                    }
                                    else
                                    {
                                        cntDic.Add(d, 1);
                                    }
                                }
                                rsVl = maxKy;
                                break;
                            default:
                                break;
                        }
                        sRow.set_Value(fldIndex, rsVl);
                    }
                    catch
                    {
                        Console.WriteLine(rsVl.ToString());
                    }

                }
                sRow.Store();
                sRow = sCur.NextFeature();
            }
            return outLst.ToArray();

        }
        /// <summary>
        /// Remaps the values of a given raster to new set of values
        /// </summary>
        /// <param name="inRaster">input raster</param>
        /// <param name="filter">a remap filter</param>
        /// <returns>IRaster with remaped values</returns>
        public IRaster calcRemapFunction(object inRaster, IRemapFilter filter)
        {
            IRaster rRst = returnRaster(inRaster);
            IRasterProps rsProps = (IRasterProps)rRst;
            IDoubleArray rangeArray = new DoubleArrayClass();
            IDoubleArray valueArray = new DoubleArrayClass();
            double min,max,vl;
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc = new RemapFunctionClass();
            IRemapFunctionArguments args = new RemapFunctionArgumentsClass();
            args.AllowUnmatched = filter.AllowUnmatched;
            args.Raster = rRst;
            for (int i = 0; i < filter.ClassCount; i++)
            {
                filter.QueryClass(i, out min, out max, out vl);
                rangeArray.Add(min);
                rangeArray.Add(max);
                valueArray.Add(vl);
            }
            args.InputRanges = rangeArray;
            args.OutputValues = valueArray;
            frDset.Init(rsFunc, args);
            return createRaster((IRasterDataset)frDset);

        }
        /// <summary>
        /// Calculates a trend raster from double filter
        /// </summary>
        /// <param name="inRaster">string, IRasterDataset, or IRaster</param>
        /// <param name="doubleFilter">a double array of plan values</param>
        /// <returns>IRaster</returns>
        public IRaster calcTrendFunction(object inRaster, double[] doubleFilter)
        {
            IDoubleArray dbArray = new DoubleArrayClass();
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc = new TrendFunctionClass();
            ITrendFunctionArguments args = new TrendFunctionArgumentsClass();
            args.Raster = returnRaster(inRaster,rstPixelType.PT_FLOAT);
            for (int i = 0; i < doubleFilter.Length; i++)
            {
                dbArray.Add(doubleFilter[i]);
            }
            args.PlaneParameters = dbArray;
            frDset.Init(rsFunc, args);
            return createRaster((IRasterDataset)frDset);

        }
        public IRaster calcPolytomousLogisticRegressFunction(object inRaster, double[][] slopes)
        {
            IRaster rRst = returnRaster(inRaster);
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc = new FunctionRasters.polytomousLogisticFunctionDataset();
            FunctionRasters.polytomousLogisticFunctionArguments args = new FunctionRasters.polytomousLogisticFunctionArguments(this);
            args.InRasterCoefficients = rRst;
            args.Slopes = slopes;
            frDset.Init(rsFunc, args);
            IRaster outRs = createRaster((IRasterDataset)frDset);
            return outRs;

        }
        public IRaster calcSoftMaxNnetFunction(object inRaster, Statistics.dataPrepSoftMaxPlr sm)
        {
            IRaster rRst = returnRaster(inRaster);
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc = new FunctionRasters.softMaxFunctionDataset();
            FunctionRasters.softMaxFunctionArguments args = new FunctionRasters.softMaxFunctionArguments(this);
            args.InRasterCoefficients = rRst;
            args.LogitModel = sm;           
            frDset.Init(rsFunc, args);
            IRaster outRs = createRaster((IRasterDataset)frDset);
            return outRs;

        }
        public IRaster calcPrincipleComponentsFunction(IRaster inRaster, Statistics.dataPrepPrincipleComponents pca)
        {
            IRaster rRst = returnRaster(inRaster);
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc = new FunctionRasters.pcaDataset();
            FunctionRasters.pcaArguments args = new FunctionRasters.pcaArguments(this);
            args.InRasterCoefficients = rRst;
            args.PCA = pca;
            frDset.Init(rsFunc, args);
            IRaster outRs = createRaster((IRasterDataset)frDset);
            return outRs;
        }
        public IRaster calcMosaicFunction(IRaster[] inRasters,mergeType mType)
        {
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc = null;
            switch (mType)
            {
                case mergeType.LAST:
                    rsFunc = new FunctionRasters.mergeFunctionDatasetLast();
                    break;
                case mergeType.MIN:
                    rsFunc = new FunctionRasters.mergeFunctionDatasetMin();
                    break;
                case mergeType.MAX:
                    rsFunc = new FunctionRasters.mergeFunctionDatasetMax();
                    break;
                case mergeType.MEAN:
                    rsFunc = new FunctionRasters.mergeFunctionDatasetMean();
                    break;
                default:
                    rsFunc = new FunctionRasters.mergeFunctionDatasetFirst();
                    break;
            }
            FunctionRasters.mergeFunctionArguments args = new FunctionRasters.mergeFunctionArguments(this);
            args.InRaster = inRasters;
            frDset.Init(rsFunc, args);
            IRaster outRs = createRaster((IRasterDataset)frDset);
            return outRs;
        }
        public IRaster calcClustFunctionKmean(IRaster inRaster, Statistics.dataPrepClusterKmean clus)
        {
            IRaster rRst = returnRaster(inRaster);
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc = new FunctionRasters.clusterFunctionKmeanDataset();
            FunctionRasters.clusterFunctionArguments args = new FunctionRasters.clusterFunctionArguments(this);
            args.InRasterCoefficients = rRst;
            args.ClusterModel = clus;
            frDset.Init(rsFunc, args);
            IRaster outRs = createRaster((IRasterDataset)frDset);
            return outRs;
        }
        public IRaster calcClustFunctionBinary(IRaster inRaster, Statistics.dataPrepClusterBinary clus)
        {
            IRaster rRst = returnRaster(inRaster);
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc = new FunctionRasters.clusterFunctionBinaryDataset();
            FunctionRasters.clusterFunctionArguments args = new FunctionRasters.clusterFunctionArguments(this);
            args.InRasterCoefficients = rRst;
            args.ClusterModel = clus;
            frDset.Init(rsFunc, args);
            IRaster outRs = createRaster((IRasterDataset)frDset);
            return outRs;
        }
        public IRaster calcClustFunctionGaussian(IRaster inRaster, Statistics.dataPrepClusterGaussian clus)
        {
            IRaster rRst = returnRaster(inRaster);
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc = new FunctionRasters.clusterFunctionGaussianDataset();
            FunctionRasters.clusterFunctionArguments args = new FunctionRasters.clusterFunctionArguments(this);
            args.InRasterCoefficients = rRst;
            args.ClusterModel = clus;
            frDset.Init(rsFunc, args);
            IRaster outRs = createRaster((IRasterDataset)frDset);
            return outRs;
        }
        public IRaster calcPairedTTestFunction(IRaster inRaster, Statistics.dataPrepPairedTTest pairedttest)
        {
            IRaster rRst = returnRaster(inRaster);
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc = new FunctionRasters.pairedttestFunctionDataset();
            FunctionRasters.pairedttestFunctionArguments args = new FunctionRasters.pairedttestFunctionArguments(this);
            args.InRasterCoefficients = rRst;
            args.TTestModel = pairedttest;
            frDset.Init(rsFunc, args);
            IRaster outRs = createRaster((IRasterDataset)frDset);
            return outRs;
        }
        public IRaster calcGlmFunction(IRaster inRaster, Statistics.dataPrepGlm glm)
        {
            IRaster rRst = returnRaster(inRaster);
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc = new FunctionRasters.glmFunctionDataset();
            FunctionRasters.glmFunctionArguments args = new FunctionRasters.glmFunctionArguments(this);
            args.InRasterCoefficients = rRst;
            args.GlmModel = glm;
            frDset.Init(rsFunc, args);
            IRaster outRs = createRaster((IRasterDataset)frDset);
            return outRs;
        }
        public IRaster calcTTestFunction(IRaster inRaster, Statistics.dataPrepTTest ttest)
        {
            IRaster rRst = returnRaster(inRaster);
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc = new FunctionRasters.ttestFunctionDataset();
            FunctionRasters.ttestFunctionArguments args = new FunctionRasters.ttestFunctionArguments(this);
            args.InRasterCoefficients = rRst;
            args.TTestModel = ttest;
            frDset.Init(rsFunc, args);
            IRaster outRs = createRaster((IRasterDataset)frDset);
            return outRs;
        }
        public IRaster calcRandomForestFunction(object inRaster, Statistics.dataPrepRandomForest rf)
        {
            IRaster rRst = returnRaster(inRaster);
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc = new FunctionRasters.randomForestDataset();
            FunctionRasters.randomForestArguments args = new FunctionRasters.randomForestArguments(this);
            args.InRasterCoefficients = rRst;
            args.RandomForestModel = rf;
            frDset.Init(rsFunc, args);
            IRaster outRs = createRaster((IRasterDataset)frDset);
            return outRs;

        }
        /// <summary>
        /// regresses sums an intercept value to the sum product of a series of raster bands and corresponding slope values. Number of bands and slope values must match
        /// </summary>
        /// <param name="inRaster">string IRaster, or IRasterDataset that has the same number of bands as the slopes array has values</param>
        /// <param name="slopes">double[] representing the corresponding slope values the first value in the array is the intercept</param>
        /// <returns></returns>
        public IRaster calcRegressFunction(object inRaster, List<float[]> slopes)
        {
            IRaster rRst = returnRaster(inRaster,rstPixelType.PT_FLOAT);
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc =  new FunctionRasters.regressionFunctionDataset();      
            FunctionRasters.regressionFunctionArguments args = new FunctionRasters.regressionFunctionArguments(this);
            args.InRasterCoefficients = rRst;
            args.Slopes = slopes;
            frDset.Init(rsFunc, args);
            IRaster outRs = returnRaster((IRasterDataset)frDset);
            return outRs;

        }
        public IRaster calcCensoredRegressFunction(object inRaster, List<float[]> slopes,float lowerLimit=0)
        {
            IRaster rRst = returnRaster(inRaster);
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc = new FunctionRasters.tobitFunctionDataset();
            FunctionRasters.tobitFunctionArguments args = new FunctionRasters.tobitFunctionArguments(this);
            args.InRasterCoefficients = rRst;
            args.Slopes = slopes;
            args.CensoredValue = lowerLimit;
            frDset.Init(rsFunc, args);
            IRaster outRs = returnRaster((IRasterDataset)frDset);
            return outRs;

        }
        public IRaster calcTobitRegressFunction(object inRaster, string modelPath, float lowerLimit = 0)
        {
            IRaster rRst = returnRaster(inRaster);
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc = new FunctionRasters.tobitFunctionDataset();
            FunctionRasters.tobitFunctionArguments args = new FunctionRasters.tobitFunctionArguments(this);
            args.InRasterCoefficients = rRst;
            args.TobitModelPath = modelPath;
            args.CensoredValue = lowerLimit;
            frDset.Init(rsFunc, args);
            IRaster outRs = returnRaster((IRasterDataset)frDset);
            return outRs;

        }
        /// <summary>
        /// Remaps values greater than or equal to the input vl to 1. Values less than vl =0
        /// </summary>
        /// <param name="inRaster">string, IRasterDataset, or IRaster</param>
        /// <param name="vl">value or raster to compare against</param>
        /// <returns>IRaster</returns>
        public IRaster calcGreaterEqualFunction(object inRaster, object compareRaster)
        {
            IRaster rs = returnRaster(inRaster);
            IRaster outRs = null;
            if (isNumeric(compareRaster.ToString()))
            {
                //Console.WriteLine("Is Number");
                double vl = System.Convert.ToDouble(compareRaster);
                IRemapFilter rFilt = new RemapFilterClass();
                IRasterProps rsP = (IRasterProps)rs;
                rstPixelType pType = rsP.PixelType;
                double max, min;
                max = 0;
                min = 0;
                #region set min max
                getMinMax(pType, ref max, ref min);
                #endregion
                rFilt.AddClass(min, vl, 0);
                rFilt.AddClass(vl, max, 1);
                outRs = calcRemapFunction(rs, rFilt);
            }
            else
            {
                IRaster crs = returnRaster(compareRaster);
                //Console.WriteLine("Is Raster");
                IRaster minRst = calcArithmaticFunction(rs, crs, esriRasterArithmeticOperation.esriRasterMinus);
                outRs = calcGreaterEqualFunction(minRst, 0);
            }
            functionModel.estimateStatistics(outRs);
            return outRs;
        }
        /// <summary>
        /// Remaps values greater than to the input vl to 1. Values less than vl =0
        /// </summary>
        /// <param name="inRaster">string, IRasterDataset, or IRaster</param>
        /// <param name="vl">value or raster to compare against</param>
        /// <returns>IRaster</returns>
        public IRaster calcGreaterFunction(object inRaster, object compareRaster)
        {
            IRaster rs = returnRaster(inRaster);
            IRaster outRs = null;
            if (isNumeric(compareRaster.ToString()))
            {
                //Console.WriteLine("Is Number");
                double vl = System.Convert.ToDouble(compareRaster);
                IRemapFilter rFilt = new RemapFilterClass();
                IRasterProps rsP = (IRasterProps)rs;
                rstPixelType pType = rsP.PixelType;
                double max, min;
                max = 0;
                min = 0;
                #region set min max
                getMinMax(pType, ref max, ref min);
                #endregion
                double vlP1 = vl + 0.000001;
                rFilt.AddClass(min, vlP1, 0);
                rFilt.AddClass(vlP1, max, 1);
                outRs = calcRemapFunction(rs, rFilt);
            }
            else
            {
                IRaster crs = returnRaster(compareRaster);
                //Console.WriteLine("Is Raster");
                IRaster minRst = calcArithmaticFunction(rs, crs, esriRasterArithmeticOperation.esriRasterMinus);
                outRs = calcGreaterFunction(minRst, 0);
            }
            functionModel.estimateStatistics(outRs);
            return outRs;
        }
        /// <summary>
        /// returns the potential max min values of a raster given a pixel type by reference
        /// </summary>
        /// <param name="pType">raser pixel type</param>
        /// <param name="max">reference value max</param>
        /// <param name="min">reference value min</param>
        private void getMinMax(rstPixelType pType, ref double max, ref double min)
        {
            switch (pType)
            {
                case rstPixelType.PT_CHAR:
                    max = 128;
                    min = -128;
                    break;
                case rstPixelType.PT_CLONG:
                case rstPixelType.PT_COMPLEX:
                case rstPixelType.PT_CSHORT:
                case rstPixelType.PT_DCOMPLEX:
                    max = 4294967296;
                    min = -1;
                    break;
                case rstPixelType.PT_LONG:
                case rstPixelType.PT_FLOAT:
                    max = 2147483648;
                    min = -2147483649;
                    break;
                case rstPixelType.PT_SHORT:
                    max = 32768;
                    min = -32769;
                    break;
                case rstPixelType.PT_U1:
                    max = 2;
                    min = -1;
                    break;
                case rstPixelType.PT_U2:
                    max = 4;
                    min = -1;
                    break;
                case rstPixelType.PT_U4:
                    max = 16;
                    min = -1;
                    break;
                case rstPixelType.PT_UCHAR:
                    max = 256;
                    min = -1;
                    break;
                case rstPixelType.PT_ULONG:
                case rstPixelType.PT_UNKNOWN:
                    max = 4294967296;
                    min = -4294967297;
                    break;
                case rstPixelType.PT_USHORT:
                    max = 65536;
                    min = -1;
                    break;
                default:
                    double b64 = Math.Pow(2, 64);
                    max = b64;
                    min = (b64 * -1) + -1;
                    break;
            }
        }
        /// <summary>
        /// Remaps values less than or equal to the compareRaster to 1. Values greater than compareRaster = 0
        /// </summary>
        /// <param name="inRaster">string, IRasterDataset, or IRaster</param>
        /// <param name="vl">value or Raserter to compare against</param>
        /// <returns>IRaster</returns>
        public IRaster calcLessEqualFunction(object inRaster, object compareRaster)
        {
            IRaster rs = returnRaster(inRaster);
            IRaster outRs = null;
            if (isNumeric(compareRaster.ToString()))
            {
                double vl = System.Convert.ToDouble(compareRaster);
                IRemapFilter rFilt = new RemapFilterClass();
                IRasterProps rsP = (IRasterProps)rs;
                rstPixelType pType = rsP.PixelType;
                double max, min;
                max = 0;
                min = 0;
                #region set min max
                getMinMax(pType, ref max, ref min);
                #endregion
                double vlP1 = vl + 0.000001;
                rFilt.AddClass(min, vlP1, 1);
                rFilt.AddClass(vlP1, max, 0);
                outRs = calcRemapFunction(rs, rFilt);
            }
            else
            {
                IRaster crs = returnRaster(compareRaster);
                IRaster minRst = calcArithmaticFunction(rs, crs, esriRasterArithmeticOperation.esriRasterMinus);
                outRs = calcLessEqualFunction(minRst, 0);
            }
            functionModel.estimateStatistics(outRs);
            return outRs;
        }
        /// <summary>
        /// Remaps values less than to the compareRaster to 1. Values greater than compareRaster = 0
        /// </summary>
        /// <param name="inRaster">string, IRasterDataset, or IRaster</param>
        /// <param name="vl">value or Raserter to compare against</param>
        /// <returns>IRaster</returns>
        public IRaster calcLessFunction(object inRaster, object compareRaster)
        {
            IRaster rs = returnRaster(inRaster);
            IRaster outRs = null;
            if (isNumeric(compareRaster.ToString()))
            {
                double vl = System.Convert.ToDouble(compareRaster);
                IRemapFilter rFilt = new RemapFilterClass();
                IRasterProps rsP = (IRasterProps)rs;
                rstPixelType pType = rsP.PixelType;
                double max, min;
                max = 0;
                min = 0;
                #region set min max
                getMinMax(pType, ref max, ref min);
                #endregion
                rFilt.AddClass(min, vl, 1);
                rFilt.AddClass(vl, max, 0);
                outRs = calcRemapFunction(rs, rFilt);
            }
            else
            {
                IRaster crs = returnRaster(compareRaster);
                IRaster minRst = calcArithmaticFunction(rs, crs, esriRasterArithmeticOperation.esriRasterMinus);
                outRs = calcLessFunction(minRst, 0);
            }
            functionModel.estimateStatistics(outRs);
            return outRs;
        }
        /// <summary>
        /// Remaps values equal to the compare Raster or value cell values = 1. Values less than or greater than compare raster or value = 0
        /// </summary>
        /// <param name="inRaster">string, IRasterDataset, or IRaster</param>
        /// <param name="vl">value to compare against</param>
        /// <returns>IRaster</returns>
        public IRaster calcEqualFunction(object inRaster, object compareRaster)
        {
            IRaster rs = returnRaster(inRaster);
            IRaster outRs = null;
            if (isNumeric(compareRaster.ToString()))
            {
                double vl = System.Convert.ToDouble(compareRaster);
                IRemapFilter rFilt = new RemapFilterClass();
                IRasterProps rsP = (IRasterProps)rs;
                rstPixelType pType = rsP.PixelType;
                double max, min;
                max = 0;
                min = 0;
                #region set min max
                getMinMax(pType, ref max, ref min);
                #endregion
                double vlP1 = vl + 0.000001;
                rFilt.AddClass(min, vl, 0);
                rFilt.AddClass(vl, vlP1, 1);
                rFilt.AddClass(vlP1, max, 0);
                outRs = calcRemapFunction(rs, rFilt);
            }
            else
            {
                IRaster crs = returnRaster(compareRaster);
                IRaster minRst = calcArithmaticFunction(rs, crs, esriRasterArithmeticOperation.esriRasterMinus);
                outRs = calcEqualFunction(minRst, 0);
            }
            functionModel.estimateStatistics(outRs);
            return outRs;
        }
        public IRaster calcCombineRasterFunction(IRaster[] inRasters)
        {
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc = new FunctionRasters.combineFunctionDataset();
            FunctionRasters.combineFunctionArguments args = new FunctionRasters.combineFunctionArguments(this);
            args.InRaster = inRasters;
            frDset.Init(rsFunc, args);
            IRaster outRs = createRaster((IRasterDataset)frDset);
            //functionModel.estimateStatistics(outRs, functionModel.dem.Slope);
            return outRs;
        }
        /// <summary>
        /// Creates a constant raster given a template raster and a double value
        /// </summary>
        /// <param name="templateRaster">Raster that has the extent and cell size of the desired constant raster</param>
        /// <param name="rasterValue">double value that all cells will have</param>
        /// <returns>a constant raster IRaster</returns>
        public IRaster constantRasterFunction(object templateRaster, double rasterValue)
        {
            IRaster inRaster = returnRaster(templateRaster,rstPixelType.PT_FLOAT);
            IRasterFunction identFunction = (IRasterFunction)new IdentityFunction();
            identFunction.Bind(inRaster);
            IConstantFunctionArguments rasterFunctionArguments = (IConstantFunctionArguments)new ConstantFunctionArguments();
            rasterFunctionArguments.Constant = rasterValue;
            rasterFunctionArguments.RasterInfo = identFunction.RasterInfo;
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc = new ConstantFunction();
            frDset.Init(rsFunc, rasterFunctionArguments);
            IRaster outRs = returnRaster((IRasterDataset)frDset);
            //functionModel.estimateStatistics(rasterValue,outRs);
            return outRs;

        }
        public IRaster constantRasterFunction(IRaster template,IEnvelope NewExtent, double rasterValue, IPnt cellSize)
        {
            IRaster inRaster = returnRaster(template);
            IRasterFunction identFunction = (IRasterFunction)new IdentityFunction();
            identFunction.Bind(inRaster);
            IConstantFunctionArguments rasterFunctionArguments = (IConstantFunctionArguments)new ConstantFunctionArguments();
            rasterFunctionArguments.Constant = rasterValue;
            IRasterInfo rsInfo = identFunction.RasterInfo;
            rsInfo.NativeExtent = NewExtent;
            rsInfo.Extent = NewExtent;
            //rsInfo.PixelType = rstPixelType.PT_FLOAT;
            //rsInfo.NativeSpatialReference = sr;
            rsInfo.CellSize = cellSize;
            //rsInfo.BandCount = bandCount;
            //rsInfo.Format = rasterUtil.rasterType.GDB.ToString();
            //float[] ndArr = new float[bandCount];
            //for (int i = 0; i < bandCount; i++)
            //{
            //    ndArr[i] = (float)getNoDataValue(rstPixelType.PT_FLOAT);
            //}
            //rsInfo.NoData = ndArr;
            rasterFunctionArguments.RasterInfo = rsInfo;
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc = new ConstantFunction();
            frDset.Init(rsFunc, rasterFunctionArguments);
            IRaster outRs = returnRaster((IRasterDataset)frDset);
            //functionModel.estimateStatistics(rasterValue,outRs);
            return outRs;

        }
        /// <summary>
        /// Used as an if then else statement. The condRaster raster is meant to have values of 1 or 0. If a cell within the input raster has a value 1
        /// then the cell gets the value of inRaster1's corresponding cell. Otherwise that cell gets the value of the inRaster2's corresponding cell.
        /// </summary>
        /// <param name="condRaster">string path, IRaster, IRasterDataset thats cell values are 0 or 1</param>
        /// <param name="inRaster1">string path, IRaster, IRasterDataset, or a numeric value</param>
        /// <param name="inRaster2">string path, IRaster, IRasterDataset, or a numeric value</param>
        /// <returns>IRaster</returns>
        public IRaster conditionalRasterFunction(object condRaster, object trueRaster, object falseRaster)
        {
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            FunctionRasters.conditionalFunctionDataset rsFunc = new FunctionRasters.conditionalFunctionDataset();
            FunctionRasters.conditionalFunctionArguments args = new FunctionRasters.conditionalFunctionArguments(this);
            IRaster conRs = returnRaster(condRaster);
            if (conRs==null)
            {
                Console.WriteLine("Condition Raster must be a raster");
                return null;
            }
            IRaster iR1, iR2;
            if (isNumeric(trueRaster.ToString()) && !isNumeric(falseRaster.ToString()))
            {
                iR2 = returnRaster(falseRaster,rstPixelType.PT_FLOAT);
                iR1 = constantRasterFunction(conRs, System.Convert.ToDouble(trueRaster));
            }
            else if (isNumeric(falseRaster.ToString()) && !isNumeric(trueRaster.ToString()))
            {
                iR1 = returnRaster(trueRaster,rstPixelType.PT_FLOAT);
                iR2 = constantRasterFunction(conRs, System.Convert.ToDouble(falseRaster));
            }
            else if (isNumeric(falseRaster.ToString()) && isNumeric(trueRaster.ToString()))
            {
                iR1 = constantRasterFunction(conRs, System.Convert.ToDouble(trueRaster));
                iR2 = constantRasterFunction(conRs, System.Convert.ToDouble(falseRaster));
            }
            else
            {
                iR1 = returnRaster(trueRaster, rstPixelType.PT_FLOAT);
                iR2 = returnRaster(falseRaster,rstPixelType.PT_FLOAT);
            }
            args.ConditionalRaster = conRs;
            args.TrueRaster = iR1;
            args.FalseRaster = iR2;
            frDset.Init(rsFunc, args);
            IRaster outRs = createRaster((IRasterDataset)frDset);
            //functionModel.estimateStatistics(iR1, iR2, outRs, esriRasterArithmeticOperation.esriRasterPlus);
            return outRs;
            
        }
        /// <summary>
        /// LocalStatistics
        /// </summary>
        /// <param name="inRaster">string, IRasterDataset, or Raster</param>
        /// <returns>IRaster</returns>
        public IRaster localStatisticsfunction(object inRaster, rasterUtil.localType op)
        {
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc = null;
            switch (op)
            {
                case localType.MAX:
                    rsFunc = new FunctionRasters.localMaxFunctionDataset();
                    break;
                case localType.MIN:
                    rsFunc = new FunctionRasters.localMinFunctionDataset();
                    break;
                case localType.SUM:
                    rsFunc = new FunctionRasters.localSumFunctionDataset();
                    break;
                case localType.MULTIPLY:
                    rsFunc = new FunctionRasters.localMultiplyFunctionDataset();
                    break;
                case localType.DIVIDE:
                    rsFunc = new FunctionRasters.localDividFunctionDataset();
                    break;
                case localType.SUBTRACT:
                    rsFunc = new FunctionRasters.localSubtractFunctionDataset();
                    break;
                case localType.POWER:
                    rsFunc = new FunctionRasters.localPowFunctionDataset();
                    break;
                case localType.MEAN:
                    rsFunc = new FunctionRasters.localMeanFunctionDataset();
                    break;
                case localType.VARIANCE:
                    rsFunc = new FunctionRasters.localVarianceFunctionDataset();
                    break;
                case localType.STD:
                    rsFunc = new FunctionRasters.localStandardDeviationFunctionDataset();
                    break;
                case localType.MODE:
                    rsFunc = new FunctionRasters.localModeFunctionDataset();
                    break;
                case localType.MEDIAN:
                    rsFunc = new FunctionRasters.localMedianFunctionDataset();
                    break;
                case localType.UNIQUE:
                    rsFunc = new FunctionRasters.localUniqueValuesFunctionDataset();
                    break;
                case localType.ENTROPY:
                    rsFunc = new FunctionRasters.localEntropyFunctionDataset();
                    break;
                case localType.MAXBAND:
                    rsFunc = new FunctionRasters.localMaxBandFunction();
                    break;
                case localType.MINBAND:
                    rsFunc = new FunctionRasters.localMinBandFunction();
                    break;
                case localType.ASM:
                    rsFunc = new FunctionRasters.localAsmFunctionDataset();
                    break;
                default:
                    break;
            }
            FunctionRasters.LocalFunctionArguments args = new FunctionRasters.LocalFunctionArguments(this);
            
            IRaster inRs = returnRaster(inRaster);
            args.InRaster = inRs;
            IRaster outRs = null;
            frDset.Init(rsFunc, args);
            outRs = createRaster((IRasterDataset)frDset);
            //functionModel.estimateStatistics(inRs, outRs, op);
            return outRs;

        }
        /// <summary>
        /// Clips a raster to the boundary of a polygon
        /// </summary>
        /// <param name="inRaster">IRaster, IRasterDataset, or string</param>
        /// <param name="geo">Polygon Geometry</param>
        /// <param name="clipType">the type of clip either inside or outside</param>
        /// <returns></returns>
        public IRaster clipRasterFunction(object inRaster,IGeometry geo,esriRasterClippingType clipType)
        {
            IRaster rRst = returnRaster(inRaster);
            IRaster2 rRst2 = (IRaster2)rRst;
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            //IRasterFunction rsFunc = new esriUtil.FunctionRasters.clipFunctionDataset();
            IRasterFunction rsFunc = new ClipFunctionClass();
            rsFunc.PixelType = ((IRasterProps)rRst).PixelType;
            IEnvelope env = geo.Envelope;
            //IRasterProps rsProps = (IRasterProps)rRst;
            IPnt cSize = getCellSize(rRst);
            double hX = cSize.X / 2;
            double hY = cSize.Y / 2;
            double xMin = env.XMin;
            double xMax = env.XMax;
            double yMin = env.YMin;
            double yMax = env.YMax;
            int clm, rw;
            rRst2.MapToPixel(xMin, yMin,out clm,out rw);
            rRst2.PixelToMap(clm, rw, out xMin, out yMin);
            xMin = xMin - hX;
            yMin = yMin - hY;
            rRst2.MapToPixel(xMax, yMax, out clm, out rw);
            rRst2.PixelToMap(clm, rw, out xMax, out yMax);
            xMax = xMax + hX;
            yMax = yMax + hY;
            env.PutCoords(xMin, yMin, xMax, yMax);
            //IEnvelope rsExtent = rsProps.Extent;
            //rsFunc.RasterInfo.CellSize = cSize;           
            //double w = (((env.XMax - env.XMin) / cSize.X) + 1)*cSize.X;
            //double h = (((env.YMax - env.YMin) / cSize.Y) + 1)*cSize.Y;
            //double lw = (System.Convert.ToInt32((env.XMin - rsExtent.XMin) / cSize.X) - 1)*cSize.X;
            //double lh = (System.Convert.ToInt32((env.YMin - rsExtent.YMin) / cSize.Y) - 1)*cSize.Y;
            //env.XMin = rsExtent.XMin + lw;
            //env.YMin = rsExtent.YMin + lh;
            //env.XMax = env.XMin + w;
            //env.YMax = env.YMin + h;
            //rsFunc.RasterInfo.Extent = env;
            //esriUtil.FunctionRasters.clipFunctionArgument args = new esriUtil.FunctionRasters.clipFunctionArgument();// ClipFunctionArgumentsClass();
            //args.Geometry = geo;
            IClipFunctionArguments args = new ClipFunctionArgumentsClass();
            args.Extent = env;
            //args.ClipType = clipType;
            args.ClippingGeometry = geo;
            args.ClippingType = clipType;
            args.Raster = rRst;
            //args.InRaster = rRst;
            frDset.Init(rsFunc, args);
            //frDset.Simplify();
            return createRaster((IRasterDataset)frDset);
        }
        /// <summary>
        /// Creates an in memory raster
        /// </summary>
        /// <param name="inRaster">template raster</param>
        /// <param name="outName">new name </param>
        /// <param name="wks">workspace</param>
        /// <returns>rasterdataset</returns>
        public IRasterDataset CreateMemoryRaster(IRaster inRaster, string outName, IWorkspace wks)
        {
            string txt = "MEM";
            esriWorkspaceType tp = wks.Type;
            if (outName.Length > 12)
            {
                outName.Substring(12);
            }
            ISaveAs sv = (ISaveAs)inRaster;
            IRasterDataset rsDset = (IRasterDataset)sv.SaveAs(outName, wks, txt);
            calcStatsAndHist(rsDset);
            return rsDset;
        }
        /// <summary>
        /// looks to see if raster exists
        /// </summary>
        /// <param name="wks"></param>
        /// <param name="inName"></param>
        /// <returns></returns>
        public bool rasterExists(IWorkspace wks, string inName)
        {

            return geoUtil.ftrExists(wks,inName);
        }
        /// <summary>
        /// checks to see if a name exists and if so returns a new string name with a prefix _
        /// </summary>
        /// <param name="wks"></param>
        /// <param name="inName"></param>
        /// <returns></returns>
        public string getSafeOutputName(IWorkspace wks, string inName)
        {
            string rstOut = inName;
            if (rstOut.Length > 12) rstOut.Substring(0, 12);
            foreach (string s in new string[] { " ","`", "~", "!", ".", ",", "@", "#", "$", "%", "^", "&", "*", "(", ")", "+", "=", "-" })
            {
                rstOut = rstOut.Replace(s, "_");
            }
            while (((System.IO.Directory.Exists(wks.PathName+"\\"+rstOut)==true)||(geoUtil.ftrExists(wks, rstOut)==true))==true)
            {
                if (rstOut == "____________") break;
                if (rstOut.Length > 11)
                {
                    rstOut =  "_" + rstOut.Substring(0, 10);
                }
                else
                {
                    rstOut =  "_"+rstOut;
                }
            }
            return rstOut;
        }
        /// <summary>
        /// calculates a and function
        /// </summary>
        /// <param name="rs1"></param>
        /// <param name="rs2"></param>
        /// <returns></returns>
        public IRaster calcAndFunction(object rs1, object rs2)
        {
            IRaster rs3 = calcGreaterEqualFunction(rs1, 1);
            IRaster rs4 = calcGreaterEqualFunction(rs2, 1);
            IRaster rs5 = calcArithmaticFunction(rs3, rs4, esriRasterArithmeticOperation.esriRasterPlus);
            IRaster outRs = calcEqualFunction(rs5,2);
            return outRs;
        }
        /// <summary>
        /// calculates a or function
        /// </summary>
        /// <param name="rs1"></param>
        /// <param name="rs2"></param>
        /// <returns></returns>
        public IRaster calcOrFunction(object rs1, object rs2)
        {
            IRaster rs3 = calcGreaterEqualFunction(rs1, 1);
            IRaster rs4 = calcGreaterEqualFunction(rs2, 1);
            IRaster rs5 = calcArithmaticFunction(rs3, rs4, esriRasterArithmeticOperation.esriRasterPlus);
            IRaster outRs = calcGreaterEqualFunction(rs5, 1);
            return outRs;
        }
        /// <summary>
        /// creates a composite band function
        /// </summary>
        /// <param name="rsArray"></param>
        /// <returns></returns>
        public IRaster compositeBandFunction(IRaster[] rsArray)
        {
            IRasterBandCollection rsBc = new RasterClass();
            foreach(IRaster rs in rsArray)
            {
                rsBc.AppendBands((IRasterBandCollection)rs);
            }
            return (IRaster)rsBc;
        }
        /// <summary>
        /// calculates a slope function
        /// </summary>
        /// <param name="inRaster"></param>
        /// <returns></returns>
        public IRaster calcSlopeFunction(IRaster inRaster)
        {
            IRaster rRst = returnRaster(inRaster,rstPixelType.PT_FLOAT);
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc = new SlopeFunctionClass();
            ISlopeFunctionArguments args = new SlopeFunctionArgumentsClass();
            args.DEM = rRst;
            args.ZFactor = 1;
            frDset.Init(rsFunc, args);
            IRaster outRs = createRaster((IRasterDataset)frDset);
            //functionModel.estimateStatistics(outRs, functionModel.dem.Slope);
            return outRs;
        }
        public IRaster calcTasseledCap7Function(object inRaster) //assumes at sensor reflectance
        {
            List<float[]> slopes = new List<float[]>();
            slopes.Add(new float[] { 0f, 0.3561f, 0.3972f, 0.3904f, 0.6966f, 0.2286f, 0.1596f });//brightness
            slopes.Add(new float[] { 0f, -0.3344f, -0.3544f, -0.4556f, 0.6966f, -0.0242f, -0.2630f });//greenness
            slopes.Add(new float[] { 0f, 0.2626f, 0.2141f, 0.0926f, 0.0656f, -0.7629f, -0.5388f });//wetness
            return calcRegressFunction(inRaster, slopes);
        }
        public IRaster calcNDVIFunction(object inRaster, int visibleBandId, int irBandId)
        {
            IRaster visRs = getBand(inRaster,visibleBandId);
            IRaster irRs = getBand(inRaster, irBandId);
            IRaster sRs = calcArithmaticFunction(irRs, visRs, esriRasterArithmeticOperation.esriRasterMinus);
            IRaster pRs = calcArithmaticFunction(irRs, visRs, esriRasterArithmeticOperation.esriRasterPlus);
            return calcArithmaticFunction(sRs, pRs, esriRasterArithmeticOperation.esriRasterDivide);
            //IRaster rRst = returnRaster(inRaster,rstPixelType.PT_FLOAT);
            //string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            //IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            //IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            //frDsetName.FullName = tempAr;
            //frDset.FullName = (IName)frDsetName;
            //IRasterFunction rsFunc = new NDVIFunctionClass();
            //INDVIFunctionArguments args = new NDVIFunctionArgumentsClass();
            //args.InfraredBandID = irBandId;
            //args.VisibleBandID = visibleBandId;
            //args.Raster = rRst;
            //frDset.Init(rsFunc, args);
            //IRaster outRs = createRaster((IRasterDataset)frDset);
            //functionModel.estimateStatistics(outRs, functionModel.dem.Slope);
            //return outRs;
        }
        /// <summary>
        /// calculates an aspect function
        /// </summary>
        /// <param name="inRaster"></param>
        /// <returns></returns>
        public IRaster calcAspectFunction(IRaster inRaster)
        {
            IRaster rRst = returnRaster(inRaster,rstPixelType.PT_FLOAT);
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc = new AspectFunctionClass();
            frDset.Init(rsFunc, inRaster);
            IRaster outRs = createRaster((IRasterDataset)frDset);
            //IRasterBandCollection rsBc = (IRasterBandCollection)outRs;
            //for (int i = 0; i < rsBc.Count; i++)
            //{
            //    IRasterBand rsB = rsBc.Item(i);
            //    IRasterBandEdit rsBE = (IRasterBandEdit)rsB;
            //    IRasterStatistics rStats = new RasterStatisticsClass();
            //    rsB.Statistics.Maximum = 360;
            //    rsB.Statistics.Minimum = 0;
            //    rsB.Statistics.Mean = 180;
            //    rsB.Statistics.StandardDeviation = 60;
            //    rsB.Statistics.SkipFactorX = 1;
            //    rsB.Statistics.SkipFactorY = 1;
            //    rsBE.AlterStatistics(rStats);
            //} 
            //functionModel.estimateStatistics(outRs,functionModel.dem.Aspect);
            return outRs;
        }
        
        /// <summary>
        /// converts an aspect raster to a nortsouth raster
        /// </summary>
        /// <param name="inRaster"></param>
        /// <returns></returns>
        public IRaster calcNorthSouthFunction(IRaster DEM)
        {
            IRaster rs = calcAspectFunction(DEM);
            IRaster rs2 = calcMathRasterFunction(rs,transType.RADIANS);
            IRaster outRs = calcMathRasterFunction(rs2, transType.COS);
            return outRs;
        }
        /// <summary>
        /// converts an aspect raster to a east west raster 
        /// </summary>
        /// <param name="inRaster"></param>
        /// <returns></returns>
        public IRaster calcEastWestFunction(IRaster DEM)
        {
            IRaster rs = calcAspectFunction(DEM);
            IRaster rs2 = calcMathRasterFunction(rs, transType.RADIANS);
            IRaster outRs = calcMathRasterFunction(rs2, transType.SIN);
            return outRs;
        }
        /// <summary>
        /// converts a raster to a polygon
        /// </summary>
        /// <param name="rs"></param>
        /// <param name="outWorkSpace"></param>
        /// <param name="outName"></param>
        /// <param name="smooth"></param>
        /// <returns></returns>
        public IFeatureClass convertRasterToPolygon(IRaster rs, IWorkspace outWorkSpace, string outName, bool smooth)
        {
            ESRI.ArcGIS.GeoAnalyst.IConversionOp convOp = new ESRI.ArcGIS.GeoAnalyst.RasterConversionOpClass();
            IGeoDataset geoDset = convOp.RasterDataToPolygonFeatureData((IGeoDataset)rs, outWorkSpace, outName, smooth);
            return (IFeatureClass)geoDset;
        }
        /// <summary>
        /// converts a raster to a polyline
        /// </summary>
        /// <param name="rs"></param>
        /// <param name="outWorkSpace"></param>
        /// <param name="outName"></param>
        /// <param name="zeroAsBackground"></param>
        /// <param name="smooth"></param>
        /// <returns></returns>
        public IFeatureClass convertRasterToPolyLine(IRaster rs, IWorkspace outWorkSpace, string outName, bool zeroAsBackground, bool smooth)
        {
            ESRI.ArcGIS.GeoAnalyst.IConversionOp convOp = new ESRI.ArcGIS.GeoAnalyst.RasterConversionOpClass();
            object minDangel = 0;
            IGeoDataset geoDset = convOp.RasterDataToLineFeatureData((IGeoDataset)rs, outWorkSpace, outName, zeroAsBackground, smooth, ref minDangel);
            return (IFeatureClass)geoDset;
        }
        /// <summary>
        /// converts a raster to a series of points
        /// </summary>
        /// <param name="rs"></param>
        /// <param name="outWorkSpace"></param>
        /// <param name="outName"></param>
        /// <returns></returns>
        public IFeatureClass convertRasterToPoint(IRaster rs, IWorkspace outWorkSpace, string outName)
        {
            ESRI.ArcGIS.GeoAnalyst.IConversionOp convOp = new ESRI.ArcGIS.GeoAnalyst.RasterConversionOpClass();
            object minDangel = 0;
            IGeoDataset geoDset = convOp.RasterDataToPointFeatureData((IGeoDataset)rs, outWorkSpace, outName);
            return (IFeatureClass)geoDset;
        }
        public IRaster convertFeatureClassToRaster(IFeatureClass featureClass, rasterUtil.rasterType rasterType, IWorkspace outWorkSpace, string outName, double cellSize, IRasterDataset snapRaster)
        {
            return convertFeatureClassToRaster(featureClass, rasterType, outWorkSpace, outName, cellSize, snapRaster, null);
        }
        public IRaster convertFeatureClassToRaster(IFeatureClass featureClass, rasterUtil.rasterType rasterType, IWorkspace outWorkSpace, string outName, double cellSize, IRasterDataset snapRaster, IEnvelope extent)
        {
            ESRI.ArcGIS.GeoAnalyst.IConversionOp convOp = new ESRI.ArcGIS.GeoAnalyst.RasterConversionOpClass();
            ESRI.ArcGIS.GeoAnalyst.IRasterAnalysisEnvironment rasterAnalysisEnvironment = (ESRI.ArcGIS.GeoAnalyst.IRasterAnalysisEnvironment)convOp;
            rasterAnalysisEnvironment.OutSpatialReference = ((IGeoDataset)featureClass).SpatialReference;
            rasterAnalysisEnvironment.OutWorkspace = outWorkSpace;
            object cellS = cellSize;
            object ext = ((IGeoDataset)featureClass).Extent;
            object snap = Type.Missing;
            if(snapRaster!=null)
            {
                snap = snapRaster;
            }
            if (extent != null)
            {
                ext = extent;
            }
            rasterAnalysisEnvironment.SetCellSize(ESRI.ArcGIS.GeoAnalyst.esriRasterEnvSettingEnum.esriRasterEnvValue, ref cellS);
            rasterAnalysisEnvironment.SetExtent(ESRI.ArcGIS.GeoAnalyst.esriRasterEnvSettingEnum.esriRasterEnvValue, ref ext,ref snap);
            string fmt = rasterType.ToString();
            if (fmt == "IMAGINE")
            {
                fmt = "IMAGINE image";
                if (!outName.ToLower().EndsWith(".img")) outName = outName + ".img";
            }
            IRasterDataset geoDset = convOp.ToRasterDataset((IGeoDataset)featureClass, fmt, outWorkSpace, outName);
            IGeoDatasetSchemaEdit2 geoSch = (IGeoDatasetSchemaEdit2)geoDset;
            if (geoSch.CanAlterSpatialReference) geoSch.AlterSpatialReference(rasterAnalysisEnvironment.OutSpatialReference);
            return returnRaster(geoDset);
        }
        /// <summary>
        /// Converts a raster to different pixel depth
        /// </summary>
        /// <param name="inRaster"></param>
        /// <param name="pType"></param>
        /// <returns></returns>
        public IRaster convertToDifFormatFunction(object inRaster, rstPixelType pType)
        {
            IRaster rs = returnRaster(inRaster);
            IRasterProps rsP = (IRasterProps)rs;
            System.Array noDataArr = (System.Array)rsP.NoDataValue;
            double newNoDataVl = getNoDataValue(pType);
            //double[] newNoDataArr = new double[noDataArr.Length];
            //for (int i = 0; i < noDataArr.Length; i++)
            //{
            //    newNoDataArr[i] = newNoDataVl;
            //}
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc = new IdentityFunctionClass();
            rsFunc.PixelType = pType;
            frDset.Init(rsFunc, rs);
            IRaster inRs = createRaster((IRasterDataset)frDset);
            IRaster outRs = setnullToValueFunction(inRs, newNoDataVl);
            IRasterProps oProp = (IRasterProps)outRs;
            oProp.NoDataValue = newNoDataVl;
            tempAr = funcDir + "\\" + FuncCnt + ".afr";
            frDset = new FunctionRasterDatasetClass();
            frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            rsFunc = new IdentityFunctionClass();
            frDset.Init(rsFunc, outRs);
            return createRaster((IRasterDataset)frDset);
        }
        public IRaster setnullToValueFunction(object inRaster, double vl)
        {
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc = new FunctionRasters.nullToValueFunctionDataset();
            FunctionRasters.nullToValueFunctionArguments args = new FunctionRasters.nullToValueFunctionArguments(this);
            args.InRaster = returnRaster(inRaster);
            args.NewValue = vl;
            frDset.Init(rsFunc, args);
            IRaster outRs = createRaster((IRasterDataset)frDset);
            return outRs;
        }
        /// <summary>
        /// creates a mask of valid values (greater than equal to min and less than equal to max)
        /// </summary>
        /// <param name="inRaster"></param>
        /// <param name="BandRanges"></param>
        /// <returns></returns>
        public IRaster maskDataRange(object inRaster, double[][] BandRanges)
        {
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc = new MaskFunctionClass();
            IMaskFunctionArguments args = new MaskFunctionArgumentsClass();
            IDoubleArray dbArray = new DoubleArrayClass();
            foreach (double[] d in BandRanges)
            {
                dbArray.Add(d[0]);
                dbArray.Add(d[1]);
            }
            args.Raster = returnRaster(inRaster);
            args.IncludedRanges = dbArray;
            frDset.Init(rsFunc, args);
            IRaster outRs = createRaster((IRasterDataset)frDset);
            return outRs;

        }
        public IRaster setNullValue(object inRaster, int vl)
        {
            IRaster rs = returnRaster(inRaster);
            IRasterBandCollection rsBc = (IRasterBandCollection)rs;
            IStringArray stArr = new StrArrayClass();
            for (int i = 0; i < rsBc.Count; i++)
            {
                stArr.Add(vl.ToString());
                stArr.Add(vl.ToString());
            }
            return setValueRangeToNodata(rs,stArr);
            //IRaster rs = returnRaster(inRaster);
            //IRasterProps rsProps = (IRasterProps)rs;
            //IRasterBandCollection rsBc = (IRasterBandCollection)rs;
            //int[] nodataArray = new int[rsBc.Count];
            //for (int i = 0; i < rsBc.Count; i++)
            //{
            //    nodataArray[i] = vl;
            //}
            //rsProps.NoDataValue = nodataArray;
            //return rs;
        }
        public IRaster setValueRangeToNodata(object inRaster,IStringArray sArray)
        {
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc = new MaskFunctionClass();
            IMaskFunctionArguments args = new MaskFunctionArgumentsClass();
            args.Raster = returnRaster(inRaster);
            args.NoDataValues = sArray;
            frDset.Init(rsFunc, args);
            IRaster outRs = createRaster((IRasterDataset)frDset);
            return outRs;

            //IRaster rs = returnRaster(inRaster);

            //IRasterProps rsProps = (IRasterProps)rs;
            //IRasterBandCollection rsBc = (IRasterBandCollection)rs;
            //int bCnt = rsBc.Count;
            //System.Array noDataArr = (System.Array)rsProps.NoDataValue;
            //IRasterBandCollection rsBcOut = new RasterClass();
            //for (int i = 0; i < bCnt; i++)
            //{
            //    IRaster brs = getBand(rs, i);
            //    double noData = System.Convert.ToDouble(noDataArr.GetValue(i));
            //    IRemapFilter rFilt = new RemapFilterClass();
            //    foreach (double[] d in minMaxList)
            //    {
            //        rFilt.AddClass(d[0], d[1], noData);
            //    }
            //    rsBcOut.AppendBands((IRasterBandCollection)calcRemapFunction(brs, rFilt));
            //}
            //return (IRaster)rsBcOut;
        }
        /// <summary>
        /// retrieves the appropriate no data value for a given rstPixeltype
        /// </summary>
        /// <param name="pType">type of pixel</param>
        /// <returns></returns>
        public static double getNoDataValue(rstPixelType pType)
        {
            double minVl = Double.MinValue;
            switch (pType)
            {
                case rstPixelType.PT_CHAR:
                    minVl = SByte.MinValue;
                    break;
                case rstPixelType.PT_FLOAT:
                    minVl = Single.MinValue;
                    break;
                case rstPixelType.PT_LONG:
                    minVl = Int32.MinValue;
                    break;
                case rstPixelType.PT_SHORT:
                    minVl = Int16.MinValue;
                    break;
                case rstPixelType.PT_U1:
                    minVl = 2;
                    break;
                case rstPixelType.PT_U2:
                    minVl = Math.Pow(2,2);
                    break;
                case rstPixelType.PT_U4:
                    minVl = Math.Pow(2,4);
                    break;
                case rstPixelType.PT_UCHAR:
                    minVl = Byte.MaxValue;
                    break;
                case rstPixelType.PT_ULONG:
                    minVl = UInt32.MaxValue;
                    break;
                case rstPixelType.PT_USHORT:
                    minVl = UInt16.MaxValue;
                    break;
                default:
                    break;
            }
            return minVl;
        }
        /// <summary>
        /// builds a vat table for a raster
        /// </summary>
        /// <param name="inRaster"></param>
        public ITable buildVat(object inRaster)
        {
            IRaster2 rs = (IRaster2)returnRaster(inRaster);
            IRasterProps prop = (IRasterProps)rs;
            rstPixelType rsType = prop.PixelType;
            if (rsType == rstPixelType.PT_FLOAT || rsType == rstPixelType.PT_DOUBLE)
            {
                return null;
            }
            IRasterDataset rsDset = rs.RasterDataset;
            IRasterDatasetEdit2 rsDsetE = (IRasterDatasetEdit2)rsDset;
            if (rs.AttributeTable != null)
            {
                rsDsetE.DeleteAttributeTable();
            }
            rsDsetE.BuildAttributeTable();
            rs = (IRaster2)((IRasterDataset2)rsDset).CreateFullRaster();
            return rs.AttributeTable;
        }
        /// <summary>
        /// defines unique regions using a 4 neighbor window
        /// </summary>
        /// <param name="inRaster"></param>
        /// <param name="wks"></param>
        /// <param name="outName"></param>
        /// <returns></returns>
        public IRaster regionGroup(object inRaster)
        {
            IRaster iR1 = returnRaster(inRaster);
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc = new FunctionRasters.regionGroupFunctionDataset();
            FunctionRasters.regionGroupFunctionArguments args = new FunctionRasters.regionGroupFunctionArguments(this);
            args.InRaster = iR1;
            frDset.Init(rsFunc, args);
            IRaster rs = createRaster((IRasterDataset)frDset);
            return rs;
        }
        
        /// <summary>
        /// performs block summarization
        /// </summary>
        /// <param name="inRaster"></param>
        /// <param name="outWks"></param>
        /// <param name="outRsName"></param>
        /// <param name="numCells"></param>
        /// <returns></returns>
        public IRaster calcAggregationFunction(object inRaster, int cells, focalType statType)
        {
            IRaster iR1 = returnRaster(inRaster);
            string tempAr = funcDir + "\\" + FuncCnt + ".afr";
            IFunctionRasterDataset frDset = new FunctionRasterDatasetClass();
            IFunctionRasterDatasetName frDsetName = new FunctionRasterDatasetNameClass();
            frDsetName.FullName = tempAr;
            frDset.FullName = (IName)frDsetName;
            IRasterFunction rsFunc = null;
            switch (statType)
            {
                case focalType.MIN:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.aggregationHelperMin();
                    break;
                case focalType.SUM:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.aggregationHelperSum();
                    break;
                case focalType.MEAN:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.aggregationHelperMean();
                    break;
                case focalType.MODE:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.aggregationHelperMode();
                    break;
                case focalType.MEDIAN:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.aggregationHelperMedian();
                    break;
                case focalType.VARIANCE:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.aggregationHelperVar();
                    break;
                case focalType.STD:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.aggregationHelperStd();
                    break;
                case focalType.UNIQUE:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.aggregationHelperUnique();
                    break;
                case focalType.ENTROPY:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.aggregationHelperEntropy();
                    break;
                case focalType.ASM:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.aggregationHelperASM();
                    break;
                default:
                    rsFunc = new FunctionRasters.NeighborhoodHelper.aggregationHelperMax();
                    break;
            }
            FunctionRasters.aggregationFunctionArguments args = new FunctionRasters.aggregationFunctionArguments(this);
            args.Cells = cells;
            args.InRaster = iR1;
            frDset.Init(rsFunc, args);
            IRaster outRs = createRaster((IRasterDataset)frDset);
            return outRs;
        }
       
        /// <summary>
        /// retrieves a safe value for a given a raster pixel type
        /// </summary>
        /// <param name="value"></param>
        /// <param name="pType"></param>
        /// <returns></returns>
        public static object getSafeValue(double value, rstPixelType pType)
        {
            object safeValue = value;
            switch (pType)
            {
                case rstPixelType.PT_CHAR:
                    safeValue = System.Convert.ToSByte(value);
                    break;
                case rstPixelType.PT_CLONG:
                case rstPixelType.PT_COMPLEX:
                case rstPixelType.PT_CSHORT:
                case rstPixelType.PT_DCOMPLEX:
                case rstPixelType.PT_DOUBLE:
                    safeValue = value;
                    break;
                case rstPixelType.PT_FLOAT:
                    safeValue = System.Convert.ToSingle(value);
                    break;
                case rstPixelType.PT_LONG:
                    safeValue = System.Convert.ToInt32(value);
                    break;
                case rstPixelType.PT_SHORT:
                    safeValue = System.Convert.ToInt16(value);
                    break;
                case rstPixelType.PT_U1:
                    safeValue = System.Convert.ToByte(value);
                    break;
                case rstPixelType.PT_U2:
                    safeValue = System.Convert.ToByte(value);
                    break;
                case rstPixelType.PT_U4:
                    safeValue = System.Convert.ToByte(value);
                    break;
                case rstPixelType.PT_UCHAR:
                    safeValue = System.Convert.ToByte(value);
                    break;
                case rstPixelType.PT_ULONG:
                    safeValue = System.Convert.ToUInt32(value);
                    break;
                case rstPixelType.PT_UNKNOWN:
                    safeValue = value;
                    break;
                case rstPixelType.PT_USHORT:
                    safeValue = System.Convert.ToUInt16(value);
                    break;
                default:
                    break;
            }
            return safeValue;
        }
        public IRaster mosaicRastersFunction(IWorkspace wks, string mosaicName, IRaster[] rasters)
        {
            return mosaicRastersFunction(wks, mosaicName, rasters,esriMosaicMethod.esriMosaicNone,rstMosaicOperatorType.MT_FIRST,true, true, true, true);

        }
        public IRaster mosaicRastersFunction(IWorkspace wks, string mosaicName, IRaster[] rasters, esriMosaicMethod mosaicmethod, rstMosaicOperatorType mosaictype, bool buildfootprint, bool buildboudary, bool seamlines, bool buildOverview)
        {
            IRaster rs1 = rasters[0];
            IEnvelope env = getCombinedExtents(rasters);
            int ht = System.Convert.ToInt32(env.Height);
            int wd = System.Convert.ToInt32(env.Width);
            int rec = System.Convert.ToInt32(ht * wd);
            IRasterProps rs1Props = (IRasterProps)rs1;
            IRasterBandCollection rsBc = (IRasterBandCollection)rs1;
            IGeoDataset rs1_2 = (IGeoDataset)rs1;
            ISpatialReference sr = rs1_2.SpatialReference;
            string mNm = getSafeOutputName(wks,mosaicName);
            IMosaicDataset msDset = createMosaicDataset(wks, sr, mNm, rs1Props.PixelType, rsBc.Count);
            msDset.MosaicFunction.MaxMosaicImageCount = rasters.Length;
            msDset.MosaicFunction.MosaicMethod = mosaicmethod;
            msDset.MosaicFunction.MosaicOperatorType = mosaictype;
            IFunctionRasterDataset fDset = (IFunctionRasterDataset)msDset;
            IPropertySet pSet = (fDset).Properties;
            pSet.SetProperty("MaxImageHeight", ht);
            pSet.SetProperty("MaxImageWidth", wd);
            pSet.SetProperty("MaxRecordCount", rec);
            pSet.SetProperty("DefaultResamplingMethod", 0);
            pSet.SetProperty("MaxMosaicImageCount", rasters.Length);
            pSet.SetProperty("MaxDownloadImageCount", rasters.Length);
            pSet.SetProperty("IsPreprocessedData", "True");
            pSet.SetProperty("MosaicOperator", 4);
            pSet.SetProperty("MosaicMethod", 0);
            fDset.Properties = pSet;
            IMosaicDatasetOperation msDsetOp = (IMosaicDatasetOperation)msDset;
            addRastersToMosaicDataset(msDset, rasters);
            ICalculateCellSizeRangesParameters computeArgs = new CalculateCellSizeRangesParametersClass();
            msDsetOp.CalculateCellSizeRanges(computeArgs, null);
            if (buildfootprint)
            {
                IBuildFootprintsParameters fpArgs = new BuildFootprintsParametersClass();
                fpArgs.Method = esriBuildFootprintsMethods.esriBuildFootprintsByGeometry;
                msDsetOp.BuildFootprints(fpArgs, null);
            }
            if(buildboudary)
            {
                IBuildBoundaryParameters bndArgs = new BuildBoundaryParametersClass();
                bndArgs.AppendToExistingBoundary=true;
                msDsetOp.BuildBoundary(bndArgs,null);
            }
            if(seamlines)
            {
                IBuildSeamlinesParameters smArgs = new BuildSeamlinesParametersClass();
                smArgs.ModifySeamlines = true;
                msDsetOp.BuildSeamlines(smArgs, null);
            }
            if (buildOverview)
            {
                IDefineOverviewsParameters ofPar = new DefineOverviewsParametersClass();
                ofPar.ForceOverviewTiles = true;
                ((IOverviewTileParameters)ofPar).OverviewFactor = 3;
                msDsetOp.DefineOverviews(ofPar, null);
                IGenerateOverviewsParameters ovArgs = new GenerateOverviewsParametersClass();
                ovArgs.GenerateMissingImages = true;
                ovArgs.GenerateStaleImages = true;
                msDsetOp.GenerateOverviews(ovArgs, null);
            }
            fDset.Init((IRasterFunction)msDset.MosaicFunction, msDset.MosaicFunctionArguments);
            IRaster rs = createRaster((IRasterDataset)fDset);
            return rs;
        }

        private IEnvelope getCombinedExtents(IRaster[] rasters)
        {
            ISpatialReference sr1 = null;
            IEnvelope env = null;
            foreach (IRaster rs in rasters)
            {
                IRasterProps rsP = (IRasterProps)rs;
                IEnvelope ext = rsP.Extent;
                ISpatialReference sr2 = ext.SpatialReference;
                //Console.WriteLine(ext.SpatialReference.Name);
                if (env == null)
                {
                    env = ext;
                    sr1 = ext.SpatialReference;
                }
                else
                {
                    if (sr1.Name != sr2.Name)
                    {
                        ext.SpatialReference = env.SpatialReference;
                    }
                    env.Union(ext);
                   
                }
            }
            return env;
        }
       
        public void addRastersToMosaicDataset(IMosaicDataset mosaicDataSet, IRaster[] rasters)
        {
            IMosaicDatasetOperation mOp = (IMosaicDatasetOperation)mosaicDataSet;
            foreach(IRaster rs in rasters)
            {
                IAddRastersParameters addRs = new AddRastersParametersClass();
                IRasterDatasetCrawler rsDsetCrawl = new RasterDatasetCrawlerClass();
               
                rsDsetCrawl.RasterDataset = ((IRaster2)rs).RasterDataset;
                IRasterTypeFactory rsFact = new RasterTypeFactoryClass();
                IRasterType rsType = rsFact.CreateRasterType("Raster dataset");
                rsType.FullName = rsDsetCrawl.DatasetName;
                addRs.Crawler = (IDataSourceCrawler)rsDsetCrawl;
                addRs.RasterType = rsType;
                mOp.AddRasters(addRs, null);
            }
            return;

        }
        public IMosaicDataset createMosaicDataset(IWorkspace wks, ISpatialReference spatialReference, string mosaicDataSetName, rstPixelType pType, int numBands)
        {
            ICreateMosaicDatasetParameters crParam = new CreateMosaicDatasetParametersClass();
            crParam.BandCount = numBands;
            crParam.PixelType = pType;
            IMosaicWorkspaceExtensionHelper mosaicExtHelper = new MosaicWorkspaceExtensionHelperClass();
            IMosaicWorkspaceExtension mosaicExt = mosaicExtHelper.FindExtension(wks);
            IMosaicDataset mosaicDset = mosaicExt.CreateMosaicDataset(mosaicDataSetName, spatialReference,(ICreateMosaicDatasetParameters)crParam,"");
            return mosaicDset;
        }
        public IMosaicDataset openMosaicDataset(IWorkspace wks, string mosaicDatasetName)
        {
            IMosaicWorkspaceExtensionHelper mosaicExtHelper = new MosaicWorkspaceExtensionHelperClass();
            IMosaicWorkspaceExtension mosaicExt = mosaicExtHelper.FindExtension(wks);
            IMosaicDataset mosaicDset = mosaicExt.OpenMosaicDataset(mosaicDatasetName);
            return mosaicDset;
        }

        public IRaster mergeRasterFunction(IRaster[] inRasters,rstMosaicOperatorType mergeMethod,string rstNm)
        {
            string dbStr = mosaicDir + "\\rsCatDb.gdb";
            IWorkspace wks = null;
            if (!System.IO.Directory.Exists(dbStr))
            {
                wks = geoUtil.CreateWorkSpace(mosaicDir, "rsCatDb.gdb");
            }
            else
            {
                wks = geoUtil.OpenRasterWorkspace(dbStr);
            }
            rstNm = getSafeOutputName(wks,rstNm);
            IRaster rs = mosaicRastersFunction(wks, rstNm, inRasters,esriMosaicMethod.esriMosaicNone,mergeMethod,false,false,false,false);
            return rs;
        }
        public ITable zonalStats(IFeatureClass inFeatureClass, string fieldName, IRaster inValueRaster, string outTableName, zoneType[] zoneTypes,esriUtil.Forms.RunningProcess.frmRunningProcessDialog rd,bool classCounts=false)
        {
            FunctionRasters.zonalHelper zH = new FunctionRasters.zonalHelper(this,rd);
            zH.InValueRaster = returnRaster(inValueRaster);
            //zH.convertFeatureToRaster(inFeatureClass, fieldName);
            zH.InZoneFeatureClass = inFeatureClass;
            zH.InZoneField = fieldName;
            zH.ZoneTypes = zoneTypes;
            zH.OutTableName = outTableName;
            zH.ZoneClassCount = classCounts;
            zH.setZoneValues();
            return zH.OutTable;
        }
        public ITable zonalStats(IRaster inZoneRaster, IRaster inValueRaster, string outTableName, zoneType[] zoneTypes, esriUtil.Forms.RunningProcess.frmRunningProcessDialog rd,bool classCounts=false)
        {
            FunctionRasters.zonalHelper zH = new FunctionRasters.zonalHelper(this,rd);
            zH.InValueRaster = returnRaster(inValueRaster);
            zH.InZoneRaster = returnRaster(inZoneRaster);
            zH.ZoneTypes = zoneTypes;
            zH.OutTableName = outTableName;
            zH.ZoneClassCount = classCounts;
            zH.setZoneValues();
            return zH.OutTable;
        }
        public IGeometry extractDomain(IRaster rs, bool pCenterbased=false)
        {
            IRaster rs1 = rs;
            if (((IRasterBandCollection)rs1).Count > 1)
            {
                rs1 = getBand(rs, 0);
            }
            IRasterDomainExtractor dEx = new RasterDomainExtractorClass();
            IPolygon poly = dEx.ExtractDomain(rs1,pCenterbased);
            return (IGeometry)poly;
        }

        public static void cleanupTempDirectories()
        {
            string mainPath = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string func = mainPath + "\\RmrsRasterUtilityHelp\\func";
            string mos = mainPath + "\\RmrsRasterUtilityHelp\\mosaic";
            string conv = mainPath + "\\RmrsRasterUtilityHelp\\conv";
            string[] dirs = {func,mos,conv};
            foreach (string s in dirs)
            {
                try
                {
                    System.IO.DirectoryInfo dInfo = new System.IO.DirectoryInfo(s);
                    if(dInfo.Exists) dInfo.Delete(true);
                }
                catch
                {
                }
            }
        }
        public void removeLock(IDataset rDset)
        {
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(rDset.Workspace);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(rDset);
        }
        
        public static bool isNullData(object inValue, object noDataValue)
        {
            try
            {
                double inVl = System.Convert.ToDouble(inValue);
                double ndVl = System.Convert.ToDouble(noDataValue);
                if (inVl.Equals(ndVl) || Double.IsNaN(inVl) || Double.IsInfinity(inVl))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                Console.WriteLine("failed isNullData " + inValue.ToString());
                return true;
            }
        }        
    }
}
