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

namespace Rock
{
    /// <summary>
    /// Marks the version at which an [Obsolete] item became obsolete. If the HotFix version matters then include it, otherwise only the major version is required (e.g. 1.8, 1.8.5, 1.9.1, 1.10 ).
    /// The process for this is:
    /// At the beginning of a new version, we might update [Obsolete]/[RockObsolete] methods as follows
    ///  + Last N public releases and develop: Warning
    ///  + N + 1 major public releases ago: Error( set[Obsolete] error parameter to true which will prevent it from getting compiled if used)
    ///  + N + 2 major public versions ago:: Delete the obsolete item
    ///  + You can use the CodeGenerator and set the 'Report Obsolete' option to find all those.
    ///
    /// Example: If the last major public release is 1.9, we'll decide how many versions ago that N should be. Let's say that N is 2
    ///   + 1.10 (develop) warnings for new obsolete methods that are added
    ///   + 1.9 warnings
    ///   + 1.8 warnings (if N is 2)
    ///   + 1.7 errors
    ///   + 1.6 delete
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage( AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Delegate, Inherited = false )]
    public class RockObsolete : System.Attribute
    {
        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public string Version { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockObsolete"/> class.
        /// </summary>
        /// <param name="version">The version when this became obsolete (for example, "1.10"). If the HotFix version matters then include it, otherwise only the major version is required (e.g. 1.8, 1.8.5, 1.9.1, 1.10 )</param>
        public RockObsolete( string version )
        {
            Version = version;
        }
    }
}
