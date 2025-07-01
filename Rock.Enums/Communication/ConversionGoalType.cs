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

namespace Rock.Enums.Communication
{
    /// <summary>
    /// A conversion goal type for a Communication Flow. This is used to track the success of a Communication Flow in achieving its goals.
    /// </summary>
    public enum ConversionGoalType
    {
        /// <summary>
        /// Recipients have completed a Workflow form. This means they initiated a workflow and it was marked Completed.
        /// </summary>
        CompletedForm = 1,

        /// <summary>
        /// Recipients have registered for an event.
        /// </summary>
        Registered = 2,

        /// <summary>
        /// Recipients have joined a Group of a given Group Type.
        /// </summary>
        JoinedGroupType = 3,

        /// <summary>
        /// Recipients have joined a Group.
        /// </summary>
        JoinedGroup = 4,

        /// <summary>
        /// Recipients have entered a Data View.
        /// </summary>
        EnteredDataView = 5,

        /// <summary>
        /// Recipients have taken a step in a Workflow.
        /// </summary>
        TookStep = 6
    }
}
