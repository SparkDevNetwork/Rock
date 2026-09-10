using System;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.Configuration;
using Rock.Data;
using Rock.Jobs;
using Rock.Model;
using Rock.SystemKey;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

namespace Rock.Tests.Jobs
{
    [TestClass]
    public class UpdateEntityUsageTests
    {
        #region UpdateMediaUsage

        [TestMethod]
        public void UpdateMediaUsage_WithNoAttributes_DeletesMetadata()
        {
            var metadataHelperMock = new Mock<MetadataHelper>( MockBehavior.Strict );
            var processedCount = 0;

            metadataHelperMock.Setup( m => m.DeleteEntityValue( It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<RockContext>() ) );

            using var app = TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( metadataHelperMock.Object ) );
            var rockContext = app.App.CreateRockContext();

            var mediaFieldType = new FieldType { Guid = SystemGuid.FieldType.MEDIA_ELEMENT.AsGuid() };
            var mediaElement = new MediaElement
            {
                Id = 42,
                Guid = new Guid( "33869839-9b81-4510-9058-fd1dfdbab1b6" ),
            };

            rockContext.Set<MediaElement>().Add( mediaElement );

            var job = new UpdateEntityUsage();

            job.UpdateMediaUsage( rockContext, ref processedCount );

            metadataHelperMock.Verify( m => m.DeleteEntityValue( It.IsAny<int>(), mediaElement.Id, MetadataKey.EntityUsage, It.IsAny<RockContext>() ), Times.Once );
        }

        [TestMethod]
        public void UpdateMediaUsage_WithNoAttributeValues_DeletesMetadata()
        {
            var metadataHelperMock = new Mock<MetadataHelper>( MockBehavior.Strict );
            var processedCount = 0;

            metadataHelperMock.Setup( m => m.DeleteEntityValue( It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<RockContext>() ) );

            using var app = TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( metadataHelperMock.Object ) );
            var rockContext = app.App.CreateRockContext();

            var entityType = EntityTypeCache.Get<ContentChannelItem>( true, rockContext );
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

            rockContext.Set<Rock.Model.Attribute>().Add( attribute );
            rockContext.Set<MediaElement>().Add( mediaElement );

            var job = new UpdateEntityUsage();

            job.UpdateMediaUsage( rockContext, ref processedCount );

            metadataHelperMock.Verify( m => m.DeleteEntityValue( It.IsAny<int>(), mediaElement.Id, MetadataKey.EntityUsage, It.IsAny<RockContext>() ), Times.Once );
        }

        [TestMethod]
        public void UpdateMediaUsage_WithMissingEntityType_DeletesMetadata()
        {
            var metadataHelperMock = new Mock<MetadataHelper>( MockBehavior.Strict );
            var processedCount = 0;

            metadataHelperMock.Setup( m => m.DeleteEntityValue( It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<RockContext>() ) );

            using var app = TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( metadataHelperMock.Object ) );
            var rockContext = app.App.CreateRockContext();

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

            rockContext.Set<Rock.Model.Attribute>().Add( attribute );
            rockContext.Set<AttributeValue>().Add( attributeValue );
            rockContext.Set<EntityType>().Add( entityType );
            rockContext.Set<ContentChannelItem>().Add( contentChannelItem );
            rockContext.Set<MediaElement>().Add( mediaElement );

            var job = new UpdateEntityUsage();

            job.UpdateMediaUsage( rockContext, ref processedCount );

            metadataHelperMock.Verify( m => m.DeleteEntityValue( It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<RockContext>() ), Times.Once );
        }

        [TestMethod]
        public void UpdateMediaUsage_WithReferences_SetsMetadata()
        {
            var metadataHelperMock = new Mock<MetadataHelper>( MockBehavior.Strict );
            var processedCount = 0;

            metadataHelperMock.Setup( m => m.SaveEntityValue( It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<RockContext>() ) );

            using var app = TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( metadataHelperMock.Object ) );
            var rockContext = app.App.CreateRockContext();

            var entityType = EntityTypeCache.Get<ContentChannelItem>( true, rockContext );
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

            rockContext.Set<Rock.Model.Attribute>().Add( attribute );
            rockContext.Set<AttributeValue>().Add( attributeValue );
            rockContext.Set<ContentChannelItem>().Add( contentChannelItem );
            rockContext.Set<MediaElement>().Add( mediaElement );

            var job = new UpdateEntityUsage();

            job.UpdateMediaUsage( rockContext, ref processedCount );

            metadataHelperMock.Verify( m => m.SaveEntityValue( It.IsAny<int>(), mediaElement.Id, MetadataKey.EntityUsage, It.IsAny<string>(), It.IsAny<RockContext>() ), Times.Once );
        }

        #endregion

        #region UpdateContentChannelItemMediaUsage

        [TestMethod]
        public void UpdateContentChannelItemMediaUsage_WithReferences_SetsContentChannelItemMediaMetadata()
        {
            var metadataHelperMock = new Mock<MetadataHelper>( MockBehavior.Strict );
            var processedCount = 0;

            metadataHelperMock.Setup( m => m.SaveEntityValue( It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<RockContext>() ) );

            using var app = TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( metadataHelperMock.Object ) );
            var rockContext = app.App.CreateRockContext();

            var entityType = EntityTypeCache.Get<ContentChannelItem>( true, rockContext );
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

            rockContext.Set<Rock.Model.Attribute>().Add( attribute );
            rockContext.Set<AttributeValue>().Add( attributeValue );
            rockContext.Set<ContentChannelItem>().Add( contentChannelItem );
            rockContext.Set<MediaElement>().Add( mediaElement );

            var job = new UpdateEntityUsage();

            job.UpdateContentChannelItemMediaUsage( rockContext, ref processedCount );

            metadataHelperMock.Verify( m => m.SaveEntityValue( It.IsAny<int>(), contentChannelItem.Id, MetadataKey.MediaElements, It.IsAny<string>(), It.IsAny<RockContext>() ), Times.Once );
        }

        [TestMethod]
        public void UpdateContentChannelItemMediaUsage_WithNoMediaReferences_DeletesContentChannelItemMediaMetadata()
        {
            var metadataHelperMock = new Mock<MetadataHelper>( MockBehavior.Strict );
            var processedCount = 0;

            metadataHelperMock.Setup( m => m.DeleteEntityValue( It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<RockContext>() ) );

            using var app = TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( metadataHelperMock.Object ) );
            var rockContext = app.App.CreateRockContext();

            var entityType = EntityTypeCache.Get<ContentChannelItem>( true, rockContext );
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

            // Intentionally not adding any attribute values so the content
            // channel item has no media element references.
            rockContext.Set<ContentChannelItem>().Add( contentChannelItem );
            rockContext.Set<MediaElement>().Add( mediaElement );

            var job = new UpdateEntityUsage();

            job.UpdateContentChannelItemMediaUsage( rockContext, ref processedCount );

            metadataHelperMock.Verify( m => m.DeleteEntityValue( It.IsAny<int>(), contentChannelItem.Id, MetadataKey.MediaElements, It.IsAny<RockContext>() ), Times.Once );
        }

        #endregion
    }
}
