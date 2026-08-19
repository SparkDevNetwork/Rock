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

namespace Rock.ViewModels.Blocks.Crm.PersonDetail.EditPerson
{
    /// <summary>
    /// The request sent to the Edit Person block's Save action.
    /// </summary>
    public class EditPersonSaveRequestBag
    {
        /// <summary>
        /// Gets or sets the identifier key of the person to save.
        /// </summary>
        public string PersonIdKey { get; set; }

        /// <summary>
        /// Gets or sets the edited person values.
        /// </summary>
        public EditPersonBag Person { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user has confirmed reassigning a giving
        /// envelope number that is already in use by another person.
        /// </summary>
        public bool IsGivingEnvelopeNumberConfirmed { get; set; }
    }
}
