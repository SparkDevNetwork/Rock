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
using System.Linq;

using Rock.Data;
using Rock.Enums.CheckIn;
using Rock.Model;
using Rock.Observability;
using Rock.Utility;
using Rock.ViewModels.CheckIn;
using Rock.Web.Cache;

namespace Rock.CheckIn.v2
{
    /// <summary>
    /// The check-in session handles all logic related to the process of checking
    /// in one or more attendees.
    /// </summary>
    internal class CheckInSession
    {
        #region Properties

        /// <summary>
        /// Gets the check-in director managing this session.
        /// </summary>
        /// <value>The check-in director.</value>
        public CheckInDirector Director { get; }

        /// <summary>
        /// The context to use when accessing the database.
        /// </summary>
        /// <value>The database context.</value>
        public RockContext RockContext => Director.RockContext;

        /// <summary>
        /// Gets the check-in template configuration.
        /// </summary>
        /// <value>The check-in template configuration.</value>
        public TemplateConfigurationData TemplateConfiguration { get; }

        /// <summary>
        /// <para>
        /// Gets the attendees that have been loaded as part of this session.
        /// This is set after calling one of the LoadAttendees methods.
        /// </para>
        /// <para>
        /// This property does not persist between API calls since a new session
        /// object is created each time. So the list of attendees would not be
        /// available when, for example, saving attendance.
        /// </para>
        /// </summary>
        /// <value>The attendees.</value>
        public IReadOnlyList<Attendee> Attendees { get; private set; }

        /// <summary>
        /// Gets the opportunity filter provider to be used with this instance.
        /// </summary>
        /// <value>The opportunity filter provider.</value>
        public virtual DefaultOpportunityFilterProvider OpportunityFilterProvider { get; }

        /// <summary>
        /// Gets the selection provider to be used with this instance.
        /// </summary>
        /// <value>The selection provider.</value>
        public virtual DefaultSelectionProvider SelectionProvider { get; }

        /// <summary>
        /// Gets the search provider to be used with this instance.
        /// </summary>
        /// <value>The search provider.</value>
        public virtual DefaultSearchProvider SearchProvider { get; }

