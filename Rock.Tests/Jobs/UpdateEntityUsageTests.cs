using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.Data;
using Rock.Jobs;
using Rock.Model;
using Rock.SystemKey;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Jobs
{
    [TestClass]
    public class UpdateEntityUsageTests
    {
        [TestMethod]
        public void UpdateEntityUsage_WithNoAttributes_DeletesMetadata()
        {
            var metadataHelperMock = new Mock<MetadataHelper>( MockBehavior.Strict );
            var mediaElementId = 42;
            var processedCount = 0;

            metadataHelperMock.Setup( m => m.DeleteEntityValue( It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<RockContext>() ) );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( metadataHelperMock.Object ) ) )
            {
                var rockContextMock = MockDatabaseHelper.GetRockContextMock();
                var mediaFieldType = new FieldType { Guid = SystemGuid.FieldType.MEDIA_ELEMENT.AsGuid() };

                var mediaElementMock = MockDatabaseHelper.CreateEntityMock<MediaElement>( mediaElementId, new Guid( "33869839-9b81-4510-9058-fd1dfdbab1b6" ) );

                rockContextMock.SetupDbSet<Rock.Model.Attribute>();
                rockContextMock.SetupDbSet<AttributeValue>();
                rockContextMock.SetupDbSet( mediaElementMock.Object );

                var job = new UpdateEntityUsage();

                job.UpdateMediaUsage( rockContextMock.Object, ref processedCount );

                metadataHelperMock.Verify( m => m.DeleteEntityValue( It.IsAny<int>(), mediaElementId, MetadataKey.EntityUsage, It.IsAny<RockContext>() ), Times.Once );
            }
        }

        [TestMethod]
        public void UpdateEntityUsage_WithNoAttributeValues_DeletesMetadata()
        {
            var entityTypeId = 1;
            var attributeId = 2;
            var mediaElementId = 3;
            var metadataHelperMock = new Mock<MetadataHelper>( MockBehavior.Strict );
            var processedCount = 0;

            metadataHelperMock.Setup( m => m.DeleteEntityValue( It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<RockContext>() ) );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( metadataHelperMock.Object ) ) )
            {
                var rockContextMock = MockDatabaseHelper.GetRockContextMock();
                var mediaFieldType = new FieldType { Guid = SystemGuid.FieldType.MEDIA_ELEMENT.AsGuid() };

                var mediaElementMock = MockDatabaseHelper.CreateEntityMock<MediaElement>( mediaElementId, new Guid( "33869839-9b81-4510-9058-fd1dfdbab1b6" ) );

                var entityTypeMock = MockDatabaseHelper.CreateEntityMock<EntityType>( entityTypeId, new Guid( "1d5b4f2e-8f3c-4f2e-9f3c-8f3c4f2e9f3c" ) );
                entityTypeMock.Object.Name = "Rock.Model.ContentChannelItem";
                entityTypeMock.Object.AssemblyName = typeof( ContentChannelItem ).AssemblyQualifiedName;

                var attributeMock = MockDatabaseHelper.CreateEntityMock<Rock.Model.Attribute>( attributeId, new Guid( "7cc5881c-148d-41fe-95dc-51bd20e62339" ) );
                attributeMock.Setup( m => m.FieldType ).Returns( mediaFieldType );
                attributeMock.Object.EntityTypeId = entityTypeId;

                rockContextMock.SetupDbSet( attributeMock.Object );
                rockContextMock.SetupDbSet<AttributeValue>();
                rockContextMock.SetupDbSet( entityTypeMock.Object );
                rockContextMock.SetupDbSet( mediaElementMock.Object );

                var job = new UpdateEntityUsage();

                job.UpdateMediaUsage( rockContextMock.Object, ref processedCount );

                metadataHelperMock.Verify( m => m.DeleteEntityValue( It.IsAny<int>(), mediaElementId, MetadataKey.EntityUsage, It.IsAny<RockContext>() ), Times.Once );
            }
        }

        [TestMethod]
        public void UpdateEntityUsage_WithMissingEntityType_DeletesMetadata()
        {
            var entityTypeId = 1;
            var attributeId = 2;
            var attributeValueId = 3;
            var contentChannelItemId = 4;
            var mediaElementId = 5;
            var metadataHelperMock = new Mock<MetadataHelper>( MockBehavior.Strict );
            var processedCount = 0;

            metadataHelperMock.Setup( m => m.DeleteEntityValue( It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<RockContext>() ) );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( metadataHelperMock.Object ) ) )
            {
                var rockContextMock = MockDatabaseHelper.GetRockContextMock();
                var mediaFieldType = new FieldType { Guid = SystemGuid.FieldType.MEDIA_ELEMENT.AsGuid() };

                var mediaElementMock = MockDatabaseHelper.CreateEntityMock<MediaElement>( mediaElementId, new Guid( "33869839-9b81-4510-9058-fd1dfdbab1b6" ) );

                var entityTypeMock = MockDatabaseHelper.CreateEntityMock<EntityType>( entityTypeId, new Guid( "1d5b4f2e-8f3c-4f2e-9f3c-8f3c4f2e9f3c" ) );
                entityTypeMock.Object.Name = "Rock.Model.ContentChannelItem";
                // Intentionally not setting AssemblyName to simulate missing entity type.

                var attributeMock = MockDatabaseHelper.CreateEntityMock<Rock.Model.Attribute>( attributeId, new Guid( "7cc5881c-148d-41fe-95dc-51bd20e62339" ) );
                attributeMock.Setup( m => m.FieldType ).Returns( mediaFieldType );
                attributeMock.Object.EntityTypeId = entityTypeId;

                var attributeValueMock = MockDatabaseHelper.CreateEntityMock<AttributeValue>( attributeValueId, new Guid( "89818fd3-d6e4-4ad1-ba54-0e4377c54083" ) );
                attributeValueMock.Setup( m => m.Attribute ).Returns( attributeMock.Object );
                attributeValueMock.Object.AttributeId = attributeId;
                attributeValueMock.Object.Value = mediaElementMock.Object.Guid.ToString();
                attributeValueMock.Object.EntityId = contentChannelItemId;

                var contentChannelItem = MockDatabaseHelper.CreateEntityMock<ContentChannelItem>( contentChannelItemId, new Guid( "2d5b4f2e-8f3c-4f2e-9f3c-8f3c4f2e9f3c" ) );
                contentChannelItem.Object.Title = "Test Content Channel Item";

                rockContextMock.SetupDbSet( attributeMock.Object );
                rockContextMock.SetupDbSet( attributeValueMock.Object );
                rockContextMock.SetupDbSet( entityTypeMock.Object );
                rockContextMock.SetupDbSet( contentChannelItem.Object );
                rockContextMock.SetupDbSet( mediaElementMock.Object );

                var job = new UpdateEntityUsage();

                job.UpdateMediaUsage( rockContextMock.Object, ref processedCount );

                metadataHelperMock.Verify( m => m.DeleteEntityValue( It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<RockContext>() ), Times.Once );
            }
        }

        [TestMethod]
        public void UpdateEntityUsage_WithReferences_SetsMetadata()
        {
            var entityTypeId = 1;
            var attributeId = 2;
            var attributeValueId = 3;
            var contentChannelItemId = 4;
            var mediaElementId = 5;
            var metadataHelperMock = new Mock<MetadataHelper>( MockBehavior.Strict );
            var processedCount = 0;

            metadataHelperMock.Setup( m => m.SaveEntityValue( It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<RockContext>() ) );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( metadataHelperMock.Object ) ) )
            {
                var rockContextMock = MockDatabaseHelper.GetRockContextMock();
                var mediaFieldType = new FieldType { Guid = SystemGuid.FieldType.MEDIA_ELEMENT.AsGuid() };

                var mediaElementMock = MockDatabaseHelper.CreateEntityMock<MediaElement>( mediaElementId, new Guid( "33869839-9b81-4510-9058-fd1dfdbab1b6" ) );

                var entityTypeMock = MockDatabaseHelper.CreateEntityMock<EntityType>( entityTypeId, new Guid( "1d5b4f2e-8f3c-4f2e-9f3c-8f3c4f2e9f3c" ) );
                entityTypeMock.Object.Name = "Rock.Model.ContentChannelItem";
                entityTypeMock.Object.AssemblyName = typeof( ContentChannelItem ).AssemblyQualifiedName;

                var attributeMock = MockDatabaseHelper.CreateEntityMock<Rock.Model.Attribute>( attributeId, new Guid( "7cc5881c-148d-41fe-95dc-51bd20e62339" ) );
                attributeMock.Setup( m => m.FieldType ).Returns( mediaFieldType );
                attributeMock.Object.EntityTypeId = entityTypeId;

                var attributeValueMock = MockDatabaseHelper.CreateEntityMock<AttributeValue>( attributeValueId, new Guid( "89818fd3-d6e4-4ad1-ba54-0e4377c54083" ) );
                attributeValueMock.Setup( m => m.Attribute ).Returns( attributeMock.Object );
                attributeValueMock.Object.AttributeId = attributeId;
                attributeValueMock.Object.Value = mediaElementMock.Object.Guid.ToString();
                attributeValueMock.Object.EntityId = contentChannelItemId;

                var contentChannelItem = MockDatabaseHelper.CreateEntityMock<ContentChannelItem>( contentChannelItemId, new Guid( "2d5b4f2e-8f3c-4f2e-9f3c-8f3c4f2e9f3c" ) );
                contentChannelItem.Object.Title = "Test Content Channel Item";

                rockContextMock.SetupDbSet( attributeMock.Object );
                rockContextMock.SetupDbSet( attributeValueMock.Object );
                rockContextMock.SetupDbSet( entityTypeMock.Object );
                rockContextMock.SetupDbSet( contentChannelItem.Object );
                rockContextMock.SetupDbSet( mediaElementMock.Object );

                var job = new UpdateEntityUsage();

                job.UpdateMediaUsage( rockContextMock.Object, ref processedCount );

                metadataHelperMock.Verify( m => m.SaveEntityValue( It.IsAny<int>(), mediaElementId, MetadataKey.EntityUsage, It.IsAny<string>(), It.IsAny<RockContext>() ), Times.Once );
            }
        }
    }
}
