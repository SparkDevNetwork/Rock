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

    public class VolunteerGenerosityAnalysis : RockListBlockType<VolunteerGenerosityDataBag>
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

                // Check if dataset needs to be refreshed
                if ( dataset == null || string.IsNullOrWhiteSpace( dataset.ResultData ) || !dataset.LastRefreshDateTime.HasValue )
                {
                    // Refresh the dataset
                    RefreshData();
                }

                // Proceed with normal data processing
                if ( !string.IsNullOrWhiteSpace( dataset.ResultData ) )
                {
                    try
                    {
                        var analysisData = dataset.ResultData.FromJsonOrNull<List<VolunteerGenerosityDataBag>>();

                        var uniqueCampuses = analysisData.SelectMany( d => d.Groups.Select( g => g.ShortCode ) ).Distinct().ToList();
                        var uniqueGroups = analysisData.SelectMany( d => d.Groups.Select( g => g.GroupName ) ).Distinct().ToList();

                        var bag = new
                        {
                            UniqueCampuses = uniqueCampuses,
                            UniqueGroups = uniqueGroups
                        };

                        return bag;
                    }
                    catch ( Exception ex )
                    {
                        throw new Exception( $"Error processing dataset: {ex.Message}", ex );
                    }
                }

                return null;
            }
        }

        protected override IQueryable<VolunteerGenerosityDataBag> GetListQueryable( RockContext rockContext )
        {
            var datasetGuid = new Guid( VolunteerGenerosityDatasetGuid );
            var dataset = PersistedDatasetCache.Get( datasetGuid );

            if ( dataset == null || string.IsNullOrWhiteSpace( dataset.ResultData ) )
            {
                return Enumerable.Empty<VolunteerGenerosityDataBag>().AsQueryable();
            }

            var analysisData = dataset.ResultData.FromJsonOrNull<List<VolunteerGenerosityDataBag>>();

            // Extract lastUpdated and estimatedRefreshTime from the dataset
            var lastUpdated = dataset.LastRefreshDateTime?.ToString() ?? string.Empty;
            var estimatedRefreshTime = dataset.TimeToBuildMS.Value;

            // Populate the Person data for each record
            foreach ( var data in analysisData )
            {
                // First ensure that data.Person is not null before accessing its properties
                if ( data.Person == null )
                {
                    data.Person = new PersonDtoBag();
                }

                // Now set the properties
                data.Person.NickName = data.NickName;
                data.Person.LastName = data.LastName;
                data.Person.PhotoUrl = data.PhotoUrl;
                data.Person.IdKey = data.PersonId.ToString();

                data.LastUpdated = lastUpdated;
                data.EstimatedRefreshTime = estimatedRefreshTime;
            }

            IQueryable<VolunteerGenerosityDataBag> query = analysisData.AsQueryable();

            // Apply filters
            if ( FilterDateRange.HasValue && FilterDateRange.Value > 0 )
            {
                var cutoffDate = RockDateTime.Today.AddDays( -FilterDateRange.Value ).Date;
                query = query.Where( d => d.Groups.Any( g => g.LastAttendanceDate >= cutoffDate ) );
            }

            if ( !string.IsNullOrWhiteSpace( FilterCampus ) && FilterCampus != "All" )
            {
                query = query.Where( d => d.Groups.Any( g => g.ShortCode.Equals( FilterCampus, StringComparison.OrdinalIgnoreCase ) ) );
            }

            if ( !string.IsNullOrWhiteSpace( FilterTeam ) && FilterTeam != "All" )
            {
                query = query.Where( d => d.Groups.Any( g => g.GroupName.Equals( FilterTeam, StringComparison.OrdinalIgnoreCase ) ) );
            }

            return query;
        }

        protected override GridBuilder<VolunteerGenerosityDataBag> GetGridBuilder()
        {
            return new GridBuilder<VolunteerGenerosityDataBag>()
                .AddField( "id", d => d.PersonId )
                .AddTextField( "campus", d => d.Groups.FirstOrDefault()?.ShortCode )
                .AddTextField( "lastAttendanceDate", d => d.Groups.FirstOrDefault()?.LastAttendanceDate.ToShortDateString() )
                .AddTextField( "team", d => d.Groups.FirstOrDefault()?.GroupName )
                .AddTextField( "givingMonths", d => string.Join( ", ", d.Groups.FirstOrDefault()?.GivingData.Select( g => g.MonthYearFormatted ) ) )
                .AddTextField( "lastUpdated", d => d.LastUpdated )
                .AddTextField( "estimatedRefreshTime", d => d.EstimatedRefreshTime.ToString() )
                .AddField( "person", d => d.Person );
        }

        #endregion

        #region Block Actions

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

                // Refresh the dataset and save the changes
                dataset.UpdateResultData();
                rockContext.SaveChanges();

                // Update the cache
                PersistedDatasetCache.UpdateCachedEntity( dataset.Id, System.Data.Entity.EntityState.Modified );

                return ActionOk();
            }
        }

        #endregion
    }
}

