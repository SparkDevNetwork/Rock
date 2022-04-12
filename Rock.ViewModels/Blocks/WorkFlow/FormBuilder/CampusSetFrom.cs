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

namespace Rock.ViewModels.Blocks.WorkFlow.FormBuilder
{
    /// <summary>
    /// Defines the way the campus context is set when a form is processed.
    /// </summary>
    public enum CampusSetFrom
    {
        /// <summary>
        /// Use the campus of the current person who is logged in while filling out the form.
        /// </summary>
        CurrentPerson = 0,

        /// <summary>
        /// Use the campus of the person in the Person attribute.
        /// </summary>
        WorkflowPerson = 1,

        /// <summary>
        /// Use the Campus from the "Campus" query string which could be an Id or Guid.
        /// </summary>
        QueryString = 2
    }
}
