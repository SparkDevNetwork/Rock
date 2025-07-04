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

    [TextValueFilterField( "Message",
        Key = AttributeKey.Message,
        Description = "The message body content that will be filtered on.",
        IsRequired = false,
        Category = AttributeCategories.Filters,
        Order = 1)]

    [WorkflowTypeField( "Workflow Type",
        Key = AttributeKey.WorkflowType,
        Category = AttributeCategories.Workflow,
        Description = "The type of workflow to launch.",
        AllowMultiple = false,
        IsRequired = true,
        Order = 2 )]

    [BooleanField( "Pass Nameless Person",
        Key = AttributeKey.PassNamelessPerson,
        Category = AttributeCategories.Workflow,
        Description = "If a matching person is not found in the database should a nameless person record be passed to the workflow.",
        ControlType = BooleanFieldType.BooleanControlType.Checkbox,
        DefaultBooleanValue = true,
        Order = 3 )]

    [TextField( "Workflow Name Template",
        Key = AttributeKey.WorkflowNameTemplate,
        Category = AttributeCategories.Workflow,
        Description = "The lava template to use for setting the workflow name. See the defined type's help text for a listing of merge fields. <span class='tip tip-lava'></span>",
        IsRequired = false,
        Order = 4 )]

    [KeyValueListField( "Workflow Attributes",
        Key = AttributeKey.WorkflowAttributes,
        Category = AttributeCategories.Workflow,
        Description = "Key/value list of workflow attributes to set with the given lava merge template. See the defined type’s help text for a listing of merge fields. <span class='tip tip-lava'></span>",
        IsRequired = false,
        DefaultValue = "",
        KeyPrompt = "Attribute Key", 
        ValuePrompt = "Merge Template",
        Order = 5 )]

    [Rock.SystemGuid.EntityTypeGuid( "D1528FAB-EEE5-4273-8EF9-69163483BA1B")]
    public class SmsActionLaunchWorkflow : SmsActionComponent
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string Message = "Message";
            public const string WorkflowType = "WorkflowType";
            public const string PassNamelessPerson = "PassNamelessPerson";
            public const string WorkflowNameTemplate = "WorkflowNameTemplate";
            public const string WorkflowAttributes = "WorkflowAttributes";
        }


        #endregion Attribute Keys

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
            var workflowType = WorkflowTypeCache.Get( GetAttributeValue( action, AttributeKey.WorkflowType ).AsGuid() );
            if ( workflowType == null )
            {
                return false;
            }

            //
            // Get the filter expression for the message body.
            //
            var attribute = action.Attributes.ContainsKey( AttributeKey.Message ) ? action.Attributes[AttributeKey.Message] : null;
            var msg = GetAttributeValue( action, AttributeKey.Message );
            var filter = ValueFilterFieldType.GetFilterExpression( attribute?.QualifierValues, msg );

            //
            // Evaluate the message against the filter and return the match state.
            //
            return filter != null ? filter.Evaluate( message, AttributeKey.Message ) : true;
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
#if REVIEW_WEBFORMS
                    workflowAttributesSettings = keyValueField.GetValuesFromString( null,
#else
                    workflowAttributesSettings = keyValueField.GetValuesFromString(
#endif
                        GetAttributeValue( action, AttributeKey.WorkflowAttributes ),
                        workflowAttributes.QualifierValues,
                        false );
                }
            }

            var workflowType = WorkflowTypeCache.Get( GetAttributeValue( action, AttributeKey.WorkflowType ).AsGuid() );

            Rock.Model.Person fromPerson;

            var passNamelessPerson = GetAttributeValue( action, AttributeKey.PassNamelessPerson ).AsBooleanOrNull() ?? false;
            if ( message.FromPerson != null && message.FromPerson.IsNameless() && passNamelessPerson == false )
            {
                fromPerson = null;
            }
            else
            {
                fromPerson = message.FromPerson;
            }

            //
            // Launch the workflow.
            //
            Rock.Utility.TextToWorkflow.LaunchWorkflow( workflowType,
                GetAttributeValue( action, AttributeKey.WorkflowNameTemplate ),
                fromPerson,
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
