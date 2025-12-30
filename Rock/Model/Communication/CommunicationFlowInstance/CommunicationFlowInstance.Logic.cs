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

namespace Rock.Model
{
    public partial class CommunicationFlowInstance
    {
        /// <summary>
        /// Determines if there are more communications to send or schedule for this Communication Flow Instance.
        /// </summary>
        internal bool GetHasMoreCommunicationsToSendOrSchedule()
        {
            var communicationsToSend = CommunicationFlow?.CommunicationFlowCommunications?.Count ?? 0;
            var communicationsSentOrScheduled = CommunicationFlowInstanceCommunications?.Count ?? 0;

            return communicationsSentOrScheduled < communicationsToSend;
        }
    }
}
