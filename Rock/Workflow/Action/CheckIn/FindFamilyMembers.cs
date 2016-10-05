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
using System.Data.Entity;
using System.Linq;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Finds family members in a given family
    /// </summary>
    [ActionCategory( "Check-In" )]
    [Description( "Finds family members in a given family" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Find Family Members" )]
    public class FindFamilyMembers : CheckInActionComponent
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
            if ( checkInState != null )
            {
                var family = checkInState.CheckIn.CurrentFamily;
                if ( family != null )
                {
                    var service = new GroupMemberService( rockContext );

                    var people = service.GetByGroupId( family.Group.Id ).AsNoTracking();
                    if ( checkInState.CheckInType != null && checkInState.CheckInType.PreventInactivePeopele )
                    {
                        var dvActive = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() );
                        if ( dvActive != null )
                        {
                            people = people.Where( m => m.Person.RecordStatusValueId == dvActive.Id );
                        }
                    }

                    foreach ( var groupMember in people.ToList() )
                    {
                        if ( !family.People.Any( p => p.Person.Id == groupMember.PersonId ) )
                        {
                            var person = new CheckInPerson();
                            person.Person = groupMember.Person.Clone( false );
                            person.FamilyMember = true;
                            family.People.Add( person );
                        }
                    }

                    return true;
                }
                else
                {
                    errorMessages.Add( "There is not a family that is selected" );
                }

                return false;
            }

            return false;
        }
    }
}