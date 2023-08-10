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

using Rock.CheckIn;

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

            if ( entity is CheckInState )
            {
                return ( CheckInState ) entity;
            }

            errorMessages.Add( "Could not get CheckInState object" );
            return null;
        }

    }
}