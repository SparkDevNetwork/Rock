//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Rock.CheckIn;
using Rock.Model;

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Finds family members in a given family
    /// </summary>
    [Description("Finds family members in a given family")]
    [Export(typeof(ActionComponent))]
    [ExportMetadata( "ComponentName", "Find Family Members" )]
    public class FindFamilyMembers : CheckInActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="action">The workflow action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override bool Execute( Model.WorkflowAction action, Data.IEntity entity, out List<string> errorMessages )
        {
            var checkInState = GetCheckInState( action, out errorMessages );
            if ( checkInState != null )
            {
                var family = checkInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault();
                if ( family != null )
                {
                    var service = new GroupMemberService();
                    foreach ( var groupMember in service.GetByGroupId( family.Group.Id ) )
                    {
                        var familyMember = family.FamilyMembers.Where( m => m.Person.Id == groupMember.PersonId).FirstOrDefault();
                        if (familyMember == null)
                        {
                            familyMember = new CheckInPerson();
                            familyMember.Person = new Person();
                            familyMember.Person.CopyPropertiesFrom(groupMember.Person);
                            family.FamilyMembers.Add(familyMember);
                        }
                    }

                    SetCheckInState( action, checkInState );
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