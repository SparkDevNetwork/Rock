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
using System.Net.Mail;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Communication;
using Rock.Tests.Shared.Constants;

namespace Rock.Tests.Communication
{
    [TestCategory( TestFeatures.Communications )]
    [TestClass]
    public class EmailTransportComponentReplyToTests
    {
        [TestMethod]
        public void MailAddress_WithCommaSeparatedReplyTo_ThrowsFormatException()
        {
            // Characterizes the old behavior: the record save parsed the whole Reply-To header as one address.
            Assert.ThrowsExactly<FormatException>( () => new MailAddress( "person@aol.com,\"Person Name\" <person@aol.com>" ) );
        }

        [TestMethod]
        public void GetReplyToAddressesForCommunicationRecord_WithReplyToAndSameSenderInDisplayNameForm_ReturnsSingleAddress()
        {
            var result = EmailTransportComponent.GetReplyToAddressesForCommunicationRecord( "person@aol.com,\"Person Name\" <person@aol.com>" );

            Assert.AreEqual( "person@aol.com", result );
        }

        [TestMethod]
        public void GetReplyToAddressesForCommunicationRecord_WithReplyToAndDifferentSender_ReturnsBothPlainAddresses()
        {
            var result = EmailTransportComponent.GetReplyToAddressesForCommunicationRecord( "a@x.com,\"Person Name\" <b@y.com>" );

            Assert.AreEqual( "a@x.com,b@y.com", result );
        }

        [TestMethod]
        public void GetReplyToAddressesForCommunicationRecord_WithCommaInDisplayName_ReturnsPlainAddresses()
        {
            var result = EmailTransportComponent.GetReplyToAddressesForCommunicationRecord( "a@x.com,\"Doe, John\" <b@y.com>" );

            Assert.AreEqual( "a@x.com,b@y.com", result );
        }

        [TestMethod]
        public void GetReplyToAddressesForCommunicationRecord_WithSingleDisplayNameAddress_ReturnsPlainAddress()
        {
            var result = EmailTransportComponent.GetReplyToAddressesForCommunicationRecord( "\"Person Name\" <person@aol.com>" );

            Assert.AreEqual( "person@aol.com", result );
        }

        [TestMethod]
        public void GetReplyToAddressesForCommunicationRecord_WithPlainAddress_ReturnsSameAddress()
        {
            var result = EmailTransportComponent.GetReplyToAddressesForCommunicationRecord( "person@aol.com" );

            Assert.AreEqual( "person@aol.com", result );
        }

        [TestMethod]
        public void GetReplyToAddressesForCommunicationRecord_WithDuplicatesDifferingByCase_ReturnsOneAddress()
        {
            var result = EmailTransportComponent.GetReplyToAddressesForCommunicationRecord( "Person@AOL.com, person@aol.com" );

            Assert.AreEqual( "Person@AOL.com", result );
        }
    }
}
