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
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    public partial class RegistrationRegistrantService
    {
        /// <summary>
        /// Gets the group placement registrants.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public List<GroupPlacementRegistrant> GetGroupPlacementRegistrants( GetGroupPlacementRegistrantsParameters options )
        {
            var rockContext = this.Context as RockContext;

            var registrationRegistrantService = new RegistrationRegistrantService( rockContext );
            var registrationRegistrantQuery = registrationRegistrantService.Queryable();

            registrationRegistrantQuery = registrationRegistrantQuery
                .Where( a => a.Registration.RegistrationInstance.RegistrationTemplateId == options.RegistrationTemplateId );

            if ( options.RegistrationInstanceId.HasValue )
            {
                registrationRegistrantQuery = registrationRegistrantQuery.Where( a => a.Registration.RegistrationInstanceId == options.RegistrationInstanceId.Value );
            }
            else
            {
                if ( options.RegistrationTemplateInstanceIds?.Any() == true )
                {
                    registrationRegistrantQuery = registrationRegistrantQuery.Where( a => options.RegistrationTemplateInstanceIds.Contains( a.Registration.RegistrationInstanceId ) );
                }
            }

            if ( options.DataViewFilterId.HasValue )
            {
                var dataFilter = new DataViewFilterService( new RockContext() ).Get( options.DataViewFilterId.Value );
                List<string> errorMessages = new List<string>();

                var expression = dataFilter?.GetExpression( typeof( RegistrationRegistrant ), registrationRegistrantService, registrationRegistrantService.ParameterExpression, errorMessages );
                if ( expression != null )
                {
                    registrationRegistrantQuery = registrationRegistrantQuery.Where( registrationRegistrantService.ParameterExpression, expression );
                }
            }

            if ( options.RegistrantId.HasValue )
            {
                registrationRegistrantQuery = registrationRegistrantQuery.Where( a => a.Id == options.RegistrantId.Value );
            }

            var registrationTemplatePlacement = new RegistrationTemplatePlacementService( rockContext ).Get( options.RegistrationTemplatePlacementId );

            registrationRegistrantQuery = registrationRegistrantQuery.OrderBy( a => a.PersonAlias.Person.LastName ).ThenBy( a => a.PersonAlias.Person.NickName );

            var registrationTemplatePlacementService = new RegistrationTemplatePlacementService( rockContext );
            var registrationInstanceService = new RegistrationInstanceService( rockContext );

            // get a queryable of PersonIds for the registration template shared groups so we can determine if the registrant has been placed
            var registrationTemplatePlacementGroupsPersonIdQuery = registrationTemplatePlacementService.GetRegistrationTemplatePlacementPlacementGroups( registrationTemplatePlacement ).SelectMany( a => a.Members ).Select( a => a.PersonId );

            // and also get a queryable of PersonIds for the registration instance placement groups so we can determine if the registrant has been placed 
            IQueryable<InstancePlacementGroupPersonId> allInstancesPlacementGroupInfoQuery = null;

            if ( options.RegistrationInstanceId.HasValue )
            {
                allInstancesPlacementGroupInfoQuery =
                    registrationInstanceService.GetRegistrationInstancePlacementGroups( registrationInstanceService.Get( options.RegistrationInstanceId.Value ) )
                        .Where( a => a.GroupTypeId == registrationTemplatePlacement.GroupTypeId )
                        .SelectMany( a => a.Members ).Select( a => a.PersonId )
                        .Select( s => new InstancePlacementGroupPersonId
                        {
                            PersonId = s,
                            RegistrationInstanceId = options.RegistrationInstanceId.Value
                        } );
            }
            else
            {
                if ( options.RegistrationTemplateInstanceIds?.Any() == true )
                {
                    foreach ( var registrationInstanceId in options.RegistrationTemplateInstanceIds )
                    {
                        var instancePlacementGroupInfoQuery = registrationInstanceService.GetRegistrationInstancePlacementGroups( registrationInstanceService.Get( registrationInstanceId ) )
                        .Where( a => a.GroupTypeId == registrationTemplatePlacement.GroupTypeId )
                        .SelectMany( a => a.Members ).Select( a => a.PersonId )
                        .Select( s => new InstancePlacementGroupPersonId
                        {
                            PersonId = s,
                            RegistrationInstanceId = registrationInstanceId
                        } );

                        if ( allInstancesPlacementGroupInfoQuery == null )
                        {
                            allInstancesPlacementGroupInfoQuery = instancePlacementGroupInfoQuery;
                        }
                        else
                        {
                            allInstancesPlacementGroupInfoQuery = allInstancesPlacementGroupInfoQuery.Union( instancePlacementGroupInfoQuery );
                        }
                    }
                }
            }

            if ( allInstancesPlacementGroupInfoQuery == null )
            {
                return null;
            }

            // select in a way to avoid lazy loading
            var registrationRegistrantPlacementQuery = registrationRegistrantQuery.Select( r => new
            {
                Registrant = r,
                r.PersonAlias.Person,
                RegistrationInstanceName = r.Registration.RegistrationInstance.Name,

                // marked as AlreadyPlacedInGroup if the Registrant is a member of any of the registrant template placement group or the registration instance placement groups
                AlreadyPlacedInGroup =
                    registrationTemplatePlacementGroupsPersonIdQuery.Contains( r.PersonAlias.PersonId )
                    || allInstancesPlacementGroupInfoQuery.Any( x => x.RegistrationInstanceId == r.Registration.RegistrationInstanceId && x.PersonId == r.PersonAlias.PersonId )
            } );

            var registrationRegistrantPlacementList = registrationRegistrantPlacementQuery.AsNoTracking().ToList();

            var groupPlacementRegistrantList = registrationRegistrantPlacementList
                .Select( x => new GroupPlacementRegistrant( x.Registrant, x.Person, x.AlreadyPlacedInGroup, x.RegistrationInstanceName, options ) )
                .ToList();

            return groupPlacementRegistrantList.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        private class InstancePlacementGroupPersonId
        {
            /// <summary>
            /// Gets or sets the person identifier.
            /// </summary>
            /// <value>
            /// The person identifier.
            /// </value>
            public int PersonId { get; set; }

            /// <summary>
            /// Gets or sets the registration instance identifier.
            /// </summary>
            /// <value>
            /// The registration instance identifier.
            /// </value>
            public int RegistrationInstanceId { get; set; }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class GroupPlacementRegistrant
    {
        /// <summary>
        /// Gets or sets the registration registrant.
        /// </summary>
        /// <value>
        /// The registration registrant.
        /// </value>
        private RegistrationRegistrant RegistrationRegistrant { get; set; }

        /// <summary>
        /// Gets or sets the options.
        /// </summary>
        /// <value>
        /// The options.
        /// </value>
        private GetGroupPlacementRegistrantsParameters Options { get; set; }

        /// <summary>
        /// Gets or sets the person.
        /// </summary>
        /// <value>
        /// The person.
        /// </value>
        private Person Person { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupPlacementRegistrant" /> class.
        /// </summary>
        /// <param name="registrationRegistrant">The registration registrant.</param>
        /// <param name="person">The person.</param>
        /// <param name="alreadyPlacedInGroup">if set to <c>true</c> [already placed in group].</param>
        /// <param name="registrationInstanceName"></param>
        /// <param name="options">The options.</param>
        public GroupPlacementRegistrant( RegistrationRegistrant registrationRegistrant, Person person, bool alreadyPlacedInGroup, string registrationInstanceName, GetGroupPlacementRegistrantsParameters options )
        {
            this.RegistrationRegistrant = registrationRegistrant;
            this.Person = person;
            this.AlreadyPlacedInGroup = alreadyPlacedInGroup;
            this.RegistrationInstanceName = registrationInstanceName;
            this.Options = options;
        }

        /// <summary>
        /// Gets the person identifier.
        /// </summary>
        /// <value>
        /// The person identifier.
        /// </value>
        public int PersonId => this.Person.Id;

        /// <summary>
        /// Gets the name of the person.
        /// </summary>
        /// <value>
        /// The name of the person.
        /// </value>
        public string PersonName => this.Person.FullName;

        /// <summary>
        /// Gets the person gender.
        /// </summary>
        /// <value>
        /// The person gender.
        /// </value>
        public Gender PersonGender => this.Person.Gender;

        /// <summary>
        /// Gets the registrant identifier.
        /// </summary>
        /// <value>
        /// The registrant identifier.
        /// </value>
        public int RegistrantId => RegistrationRegistrant.Id;

        /// <summary>
        /// Gets the name of the registration instance.
        /// </summary>
        /// <value>
        /// The name of the registration instance.
        /// </value>
        public string RegistrationInstanceName { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [already placed in group].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [already placed in group]; otherwise, <c>false</c>.
        /// </value>
        public bool AlreadyPlacedInGroup { get; private set; }

        /// <summary>
        /// Gets the fees.
        /// </summary>
        /// <value>
        /// The fees.
        /// </value>
        public Dictionary<string, decimal> Fees
        {
            get
            {
                if ( this.Options.IncludeFees )
                {
                    var feeInfo = RegistrationRegistrant.Fees.Select( a => new
                    {
                        FeeName = a.RegistrationTemplateFeeItem?.Name ?? "Fee",
                        a.TotalCost
                    } ).ToDictionary( a => a.FeeName, v => v.TotalCost );

                    return feeInfo;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the attributes.
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        public Dictionary<string, AttributeCache> Attributes
        {
            get
            {
                if ( !Options.DisplayedAttributeIds.Any() )
                {
                    // don't spend time loading attributes if there aren't any to be displayed
                    return null;
                }

                if ( RegistrationRegistrant.AttributeValues == null )
                {
                    RegistrationRegistrant.LoadAttributes();
                }

                var displayedAttributeValues = RegistrationRegistrant
                        .Attributes.Where( a => Options.DisplayedAttributeIds.Contains( a.Value.Id ) )
                        .ToDictionary( k => k.Key, v => v.Value );

                return displayedAttributeValues;
            }
        }

        /// <summary>
        /// Gets the displayed attribute values.
        /// </summary>
        /// <value>
        /// The displayed attribute values.
        /// </value>
        public Dictionary<string, AttributeValueCache> AttributeValues
        {
            get
            {
                if ( !Options.DisplayedAttributeIds.Any() )
                {
                    // don't spend time loading attributes if there aren't any to be displayed
                    return null;
                }

                if ( RegistrationRegistrant.AttributeValues == null )
                {
                    RegistrationRegistrant.LoadAttributes();
                }

                var displayedAttributeValues = RegistrationRegistrant
                    .AttributeValues.Where( a => Options.DisplayedAttributeIds.Contains( a.Value.AttributeId ) )
                    .ToDictionary( k => k.Key, v => v.Value );

                return displayedAttributeValues;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class GetGroupPlacementRegistrantsParameters
    {
        /// <summary>
        /// Gets or sets the registration template identifier.
        /// </summary>
        /// <value>
        /// The registration template identifier.
        /// </value>
        public int RegistrationTemplateId { get; set; }

        /// <summary>
        /// Gets or sets the registration template instance ids.
        /// </summary>
        /// <value>
        /// The registration template instance ids.
        /// </value>
        public int[] RegistrationTemplateInstanceIds { get; set; }

        /// <summary>
        /// Gets or sets the registration template placement identifier.
        /// </summary>
        /// <value>
        /// The registration template placement identifier.
        /// </value>
        public int RegistrationTemplatePlacementId { get; set; }

        /// <summary>
        /// Gets or sets the registration instance identifier.
        /// </summary>
        /// <value>
        /// The registration instance identifier.
        /// </value>
        public int? RegistrationInstanceId { get; set; }

        /// <summary>
        /// Gets or sets the registrant identifier.
        /// </summary>
        /// <value>
        /// The registrant identifier.
        /// </value>
        public int? RegistrantId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [include fees].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [include fees]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeFees { get; set; }

        /// <summary>
        /// Gets or sets the data view filter identifier.
        /// </summary>
        /// <value>
        /// The data view filter identifier.
        /// </value>
        public int? DataViewFilterId { get; set; }

        /// <summary>
        /// Gets or sets the displayed attribute ids.
        /// </summary>
        /// <value>
        /// The displayed attribute ids.
        /// </value>
        public int[] DisplayedAttributeIds { get; set; } = new int[0];
    }
}
