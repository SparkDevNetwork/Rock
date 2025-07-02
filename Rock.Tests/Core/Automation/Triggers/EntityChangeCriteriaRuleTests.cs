using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.Core.Automation.Triggers;
using Rock.Data;
using Rock.Enums.Core.Automation.Triggers;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Utility.Enums;
using Rock.ViewModels.Core.Automation.Triggers;

namespace Rock.Tests.Core.Automation.Triggers
{
    [TestClass]
    public class EntityChangeCriteriaRuleTests
    {
        #region IsMatch

        [TestMethod]
        public void IsMatch_ReturnsFalseForUnknownChangetype()
        {
            var rule = new EntityChangeCriteriaRule( typeof( Group ), new EntityChangeSimpleCriteriaRuleBag
            {
                ChangeType = ( EntityChangeSimpleChangeType ) ( -1 ),
                Property = nameof( Group.Name )
            } );

            var entryMock = new Mock<IEntitySaveEntry>();

            var result = rule.IsMatch( entryMock.Object );

            Assert.That.IsFalse( result );
        }

        #endregion

        #region IsMatch AnyChange

        [TestMethod]
        public void AnyChange_MatchesWhenPropertyChanged()
        {
            var rule = new EntityChangeCriteriaRule( typeof( Group ), new EntityChangeSimpleCriteriaRuleBag
            {
                ChangeType = EntityChangeSimpleChangeType.AnyChange,
                Property = nameof( Group.Name )
            } );

            var entryMock = new Mock<IEntitySaveEntry>();
            entryMock.Setup( e => e.ModifiedProperties )
                .Returns( new List<string> { nameof( Group.Name ) } );

            var result = rule.IsMatch( entryMock.Object );

            Assert.That.IsTrue( result );
        }

        [TestMethod]
        public void AnyChange_DoesNotMatchWhenPropertyNotChanged()
        {
            var rule = new EntityChangeCriteriaRule( typeof( Group ), new EntityChangeSimpleCriteriaRuleBag
            {
                ChangeType = EntityChangeSimpleChangeType.AnyChange,
                Property = nameof( Group.Name )
            } );

            var entryMock = new Mock<IEntitySaveEntry>();
            entryMock.Setup( e => e.ModifiedProperties )
                .Returns( new List<string> { nameof( Group.Description ) } );

            var result = rule.IsMatch( entryMock.Object );

            Assert.That.IsFalse( result );
        }

        [TestMethod]
        public void AnyChange_DoesNotMatchWithInvalidProperty()
        {
            var rule = new EntityChangeCriteriaRule( typeof( GroupMember ), new EntityChangeSimpleCriteriaRuleBag
            {
                ChangeType = EntityChangeSimpleChangeType.AnyChange,
                Property = nameof( Group.Name )
            } );

            var entryMock = new Mock<IEntitySaveEntry>();
            entryMock.Setup( e => e.ModifiedProperties )
                .Returns( new List<string> { nameof( GroupMember.Note ) } );

            var result = rule.IsMatch( entryMock.Object );

            Assert.That.IsFalse( result );
        }

        #endregion

        #region IsMatch HasSpecificValue

        [TestMethod]
        public void HasSpecificValue_ReturnsFalseForUnknownProperty()
        {
            var rule = new EntityChangeCriteriaRule( typeof( Group ), new EntityChangeSimpleCriteriaRuleBag
            {
                ChangeType = EntityChangeSimpleChangeType.HasSpecificValue,
                Property = "UnknownProperty"
            } );

            var entryMock = new Mock<IEntitySaveEntry>();

            var result = rule.IsMatch( entryMock.Object );

            Assert.That.IsFalse( result );
        }

        [TestMethod]
        public void HasSpecificValue_ReturnsFalseForNoMatch()
        {
            var rule = new EntityChangeCriteriaRule( typeof( Group ), new EntityChangeSimpleCriteriaRuleBag
            {
                ChangeType = EntityChangeSimpleChangeType.HasSpecificValue,
                Property = nameof( Group.Name ),
                UpdatedValue = "Test"
            } );

            var group = new Group { Name = "No Match" };
            var entryMock = new Mock<IEntitySaveEntry>();
            entryMock.Setup( e => e.OriginalValues )
                .Returns( new Dictionary<string, object>
                {
                    [nameof( Group.Name )] = "No Match"
                } );
            entryMock.Setup( e => e.Entity ).Returns( group );

            var result = rule.IsMatch( entryMock.Object );

            Assert.That.IsFalse( result );
        }

