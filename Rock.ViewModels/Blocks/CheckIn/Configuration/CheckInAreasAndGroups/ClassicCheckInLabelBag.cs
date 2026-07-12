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

namespace Rock.ViewModels.Blocks.CheckIn.Configuration.CheckInAreasAndGroups
{
    /// <summary>
    /// A bag that contains information about a classic check-in label for the Check-in Areas and Groups block.
    /// </summary>
    public class ClassicCheckInLabelBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the binary file this label attachment points at.
        /// </summary>
        public Guid BinaryFileGuid { get; set; }

        /// <summary>
        /// Gets or sets the label's display name, sourced from the binary file's name.
        /// </summary>
        public string FileName { get; set; }
    }
}
