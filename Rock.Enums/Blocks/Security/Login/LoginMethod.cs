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
namespace Rock.Enums.Blocks.Security.Login
{
    /// <summary>
    /// The login method in the Login block.
    /// </summary>
    public enum LoginMethod
    {
        /// <summary>
        /// The internal database login method
        /// </summary>
        InternalDatabase = 0,

        /// <summary>
        /// The passwordless login method
        /// </summary>
        Passwordless = 1
    }
}
