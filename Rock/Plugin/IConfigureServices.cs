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

using Microsoft.Extensions.DependencyInjection;

namespace Rock.Plugin
{
    /// <summary>
    /// Defines a contract that a plugin can implement to configure services
    /// that will be added to the Rock dependency injection container.
    /// </summary>
    internal interface IConfigureServices
    {
        /// <summary>
        /// Configures the application's service collection with required services and dependencies.
        /// </summary>
        /// <remarks>
        /// This method is called early during the application startup. As such,
        /// nothing is available for use except the <paramref name="services"/>
        /// parameter. Do not attempt to use cache, database, or other such calls.
        /// </remarks>
        /// <param name="services">The service collection to which application services are added.</param>
        void ConfigureServices( IServiceCollection services );
    }
}
