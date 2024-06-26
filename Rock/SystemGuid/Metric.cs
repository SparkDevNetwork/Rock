﻿// <copyright>
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
namespace Rock.SystemGuid
{
    /// <summary>
    /// System Metrics
    /// </summary>
    public class Metric
    {
        #region Hosting Metrics

        /// <summary>
        /// Hosting Metric hard connects per second Guid
        /// </summary>
        public const string HOSTING_HARD_CONNECTS_PER_SECOND = "64D538D0-EE05-4646-91F5-EBE06460BDAB";

        /// <summary>
        /// Hosting Metric number of active connections Guid
        /// </summary>
        public const string HOSTING_NUMBER_OF_ACTIVE_CONNECTIONS = "68C54F46-A99E-4DD1-91CA-FC5941E6CFBE";

        /// <summary>
        /// Hosting Metric number of free connections Guid
        /// </summary>
        public const string HOSTING_NUMBER_OF_FREE_CONNECTIONS = "8A1F73DD-4275-47C0-AF2A-6EABDA06E3C7";

        /// <summary>
        /// Hosting Metric soft connects per second Guid
        /// </summary>
        public const string HOSTING_SOFT_CONNECTS_PER_SECOND = "F90F9446-8754-4001-887C-1AB920968C6D";

        #endregion

        #region Tithing Overview

        /// <summary>
        /// Tithing overview by campus metric Guid.
        /// </summary>
        public const string TITHING_OVERVIEW_BY_CAMPUS_METRIC_GUID = "F4951A42-9F71-4CB1-A46E-2A7ED84CD923";

        /// <summary>
        /// Tithing households by campus metric Guid.
        /// </summary>
        public const string TITHING_HOUSEHOLDS_BY_CAMPUS_METRIC_GUID = "2B798177-E8F4-46DB-A1D7-308D63CA519A";

        /// <summary>
        /// Giving households by campus metric Guid.
        /// </summary>
        public const string GIVING_HOUSEHOLDS_BY_CAMPUS_METRIC_GUID = "B5BFAB51-9B46-4E7E-992E-B0119E4D25EC";

        #endregion
    }
}
