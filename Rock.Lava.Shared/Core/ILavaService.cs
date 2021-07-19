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
    /// Represents a component that is capable of providing extension services to the Lava library.
    /// </summary>
    public interface ILavaService
    {
        /// <summary>
        /// The friendly name of the service.
        /// </summary>
        string ServiceName { get; }

        /// <summary>
        /// A unique identifier for the service.
        /// </summary>
        Guid ServiceIdentifier { get; }

        /// <summary>
        /// Called when an instance of the service is first initialized.
        /// </summary>
        /// <param name="settings"></param>
        void OnInitialize( object settings );
    }
}
