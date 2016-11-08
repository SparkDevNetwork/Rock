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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        [RockIndexField]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        [RockIndexField]
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
            contentChannelItemIndex.SourceIndexModel = "Rock.Model.ContentChannel";

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

            if ( contentChannelItem.ContentChannel != null && contentChannelItem.ContentChannel.RequiresApproval && contentChannelItem.ApprovedDateTime != null )
            {
                contentChannelItemIndex.IsApproved = true;
            }

            AddIndexableAttributes( contentChannelItemIndex, contentChannelItem );

            return contentChannelItemIndex;
        }

        /// <summary>
        /// Gets the document URL.
        /// </summary>
        /// <param name="displayOptions"></param>
        /// <returns></returns>
        public override string GetDocumentUrl( Dictionary<string, object> displayOptions = null )
        {
            string url = string.Empty;

            if ( displayOptions != null )
            {
                if ( displayOptions.ContainsKey( "ChannelItem.Url" ) )
                {
                    url = displayOptions["ChannelItem.Url"].ToString();
                }
            }

            // if url was not passed in use default from content channel
            if ( string.IsNullOrWhiteSpace( url ) )
            {
                var channel = new ContentChannelService( new RockContext() ).Get( this.ContentChannelId );
                url = channel.ItemUrl;
            }

            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "Id", this.Id );
            mergeFields.Add( "Title", this.Title );
            mergeFields.Add( "ContentChannelId", this.ContentChannelId );

            return url.ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Formats the search result.
        /// </summary>
        /// <param name="person"></param>
        /// <param name="displayOptions"></param>
        /// <returns></returns>
        public override FormattedSearchResult FormatSearchResult( Person person, Dictionary<string, object> displayOptions = null )
        {
            bool showSummary = true;
            string url = string.Empty;
            bool isSecurityDisabled = false;

            if ( displayOptions != null )
            {
                if ( displayOptions.ContainsKey( "ChannelItem.ShowSummary" ) )
                {
                    showSummary = displayOptions["ChannelItem.ShowSummary"].ToString().AsBoolean();
                }
                if ( displayOptions.ContainsKey( "ChannelItem.Url" ) )
                {
                    url = displayOptions["ChannelItem.Url"].ToString();
                }
                if ( displayOptions.ContainsKey( "ChannelItem.IsSecurityDisabled" ) )
                {
                    isSecurityDisabled = displayOptions["ChannelItem.IsSecurityDisabled"].ToString().AsBoolean();
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
            if ( string.IsNullOrWhiteSpace( url ) )
            {
                var channel = new ContentChannelService( new RockContext() ).Get( this.ContentChannelId );
                url = channel.ItemUrl;
            }

            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "Id", this.Id );
            mergeFields.Add( "Title", this.Title );
            mergeFields.Add( "ContentChannelId", this.ContentChannelId );

            var summary = this["Summary"] ?? "" + this["summaryText"] ?? "";

            return new FormattedSearchResult() { IsViewAllowed = true, FormattedResult = $@"
                            <div class='row model-cannavigate' data-href='{url.ResolveMergeFields( mergeFields )}'>
                                <div class='col-sm-1'>
                                    <i class='{this.IconCssClass} fa-2x'></i>
                                </div>
                                <div class='col-sm-4'>
                                    {this.Title} <small>({this.ContentChannel})</small>
                                </div>
                                <div class='col-sm-7'>
                                    {(showSummary ? summary : "")}
                                </div>
                            </div>" };
        }
    }
}
