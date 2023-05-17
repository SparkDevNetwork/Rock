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
    /// The sort property
    /// </summary>
    [Enums.EnumDomain( "Connection" )]
    public enum ConnectionRequestViewModelSortProperty
    {
        /// <summary>
        /// The requestor
        /// </summary>
        Requestor,

        /// <summary>
        /// The requestor desc
        /// </summary>
        RequestorDesc,

        /// <summary>
        /// The connector
        /// </summary>
        Connector,

        /// <summary>
        /// The connector desc
        /// </summary>
        ConnectorDesc,

        /// <summary>
        /// The date added
        /// </summary>
        DateAdded,

        /// <summary>
        /// The date added desc
        /// </summary>
        DateAddedDesc,

        /// <summary>
        /// The last activity
        /// </summary>
        LastActivity,

        /// <summary>
        /// The last activity desc
        /// </summary>
        LastActivityDesc,

        /// <summary>
        /// The order
        /// </summary>
        Order,

        /// <summary>
        /// The campus
        /// </summary>
        Campus,

        /// <summary>
        /// The campus desc
        /// </summary>
        CampusDesc,

        /// <summary>
        /// The group
        /// </summary>
        Group,

        /// <summary>
        /// The group desc
        /// </summary>
        GroupDesc,

        /// <summary>
        /// The status
        /// </summary>
        Status,

        /// <summary>
        /// The status desc
        /// </summary>
        StatusDesc,

        /// <summary>
        /// The state
        /// </summary>
        State,

        /// <summary>
        /// The state desc
        /// </summary>
        StateDesc
    }
}
