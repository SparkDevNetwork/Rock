// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace church.ccv.Utility
{
    /// <summary>
    /// Sends a birthday email
    /// </summary>
    [IntegerField( "Days Back","The number of days back to look for first-time visitors.", true, 5 )]
    [WorkflowTypeField("Workflow Type", "The workflow type to launch.", false, true)]
    [TextField("First Visit Attribute Key", "The person attribute key to use to find the first visit date.")]
    [GroupTypeField("Geofence Group Type", "Group type to use as a geo-fence (aka neighborhood).",true)]
    [IntegerField("Miniumum Head Of House Age", "The minimum age the head of house should be to create a workflow. This keeps from adding workflows when the family only has a child on the record.", true, 0)]
    [DisallowConcurrentExecution]
    public class CreateFirstVisitWorkflows : IJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public CreateFirstVisitWorkflows()
        {
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            var rockContext = new RockContext();

            GroupMemberService groupMemberService = new GroupMemberService( rockContext );

            JobDataMap dataMap = context.JobDetail.JobDataMap;

            int daysBack = dataMap.GetString( "DaysBack" ).AsInteger();
            string attributeKey = dataMap.GetString("FirstVisitAttributeKey");
            int minAgeHeadOfHouse = dataMap.GetString( "MiniumumHeadOfHouseAge" ).AsInteger();
            Guid geofenceGroupType = dataMap.GetString( "GeofenceGroupType" ).AsGuid();
            Guid workflowTypeGuid = dataMap.GetString("WorkflowType").AsGuid();

            if (workflowTypeGuid != null && workflowTypeGuid != Guid.Empty) {
            
                // get lookup values
                Guid personEntityGuid = new Guid(Rock.SystemGuid.EntityType.PERSON);
                Guid familyGroupTypeGuid = new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );
                Guid adultRoleGuid = new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT );
                Guid childRoleGuid = new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD );
                Guid homePhoneGuid = new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME );
                Guid mobilePhoneGuid = new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE );
                Guid homeAddressTypeGuid = new Guid( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME );

                // get workflow type id
                var workflowType = new WorkflowTypeService( rockContext ).Get( workflowTypeGuid );

                // calculate min date
                var backDate = RockDateTime.Now.AddDays(( daysBack * -1));

                // build query to get family groups (w/ members) where the first visit date is greater than the search date
                // and there is not a first-time visitor workflow already for anyone in the family
                
                // get attribute id
                var firstVisitAttributeId = new AttributeService( rockContext ).Queryable().AsNoTracking()
                                                .Where( a =>
                                                            a.EntityType.Guid == personEntityGuid &&
                                                            a.Key == attributeKey )
                                                .Select( a => a.Id ).FirstOrDefault();

                // get group type id for family groups
                var familyGroupTypeId = new GroupTypeService( rockContext ).Get( familyGroupTypeGuid ).Id;
                
                var visitorIds = new AttributeValueService( rockContext ).Queryable().AsNoTracking()
                                    .Where( a => 
                                        a.AttributeId == firstVisitAttributeId &&
                                        a.ValueAsDateTime >= backDate )
                                    .Select( a => a.EntityId );

                var workflowInitiators = new WorkflowService(rockContext).Queryable().AsNoTracking()
                                     .Where( w => w.WorkflowTypeId == workflowType.Id)
                                     .Select( w => w.InitiatorPersonAlias.Person.Id);


                var families = new GroupService( rockContext ).Queryable().AsNoTracking()
                                    .Where( g =>
                                            g.GroupTypeId == familyGroupTypeId &&
                                            g.Members.Any( m => visitorIds.Contains( m.PersonId ) ) &&
                                            !g.Members.Any( m => workflowInitiators.Contains( m.PersonId ) ) )
                                    .ToList();


                foreach ( var family in families )
                {
                    var headOfHouse = family.Members
                                            .OrderBy( m => m.GroupRole.Order )
                                            .ThenBy( m => m.Person.Gender  )
                                            .FirstOrDefault()
                                            .Person;

                    var headOfHouseAliasIds = headOfHouse.Aliases.Select( a => a.Id ).ToList();

                    // only create workflow is the head of house age is not known or it's greater than the min age
                    if ( !headOfHouse.Age.HasValue || headOfHouse.Age > minAgeHeadOfHouse )
                    {
                        // don't create a workflow if there is no contact info
                        int addressCount = family.GroupLocations.Count();
                        string homePhone = family.Members
                                                .OrderBy( m => m.GroupRole.Order )
                                                .ThenBy( m => m.Person.Gender )
                                                .SelectMany( m => m.Person.PhoneNumbers )
                                                .Where( p => p.NumberTypeValue.Guid == homePhoneGuid )
                                                .Select( p => p.NumberFormatted )
                                                .FirstOrDefault();
                        string mobilePhone = family.Members
                                                .OrderBy( m => m.GroupRole.Order )
                                                .ThenBy( m => m.Person.Gender )
                                                .SelectMany( m => m.Person.PhoneNumbers )
                                                .Where( p => p.NumberTypeValue.Guid == mobilePhoneGuid )
                                                .Select( p => p.NumberFormatted )
                                                .FirstOrDefault();

                        if ( addressCount > 0 || !string.IsNullOrWhiteSpace( homePhone ) || !string.IsNullOrWhiteSpace( mobilePhone ) )
                        {
                            // get first visit
                            var familyMemberPersonIds = family.Members.Select( m => m.PersonId ).ToList();
                            var firstVisitDates = new AttributeValueService( rockContext ).Queryable().AsNoTracking()
                                                    .Where( a =>
                                                        a.Attribute.Key == attributeKey &&
                                                        a.AttributeId == firstVisitAttributeId &&
                                                        familyMemberPersonIds.Contains( a.EntityId.Value ) )
                                                    .OrderByDescending( a => a.ValueAsDateTime )
                                                    .ToList();

                            // don't create a workflow if anyone in the family has an earlier first visit
                            if ( !( firstVisitDates.OrderBy( d => d.ValueAsDateTime ).Select( d => d.ValueAsDateTime ).First() < backDate ) )
                            {
                                var adults = family.Members
                                                .Where( m => m.GroupRole.Guid == adultRoleGuid );

                                var children = family.Members
                                                .Where( m => m.GroupRole.Guid == childRoleGuid );

                                // create new workflow
                                var visitorWorkflow = Rock.Model.Workflow.Activate( workflowType, family.Name );
                                visitorWorkflow.InitiatorPersonAliasId = headOfHouse.PrimaryAliasId;

                                // set attribute values
                                visitorWorkflow.SetAttributeValue( "HeadOfHouse", headOfHouse.PrimaryAlias.Guid.ToString() );
                                visitorWorkflow.SetAttributeValue( "Family", family.ToString() );
                                visitorWorkflow.SetAttributeValue( "FirstVisitDate", firstVisitDates.OrderByDescending( d => d.ValueAsDateTime ).Select( d => d.ValueAsDateTime ).FirstOrDefault().ToString() );
                                visitorWorkflow.SetAttributeValue( "Campus", family.Campus.Name );
                                visitorWorkflow.SetAttributeValue( "Children", BuildPersonList( children.Select( m => m.Person ).ToList() ) );
                                visitorWorkflow.SetAttributeValue( "Adults", BuildPersonList( adults.Select( m => m.Person ).ToList() ) );
                                visitorWorkflow.SetAttributeValue( "HomePhone", homePhone );
                                visitorWorkflow.SetAttributeValue( "MobilePhone", mobilePhone );

                                var homeAddressGuid = family.GroupLocations
                                        .Where( l => l.Location != null && l.Location.LocationTypeValue != null && l.Location.LocationTypeValue.Guid == homeAddressTypeGuid )
                                        .Select( l => l.Location.Guid )
                                        .FirstOrDefault();

                                if ( homeAddressGuid != null )
                                {
                                    visitorWorkflow.SetAttributeValue( "HomeAddress", homeAddressGuid.ToString() );
                                }

                                // get neighborhood info
                                if ( geofenceGroupType != Guid.Empty )
                                {
                                    var neighborhoods = new GroupService( rockContext ).GetGeofencingGroups( headOfHouse.Id, geofenceGroupType ).AsNoTracking();

                                    if ( neighborhoods != null && neighborhoods.Count() > 0 )
                                    {
                                        var neighborhood = neighborhoods.FirstOrDefault();
                                        visitorWorkflow.SetAttributeValue( "Neighborhood", neighborhood.Name );

                                        var neighborhoodPastor = groupMemberService.Queryable().AsNoTracking()
                                                        .Where( m =>
                                                            m.GroupId == neighborhood.Id &&
                                                            m.GroupRole.IsLeader )
                                                        .Select( m => m.Person ).FirstOrDefault();

                                        visitorWorkflow.SetAttributeValue( "NeighborhoodPastor", neighborhoodPastor.PrimaryAlias.Guid.ToString() );
                                    }
                                }

                                var workflowService = new Rock.Model.WorkflowService( rockContext );
                                workflowService.Add( visitorWorkflow );

                                rockContext.WrapTransaction( () =>
                                {
                                    rockContext.SaveChanges();
                                    visitorWorkflow.SaveAttributeValues( rockContext );
                                } );
                            }
                        }
                    }
                    
                }
            }
            
        }

        private string BuildPersonList( List<Person> people )
        {
            StringBuilder output = new StringBuilder();
            if ( people.Count() > 0 )
            {
                output.Append( "<ul>" );
                foreach ( var person in people )
                {                    
                    string age = string.Empty;

                    if ( person.Age != null )
                    {
                        age = string.Format( "Age: {0}", person.Age.ToString() );
                    }

                    output.Append( string.Format( "<li>{0} <small>{1}</small></li>", person.FullName, age ) );
                }
                output.Append( "</ul>" );
            }

            return output.ToString();
        }
    }
}
