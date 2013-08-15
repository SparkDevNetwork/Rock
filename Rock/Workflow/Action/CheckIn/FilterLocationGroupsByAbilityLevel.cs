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
using Rock.Attribute;

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Removes the locations and groups for each selected family member
    /// if the person's ability level does not match the groups.
    /// </summary>
    [Description( "Removes the locations and groups for each selected family member if the person's ability level does not match the groups." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Filter Location Groups By Ability Level" )]
    public class FilterLocationGroupsByAbilityLevel : CheckInActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="action">The workflow action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override bool Execute( Model.WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            var checkInState = GetCheckInState( entity, out errorMessages );
            if ( checkInState == null )
            {
                return false;
            }

            var family = checkInState.CheckIn.Families.FirstOrDefault( f => f.Selected );
            if ( family != null )
            {
                foreach ( var person in family.People.Where( p => p.Selected && p.Person.LastName.Length > 0 ) )
                {
                    person.Person.LoadAttributes();
                    string personAbilityLevel = person.Person.GetAttributeValue( "AbilityLevel" );
                    if ( personAbilityLevel == null )
                    {
                        continue;
                    }

                    // Now dig down until we get the "group" because that's where the attribute is..
                    foreach ( var groupType in person.GroupTypes.Where( g => g.Selected ).ToList() )
                    {
                        foreach ( var group in groupType.Groups.ToList() )
                        {
                            // If the group has abilities but the person's ability is
                            // not in that list, then remove the group.
                            if ( ! group.Group.GetAttributeValues( "AbilityLevel" ).Any( a => a.Contains( personAbilityLevel ) ) )
                            {
                                groupType.Groups.Remove( group );
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}