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

/*
    The original location of this enum was Rock/Utility/Enums/ElevatedSecurityLevel.cs
    in the Rock project. It was relocated to Rock.Enums/Security/ in Rock 20.X so that
    Rock.ViewModels (which references Rock.Enums but not Rock.dll) can surface the type
    on bag definitions. The Rock.Utility.Enums namespace is preserved so existing source
    references compile unchanged, and a TypeForwardedTo attribute in Rock's AssemblyInfo
    keeps plugins compiled against the old Rock.dll resolving the type at runtime.

    The [EnumDomain] attribute is fully-qualified as [Rock.Enums.EnumDomain(...)] to
    avoid a name-resolution conflict with sibling files in this assembly that live in
    namespace Rock.Utility (Connection/CreateConnectionRequestOptions.cs,
    Connection/FamilyLimits.cs, Core/TimeIntervalUnit.cs). Without the qualifier the
    unqualified token "Enums" inside namespace Rock.Utility resolves to the new sub-
    namespace Rock.Utility.Enums first, which doesn't contain EnumDomain.
*/

namespace Rock.Utility.Enums
{
    /// <summary>
    /// Used by the group to determine what AccountProtectionProfile a
    /// Person should be assigned.
    /// </summary>
    [Rock.Enums.EnumDomain( "Security" )]
    public enum ElevatedSecurityLevel
    {
        /// <summary>
        /// The group members of this type will get an AccountProtectionProfile of Low.
        /// </summary>
        None = 0,

        /// <summary>
        /// The group members of this type will get an AccountProtectionProfile of High.
        /// </summary>
        High = 1,

        /// <summary>
        /// The group members of this type will get an AccountProtectionProfile of Extreme.
        /// </summary>
        Extreme = 2,
    }
}
