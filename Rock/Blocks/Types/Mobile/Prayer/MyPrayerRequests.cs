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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Common.Mobile.Blocks.Content;
using Rock.Data;
using Rock.Model;

namespace Rock.Blocks.Types.Mobile.Prayer
{
    /// <summary>
    /// Shows a list of prayer requests that the user has previously entered.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "My Prayer Requests" )]
    [Category( "Mobile > Prayer" )]
    [Description( "Shows a list of prayer requests that the user has previously entered." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [LinkedPage( "Edit Page",
        Description = "The page that will be used for editing a prayer request.",
        IsRequired = false,
        Key = AttributeKeys.EditPage,
        Order = 0 )]

    [LinkedPage( "Answer Page",
        Description = "The page that will be used for allowing the user to enter an answer to prayer.",
        IsRequired = false,
        Key = AttributeKeys.AnswerPage,
        Order = 1 )]

    [BlockTemplateField( "Template",
        Description = "The template for how to display the prayer request.",
        TemplateBlockValueGuid = SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_MY_PRAYER_REQUESTS,
        DefaultValue = "DED26289-4746-4233-A5BD-D4095248023D",
        IsRequired = true,
        Key = AttributeKeys.Template,
        Order = 2 )]

    [BooleanField( "Show Expired",
        Description = "Include expired prayer requests in the list.",
        IsRequired = false,
        DefaultBooleanValue = true,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Key = AttributeKeys.ShowExpired,
        Order = 3 )]

    [IntegerField( "Days Back to Show",
        Description = "The number of days back to search for prayer requests. Leave blank for no limit.",
        IsRequired = false,
        Key = AttributeKeys.DaysBackToShow,
        Order = 4 )]

    [IntegerField( "Max Results",
        Description = "The maximum number of results to display. Leave blank for no limit.",
        IsRequired = false,
        Key = AttributeKeys.MaxResults,
        Order = 5 )]

    [BooleanField( "Include Group Requests",
        Description = "Includes prayer requests that are attached to a group.",
        IsRequired = false,
        DefaultBooleanValue = false,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Key = AttributeKeys.IncludeGroupRequests,
        Order = 6 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_MY_PRAYER_REQUESTS_BLOCK_TYPE )]
    [Rock.SystemGuid.BlockTypeGuid( "C095B269-36E2-446A-B73E-2C8CC4B7BF37")]
    public class MyPrayerRequests : RockBlockType
    {
        #region Block Attributes

        /// <summary>
        /// The block setting attribute keys for the <see cref="MyPrayerRequests"/> block.
        /// </summary>
        protected static class AttributeKeys
        {
            /// <summary>
            /// The edit page key.
            /// </summary>
            public const string EditPage = "EditPage";

            /// <summary>
            /// The answer page key.
            /// </summary>
            public const string AnswerPage = "AnswerPage";

            /// <summary>
            /// The template key attribute key.
            /// </summary>
            public const string Template = "Template";

            /// <summary>
            /// The show expired key.
            /// </summary>
            public const string ShowExpired = "ShowExpired";

            /// <summary>
            /// The days back to show key.
            /// </summary>
            public const string DaysBackToShow = "DaysBackToShow";

            /// <summary>
            /// The maximum results key.
            /// </summary>
            public const string MaxResults = "MaxResults";

            /// <summary>
            /// The include group requests key.
            /// </summary>
            public const string IncludeGroupRequests = "IncludeGroupRequests";
        }

        /// <summary>
        /// Gets the edit page unique identifier.
        /// </summary>
        /// <value>
        /// The edit page unique identifier.
        /// </value>
        protected Guid? EditPageGuid => GetAttributeValue( AttributeKeys.EditPage ).AsGuidOrNull();

        /// <summary>
        /// Gets the answer page unique identifier.
        /// </summary>
        /// <value>
        /// The answer page unique identifier.
        /// </value>
        protected Guid? AnswerPageGuid => GetAttributeValue( AttributeKeys.AnswerPage ).AsGuidOrNull();

        /// <summary>
        /// Gets the template.
        /// </summary>
        /// <value>
        /// The template.
        /// </value>
        protected string Template => Rock.Field.Types.BlockTemplateFieldType.GetTemplateContent( GetAttributeValue( AttributeKeys.Template ) );

        /// <summary>
        /// Gets a value indicating whether to show expired prayer requests.
        /// </summary>
        /// <value>
        ///   <c>true</c> if to show expired prayer requests; otherwise, <c>false</c>.
        /// </value>
        protected bool ShowExpired => GetAttributeValue( AttributeKeys.ShowExpired ).AsBoolean();

        /// <summary>
        /// Gets the number of days back to show prayer requests for.
        /// </summary>
        /// <value>
        /// The number of days back to show prayer requests for.
        /// </value>
        protected int? DaysBackToShow => GetAttributeValue( AttributeKeys.DaysBackToShow ).AsIntegerOrNull();

        /// <summary>
        /// Gets the maximum results to pass to Lava.
        /// </summary>
        /// <value>
        /// The maximum results to pass to Lava.
        /// </value>
        protected int? MaxResults => GetAttributeValue( AttributeKeys.MaxResults ).AsIntegerOrNull();

        /// <summary>
        /// Gets a value that specifies if group requests should be included by default.
        /// If <c>false</c> and no group is specified in the page parameters then any
        /// requests that are attached to a group will be excluded.
        /// </summary>
        /// <value>
        /// A value that specifies if group requests should be included by default.
        /// </value>
        protected bool IncludeGroupRequests => GetAttributeValue( AttributeKeys.IncludeGroupRequests ).AsBoolean( false );

        #endregion

        #region Page Parameters

        private static class PageParameterKey
        {
            /// <summary>
            /// The unique identifier to limit results to when specified.
            /// </summary>
            public const string GroupGuid = "GroupGuid";
        }

        /// <summary>
        /// Gets the unique group identifier that will be used when limiting results
        /// or <c>null</c> if no filtering by group should be performed.
        /// </summary>
        /// <value>
        /// The unique group identifier that will be used when limiting results or
        /// <c>null</c>.
        /// </value>
        protected Guid? GroupGuid => RequestContext.GetPageParameter( PageParameterKey.GroupGuid ).AsGuidOrNull();

        #endregion

        #region IRockMobileBlockType Implementation

        /// <inheritdoc/>
        public override Version RequiredMobileVersion => new Version( 1, 2 );

        /// <summary>
        /// Gets the property values that will be sent to the device in the application bundle.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetMobileConfigurationValues()
        {
            //
            // Indicate that we are a dynamic content providing block.
            //
            return new Rock.Common.Mobile.Blocks.Content.Configuration
            {
                Content = null,
                DynamicContent = true
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Builds the content to be displayed on the block.
        /// </summary>
        /// <returns>A string containing the XAML content to be displayed.</returns>
        private string BuildContent()
        {
            using ( var rockContext = new RockContext() )
            {
                List<PrayerRequest> prayerRequests = new List<PrayerRequest>();

                if ( RequestContext.CurrentPerson != null )
                {
                    var prayerRequestService = new PrayerRequestService( rockContext );
                    var limitDate = DateTime.MinValue;

                    // Configure our limit date to reflect the number of days
                    // back they want to include.
                    if ( DaysBackToShow.HasValue )
                    {
                        limitDate = RockDateTime.Now.AddDays( -DaysBackToShow.Value );
                    }

                    // Build the basic query to filter prayer requests that
                    // the current person has entered.
                    var prayerRequestQuery = prayerRequestService.Queryable()
                        .AsNoTracking()
                        .Where( a => a.RequestedByPersonAlias != null && a.RequestedByPersonAlias.PersonId == RequestContext.CurrentPerson.Id )
                        .Where( a => a.EnteredDateTime >= limitDate );

                    // Filter out expired requests if block settings say so.
                    if ( !ShowExpired )
                    {
                        prayerRequestQuery = prayerRequestQuery
                            .Where( a => !a.ExpirationDate.HasValue || a.ExpirationDate.Value > RockDateTime.Now );
                    }

                    // Filter by group if it has been specified.
                    if ( GroupGuid.HasValue )
                    {
                        prayerRequestQuery = prayerRequestQuery
                            .Where( a => a.Group.Guid == GroupGuid.Value );
                    }

                    // If we are not filtering by group, then exclude any group requests
                    // unless the block setting including them is enabled.
                    if ( !GroupGuid.HasValue && !IncludeGroupRequests )
                    {
                        prayerRequestQuery = prayerRequestQuery
                            .Where( a => !a.GroupId.HasValue );
                    }

                    // Limit results to the maximum number requested.
                    if ( MaxResults.HasValue )
                    {
                        prayerRequestQuery = prayerRequestQuery.Take( MaxResults.Value );
                    }

                    // Get prayer requests, ordered by the date they were
                    // entered in descending order.
                    prayerRequests = prayerRequestQuery
                        .OrderByDescending( a => a.EnteredDateTime )
                        .ToList();
                }

                // Generate our lava merge fields that will be passed
                // to the template for merging.
                var mergeFields = RequestContext.GetCommonMergeFields();
                mergeFields.Add( "PrayerRequestItems", prayerRequests );
                mergeFields.Add( "EditPage", EditPageGuid );
                mergeFields.Add( "AnswerPage", AnswerPageGuid );

                return Template.ResolveMergeFields( mergeFields );
            }
        }

        /// <summary>
        /// Deletes the request and returns a new set of requests.
        /// </summary>
        /// <returns>
        /// The response to send back to the client.
        /// </returns>
        private CallbackResponse DeleteRequest( Guid requestGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var prayerRequestService = new PrayerRequestService( rockContext );
                var prayerRequest = prayerRequestService.Get( requestGuid );

                if ( prayerRequest == null )
                {
                    return new CallbackResponse
                    {
                        Error = "We couldn't find that prayer request."
                    };
                }

                var canDelete = prayerRequest.RequestedByPersonAlias != null && prayerRequest.RequestedByPersonAlias.PersonId == RequestContext.CurrentPerson?.Id;

                if ( !canDelete )
                {
                    return new CallbackResponse
                    {
                        Error = "You do not have permission to delete this prayer request."
                    };
                }

                prayerRequestService.Delete( prayerRequest );

                // Save all changes to database.
                rockContext.SaveChanges();
            }

            return new CallbackResponse
            {
                Content = BuildContent()
            };
        }

        #endregion

        #region Action Methods

        /// <summary>
        /// Gets the current configuration for this block.
        /// </summary>
        /// <returns>The initial content to display.</returns>
        [BlockAction]
        public object GetInitialContent()
        {
            return new CallbackResponse
            {
                Content = BuildContent()
            };
        }

        /// <summary>
        /// Deletes the prayer request.
        /// </summary>
        /// <param name="requestGuid">The prayer request unique identifier.</param>
        /// <returns>An object that contains the new content to be displayed.</returns>
        [BlockAction]
        public object Delete( Guid requestGuid )
        {
            return DeleteRequest( requestGuid );
        }

        #endregion
    }
}
