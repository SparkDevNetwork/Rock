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

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Removes the locations and groups for each selected family member that
    /// are not specific to their last name.
    /// </summary>
    [Description( "Removes the groups for each selected family member that are not specific to their last name." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Filter Groups By LastName" )]
    public class FilterGroupsByLastName : CheckInActionComponent
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
                foreach ( var person in family.People.Where( p => p.Selected && p.Person.LastName.Length > 0 ) )
                {
                    char lastInitial = char.Parse( person.Person.LastName.Substring( 0, 1 ).ToUpper() );
                    foreach ( var groupType in person.GroupTypes.Where( g => g.Selected ).ToList() )
                    {
                        foreach ( var group in groupType.Groups.ToList() )
                        {
                            string lastNameBeginLetterRange = group.Group.GetAttributeValue( "LastNameBeginLetterRange" ).Trim();
                            string lastNameEndLetterRange = group.Group.GetAttributeValue( "LastNameEndLetterRange" ).Trim();

                            char rangeStart = ( lastNameBeginLetterRange == "" ) ? 'A' : char.Parse( lastNameBeginLetterRange.ToUpper() );
                            char rangeEnd = ( lastNameEndLetterRange == "" ) ? 'Z' : char.Parse( lastNameEndLetterRange.ToUpper() );

                            // If the last name is not in range, remove the group
                            if ( !( lastInitial >= rangeStart && lastInitial <= rangeEnd ) )
                            {
                                groupType.Groups.Remove( group );
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}