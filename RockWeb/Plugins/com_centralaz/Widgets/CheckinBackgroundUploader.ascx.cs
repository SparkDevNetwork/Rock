// <copyright>
// Copyright by Central Christian Church
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
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;

namespace RockWeb.Plugins.com_centralaz.Widgets
{
    /// <summary>
    /// Allows a user to upload a background photo for a check-in theme. Users with admin access can select the themes that users with edit access can update.
    /// </summary>
    [DisplayName( "Background Uploader (for Check-in Themes)" )]
    [Category( "com_centralaz > Widgets" )]
    [Description( "Allows a user to upload a background photo for a check-in theme. Users with admin access can select the themes that users with edit access can update." )]

    [CustomCheckboxListField( "Allowed Themes", "The themes that will be available in the dropdown.", "CheckinAdventureKids,CheckinBlueCrystal,CheckinPark", true )]
    [BooleanField( "Set Size", "If set to true, the Height and Width settings will be used in the image tag's style setting. NOTE: Constraining size can cause distortion to responsive images when user resizes browser window.", false, "Sizing", 0 )]
    [IntegerField( "Height", "The height (in px) to constrain the photo.", false, -1, "Sizing", 1 )]
    [IntegerField( "Width", "The width (in px) to constrain the photo.", false, -1, "Sizing", 2 )]

    public partial class CheckinBackgroundUploader : Rock.Web.UI.RockBlock
    {
        #region Fields

        private int? _height = null;
        private int? _width = null;
        private bool? _specifyingSize = null;

        #endregion

        #region Properties

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

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

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

            if ( !this.IsPostBack )
            {
                LoadAdminDropDown();
                if ( UserCanAdministrate )
                {
                    divAdmin.Visible = true;
                }

                LoadDropDowns();
                SetImage();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the HtmlContentDetail control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            SetImage();
        }

        /// <summary>
        /// Gets the size of the image.
        /// </summary>
        /// <returns></returns>
        protected string GetImageSize()
        {
            if ( SpecifyingSize )
            {
                return string.Format( "height: {0}px; width:{1}px", Height, Width );
            }
            return string.Empty;
        }

