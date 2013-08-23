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
        public override bool Execute( Model.WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            var checkInState = GetCheckInState( entity, out errorMessages );
            if ( checkInState != null )
            {
                var family = checkInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault();
                if ( family != null )
                {
                    foreach ( var person in family.People )
                    {
                        double? age = person.Person.AgePrecise;

                        foreach ( var groupType in person.GroupTypes.ToList() )
                        {
                            string ageRange = groupType.GroupType.GetAttributeValue( "AgeRange" ) ?? string.Empty;
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
                                        person.GroupTypes.Remove( groupType );
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
                                        person.GroupTypes.Remove( groupType );
                                        continue;
                                    }
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