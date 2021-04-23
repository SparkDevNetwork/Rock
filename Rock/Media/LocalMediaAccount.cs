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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using Rock.Data;
using Rock.Model;
using Rock.Web;

namespace Rock.Media
{
    /// <summary>
     /// 
     /// </summary>
     /// <seealso cref="Rock.Media.MediaAccountComponent" />
    [Description( "Local Media Account" )]
    [Export( typeof( MediaAccountComponent ) )]
    [ExportMetadata( "ComponentName", "LocalMediaAccount" )]
    public class LocalMediaAccount : MediaAccountComponent
    {
        #region Media Account Component Implementation

        /// <summary>
        /// Gets the html to display on media account detail page.
        /// </summary>
        /// <param name="mediaAccount">The media account.</param>
        /// <returns></returns>
        public override string GetAccountSummary( MediaAccount mediaAccount )
        {
            return $"<p>{mediaAccount.Name}</p>";
        }

        /// <summary>
        /// Gets the html to display on media folder detail page.
        /// </summary>
        /// <param name="mediaAccount">The media account.</param>
        /// <param name="folderId">The folder identifier.</param>
        /// <returns></returns>
        public override string GetFolderSummary( MediaAccount mediaAccount, string folderId )
        {
            var folderSummaryHtml = string.Empty;
            var folderIdInt = folderId.AsIntegerOrNull();
            if ( mediaAccount != null && folderIdInt.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var mediaFolder = new MediaFolderService( rockContext ).Get( folderIdInt.Value );
                    if ( mediaFolder != null )
                    {
                        var descriptionList = new DescriptionList();
                        descriptionList.Add( "Name", mediaFolder.Name );
                        if ( mediaFolder.Description.IsNotNullOrWhiteSpace() )
                        {
                            descriptionList.Add( "Description", mediaFolder.Description );
                        }

                        folderSummaryHtml = descriptionList.Html;
                    }
                }
            }
    
            return folderSummaryHtml;
        }

        /// <summary>
        /// Gets the html to display on media element detail page.
        /// </summary>
        /// <param name="mediaAccount">The media account.</param>
        /// <param name="folderId">The folder identifier.</param>
        /// <param name="elementId">The element identifier.</param>
        /// <returns></returns>
        public override string GetMediaElementSummary( MediaAccount mediaAccount, string folderId, string elementId )
        {
            var elementSummaryHtml = string.Empty;
            var elementIdInt = elementId.AsIntegerOrNull();
            if ( mediaAccount != null && elementIdInt.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var mediaElement = new MediaElementService( rockContext ).Get( elementIdInt.Value );
                    if ( mediaElement != null )
                    {
                        var descriptionList = new DescriptionList();
                        descriptionList.Add( "Name", mediaElement.Name );
                        if ( mediaElement.Description.IsNotNullOrWhiteSpace() )
                        {
                            descriptionList.Add( "Description", mediaElement.Description );
                        }

                        elementSummaryHtml = descriptionList.Html;
                    }
                }
            }

            return elementSummaryHtml;
        }

        /// <summary>
        /// Gets a value if this account allows the individual to add/edit/delete folders and media files.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allows manual entry]; otherwise, <c>false</c>.
        /// </value>
        public override bool AllowsManualEntry
        {
            get { return true; }
        }

        /// <summary>
        /// Returns a boolean value indicating if full sync of the folders and media is successful. 
        /// </summary>
        /// <param name="mediaAccount">The media account.</param>
        /// <returns></returns>
        public override bool SyncMedia( MediaAccount mediaAccount )
        {
            return false;
        }

        /// <summary>
        /// Returns a boolean value indicating if analytics is synced. 
        /// </summary>
        /// <param name="mediaAccount">The media account.</param>
        /// <returns></returns>
        public override bool SyncAnalytics( MediaAccount mediaAccount )
        {
            return false;
        }

        /// <summary>
        /// Gets the list of all the media elements for the given folder from the provider.
        /// </summary>
        /// <param name="mediaAccount">The media account.</param>
        /// <param name="folderId">The folder identifier.</param>
        /// <returns></returns>
        public override List<MediaElement> GetMediaElementsfromFolder( MediaAccount mediaAccount, string folderId )
        {
            var mediaElements = new List<MediaElement>();
            var folderIdInt = folderId.AsIntegerOrNull();
            if ( mediaAccount != null && folderIdInt.HasValue )
            {
                var rockContext = new RockContext();
                mediaElements = new MediaElementService( rockContext )
                    .Queryable()
                    .Where( a => a.MediaFolderId == folderIdInt && a.MediaFolder.MediaAccountId == mediaAccount.Id )
                    .ToList();
            }

            return mediaElements;
        }

        /// <summary>
        /// Gets the list of folders from the provier.
        /// </summary>
        /// <param name="mediaAccount">The media account.</param>
        /// <returns></returns>
        public override List<MediaFolder> GetFolders( MediaAccount mediaAccount )
        {
            var mediaFolders = new List<MediaFolder>();
            if ( mediaAccount != null )
            {
                var rockContext = new RockContext();
                mediaFolders = new MediaFolderService( rockContext )
                    .Queryable()
                    .Where( a => a.MediaAccountId == mediaAccount.Id )
                    .ToList();
            }

            return mediaFolders;
        }

        #endregion
    }
}