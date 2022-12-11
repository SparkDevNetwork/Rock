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
namespace Rock.Model
{
    /// <summary>
    /// Gets the status of the Assessment  (i.e. Pending, Complete)
    /// Complete should only be used if the Assessment was actually completed, everything else is pending.
    /// </summary>
    [Enums.EnumDomain( "CRM" )]
    public enum AssessmentRequestStatus
    {
        /// <summary>
        /// Pending Status, anything that wasn't completed.
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Complete Status, only use if assessment was actually completed
        /// </summary>
        Complete = 1,
    }
}
