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

using Rock.ViewModels.Utility;

using System;
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Engagement.StepTypeDetail
{
    /// <summary>
    /// The Bag used to transfer step types
    /// </summary>
    public class StepTypeTransferBag
    {
        /// <summary>
        /// Gets or sets the IdKey for the Step Type that is being transferred.
        /// </summary>
        /// <value>
        /// The Step Type IdKey.
        /// </value>
        public string StepTypeIdKey { get; set; }

        /// <summary>
        /// Gets or sets a Step Program.
        /// </summary>
        /// <value>
        /// The step program.
        /// </value>
        public Guid? TargetStepProgramGuid { get; set; }

        /// <summary>
        /// Gets or sets the step status mappings for the transfer.
        /// </summary>
        /// <value>
        /// The step status mappings.
        /// </value>
        public Dictionary<string, string> StepStatusMappings { get; set; }
    }
}
