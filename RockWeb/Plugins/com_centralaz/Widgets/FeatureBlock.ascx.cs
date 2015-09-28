// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_centralaz.Widgets
{
    /// <summary>
    /// A simple, easy to use photo gallery widget that stores photos on the filesystem.
    /// </summary>
    [DisplayName( "Feature Block" )]
    [Category( "com_centralaz > Widgets" )]
    [Description( "Allows a user to select a photo to display in a zone." )]

    [SecurityAction( Authorization.EDIT, "The roles and/or users that can edit the HTML content." )]
    [SecurityAction( Authorization.APPROVE, "The roles and/or users that have access to approve HTML content." )]

    [TextField( "Image Subfolder", "The subfolder to use when displaying or uploading images. It will be appended to the base folder ~/Content/ExternalSite/Headers/", false, "", "", 2 )]
    [TextField( "Feature Title" )]

    [CodeEditorField( "Lava Template", "Lava template to use to display the header.", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 400, true, @"{% include '~/Plugins/com_centralaz/Widgets/Lava/FeatureBlock.lava' %}", "", 2 )]
    [BooleanField( "Enable Debug", "Display a list of merge fields available for lava.", false, "", 3 )]

    public partial class FeatureBlock : RockBlockCustomSettings
    {
        #region Fields

        private string _virtualBasePath = "~/Content/ExternalSite/Headers";
        private string _physicalPath;
        private int? _height = null;
        private int? _width = null;
        private bool? _specifyingSize = null;

        #endregion

        #region Properties

        /// <summary>
        /// Relative path to the Images Folder
        /// </summary>
        public string ImageFolderPath { get; set; }

        /// <summary>
        /// The number of milliseconds to pause beteween photos.
        /// </summary>
        /// <value>
        /// number of milliseconds
        /// </value>
        public int PauseMilliseconds
        {
            get
            {
                return GetAttributeValue( "PauseSeconds" ).AsInteger() * 1000;
            }
            private set { }
        }

        /// <summary>
        /// Gets a value indicating whether the height and width should be set on each image.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [specifying size]; otherwise, <c>false</c>.
        /// </value>
        public bool SpecifyingSize
        {
            get
            {
                if ( !_specifyingSize.HasValue )
                {
                    _specifyingSize = GetAttributeValue( "SetSize" ).AsBooleanOrNull();
                }
                return _specifyingSize ?? false;
            }
            private set { }
        }

        /// <summary>
        /// The height to constrain the photo to use.
        /// </summary>
        /// <value>
        /// height in pixels
        /// </value>
        public int? Height
        {
            get
            {
                if ( !_height.HasValue )
                {
                    _height = GetAttributeValue( "Height" ).AsInteger();
                }
                return _height;
            }
            private set { }
        }

        /// <summary>
        /// The width to constrain the photo to use.
        /// </summary>
        /// <value>
        /// width in pixels
        /// </value>
        public int? Width
        {
            get
            {
                if ( !_width.HasValue )
                {
                    _width = GetAttributeValue( "Width" ).AsInteger();
                }
                return _width;
            }
            private set { }
        }

        /// <summary>
        /// Gets the settings tool tip.
        /// </summary>
        /// <value>
        /// The settings tool tip.
        /// </value>
        public override string SettingsToolTip
        {
            get
            {
                return "Edit Photos";
            }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddCSSLink( ResolveRockUrl( "~/Plugins/com_centralaz/Widgets/Styles/dropzone.css" ) );
            RockPage.AddScriptLink( "~/Plugins/com_centralaz/Widgets/Scripts/dropzone.js" );

            this.BlockUpdated += HtmlContentDetail_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlHtmlContent );

            var subfolder = GetAttributeValue( "ImageSubfolder" );

            // Tack on the subfolder if given.
            if ( !string.IsNullOrEmpty( subfolder ) )
            {
                ImageFolderPath = string.Format( "{0}{1}{2}", _virtualBasePath, subfolder.StartsWith( "/" ) ? "" : "/", subfolder );
            }
            else
            {
                ImageFolderPath = _virtualBasePath;
            }

            _physicalPath = Server.MapPath( ImageFolderPath );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            if ( !this.IsPostBack )
            {
                ShowView();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the HtmlContentDetail control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void HtmlContentDetail_BlockUpdated( object sender, EventArgs e )
        {
            ShowView();
        }

        /// <summary>
        /// Handles the Click event of the lbOk control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbOk_Click( object sender, EventArgs e )
        {
            SetAttributeValue( "FeatureTitle", tbTitle.Text );
            SaveAttributeValues();

            ShowView();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the settings.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override void ShowSettings()
        {
            tbTitle.Text = GetAttributeValue( "FeatureTitle" );
            pnlEditModel.Visible = true;
            upnlHtmlContent.Update();
            mdEdit.Show();
        }

        /// <summary>
        /// Shows the view.
        /// </summary>
        protected void ShowView()
        {
            mdEdit.Hide();
            pnlEditModel.Visible = false;
            upnlHtmlContent.Update();
            string html = string.Empty;

            // add content to the content window
            BindViewData();
        }

        /// <summary>
        /// Get File Name
        /// </summary>
        /// <param name="path">full path</param>
        /// <returns>string containing the file name</returns>
        private string GetFileName( string path )
        {
            DateTime timestamp = DateTime.Now;
            string fileName = string.Empty;
            try
            {
                if ( path.Contains( '\\' ) ) fileName = path.Split( '\\' ).Last();
                if ( path.Contains( '/' ) ) fileName = path.Split( '/' ).Last();
            }
            catch
            {
            }
            return fileName;
        }

        private void BindViewData()
        {
            var images = GetImageList();

            var mergeFields = new Dictionary<string, object>();

            // add linked pages
            mergeFields.Add( "ImageUrl", ResolveRockUrlIncludeRoot( images.FirstOrDefault() ) );
            mergeFields.Add( "ImageTitle", GetAttributeValue( "FeatureTitle" ) );

            mergeFields.Add( "CurrentPerson", CurrentPerson );
            mergeFields.Add( "CurrentUser", CurrentUser );

            var globalAttributeFields = Rock.Web.Cache.GlobalAttributesCache.GetMergeFields( CurrentPerson );
            globalAttributeFields.ToList().ForEach( d => mergeFields.Add( d.Key, d.Value ) );

            lOutput.Text = GetAttributeValue( "LavaTemplate" ).ResolveMergeFields( mergeFields );

            // show debug info
            if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && IsUserAuthorized( Authorization.EDIT ) )
            {
                lDebug.Visible = true;
                lDebug.Text = mergeFields.lavaDebugInfo();
            }

        }

        private List<string> GetImageList()
        {
            var images = new List<string>();

            try
            {
                var imagesFolder = new DirectoryInfo( _physicalPath );
                foreach ( var item in imagesFolder.EnumerateFiles().OrderByDescending( f => f.LastWriteTime ) )
                {
                    if ( item is FileInfo )
                    {
                        //add virtual path of the image to the images list
                        images.Add( string.Format( "{0}/{1}", ImageFolderPath, item.Name ) );
                    }
                }
            }
            catch
            {
            }
            return images;
        }

        #endregion
    }
}