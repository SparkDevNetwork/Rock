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
using Rock.Model;

namespace Rock.Media
{
    /// <summary>
    /// Base class for media account components
    /// </summary>
    public abstract class MediaAccountComponent : Rock.Extension.Component, IMediaAccountComponent
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaAccountComponent"/> class.
        /// </summary>
        public MediaAccountComponent() : base( false )
        {
            // Override default constructor of Component that loads attributes
        }

        #endregion Constructors

        #region Component Overrides

        /// <summary>
        /// Gets the attribute value defaults.
        /// </summary>
        /// <value>
        /// The attribute defaults.
        /// </value>
        public override Dictionary<string, string> AttributeValueDefaults
        {
            get => new Dictionary<string, string>
            {
                { "Active", "True" },
                { "Order", "0" }
            };
        }

        /// <summary>
        /// Gets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public override bool IsActive
        {
            get => true;
        }

        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public override int Order
        {
            get => 0;
        }

        /// <summary>
        /// Gets the value of an attribute key. Do not use this method. Use <see cref="GetAttributeValue(MediaAccount, string)" />
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public override string GetAttributeValue( string key )
        {
            throw new Exception( "Use the GetAttributeValue( MediaAccount mediaAccount, string key ) method instead." );
        }

        #endregion Component Overrides

        #region Public Methods

        /// <summary>
        /// Loads the attributes for the <see cref="MediaAccount" />.
        /// </summary>
        /// <param name="mediaAccount"></param>
        public void LoadAttributes( MediaAccount mediaAccount )
        {
            if ( mediaAccount is null )
            {
                throw new ArgumentNullException( nameof( mediaAccount ) );
            }

            mediaAccount.LoadAttributes();
        }

        /// <summary>
        /// Gets the attribute value for the media account
        /// </summary>
        /// <param name="mediaAccount"></param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public string GetAttributeValue( MediaAccount mediaAccount, string key )
        {
            if ( mediaAccount.AttributeValues == null )
            {
                mediaAccount.LoadAttributes();
            }

            return mediaAccount.GetAttributeValue( key );
        }

        #endregion Public Methods

        #region Abstract Methods

        /// <summary>
        /// Gets the html to display on media account detail page.
        /// </summary>
        /// <param name="mediaAccount">The media account.</param>
        /// <returns></returns>
        public abstract string GetAccountSummary( MediaAccount mediaAccount );

        /// <summary>
        /// Gets the html to display on media folder detail page.
        /// </summary>
        /// <param name="mediaAccount">The media account.</param>
        /// <param name="folderId">The folder identifier.</param>
        /// <returns></returns>
        public abstract string GetFolderSummary( MediaAccount mediaAccount, string folderId );

        /// <summary>
        /// Gets the html to display on media element detail page.
        /// </summary>
        /// <param name="mediaAccount">The media account.</param>
        /// <param name="folderId">The folder identifier.</param>
        /// <param name="elementId">The element identifier.</param>
        /// <returns></returns>
        public abstract string GetMediaElementSummary( MediaAccount mediaAccount, string folderId, string elementId );

        /// <summary>
        /// Returns a boolean value indicating if full sync of the folders and media is successful. 
        /// </summary>
        /// <param name="mediaAccount">The media account.</param>
        /// <returns></returns>
        public abstract bool SyncMedia( MediaAccount mediaAccount );

        /// <summary>
        /// Returns a boolean value indicating if analytics is synced. 
        /// </summary>
        /// <param name="mediaAccount">The media account.</param>
        /// <returns></returns>
        public abstract bool SyncAnalytics( MediaAccount mediaAccount );

        /// <summary>
        /// Gets the list of all the media elements for the given folder from the provider.
        /// </summary>
        /// <param name="mediaAccount">The media account.</param>
        /// <param name="folderId">The folder identifier.</param>
        /// <returns></returns>
        public abstract List<MediaElement> GetMediaElementsfromFolder( MediaAccount mediaAccount, string folderId );

        /// <summary>
        /// Gets the list of folders from the provier.
        /// </summary>
        /// <param name="mediaAccount">The media account.</param>
        /// <returns></returns>
        public abstract List<MediaFolder> GetFolders( MediaAccount mediaAccount );

        /// <summary>
        /// Gets a value if this account allows the individual to add/edit/delete folders and media files.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allows manual entry]; otherwise, <c>false</c>.
        /// </value>
        public abstract bool AllowsManualEntry { get; }

        #endregion Abstract Methods
    }
}