        [TestMethod]
        public void HasSpecificValue_ReturnsFalseForMatchingOriginalValue()
        {
            var rule = new EntityChangeCriteriaRule( typeof( Group ), new EntityChangeSimpleCriteriaRuleBag
            {
                ChangeType = EntityChangeSimpleChangeType.HasSpecificValue,
                Property = nameof( Group.Name ),
                UpdatedValue = "Test"
            } );

            var group = new Group { Name = "No Match" };
            var entryMock = new Mock<IEntitySaveEntry>();
            entryMock.Setup( e => e.OriginalValues )
                .Returns( new Dictionary<string, object>
                {
                    [nameof( Group.Name )] = "Test"
                } );
            entryMock.Setup( e => e.Entity ).Returns( group );

            var result = rule.IsMatch( entryMock.Object );

            Assert.That.IsFalse( result );
        }

        [TestMethod]
        public void HasSpecificValue_ReturnsTrueForMatchingCurrentValue()
        {
            var rule = new EntityChangeCriteriaRule( typeof( Group ), new EntityChangeSimpleCriteriaRuleBag
            {
                ChangeType = EntityChangeSimpleChangeType.HasSpecificValue,
                Property = nameof( Group.Name ),
                UpdatedValue = "Test"
            } );

            var group = new Group { Name = "Test" };
            var entryMock = new Mock<IEntitySaveEntry>();
            entryMock.Setup( e => e.OriginalValues )
                .Returns( new Dictionary<string, object>
                {
                    [nameof( Group.Name )] = "No Match"
                } );
            entryMock.Setup( e => e.Entity ).Returns( group );

            var result = rule.IsMatch( entryMock.Object );

            Assert.That.IsTrue( result );
        }

        [TestMethod]
        public void HasSpecificValue_DoesNotThrowForMissingOriginalValue()
        {
            var rule = new EntityChangeCriteriaRule( typeof( Group ), new EntityChangeSimpleCriteriaRuleBag
            {
                ChangeType = EntityChangeSimpleChangeType.HasSpecificValue,
                Property = nameof( Group.Name ),
                UpdatedValue = "Test"
            } );

            var group = new Group { Name = "No Match" };
            var entryMock = new Mock<IEntitySaveEntry>();
            entryMock.Setup( e => e.OriginalValues )
                .Returns( new Dictionary<string, object>() );
            entryMock.Setup( e => e.Entity ).Returns( group );

            var result = rule.IsMatch( entryMock.Object );

            Assert.That.IsFalse( result );
        }

        #endregion

        #region IsMatch ChangedFromValue

        [TestMethod]
        public void ChangedFromValue_ReturnsFalseForUnknownProperty()
        {
            var rule = new EntityChangeCriteriaRule( typeof( Group ), new EntityChangeSimpleCriteriaRuleBag
            {
                ChangeType = EntityChangeSimpleChangeType.ChangedFromValue,
                Property = "UnknownProperty"
            } );

            var entryMock = new Mock<IEntitySaveEntry>();

            var result = rule.IsMatch( entryMock.Object );

            Assert.That.IsFalse( result );
        }

        [TestMethod]
        public void ChangedFromValue_ReturnsFalseForNoMatch()
        {
            var rule = new EntityChangeCriteriaRule( typeof( Group ), new EntityChangeSimpleCriteriaRuleBag
            {
                ChangeType = EntityChangeSimpleChangeType.ChangedFromValue,
                Property = nameof( Group.Name ),
                OriginalValue = "Test"
            } );

            var entryMock = new Mock<IEntitySaveEntry>();
            entryMock.Setup( e => e.ModifiedProperties )
                .Returns( new List<string> { nameof( Group.Name ) } );
            entryMock.Setup( e => e.OriginalValues )
                .Returns( new Dictionary<string, object>
                {
                    [nameof( Group.Name )] = "No Match"
                } );

            var result = rule.IsMatch( entryMock.Object );

            Assert.That.IsFalse( result );
        }

