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

namespace Rock.Field
{
    /// <summary>
    /// Identifies a field type whose security grant rules are safe to give to
    /// anyone that can see the edit control for an attribute of this type,
    /// including anonymous visitors on public pages. Field types should only
    /// implement this if their rules are read-only and limited to the data
    /// already selectable by the attribute's configuration.
    /// </summary>
    /// <remarks>
    /// Attributes of these field types will include their own security grant
    /// token when they are prepared for editing. Field types that only
    /// implement <see cref="ISecurityGrantFieldType"/> rely on the block to
    /// provide a security grant instead.
    /// </remarks>
    internal interface IPublicSecurityGrantFieldType : ISecurityGrantFieldType
    {
    }
}
