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
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Communication;
using Rock.Tests.Shared.Constants;

namespace Rock.Tests.Communication
{
    [TestCategory( TestFeatures.Communications )]
    [TestClass]
    public class EmailAddressFieldValidatorTests
    {
        [TestMethod]
        public void EmailAddressFieldValidator_WithLavaAllowedAndHasLavaContent_IsValid()
        {
            var validator = new EmailAddressFieldValidator() { LavaIsAllowed = true, MultipleAddressesAreAllowed = false };

            var result = validator.Validate( "{% if CurrentPerson.Email contains 'rocksolid.org' %}{{ CurrentPerson.Email }}{% endif %}" );

            Assert.AreEqual( EmailFieldValidationResultSpecifier.Valid, result );
        }

        [TestMethod]
        public void EmailAddressFieldValidator_WithLavaAllowedAndHasNoLavaContent_IsValid()
        {
            var validator = new EmailAddressFieldValidator() { LavaIsAllowed = true, MultipleAddressesAreAllowed = false };

            var result = validator.Validate( "test1@rocksolid.church" );

            Assert.AreEqual( EmailFieldValidationResultSpecifier.Valid, result );
        }

        [TestMethod]
        public void EmailAddressFieldValidator_WithLavaAllowedAndMixedContent_IsValid()
        {
            var validator = new EmailAddressFieldValidator() { LavaIsAllowed = true, MultipleAddressesAreAllowed = true };

            var result = validator.Validate( "ted.decker@rocksolid.church, {{CurrentPerson.Email}}, cindy.decker@rocksolid.church, {% if CurrentPerson.Email contains '@rocksolid.church' %}{{ CurrentPerson.Email }}{% endif %}, alex.decker@rocksolid.church, {[ myemailshortcode ]}" );

            Assert.AreEqual( EmailFieldValidationResultSpecifier.Valid, result );
        }

        [TestMethod]
        public void EmailAddressFieldValidator_WithLavaAllowedAndMultipleAddressesDisallowedAndContainsLavaBlock_IsValid()
        {
            var validator = new EmailAddressFieldValidator() { LavaIsAllowed = true, MultipleAddressesAreAllowed = false };

            var result = validator.Validate( "{% if CurrentPerson.Email contains 'fakeinbox.com' %}fake_{{ CurrentPerson.Email }}{% else %}from_me@nothing.com{% endif %}" );

            Assert.AreEqual( EmailFieldValidationResultSpecifier.Valid, result );
        }

        [TestMethod]
        public void EmailAddressFieldValidator_WithLavaAllowedAndMultipleAddressesDisallowedAndContainsTwoAddresses_IsInvalid()
        {
            var validator = new EmailAddressFieldValidator() { LavaIsAllowed = true, MultipleAddressesAreAllowed = false };

            var result = validator.Validate( " ted.decker@rocksolid.church, {{ Person.Email | Remove:',' }}, cindy.decker@rocksolid.church " );

            Assert.AreEqual( EmailFieldValidationResultSpecifier.InvalidMultipleAddressesNotAllowed, result );
        }

        [TestMethod]
        public void EmailAddressFieldValidator_WithLavaAllowedAndMultipleAddressesDisallowedAndLavaContainsComma_IsValid()
        {
            var validator = new EmailAddressFieldValidator() { LavaIsAllowed = true, MultipleAddressesAreAllowed = false };

            var result = validator.Validate( "{{ Person.Email | Remove:',' }}" );

            Assert.AreEqual( EmailFieldValidationResultSpecifier.Valid, result );
        }

        [TestMethod]
        public void EmailAddressFieldValidator_WithLavaAllowedAndContainsShortcode_IsValid()
        {
            var validator = new EmailAddressFieldValidator() { LavaIsAllowed = true, MultipleAddressesAreAllowed = false };

            var result = validator.Validate( "{[ mycustomemailshortcode ]}" );

            Assert.AreEqual( EmailFieldValidationResultSpecifier.Valid, result );
        }

