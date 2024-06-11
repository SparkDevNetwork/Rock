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

using System;
using System.Collections.Specialized;
using System.Linq;
using RestSharp.Extensions;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class Interaction : Model<Interaction>
    {
        /// <summary>
        /// Sets the Urchin Tracking Module (UTM) fields from URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <remarks>
        /// Only those UTM fields that are specified in the URL are updated. Unspecified fields retain their current values.
        /// </remarks>
        public void SetUTMFieldsFromURL( string url )
        {
            if ( url.IsNullOrWhiteSpace() || url.IndexOf( "utm_", StringComparison.OrdinalIgnoreCase ) < 0 )
            {
                return;
            }

            try
            {
                NameValueCollection urlParams;

                if ( Uri.TryCreate( url, UriKind.Absolute, out var uri ) )
                {
                    urlParams = System.Web.HttpUtility.ParseQueryString( uri.Query );
                }
                else if ( url.IndexOf( "?" ) >= 0 )
                {
                    // If it's not a full URI but has a "?" character then
                    // assume it's a special format from an external application
                    // and just take everything after the "?".
                    urlParams = System.Web.HttpUtility.ParseQueryString( url.Substring( url.IndexOf( "?" ) + 1 ) );
                }
                else
                {
                    // Assume it's just a plain query string already.
                    urlParams = System.Web.HttpUtility.ParseQueryString( url );
                }

                // Get the UTM Defined Value fields.
                int? utmValueId;
                string utmValueText;

                GetUtmDefinedValueOrTextFromQueryString( urlParams, "utm_source", SystemGuid.DefinedType.UTM_SOURCE, out utmValueId, out utmValueText );
                this.SourceValueId = utmValueId;
                this.Source = utmValueText.Truncate( 25 );

                GetUtmDefinedValueOrTextFromQueryString( urlParams, "utm_medium", SystemGuid.DefinedType.UTM_MEDIUM, out utmValueId, out utmValueText );
                this.MediumValueId = utmValueId;
                this.Medium = utmValueText.Truncate( 25 );

                GetUtmDefinedValueOrTextFromQueryString( urlParams, "utm_campaign", SystemGuid.DefinedType.UTM_CAMPAIGN, out utmValueId, out utmValueText );
                this.CampaignValueId = utmValueId;
                this.Campaign = utmValueText.Truncate( 50 );

                // Get the UTM Text Fields.
                var content = urlParams.Get( "utm_content" );
                if ( content.IsNotNullOrWhiteSpace() )
                {
                    this.Content = content.Truncate( 50 );
                }

                var term = urlParams.Get( "utm_term" );
                if ( term.IsNotNullOrWhiteSpace() )
                {
                    this.Term = term.Truncate( 50 );
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( new Exception( $"Error parsing '{url}' to UTM fields.", ex ), null );
            }
        }

        private void GetUtmDefinedValueOrTextFromQueryString( NameValueCollection urlParams, string parameterName, string definedTypeGuid, out int? utmValueId, out string utmValueText )
        {
            utmValueId = null;
            utmValueText = null;

            var valueText = urlParams.Get( parameterName );
            if ( valueText.IsNullOrWhiteSpace() )
            {
                return;
            }

            valueText = valueText.UrlDecode();

            utmValueId = DefinedTypeCache.Get( definedTypeGuid.AsGuid() )
                                    .DefinedValues
                                    .FirstOrDefault( v => v.Value.Equals( valueText, StringComparison.OrdinalIgnoreCase ) )?.Id;
            // If no matching Defined Value, return the text value.
            if ( utmValueId == null )
            {
                utmValueText = valueText;
            }
        }

        /// <summary>
        /// Sets the interaction data (for example, the URL of the request), and obfuscates sensitive data that might be in the interactionData
        /// </summary>
        /// <param name="interactionData">The interaction data.</param>
        public void SetInteractionData( string interactionData )
        {
            this.InteractionData = interactionData.IsNotNullOrWhiteSpace() ? PersonToken.ObfuscateRockMagicToken( interactionData ) : string.Empty;
        }

        /// <summary>
        /// Get the name of the UtmSource.
        /// </summary>
        /// <returns></returns>
        public string GetUtmSourceName()
        {
            return GetUtmComponentNameFromDefinedValueOrText( SystemGuid.DefinedType.UTM_SOURCE, this.SourceValueId, this.Source );
        }

        /// <summary>
        /// Get the name of the UtmMedium.
        /// </summary>
        /// <returns></returns>
        public string GetUtmMediumName()
        {
            return GetUtmComponentNameFromDefinedValueOrText( SystemGuid.DefinedType.UTM_MEDIUM, this.MediumValueId, this.Medium );
        }

        /// <summary>
        /// Get the name of the UtmCampaign.
        /// </summary>
        /// <returns></returns>
        public string GetUtmCampaignName()
        {
            return GetUtmComponentNameFromDefinedValueOrText( SystemGuid.DefinedType.UTM_CAMPAIGN, this.CampaignValueId, this.Campaign );
        }

        private string GetUtmComponentNameFromDefinedValueOrText( string definedTypeGuid, int? definedValueId, string textValue )
        {
            if ( this.SourceValueId != null )
            {
                var name = DefinedTypeCache.Get( definedTypeGuid )
                    .DefinedValues
                    .FirstOrDefault( v => v.Id == definedValueId )?.Value;
                return name;
            }

            return textValue;
        }

    }
}
