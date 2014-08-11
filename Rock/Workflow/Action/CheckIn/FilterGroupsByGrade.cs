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
        public override bool Execute( RockContext rockContext, Model.WorkflowAction action, Object entity, out List<string> errorMessages )
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
                    int? personsGrade = person.Person.Grade;

                    if ( personsGrade == null )
                    {
                        continue;
                    }

                    foreach ( var groupType in person.GroupTypes.ToList() )
                    {
                        foreach ( var group in groupType.Groups.ToList() )
                        {
                            string gradeRange = group.Group.GetAttributeValue( "GradeRange" ) ?? string.Empty;

                            string[] gradeRangePair = gradeRange.Split( new char[] { ',' }, StringSplitOptions.None );
                            string minGradeValue = null;
                            string maxGradeValue = null;
                            if ( gradeRangePair.Length == 2 )
                            {
                                minGradeValue = gradeRangePair[0];
                                maxGradeValue = gradeRangePair[1];
                            }

                            // if the group type specifies a min grade then the person's grade MUST match
                            if ( minGradeValue != null )
                            {
                                int minGrade = 0;
                                if ( int.TryParse( minGradeValue, out minGrade ) )
                                {
                                    // remove if the person does not have a grade or if their grade is less than the min
                                    if ( !personsGrade.HasValue || personsGrade < minGrade )
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

                            // if the group type specifies a min grade then the person's grade MUST match
                            if ( maxGradeValue != null )
                            {
                                int maxGrade = 0;
                                if ( int.TryParse( maxGradeValue, out maxGrade ) )
                                {
                                    // remove if the person does not have a grade or if their grade is more than the max
                                    if ( !personsGrade.HasValue || personsGrade > maxGrade )
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