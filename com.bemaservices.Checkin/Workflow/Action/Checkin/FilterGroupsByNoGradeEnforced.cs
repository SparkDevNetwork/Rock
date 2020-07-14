// <copyright>
// Copyright by BEMA Information Technologies
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
//
// </copyright>

///
/// Created By: BEMA Services, Inc.
/// Author: Sam Crisp
/// Description:
///     Use this checkin workflow action directly after the Filter Groups By Age and Filter Groups By Grade actions in the Person Search activity.
///     This filter will remove from a person with an grade any groups that use the age or ability level filters (but no graderange filter).
///     This allows children with a grade to not have the option to select a criteria-based group with no grade-related filters.
///     To mark the groups as 'No Grade Enforced', add a group boolean attribute to the Check-In By Age or Check-In By Ability Level group types of 'NoGradeEnforced'
///
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using Rock;
using Rock.Model;
using Rock.Attribute;
using Rock.Data;
using Rock.Workflow;
using Rock.Workflow.Action.CheckIn;
using Rock.Web.Cache;

namespace com_bemaservices.Workflow.Action.CheckIn
{
    /// <summary>
    /// Removes (or excludes) the groups for each selected family member that are not specific to their grade.
    /// </summary>
    [ActionCategory( "BEMA Services > Check-In" )]
    [Description( "Removes (or excludes) the groups for each selected family member that are marked to filter by age but person has a grade. Add 'NoGradeEnforced' boolean attribute to age-ranged groups" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Filter Groups By No Grade (Enforced)" )]
    [BooleanField( "Remove", "Select 'Yes' if groups should be be removed.  Select 'No' if they should just be marked as excluded.", true )]
    [AttributeField ( Rock.SystemGuid.EntityType.GROUP, "No Grade Enforced Attribute", "Select the check-in group attribute used to indicate if blank values for grade should require no grade on the person.", true, false,
         order: 4 )]
    public class FilterGroupsByNoGradeEnforced : CheckInActionComponent
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
        public override bool Execute( RockContext rockContext, Rock.Model.WorkflowAction action, object entity, out List<string> errorMessages )
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

                var noGradeEnforcedAttributeKey = string.Empty;
                var noGradeEnforcedAttributeGuid = GetAttributeValue ( action, "NoGradeEnforcedAttribute" ).AsGuidOrNull ();
                if ( noGradeEnforcedAttributeGuid.HasValue )
                {
                    var attribute = AttributeCache.Get ( noGradeEnforcedAttributeGuid.Value, rockContext );
                    if ( attribute != null )
                    {
                        noGradeEnforcedAttributeKey = attribute.Key;
                    }
                }

                foreach ( var person in family.People )
                {
                    int? personsGradeOffset = person.Person.GradeOffset;
                    if ( personsGradeOffset == null && !gradeRequired )
                    {
                        continue;
                    }

                    foreach ( var groupType in person.GroupTypes.ToList() )
                    {
                        foreach ( var group in groupType.Groups.ToList() )
                        {
                            string gradeOffsetRange = group.Group.GetAttributeValue( "GradeRange" ) ?? string.Empty;
                            bool noGradeEnforced = group.Group.GetAttributeValue( noGradeEnforcedAttributeKey ).AsBoolean();

                            if( gradeOffsetRange.IsNullOrWhiteSpace() && noGradeEnforced && personsGradeOffset.HasValue )
                            {
                                //Since this person has a grade AND this group has No Grade Enforced, remove this group from this person
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