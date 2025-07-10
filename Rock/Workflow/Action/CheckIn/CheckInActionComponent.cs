// <copyright>
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
using System.Linq;

using Microsoft.Extensions.Logging;

using Rock.CheckIn;

[assembly: Rock.Logging.RockLoggingCategory( "Rock.Workflow.Action.CheckIn" )]

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// An action component specifically for a check-in workflow
    /// </summary>
    public abstract class CheckInActionComponent : ActionComponent
    {
        /// <summary>
        /// Gets the state of the check-in.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        protected CheckInState GetCheckInState( Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            var logger = Rock.Logging.RockLogger.LoggerFactory.CreateLogger( GetType().FullName );

            if ( entity is CheckInState state )
            {
                if ( logger.IsEnabled( LogLevel.Trace ) )
                {
                    logger.LogTrace( "GetCheckInState: FamilyId={familyId}, Family={family}",
                        state.CheckIn.CurrentFamily?.Group?.Id,
                        state.CheckIn.CurrentFamily?.Group?.Name );

                    if ( state.CheckIn.CurrentFamily?.People != null )
                    {
                        for ( var i = 0; i < state.CheckIn.CurrentFamily.People.Count; i++ )
                        {
                            var person = state.CheckIn.CurrentFamily.People[i];

                            logger.LogTrace( "GetCheckInState: Person[{index}]: Id={id}, Name={name}, Age={age}, Grade={grade}, Excluded={excluded}, IncludedGroupTypeIds={includedGroupTypeIds}, IncludedGroupIds={includedGroupIds}",
                                i,
                                person.Person.Id,
                                person.Person.NickName,
                                person.Person.AgePrecise,
                                person.Person.GradeFormatted,
                                person.ExcludedByFilter,
                                string.Join( ",", person.GroupTypes.Where( gt => !gt.ExcludedByFilter ).Select( gt => gt.GroupType.Id ) ),
                                string.Join( ",", person.GroupTypes.Where( gt => !gt.ExcludedByFilter ).SelectMany( gt => gt.Groups ).Where( g => !g.ExcludedByFilter ).Select( gt => gt.Group.Id ) ) );
                        }
                    }
                }

                return state;
            }

            logger.LogError( "GetCheckInState Not Found" );

            errorMessages.Add( "Could not get CheckInState object" );
            return null;
        }

    }
}