        [TestMethod]
        public void ChangedFromValue_ReturnsFalseForUnchangedProperty()
        {
            var rule = new EntityChangeCriteriaRule( typeof( Group ), new EntityChangeSimpleCriteriaRuleBag
            {
                ChangeType = EntityChangeSimpleChangeType.ChangedFromValue,
                Property = nameof( Group.Name ),
                OriginalValue = "Test"
            } );

            var entryMock = new Mock<IEntitySaveEntry>();
            entryMock.Setup( e => e.OriginalValues )
                .Returns( new Dictionary<string, object>
                {
                    [nameof( Group.Name )] = "Test"
                } );
            entryMock.Setup( e => e.ModifiedProperties )
                .Returns( new List<string>() );

            var result = rule.IsMatch( entryMock.Object );

            Assert.That.IsFalse( result );
        }

        [TestMethod]
        public void ChangedFromValue_ReturnsTrueForMatchingOriginalValue()
        {
            var rule = new EntityChangeCriteriaRule( typeof( Group ), new EntityChangeSimpleCriteriaRuleBag
            {
                ChangeType = EntityChangeSimpleChangeType.ChangedFromValue,
                Property = nameof( Group.Name ),
                OriginalValue = "Test"
            } );

            var entryMock = new Mock<IEntitySaveEntry>();
            entryMock.Setup( e => e.OriginalValues )
                .Returns( new Dictionary<string, object>
                {
                    [nameof( Group.Name )] = "Test"
                } );
            entryMock.Setup( e => e.ModifiedProperties )
                .Returns( new List<string> { nameof( Group.Name ) } );

            var result = rule.IsMatch( entryMock.Object );

            Assert.That.IsTrue( result );
        }

        [TestMethod]
        public void ChangedFromValue_DoesNotThrowForMissingOriginalValue()
        {
            var rule = new EntityChangeCriteriaRule( typeof( Group ), new EntityChangeSimpleCriteriaRuleBag
            {
                ChangeType = EntityChangeSimpleChangeType.ChangedFromValue,
                Property = nameof( Group.Name ),
                OriginalValue = "Test"
            } );

            var entryMock = new Mock<IEntitySaveEntry>();
            entryMock.Setup( e => e.OriginalValues )
                .Returns( new Dictionary<string, object>() );
            entryMock.Setup( e => e.ModifiedProperties )
                .Returns( new List<string> { nameof( Group.Name ) } );

            var result = rule.IsMatch( entryMock.Object );

            Assert.That.IsFalse( result );
        }

        #endregion

        #region IsMatch ChangedToValue

        [TestMethod]
        public void ChangedToValue_ReturnsFalseForUnknownProperty()
        {
            var rule = new EntityChangeCriteriaRule( typeof( Group ), new EntityChangeSimpleCriteriaRuleBag
            {
                ChangeType = EntityChangeSimpleChangeType.ChangedToValue,
                Property = "UnknownProperty"
            } );

            var entryMock = new Mock<IEntitySaveEntry>();

            var result = rule.IsMatch( entryMock.Object );

            Assert.That.IsFalse( result );
        }

        [TestMethod]
        public void ChangedToValue_ReturnsFalseForNoMatch()
        {
            var rule = new EntityChangeCriteriaRule( typeof( Group ), new EntityChangeSimpleCriteriaRuleBag
            {
                ChangeType = EntityChangeSimpleChangeType.ChangedToValue,
                Property = nameof( Group.Name ),
                UpdatedValue = "Test"
            } );

            var entryMock = new Mock<IEntitySaveEntry>();
            entryMock.Setup( e => e.ModifiedProperties )
                .Returns( new List<string> { nameof( Group.Name ) } );
            entryMock.Setup( e => e.Entity )
                .Returns( new Group { Name = "No Match" } );

            var result = rule.IsMatch( entryMock.Object );

            Assert.That.IsFalse( result );
        }

