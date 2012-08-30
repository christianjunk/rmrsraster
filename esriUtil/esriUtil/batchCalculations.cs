﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;

namespace esriUtil
{
    class batchCalculations
    {
        public batchCalculations()
        {
            rsUtil = new rasterUtil();
        }
        public batchCalculations(rasterUtil rasterUtility, esriUtil.Forms.RunningProcess.frmRunningProcessDialog runningDialog)
        {
            rsUtil = rasterUtility;
            if (rp != null) rp = runningDialog;
        }
        public enum batchGroups { ARITHMETIC, MATH, SETNULL, LOGICAL, CLIP, CONDITIONAL, CONVOLUTION, FOCAL, FOCALSAMPLE, LOCALSTATISTICS, LINEARTRANSFORM, RESCALE, REMAP, COMPOSITE, EXTRACTBAND, CONVERTPIXELTYPE, GLCM, LANDSCAPE, ZONALSTATS, SAVEFUNCTIONRASTER, ADDRASTERTOMAP, ADDFEATURECLASSTOMAP, BUILDRASTERSTATS, BUILDRASTERVAT, MOSAIC, MERGE, SAMPLERASTER, CLUSTERSAMPLERASTER, CREATERANDOMSAMPLE, CREATESTRATIFIEDRANDOMSAMPLE, CREATEREGRESSIONRASTER };
        private rasterUtil rsUtil = null;
        private System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
        private System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
        private geoDatabaseUtility geoUtil = new geoDatabaseUtility();
        private string path = "";
        public string BatchPath 
        {
            get 
            { 
                return path; 
            } 
            set 
            { 
                path = value; 
            } 
        }
        private esriUtil.Forms.RunningProcess.frmRunningProcessDialog rp = new Forms.RunningProcess.frmRunningProcessDialog(false);
        private Dictionary<string, IRaster> rstDic = new Dictionary<string, IRaster>();
        private Dictionary<string, IFeatureClass> ftrDic = new Dictionary<string, IFeatureClass>();
        private Dictionary<string, ITable> tblDic = new Dictionary<string, ITable>();
        public Dictionary<string, IRaster> RasterDictionary { get { return rstDic; } set { rstDic = value; } }
        public Dictionary<string, IFeatureClass> FeatureClassDictionary { get { return ftrDic; } set { ftrDic = value; } }
        public Dictionary<string, ITable> TableDictionary { get { return tblDic; } set { tblDic = value; } }
        private List<string> lnLst = new List<string>();
        public void manuallyAddBatchLines(string[] lines)
        {
            lnLst.Clear();
            foreach (string s in lines)
            {
                lnLst.Add(s);
            }
        }
        public void saveBatchFile(string[] lines)
        {
            sfd.AddExtension = true;
            sfd.Filter = "Batch File|*.bch";
            sfd.DefaultExt = "bch";
            System.Windows.Forms.DialogResult dr = sfd.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                string outFileName = sfd.FileName;
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(outFileName,false))
                {
                    foreach (string s in lines)
                    {
                        if (s.Length > 0)
                        {
                            sw.WriteLine(s);
                        }
                    }
                    sw.Close();
                }
                path = outFileName;
                loadBatchFile();
            }

        }
        public string openBatchFile()
        {
            string ostr = "";
            ofd.AddExtension = true;
            ofd.DefaultExt = "bch";
            ofd.Multiselect = false;
            ofd.Title = "Open Batch File";
            ofd.Filter = "Batch File|*.bch";
            System.Windows.Forms.DialogResult ds = ofd.ShowDialog();
            if (ds == System.Windows.Forms.DialogResult.OK)
            {
                path = ofd.FileName;
                ostr = loadBatchFile();
            }
            return ostr;
        }
        public string loadBatchFile()
        {
            //Console.WriteLine(path);
            StringBuilder sb = new StringBuilder();
            lnLst.Clear();
            using (System.IO.StreamReader sr = new System.IO.StreamReader(path))
            {
                string ln = sr.ReadLine();
                while(ln!=null)
                {
                    Console.WriteLine(ln);
                    if (ln.Length > 0)
                    {
                        sb.AppendLine(ln);
                        lnLst.Add(ln);
                    }
                    ln = sr.ReadLine();
                }
                sr.Close();
            }
            return sb.ToString();
        }
        public void runBatch()
        {
            rp.addMessage("Running in batch mode...");
            DateTime dt = DateTime.Now;
            try
            {
                string func = "";
                string param = "";
                string outName = "";
                foreach (string ln in lnLst)
                {
                    rp.stepPGBar(5);
                    rp.Refresh();
                    if (ln.Length > 0)
                    {
                        getFunctionAndParameters(ln, out func, out param, out outName);
                        if (func == "" || param == "" || outName == "")
                        {
                            continue;
                        }
                        else
                        {
                            try
                            {
                                rp.addMessage("Running process: " + ln);
                                rp.Refresh();
                                batchGroups bg = (batchGroups)Enum.Parse(typeof(batchGroups), func);
                                string[] paramArr = param.Split(new char[] { ';' });
                                switch (bg)
                                {
                                    case batchGroups.ARITHMETIC:
                                        rstDic[outName] = createArithmeticFunction(paramArr);
                                        break;
                                    case batchGroups.MATH:
                                        rstDic[outName] = createMathFunction(paramArr);
                                        break;
                                    case batchGroups.SETNULL:
                                        rstDic[outName] = createSetNullFunction(paramArr);
                                        break;
                                    case batchGroups.LOGICAL:
                                        rstDic[outName] = createLogicalFunction(paramArr);
                                        break;
                                    case batchGroups.CLIP:
                                        rstDic[outName] = createClipFunction(paramArr);
                                        break;
                                    case batchGroups.CONDITIONAL:
                                        rstDic[outName] = createConditionalFunction(paramArr);
                                        break;
                                    case batchGroups.CONVOLUTION:
                                        rstDic[outName] = createConvolutionFunction(paramArr);
                                        break;
                                    case batchGroups.FOCAL:
                                        rstDic[outName] = createFocalFunction(paramArr);//pickback up here
                                        break;
                                    case batchGroups.LOCALSTATISTICS:
                                        rstDic[outName] = createLocalFunction(paramArr);
                                        break;
                                    case batchGroups.LINEARTRANSFORM:
                                        rstDic[outName] = createLinearTransformFunction(paramArr);
                                        break;
                                    case batchGroups.RESCALE:
                                        rstDic[outName] = createRescaleFunction(paramArr);
                                        break;
                                    case batchGroups.REMAP:
                                        rstDic[outName] = createRemapFunction(paramArr);
                                        break;
                                    case batchGroups.COMPOSITE:
                                        rstDic[outName] = createCompositeFunction(paramArr);
                                        break;
                                    case batchGroups.EXTRACTBAND:
                                        rstDic[outName] = createExtractFunction(paramArr);
                                        break;
                                    case batchGroups.GLCM:
                                        rstDic[outName] = createGLCMFunction(paramArr);
                                        break;
                                    case batchGroups.LANDSCAPE:
                                        rstDic[outName] = createLandscapeFunction(paramArr);
                                        break;
                                    case batchGroups.ZONALSTATS:
                                        tblDic[outName] = createZonalStats(paramArr);
                                        break;
                                    case batchGroups.SAVEFUNCTIONRASTER:
                                        rstDic[outName] = saveRaster(paramArr);
                                        break;
                                    case batchGroups.BUILDRASTERSTATS:
                                        rstDic[outName] = buildRasterStats(paramArr);
                                        break;
                                    case batchGroups.BUILDRASTERVAT:
                                        tblDic[outName] = buildRasterVat(paramArr);
                                        break;
                                    case batchGroups.MOSAIC:
                                        rstDic[outName] = createMosaic(paramArr);
                                        break;
                                    case batchGroups.MERGE:
                                        rstDic[outName] = createMerge(paramArr);
                                        break;
                                    case batchGroups.SAMPLERASTER:
                                        sampleRaster(paramArr);
                                        break;
                                    case batchGroups.CREATERANDOMSAMPLE:
                                        ftrDic[outName] = createRandomSample(paramArr);
                                        break;
                                    case batchGroups.CREATESTRATIFIEDRANDOMSAMPLE:
                                        ftrDic[outName] = createStratifiedRandomSample(paramArr);
                                        break;
                                    case batchGroups.CLUSTERSAMPLERASTER:
                                        clusterSampleRaster(paramArr);
                                        break;
                                    case batchGroups.FOCALSAMPLE:
                                        rstDic[outName] = createFocalSampleFunction(paramArr);
                                        break;
                                    case batchGroups.CREATEREGRESSIONRASTER:
                                        rstDic[outName] = createRegressionRaster(paramArr);
                                        break;
                                    case batchGroups.ADDRASTERTOMAP:
                                        addRasterToMap(paramArr);
                                        break;
                                    case batchGroups.ADDFEATURECLASSTOMAP:
                                        addFeatureClassToMap(paramArr);
                                        break;
                                    case batchGroups.CONVERTPIXELTYPE:
                                        rstDic[outName] = convertPixelType(paramArr);
                                        break;
                                    default:
                                        break;
                                }
                            }
                            catch (Exception e)
                            {
                                if (rp != null)
                                {
                                    rp.addMessage("Error in function " + func + "\n" + e.ToString());
                                }
                                Console.WriteLine(e.ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                rp.addMessage(e.ToString());
            }
            finally
            {
                DateTime dt2 = DateTime.Now;
                TimeSpan ts = dt2.Subtract(dt);
                string tsStr = ts.Days.ToString() + " days, " + ts.Minutes.ToString() + " minutes, and " + ts.Seconds.ToString() + " seconds";
                rp.stepPGBar(100);
                rp.addMessage("Fished Batch Process in " + tsStr);
                rp.enableClose();
            }
        }

        private IRaster convertPixelType(string[] paramArr)
        {
            IRaster inRaster = getRaster(paramArr[0]);
            rstPixelType pType = (rstPixelType)Enum.Parse(typeof(rstPixelType),paramArr[1]);
            return rsUtil.convertToDifFormatFunction(inRaster, pType);
        }

        private void addFeatureClassToMap(string[] paramArr)
        {
            try
            {
                Type t = Type.GetTypeFromCLSID(typeof(ESRI.ArcGIS.Framework.AppRefClass).GUID);
                System.Object obj = Activator.CreateInstance(t);
                ESRI.ArcGIS.Framework.IApplication app = (ESRI.ArcGIS.Framework.IApplication)obj;
                ESRI.ArcGIS.Framework.IDocument doc = app.Document;
                ESRI.ArcGIS.ArcMapUI.IMxDocument mxDoc = (ESRI.ArcGIS.ArcMapUI.IMxDocument)doc;
                IMap map = mxDoc.FocusMap;
                IFeatureClass ftrCls = getFeatureClass(paramArr[0]);
                IFeatureLayer ftrLyr = new FeatureLayerClass();
                ftrLyr.FeatureClass = ftrCls;
                ftrLyr.Name = paramArr[1];
                ftrLyr.Visible = false;
                map.AddLayer((ILayer)ftrLyr);
            }
            catch (Exception e)
            {
                if (rp != null)
                {
                    rp.addMessage(e.ToString());
                }
                else
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }

        private void addRasterToMap(string[] paramArr)
        {
            
            try
            {
                IRaster rs = getRaster(paramArr[0]);
                Type t = Type.GetTypeFromCLSID(typeof(ESRI.ArcGIS.Framework.AppRefClass).GUID);
                System.Object obj = Activator.CreateInstance(t);
                ESRI.ArcGIS.Framework.IApplication app = (ESRI.ArcGIS.Framework.IApplication)obj;
                ESRI.ArcGIS.Framework.IDocument doc = app.Document;
                ESRI.ArcGIS.ArcMapUI.IMxDocument mxDoc = (ESRI.ArcGIS.ArcMapUI.IMxDocument)doc;
                IMap map = mxDoc.FocusMap;
                IRasterLayer rsLyr = new RasterLayerClass();
                rsLyr.CreateFromRaster(rs);
                rsLyr.Name = paramArr[1];
                rsLyr.Visible = false;
                map.AddLayer((ILayer)rsLyr);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                
            }
            catch(Exception e)
            {
                if (rp != null)
                {
                    rp.addMessage(e.ToString());
                }
                else
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }

        private IRaster createRegressionRaster(string[] paramArr)
        {
            regressionRaster rr = new regressionRaster(ref rsUtil);
            string rrStr = paramArr[0];
            IRasterBandCollection rsBC = new RasterClass();
            foreach (string s in rrStr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                rsBC.AppendBands((IRasterBandCollection)getRaster(s.Trim()));
            }
            rr.InRaster = (IRaster)rsBC;
            rr.Dependentfield = getDependentField(paramArr[1]);
            rr.SasOutputFile = paramArr[1];
            return rr.createModelRaster(null);
        }

        private string getDependentField(string p)
        {
            List<string> depFlsLst = new List<string>();
            using (System.IO.StreamReader sr = new System.IO.StreamReader(p))
            {
                string sF = sr.ReadLine();
                string sV = sr.ReadLine();
                
                while (sV != null)
                {
                    depFlsLst.Add(sV.Split(new char[] { ',' })[2]);
                    sV = sr.ReadLine();
                }
                sr.Close();

            }
            return String.Join(" ", depFlsLst.ToArray());
        }

        private IRaster createFocalSampleFunction(string[] paramArr)
        {
            IRaster inRaster = getRaster(paramArr[0]);
            HashSet<string> offset = new HashSet<string>();
            foreach(string s in paramArr[1].Split(new char[]{','},StringSplitOptions.RemoveEmptyEntries))//azimuth:distance -> azimuth;distance
            {
                offset.Add(s.Trim().Replace(":",";"));
            }
            rasterUtil.focalType statType = (rasterUtil.focalType)Enum.Parse(typeof(rasterUtil.focalType),paramArr[2]);
            return rsUtil.calcFocalSampleFunction(inRaster, offset, statType);
        }

        private void clusterSampleRaster(string[] paramArr)
        {
            throw new NotImplementedException();
        }

        private IFeatureClass createStratifiedRandomSample(string[] paramArr)
        {
            throw new NotImplementedException();
        }

        private IFeatureClass createRandomSample(string[] paramArr)
        {
            throw new NotImplementedException();
        }

        private void sampleRaster(string[] paramArr)
        {
            IFeatureClass inFtrCls = getFeatureClass(paramArr[0]);
            IRaster sampleRst = getRaster(paramArr[1]);
            string inName = paramArr[2];
            if (inName == "" || inName == "null")
            {
                inName = null;
            }
            rsUtil.sampleRaster(inFtrCls, sampleRst, inName);
        }

        private IRaster createMerge(string[] paramArr)
        {
            throw new NotImplementedException();
        }

        private IRaster createMosaic(string[] paramArr)
        {
            throw new NotImplementedException();
        }

        private ITable buildRasterVat(string[] paramArr)
        {
            IRaster rs = getRaster(paramArr[0]);
            return rsUtil.buildVat(rs);
        }

        private IRaster buildRasterStats(string[] paramArr)
        {
            IRaster rs = getRaster(paramArr[0]);
            int skipFactor = System.Convert.ToInt32(paramArr[1]);
            return rsUtil.calcStatsAndHist(rs, skipFactor);
            
        }

        private IRaster saveRaster(string[] paramArr)
        {
            IRaster rs = getRaster(paramArr[0]);
            string outName = paramArr[1];
            IWorkspace wks = geoUtil.OpenRasterWorkspace(paramArr[2]);
            rasterUtil.rasterType rastertype = (rasterUtil.rasterType)Enum.Parse(typeof(rasterUtil.rasterType),paramArr[3].ToUpper());
            return rsUtil.returnRaster(rsUtil.saveRasterToDataset(rs, outName, wks, rastertype));
        }

        private IRaster createLandscapeFunction(string[] paramArr)
        {
            IRaster rs = getRaster(paramArr[0]);
            if (paramArr.Length < 4)
            {
                int radius = System.Convert.ToInt32(paramArr[1]);
                rasterUtil.focalType statType = (rasterUtil.focalType)Enum.Parse(typeof(rasterUtil.focalType), paramArr[2]);
                rasterUtil.landscapeType landType = (rasterUtil.landscapeType)Enum.Parse(typeof(rasterUtil.focalType), paramArr[3]);
                return rsUtil.calcLandscapeFunction(rs, radius, statType, landType);
            }
            else
            {
                int clm = System.Convert.ToInt32(paramArr[1]);
                int rws = System.Convert.ToInt32(paramArr[2]);
                rasterUtil.focalType statType = (rasterUtil.focalType)Enum.Parse(typeof(rasterUtil.focalType), paramArr[3]);
                rasterUtil.landscapeType landType = (rasterUtil.landscapeType)Enum.Parse(typeof(rasterUtil.focalType), paramArr[4]);
                return rsUtil.calcLandscapeFunction(rs, clm, rws, statType, landType);
            }
            

        }

        private IRaster createGLCMFunction(string[] paramArr)
        {
            IRaster inRaster = getRaster(paramArr[0]);
            bool horizontal = true;
            rasterUtil.glcmMetric glcmType = rasterUtil.glcmMetric.CONTRAST;
            if (paramArr.Length <5)
            {
                int radius = System.Convert.ToInt32(paramArr[1]);
                string horz = paramArr[2].ToLower();
                if (horz == "false")
                {
                    horizontal = false;
                }
                glcmType = (rasterUtil.glcmMetric)Enum.Parse(typeof(rasterUtil.glcmMetric), paramArr[3].ToUpper());
                return rsUtil.calcGLCMFunction(inRaster, radius, horizontal, glcmType);
            }
            else
            {
                int clms = System.Convert.ToInt32(paramArr[1]);
                int rws = System.Convert.ToInt32(paramArr[2]);
                string horz = paramArr[3].ToLower();
                if (horz == "false")
                {
                    horizontal = false;
                }
                glcmType = (rasterUtil.glcmMetric)Enum.Parse(typeof(rasterUtil.glcmMetric), paramArr[4].ToUpper());
                return rsUtil.calcGLCMFunction(inRaster, clms, rws, horizontal, glcmType);
            }
        }

        private IRaster createExtractFunction(string[] paramArr)
        {
            IRaster inRaster = getRaster(paramArr[0]);
            IRasterBandCollection rsBCO = (IRasterBandCollection)inRaster;
            IRasterBandCollection rsBC = new RasterClass();
            foreach (string s in paramArr[1].Split(new char[]{','}))
            {
                int bnd = System.Convert.ToInt32(s.Trim()) - 1;
                rsBC.AppendBand(rsBCO.Item(bnd));
            }
            return (IRaster)rsBC;
        }

        private IRaster createCompositeFunction(string[] paramArr)
        {
            IRasterBandCollection rsBC = new RasterClass();
            foreach(string s in paramArr[0].Split(new char[]{','}))
            {
                IRaster rs = getRaster(s);
                rsBC.AppendBands((IRasterBandCollection)rs);
            }
            return (IRaster)rsBC;
        }

        private IRaster createRemapFunction(string[] paramArr)
        {
            throw new NotImplementedException();
        }

        private IRaster createRescaleFunction(string[] paramArr)
        {
            throw new NotImplementedException();
        }

        private IRaster createLinearTransformFunction(string[] paramArr)
        {
            throw new NotImplementedException();
        }

        private IRaster createLocalFunction(string[] paramArr)
        {
            throw new NotImplementedException();
        }

        private IRaster createFocalFunction(string[] paramArr)
        {
            IRaster rs = getRaster(paramArr[0]);
            string fTypeStr = null;
            if (paramArr.Length < 4)
            {
                fTypeStr = paramArr[2];
                rasterUtil.focalType fType = (rasterUtil.focalType)Enum.Parse(typeof(rasterUtil.focalType), fTypeStr);
                int rad = System.Convert.ToInt32(paramArr[1]);
                return rsUtil.calcFocalStatisticsFunction(rs, rad, fType);
            }
            else
            {
                fTypeStr = paramArr[3];
                rasterUtil.focalType fType = (rasterUtil.focalType)Enum.Parse(typeof(rasterUtil.focalType), fTypeStr);
                int clms = System.Convert.ToInt32(paramArr[1]);
                int rws = System.Convert.ToInt32(paramArr[2]);
                return rsUtil.calcFocalStatisticsFunction(rs, clms, rws, fType);
            }
        }

        private IRaster createConvolutionFunction(string[] paramArr)
        {
            IRaster rs = getRaster(paramArr[0]);
            int wd = System.Convert.ToInt32(paramArr[1]);
            int ht = System.Convert.ToInt32(paramArr[2]);
            string krn = paramArr[3];
            List<double> dblLst = new List<double>();
            foreach (string s in krn.Split(new char[] { ',' }))
            {
                dblLst.Add(System.Convert.ToDouble(s));
            }
            return rsUtil.convolutionRasterFunction(rs, wd, ht, dblLst.ToArray());
        }

        private IRaster createConditionalFunction(string[] paramArr)
        {
            string inRs1Str = paramArr[1];
            string inRs2Str = paramArr[2];
            object inRs1 = null;
            object inRs2 = null;
            if (rsUtil.isNumeric(inRs1Str))
            {
                inRs1 = inRs1Str;
            }
            else
            {
                inRs1 = getRaster(inRs1Str);
            }
            if (rsUtil.isNumeric(inRs2Str))
            {
                inRs2 = inRs2Str;
            }
            else
            {
                inRs2 = getRaster(inRs2Str);
            }
            IRaster conRs = getRaster(paramArr[0]);
            return rsUtil.conditionalRasterFunction(conRs, inRs1, inRs2);
        }

        private IRaster createClipFunction(string[] paramArr)
        {
            IRaster rs = getRaster(paramArr[0]);
            IFeatureClass ftrCls = getFeatureClass(paramArr[1]);
            IGeometry geo = ((IGeoDataset)ftrCls).Extent;
            return rsUtil.clipRasterFunction(rs, geo, esriRasterClippingType.esriRasterClippingOutside);
        }

        private IRaster createLogicalFunction(string[] paramArr)
        {
            rasterUtil.logicalType opType = (rasterUtil.logicalType)Enum.Parse(typeof(rasterUtil.logicalType),paramArr[2].ToUpper());
            string p1 = paramArr[0];
            string p2 = paramArr[1];
            object inRs1 = null;
            object inRs2 = null;
            if (rsUtil.isNumeric(p1))
            {
                inRs1 = p1;
            }
            else
            {
                inRs1 = getRaster(p1);
            }
            if (rsUtil.isNumeric(p2))
            {
                inRs2 = p2;
            }
            else
            {
                inRs2 = getRaster(p2);
            }
            IRaster rs = null;
            switch (opType)
            {
                case rasterUtil.logicalType.GT:
                    rs = rsUtil.calcGreaterFunction(inRs1, inRs2);
                    break;
                case rasterUtil.logicalType.LT:
                    rs = rsUtil.calcLessFunction(inRs1, inRs2);
                    break;
                case rasterUtil.logicalType.GE:
                    rs = rsUtil.calcGreaterEqualFunction(inRs1, inRs2);
                    break;
                case rasterUtil.logicalType.LE:
                    rs = rsUtil.calcLessEqualFunction(inRs1, inRs2);
                    break;
                case rasterUtil.logicalType.EQ:
                    rs = rsUtil.calcEqualFunction(inRs1, inRs2);
                    break;
                case rasterUtil.logicalType.AND:
                    rs = rsUtil.calcAndFunction(inRs1, inRs2);
                    break;
                default:
                    rs = rsUtil.calcOrFunction(inRs1, inRs2);
                    break;
            }
            return rs;
        }

        private IRaster createSetNullFunction(string[] paramArr)
        {
            IRaster rs = getRaster(paramArr[0]);
            List<double[]> minMaxLst = new List<double[]>();
            string vlStr = paramArr[1];
            string[] vlArr = vlStr.Split(new char[] { ',' });
            foreach (string s in vlArr)
            {
                string[] rngArr = s.Split(new char[] { '-' });
                double[] mm = { System.Convert.ToDouble(rngArr[0]), System.Convert.ToDouble(rngArr[1]) };
                minMaxLst.Add(mm);
            }
            return rsUtil.setValueRangeToNodata(rs,minMaxLst);
        }

        private IRaster createMathFunction(string[] paramArr)
        {
            IRaster rs = getRaster(paramArr[0]);
            rasterUtil.transType tType = (rasterUtil.transType)Enum.Parse(typeof(rasterUtil.transType), paramArr[1]);
            return rsUtil.calcMathRasterFunction(rs, tType);
            
        }
        private ITable createZonalStats(string[] paramArr)
        {
            string zTStr = paramArr[paramArr.Length-1];
            string[] zTStrArr = zTStr.Split(new char[]{','});
            List<rasterUtil.zoneType> zLst = new List<rasterUtil.zoneType>();
            foreach(string s in zTStrArr)
            {
                rasterUtil.zoneType zT = (rasterUtil.zoneType)Enum.Parse(typeof(rasterUtil.zoneType),s.ToUpper());
                zLst.Add(zT);
            }
            if(paramArr.Length<4)
            {
                IRaster rs1 = getRaster(paramArr[0]);
                IRaster rs2 = getRaster(paramArr[1]);
                return rsUtil.zonalStats(rs1, rs2, zLst.ToArray(),rp);
            }
            else
            {
                IFeatureClass inFtrCls = getFeatureClass(paramArr[0]);
                string inFtrFld = paramArr[1];
                IRaster vRs = getRaster(paramArr[2]);
                return rsUtil.zonalStats(inFtrCls, inFtrFld, vRs, zLst.ToArray(),rp);
            }
        }

        private IFeatureClass getFeatureClass(string p)
        {
            string tp = p.Trim();
            if (ftrDic.ContainsKey(tp))
            {
                return ftrDic[tp];
            }
            else
            {
                IFeatureClass outFtrCls = geoUtil.getFeatureClass(tp);
                return outFtrCls;
            }
        }

        private IRaster createArithmeticFunction(string[] paramArr)
        {
            string inRs1Str = paramArr[0];
            string inRs2Str = paramArr[1];
            object inRs1 = null;
            object inRs2 = null;
            if (rsUtil.isNumeric(inRs1Str))
            {
                inRs1 = inRs1Str;
            }
            else
            {
                inRs1 = getRaster(inRs1Str);
            }
            if (rsUtil.isNumeric(inRs2Str))
            {
                inRs2 = inRs2Str;
            }
            else
            {
                inRs2 = getRaster(inRs2Str);
            }
            esriRasterArithmeticOperation op = (esriRasterArithmeticOperation)Enum.Parse(typeof(esriRasterArithmeticOperation), paramArr[2]);
            return rsUtil.calcArithmaticFunction(inRs1, inRs2, op);
        }

        private IRaster getRaster(string p)
        {
            string tp = p.Trim();
            if (rstDic.ContainsKey(tp))
            {
                return rstDic[tp];
            }
            else
            {
                IRaster outRs = rsUtil.returnRaster(tp);
                return outRs;
            }
        }

        public void getFunctionAndParameters(string ln, out string func, out string param, out string outName)
        {
            func = "";
            param = "";
            outName = "";
            try
            {
                string[] lnArr = ln.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                outName = lnArr[0].Trim();
                string lnS = lnArr[1].Trim();
                int lP = lnS.IndexOf("(");
                int rP = lnS.IndexOf(")");
                func = lnS.Substring(0, lP).ToUpper();
                param = lnS.Substring(lP+1, lnS.Length - (lP+2));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public void syntaxExample(batchCalculations.batchGroups batchFunction)
        {
            string msg = "working on this";
            switch (batchFunction)
            {
                case batchGroups.ARITHMETIC:
                    msg = "outRS = " + batchFunction.ToString() + "(in_Raster1;in_Raster2;arithmeticFunction)";
                    break;
                case batchGroups.MATH:
                    msg = "outRS = " + batchFunction.ToString() + "(in_Raster1;mathFunction)";
                    break;
                case batchGroups.SETNULL:
                    msg = "outRS = " + batchFunction.ToString() + "(in_Raster1;0-4,8-9,13-15)";
                    break;
                case batchGroups.LOGICAL:
                    msg = "outRS = " + batchFunction.ToString() + "(in_Raster1;in_Raster2;logicalOperator)";
                    break;
                case batchGroups.CLIP:
                    msg = "outRS = " + batchFunction.ToString() + "(in_Raster;inFeatureClass;logicalOperator)";
                    break;
                case batchGroups.CONDITIONAL:
                    msg = "outRS = " + batchFunction.ToString() + "(in_Raster1;in_Raster2;in_Raster3)";
                    break;
                case batchGroups.CONVOLUTION:
                    msg = "outRS = " + batchFunction.ToString() + "(in_Raster1;width;height;0,1,0,1,0,1,0,1,0)";
                    break;
                case batchGroups.FOCAL:
                    msg = "outRS = " + batchFunction.ToString() + "(in_Raster1;width;height;focalStat)\noutRS = " + batchFunction.ToString() + "(in_Raster1;radius;focalStat)";
                    break;
                case batchGroups.LOCALSTATISTICS:
                    break;
                case batchGroups.LINEARTRANSFORM:
                    break;
                case batchGroups.RESCALE:
                    break;
                case batchGroups.REMAP:
                    break;
                case batchGroups.COMPOSITE:
                    msg = "outRs = " + batchFunction.ToString() + "(inRaster,inRaster2,inRaster3)";
                    break;
                case batchGroups.EXTRACTBAND:
                    msg = "outRs = " + batchFunction.ToString() + "(inRaster;1,2,5)";
                    break;
                case batchGroups.GLCM:
                    msg = "outRS = " + batchFunction.ToString() + "(in_Raster1;width;height;horizontal(true,false);glcmMetric)\noutRS = " + batchFunction.ToString() + "(in_Raster1;radius;horizontal(true,false);glcmMetric)";
                    break;
                case batchGroups.LANDSCAPE:
                    break;
                case batchGroups.ZONALSTATS:
                    msg = "outTbl = " + batchFunction.ToString() + "(ZoneRaster;ValueRaster;MAX,MIN,SUM)\noutTbl = " + batchFunction.ToString() + "(ZoneFeatureClass;ZoneField;ValueRaster2;MAX,MIN,SUM)";
                    break;
                case batchGroups.SAVEFUNCTIONRASTER:
                    msg = "outRs = " + batchFunction.ToString() + "(inRaster;outName;outWorkspace;rasterType)";
                    break;
                case batchGroups.BUILDRASTERSTATS:
                    msg = "outRS = " + batchFunction.ToString() + "(in_Raster;skipFactor)";
                    break;
                case batchGroups.BUILDRASTERVAT:
                    msg = "outTable = " + batchFunction.ToString() + "(in_Raster)";
                    break;
                case batchGroups.MOSAIC:
                    break;
                case batchGroups.MERGE:
                    break;
                case batchGroups.SAMPLERASTER:
                    msg = "outFtrClass = " + batchFunction.ToString() + "(inFeatureClass,sampleRaster,FieldNamePrefix)";
                    break;
                case batchGroups.CLUSTERSAMPLERASTER:
                    break;
                case batchGroups.CREATERANDOMSAMPLE:
                    break;
                case batchGroups.CREATESTRATIFIEDRANDOMSAMPLE:
                    break;
                case batchGroups.FOCALSAMPLE:
                    msg = "outRS = " + batchFunction.ToString() + "(in_Raster;360:37.56,120:26.25,240:2;focalType)";
                    break;
                case batchGroups.CREATEREGRESSIONRASTER:
                    msg = "outRS = " + batchFunction.ToString() + "(slopeRaster1,slopeRaster2,slopeRaster3;<pathToSASOutputEstimates e.g. c:\\temp\\outest.csv>)";
                    break;
                case batchGroups.ADDFEATURECLASSTOMAP:
                    msg = "outFtrClass = " + batchFunction.ToString() + "(inFeatureClass;layerName)";
                    break;
                case batchGroups.ADDRASTERTOMAP:
                    msg = "outFtrClass = " + batchFunction.ToString() + "(inRaster;layerName)";
                    break;
                case batchGroups.CONVERTPIXELTYPE:
                    msg = "outFtrClass = " + batchFunction.ToString() + "(inRaster;rstPixelType)";
                    break;
                default:
                    break;
            }
            System.Windows.Forms.MessageBox.Show(msg);
        }
    }
}