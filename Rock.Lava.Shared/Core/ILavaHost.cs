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

namespace Rock.Lava
{
    /// <summary>
    /// Represents a Lava Service component that provides information about the hosting environment.
    /// </summary>
    public interface ILavaHost : ILavaService
    {
        /// <summary>
        /// Returns a named configuration value from the host environment.
        /// </summary>
        /// <param name="settingKey"></param>
        /// <returns></returns>
        bool TryGetConfigurationSetting( string settingKey, out string value );

        /// <summary>
        /// Resolve a relative URL to an absolute URL for the host application environment.
        /// </summary>
        /// <param name="inputUrl"></param>
        /// <returns></returns>
        string ResolveUrl( string inputUrl );
    }
}
