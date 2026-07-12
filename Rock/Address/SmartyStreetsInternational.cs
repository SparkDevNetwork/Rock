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
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

using Newtonsoft.Json;
using RestSharp;
using Rock.Attribute;
using Rock.Web.Cache;

namespace Rock.Address
{
    /// <summary>
    /// Address verification and (optional) geocoding service backed by Smarty's
    /// International Street Address API. Handles non-US addresses; US addresses
    /// are intentionally skipped so the existing <see cref="SmartyStreets"/>
    /// component can process them.
    /// </summary>
    [Description( "Address verification service from SmartyStreets International Address Verification" )]
    [Export( typeof( VerificationComponent ) )]
    [ExportMetadata( "ComponentName", "Smarty Streets International" )]

    [TextField(
        "Auth ID",
        Description = "The Smarty Streets Authorization ID for your International Address Verification subscription.",
        IsRequired = false,
        DefaultValue = "",
        Category = "",
        Order = 1,
        Key = AttributeKey.AuthID )]
    [TextField(
        "Auth Token",
        Description = "The Smarty Streets Authorization Token for your International Address Verification subscription.",
        IsRequired = false,
        DefaultValue = "",
        Category = "",
        Order = 2,
        Key = AttributeKey.AuthToken )]
    [BooleanField(
        "Enable International Geocoding",
        Description = "When enabled, Smarty Streets populates Location.GeoPoint from the response when possible. Requires an additional International Geocoding subscription/add-on from SmartyStreets on top of the base International Address Verification subscription. When disabled, the GeoPoint is not populated.",
        DefaultBooleanValue = false,
        Category = "",
        Order = 3,
        Key = AttributeKey.EnableInternationalGeocoding )]
    [CustomCheckboxListField(
        "Acceptable Verification Statuses",
        Description = "The confidence levels at which Smarty's verification result will be accepted as standardized. Verified = strong match. Partial = most parts of the address matched. Ambiguous = the input matched several possible addresses.",
        ListSource = "Verified, Partial, Ambiguous",
        IsRequired = false,
        DefaultValue = "Verified,Partial",
        Order = 4,
        Key = AttributeKey.AcceptableVerificationStatuses )]
    // NOTE: Lower precisions ( Locality / AdministrativeArea / None ) are intentionally not offered because accepting them would overwrite Street1 with Smarty's guess for a street it could not actually verify.
    [CustomCheckboxListField(
        "Acceptable Address Precisions",
        Description = "How precisely Smarty must have matched the address before Rock will treat it as standardized. DeliveryPoint is matched to a specific mailing point, Premise is matched to a specific building, and Thoroughfare is matched to a specific street.",
        ListSource = "DeliveryPoint, Premise, Thoroughfare",
        IsRequired = false,
        DefaultValue = "DeliveryPoint,Premise,Thoroughfare",
        Order = 5,
        Key = AttributeKey.AcceptableAddressPrecisions )]
    [CustomCheckboxListField(
        "Acceptable Geocode Precisions",
        Description = "How precisely Smarty must have geocoded the address before Rock will use the returned coordinates. Lower precisions ( Locality, PostalCode, AdministrativeArea ) give approximate coordinates centered on the city, postal area, or state, which can still be useful for proximity searches. Only applies when International Geocoding is enabled.",
        ListSource = "DeliveryPoint, Premise, Thoroughfare, Locality, PostalCode, AdministrativeArea",
        IsRequired = false,
        DefaultValue = "DeliveryPoint,Premise,Thoroughfare",
        Order = 6,
        Key = AttributeKey.AcceptableGeocodePrecisions )]

