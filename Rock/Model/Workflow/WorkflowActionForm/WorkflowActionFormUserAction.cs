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

namespace Rock.Model
{
    /// <summary>
    /// Represents an action that can be selected by the user on a workflow form.
    /// </summary>
    public class WorkflowActionFormUserAction
    {
        /// <summary>
        /// The name of the action.
        /// </summary>
        public string ActionName { get; set; }

        /// <summary>
        /// The unique identifier of the workflow activity that will be activated if this action is selected.
        /// </summary>
        public string ActivateActivityTypeGuid { get; set; }

        /// <summary>
        /// The type of action.
        /// </summary>
        public string ButtonTypeGuid { get; set; }

        /// <summary>
        /// The response message displayed to the user if the action is performed.
        /// </summary>
        public string ResponseText { get; set; }

        /// <summary>
        /// A flag indicating if this form action triggers validation of the form fields.
        /// </summary>
        public bool CausesValidation { get; set; }

        /// <summary>
        /// Returns a collection of Workflow Form Actions from a Uri Encoded string.
        /// </summary>
        /// <param name="encodedString"></param>
        /// <returns></returns>
        /// <remarks>
        /// The Uri Encoded string is used to serialize a collection of actions for portability and storage.
        /// </remarks>
        public static List<WorkflowActionFormUserAction> FromUriEncodedString( string encodedString )
        {
            Guid buttonCancelGuid = Rock.SystemGuid.DefinedValue.BUTTON_HTML_CANCEL.AsGuid();

            var buttons = new List<WorkflowActionFormUserAction>();

            var buttonList = Rock.Utility.RockSerializableList.FromUriEncodedString( encodedString, StringSplitOptions.RemoveEmptyEntries );

            // Without any other way of determining this, assume that the built-in Cancel button is the only action that does not cause validation.
            var nonValidationButtonList = new List<Guid> { buttonCancelGuid };

            foreach ( var buttonDefinitionText in buttonList.List )
            {
                var button = new WorkflowActionFormUserAction();

                string[] nameValueResponse = buttonDefinitionText.Split( new char[] { '^' } );

                // Button Name
                button.ActionName = nameValueResponse.Length > 0 ? nameValueResponse[0] : string.Empty;

                // Button Type
                if ( nameValueResponse.Length > 1 )
                {
                    button.ButtonTypeGuid = nameValueResponse[1];
                }

                if ( button.ButtonTypeGuid.IsNullOrWhiteSpace() )
                {
                    button.ButtonTypeGuid = Rock.SystemGuid.DefinedValue.BUTTON_HTML_PRIMARY;
                }

                // Determine if the button causes form validation.                    
                button.CausesValidation = !nonValidationButtonList.Contains( button.ButtonTypeGuid.AsGuid() );

                if ( button.ButtonTypeGuid.AsGuid() == buttonCancelGuid )
                {
                    button.CausesValidation = false;
                }
                else
                {
                    // By default, assume that an action button triggers validation of the form.
                    button.CausesValidation = true;
                }

                // Button Activity
                button.ActivateActivityTypeGuid = nameValueResponse.Length > 2 ? nameValueResponse[2] : string.Empty;

                // Response Text
                button.ResponseText = nameValueResponse.Length > 3 ? nameValueResponse[3] : string.Empty;

                buttons.Add( button );
            }

            return buttons;
        }
    }
}