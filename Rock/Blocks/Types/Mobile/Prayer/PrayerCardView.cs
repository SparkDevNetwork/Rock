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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.ClientService.Core.Campus;
using Rock.ClientService.Core.Campus.Options;
using Rock.Data;
using Rock.Model;
using Rock.Utility;

namespace Rock.Blocks.Types.Mobile.Prayer
{
    /// <summary>
    /// Provides an additional experience to pray using a card based view.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Prayer Card View" )]
    [Category( "Mobile > Prayer" )]
    [Description( "Provides an additional experience to pray using a card based view." )]
    [IconCssClass( "fa fa-th" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [BlockTemplateField( "Template",
        Description = "The Lava template that lays out the view of the prayer requests.",
        TemplateBlockValueGuid = SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_PRAYER_PRAYER_CARD_VIEW,
        DefaultValue = "757935E7-AB6D-47B6-A6C4-1CA5920C922E",
        IsRequired = true,
        Key = AttributeKey.Template,
        Order = 0 )]

    [CodeEditorField(
        "Title Content",
        Description = "The XAML content to show below the campus picker and above the prayer requests.",
        IsRequired = false,
        Key = AttributeKey.TitleContent,
        Order = 1 )]

    [BooleanField(
        "Hide Campus When Known",
        Description = "Will hide the campus picker when a campus is known from either the Current Person's campus or passed in CampusGuid page parameter.",
        IsRequired = false,
        DefaultBooleanValue = false,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Toggle,
        Key = AttributeKey.HideCampusWhenKnown,
        Order = 2 )]

    [BooleanField(
        "Always Hide Campus",
        Description = "Hides the campus picker and disables filtering by campus.",
        IsRequired = false,
        DefaultBooleanValue = false,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Toggle,
        Key = AttributeKey.AlwaysHideCampus,
        Order = 3 )]

    [CategoryField(
        "Category",
        Description = "A top level category. This controls which categories are shown when starting a prayer session.",
        EntityType = typeof( Rock.Model.PrayerRequest ),
        IsRequired = true,
        Key = AttributeKey.Category,
        Order = 4 )]

    [BooleanField(
        "Public Only",
        Description = "If selected, all non-public prayer requests will be excluded.",
        IsRequired = false,
        DefaultBooleanValue = true,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Toggle,
        Key = AttributeKey.PublicOnly,
        Order = 5 )]

    [BooleanField(
        "Include Unapproved",
        Description = "If selected, all unapproved prayer requests will be included.",
        IsRequired = false,
        DefaultBooleanValue = false,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Toggle,
        Key = AttributeKey.IncludeUnapproved,
        Order = 6 )]

    [EnumField(
        "Order",
        Description = "The order that requests should be displayed.",
        IsRequired = true,
        EnumSourceType = typeof( PrayerRequestOrder ),
        DefaultEnumValue = ( int ) PrayerRequestOrder.LeastPrayedFor,
        Key = AttributeKey.PrayerOrder,
        Order = 7 )]

    [DefinedValueField(
        "Campus Types",
        Key = AttributeKey.CampusTypes,
        Description = "Allows selecting which campus types to filter campuses by.",
        IsRequired = false,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_TYPE,
        AllowMultiple = true,
        Order = 8 )]

    [DefinedValueField(
        "Campus Statuses",
        Key = AttributeKey.CampusStatuses,
        Description = "This allows selecting which campus statuses to filter campuses by.",
        IsRequired = false,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_STATUS,
        AllowMultiple = true,
        Order = 9 )]

    [IntegerField(
        "Max Requests",
        Description = "The maximum number of requests to display. Leave blank for all.",
        IsRequired = false,
        Key = AttributeKey.MaxRequests,
        Order = 10 )]

    [BooleanField(
        "Load Last Prayed Collection",
        Description = "Loads an optional collection of last prayed times for the requests. This is available as a separate merge field in Lava.",
        IsRequired = false,
        DefaultBooleanValue = false,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Toggle,
        Key = AttributeKey.LoadLastPrayedCollection,
        Order = 11 )]

    [WorkflowTypeField(
        "Prayed Workflow",
        AllowMultiple = false,
        Key = AttributeKey.PrayedWorkflow,
        Description = "The workflow type to launch when someone presses the Pray button. Prayer Request will be passed to the workflow as a generic \"Entity\" field type. Additionally if the workflow type has any of the following attribute keys defined, those attribute values will also be set: PrayerOfferedByPersonId.",
        IsRequired = false,
        Order = 12 )]

    [BooleanField( "Include Group Requests",
        Description = "Includes prayer requests that are attached to a group.",
        IsRequired = false,
        DefaultBooleanValue = false,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Toggle,
        Key = AttributeKey.IncludeGroupRequests,
        Order = 13 )]

    [IntegerField(
        "Prayed For in Last x Minutes Filter",
        Description = "An integer (minutes) that you can use to filter out recently prayed for items. Uses interaction data. 0 to disable.",
        IsRequired = true,
        DefaultIntegerValue = 0,
        Key = AttributeKey.MinutesToFilter,
        Order = 14 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_PRAYER_PRAYER_CARD_VIEW_BLOCK_TYPE )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.MOBILE_PRAYER_PRAYER_CARD_VIEW )]
    public class PrayerCardView : RockBlockType
    {
        #region Page Parameters

        /// <summary>
        /// The page parameter keys for the <see cref="PrayerCardView"/> block.
        /// </summary>
        private static class PageParameterKey
        {
            /// <summary>
            /// The campus unique identifier key.
            /// </summary>
            public const string CampusGuid = "CampusGuid";

            /// <summary>
            /// The unique identifier of the group to use when filtering prayer
            /// requests.
            /// </summary>
            public const string GroupGuid = "GroupGuid";
        }

        /// <summary>
        /// The unique identifier of the group to use when filtering prayer
        /// requests.
        /// </summary>
        protected Guid? GroupGuid => RequestContext.GetPageParameter( PageParameterKey.GroupGuid ).AsGuidOrNull();

        #endregion

        #region Block Attributes

        /// <summary>
        /// The block setting attribute keys for the <see cref="PrayerCardView"/> block.
        /// </summary>
        private static class AttributeKey
        {
            public const string Template = "Template";

            public const string TitleContent = "TitleContent";

            public const string HideCampusWhenKnown = "HideCampusWhenKnown";

            public const string AlwaysHideCampus = "AlwaysHideCampus";

            public const string Category = "Category";

            public const string PublicOnly = "PublicOnly";

            public const string IncludeUnapproved = "IncludeUnapproved";

            public const string PrayerOrder = "PrayerOrder";

            public const string CampusTypes = "CampusTypes";

            public const string CampusStatuses = "CampusStatuses";

            public const string MaxRequests = "MaxRequests";

            public const string LoadLastPrayedCollection = "LoadLastPrayedCollection";

            public const string PrayedWorkflow = "PrayedWorkflow";

            /// <summary>
            /// The include group requests key.
            /// </summary>
            public const string IncludeGroupRequests = "IncludeGroupRequests";

            /// <summary>
            /// The minutes to filter.
            /// </summary>
            public const string MinutesToFilter = "MinutesToFilter";
        }

        /// <summary>
        /// Gets the template.
        /// </summary>
        /// <value>
        /// The template.
        /// </value>
        protected string Template => Rock.Field.Types.BlockTemplateFieldType.GetTemplateContent( GetAttributeValue( AttributeKey.Template ) );

        /// <summary>
        /// Gets the title content to display above the prayer cards.
        /// </summary>
        /// <value>
        /// The title content to display above the prayer cards.
        /// </value>
        protected string TitleContent => GetAttributeValue( AttributeKey.TitleContent );

        /// <summary>
        /// Gets a value indicating whether to hide the campus picker if campus is already known.
        /// </summary>
        /// <value>
        ///   <c>true</c> if campus picker should be hidden if campus is already known; otherwise, <c>false</c>.
        /// </value>
        protected bool HideCampusWhenKnown => GetAttributeValue( AttributeKey.HideCampusWhenKnown ).AsBoolean();

        /// <summary>
        /// Gets a value indicating whether to always hide the campus picker. If
        /// enabled then no campus filtering will be performed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the campus picker should always be hidden; otherwise, <c>false</c>.
        /// </value>
        protected bool AlwaysHideCampus => GetAttributeValue( AttributeKey.AlwaysHideCampus ).AsBoolean();

        /// <summary>
        /// Gets the root prayer category to limit display of prayer requests to.
        /// </summary>
        /// <value>
        /// The root prayer category to limit display of prayer requests to.
        /// </value>
        protected Guid? CategoryGuid => GetAttributeValue( AttributeKey.Category ).AsGuidOrNull();

        /// <summary>
        /// Gets a value indicating whether only public prayer requests are shown.
        /// </summary>
        /// <value>
        ///   <c>true</c> if only public prayer requests are shown; otherwise, <c>false</c>.
        /// </value>
        protected bool PublicOnly => GetAttributeValue( AttributeKey.PublicOnly ).AsBoolean();

        /// <summary>
        /// Whether or not unapproved requests should be included.
        /// </summary>
        protected bool IncludeUnapproved => RequestContext?.GetPageParameter( AttributeKey.IncludeUnapproved )?.AsBooleanOrNull() ?? GetAttributeValue( AttributeKey.IncludeUnapproved ).AsBoolean();

        /// <summary>
        /// Gets the order of the prayer requests.
        /// </summary>
        /// <value>
        /// The order of the prayer requests.
        /// </value>
        protected PrayerRequestOrder PrayerOrder => GetAttributeValue( AttributeKey.PrayerOrder ).ConvertToEnum<PrayerRequestOrder>( PrayerRequestOrder.LeastPrayedFor );

        /// <summary>
        /// Gets the campus types to limit the campus picker to.
        /// </summary>
        /// <value>
        /// The campus types to limit the campus picker to.
        /// </value>
        protected List<Guid> CampusTypeGuids => GetAttributeValue( AttributeKey.CampusTypes ).SplitDelimitedValues().AsGuidList();

        /// <summary>
        /// Gets the campus statuses to limit the campus picker to.
        /// </summary>
        /// <value>
        /// The campus statuses to limit the campus picker to.
        /// </value>
        protected List<Guid> CampusStatusGuids => GetAttributeValue( AttributeKey.CampusStatuses ).SplitDelimitedValues().AsGuidList();

        /// <summary>
        /// Gets the maximum number of prayer requests to include.
        /// </summary>
        /// <value>
        /// The maximum number of prayer requests to include.
        /// </value>
        protected int? MaxRequests => GetAttributeValue( AttributeKey.MaxRequests ).AsIntegerOrNull();

        /// <summary>
        /// Gets a value indicating whether the last prayed collection should be loaded.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the last prayed collection should be loaded; otherwise, <c>false</c>.
        /// </value>
        protected bool LoadLastPrayedCollection => GetAttributeValue( AttributeKey.LoadLastPrayedCollection ).AsBoolean();

        /// <summary>
        /// Gets the unique identifier of the workflow type to launch when a prayer is prayed for.
        /// </summary>
        /// <value>
        /// The unique identifier of the workflow type to launch when a prayer is prayed for.
        /// </value>
        protected Guid? PrayedWorkflowGuid => GetAttributeValue( AttributeKey.PrayedWorkflow ).AsGuidOrNull();

        /// <summary>
        /// Gets a value that specifies if group requests should be included by default.
        /// If <c>false</c> and no group is specified in the page parameters then any
        /// requests that are attached to a group will be excluded.
        /// </summary>
        /// <value>
        /// A value that specifies if group requests should be included by default.
        /// </value>
        protected bool IncludeGroupRequests => GetAttributeValue( AttributeKey.IncludeGroupRequests ).AsBoolean( false );

        #endregion

        #region IRockMobileBlockType Implementation

        /// <inheritdoc/>
        public override Version RequiredMobileVersion => new Version( 1, 3 );

        /// <summary>
        /// Gets the property values that will be sent to the device in the application bundle.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetMobileConfigurationValues()
        {
            using ( var rockContext = new RockContext() )
            {
                //
                // Indicate that we are a dynamic content providing block.
                //
                return new
                {
                    TitleContent,
                    HideCampusWhenKnown,
                    AlwaysHideCampus,
                    Campuses = GetValidCampuses( rockContext )
                };
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the valid campuses that have been configured on the block settings.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>A collection of list items.</returns>
        private List<ViewModels.Utility.ListItemBag> GetValidCampuses( RockContext rockContext )
        {
            var campusClientService = new CampusClientService( rockContext, RequestContext.CurrentPerson );

            // Bypass security because the admin has specified which campuses
            // they want to show up.
            campusClientService.EnableSecurity = false;

            return campusClientService.GetCampusesAsListItems( new CampusOptions
            {
                LimitCampusTypes = CampusTypeGuids,
                LimitCampusStatuses = CampusStatusGuids
            } );
        }

        /// <summary>
        /// Builds the content to be displayed on the block.
        /// </summary>
        /// <returns>A string containing the XAML content to be displayed.</returns>
        private string BuildContent( Guid? campusGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var validCampuses = GetValidCampuses( rockContext );

                // Validate the campus provided by the user.
                if ( campusGuid.HasValue && !validCampuses.Any( c => c.Value.ToLower() == campusGuid.Value.ToString().ToLower() ) )
                {
                    campusGuid = null;
                }

                var prayerRequests = GetPrayerRequests( campusGuid, rockContext );
                var mergeFields = RequestContext.GetCommonMergeFields();

                mergeFields.AddOrReplace( "PrayerRequestItems", prayerRequests );
                mergeFields.AddOrReplace( "PrayedWorkflowType", PrayedWorkflowGuid );

                if ( LoadLastPrayedCollection )
                {
                    mergeFields.Add( "LastPrayed", GetLastPrayedDetails( prayerRequests, rockContext ) );
                }

                return Template.ResolveMergeFields( mergeFields );
            }
        }

        /// <summary>
        /// Gets the prayer requests that match our configuration.
        /// </summary>
        /// <param name="campusGuid">The campus unique identifier.</param>
        /// <param name="rockContext">The rock context to operate in.</param>
        /// <returns>A collection of <see cref="PrayerRequest"/> objects.</returns>
        private List<PrayerRequest> GetPrayerRequests( Guid? campusGuid, RockContext rockContext )
        {
            var prayerRequestService = new PrayerRequestService( rockContext );

            var queryOptions = new PrayerRequestQueryOptions
            {
                IncludeNonPublic = !PublicOnly,
                IncludeEmptyCampus = true,
                IncludeUnapproved = IncludeUnapproved,
                IncludeGroupRequests = IncludeGroupRequests,
                Categories = new List<Guid> { CategoryGuid ?? Guid.Empty },
                MinutesToFilter = GetAttributeValue( AttributeKey.MinutesToFilter ).AsInteger(),
                CurrentPersonId = RequestContext.CurrentPerson?.Id
            };

            // If we have been requested to show only prayer requests attached
            // to a specific group, then add that identifier to the options.
            if ( GroupGuid.HasValue )
            {
                queryOptions.GroupGuids = new List<Guid> { GroupGuid.Value };
            }

            // If we have shown the campus picker and been provided with a campus
            // then add its identifier to the options.
            if ( !AlwaysHideCampus && campusGuid.HasValue )
            {
                queryOptions.Campuses = new List<Guid> { campusGuid.Value };
            }

            // Get the prayer requests filtered to our block settings.
            IEnumerable<PrayerRequest> prayerRequests = prayerRequestService.GetPrayerRequests( queryOptions );

            // Order by how the block has been configured.
            prayerRequests = prayerRequests.OrderBy( PrayerOrder );

            // Limit the maximum number of prayer requests.
            if ( MaxRequests.HasValue )
            {
                prayerRequests = prayerRequests.Take( MaxRequests.Value );
            }

            return prayerRequests.ToList();
        }

        /// <summary>
        /// Gets the last prayed details.
        /// </summary>
        /// <param name="prayerRequests">The prayer requests.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>A collection of last prayed details.</returns>
        private List<PrayerRequestLastPrayedDetail> GetLastPrayedDetails( List<PrayerRequest> prayerRequests, RockContext rockContext )
        {
            var prayerRequestService = new PrayerRequestService( rockContext );
            var prayerRequestIds = prayerRequests.Select( r => r.Id ).ToList();
            var lastPrayedInteractions = prayerRequestService.GetLastPrayedDetails( prayerRequestIds );

            return lastPrayedInteractions.ToList();

        }

        #endregion

        #region Action Methods

        /// <summary>
        /// Gets the content of the cards to be displayed.
        /// </summary>
        /// <param name="campusGuid">The campus unique identifier.</param>
        /// <returns>The client specific content to display.</returns>
        [BlockAction]
        public BlockActionResult GetCardContent( Guid? campusGuid )
        {
            return ActionOk( new
            {
                Content = BuildContent( campusGuid )
            } );
        }

        #endregion
    }
}
