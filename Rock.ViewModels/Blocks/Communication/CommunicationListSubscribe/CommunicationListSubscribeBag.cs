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

namespace Rock.ViewModels.Blocks.Communication.CommunicationListSubscribe
{
    /// <summary>
    /// A bag that contains the data needed by the Communication List Subscribe block
    /// to render the list of communication lists and their subscription states.
    /// </summary>
    public class CommunicationListSubscribeBag
    {
        /// <summary>
        /// Gets or sets the list of communication list items that the person
        /// can subscribe to or unsubscribe from.
        /// </summary>
        public List<CommunicationListItemBag> CommunicationLists { get; set; }
    }
}
