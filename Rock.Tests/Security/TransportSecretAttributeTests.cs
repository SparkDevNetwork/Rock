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
using System.Linq;
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Attribute;
using Rock.Communication.Transport;
using Rock.SystemKey;

namespace Rock.Tests.Security
{
    /// <summary>
    /// Tests that the secret settings of the communication transports are
    /// stored encrypted and edited in a masked box.
    /// </summary>
    [TestClass]
    public class TransportSecretAttributeTests
    {
        [DataTestMethod]
        [DataRow( typeof( MailgunHttp ), "APIKey" )]
        [DataRow( typeof( MailgunHttp ), "HTTPWebhookSigningKey" )]
        [DataRow( typeof( Rock.Communication.Transport.Twilio ), TwilioAttributeKey.AuthToken )]
        [DataRow( typeof( Firebase ), "ServerKey" )]
        [DataRow( typeof( Firebase ), "ServiceAccountJson" )]
        [DataRow( typeof( RockMobilePush ), "ServerKey" )]
        [DataRow( typeof( RockMobilePush ), "ServiceAccountJson" )]
        [DataRow( typeof( SMTP ), "Password" )]
        public void TransportSecret_IsEncryptedPasswordField( Type transportType, string attributeKey )
        {
            var attribute = transportType.GetCustomAttributes<FieldAttribute>()
                .SingleOrDefault( a => a.Key == attributeKey );

            Assert.IsNotNull( attribute, $"{transportType.Name} has no {attributeKey} attribute." );
            Assert.IsInstanceOfType( attribute, typeof( EncryptedTextFieldAttribute ) );
            Assert.IsTrue( ( ( EncryptedTextFieldAttribute ) attribute ).IsPassword );
        }
    }
}
