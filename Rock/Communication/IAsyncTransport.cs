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
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rock.Communication
{
    /// <summary>
    /// This interface will need to be implemented by any transport that want to use the parallelization features.
    /// </summary>
    public interface IAsyncTransport
    {
        /// <summary>
        /// Gets the maximum parallelization.
        /// </summary>
        /// <value>
        /// The maximum parallelization.
        /// </value>
        int MaxParallelization { get; }

        /// <summary>
        /// Sends the asynchronous.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <param name="mediumEntityTypeId">The medium entity type identifier.</param>
        /// <param name="mediumAttributes">The medium attributes.</param>
        /// <returns></returns>
        Task SendAsync( Model.Communication communication, int mediumEntityTypeId, Dictionary<string, string> mediumAttributes );

        /// <summary>
        /// Sends the asynchronous.
        /// </summary>
        /// <param name="rockMessage">The rock message.</param>
        /// <param name="mediumEntityTypeId">The medium entity type identifier.</param>
        /// <param name="mediumAttributes">The medium attributes.</param>
        /// <returns></returns>
        Task<SendMessageResult> SendAsync( RockMessage rockMessage, int mediumEntityTypeId, Dictionary<string, string> mediumAttributes );
    }
}
