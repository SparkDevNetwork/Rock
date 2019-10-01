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
namespace Rock.Utility.SparkDataApi
{
    /// <summary>
    /// Group name and transaction key structure
    /// </summary>
    public class GroupNameTransactionKey
    {
        /// <summary>
        /// Gets or sets the group name.
        /// </summary>
        /// <value>
        /// The group name.
        /// </value>
        public string GroupName { get; set; }

        /// <summary>
        /// Gets or sets the transaction key.
        /// </summary>
        /// <value>
        /// The transaction key.
        /// </value>
        public string TransactionKey { get; set; }
    }
}
