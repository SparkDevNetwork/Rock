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

using Rock.Attribute;
using Rock.Model;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Prayer.PrayerCardView;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Prayer
{
    /// <summary>
    /// Provides an additional experience to pray using a card based view.
    /// </summary>
    [DisplayName( "Prayer Card View" )]
    [Category( "Prayer" )]
    [Description( "Provides an additional experience to pray using a card based view." )]
    [IconCssClass( "ti ti-grid-dots" )]
    [SupportedSiteTypes( SiteType.Web )]

    #region Block Attributes

    [CodeEditorField( "Display Lava Template",
        Description = "The Lava template that lays out the view of the prayer requests. Pray and Flag buttons must carry data-action=\"pray\" or data-action=\"flag\" and data-key=\"{{ item.IdKey }}\"; script tags in the template are not executed.",
        Key = AttributeKey.DisplayLavaTemplate,
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 400,
        IsRequired = false,
        DefaultValue = LavaTemplateDefaultValue,
        Order = 0 )]

    [TextField( "Prayed Button Text",
        Description = "The text to display inside the Prayed button.",
        Key = AttributeKey.PrayedButtonText,
        DefaultValue = "I Prayed",
        IsRequired = true,
        Order = 1 )]

    [CategoryField( "Category",
        Description = "A top level category. This controls which categories are shown when starting a prayer session.",
        Key = AttributeKey.Category,
        EntityTypeName = "Rock.Model.PrayerRequest",
        AllowMultiple = false,
        IsRequired = false,
        Category = AttributeCategory.Filtering,
        Order = 2 )]

    [BooleanField( "Public Only",
        Description = "If selected, all non-public prayer request will be excluded.",
        Key = AttributeKey.PublicOnly,
        DefaultBooleanValue = true,
        IsRequired = true,
        Category = AttributeCategory.Filtering,
        Order = 3 )]

    [BooleanField( "Enable Prayer Team Flagging",
        Description = "If enabled, members of the prayer team can flag a prayer request if they feel the request is inappropriate and needs review by an administrator.",
        Key = AttributeKey.EnablePrayerTeamFlagging,
        DefaultBooleanValue = false,
        Category = AttributeCategory.Flagging,
        Order = 4 )]

    [IntegerField( "Flag Limit",
        Description = "The number of flags a prayer request has to get from the prayer team before it is automatically unapproved.",
        Key = AttributeKey.FlagLimit,
        DefaultIntegerValue = 1,
        IsRequired = false,
        Category = AttributeCategory.Flagging,
        Order = 5 )]

    [EnumField( "Order",
        Description = "The order that the requests should be displayed.",
        Key = AttributeKey.Order,
        EnumSourceType = typeof( PrayerRequestOrder ),
        DefaultEnumValue = ( int ) PrayerRequestOrder.LeastPrayedFor,
        IsRequired = true,
        Category = AttributeCategory.Filtering,
        Order = 6 )]

    [BooleanField( "Show Campus Filter",
        Description = "Shows or hides the campus filter.",
        Key = AttributeKey.ShowCampusFilter,
        DefaultBooleanValue = false,
        Category = AttributeCategory.Filtering,
        Order = 7 )]

    [DefinedValueField( "Campus Types",
        Description = "Allows selecting which campus types to filter campuses by.",
        Key = AttributeKey.CampusTypes,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_TYPE,
        AllowMultiple = true,
        IsRequired = false,
        Category = AttributeCategory.Filtering,
        Order = 8 )]

    [DefinedValueField( "Campus Statuses",
        Description = "This allows selecting which campus statuses to filter campuses by.",
        Key = AttributeKey.CampusStatuses,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_STATUS,
        AllowMultiple = true,
        IsRequired = false,
        Category = AttributeCategory.Filtering,
        Order = 9 )]

    [IntegerField( "Max Results",
        Description = "The maximum number of requests to display. Leave blank for all.",
        Key = AttributeKey.MaxResults,
        IsRequired = false,
        Category = AttributeCategory.Filtering,
        Order = 10 )]

    [WorkflowTypeField( "Prayed Workflow",
        Description = "The workflow type to launch when someone presses the Pray button. Prayer Request will be passed to the workflow as a generic \"Entity\" field type. Additionally if the workflow type has any of the following attribute keys defined, those attribute values will also be set: PrayerOfferedByPersonAliasGuid, PrayerOfferedByPerson.",
        Key = AttributeKey.PrayedWorkflow,
        AllowMultiple = false,
        IsRequired = false,
        Order = 11 )]

    [WorkflowTypeField( "Flagged Workflow",
        Description = "The workflow type to launch when someone presses the Flag button. Prayer Request will be passed to the workflow as a generic \"Entity\" field type. Additionally if the workflow type has any of the following attribute keys defined, those attribute values will also be set: FlaggedByPersonId.",
        Key = AttributeKey.FlaggedWorkflow,
        AllowMultiple = false,
        IsRequired = false,
        Category = AttributeCategory.Flagging,
        Order = 12 )]

    [BooleanField( "Load Last Prayed Collection",
        Description = "Loads an optional collection of last prayed times for the requests. This is available as a separate merge field in Lava.",
        Key = AttributeKey.LoadLastPrayedCollection,
        DefaultBooleanValue = false,
        Order = 13 )]

    [BooleanField( "Create Interactions for Prayers",
        Description = "If enabled then this block will record an Interaction whenever somebody prays for a prayer request.",
        Key = AttributeKey.CreateInteractionsForPrayers,
        DefaultBooleanValue = true,
        IsRequired = true,
        Order = 14 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "85FE88A7-0E8E-41E2-805E-1FA9E9BC2D73" )]
    // Development GUID so this block can be tested next to the WebForms block.
    // Swap to the original 1FEE129E-E46A-4805-AF5A-6F98E1DA7A16 at chop time.
    [Rock.SystemGuid.BlockTypeGuid( "2B0B4AED-0D65-42B0-B1E6-20E904966986" )]
    public class PrayerCardView : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DisplayLavaTemplate = "DisplayLavaTemplate";
            public const string PrayedButtonText = "PrayedButtonText";
            public const string Category = "Category";
            public const string PublicOnly = "PublicOnly";
            public const string EnablePrayerTeamFlagging = "EnablePrayerTeamFlagging";
            public const string FlagLimit = "FlagLimit";
            public const string Order = "Order";
            public const string ShowCampusFilter = "ShowCampusFilter";
            public const string CampusTypes = "CampusTypes";
            public const string CampusStatuses = "CampusStatuses";
            public const string MaxResults = "MaxResults";
            public const string PrayedWorkflow = "PrayedWorkflow";
            public const string FlaggedWorkflow = "FlaggedWorkflow";
            public const string LoadLastPrayedCollection = "LoadLastPrayedCollection";
            public const string CreateInteractionsForPrayers = "CreateInteractionsForPrayers";
        }

        private static class AttributeCategory
        {
            public const string Filtering = "Filtering";
            public const string Flagging = "Flagging";
        }

        private static class PageParameterKey
        {
            public const string CampusId = "CampusId";
            public const string CategoryId = "CategoryId";
            public const string GroupGuid = "GroupGuid";
        }

        private static class PersonPreferenceKey
        {
            /// <summary>
            /// Same key and Id-based value the WebForms block used so saved selections survive the upgrade.
            /// </summary>
            public const string Campus = "selected-campus";
        }

        #endregion Keys

        #region Attribute Default Values

        /// <summary>
        /// The default value for the Display Lava Template block attribute.
        /// </summary>
        private const string LavaTemplateDefaultValue = @"<div class=""row d-flex flex-wrap"">
    {% for item in PrayerRequestItems %}
        <div class=""col-md-4 col-sm-6 col-xs-12 mb-4"">
            <div class=""card h-100"">
                <div class=""card-body"">
                    <h3 class=""card-title mt-0"">{{ item.FirstName }} {{ item.LastName }}</h3>
                    {% if item.Category != null %}
                    <p class=""card-subtitle mb-2""><span class=""label label-primary"">{{ item.Category.Name }}</span></p>
                    {% endif %}
                    <p class=""card-text"">
                    {{ item.Text }}
                    </p>
                </div>

                <div class=""card-footer bg-white border-0"">
                    {% if EnablePrayerTeamFlagging %}
                    <a href=""#"" class=""btn btn-link btn-sm pl-0 text-muted"" data-action=""flag"" data-key=""{{ item.IdKey }}""><i class=""ti ti-flag""></i> <span>Flag</span></a>
                    {% endif %}
                    <a href=""#"" class=""btn btn-primary btn-sm pull-right"" data-action=""pray"" data-key=""{{ item.IdKey }}"">Pray</a>
                </div>
            </div>
        </div>
    {% endfor -%}
