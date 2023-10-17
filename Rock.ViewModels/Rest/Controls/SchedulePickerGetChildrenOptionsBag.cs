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

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// The options that can be passed to the GetChildren API action of
    /// the RegistrationTemplatePicker control.
    /// </summary>
    public class SchedulePickerGetChildrenOptionsBag
    {
        /// <summary>
        /// The parent unique identifier whose children are to
        /// be retrieved. If null then the root items are being requested.
        /// </summary>
        public Guid? ParentGuid { get; set; }

        /// <summary>
        /// Whether to include schedules marked as inactive in the results.
        /// </summary>
        public bool IncludeInactiveItems { get; set; } = false;

        /// <summary>
        /// Whether to include schedules marked as private in the results.
        /// </summary>
        public bool IncludePrivateItems { get; set; } = false;

        /// <summary>
        /// Gets or sets the security grant token to use when performing
        /// authorization checks.
        /// </summary>
        public string SecurityGrantToken { get; set; }
    }
}

