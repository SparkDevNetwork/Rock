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

// This file contains various shims for Entity Framework Core that are needed
// to help migration from EF6 to EF Core, or to provide compatibility.

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// Various extension methods to provide future compatibility with Entity
    /// Framework Core.
    /// </summary>
    public static class EntityFrameworkCoreShims
    {
        /// <summary>
        /// Returns the timeout (in seconds) set for commands executed.
        /// </summary>
        /// <param name="databaseFacade">The <see cref="System.Data.Entity.Database" /> for the context.</param>
        /// <returns>The timeout, in seconds, or null if no timeout has been set.</returns>
        public static int? GetCommandTimeout( this System.Data.Entity.Database databaseFacade )
        {
            return databaseFacade.CommandTimeout;
        }

        /// <summary>
        /// Sets the timeout (in seconds) to use for commands executed.
        /// </summary>
        /// <param name="databaseFacade">The <see cref="System.Data.Entity.Database" /> for the context.</param>
        /// <param name="timeout">The timeout to use, in seconds.</param>
        public static void SetCommandTimeout( this System.Data.Entity.Database databaseFacade, int? timeout )
        {
            databaseFacade.CommandTimeout = timeout;
        }
    }
}
