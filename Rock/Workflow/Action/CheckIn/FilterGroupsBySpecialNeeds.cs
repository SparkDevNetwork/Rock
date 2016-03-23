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
    /// Removes (or excludes) the location's "special needs" groups for each selected family member
    /// if the person is not "special needs".  The filter can ALSO be configured to
    /// remove normal (non-special needs) groups when the person is "special needs".
    /// </summary>
    [ActionCategory( "Check-In" )]
    [Description( "Removes (or excludes) the groups for each selected family member that are not specific to their special needs attribute." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Filter Groups By Special Needs" )]
    [AttributeField( "72657ED8-D16E-492E-AC12-144C5E7567E7", "Person Special Needs Attribute", "Select the attribute used to filter special needs people.", true, false, "8B562561-2F59-4F5F-B7DC-92B2BB7BB7CF", order: 1 )]
    [AttributeField( "9BBFDA11-0D22-40D5-902F-60ADFBC88987", "Group Special Needs Attribute", "Select the attribute used to filter special needs groups.", true, false, "9210EC95-7B85-4D11-A82E-0B677B32704E", order: 2 )]
    [BooleanField( "Remove (or exclude) Special Needs Groups", "If set to true, special-needs groups will be removed if the person is NOT special needs. This basically prevents non-special-needs kids from getting put into special needs classes.  Default true.", true, key: "RemoveSpecialNeedsGroups", order: 3 )]
    [BooleanField( "Remove (or exclude) Non-Special Needs Groups", "If set to true, non-special-needs groups will be removed if the person is special needs.  This basically prevents special needs kids from getting put into regular classes.  Default false.", false, key: "RemoveNonSpecialNeedsGroups", order: 4 )]
    [BooleanField( "Remove", "Select 'Yes' if groups should be be removed.  Select 'No' if they should just be marked as excluded.", true, order: 5 )]
    public class FilterGroupsBySpecialNeeds : CheckInActionComponent
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

            var personSpecialNeedsKey = string.Empty;
            var groupSpecialNeedsKey = string.Empty;
            var personSpecialNeedsGuid = GetAttributeValue( action, "PersonSpecialNeedsAttribute" ).AsGuid();
            var groupSpecialNeedsGuid = GetAttributeValue( action, "GroupSpecialNeedsAttribute" ).AsGuid();
            bool removeSNGroups = GetAttributeValue( action, "RemoveSpecialNeedsGroups" ).AsBoolean( true );
            bool removeNonSNGroups = GetAttributeValue( action, "RemoveNonSpecialNeedsGroups" ).AsBoolean();

            // get the admin-selected attribute key instead of using a hardcoded key
            if ( personSpecialNeedsGuid != Guid.Empty )
            {
                personSpecialNeedsKey = rockContext.Attributes.Where( a => a.Guid == personSpecialNeedsGuid ).Select( a => a.Key ).FirstOrDefault();
            }
            else
            {
                action.AddLogEntry( string.Format( "The selected Person Special Needs attribute is missing or invalid for '{0}'.", action.ActionType.Name ) );
            }

            if ( groupSpecialNeedsGuid != Guid.Empty )
            {
                groupSpecialNeedsKey = rockContext.Attributes.Where( a => a.Guid == groupSpecialNeedsGuid ).Select( a => a.Key ).FirstOrDefault();
            }
            else
            {
                action.AddLogEntry( string.Format( "The selected Group Special Needs attribute is missing or invalid for '{0}'.", action.ActionType.Name ) );
            }

            var family = checkInState.CheckIn.Families.FirstOrDefault( f => f.Selected );
            if ( family != null )
            {
                var remove = GetAttributeValue( action, "Remove" ).AsBoolean();

                foreach ( var person in family.People )
                {
                    if ( person.Person.Attributes == null )
                    {
                        person.Person.LoadAttributes( rockContext );
                    }

                    bool isSNPerson = person.Person.GetAttributeValue( personSpecialNeedsKey ).AsBoolean();
                    foreach ( var groupType in person.GroupTypes.ToList() )
                    {
                        foreach ( var group in groupType.Groups.ToList() )
                        {
                            bool isSNGroup = group.Group.GetAttributeValue( groupSpecialNeedsKey ).AsBoolean();

                            // If the group is special needs but the person is not, then remove it.
                            if ( removeSNGroups && isSNGroup && !( isSNPerson ) )
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

                            // or if the setting is enabled and the person is SN but the group is not.
                            if ( removeNonSNGroups && isSNPerson && !isSNGroup )
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