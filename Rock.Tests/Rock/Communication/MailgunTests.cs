using System;
using System.Collections.Generic;
using Xunit;

using Rock.Communication.Transport;
using Rock.Communication;
using System.Linq;
using Rock.Data;
using Rock.Model;

namespace Rock.Tests.Rock.Communication
{
    public class MailgunTests
    {
        [Fact]
        public void HttpSendRockMessage()
        {
            var errorMessages = new List<string>();
            var binaryFileService = new BinaryFileService( new RockContext() );

            var rockEmailMessage = new RockEmailMessage
            {
                FromName = "Spark Info",
                FromEmail = "info@sparkdevnetwork.org",
                ReplyToEmail = "info@sparkdevnetwork.org",
                CCEmails = new List<string>() { "ethantd1@hotmail.com" },
                //BCCEmails = new List<string>() { "ethantd1@gmail.com" },
                Subject = "Mailgun HTTP Tests",
                Message = "This is a test of the mailgun HTTP API",
                //MessageMetaData = new Dictionary<string, string>() { { "", "" }, { "", "" } },
                Attachments = new List<BinaryFile>() { binaryFileService.Get( 10 ) }
            };

            rockEmailMessage.AddRecipient( "ethan@sparkdevnetwork.org" );

            MailGunHtml mailGunHtml = new MailGunHtml();
            mailGunHtml.Send( rockEmailMessage, 0, null, out errorMessages );

            Assert.True( !errorMessages.Any() );
            Assert.Equal( System.Net.HttpStatusCode.OK, mailGunHtml.Response.StatusCode );
        }

        [Fact]
        public void HttpSendCommunication()
        {



        }
    }
}
