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

namespace Rock.ViewModels.Blocks.Communication.CommunicationEntry
{
    /// <summary>
    /// Bag containing the information for the Communication Entry block to save a metrics reminder.
    /// </summary>
    public class CommunicationEntrySaveMetricsReminderRequestBag
    {
        /// <summary>
        /// Gets or sets the Communication unique identifier.
        /// </summary>
        public Guid CommunicationGuid { get; set; }

        /// <summary>
        /// Gets or sets the number of days to wait after the communication is sent to send the email metrics reminder communication.
        /// </summary>
        public int DaysUntilReminder { get; set; }
    }
}
