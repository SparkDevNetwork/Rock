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

namespace Rock.SystemGuid
{
    /// <summary>
    /// Guids for System Settings. NOTE: Some of these are referenced in Migrations to avoid string-typos.
    /// </summary>
    [Obsolete("This functionality is no longer used.")]
    public static class SystemSetting
    {
        /// <summary>
        /// Do Not Disturb Start Setting
        /// </summary>
        [Obsolete("This functionality is no longer used.")]
        public const string DO_NOT_DISTURB_START = "4A558666-32C7-4490-B860-0F41358E14CA";

        /// <summary>
        /// Do Not Disturb End Setting
        /// </summary>
        [Obsolete("This functionality is no longer used.")]
        public const string DO_NOT_DISTURB_END = "661802FC-E636-4CE2-B75A-4AC05595A347";

        /// <summary>
        /// Do Not Disturb Active Setting
        /// </summary>
        [Obsolete("This functionality is no longer used.")]
        public const string DO_NOT_DISTURB_ACTIVE = "1BE30413-5C90-4B78-B324-BD31AA83C002";
    }
}
