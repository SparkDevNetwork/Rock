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
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;

using Rock.Attribute;
using Rock.Field.Types;
using Rock.Web.Cache;

namespace Rock.Communication.SmsActions
{
    /// <summary>
    /// Processes an SMS Action by launching a workflow.
    /// </summary>
    /// <seealso cref="Rock.Communication.SmsActions.SmsActionComponent" />
    [Description( "Launches a workflow to process a message." )]
    [Export( typeof( SmsActionComponent ) )]
    [ExportMetadata( "ComponentName", "Reply" )]
    [TextValueFilterField( "Message", "The message body content that will be filtered on.", false, order: 1, category: AttributeCategories.Filters )]
    [WorkflowTypeField( "Workflow Type", "The type of workflow to launch.", false, true, order: 2, category: AttributeCategories.Workflow )]
    [TextField( "Workflow Name Template", "The lava template to use for setting the workflow name. See the defined type's help text for a listing of merge fields. <span class='tip tip-lava'></span>", false, "", order: 3, category: AttributeCategories.Workflow )]
    [KeyValueListField( "Workflow Attributes", "Key/value list of workflow attributes to set with the given lava merge template. See the defined type’s help text for a listing of merge fields. <span class='tip tip-lava'></span>", false, "", "Attribute Key", "Merge Template", order: 4, category: AttributeCategories.Workflow )]
    public class SmsActionLaunchWorkflow : SmsActionComponent
    {
        /// <summary>
        /// Categories for the attributes
        /// </summary>
        protected class AttributeCategories : BaseAttributeCategories
        {
            /// <summary>
            /// The Workflow category
            /// </summary>
            public const string Workflow = "Workflow";
        }

        #region Properties

        /// <summary>
        /// Gets the component title to be displayed to the user.
        /// </summary>
        /// <value>
        /// The component title to be displayed to the user.
        /// </value>
        public override string Title => "Launch Workflow";

        /// <summary>
        /// Gets the icon CSS class used to identify this component type.
        /// </summary>
        /// <value>
        /// The icon CSS class used to identify this component type.
        /// </value>
        public override string IconCssClass => "fa fa-gears";

        /// <summary>
        /// Gets the description of this SMS Action.
        /// </summary>
        /// <value>
        /// The description of this SMS Action.
        /// </value>
        public override string Description => "Launches a workflow to process a message.";

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Checks the attributes for this component and determines if the message
        /// should be processed.
        /// </summary>
        /// <param name="action">The action that contains the configuration for this component.</param>
        /// <param name="message">The message that is to be checked.</param>
        /// <param name="errorMessage">If there is a problem processing, this should be set</param>
        /// <returns>
        ///   <c>true</c> if the message should be processed.
        /// </returns>
        public override bool ShouldProcessMessage( SmsActionCache action, SmsMessage message, out string errorMessage )
        {
            //
            // Give the base class a chance to check it's own settings to see if we
            // should process this message.
            //
            if ( !base.ShouldProcessMessage( action, message, out errorMessage ) )
            {
                return false;
            }

            //
            // Check if we have a valid workflow type.
            //
            var workflowType = WorkflowTypeCache.Get( GetAttributeValue( action, "WorkflowType" ).AsGuid() );
            if ( workflowType == null )
            {
                return false;
            }

            //
            // Get the filter expression for the message body.
            //
            var attribute = action.Attributes.ContainsKey( "Message" ) ? action.Attributes["Message"] : null;
            var msg = GetAttributeValue( action, "Message" );
            var filter = ValueFilterFieldType.GetFilterExpression( attribute?.QualifierValues, msg );

            //
            // Evaluate the message against the filter and return the match state.
            //
            return filter != null ? filter.Evaluate( message, "Message" ) : true;
        }

        /// <summary>
        /// Processes the message that was received from the remote user.
        /// </summary>
        /// <param name="action">The action that contains the configuration for this component.</param>
        /// <param name="message">The message that was received by Rock.</param>
        /// <param name="errorMessage">If there is a problem processing, this should be set</param>
        /// <returns>An SmsMessage that will be sent as the response or null if no response should be sent.</returns>
        public override SmsMessage ProcessMessage( SmsActionCache action, SmsMessage message, out string errorMessage )
        {
            errorMessage = string.Empty;

            //
            // Get the list of workflow attributes to set.
            //
            var workflowAttributesSettings = new List<KeyValuePair<string, object>>();
            var workflowAttributes = action.Attributes["WorkflowAttributes"];
            if ( workflowAttributes != null )
            {
                if ( workflowAttributes.FieldType.Field is KeyValueListFieldType keyValueField )
                {
                    workflowAttributesSettings = keyValueField.GetValuesFromString( null,
                        GetAttributeValue( action, "WorkflowAttributes" ),
                        workflowAttributes.QualifierValues,
                        false );
                }
            }

            var workflowType = WorkflowTypeCache.Get( GetAttributeValue( action, "WorkflowType" ).AsGuid() );

            //
            // Launch the workflow.
            //
            Rock.Utility.TextToWorkflow.LaunchWorkflow( workflowType,
                GetAttributeValue( action, "WorkflowNameTemplate" ),
                message.FromPerson,
                message.FromNumber.Replace( "+", "" ),
                message.ToNumber.Replace( "+", "" ),
                message.Message,
                message.Attachments,
                workflowAttributesSettings,
                out string response );

            //
            // If there is no response message then return null.
            //
            if ( string.IsNullOrWhiteSpace( response ) )
            {
                return null;
            }

            return new SmsMessage
            {
                Message = response.Trim()
            };
        }

        #endregion
    }
}
