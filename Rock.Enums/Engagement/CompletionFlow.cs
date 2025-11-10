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
using System.ComponentModel;

namespace Rock.Enums.Engagement
{
    /// <summary>
    /// Defines how the steps in a program are ordered and how prerequisites are handled.
    /// </summary>
    public enum CompletionFlow
    {
        /// <summary>
        /// There will be no concept of a pre-req on linear required. Existing ones would be removed. The pre-req is the order.
        /// </summary>
        [Description( "Linear (Required)" )]
        LinearRequired = 0,

        /// <summary>
        /// The ordering will only be used for suggestions and the pre-req logic would be displayed and enforced.
        /// </summary>
        [Description( "Linear (Preferred)" )]
        LinearPreferred = 1,

        /// <summary>
        /// Order is just for display purposes. We'll still show the pre-req and enforce them though.
        /// </summary>
        [Description( "Non-Linear" )]
        NonLinear = 2,
    }
}
