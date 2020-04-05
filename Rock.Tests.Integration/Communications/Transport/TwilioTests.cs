using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock;
using Rock.Communication.Medium;
using Rock.Communication.Transport;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared;

namespace Rock.Tests.Integration.Communications
{
    public class TwilioTests
    {
        [TestMethod]
        [Ignore("Dependant on a DB. Useful for debugging and adding data.")]
        public void ProcessResponseTestToRockWithKnownNumbers()
        {
            string toPhone = "+16237777794";
            string fromPhone = "+16128750967";
            string message = "Message sent on " + RockDateTime.Now.ToString();
            string errorMessage = "";

            var sms = new Rock.Communication.Medium.Sms();
            sms.ProcessResponse( toPhone, fromPhone, message, out errorMessage );
        }

        [TestMethod]
        [Ignore("Dependant on a DB. Useful for debugging and adding data.")]
        public void ProcessResponseTestToRockWithUnknownSenderNumber()
        {
            string toPhone = "+16237777794";
            string fromPhone = "+16128750971";
            string message = "Message from unknown sender sent on " + RockDateTime.Now.ToString();
            string errorMessage = "";

            var sms = new Rock.Communication.Medium.Sms();
            sms.ProcessResponse( toPhone, fromPhone, message, out errorMessage );
        }

    }
}
