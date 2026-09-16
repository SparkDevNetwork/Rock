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

namespace Rock.ViewModels.Blocks.Reminders.ReminderList
{
    /// <summary>
    /// Request body for the <c>ReassignReminder</c> block action.
    /// </summary>
    public class ReassignReminderRequestBag
    {
        /// <summary>
        /// Gets or sets the hashed reminder identifier being reassigned.
        /// </summary>
        public string ReminderIdKey { get; set; }

        /// <summary>
        /// Gets or sets the PersonAlias guid emitted by the PersonPicker. The
        /// server resolves this to the corresponding PersonAliasId.
        /// </summary>
        public Guid? NewPersonAliasGuid { get; set; }
    }
}
