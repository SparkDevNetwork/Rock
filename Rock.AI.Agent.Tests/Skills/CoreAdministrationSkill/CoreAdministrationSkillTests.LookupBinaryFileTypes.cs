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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Tests.Shared.TestFramework;

namespace Rock.AI.Agent.Tests.Skills.CoreAdministrationSkill;

public partial class CoreAdministrationSkillTests
{
    #region LookupBinaryFileTypes

    [TestMethod]
    public void LookupBinaryFileTypes_WithNone_ReturnsNoData()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.LookupBinaryFileTypes();

        Assert.AreEqual( ToolStatus.NoData, result.GetStatus() );
    }

    [TestMethod]
    public void LookupBinaryFileTypes_WithTypes_ReturnsThem()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        rockContext.Set<BinaryFileType>().Add( new BinaryFileType
        {
            Id = 1,
            Guid = new Guid( "7d000001-0000-4000-8000-000000000001" ),
            Name = "Person Image",
            CacheControlHeaderSettings = "{}"
        } );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.LookupBinaryFileTypes();

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void LookupBinaryFileTypes_WithNonMatchingFilter_ReturnsNoData()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        rockContext.Set<BinaryFileType>().Add( new BinaryFileType
        {
            Id = 1,
            Guid = new Guid( "7d000002-0000-4000-8000-000000000002" ),
            Name = "Person Image",
            CacheControlHeaderSettings = "{}"
        } );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.LookupBinaryFileTypes( "Nonexistent" );

        Assert.AreEqual( ToolStatus.NoData, result.GetStatus() );
    }

    #endregion
}
