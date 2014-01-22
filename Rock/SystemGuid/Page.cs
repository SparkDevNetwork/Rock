// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    /// System Pages.  NOTE: Some of these are referenced in Migrations to avoid string-typos.
    /// </summary>
    public static class Page
    {
        /// <summary>
        /// Gets the Plugin Settings guid
        /// </summary>
        public const string PLUGIN_SETTINGS= "1AFDA740-8119-45B8-AF4D-58856D469BE5";

        /// <summary>
        /// Gets the Plugin Manager guid
        /// </summary>
        public const string PLUGIN_MANAGER= "B13FCF9A-FAE5-4E53-AF7C-32DF9CA5AAE3";
    }
}