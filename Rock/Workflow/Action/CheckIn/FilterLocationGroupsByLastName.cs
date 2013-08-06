//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Removes the locations and groups for each selected family member that
    /// are not specific to their last name.
    /// </summary>
    [Description( "Removes the locations and groups for each selected family member that are not specific to their last name." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Filter Location Groups By LastName" )]
    public class FilterLocationGroupsByLastName : CheckInActionComponent
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
            if ( checkInState == null )
            {
                return false;
            }

            var family = checkInState.CheckIn.Families.FirstOrDefault( f => f.Selected );
            if ( family != null )
            {
                foreach ( var person in family.People.Where( p => p.Selected && p.Person.LastName.Length > 0 ) )
                {
                    char lastInitial = char.Parse( person.Person.LastName.Substring( 0, 1 ).ToUpper() );

                    // Now dig down until we get the "group" because that's where the Lastname letter
                    // attributes are...
                    foreach ( var groupType in person.GroupTypes.Where( g => g.Selected ).ToList() )
                    {
                        foreach ( var group in groupType.Groups.ToList() )
                        {
                            string lastNameBeginLetterRange = group.Group.GetAttributeValue( "LastNameBeginLetterRange" ).Trim();
                            string lastNameEndLetterRange = group.Group.GetAttributeValue( "LastNameEndLetterRange" ).Trim();

                            char rangeStart = ( lastNameBeginLetterRange == "" ) ? 'A' : char.Parse( lastNameBeginLetterRange.ToUpper() );
                            char rangeEnd = ( lastNameEndLetterRange == "" ) ? 'Z' : char.Parse( lastNameEndLetterRange.ToUpper() );

                            // If the last name is not in range, remove the group
                            if ( !( lastInitial >= rangeStart && lastInitial <= rangeEnd ) )
                            {
                                groupType.Groups.Remove( group );
                            }
                        }
                    }
                }
            }

            SetCheckInState( action, checkInState );
            return true;
        }
    }
}