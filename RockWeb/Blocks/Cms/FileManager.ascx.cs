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
    [DisplayName( "File Browser" )]
    [Category( "CMS" )]
    [Description( "Block that can be used to browse and manage files on the web server" )]

    [TextField( "Root Folder", "The Root folder to browse", true, "~/Content" )]
    [CustomDropdownListField( "Browse Mode", "", "doc,image", true, "doc" )]
    public partial class FileManager : RockBlock
    {
        /// <summary>
        /// Handles the Click event of the btnFileBrowser control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnFileBrowser_Click( object sender, EventArgs e )
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
            mdFileBrowser.Visible = true;
            mdFileBrowser.Show();
        }
    }
}