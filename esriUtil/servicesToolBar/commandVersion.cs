﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;


namespace servicesToolBar
{
    public class commandVersion : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public commandVersion()
        {
        }

        protected override void OnClick()
        {
            string txt = "RMRS Raster Utility\nAuthor: " + servicesToolBar.ThisAddIn.Author + "\nVersion: " + servicesToolBar.ThisAddIn.Version;
            System.Windows.Forms.MessageBox.Show(txt,"Toolbar Info",System.Windows.Forms.MessageBoxButtons.OK,System.Windows.Forms.MessageBoxIcon.Information);
        }

        protected override void OnUpdate()
        {
        }
    }
}
