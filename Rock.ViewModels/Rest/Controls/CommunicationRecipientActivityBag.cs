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

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// A bag that contains information about a specific activity for a communication recipient.
    /// </summary>
    public class CommunicationRecipientActivityBag
    {
        /// <summary>
        /// The activity that took place.
        /// </summary>
        public CommunicationRecipientActivity Activity { get; set; }

        /// <summary>
        /// The activity datetime.
        /// </summary>
        public DateTime ActivityDateTime { get; set; }

        /// <summary>
        /// A description of the activity.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// A tooltip containing more information about the activity.
        /// </summary>
        public string Tooltip { get; set; }
    }
}
