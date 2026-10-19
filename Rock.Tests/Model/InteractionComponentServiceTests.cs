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
using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Model
{
    /// <summary>
    /// This suite checks the <see cref="InteractionComponentService"/> get or
    /// create methods to make sure an existing component is only renamed when
    /// the caller allows it.
    /// </summary>
    [TestClass]
    public class InteractionComponentServiceTests
    {
        private const int ChannelId = 1;
        private const int EntityId = 5;
        private const string OriginalName = "Original";
        private const string PostedName = "<b>x</b>";

        private static List<InteractionComponent> GetComponents()
        {
            return new List<InteractionComponent>
            {
                new InteractionComponent
                {
                    Id = 10,
                    Guid = new Guid( "6c1e9b3a-2f4d-4f8e-9a51-1d7f3c2b8e40" ),
                    InteractionChannelId = ChannelId,
                    EntityId = EntityId,
                    Name = OriginalName
                }
            };
        }

        [TestMethod]
        public void GetComponentByChannelIdAndEntityId_WithoutUpdateExistingName_KeepsExistingName()
        {
            var components = GetComponents();
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock( false );
            rockContextMock.SetupDbSet( components );

            var service = new InteractionComponentService( rockContextMock.Object );
            var component = service.GetComponentByChannelIdAndEntityId( ChannelId, EntityId, PostedName, false );

            Assert.AreSame( components[0], component );
            Assert.AreEqual( OriginalName, component.Name );
            Assert.AreEqual( 1, components.Count );
        }

        [TestMethod]
        public void GetComponentByChannelIdAndEntityId_WithDefaultOverload_UpdatesExistingName()
        {
            var components = GetComponents();
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock( false );
            rockContextMock.SetupDbSet( components );

            var service = new InteractionComponentService( rockContextMock.Object );
            var component = service.GetComponentByChannelIdAndEntityId( ChannelId, EntityId, PostedName );

            Assert.AreSame( components[0], component );
            Assert.AreEqual( PostedName, component.Name );
            Assert.AreEqual( 1, components.Count );
        }

        [TestMethod]
        public void GetComponentByChannelIdAndEntityId_WithoutUpdateExistingNameAndNoMatch_CreatesComponentWithName()
        {
            var components = GetComponents();
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock( false );
            rockContextMock.SetupDbSet( components );

            var service = new InteractionComponentService( rockContextMock.Object );
            var component = service.GetComponentByChannelIdAndEntityId( ChannelId, EntityId + 1, PostedName, false );

            Assert.AreNotSame( components[0], component );
            Assert.AreEqual( 2, components.Count );
            Assert.AreSame( components[1], component );
            Assert.AreEqual( PostedName, component.Name );
            Assert.AreEqual( ChannelId, component.InteractionChannelId );
            Assert.AreEqual( EntityId + 1, component.EntityId );
            Assert.AreEqual( OriginalName, components[0].Name );
        }

        [TestMethod]
        public void GetComponentByComponentName_WithMatchingName_ReturnsExistingComponent()
        {
            var components = GetComponents();
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock( false );
            rockContextMock.SetupDbSet( components );

            var service = new InteractionComponentService( rockContextMock.Object );
            var component = service.GetComponentByComponentName( ChannelId, OriginalName );

            Assert.AreSame( components[0], component );
            Assert.AreEqual( OriginalName, component.Name );
            Assert.AreEqual( 1, components.Count );
        }
    }
}
