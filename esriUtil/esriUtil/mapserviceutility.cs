﻿using System;
using System.Collections.Generic;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.GISClient;
using ESRI.ArcGIS.Carto;
using System.Text;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Catalog;

namespace esriUtil
{
    /// <summary>
    /// a map services utility used to automate downloads from map services
    /// </summary>
    public class mapserviceutility
    {
        /// <summary>
        /// default constructor
        /// </summary>
        public mapserviceutility()//Constructor
        {
            gisProdServer = @"http://services.arcgisonline.com/ArcGIS/services";
            appDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            string mpLcDb = esriUtil.Properties.Settings.Default.MapServicesLocalDatabase;
            Console.WriteLine("default location = ." + mpLcDb + ".");
            if (mpLcDb == "\"\""||!System.IO.Directory.Exists(mpLcDb))
            {
                mpLcDb = getLocalDB();
                esriUtil.Properties.Settings.Default.MapServicesLocalDatabase = mpLcDb;
                esriUtil.Properties.Settings.Default.Save();
            }
            servWks = gdutil.OpenWorkSpace(LcCacheDb);
            fWks = (IFeatureWorkspace)servWks;
            IWorkspace2 wks2 = (IWorkspace2)fWks;

            if(wks2.get_NameExists(esriDatasetType.esriDTTable,"SERVICECONNECTIONS"))
            {
                tblCon = fWks.OpenTable("SERVICECONNECTIONS");
                tblSrv = fWks.OpenTable("SERVICES");
                tblLyr = fWks.OpenTable("LAYERS");
            }
            else
            {
                try
                {
                    createConnectionDb();
                    updateConnectionTable(gisProdServer);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
           
        }
        /// <summary>
        /// Changes the default database used to store mapServices
        /// </summary>
        public void changeLocalDatabase()
        {
            string mpLcDb = getLocalDB();
            esriUtil.Properties.Settings.Default.MapServicesLocalDatabase = mpLcDb;
            esriUtil.Properties.Settings.Default.Save();
            servWks = gdutil.OpenWorkSpace(LcCacheDb);
            fWks = (IFeatureWorkspace)servWks;
            IWorkspace2 wks2 = (IWorkspace2)fWks;

            if (wks2.get_NameExists(esriDatasetType.esriDTTable, "SERVICECONNECTIONS"))
            {
                tblCon = fWks.OpenTable("SERVICECONNECTIONS");
                tblSrv = fWks.OpenTable("SERVICES");
                tblLyr = fWks.OpenTable("LAYERS");
            }
            else
            {
                try
                {
                    createConnectionDb();
                    updateConnectionTable(gisProdServer);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }
        private string getLocalDB()
        {
            string outPath = null;
            string outName = "";
            string outFullPath = null;
            ESRI.ArcGIS.CatalogUI.IGxDialog gxDialog = new ESRI.ArcGIS.CatalogUI.GxDialogClass();
            gxDialog.AllowMultiSelect = false;
            ESRI.ArcGIS.Catalog.IGxObjectFilter flt = null;
            flt = new ESRI.ArcGIS.Catalog.GxFilterFileGeodatabasesClass();
            gxDialog.ObjectFilter = flt;
            gxDialog.Title = "Select a GeoDatabase";
            ESRI.ArcGIS.Catalog.IEnumGxObject eGxObj;
            if (gxDialog.DoModalOpen(0, out eGxObj))
            {
                ESRI.ArcGIS.Catalog.IGxObject gxObj = eGxObj.Next();
                outPath = gxObj.FullName;
                outName = gxObj.BaseName;
                outFullPath = outPath;
            }
            return outFullPath;
        }
        /// <summary>
        /// compacts the default database
        /// </summary>
        public void compactDatabase()
        {
            IDatabaseCompact dC = (IDatabaseCompact)servWks;
            if (dC.CanCompact())
            {
                dC.Compact();
            }
        }
        /// <summary>
        /// gets the connection id of the service
        /// </summary>
        /// <param name="connection">full url of the connection</param>
        /// <returns>the connection id</returns>
        public string getConnectionOID(string connection)
        {
            string sOid = null;
            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = "lower(CONNECTION) = '" + connection.ToLower() + "'";
            ICursor sCur = tblCon.Search(qf, false);
            IRow sRow = sCur.NextRow();
            while (sRow != null)
            {
                sOid = sRow.OID.ToString();
                sRow = sCur.NextRow();
            }
            return sOid;

        }
        /// <summary>
        /// get the service id of a given service name
        /// </summary>
        /// <param name="connection">the url of the connection</param>
        /// <param name="serviceName">the name of the service at that connection</param>
        /// <returns>the service id</returns>
        public string getServiceOID(string connection,string serviceName)
        {
            string sOid = null;
            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = "FKID = " + getConnectionOID(connection) + " and lower(SERVICE) = '" + serviceName.ToLower() + "'";
            ICursor sCur = tblSrv.Search(qf, false);
            IRow sRow = sCur.NextRow();
            while (sRow != null)
            {
                sOid = sRow.OID.ToString();
                sRow = sCur.NextRow();
            }
            return sOid;

        }
        /// <summary>
        /// method used internally to update the defalt layer table within the default geodatabase that stores all the server related layer info
        /// </summary>
        /// <param name="lyrDic">the dictionary that has all related values</param>
        /// <param name="connection">url</param>
        /// <param name="serviceName">string service name</param>
        public void updateLayerTable(Dictionary<string,int> lyrDic, string connection, string serviceName)
        {
            string sOid = getServiceOID(connection,serviceName);
            if (sOid == null)
            {
                return;
            }
            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = "FKID = " + sOid;
            removeExistingRecords(tblLyr, qf);
            IWorkspaceEdit wksE = (IWorkspaceEdit)servWks;
            bool weStart = false;
            if (!wksE.IsBeingEdited())
            {
                wksE.StartEditing(false);
                weStart = true;
            }
            wksE.StartEditOperation();
            try
            {
                int lfkIndex = tblLyr.FindField("FKID");
                int lyrIndex = tblLyr.FindField("LAYERS");
                int lyrIdIndex = tblLyr.FindField("LAYERID");
                int updIndex = tblLyr.FindField("UPDATE");
    
                foreach (KeyValuePair<string, int> kVp2 in lyrDic)
                {
                    IRow row3 = tblLyr.CreateRow();
                    string ky2 = kVp2.Key;
                    int vl2 = kVp2.Value;
                    string updateVl = "NO";
                    string ky2l = ky2.ToLower();
                    if (serviceName.EndsWith("msdi_framwork/plss") || serviceName.EndsWith("msdi_framwork/parcels") || serviceName.StartsWith("naip"))
                    {
                        updateVl = "YES";
                    }
                    row3.set_Value(lfkIndex, sOid);
                    row3.set_Value(lyrIndex, ky2);
                    row3.set_Value(lyrIdIndex, vl2);
                    row3.set_Value(updIndex, updateVl);
                    row3.Store();

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                wksE.StopEditOperation();
                if (weStart)
                {
                    wksE.StopEditing(true);
                }
            }
            
        }
        /// <summary>
        /// updates a given row within the layer table
        /// </summary>
        /// <param name="qf">query filter</param>
        /// <param name="updateValue">value to update with</param>
        public void updateLayerRow(IQueryFilter qf,string updateValue)
        {
            IWorkspaceEdit wksE = (IWorkspaceEdit)servWks;
            bool weEdit = false;
            if (!wksE.IsBeingEdited())
            {
                wksE.StartEditing(false);
                weEdit = true;

            }
            wksE.StartEditOperation();
            try
            {
                ICursor uCur = tblLyr.Update(qf, false);
                int uIndex = uCur.FindField("UPDATE");
                IRow uRow = uCur.NextRow();
                while (uRow != null)
                {
                    uRow.set_Value(uIndex, updateValue);
                    uCur.UpdateRow(uRow);
                    uRow = uCur.NextRow();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                wksE.StopEditOperation();
                if (weEdit)
                {
                    wksE.StopEditing(true);
                }
            }
        }
        /// <summary>
        /// method used internally to update the defalt service table within the default geodatabase that stores all the server related server info
        /// </summary>
        /// <param name="lyrDic">the dictionary that has all related values</param>
        /// <param name="connection">url</param>
        public void updateServiceTable(string svConnection)
        {
            Dictionary<string, IAGSServerObjectName3> msSvrDic = getServices(svConnection);
            string conOidValue = getConnectionOID(svConnection);
            if (conOidValue == null)
            {
                return;
            }
            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = "FKID = " + conOidValue;
            removeExistingRecords(tblSrv, qf);
            int srvIndex = tblSrv.FindField("SERVICE");
            int urlIndex = tblSrv.FindField("URL");
            int sfkIndex = tblSrv.FindField("FKID");
            int sTypeIndex = tblSrv.FindField("STYPE");

            foreach (KeyValuePair<string, IAGSServerObjectName3> kVp in msSvrDic)
            {
                
                IWorkspaceEdit wksE = (IWorkspaceEdit)servWks;
                bool weStart = false;
                if (!wksE.IsBeingEdited())
                {
                    wksE.StartEditing(false);
                    weStart = true;
                }
                wksE.StartEditOperation();
                try
                {
                    IRow row2 = tblSrv.CreateRow();
                    int srvOidValue = row2.OID;
                    string ky = kVp.Key;
                    string kyl = ky.ToLower();
                    string vl = kVp.Value.URL;
                    string lTyp = kVp.Value.Type;
                    row2.set_Value(srvIndex, ky);
                    row2.set_Value(urlIndex, vl);
                    row2.set_Value(sTypeIndex, lTyp);
                    row2.set_Value(sfkIndex, System.Convert.ToInt32(conOidValue));
                    row2.Store();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                finally
                {
                    wksE.StopEditOperation();
                    if (weStart)
                    {
                        wksE.StopEditing(true);
                    }
                }
            }
        }
        /// <summary>
        /// method used internally to update the defalt connection table within the default geodatabase that stores all the connection related layer info
        /// </summary>
        /// <param name="lyrDic">the dictionary that has all related values</param>
        /// <param name="connection">url</param>
        public void updateConnectionTable(string svConnection)
        {
            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = "CONNECTION = '" + svConnection + "'";
            removeExistingRecords(tblCon,qf);
            int conIndex = tblCon.FindField("CONNECTION");
            IWorkspaceEdit wksE = (IWorkspaceEdit)servWks;
            bool weStart = false;
            if (!wksE.IsBeingEdited())
            {
                wksE.StartEditing(false);
                weStart = true;
            }
            wksE.StartEditOperation();
            try
            {
                IRow row = tblCon.CreateRow();
                int conOidValue = row.OID;
                row.set_Value(conIndex, svConnection);
                row.Store();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                wksE.StopEditOperation();
                if (weStart)
                {
                    wksE.StopEditing(true);
                }
                
            }
        }
        /// <summary>
        /// removes existing records from the corresponding tables
        /// </summary>
        /// <param name="tbl">ITable to remove records from </param>
        /// <param name="qf">query used to specify the records</param>
        public void removeExistingRecords(ITable tbl,IQueryFilter qf)
        {
            IWorkspaceEdit wksE = (IWorkspaceEdit)servWks;
            bool weEdit = false;
            if (!wksE.IsBeingEdited())
            {
                wksE.StartEditing(false);
                weEdit = true;
            }
            try
            {
                IQueryFilter qf2 = new QueryFilterClass();
                IDataset dSet = (IDataset)tbl;
                string tblName = dSet.Name;
                ICursor dCur = tbl.Search(qf, false);
                IRow dRow = dCur.NextRow();
                while (dRow != null)
                {
                    int oid = dRow.OID;
                    qf2.WhereClause = "FKID = " + oid;
                    switch (tblName.ToLower())
                    {
                        case "serviceconnections":
                            
                            removeExistingRecords(tblSrv, qf2);
                            break;
                        case "services":
                            removeExistingRecords(tblLyr, qf2);
                            break;
                        case "layers":
                        case "default":
                            break;

                    }
                    //send deletes to the other 2 tables
                    dRow.Delete();
                    dRow = dCur.NextRow();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                wksE.StopEditOperation();
                if (weEdit)
                {
                    wksE.StopEditing(true);
                }
            }
            

        }
        /// <summary>
        /// gets the service type from the service tables given a url and service name
        /// </summary>
        /// <param name="seviceConnection">url to service</param>
        /// <param name="service">service name</param>
        /// <returns>the type of service map, image, etc</returns>
        public string getServiceType(string seviceConnection, string service)
        {
            string soid = getServiceOID(seviceConnection, service);
            if (soid == null)
            {
                return null;
            }
            IRow row = tblSrv.GetRow(System.Convert.ToInt32(soid));
            return row.get_Value(row.Fields.FindField("STYPE")).ToString();
        }
        /// <summary>
        /// returns the images from an IImageServer server
        /// </summary>
        /// <param name="ims">IImageServer object</param>
        /// <returns>the a dictionary of service name and its corresponding id</returns>
        public Dictionary<string, int> getImages(IImageServer ims)
        {
            Dictionary<string, int> imsDic = new Dictionary<string, int>();
            IImageServiceInfo  imsInfo = ims.ServiceInfo;
            string iNm = imsInfo.Name;
            int bCnt = imsInfo.BandCount;
            imsDic.Add(iNm, bCnt);
            return imsDic;
        }
        /// <summary>
        /// return the layers from a mapservice
        /// </summary>
        /// <param name="ms2">IMapServer2 object</param>
        /// <returns>a dictionary of service name and its corresponding id</returns>
        public Dictionary<string, int> getLayers(IMapServer2 ms2)
        {
            Dictionary<string, int> lyrDic = new Dictionary<string, int>();
            try
            {
                for (int m = 0; m < ms2.MapCount; m++)
                {
                    string mName = ms2.get_MapName(m);
                    IMapServerInfo msInfo = ms2.GetServerInfo(mName);
                    IMapLayerInfos mLyrInfos = msInfo.MapLayerInfos;
                    for (int j = 0; j < mLyrInfos.Count; j++)
                    {
                        IMapLayerInfo mLyrInfo = mLyrInfos.get_Element(j);
                        if (mLyrInfo.IsFeatureLayer)
                        {
                            string lNm = mLyrInfo.Name;
                            int lyrId = mLyrInfo.ID;
                            if (!lyrDic.ContainsKey(lNm))
                            {
                                lyrDic.Add(lNm, lyrId);
                            }
                        }

                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return lyrDic;
        }
        /// <summary>
        /// return a dicitionary fo service name by IAGSServerObjectsName3 given a url
        /// </summary>
        /// <param name="service">url to a ArcGIS server</param>
        /// <returns>return a dicitionary fo service name by IAGSServerObjectsName3 given a url</returns>
        public Dictionary<string, IAGSServerObjectName3> getServices(string service)
        {
            Dictionary<string, IAGSServerObjectName3> serviceDic = new Dictionary<string, IAGSServerObjectName3>();
            IAGSServerConnection2 sConn = getServiceConnection(service);
            IAGSEnumServerObjectName eSobjN = sConn.ServerObjectNames;
            IAGSServerObjectName3 sobjN = (IAGSServerObjectName3)eSobjN.Next();
            while (sobjN != null)
            {
                string nm = sobjN.Name;
                string tp = sobjN.Type;
                if (tp.ToLower()=="mapserver"||tp.ToLower()=="imageserver")
                {
                    if (!serviceDic.ContainsKey(nm))
                    {
                        serviceDic.Add(nm, (IAGSServerObjectName3)sobjN);
                    }
                }
                sobjN = (IAGSServerObjectName3)eSobjN.Next();
            }
            return serviceDic;
        }
        /// <summary>
        /// creates the Connection database used to auto download info from services
        /// </summary>
        public void createConnectionDb()
        {
            //servWks = gdutil.CreateWorkSpace(LcServiceCacheDir, System.IO.Path.GetFileName(LcCacheDb));// gdutil.CreateFileGdbOldWorkSpace(LcCacheDb, "9.3");
            try
            {
                UID uid = new UIDClass();
                uid.Value = "esriGeoDatabase.Object";
                IObjectClassDescription objectClassDescription = new ObjectClassDescriptionClass();
                
                fWks = (IFeatureWorkspace)servWks;
                //Create ServiceConnections table
                IFields tblFlds = objectClassDescription.RequiredFields;
                IFieldsEdit tblFldsE = (IFieldsEdit)tblFlds;
                IField fld = new FieldClass();
                IFieldEdit fldE = (IFieldEdit)fld;
                fldE.Name_2 = "CONNECTION";
                fldE.Type_2 = esriFieldType.esriFieldTypeString;
                fldE.Length_2 = 1000;
                fldE.IsNullable_2 = true;
                fldE.Editable_2 = true;
                fldE.AliasName_2 = "CONNECTION";
                tblFldsE.AddField(fldE);
                tblCon = fWks.CreateTable("SERVICECONNECTIONS", tblFldsE, uid, null, "");
                //Create Services Table related to ServiceConnctions
                tblFlds = objectClassDescription.RequiredFields;
                tblFldsE = (IFieldsEdit)tblFlds;
                fld = new FieldClass();
                fldE = (IFieldEdit)fld;
                fldE.Name_2 = "FKID";
                fldE.Type_2 = esriFieldType.esriFieldTypeInteger;
                fldE.IsNullable_2 = true;
                fldE.Editable_2 = true;
                fldE.AliasName_2 = "FKID";
                tblFldsE.AddField(fldE);
                fld = new FieldClass();
                fldE = (IFieldEdit)fld;
                fldE.Name_2 = "SERVICE";
                fldE.Type_2 = esriFieldType.esriFieldTypeString;
                fldE.Length_2 = 50;
                fldE.IsNullable_2 = true;
                fldE.Editable_2 = true;
                fldE.AliasName_2 = "SERVICE";
                tblFldsE.AddField(fldE);
                fld = new FieldClass();
                fldE = (IFieldEdit)fld;
                fldE.Name_2 = "URL";
                fldE.Type_2 = esriFieldType.esriFieldTypeString;
                fldE.Length_2 = 1000;
                fldE.IsNullable_2 = true;
                fldE.Editable_2 = true;
                fldE.AliasName_2 = "URL";
                tblFldsE.AddField(fldE);
                fld = new FieldClass();
                fldE = (IFieldEdit)fld;
                fldE.Name_2 = "STYPE";
                fldE.Type_2 = esriFieldType.esriFieldTypeString;
                fldE.Length_2 = 20;
                fldE.IsNullable_2 = true;
                fldE.Editable_2 = true;
                fldE.AliasName_2 = "STYPE";
                tblFldsE.AddField(fldE);
                tblSrv = fWks.CreateTable("SERVICES", tblFldsE, uid, null, "");
                //Create Layer Table related to Services
                tblFlds = objectClassDescription.RequiredFields;
                tblFldsE = (IFieldsEdit)tblFlds;
                fld = new FieldClass();
                fldE = (IFieldEdit)fld;
                fldE.Name_2 = "FKID";
                fldE.Type_2 = esriFieldType.esriFieldTypeInteger;
                fldE.IsNullable_2 = true;
                fldE.Editable_2 = true;
                fldE.AliasName_2 = "FKID";
                tblFldsE.AddField(fldE);
                fld = new FieldClass();
                fldE = (IFieldEdit)fld;
                fldE.Name_2 = "LAYERS";
                fldE.Type_2 = esriFieldType.esriFieldTypeString;
                fldE.Length_2 = 50;
                fldE.IsNullable_2 = true;
                fldE.Editable_2 = true;
                fldE.AliasName_2 = "LAYERS";
                tblFldsE.AddField(fldE);
                fld = new FieldClass();
                fldE = (IFieldEdit)fld;
                fldE.Name_2 = "LAYERID";
                fldE.Type_2 = esriFieldType.esriFieldTypeInteger;
                fldE.IsNullable_2 = true;
                fldE.Editable_2 = true;
                fldE.AliasName_2 = "LAYERID";
                tblFldsE.AddField(fldE);
                fld = new FieldClass();
                fldE = (IFieldEdit)fld;
                fldE.Name_2 = "UPDATE";
                fldE.Type_2 = esriFieldType.esriFieldTypeString;
                fldE.Length_2 = 3;
                fldE.IsNullable_2 = true;
                fldE.DefaultValue_2 = "NO";
                fldE.Editable_2 = true;
                fldE.AliasName_2 = "UPDATE";
                tblFldsE.AddField(fldE);
                tblLyr = fWks.CreateTable("LAYERS", tblFldsE, uid, null, "");
                int conIndex = tblCon.FindField("CONNECTION");
                IWorkspaceEdit wksE = (IWorkspaceEdit)servWks;
                bool weStart = false;
                if (!wksE.IsBeingEdited())
                {
                    wksE.StartEditing(false);
                    weStart = true;
                }
                wksE.StartEditOperation();
                IRow row = tblCon.CreateRow();
                row.set_Value(conIndex, gisProdServer);
                row.Store();
                wksE.StopEditOperation();
                if (weStart)
                {
                    wksE.StopEditing(true);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
            }
        }
        /// <summary>
        /// returns a list of connection strings from for records that exist within the connection database
        /// </summary>
        public List<string> getConnectionStrings()
        {
            List<string> conStrLst = new List<string>();
            ICursor sCur = tblCon.Search(null, false);
            IRow sRow = sCur.NextRow();
            int conIndex = tblCon.FindField("CONNECTION");
            while (sRow != null)
            {
                string vl = sRow.get_Value(conIndex).ToString();
                if (!conStrLst.Contains(vl))
                {
                    conStrLst.Add(vl);
                }
                sRow = sCur.NextRow();
            }
            return conStrLst;
        }
        /// <summary>
        /// returns a list of services for a given connection string within the connection database
        /// </summary>
        public List<string> getServiceStrings(string connection)
        {
            List<string> conStrLst = new List<string>();
            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = "FKID = " + getConnectionOID(connection);
            ICursor sCur = tblSrv.Search(qf, false);
            IRow sRow = sCur.NextRow();
            int conIndex = tblSrv.FindField("SERVICE");
            while (sRow != null)
            {
                string vl = sRow.get_Value(conIndex).ToString();
                if (!conStrLst.Contains(vl))
                {
                    conStrLst.Add(vl);
                }
                sRow = sCur.NextRow();
            }
            return conStrLst;
        }
        /// <summary>
        /// returns a dicitonary of layers that identify if that layer needs to be updated within the database (true false)
        /// </summary>
        public Dictionary<string,bool> getLayerDic(string connection, string service)
        {
            Dictionary<string, bool> lyrDic = new Dictionary<string,bool>();
            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = "FKID = " + getServiceOID(connection, service);
            ICursor sCur = tblLyr.Search(qf, false);
            IRow sRow = sCur.NextRow();
            int lyrIndex = tblLyr.FindField("LAYERS");
            int upIndex = tblLyr.FindField("UPDATE");
            while (sRow != null)
            {
                string lyrVl = sRow.get_Value(lyrIndex).ToString();
                string upVl = sRow.get_Value(upIndex).ToString().ToLower();
                bool up = false;
                if (upVl == "yes")
                {
                    up = true;
                }
                if (!lyrDic.ContainsKey(lyrVl))
                {
                    lyrDic.Add(lyrVl,up);
                }
                sRow = sCur.NextRow();
            }
            return lyrDic;
        }   
        private ITable tblCon = null;
        private ITable tblSrv = null;
        private ITable tblLyr = null;
        /// <summary>
        /// returns the ITable of the connection table
        /// </summary>
        public ITable ConnectionsTable { get { return tblCon; } }
        /// <summary>
        /// returns the ITable of the services table
        /// </summary>
        public ITable ServicesTable { get { return tblSrv; } }
        /// <summary>
        /// returns the ITable of the Layers table
        /// </summary>
        public ITable LayersTable { get { return tblLyr; } }
        private geoDatabaseUtility gdutil = new geoDatabaseUtility();
        private string gisProdServer = "";
        private IWorkspace servWks = null;
        private IFeatureWorkspace fWks = null;
        private string tempDir = Environment.GetEnvironmentVariable("TEMP");
        private string appDir = "";
        /// <summary>
        /// returns the base directory of the default geodatabase
        /// </summary>
        public string LcServiceCacheDir
        {
            get
            {
                return System.IO.Path.GetDirectoryName(LcCacheDb);
            }
        }
        /// <summary>
        /// return the path to the default database
        /// </summary>
        public string LcCacheDb { get { return esriUtil.Properties.Settings.Default.MapServicesLocalDatabase; } }
        private IAGSServerConnectionFactory2 sConnFact = new AGSServerConnectionFactoryClass();
        /// <summary>
        /// returns the service connection given a string path 
        /// </summary>
        /// <param name="serviceConnection">url</param>
        public IAGSServerConnection2 getServiceConnection(string serviceConnection)
        {
            IAGSServerConnection2 sConn2 = null;
            try
            {
                IPropertySet pSet = new PropertySetClass();
                string mc = "URL";
                if (!serviceConnection.ToLower().StartsWith("http"))
                {
                    mc = "MACHINE";
                }
                pSet.SetProperty(mc, serviceConnection);
                sConn2 = (IAGSServerConnection2)sConnFact.Open(pSet, 0);
                Console.WriteLine(serviceConnection);
                Console.WriteLine(mc);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return sConn2;
        }
        /// <summary>
        /// returns a service object given a server, service, and type
        /// </summary>
        /// <param name="serviceConnection">url</param>
        /// <param name="service">service name</param>
        /// <param name="serviceType">service type</param>
        /// <returns></returns>
        public IAGSServerObjectName3 getServiceObject(IAGSServerConnection2 serviceConnection, string service, string serviceType)
        {
            IAGSServerObjectName3 sobjN3 = null;
            IAGSEnumServerObjectName eSobjN = serviceConnection.ServerObjectNames;
            eSobjN.Reset();
            IAGSServerObjectName sobjN = eSobjN.Next();
            while (sobjN != null)
            {
                string nm = sobjN.Name.ToLower();
                string tp = sobjN.Type.ToLower();
                if (nm == service.ToLower()&&tp==serviceType.ToLower())
                {
                    sobjN3 = (IAGSServerObjectName3)sobjN;
                    break;
                }
                sobjN = eSobjN.Next();
                
            }
            return sobjN3;
        }
        /// <summary>
        /// Return the map service given a sereverobject name
        /// </summary>
        /// <param name="sobj">server object</param>
        public IMapServer2 getMapService(IAGSServerObjectName3 sobj)
        {
            string typ = sobj.Type.ToLower();
            if (typ == "mapserver")
            {
                return (IMapServer2)((IName)sobj).Open();
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// return an image server object given a serverobject name
        /// </summary>
        /// <param name="sobj">server object name</param>
        /// <returns></returns>
        public IImageServer getIMageService(IAGSServerObjectName3 sobj)
        {
            string typ = sobj.Type.ToLower();
            if (typ == "imageserver")
            {
                return (IImageServer)((IName)sobj).Open();
            }
            else
            {
                return null;
            }
        }
        private bool checkServerLayer(IMapServer2 ms2, int lyrID, string lyrName)
        {
            IMapServerInfo2 msInfo2 = (IMapServerInfo2)ms2.GetServerInfo(ms2.DefaultMapName);
            IMapLayerInfos msLyrInfos = msInfo2.MapLayerInfos;
            IMapLayerInfo msLyrInfo = msLyrInfos.get_Element(lyrID);
            string nm = msLyrInfo.Name;
            if (lyrName.ToLower() != nm.ToLower())
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        /// <summary>
        /// updates featureclasses that need updatign given a spatial filter
        /// </summary>
        /// <param name="sFlt">spatial filter</param>
        public void getDbFtrClassesThatNeedUpdating(ISpatialFilter sFlt)
        {
            Dictionary<string, int> lyrId = new Dictionary<string, int>();
            Dictionary<int, Dictionary<string, int>> svDic = new Dictionary<int,Dictionary<string,int>>();   
            IQueryFilter qf = new QueryFilterClass();
            try
            {
                for (int i = 0; i < tblLyr.Fields.FieldCount; i++)
                {
                    IField fld = tblLyr.Fields.get_Field(i);
                    Console.WriteLine(fld.Name);
                }
                qf.WhereClause = "\"UPDATE\" = 'YES'";
                ICursor SCur = tblLyr.Search(qf, false);
                int lyIdIndex, lyIndex, lyFkIndex;
                lyIdIndex = tblLyr.FindField("LAYERID");
                lyIndex = tblLyr.FindField("LAYERS");
                lyFkIndex = tblLyr.FindField("FKID");
                IRow sRow = SCur.NextRow();
                while (sRow != null)
                {
                    int fkVl = System.Convert.ToInt32(sRow.get_Value(lyFkIndex));
                    string lyVl = sRow.get_Value(lyIndex).ToString();
                    int lyIdVl = System.Convert.ToInt32(sRow.get_Value(lyIdIndex));
                    if (svDic.TryGetValue(fkVl, out lyrId))
                    {
                        lyrId.Add(lyVl, lyIdVl);
                    }
                    else
                    {
                        lyrId = new Dictionary<string, int>();
                        lyrId.Add(lyVl, lyIdVl);
                        svDic.Add(fkVl, lyrId);
                    }
                    sRow = SCur.NextRow();
                }
                foreach (KeyValuePair<int, Dictionary<string, int>> kvp in svDic)
                {
                    int svKey = kvp.Key;
                    lyrId = kvp.Value;
                    IRow row = tblSrv.GetRow(svKey);
                    string svName = row.get_Value(row.Fields.FindField("SERVICE")).ToString();
                    int conFk = System.Convert.ToInt32(row.get_Value(row.Fields.FindField("FKID")));
                    row = tblCon.GetRow(conFk);
                    string cnName = row.get_Value(row.Fields.FindField("CONNECTION")).ToString();
                    fillDbFtrClasses(sFlt, cnName, svName, lyrId);
                }

                compactDatabase();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            

            return ;
        }
        /// <summary>
        /// Fills in the features that need updating
        /// </summary>
        /// <param name="sFlt">filter</param>
        /// <param name="serviceConnection">connection</param>
        /// <param name="service">service</param>
        /// <param name="lyrID">layerid</param>
        /// <returns>any messages from the updating processes</returns>
        public string fillDbFtrClasses(ISpatialFilter sFlt, string serviceConnection, string service, Dictionary<string,int> lyrID)
        {
            esriUtil.Forms.RunningProcess.frmRunningProcessDialog rpd = new esriUtil.Forms.RunningProcess.frmRunningProcessDialog(true);
            rpd.Show();
            rpd.addMessage("Downloading data for " + service + " Map Service");
            bool lyrTblNeedsUpdating = false;
            StringBuilder msg = new StringBuilder();
            IWorkspace2 wks2 = (IWorkspace2)servWks;
            
            string svID = getServiceOID(serviceConnection,service);
            IAGSServerConnection2 conn = getServiceConnection(serviceConnection);
            IAGSServerObjectName3 sobj3 = getServiceObject(conn,service,"MapServer");
            IMapServer2 ms2 = getMapService(sobj3);
            if (ms2 == null)
            {
                msg.AppendLine("Could not get map service "+service);
                rpd.addMessage("Could not get map service "+service);
                updateServiceTable(serviceConnection);
                return msg.ToString();
            }
            IQueryFilter qryFl = new QueryFilterClass();
            string mName = ms2.DefaultMapName;
            IWorkspaceEdit wksE = (IWorkspaceEdit)servWks;
            bool weEdit = false;
            if (!wksE.IsBeingEdited())
            {
                wksE.StartEditing(false);
                weEdit = true;
            }
            wksE.StartEditOperation();
            try
            {
                int stp = 100 / lyrID.Count;
                foreach (KeyValuePair<string, int> kvp in lyrID)
                {
                    string lyrName = kvp.Key;
                    int lyrId = kvp.Value;
                    if (!checkServerLayer(ms2, lyrId, lyrName))
                    {
                        lyrTblNeedsUpdating = true;
                        msg.AppendLine("Could not find " + lyrName + ". Moving to the next Layer and updating tables.");
                        continue;

                    }
                    else
                    {
                        string x = "Updating records for " + lyrName;
                        rpd.addMessage(x);
                        rpd.stepPGBar(stp);
                        msg.AppendLine(x);
                    }
                    string ftrClassName = lyrName + "_" + svID + "_" + lyrId.ToString();
                    string fFtrClassName = geoDatabaseUtility.parsName(ftrClassName);
                    
                    IFeatureClass ftrCls;
                    int drcd = 1;
                    bool ftrExists = wks2.get_NameExists(esriDatasetType.esriDTFeatureClass, fFtrClassName);
                    if (!ftrExists)
                    {
                        ftrCls = createFtrClass(ms2, lyrId, fFtrClassName);
                        ISchemaLock sLock = (ISchemaLock)ftrCls;
                        try
                        {
                            sLock.ChangeSchemaLock(esriSchemaLock.esriExclusiveSchemaLock);
                            IClassSchemaEdit3 cSchEdit3 = (IClassSchemaEdit3)ftrCls;
                            cSchEdit3.AlterAliasName(lyrName);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                        }
                        finally
                        {
                            sLock.ChangeSchemaLock(esriSchemaLock.esriSharedSchemaLock);
                        }
                        
                    }
                    else
                    {
                        ftrCls = fWks.OpenFeatureClass(fFtrClassName);
                    }
                    string oidName = ftrCls.OIDFieldName;
                    int maxRecords = getMaxRecords(ms2);
                    List<string> fIdLst = new List<string>();
                    IFIDSet2 idSet = (IFIDSet2)ms2.QueryFeatureIDs(mName, lyrId, sFlt);
                    idSet.Reset();
                    IFeatureCursor fCur = ftrCls.Search(sFlt, false);
                    IFeature ftr = fCur.NextFeature();
                    while (ftr != null)
                    {
                        ftr.Delete();
                        ftr = fCur.NextFeature();
                    }

                    int fId;
                    int rc = 1;
                    idSet.Next(out fId);
                    while (fId > -1)
                    {
                        if (drcd > maxRecords)
                        {
                            rpd.addMessage("\tDownloading Index " + rc.ToString());
                            rc+=1;
                            qryFl.WhereClause = String.Join(" or ", fIdLst.ToArray());
                            IRecordSet rSet = ms2.QueryFeatureData(mName, lyrId, qryFl);
                            IFields rFlds = rSet.Fields;
                            ICursor sCur = rSet.get_Cursor(false);
                            IRow sRow = sCur.NextRow();
                            while (sRow != null)
                            {
                                fCur = ftrCls.Insert(true);
                                IFeatureBuffer ftrBuff = ftrCls.CreateFeatureBuffer();
                                for (int f = 0; f < rFlds.FieldCount; f++)
                                {
                                    IField rFld = rFlds.get_Field(f);
                                    int fIndex = ftrBuff.Fields.FindField(rFld.Name);
                                    if (fIndex > -1)
                                    {
                                        try
                                        {
                                            ftrBuff.set_Value(fIndex, sRow.get_Value(f));
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine(e.ToString());
                                        }
                                    }
                                }

                                fCur.InsertFeature(ftrBuff);
                                fCur.Flush();
                                sRow = sCur.NextRow();
                            }

                            drcd = 1;
                            fIdLst.Clear();
                        }
                        else
                        {
                            fIdLst.Add(oidName + " = '" + fId.ToString() + "'");
                            drcd += 1;
                        }
                        idSet.Next(out fId);
                    }
                    if (fIdLst.Count > 0)
                    {
                        rpd.addMessage("\tDownloading Index " + rc.ToString());
                        qryFl.WhereClause = String.Join(" or ", fIdLst.ToArray());
                        IRecordSet rSet = ms2.QueryFeatureData(mName, lyrId, qryFl);
                        IFields rFlds = rSet.Fields;
                        ICursor sCur = rSet.get_Cursor(false);
                        IRow sRow = sCur.NextRow();
                        while (sRow != null)
                        {
                            fCur = ftrCls.Insert(true);
                            IFeatureBuffer ftrBuff = ftrCls.CreateFeatureBuffer();
                            for (int f = 0; f < rFlds.FieldCount; f++)
                            {
                                IField rFld = rFlds.get_Field(f);
                                if (rFld.Editable)
                                {
                                    int fIndex = ftrBuff.Fields.FindField(rFld.Name);
                                    if (fIndex > -1)
                                    {
                                        try
                                        {
                                            ftrBuff.set_Value(fIndex, sRow.get_Value(f));
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine(e.ToString());
                                        }
                                    }
                                }
                            }

                            fCur.InsertFeature(ftrBuff);
                            fCur.Flush();
                            sRow = sCur.NextRow();
                        }
                    }
                    drcd = 1;
                    fIdLst.Clear();
                }
                if (lyrTblNeedsUpdating)
                {
                    Dictionary<string,int> dic = getLayers(ms2);
                    updateLayerTable(dic,serviceConnection, service);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                
                wksE.StopEditOperation();
                if (weEdit)
                {
                    wksE.StopEditing(true);
                }
                rpd.stepPGBar(100);
                rpd.enableClose();
                rpd.TopMost = false;
            }
            return msg.ToString();

        }
        /// <summary>
        /// fills in the rasters within a database given the layer and and extent
        /// </summary>
        /// <param name="imSvLyr">raster layer</param>
        /// <param name="ext">spatial extent</param>
        /// <returns>method messages</returns>
        public string fillDbRaster(IImageServerLayer imSvLyr, ESRI.ArcGIS.Geometry.IEnvelope ext)
        {
            return fillDbRaster(imSvLyr, null, ext,imSvLyr.ServiceInfo.SpatialReference);
        }
        /// <summary>
        /// fills in the rasters within a database given the layer, extent, and spatial reference
        /// </summary>
        /// <param name="imSvLyr">raster layer</param>
        /// <param name="ext">spatial extent</param>
        /// <param name="sr">spatial reference</param>
        /// <returns>method messages</returns>
        public string fillDbRaster(IImageServerLayer imSvLyr, IWorkspace wks,ESRI.ArcGIS.Geometry.IEnvelope ext,ISpatialReference sr)
        {
            StringBuilder msg = new StringBuilder();
            if (wks == null)
            {
                wks = servWks;
            }
            try
            {
                int minX = System.Convert.ToInt32(ext.XMin);
                int maxX = System.Convert.ToInt32(ext.XMax);
                int minY = System.Convert.ToInt32(ext.YMin);
                int maxY = System.Convert.ToInt32(ext.YMax);
                int tile = 1;
                IRaster rast = imSvLyr.Raster;
                ISaveAs saveas = (ISaveAs)rast;
                IRasterProps rasterProps = (IRasterProps)rast;
                imSvLyr.SpatialReference = sr;
                string nm = imSvLyr.ServiceInfo.Name;
                string rNm = nm;
                if (rNm.Length > 12)
                {
                    rNm = nm.Substring(nm.Length - 7, 7);
                }
                int mCols = System.Convert.ToInt32(imSvLyr.ServiceInfo.MaxNCols * .90);
                int mRows = System.Convert.ToInt32(imSvLyr.ServiceInfo.MaxNRows * .90);
                bool testLng = true;                    
                for (int i = minX; ((i < maxX) && (testLng)); i += mRows)
                {
                    for (int j = minY; ((j < maxY) && (testLng)); j += mCols)
                    {

                        IEnvelope clipEnvelope = new EnvelopeClass();
                        clipEnvelope.PutCoords(i, j, i + mRows, j + mCols);
                        rasterProps.Extent = clipEnvelope;
                        rasterProps.Width = mRows;
                        rasterProps.Height = mCols;
                        rasterProps.SpatialReference = sr;
                        string r = rNm + "_T" + tile.ToString();
                        if (r.Length > 12)
                        {
                            msg.AppendLine("Too many tiles. Ending at Tile: " + tile);
                            testLng = false;
                            break;
                        }
                        if(((IWorkspace2)wks).get_NameExists(esriDatasetType.esriDTRasterDataset,r))
                        {
                            Console.WriteLine("Deleting Raster " + r);
                            ((IRasterWorkspaceEx)wks).DeleteRasterDataset(r); 
                        }
                        Console.WriteLine("Creating tile " + tile.ToString());
                        Console.WriteLine("TestLength  = " + testLng.ToString());
                        saveas.SaveAs(r, wks, "gdb");
                        msg.AppendLine("Added Tile " + r);
                        tile++;
                    }
                }
            }
            catch (Exception e)
            {
                string x = e.ToString();
                msg.AppendLine(x);
                Console.WriteLine("Error: " + x);
            }
            return msg.ToString();
        }
        /// <summary>
        /// returns the maximum number of records for a given mapServer
        /// </summary>
        /// <param name="ms2">Map service</param>
        public int getMaxRecords(IMapServer2 ms2)
        {
            IPropertySet pSet = ms2.ServiceConfigurationInfo;
            return System.Convert.ToInt32(pSet.GetProperty("MaximumRecordCount"));
        }
        /// <summary>
        /// return the number of bands for a given image server
        /// </summary>
        /// <param name="is2">image server</param>
        /// <returns></returns>
        public int getBandCount(IImageServer is2)
        {
            IImageServiceInfo pSet = is2.ServiceInfo;
            return pSet.BandCount;
        }      
        /// <summary>
        /// creates a feature class in the default database given a map server lyr id and ftrName
        /// </summary>
        /// <param name="ms2">mapservice</param>
        /// <param name="lyrId">layer id of the feature on the map service</param>
        /// <param name="ftrName">the name of the new map service</param>
        /// <returns>the feature class created</returns>
        private IFeatureClass createFtrClass(IMapServer2 ms2,int lyrId,string ftrName)
        {
            Console.WriteLine(ftrName);
            string mapN = ms2.DefaultMapName;
            IMapServerInfo2  msInfo2 = (IMapServerInfo2)ms2.GetServerInfo(mapN);
            IMapLayerInfos mLyrInfos = msInfo2.MapLayerInfos;
            IMapLayerInfo2 mLyrInfo2 = (IMapLayerInfo2)mLyrInfos.get_Element(lyrId);
            IFields flds = mLyrInfo2.Fields;
            IFields fldsClone = (IFields)((IClone)flds).Clone();
            UID CLSID = new UIDClass();
            CLSID.Value = "esriGeoDatabase.Feature";
            string sFld = "SHAPE";
            string oFld = "OBJECTID";
            bool fOid = false;
            bool fshape = false;
            for(int i=0;i<flds.FieldCount;i++)
            {
                IField fld = flds.get_Field(i);
                esriFieldType fType = fld.Type;
                if(fType== esriFieldType.esriFieldTypeGeometry)
                {
                    sFld = fld.Name;
                    fshape = true;
                }
                else if (fType==esriFieldType.esriFieldTypeOID)
                {
                    oFld = fld.Name;
                    fOid = true;
                }
                else
                {
                }
                if(fshape&&fOid)
                {
                    break;
                }
            }
            if (fshape && fOid)
            {
                return fWks.CreateFeatureClass(ftrName, fldsClone, CLSID, null, esriFeatureType.esriFTSimple, sFld, "");
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// deletes all records within the default database
        /// </summary>
        public void clearCache()
        {
            IEnumDataset eDset =  servWks.get_Datasets(esriDatasetType.esriDTFeatureClass);
            IDataset dSet = eDset.Next();
            while (dSet != null)
            {
                if (dSet.CanDelete())
                {
                    dSet.Delete();
                }
                dSet = eDset.Next();
            }
            eDset = servWks.get_Datasets(esriDatasetType.esriDTRasterDataset);
            dSet = eDset.Next();
            while (dSet != null)
            {
                if (dSet.CanDelete())
                {
                    dSet.Delete();
                }
                dSet = eDset.Next();
            }

        }
    }
}
