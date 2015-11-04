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
using System.ComponentModel;
using System.Web;

using Rock.Attribute;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Cms
{
    [DisplayName( "File Manager" )]
    [Category( "CMS" )]
    [Description( "Block that can be used to browse and manage files on the web server" )]

    [TextField( "Root Folder", "The Root folder to browse", true, "~/Content" )]
    [CustomDropdownListField( "Browse Mode", "Select 'image' to show only image files. Select 'doc' to show all files. Also, in 'image' mode, the ImageUploader handler will process uploaded files instead of FileUploader.", "doc,image", true, "doc" )]
    public partial class FileManager : RockBlock
    {
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

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            SetupIFrame();
        }

        /// <summary>
        /// Sets up the iframe.
        /// </summary>
        private void SetupIFrame()
        {
            var globalAttributesCache = GlobalAttributesCache.Read();

            string imageFileTypeWhiteList = globalAttributesCache.GetValue( "ContentImageFiletypeWhitelist" );
            string fileTypeBlackList = globalAttributesCache.GetValue( "ContentFiletypeBlacklist" );

            var iframeUrl = ResolveRockUrl( "~/ckeditorplugins/rockfilebrowser" );
            string rootFolder = GetAttributeValue( "RootFolder" );
            string browseMode = GetAttributeValue( "BrowseMode" );
            if ( string.IsNullOrWhiteSpace( browseMode ) )
            {
                browseMode = "doc";
            }

            iframeUrl += "?rootFolder=" + HttpUtility.UrlEncode( Encryption.EncryptString( rootFolder ) );
            iframeUrl += "&browserMode=" + browseMode;
            iframeUrl += "&fileTypeBlackList=" + HttpUtility.UrlEncode( fileTypeBlackList );
            iframeUrl += "&imageFileTypeWhiteList=" + HttpUtility.UrlEncode( imageFileTypeWhiteList );
            iframeUrl += "&theme=" + this.RockPage.Site.Theme;

            iframeFileBrowser.Src = iframeUrl;
        }
    }
}