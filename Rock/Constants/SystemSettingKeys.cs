// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
namespace Rock.Constants
{
    /// <summary>
    /// This class holds Rock's well known System Setting keys.
    /// </summary>
    public static class SystemSettingKeys
    {
        /// <summary>
        /// This system setting's guid represents a unique identifier for each installation of Rock.
        /// The value it stores is the current version of Rock for that installation.
        /// </summary>
        public static readonly string ROCK_INSTANCE_ID = "RockInstanceId";

        /// <summary>
        /// This system setting is the guid for the organization's location record.
        /// </summary>
        public static readonly string ORG_LOC_GUID = "com.rockrms.orgLocatoinGuid";
        
        /// <summary>
        /// This system setting is the state for the organization's location record.
        /// </summary>
        public static readonly string ORG_LOC_STATE = "com.rockrms.orgLocationState";

        /// <summary>
        /// This system setting is the country for the organization's location record.
        /// </summary>
        public static readonly string ORG_LOC_COUNTRY = "com.rockrms.orgLocationCountry";

        /// <summary>
        /// Holds the System Setting key for the sample data load date/time.
        /// </summary>
        public static readonly string SAMPLEDATA_DATE = "com.rockrms.sampledata.datetime";
    }
}