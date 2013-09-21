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
    /// Removes the groups for each selected family member that are not specific to their grade.
    /// </summary>
    [Description( "Removes the groups for each selected family member that are not specific to their grade." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Filter Groups By Grade" )]
    public class FilterGroupsByGrade : CheckInActionComponent
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
                                        groupType.Groups.Remove( group );
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