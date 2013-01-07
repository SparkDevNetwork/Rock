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
        /// Gets the state of the check in.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        protected CheckInState GetCheckInState( Model.WorkflowAction action, out List<string> errorMessages )
        {
            CheckInState state = null;

            errorMessages = new List<string>();

            string stateString = action.Activity.Workflow.GetAttributeValue( "CheckInState" );
            if ( !String.IsNullOrEmpty( stateString ) )
            {
                state = CheckInState.FromJson( stateString );
                if ( state == null )
                {
                    errorMessages.Add( "Could not deserialize CheckInState" );
                }
            }
            else
            {
                errorMessages.Add( "Could not get CheckInState attribute" );
            }

            return state;
        }

        /// <summary>
        /// Sets the state of the check in.
        /// </summary>
        /// <param name="action">The action.</param>
        protected void SetCheckInState ( Model.WorkflowAction action, CheckInState checkInState)
        {
            string stateString = checkInState.ToJson();
            action.Activity.Workflow.SetAttributeValue( "CheckInState", stateString );
        }
    }
}