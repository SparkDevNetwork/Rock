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

namespace Rock.Configuration.ConnectedServices
{
    /// <summary>
    /// Represents the generic wrapper for a service entry bundle.
    /// </summary>
    /// <typeparam name="T">The type for the <see cref="Settings"/> property.</typeparam>
    internal class ServiceEntryBundle<T>
        where T : class
    {
        /// <summary>
        /// The unique identifier for the bundle.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The name of the bundle.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The configuration settings for the bundle.
        /// </summary>
        public T Settings { get; set; }
    }
}