        /// <summary>
        /// Handles the FileUploaded event of the fuPhoto control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void fuPhoto_FileUploaded( object sender, EventArgs e )
        {
            var x = ( Rock.Web.UI.Controls.FileUploader ) sender;
            var z = x.UploadedContentFilePath;
            var y = x.UploadUrl;
            SetImage();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlTheme control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlTheme_SelectedIndexChanged( object sender, EventArgs e )
        {
            SetImage();
        }

        /// <summary>
        /// Handles the Click event of the btnUpdateSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnUpdateSettings_Click( object sender, EventArgs e )
        {
            SetAttributeValue( "AllowedThemes", cblAllowedThemes.SelectedValues.AsDelimited( "," ) );
            SaveAttributeValues();

            LoadDropDowns();
            SetImage();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the admin drop down.
        /// </summary>
        private void LoadAdminDropDown()
        {
            var rockContext = new RockContext();
            var blockId = this.BlockId;
            var attribute = new AttributeService( rockContext ).Queryable().Where( a =>
                this.BlockCache.BlockTypeId.ToString() == a.EntityTypeQualifierValue &&
                a.EntityTypeQualifierColumn == "BlockTypeId" &&
                a.Key == "AllowedThemes" )
                .FirstOrDefault();
            if ( attribute != null )
            {
                // Grab the attribute qualifier and value
                var attributeQualifier = new AttributeQualifierService( rockContext ).GetByAttributeId( attribute.Id ).Where( q => q.Key == "values" ).FirstOrDefault();
                var attributeValue = new AttributeValueService( rockContext ).GetByAttributeIdAndEntityId( attribute.Id, blockId );

                if ( attributeValue == null )
                {
                    return;
                }
                // Send the themes in the qualifier and value to a list
                var attributeQualifierThemes = attributeQualifier.Value.SplitDelimitedValues().ToList();
                var attributeValueThemes = attributeValue.Value.SplitDelimitedValues().ToList();
                var directoryThemes = new List<string>();

                // Add any new themes
                var themesFolder = new DirectoryInfo( Server.MapPath( "~/Themes" ) );
                foreach ( var directory in themesFolder.EnumerateDirectories() )
                {
                    if ( directory is DirectoryInfo )
                    {
                        var backgroundImageJpg = new FileInfo( directory.FullName + "/Assets/Images/background.jpg" );
                        var backgroundImagePng = new FileInfo( directory.FullName + "/Assets/Images/background.png" );
                        if ( backgroundImageJpg.Exists || backgroundImagePng.Exists )
                        {
                            directoryThemes.Add( directory.Name );
                            if ( !attributeQualifierThemes.Contains( directory.Name ) )
                            {
                                attributeQualifierThemes.Add( directory.Name );
                            }
                        }
                    }
                }

                // Remove any nonexisting themes
                var toRemove = new List<string>();
                foreach ( var attributeTheme in attributeQualifierThemes )
                {
                    if ( !directoryThemes.Contains( attributeTheme ) )
                    {
                        toRemove.Add( attributeTheme );
                    }
                }

                //Done in two foreach's because modifying a list while iterating through it causes an exception.
                foreach ( var theme in toRemove )
                {
                    attributeQualifierThemes.Remove( theme );
                    attributeValueThemes.Remove( theme );
                }

                // Save changes
                attributeValue.Value = attributeValueThemes.AsDelimited( "," );
                attributeQualifier.Value = attributeQualifierThemes.AsDelimited( "," );
                rockContext.SaveChanges();
                Rock.Web.Cache.BlockCache.Flush( blockId );

                // Populate Admin Checkbox List
                cblAllowedThemes.Items.Clear();
                foreach ( var theme in attributeQualifierThemes )
                {
                    cblAllowedThemes.Items.Add( new ListItem( theme, theme ) );
                }
                cblAllowedThemes.SetValues( attributeValueThemes );
            }
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            var selectedvalue = ddlTheme.SelectedValue;
            ddlTheme.Items.Clear();
            var approvedThemes = GetAttributeValue( "AllowedThemes" ).SplitDelimitedValues();

            var themesFolder = new DirectoryInfo( Server.MapPath( "~/Themes" ) );
            foreach ( var theme in approvedThemes )
            {
                ddlTheme.Items.Add( new ListItem( theme, theme ) );
            }

            if ( !String.IsNullOrWhiteSpace( selectedvalue ) && ddlTheme.Items.FindByValue( selectedvalue ) != null )
            {
                ddlTheme.SelectedValue = selectedvalue;
            }
        }

        /// <summary>
        /// Sets the image.
        /// </summary>
        protected void SetImage()
        {
            imbGalleryItem.ImageUrl = ResolveRockUrlIncludeRoot( GetImageUrl() );
            if ( SpecifyingSize )
            {
                if ( imbGalleryItem != null )
                {
                    imbGalleryItem.Attributes.Add( "style", string.Format( "height: {0}px; width:{1}px", Height, Width ) );
                }
            }

            if ( UserCanEdit )
            {
                fuPhoto.RootFolder = String.Format( "~/Themes/{0}/Assets/Images", ddlTheme.SelectedValue );
            }
            else
            {
                fuPhoto.Visible = false;
            }
        }

        /// <summary>
        /// Gets the image url.
        /// </summary>
        /// <returns></returns>
        private string GetImageUrl()
        {
            var imageUrl = String.Empty;

            try
            {
                var virtualPath = String.Format( "~/Themes/{0}/Assets/Images", ddlTheme.SelectedValue );
                var imagesFolder = new DirectoryInfo( Server.MapPath( virtualPath ) );
                foreach ( var item in imagesFolder.EnumerateFiles() )
                {
                    if ( item is FileInfo )
                    {
                        if ( item.Name == "background.jpg" || item.Name == "background.png" )
                        {
                            imageUrl = string.Format( "{0}/{1}?t={2}", virtualPath, item.Name, DateTime.Now.Ticks );
                        }
                    }
                }
            }
            catch
            {
            }
            return imageUrl;
        }

        #endregion
    }
}