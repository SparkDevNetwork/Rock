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
using System.Linq;
using Rock.Communication.SmsActions;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class SmsActionService
    {
        /// <summary>
        /// Processes the incoming message.
        /// </summary>
        /// <param name="message">The message received by the communications component.</param>
        /// <returns>If not null, identifies the response that should be sent back to the sender.</returns>
        static public SmsMessage ProcessIncomingMessage( SmsMessage message )
        {
            SmsMessage response = null;

            var smsActions = SmsActionCache.All()
                .Where( a => a.IsActive )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Id );

            foreach ( var smsAction in smsActions )
            {
                if ( smsAction.SmsActionComponent != null )
                {
                    //
                    // Check if the action wants to process this message.
                    //
                    if ( !smsAction.SmsActionComponent.ShouldProcessMessage( smsAction, message ) )
                    {
                        continue;
                    }

                    //
                    // Process the message and use either the response returned by the action
                    // or the previous response we already had.
                    //
                    response = smsAction.SmsActionComponent.ProcessMessage( smsAction, message ) ?? response;

                    //
                    // If the action is set to not continue after processing then stop.
                    //
                    if ( !smsAction.ContinueAfterProcessing )
                    {
                        return response;
                    }
                }
            }

            return response;
        }
    }
}
