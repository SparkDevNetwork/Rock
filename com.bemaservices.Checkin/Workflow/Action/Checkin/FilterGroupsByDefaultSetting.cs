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
///     Use this checkin workflow action directly after the Filter Groups By Gender action in the Person Search activity.
///     This filter will remove from a person any check-in group marked "Default" if they are a member of another group.
///     This allows children to be placed in a group where they don;t meet the criteria.  It requires use of the Member/Grade/Age filter as well.
///     To mark the groups as 'Default', add a group boolean attribute to the Check-In By Age /Grade/Ability Level group types of 'Is Default' (key 'IsDefault')
///

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Workflow;
using Rock.Workflow.Action.CheckIn;

namespace com_bemaservices.Workflow.Action.CheckIn
{
    /// <summary>
    /// Removes (or excludes) the groups for each selected family member that are not specific to their age or birthdate.
    /// </summary>
    [ActionCategory( "BEMA Services > Check-In" )]
    [Description( "Removes (or excludes) groups marked as Default if there are any groups available that are not Default" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Filter Groups By Default Setting" )]

    [BooleanField( "Remove", "Select 'Yes' if groups should be be removed.  Select 'No' if they should just be marked as excluded.", true )]
    public class FilterGroupsByDefaultSetting : CheckInActionComponent
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
        public override bool Execute( RockContext rockContext, Rock.Model.WorkflowAction action, Object entity, out List<string> errorMessages )
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

                foreach ( var person in family.People )
                {
                    // If the person qualifies for non-default groups
                    if ( person.GroupTypes.SelectMany( t => t.Groups.Select( g => g.Group ) ).Any( g => ( g.GetAttributeValue( "IsDefault" ).AsBooleanOrNull() ?? false ) == false ) )
                    {
                        // Remove all the default groups they qualified for
                        foreach ( var groupType in person.GroupTypes.ToList() )
                        {
                            foreach ( var group in groupType.Groups.ToList() )
                            {
                                var isDefault = group.Group.GetAttributeValue( "IsDefault" ).AsBooleanOrNull() ?? false;

                                if ( isDefault )
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
            }

            return true;
        }
    }
}