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

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Rock.Utility
{
    /// <summary>
    /// Represent the Cache-Control settings used inside Rock.
    /// </summary>
    public class RockCacheability
    {
        private const string PUBLIC_OPTION = "public";
        private const string PRIVATE_OPTION = "private";
        private const string NO_CACHE_OPTION = "no-cache";
        private const string NO_STORE_OPTION = "no-store";
        private const string MAX_AGE_OPTION = "max-age";
        private const string MAX_SHARED_AGE_OPTION = "s-maxage";

        private readonly HashSet<string> _noAgeOptions = new HashSet<string>( new string[] { NO_CACHE_OPTION, NO_STORE_OPTION } );
        private readonly Dictionary<RockCacheablityType, string> _cacheabilityTypeOptionMap = new Dictionary<RockCacheablityType, string>
        {
                {RockCacheablityType.Public, PUBLIC_OPTION },
                {RockCacheablityType.Private, PRIVATE_OPTION },
                {RockCacheablityType.NoCache, NO_CACHE_OPTION },
                {RockCacheablityType.NoStore, NO_STORE_OPTION },
        };

        private readonly Dictionary<string, bool> _supportedCacheOptions = new Dictionary<string, bool>
        {
            {PUBLIC_OPTION, false },
            {PRIVATE_OPTION, false },
            {NO_CACHE_OPTION, false },
            {NO_STORE_OPTION, false },
            {MAX_AGE_OPTION, true },
            {MAX_SHARED_AGE_OPTION, true }
        };

        private readonly Dictionary<string, TimeInterval> _cacheOptions = new Dictionary<string, TimeInterval>();

        private RockCacheablityType _rockCacheablityType;
        /// <summary>
        /// Gets or sets the directive that will be used in the Cache-Control header.
        /// </summary>
        /// <value>
        /// The Rock Cacheablity Type.
        /// </value>
        public RockCacheablityType RockCacheablityType
        {
            get
            {
                return _rockCacheablityType;
            }
            set
            {
                _rockCacheablityType = value;
                var option = _cacheabilityTypeOptionMap[_rockCacheablityType];
                SetBooleanPropertyValue( option, true );
            }
        }

        /// <summary>
        /// Gets or sets the maximum age in the Cache-Control header.
        /// This value will be ignored for no-cache and no-store.
        /// This value corresponds to the max-age directive of the Cache-Control header
        /// </summary>
        /// <value>
        /// The maximum age.
        /// </value>
        public TimeInterval MaxAge
        {
            get => GetTimeIntervalPropertyValue( MAX_AGE_OPTION );
            set => SetTimeIntervalPropertyValue( MAX_AGE_OPTION, value );
        }

        /// <summary>
        /// Gets or sets the shared maximum age in the Cache-Control header.
        /// This value will be ignored for no-cache and no-store.
        /// This value corresponds to the s-maxage directive of the Cache-Control header
        /// </summary>
        /// <value>
        /// The maximum shared age.
        /// </value>
        public TimeInterval SharedMaxAge
        {
            get => GetTimeIntervalPropertyValue( MAX_SHARED_AGE_OPTION );
            set => SetTimeIntervalPropertyValue( MAX_SHARED_AGE_OPTION, value );
        }

        /// <summary>
        /// Returns a value indicating whether or not the specified cacheablity type supports the age directives.
        /// </summary>
        /// <param name="cacheablityType">Type of the cacheablity.</param>
        /// <returns></returns>
        public bool OptionSupportsAge( RockCacheablityType cacheablityType )
        {
            return !_noAgeOptions.Contains( _cacheabilityTypeOptionMap[cacheablityType] );
        }

        /// <summary>
        /// Returns the string that can be used as the Cache-Control header.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var sbCacheString = new StringBuilder();
            foreach ( var option in _cacheOptions )
            {
                if ( _supportedCacheOptions[option.Key] )
                {
                    if ( option.Value != null )
                    {
                        AppendComma( sbCacheString );
                        sbCacheString.Append( $"{option.Key}={option.Value.ToSeconds()}" );
                    }
                }
                else
                {
                    AppendComma( sbCacheString );
                    sbCacheString.Append( option.Key );
                }
            }
            return sbCacheString.ToString();
        }

        private static void AppendComma( StringBuilder sbCacheString )
        {
            if ( sbCacheString.Length > 0 )
            {
                sbCacheString.Append( "," );
            }
        }

        /// <summary>
        /// Setups the HTTP cache policy based on the current RockCacheability properties.
        /// </summary>
        /// <param name="httpCachePolicy">The HTTP cache policy.</param>
        public void SetupHttpCachePolicy( HttpCachePolicy httpCachePolicy )
        {
            switch ( RockCacheablityType )
            {
                case RockCacheablityType.NoCache:
                    httpCachePolicy.SetCacheability( HttpCacheability.NoCache );
                    break;
                case RockCacheablityType.Public:
                    httpCachePolicy.SetCacheability( HttpCacheability.Public );
                    break;
                case RockCacheablityType.Private:
                    httpCachePolicy.SetCacheability( HttpCacheability.Private );
                    break;
                case RockCacheablityType.NoStore:
                    httpCachePolicy.SetNoStore();
                    break;
            }

            if ( OptionSupportsAge( RockCacheablityType ) )
            {
                if ( MaxAge != null )
                {
                    httpCachePolicy.SetMaxAge( new System.TimeSpan( 0, 0, MaxAge.ToSeconds() ) );
                }

                if ( SharedMaxAge != null )
                {
                    httpCachePolicy.SetProxyMaxAge( new System.TimeSpan( 0, 0, SharedMaxAge.ToSeconds() ) );
                }
            }
        }

        private void SetBooleanPropertyValue( string option, bool value )
        {
            // Remove the main cache header options
            _cacheOptions.Remove( PUBLIC_OPTION );
            _cacheOptions.Remove( PRIVATE_OPTION );
            _cacheOptions.Remove( NO_CACHE_OPTION );
            _cacheOptions.Remove( NO_STORE_OPTION );

            if ( value )
            {
                _cacheOptions[option] = null;
                var optionDoesNotSupportAge = option == NO_CACHE_OPTION || option == NO_STORE_OPTION;

                if ( optionDoesNotSupportAge )
                {
                    _cacheOptions.Remove( MAX_AGE_OPTION );
                    _cacheOptions.Remove( MAX_SHARED_AGE_OPTION );
                }
            }
        }

        private void SetTimeIntervalPropertyValue( string option, TimeInterval value )
        {
            var isAgeOption = option == MAX_AGE_OPTION || option == MAX_SHARED_AGE_OPTION;
            var currentSettingsSupportAgeOption = !_noAgeOptions.Any( ao => _cacheOptions.Keys.Contains( ao ) );

            if ( isAgeOption && currentSettingsSupportAgeOption )
            {
                _cacheOptions[option] = value;
            }

            if ( !isAgeOption )
            {
                _cacheOptions[option] = value;
            }
        }

        private TimeInterval GetTimeIntervalPropertyValue( string option )
        {
            if ( _cacheOptions.Keys.Contains( option ) )
            {
                return _cacheOptions[option];
            }
            return null;
        }
    }
}
