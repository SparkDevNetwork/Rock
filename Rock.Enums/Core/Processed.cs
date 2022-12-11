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
    /// Represents the state of NCOA
    /// </summary>
    [Enums.EnumDomain( "Core" )]
    public enum Processed
    {
        /// <summary>
        /// NotProcessed
        /// </summary>
        NotProcessed = 0,

        /// <summary>
        /// Complete
        /// </summary>
        Complete = 1,

        /// <summary>
        /// Manual Update Required
        /// </summary>
        ManualUpdateRequired = 2,

        /// <summary>
        /// Manual update required or not processed
        /// </summary>
        ManualUpdateRequiredOrNotProcessed = 3,

        /// <summary>
        /// All records
        /// </summary>
        All = 4
    }
}
