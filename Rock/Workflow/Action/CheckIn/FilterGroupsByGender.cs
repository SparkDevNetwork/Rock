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
using Rock.Model;

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Removes the grouptypes from each family member that are not specific to their grade
    /// </summary>
    [Description( "Removes the groups from each family member that are not specific to gender." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Filter Groups By Gender" )]
    public class FilterGroupsByGender : CheckInActionComponent
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
            if ( checkInState != null )
            {
                var family = checkInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault();
                if ( family != null )
                {
                    foreach ( var person in family.People.Where( p => p.Selected ) )
                    {
                        string personsGender = person.Person.Gender.ToString( "d" );

                        foreach ( var groupType in person.GroupTypes.ToList() )
                        {
                            foreach ( var group in groupType.Groups.ToList() )
                            {
                                var groupAttributes = group.Group.GetAttributeValues( "Gender" );
                                if ( groupAttributes.Any() && !groupAttributes.Contains( personsGender ) )
                                {
                                    groupType.Groups.Remove( group );
                                }
                            }
                        }
                        
                    }
                }

                return true;
            }

            return false;
        }
    }
}