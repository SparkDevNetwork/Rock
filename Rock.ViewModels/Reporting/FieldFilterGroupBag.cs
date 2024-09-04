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

using Rock.Model;

namespace Rock.ViewModels.Reporting
{
    /// <summary>
    /// A group of filter rules/expressions that make up a logical comparison group.
    /// </summary>
    public class FieldFilterGroupBag
    {
        /// <summary>
        /// The unique identifier of this filter group.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// The logic operator to use when joining all rules and child-groups in this group.
        /// </summary>
        public FilterExpressionType ExpressionType { get; set; }

        /// <summary>
        /// The collection of rules/expression that make up this group.
        /// </summary>
        public List<FieldFilterRuleBag> Rules { get; set; }

        /// <summary>
        /// The collection of child groups that make up any nested expressions in this group.
        /// </summary>
        public List<FieldFilterGroupBag> Groups { get; set; }
    }
}
