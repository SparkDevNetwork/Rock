using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.BulkImport;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Controllers;
using Rock.Tests.Shared;

namespace Rock.Tests.Integration.Rest.ControllersTests
{
    [TestClass]
    public class InteractionsControllerTests
    {
        [TestMethod]
        public void BulkInteractionImportShouldCreateNewInteractionChannelWithChannelTypeMediumValueGuid()
        {
            var interactionChannelForeignKey = Guid.NewGuid().ToString();
            var controller = new InteractionsController();
            var interactionsImport = new InteractionsImport
            {
                Interactions = new List<InteractionImport>
                {
                    new InteractionImport
                    {
                        InteractionChannelForeignKey = interactionChannelForeignKey,
                        InteractionChannelName = $"Test Channel Name {nameof(BulkInteractionImportShouldCreateNewInteractionChannelWithChannelTypeMediumValueGuid)}",
                        InteractionChannelChannelTypeMediumValueGuid = SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_CONTENTCHANNEL.AsGuidOrNull(),
                        InteractionComponentId = 17,
                        Interaction = new InteractionImportInteraction
                        {
                            Operation = "Test Operation",
                            InteractionDateTime = DateTime.Now,
                            InteractionSummary = $"Test Summary {nameof(BulkInteractionImportShouldCreateNewInteractionChannelWithChannelTypeMediumValueGuid)}",
                            InteractionData = $"Test Data {nameof(BulkInteractionImportShouldCreateNewInteractionChannelWithChannelTypeMediumValueGuid)}",
                            ForeignKey = interactionChannelForeignKey
                        }
                    }
                }
            };

            controller.InteractionImport( interactionsImport );

            var validationQuery = $"SELECT * FROM InteractionChannel WHERE ForeignKey = '{interactionChannelForeignKey}'";
            var result = new RockContext().Database.SqlQuery<Rock.Model.InteractionChannel>( validationQuery ).ToListAsync().Result;
            var expectedChannelTypeMediumValueId = new DefinedValueService( new RockContext() ).Get( SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_CONTENTCHANNEL.AsGuid() ).Id;

            Assert.That.IsNotNull( result );
            Assert.That.IsTrue( result.Count > 0 );
            Assert.That.AreEqual( expectedChannelTypeMediumValueId, result[0].ChannelTypeMediumValueId );

            validationQuery = $"SELECT * FROM Interaction WHERE ForeignKey = '{interactionChannelForeignKey}'";
            var interactionResult = new RockContext().Database.SqlQuery<Interaction>( validationQuery ).ToListAsync().Result;

            Assert.That.IsNotNull( interactionResult );
            Assert.That.IsTrue( interactionResult.Count > 0 );
        }

        [TestMethod]
        public void BulkInteractionImportShouldCreateNewInteractionChannelWithChannelTypeMediumValueId()
        {
            var interactionChannelForeignKey = Guid.NewGuid().ToString();
            var expectedChannelTypeMediumValueId = new DefinedValueService( new RockContext() ).Get( SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_CONTENTCHANNEL.AsGuid() ).Id;

            var controller = new InteractionsController();
            var interactionsImport = new InteractionsImport
            {
                Interactions = new List<InteractionImport>
                {
                    new InteractionImport
                    {
                        InteractionChannelForeignKey = interactionChannelForeignKey,
                        InteractionChannelName = $"Test Channel Name {nameof(BulkInteractionImportShouldCreateNewInteractionChannelWithChannelTypeMediumValueId)}",
                        InteractionChannelChannelTypeMediumValueId = expectedChannelTypeMediumValueId,
                        InteractionComponentId = 17,
                        Interaction = new InteractionImportInteraction
                        {
                            Operation = "Test Operation",
                            InteractionDateTime = DateTime.Now,
                            InteractionSummary = $"Test Summary {nameof(BulkInteractionImportShouldCreateNewInteractionChannelWithChannelTypeMediumValueId)}",
                            InteractionData = $"Test Data {nameof(BulkInteractionImportShouldCreateNewInteractionChannelWithChannelTypeMediumValueId)}",
                            ForeignKey = interactionChannelForeignKey
                        }
                    }
                }
            };

            controller.InteractionImport( interactionsImport );

            var validationQuery = $"SELECT * FROM InteractionChannel WHERE ForeignKey = '{interactionChannelForeignKey}'";
            var result = new RockContext().Database.SqlQuery<Rock.Model.InteractionChannel>( validationQuery ).ToListAsync().Result;


            Assert.That.IsNotNull( result );
            Assert.That.IsTrue( result.Count > 0 );
            Assert.That.AreEqual( expectedChannelTypeMediumValueId, result[0].ChannelTypeMediumValueId );

            validationQuery = $"SELECT * FROM Interaction WHERE ForeignKey = '{interactionChannelForeignKey}'";
            var interactionResult = new RockContext().Database.SqlQuery<Interaction>( validationQuery ).ToListAsync().Result;

            Assert.That.IsNotNull( interactionResult );
            Assert.That.IsTrue( interactionResult.Count > 0 );
        }

        [TestMethod]
        public void BulkInteractionImportShouldCreateNewInteractionChannelWithNoChannelTypeMediumValue()
        {
            var interactionChannelForeignKey = Guid.NewGuid().ToString();
            var controller = new InteractionsController();
            var interactionsImport = new InteractionsImport
            {
                Interactions = new List<InteractionImport>
                {
                    new InteractionImport
                    {
                        InteractionChannelForeignKey = interactionChannelForeignKey,
                        InteractionChannelName = $"Test Channel Name {nameof(BulkInteractionImportShouldCreateNewInteractionChannelWithNoChannelTypeMediumValue)}",
                        InteractionComponentId = 17,
                        Interaction = new InteractionImportInteraction
                        {
                            Operation = "Test Operation",
                            InteractionDateTime = DateTime.Now,
                            InteractionSummary = $"Test Summary {nameof(BulkInteractionImportShouldCreateNewInteractionChannelWithNoChannelTypeMediumValue)}",
                            InteractionData = $"Test Data {nameof(BulkInteractionImportShouldCreateNewInteractionChannelWithNoChannelTypeMediumValue)}",
                            ForeignKey = interactionChannelForeignKey
                        }
                    }
                }
            };

            controller.InteractionImport( interactionsImport );

            var validationQuery = $"SELECT * FROM InteractionChannel WHERE ForeignKey = '{interactionChannelForeignKey}'";
            var result = new RockContext().Database.SqlQuery<Rock.Model.InteractionChannel>( validationQuery ).ToListAsync().Result;


            Assert.That.IsNotNull( result );
            Assert.That.IsTrue( result.Count > 0 );

            validationQuery = $"SELECT * FROM Interaction WHERE ForeignKey = '{interactionChannelForeignKey}'";
            var interactionResult = new RockContext().Database.SqlQuery<Interaction>( validationQuery ).ToListAsync().Result;

            Assert.That.IsNotNull( interactionResult );
            Assert.That.IsTrue( interactionResult.Count > 0 );
        }
    }
}