</div>";

        #endregion Attribute Default Values

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<PrayerCardViewBag, PrayerCardViewOptionsBag>
            {
                Options = GetOptionsBag(),
                Bag = new PrayerCardViewBag
                {
                    Content = string.Empty,
                    SelectedCampus = null,
                    HasPrayerRequests = false
                }
            };

            return box;
        }

        /// <summary>
        /// Builds the static configuration options from the block settings.
        /// </summary>
        /// <returns>The populated options bag.</returns>
        private PrayerCardViewOptionsBag GetOptionsBag()
        {
            return new PrayerCardViewOptionsBag
            {
                IsCampusFilterVisible = GetAttributeValue( AttributeKey.ShowCampusFilter ).AsBoolean(),
                CampusTypeFilterGuids = GetAttributeValue( AttributeKey.CampusTypes ).SplitDelimitedValues().AsGuidList(),
                CampusStatusFilterGuids = GetAttributeValue( AttributeKey.CampusStatuses ).SplitDelimitedValues().AsGuidList(),
                PrayedButtonText = GetAttributeValue( AttributeKey.PrayedButtonText ),
                IsPrayerTeamFlaggingEnabled = GetAttributeValue( AttributeKey.EnablePrayerTeamFlagging ).AsBoolean()
            };
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Records that the current person prayed for the specified request.
        /// </summary>
        /// <param name="idKey">The identifier of the prayer request that was prayed for.</param>
        /// <returns>An empty 200-OK response.</returns>
        [BlockAction]
        public BlockActionResult PrayRequest( string idKey )
        {
            return ActionOk();
        }

        /// <summary>
        /// Flags the specified request as inappropriate for administrator review.
        /// </summary>
        /// <param name="idKey">The identifier of the prayer request to flag.</param>
        /// <returns>An empty 200-OK response.</returns>
        [BlockAction]
        public BlockActionResult FlagPrayerRequest( string idKey )
        {
            return ActionOk();
        }

        /// <summary>
        /// Saves the person's campus selection and returns the re-rendered cards.
        /// </summary>
        /// <param name="campusGuid">The unique identifier of the selected campus, or empty for all campuses.</param>
        /// <returns>The refreshed block data.</returns>
        [BlockAction]
        public BlockActionResult SelectCampus( string campusGuid )
        {
            return ActionOk( new PrayerCardViewBag
            {
                Content = string.Empty,
                SelectedCampus = null,
                HasPrayerRequests = false
            } );
        }

        #endregion Block Actions
    }
}
