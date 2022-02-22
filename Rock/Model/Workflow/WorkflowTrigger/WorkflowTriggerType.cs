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

namespace Rock.Model
{
    /// <summary>
    /// Type of workflow trigger
    /// </summary>
    public enum WorkflowTriggerType
    {
        /// <summary>
        /// Pre Save
        /// </summary>
        PreSave = 0,

        /// <summary>
        /// Post Save
        /// </summary>
        PostSave = 1,

        /// <summary>
        /// Pre Delete
        /// </summary>
        PreDelete = 2,

        /// <summary>
        /// Post Delete
        /// </summary>
        PostDelete = 3,

        /// <summary>
        /// Immediate Post Save
        /// </summary>
        ImmediatePostSave = 4,

        /// <summary>
        /// Post Add
        /// </summary>
        PostAdd = 5,
    }
}