        [TestMethod]
        public void ChangedToValue_ReturnsFalseForUnchangedProperty()
        {
            var rule = new EntityChangeCriteriaRule( typeof( Group ), new EntityChangeSimpleCriteriaRuleBag
            {
                ChangeType = EntityChangeSimpleChangeType.ChangedToValue,
                Property = nameof( Group.Name ),
                UpdatedValue = "Test"
            } );

            var entryMock = new Mock<IEntitySaveEntry>();
            entryMock.Setup( e => e.ModifiedProperties )
                .Returns( new List<string>() );

            var result = rule.IsMatch( entryMock.Object );

            Assert.That.IsFalse( result );
        }

        [TestMethod]
        public void ChangedToValue_ReturnsTrueForMatchingUpdatedValue()
        {
            var rule = new EntityChangeCriteriaRule( typeof( Group ), new EntityChangeSimpleCriteriaRuleBag
            {
                ChangeType = EntityChangeSimpleChangeType.ChangedToValue,
                Property = nameof( Group.Name ),
                UpdatedValue = "Test"
            } );

            var entryMock = new Mock<IEntitySaveEntry>();
            entryMock.Setup( e => e.ModifiedProperties )
                .Returns( new List<string> { nameof( Group.Name ) } );
            entryMock.Setup( e => e.Entity )
                .Returns( new Group { Name = "Test" } );

            var result = rule.IsMatch( entryMock.Object );

            Assert.That.IsTrue( result );
        }

        #endregion

        #region IsMatch ChangedFromValueToValue

        [TestMethod]
        public void ChangedFromValueToValue_ReturnsFalseForUnknownProperty()
        {
            var rule = new EntityChangeCriteriaRule( typeof( Group ), new EntityChangeSimpleCriteriaRuleBag
            {
                ChangeType = EntityChangeSimpleChangeType.ChangedFromValueToValue,
                Property = "UnknownProperty"
            } );

            var entryMock = new Mock<IEntitySaveEntry>();

            var result = rule.IsMatch( entryMock.Object );

            Assert.That.IsFalse( result );
        }

        [TestMethod]
        public void ChangedFromValueToValue_ReturnsFalseForNoMatchOnOriginalValue()
        {
            var rule = new EntityChangeCriteriaRule( typeof( Group ), new EntityChangeSimpleCriteriaRuleBag
            {
                ChangeType = EntityChangeSimpleChangeType.ChangedFromValueToValue,
                Property = nameof( Group.Name ),
                OriginalValue = "Test",
                UpdatedValue = "Test2"
            } );

            var entryMock = new Mock<IEntitySaveEntry>();
            entryMock.Setup( e => e.ModifiedProperties )
                .Returns( new List<string> { nameof( Group.Name ) } );
            entryMock.Setup( e => e.OriginalValues )
                .Returns( new Dictionary<string, object>
                {
                    [nameof( Group.Name )] = "No Match"
                } );
            entryMock.Setup( e => e.Entity )
                .Returns( new Group { Name = "Test2" } );

            var result = rule.IsMatch( entryMock.Object );

            Assert.That.IsFalse( result );
        }

        [TestMethod]
        public void ChangedFromValueToValue_ReturnsFalseForNoMatchOnUpdatedValue()
        {
            var rule = new EntityChangeCriteriaRule( typeof( Group ), new EntityChangeSimpleCriteriaRuleBag
            {
                ChangeType = EntityChangeSimpleChangeType.ChangedFromValueToValue,
                Property = nameof( Group.Name ),
                OriginalValue = "Test",
                UpdatedValue = "Test2"
            } );

            var entryMock = new Mock<IEntitySaveEntry>();
            entryMock.Setup( e => e.ModifiedProperties )
                .Returns( new List<string> { nameof( Group.Name ) } );
            entryMock.Setup( e => e.OriginalValues )
                .Returns( new Dictionary<string, object>
                {
                    [nameof( Group.Name )] = "Test"
                } );
            entryMock.Setup( e => e.Entity )
                .Returns( new Group { Name = "No Match" } );

            var result = rule.IsMatch( entryMock.Object );

            Assert.That.IsFalse( result );
        }

