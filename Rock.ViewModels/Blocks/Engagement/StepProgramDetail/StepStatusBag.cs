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

namespace Rock.ViewModels.Blocks.Engagement.StepProgramDetail
{
    /// <summary>
    /// 
    /// </summary>
    public class StepStatusBag
    {
        /// <summary>
        /// The Id of the Step Status
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The name of the Step Status
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Is Step Status Active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsCompleteStatus { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string StatusColor { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Guid? Guid { get; set; }
    }
}
