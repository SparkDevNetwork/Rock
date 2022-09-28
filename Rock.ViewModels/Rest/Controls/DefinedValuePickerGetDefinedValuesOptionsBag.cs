﻿// <copyright>
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
    /// The options that can be passed to the GetDefinedValues API action of
    /// the DefinedValuePicker control.
    /// </summary>
    public class DefinedValuePickerGetDefinedValuesOptionsBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the defined type to load values from.
        /// </summary>
        /// <value>The unique identifier of the defined type to load values from.</value>
        public Guid DefinedTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether inactive values should be included.
        /// </summary>
        /// <value><c>true</c> if inactive values should be included; otherwise, <c>false</c>.</value>
        public bool IncludeInactive { get; set; }

        /// <summary>
        /// Gets or sets the security grant token to use when performing
        /// authorization checks.
        /// </summary>
        /// <value>The security grant token.</value>
        public string SecurityGrantToken { get; set; }
    }
}
