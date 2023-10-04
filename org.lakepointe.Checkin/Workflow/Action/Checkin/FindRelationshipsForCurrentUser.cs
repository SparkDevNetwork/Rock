using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Caching;

using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow;
using Rock.Workflow.Action.CheckIn;

namespace org.lakepointe.Checkin.Workflow.Action.Checkin
{

    [ActionCategory("LPC Check-In")]
    [Description("Finds people with a relationship to the current user, if current user is unknown, will look for relationships for current family.")]
    [Export( typeof(ActionComponent))]
    [ExportMetadata("ComponentName", "Find Relationships for Current User")]
    public class FindRelationshipsForCurrentUser : CheckInActionComponent
    {
        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {
            string cacheKey = "Rock.FindRelationships.Roles";

            List<int> roles = RockCache.Get( cacheKey ) as List<int>;

            if ( roles == null )
            {
                roles = new List<int>();

                foreach ( var role in new GroupTypeRoleService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( r => r.GroupType.Guid.Equals( new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS ) ) ) )
                {
                    role.LoadAttributes( rockContext );
                    if ( role.Attributes.ContainsKey( "CanCheckin" ) )
                    {
                        bool canCheckIn = false;
                        if ( bool.TryParse( role.GetAttributeValue( "CanCheckin" ), out canCheckIn ) && canCheckIn )
                        {
                            roles.Add( role.Id );
                        }
                    }
                }

                RockCache.AddOrUpdate( cacheKey, null, roles, RockDateTime.Now.AddSeconds( 300 ) );
            }

            var checkInState = GetCheckInState( entity, out errorMessages );
            if ( checkInState != null )
            {
                if ( !roles.Any() )
                {
                    return true;
                }

                var family = checkInState.CheckIn.CurrentFamily;
                if ( family != null )
                {
                    bool preventInactive = ( checkInState.CheckInType != null && checkInState.CheckInType.PreventInactivePeople );
                    var dvInactive = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() );

                    var groupMemberService = new GroupMemberService( rockContext );

                    var familyMemberIds = new List<int>();

                    if ( checkInState.CheckIn.CheckedInByPersonAliasId.HasValue )
                    {
                        //If we know who is Checking in, only include them in the list of
                        //family members to retrieve relationships for.
                        var checkedInByPerson = new PersonAliasService( rockContext ).GetPerson( checkInState.CheckIn.CheckedInByPersonAliasId.Value );

                        if ( checkedInByPerson != null && checkedInByPerson.Id > 0 )
                        {
                            familyMemberIds.Add( checkedInByPerson.Id );
                        }
                    }
                    else
                    {
                        //If we don't know who is checking in, searched based on the
                        //entire family
                        familyMemberIds.AddRange( family.People.Select( p => p.Person.Id ).ToList() );
                    }

                    var knownRelationshipGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid() );
                    if ( knownRelationshipGroupType != null )
                    {
                        var ownerRole = knownRelationshipGroupType.Roles.FirstOrDefault( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid() );
                        if ( ownerRole != null )
                        {
                            // Get the Known Relationship group id's for each person in the family
                            var relationshipGroupIds = groupMemberService
                                .Queryable().AsNoTracking()
                                .Where( g =>
                                    g.GroupRoleId == ownerRole.Id &&
                                    familyMemberIds.Contains( g.PersonId ) )
                                .Select( g => g.GroupId );

                            // Get anyone in any of those groups that has a role with the canCheckIn attribute set
                            var personIds = groupMemberService
                                .Queryable().AsNoTracking()
                                .Where( g =>
                                    relationshipGroupIds.Contains( g.GroupId ) &&
                                    roles.Contains( g.GroupRoleId ) )
                                .Select( g => g.PersonId )
                                .ToList();

                            foreach ( var person in new PersonService( rockContext )
                                .Queryable().AsNoTracking()
                                .Where( p => personIds.Contains( p.Id ) )
                                .ToList() )
                            {
                                if ( !family.People.Any( p => p.Person.Id == person.Id ) )
                                {
                                    if ( !preventInactive || dvInactive == null || person.RecordStatusValueId != dvInactive.Id )
                                    {
                                        var relatedPerson = new CheckInPerson();
                                        relatedPerson.Person = person.Clone( false );
                                        relatedPerson.FamilyMember = false;
                                        family.People.Add( relatedPerson );
                                    }
                                }
                            }
                        }
                    }

                    return true;
                }
                else
                {
                    errorMessages.Add( "There is not a family that is selected" );
                }

                return false;
            }

            return false;
        }
    }
}
