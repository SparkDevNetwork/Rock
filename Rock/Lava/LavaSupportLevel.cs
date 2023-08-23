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
    /// 
    /// </summary>
    [RockObsolete( "1.16" )]
    [Obsolete( "Legacy Lava is no longer supported." )]
    public enum LavaSupportLevel
    {
        /// <summary>
        /// Loads GlobalAttributes as a MergeField and supports the old .AttributeKey syntax
        /// Slower, but supports the old syntax
        /// </summary>
        Legacy,

        /// <summary>
        /// Loads GlobalAttributes as a MergeField and supports the old .AttributeKey syntax, but will log an exception when it detects that the old syntax is used
        /// Slower than NoLegacy, but will help you find any old legacy syntax that you need to clean up
        /// </summary>
        LegacyWithWarning,

        /// <summary>
        /// Does not load the old GlobalAttributes MergeField and does not support the old .AttributeKey syntax and does not try to detect the old syntax. 
        /// Best Performance
        /// </summary>
        NoLegacy
    }
}
