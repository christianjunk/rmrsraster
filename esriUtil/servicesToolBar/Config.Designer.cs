//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.269
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace servicesToolBar {
    using ESRI.ArcGIS.Framework;
    using ESRI.ArcGIS.ArcMapUI;
    using System;
    using System.Collections.Generic;
    using ESRI.ArcGIS.Desktop.AddIns;
    
    
    /// <summary>
    /// A class for looking up declarative information in the associated configuration xml file (.esriaddinx).
    /// </summary>
    internal class ThisAddIn {
        
        internal static string Name {
            get {
                return "servicesToolBar";
            }
        }
        
        internal static string AddInID {
            get {
                return "{00ebf4c7-7068-4ef2-a175-a79568a85cb9}";
            }
        }
        
        internal static string Company {
            get {
                return "USDA Forest Service";
            }
        }
        
        internal static string Version {
            get {
                return "1";
            }
        }
        
        internal static string Description {
            get {
                return "Toolbar for Raster Processing";
            }
        }
        
        internal static string Author {
            get {
                return "John Hogland";
            }
        }
        
        internal static string Date {
            get {
                return "6/18/2012";
            }
        }
        
        /// <summary>
        /// A class for looking up Add-in id strings declared in the associated configuration xml file (.esriaddinx).
        /// </summary>
        internal class IDs {
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandServiceSetup', the id declared for Add-in Button class 'commandServiceSetup'
            /// </summary>
            internal static string commandServiceSetup {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandServiceSetup";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandLoad', the id declared for Add-in Button class 'commandLoad'
            /// </summary>
            internal static string commandLoad {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandLoad";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandAddData', the id declared for Add-in Button class 'commandAddData'
            /// </summary>
            internal static string commandAddData {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandAddData";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandAddRasterData', the id declared for Add-in Button class 'commandAddRasterData'
            /// </summary>
            internal static string commandAddRasterData {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandAddRasterData";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandRandomSample', the id declared for Add-in Button class 'commandRandomSample'
            /// </summary>
            internal static string commandRandomSample {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandRandomSample";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandStratifiedRandomSample', the id declared for Add-in Button class 'commandStratifiedRandomSample'
            /// </summary>
            internal static string commandStratifiedRandomSample {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandStratifiedRandomSample";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandCreateGLCM', the id declared for Add-in Button class 'commandCreateGLCM'
            /// </summary>
            internal static string commandCreateGLCM {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandCreateGLCM";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandPlrClassification', the id declared for Add-in Button class 'commandPlrClassification'
            /// </summary>
            internal static string commandPlrClassification {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandPlrClassification";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandPLRModel', the id declared for Add-in Button class 'commandPLRModel'
            /// </summary>
            internal static string commandPLRModel {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandPLRModel";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandSampleGLCM', the id declared for Add-in Button class 'commandSampleGLCM'
            /// </summary>
            internal static string commandSampleGLCM {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandSampleGLCM";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandSampleRaster', the id declared for Add-in Button class 'commandSampleRaster'
            /// </summary>
            internal static string commandSampleRaster {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandSampleRaster";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandClipRaster', the id declared for Add-in Button class 'commandClipRaster'
            /// </summary>
            internal static string commandClipRaster {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandClipRaster";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandExportToCsv', the id declared for Add-in Button class 'commandExportToCsv'
            /// </summary>
            internal static string commandExportToCsv {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandExportToCsv";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandRegModel', the id declared for Add-in Button class 'commandRegModel'
            /// </summary>
            internal static string commandRegModel {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandRegModel";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandRegRun', the id declared for Add-in Button class 'commandRegRun'
            /// </summary>
            internal static string commandRegRun {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandRegRun";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandArithmeticRaster', the id declared for Add-in Button class 'commandArithmeticRaster'
            /// </summary>
            internal static string commandArithmeticRaster {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandArithmeticRaster";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandFocalAnalysis', the id declared for Add-in Button class 'commandFocalAnalysis'
            /// </summary>
            internal static string commandFocalAnalysis {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandFocalAnalysis";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandLogicalAnalysis', the id declared for Add-in Button class 'commandLogicalAnalysis'
            /// </summary>
            internal static string commandLogicalAnalysis {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandLogicalAnalysis";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandRescaleRaster', the id declared for Add-in Button class 'commandRescaleRaster'
            /// </summary>
            internal static string commandRescaleRaster {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandRescaleRaster";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandConvolutionRaster', the id declared for Add-in Button class 'commandConvolutionRaster'
            /// </summary>
            internal static string commandConvolutionRaster {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandConvolutionRaster";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandConditionalRaster', the id declared for Add-in Button class 'commandConditionalRaster'
            /// </summary>
            internal static string commandConditionalRaster {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandConditionalRaster";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandSaveRaster', the id declared for Add-in Button class 'commandSaveRaster'
            /// </summary>
            internal static string commandSaveRaster {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandSaveRaster";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandFunctionalModeling', the id declared for Add-in Button class 'commandFunctionalModeling'
            /// </summary>
            internal static string commandFunctionalModeling {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandFunctionalModeling";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandSampleRasterCluster', the id declared for Add-in Button class 'commandSampleRasterCluster'
            /// </summary>
            internal static string commandSampleRasterCluster {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandSampleRasterCluster";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandCompositeRaster', the id declared for Add-in Button class 'commandCompositeRaster'
            /// </summary>
            internal static string commandCompositeRaster {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandCompositeRaster";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandSummarizeRaster', the id declared for Add-in Button class 'commandSummarizeRaster'
            /// </summary>
            internal static string commandSummarizeRaster {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandSummarizeRaster";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandLinearTransformRaster', the id declared for Add-in Button class 'commandLinearTransformRaster'
            /// </summary>
            internal static string commandLinearTransformRaster {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandLinearTransformRaster";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandExplodeSample', the id declared for Add-in Button class 'commandExplodeSample'
            /// </summary>
            internal static string commandExplodeSample {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandExplodeSample";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandExtractRasterBands', the id declared for Add-in Button class 'commandExtractRasterBands'
            /// </summary>
            internal static string commandExtractRasterBands {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandExtractRasterBands";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandRemap', the id declared for Add-in Button class 'commandRemap'
            /// </summary>
            internal static string commandRemap {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandRemap";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandHelp', the id declared for Add-in Button class 'commandHelp'
            /// </summary>
            internal static string commandHelp {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandHelp";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandVersion', the id declared for Add-in Button class 'commandVersion'
            /// </summary>
            internal static string commandVersion {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandVersion";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandFIABiomassSummary', the id declared for Add-in Button class 'commandFIABiomassSummary'
            /// </summary>
            internal static string commandFIABiomassSummary {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandFIABiomassSummary";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandAddTiledImageServer', the id declared for Add-in Button class 'commandAddTiledImageServer'
            /// </summary>
            internal static string commandAddTiledImageServer {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandAddTiledImageServer";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandAddFunctionModelToMap', the id declared for Add-in Button class 'commandAddFunctionModelToMap'
            /// </summary>
            internal static string commandAddFunctionModelToMap {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandAddFunctionModelToMap";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandDeleteFunctionModel', the id declared for Add-in Button class 'commandDeleteFunctionModel'
            /// </summary>
            internal static string commandDeleteFunctionModel {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandDeleteFunctionModel";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandSedByArival', the id declared for Add-in Button class 'commandSedByArival'
            /// </summary>
            internal static string commandSedByArival {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandSedByArival";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandLandfireSegmentation', the id declared for Add-in Button class 'commandLandfireSegmentation'
            /// </summary>
            internal static string commandLandfireSegmentation {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandLandfireSegmentation";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandMathRaster', the id declared for Add-in Button class 'commandMathRaster'
            /// </summary>
            internal static string commandMathRaster {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandMathRaster";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandLandscapeMetrics', the id declared for Add-in Button class 'commandLandscapeMetrics'
            /// </summary>
            internal static string commandLandscapeMetrics {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandLandscapeMetrics";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandConvertPixelTypes', the id declared for Add-in Button class 'commandConvertPixelTypes'
            /// </summary>
            internal static string commandConvertPixelTypes {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandConvertPixelTypes";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandCalculateRasterStatistics', the id declared for Add-in Button class 'commandCalculateRasterStatistics'
            /// </summary>
            internal static string commandCalculateRasterStatistics {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandCalculateRasterStatistics";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandAccuracyAssessment', the id declared for Add-in Button class 'commandAccuracyAssessment'
            /// </summary>
            internal static string commandAccuracyAssessment {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandAccuracyAssessment";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandWebSite', the id declared for Add-in Button class 'commandWebSite'
            /// </summary>
            internal static string commandWebSite {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandWebSite";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandCreateMosaic', the id declared for Add-in Button class 'commandCreateMosaic'
            /// </summary>
            internal static string commandCreateMosaic {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandCreateMosaic";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_commandMergeRasters', the id declared for Add-in Button class 'commandMergeRasters'
            /// </summary>
            internal static string commandMergeRasters {
                get {
                    return "USDA_Forest_Service_servicesToolBar_commandMergeRasters";
                }
            }
            
            /// <summary>
            /// Returns 'USDA_Forest_Service_servicesToolBar_rmrsRasterUtilityExtension', the id declared for Add-in Extension class 'rmrsRasterUtilityExtension'
            /// </summary>
            internal static string rmrsRasterUtilityExtension {
                get {
                    return "USDA_Forest_Service_servicesToolBar_rmrsRasterUtilityExtension";
                }
            }
        }
    }
    
internal static class ArcMap
{
  private static IApplication s_app = null;
  private static IDocumentEvents_Event s_docEvent;

  public static IApplication Application
  {
    get
    {
      if (s_app == null)
        s_app = Internal.AddInStartupObject.GetHook<IMxApplication>() as IApplication;

      return s_app;
    }
  }

  public static IMxDocument Document
  {
    get
    {
      if (Application != null)
        return Application.Document as IMxDocument;

      return null;
    }
  }
  public static IMxApplication ThisApplication
  {
    get { return Application as IMxApplication; }
  }
  public static IDockableWindowManager DockableWindowManager
  {
    get { return Application as IDockableWindowManager; }
  }
  public static IDocumentEvents_Event Events
  {
    get
    {
      s_docEvent = Document as IDocumentEvents_Event;
      return s_docEvent;
    }
  }
}

namespace Internal
{
  [StartupObjectAttribute()]
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
  [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
  public sealed partial class AddInStartupObject : AddInEntryPoint
  {
    private static AddInStartupObject _sAddInHostManager;
    private List<object> m_addinHooks = null;

    [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
    public AddInStartupObject()
    {
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
    protected override bool Initialize(object hook)
    {
      bool createSingleton = _sAddInHostManager == null;
      if (createSingleton)
      {
        _sAddInHostManager = this;
        m_addinHooks = new List<object>();
        m_addinHooks.Add(hook);
      }
      else if (!_sAddInHostManager.m_addinHooks.Contains(hook))
        _sAddInHostManager.m_addinHooks.Add(hook);

      return createSingleton;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
    protected override void Shutdown()
    {
      _sAddInHostManager = null;
      m_addinHooks = null;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
    internal static T GetHook<T>() where T : class
    {
      if (_sAddInHostManager != null)
      {
        foreach (object o in _sAddInHostManager.m_addinHooks)
        {
          if (o is T)
            return o as T;
        }
      }

      return null;
    }

    // Expose this instance of Add-in class externally
    public static AddInStartupObject GetThis()
    {
      return _sAddInHostManager;
    }
  }
}
}
