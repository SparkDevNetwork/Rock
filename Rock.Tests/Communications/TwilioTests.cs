using System.Collections.Generic;
using System.Linq;
using Xunit;

using Rock;
using Rock.Communication.Medium;
using Rock.Communication.Transport;
using Rock.Data;
using Rock.Model;

namespace Rock.Tests.Communications
{
    public class TwilioTests
    {
        [Fact(Skip = "Dependant on a DB. Useful for debugging and adding data.")]
        public void ProcessResponseTestToRockWithKnownNumbers()
        {
            string toPhone = "+16237777794";
            string fromPhone = "+16128750967";
            string message = "Message sent on " + RockDateTime.Now.ToString();
            string errorMessage = "";

            var sms = new Sms();
            sms.ProcessResponse( toPhone, fromPhone, message, out errorMessage );
        }

        [Fact(Skip = "Dependant on a DB. Useful for debugging and adding data.")]
        public void ProcessResponseTestToRockWithUnknownSenderNumber()
        {
            string toPhone = "+16237777794";
            string fromPhone = "+16128750971";
            string message = "Message from unknown sender sent on " + RockDateTime.Now.ToString();
            string errorMessage = "";

            var sms = new Sms();
            sms.ProcessResponse( toPhone, fromPhone, message, out errorMessage );
        }

    }
}