    [Rock.SystemGuid.EntityTypeGuid( "2F47652E-9C13-4407-9094-A14FADE5C51F" )]
    public class SmartyStreetsInternational : VerificationComponent
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string AuthID = "AuthID";
            public const string AuthToken = "AuthToken";
            public const string EnableInternationalGeocoding = "EnableInternationalGeocoding";
            public const string AcceptableVerificationStatuses = "AcceptableVerificationStatuses";
            public const string AcceptableAddressPrecisions = "AcceptableAddressPrecisions";
            public const string AcceptableGeocodePrecisions = "AcceptableGeocodePrecisions";
        }

        #endregion

        private const string ApiEndpoint = "https://international-street.api.smarty.com/verify";

        /*
            2026-06-04 - NA

            Tokens that, if present in an address_format line template, mark the line
            as NOT a street line. Locality, state/province, postal, building, and
            country tokens flow to Location.City / .State / .PostalCode / .Country
            via the components block — never copied from a root address{N} field.

            Classification is "denylist of non-street tokens" rather than "allowlist of
            street tokens" because Smarty composes tokens with separators like hyphens
            (e.g., "sub_building_number-premise thoroughfare" for Canadian addresses),
            and new street-level token names (premise_number, sub_building_number, etc.)
            ship without warning. An allowlist breaks on either layout; a denylist of
            the well-known non-street tokens is robust to both.

            Reason: Without this classification, Street2 would end up holding a value
            like "Edmonton AB T6K 2R1" that already lives in City / State / PostalCode.
        */
        private static readonly HashSet<string> NonStreetLineTokens = new HashSet<string>( StringComparer.OrdinalIgnoreCase )
        {
            "building",
            "locality",
            "dependent_locality",
            "double_dependent_locality",
            "administrative_area",
            "sub_administrative_area",
            "postal_code",
            "postal_code_short",
            "postal_code_extra",
            "country",
            "country_iso_alpha_2",
            "country_iso_alpha_3"
        };

        /// <summary>
        /// Gets a value indicating whether the configuration supports geocoding.
        /// </summary>
        public override bool SupportsGeocoding
        {
            get { return GetAttributeValue( AttributeKey.EnableInternationalGeocoding ).AsBoolean(); }
        }

        /// <summary>
        /// Standardizes (and optionally geocodes) a non-US address using Smarty's
        /// International Street Address API. US addresses are skipped so the
        /// existing <see cref="SmartyStreets"/> component handles them.
        /// </summary>
        public override VerificationResult Verify( Rock.Model.Location location, out string resultMsg )
        {
            resultMsg = string.Empty;

            if ( location == null )
            {
                resultMsg = "No location provided.";
                return VerificationResult.None;
            }

            // Determine whether this address should be handled by the US Street API or
            // the International Street API. US addresses (empty country, "US", or "USA")
            // are skipped here; the existing SmartyStreets US component handles them.
            var country = location.Country?.Trim();
            var isUsAddress = string.IsNullOrWhiteSpace( country )
                || country.Equals( "US", StringComparison.OrdinalIgnoreCase )
                || country.Equals( "USA", StringComparison.OrdinalIgnoreCase );

            if ( isUsAddress )
            {
                resultMsg = "Skipped: US address handled by US Smarty Streets service.";
                return VerificationResult.None;
            }

            var authId = GetAttributeValue( AttributeKey.AuthID );
            var authToken = GetAttributeValue( AttributeKey.AuthToken );

            if ( string.IsNullOrWhiteSpace( authId ) || string.IsNullOrWhiteSpace( authToken ) )
            {
                Rock.Model.ExceptionLogService.LogException( $"{GetType().Name}: Auth ID / Auth Token are not configured. Configure them under Settings > System > Location Services, or deactivate this component." );
                resultMsg = "Not Configured";
                return VerificationResult.ConnectionError;
            }

            var enableGeocoding = GetAttributeValue( AttributeKey.EnableInternationalGeocoding ).AsBoolean();
            var acceptableVerificationStatuses = GetAttributeValue( AttributeKey.AcceptableVerificationStatuses ).SplitDelimitedValues();
            var acceptableAddressPrecisions = GetAttributeValue( AttributeKey.AcceptableAddressPrecisions ).SplitDelimitedValues();
            var acceptableGeocodePrecisions = GetAttributeValue( AttributeKey.AcceptableGeocodePrecisions ).SplitDelimitedValues();

            var client = new RestClient( ApiEndpoint );
            var request = new RestRequest( Method.GET );
            request.AddQueryParameter( "auth-id", authId );
            request.AddQueryParameter( "auth-token", authToken );
            request.AddQueryParameter( "address1", location.Street1 ?? string.Empty );
            request.AddQueryParameter( "country", country );

            if ( !string.IsNullOrWhiteSpace( location.Street2 ) )
            {
                request.AddQueryParameter( "address2", location.Street2 );
            }

            if ( !string.IsNullOrWhiteSpace( location.City ) )
            {
                request.AddQueryParameter( "locality", location.City );
            }

            if ( !string.IsNullOrWhiteSpace( location.State ) )
            {
                request.AddQueryParameter( "administrative_area", location.State );
            }

            if ( !string.IsNullOrWhiteSpace( location.PostalCode ) )
            {
                request.AddQueryParameter( "postal_code", location.PostalCode );
            }

            // Only send geocode=true when the admin has explicitly opted in via the
            // attribute (which implies the partner's Smarty plan has the International
            // Geocoding add-on). Sending it without the subscription returns a 402 (PaymentRequired).
            if ( enableGeocoding )
            {
                request.AddQueryParameter( "geocode", "true" );
            }

            request.AddHeader( "Accept", "application/json" );

            var response = client.Execute( request );

            /*
                2026-06-04 - NA

                The resultMsg lands in Location.StandardizeAttemptedResult /
                Location.GeocodeAttemptedResult, both nvarchar(200), and in
                ServiceLog.Result. It is meant to be a short status that an admin
                can glance at on a Location record, not a place for full diagnostic
                text. Full detail (subscription tier guidance, raw response body,
                etc.) goes to Rock's Exception Log via ExceptionLogService.LogException.

                Reason: Long resultMsg strings would otherwise exceed the 200-char
                column and fail entity validation on SaveChanges.
            */

            if ( response.StatusCode == HttpStatusCode.PaymentRequired )
            {
                Rock.Model.ExceptionLogService.LogException( $"{GetType().Name}: Smarty returned 402 Payment Required. The configured Smarty plan does not include the International Geocoding add-on subscription. Either disable 'Enable International Geocoding' on this component, or upgrade the Smarty subscription." );
                resultMsg = response.StatusDescription;
                return VerificationResult.ConnectionError;
            }

            if ( response.StatusCode == HttpStatusCode.Unauthorized )
            {
                Rock.Model.ExceptionLogService.LogException( $"{GetType().Name}: Smarty returned 401 Unauthorized. The Auth ID or Auth Token configured on this component is not accepted by Smarty." );
                resultMsg = response.StatusDescription;
                return VerificationResult.ConnectionError;
            }

            if ( response.StatusCode != HttpStatusCode.OK )
            {
                Rock.Model.ExceptionLogService.LogException( $"{GetType().Name}: Smarty returned HTTP {(int)response.StatusCode} {response.StatusDescription}. Response body: {response.Content?.Left( 1000 )}" );
                resultMsg = response.StatusDescription;
                return VerificationResult.ConnectionError;
            }

            var candidates = JsonConvert.DeserializeObject<List<InternationalCandidate>>( response.Content );
            if ( candidates == null || !candidates.Any() )
            {
                resultMsg = "No Match";
                return VerificationResult.None;
            }

            var candidate = candidates.First();
            var verificationStatus = candidate.analysis?.verification_status ?? string.Empty;
            var addressPrecision = candidate.analysis?.address_precision ?? string.Empty;
            var geocodePrecision = candidate.metadata?.geocode_precision ?? string.Empty;

            resultMsg = $"VerificationStatus:{verificationStatus}; AddressPrecision:{addressPrecision}; GeocodePrecision:{geocodePrecision}";

            location.StandardizeAttemptedResult = verificationStatus;
            location.GeocodeAttemptedResult = geocodePrecision;

            var result = VerificationResult.None;

            // Standardization requires BOTH the verification status and the address
            // precision returned by Smarty to be on the configured "Acceptable" lists.
            // Matches the gating pattern used by the existing US SmartyStreets service
            // for Acceptable DPV Codes, so the admin UI checkboxes do what they look
            // like they do. An admin who clears either list intentionally blocks
            // standardization, which is identical to the US service's behavior.
            if ( !string.IsNullOrWhiteSpace( verificationStatus )
                && acceptableVerificationStatuses.Contains( verificationStatus, StringComparer.OrdinalIgnoreCase )
                && !string.IsNullOrWhiteSpace( addressPrecision )
                && acceptableAddressPrecisions.Contains( addressPrecision, StringComparer.OrdinalIgnoreCase ) )
            {
                ApplyStandardization( location, candidate );
                result |= VerificationResult.Standardized;
            }

            if ( enableGeocoding
                && !string.IsNullOrWhiteSpace( geocodePrecision )
                && acceptableGeocodePrecisions.Contains( geocodePrecision, StringComparer.OrdinalIgnoreCase )
                && candidate.metadata != null )
            {
                if ( location.SetLocationPointFromLatLong( candidate.metadata.latitude, candidate.metadata.longitude ) )
                {
                    result |= VerificationResult.Geocoded;
                }
            }

            return result;
        }

        /// <summary>
        /// Applies the standardized address fields from the API response to the
        /// supplied <see cref="Rock.Model.Location"/>. Only called when the
        /// candidate's address_precision is acceptable.
        /// </summary>
        private static void ApplyStandardization( Rock.Model.Location location, InternationalCandidate candidate )
        {
            var components = candidate.components;
            var metadata = candidate.metadata;

            // City / State / PostalCode always come from components (never from a root
            // address field) because the root address fields are pre-assembled mailing
            // lines that mix locality, state, and postal code together.
            if ( !string.IsNullOrWhiteSpace( components?.locality ) )
            {
                location.City = components.locality;
            }

            if ( !string.IsNullOrWhiteSpace( components?.administrative_area ) )
            {
                location.State = components.administrative_area;
            }

            if ( !string.IsNullOrWhiteSpace( components?.postal_code ) )
            {
                location.PostalCode = components.postal_code;
            }

            ApplyCountry( location, components?.country_iso_3 );
            ApplyStreetLines( location, candidate, metadata?.address_format );
        }

        /// <summary>
        /// Resolves the International API's alpha-3 country code to Rock's alpha-2
        /// value via the new well-known <see cref="Rock.SystemKey.CountryAttributeKey.ISO3166Alpha3"/>
        /// attribute on the Countries <see cref="Rock.Model.DefinedType"/>. Falls back
        /// to the raw alpha-3 value if no matching DefinedValue is seeded.
        /// </summary>
        private static void ApplyCountry( Rock.Model.Location location, string iso3 )
        {
            if ( string.IsNullOrWhiteSpace( iso3 ) )
            {
                return;
            }

            var countriesDefinedType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.LOCATION_COUNTRIES );
            var countryValue = countriesDefinedType?.DefinedValues
                .FirstOrDefault( dv => string.Equals(
                    dv.GetAttributeValue( Rock.SystemKey.CountryAttributeKey.ISO3166Alpha3 ),
                    iso3,
                    StringComparison.OrdinalIgnoreCase ) );

            location.Country = countryValue?.Value ?? iso3;
        }

        /// <summary>
        /// Selects the "street line" entries from metadata.address_format and writes
        /// them to Street1 / Street2 from the matching root addressN fields. See
        /// "Street Line Selection Algorithm" in the spec.
        /// </summary>
        private static void ApplyStreetLines( Rock.Model.Location location, InternationalCandidate candidate, string addressFormat )
        {
            if ( string.IsNullOrWhiteSpace( addressFormat ) )
            {
                return;
            }

            var lineTemplates = addressFormat.Split( '|' );
            var streetLineValues = new List<string>();

            for ( int i = 0; i < lineTemplates.Length; i++ )
            {
                if ( !IsStreetLine( lineTemplates[i] ) )
                {
                    continue;
                }

                // address_format index N (0-based) maps to root address{N+1} (1-based).
                var lineValue = GetRootAddressLine( candidate, i + 1 );
                if ( !string.IsNullOrWhiteSpace( lineValue ) )
                {
                    streetLineValues.Add( lineValue );
                }
            }

            if ( streetLineValues.Count >= 1 )
            {
                location.Street1 = streetLineValues[0];
            }

            if ( streetLineValues.Count >= 2 )
            {
                location.Street2 = streetLineValues[1];
            }
        }

        /// <summary>
        /// Returns true when an address_format line template contains zero locality-,
        /// state-, postal-, building-, or country-level tokens. Tokens are extracted
        /// by splitting on anything that is not a letter or underscore so combined
        /// forms like "sub_building_number-premise thoroughfare" still produce clean
        /// tokens ( "sub_building_number", "premise", "thoroughfare" ) for matching.
        /// </summary>
        private static bool IsStreetLine( string lineTemplate )
        {
            if ( string.IsNullOrWhiteSpace( lineTemplate ) )
            {
                return false;
            }

            var tokens = Regex.Split( lineTemplate, @"[^a-zA-Z_]+" )
                .Where( t => !string.IsNullOrEmpty( t ) )
                .ToList();

            if ( tokens.Count == 0 )
            {
                return false;
            }

            return !tokens.Any( t => NonStreetLineTokens.Contains( t ) );
        }

        /// <summary>
        /// Returns the value of the root <c>addressN</c> field for the given 1-based
        /// index. Smarty's International response surfaces up to 12 mailing-format
        /// lines as <c>address1</c> through <c>address12</c>.
        /// </summary>
        private static string GetRootAddressLine( InternationalCandidate candidate, int oneBasedIndex )
        {
            switch ( oneBasedIndex )
            {
                case 1: return candidate.address1;
                case 2: return candidate.address2;
                case 3: return candidate.address3;
                case 4: return candidate.address4;
                case 5: return candidate.address5;
                case 6: return candidate.address6;
                case 7: return candidate.address7;
                case 8: return candidate.address8;
                case 9: return candidate.address9;
                case 10: return candidate.address10;
                case 11: return candidate.address11;
                case 12: return candidate.address12;
                default: return null;
            }
        }

