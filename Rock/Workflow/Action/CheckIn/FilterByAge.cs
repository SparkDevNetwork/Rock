// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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
    [ActionCategory( "Check-In" )]
    [Description( "Removes (or excludes) the grouptypes from each family member that are not specific to their age" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Filter By Age" )]

    [BooleanField( "Remove", "Select 'Yes' if group types should be be removed.  Select 'No' if they should just be marked as excluded.", true, "", 0 )]
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
                var family = checkInState.CheckIn.CurrentFamily;
                if ( family != null )
                {
                    var remove = GetAttributeValue( action, "Remove" ).AsBoolean();
                    bool ageRequired = checkInState.CheckInType == null || checkInState.CheckInType.AgeRequired;

                    foreach ( var person in family.People )
                    {
                        var ageAsDouble = person.Person.AgePrecise;
                        decimal? age = ageAsDouble.HasValue ? Convert.ToDecimal( ageAsDouble.Value ) : (decimal?)null;
                        DateTime? birthdate = person.Person.BirthDate;

                        foreach ( var groupType in person.GroupTypes.ToList() )
                        {
                            bool? ageMatch = null;
                            bool? birthdayMatch = null;

                            string ageRange = groupType.GroupType.GetAttributeValue( "AgeRange" ) ?? string.Empty;
                            string[] ageRangePair = ageRange.Split( new char[] { ',' }, StringSplitOptions.None );
                            string minAgeValue = null;
                            string maxAgeValue = null;
                            if ( ageRangePair.Length == 2 )
                            {
                                minAgeValue = ageRangePair[0];
                                maxAgeValue = ageRangePair[1];
                            }

                            decimal? minAge = minAgeValue.AsDecimalOrNull();
                            decimal? maxAge = maxAgeValue.AsDecimalOrNull();

                            if ( minAge.HasValue || maxAge.HasValue )
                            {
                                if ( age.HasValue )
                                {
                                    if ( minAge.HasValue && age.Value < minAge.Value )
                                    {
                                        ageMatch = false;
                                    }

                                    if ( maxAge.HasValue && age.Value > maxAge.Value )
                                    {
                                        ageMatch = false;
                                    }

                                    if ( !ageMatch.HasValue )
                                    {
                                        ageMatch = true;
                                    }
                                }
                                else
                                {
                                    if ( ageRequired )
                                    {
                                        ageMatch = false;
                                    }
                                }
                            }

                            if ( !ageMatch.HasValue || !ageMatch.Value )
                            {
                                var birthdateRange = groupType.GroupType.GetAttributeValue( "BirthdateRange" ) ?? string.Empty;
                                var birthdateRangePair = birthdateRange.Split( new char[] { ',' }, StringSplitOptions.None );
                                string minBirthdateValue = null;
                                string maxBirthdateValue = null;

                                if ( birthdateRangePair.Length == 2 )
                                {
                                    minBirthdateValue = birthdateRangePair[0];
                                    maxBirthdateValue = birthdateRangePair[1];
                                }

                                DateTime? minBirthdate = minBirthdateValue.AsDateTime();
                                DateTime? maxBirthdate = maxBirthdateValue.AsDateTime();
                                if ( minBirthdate.HasValue || maxBirthdate.HasValue )
                                {
                                    if ( birthdate.HasValue )
                                    {
                                        if ( minBirthdate.HasValue && birthdate.Value < minBirthdate.Value )
                                        {
                                            birthdayMatch = false;
                                        }

                                        if ( maxBirthdate.HasValue && birthdate.Value > maxBirthdate.Value )
                                        {
                                            birthdayMatch = false;
                                        }

                                        if ( !birthdayMatch.HasValue )
                                        {
                                            birthdayMatch = true;
                                        }
                                    }
                                    else
                                    {
                                        if ( ageRequired )
                                        {
                                            birthdayMatch = false;
                                        }
                                    }
                                }
                            }

                            if ( ( ageMatch.HasValue || birthdayMatch.HasValue ) && !( ( ageMatch ?? false ) || ( birthdayMatch ?? false ) ) )
                            {
                                if ( remove )
                                {
                                    person.GroupTypes.Remove( groupType );
                                }
                                else
                                {
                                    groupType.ExcludedByFilter = true;
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

