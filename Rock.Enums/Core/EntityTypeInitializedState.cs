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

using System;

namespace Rock.Enums.Core
{
    /// <summary>
    /// The state flags that determine which parts of the entity type have
    /// been initialized internally by Rock.
    /// </summary>
    [Flags]
    public enum EntityTypeInitializedState
    {
        /// <summary>
        /// No initialization of the entity type has been performed yet.
        /// </summary>
        None = 0,

        /// <summary>
        /// The default security for the entity type has been initialized.
        /// </summary>
        DefaultSecurity = 0x0001,

        /// <summary>
        /// The entity type has been fully initialized.
        /// </summary>
        FullyInitialized = DefaultSecurity
    }
}
