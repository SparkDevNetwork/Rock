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
namespace Rock.SystemKey
{
    /// <summary>
    /// Keys for metadata values that can be associated with various entities.
    /// These are used with <see cref="ExtensionMethods.GetMetadataValue(Data.IEntity, string, Data.RockContext)"/>
    /// and related methods.
    /// </summary>
    public class MetadataKey
    {
        /// <summary>
        /// The metadata key for storing the entity usage JSON. This should
        /// be stored and retrieved using a list of <see cref="Core.LinkageSummary"/>
        /// objects.
        /// </summary>
        public const string EntityUsage = "core.entityUsage";

        /// <summary>
        /// The metadata key for storing the JSON array of media element identifiers
        /// used by a content channel item. This should be stored and retrieved as
        /// a list of <see cref="int"/> values representing media element identifiers.
        /// </summary>
        public const string MediaElements = "core.mediaElements";

        /// <summary>
        /// The metadata key, on the platform mobile application Site, for the last builder
        /// ladder version that completed, stored as a version string such as "21.0". Written
        /// by <see cref="Mobile.PlatformMobileAppBuilder"/> inside each version's transaction.
        /// </summary>
        public const string PlatformMobileAppVersion = "core.platformMobileAppVersion";

        /// <summary>
        /// The metadata key, on the platform mobile application Site, for the JSON report of
        /// the last Build, Update or Repair run, stored as a
        /// <see cref="Mobile.PlatformMobileAppRunReport"/>.
        /// </summary>
        public const string PlatformMobileAppLastRun = "core.platformMobileAppLastRun";

        /// <summary>
        /// The metadata key, on the platform mobile application Site, for the hash of the
        /// campus payload last published to the church directory. See
        /// <see cref="Mobile.PlatformMobileAppCampusPayload"/>.
        /// </summary>
        public const string PlatformMobileAppCampusHash = "core.platformMobileAppCampusHash";
    }
}