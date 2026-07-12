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

using Rock.Model;


namespace Rock.ViewModels.Blocks.Event.RegistrantDetail
{
    /// <summary>
    /// Describes a registration template fee definition sent to the client
    /// to render the appropriate fee controls.
    /// </summary>
    public class RegistrationTemplateFeeBag
    {
        /// <summary>
        /// Gets or sets the fee identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the display name of this fee.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the fee type, which controls whether one or multiple
        /// fee items are rendered.
        /// </summary>
        public RegistrationFeeType FeeType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the registrant may select
        /// a quantity greater than one. When false, the control renders as a checkbox.
        /// </summary>
        public bool AllowMultiple { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the fee control should be
        /// hidden entirely when no items remain. When <c>false</c>, the control
        /// is shown but disabled instead.
        /// </summary>
        public bool HideWhenNoneRemaining { get; set; }

        /// <summary>
        /// Gets or sets the selectable items for this fee.
        /// </summary>
        public List<RegistrationTemplateFeeItemBag> Items { get; set; }
    }
}
