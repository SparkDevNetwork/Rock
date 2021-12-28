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
using System.Linq;

using Rock.Attribute;
using Rock.Common.Mobile.Blocks.Content;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Blocks.Types.Mobile.Events
{
    /// <summary>
    /// Allows the user to read through and pray for prayer requests.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockMobileBlockType" />

    [DisplayName( "Prayer Session" )]
    [Category( "Mobile > Prayer" )]
    [Description( "Allows the user to read through and pray for prayer requests." )]
    [IconCssClass( "fa fa-pray" )]

    #region Block Attributes

    [TextField( "Prayed Button Text",
        Description = "The text to display inside the Prayed button. Available in the XAML template as lava variable 'PrayedButtonText'.",
        DefaultValue = "I've Prayed",
        IsRequired = true,
        Key = AttributeKeys.PrayedButtonText,
        Order = 0 )]

    [BooleanField( "Show Follow Button",
        Description = "Indicates if the Follow button should be shown. Available in the XAML template as lava variable 'ShowFollowButton'.",
        DefaultBooleanValue = true,
        IsRequired = true,
        Key = AttributeKeys.ShowFollowButton,
        Order = 1 )]

    [BooleanField( "Show Inappropriate Button",
        Description = "Indicates if the button to flag a request as inappropriate should be shown. Available in the XAML template as lava variable 'ShowInappropriateButton'.",
        DefaultBooleanValue = true,
        IsRequired = true,
        Key = AttributeKeys.ShowInappropriateButton,
        Order = 2 )]

    [BooleanField( "Public Only",
        Description = "If enabled then only prayer requests marked as public will be shown.",
        DefaultBooleanValue = false,
        IsRequired = true,
        Key = AttributeKeys.PublicOnly,
        Order = 3 )]

    [IntegerField( "Inappropriate Flag Limit",
        Description = "The number of flags a prayer request has to get from the prayer team before it is automatically unapproved.",
        IsRequired = false,
        Key = AttributeKeys.InappropriateFlagLimit,
        Order = 4 )]

    [BlockTemplateField( "Template",
        Description = "The template to use when rendering prayer requests.",
        TemplateBlockValueGuid = SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_PRAYER_SESSION,
        IsRequired = true,
        DefaultValue = "",
        Key = AttributeKeys.Template,
        Order = 5 )]

    [BooleanField( "Create Interactions for Prayers",
        Description = "If enabled then this block will record an Interaction whenever somebody prays for a prayer request.",
        DefaultBooleanValue = true,
        IsRequired = true,
        Key = AttributeKeys.CreateInteractionsForPrayers,
        Order = 6 )]

    [BooleanField( "Include Group Requests",
        Description = "Includes prayer requests that are attached to a group.",
        IsRequired = false,
        DefaultBooleanValue = false,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Key = AttributeKeys.IncludeGroupRequests,
        Order = 7 )]

    #endregion

    public class PrayerSession : RockMobileBlockType
    {
        #region Block Attributes

        /// <summary>
        /// The block setting attribute keys for the <see cref="PrayerSession"/> block.
        /// </summary>
        public static class AttributeKeys
        {
            /// <summary>
            /// The prayed button text key.
            /// </summary>
            public const string PrayedButtonText = "PrayedButtonText";

            /// <summary>
            /// The show follow button key.
            /// </summary>
            public const string ShowFollowButton = "ShowFollowButton";

            /// <summary>
            /// The show inappropriate button key.
            /// </summary>
            public const string ShowInappropriateButton = "ShowInappropriateButton";

            /// <summary>
            /// The public only key.
            /// </summary>
            public const string PublicOnly = "PublicOnly";

            /// <summary>
            /// The inappropriate flag limit key.
            /// </summary>
            public const string InappropriateFlagLimit = "InappropriateFlagLimit";

            /// <summary>
            /// The template key.
            /// </summary>
            public const string Template = "Template";

            /// <summary>
            /// The create interactions for prayers key.
            /// </summary>
            public const string CreateInteractionsForPrayers = "CreateInteractionsForPrayers";

            /// <summary>
            /// The include group requests key.
            /// </summary>
            public const string IncludeGroupRequests = "IncludeGroupRequests";
        }

        /// <summary>
        /// Gets the prayed button text.
        /// </summary>
        /// <value>
        /// The prayed button text.
        /// </value>
        protected string PrayedButtonText => GetAttributeValue( AttributeKeys.PrayedButtonText );

        /// <summary>
        /// Gets the show follow button visible state.
        /// </summary>
        /// <value>
        /// The show follow button visible state.
        /// </value>
        protected bool ShowFollowButton => GetAttributeValue( AttributeKeys.ShowFollowButton ).AsBoolean();

        /// <summary>
        /// Gets the show inappropriate button visible state.
        /// </summary>
        /// <value>
        /// The show inappropriate button visible state.
        /// </value>
        protected bool ShowInappropriateButton => GetAttributeValue( AttributeKeys.ShowInappropriateButton ).AsBoolean();

        /// <summary>
        /// Gets a value that determines if only public prayer requests should be shown.
        /// </summary>
        /// <value>
        /// A value that determines if only public prayer requests should be shown.
        /// </value>
        protected bool PublicOnly => GetAttributeValue( AttributeKeys.PublicOnly ).AsBoolean();

        /// <summary>
        /// Gets the inappropriate flag limit.
        /// </summary>
        /// <value>
        /// The inappropriate flag limit.
        /// </value>
        protected int? InappropriateFlagLimit => GetAttributeValue( AttributeKeys.InappropriateFlagLimit ).AsIntegerOrNull();

        /// <summary>
        /// Gets the template.
        /// </summary>
        /// <value>
        /// The template.
        /// </value>
        protected string Template => Rock.Field.Types.BlockTemplateFieldType.GetTemplateContent( GetAttributeValue( AttributeKeys.Template ) );

        /// <summary>
        /// Gets a value indicating whether interactions are created for prayers.
        /// </summary>
        /// <value>
        ///   <c>true</c> if interactions are created for prayers; otherwise, <c>false</c>.
        /// </value>
        protected bool CreateInteractionsForPrayers => GetAttributeValue( AttributeKeys.CreateInteractionsForPrayers ).AsBoolean();

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

        /// <summary>
        /// The expeced page parameter keys for the <see cref="PrayerSession"/> block.
        /// </summary>
        public static class PageParameterKeys
        {
            /// <summary>
            /// The category key. Value should be blank or a GUID value.
            /// </summary>
            public const string PrayerCategory = "Category";

            /// <summary>
            /// My campus key, value should be blank or a boolean value.
            /// </summary>
            public const string MyCampus = "MyCampus";

            /// <summary>
            /// The unique identifier of the group to use when filtering prayer
            /// requests.
            /// </summary>
            public const string GroupGuid = "GroupGuid";
        }

        /// <summary>
        /// Gets the prayer category.
        /// </summary>
        /// <value>
        /// The prayer category.
        /// </value>
        protected Guid PrayerCategory => RequestContext.GetPageParameter( PageParameterKeys.PrayerCategory ).AsGuid();

        /// <summary>
        /// Gets a value indicating whether the prayer session should be limited to the user's campus.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the prayer session should be limited to the user's campus; otherwise, <c>false</c>.
        /// </value>
        protected bool MyCampus => RequestContext.GetPageParameter( PageParameterKeys.MyCampus ).AsBooleanOrNull() ?? false;

        /// <summary>
        /// The unique identifier of the group to use when filtering prayer
        /// requests.
        /// </summary>
        protected Guid? GroupGuid => RequestContext.GetPageParameter( PageParameterKeys.GroupGuid ).AsGuidOrNull();

        #endregion

        #region IRockMobileBlockType Implementation

        /// <summary>
        /// Gets the required mobile application binary interface version required to render this block.
        /// </summary>
        /// <value>
        /// The required mobile application binary interface version required to render this block.
        /// </value>
        public override int RequiredMobileAbiVersion => 1;

        /// <summary>
        /// Gets the class name of the mobile block to use during rendering on the device.
        /// </summary>
        /// <value>
        /// The class name of the mobile block to use during rendering on the device
        /// </value>
        public override string MobileBlockType => "Rock.Mobile.Blocks.Prayer.PrayerSession";

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
                DynamicContent = true
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Builds the content to be shown.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <returns>
        /// A string containing the XAML content.
        /// </returns>
        protected virtual string BuildContent( string context = null )
        {
            var template = Template;
            var mergeFields = RequestContext.GetCommonMergeFields();
            SessionContext sessionContext;
            PrayerRequest request;
            var rockContext = new RockContext();

            if ( context.IsNotNullOrWhiteSpace() )
            {
                var prayerRequestService = new PrayerRequestService( rockContext );

                sessionContext = Encryption.DecryptString( context ).FromJsonOrNull<SessionContext>();

                //
                // Update the prayer count on the last prayer request.
                //
                var lastRequest = prayerRequestService.Get( sessionContext.RequestIds[sessionContext.Index] );
                lastRequest.PrayerCount = ( lastRequest.PrayerCount ?? 0 ) + 1;
                rockContext.SaveChanges();

                if ( CreateInteractionsForPrayers )
                {
                    PrayerRequestService.EnqueuePrayerInteraction( lastRequest, RequestContext.CurrentPerson, PageCache.Layout.Site.Name, RequestContext.ClientInformation?.Browser?.String, RequestContext.ClientInformation.IpAddress, null );
                }

                //
                // Move to the next prayer request, or else indicate that we have
                // finished this session.
                //
                sessionContext.Index += 1;
                if ( sessionContext.RequestIds.Count > sessionContext.Index )
                {
                    var requestId = sessionContext.RequestIds[sessionContext.Index];
                    request = prayerRequestService.Get( requestId );
                }
                else
                {
                    request = null;
                }
            }
            else
            {
                var query = GetPrayerRequests( rockContext );

                sessionContext = new SessionContext
                {
                    RequestIds = query.Select( a => a.Id ).ToList()
                };

                request = query.FirstOrDefault();
            }

            mergeFields.Add( "PrayedButtonText", PrayedButtonText );
            mergeFields.Add( "ShowFollowButton", ShowFollowButton );
            mergeFields.Add( "ShowInappropriateButton", ShowInappropriateButton );
            mergeFields.Add( "SessionContext", Encryption.EncryptString( sessionContext.ToJson() ) );
            mergeFields.Add( "Request", request );

            return template.ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Gets the prayer requests for the new session.
        /// </summary>
        /// <returns>An enumerable of <see cref="PrayerRequest"/> objects.</returns>
        protected virtual IEnumerable<PrayerRequest> GetPrayerRequests( RockContext rockContext )
        {
            var prayerRequestService = new PrayerRequestService( rockContext );
            var category = CategoryCache.Get( PrayerCategory );

            if ( category == null && !GroupGuid.HasValue )
            {
                return null;
            }

            var query = prayerRequestService.GetByCategoryIds( new List<int> { category.Id } );

            if ( PublicOnly )
            {
                query = query.Where( a => a.IsPublic.HasValue && a.IsPublic.Value );
            }

            if ( MyCampus )
            {
                var campusId = RequestContext.CurrentPerson?.PrimaryCampusId;

                if ( campusId.HasValue )
                {
                    query = query.Where( a => a.CampusId.HasValue && a.CampusId == campusId );
                }
            }

            // Filter by group if it has been specified.
            if ( GroupGuid.HasValue )
            {
                query = query.Where( a => a.Group != null && a.Group.Guid == GroupGuid.Value );
            }

            // If we are not filtering by group, then exclude any group requests
            // unless the block setting including them is enabled.
            if ( !GroupGuid.HasValue && !IncludeGroupRequests )
            {
                query = query.Where( a => !a.GroupId.HasValue );
            }

            query = query.OrderByDescending( a => a.IsUrgent )
                .ThenBy( a => a.PrayerCount );

            return query;
        }

        #endregion

        #region Action Methods

        /// <summary>
        /// Gets the initial content for this block.
        /// </summary>
        /// <returns>The content to be displayed.</returns>
        [BlockAction]
        public object GetInitialContent()
        {
            return new CallbackResponse
            {
                Content = BuildContent()
            };
        }

        /// <summary>
        /// Gets the next prayer request after the specified one.
        /// </summary>
        /// <param name="sessionContext">The session context of the prayer session.</param>
        /// <returns>
        /// The content to be displayed.
        /// </returns>
        [BlockAction]
        public object NextRequest( string sessionContext )
        {
            return new CallbackResponse
            {
                Content = BuildContent( sessionContext )
            };
        }

        /// <summary>
        /// Flags the currently viewed prayer request.
        /// </summary>
        /// <param name="sessionContext">The session context of the prayer session.</param>
        /// <returns>
        /// The content to be displayed.
        /// </returns>
        [BlockAction]
        public object FlagRequest( string sessionContext )
        {
            using ( var rockContext = new RockContext() )
            {
                var context = Encryption.DecryptString( sessionContext ).FromJsonOrNull<SessionContext>() ?? new SessionContext();
                var request = new PrayerRequestService( rockContext ).Get( context.RequestIds[context.Index] );

                request.FlagCount = ( request.FlagCount ?? 0 ) + 1;
                if ( InappropriateFlagLimit.HasValue && request.FlagCount >= InappropriateFlagLimit )
                {
                    request.IsApproved = false;
                }

                rockContext.SaveChanges();
            }

            return new CallbackResponse
            {
                Content = BuildContent( sessionContext )
            };
        }

        #endregion

        #region Support Classes

        private class SessionContext
        {
            public int Index { get; set; }

            public List<int> RequestIds { get; set; } = new List<int>();
        }

        #endregion
    }
}
