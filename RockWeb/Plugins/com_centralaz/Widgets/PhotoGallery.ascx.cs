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
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_centralaz.Widgets
{
    /// <summary>
    /// A simple, easy to use photo gallery widget that stores photos on the filesystem.
    /// </summary>
    [DisplayName( "Photo Gallery" )]
    [Category( "com_centralaz > Widgets" )]
    [Description( "Allows a user to select photos to display in a carousel." )]

    [SecurityAction( Authorization.EDIT, "The roles and/or users that can edit the HTML content.")]
    [SecurityAction( Authorization.APPROVE, "The roles and/or users that have access to approve HTML content." )]

    [TextField( "Image Subfolder", "The subfolder to use when displaying or uploading images. It will be appended to the base folder ~/Content/ExternalSite/", false, "", "", 2 )]
    [IntegerField( "Pause Seconds", "The number of seconds to pause on each photo (default 4 seconds).", false, 4 )]

    [BooleanField( "Set Size", "If set to true, the Height and Width settings will be used in the image tag's style setting. NOTE: Constraining size can cause distortion to responsive images when user resizes browser window.", false, "Sizing", 0 )]
    [IntegerField( "Height", "The height (in px) to constrain the photo.", false, -1, "Sizing", 1 )]
    [IntegerField( "Width", "The width (in px) to constrain the photo.", false, -1, "Sizing", 2 )]

    public partial class PhotoGallery : RockBlockCustomSettings
    {
        #region Fields

        private string _virtualBasePath = "~/Content/ExternalSite/";
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
                return GetAttributeValue("PauseSeconds").AsInteger() * 1000;
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
                if ( ! _height.HasValue )
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
                if ( ! _width.HasValue )
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
                ImageFolderPath = string.Format( "{0}{1}{2}", _virtualBasePath, subfolder.StartsWith("/") ? "" : "/", subfolder);
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
            else
            {
                BindEditData();
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
            ShowView();
        }

        protected void lvImages_ItemDataBound( object sender, ListViewItemEventArgs e )
        {

        }

        protected void rptPhoto_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( SpecifyingSize  )
            {
                if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
                {
                    var imgPhoto = e.Item.FindControl( "imgPhoto" ) as HtmlImage;
                    if ( imgPhoto != null )
                    {
                        imgPhoto.Attributes.Add( "style", string.Format( "height: {0}px; width:{1}px", Height, Width ) );
                    }
                }
            }
        }

        /// <summary>
        /// Redirects to the full image when the image is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void imbItem_Command( object sender, CommandEventArgs e )
        {
            Response.Redirect( e.CommandArgument as string );
        }

        /// <summary>
        /// Performs commands for bound buttons in the ImageListView. In this case 
        /// 'Remove (Delete)'
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lvImages_ItemCommand( object sender, ListViewCommandEventArgs e )
        {
            /* We have not bound the control to any DataSource derived controls, 
            nor do we use any key to identify the image. Hence, it makes more sense not to 
            use 'delete' but to use a custom command 'Remove' which can be fired as a 
            generic ItemCommand, and the ListViewCommandEventArgs e will have 
            the CommandArgument passed by the 'Remove' button In this case, it is the bound 
            ImageUrl that we are passing, and making use it of to delete the image.*/
            switch ( e.CommandName )
            {
                case "Remove":
                    var path = e.CommandArgument as string;
                    if ( path != null )
                    {
                        try
                        {
                            FileInfo fi = new FileInfo( Server.MapPath( path ) );
                            fi.Delete();

                            //Display message
                            Parent.Controls.Add( new Label()
                            {
                                Text = GetFileName( path ) + " deleted successfully!"
                            } );

                        }
                        catch ( Exception ex )
                        {
                        }
                    }
                    BindEditData();
                    break;
                default:
                    break;
            }
        }


        #endregion

        #region Methods

        /// <summary>
        /// Shows the settings.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override void ShowSettings()
        {
            pnlEditModel.Visible = true;
            upnlHtmlContent.Update();
            mdEdit.Show();
        }

        //private Dictionary<string, object> GetPageProperties()
        //{
        //    Dictionary<string, object> pageProperties = new Dictionary<string, object>();
        //    pageProperties.Add( "Id", this.RockPage.PageId.ToString() );
        //    pageProperties.Add( "BrowserTitle", this.RockPage.BrowserTitle );
        //    pageProperties.Add( "PageTitle", this.RockPage.PageTitle );
        //    pageProperties.Add( "Site", this.RockPage.Site.Name );
        //    pageProperties.Add( "SiteId", this.RockPage.Site.Id.ToString() );
        //    pageProperties.Add( "LayoutId", this.RockPage.Layout.Id.ToString() );
        //    pageProperties.Add( "Layout", this.RockPage.Layout.Name );
        //    pageProperties.Add( "SiteTheme", this.RockPage.Site.Theme );
        //    pageProperties.Add( "PageIcon", this.RockPage.PageIcon );
        //    pageProperties.Add( "Description", this.RockPage.MetaDescription );
        //    return pageProperties;
        //}

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
            catch ( Exception ex )
            {
            }
            return fileName;
        }

        /// <summary>
        /// Binds the lvImage to current DataSource
        /// </summary>
        private void BindEditData()
        {
            var images = GetImageList();

            lvImages.DataSource = images;
            lvImages.DataBind();

        }

        private void BindViewData()
        {
            var images = GetImageList();

            rptPhoto.DataSource = images;
            rptPhoto.DataBind();

        }

        private List<string> GetImageList()
        {
            var images = new List<string>();

            try
            {
                var imagesFolder = new DirectoryInfo( _physicalPath );
                foreach ( var item in imagesFolder.EnumerateFiles() )
                {
                    if ( item is FileInfo )
                    {
                        //add virtual path of the image to the images list
                        images.Add( string.Format( "{0}/{1}", ImageFolderPath, item.Name ) );
                    }
                }
            }
            catch ( Exception ex )
            {
                //log exception
            }
            return images;
        }

        #endregion

        enum Tristate : byte
        {
            Unknown = 0,
            True = 1,
            False = 2
        }
    }
}