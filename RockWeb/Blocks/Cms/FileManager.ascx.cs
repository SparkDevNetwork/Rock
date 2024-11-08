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
using System.ComponentModel;
using System.Web;

using Rock;
using Rock.Attribute;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Cms
{
    [DisplayName( "File Manager" )]
    [Category( "CMS" )]
    [Description( "Block that can be used to browse and manage files on the web server" )]

    #region Block Attributes

    [TextField(
        "Root Folder",
        Description = "The Root folder to browse",
        IsRequired = true,
        DefaultValue = "~/Content",
        Order = 0,
        Key = AttributeKey.RootFolder )]

    [CustomDropdownListField(
        "Browse Mode",
        Description = "Select 'image' to show only image files. Select 'doc' to show all files. Also, in 'image' mode, the ImageUploader handler will process uploaded files instead of FileUploader.",
        ListSource = "doc,image",
        IsRequired = true,
        DefaultValue = "doc",
        Order = 1,
        Key = AttributeKey.BrowseMode )]

    [LinkedPage(
        "File Editor Page",
        Description = "Page used to edit  the contents of a file.",
        IsRequired = false,
        Order = 2,
        Key = AttributeKey.FileEditorPage )]

    [BooleanField(
        "Enable Zip Upload",
        Key = AttributeKey.ZipUploaderEnabled,
        Description = "Set this to true to enable the Zip File uploader.",
        DefaultBooleanValue = false,
        Order = 3
        )]

    #endregion Block Attributes
    [Rock.SystemGuid.BlockTypeGuid( "BA327D25-BD8A-4B67-B04C-17B499DDA4B6" )]
    public partial class FileManager : RockBlock
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string RootFolder = "RootFolder";
            public const string BrowseMode = "BrowseMode";
            public const string FileEditorPage = "FileEditorPage";
            public const string ZipUploaderEnabled = "ZipUploaderEnabled";
        }

        #endregion Attribute Keys

        #region Page Parameter Keys

        private static class PageParameterKey
        {
            public const string RelativeFilePath = "RelativeFilePath";
        }

        #endregion Page Parameter Keys

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

            SetupIFrame();
        }

        #endregion Base Control Methods

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            SetupIFrame();
        }

        #endregion Events

        #region Methods

        /// <summary>
        /// Sets up the iframe.
        /// </summary>
        private void SetupIFrame()
        {
            var iframeUrl = ResolveRockUrl( "~/htmleditorplugins/rockfilebrowser" );
            string rootFolder = GetAttributeValue( AttributeKey.RootFolder );
            bool zipUploaderEnabled = GetAttributeValue( AttributeKey.ZipUploaderEnabled ).AsBoolean();
            string browseMode = GetAttributeValue( AttributeKey.BrowseMode );
            string url = LinkedPageUrl( AttributeKey.FileEditorPage );
            if ( string.IsNullOrWhiteSpace( browseMode ) )
            {
                browseMode = "doc";
            }

            iframeUrl += "?RootFolder=" + HttpUtility.UrlEncode( Encryption.EncryptString( rootFolder ) );

            if ( zipUploaderEnabled )
            {
                iframeUrl += "&ZipUploaderEnabled=" + HttpUtility.UrlEncode( Encryption.EncryptString( zipUploaderEnabled.ToTrueFalse() ) );
            }

            iframeUrl += "&BrowserMode=" + browseMode;
            iframeUrl += "&EditFilePage=" + HttpUtility.UrlEncode( url );

            if ( PageParameter( PageParameterKey.RelativeFilePath ).IsNotNullOrWhiteSpace() )
            {
                iframeUrl += "&RelativeFilePath=" + HttpUtility.UrlEncode( PageParameter( PageParameterKey.RelativeFilePath ) );
            }
            
            iframeUrl += "&EditorTheme=" + this.RockPage.Site.Theme;

            iframeFileBrowser.Src = iframeUrl;
        }

        #endregion Methods
    }
}