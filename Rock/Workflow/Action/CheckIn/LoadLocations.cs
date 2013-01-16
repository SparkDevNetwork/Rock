//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Rock;
using Rock.CheckIn;
using Rock.Model;

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Adds the locations for each members group types
    /// </summary>
    [Description("Adds the locations for each members group types")]
    [Export(typeof(ActionComponent))]
    [ExportMetadata( "ComponentName", "Load Locations" )]
    public class LoadLocations : CheckInActionComponent
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
                foreach ( var family in checkInState.CheckIn.Families.Where( f => f.Selected ) )
                {
                    foreach ( var person in family.People.Where( p => p.Selected ) )
                    {
                        foreach ( var groupType in person.GroupTypes.Where( g => g.Selected ) )
                        {
                            var kioskGroupType = checkInState.Kiosk.KioskGroupTypes.Where( g => g.GroupType.Id == groupType.GroupType.Id ).FirstOrDefault();
                            if ( kioskGroupType != null )
                            {
                                foreach ( var kioskLocation in kioskGroupType.KioskLocations )
                                {
                                    if ( !groupType.Locations.Any( l => l.Location.Id == kioskLocation.Location.Id ) )
                                    {
                                        var checkInLocation = new CheckInLocation();
                                        checkInLocation.Location = kioskLocation.Location.Clone(false);
                                        checkInLocation.Location.CopyAttributesFrom( kioskLocation.Location );
                                        groupType.Locations.Add(checkInLocation);
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