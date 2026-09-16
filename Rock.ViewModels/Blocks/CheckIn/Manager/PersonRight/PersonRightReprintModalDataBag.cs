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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.CheckIn.Manager.PersonRight
{
    /// <summary>
    /// Response from the ShowReprintLabelsModal block action. Describes which
    /// of the two label modals should open, plus the list of labels and
    /// printers to populate in the modal.
    /// </summary>
    public class PersonRightReprintModalDataBag
    {
        /// <summary>
        /// Gets or sets an error / status message that should be surfaced to
        /// the user (e.g. "No labels were found for re-printing."). Rendered
        /// as a modal alert; when set, the modals do not open.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the modal mode. "Legacy" opens the legacy labels
        /// modal, "NextGen" opens the next-gen labels modal, null when there
        /// is nothing to show (see <see cref="ErrorMessage"/>).
        /// </summary>
        public string Mode { get; set; }

        /// <summary>
        /// Gets or sets the label options rendered in the modal's check-box
        /// list. For legacy, Value is the label file guid; for next-gen,
        /// Value is the label-type value.
        /// </summary>
        public List<ListItemBag> Labels { get; set; }

        /// <summary>
        /// Gets or sets the printer options for the modal's dropdown. The
        /// list includes a leading empty option and, when a Zebra client
        /// printer is available, a "(local printer)" entry whose value is
        /// <see cref="System.Guid.Empty"/>.
        /// </summary>
        public List<ListItemBag> Printers { get; set; }

        /// <summary>
        /// Gets or sets the printer Guid (as a string) that should be
        /// pre-selected when the modal opens, read from the check-in manager
        /// cookie. Null / empty leaves the dropdown unselected.
        /// </summary>
        public string SelectedPrinterGuid { get; set; }
    }
}