        [TestMethod]
        public void ChangedFromValueToValue_ReturnsFalseForUnchangedProperty()
        {
            var rule = new EntityChangeCriteriaRule( typeof( Group ), new EntityChangeSimpleCriteriaRuleBag
            {
                ChangeType = EntityChangeSimpleChangeType.ChangedFromValueToValue,
                Property = nameof( Group.Name ),
                OriginalValue = "Test",
                UpdatedValue = "Test2"
            } );

            var entryMock = new Mock<IEntitySaveEntry>();
            entryMock.Setup( e => e.ModifiedProperties )
                .Returns( new List<string>() );
            entryMock.Setup( e => e.OriginalValues )
                .Returns( new Dictionary<string, object>
                {
                    [nameof( Group.Name )] = "Test"
                } );
            entryMock.Setup( e => e.Entity )
                .Returns( new Group { Name = "Test2" } );

            var result = rule.IsMatch( entryMock.Object );

            Assert.That.IsFalse( result );
        }

        [TestMethod]
        public void ChangedFromValueToValue_ReturnsFalseForMissingOriginalValue()
        {
            var rule = new EntityChangeCriteriaRule( typeof( Group ), new EntityChangeSimpleCriteriaRuleBag
            {
                ChangeType = EntityChangeSimpleChangeType.ChangedFromValueToValue,
                Property = nameof( Group.Name ),
                OriginalValue = "Test",
                UpdatedValue = "Test2"
            } );

            var entryMock = new Mock<IEntitySaveEntry>();
            entryMock.Setup( e => e.ModifiedProperties )
                .Returns( new List<string> { nameof( Group.Name ) } );
            entryMock.Setup( e => e.OriginalValues )
                .Returns( new Dictionary<string, object>() );
            entryMock.Setup( e => e.Entity )
                .Returns( new Group { Name = "Test2" } );

            var result = rule.IsMatch( entryMock.Object );

            Assert.That.IsFalse( result );
        }

        [TestMethod]
        public void ChangedFromValueToValue_ReturnsTrueForMatchingValues()
        {
            var rule = new EntityChangeCriteriaRule( typeof( Group ), new EntityChangeSimpleCriteriaRuleBag
            {
                ChangeType = EntityChangeSimpleChangeType.ChangedFromValueToValue,
                Property = nameof( Group.Name ),
                OriginalValue = "Test",
                UpdatedValue = "Test2"
            } );

            var entryMock = new Mock<IEntitySaveEntry>();
            entryMock.Setup( e => e.ModifiedProperties )
                .Returns( new List<string> { nameof( Group.Name ) } );
            entryMock.Setup( e => e.OriginalValues )
                .Returns( new Dictionary<string, object>
                {
                    [nameof( Group.Name )] = "Test"
                } );
            entryMock.Setup( e => e.Entity )
                .Returns( new Group { Name = "Test2" } );

            var result = rule.IsMatch( entryMock.Object );

            Assert.That.IsTrue( result );
        }

        #endregion

        #region ParseValue

        [TestMethod]
        public void ParseValue_ReturnsNullForUnknownProperty()
        {
            var result = EntityChangeCriteriaRule.ParseValue( typeof( Group ), "UnknownName", "value" );

            Assert.That.IsNull( result );
        }

        [TestMethod]
        public void ParseValue_ReturnsNullForNullValue()
        {
            var result = EntityChangeCriteriaRule.ParseValue( typeof( Group ), nameof( Group.Name ), null );

            Assert.That.IsNull( result );
        }

        [TestMethod]
        public void ParseValue_ReturnsNullForEmptyStringValue()
        {
            var result = EntityChangeCriteriaRule.ParseValue( typeof( Group ), nameof( Group.Name ), "" );

            Assert.That.IsNull( result );
        }

        [TestMethod]
        public void ParseValue_ReturnsEnumTypeForInvalidValue()
        {
            var result = EntityChangeCriteriaRule.ParseValue( typeof( Group ), nameof( Group.ElevatedSecurityLevel ), "Invalid" );

            Assert.That.IsNull( result );
        }

