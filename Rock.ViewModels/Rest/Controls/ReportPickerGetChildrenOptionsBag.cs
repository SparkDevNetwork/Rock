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
using System.Collections.Generic;

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// The options that can be passed to the GetChildren API action of
    /// the RegistrationTemplatePicker control.
    /// </summary>
    public class ReportPickerGetChildrenOptionsBag
    {
        /// <summary>
        /// The parent unique identifier whose children are to
        /// be retrieved. If null then the root items are being requested.
        /// </summary>
        public Guid? ParentGuid { get; set; }

        /// <summary>
        /// A list of category GUIDs to filter the results.
        /// </summary>
        public List<Guid> IncludeCategoryGuids { get; set; }

        /// <summary>
        /// Guid of an Entity Type to filter results by the reports that relate to this entity type.
        /// </summary>
        public Guid? EntityTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the security grant token to use when performing
        /// authorization checks.
        /// </summary>
        public string SecurityGrantToken { get; set; }
    }
}