        [TestMethod]
        public void EmailAddressFieldValidator_WithLavaAllowedAndVariableNestedInBlock_IsValid()
        {
            var validator = new EmailAddressFieldValidator() { LavaIsAllowed = true, MultipleAddressesAreAllowed = false };

            var result = validator.Validate( "{% if CurrentPerson.Email contains 'fakeinbox.com' %}fake_{{ CurrentPerson.Email }}{% endif %}" );

            Assert.AreEqual( EmailFieldValidationResultSpecifier.Valid, result );
        }

        [TestMethod]
        public void EmailAddressFieldValidator_WithMultipleAddressesAllowedAndContainsTwoAddresses_IsValid()
        {
            var validator = new EmailAddressFieldValidator() { MultipleAddressesAreAllowed = true };

            var result = validator.Validate( "test1@rocksolid.church, ted.decker@rocksolid.church" );

            Assert.AreEqual( EmailFieldValidationResultSpecifier.Valid, result );
        }

        [TestMethod]
        public void EmailAddressFieldValidator_WithMultipleAddressesDisallowedAndContainsTwoAddresses_IsInvalid()
        {
            var validator = new EmailAddressFieldValidator() { MultipleAddressesAreAllowed = false };

            var result = validator.Validate( " ted.decker@rocksolid.church, cindy.decker@rocksolid.church " );

            Assert.AreEqual( EmailFieldValidationResultSpecifier.InvalidMultipleAddressesNotAllowed, result );
        }

        [TestMethod]
        public void EmailAddressFieldValidator_WithSingleAddressHavingInternalWhitespace_IsInvalid()
        {
            var validator = new EmailAddressFieldValidator() { LavaIsAllowed = true, MultipleAddressesAreAllowed = false };

            var result = validator.Validate( "ted.decker@rock   solid.church" );

            Assert.AreEqual( EmailFieldValidationResultSpecifier.InvalidEmailAddressFormat, result );
        }

        [TestMethod]
        public void EmailAddressFieldValidator_WithSingleAddressHavingExternalWhitespace_IsValid()
        {
            var validator = new EmailAddressFieldValidator() { MultipleAddressesAreAllowed = false };

            var result = validator.Validate( "  ted.decker@rocksolid.church   " );

            Assert.AreEqual( EmailFieldValidationResultSpecifier.Valid, result );
        }

        [TestMethod]
        public void EmailAddressFieldValidator_WithSingleAddressContainingHyphen_IsValid()
        {
            var validator = new EmailAddressFieldValidator() { LavaIsAllowed = true, MultipleAddressesAreAllowed = false };

            var result = validator.Validate( "ted.decker-email2@rocksolid.church" );

            Assert.AreEqual( EmailFieldValidationResultSpecifier.Valid, result );
        }

        [TestMethod]
        public void EmailAddressFieldValidator_IsValidCheckWithEmptyInput_ReturnsFalse()
        {
            Assert.IsFalse( EmailAddressFieldValidator.IsValid( null ) );
            Assert.IsFalse( EmailAddressFieldValidator.IsValid( string.Empty ) );
        }

        [TestMethod]
        public void EmailAddressFieldValidator_IsValidCheckWithValidAddress_ReturnsTrue()
        {
            Assert.IsTrue( EmailAddressFieldValidator.IsValid( "ted@rocksolidchurchdemo.com" ) );
        }

        #region GetValidEmailAddresses

        [TestMethod]
        public void GetValidEmailAddresses_WithSingleValidAddress_ReturnsAddress()
        {
            var emails = EmailAddressFieldValidator.GetValidEmailAddresses( " ted@rocksolidchurchdemo.com ", out var hasLavaAddress, out var hasInvalidAddress );

            CollectionAssert.AreEqual( new[] { "ted@rocksolidchurchdemo.com" }, emails );
            Assert.IsFalse( hasLavaAddress );
            Assert.IsFalse( hasInvalidAddress );
        }

