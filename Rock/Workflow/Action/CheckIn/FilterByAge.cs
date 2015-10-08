// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Rock.Attribute;
using Rock.Data;

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Removes (or excludes) the grouptypes from each family member that are not specific to their age
    /// </summary>
    [Description( "Removes (or excludes) the grouptypes from each family member that are not specific to their age" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Filter By Age" )]

    [BooleanField( "Remove", "Select 'Yes' if group types should be be removed.  Select 'No' if they should just be marked as excluded.", true, "", 0 )]
    [BooleanField( "Age Required", "Select 'Yes' if groups with an age filter should be removed/excluded when person does not have an age.", true, "", 1 )]
    public class FilterByAge : CheckInActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The workflow action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override bool Execute( RockContext rockContext, Model.WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            var checkInState = GetCheckInState( entity, out errorMessages );
            if ( checkInState != null )
            {
                var family = checkInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault();
                if ( family != null )
                {
                    var remove = GetAttributeValue( action, "Remove" ).AsBoolean();

                    foreach ( var person in family.People )
                    {
                        double? age = person.Person.AgePrecise;
                        bool ageRequired = GetAttributeValue( action, "AgeRequired" ).AsBoolean( true );

                        if ( age == null && !ageRequired )
                        {
                            continue;
                        }

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
                                        if ( remove )
                                        {
                                            person.GroupTypes.Remove( groupType );
                                        }
                                        else
                                        {
                                            groupType.ExcludedByFilter = true;
                                        }
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
                                        if ( remove )
                                        {
                                            person.GroupTypes.Remove( groupType );
                                        }
                                        else
                                        {
                                            groupType.ExcludedByFilter = true;
                                        }
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