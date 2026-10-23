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
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.AI.Agent.Classes;
using Rock.Configuration;
using Rock.Data;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Security;
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;

namespace Rock.AI.Agent.Tests.Skills.ContentChannelSkill;

public partial class ContentChannelSkillTests
{
    #region AddOrUpdateContentChannelItem

    [TestMethod]
    public void AddOrUpdateContentChannelItem_UpdateWithEdit_UpdatesItem()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var contentChannel = SeedContentChannel( rockContext, 10 );
        var item = SeedContentChannelItem( rockContext, 20, contentChannel );
        MockAuthorizationHelper.AllowAllUsers<ContentChannel>( rockContext, Authorization.EDIT, contentChannel.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateContentChannelItem(
            contentChannelItemIdKey: IdHasher.Instance.GetHash( item.Id ),
            name: new SetOrClear<string> { Value = "Renamed Item" } );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.AreEqual( "Renamed Item", item.Title );
    }

    [TestMethod]
    public void AddOrUpdateContentChannelItem_UpdateWithoutEdit_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var contentChannel = SeedContentChannel( rockContext, 10 );
        var item = SeedContentChannelItem( rockContext, 20, contentChannel );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateContentChannelItem(
            contentChannelItemIdKey: IdHasher.Instance.GetHash( item.Id ),
            name: new SetOrClear<string> { Value = "Renamed Item" } );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "not authorized" ) ) );
        Assert.AreEqual( "Original Item", item.Title );
    }

    [TestMethod]
    public void AddOrUpdateContentChannelItem_AddWithChannelEdit_AddsItem()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var contentChannel = SeedContentChannel( rockContext, 10 );
        MockAuthorizationHelper.AllowAllUsers<ContentChannel>( rockContext, Authorization.EDIT, contentChannel.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateContentChannelItem(
            contentChannelIdKey: IdHasher.Instance.GetHash( contentChannel.Id ),
            name: new SetOrClear<string> { Value = "New Item" } );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.IsTrue( rockContext.Set<ContentChannelItem>().Any( i => i.Title == "New Item" && i.ContentChannelId == contentChannel.Id ) );
    }

    [TestMethod]
    public void AddOrUpdateContentChannelItem_AddWithoutChannelEdit_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var contentChannel = SeedContentChannel( rockContext, 10 );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateContentChannelItem(
            contentChannelIdKey: IdHasher.Instance.GetHash( contentChannel.Id ),
            name: new SetOrClear<string> { Value = "New Item" } );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "not authorized" ) ) );
        Assert.IsFalse( rockContext.Set<ContentChannelItem>().Any( i => i.Title == "New Item" && i.Id != 0 ) );
    }

    /// <summary>
    /// Seeds a content channel, along with its channel type, and returns it.
    /// The mocked context performs no navigation-property fixup, so the type
    /// is wired by hand because channel security falls back through it.
    /// </summary>
    private static ContentChannel SeedContentChannel( RockContext rockContext, int id )
    {
        var contentChannelType = new ContentChannelType
        {
            Id = id + 1000,
            Guid = Guid.NewGuid(),
            Name = $"Channel Type {id}"
        };

        rockContext.Set<ContentChannelType>().Add( contentChannelType );

        var contentChannel = new ContentChannel
        {
            Id = id,
            Guid = Guid.NewGuid(),
            Name = $"Channel {id}",
            ContentChannelTypeId = contentChannelType.Id,
            ContentChannelType = contentChannelType
        };

        rockContext.Set<ContentChannel>().Add( contentChannel );

        return contentChannel;
    }

    /// <summary>
    /// Seeds an item in the content channel and returns it. The channel is
    /// wired by hand because item security falls back to it.
    /// </summary>
    private static ContentChannelItem SeedContentChannelItem( RockContext rockContext, int id, ContentChannel contentChannel )
    {
        var item = new ContentChannelItem
        {
            Id = id,
            Guid = Guid.NewGuid(),
            Title = "Original Item",
            ContentChannelId = contentChannel.Id,
            ContentChannel = contentChannel,
            ContentChannelTypeId = contentChannel.ContentChannelTypeId,
            ContentChannelType = contentChannel.ContentChannelType
        };

        rockContext.Set<ContentChannelItem>().Add( item );

        return item;
    }

    #endregion
}
