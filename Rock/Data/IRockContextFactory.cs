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

namespace Rock.Data
{
    /// <summary>
    /// Defines a factory for creating <see cref="RockContext" /> instances.
    /// </summary>
    /// <remarks>
    /// This should not be inherited by plugins. New methods or properties may
    /// be added without warning.
    /// </remarks>
    public interface IRockContextFactory
    {
        /// <summary>
        /// Creates a new <see cref="RockContext" /> instance.
        /// </summary>
        /// <remarks>
        /// The caller is responsible for disposing the context; it will not
        /// be disposed by any dependency injection container.
        /// </remarks>
        /// <returns>A new context instance.</returns>
        RockContext CreateRockContext();
    }
}
