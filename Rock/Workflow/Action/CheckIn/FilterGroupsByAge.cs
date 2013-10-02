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
    /// Removes the groups for each selected family member that are not specific to their age.
    /// </summary>
    [Description( "Removes the groups for each selected family member that are not specific to their age." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Filter Groups By Age" )]
    public class FilterGroupsByAge : CheckInActionComponent
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
                foreach ( var person in family.People )
                {
                    double? age = person.Person.AgePrecise;

                    if ( age == null )
                    {
                        continue;
                    }

                    foreach ( var groupType in person.GroupTypes.ToList() )
                    {
                        foreach ( var group in groupType.Groups.ToList() )
                        {
                            string ageRange = group.Group.GetAttributeValue( "AgeRange" ) ?? string.Empty;

                            string[] ageRangePair = ageRange.Split( new char[] { ',' }, StringSplitOptions.None );
                            string minAgeValue = null;
                            string maxAgeValue = null;
                            if ( ageRangePair.Length == 2 )
                            {
                                minAgeValue = ageRangePair[0];
                                maxAgeValue = ageRangePair[1];
                            }

                            if ( minAgeValue != null )
                            {
                                double minAge = 0;
                                if ( double.TryParse( minAgeValue, out minAge ) )
                                {
                                    if ( !age.HasValue || age < minAge )
                                    {
                                        groupType.Groups.Remove( group );
                                        continue;
                                    }
                                }
                            }

                            if ( maxAgeValue != null )
                            {
                                double maxAge = 0;
                                if ( double.TryParse( maxAgeValue, out maxAge ) )
                                {
                                    if ( !age.HasValue || age > maxAge )
                                    {
                                        groupType.Groups.Remove( group );
                                        continue;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}