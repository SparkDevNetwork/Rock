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
using DotLiquid;

using UAParser;

namespace Rock.Web
{
    /// <summary>
    /// Class to hold details about a
    /// </summary>
    public class BrowserClient : DotLiquid.Drop
    {
        /// <summary>
        /// Gets or sets the type of the client.
        /// </summary>
        /// <value>
        /// The type of the client.
        /// </value>
        public string ClientType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the client information.
        /// </summary>
        /// <value>
        /// The client information.
        /// </value>
        public BrowserInfo BrowserInfo { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is mobile.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is mobile; otherwise, <c>false</c>.
        /// </value>
        public bool IsMobile { get; set; } = false;
    }

    /// <summary>
    /// Information about the browser
    /// </summary>
    /// <seealso cref="DotLiquid.Drop" />
    public class BrowserInfo : Drop
    {
        private ClientInfo _client = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="BrowserInfo"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        public BrowserInfo( ClientInfo client )
        {
            _client = client;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BrowserInfo"/> class.
        /// </summary>
        /// <param name="userAgent">The user agent.</param>
        public BrowserInfo( string userAgent )
        {
            UAParser.Parser uaParser = UAParser.Parser.GetDefault();
            _client = uaParser.Parse( userAgent );
        }

        /// <summary>
        /// Gets the string.
        /// </summary>
        /// <value>
        /// The string.
        /// </value>
        public string String
        {
            get
            {
                return _client.String;
            }
        }

        /// <summary>
        /// Gets the Operating System.
        /// </summary>
        /// <value>
        /// The os.
        /// </value>
        public BrowserOS OS
        {
            get
            {
                if ( _browserOS == null )
                {
                    _browserOS = new BrowserOS( _client.OS.Family, _client.OS.Major, _client.OS.Minor, _client.OS.Patch, _client.OS.PatchMinor );
                }

                return _browserOS;
            }
        }
        private BrowserOS _browserOS = null;

        /// <summary>
        /// Gets the device.
        /// </summary>
        /// <value>
        /// The device.
        /// </value>
        public BrowserDevice Device
        {
            get
            {
                if (_device == null )
                {
                    _device = new BrowserDevice( _client.Device.Family, _client.Device.Brand, _client.Device.Model );
                }
                return _device;
            }
        }
        private BrowserDevice _device = null;

        /// <summary>
        /// Gets the user agent.
        /// </summary>
        /// <value>
        /// The user agent.
        /// </value>
        public BrowserUserAgent UserAgent {
            get
            {
                if ( _browserUserAgent == null )
                {
                    _browserUserAgent = new BrowserUserAgent( _client.UserAgent.Family, _client.UserAgent.Major, _client.UserAgent.Minor, _client.UserAgent.Patch );
                }

                return _browserUserAgent;
            }
        }
        private BrowserUserAgent _browserUserAgent = null;

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return _client.ToString();
        }
    }

    /// <summary>
    /// Information about the browser's OS
    /// </summary>
    public class BrowserOS : Drop
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrowserOS"/> class.
        /// </summary>
        /// <param name="family">The family.</param>
        /// <param name="major">The major.</param>
        /// <param name="minor">The minor.</param>
        /// <param name="patch">The patch.</param>
        /// <param name="patchMinor">The patch minor.</param>
        public BrowserOS( string family, string major, string minor, string patch, string patchMinor )
        {
            Family = family;
            Major = major;
            Minor = minor;
            Patch = patch;
            PatchMinor = patchMinor;
        }

        /// <summary>
        /// Gets the family.
        /// </summary>
        /// <value>
        /// The family.
        /// </value>
        public string Family { get; }

        /// <summary>
        /// Gets the major.
        /// </summary>
        /// <value>
        /// The major.
        /// </value>
        public string Major { get; }

        /// <summary>
        /// Gets the minor.
        /// </summary>
        /// <value>
        /// The minor.
        /// </value>
        public string Minor { get; }

        /// <summary>
        /// Gets the patch.
        /// </summary>
        /// <value>
        /// The patch.
        /// </value>
        public string Patch { get; }

        /// <summary>
        /// Gets the patch minor.
        /// </summary>
        /// <value>
        /// The patch minor.
        /// </value>
        public string PatchMinor { get; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{Family} {Major} {Minor} {Patch} {PatchMinor}";
        }
    }

    /// <summary>
    /// Information about the browser device
    /// </summary>
    /// <seealso cref="DotLiquid.Drop" />
    public class BrowserDevice : Drop
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrowserDevice"/> class.
        /// </summary>
        /// <param name="family">The family.</param>
        /// <param name="brand">The brand.</param>
        /// <param name="model">The model.</param>
        public BrowserDevice( string family, string brand, string model )
        {
            Family = family;
            Brand = brand;
            Model = model;
        }

        /// <summary>
        /// Gets the brand.
        /// </summary>
        /// <value>
        /// The brand.
        /// </value>
        public string Brand { get; }

        /// <summary>
        /// Gets the family.
        /// </summary>
        /// <value>
        /// The family.
        /// </value>
        public string Family { get; }

        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        public string Model { get; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{Brand} {Family} {Model}";
        }
    }

    /// <summary>
    /// The browser Agent
    /// </summary>
    public class BrowserUserAgent : Drop
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrowserUserAgent"/> class.
        /// </summary>
        /// <param name="family">The family.</param>
        /// <param name="major">The major.</param>
        /// <param name="minor">The minor.</param>
        /// <param name="patch">The patch.</param>
        public BrowserUserAgent( string family, string major, string minor, string patch )
        {
            Family = family;
            Major = major;
            Minor = minor;
            Patch = patch;
        }

        /// <summary>
        /// Gets the family.
        /// </summary>
        /// <value>
        /// The family.
        /// </value>
        public string Family { get; }

        /// <summary>
        /// Gets the major.
        /// </summary>
        /// <value>
        /// The major.
        /// </value>
        public string Major { get; }

        /// <summary>
        /// Gets the minor.
        /// </summary>
        /// <value>
        /// The minor.
        /// </value>
        public string Minor { get; }

        /// <summary>
        /// Gets the patch.
        /// </summary>
        /// <value>
        /// The patch.
        /// </value>
        public string Patch { get; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{Family} {Major} {Minor} {Patch}";
        }
    }
}
