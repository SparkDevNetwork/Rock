using System.Linq;
using System.Net.Http;
using System.Web;

using UAParser;

namespace Rock.Net
{
    public class ClientInformation
    {
        public string IpAddress { get; }

        public ClientInfo Browser { get; }

        internal ClientInformation( HttpRequest request )
        {
            //
            // Set IP Address.
            //
            IpAddress = string.Empty;

            // http://stackoverflow.com/questions/735350/how-to-get-a-users-client-ip-address-in-asp-net
            string ipAddress = request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if ( !string.IsNullOrEmpty( ipAddress ) )
            {
                string[] addresses = ipAddress.Split( ',' );
                if ( addresses.Length != 0 )
                {
                    IpAddress = addresses[0];
                }
            }
            else
            {
                IpAddress = request.ServerVariables["REMOTE_ADDR"];
            }

            // nicely format localhost
            if ( IpAddress == "::1" )
            {
                IpAddress = "localhost";
            }

            Parser uaParser = Parser.GetDefault();
            Browser = uaParser.Parse( request.UserAgent );
        }

        internal ClientInformation( HttpRequestMessage request )
        {
            //
            // Set IP Address.
            //
            IpAddress = string.Empty;

            // http://stackoverflow.com/questions/735350/how-to-get-a-users-client-ip-address-in-asp-net
            if ( request.Headers.Contains( "X-FORWARDED-FOR" ) )
            {
                IpAddress = request.Headers.GetValues( "X-FORWARDED-FOR" ).First();
            }
            else if ( request.Properties.ContainsKey( "MS_HttpContext" ) )
            {
                IpAddress = ( ( HttpContextWrapper ) request.Properties["MS_HttpContext"] )?.Request?.UserHostAddress ?? string.Empty;
            }

            // nicely format localhost
            if ( IpAddress == "::1" )
            {
                IpAddress = "localhost";
            }

            Parser uaParser = Parser.GetDefault();
            Browser = uaParser.Parse( request.Headers.UserAgent.ToString() );
        }
    }
}
