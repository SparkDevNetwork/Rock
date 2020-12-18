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
using Rock.Web.Cache;

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Removes (or excludes) the groups for each selected family member that are not specific to their grade.
    /// </summary>
    [ActionCategory( "Check-In" )]
    [Description( "Removes (or excludes) the groups for each selected family member that are not specific to their grade and age." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Filter Groups By Grade and Age" )]

    [BooleanField( "Remove", "Select 'Yes' if groups should be removed.  Select 'No' if they should just be marked as excluded.", true , "", 0)]
    [BooleanField( "Prioritize Grade", "Exclude groups that do not match by grade if one (or more) groups are found that do match by grade.", false, "", 1 )]
    [AttributeField( Rock.SystemGuid.EntityType.GROUP, "Group Age Range Attribute", "Select the attribute used to define the age range of the group", true, false,
        Rock.SystemGuid.Attribute.GROUP_AGE_RANGE, order: 2 )]
    [AttributeField( Rock.SystemGuid.EntityType.GROUP, "Group Birthdate Range Attribute", "Select the attribute used to define the birthdate range of the group", true, false,
        Rock.SystemGuid.Attribute.GROUP_BIRTHDATE_RANGE, order: 3 )]
    public class FilterGroupsByGradeAndAge : CheckInActionComponent
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
        public override bool Execute( RockContext rockContext, Model.WorkflowAction action, object entity, out List<string> errorMessages )
        {
            var checkInState = GetCheckInState( entity, out errorMessages );
            if ( checkInState == null )
            {
                return false;
            }

            var family = checkInState.CheckIn.CurrentFamily;
            if ( family != null )
            {
                var remove = GetAttributeValue( action, "Remove" ).AsBoolean();
                bool gradeRequired = checkInState.CheckInType == null || checkInState.CheckInType.GradeRequired;
                bool ageRequired = checkInState.CheckInType == null || checkInState.CheckInType.AgeRequired;

                // get the Age Range
                var ageRangeAttributeKey = string.Empty;
                var ageRangeAttributeGuid = GetAttributeValue( action, "GroupAgeRangeAttribute" ).AsGuidOrNull();
                if ( ageRangeAttributeGuid.HasValue )
                {
                    var attribute = AttributeCache.Get( ageRangeAttributeGuid.Value, rockContext );
                    if ( attribute != null )
                    {
                        ageRangeAttributeKey = attribute.Key;
                    }
                }

                // get the admin-selected attribute key instead of using a hardcoded key
                var birthdateRangeAttributeKey = string.Empty;
                var birthdateRangeAttributeGuid = GetAttributeValue( action, "GroupBirthdateRangeAttribute" ).AsGuidOrNull();
                if ( birthdateRangeAttributeGuid.HasValue )
                {
                    var attribute = AttributeCache.Get( birthdateRangeAttributeGuid.Value, rockContext );
                    if ( attribute != null )
                    {
                        birthdateRangeAttributeKey = attribute.Key;
                    }
                }

                var personGradeMatches = new Dictionary<int, List<int>>();

                foreach ( var person in family.People )
                {
                    personGradeMatches.Add( person.Person.Id, new List<int>() );

                    int? gradeOffset = person.Person.GradeOffset;
                    var ageAsDouble = person.Person.AgePrecise;
                    decimal? age = ageAsDouble.HasValue ? Convert.ToDecimal( ageAsDouble.Value ) : (decimal?)null;
                    DateTime? birthdate = person.Person.BirthDate;

                    foreach ( var groupType in person.GroupTypes.ToList() )
                    {
                        foreach ( var group in groupType.Groups.ToList() )
                        {
                            bool? isMatch = null;

                            // First check to see 
                            string gradeOffsetRange = group.Group.GetAttributeValue( "GradeRange" ) ?? string.Empty;
                            var gradeOffsetRangePair = gradeOffsetRange.Split( new char[] { ',' }, StringSplitOptions.None ).AsGuidOrNullList().ToArray();
                            DefinedValueCache minGradeDefinedValue = null;
                            DefinedValueCache maxGradeDefinedValue = null;
                            if ( gradeOffsetRangePair.Length == 2 )
                            {
                                minGradeDefinedValue = gradeOffsetRangePair[0].HasValue ? DefinedValueCache.Get( gradeOffsetRangePair[0].Value ) : null;
                                maxGradeDefinedValue = gradeOffsetRangePair[1].HasValue ? DefinedValueCache.Get( gradeOffsetRangePair[1].Value ) : null;
                            }
                            if ( maxGradeDefinedValue != null || minGradeDefinedValue != null )
                            {
                                if ( gradeOffset.HasValue )
                                {
                                    // if the group type specifies a min grade (max gradeOffset)...
                                    if ( maxGradeDefinedValue != null )
                                    {
                                        // NOTE: minGradeOffset is actually based on the MAX Grade since GradeOffset's are Years Until Graduation
                                        int? minGradeOffset = maxGradeDefinedValue.Value.AsIntegerOrNull();
                                        if ( minGradeOffset.HasValue && gradeOffset.Value < minGradeOffset.Value )
                                        {
                                            isMatch = false;
                                        }
                                    }

                                    // if the group type specifies a max grade (min gradeOffset)...
                                    if ( minGradeDefinedValue != null )
                                    {
                                        // NOTE: maxGradeOffset is actually based on the MIN Grade since GradeOffset's are Years Until Graduation
                                        int? maxGradeOffset = minGradeDefinedValue.Value.AsIntegerOrNull();
                                        if ( maxGradeOffset.HasValue && gradeOffset.Value > maxGradeOffset.Value )
                                        {
                                            isMatch = false;
                                        }
                                    }

                                    // If the person has a grade, and the group has a matching grade range and it wasn't excluded, then assume a match
                                    // and don't bother checking age.
                                    if ( !isMatch.HasValue )
                                    {
                                        isMatch = true;
                                    }
                                }
                                else
                                {
                                    if ( gradeRequired )
                                    {
                                        isMatch = false;
                                    }
                                }
                            }

                            // If group was not included or excluded based on grade, then check the age.
                            if ( !isMatch.HasValue )
                            {
                                bool? ageMatch = null;
                                bool? birthdayMatch = null;

                                var ageRange = group.Group.GetAttributeValue( ageRangeAttributeKey ).ToStringSafe();

                                var ageRangePair = ageRange.Split( new char[] { ',' }, StringSplitOptions.None );
                                decimal? minAge = null;
                                decimal? maxAge = null;

                                if ( ageRangePair.Length == 2 )
                                {
                                    minAge = ageRangePair[0].AsDecimalOrNull();
                                    maxAge = ageRangePair[1].AsDecimalOrNull();
                                }

                                if ( minAge.HasValue || maxAge.HasValue )
                                {
                                    if ( age.HasValue )
                                    {
                                        if ( minAge.HasValue )
                                        {
                                            int groupMinAgePrecision = minAge.Value.GetDecimalPrecision();
                                            decimal? personAgePrecise = age.Floor( groupMinAgePrecision );
                                            if ( personAgePrecise < minAge )
                                            {
                                                ageMatch = false;
                                            }
                                        }

                                        if ( maxAge.HasValue )
                                        {
                                            int groupMaxAgePrecision = maxAge.Value.GetDecimalPrecision();
                                            decimal? personAgePrecise = age.Floor( groupMaxAgePrecision );
                                            if ( personAgePrecise > maxAge )
                                            {
                                                ageMatch = false;
                                            }
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

                                // If group was not included or excluded based on grade and age did not match, then check the birthdate.
                                if ( !ageMatch.HasValue || !ageMatch.Value )
                                {
                                    var birthdateRange = group.Group.GetAttributeValue( birthdateRangeAttributeKey ).ToStringSafe();

                                    var birthdateRangePair = birthdateRange.Split( new char[] { ',' }, StringSplitOptions.None );
                                    DateTime? minBirthdate = null;
                                    DateTime? maxBirthdate = null;

                                    if ( birthdateRangePair.Length == 2 )
                                    {
                                        minBirthdate = birthdateRangePair[0].AsDateTime();
                                        maxBirthdate = birthdateRangePair[1].AsDateTime();
                                    }

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

                                if ( ageMatch.HasValue || birthdayMatch.HasValue )
                                {
                                    if ( !( ( ageMatch ?? false ) || ( birthdayMatch ?? false ) ) )
                                    {
                                        isMatch = false;
                                    }
                                    else
                                    {
                                        isMatch = true;
                                    }
                                }
                            }
                            else
                            {
                                if ( isMatch.Value )
                                {
                                    // If the group was matched on grade, add it to the list of matched groups for the person
                                    personGradeMatches[person.Person.Id].Add( group.Group.Id );
                                }
                            }

                            if ( isMatch.HasValue && !isMatch.Value )
                            {
                                if ( remove )
                                {
                                    groupType.Groups.Remove( group );
                                }
                                else
                                {
                                    group.ExcludedByFilter = true;
                                }
                            }
                        }
                    }
                }

                // If grade is being prioritized, and there was at least one group matched by grade, remove all
                // the groups that did not match by grade
                if ( GetAttributeValue( action, "PrioritizeGrade" ).AsBoolean() )
                {
                    foreach ( var person in family.People )
                    {
                        // Check if person matched any group by grade
                        if ( personGradeMatches[person.Person.Id].Any() )
                        {
                            // Check every group to see if it was matched by grade...
                            foreach ( var groupType in person.GroupTypes.ToList() )
                            {
                                foreach ( var group in groupType.Groups.ToList() )
                                {
                                    if ( !personGradeMatches[person.Person.Id].Contains( group.Group.Id ) )
                                    {
                                        // ...if not, remove it
                                        if ( remove )
                                        {
                                            groupType.Groups.Remove( group );
                                        }
                                        else
                                        {
                                            group.ExcludedByFilter = true;
                                        }
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