using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;
using Rock;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow;
using Rock.Workflow.Action.CheckIn;


namespace org.lakepointe.Checkin.Workflow.Action.Checkin
{
    [ActionCategory( "LPC Check-In" )]
    [Description( "Finds current person in family if known, otherwise finds family members for selected family. " )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Find Current User or Family Members" )]
    public class FindFamilyMembersOrCurrentPerson : CheckInActionComponent
    {
        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {
            var checkInState = GetCheckInState( entity, out errorMessages );
            if ( checkInState != null )
            {
                var family = checkInState.CheckIn.CurrentFamily;
                if ( family != null )
                {
                    var service = new GroupMemberService( rockContext );

                    var people = service.GetByGroupId( family.Group.Id ).AsNoTracking();

                    if(checkInState.CheckIn.CheckedInByPersonAliasId.HasValue)
                    {
                        var checkedInByPerson = new PersonAliasService( rockContext ).GetPerson( checkInState.CheckIn.CheckedInByPersonAliasId.Value );
                        if ( checkedInByPerson != null )
                        {
                            people = people.Where( m => m.PersonId == checkedInByPerson.Id );
                        }
                    }

                    if ( checkInState.CheckInType != null && checkInState.CheckInType.PreventInactivePeople )
                    {
                        var dvInactive = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() );
                        if ( dvInactive != null )
                        {
                            people = people.Where( m => m.Person.RecordStatusValueId != dvInactive.Id );
                        }
                    }

                    foreach ( var groupMember in people.ToList() )
                    {
                        if ( !family.People.Any( p => p.Person.Id == groupMember.PersonId ) )
                        {
                            var person = new CheckInPerson();
                            person.Person = groupMember.Person.Clone( false );
                            person.FamilyMember = true;
                            family.People.Add( person );
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
