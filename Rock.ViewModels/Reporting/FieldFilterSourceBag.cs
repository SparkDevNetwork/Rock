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

using Rock.Enums.Reporting;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Reporting
{
    /// <summary>
    /// Describes a single source item an individual can pick from when building
    /// a custom filter. This contains the information required to determine the
    /// name to display, how to identify the source value and any other information
    /// required to build the filter UI.
    /// </summary>
    public class FieldFilterSourceBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of this source item.
        /// </summary>
        /// <value>The unique identifier of this source item.</value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the type of this source item. This indicates which
        /// other properties are valid for inspection.
        /// </summary>
        /// <value>The type of this source item.</value>
        public FieldFilterSourceType Type { get; set; }

        /// <summary>
        /// Gets or sets the attribute if the source type is Attribute.
        /// </summary>
        /// <value>The attribute if the source type is Attribute.</value>
        public PublicAttributeBag Attribute { get; set; }
    }
}
