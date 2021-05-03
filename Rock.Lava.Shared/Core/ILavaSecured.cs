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
    /// A Lava template element that requires specific permission to execute.
    /// </summary>
    public interface ILavaSecured
    {
        /// <summary>
        /// The key that uniquely identifies the permission required to execute this command.
        /// </summary>
        /// <remarks>
        /// This key is recorded when the permission is granted, and it should not be changed without a corresponding data migration.
        /// </remarks>
        string RequiredPermissionKey { get; }
    }
}
