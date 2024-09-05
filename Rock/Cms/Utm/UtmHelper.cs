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
using System.Linq;
using System.Web;
using Rock.Model;
using Rock.Transactions;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Cms.Utm
{
    /// <summary>
    /// Methods for processing Urchin Tracking Module (UTM) data.
    /// </summary>
    public class UtmHelper
    {
        private static Guid _utmSourceGuid = SystemGuid.DefinedType.UTM_SOURCE.AsGuid();
        private static Guid _utmMediumGuid = SystemGuid.DefinedType.UTM_MEDIUM.AsGuid();
        private static Guid _utmCampaignGuid = SystemGuid.DefinedType.UTM_CAMPAIGN.AsGuid();

        /// <summary>
        /// Retrieve UTM data for the current request from the associated cookie.
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <returns></returns>
        public static UtmCookieData GetUtmCookieDataFromRequest( HttpRequest httpRequest )
        {
            // If we have a UTM cookie, add the information to the interaction.
            UtmCookieData utmInfo = null;
            if ( httpRequest != null
                 && httpRequest.Cookies.AllKeys.Contains( UtmCookieData.UTM_COOKIE_NAME ) )
            {
                utmInfo = httpRequest.Cookies[UtmCookieData.UTM_COOKIE_NAME]?.Value.FromJsonOrNull<UtmCookieData>();
            }

            return utmInfo;
        }

        /// <summary>
        /// Store UTM data for the current request in a cookie.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="utmCookieData"></param>
        /// <returns></returns>
        public static void SetUtmCookieDataForRequest( HttpContextBase httpContext, UtmCookieData utmCookieData )
        {
            if ( httpContext == null
                 || utmCookieData == null
                 || utmCookieData.IsEmpty() )
            {
                return;
            }

            var cookie = new HttpCookie( UtmCookieData.UTM_COOKIE_NAME, utmCookieData.ToJson() );

            WebRequestHelper.AddOrUpdateCookie( httpContext, cookie );
        }

        /// <inheritdoc cref="SetUtmCookieDataForRequest(HttpContextBase, UtmCookieData)" />
        public static void SetUtmCookieDataForRequest( HttpContext httpContext, UtmCookieData utmCookieData )
        {
            SetUtmCookieDataForRequest( new HttpContextWrapper( httpContext ), utmCookieData );
        }

        /// <summary>
        /// Add UTM data to an action that registers a page interaction.
        /// </summary>
        /// <param name="actionInfo"></param>
        /// <param name="utmInfo"></param>
        /// <returns></returns>
        internal static void AddUtmInfoToRegisterPageInteractionAction( RegisterPageInteractionActionInfo actionInfo, UtmCookieData utmInfo )
        {
            if ( utmInfo == null )
            {
                return;
            }

            // Decode the values. Use UnescapeDataString() here because HttpUtility.UrlDecode() replaces "+" with " " which is unwanted behavior.
            actionInfo.InteractionSource = System.Uri.UnescapeDataString( utmInfo.Source.ToStringSafe() );
            actionInfo.InteractionMedium = System.Uri.UnescapeDataString( utmInfo.Medium.ToStringSafe() );
            actionInfo.InteractionCampaign = System.Uri.UnescapeDataString( utmInfo.Campaign.ToStringSafe() );
            actionInfo.InteractionTerm = System.Uri.UnescapeDataString( utmInfo.Term.ToStringSafe() );
            actionInfo.InteractionContent = System.Uri.UnescapeDataString( utmInfo.Content.ToStringSafe() );
        }

        /// <summary>
        /// Add UTM data to a transaction that registers an interaction.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="utmInfo"></param>
        /// <returns></returns>
        public static void AddUtmInfoToInteractionTransactionInfo( InteractionTransactionInfo info, UtmCookieData utmInfo )
        {
            if ( utmInfo == null )
            {
                return;
            }

            // Decode the values. Use UnescapeDataString() here because HttpUtility.UrlDecode() replaces "+" with " " which is unwanted behavior.
            info.InteractionSource = System.Uri.UnescapeDataString( utmInfo.Source.ToStringSafe() );
            info.InteractionMedium = System.Uri.UnescapeDataString( utmInfo.Medium.ToStringSafe() );
            info.InteractionCampaign = System.Uri.UnescapeDataString( utmInfo.Campaign.ToStringSafe() );
            info.InteractionTerm = System.Uri.UnescapeDataString( utmInfo.Term.ToStringSafe() );
            info.InteractionContent = System.Uri.UnescapeDataString( utmInfo.Content.ToStringSafe() );
        }

        /// <summary>
        /// Get the UTM Source Name.
        /// </summary>
        /// <param name="valueId"></param>
        /// <param name="text"></param>
        /// <returns>
        /// If <paramref name="valueId"/> references a valid Defined Value, the value key is returned; if not, <paramref name="text"/> is returned.
        /// </returns>
        public static string GetUtmSourceNameFromDefinedValueOrText( int? valueId, string text )
        {
            return GetUtmComponentNameFromDefinedValueOrText( _utmSourceGuid, valueId, text );
        }

        /// <summary>
        /// Get the UTM Medium Name.
        /// </summary>
        /// <param name="valueId"></param>
        /// <param name="text"></param>
        /// <returns>
        /// If <paramref name="valueId"/> references a valid Defined Value, the value key is returned; if not, <paramref name="text"/> is returned.
        /// </returns>
        public static string GetUtmMediumNameFromDefinedValueOrText( int? valueId, string text )
        {
            return GetUtmComponentNameFromDefinedValueOrText( _utmMediumGuid, valueId, text );
        }

        /// <summary>
        /// Get the UTM Campaign Name.
        /// </summary>
        /// <param name="valueId"></param>
        /// <param name="text"></param>
        /// <returns>
        /// If <paramref name="valueId"/> references a valid Defined Value, the value key is returned; if not, <paramref name="text"/> is returned.
        /// </returns>
        public static string GetUtmCampaignNameFromDefinedValueOrText( int? valueId, string text )
        {
            return GetUtmComponentNameFromDefinedValueOrText( _utmCampaignGuid, valueId, text );
        }

        /// <summary>
        /// Resolve a Utm Field value to a DefinedValue if a matching value is found, or a text string if it is not.
        /// </summary>
        /// <param name="inputValue"></param>
        /// <param name="definedTypeGuid"></param>
        /// <param name="utmValueId"></param>
        /// <param name="utmValueText"></param>
        public static void GetUtmDefinedValueOrTextFromInputValue( string inputValue, string definedTypeGuid, out int? utmValueId, out string utmValueText )
        {
            utmValueId = null;
            utmValueText = string.Empty;

            // If the value is specified more than once, use only the last entry.
            if ( inputValue.IsNullOrWhiteSpace() )
            {
                return;
            }

            inputValue = System.Uri.UnescapeDataString( inputValue.Trim() );

            var definedValue = DefinedTypeCache.Get( definedTypeGuid.AsGuid() )
                .GetDefinedValueFromValue( inputValue );

            if ( definedValue == null )
            {
                utmValueText = inputValue;
            }
            else
            {
                utmValueId = definedValue?.Id;
                utmValueText = definedValue?.Value ?? string.Empty;
            }
        }

        private static string GetUtmComponentNameFromDefinedValueOrText( Guid definedTypeGuid, int? definedValueId, string textValue )
        {
            if ( definedValueId != null )
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
