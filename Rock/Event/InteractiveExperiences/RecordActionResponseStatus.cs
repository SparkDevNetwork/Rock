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

namespace Rock.Event.InteractiveExperiences
{
    /// <summary>
    /// The result of a call to <see cref="Model.InteractiveExperienceAnswerService.RecordActionResponse(int, int, int?, int?, int?, string)"/>.
    /// </summary>
    internal enum RecordActionResponseStatus
    {
        /// <summary>
        /// The response was saved.
        /// </summary>
        Success = 0,

        /// <summary>
        /// The response was not saved due to invalid parameters provided.
        /// </summary>
        InvalidParameters = 1,

        /// <summary>
        /// The response was not saved due to a previous answer already being recorded.
        /// </summary>
        DuplicateResponse = 2
    }
}
