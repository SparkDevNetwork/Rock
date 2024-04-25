using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Communication;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Modules.Communications.Transport
{
    [TestClass]
    public class SMTPTests : DatabaseTestsBase
    {
        private static Guid ServerAttributeGuid = SystemGuid.Attribute.COMMUNICATION_TRANSPORT_SMTP_SERVER.AsGuid();
        private static string _serverAttributeValueValue;
        private static Guid _newServerAttributeValueGuid;

        #region Setup & Teardown

        [ClassInitialize]
        public static void Initialize( TestContext context )
        {
            TestDataHelper.SetAttributeValue( ServerAttributeGuid, 0, "localhost", out string previousValue, out Guid newAttributeValueGuid );
            _serverAttributeValueValue = previousValue;
            _newServerAttributeValueGuid = newAttributeValueGuid;
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            if ( !_newServerAttributeValueGuid.IsEmpty() )
            {
                // Delete it.
                TestDataHelper.DeleteAttributeValue( _newServerAttributeValueGuid );
            }

            if ( !string.IsNullOrEmpty( _serverAttributeValueValue ) )
            {
                // Set it back.
                TestDataHelper.SetAttributeValue( ServerAttributeGuid, 0, _serverAttributeValueValue, out _, out _ );
            }
        }

        #endregion

        #region Send Tests

        [TestMethod]
        public void SendShouldSendEmailWhenReplyToEmailIsNotDefined()
        {
            // Arrange
            var smtp = new MockSmtpTransport();

            var rockEmailMessage = new RockEmailMessage();
            rockEmailMessage.ReplyToEmail = null;

            rockEmailMessage.AddRecipient( new RockEmailMessageRecipient( new Person
            {
                Email = "test@test.com",
                FirstName = "Test",
                LastName = "User"
            }, new Dictionary<string, object>() ) );

            // Act
            smtp.Send( rockEmailMessage, 0, new Dictionary<string, string>(), out List<string> errorMessages );

            // Assert
            Assert.That.AreEqual( 0, errorMessages.Count, errorMessages.JoinStrings(", ") );
        }

        #endregion
    }
}
