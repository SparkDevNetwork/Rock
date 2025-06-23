using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.Core.Automation.Triggers;
using Rock.Data;
using Rock.Enums.Core.Automation.Triggers;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.ViewModels.Core.Automation.Triggers;

namespace Rock.Tests.Core.Automation.Triggers
{
    [TestClass]
    public class EntityChangeCriteriaTests
    {
        #region IsMatch

        [TestMethod]
        public void IsMatch_ReturnsFalseWhenExceptionThrown()
        {
            var entryMock = new Mock<IEntitySaveEntry>();
            // Intentionally don't mock the ModifiedProperties so that it
            // generates an exception when called.

            var criteriaBag = new EntityChangeSimpleCriteriaBag
            {
                AreAllRulesRequired = false,
                Rules = new List<EntityChangeSimpleCriteriaRuleBag>
                {
                    new EntityChangeSimpleCriteriaRuleBag
                    {
                        ChangeType = EntityChangeSimpleChangeType.AnyChange,
                        Property = nameof( Group.Name )
                    },
                    new EntityChangeSimpleCriteriaRuleBag
                    {
                        ChangeType = EntityChangeSimpleChangeType.AnyChange,
                        Property = nameof( Group.Description )
                    }
                }
            };

            var criteria = new EntityChangeCriteria( typeof( Group ), 0, criteriaBag, null );

            var result = criteria.IsMatch( entryMock.Object );

            Assert.That.IsFalse( result );
        }

        [TestMethod]
        public void IsMatch_ReturnsTrueWithNullSimpleCriteria()
        {
            var entryMock = new Mock<IEntitySaveEntry>();
            var criteria = new EntityChangeCriteria( typeof( Group ), 0, null, null );

            var result = criteria.IsMatch( entryMock.Object );

            Assert.That.IsTrue( result );
        }

        [TestMethod]
        public void IsMatch_ReturnsTrueWithAllRulesRequiredAndBothRulesMatch()
        {
            var entryMock = new Mock<IEntitySaveEntry>();
            entryMock.Setup( e => e.ModifiedProperties )
                .Returns( new List<string> { nameof( Group.Name ), nameof( Group.Description ) } );

            var criteriaBag = new EntityChangeSimpleCriteriaBag
            {
                AreAllRulesRequired = true,
                Rules = new List<EntityChangeSimpleCriteriaRuleBag>
                {
                    new EntityChangeSimpleCriteriaRuleBag
                    {
                        ChangeType = EntityChangeSimpleChangeType.AnyChange,
                        Property = nameof( Group.Name )
                    },
                    new EntityChangeSimpleCriteriaRuleBag
                    {
                        ChangeType = EntityChangeSimpleChangeType.AnyChange,
                        Property = nameof( Group.Description )
                    }
                }
            };

            var criteria = new EntityChangeCriteria( typeof( Group ), 0, criteriaBag, null );

            var result = criteria.IsMatch( entryMock.Object );

            Assert.That.IsTrue( result );
        }

        [TestMethod]
        public void IsMatch_ReturnsFalseWithAllRulesRequiredAndOneRuleDoesNotMatch()
        {
            var entryMock = new Mock<IEntitySaveEntry>();
            entryMock.Setup( e => e.ModifiedProperties )
                .Returns( new List<string> { nameof( Group.Name ) } );

            var criteriaBag = new EntityChangeSimpleCriteriaBag
            {
                AreAllRulesRequired = true,
                Rules = new List<EntityChangeSimpleCriteriaRuleBag>
                {
                    new EntityChangeSimpleCriteriaRuleBag
                    {
                        ChangeType = EntityChangeSimpleChangeType.AnyChange,
                        Property = nameof( Group.Name )
                    },
                    new EntityChangeSimpleCriteriaRuleBag
                    {
                        ChangeType = EntityChangeSimpleChangeType.AnyChange,
                        Property = nameof( Group.Description )
                    }
                }
            };

            var criteria = new EntityChangeCriteria( typeof( Group ), 0, criteriaBag, null );

            var result = criteria.IsMatch( entryMock.Object );

            Assert.That.IsFalse( result );
        }

        [TestMethod]
        public void IsMatch_ReturnsTrueWithoutAllRulesRequiredAndOneRuleDoesNotMatch()
        {
            var entryMock = new Mock<IEntitySaveEntry>();
            entryMock.Setup( e => e.ModifiedProperties )
                .Returns( new List<string> { nameof( Group.Name ) } );

            var criteriaBag = new EntityChangeSimpleCriteriaBag
            {
                AreAllRulesRequired = false,
                Rules = new List<EntityChangeSimpleCriteriaRuleBag>
                {
                    new EntityChangeSimpleCriteriaRuleBag
                                        {
                        ChangeType = EntityChangeSimpleChangeType.AnyChange,
                        Property = nameof( Group.Name )
                    },
                    new EntityChangeSimpleCriteriaRuleBag
                    {
                        ChangeType = EntityChangeSimpleChangeType.AnyChange,
                        Property = nameof( Group.Description )
                    }
                }
            };

            var criteria = new EntityChangeCriteria( typeof( Group ), 0, criteriaBag, null );

            var result = criteria.IsMatch( entryMock.Object );

            Assert.That.IsTrue( result );
        }

        [TestMethod]
        public void IsMatch_ReturnsFalseWithNullAdvancedCriteria()
        {
            var entryMock = new Mock<IEntitySaveEntry>();
            var criteria = new EntityChangeCriteria( typeof( Group ), 1, null, null );

            var result = criteria.IsMatch( entryMock.Object );

            Assert.That.IsFalse( result );
        }

        [TestMethod]
        public void IsMatch_ReturnsFalseWithEmptyAdvancedCriteria()
        {
            var entryMock = new Mock<IEntitySaveEntry>();
            var criteria = new EntityChangeCriteria( typeof( Group ), 1, null, string.Empty );

            var result = criteria.IsMatch( entryMock.Object );

            Assert.That.IsFalse( result );
        }

        [TestMethod]
        public void IsMatch_ReturnsFalseWithInvalidPropertyAdvancedCriteria()
        {
            var entryMock = new Mock<IEntitySaveEntry>();
            var criteria = new EntityChangeCriteria( typeof( Group ), 1, null, "DoDeDa == 2" );

            var result = criteria.IsMatch( entryMock.Object );

            Assert.That.IsFalse( result );
        }

        [TestMethod]
        public void IsMatch_ReturnsTrueWithMatchingAdvancedCriteria()
        {
            var entryMock = new Mock<IEntitySaveEntry>();
            entryMock.Setup( e => e.Entity )
                .Returns( new Group { Id = 2 } );

            var criteria = new EntityChangeCriteria( typeof( Group ), 1, null, "Id == 2" );

            var result = criteria.IsMatch( entryMock.Object );

            Assert.That.IsTrue( result );
        }

        [TestMethod]
        public void IsMatch_ReturnsFalseWithNonMatchingAdvancedCriteria()
        {
            var entryMock = new Mock<IEntitySaveEntry>();
            entryMock.Setup( e => e.Entity )
                .Returns( new Group { Id = 1 } );

            var criteria = new EntityChangeCriteria( typeof( Group ), 1, null, "Id == 2" );

            var result = criteria.IsMatch( entryMock.Object );

            Assert.That.IsFalse( result );
        }

        #endregion
    }
}
