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

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rock.AI.Agent.Tests.Skills.CommunityKnowledgeBaseSkill;

public partial class CommunityKnowledgeBaseSkillTests
{
    #region DetermineRockVersion

    private const string ServerVersion = "20";

    [TestMethod]
    [DataRow( null, false )]
    [DataRow( "", false )]
    [DataRow( "   ", true )]
    public void DetermineRockVersion_WithBlankVersion_UsesServerVersion( string requestedVersion, bool isVersionOverrideAllowed )
    {
        var errorMessage = DetermineRockVersion( requestedVersion, isVersionOverrideAllowed, out var resolvedVersion );

        Assert.IsNull( errorMessage );
        Assert.AreEqual( ServerVersion, resolvedVersion );
    }

    [TestMethod]
    public void DetermineRockVersion_WithOverrideAllowed_UsesRequestedVersion()
    {
        var errorMessage = DetermineRockVersion( "19", true, out var resolvedVersion );

        Assert.IsNull( errorMessage );
        Assert.AreEqual( "19", resolvedVersion );
    }

    [TestMethod]
    [DataRow( "19.1" )]
    [DataRow( "19.0" )]
    [DataRow( "19.0.4" )]
    [DataRow( " 19.1 " )]
    [DataRow( "019" )]
    public void DetermineRockVersion_WithMinorOrPadding_SendsMajorOnly( string requestedVersion )
    {
        var errorMessage = DetermineRockVersion( requestedVersion, true, out var resolvedVersion );

        Assert.IsNull( errorMessage );
        Assert.AreEqual( "19", resolvedVersion );
    }

    [TestMethod]
    [DataRow( "latest", true )]
    [DataRow( "v19", true )]
    [DataRow( "Rock 19", true )]
    [DataRow( ".19", true )]
    [DataRow( "19.", true )]
    [DataRow( "abc", false )]
    public void DetermineRockVersion_WithNonNumericVersion_IsRefused( string requestedVersion, bool isVersionOverrideAllowed )
    {
        var errorMessage = DetermineRockVersion( requestedVersion, isVersionOverrideAllowed, out var resolvedVersion );

        Assert.IsNotNull( errorMessage );
        Assert.IsNull( resolvedVersion );
        StringAssert.Contains( errorMessage, "is not a Rock version" );
    }

    [TestMethod]
    [DataRow( "19" )]
    [DataRow( "21.2" )]
    public void DetermineRockVersion_WithDifferentVersionWhileLocked_IsRefused( string requestedVersion )
    {
        var errorMessage = DetermineRockVersion( requestedVersion, false, out var resolvedVersion );

        // Refused rather than quietly replaced with the server's version, so the
        // agent never answers with one release's facts while believing they
        // describe another.
        Assert.IsNotNull( errorMessage );
        Assert.IsNull( resolvedVersion );
        StringAssert.Contains( errorMessage, $"locked to Rock {ServerVersion}" );
        StringAssert.Contains( errorMessage, "version override is off" );
    }

    [TestMethod]
    [DataRow( "20" )]
    [DataRow( "20.1" )]
    public void DetermineRockVersion_WithServerVersionWhileLocked_IsAccepted( string requestedVersion )
    {
        var errorMessage = DetermineRockVersion( requestedVersion, false, out var resolvedVersion );

        Assert.IsNull( errorMessage );
        Assert.AreEqual( ServerVersion, resolvedVersion );
    }

    #endregion

    #region Support

    private static string DetermineRockVersion( string requestedVersion, bool isVersionOverrideAllowed, out string resolvedVersion )
    {
        return Rock.AI.Agent.Skills.CommunityKnowledgeBaseSkill.DetermineRockVersion( ServerVersion, requestedVersion, isVersionOverrideAllowed, out resolvedVersion );
    }

    #endregion
}
