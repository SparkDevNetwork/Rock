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
    /// The state of the platform mobile application in this Rock database, compared with the
    /// builder ladder compiled into the running Rock. It decides which single builder action
    /// the control panel offers.
    /// </summary>
    public enum PlatformMobileAppBuildState
    {
        /// <summary>
        /// The platform Site does not exist yet. The builder offers Build.
        /// </summary>
        NotBuilt = 0,

        /// <summary>
        /// The stamp is below the top of the ladder, so a newer Rock carries definition
        /// changes this database has not applied. The builder offers Update.
        /// </summary>
        UpdateAvailable = 1,

        /// <summary>
        /// The stamp equals the top of the ladder. The builder offers Repair.
        /// </summary>
        Current = 2,

        /// <summary>
        /// The stamp is above the top of the ladder: the database is ahead of the Rock code,
        /// after a Rock downgrade or a newer database restored onto an older Rock. The builder
        /// offers nothing, because running an older ladder would undo newer work.
        /// </summary>
        AheadOfCode = 3
    }
}
