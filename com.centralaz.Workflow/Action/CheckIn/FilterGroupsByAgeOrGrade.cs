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

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow;
using Rock.Workflow.Action.CheckIn;

namespace com.centralaz.Workflow.Action.CheckIn
{
    /// <summary>
    /// Removes (or excludes) the groups for each selected family member that are not specific to their age or group.
    /// At least one must match otherwise the group will be removed.
    /// </summary>
    [ActionCategory( "com_centralaz: Check-In" )]
    [Description( "Removes (or excludes) the groups for each selected family member that are not specific to their age or grade. At least one of them must match otherwise the group will be removed." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Filter Groups By Age or Grade" )]

    [BooleanField( "Remove", "Select 'Yes' if groups should be be removed.  Select 'No' if they should just be marked as excluded.", true, "", 0 )]
    [AttributeField( Rock.SystemGuid.EntityType.GROUP, "Group Age Range Attribute", "Select the attribute used to define the age range of the group", true, false, "43511B8F-71D9-423A-85BF-D1CD08C1998E", order: 2 )]
    public class FilterGroupsByAgeOrGrade : CheckInActionComponent
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
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
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

                // get the admin-selected attribute key instead of using a hardcoded key
                var ageRangeAttributeKey = string.Empty;
                var ageRangeAttributeGuid = GetAttributeValue( action, "GroupAgeRangeAttribute" ).AsGuid();
                if ( ageRangeAttributeGuid != Guid.Empty )
                {
                    ageRangeAttributeKey = AttributeCache.Read( ageRangeAttributeGuid, rockContext ).Key;
                }

                // log a warning if the attribute is missing or invalid
                if ( string.IsNullOrWhiteSpace( ageRangeAttributeKey ) )
                {
                    action.AddLogEntry( string.Format( "The Group Age Range attribute is not selected or invalid for '{0}'.", action.ActionType.Name ) );
                }

                foreach ( var person in family.People )
                {
                    var ageAsDouble = person.Person.AgePrecise;
                    decimal? age = null;
                    int? personsGradeOffset = person.Person.GradeOffset;

                    if ( ageAsDouble.HasValue )
                    {
                        age = Convert.ToDecimal( ageAsDouble.Value );
                    }

                    foreach ( var groupType in person.GroupTypes.ToList() )
                    {
                        foreach ( var group in groupType.Groups.ToList() )
                        {
                            if ( !IsMatchAgeOrGrade( group, age, personsGradeOffset, ageRangeAttributeKey ) )
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
            }

            return true;
        }

        /// <summary>
        /// Determines whether the given group's age or grade matches the given person's age or person grade offset.
        /// This method short-circuits with true as soon as one matches.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="age">The age.</param>
        /// <param name="personsGradeOffset">The persons grade offset.</param>
        /// <param name="ageRangeAttributeKey">The age range attribute key.</param>
        /// <returns></returns>
        private bool IsMatchAgeOrGrade( Rock.CheckIn.CheckInGroup group, decimal? age, int? personsGradeOffset, string ageRangeAttributeKey )
        {
            var ageRange = group.Group.GetAttributeValue( ageRangeAttributeKey ).ToStringSafe();
            var ageRangePair = ageRange.Split( new char[] { ',' }, StringSplitOptions.None );
            string minAgeValue = null;
            string maxAgeValue = null;

            if ( ageRangePair.Length == 2 )
            {
                minAgeValue = ageRangePair[0];
                maxAgeValue = ageRangePair[1];
            }

            // Check group min and max age
            if ( minAgeValue != null && maxAgeValue != null )
            {
                decimal minAge = 0;
                decimal maxAge = 0;

                if ( decimal.TryParse( minAgeValue, out minAge ) )
                {
                    decimal? personAgePrecise = null;

                    if ( age.HasValue )
                    {
                        int groupMinAgePrecision = minAge.GetDecimalPrecision();
                        personAgePrecise = age.Floor( groupMinAgePrecision );
                        if ( personAgePrecise >= minAge && personAgePrecise <= maxAge )
                        {
                            return true;
                        }
                    }
                }
            }

            // Check group min and max grade
            string gradeOffsetRange = group.Group.GetAttributeValue( "GradeRange" ) ?? string.Empty;
            var gradeOffsetRangePair = gradeOffsetRange.Split( new char[] { ',' }, StringSplitOptions.None ).AsGuidOrNullList().ToArray();
            DefinedValueCache minGradeDefinedValue = null;
            DefinedValueCache maxGradeDefinedValue = null;
            if ( gradeOffsetRangePair.Length == 2 )
            {
                minGradeDefinedValue = gradeOffsetRangePair[0].HasValue ? DefinedValueCache.Read( gradeOffsetRangePair[0].Value ) : null;
                maxGradeDefinedValue = gradeOffsetRangePair[1].HasValue ? DefinedValueCache.Read( gradeOffsetRangePair[1].Value ) : null;
            }

            /*
             * example (assuming defined values are the stock values):
             * minGrade,maxGrade of between 4th and 6th grade
             * 4th grade is 8 years until graduation
             * 6th grade is 6 years until graduation
             * GradeOffsetRange would be 8 and 6
             * if person is in:
             *      7th grade or older (gradeOffset 5 or smaller), they would be NOT included
             *      6th grade (gradeOffset 6), they would be included
             *      5th grade (gradeOffset 7), they would be included
             *      4th grade (gradeOffset 8), they would be included
             *      3th grade or younger (gradeOffset 9 or bigger), they would be NOT included
             *      NULL grade, not included
             */

            // if the group type specifies a min grade (max gradeOffset)...
            if ( maxGradeDefinedValue != null && minGradeDefinedValue != null )
            {
                // NOTE: minGradeOffset is actually based on the MAX Grade since GradeOffset's are Years Until Graduation
                int? minGradeOffset = maxGradeDefinedValue.Value.AsIntegerOrNull();

                // NOTE: maxGradeOffset is actually based on the MIN Grade since GradeOffset's are Years Until Graduation
                int? maxGradeOffset = minGradeDefinedValue.Value.AsIntegerOrNull();

                if ( minGradeOffset.HasValue && maxGradeOffset.HasValue )
                {
                    // remove if the person does not have a grade or if their grade offset is more than the max offset (too young)
                    // example person is in 3rd grade (offset 9) and range is 4th to 6th (offset 6 to 8)
                    if ( personsGradeOffset.HasValue && personsGradeOffset >= minGradeOffset.Value && personsGradeOffset <= maxGradeOffset.Value )
                    {
                        return true;
                    }
                }
            }

            // otherwise return false when noththing matched
            return false;
        }
    }
}