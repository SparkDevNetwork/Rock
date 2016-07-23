﻿// <copyright>
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
using Rock.CheckIn;
using Rock.Data;

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Removes (or excludes) the groups for each selected family member if the person's ability level does not match the groups.
    /// </summary>
    [ActionCategory( "Check-In" )]
    [Description( "Removes (or excludes) the groups for each selected family member if the person's ability level does not match the groups." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Filter Groups By Ability Level" )]
    [BooleanField( "Remove", "Select 'Yes' if groups should be be removed.  Select 'No' if they should just be marked as excluded.", true )]
    public class FilterGroupsByAbilityLevel : CheckInActionComponent
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
            var family = checkInState.CheckIn.CurrentFamily;
            if ( family != null )
            {
                var remove = GetAttributeValue( action, "Remove" ).AsBoolean();

                if ( checkInState.CheckInType.TypeOfCheckin == TypeOfCheckin.Family )
                {
                    var currentPerson = checkInState.CheckIn.CurrentPerson;
                    if ( currentPerson != null )
                    {
                        FilterGroups( currentPerson, rockContext, remove );
                    }
                }
                else
                {
                    foreach ( var person in family.People )
                    {
                        FilterGroups( person, rockContext, remove );
                    }
                }
            }

            return true;
        }

        private void FilterGroups( CheckInPerson person, RockContext rockContext, bool remove )
        {
            if ( person.Person.Attributes == null )
            {
                person.Person.LoadAttributes( rockContext );
            }

            string personAbilityLevel = person.Person.GetAttributeValue( "AbilityLevel" ).ToUpper();
            if ( !string.IsNullOrWhiteSpace( personAbilityLevel ) )
            {
                foreach ( var groupType in person.GroupTypes.ToList() )
                {
                    foreach ( var group in groupType.Groups.ToList() )
                    {
                        var groupAttributes = group.Group.GetAttributeValues( "AbilityLevel" );
                        if ( groupAttributes.Any() && !groupAttributes.Contains( personAbilityLevel, StringComparer.OrdinalIgnoreCase ) )
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
}