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
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Rock.Data;
using Rock.Reporting;
using Rock.Utility;
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
        /// <param name="currentPerson">The current person.</param>
        /// <returns></returns>
        public List<GroupPlacementRegistrant> GetGroupPlacementRegistrants( GetGroupPlacementRegistrantsParameters options, Person currentPerson )
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
            else if ( options.RegistrationTemplateInstanceIds?.Any() == true )
            {
                registrationRegistrantQuery = registrationRegistrantQuery.Where( a => options.RegistrationTemplateInstanceIds.Contains( a.Registration.RegistrationInstanceId ) );
            }

            if ( options.RegistrantPersonDataViewFilterId.HasValue )
            {
                var dataFilter = new DataViewFilterService( rockContext ).Get( options.RegistrantPersonDataViewFilterId.Value );
                var personService = new PersonService( rockContext );
                var paramExpression = personService.ParameterExpression;

                var personWhereExpression = dataFilter?.GetExpression( typeof( Person ), personService, paramExpression );
                if ( personWhereExpression != null )
                {
                    var personIdQry = personService.Queryable().Where( paramExpression, personWhereExpression, null ).Select( x => x.Id );
                    registrationRegistrantQuery = registrationRegistrantQuery.Where( a => personIdQry.Contains( a.PersonAlias.PersonId ) );
                }
            }

            if ( options.RegistrantId.HasValue )
            {
                registrationRegistrantQuery = registrationRegistrantQuery.Where( a => a.Id == options.RegistrantId.Value );
            }

            var registrationInstanceGroupPlacementBlock = BlockCache.Get( options.BlockId );
            if ( registrationInstanceGroupPlacementBlock != null && currentPerson != null )
            {
                const string RegistrantAttributeFilter_RegistrationInstanceId = "RegistrantAttributeFilter_RegistrationInstanceId_{0}";
                const string RegistrantAttributeFilter_RegistrationTemplateId = "RegistrantAttributeFilter_RegistrationTemplateId_{0}";
                var preferences = PersonPreferenceCache.GetPersonPreferenceCollection( currentPerson, registrationInstanceGroupPlacementBlock );
                string userPreferenceKey;
                if ( options.RegistrationInstanceId.HasValue )
                {
                    userPreferenceKey = string.Format( RegistrantAttributeFilter_RegistrationInstanceId, options.RegistrationInstanceId );
                }
                else
                {
                    userPreferenceKey = string.Format( RegistrantAttributeFilter_RegistrationTemplateId, options.RegistrationTemplateId );
                }

                var attributeFilters = preferences.GetValue( userPreferenceKey ).FromJsonOrNull<Dictionary<int, string>>() ?? new Dictionary<int, string>();
                var parameterExpression = registrationRegistrantService.ParameterExpression;
                Expression registrantWhereExpression = null;
                foreach ( var attributeFilter in attributeFilters )
                {
                    var attribute = AttributeCache.Get( attributeFilter.Key );
                    var attributeFilterValues = attributeFilter.Value.FromJsonOrNull<List<string>>();
                    var entityField = EntityHelper.GetEntityFieldForAttribute( attribute );
                    if ( entityField != null && attributeFilterValues != null )
                    {
                        var attributeWhereExpression = ExpressionHelper.GetAttributeExpression( registrationRegistrantService, parameterExpression, entityField, attributeFilterValues );
                        if ( registrantWhereExpression == null )
                        {
                            registrantWhereExpression = attributeWhereExpression;
                        }
                        else
                        {
                            registrantWhereExpression = Expression.AndAlso( registrantWhereExpression, attributeWhereExpression );
                        }
                    }
                }

                if ( registrantWhereExpression != null )
                {
                    registrationRegistrantQuery = registrationRegistrantQuery.Where( parameterExpression, registrantWhereExpression );
                }
            }

            var registrationTemplatePlacement = new RegistrationTemplatePlacementService( rockContext ).Get( options.RegistrationTemplatePlacementId );

            if ( options.FilterFeeId.HasValue )
            {
                registrationRegistrantQuery = registrationRegistrantQuery.Where( a => a.Fees.Any( f => f.RegistrationTemplateFeeId == options.FilterFeeId.Value ) );
            }

            if ( options.FilterFeeOptionIds?.Any() == true )
            {
                registrationRegistrantQuery = registrationRegistrantQuery.Where( a => a.Fees.Any( f => f.RegistrationTemplateFeeItemId.HasValue && options.FilterFeeOptionIds.Contains( f.RegistrationTemplateFeeItemId.Value ) ) );
            }

            // don't include registrants that are on the waiting list
            registrationRegistrantQuery = registrationRegistrantQuery.Where( a => a.OnWaitList == false );

            registrationRegistrantQuery = registrationRegistrantQuery.OrderBy( a => a.PersonAlias.Person.LastName ).ThenBy( a => a.PersonAlias.Person.NickName );

            var registrationTemplatePlacementService = new RegistrationTemplatePlacementService( rockContext );
            var registrationInstanceService = new RegistrationInstanceService( rockContext );

            // get a queryable of PersonIds for the registration template shared groups so we can determine if the registrant has been placed
            var registrationTemplatePlacementGroupsPersonIdQuery = registrationTemplatePlacementService.GetRegistrationTemplatePlacementPlacementGroups( registrationTemplatePlacement ).SelectMany( a => a.Members ).Select( a => a.PersonId );

            // and also get a queryable of PersonIds for the registration instance placement groups so we can determine if the registrant has been placed 
            IQueryable<InstancePlacementGroupPersonId> allInstancesPlacementGroupInfoQuery = null;

            if ( !options.RegistrationInstanceId.HasValue && ( options.RegistrationTemplateInstanceIds == null || !options.RegistrationTemplateInstanceIds.Any() ) )
            {
                // if neither RegistrationInstanceId or RegistrationTemplateInstanceIds was specified, use all of the RegistrationTemplates instances
                options.RegistrationTemplateInstanceIds = new RegistrationTemplateService( rockContext ).GetSelect( options.RegistrationTemplateId, s => s.Instances.Select( i => i.Id ) ).ToArray();
            }

            if ( options.RegistrationInstanceId.HasValue )
            {
                // PlacementId for 'ByPlacement' method is needed in order to allow a registrant to be available for other placement groups in the same instance.
                allInstancesPlacementGroupInfoQuery =
                    registrationInstanceService.GetRegistrationInstancePlacementGroupsByPlacement(
                        registrationInstanceService.Get( options.RegistrationInstanceId.Value ),
                        options.RegistrationTemplatePlacementId )
                        .Where( a => a.GroupTypeId == registrationTemplatePlacement.GroupTypeId )
                        .SelectMany( a => a.Members ).Select( a => a.PersonId )
                        .Select( s => new InstancePlacementGroupPersonId
                        {
                            PersonId = s,
                            RegistrationInstanceId = options.RegistrationInstanceId.Value
                        } );
            }
            else if ( options.RegistrationTemplateInstanceIds?.Any() == true )
            {
                foreach ( var registrationInstanceId in options.RegistrationTemplateInstanceIds )
                {
                    // PlacementId for 'ByPlacement' method is needed in order to allow a registrant to be available for other placement groups in the same instance.
                    var instancePlacementGroupInfoQuery = registrationInstanceService.GetRegistrationInstancePlacementGroupsByPlacement(
                        registrationInstanceService.Get( registrationInstanceId ),
                        options.RegistrationTemplatePlacementId )
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

            if ( allInstancesPlacementGroupInfoQuery == null )
            {
                throw new ArgumentNullException( "Registration Instance(s) must be specified" );
            }

            // select in a way to avoid lazy loading
            var registrationRegistrantPlacementQuery = registrationRegistrantQuery.Select( r => new
            {
                Registrant = r,
                r.PersonAlias.Person,
                r.Registration.RegistrationInstance,

                // marked as AlreadyPlacedInGroup if the Registrant is a member of any of the registrant template placement group or the registration instance placement groups
                AlreadyPlacedInGroup =
                    registrationTemplatePlacementGroupsPersonIdQuery.Contains( r.PersonAlias.PersonId )
                    || allInstancesPlacementGroupInfoQuery.Any( x => x.RegistrationInstanceId == r.Registration.RegistrationInstanceId && x.PersonId == r.PersonAlias.PersonId )
            } );

            var registrationRegistrantPlacementList = registrationRegistrantPlacementQuery.AsNoTracking().ToList();

            var groupPlacementRegistrantList = registrationRegistrantPlacementList
                .Select( x => new GroupPlacementRegistrant( x.Registrant, x.Person, x.AlreadyPlacedInGroup, x.RegistrationInstance, options ) )
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
        /// Gets or sets the registration instance.
        /// </summary>
        /// <value>
        /// The registration instance.
        /// </value>
        private RegistrationInstance RegistrationInstance { get; set; }

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
        [Obsolete( "Use the other GroupPlacementRegistrant constructor ", true )]
        [RockObsolete( "1.10.3" )]
        public GroupPlacementRegistrant( RegistrationRegistrant registrationRegistrant, Person person, bool alreadyPlacedInGroup, string registrationInstanceName, GetGroupPlacementRegistrantsParameters options )
            : this( registrationRegistrant, person, alreadyPlacedInGroup, registrationRegistrant.Registration.RegistrationInstance, options )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupPlacementRegistrant" /> class.
        /// </summary>
        /// <param name="registrationRegistrant">The registration registrant.</param>
        /// <param name="person">The person.</param>
        /// <param name="alreadyPlacedInGroup">if set to <c>true</c> [already placed in group].</param>
        /// <param name="registrationInstance">The registration instance.</param>
        /// <param name="options">The options.</param>
        public GroupPlacementRegistrant( RegistrationRegistrant registrationRegistrant, Person person, bool alreadyPlacedInGroup, RegistrationInstance registrationInstance, GetGroupPlacementRegistrantsParameters options )
        {
            this.RegistrationRegistrant = registrationRegistrant;
            this.Person = person;
            this.AlreadyPlacedInGroup = alreadyPlacedInGroup;
            this.RegistrationInstance = registrationInstance;
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
        public string RegistrationInstanceName => RegistrationInstance.Name;

        /// <summary>
        /// Gets the registration instance identifier.
        /// </summary>
        /// <value>
        /// The registration instance identifier.
        /// </value>
        public int RegistrationInstanceId => RegistrationInstance.Id;

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
        public Dictionary<string, string> Fees
        {
            get
            {
                if ( !this.Options.IncludeFees )
                {
                    return new Dictionary<string, string>();
                }

                var fees = RegistrationRegistrant.Fees.Where( a => a.Quantity > 0 ).Select( fee =>
                  {
                      string feeLabel;
                      if ( fee.RegistrationTemplateFee.FeeType == RegistrationFeeType.Multiple && fee.Option.IsNotNullOrWhiteSpace() )
                      {
                          feeLabel = $"{fee.RegistrationTemplateFee.Name}-{fee.Option}";
                      }
                      else
                      {
                          feeLabel = fee.Option;
                      }

                      string feeCostText;
                      if ( fee.Quantity > 1 )
                      {
                          feeCostText = $"{fee.Quantity} at {fee.Cost.FormatAsCurrency()}";
                      }
                      else
                      {
                          feeCostText = fee.TotalCost.FormatAsCurrency();
                      }

                      var result = new
                      {
                          FeeName = feeLabel,
                          TotalCostText = feeCostText
                      };

                      return result;
                  } ).ToDictionary( a => a.FeeName, v => v.TotalCostText );

                return fees;
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
        public int? RegistrantPersonDataViewFilterId { get; set; }

        /// <summary>
        /// Gets or sets the displayed attribute ids.
        /// </summary>
        /// <value>
        /// The displayed attribute ids.
        /// </value>
        public int[] DisplayedAttributeIds { get; set; } = new int[0];

        /// <summary>
        /// Gets or sets the block identifier.
        /// </summary>
        /// <value>
        /// The block identifier.
        /// </value>
        public int BlockId { get; set; }

        /// <summary>
        /// Gets or sets the filter fee identifier.
        /// </summary>
        /// <value>
        /// The filter fee identifier.
        /// </value>
        public int? FilterFeeId { get; set; }

        /// <summary>
        /// Gets the filter fee option ids.
        /// </summary>
        /// <value>
        /// The filter fee option ids.
        /// </value>
        public int[] FilterFeeOptionIds { get; set; } = new int[0];
    }
}