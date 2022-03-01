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

namespace Rock.Field
{
    /// <summary>
    /// Identifies the way a field type is going to be used and presented in
    /// the system.
    /// </summary>
    [Flags]
    public enum FieldTypeUsage
    {
        #region Categorization Flags 0x0001 - 0x0080

        /// <summary>
        /// Field type is considered common. It is one that is used extremely
        /// often and should be more easily accessible than others.
        /// </summary>
        Common = 0x0001,

        /// <summary>
        /// Field type is considered advanced. It will get used by normal users
        /// but far less frequently than the common fields.
        /// </summary>
        Advanced = 0x0002,

        /// <summary>
        /// Field type is considered administrative. It should not generally be
        /// used by normal users, though it will likely be used by administrative
        /// and power users.
        /// </summary>
        Administrative = 0x0004,

        #endregion

        #region Visibility Flags 0x0100 - 0x8000

        /// <summary>
        /// Field type is for system use. It should not show up on any lists that
        /// an individual can select from for actual use. Though it might still
        /// need to show up on certain administrative pages, such as setting
        /// security for field types.
        /// </summary>
        System = 0x0100,

        #endregion
    }
}