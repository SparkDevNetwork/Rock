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
using System.Collections.Generic;
using Rock.Model;

namespace Rock.Media
{
    /// <summary>
    /// 
    /// </summary>
    public interface IMediaAccountComponent
    {
        /// <summary>
        /// Gets the attribute value for the media account.
        /// </summary>
        /// <param name="mediaAccount">The media account.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        string GetAttributeValue( MediaAccount mediaAccount, string key );

        /// <summary>
        /// Gets the html to display on media account detail page.
        /// </summary>
        /// <param name="mediaAccount">The media account.</param>
        /// <returns></returns>
        string GetAccountSummary( MediaAccount mediaAccount );

        /// <summary>
        /// Gets the html to display on media folder detail page.
        /// </summary>
        /// <param name="mediaAccount">The media account.</param>
        /// <param name="folderId">The folder identifier.</param>
        /// <returns></returns>
        string GetFolderSummary( MediaAccount mediaAccount, string folderId );

        /// <summary>
        /// Gets the html to display on media element detail page.
        /// </summary>
        /// <param name="mediaAccount">The media account.</param>
        /// <param name="folderId">The folder identifier.</param>
        /// <param name="elementId">The element identifier.</param>
        /// <returns></returns>
        string GetMediaElementSummary( MediaAccount mediaAccount, string folderId, string elementId );

        /// <summary>
        /// Gets a value if this account allows the individual to add/edit/delete folders and media files.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allows manual entry]; otherwise, <c>false</c>.
        /// </value>
        bool AllowsManualEntry { get; }

        /// <summary>
        /// Returns a boolean value indicating if full sync of the folders and media is successful. 
        /// </summary>
        /// <param name="mediaAccount">The media account.</param>
        /// <returns></returns>
        bool SyncMedia( MediaAccount mediaAccount );

        /// <summary>
        /// Returns a boolean value indicating if analytics is synced. 
        /// </summary>
        /// <param name="mediaAccount">The media account.</param>
        /// <returns></returns>
        bool SyncAnalytics( MediaAccount mediaAccount );

        /// <summary>
        /// Gets the list of all the media elements for the given folder from the provider.
        /// </summary>
        /// <param name="mediaAccount">The media account.</param>
        /// <param name="folderId">The folder identifier.</param>
        /// <returns></returns>
        List<MediaElement> GetMediaElementsfromFolder( MediaAccount mediaAccount, string folderId );

        /// <summary>
        /// Gets the list of folders from the provier.
        /// </summary>
        /// <param name="mediaAccount">The media account.</param>
        /// <returns></returns>
        List<MediaFolder> GetFolders( MediaAccount mediaAccount );
    }
}