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
namespace Rock.Enums.Cms
{
    /// <summary>
    /// The outcome of the last attempt to persist a dataset.
    /// </summary>
    public enum PersistedDatasetStatus
    {
        /// <summary>
        /// The status of the dataset is unknown or has not been attempted to be persisted yet.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The dataset was successfully persisted and is ready to be used.
        /// </summary>
        Ready = 1,

        /// <summary>
        /// The attempt to persist the dataset failed. The dataset is not ready to be used and may need to be re-attempted.
        /// </summary>
        Failed = 2,
    }
}