        [TestMethod]
        public void GetValidEmailAddresses_WithDelimitedList_ReturnsEachAddress()
        {
            var emails = EmailAddressFieldValidator.GetValidEmailAddresses( "ted@rocksolidchurchdemo.com,cindy+test@rocksolidchurchdemo.com; Alex.Decker@RockSolidChurchDemo.com", out var hasLavaAddress, out var hasInvalidAddress );

            CollectionAssert.AreEqual( new[] { "ted@rocksolidchurchdemo.com", "cindy+test@rocksolidchurchdemo.com", "Alex.Decker@RockSolidChurchDemo.com" }, emails );
            Assert.IsFalse( hasLavaAddress );
            Assert.IsFalse( hasInvalidAddress );
        }

        [TestMethod]
        public void GetValidEmailAddresses_WithLavaTag_SkipsEveryPiece()
        {
            // The split on pipes leaves "PersonTokenCreate}}@evil.example", which is
            // a valid looking address and must still be skipped.
            var emails = EmailAddressFieldValidator.GetValidEmailAddresses( "{{1|PersonTokenCreate}}@evil.example", out var hasLavaAddress, out _ );

            Assert.AreEqual( 0, emails.Count );
            Assert.IsTrue( hasLavaAddress );
        }

        [TestMethod]
        public void GetValidEmailAddresses_WithSpacedLavaTag_SkipsEveryPiece()
        {
            var emails = EmailAddressFieldValidator.GetValidEmailAddresses( "tester+{{ 7 | Plus:7 }}@example.com", out var hasLavaAddress, out _ );

            Assert.AreEqual( 0, emails.Count );
            Assert.IsTrue( hasLavaAddress );
        }

        [TestMethod]
        public void GetValidEmailAddresses_WithLavaAndValidAddress_ReturnsOnlyValidAddress()
        {
            var emails = EmailAddressFieldValidator.GetValidEmailAddresses( "ted@rocksolidchurchdemo.com,{{ Workflow | Attribute:'Email' }}", out var hasLavaAddress, out _ );

            CollectionAssert.AreEqual( new[] { "ted@rocksolidchurchdemo.com" }, emails );
            Assert.IsTrue( hasLavaAddress );
        }

        [TestMethod]
        public void GetValidEmailAddresses_WithInvalidAddress_SkipsAddress()
        {
            var emails = EmailAddressFieldValidator.GetValidEmailAddresses( "not-an-email ted@rocksolidchurchdemo.com", out var hasLavaAddress, out var hasInvalidAddress );

            CollectionAssert.AreEqual( new[] { "ted@rocksolidchurchdemo.com" }, emails );
            Assert.IsFalse( hasLavaAddress );
            Assert.IsTrue( hasInvalidAddress );
        }

        [TestMethod]
        public void GetValidEmailAddresses_WithEmptyInput_ReturnsEmptyList()
        {
            Assert.AreEqual( 0, EmailAddressFieldValidator.GetValidEmailAddresses( null, out _, out _ ).Count );
            Assert.AreEqual( 0, EmailAddressFieldValidator.GetValidEmailAddresses( "  ", out _, out _ ).Count );
        }

        [TestMethod]
        public void ContainsLavaBrace_DetectsEitherBrace()
        {
            Assert.IsTrue( EmailAddressFieldValidator.ContainsLavaBrace( "{{ Person.FullName }}" ) );
            Assert.IsTrue( EmailAddressFieldValidator.ContainsLavaBrace( "Name}" ) );
            Assert.IsTrue( EmailAddressFieldValidator.ContainsLavaBrace( "{Name" ) );
            Assert.IsFalse( EmailAddressFieldValidator.ContainsLavaBrace( "Ted Decker" ) );
            Assert.IsFalse( EmailAddressFieldValidator.ContainsLavaBrace( null ) );
        }

        #endregion GetValidEmailAddresses

    }
}
