using System;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.Data;
using Rock.Jobs;
using Rock.Model;
using Rock.SystemKey;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

namespace Rock.Tests.Jobs
{
    [TestClass]
    public class UpdateEntityUsageTests
    {
        [TestMethod]
        public void UpdateEntityUsage_WithNoAttributes_DeletesMetadata()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
            var metadataHelperMock = new Mock<MetadataHelper>( MockBehavior.Strict );
            var processedCount = 0;

            metadataHelperMock.Setup( m => m.DeleteEntityValue( It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<RockContext>() ) );

            void configureServices( ServiceCollection serviceCollection )
            {
                serviceCollection.AddSingleton( metadataHelperMock.Object );
                serviceCollection.AddSingleton( rockContextFactory );
            }

            using ( TestHelper.CreateScopedRockApp( configureServices ) )
            {
                var mediaFieldType = new FieldType { Guid = SystemGuid.FieldType.MEDIA_ELEMENT.AsGuid() };
                var mediaElement = new MediaElement
                {
                    Id = 42,
                    Guid = new Guid( "33869839-9b81-4510-9058-fd1dfdbab1b6" ),
                };

                rockContextMock.Object.Set<MediaElement>().Add( mediaElement );

                var job = new UpdateEntityUsage();

                job.UpdateMediaUsage( rockContextMock.Object, ref processedCount );

                metadataHelperMock.Verify( m => m.DeleteEntityValue( It.IsAny<int>(), mediaElement.Id, MetadataKey.EntityUsage, It.IsAny<RockContext>() ), Times.Once );
            }
        }

        [TestMethod]
        public void UpdateEntityUsage_WithNoAttributeValues_DeletesMetadata()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
            var metadataHelperMock = new Mock<MetadataHelper>( MockBehavior.Strict );
            var processedCount = 0;

            metadataHelperMock.Setup( m => m.DeleteEntityValue( It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<RockContext>() ) );

            void configureServices( ServiceCollection serviceCollection )
            {
                serviceCollection.AddSingleton( metadataHelperMock.Object );
                serviceCollection.AddSingleton( rockContextFactory );
            }