#pragma warning disable

        /// <summary>
        /// Top-level candidate object returned by the International Street API.
        /// Field names match Smarty's response payload verbatim (snake_case).
        /// </summary>
        public class InternationalCandidate
        {
            public string organization { get; set; }
            public string address1 { get; set; }
            public string address2 { get; set; }
            public string address3 { get; set; }
            public string address4 { get; set; }
            public string address5 { get; set; }
            public string address6 { get; set; }
            public string address7 { get; set; }
            public string address8 { get; set; }
            public string address9 { get; set; }
            public string address10 { get; set; }
            public string address11 { get; set; }
            public string address12 { get; set; }
            public InternationalComponents components { get; set; }
            public InternationalMetadata metadata { get; set; }
            public InternationalAnalysis analysis { get; set; }
        }

        public class InternationalComponents
        {
            public string country_iso_3 { get; set; }
            public string locality { get; set; }
            public string administrative_area { get; set; }
            public string administrative_area_iso2 { get; set; }
            public string postal_code { get; set; }
            public string postal_code_short { get; set; }
            public string postal_code_extra { get; set; }
            public string premise { get; set; }
            public string premise_extra { get; set; }
            public string thoroughfare { get; set; }
            public string dependent_thoroughfare { get; set; }
            public string building { get; set; }
            public string sub_building { get; set; }
            public string post_box { get; set; }
        }

        public class InternationalMetadata
        {
            public double latitude { get; set; }
            public double longitude { get; set; }
            public string geocode_precision { get; set; }
            public string max_geocode_precision { get; set; }
            public string geocode_classification { get; set; }
            public string address_format { get; set; }
        }

        public class InternationalAnalysis
        {
            public string verification_status { get; set; }
            public string address_precision { get; set; }
            public string max_address_precision { get; set; }
        }

#pragma warning restore

    }
}
