using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Communication;
using Rock.Communication.Transport;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Communications.Transport
{
    [TestClass]
    public class SMTPTests
    {
        private static Guid ServerAttributeGuid = SystemGuid.Attribute.COMMUNICATION_TRANSPORT_SMTP_SERVER.AsGuid();
        private static string _serverAttributeValueValue;
        private static Guid _newServerAttributeValueGuid;

        #region Setup & Teardown

        [ClassInitialize]
        public static void Initialize( TestContext context )
        {
            SetAttributeValue( ServerAttributeGuid, 0, "localhost", out string previousValue, out Guid newAttributeValueGuid );
            _serverAttributeValueValue = previousValue;
            _newServerAttributeValueGuid = newAttributeValueGuid;
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            if ( !_newServerAttributeValueGuid.IsEmpty() )
            {
                // Delete it.
                DeleteAttributeValue( _newServerAttributeValueGuid );
            }

            if ( !string.IsNullOrEmpty( _serverAttributeValueValue ) )
            {
                // Set it back.
                SetAttributeValue( ServerAttributeGuid, 0, _serverAttributeValueValue, out _, out _ );
            }
        }

        #endregion

        #region Send Tests

        [TestMethod]
        public void SendShouldSendEmailWhenReplyToEmailIsNotDefined()
        {
            // Arrange
            var smtp = new SMTP();

            var rockEmailMessage = new RockEmailMessage();
            rockEmailMessage.ReplyToEmail = null;

            rockEmailMessage.AddRecipient( new RockEmailMessageRecipient( new Person
            {
                Email = "test@test.com",
                FirstName = "Test",
                LastName = "User"
            }, new Dictionary<string, object>() ) );

            // Act
            smtp.Send( (RockMessage)rockEmailMessage, 0, null, out List<string> errorMessages );

            // Assert
            Assert.That.AreEqual( 0, errorMessages.Count );
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Sets the value of an existing <see cref="AttributeValue"/> or creates a new database record if one doesn't already exist.
        /// </summary>
        /// <param name="attributeGuid">The parent <see cref="Rock.Model.Attribute"/> unique identifier.</param>
        /// <param name="entityId">The ID of the entity - if any - to which this <see cref="AttributeValue"/> belongs.</param>
        /// <param name="value">The value to be set.</param>
        /// <param name="previousValue">If a <see cref="AttributeValue"/> already exists in the database, it's value will be returned, so you can set it back after the current tests complete.</param>
        /// <param name="newAttributeValueGuid">If a <see cref="AttributeValue"/> doesn't already exist in the database, the <see cref="Guid"/> of the newly-created record will be returned, so you can delete it after the current tests complete.</param>
        private static void SetAttributeValue( Guid attributeGuid, int? entityId, string value, out string previousValue, out Guid newAttributeValueGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                previousValue = null;
                newAttributeValueGuid = Guid.Empty;

                var attributeId = AttributeCache.GetId( attributeGuid );
                if ( !attributeId.HasValue )
                {
                    return;
                }

                var attributeValueService = new AttributeValueService( rockContext );

                var attributeValue = attributeValueService.GetByAttributeIdAndEntityId( attributeId.Value, entityId );

                if ( attributeValue == null )
                {
                    attributeValue = new AttributeValue
                    {
                        AttributeId = attributeId.Value,
                        EntityId = entityId,
                        Value = value
                    };

                    attributeValueService.Add( attributeValue );

                    // Remember this so we can delete this AttributeValue upon cleanup.
                    newAttributeValueGuid = attributeValue.Guid;
                }
                else
                {
                    // Remeber this so we can set it back upon cleanup.
                    previousValue = attributeValue.Value;
                    attributeValue.Value = value;
                }

                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Deletes the attribute value.
        /// </summary>
        /// <param name="attributeValueGuid">The attribute value unique identifier.</param>
        private static void DeleteAttributeValue( Guid attributeValueGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var attributeValueService = new AttributeValueService( rockContext );
                var attributeValue = attributeValueService.Get( attributeValueGuid );
                if ( attributeValue == null )
                {
                    return;
                }

                attributeValueService.Delete( attributeValue );
                rockContext.SaveChanges();
            }
        }

        #endregion
    }
}
