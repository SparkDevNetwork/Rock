using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Field.Types;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Field.Types.Tests
{
    [TestClass()]
    public class BooleanFieldTypeTests
    {
        [TestMethod()]
        public void ShouldReturnEmptyStringWhenPrivateValueIsNull ()
        {
            var expectedText = string.Empty;
            string privateBooleanValue = null;

            var actualText = ( new BooleanFieldType() ).GetTextValue( privateBooleanValue, new Dictionary<string, string>() );
            Assert.AreEqual( expectedText, actualText );
        }

        [TestMethod()]
        public void ShouldReturnTrueTextWhenPrivateValueIsTrue ()
        {
            var trueText = "True Text";
            var falseText = "False Text";
            string expectedText = trueText;
            string privateBooleanValue = "True";
            var privateConfigurationValues = new Dictionary<string, string>
            {
                [BooleanFieldType.ConfigurationKey.TrueText] = trueText,
                [BooleanFieldType.ConfigurationKey.FalseText] = falseText
            };

            var actualText = ( new BooleanFieldType() ).GetTextValue( privateBooleanValue, privateConfigurationValues );
            Assert.AreEqual( expectedText, actualText );
        }

        [TestMethod()]
        public void ShouldReturnFalseTextWhenPrivateValueIsFalse()
        {
            var trueText = "True Text";
            var falseText = "False Text";
            string expectedText = falseText;
            string privateBooleanValue = "False";
            var privateConfigurationValues = new Dictionary<string, string>
            {
                [BooleanFieldType.ConfigurationKey.TrueText] = trueText,
                [BooleanFieldType.ConfigurationKey.FalseText] = falseText
            };

            var actualText = ( new BooleanFieldType() ).GetTextValue( privateBooleanValue, privateConfigurationValues );
            Assert.AreEqual( expectedText, actualText );
        }

        [TestMethod()]
        public void ShouldReturnYesWhenPrivateValueIsTrueAndConfigurationHasEmptyTrueText()
        {
            string expectedText = "Yes";
            string privateBooleanValue = "True";
            var falseText = "False Text";
            var privateConfigurationValues = new Dictionary<string, string>
            {
                [BooleanFieldType.ConfigurationKey.TrueText] = string.Empty,
                [BooleanFieldType.ConfigurationKey.FalseText] = falseText
            };

            var actualText = ( new BooleanFieldType() ).GetTextValue( privateBooleanValue, privateConfigurationValues );
            Assert.AreEqual( expectedText, actualText );
        }

        [TestMethod()]
        public void ShouldReturnNoWhenPrivateValueIsFalseAndConfigurationHasEmptyFalseText()
        {
            string expectedText = "No";
            string privateBooleanValue = "False";
            var trueText = "True Text";
            var privateConfigurationValues = new Dictionary<string, string>
            {
                [BooleanFieldType.ConfigurationKey.TrueText] = trueText,
                [BooleanFieldType.ConfigurationKey.FalseText] = string.Empty
            };

            var actualText = ( new BooleanFieldType() ).GetTextValue( privateBooleanValue, privateConfigurationValues );
            Assert.AreEqual( expectedText, actualText );
        }

        [TestMethod()]
        public void ShouldReturnYesWhenPrivateValueIsTrueAndConfigurationHasNoTrueText()
        {
            string expectedText = "Yes";
            string privateBooleanValue = "True";
            var falseText = "False Text";
            var privateConfigurationValues = new Dictionary<string, string>
            {
                [BooleanFieldType.ConfigurationKey.FalseText] = falseText
            };

            var actualText = ( new BooleanFieldType() ).GetTextValue( privateBooleanValue, privateConfigurationValues );
            Assert.AreEqual( expectedText, actualText );
        }

        [TestMethod()]
        public void ShouldReturnNoWhenPrivateValueIsFalseAndConfigurationHasNoFalseText()
        {
            string expectedText = "No";
            string privateBooleanValue = "False";
            var trueText = "True Text";
            var privateConfigurationValues = new Dictionary<string, string>
            {
                [BooleanFieldType.ConfigurationKey.TrueText] = trueText
            };

            var actualText = ( new BooleanFieldType() ).GetTextValue( privateBooleanValue, privateConfigurationValues );
            Assert.AreEqual( expectedText, actualText );
        }

    }
}