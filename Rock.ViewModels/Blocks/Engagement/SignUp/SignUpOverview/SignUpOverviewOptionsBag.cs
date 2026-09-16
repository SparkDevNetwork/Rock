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

namespace Rock.ViewModels.Blocks.Engagement.SignUp.SignUpOverview
{
    /// <summary>
    /// The additional configuration options for the Sign-Up Overview block.
    /// </summary>
    public class SignUpOverviewOptionsBag
    {
        /// <summary>
        /// Gets or sets the unique identifiers of the group types that represent sign-up
        /// projects (the sign-up group type itself plus any group types that inherit from
        /// it). The parent group filter's group picker is limited to these group types.
        /// </summary>
        public List<Guid> SignUpProjectGroupTypeGuids { get; set; }

        /// <summary>
        /// Gets or sets the file name (without extension) to use when exporting the
        /// opportunities grid.
        /// </summary>
        public string ExportFileName { get; set; }
    }
}
