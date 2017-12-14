// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Web.UI;
using Rock.Web;
using System.IO.Compression;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Font Awesome Settings" )]
    [Category( "CMS" )]
    [Description( "Block that can be used to configure Font Awesome" )]
    public partial class FontAwesomeSettings : RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                var fontAwesomeProKey = SystemSettings.GetValue( "core_FontAwesomeProKey" );
                tbFontAwesomeProKey.Text = fontAwesomeProKey;
                pnlFontAwesomeFree.Visible = fontAwesomeProKey.IsNullOrWhiteSpace();
                btnInstallUpdate.Text = fontAwesomeProKey.IsNullOrWhiteSpace() ? "Install" : "Update";
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }

        /// <summary>
        /// Handles the Click event of the btnInstallUpdate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnInstallUpdate_Click( object sender, EventArgs e )
        {
            var fontAwesomeProKey = SystemSettings.GetValue( "core_FontAwesomeProKey" );
            List<string> updatedItems = new List<string>();

            if ( fontAwesomeProKey != tbFontAwesomeProKey.Text )
            {
                SystemSettings.SetValue( "core_FontAwesomeProKey", tbFontAwesomeProKey.Text );
                updatedItems.Add( "Font Awesome Pro Key" );
            }

            if ( fupFontAwesomeProPackage.BinaryFileId.HasValue )
            {
                var rockContext = new RockContext();
                var fontawesomePackageBinaryFile = new BinaryFileService( rockContext ).Get( fupFontAwesomeProPackage.BinaryFileId.Value );
                if ( fontawesomePackageBinaryFile != null )
                {
                    ZipArchive fontawesomePackageZip = new ZipArchive( fontawesomePackageBinaryFile.ContentStream );
                    string webFontsWithCssFolder = fontawesomePackageZip.Entries.Where( a => a.FullName.EndsWith( "/web-fonts-with-css/" ) ).Select( a => a.FullName ).FirstOrDefault();

                    var webFontsWithCssFiles = fontawesomePackageZip.Entries.Where( a => a.Name.IsNotNullOrWhitespace() && a.FullName.StartsWith( webFontsWithCssFolder ) ).ToList();

                    var lessFileEntries = webFontsWithCssFiles
                        .Where( a => new DirectoryInfo( Path.GetDirectoryName( a.FullName ) ).Name.Equals( "less" ) ).ToList();

                    var webFontsFileEntries = webFontsWithCssFiles
                        .Where( a => new DirectoryInfo( Path.GetDirectoryName( a.FullName ) ).Name.Equals( "webfonts" ) ).ToList();

                    var fontAwesomeFontsFolder = this.Request.MapPath( "~/Assets/Fonts/FontAwesome" );
                    var fontAwesomeStylesFolder = this.Request.MapPath( "~/Styles/FontAwesome" );

                    string[] excludedLessFiles = new string[] { "fa-regular.less", "fa-solid.less", "fontawesome.less" };
                    string[] parsedLessFiles = new string[] { "fa-brands.less", "fa-light.less" };
                    foreach ( var lessFileEntry in lessFileEntries )
                    {
                        if ( !excludedLessFiles.Contains( lessFileEntry.Name ) )
                        {
                            var destLessFileName = Path.Combine( fontAwesomeStylesFolder, lessFileEntry.Name );
                            lessFileEntry.ExtractToFile( destLessFileName );

                            if ( parsedLessFiles.Contains( lessFileEntry.Name ) )
                            {
                                // replace 'Font Awesome 5 Pro/Brands' with simply 'Font Awesome 5'
                                var updatedFileLines = File.ReadAllLines( destLessFileName ).Select( a =>
                                    a.Replace( "Font Awesome 5 Pro", "Font Awesome 5" )
                                    .Replace( "Font Awesome 5 Brands", "Font Awesome 5" )
                                );

                                File.WriteAllLines( destLessFileName, updatedFileLines );
                            }
                        }
                    }

                    foreach ( var webFontsFileEntry in webFontsFileEntries )
                    {
                        var destFontFile = Path.Combine( fontAwesomeFontsFolder, webFontsFileEntry.Name );
                        webFontsFileEntry.ExtractToFile( destFontFile );
                    }


                    updatedItems.Add( "Font Awesome Pro Package" );


                }
            }

            if ( updatedItems.Any() )
            {
                nbInstallSuccess.Text = updatedItems.AsDelimited( ", ", " and " ) + " updated.";
                nbInstallSuccess.NotificationBoxType = NotificationBoxType.Success;
                nbInstallSuccess.Visible = true;
            }
            else
            {
                nbInstallSuccess.Text = "Please upload a Font Awesome Pro Package to update the Font Awesome fonts";
                nbInstallSuccess.NotificationBoxType = NotificationBoxType.Warning;
                nbInstallSuccess.Visible = true;
            }
        }

        #endregion
    }
}