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
using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.ViewModel.Client;

namespace Rock.Blocks.Types.Mobile.Prayer
{
    /// <summary>
    /// Provides an additional experience to pray using a card based view.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockMobileBlockType" />

    [DisplayName( "Prayer Card View" )]
    [Category( "Mobile > Prayer" )]
    [Description( "Provides an additional experience to pray using a card based view." )]
    [IconCssClass( "fa fa-th" )]

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

    [CategoryField(
        "Category",
        Description = "A top level category. This controls which categories are shown when starting a prayer session.",
        EntityType = typeof( Rock.Model.PrayerRequest ),
        IsRequired = true,
        Key = AttributeKey.Category,
        Order = 3 )]

    [BooleanField(
        "Public Only",
        Description = "If selected, all non-public prayer requests will be excluded.",
        IsRequired = false,
        DefaultBooleanValue = true,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Toggle,
        Key = AttributeKey.PublicOnly,
        Order = 4 )]

    [EnumField(
        "Order",
        Description = "The order that requests should be displayed.",
        IsRequired = true,
        EnumSourceType = typeof( PrayerRequestOrder ),
        DefaultEnumValue = ( int ) PrayerRequestOrder.LeastPrayedFor,
        Key = AttributeKey.PrayerOrder,
        Order = 5 )]

    [DefinedValueField(
        "Campus Types",
        Key = AttributeKey.CampusTypes,
        Description = "Allows selecting which campus types to filter campuses by.",
        IsRequired = false,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_TYPE,
        AllowMultiple = true,
        Order = 6 )]

    [DefinedValueField(
        "Campus Statuses",
        Key = AttributeKey.CampusStatuses,
        Description = "This allows selecting which campus statuses to filter campuses by.",
        IsRequired = false,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_STATUS,
        AllowMultiple = true,
        Order = 7 )]

    [IntegerField(
        "Max Requests",
        Description = "The maximum number of requests to display. Leave blank for all.",
        IsRequired = false,
        Key = AttributeKey.MaxRequests,
        Order = 8 )]

    [BooleanField(
        "Load Last Prayed Collection",
        Description = "Loads an optional collection of last prayed times for the requests. This is available as a separate merge field in Lava.",
        IsRequired = false,
        DefaultBooleanValue = false,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Toggle,
        Key = AttributeKey.LoadLastPrayedCollection,
        Order = 9 )]

    [WorkflowTypeField(
        "Prayed Workflow",
        AllowMultiple = false,
        Key = AttributeKey.PrayedWorkflow,
        Description = "The workflow type to launch when someone presses the Pray button. Prayer Request will be passed to the workflow as a generic \"Entity\" field type. Additionally if the workflow type has any of the following attribute keys defined, those attribute values will also be set: PrayerOfferedByPersonId.",
        IsRequired = false,
        Order = 10 )]

    #endregion

    public class PrayerCardView : RockMobileBlockType
    {
        /// <summary>
        /// The page parameter keys for the <see cref="PrayerCardView"/> block.
        /// </summary>
        private static class PageParameterKey
        {
            /// <summary>
            /// The campus unique identifier key.
            /// </summary>
            public const string CampusGuid = "CampusGuid";
        }

        #region Block Attributes

        /// <summary>
        /// The block setting attribute keys for the <see cref="PrayerCardView"/> block.
        /// </summary>
        private static class AttributeKey
        {
            public const string Template = "Template";

            public const string TitleContent = "TitleContent";

            public const string HideCampusWhenKnown = "HideCampusWhenKnown";

            public const string Category = "Category";

            public const string PublicOnly = "PublicOnly";

            public const string PrayerOrder = "PrayerOrder";

            public const string CampusTypes = "CampusTypes";

            public const string CampusStatuses = "CampusStatuses";

            public const string MaxRequests = "MaxRequests";

            public const string LoadLastPrayedCollection = "LoadLastPrayedCollection";

            public const string PrayedWorkflow = "PrayedWorkflow";
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

        #endregion

        #region IRockMobileBlockType Implementation

        /// <summary>
        /// Gets the required mobile application binary interface version required to render this block.
        /// </summary>
        /// <value>
        /// The required mobile application binary interface version required to render this block.
        /// </value>
        public override int RequiredMobileAbiVersion => 3;

        /// <summary>
        /// Gets the class name of the mobile block to use during rendering on the device.
        /// </summary>
        /// <value>
        /// The class name of the mobile block to use during rendering on the device
        /// </value>
        public override string MobileBlockType => "Rock.Mobile.Blocks.Prayer.PrayerCardView";

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
        private List<ViewModel.NonEntities.ListItemViewModel> GetValidCampuses( RockContext rockContext )
        {
            var helper = new ClientHelper( rockContext, RequestContext.CurrentPerson );

            // We are not running as the target user so don't try to enforce
            // security based on the currently logged in person when retrieving
            // the list of campuses.
            helper.EnableSecurity = false;

            return helper.GetCampusesAsListItems( new ViewModel.Client.CampusOptions
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

            // Get the prayer requests filtered to our block settings.
            IEnumerable<PrayerRequest> prayerRequests = prayerRequestService.GetPrayerRequests( new PrayerRequestQueryOptions
            {
                IncludeNonPublic = !PublicOnly,
                Campuses = campusGuid.HasValue ? new List<Guid> { campusGuid.Value } : null,
                Categories = new List<Guid> { CategoryGuid ?? Guid.Empty }
            } );

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
