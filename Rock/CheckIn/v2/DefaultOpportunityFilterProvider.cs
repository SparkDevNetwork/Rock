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

using Rock.CheckIn.v2.Filters;

namespace Rock.CheckIn.v2
{
    /// <summary>
    /// Provides the logic for filtering opportunities in check-in.
    /// </summary>
    internal class DefaultOpportunityFilterProvider
    {
        #region Fields

        /// <summary>
        /// The default group filter types.
        /// </summary>
        private static readonly List<Type> _defaultGroupFilterTypes = new List<Type>
        {
            typeof( AgeOpportunityFilter ),
            typeof( BirthMonthOpportunityFilter ),
            typeof( GradeOpportunityFilter ),
            typeof( GenderOpportunityFilter ),
            typeof( MembershipOpportunityFilter ),
            typeof( DataViewOpportunityFilter )
        };

        /// <summary>
        /// The default location filter types.
        /// </summary>
        private static readonly List<Type> _defaultLocationFilterTypes = new List<Type>
        {
            typeof( LocationClosedOpportunityFilter ),
            typeof( ThresholdOpportunityFilter )
        };

        /// <summary>
        /// The default schedule filter types.
        /// </summary>
        private static readonly List<Type> _defaultScheduleFilterTypes = new List<Type>
        {
            typeof( DuplicateCheckInOpportunityFilter )
        };

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the check-in template configuration in effect during filtering.
        /// </summary>
        /// <value>The check-in template configuration.</value>
        protected TemplateConfigurationData TemplateConfiguration => Session.TemplateConfiguration;

        /// <summary>
        /// Gets or sets the check-in session.
        /// </summary>
        /// <value>The check-in session.</value>
        protected CheckInSession Session { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultOpportunityFilterProvider"/> class.
        /// </summary>
        /// <param name="session">The check-in session.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="session"/> is <c>null</c>.</exception>
        public DefaultOpportunityFilterProvider( CheckInSession session )
        {
            Session = session ?? throw new ArgumentNullException( nameof( session ) );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Filters the check-in opportunities for a single person.
        /// </summary>
        /// <param name="person">The person to use when filtering opportunities.</param>
        public virtual void FilterPersonOpportunities( Attendee person )
        {
            var groupFilters = GetGroupFilters( person );
            var locationFilters = GetLocationFilters( person );
            var scheduleFilters = GetScheduleFilters( person );

            // Run group filters.
            groupFilters.ForEach( f => f.FilterGroups( person.Opportunities ) );

            // If this person is not already disabled and there are no groups
            // then disable the person with appropriate reason.
            if ( !person.IsUnavailable && person.Opportunities.Groups.Count == 0 )
            {
                person.IsUnavailable = true;
                person.UnavailableMessage = "No Matching Groups Found";
            }

            // Remove any locations that have no group referencing them.
            var allReferencedLocationIds = new HashSet<string>( person.Opportunities.Groups.SelectMany( g => g.LocationIds ) );
            person.Opportunities.Locations.RemoveAll( l => !allReferencedLocationIds.Contains( l.Id ) );

            // Run location filters.
            locationFilters.ForEach( f => f.FilterLocations( person.Opportunities ) );

            // If this person is not already disabled and there are no locations
            // then disable the person with appropriate reason.
            if ( !person.IsUnavailable && person.Opportunities.Locations.Count == 0 )
            {
                person.IsUnavailable = true;
                person.UnavailableMessage = "No Locations Available";
            }

            // Remove any schedules that have no group referencing them.
            var allReferencedScheduleIds = new HashSet<string>( person.Opportunities.Locations.SelectMany( l => l.ScheduleIds ) );
            person.Opportunities.Schedules.RemoveAll( s => !allReferencedScheduleIds.Contains( s.Id ) );

            // Run schedule filters.
            scheduleFilters.ForEach( f => f.FilterSchedules( person.Opportunities ) );
        }

        /// <summary>
        /// Removes any opportunity items that are "empty". Meaning, if a group has
        /// no locations then it can't be available as a choice so it will be
        /// removed.
        /// </summary>
        /// <param name="person">The person whose opportunities should be cleaned up.</param>
        public virtual void RemoveEmptyOpportunities( Attendee person )
        {
            person.Opportunities.RemoveEmptyOpportunities();
        }

        /// <summary>
        /// Gets the filter type definitions to use when filtering opportunities for
        /// groups.
        /// </summary>
        /// <returns>A collection of <see cref="Type"/> objects.</returns>
        protected virtual IReadOnlyCollection<Type> GetGroupFilterTypes()
        {
            return _defaultGroupFilterTypes;
        }

        /// <summary>
        /// Gets the filter type definitions to use when filtering opportunities for
        /// locations.
        /// </summary>
        /// <returns>A collection of <see cref="Type"/> objects.</returns>
        protected virtual IReadOnlyCollection<Type> GetLocationFilterTypes()
        {
            return _defaultLocationFilterTypes;
        }

        /// <summary>
        /// Gets the filter type definitions to use when filtering opportunities for
        /// schedules.
        /// </summary>
        /// <returns>A collection of <see cref="Type"/> objects.</returns>
        protected virtual IReadOnlyCollection<Type> GetScheduleFilterTypes()
        {
            return _defaultScheduleFilterTypes;
        }

        /// <summary>
        /// Gets the filters to use when filtering opportunities for a specific group.
        /// </summary>
        /// <param name="person">The person to filter opportunities for.</param>
        /// <returns>A list of <see cref="OpportunityFilter"/> objects that will perform filtering logic.</returns>
        private List<OpportunityFilter> GetGroupFilters( Attendee person )
        {
            var types = GetGroupFilterTypes();

            return CreateOpportunityFilters( types, person );
        }

        /// <summary>
        /// Gets the filters to use when filtering opportunities for a specific location.
        /// </summary>
        /// <param name="person">The person to filter opportunities for.</param>
        /// <returns>A list of <see cref="OpportunityFilter"/> objects that will perform filtering logic.</returns>
        private List<OpportunityFilter> GetLocationFilters( Attendee person )
        {
            var types = GetLocationFilterTypes();

            return CreateOpportunityFilters( types, person );
        }

        /// <summary>
        /// Gets the filters to use when filtering opportunities for a specific
        /// schedule.
        /// </summary>
        /// <param name="person">The person to filter opportunities for.</param>
        /// <returns>A list of <see cref="OpportunityFilter"/> objects that will perform filtering logic.</returns>
        private List<OpportunityFilter> GetScheduleFilters( Attendee person )
        {
            var types = GetScheduleFilterTypes();

            return CreateOpportunityFilters( types, person );
        }

        /// <summary>
        /// Creates the opportunity filters specified by the types. This filters will
        /// be properly initialized before returning.
        /// </summary>
        /// <param name="filterTypes">The filter types.</param>
        /// <param name="person">The person to filter for.</param>
        /// <returns>A collection of filter instances.</returns>
        private List<OpportunityFilter> CreateOpportunityFilters( IReadOnlyCollection<Type> filterTypes, Attendee person )
        {
            var expectedType = typeof( OpportunityFilter );

            return filterTypes
                .Where( t => expectedType.IsAssignableFrom( t ) )
                .Select( t =>
                {
                    var filter = ( OpportunityFilter ) Activator.CreateInstance( t );

                    filter.TemplateConfiguration = TemplateConfiguration;
                    filter.Session = Session;
                    filter.Person = person;

                    return filter;
                } )
                .ToList();
        }

        #endregion
    }
}
