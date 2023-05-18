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
using System;

namespace Rock.Constants
{
    /// <summary>
    /// This class holds Rock's well known System Setting keys.
    /// </summary>
    [Obsolete( "Use Rock.SystemKey.SystemSetting instead", true )]
    [RockObsolete( "1.10")]
    public static class SystemSettingKeys
    {
        /// <summary>
        /// This system setting's guid represents a unique identifier for each installation of Rock.
        /// The value it stores is the current version of Rock for that installation.
        /// </summary>
        [Obsolete( "Use Rock.SystemKey.SystemSetting.ROCK_INSTANCE_ID instead", true )]
        [RockObsolete( "1.10")]
        public static readonly string ROCK_INSTANCE_ID = Rock.SystemKey.SystemSetting.ROCK_INSTANCE_ID;

        /// <summary>
        /// Holds the System Setting key for the sample data load date/time.
        /// </summary>
        [Obsolete( "Use Rock.SystemKey.SystemSetting.SAMPLEDATA_DATE instead", true )]
        [RockObsolete( "1.10")]
        public static readonly string SAMPLEDATA_DATE = Rock.SystemKey.SystemSetting.SAMPLEDATA_DATE;
    }
}