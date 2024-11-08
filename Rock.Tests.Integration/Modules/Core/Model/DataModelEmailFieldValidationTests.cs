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
using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Modules.Core.Model.Validation.Email
{
    [TestCategory( TestFeatures.DataModelValidation )]
    [TestClass]
    public class DataModelEmailFieldValidationTests : DatabaseTestsBase
    {
        /// <summary>
        /// Verify that an email field decorated with the EmailMaskValidatorAttribute accepts valid email addresses.
        /// </summary>
        /// <remarks>
        /// Test cases are sourced from https://en.wikipedia.org/wiki/Email_address#Valid_email_addresses.
        /// </remarks>

        /*
            [2024-05-04] DL

            These additional cases should be valid according to the RFC standard, but they do not match the current regex pattern.
            They represent specific edge-cases that are unlikely to arise in normal use.

            [DataRow( "postmaster@[IPv6:2001:0db8:85a3:0000:0000:8a2e:0370:7334]", "(IPv6 uses a different syntax)" )]
            [DataRow( "_test@[IPv6:2001:0db8:85a3:0000:0000:8a2e:0370:7334]", "" )]
            [DataRow( "admin@example", "local domain name with no top-level domain" )]
            [DataRow( "\" \"@example.org", "space between the quotes" )]
            [DataRow( "\"very.(),:;<>[]\\\".VERY.\\\"very@\\\\ \\\"very\\\".unusual\"@strange.example.com", "" )]
        */

        [DataTestMethod]
        [DataRow( "simple@example.com", "" )]
        [DataRow( "very.common@example.co.uk", "multiple sub-domains in host-part" )]
        [DataRow( "user_name1234@email-provider.net", "dash in host-part" )]
        [DataRow( "FirstName.LastName@EasierReading.org", "allow mixed case" )]
        [DataRow( "x@example.com", "one-letter local-part" )]
        [DataRow( "long.email-address_with_hyphens-and-underscores@and.subdomains.example.com", "multiple subdomains in host-part" )]
        [DataRow( "user.name+tag_sorting@example.com", "name-part contains subaddressing/plus-addressing" )]
        [DataRow( "name/surname@example.com", "slashes are an allowable printable character" )]
        [DataRow( "\"john..doe\"@example.org", "quoted double dot" )]
        [DataRow( "mailhost!username@example.org", "bangified host route used for uucp mailers" )]
        [DataRow( "user%example.com@example.org", "% escaped mail route to user@example.com via example.org" )]
        [DataRow( "user-@example.org", "name-part ends with special character" )]
        [DataRow( "postmaster@[123.123.123.123]", "IP addresses are allowed instead of domains when in square brackets, but strongly discouraged" )]
        [DataRow( "john.doe+jane.doe+no.doe@example.com", "local-part contains subaddress with embedded '+'" )]
        public void EmailField_WithEmailAddressValidationAttribute_AcceptsValidEmailAddress( string email, string description = null )
        {
            var rockContext = new RockContext();

            var prayerRequest = CreateNewTestPrayerRequest( rockContext, email );

            var prayerRequestService = new PrayerRequestService( rockContext );
            prayerRequestService.Add( prayerRequest );

            rockContext.SaveChanges();

            var savedRequest = prayerRequestService.Get( prayerRequest.Guid );

            Assert.That.AreEqual( email, savedRequest.Email );
        }

        /// <summary>
        /// Verify that an email field decorated with the EmailMaskValidatorAttribute rejects invalid email addresses.
        /// </summary>
        /// <remarks>
        /// Test cases are sourced from https://en.wikipedia.org/wiki/Email_address#Valid_email_addresses.
        /// </remarks>

        /*
            [2024-05-04] DL

            This additional case should be invalid according to the RFC standard, but it is considered a match by the current regex pattern.

            [DataRow( "1234567890123456789012345678901234567890123456789012345678901234+x@example.com", "local-part is longer than 64 characters" )]
        */

        [DataTestMethod]
        [DataRow( "abc.example.com", "no @ character to separate local-part and domain-part" )]
        [DataRow( "a@b@c@example.com", "only one @ is allowed outside quotation marks" )]
        [DataRow( "a\"b(c)d,e:f;g<h>i[j\\k]l@example.com", "none of the special characters in this local-part are allowed outside quotation marks" )]
        [DataRow( "just\"not\"right@example.com", "quoted strings must be dot separated or be the only element making up the local-part" )]
        [DataRow( "this is\"not\\allowed@example.com", "spaces, quotes, and backslashes may only exist when within quoted strings and preceded by a backslash" )]
        [DataRow( "this\\ still\\\"not\\\\allowed@example.com", "even if escaped (preceded by a backslash), spaces, quotes, and backslashes must still be contained by quotes" )]
        [DataRow( "i.like.underscores@but_they_are_not_allowed_in_this_part", "underscore is not allowed in domain-part" )]
        public void EmailField_WithEmailAddressValidationAttribute_RejectsInvalidEmailAddress( string email, string rule )
        {
            var rockContext = new RockContext();
            var prayerRequestService = new PrayerRequestService( rockContext );

            var prayerRequest = CreateNewTestPrayerRequest( rockContext, email );
            prayerRequestService.Add( prayerRequest );

            var isValid = false;
            try
            {
                rockContext.SaveChanges();
            }
            catch ( Exception ex )
            {
                isValid = ex.Message.Contains( "Entity Validation Error" );
            }

            if ( !isValid )
            {
                Assert.That.Fail( $"Email Validation failed: {email} ({rule})" );
            }
        }

        private PrayerRequest CreateNewTestPrayerRequest( RockContext rockContext, string email )
        {
            var personService = new PersonService( rockContext );

            var newRequestGuid = Guid.NewGuid();

            var prayerRequest = new PrayerRequest()
            {
                Guid = newRequestGuid,
                RequestedByPersonAliasId = personService.GetByIdentifierOrThrow( TestGuids.TestPeople.BillMarble.AsGuid() ).PrimaryAliasId,
                FirstName = "Bill",
                Text = $"Test Request ({newRequestGuid})",
                Email = email
            };

            return prayerRequest;
        }
    }

}