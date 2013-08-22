//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
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
        /// <param name="action">The action.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        protected CheckInState GetCheckInState( Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            if ( entity is CheckInState )
            {
                return (CheckInState)entity;
            }

            errorMessages.Add( "Could not get CheckInState object" );
            return null;
        }

    }
}