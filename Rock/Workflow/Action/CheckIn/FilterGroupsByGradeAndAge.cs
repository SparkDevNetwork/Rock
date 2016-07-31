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

    [BooleanField( "Remove", "Select 'Yes' if groups should be be removed.  Select 'No' if they should just be marked as excluded.", true )]
    [AttributeField( Rock.SystemGuid.EntityType.GROUP, "Group Age Range Attribute", "Select the attribute used to define the age range of the group", true, false, "43511B8F-71D9-423A-85BF-D1CD08C1998E", order: 2 )]
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

                // get the admin-selected attribute key instead of using a hardcoded key
                var ageRangeAttributeKey = string.Empty;
                var ageRangeAttributeGuid = GetAttributeValue( action, "GroupAgeRangeAttribute" ).AsGuid();
                if ( ageRangeAttributeGuid != Guid.Empty )
                {
                    ageRangeAttributeKey = AttributeCache.Read( ageRangeAttributeGuid, rockContext ).Key;
                }

                foreach ( var person in family.People )
                {
                    int? gradeOffset = person.Person.GradeOffset;
                    var ageAsDouble = person.Person.AgePrecise;
                    decimal? age = null;
                    if ( ageAsDouble.HasValue )
                    {
                        age = Convert.ToDecimal( ageAsDouble.Value );
                    }

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
                                minGradeDefinedValue = gradeOffsetRangePair[0].HasValue ? DefinedValueCache.Read( gradeOffsetRangePair[0].Value ) : null;
                                maxGradeDefinedValue = gradeOffsetRangePair[1].HasValue ? DefinedValueCache.Read( gradeOffsetRangePair[1].Value ) : null;
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
                                                isMatch = false;
                                            }
                                        }

                                        if ( maxAge.HasValue )
                                        {
                                            int groupMaxAgePrecision = maxAge.Value.GetDecimalPrecision();
                                            decimal? personAgePrecise = age.Floor( groupMaxAgePrecision );
                                            if ( personAgePrecise > maxAge )
                                            {
                                                isMatch = false;
                                            }
                                        }

                                        if ( !isMatch.HasValue )
                                        {
                                            isMatch = true;
                                        }
                                    }
                                    else
                                    {
                                        if ( ageRequired )
                                        {
                                            isMatch = false;
                                        }
                                    }
                                }
                            }

                            if ( !isMatch.HasValue || !isMatch.Value )
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
    }
}