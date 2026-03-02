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

    [DisplayName( "NCOA Results" )]
    [Category( "CRM" )]
    [Description( "Displays a list of ncoa results." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

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

        protected Processed? FilterProcessed
        {
            get
            {
                var processedValue = PersonPreferences
                    .GetValue( PreferenceKey.FilterProcessed );

                if ( processedValue == null )
                {
                    return null;
                }

                if ( int.TryParse( processedValue, out var intValue ) )
                {
                    return ( Processed ) intValue;
                }

                return null;
            }
        }

        protected ListItemBag FilterMoveDate => PersonPreferences
            .GetValue( PreferenceKey.FilterMoveDate )
            .FromJsonOrNull<ListItemBag>();

        protected ListItemBag FilterNcoaProcessedDate => PersonPreferences
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
            var entityTypeId = EntityTypeCache.Get<NcoaDataBag>( false )?.Id;

            if ( entityTypeId.HasValue )
            {
                return AttributeCache.GetOrderedGridAttributes( entityTypeId, string.Empty, string.Empty );
            }

            return new List<AttributeCache>();
        }


        /// <summary>
        /// Formats the address.
        /// </summary>
        /// <param name="street1">The street1.</param>
        /// <param name="street2">The street2.</param>
        /// <param name="city">The city.</param>
        /// <param name="state">The state.</param>
        /// <param name="postalCode">The postal code.</param>
        /// <returns>The formated address</returns>
        private string FormattedAddress( string street1, string street2, string city, string state, string postalCode )
        {
            if ( string.IsNullOrWhiteSpace( street1 ) &&
            string.IsNullOrWhiteSpace( street2 ) &&
            string.IsNullOrWhiteSpace( city ) )
            {
                return string.Empty;
            }

            string result = string.Format( "{0} {1} {2}, {3} {4}",
              street1, street2, city, state, postalCode ).ReplaceWhileExists( "  ", " " );

            // Remove blank lines
            while ( result.Contains( Environment.NewLine + Environment.NewLine ) )
            {
                result = result.Replace( Environment.NewLine + Environment.NewLine, Environment.NewLine );
            }
            while ( result.Contains( "\x0A\x0A" ) )
            {
                result = result.Replace( "\x0A\x0A", "\x0A" );
            }

            if ( string.IsNullOrWhiteSpace( result.Replace( ",", string.Empty ) ) )
            {
                return string.Empty;
            }

            return result;
        }

        #endregion

        #region Block Actions

        [BlockAction]
        public BlockActionResult GetNcoaData()
        {
            int resultCount = GetAttributeValue( AttributeKey.ResultCount ).AsIntegerOrNull() ?? 20;

            var query = new NcoaHistoryService( RockContext ).Queryable();

            var processed = FilterProcessed;

            if ( processed.HasValue )
            {
                if ( processed.Value != Processed.All && processed.Value != Processed.ManualUpdateRequiredOrNotProcessed )
                {
                    query = query.Where( i => i.Processed == processed );
                }
                else if ( processed.Value == Processed.ManualUpdateRequiredOrNotProcessed )
                {
                    query = query.Where( i => i.Processed == Processed.ManualUpdateRequired || i.Processed == Processed.NotProcessed );
                }
            }

            var ncoaHistoryData = query
            .Select( i => new
            {
                i.Id,
                i.NcoaType,
                i.Processed,
                i.MoveDate,
                i.MoveDistance,

                i.OriginalStreet1,
                i.OriginalStreet2,
                i.OriginalCity,
                i.OriginalState,
                i.OriginalPostalCode,

                i.UpdatedStreet1,
                i.UpdatedStreet2,
                i.UpdatedCity,
                i.UpdatedState,
                i.UpdatedPostalCode
            } )
            .ToList();

            var bag = new NcoaResultsBag
            {
                NcoaList = ncoaHistoryData.Select( i => new NcoaDataBag
                {
                    IdKey = i.Id.AsIdKey(),
                    Type = i.NcoaType.ToString(),
                    OriginalAddress = FormattedAddress(
                            i.OriginalStreet1, i.OriginalStreet2, i.OriginalCity, i.OriginalState, i.OriginalPostalCode )
                        .ConvertCrLfToHtmlBr(),
                    NewAddress = FormattedAddress(
                            i.UpdatedStreet1, i.UpdatedStreet2, i.UpdatedCity, i.UpdatedState, i.UpdatedPostalCode )
                        .ConvertCrLfToHtmlBr(),
                    MoveDate = i.MoveDate,
                    MoveDistance = i.MoveDistance,
                    Status = i.Processed == Processed.Complete ? "Processed" : "Not Processed"
                } ).ToList()
            };

            return ActionOk( bag );
        }
        #endregion
    }
}
