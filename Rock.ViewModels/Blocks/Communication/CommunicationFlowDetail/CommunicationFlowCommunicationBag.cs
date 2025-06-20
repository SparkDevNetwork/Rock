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

using Rock.Enums.Communication;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Communication.CommunicationFlowDetail
{
    /// <summary>
    /// The communication flow communication details for the Communication Flow Detail block.
    /// </summary>
    public class CommunicationFlowCommunicationBag
    {
        public Guid Guid { get; set; }

        public string Name { get; set; }

        public int DaysToWait { get; set; }

        public TimeSpan TimeToSend { get; set; }

        public CommunicationType CommunicationType { get; set; }

        public CommunicationFlowDetailCommunicationTemplateBag CommunicationTemplate { get; set; }

        public int Order { get; set; }

        public string TestEmailAddress { get; set; }

        public string TestSmsPhoneNumber { get; set; }
    }
}
