using System.Linq;
using System.Net.Http;
using System.Web;

using UAParser;

namespace Rock.Net
{
    /// <summary>
    /// Provides information on a remote client that is making a request to the server.
    /// </summary>
    public class ClientInformation
    {
        #region Properties

        /// <summary>
        /// Gets the ip address.
        /// </summary>
        /// <value>
        /// The ip address.
        /// </value>
        public string IpAddress { get; }

        /// <summary>
        /// Gets the browser object that identifies what we know about the browser.
        /// </summary>
        /// <value>
        /// The browser object that identifies what we know about the browser.
        /// </value>
        public ClientInfo Browser { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientInformation"/> class.
        /// </summary>
        /// <param name="request">The request to initalize from.</param>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientInformation"/> class.
        /// </summary>
        /// <param name="request">The request to initalize from.</param>
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

        #endregion
    }
}
