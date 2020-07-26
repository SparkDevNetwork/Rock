﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Rock.Communication;
using Rock.Communication.Transport;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Communications
{
    [TestClass]
    public class EmailTransportComponentTests
    {
        private static byte[] _testTextFileBytes = File.ReadAllBytes( @"TestData\TextDoc.txt" );
        private static byte[] _testJpgFileBytes = File.ReadAllBytes( @"TestData\test.jpg" );

        #region Rock Email Message Test
        [TestMethod]
        public void SendRockMessageShouldReplaceUnsafeFromWithOrganizationEmail()
        {
            var expectedFromEmail = "test@org.com";
            var expectedFromName = "Test Name";

            var globalAttributes = GlobalAttributesCache.Get();
            globalAttributes.SetValue( "OrganizationEmail", expectedFromEmail, false, null );

            var actualEmail = new RockEmailMessage()
            {
                FromEmail = "info@test.com",
                FromName = expectedFromName,
                ReplyToEmail = "replyto@test.com"
            };

            actualEmail.AddRecipient( new RockEmailMessageRecipient( new Person
            {
                Email = "test@test.com",
                FirstName = "Test",
                LastName = "User"
            }, new Dictionary<string, object>() ) );

            var expectedEmail = new RockEmailMessage()
            {
                FromName = expectedFromName,
                FromEmail = expectedFromEmail,
                ReplyToEmail = $"{actualEmail.ReplyToEmail},{new MailAddress( actualEmail.FromEmail, actualEmail.FromName ).ToString()}"
            };

            var emailSendResponse = new EmailSendResponse
            {
                Status = Rock.Model.CommunicationRecipientStatus.Delivered,
                StatusNote = "Email Sent."
            };

            var emailTransport = new Mock<EmailTransportComponent>()
            {
                CallBase = true
            };

            emailTransport
                .Protected()
                .Setup<EmailSendResponse>( "SendEmail", ItExpr.IsAny<RockEmailMessage>() )
                .Returns( emailSendResponse )
                .Verifiable();

            emailTransport
                .Object
                .Send( actualEmail, 0, new Dictionary<string, string>(), out var errorMessages );

            Assert.That.IsEmpty( errorMessages );

            emailTransport
                .Protected()
                .Verify( "SendEmail",
                    Times.Once(),
                    ItExpr.Is<RockEmailMessage>( rem =>
                        rem.FromEmail == expectedEmail.FromEmail &&
                        rem.FromName == expectedEmail.FromName &&
                        rem.ReplyToEmail == expectedEmail.ReplyToEmail
                    )
                );
        }

        [TestMethod]
        public void SendRockMessageShouldNotReplaceSafeFromEmail()
        {
            AddSafeDomains();

            var expectedFromEmail = "test@org.com";
            var expectedFromName = "Test Name";

            var globalAttributes = GlobalAttributesCache.Get();
            globalAttributes.SetValue( "OrganizationEmail", "test@organization.com", false, null );

            var actualEmail = new RockEmailMessage()
            {
                FromEmail = expectedFromEmail,
                FromName = expectedFromName
            };

            actualEmail.AddRecipient( new RockEmailMessageRecipient( new Person
            {
                Email = "test@test.com",
                FirstName = "Test",
                LastName = "User"
            }, new Dictionary<string, object>() ) );

            var expectedEmail = new RockEmailMessage()
            {
                FromName = expectedFromName,
                FromEmail = expectedFromEmail
            };

            var emailSendResponse = new EmailSendResponse
            {
                Status = Rock.Model.CommunicationRecipientStatus.Delivered,
                StatusNote = "Email Sent."
            };

            var emailTransport = new Mock<EmailTransportComponent>()
            {
                CallBase = true
            };

            emailTransport
                .Protected()
                .Setup<EmailSendResponse>( "SendEmail", ItExpr.IsAny<RockEmailMessage>() )
                .Returns( emailSendResponse )
                .Verifiable();

            emailTransport
                .Object
                .Send( actualEmail, 0, new Dictionary<string, string>(), out var errorMessages );

            Assert.That.IsEmpty( errorMessages );

            emailTransport
                .Protected()
                .Verify( "SendEmail",
                    Times.Once(),
                    ItExpr.Is<RockEmailMessage>( rem =>
                        rem.FromEmail == expectedEmail.FromEmail &&
                        rem.FromName == expectedEmail.FromName
                    )
                );
        }

        [TestMethod]
        public void SendRockMessageShouldNotReplaceUnsafeFromButSafeToEmail()
        {
            AddSafeDomains();

            var expectedFromEmail = "from@organization.com";
            var expectedFromName = "Test Name";

            var globalAttributes = GlobalAttributesCache.Get();
            globalAttributes.SetValue( "OrganizationEmail", "org@organization.com", false, null );

            var actualEmail = new RockEmailMessage()
            {
                FromEmail = expectedFromEmail,
                FromName = expectedFromName
            };

            actualEmail.AddRecipient( new RockEmailMessageRecipient( new Person
            {
                Email = "test@org.com",
                FirstName = "Test",
                LastName = "User"
            }, new Dictionary<string, object>() ) );

            var expectedEmail = new RockEmailMessage()
            {
                FromName = expectedFromName,
                FromEmail = expectedFromEmail
            };

            var emailSendResponse = new EmailSendResponse
            {
                Status = Rock.Model.CommunicationRecipientStatus.Delivered,
                StatusNote = "Email Sent."
            };

            var emailTransport = new Mock<EmailTransportComponent>()
            {
                CallBase = true
            };

            emailTransport
                .Protected()
                .Setup<EmailSendResponse>( "SendEmail", ItExpr.IsAny<RockEmailMessage>() )
                .Returns( emailSendResponse )
                .Verifiable();

            emailTransport
                .Object
                .Send( actualEmail, 0, new Dictionary<string, string>(), out var errorMessages );

            Assert.That.IsEmpty( errorMessages );

            emailTransport
                .Protected()
                .Verify( "SendEmail",
                    Times.Once(),
                    ItExpr.Is<RockEmailMessage>( rem =>
                        rem.FromEmail == expectedEmail.FromEmail &&
                        rem.FromName == expectedEmail.FromName
                    )
                );
        }

        [TestMethod]
        public void SendRockMessageShouldReplaceFromWithSafeFromEmailWhenNoSafeToEmailFound()
        {
            AddSafeDomains();

            var expectedFromEmail = "org@organization.com";
            var expectedFromName = "Test Name";

            var globalAttributes = GlobalAttributesCache.Get();
            globalAttributes.SetValue( "OrganizationEmail", expectedFromEmail, false, null );

            var actualEmail = new RockEmailMessage()
            {
                FromEmail = "from@organization.com",
                FromName = expectedFromName
            };

            actualEmail.AddRecipient( new RockEmailMessageRecipient( new Person
            {
                Email = "test@test.com",
                FirstName = "Test",
                LastName = "User"
            }, new Dictionary<string, object>() ) );

            var expectedEmail = new RockEmailMessage()
            {
                FromName = expectedFromName,
                FromEmail = expectedFromEmail
            };

            var emailSendResponse = new EmailSendResponse
            {
                Status = Rock.Model.CommunicationRecipientStatus.Delivered,
                StatusNote = "Email Sent."
            };

            var emailTransport = new Mock<EmailTransportComponent>()
            {
                CallBase = true
            };

            emailTransport
                .Protected()
                .Setup<EmailSendResponse>( "SendEmail", ItExpr.IsAny<RockEmailMessage>() )
                .Returns( emailSendResponse )
                .Verifiable();

            emailTransport
                .Object
                .Send( actualEmail, 0, new Dictionary<string, string>(), out var errorMessages );

            Assert.That.IsEmpty( errorMessages );

            emailTransport
                .Protected()
                .Verify( "SendEmail",
                    Times.Once(),
                    ItExpr.Is<RockEmailMessage>( rem =>
                        rem.FromEmail == expectedEmail.FromEmail &&
                        rem.FromName == expectedEmail.FromName
                    )
                );
        }

        [TestMethod]
        public void SendRockMessageWithNoFromEmailShouldGetOrgEmail()
        {
            var expectedFromEmail = "org@organization.com";
            var expectedFromName = "Test Name";

            var globalAttributes = GlobalAttributesCache.Get();
            globalAttributes.SetValue( "OrganizationEmail", expectedFromEmail, false, null );

            var actualEmail = new RockEmailMessage()
            {
                FromEmail = "",
                FromName = expectedFromName
            };

            actualEmail.AddRecipient( new RockEmailMessageRecipient( new Person
            {
                Email = "test@test.com",
                FirstName = "Test",
                LastName = "User"
            }, new Dictionary<string, object>() ) );

            var expectedEmail = new RockEmailMessage()
            {
                FromName = expectedFromName,
                FromEmail = expectedFromEmail
            };

            var emailSendResponse = new EmailSendResponse
            {
                Status = Rock.Model.CommunicationRecipientStatus.Delivered,
                StatusNote = "Email Sent."
            };

            var emailTransport = new Mock<EmailTransportComponent>()
            {
                CallBase = true
            };

            emailTransport
                .Protected()
                .Setup<EmailSendResponse>( "SendEmail", ItExpr.IsAny<RockEmailMessage>() )
                .Returns( emailSendResponse )
                .Verifiable();

            emailTransport
                .Object
                .Send( actualEmail, 0, new Dictionary<string, string>(), out var errorMessages );

            Assert.That.IsEmpty( errorMessages );

            emailTransport
                .Protected()
                .Verify( "SendEmail",
                    Times.Once(),
                    ItExpr.Is<RockEmailMessage>( rem =>
                        rem.FromEmail == expectedEmail.FromEmail &&
                        rem.FromName == expectedEmail.FromName
                    )
                );
        }

        [TestMethod]
        public void SendRockMessageWithNoFromEmailAfterLavaEvaluationShouldGetOrgEmail()
        {
            var expectedFromEmail = "org@organization.com";
            var expectedFromName = "Test Name";

            var globalAttributes = GlobalAttributesCache.Get();
            globalAttributes.SetValue( "OrganizationEmail", expectedFromEmail, false, null );

            var actualEmail = new RockEmailMessage()
            {
                FromEmail = "{{fromEmail}}",
                FromName = expectedFromName
            };

            actualEmail.AddRecipient( new RockEmailMessageRecipient( new Person
            {
                Email = "test@test.com",
                FirstName = "Test",
                LastName = "User"
            }, new Dictionary<string, object>() ) );

            var expectedEmail = new RockEmailMessage()
            {
                FromName = expectedFromName,
                FromEmail = expectedFromEmail
            };

            var emailSendResponse = new EmailSendResponse
            {
                Status = Rock.Model.CommunicationRecipientStatus.Delivered,
                StatusNote = "Email Sent."
            };

            var emailTransport = new Mock<EmailTransportComponent>()
            {
                CallBase = true
            };

            emailTransport
                .Protected()
                .Setup<EmailSendResponse>( "SendEmail", ItExpr.IsAny<RockEmailMessage>() )
                .Returns( emailSendResponse )
                .Verifiable();

            emailTransport
                .Object
                .Send( actualEmail, 0, new Dictionary<string, string> { { "fromEmail", "" } }, out var errorMessages );

            Assert.That.IsEmpty( errorMessages );

            emailTransport
                .Protected()
                .Verify( "SendEmail",
                    Times.Once(),
                    ItExpr.Is<RockEmailMessage>( rem =>
                        rem.FromEmail == expectedEmail.FromEmail &&
                        rem.FromName == expectedEmail.FromName
                    )
                );
        }

        [TestMethod]
        public void SendRockMessageWithAnInvalidFromEmailShouldCauseError()
        {
            var actualEmail = new RockEmailMessage()
            {
                FromEmail = "invalidEmailAddress",
                FromName = "Test Name"
            };

            actualEmail.AddRecipient( new RockEmailMessageRecipient( new Person
            {
                Email = "test@test.com",
                FirstName = "Test",
                LastName = "User"
            }, new Dictionary<string, object>() ) );

            var emailSendResponse = new EmailSendResponse
            {
                Status = Rock.Model.CommunicationRecipientStatus.Delivered,
                StatusNote = "Email Sent."
            };

            var emailTransport = new Mock<EmailTransportComponent>()
            {
                CallBase = true
            };

            emailTransport
                .Protected()
                .Setup<EmailSendResponse>( "SendEmail", ItExpr.IsAny<RockEmailMessage>() )
                .Returns( emailSendResponse );

            emailTransport
                .Object
                .Send( actualEmail, 0, new Dictionary<string, string>(), out var errorMessages );

            Assert.That.AreEqual( 1, errorMessages.Count );
            Assert.That.AreEqual( "The specified string is not in the form required for an e-mail address.", errorMessages[0] );
        }

        [TestMethod]
        public void SendRockMessageWithAnNoFromEmailAndNoOrgEmailShouldCauseError()
        {
            var globalAttributes = GlobalAttributesCache.Get();
            globalAttributes.SetValue( "OrganizationEmail", "", false, null );

            var actualEmail = new RockEmailMessage()
            {
                FromEmail = "",
                FromName = "Test Name"
            };

            actualEmail.AddRecipient( new RockEmailMessageRecipient( new Person
            {
                Email = "test@test.com",
                FirstName = "Test",
                LastName = "User"
            }, new Dictionary<string, object>() ) );

            var emailSendResponse = new EmailSendResponse
            {
                Status = Rock.Model.CommunicationRecipientStatus.Delivered,
                StatusNote = "Email Sent."
            };

            var emailTransport = new Mock<EmailTransportComponent>()
            {
                CallBase = true
            };

            emailTransport
                .Protected()
                .Setup<EmailSendResponse>( "SendEmail", ItExpr.IsAny<RockEmailMessage>() )
                .Returns( emailSendResponse );

            emailTransport
                .Object
                .Send( actualEmail, 0, new Dictionary<string, string>(), out var errorMessages );

            Assert.That.AreEqual( 1, errorMessages.Count );
            Assert.That.AreEqual( "A From address was not provided.", errorMessages[0] );
        }

        [TestMethod]
        public void SendRockMessageShouldPopulatePropertiesCorrectly()
        {
            var expectedEmail = "test@test.com";
            var globalAttributes = GlobalAttributesCache.Get();
            globalAttributes.SetValue( "OrganizationEmail", expectedEmail, false, null );

            var actualPerson = new Person
            {
                Email = "test@test.com",
                FirstName = "Test",
                LastName = "User"
            };

            var actualEmailMessage = new RockEmailMessage()
            {
                FromEmail = expectedEmail,
                FromName = "Test Name",
                AppRoot = "test/approot",
                BCCEmails = new List<string> { "bcc1@test.com", "bcc2@test.com" },
                CCEmails = new List<string> { "cc1@test.com", "cc2@test.com" },
                CssInliningEnabled = true,
                CurrentPerson = actualPerson,
                EnabledLavaCommands = "RockEntity",
                Message = "HTML Message",
                MessageMetaData = new Dictionary<string, string> { { "test", "test1" } },
                PlainTextMessage = "Text Message",
                ReplyToEmail = "replyto@email.com",
                Subject = "Test Subject",
            };
            actualEmailMessage.AddRecipient( new RockEmailMessageRecipient( actualPerson, new Dictionary<string, object>() ) );

            var expectedEmailMessage = new RockEmailMessage()
            {
                FromEmail = expectedEmail,
                FromName = "Test Name",
                AppRoot = "test/approot",
                BCCEmails = new List<string> { "bcc1@test.com", "bcc2@test.com" },
                CCEmails = new List<string> { "cc1@test.com", "cc2@test.com" },
                CssInliningEnabled = true,
                CurrentPerson = actualPerson,
                EnabledLavaCommands = "RockEntity",
                Message = "HTML Message",
                MessageMetaData = new Dictionary<string, string> { { "test", "test1" } },
                PlainTextMessage = "Text Message",
                ReplyToEmail = "replyto@email.com",
                Subject = "Test Subject",
            };
            expectedEmailMessage.AddRecipient( new RockEmailMessageRecipient( actualPerson, new Dictionary<string, object>() ) );

            var emailSendResponse = new EmailSendResponse
            {
                Status = Rock.Model.CommunicationRecipientStatus.Delivered,
                StatusNote = "Email Sent."
            };

            var emailTransport = new Mock<EmailTransportComponent>()
            {
                CallBase = true
            };

            emailTransport
                .Protected()
                .Setup<EmailSendResponse>( "SendEmail", ItExpr.IsAny<RockEmailMessage>() )
                .Returns( emailSendResponse )
                .Verifiable();

            emailTransport
                .Object
                .Send( actualEmailMessage, 0, new Dictionary<string, string>(), out var errorMessages );

            Assert.That.IsEmpty( errorMessages );

            emailTransport
                .Protected()
                .Verify( "SendEmail",
                    Times.Once(),
                    ItExpr.Is<RockEmailMessage>( rem =>
                        rem.FromEmail == expectedEmailMessage.FromEmail &&
                        rem.FromName == expectedEmailMessage.FromName &&
                        rem.AppRoot == expectedEmailMessage.AppRoot &&
                        rem.CssInliningEnabled == expectedEmailMessage.CssInliningEnabled &&
                        rem.EnabledLavaCommands == expectedEmailMessage.EnabledLavaCommands &&
                        rem.Message == expectedEmailMessage.Message &&
                        rem.PlainTextMessage == expectedEmailMessage.PlainTextMessage &&
                        rem.ReplyToEmail.Contains( expectedEmailMessage.ReplyToEmail ) &&
                        rem.Subject == expectedEmailMessage.Subject &&
                        AreEquivelent( rem.BCCEmails, expectedEmailMessage.BCCEmails ) &&
                        AreEquivelent( rem.CCEmails, expectedEmailMessage.CCEmails )
                    )
                );
        }

        [TestMethod]
        public void SendRockMessageShouldHandleAttachmentsCorrectly()
        {
            var binaryFiles = AddBinaryFiles();
            var expectedEmail = "test@test.com";
            var globalAttributes = GlobalAttributesCache.Get();
            globalAttributes.SetValue( "OrganizationEmail", expectedEmail, false, null );

            var actualPerson = new Person
            {
                Email = "test@test.com",
                FirstName = "Test",
                LastName = "User"
            };

            var actualEmailMessage = new RockEmailMessage()
            {
                FromEmail = expectedEmail,
                FromName = "Test Name",
                AppRoot = "test/approot",
                BCCEmails = new List<string> { "bcc1@test.com", "bcc2@test.com" },
                CCEmails = new List<string> { "cc1@test.com", "cc2@test.com" },
                CssInliningEnabled = true,
                CurrentPerson = actualPerson,
                EnabledLavaCommands = "RockEntity",
                Message = "HTML Message",
                MessageMetaData = new Dictionary<string, string> { { "test", "test1" } },
                PlainTextMessage = "Text Message",
                ReplyToEmail = "replyto@email.com",
                Subject = "Test Subject"
            };
            actualEmailMessage.AddRecipient( new RockEmailMessageRecipient( actualPerson, new Dictionary<string, object>() ) );

            var expectedEmailMessage = new RockEmailMessage()
            {
                FromEmail = expectedEmail,
                FromName = "Test Name",
                AppRoot = "test/approot",
                Attachments = binaryFiles,
                BCCEmails = new List<string> { "bcc1@test.com", "bcc2@test.com" },
                CCEmails = new List<string> { "cc1@test.com", "cc2@test.com" },
                CssInliningEnabled = true,
                CurrentPerson = actualPerson,
                EnabledLavaCommands = "RockEntity",
                Message = "HTML Message",
                MessageMetaData = new Dictionary<string, string> { { "test", "test1" } },
                PlainTextMessage = "Text Message",
                ReplyToEmail = "replyto@email.com",
                Subject = "Test Subject",
            };
            expectedEmailMessage.AddRecipient( new RockEmailMessageRecipient( actualPerson, new Dictionary<string, object>() ) );

            var emailSendResponse = new EmailSendResponse
            {
                Status = Rock.Model.CommunicationRecipientStatus.Delivered,
                StatusNote = "Email Sent."
            };

            var emailTransport = new Mock<EmailTransportComponent>()
            {
                CallBase = true
            };

            emailTransport
                .Protected()
                .Setup<EmailSendResponse>( "SendEmail", ItExpr.IsAny<RockEmailMessage>() )
                .Returns( emailSendResponse )
                .Verifiable();

            emailTransport
                .Object
                .Send( expectedEmailMessage, 0, new Dictionary<string, string>(), out var errorMessages );

            Assert.That.IsEmpty( errorMessages );

            emailTransport
                .Protected()
                .Verify( "SendEmail",
                    Times.Once(),
                    ItExpr.Is<RockEmailMessage>( rem =>
                        AreEquivelent( expectedEmailMessage.Attachments.Select( a => a.ContentStream.Length ).ToList(), rem.Attachments.Select( a => a.ContentStream.Length ).ToList() ) &&
                        AreEquivelent( expectedEmailMessage.Attachments.Select( a => a.FileName ).ToList(), rem.Attachments.Select( a => a.FileName ).ToList() )
                    )
                );
        }


        #endregion

        #region Rock Communications Test
        [TestMethod]
        public void SendCommunicationShouldReplaceUnsafeFromWithOrganizationEmail()
        {
            var expectedFromEmail = "test@org.com";
            var expectedFromName = "Test Name";

            var globalAttributes = GlobalAttributesCache.Get();
            globalAttributes.SetValue( "OrganizationEmail", expectedFromEmail, false, null );

            var actualCommunication = CreateCommunication( expectedFromName );

            var expectedCommunication = new RockEmailMessage()
            {
                FromName = expectedFromName,
                FromEmail = expectedFromEmail,
                ReplyToEmail = $"{actualCommunication.ReplyToEmail},{new MailAddress( actualCommunication.FromEmail, actualCommunication.FromName ).ToString()}"
            };

            var emailSendResponse = new EmailSendResponse
            {
                Status = Rock.Model.CommunicationRecipientStatus.Delivered,
                StatusNote = "Email Sent."
            };

            var emailTransport = new Mock<EmailTransportComponent>()
            {
                CallBase = true
            };

            emailTransport
                .Protected()
                .Setup<EmailSendResponse>( "SendEmail", ItExpr.IsAny<RockEmailMessage>() )
                .Returns( emailSendResponse )
                .Verifiable();

            emailTransport
                .Object
                .Send( actualCommunication, 37, new Dictionary<string, string>() );

            emailTransport
                .Protected()
                .Verify( "SendEmail",
                    Times.Once(),
                    ItExpr.Is<RockEmailMessage>( rem =>
                        rem.FromEmail == expectedCommunication.FromEmail &&
                        rem.FromName == expectedCommunication.FromName &&
                        rem.ReplyToEmail == expectedCommunication.ReplyToEmail
                    )
                );
        }

        [TestMethod]
        public void SendCommunicationShouldNotReplaceSafeFromEmail()
        {
            AddSafeDomains();

            var expectedFromEmail = "test@org.com";
            var expectedFromName = "Test Name";

            var globalAttributes = GlobalAttributesCache.Get();
            globalAttributes.SetValue( "OrganizationEmail", "test@organization.com", false, null );

            var actualCommunication = CreateCommunication( expectedFromName, expectedFromEmail );

            var expectedEmail = new RockEmailMessage()
            {
                FromName = expectedFromName,
                FromEmail = expectedFromEmail
            };

            var emailSendResponse = new EmailSendResponse
            {
                Status = Rock.Model.CommunicationRecipientStatus.Delivered,
                StatusNote = "Email Sent."
            };

            var emailTransport = new Mock<EmailTransportComponent>()
            {
                CallBase = true
            };

            emailTransport
                .Protected()
                .Setup<EmailSendResponse>( "SendEmail", ItExpr.IsAny<RockEmailMessage>() )
                .Returns( emailSendResponse )
                .Verifiable();

            emailTransport
                .Object
                .Send( actualCommunication, 37, new Dictionary<string, string>() );

            emailTransport
                .Protected()
                .Verify( "SendEmail",
                    Times.Once(),
                    ItExpr.Is<RockEmailMessage>( rem =>
                        rem.FromEmail == expectedEmail.FromEmail &&
                        rem.FromName == expectedEmail.FromName
                    )
                );
        }

        [TestMethod]
        public void SendCommunicationShouldNotReplaceUnsafeFromButSafeToEmail()
        {
            AddSafeDomains();

            var expectedFromEmail = "from@organization.com";
            var expectedFromName = "Test Name";

            var globalAttributes = GlobalAttributesCache.Get();
            globalAttributes.SetValue( "OrganizationEmail", "org@organization.com", false, null );

            var actualCommunication = CreateCommunication( expectedFromName, expectedFromEmail, "test@org.com" );

            var expectedEmail = new RockEmailMessage()
            {
                FromName = expectedFromName,
                FromEmail = expectedFromEmail
            };

            var emailSendResponse = new EmailSendResponse
            {
                Status = Rock.Model.CommunicationRecipientStatus.Delivered,
                StatusNote = "Email Sent."
            };

            var emailTransport = new Mock<EmailTransportComponent>()
            {
                CallBase = true
            };

            emailTransport
                .Protected()
                .Setup<EmailSendResponse>( "SendEmail", ItExpr.IsAny<RockEmailMessage>() )
                .Returns( emailSendResponse )
                .Verifiable();

            emailTransport
                .Object
                .Send( actualCommunication, 37, new Dictionary<string, string>() );

            emailTransport
                .Protected()
                .Verify( "SendEmail",
                    Times.Once(),
                    ItExpr.Is<RockEmailMessage>( rem =>
                        rem.FromEmail == expectedEmail.FromEmail &&
                        rem.FromName == expectedEmail.FromName
                    )
                );
        }

        [TestMethod]
        public void SendCommunicationShouldReplaceFromWithSafeFromEmailWhenNoSafeToEmailFound()
        {
            AddSafeDomains();

            var expectedFromEmail = "org@organization.com";
            var expectedFromName = "Test Name";

            var globalAttributes = GlobalAttributesCache.Get();
            globalAttributes.SetValue( "OrganizationEmail", expectedFromEmail, false, null );

            var actualCommunication = CreateCommunication( expectedFromName, "from@organization.com" );

            var expectedEmail = new RockEmailMessage()
            {
                FromName = expectedFromName,
                FromEmail = expectedFromEmail
            };

            var emailSendResponse = new EmailSendResponse
            {
                Status = Rock.Model.CommunicationRecipientStatus.Delivered,
                StatusNote = "Email Sent."
            };

            var emailTransport = new Mock<EmailTransportComponent>()
            {
                CallBase = true
            };

            emailTransport
                .Protected()
                .Setup<EmailSendResponse>( "SendEmail", ItExpr.IsAny<RockEmailMessage>() )
                .Returns( emailSendResponse )
                .Verifiable();

            emailTransport
                .Object
                .Send( actualCommunication, 37, new Dictionary<string, string>() );

            emailTransport
                .Protected()
                .Verify( "SendEmail",
                    Times.Once(),
                    ItExpr.Is<RockEmailMessage>( rem =>
                        rem.FromEmail == expectedEmail.FromEmail &&
                        rem.FromName == expectedEmail.FromName
                    )
                );
        }

        [TestMethod]
        public void SendCommunicationWithNoFromEmailShouldGetOrgEmail()
        {
            var expectedFromEmail = "org@organization.com";
            var expectedFromName = "Test Name";

            var globalAttributes = GlobalAttributesCache.Get();
            globalAttributes.SetValue( "OrganizationEmail", expectedFromEmail, false, null );

            var actualCommunication = CreateCommunication( expectedFromName, "" );

            var expectedEmail = new RockEmailMessage()
            {
                FromName = expectedFromName,
                FromEmail = expectedFromEmail
            };

            var emailSendResponse = new EmailSendResponse
            {
                Status = Rock.Model.CommunicationRecipientStatus.Delivered,
                StatusNote = "Email Sent."
            };

            var emailTransport = new Mock<EmailTransportComponent>()
            {
                CallBase = true
            };

            emailTransport
                .Protected()
                .Setup<EmailSendResponse>( "SendEmail", ItExpr.IsAny<RockEmailMessage>() )
                .Returns( emailSendResponse )
                .Verifiable();

            emailTransport
                .Object
                .Send( actualCommunication, 37, new Dictionary<string, string>() );

            emailTransport
                .Protected()
                .Verify( "SendEmail",
                    Times.Once(),
                    ItExpr.Is<RockEmailMessage>( rem =>
                        rem.FromEmail == expectedEmail.FromEmail &&
                        rem.FromName == expectedEmail.FromName
                    )
                );
        }

        [TestMethod]
        public void SendCommunicationWithNoFromEmailAfterLavaEvaluationShouldGetOrgEmail()
        {
            var expectedFromEmail = "org@organization.com";
            var expectedFromName = "Test Name";

            var globalAttributes = GlobalAttributesCache.Get();
            globalAttributes.SetValue( "OrganizationEmail", expectedFromEmail, false, null );

            var actualCommunication = CreateCommunication( expectedFromName, "{{fromEmail}}" );

            var expectedEmail = new RockEmailMessage()
            {
                FromName = expectedFromName,
                FromEmail = expectedFromEmail
            };

            var emailSendResponse = new EmailSendResponse
            {
                Status = Rock.Model.CommunicationRecipientStatus.Delivered,
                StatusNote = "Email Sent."
            };

            var emailTransport = new Mock<EmailTransportComponent>()
            {
                CallBase = true
            };

            emailTransport
                .Protected()
                .Setup<EmailSendResponse>( "SendEmail", ItExpr.IsAny<RockEmailMessage>() )
                .Returns( emailSendResponse )
                .Verifiable();

            emailTransport
                .Object
                .Send( actualCommunication, 37, new Dictionary<string, string>() );

            emailTransport
                .Protected()
                .Verify( "SendEmail",
                    Times.Once(),
                    ItExpr.Is<RockEmailMessage>( rem =>
                        rem.FromEmail == expectedEmail.FromEmail &&
                        rem.FromName == expectedEmail.FromName
                    )
                );
        }

        [TestMethod]
        public void SendCommunicationWithAnInvalidFromEmailShouldCauseError()
        {
            var actualCommunication = CreateCommunication( "Test Name", "invalidEmailAddress" );

            var emailSendResponse = new EmailSendResponse
            {
                Status = Rock.Model.CommunicationRecipientStatus.Delivered,
                StatusNote = "Email Sent."
            };

            var emailTransport = new Mock<EmailTransportComponent>()
            {
                CallBase = true
            };

            emailTransport
                .Protected()
                .Setup<EmailSendResponse>( "SendEmail", ItExpr.IsAny<RockEmailMessage>() )
                .Returns( emailSendResponse );

            emailTransport
                .Object
                .Send( actualCommunication, 37, new Dictionary<string, string>() );

            var actualReciepent = new CommunicationRecipientService( new RockContext() ).Get( actualCommunication.Recipients.FirstOrDefault().Id );
            Assert.That.AreEqual( CommunicationRecipientStatus.Failed, actualReciepent.Status );
            Assert.That.AreEqual( "Exception: The specified string is not in the form required for an e-mail address.", actualReciepent.StatusNote );
        }

        [TestMethod]
        public void SendCommunicationWithAnNoFromEmailAndNoOrgEmailShouldCauseError()
        {
            var globalAttributes = GlobalAttributesCache.Get();
            globalAttributes.SetValue( "OrganizationEmail", "", false, null );

            var actualCommunication = CreateCommunication( "Test Name", "" );

            var emailSendResponse = new EmailSendResponse
            {
                Status = Rock.Model.CommunicationRecipientStatus.Delivered,
                StatusNote = "Email Sent."
            };

            var emailTransport = new Mock<EmailTransportComponent>()
            {
                CallBase = true
            };

            emailTransport
                .Protected()
                .Setup<EmailSendResponse>( "SendEmail", ItExpr.IsAny<RockEmailMessage>() )
                .Returns( emailSendResponse );

            emailTransport
                .Object
                .Send( actualCommunication, 37, new Dictionary<string, string>() );

            var actualReciepent = new CommunicationRecipientService( new RockContext() ).Get( actualCommunication.Recipients.FirstOrDefault().Id );
            Assert.That.AreEqual( CommunicationRecipientStatus.Failed, actualReciepent.Status );
            Assert.That.Contains( actualReciepent.StatusNote, "Exception: The parameter 'address' cannot be an empty string." );
        }

        [TestMethod]
        public void SendCommunicationShouldPopulatePropertiesCorrectly()
        {
            var expectedEmail = "test@test.com";
            var globalAttributes = GlobalAttributesCache.Get();
            globalAttributes.SetValue( "OrganizationEmail", expectedEmail, false, null );
            globalAttributes.SetValue( "PublicApplicationRoot", "test/approot", false, null );

            var actualCommunication = CreateCommunication( "Test Name", expectedEmail );

            var expectedEmailMessage = new RockEmailMessage()
            {
                FromEmail = expectedEmail,
                FromName = "Test Name",
                AppRoot = "test/approot/",
                Attachments = new List<BinaryFile> { new BinaryFile { FileName = "test.txt" } },
                BCCEmails = new List<string> { "bcc1@test.com", "bcc2@test.com" },
                CCEmails = new List<string> { "cc1@test.com", "cc2@test.com" },
                CssInliningEnabled = true,
                EnabledLavaCommands = "RockEntity",
                Message = "<html><head></head><body>HTML Message</body></html>",
                MessageMetaData = new Dictionary<string, string> { { "test", "test1" } },
                PlainTextMessage = "Text Message",
                ReplyToEmail = "replyto@email.com",
                Subject = "Test Subject",
            };

            var emailSendResponse = new EmailSendResponse
            {
                Status = Rock.Model.CommunicationRecipientStatus.Delivered,
                StatusNote = "Email Sent."
            };

            var emailTransport = new Mock<EmailTransportComponent>()
            {
                CallBase = true
            };

            emailTransport
                .Protected()
                .Setup<EmailSendResponse>( "SendEmail", ItExpr.IsAny<RockEmailMessage>() )
                .Returns( emailSendResponse )
                .Verifiable();

            emailTransport
                .Object
                .Send( actualCommunication, 37, new Dictionary<string, string> { { "DefaultPlainText", "Text Message" } } );

            emailTransport
                .Protected()
                .Verify( "SendEmail",
                    Times.Once(),
                    ItExpr.Is<RockEmailMessage>( rem =>
                        rem.FromEmail == expectedEmailMessage.FromEmail &&
                        rem.FromName == expectedEmailMessage.FromName &&
                        rem.AppRoot == expectedEmailMessage.AppRoot &&
                        rem.CssInliningEnabled == expectedEmailMessage.CssInliningEnabled &&
                        rem.EnabledLavaCommands == expectedEmailMessage.EnabledLavaCommands &&
                        rem.Message == expectedEmailMessage.Message &&
                        rem.PlainTextMessage == expectedEmailMessage.PlainTextMessage &&
                        rem.ReplyToEmail.Contains( expectedEmailMessage.ReplyToEmail ) &&
                        rem.Subject == expectedEmailMessage.Subject &&
                        AreEquivelent( rem.BCCEmails, expectedEmailMessage.BCCEmails ) &&
                        AreEquivelent( rem.CCEmails, expectedEmailMessage.CCEmails )
                    )
                );
        }

        [TestMethod]
        public void SendCommunicationShouldHandleAttachmentsCorrectly()
        {
            var expectedEmail = "test@test.com";
            var globalAttributes = GlobalAttributesCache.Get();
            globalAttributes.SetValue( "OrganizationEmail", expectedEmail, false, null );
            globalAttributes.SetValue( "PublicApplicationRoot", "test/approot", false, null );

            var actualCommunication = CreateCommunication( "Test Name", expectedEmail, attachments: AddBinaryFiles() );

            var expectedEmailMessage = new RockEmailMessage()
            {
                FromEmail = expectedEmail,
                FromName = "Test Name",
                AppRoot = "test/approot/",
                Attachments = AddBinaryFiles(),
                BCCEmails = new List<string> { "bcc1@test.com", "bcc2@test.com" },
                CCEmails = new List<string> { "cc1@test.com", "cc2@test.com" },
                CssInliningEnabled = true,
                EnabledLavaCommands = "RockEntity",
                Message = "<html><head></head><body>HTML Message</body></html>",
                MessageMetaData = new Dictionary<string, string> { { "test", "test1" } },
                PlainTextMessage = "Text Message",
                ReplyToEmail = "replyto@email.com",
                Subject = "Test Subject",
            };

            var emailSendResponse = new EmailSendResponse
            {
                Status = Rock.Model.CommunicationRecipientStatus.Delivered,
                StatusNote = "Email Sent."
            };

            var emailTransport = new Mock<EmailTransportComponent>()
            {
                CallBase = true
            };

            emailTransport
                .Protected()
                .Setup<EmailSendResponse>( "SendEmail", ItExpr.IsAny<RockEmailMessage>() )
                .Returns( emailSendResponse )
                .Verifiable();

            emailTransport
                .Object
                .Send( actualCommunication, 37, new Dictionary<string, string> { { "DefaultPlainText", "Text Message" } } );

            emailTransport
                .Protected()
                .Verify( "SendEmail",
                    Times.Once(),
                    ItExpr.Is<RockEmailMessage>( rem =>
                        AreEquivelent( expectedEmailMessage.Attachments.Select( a => a.ContentStream.Length ).ToList(), rem.Attachments.Select( a => a.ContentStream.Length ).ToList() ) &&
                        AreEquivelent( expectedEmailMessage.Attachments.Select( a => a.FileName ).ToList(), rem.Attachments.Select( a => a.FileName ).ToList() )
                    )
                );
        }
        #endregion

        private void AddSafeDomains()
        {
            using ( var rockContext = new RockContext() )
            {
                var definedTypeService = new DefinedValueService( rockContext );
                var definedType = new DefinedTypeService( rockContext ).Get( SystemGuid.DefinedType.COMMUNICATION_SAFE_SENDER_DOMAINS.AsGuid() );

                rockContext.Database.ExecuteSqlCommand( $"DELETE DefinedValue WHERE DefinedTypeId = {definedType.Id} AND Value = 'org.com'" );

                var definedValue = new DefinedValue { Id = 0 };
                definedValue.DefinedTypeId = definedType.Id;
                definedValue.IsSystem = false;
                definedValue.Value = "org.com";
                definedValue.Description = "This is a test safe domain.";
                definedValue.IsActive = true;
                definedTypeService.Add( definedValue );
                rockContext.SaveChanges();

                definedValue.LoadAttributes();
                definedValue.SetAttributeValue( "SafeToSendTo", "true" );
                definedValue.SaveAttributeValues( rockContext );
            }
        }

        private bool AreEquivelent<T>( List<T> expectedList, List<T> actualList )
        {
            Assert.That.AreEqual( expectedList, actualList );
            return true;
        }

        private int _communicationId = 0;
        private Rock.Model.Communication CreateCommunication( string expectedFromName,
            string expectedFromEmail = "info@test.com",
            string toEmailAddress = "test@test.com",
            List<BinaryFile> attachments = null )
        {
            var actualPerson = new Person
            {
                Email = toEmailAddress,
                FirstName = "Test",
                LastName = "User"
            };

            var actualCommunication = new Rock.Model.Communication()
            {
                FromEmail = expectedFromEmail,
                FromName = expectedFromName,
                Status = CommunicationStatus.Approved,
                ReplyToEmail = "replyto@test.com"
            };

            actualCommunication.BCCEmails = "bcc1@test.com,bcc2@test.com";
            actualCommunication.CCEmails = "cc1@test.com,cc2@test.com";
            actualCommunication.CommunicationTemplate = new CommunicationTemplate
            {
                Name = Guid.NewGuid().ToString(),
                CssInliningEnabled = true
            };
            actualCommunication.EnabledLavaCommands = "RockEntity";
            actualCommunication.Message = "HTML Message";
            actualCommunication.AdditionalLavaFields = new Dictionary<string, object> { { "test", "test1" } };
            actualCommunication.ReplyToEmail = "replyto@email.com";
            actualCommunication.Subject = "Test Subject";

            if ( attachments != null )
            {
                var communicationAttachments = new List<CommunicationAttachment>();

                for ( int i = 0; i < attachments.Count; i++ )
                {
                    communicationAttachments.Add( new CommunicationAttachment
                    {
                        BinaryFileId = attachments[i].Id,
                        CommunicationType = CommunicationType.Email
                    } );
                }

                actualCommunication.Attachments = communicationAttachments;
            }

            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );
                personService.Add( actualPerson );
                rockContext.SaveChanges();

                var communicationService = new CommunicationService( rockContext );
                communicationService.Add( actualCommunication );
                rockContext.SaveChanges();

                _communicationId = actualCommunication.Id;

                actualCommunication.Recipients.Add( new CommunicationRecipient
                {
                    PersonAlias = actualPerson.Aliases.FirstOrDefault(),
                    Status = CommunicationRecipientStatus.Pending,
                    MediumEntityTypeId = 37
                } );

                rockContext.SaveChanges();
            }

            return actualCommunication;
        }

        private List<BinaryFile> _testFiles = null;
        private List<BinaryFile> AddBinaryFiles()
        {
            if ( _testFiles != null )
            {
                return _testFiles;
            }

            var rockContext = new RockContext();
            var textFile = new BinaryFile
            {
                BinaryFileTypeId = 3,
                FileName = "test.txt",
                MimeType = "text/plain",
                DatabaseData = new BinaryFileData
                {
                    Content = _testTextFileBytes
                }
            };

            var jpgFile = new BinaryFile
            {
                BinaryFileTypeId = 3,
                FileName = "test.jpg",
                MimeType = "image/jpeg",
                DatabaseData = new BinaryFileData
                {
                    Content = _testJpgFileBytes
                }
            };

            var binaryFileService = new BinaryFileService( rockContext );
            binaryFileService.Add( textFile );
            binaryFileService.Add( jpgFile );
            rockContext.SaveChanges();

            return new List<BinaryFile>
            {
                textFile,
                jpgFile
            };
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            using ( var rockContext = new RockContext() )
            {
                if ( _communicationId > 0 )
                {
                    var communicationServer = new CommunicationService( rockContext );
                    communicationServer.Delete( communicationServer.Get( _communicationId ) );
                    rockContext.SaveChanges();
                    _communicationId = 0;
                }

                if ( _testFiles != null )
                {
                    for ( int i = 0; i < _testFiles.Count; i++ )
                    {
                        var binaryFileService = new BinaryFileService( rockContext );
                        binaryFileService.Delete( _testFiles[i] );
                    }
                    rockContext.SaveChanges();
                }
            }
        }
    }
}