        /// <summary>
        /// Gets the save provider to be used with this instance.
        /// </summary>
        /// <value>The save provider.</value>
        public virtual DefaultSaveProvider SaveProvider { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckInSession"/> class.
        /// </summary>
        /// <param name="director">The director to get base information from.</param>
        /// <param name="templateConfiguration">The check-in template configuration.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="director"/> is <c>null</c>.</exception>
        /// <exception cref="System.ArgumentNullException"><paramref name="templateConfiguration"/> is <c>null</c>.</exception>
        public CheckInSession( CheckInDirector director, TemplateConfigurationData templateConfiguration )
        {
            if ( director == null )
            {
                throw new ArgumentNullException( nameof( director ) );
            }

            if ( director == null )
            {
                throw new ArgumentNullException( nameof( director ) );
            }

            Director = director;
            TemplateConfiguration = templateConfiguration;

            OpportunityFilterProvider = new DefaultOpportunityFilterProvider( this );
            SelectionProvider = new DefaultSelectionProvider( this );
            SearchProvider = new DefaultSearchProvider( this );
            SaveProvider = new DefaultSaveProvider( this );
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Searches for families that match the criteria for the configuration
        /// template.
        /// </summary>
        /// <param name="searchTerm">The search term.</param>
        /// <param name="searchType">Type of the search.</param>
        /// <returns>A collection of <see cref="FamilyBag"/> objects.</returns>
        public List<FamilyBag> SearchForFamilies( string searchTerm, FamilySearchMode searchType )
        {
            return SearchForFamilies( searchTerm, searchType, null );
        }

        /// <summary>
        /// Searches for families that match the criteria for the configuration
        /// template.
        /// </summary>
        /// <param name="searchTerm">The search term.</param>
        /// <param name="searchType">Type of the search.</param>
        /// <param name="sortByCampus">If provided, then results will be sorted by families matching this campus first.</param>
        /// <returns>A collection of <see cref="FamilyBag"/> objects.</returns>
        public List<FamilyBag> SearchForFamilies( string searchTerm, FamilySearchMode searchType, CampusCache sortByCampus )
        {
            if ( searchTerm.IsNullOrWhiteSpace() )
            {
                throw new CheckInMessageException( "Search term must not be empty." );
            }

            using ( var activity = ObservabilityHelper.StartActivity( "Search for Families" ) )
            {
                activity?.AddTag( "rock.checkin.search_provider", SearchProvider.GetType().FullName );

                var familyQry = SearchProvider.GetFamilySearchQuery( searchTerm, searchType );
                var familyIdQry = SearchProvider.GetSortedFamilyIdSearchQuery( familyQry, sortByCampus );
                var familyMemberQry = SearchProvider.GetFamilyMemberSearchQuery( familyIdQry );

                return SearchProvider.GetFamilySearchItemBags( familyMemberQry );
            }
        }

        /// <summary>
        /// Loads the attendee information for the specified family. This will
        /// populate the <see cref="Attendees"/> property and perform all
        /// filtering and default selections.
        /// </summary>
        /// <param name="familyId">The family identifier to load.</param>
        /// <param name="possibleAreas">The possible areas that are to be considered when generating the opportunities.</param>
        /// <param name="kiosk">The optional kiosk to use.</param>
        /// <param name="locations">The list of locations to use.</param>
        public void LoadAndPrepareAttendeesForFamily( string familyId, IReadOnlyCollection<GroupTypeCache> possibleAreas, DeviceCache kiosk, IReadOnlyCollection<NamedLocationCache> locations )
        {
            var opportunities = Director.GetAllOpportunities( possibleAreas, kiosk, locations );
            var groupMemberQry = GetGroupMembersQueryForFamily( familyId );
            var members = GetFamilyMemberBags( familyId, groupMemberQry );

            LoadAttendees( members.Select( fm => fm.Person ).ToList(), opportunities );
            PrepareAttendees();
        }

        /// <summary>
        /// Loads the attendee information for the specified family. This will
        /// populate the <see cref="Attendees"/> property and perform all
        /// filtering and default selections.
        /// </summary>
        /// <param name="personId">The identifier of the person to load attendee information for.</param>
        /// <param name="familyId">The family identifier to load.</param>
        /// <param name="possibleAreas">The possible areas that are to be considered when generating the opportunities.</param>
        /// <param name="kiosk">The optional kiosk to use.</param>
        /// <param name="locations">The list of locations to use.</param>
        public void LoadAndPrepareAttendeesForPerson( string personId, string familyId, IReadOnlyCollection<GroupTypeCache> possibleAreas, DeviceCache kiosk, IReadOnlyCollection<NamedLocationCache> locations )
        {
            var checkInOpportunities = Director.GetAllOpportunities( possibleAreas, kiosk, locations );
            var familyMembersQry = GetGroupMemberQueryForPerson( personId, familyId );
            var members = GetFamilyMemberBags( null, familyMembersQry );

            LoadAttendees( members.Select( fm => fm.Person ).ToList(), checkInOpportunities );
            PrepareAttendees();
        }

        /// <summary>
        /// Find all group members that match the specified family unique
        /// identifier for check-in. This normally includes immediate family
        /// members as well as people associated to the family with one of
        /// the configured "can check-in" known relationships.
        /// </summary>
        /// <param name="familyId">The family identifier.</param>
        /// <returns>A queryable that can be used to load all the group members associated with the family.</returns>
        public IQueryable<GroupMember> GetGroupMembersQueryForFamily( string familyId )
        {
            using ( var activity = ObservabilityHelper.StartActivity( "Get Group Members Query For Family" ) )
            {
                activity?.AddTag( "rock.checkin.search_provider", SearchProvider.GetType().FullName );

                return SearchProvider.GetGroupMembersForFamilyQuery( familyId );
            }
        }

        /// <summary>
        /// Find the group member that matches the specified person unique
        /// identifier for check-in. If the family unique identifier is specified
        /// then it is used to sort the result so the GroupMember record
        /// associated with that family is the one used. If the family unique
        /// identifer is not specified or not found then the first family GroupMember
        /// record will be returned.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="familyId">The family identifier used to sort the records.</param>
        /// <returns>A queryable that can be used to load this person.</returns>
        public IQueryable<GroupMember> GetGroupMemberQueryForPerson( string personId, string familyId )
        {
            using ( var activity = ObservabilityHelper.StartActivity( "Get Group Member Query For Person" ) )
            {
                activity?.AddTag( "rock.checkin.search_provider", SearchProvider.GetType().FullName );

                return SearchProvider.GetPersonForFamilyQuery( personId, familyId );
            }
        }

        /// <summary>
        /// Converts the group members into bags that represent the people
        /// for check-in.
        /// </summary>
        /// <param name="familyId">The primary family unique identifier, this is used to resolve duplicates where a family member is also marked as can check-in.</param>
        /// <param name="groupMembers">The <see cref="GroupMember"/> objects to be converted to bags.</param>
        /// <returns>A collection of <see cref="FamilyMemberBag"/> objects.</returns>
        public List<FamilyMemberBag> GetFamilyMemberBags( string familyId, IEnumerable<GroupMember> groupMembers )
        {
            using ( var activity = ObservabilityHelper.StartActivity( "Get Person Bags" ) )
            {
                activity?.AddTag( "rock.checkin.conversion_provider", Director.ConversionProvider.GetType().FullName );

                return Director.ConversionProvider.GetFamilyMemberBags( familyId, groupMembers );
            }
        }

        /// <summary>
        /// Filters the check-in opportunities for a single person.
        /// </summary>
        /// <param name="attendee">The attendee whose opportunities will be filtered.</param>
        public void FilterPersonOpportunities( Attendee attendee )
        {
            using ( var activity = ObservabilityHelper.StartActivity( $"Filter Opportunities For {attendee.Person.NickName}" ) )
            {
                activity?.AddTag( "rock.checkin.opportunity_filter_provider", OpportunityFilterProvider.GetType().FullName );

                OpportunityFilterProvider.FilterPersonOpportunities( attendee );
                OpportunityFilterProvider.RemoveEmptyOpportunities( attendee );

                // Do a final check to see if the attendee should be disabled.
                if ( !attendee.IsUnavailable )
                {
                    var noOptions = attendee.Opportunities.Groups.Count == 0
                        || attendee.Opportunities.Locations.Count == 0
                        || attendee.Opportunities.Schedules.Count == 0
                        || attendee.Opportunities.Areas.Count == 0;

                    if ( noOptions )
                    {
                        attendee.IsUnavailable = true;
                        attendee.UnavailableMessage = "Not Available For Check-in";
                    }
                }
            }
        }

        /// <summary>
        /// Loads the attendee information for the family members. This also
        /// gathers all required information to later perform filtering on the
        /// attendees.
        /// </summary>
        /// <param name="people">The <see cref="PersonBag"/> objects to be used when constructing the <see cref="Attendee"/> objects that will wrap them.</param>
        /// <param name="baseOpportunities">The opportunity collection to clone onto each attendee.</param>
        public void LoadAttendees( IReadOnlyCollection<PersonBag> people, OpportunityCollection baseOpportunities )
        {
            using ( var activity = ObservabilityHelper.StartActivity( $"Get Attendee Items" ) )
            {
                activity?.AddTag( "rock.checkin.conversion_provider", Director.ConversionProvider.GetType().FullName );

                var preSelectCutoff = RockDateTime.Today.AddDays( Math.Min( -1, 0 - TemplateConfiguration.AutoSelectDaysBack ) );
                var recentAttendance = CheckInDirector.GetRecentAttendance( preSelectCutoff, people.Select( fm => fm.Id ).ToList(), RockContext );

                var attendees = Director.ConversionProvider.GetAttendeeItems( people, baseOpportunities, recentAttendance );

                Attendees = attendees;
            }
        }

        /// <summary>
        /// Prepares all of the <see cref="Attendees"/> by filtering and
        /// applying all default selections.
        /// </summary>
        public void PrepareAttendees()
        {
            foreach ( var attendee in Attendees )
            {
                FilterPersonOpportunities( attendee );
                SetDefaultSelectionsForAttendee( attendee );
            }
        }

        /// <summary>
        /// Sets the default selections for the specified attendee. This will
        /// mark a person as pre-selected if they have recent attendance and
        /// it will also set the current selections if the check-in template
        /// is configured that way.
        /// </summary>
        /// <param name="attendee">The attendee to be checked in.</param>
        public void SetDefaultSelectionsForAttendee( Attendee attendee )
        {
            using ( var activity = ObservabilityHelper.StartActivity( $"Set Defaults for {attendee.Person.NickName}" ) )
            {
                var isAutoSelect = TemplateConfiguration.KioskCheckInType == KioskCheckInMode.Family
                    && TemplateConfiguration.AutoSelect == AutoSelectMode.PeopleAndAreaGroupLocation;

                activity?.AddTag( "rock.checkin.selection_provider", SelectionProvider.GetType().FullName );

                if ( isAutoSelect )
                {
                    attendee.SelectedOpportunities = SelectionProvider.GetDefaultSelectionsForPerson( attendee );
                }

                attendee.IsPreSelected = TemplateConfiguration.AutoSelectDaysBack > 0 && attendee.RecentAttendances.Count > 0;
                attendee.IsMultipleSelectionsAvailable = attendee.Opportunities.Areas.Count > 1
                    || attendee.Opportunities.Groups.Count > 1
                    || attendee.Opportunities.Locations.Count > 1
                    || attendee.Opportunities.Schedules.Count > 1;
            }
        }

        /// <summary>
        /// Gets the current attendance bags for the attendees. This means all
        /// the bags that represent attendance records for people that are
        /// considered to be currently checked in. This assumes the
        /// <see cref="Attendee.RecentAttendances"/> property has
        /// been populated for each attendee.
        /// </summary>
        /// <returns>A list of attendance bags.</returns>
        public List<AttendanceBag> GetCurrentAttendanceBags()
        {
            using ( var activity = ObservabilityHelper.StartActivity( "Get Current Attendance Bags" ) )
            {
                activity?.AddTag( "rock.checkin.conversion_provider", Director.ConversionProvider.GetType().FullName );

                var checkedInAttendances = new List<AttendanceBag>();
                var today = RockDateTime.Today;

                foreach ( var attendee in Attendees )
                {
                    var activeAttendances = attendee.RecentAttendances
                        .Where( a => a.StartDateTime >= today
                            && !a.EndDateTime.HasValue )
                        .ToList();

                    // We could get fancy and group things to try to improve
                    // performance a tiny bit, but it would be extremely unsual
                    // for a person to be checked into more than one thing so we
                    // will just do a simple loop.
                    foreach ( var attendance in activeAttendances )
                    {
                        var location = NamedLocationCache.GetByIdKey( attendance.LocationId, RockContext );
                        var schedule = NamedScheduleCache.GetByIdKey( attendance.ScheduleId, RockContext );

                        if ( location == null || schedule == null )
                        {
                            continue;
                        }

                        var campusId = location.GetCampusIdForLocation();
                        var now = campusId.HasValue
                            ? CampusCache.Get( campusId.Value )?.CurrentDateTime ?? RockDateTime.Now
                            : RockDateTime.Now;

                        if ( !schedule.WasScheduleOrCheckInActiveForCheckOut( now ) )
                        {
                            continue;
                        }

                        var attendanceBag = Director.ConversionProvider.GetAttendanceBag( attendance, attendee );

                        checkedInAttendances.Add( attendanceBag );
                    }
                }

                return checkedInAttendances;
            }
        }

        /// <summary>
        /// Gets the attendee bags from the <see cref="Attendees"/> loaded in
        /// this session.
        /// </summary>
        /// <returns>A list of bags that represent the attendees.</returns>
        public List<AttendeeBag> GetAttendeeBags()
        {
            using ( var activity = ObservabilityHelper.StartActivity( "Get Attendee Bags" ) )
            {
                activity?.AddTag( "rock.checkin.conversion_provider", Director.ConversionProvider.GetType().FullName );

                return Attendees
                    .Select( a => Director.ConversionProvider.GetAttendeeBag( a ) )
                    .ToList();
            }
        }

        /// <summary>
        /// Gets the set of all possible schedules for all attendees that have
        /// been loaded. If the first attendee has schedules A and B and the
        /// second attendee has schedules B and C, then the set of [A, B, C]
        /// will be returned.
        /// </summary>
        /// <returns>A list of bags representing all possible schedules.</returns>
        public List<ScheduleOpportunityBag> GetAllPossibleScheduleBags()
        {
            return Attendees
                .SelectMany( a => a.Opportunities.Schedules )
                .DistinctBy( s => s.Id )
                .Select( s => new ScheduleOpportunityBag
                {
                    Id = s.Id,
                    Name = s.Name
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the potential attendee bags from the set of attendee items.
        /// </summary>
        /// <param name="opportunityCollection">The opportunity collection to be converted to a bag.</param>
        /// <returns>A list of bags that represent the attendees.</returns>
        public OpportunityCollectionBag GetOpportunityCollectionBag( OpportunityCollection opportunityCollection )
        {
            using ( var activity = ObservabilityHelper.StartActivity( "Get Opportunity Collection Bag" ) )
            {
                activity?.AddTag( "rock.checkin.conversion_provider", Director.ConversionProvider.GetType().FullName );

                return Director.ConversionProvider.GetOpportunityCollectionBag( opportunityCollection );
            }
        }

        /// <summary>
        /// Saves the attendance requests to the database by creating or updating
        /// existing <see cref="Attendance"/> records.
        /// </summary>
        /// <param name="sessionRequest">The data that describes the check-in session.</param>
        /// <param name="requests">The attendance request details.</param>
        /// <param name="kiosk">The kiosk that is performing this check-in or <c>null</c>.</param>
        /// <param name="clientIpAddress">The remote IP address of the device performing this check-in or <c>null</c>.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="sessionRequest"/> is <c>null</c>.</exception>
        /// <exception cref="System.ArgumentNullException"><paramref name="requests"/> is <c>null</c>.</exception>
        public CheckInResultBag SaveAttendance( AttendanceSessionRequest sessionRequest, IReadOnlyCollection<AttendanceRequestBag> requests, DeviceCache kiosk, string clientIpAddress )
        {
            if ( sessionRequest == null )
            {
                throw new ArgumentNullException( nameof( sessionRequest ) );
            }

            if ( requests == null )
            {
                throw new ArgumentNullException( nameof( requests ) );
            }

            using ( var activity = ObservabilityHelper.StartActivity( "Save Attendance" ) )
            {
                activity?.AddTag( "rock.checkin.save_provider", SaveProvider.GetType().FullName );

                return SaveProvider.SaveAttendance( sessionRequest, requests, kiosk, clientIpAddress );
            }
        }

        /// <summary>
        /// Confirms the pending attendance records for the specified session.
        /// </summary>
        /// <param name="sessionGuid">The session's unique identifier.</param>
        /// <returns>An instance of <see cref="CheckInResultBag"/> that contains the result of the operation.</returns>
        public CheckInResultBag ConfirmAttendance( Guid sessionGuid )
        {
            using ( var activity = ObservabilityHelper.StartActivity( "Confirm Attendance" ) )
            {
                activity?.AddTag( "rock.checkin.save_provider", SaveProvider.GetType().FullName );

                return SaveProvider.ConfirmAttendance( sessionGuid );
            }
        }

        /// <summary>
        /// Saves the attendance requests to the database by creating or updating
        /// existing <see cref="Attendance"/> records.
        /// </summary>
        /// <param name="sessionRequest">The data that describes the check-in session.</param>
        /// <param name="attendanceIds">The attendance identifiers.</param>
        /// <param name="kiosk">The kiosk that is performing this check-in or <c>null</c>.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="sessionRequest"/> is <c>null</c>.</exception>
        /// <exception cref="System.ArgumentNullException"><paramref name="attendanceIds"/> is <c>null</c>.</exception>
        public CheckoutResultBag Checkout( AttendanceSessionRequest sessionRequest, IReadOnlyList<string> attendanceIds, DeviceCache kiosk )
        {
            if ( sessionRequest == null )
            {
                throw new ArgumentNullException( nameof( sessionRequest ) );
            }

            if ( attendanceIds == null )
            {
                throw new ArgumentNullException( nameof( attendanceIds ) );
            }

            using ( var activity = ObservabilityHelper.StartActivity( "Checkout" ) )
            {
                activity?.AddTag( "rock.checkin.save_provider", SaveProvider.GetType().FullName );

                return SaveProvider.Checkout( sessionRequest, attendanceIds, kiosk );
            }
        }

        #endregion
    }
}
