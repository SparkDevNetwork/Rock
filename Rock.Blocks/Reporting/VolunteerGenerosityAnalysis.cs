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

    public class VolunteerGenerosityAnalysis : RockListBlockType<VolunteerGenerosityPersonDataBag>
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
                        var lastUpdated = dataset.LastRefreshDateTime.HasValue ? dataset.LastRefreshDateTime.Value.ToString( "yyyy-MM-dd HH:mm:ss" ) : "N/A";
                        var estimatedRefreshTime = dataset.TimeToBuildMS.HasValue ? Math.Round( dataset.TimeToBuildMS.Value / 1000.0, 2 ) : 0.0;
                        var dataBag = dataset.ResultData.FromJsonOrNull<VolunteerGenerosityDataBag>();

                        if ( dataBag != null )
                        {
                            var uniqueCampuses = dataBag.PeopleData
                                .Where( p => !string.IsNullOrEmpty( p.PersonDetails.CampusShortCode ) )
                                .Select( p => p.PersonDetails.CampusShortCode )
                                .Distinct()
                                .ToList();

                            var uniqueGroups = dataBag.PeopleData.Select( p => p.PersonDetails.GroupName ).Distinct().ToList();

                            var bag = new VolunteerGenerositySetupBag
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

        protected override IQueryable<VolunteerGenerosityPersonDataBag> GetListQueryable( RockContext rockContext )
        {
            var datasetGuid = new Guid( VolunteerGenerosityDatasetGuid );
            var dataset = PersistedDatasetCache.Get( datasetGuid );

            if ( dataset == null || string.IsNullOrWhiteSpace( dataset.ResultData ) )
            {
                return Enumerable.Empty<VolunteerGenerosityPersonDataBag>().AsQueryable();
            }

            var dataBag = dataset.ResultData.FromJsonOrNull<VolunteerGenerosityDataBag>();
            if ( dataBag == null )
            {
                return Enumerable.Empty<VolunteerGenerosityPersonDataBag>().AsQueryable();
            }

            IEnumerable<VolunteerGenerosityPersonDataBag> filteredPeople = dataBag.PeopleData;

            // Filter by Campus
            if ( !string.IsNullOrWhiteSpace( FilterCampus ) && FilterCampus != "All" )
            {
                filteredPeople = filteredPeople.Where( person => person.PersonDetails.CampusShortCode == FilterCampus );
            }

            // Filter by Team
            if ( !string.IsNullOrWhiteSpace( FilterTeam ) && FilterTeam != "All" )
            {
                filteredPeople = filteredPeople.Where( person => person.PersonDetails.GroupName == FilterTeam );
            }

            // Filter by Date Range
            if ( FilterDateRange.HasValue && FilterDateRange.Value != 0 )
            {
                var cutoffDate = DateTime.Today.AddDays( -FilterDateRange.Value );
                filteredPeople = filteredPeople.Where( person =>
                    person.Donations.Any( d =>
                        int.TryParse( d.Year, out int year ) &&
                        int.TryParse( d.Month, out int month ) &&
                        year != 0 &&
                        month != 0 &&
                        new DateTime( year, month, 1 ) >= cutoffDate
                    )
                );
            }

            return filteredPeople.AsQueryable();
        }

        protected override GridBuilder<VolunteerGenerosityPersonDataBag> GetGridBuilder()
        {
            return new GridBuilder<VolunteerGenerosityPersonDataBag>()
                .AddField( "id", d => d.PersonDetails.PersonId )
                .AddTextField( "campus", d => d.PersonDetails.CampusShortCode )
                .AddTextField( "lastAttendanceDate", d =>
                {
                    DateTime lastAttendanceDate;
                    if ( DateTime.TryParse( d.PersonDetails.LastAttendanceDate, out lastAttendanceDate ) )
                    {
                        return lastAttendanceDate.ToString( "M/d/yyyy" );
                    }
                    return "N/A";
                } )
                .AddTextField( "team", d => d.PersonDetails.GroupName )
                .AddTextField( "givingMonths", d =>
                {
                    return string.Join( ", ", d.Donations.Select( donation =>
                    {
                        int.TryParse( donation.Year, out int year );
                        return $"{donation.MonthNameAbbreviated} {year}";
                    } ) );
                } )
                .AddField( "person", d => new VolunteerGenerosityPersonBag
                {
                    PersonId = d.PersonDetails.PersonId,
                    LastName = d.PersonDetails.LastName,
                    NickName = d.PersonDetails.NickName,
                    PhotoUrl = d.PersonDetails.PhotoUrl
                } )
                .AddField( "givingId", d => d.PersonDetails.GivingId )
                .AddField( "groupName", d => d.PersonDetails.GroupName );
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

                var lastUpdated = DateTime.Now.ToString( "yyyy-MM-dd HH:mm:ss" );
                var estimatedRefreshTime = dataset.TimeToBuildMS.HasValue ? Math.Round( dataset.TimeToBuildMS.Value / 1000.0, 2 ) : 0.0; // Convert to seconds and round to 2 decimal places

                return ActionOk( new { LastUpdated = lastUpdated, EstimatedRefreshTime = estimatedRefreshTime } );
            }
        }

        #endregion
    }
}