        [TestMethod]
        public void ParseValue_ReturnsEnumTypeForEnumProperty()
        {
            var result = EntityChangeCriteriaRule.ParseValue( typeof( Group ), nameof( Group.ElevatedSecurityLevel ), "1" );

            Assert.That.AreEqual( typeof( ElevatedSecurityLevel ), result.GetType() );
        }

        [TestMethod]
        public void ParseValue_ReturnsEnumValueForIntegerValue()
        {
            var result = EntityChangeCriteriaRule.ParseValue( typeof( Group ), nameof( Group.ElevatedSecurityLevel ), "1" );

            Assert.That.AreEqual( ElevatedSecurityLevel.High, result );
        }

        [TestMethod]
        public void ParseValue_ReturnsEnumValueForStringValue()
        {
            var result = EntityChangeCriteriaRule.ParseValue( typeof( Group ), nameof( Group.ElevatedSecurityLevel ), "High" );

            Assert.That.AreEqual( ElevatedSecurityLevel.High, result );
        }

        [TestMethod]
        public void ParseValue_ReturnsIntegerForIntegerPropertyType()
        {
            var result = EntityChangeCriteriaRule.ParseValue( typeof( Group ), nameof( Group.Id ), "123" );

            Assert.That.AreEqual( 123, result );
        }

        [TestMethod]
        public void ParseValue_ReturnsDoubleForDoublePropertyType()
        {
            var result = EntityChangeCriteriaRule.ParseValue( typeof( Interaction ), nameof( Interaction.InteractionLength ), "1.23" );

            Assert.That.AreEqual( 1.23d, result );
        }

        [TestMethod]
        public void ParseValue_ReturnsDecimalForDecimalPropertyType()
        {
            var result = EntityChangeCriteriaRule.ParseValue( typeof( Group ), nameof( Group.LeaderToLeaderRelationshipMultiplierOverride ), "1.23" );

            Assert.That.AreEqual( 1.23M, result );
        }

        [TestMethod]
        public void ParseValue_ReturnsBooleanForBooleanPropertyType()
        {
            var result = EntityChangeCriteriaRule.ParseValue( typeof( Group ), nameof( Group.IsSpecialNeeds ), "true" );

            Assert.That.AreEqual( true, result );
        }

        #endregion

        #region DoesValueMatch

        [TestMethod]
        public void DoesValueMatch_ReturnsTrueWhenBothValuesAreNull()
        {
            var result = EntityChangeCriteriaRule.DoesValueMatch( null, null );

            Assert.That.IsTrue( result );
        }

        [TestMethod]
        public void DoesValueMatch_ReturnsTrueWithNullCompareValueAndEmptyStringEntityValue()
        {
            var result = EntityChangeCriteriaRule.DoesValueMatch( null, "" );

            Assert.That.IsTrue( result );
        }

        [TestMethod]
        public void DoesValueMatch_ReturnsFalseWithNullCompareValueAndNonEmptyStringEntityValue()
        {
            var result = EntityChangeCriteriaRule.DoesValueMatch( null, "test" );

            Assert.That.IsFalse( result );
        }

        [TestMethod]
        public void DoesValueMatch_ReturnsTrueWithEqualStringValues()
        {
            var result = EntityChangeCriteriaRule.DoesValueMatch( "test", "test" );

            Assert.That.IsTrue( result );
        }

        [TestMethod]
        public void DoesValueMatch_ReturnsFalseWithDifferentStringValues()
        {
            var result = EntityChangeCriteriaRule.DoesValueMatch( "test", "fail" );

            Assert.That.IsFalse( result );
        }

        [TestMethod]
        public void DoesValueMatch_ReturnsTrueWithEqualIntegerValues()
        {
            var result = EntityChangeCriteriaRule.DoesValueMatch( 123, 123 );

            Assert.That.IsTrue( result );
        }

        [TestMethod]
        public void DoesValueMatch_ReturnsFalseWithDifferntIntegerValues()
        {
            var result = EntityChangeCriteriaRule.DoesValueMatch( 123, 456 );

            Assert.That.IsFalse( result );
        }

        #endregion
    }
}
