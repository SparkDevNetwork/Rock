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

using Rock.Data;

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Removes any person that does not have any group types
    /// </summary>
    [ActionCategory( "Check-In" )]
    [Description( "Removes any person that does not have any group types" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Remove Empty People" )]
    public class RemoveEmptyPeople : CheckInActionComponent
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
                foreach ( var family in checkInState.CheckIn.Families.ToList() )
                {
                    // Make a copy of the original set of people because we may need them
                    // if they are checking-out (not checking-in).
                    family.OriginalPeople = new List<Rock.CheckIn.CheckInPerson>( family.People );

                    foreach ( var person in family.People.ToList() )
                    {
                        if ( person.GroupTypes.Count == 0 )
                        {
                            family.People.Remove( person );
                        }
                        else if ( person.GroupTypes.All( t => t.ExcludedByFilter ) )
                        {
                            person.ExcludedByFilter = true;
                        }
                    }
                }

                return true;
            }

            return false;
        }
    }
}