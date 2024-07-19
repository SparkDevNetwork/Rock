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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Communication;
using Rock.Tests.Shared;

namespace Rock.Tests.Rock.Communication
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
            Assert.That.IsFalse( EmailAddressFieldValidator.IsValid( null ) );
            Assert.That.IsFalse( EmailAddressFieldValidator.IsValid( string.Empty ) );
        }

        [TestMethod]
        public void EmailAddressFieldValidator_IsValidCheckWithValidAddress_ReturnsTrue()
        {
            Assert.That.IsTrue( EmailAddressFieldValidator.IsValid( "ted@rocksolidchurchdemo.com" ) );
        }

    }
}
