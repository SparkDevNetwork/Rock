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

namespace Rock.ViewModels.Blocks.Group.GroupHistory
{
    /// <summary>
    /// A single value change within a Group History timeline event, such as
    /// one property that was set or changed.
    /// </summary>
    public class GroupHistoryChangeBag
    {
        /// <summary>
        /// Gets or sets the friendly name of the value that changed, such as
        /// "Description" or "Topic".
        /// </summary>
        public string ValueName { get; set; }

        /// <summary>
        /// Gets or sets the new value. Null or empty when the value was
        /// removed or is sensitive.
        /// </summary>
        public string NewValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the value had no previous
        /// value, meaning it was set for the first time rather than changed.
        /// </summary>
        public bool IsInitialValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the changed value is
        /// sensitive. Sensitive values are described without displaying the
        /// old or new value.
        /// </summary>
        public bool IsSensitive { get; set; }
    }
}
