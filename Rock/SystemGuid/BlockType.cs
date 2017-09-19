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
    /// System Blocks.  NOTE: Some of these are referenced in Migrations to avoid string-typos.
    /// </summary>
    public class BlockType
    {
        /// <summary>
        /// Gets the Plugin Manager guid
        /// </summary>
        public const string PLUGIN_MANAGER  = "F80268E6-2625-4565-AA2E-790C5E40A119";

        /// <summary>
        /// HTML Content Block Type Guid
        /// </summary>
        public const string HTML_CONTENT = "19B61D65-37E3-459F-A44F-DEF0089118A3";

        /// <summary>
        /// Page Menu Block Type Guid
        /// </summary>
        public const string PAGE_MENU = "CACB9D1A-A820-4587-986A-D66A69EE9948";

        /// <summary>
        /// Communication Detail Block Type Guid
        /// </summary>
        public const string COMMUNICATION_DETAIL = "CEDC742C-0AB3-487D-ABC2-77A0A443AEBF";

        /// <summary>
        /// Communication Entry (Simple) Block Type Guid
        /// </summary>
        public const string COMMUNICATION_ENTRY = "D9834641-7F39-4CFA-8CB2-E64068127565";

        /// <summary>
        /// Content Channel View Block Type Guid
        /// </summary>
        public const string CONTENT_CHANNEL_VIEW = "143A2345-3E26-4ED0-A2FE-42AAF11B4C0F";
    }
}