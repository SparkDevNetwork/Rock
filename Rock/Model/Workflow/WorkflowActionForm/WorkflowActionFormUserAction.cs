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

using Rock.Web.Cache;

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
            var buttons = new List<WorkflowActionFormUserAction>();

            var buttonList = Rock.Utility.RockSerializableList.FromUriEncodedString( encodedString, StringSplitOptions.RemoveEmptyEntries );

            var cancelButtonList = GetCancelButtonGuidList();

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

                // If the button is not a Cancel button, flag it as causing form validation.
                button.CausesValidation = !cancelButtonList.Contains( button.ButtonTypeGuid.AsGuid() );

                // Button Activity
                button.ActivateActivityTypeGuid = nameValueResponse.Length > 2 ? nameValueResponse[2] : string.Empty;

                // Response Text
                button.ResponseText = nameValueResponse.Length > 3 ? nameValueResponse[3] : string.Empty;

                buttons.Add( button );
            }

            return buttons;
        }

        /// <summary>
        /// Get the set of workflow action buttons that are configured to perform a cancel function.
        /// </summary>
        /// <returns></returns>
        private static List<Guid> GetCancelButtonGuidList()
        {
            // Add the default Cancel button.
            var cancelButtonList = new List<Guid> { Rock.SystemGuid.DefinedValue.BUTTON_HTML_CANCEL.AsGuid() };

            // Add other buttons that have been configured to disable validation.
            // In accordance with the Rock documentation, button validation can be disabled
            // by replacing the text "{{ ButtonClick }}" with "{{ ButtonCancel }}" in the Button HTML Attribute.
            // Add any buttons with this configuration to the collection of Cancel buttons.
            var buttonType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.BUTTON_HTML );

            foreach ( var buttonDefinedValue in buttonType.DefinedValues )
            {
                var buttonHtml = buttonDefinedValue.GetAttributeValue( "ButtonHTML" );

                if ( !buttonHtml.IsNullOrWhiteSpace() )
                {
                    buttonHtml = buttonHtml.ToLower().Replace( " ", "" );

                    if ( !buttonHtml.Contains( "{{buttonclick}}" )
                         && buttonHtml.Contains( "onclick=\"{{buttoncancel}}\"" ) )
                    {
                        cancelButtonList.Add( buttonDefinedValue.Guid );
                    }
                }
            }

            return cancelButtonList;

        }
    }
}