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

namespace Rock.ViewModels.Reporting
{
    /// <summary>
    /// A single rule for a field filter. this defines the source to obtain the
    /// left-hand value from, the right hand value, and the operator to use when
    /// comparing them.
    /// </summary>
    public class FieldFilterRuleBag
    {
        /// <summary>
        /// The unique identifier of this rule.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// The type of comparison to use when comparing the source value (left-hand
        /// side) and <see cref="Value"/> (right-hand side).
        /// </summary>
        public int ComparisonType { get; set; }

        /// <summary>
        /// The right-hand side of the comparison to use when executing the rule.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The source location for where to get the left-hand side value.
        /// </summary>
        public int SourceType { get; set; }

        /// <summary>
        /// The attribute unique identifier to use as the left-hand side value
        /// if <see cref="SourceType"/> specifies an Attribute.
        /// </summary>
        public Guid? AttributeGuid { get; set; }
    }
}
