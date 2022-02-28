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
using System.Threading.Tasks;

namespace Rock.Communication
{
    /// <summary>
    /// In order to take advantage of async your Medium Component should implement this interface.
    /// </summary>
    public interface IAsyncMediumComponent
    {
        /// <summary>
        /// Sends the asynchronous.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <returns></returns>
        Task SendAsync( Model.Communication communication );

        /// <summary>
        /// Sends the asynchronous.
        /// </summary>
        /// <param name="rockMessage">The rock message.</param>
        /// <returns></returns>
        Task<SendMessageResult> SendAsync( RockMessage rockMessage );
    }
}
