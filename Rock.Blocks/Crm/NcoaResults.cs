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
using Rock.Data;
using Rock.Enums.Controls;
using Rock.Model;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Crm.NcoaResults;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Crm
{
    /// <summary>
    /// Displays a list of Ncoa Results.
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

        private Processed? FilterProcessed => PersonPreferences
            .GetValue( PreferenceKey.FilterProcessed )
            .ConvertToEnumOrNull<Processed>() ?? Processed.ManualUpdateRequiredOrNotProcessed;

        private MoveType? FilterMoveType => PersonPreferences
            .GetValue( PreferenceKey.FilterMoveType )
            .ConvertToEnumOrNull<MoveType>();

        private AddressStatus? FilterAddressStatus => PersonPreferences
            .GetValue( PreferenceKey.FilterAddressStatus )
            .ConvertToEnumOrNull<AddressStatus>();

        private AddressInvalidReason? FilterInvalidReason => PersonPreferences
            .GetValue( PreferenceKey.FilterInvalidReason )
            .ConvertToEnumOrNull<AddressInvalidReason>();

        private SlidingDateRangeBag FilterMoveDate => PersonPreferences
            .GetValue( PreferenceKey.FilterMoveDate )
            .ToSlidingDateRangeBagOrNull();

        private SlidingDateRangeBag FilterNcoaProcessedDate => PersonPreferences
            .GetValue( PreferenceKey.FilterNcoaProcessedDate )
            .ToSlidingDateRangeBagOrNull();

        protected Decimal? FilterMoveDistance => PersonPreferences
            .GetValue( PreferenceKey.FilterMoveDistance )
            .AsDecimalOrNull();

        protected string FilterLastName => PersonPreferences
            .GetValue( PreferenceKey.FilterLastName );

        protected Guid? FilterCampus => PersonPreferences
            .GetValue( PreferenceKey.FilterCampus )
            .FromJsonOrNull<ListItemBag>()?.Value.AsGuidOrNull();



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
        /// Formats the provided address components into a single address string.
        /// </summary>
        /// <param name="street1">The first street line.</param>
        /// <param name="street2">The second street line.</param>
        /// <param name="city">The city.</param>
        /// <param name="state">The state.</param>
        /// <param name="postalCode">The postal code.</param>
        /// <returns>The formatted address, or an empty string if no meaningful address data exists.</returns>
        private string FormattedAddress( string street1, string street2, string city, string state, string postalCode )
        {
            var isAddressEmpty = string.IsNullOrWhiteSpace( street1 )
                && string.IsNullOrWhiteSpace( street2 )
                && string.IsNullOrWhiteSpace( city );

            if ( isAddressEmpty )
            {
                return string.Empty;
            }

            var result = string.Format( "{0} {1} {2}, {3} {4}",
                street1, street2, city, state, postalCode )
                .ReplaceWhileExists( "  ", " " )
                .ReplaceWhileExists( Environment.NewLine + Environment.NewLine, Environment.NewLine )
                .ReplaceWhileExists( "\x0A\x0A", "\x0A" );

            if ( string.IsNullOrWhiteSpace( result.Replace( ",", string.Empty ) ) )
            {
                return string.Empty;
            }

            return result;
        }

        public Dictionary<int, string> GetPersonNamesForFamilies( List<int> familyIds )
        {
            var groupMemberService = new GroupMemberService( RockContext );
            var familyGroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();

            var families = groupMemberService.Queryable()
                .Where( gm =>
                    familyIds.Contains( gm.GroupId ) &&
                    gm.Group.GroupType.Guid == familyGroupTypeGuid &&
                    !gm.Person.IsDeceased )
                .Select( gm => new
                {
                    gm.GroupId,
                    FullName = gm.Person.NickName + " " + gm.Person.LastName
                } )
                .ToList()
                .GroupBy( x => x.GroupId )
                .ToDictionary(
                    g => g.Key,
                    g => string.Join( ", ", g.Select( x => x.FullName ) )
                );

            return families;
        }

        #endregion

        #region Block Actions

        [BlockAction]
        public BlockActionResult GetNcoaData(int pageNumber)
        {
            int resultCount = GetAttributeValue( AttributeKey.ResultCount ).AsIntegerOrNull() ?? 20;

            pageNumber = pageNumber > 0 ? pageNumber : 1;
            resultCount = resultCount > 0 ? resultCount : 20;

            var ncoaQuery = new NcoaHistoryService( RockContext ).Queryable();

            var processed = FilterProcessed;
            var moveType = FilterMoveType;
            var moveDate = FilterMoveDate;
            var ncoaProcessedDate = FilterNcoaProcessedDate;
            var addressStatus = FilterAddressStatus;
            var addressInvalidReason = FilterInvalidReason;
            var moveDistance = FilterMoveDistance;
            var lastName = FilterLastName;
            int? campusId = null;

            if ( FilterCampus.HasValue )
            {
                campusId = CampusCache.GetId( FilterCampus.Value );
            }

            // Processed Status Filtering
            if ( processed.HasValue )
            {
                if ( processed.Value != Processed.All && processed.Value != Processed.ManualUpdateRequiredOrNotProcessed )
                {
                    ncoaQuery = ncoaQuery.Where( i => i.Processed == processed.Value );
                }
                else if ( processed.Value == Processed.ManualUpdateRequiredOrNotProcessed )
                {
                    ncoaQuery = ncoaQuery.Where( i => i.Processed == Processed.ManualUpdateRequired || i.Processed == Processed.NotProcessed );
                }
            }

            // Move Type Filtering
            if ( moveType.HasValue )
            {
                ncoaQuery = ncoaQuery.Where( i => i.MoveType == moveType.Value );
            }

            // Move Date Filtering
            if ( moveDate != null )
            {
                // Default to the last 180 days if a null/invalid range was selected.
                var defaultSlidingDateRange = new SlidingDateRangeBag
                {
                    RangeType = SlidingDateRangeType.Last,
                    TimeUnit = TimeUnitType.Day,
                    TimeValue = 180
                };

                var dateRange = moveDate.Validate( defaultSlidingDateRange ).ActualDateRange;

                if ( dateRange.Start.HasValue )
                {
                    ncoaQuery = ncoaQuery.Where( i => i.MoveDate >= dateRange.Start );
                }

                if ( dateRange.End.HasValue )
                {
                    ncoaQuery = ncoaQuery.Where( i => i.MoveDate < dateRange.End );
                }
            }

            // NCOA Processed Date Filtering
            if ( ncoaProcessedDate != null )
            {
                // Default to the last 180 days if a null/invalid range was selected.
                var defaultSlidingDateRange = new SlidingDateRangeBag
                {
                    RangeType = SlidingDateRangeType.Last,
                    TimeUnit = TimeUnitType.Day,
                    TimeValue = 180
                };

                var dateRange = ncoaProcessedDate.Validate( defaultSlidingDateRange ).ActualDateRange;

                if ( dateRange.Start.HasValue )
                {
                    ncoaQuery = ncoaQuery.Where( i => i.NcoaRunDateTime >= dateRange.Start );
                }

                if ( dateRange.End.HasValue )
                {
                    ncoaQuery = ncoaQuery.Where( i => i.NcoaRunDateTime <= dateRange.End );
                }
            }

            // Address Status Filtering
            if ( addressStatus.HasValue )
            {
                ncoaQuery = ncoaQuery.Where( i => i.AddressStatus == addressStatus.Value );
            }

            // Address Invalid Reason Filtering
            if ( addressInvalidReason.HasValue )
            {
                ncoaQuery = ncoaQuery.Where( i => i.AddressInvalidReason == addressInvalidReason.Value );
            }

            // Move Distance Filtering
            if ( moveDistance != null )
            {
                ncoaQuery = ncoaQuery.Where( i => i.MoveDistance <= moveDistance );
            }

            // Last Name Filtering
            if ( lastName.IsNotNullOrWhiteSpace() )
            {
                var personAliasQuery = new PersonAliasService( RockContext ).Queryable().Where( p => p.Person.LastName.Contains( lastName ) ).Select( p => p.Id);
                ncoaQuery = ncoaQuery.Where( i => personAliasQuery.Contains( i.PersonAliasId ) );
            }

            //Campus Filtering
            if ( campusId.HasValue )
            {
                var familyGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
                var personAliasQuery = new PersonAliasService( RockContext ).Queryable().AsNoTracking();
                var campusQuery = new GroupMemberService( RockContext )
                    .Queryable().AsNoTracking()
                    .Where( m =>
                        m.Group.GroupTypeId == familyGroupType.Id &&
                        m.Group.CampusId.HasValue &&
                        m.Group.CampusId.Value == campusId )
                    .Select( m => m.PersonId )
                    .Join( personAliasQuery, m => m, p => p.PersonId, ( m, p ) => p.Id );

                ncoaQuery = ncoaQuery.Where( i => campusQuery.Contains( i.PersonAliasId ) );
            }

            // Group duplicate family moves.
            var groupedFamilyMoves = ncoaQuery
                .Where( i => i.MoveType != MoveType.Individual )
                .GroupBy( i => new { i.FamilyId, i.MoveType, i.MoveDate } )
                .Select( g => g.OrderByDescending( x => x.Id ).FirstOrDefault() )
                .ToList();

            var individualMoves = ncoaQuery
                .Where( i => i.MoveType == MoveType.Individual )
                .ToList();

            var combinedData = groupedFamilyMoves
                .Concat( individualMoves )
                .OrderBy( i => i.FamilyId )
                .ToList();

            var totalResults = combinedData.Count();

            var ncoaHistoryData = combinedData
                .OrderBy(i => i.Id)
                .Skip( ( pageNumber - 1 ) * resultCount )
                .Take( resultCount )
                .ToList();

            // Records that are not individual move types and will represent family moves.
            var familyIds = ncoaHistoryData
                .Where( h => h.MoveType != MoveType.Individual )
                .GroupBy( h => new { h.FamilyId, h.MoveType, h.MoveDate } )
                .Select( g => g.Max( x => x.FamilyId ) ).ToList();

            var familyNamesKey = GetPersonNamesForFamilies( familyIds );

            var ncoaPersonAliasIds = ncoaHistoryData.Select( d => d.PersonAliasId ).ToList();

            var personData = new PersonAliasService( RockContext ).Queryable().AsNoTracking()
                .Where( p => ncoaPersonAliasIds.Contains( p.Id ) )
                .Select( p => new
                {
                    personAliasId = p.Id,
                    personId = p.Person.Id,
                    p.Person.NickName,
                    p.Person.LastName,
                } ).ToList();

            var ncoaItems = ncoaHistoryData.Select( i =>
            {
                var individual = personData.Where( p => p.personAliasId == i.PersonAliasId ).FirstOrDefault();


                return new NcoaDataBag
                {
                    IdKey = i.Id.AsIdKey(),
                    FamilyId = i.FamilyId,
                    Type = i.NcoaType.ToString(),
                    IndividualIdKey = individual.personId.AsIdKey(),
                    IndividualName = individual.NickName + ' ' + individual.LastName,
                    FamilyMembers = familyNamesKey.ContainsKey( i.FamilyId ) ? familyNamesKey[i.FamilyId] : string.Empty,

                    OriginalAddress = FormattedAddress(
                            i.OriginalStreet1, i.OriginalStreet2, i.OriginalCity, i.OriginalState, i.OriginalPostalCode )
                        .ConvertCrLfToHtmlBr(),

                    NewAddress = FormattedAddress(
                            i.UpdatedStreet1, i.UpdatedStreet2, i.UpdatedCity, i.UpdatedState, i.UpdatedPostalCode )
                        .ConvertCrLfToHtmlBr(),

                    MoveDate = i.MoveDate.ToShortDateString(),
                    MoveDistance = i.MoveDistance,
                    ProcessStatus = i.Processed == Processed.Complete ? "Processed" : "Not Processed",
                    AddressStatus = i.AddressStatus.ToString()
                };
            } ).ToList();

            var groupedNcoaItems = ncoaItems
                .GroupBy( n => n.FamilyId )
                .Select( g => new NcoaFamilyGroupBag
                {
                    FamilyName = familyNamesKey.ContainsKey( g.Key ) ? familyNamesKey[g.Key].Split( ' ' ).Last() : null,
                    NcoaItems = g.ToList()
                } ).ToList();


            var bag = new NcoaResultsBag
            {
                TotalResults = totalResults,
                NcoaList = groupedNcoaItems
            };

            return ActionOk( bag );
        }

        [BlockAction]
        public BlockActionResult UpdateNcoaHistoryItem(string ncoaHistoryIdKey, bool isMarkProcessed)
        {
            var ncoaHistoryItem = new NcoaHistoryService( RockContext ).Get( ncoaHistoryIdKey );

            if ( ncoaHistoryItem == null )
            {
                return ActionBadRequest( "Could not find NCOA History Item" );
            }

            if ( isMarkProcessed )
            {
                ncoaHistoryItem.Processed = Processed.Complete;
            }
            else
            {

                var groupService = new GroupService( RockContext );
                var groupLocationService = new GroupLocationService( RockContext );

                var changes = new History.HistoryChangeList();

                var ncoa = new NCOA.Ncoa();

                var previousValue = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_PREVIOUS.AsGuid() );
                int? previousValueId = previousValue == null ? ( int? ) null : previousValue.Id;
                var previousGroupLocation = ncoa.MarkAsPreviousLocation( ncoaHistoryItem, groupLocationService, previousValueId, changes );

                if ( previousGroupLocation == null )
                {
                    return ActionBadRequest( "This family is no longer associated with that location." );
                }
                ncoaHistoryItem.Processed = Processed.Complete;

                if ( changes.Any() )
                {
                    var family = groupService.Get( ncoaHistoryItem.FamilyId );
                    if ( family != null )
                    {
                        foreach ( var fm in family.Members )
                        {
                            HistoryService.SaveChanges(
                                RockContext,
                                typeof( Person ),
                                Rock.SystemGuid.Category.HISTORY_PERSON_FAMILY_CHANGES.AsGuid(),
                                fm.PersonId,
                                changes,
                                family.Name,
                                typeof( Model.Group ),
                                family.Id,
                                false );
                        }
                    }
                }
            }

            RockContext.SaveChanges();

            return ActionOk();
        }
        #endregion
    }
}
