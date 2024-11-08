using Rock;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Data;
using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using Rock.ViewModels.Blocks.Finance.VolunteerGenerosityAnalysis;
using Rock.Obsidian.UI;
using System.Globalization;

namespace Rock.Blocks.Finance
{
    [DisplayName( "Volunteer Generosity Analysis" )]
    [Category( "Finance" )]
    [Description( "Displays an analysis of volunteer generosity based on the system persisted dataset 'VolunteerGenerosity'." )]
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

                if ( dataset == null || string.IsNullOrWhiteSpace( dataset.ResultData ) || !dataset.LastRefreshDateTime.HasValue )
                {
                    RefreshData();
                    dataset = PersistedDatasetCache.Get( datasetGuid ); // Re-fetch the dataset after refresh
                }

                if ( !string.IsNullOrWhiteSpace( dataset?.ResultData ) )
                {
                    try
                    {
                        var box = dataset.ResultData.FromJsonOrNull<VolunteerGenerosityInitializationBox>();

                        if ( box != null && ( box.UniqueCampuses == null || box.UniqueGroups == null ) )
                        {
                            var peopleData = box.PeopleData;
                            var uniqueCampuses = peopleData
                                .Where( p => !string.IsNullOrEmpty( p.CampusShortCode ) )
                                .Select( p => p.CampusShortCode )
                                .Distinct()
                                .ToList();

                            var uniqueGroups = peopleData.Select( p => p.GroupName ).Distinct().ToList();
                            var hasMultipleCampuses = CampusCache.All().Count( c => ( bool ) c.IsActive ) > 1;

                            box.UniqueCampuses = uniqueCampuses;
                            box.UniqueGroups = uniqueGroups;
                            box.LastUpdated = dataset.LastRefreshDateTime.HasValue ? dataset.LastRefreshDateTime.Value.ToRockDateTimeOffset().ToString( "O" ) : null;
                            box.EstimatedRefreshTime = dataset.TimeToBuildMS.HasValue ? Math.Round( dataset.TimeToBuildMS.Value / 1000.0, 2 ) : 0.0;
                            box.ShowCampusFilter = hasMultipleCampuses;

                            foreach ( var person in peopleData )
                            {
                                person.LastAttendanceDate = person.LastAttendanceDate?.ToLocalTime();
                                person.DonationMonths = DecodeDonationDateKeys( person.DonationDateKeys );
                            }
                        }

                        return box;
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

            var dataRoot = dataset.ResultData.FromJsonOrNull<VolunteerGenerosityInitializationBox>();
            if ( dataRoot == null || dataRoot.PeopleData == null )
            {
                return Enumerable.Empty<VolunteerGenerosityDataBag>().AsQueryable();
            }

            IEnumerable<VolunteerGenerosityDataBag> filteredPeople = dataRoot.PeopleData.AsEnumerable();

            // Filter out all of the inactive or archived people
            filteredPeople = filteredPeople.Where( p => p.IsActive );

            // Filter by Campus
            if ( !string.IsNullOrWhiteSpace( FilterCampus ) && FilterCampus != "All" )
            {
                filteredPeople = filteredPeople.Where( person => person.CampusShortCode == FilterCampus );
            }

            // Filter by Team
            if ( !string.IsNullOrWhiteSpace( FilterTeam ) && FilterTeam != "All" )
            {
                filteredPeople = filteredPeople.Where( person => person.GroupName == FilterTeam );
            }

            // Filter by Date Range
            if ( FilterDateRange.HasValue && FilterDateRange.Value != 0 )
            {
                var cutoffDate = DateTime.Today.AddDays( -FilterDateRange.Value );
                filteredPeople = filteredPeople.Where( person =>
                    person.LastAttendanceDate.HasValue &&
                    person.LastAttendanceDate.Value >= cutoffDate
                );
            }

            return filteredPeople.AsQueryable();
        }

        protected override GridBuilder<VolunteerGenerosityDataBag> GetGridBuilder()
        {
            return new GridBuilder<VolunteerGenerosityDataBag>()
                .AddField( "id", d => d.PersonId )
                .AddTextField( "campus", d => d.CampusShortCode )
                .AddDateTimeField( "lastAttendanceDate", d => d.LastAttendanceDate )
                .AddTextField( "team", d => d.GroupName )
                .AddTextField( "givingMonths", d => DecodeDonationDateKeys( d.DonationDateKeys ) )
                .AddField( "donationDateKeys", d => d.DonationDateKeys )
                .AddField( "person", d => new VolunteerGenerosityPersonBag
                {
                    PersonId = d.PersonId,
                    LastName = d.LastName,
                    NickName = d.NickName,
                    PhotoUrl = Person.GetPersonPhotoUrl(
                                initials: $"{d.NickName.Substring( 0, 1 )}{d.LastName.Substring( 0, 1 )}",
                                photoId: d.PhotoId,
                                age: d.Age,
                                gender: d.Gender,
                                recordTypeValueId: null,
                                ageClassification: d.AgeClassification,
                                size: null
                               ),
                    ConnectionStatus = d.ConnectionStatus
                } )
                .AddField( "givingId", d => d.GivingId )
                .AddField( "groupName", d => d.GroupName );
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

        private string DecodeDonationDateKeys( string donationDateKeys )
        {
            if ( string.IsNullOrEmpty( donationDateKeys ) )
            {
                return null;
            }

            var donationMonths = new HashSet<string>();
            var dateKeys = donationDateKeys.Split( '|' );
            foreach ( var dateKey in dateKeys )
            {
                if ( DateTime.TryParseExact( dateKey, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date ) )
                {
                    donationMonths.Add( date.ToString( "MMM yyyy", CultureInfo.InvariantCulture ) );
                }
            }

            return string.Join( ", ", donationMonths.OrderBy( m => DateTime.ParseExact( m, "MMM yyyy", CultureInfo.InvariantCulture ) ) );
        }

        #endregion
    }
}

