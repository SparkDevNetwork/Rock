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
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.Store;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Crm.NcoaResults;
using Rock.ViewModels.Blocks.Store.PackageDetail;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

using static Rock.Blocks.Security.Oidc.AuthClientList;

namespace Rock.Blocks.Crm
{
    /// <summary>
    /// Displays a list of people.
    /// </summary>

    [DisplayName( "NcoaResults" )]
    [Category( "CRM" )]
    [Description( "Displays a list of ncoa results." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [IntegerField( "Result Count",
         Description = "How many results to show per page.",
         DefaultIntegerValue = 20,
         Key = AttributeKey.ResultCount )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "01a7925e-2532-4a9a-9dc6-8bef835761de" )]
    [Rock.SystemGuid.BlockTypeGuid( "69c53367-0d4a-49f1-b64b-863f08c2fc0b" )]
    [CustomizedGrid]
    public class NcoaResults : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string ResultCount = "ResultCount";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class PreferenceKey
        {
            public const string FilterProcessed = "filter-processed";
            public const string FilterMoveDate = "filter-move-date";
            public const string FilterNcoaProcessedDate = "filter-ncoa-processed-date";
            public const string FilterMoveType = "filter-move-type";
            public const string FilterAddressStatus = "filter-address-status";
            public const string FilterInvalidReason = "filter-invalid-reason";
            public const string FilterMoveDistance = "filter-move-distance";
            public const string FilterLastName = "filter-last-name";
            public const string FilterCampus = "filter-campus";
        }

        #endregion Keys

        #region Fields

        /// <summary>
        /// The Short Link attributes that are configured to show on the grid.
        /// </summary>
        private readonly Lazy<List<AttributeCache>> _gridAttributes = new Lazy<List<AttributeCache>>( BuildGridAttributes );

        private PersonPreferenceCollection _personPreferences;

        #endregion

        #region Properties

        public PersonPreferenceCollection PersonPreferences
        {
            get
            {
                if ( _personPreferences == null )
                {
                    _personPreferences = this.GetBlockPersonPreferences();
                }

                return _personPreferences;
            }
        }

        protected ListItemBag FilterProcessed => PersonPreferences
            .GetValue( PreferenceKey.FilterProcessed )
            .FromJsonOrNull<ListItemBag>();

        protected ListItemBag FilterMoveDate => PersonPreferences
            .GetValue( PreferenceKey.FilterMoveDate )
            .FromJsonOrNull<ListItemBag>();

        private ListItemBag FilterNcoaProcessedDateRange => PersonPreferences
            .GetValue( PreferenceKey.FilterNcoaProcessedDate )
            .FromJsonOrNull<ListItemBag>();

        protected ListItemBag FilterMoveType => PersonPreferences
            .GetValue( PreferenceKey.FilterMoveType )
            .FromJsonOrNull<ListItemBag>();

        protected ListItemBag FilterAddressStatus => PersonPreferences
            .GetValue( PreferenceKey.FilterAddressStatus )
            .FromJsonOrNull<ListItemBag>();

        protected ListItemBag FilterInvalidReason => PersonPreferences
            .GetValue( PreferenceKey.FilterInvalidReason )
            .FromJsonOrNull<ListItemBag>();

        protected ListItemBag FilterMoveDistance => PersonPreferences
            .GetValue( PreferenceKey.FilterMoveDistance )
            .FromJsonOrNull<ListItemBag>();

        protected ListItemBag FilterLastName => PersonPreferences
            .GetValue( PreferenceKey.FilterLastName )
            .FromJsonOrNull<ListItemBag>();

        protected ListItemBag FilterCampus => PersonPreferences
            .GetValue( PreferenceKey.FilterCampus )
            .FromJsonOrNull<ListItemBag>();



        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<NcoaResultsBag, NcoaResultsOptionsBag>();

            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private NcoaResultsOptionsBag GetBoxOptions()
        {
            var options = new NcoaResultsOptionsBag();
            options.ResultCount = GetAttributeValue( AttributeKey.ResultCount ).AsIntegerOrNull() ?? 20;

            return options;
        }


        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "NcoaRowId", "((Key))" )
            };
        }


        /// <summary>
        /// Builds the list of grid attributes that should be included on the Grid.
        /// </summary>
        /// <remarks>
        /// The default implementation returns only attributes that are not qualified.
        /// </remarks>
        /// <returns>A list of <see cref="AttributeCache"/> objects.</returns>
        private static List<AttributeCache> BuildGridAttributes()
        {
            var entityTypeId = EntityTypeCache.Get<NcoaRowBag>( false )?.Id;

            if ( entityTypeId.HasValue )
            {
                return AttributeCache.GetOrderedGridAttributes( entityTypeId, string.Empty, string.Empty );
            }

            return new List<AttributeCache>();
        }


        #endregion

        #region Block Actions

        //[BlockAction]
        //public BlockActionResult GetNcoaResults()
        #endregion
    }
}
