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
namespace Rock.Enums.Mobile
{
    /// <summary>
    /// The kind of run the platform mobile application builder performed. All three run the
    /// same idempotent steps; they differ only in which ladder versions are run.
    /// </summary>
    public enum PlatformMobileAppRunMode
    {
        /// <summary>
        /// The platform Site did not exist, so every ladder version ran from the beginning.
        /// </summary>
        Build = 0,

        /// <summary>
        /// Only the ladder versions above the stamp ran.
        /// </summary>
        Update = 1,

        /// <summary>
        /// The stamp was already current, so every ladder version ran again from the
        /// beginning, putting back any platform owned field or structure that had changed.
        /// </summary>
        Repair = 2
    }
}
