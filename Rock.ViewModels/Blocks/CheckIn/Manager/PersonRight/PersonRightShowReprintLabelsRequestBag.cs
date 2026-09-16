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
namespace Rock.ViewModels.Blocks.CheckIn.Manager.PersonRight
{
    /// <summary>
    /// Input to the ShowReprintLabelsModal block action.
    /// </summary>
    public class PersonRightShowReprintLabelsRequestBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the browser side detected
        /// the Zebra client-printer plugin (Windows / iPad check-in apps).
        /// When true, the printer dropdown includes a "(local printer)" entry.
        /// </summary>
        public bool HasClientPrinter { get; set; }
    }
}
