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

namespace Rock.Enums.Configuration
{
    /// <summary>
    /// A database server platform that is capable of hosting an instance of
    /// a Rock database.
    /// </summary>
    public enum DatabasePlatform
    {
        /// <summary>
        /// The database is unknown.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The database is an edition of Microsoft SQL Server.
        /// </summary>
        SqlServer = 1,

        /// <summary>
        /// The database is hosted on the Azure platform.
        /// </summary>
        AzureSql = 2,

        /// <summary>
        /// The database is hosted on an unspecified platform.
        /// </summary>
        Other = 3
    }
}
