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
    /// Removes any group that does not have any schedules
    /// </summary>
    [Description("Removes any group that does not have any schedules")]
    [Export(typeof(ActionComponent))]
    [ExportMetadata( "ComponentName", "Remove Empty Groups" )]
    public class RemoveEmptyGroups : CheckInActionComponent
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
                foreach ( var family in checkInState.CheckIn.Families.ToList() )
                {
                    foreach ( var person in family.People.ToList() )
                    {
                        foreach ( var groupType in person.GroupTypes.ToList() )
                        {
                            foreach ( var location in groupType.Locations.ToList() )
                            {
                                foreach ( var group in location.Groups.ToList() )
                                {
                                    if ( group.Schedules.Count == 0 )
                                    {
                                        location.Groups.Remove( group );
                                    }
                                }
                            }
                        }
                    }
                }

                SetCheckInState( action, checkInState );
                return true;

            }

            return false;
        }
    }
}