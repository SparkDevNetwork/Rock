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
    /// Removes the grouptypes from each family member that are not specific to their age
    /// </summary>
    [Description( "Removes the grouptypes from each family member that are not specific to their age" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Filter By Age" )]
    public class FilterByAge : CheckInActionComponent
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
                    foreach ( var person in family.People )
                    {
                        double? age = person.Person.AgePrecise;
                        if ( age == null )
                        {
                            continue;
                        }

                        foreach ( var groupType in person.GroupTypes.ToList() )
                        {
                            // Now dig down until we get the "group" because that's where the attribute is..
                            foreach ( var location in groupType.Locations.ToList() )
                            {
                                foreach ( var group in location.Groups.ToList() )
                                {
                                    string minAgeValue = group.Group.GetAttributeValue( "MinAge" );
                                    if ( minAgeValue != null )
                                    {
                                        double minAge = 0;
                                        if ( double.TryParse( minAgeValue, out minAge ) )
                                        {
                                            if ( !age.HasValue || age < minAge )
                                            {
                                                location.Groups.Remove( group );
                                            }
                                        }
                                    }

                                    string maxAgeValue = group.Group.GetAttributeValue( "MaxAge" );
                                    if ( maxAgeValue != null )
                                    {
                                        double maxAge = 0;
                                        if ( double.TryParse( maxAgeValue, out maxAge ) )
                                        {
                                            if ( !age.HasValue || age > maxAge )
                                            {
                                                location.Groups.Remove( group );
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