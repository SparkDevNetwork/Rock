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

using Rock.Data;
using Rock.Model;
using Rock.UniversalSearch.IndexModels.Attributes;

namespace Rock.UniversalSearch.IndexModels
{
    /// <summary>
    /// Content Channel Item Index
    /// </summary>
    /// <seealso cref="Rock.UniversalSearch.IndexModels.IndexModelBase" />
    public class ContentChannelItemIndex : IndexModelBase
    {
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        [RockIndexField( Analyzer = "snowball" )] //https://www.elastic.co/blog/found-text-analysis-part-1
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        [RockIndexField( Analyzer = "snowball")] //https://www.elastic.co/blog/found-text-analysis-part-1
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the content channel identifier.
        /// </summary>
        /// <value>
        /// The content channel identifier.
        /// </value>
        [RockIndexField( Type = IndexFieldType.Number)]
        public int ContentChannelId { get; set; }

        /// <summary>
        /// Gets or sets the content channel.
        /// </summary>
        /// <value>
        /// The content channel.
        /// </value>
        [RockIndexField]
        public string ContentChannel { get; set; }

        /// <summary>
        /// Gets or sets the priority.
        /// </summary>
        /// <value>
        /// The priority.
        /// </value>
        [RockIndexField( Type = IndexFieldType.Number, Index = IndexType.NotIndexed )]
        public int Priority { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        [RockIndexField( Type = IndexFieldType.String, Index = IndexType.NotIndexed )]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        [RockIndexField( Type = IndexFieldType.Date )]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the expire date.
        /// </summary>
        /// <value>
        /// The expire date.
        /// </value>
        [RockIndexField( Type = IndexFieldType.Date )]
        public DateTime? ExpireDate { get; set; }

        /// <summary>
        /// Gets or sets the permalink.
        /// </summary>
        /// <value>
        /// The permalink.
        /// </value>
        [RockIndexField( Type = IndexFieldType.String, Index = IndexType.NotIndexed )]
        public string Permalink { get; set; }

        /// <summary>
        /// Gets or sets the slug.
        /// </summary>
        /// <value>
        /// The slug.
        /// </value>
        [RockIndexField( Type = IndexFieldType.String, Index = IndexType.NotIndexed )]
        public string PrimarySlug { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is approved.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is approved; otherwise, <c>false</c>.
        /// </value>
        [RockIndexField( Type = IndexFieldType.Boolean )]
        public bool IsApproved { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        [RockIndexField( Type = IndexFieldType.String, Index = IndexType.NotIndexed )]
        public override string IconCssClass
        {
            get
            {
                return iconCssClass;
            }
            set
            {
                iconCssClass = value;
            }
        }
        private string iconCssClass = "fa fa-bullhorn";

        /// <summary>
        /// Loads the by model.
        /// </summary>
        /// <param name="contentChannelItem">The content channel item.</param>
        /// <returns></returns>
        public static ContentChannelItemIndex LoadByModel(ContentChannelItem contentChannelItem )
        {
            var contentChannelItemIndex = new ContentChannelItemIndex();
            contentChannelItemIndex.SourceIndexModel = "Rock.Model.ContentChannelItem";

            contentChannelItemIndex.Id = contentChannelItem.Id;
            contentChannelItemIndex.Title = contentChannelItem.Title;
            contentChannelItemIndex.Content = contentChannelItem.Content;
            contentChannelItemIndex.ContentChannelId = contentChannelItem.ContentChannelId;
            contentChannelItemIndex.Priority = contentChannelItem.Priority;
            contentChannelItemIndex.Status = contentChannelItem.Status.ToString();
            contentChannelItemIndex.StartDate = contentChannelItem.StartDateTime;
            contentChannelItemIndex.ExpireDate = contentChannelItem.ExpireDateTime;
            contentChannelItemIndex.Permalink = contentChannelItem.Permalink;
            contentChannelItemIndex.IconCssClass = string.IsNullOrWhiteSpace( contentChannelItem.ContentChannel.IconCssClass ) ? "fa fa-bullhorn" : contentChannelItem.ContentChannel.IconCssClass;
            contentChannelItemIndex.IsApproved = false;
            contentChannelItemIndex.ContentChannel = contentChannelItem.ContentChannel.Name;
            contentChannelItemIndex.DocumentName = contentChannelItem.Title;
            contentChannelItemIndex.PrimarySlug = contentChannelItem.PrimarySlug;

            if ( contentChannelItem.ContentChannel != null && ((contentChannelItem.ContentChannel.RequiresApproval && contentChannelItem.ApprovedDateTime != null) || contentChannelItem.ContentChannelType.DisableStatus ) )
            {
                contentChannelItemIndex.IsApproved = true;
            }

            AddIndexableAttributes( contentChannelItemIndex, contentChannelItem );

            return contentChannelItemIndex;
        }

        /// <summary>
        /// Formats the search result.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="displayOptions">The display options.</param>
        /// <param name="mergeFields"></param>
        /// <returns></returns>
        public override FormattedSearchResult FormatSearchResult( Person person, Dictionary<string, object> displayOptions = null, Dictionary<string, object> mergeFields = null )
        {
            string url = string.Empty;
            bool isSecurityDisabled = false;

            if ( displayOptions != null )
            {
                if ( displayOptions.ContainsKey( "ChannelItem-IsSecurityDisabled" ) )
                {
                    isSecurityDisabled = displayOptions["ChannelItem-IsSecurityDisabled"].ToString().AsBoolean();
                }
            }

            if ( !isSecurityDisabled )
            {
                // check security
                var contentChannelItem = new ContentChannelItemService( new RockContext() ).Get( (int)this.Id );
                var isAllowedView = false;

                if (contentChannelItem != null )
                {
                    isAllowedView = contentChannelItem.IsAuthorized( "View", person );
                }

                if ( !isAllowedView )
                {
                    return new FormattedSearchResult() { IsViewAllowed = false };
                }
            }

            // if url was not passed in use default from content channel
            if ( displayOptions == null || !displayOptions.ContainsKey( "ChannelItem-Url" ) )
            {
                var channel = new ContentChannelService( new RockContext() ).Get( this.ContentChannelId );
                if ( channel != null )
                {
                    url = channel.ItemUrl;

                    if ( url.IsNotNullOrWhiteSpace() )
                    {
                        if ( mergeFields == null )
                        {
                            mergeFields = new Dictionary<string, object>();

                        }
                        
                        mergeFields.AddOrReplace( "Id", this.Id );
                        mergeFields.AddOrReplace( "Title", this.Title );
                        mergeFields.AddOrReplace( "ContentChannelId", this.ContentChannelId );
                        mergeFields.AddOrReplace( "Slug", this.PrimarySlug );

                        if (displayOptions == null )
                        {
                            displayOptions = new Dictionary<string, object>();
                        }
                        displayOptions["ChannelItem-Url"] = url.ResolveMergeFields( mergeFields );
                    }
                }
            }

            return base.FormatSearchResult( person, displayOptions );
        }
    }
}
