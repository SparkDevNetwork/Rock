//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Rock.Attribute;
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
    [BooleanField( "Load All", "By default locations are only loaded for the selected person and group type.  Select this option to load locations for all the loaded people and group types." )]
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

                bool loadAll = false;
                if ( bool.TryParse( GetAttributeValue( action, "LoadAll" ), out loadAll ) && loadAll )
                {
                    loadAll = true;
                }

                foreach ( var family in checkInState.CheckIn.Families.Where( f => f.Selected ) )
                {
                    foreach ( var person in family.People.Where( p => p.Selected || loadAll ) )
                    {
                        foreach ( var groupType in person.GroupTypes.Where( t => t.Selected || loadAll ) )
                        {
                            var kioskGroupType = checkInState.Kiosk.FilteredGroupTypes( checkInState.ConfiguredGroupTypes ) .Where( g => g.GroupType.Id == groupType.GroupType.Id ).FirstOrDefault();
                            if ( kioskGroupType != null )
                            {
                                foreach ( var group in groupType.Groups.Where( g => g.Selected || loadAll ) )
                                {
                                    foreach ( var kioskGroup in kioskGroupType.KioskGroups )
                                    {
                                        foreach ( var kioskLocation in kioskGroup.KioskLocations )
                                        {
                                            if ( !group.Locations.Any( l => l.Location.Id == kioskLocation.Location.Id ) )
                                            {
                                                var checkInLocation = new CheckInLocation();
                                                checkInLocation.Location = kioskLocation.Location.Clone( false );
                                                checkInLocation.Location.CopyAttributesFrom( kioskLocation.Location );
                                                group.Locations.Add( checkInLocation );
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