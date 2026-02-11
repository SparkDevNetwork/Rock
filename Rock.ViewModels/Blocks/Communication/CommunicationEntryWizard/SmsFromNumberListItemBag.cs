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
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Communication.CommunicationEntryWizard
{
    /// <summary>
    /// Represents a container for SMS "from" number information, including the associated list item and assignment
    /// status for the current person.
    /// </summary>
    public class SmsFromNumberListItemBag : ListItemBag
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmsFromNumberListItemBag"/> class.
        /// </summary>
        /// <param name="listItemBag"></param>
        internal SmsFromNumberListItemBag( ListItemBag listItemBag )
        {
            Category = listItemBag.Category;
            Disabled = listItemBag.Disabled;
            Text = listItemBag.Text;
            Value = listItemBag.Value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether a number is assigned to the current person.
        /// </summary>
        public bool IsNumberAssignedToCurrentPerson { get; set; }
    }
}
