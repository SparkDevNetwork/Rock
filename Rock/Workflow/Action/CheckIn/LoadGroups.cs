//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Rock.CheckIn;
using Rock.Model;

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Loads the groups available for each loction
    /// </summary>
    [Description("Loads the groups available for each loction")]
    [Export(typeof(ActionComponent))]
    [ExportMetadata( "ComponentName", "Load Groups" )]
    public class LoadGroups : CheckInActionComponent
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
                                foreach ( var location in groupType.Locations.Where( l => l.Selected ) )
                                {
                                    var kioskLocation = kioskGroupType.KioskLocations.Where( l => l.Location.Id == location.Location.Id ).FirstOrDefault();
                                    if ( kioskLocation != null )
                                    {
                                        foreach ( var kioskGroup in kioskLocation.KioskGroups )
                                        {
                                            if ( !location.Groups.Any( g => g.Group.Id == kioskGroup.Group.Id ) )
                                            {
                                                var checkInGroup = new CheckInGroup();
                                                checkInGroup.Group = kioskGroup.Group.Clone( false );
                                                checkInGroup.Group.CopyAttributesFrom( kioskGroup.Group );
                                                location.Groups.Add( checkInGroup );
                                            }
                                        }
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