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
using Rock.Web.Cache;

namespace Rock.Communication.SmsActions
{
    [Description( "Sends a response to the message." )]
    [Export( typeof( SmsActionComponent ) )]
    [ExportMetadata( "ComponentName", "Reply" )]
    [TextValueFilterField( "Message", "The message body content that will be filtered on.", false, order: 1 )]
    [MemoField( "Response", "The response that will be sent. <span class='tip tip-lava'></span>", true, order: 2 )]
    public class SmsActionReply : SmsActionComponent
    {
        public override string Title => "Reply";

        public override string IconCssClass => "fa fa-comment-o";

        public override string Description => "Sends a response to the message.";

        protected override bool ShouldProcessMessage( SmsActionCache action, SmsMessage message )
        {
            if ( !base.ShouldProcessMessage( action, message ) )
            {
                return false;
            }

            var filter = Field.Types.ValueFilterFieldType.GetFilterExpression( null, action.GetAttributeValue( "Message" ) );

            return filter != null ? filter.Evaluate( message, "Message" ) : true;
        }

        public override bool ProcessMessage( SmsActionCache action, SmsMessage message, out SmsMessage response )
        {
            if ( !ShouldProcessMessage( action, message ) )
            {
                response = null;
                return false;
            }

            var mergeObjects = new Dictionary<string, object>
            {
                { "Message", message }
            };
            var responseMessage = action.GetAttributeValue( "Response" ).ResolveMergeFields( mergeObjects, message.FromPerson );

            response = new SmsMessage
            {
                Message = responseMessage.Trim()
            };

            return true;
        }
    }
}
