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

namespace Rock.Transactions
{
    /// <summary>
    /// Represents a Transaction that runs asynchronously
    /// </summary>
    [Obsolete( "Use Rock.Tasks instead of transactions" )]
    [RockObsolete( "1.13" )]
    public interface ITransactionWithProgress : ITransaction
    {
        /// <summary>
        /// Gets the progress. Should report between 0 and 100 to represent the percent complete or
        /// null if the progress cannot be calculated.
        /// </summary>
        /// <value>
        /// The progress.
        /// </value>
        Progress<int?> Progress { get; }
    }
}
