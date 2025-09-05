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
        /// <summary>
        /// 
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int DaysToWait { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public TimeSpan TimeToSend { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public CommunicationType CommunicationType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public CommunicationFlowDetailCommunicationTemplateBag CommunicationTemplate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string TestEmailAddress { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string TestSmsPhoneNumber { get; set; }
    }
}
