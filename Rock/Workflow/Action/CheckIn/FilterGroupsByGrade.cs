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
using Rock.Web.Cache;

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Removes (or excludes) the groups for each selected family member that are not specific to their grade.
    /// </summary>
    [Description( "Removes (or excludes) the groups for each selected family member that are not specific to their grade." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Filter Groups By Grade" )]

    [BooleanField( "Remove", "Select 'Yes' if groups should be be removed.  Select 'No' if they should just be marked as excluded.", true )]
    public class FilterGroupsByGrade : CheckInActionComponent
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

            var family = checkInState.CheckIn.Families.FirstOrDefault( f => f.Selected );
            if ( family != null )
            {
                var remove = GetAttributeValue( action, "Remove" ).AsBoolean();

                foreach ( var person in family.People )
                {
                    int? personsGradeOffset = person.Person.GradeOffset;

                    if ( personsGradeOffset == null )
                    {
                        continue;
                    }

                    foreach ( var groupType in person.GroupTypes.ToList() )
                    {
                        foreach ( var group in groupType.Groups.ToList() )
                        {
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
                            if ( maxGradeDefinedValue != null )
                            {
                                // NOTE: minGradeOffset is actually based on the MAX Grade since GradeOffset's are Years Until Graduation
                                int? minGradeOffset = maxGradeDefinedValue.Value.AsIntegerOrNull();
                                if ( minGradeOffset.HasValue )
                                {
                                    // remove if the person does not have a grade or if their grade offset is more than the max offset (too young)
                                    // example person is in 3rd grade (offset 9) and range is 4th to 6th (offset 6 to 8)
                                    if ( !personsGradeOffset.HasValue || personsGradeOffset < minGradeOffset.Value )
                                    {
                                        if ( remove ) 
                                        {
                                            groupType.Groups.Remove( group );
                                        }
                                        else
                                        {
                                            group.ExcludedByFilter = true;
                                        }

                                        continue;
                                    }
                                }
                            }

                            // if the group type specifies a max grade (min gradeOffset)...
                            if ( minGradeDefinedValue != null )
                            {
                                // NOTE: maxGradeOffset is actually based on the MIN Grade since GradeOffset's are Years Until Graduation
                                int? maxGradeOffset = minGradeDefinedValue.Value.AsIntegerOrNull();
                                if ( maxGradeOffset.HasValue )
                                {
                                    // remove if the person does not have a grade or if their grade offset is less than the min offset (too old)
                                    // example person is in 7rd grade (offset 5) and range is 4th to 6th (offset 6 to 8)
                                    if ( !personsGradeOffset.HasValue || personsGradeOffset > maxGradeOffset )
                                    {
                                        if ( remove )
                                        {
                                            groupType.Groups.Remove( group );
                                        }
                                        else
                                        {
                                            group.ExcludedByFilter = true;
                                        }

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