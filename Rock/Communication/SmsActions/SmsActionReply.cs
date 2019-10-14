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
    /// Processes an SMS Action by sending a Lava-processed response back to the sender.
    /// </summary>
    /// <seealso cref="Rock.Communication.SmsActions.SmsActionComponent" />
    [Description( "Sends a response to the message." )]
    [Export( typeof( SmsActionComponent ) )]
    [ExportMetadata( "ComponentName", "Reply" )]
    [TextValueFilterField( "Message", "The message body content that will be filtered on.", false, order: 1, category: AttributeCategories.Filters )]
    [MemoField( "Response", "The response that will be sent. <span class='tip tip-lava'></span>", true, order: 2, category: AttributeCategories.Response )]
    public class SmsActionReply : SmsActionComponent
    {
        /// <summary>
        /// Categories for the attributes
        /// </summary>
        protected class AttributeCategories : BaseAttributeCategories
        {
            /// <summary>
            /// The filters category
            /// </summary>
            public const string Response = "Response";
        }

        #region Properties

        /// <summary>
        /// Gets the component title to be displayed to the user.
        /// </summary>
        /// <value>
        /// The component title to be displayed to the user.
        /// </value>
        public override string Title => "Reply";

        /// <summary>
        /// Gets the icon CSS class used to identify this component type.
        /// </summary>
        /// <value>
        /// The icon CSS class used to identify this component type.
        /// </value>
        public override string IconCssClass => "fa fa-comment-o";

        /// <summary>
        /// Gets the description of this SMS Action.
        /// </summary>
        /// <value>
        /// The description of this SMS Action.
        /// </value>
        public override string Description => "Sends a response back to the sender.";

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
            // Process the message with lava to get the response that should be sent back.
            //
            var mergeObjects = new Dictionary<string, object>
            {
                { "Message", message }
            };
            var responseMessage = action.GetAttributeValue( "Response" ).ResolveMergeFields( mergeObjects, message.FromPerson );

            //
            // If there is no response message then return null.
            //
            if ( string.IsNullOrWhiteSpace( responseMessage ) )
            {
                return null;
            }

            return new SmsMessage
            {
                Message = responseMessage.Trim()
            };
        }

        #endregion
    }
}
