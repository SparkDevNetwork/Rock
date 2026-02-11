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

namespace Rock.Lava
{
    /// <summary>
    /// Defines a factory for creating instances of an ILavaEngine using the
    /// specified configuration options. This is normally not required as the
    /// default global Lava Engine instance is sufficient for most scenarios.
    /// </summary>
    internal interface ILavaEngineFactory
    {
        /// <summary>
        /// Creates a new instance of an ILavaEngine using the specified configuration options.
        /// </summary>
        /// <param name="options">The configuration options to use when initializing the engine. May be null.</param>
        /// <returns>An ILavaEngine instance configured according to the provided options.</returns>
        ILavaEngine CreateEngine( LavaEngineConfigurationOptions options );
    }
}
