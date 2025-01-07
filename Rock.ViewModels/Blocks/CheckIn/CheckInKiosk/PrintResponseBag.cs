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

using Rock.ViewModels.CheckIn.Labels;

namespace Rock.ViewModels.Blocks.CheckIn.CheckInKiosk
{
    /// <summary>
    /// The response bag for various print operations in the check-in kiosk.
    /// </summary>
    public class PrintResponseBag
    {
        /// <summary>
        /// Any error messages that were encountered while printing the labels.
        /// </summary>
        public List<string> ErrorMessages { get; set; }

        /// <summary>
        /// Contains all legacy labels that need to be printed on the client.
        /// </summary>
        public List<LegacyClientLabelBag> LegacyLabels { get; set; }

        /// <summary>
        /// Contains all new labels that need to be printed on the client.
        /// </summary>
        public List<ClientLabelBag> Labels { get; set; }
    }
}
