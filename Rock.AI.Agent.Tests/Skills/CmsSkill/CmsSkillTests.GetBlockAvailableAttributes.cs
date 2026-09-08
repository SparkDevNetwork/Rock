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
using Rock.Utility;

namespace Rock.AI.Agent.Tests.Skills.CmsSkill;

public partial class CmsSkillTests
{
    #region GetBlockAvailableAttributes

    [TestMethod]
    public void GetBlockAvailableAttributes_WithExistingBlock_ReturnsSuccess()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var block = new Block
        {
            Id = 800,
            Guid = new Guid( "b1000001-0000-4000-8000-000000000001" ),
            Name = "Test Block",
            BlockTypeId = 1
        };

        rockContext.Set<Block>().Add( block );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.GetBlockAvailableAttributes( blockIdKey: IdHasher.Instance.GetHash( block.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void GetBlockAvailableAttributes_WithBlockTypeBeforeCreation_ReturnsSuccess()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var blockType = new BlockType
        {
            Id = 810,
            Guid = new Guid( "b1000002-0000-4000-8000-000000000002" ),
            Name = "Test Block Type"
        };

        rockContext.Set<BlockType>().Add( blockType );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.GetBlockAvailableAttributes( blockTypeIdKey: IdHasher.Instance.GetHash( blockType.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void GetBlockAvailableAttributes_WithInvalidBlockType_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.GetBlockAvailableAttributes( blockTypeIdKey: IdHasher.Instance.GetHash( 999 ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    #endregion
}
