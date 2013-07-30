//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Removes the grouptypes from each family member that are not specific to their grade
    /// </summary>
    [Description( "Removes the grouptypes from each family member that are not specific to their grade." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Filter By Grade" )]
    public class FilterByGrade : CheckInActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="action">The workflow action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override bool Execute( Model.WorkflowAction action, Data.IEntity entity, out List<string> errorMessages )
        {
            var checkInState = GetCheckInState( action, out errorMessages );
            if ( checkInState != null )
            {
                var family = checkInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault();
                if ( family != null )
                {
                    foreach ( var person in family.People )
                    {
                        int? personsGrade = person.Person.Grade;

                        foreach ( var groupType in person.GroupTypes.ToList() )
                        {
                            string minGradeValue = groupType.GroupType.GetAttributeValue( "MinGrade" );
                            // if the group type specifies a min grade then the person's grade MUST match
                            if ( minGradeValue != null )
                            {
                                int minGrade = 0;
                                if ( int.TryParse( minGradeValue, out minGrade ) )
                                {
                                    // remove if the person does not have a grade or if their grade is less than the min
                                    if ( !personsGrade.HasValue || personsGrade < minGrade )
                                    {
                                        person.GroupTypes.Remove( groupType );
                                        continue;
                                    }
                                }
                            }

                            string maxGradeValue = groupType.GroupType.GetAttributeValue( "MaxGrade" );
                            // if the group type specifies a min grade then the person's grade MUST match
                            if ( maxGradeValue != null )
                            {
                                int maxGrade = 0;
                                if ( int.TryParse( maxGradeValue, out maxGrade ) )
                                {
                                    // remove if the person does not have a grade or if their grade is more than the max
                                    if ( !personsGrade.HasValue || personsGrade > maxGrade )
                                    {
                                        person.GroupTypes.Remove( groupType );
                                        continue;
                                    }
                                }
                            }
                        }
                    }
                }

                SetCheckInState( action, checkInState );
                return true;
            }

            return false;
        }
    }
}