using System.Collections.Generic;
using System.Linq;
using Xunit;

using Rock.Communication;
using Rock.Communication.Transport;
using Rock.Data;
using Rock.Model;

namespace Rock.Tests.CommunicationTests
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

            var mailGunHttp = new MailgunHttp();
            mailGunHttp.Send( rockEmailMessage, 0, null, out errorMessages );

            Assert.True( !errorMessages.Any() );
            Assert.Equal( System.Net.HttpStatusCode.OK, mailGunHttp.Response.StatusCode );
        }
    }
}