            using ( TestHelper.CreateScopedRockApp( configureServices ) )
            {
                var entityType = EntityTypeCache.Get<ContentChannelItem>( true, rockContextMock.Object );
                var mediaFieldType = new FieldType { Guid = SystemGuid.FieldType.MEDIA_ELEMENT.AsGuid() };

                var mediaElement = new MediaElement
                {
                    Id = 3,
                    Guid = new Guid( "33869839-9b81-4510-9058-fd1dfdbab1b6" ),
                };

                var attribute = new Rock.Model.Attribute
                {
                    Id = 2,
                    FieldType = mediaFieldType,
                    EntityTypeId = entityType.Id,
                };

                rockContextMock.Object.Set<Rock.Model.Attribute>().Add( attribute );
                rockContextMock.Object.Set<MediaElement>().Add( mediaElement );

                var job = new UpdateEntityUsage();

                job.UpdateMediaUsage( rockContextMock.Object, ref processedCount );

                metadataHelperMock.Verify( m => m.DeleteEntityValue( It.IsAny<int>(), mediaElement.Id, MetadataKey.EntityUsage, It.IsAny<RockContext>() ), Times.Once );
            }
        }

        [TestMethod]
        public void UpdateEntityUsage_WithMissingEntityType_DeletesMetadata()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
            var metadataHelperMock = new Mock<MetadataHelper>( MockBehavior.Strict );
            var processedCount = 0;

            metadataHelperMock.Setup( m => m.DeleteEntityValue( It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<RockContext>() ) );

            void configureServices( ServiceCollection serviceCollection )
            {
                serviceCollection.AddSingleton( metadataHelperMock.Object );
                serviceCollection.AddSingleton( rockContextFactory );
            }

            using ( TestHelper.CreateScopedRockApp( configureServices ) )
            {
                var mediaFieldType = new FieldType { Guid = SystemGuid.FieldType.MEDIA_ELEMENT.AsGuid() };

                var mediaElement = new MediaElement
                {
                    Id = 5,
                    Guid = new Guid( "33869839-9b81-4510-9058-fd1dfdbab1b6" ),
                };

                var contentChannelItem = new ContentChannelItem
                {
                    Id = 4,
                    Guid = new Guid( "2d5b4f2e-8f3c-4f2e-9f3c-8f3c4f2e9f3c" ),
                    Title = "Test Content Channel Item",
                };

                var entityType = new EntityType
                {
                    Id = 1,
                    Guid = new Guid( "1d5b4f2e-8f3c-4f2e-9f3c-8f3c4f2e9f3c" ),
                    Name = "Rock.Model.ContentChannelItem",
                    // Intentionally not setting AssemblyName to simulate missing entity type.
                    //AssemblyName = typeof( ContentChannelItem ).AssemblyQualifiedName,
                };

                var attribute = new Rock.Model.Attribute
                {
                    Id = 2,
                    FieldType = mediaFieldType,
                    EntityTypeId = entityType.Id,
                };

                var attributeValue = new AttributeValue
                {
                    Id = 3,
                    Attribute = attribute,
                    AttributeId = attribute.Id,
                    Value = mediaElement.Guid.ToString(),
                    EntityId = contentChannelItem.Id,
                };

                rockContextMock.Object.Set<Rock.Model.Attribute>().Add( attribute );
                rockContextMock.Object.Set<AttributeValue>().Add( attributeValue );
                rockContextMock.Object.Set<EntityType>().Add( entityType );
                rockContextMock.Object.Set<ContentChannelItem>().Add( contentChannelItem );
                rockContextMock.Object.Set<MediaElement>().Add( mediaElement );

                var job = new UpdateEntityUsage();

                job.UpdateMediaUsage( rockContextMock.Object, ref processedCount );

                metadataHelperMock.Verify( m => m.DeleteEntityValue( It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<RockContext>() ), Times.Once );
            }
        }

        [TestMethod]
        public void UpdateEntityUsage_WithReferences_SetsMetadata()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );
            var metadataHelperMock = new Mock<MetadataHelper>( MockBehavior.Strict );
            var processedCount = 0;

            metadataHelperMock.Setup( m => m.SaveEntityValue( It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<RockContext>() ) );

            void configureServices( ServiceCollection serviceCollection )
            {
                serviceCollection.AddSingleton( metadataHelperMock.Object );
                serviceCollection.AddSingleton( rockContextFactory );
            }

            using ( TestHelper.CreateScopedRockApp( configureServices ) )
            {
                var entityType = EntityTypeCache.Get<ContentChannelItem>( true, rockContextMock.Object );
                var mediaFieldType = new FieldType { Guid = SystemGuid.FieldType.MEDIA_ELEMENT.AsGuid() };

                var mediaElement = new MediaElement
                {
                    Id = 5,
                    Guid = new Guid( "33869839-9b81-4510-9058-fd1dfdbab1b6" ),
                };

                var contentChannelItem = new ContentChannelItem
                {
                    Id = 4,
                    Guid = new Guid( "2d5b4f2e-8f3c-4f2e-9f3c-8f3c4f2e9f3c" ),
                    Title = "Test Content Channel Item",
                };

                var attribute = new Rock.Model.Attribute
                {
                    Id = 2,
                    FieldType = mediaFieldType,
                    EntityTypeId = entityType.Id,
                };

                var attributeValue = new AttributeValue
                {
                    Id = 3,
                    Attribute = attribute,
                    AttributeId = attribute.Id,
                    Value = mediaElement.Guid.ToString(),
                    EntityId = contentChannelItem.Id,
                };

                rockContextMock.Object.Set<Rock.Model.Attribute>().Add( attribute );
                rockContextMock.Object.Set<AttributeValue>().Add( attributeValue );
                rockContextMock.Object.Set<ContentChannelItem>().Add( contentChannelItem );
                rockContextMock.Object.Set<MediaElement>().Add( mediaElement );

                var job = new UpdateEntityUsage();

                job.UpdateMediaUsage( rockContextMock.Object, ref processedCount );

                metadataHelperMock.Verify( m => m.SaveEntityValue( It.IsAny<int>(), mediaElement.Id, MetadataKey.EntityUsage, It.IsAny<string>(), It.IsAny<RockContext>() ), Times.Once );
            }
        }
    }
}
