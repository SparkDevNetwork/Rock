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
    /// The options that can be passed to the GetWorkflows API action of
    /// the WorkflowPicker control.
    /// </summary>
    public class WorkflowPickerGetWorkflowTypeForWorkflowOptionsBag
    {

        /// <summary>
        /// The GUID of the workflow type that this workflow is part of.
        /// </summary>
        public Guid WorkflowGuid { get; set; }

        /// <summary>
        /// The security grant token to use when performing authorization checks.
        /// </summary>
        public string SecurityGrantToken { get; set; }
    }
}

