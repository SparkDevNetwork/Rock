using Rock;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Data;
using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using Rock.ViewModels.Blocks.Reporting.VolunteerGenerosityAnalysis;
using Rock.Obsidian.UI;

namespace Rock.Blocks.Reporting
{
    [DisplayName( "Volunteer Generosity Analysis" )]
    [Category( "Reporting" )]
    [Description( "Displays an analysis of volunteer generosity based on persisted dataset 'VolunteerGenerosity'." )]
    [Rock.SystemGuid.BlockTypeGuid( "586A26F1-8A9C-4AB4-B788-9B44895B9D40" )]
    [Rock.SystemGuid.EntityTypeGuid( "4C55BFE1-7E97-4CFB-BCB7-2015AA25D9B9" )]

    public class VolunteerGenerosityAnalysis : RockListBlockType<PersonData>
    {
        #region Keys

        private static class PreferenceKey
        {
            public const string DateRange = "filter-date-range";
            public const string Campus = "filter-campus";
            public const string Team = "filter-team";
        }

        #endregion

        #region Properties

        protected int? FilterDateRange => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.DateRange ).AsIntegerOrNull();

        protected string FilterCampus => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.Campus );

        protected string FilterTeam => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.Team );

        private static readonly string VolunteerGenerosityDatasetGuid = "10539E72-B5D3-48E2-B9C6-DB43AFDAD55F";

        #endregion

        #region Methods
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var datasetGuid = new Guid( VolunteerGenerosityDatasetGuid );
                var dataset = PersistedDatasetCache.Get( datasetGuid );

                if ( dataset == null || string.IsNullOrWhiteSpace( dataset.ResultData ) || !dataset.LastRefreshDateTime.HasValue )
                {
                    RefreshData();
                    dataset = PersistedDatasetCache.Get( datasetGuid ); // Re-fetch the dataset after refresh
                }

                if ( !string.IsNullOrWhiteSpace( dataset?.ResultData ) )
                {
                    try
                    {
                        var dataBag = dataset.ResultData.FromJsonOrNull<VolunteerGenerosityDataBag>();

                        if ( dataBag != null )
                        {
                            var uniqueCampuses = dataBag.GroupData.Select( g => g.CampusShortCode ).Distinct().ToList();
                            var uniqueGroups = dataBag.GroupData.Select( g => g.GroupName ).Distinct().ToList();
                            var lastUpdated = dataset.LastRefreshDateTime.HasValue ? dataset.LastRefreshDateTime.Value.ToString( "yyyy-MM-dd" ) : "N/A";
                            var estimatedRefreshTime = dataset.TimeToBuildMS.HasValue ? dataset.TimeToBuildMS : 0;

                            var bag = new
                            {
                                UniqueCampuses = uniqueCampuses,
                                UniqueGroups = uniqueGroups,
                                LastUpdated = lastUpdated,
                                EstimatedRefreshTime = estimatedRefreshTime
                            };

                            return bag;
                        }
                    }
                    catch ( Exception ex )
                    {
                        throw new Exception( $"Error processing dataset: {ex.Message}", ex );
                    }
                }

                return null;
            }
        }

        protected override IQueryable<PersonData> GetListQueryable( RockContext rockContext )
        {
            var datasetGuid = new Guid( VolunteerGenerosityDatasetGuid );
            var dataset = PersistedDatasetCache.Get( datasetGuid );

            if ( dataset == null || string.IsNullOrWhiteSpace( dataset.ResultData ) )
            {
                return Enumerable.Empty<PersonData>().AsQueryable();
            }

            var dataBag = dataset.ResultData.FromJsonOrNull<VolunteerGenerosityDataBag>();
            if ( dataBag == null )
            {
                return Enumerable.Empty<PersonData>().AsQueryable();
            }

            IEnumerable<PersonData> filteredPeople = dataBag.PeopleData;

            // Filter by Campus
            if ( !string.IsNullOrWhiteSpace( FilterCampus ) && FilterCampus != "All" )
            {
                var campusGroupIds = dataBag.GroupData
                    .Where( g => g.CampusShortCode.Equals( FilterCampus, StringComparison.OrdinalIgnoreCase ) )
                    .Select( g => g.GroupId.ToString() );

                filteredPeople = filteredPeople.Where( p => p.GroupIds.Intersect( campusGroupIds ).Any() );
            }

            // Filter by Team
            if ( !string.IsNullOrWhiteSpace( FilterTeam ) && FilterTeam != "All" )
            {
                var teamGroupIds = dataBag.GroupData
                    .Where( g => g.GroupName.Equals( FilterTeam, StringComparison.OrdinalIgnoreCase ) )
                    .Select( g => g.GroupId.ToString() );

                filteredPeople = filteredPeople.Where( p => p.GroupIds.Intersect( teamGroupIds ).Any() );
            }

            // Filter by Date Range
            if ( FilterDateRange.HasValue )
            {
                var cutoffDate = DateTime.Today.AddDays( -FilterDateRange.Value );
                filteredPeople = filteredPeople.Where( person =>
                    dataBag.GivingData.Any( gd => gd.GivingId == person.GivingId &&
                                                 gd.Donations.Any( d => !string.IsNullOrEmpty( d.Year ) &&
                                                                       !string.IsNullOrEmpty( d.Month ) &&
                                                                       int.TryParse( d.Year, out int year ) &&
                                                                       int.TryParse( d.Month, out int month ) &&
                                                                       new DateTime( year, month, 1 ) >= cutoffDate ) ) );
            }

            return filteredPeople.AsQueryable();
        }
        protected override GridBuilder<PersonData> GetGridBuilder()
        {
            var datasetGuid = new Guid( VolunteerGenerosityDatasetGuid );
            var dataset = PersistedDatasetCache.Get( datasetGuid );
            VolunteerGenerosityDataBag dataBag = null;

            if ( dataset != null && !string.IsNullOrWhiteSpace( dataset.ResultData ) )
            {
                dataBag = dataset.ResultData.FromJsonOrNull<VolunteerGenerosityDataBag>();
            }

            return new GridBuilder<PersonData>()
                .AddField( "id", d => d.PersonId )
                .AddTextField( "campus", d =>
                {
                    var firstGroup = d.GroupIds
                        .Select( groupId => dataBag?.GroupData.FirstOrDefault( g => g.GroupId == groupId ) )
                        .FirstOrDefault();
                    return firstGroup?.CampusShortCode ?? string.Empty;
                } )
                .AddTextField( "lastAttendanceDate", d =>
                {
                    DateTime lastAttendanceDate;
                    if ( DateTime.TryParse( d.LastAttendanceDate, out lastAttendanceDate ) )
                    {
                        return lastAttendanceDate.ToString( "M/d/yyyy" );
                    }
                    return "N/A"; 
                } )
                .AddTextField( "team", d =>
                {
                    var firstGroup = d.GroupIds
                        .Select( groupId => dataBag?.GroupData.FirstOrDefault( g => g.GroupId == groupId ) )
                        .FirstOrDefault();
                    return firstGroup?.GroupName ?? string.Empty;
                } )
                .AddTextField( "givingMonths", d =>
                {
                    var givingDataItem = dataBag?.GivingData.FirstOrDefault( g => g.GivingId == d.GivingId );
                    return string.Join( ", ", givingDataItem?.Donations.Select( gd => $"{gd.MonthNameAbbreviated} {gd.Year}" ) );
                } )
                .AddTextField( "lastUpdated", d => "N/A" ) 
                .AddTextField( "estimatedRefreshTime", d => "N/A" )
                .AddField( "person", d => d );
        }

        [BlockAction]
        public BlockActionResult RefreshData()
        {
            using ( var rockContext = new RockContext() )
            {
                var datasetGuid = new Guid( VolunteerGenerosityDatasetGuid );
                var dataset = new PersistedDatasetService( rockContext )
                    .Queryable()
                    .FirstOrDefault( d => d.Guid == datasetGuid );

                if ( dataset == null )
                {
                    return ActionNotFound();
                }

                dataset.UpdateResultData();
                rockContext.SaveChanges();

                PersistedDatasetCache.UpdateCachedEntity( dataset.Id, System.Data.Entity.EntityState.Modified );

                return ActionOk();
            }
        }

        #endregion
    }
}

