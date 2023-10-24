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

namespace Rock.Utility
{
    /// <summary>
    /// Identifies the various REST v2 API endpoints that can be generated.
    /// </summary>
    [Flags]
    internal enum CodeGenerateRestEndpoint
    {
        /// <summary>
        /// No endpoints will be generated.
        /// </summary>
        None = 0x0000,

        /// <summary>
        /// The "PostItem" endpoint to create a new item will be generated.
        /// </summary>
        CreateItem = 0x0001,

        /// <summary>
        /// The "GetItem" endpoint to get a single existing item will be generated.
        /// </summary>
        ReadItem = 0x0002,

        /// <summary>
        /// The "PutItem" endpoint to replace an existing item will be generated.
        /// </summary>
        ReplaceItem = 0x0004,

        /// <summary>
        /// The "PatchItem" endpoint to update an existing item will be generated.
        /// </summary>
        UpdateItem = 0x0008,

        /// <summary>
        /// The "DeleteItem" endpoint to delete an existing item will be generated.
        /// </summary>
        DeleteItem = 0x0010,

        /// <summary>
        /// The "GetAttributeValues" endpoint to get all attribute values for
        /// an item will be generated.
        /// </summary>
        ReadAttributeValues = 0x0020,

        /// <summary>
        /// The "PatchAttributeValues" endpoint to update existing attribute values
        /// for an item will be generated.
        /// </summary>
        UpdateAttributeValues = 0x0040,

        /// <summary>
        /// The "GetSearch" and "PostSearch" endpoints to perform an Entity Search
        /// of the items will be generated.
        /// </summary>
        Search = 0x0080,

        #region Composite Values

        /// <summary>
        /// All endpoints that provide read-only access to items will be generated.
        /// </summary>
        ReadOnly = ReadItem | ReadAttributeValues | Search,

        /// <summary>
        /// All endpoints will be generated.
        /// </summary>
        All = CreateItem | ReadItem | ReplaceItem | UpdateItem | DeleteItem | ReadAttributeValues | UpdateAttributeValues | Search

        #endregion
    }
}
