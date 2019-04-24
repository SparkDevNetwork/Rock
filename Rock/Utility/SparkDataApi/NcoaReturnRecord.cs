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

using Newtonsoft.Json;

using Rock.Model;

namespace Rock.Utility.SparkDataApi
{
    /// <summary>
    /// The record that NCOA server returns.
    /// </summary>
    public class NcoaReturnRecord
    {
        /// <summary>
        /// Gets or sets the NCOA run date time.
        /// </summary>
        /// <value>
        /// The NCOA run date time.
        /// </value>
        [JsonIgnore]
        public DateTime NcoaRunDateTime { get; set; }

        /// <summary>
        /// Gets or sets the input individual identifier. '{PersonId}_{PersonAliasId}_{FamilyId}'
        /// </summary>
        /// <value>
        /// The input individual identifier.
        /// </value>
        [JsonProperty( "individual_id" )]
        public string InputIndividualId { get; set; }

        /// <summary>
        /// Gets or sets the input first name of the individual.
        /// </summary>
        /// <value>
        /// The input first name of the individual.
        /// </value>
        [JsonProperty( "individual_first_name" )]
        public string InputIndividualFirstName { get; set; }

        /// <summary>
        /// Gets or sets the input last name of the individual.
        /// </summary>
        /// <value>
        /// The input last name of the individual.
        /// </value>
        [JsonProperty( "individual_last_name" )]
        public string InputIndividualLastName { get; set; }

        /// <summary>
        /// Gets or sets the input address line1.
        /// </summary>
        /// <value>
        /// The input address line1.
        /// </value>
        [JsonProperty( "address_line_1" )]
        public string InputAddressLine1 { get; set; }

        /// <summary>
        /// Gets or sets the input address line2.
        /// </summary>
        /// <value>
        /// The input address line2.
        /// </value>
        [JsonProperty( "address_line_2" )]
        public string InputAddressLine2 { get; set; }

        /// <summary>
        /// Gets or sets the input address city.
        /// </summary>
        /// <value>
        /// The input address city.
        /// </value>
        [JsonProperty( "address_city_name" )]
        public string InputAddressCity { get; set; }

        /// <summary>
        /// Gets or sets the input address state code.
        /// </summary>
        /// <value>
        /// The input address state code.
        /// </value>
        [JsonProperty( "address_state_code" )]
        public string InputAddressStateCode { get; set; }

        /// <summary>
        /// Gets or sets the input address postal code.
        /// </summary>
        /// <value>
        /// The input address postal code.
        /// </value>
        [JsonProperty( "address_postal_code" )]
        public string InputAddressPostalCode { get; set; }

        /// <summary>
        /// Gets or sets the input address country code.
        /// </summary>
        /// <value>
        /// The input address country code.
        /// </value>
        [JsonProperty( "address_country_code" )]
        public string InputAddressCountryCode { get; set; }

        /// <summary>
        /// Gets or sets the household position.
        /// </summary>
        /// <value>
        /// The household position.
        /// </value>
        [JsonProperty( "Household Position" )]
        public int? HouseholdPosition { get; set; }

        /// <summary>
        /// Gets or sets the name identifier.
        /// </summary>
        /// <value>
        /// The name identifier.
        /// </value>
        [JsonProperty( "Name ID" )]
        public int? NameId { get; set; }

        /// <summary>
        /// Gets or sets the street suffix.
        /// </summary>
        /// <value>
        /// The street suffix.
        /// </value>
        [JsonProperty( "Street Suffix" )]
        public string StreetSuffix { get; set; }

        /// <summary>
        /// Gets or sets the type of the unit (Apartment, Unit, etc.)
        /// </summary>
        /// <value>
        /// The type of the unit.
        /// </value>
        [JsonProperty( "Unit Type" )]
        public string UnitType { get; set; }

        /// <summary>
        /// Gets or sets the unit number.
        /// </summary>
        /// <value>
        /// The unit number.
        /// </value>
        [JsonProperty( "Unit Number" )]
        public string UnitNumber { get; set; }

        /// <summary>
        /// Gets or sets the box number.
        /// </summary>
        /// <value>
        /// The box number.
        /// </value>
        [JsonProperty( "Box Number" )]
        public string BoxNumber { get; set; }

        /// <summary>
        /// Gets or sets the name of the city.
        /// </summary>
        /// <value>
        /// The name of the city.
        /// </value>
        [JsonProperty( "City Name" )]
        public string CityName { get; set; }

        /// <summary>
        /// Gets or sets the state code. 2-digit State Abbreviation (IL, NY, CA, etc) for the given address
        /// </summary>
        /// <value>
        /// The state code.
        /// </value>
        [JsonProperty( "State Code" )]
        public string StateCode { get; set; }

        /// <summary>
        /// Gets or sets the 5-digit zip postal code.
        /// </summary>
        /// <value>
        /// The postal code.
        /// </value>
        [JsonProperty( "Postal Code" )]
        public string PostalCode { get; set; }

        /// <summary>
        /// Gets or sets the +4 postal code extension.
        /// </summary>
        /// <value>
        /// The postal code extension.
        /// </value>
        [JsonProperty( "Postal Code Extension" )]
        public string PostalCodeExtension { get; set; }

        /// <summary>
        /// Gets or sets the carrier route. 4-digit Carrier Route Number indicates a group of mailing addresses that receive the same code to aid in mail delivery efficiency.
        /// </summary>
        /// <value>
        /// The carrier route.
        /// </value>
        [JsonProperty( "Carrier Route" )]
        public string CarrierRoute { get; set; }

        /// <summary>
        /// Gets or sets the address status. (V)alid, (M)ulti-Matched, or (I)nvalid
        /// </summary>
        /// <value>
        /// The address status.
        /// </value>
        [JsonProperty( "Address Status" )]
        public string AddressStatus { get; set; }

        /// <summary>
        /// Gets or sets the individual record identifier.
        /// </summary>
        /// <value>
        /// The individual record identifier.
        /// </value>
        [JsonProperty( "Individual Record ID" )]
        public int? IndividualRecordId { get; set; }

        /// <summary>
        /// Gets or sets the Delivery Point Verification (DPV) error number.
        /// <list type="table">
        /// <listheader>
        /// <term>Return code</term>
        /// <term>Caption</term>
        /// <term>Description</term>
        /// </listheader>
        /// <item>
        /// <term>1</term>
        /// <term>State Not Found</term>
        /// <term>1.1 State not found</term>
        /// </item>
        /// <item>
        /// <term>2</term>
        /// <term>City Not Found</term>
        /// <term>2.1 City not found</term>
        /// </item>
        /// <item>
        /// <term>3</term>
        /// <term>Street Not Found</term>
        /// <term>3.1 Street not found</term>
        /// </item>
        /// <item>
        /// <term>4</term>
        /// <term>Address Not Found</term>
        /// <term>4.1 Address not found</term>
        /// </item>
        /// <item>
        /// <term>5</term>
        /// <term>Can’t Assign +4</term>
        /// <term>5.1 Incomputable +4 range  5.2 +4 unavailable
        /// </term>
        /// </item>
        /// <item>
        /// <term>6</term>
        /// <term>Multiple Matches</term>
        /// <term>6.1 Multiple streets match  6.2 Multiple addresses match  6.3 Cardinal Rule multiple match
        ///</term>
        /// </item>
        /// <item>
        /// <term>7</term>
        /// <term>Time/Space Error</term>
        /// <term>7.1 Time ran out  7.2 Output too long</term>
        /// </item>
        /// <item>
        /// <term>8</term>
        /// <term>Company Warning</term>
        /// <term>8.1 Company phonetic match used  8.2 First company match used</term>
        /// </item>
        /// <item>
        /// <term>9</term>
        /// <term>State Corrected</term>
        /// <term>9.1 State determined from city  9.2 State determined from ZIP</term>
        /// </item>
        /// <item>
        /// <term>10</term>
        /// <term>City Corrected</term>
        /// <term>10.1 City phonetic match used  10.2 City determined from ZIP  10.3 Acceptable city name used</term>
        /// </item>
        /// <item>
        /// <term>11</term>
        /// <term>Street Corrected</term>
        /// <term>11.0 Address component Chg/Del/Add</term>
        /// </item>
        /// </list>
        /// </summary>
        /// <value>
        /// The error number.
        /// </value>
        [JsonProperty( "Error Number" )]
        public string ErrorNumber { get; set; }

        /// <summary>
        /// Gets or sets the type of the address. F - ZIP+4 Match on the Company Name  G - General Delivery Record  H - High Rise Record  P - PO Box Record  R - Rural Route Record  S - Street Record
        /// </summary>
        /// <value>
        /// The type of the address.
        /// </value>
        [JsonProperty( "Address Type" )]
        public string AddressType { get; set; }

        /// <summary>
        /// Gets or sets the delivery point. A 2-digit code that identifies the individual mail delivery location for this address
        /// </summary>
        /// <value>
        /// The delivery point.
        /// </value>
        [JsonProperty( "Delivery Point" )]
        public string DeliveryPoint { get; set; }

        /// <summary>
        /// Gets or sets the check digit. Check Digit for each deliverable record in the database.
        /// </summary>
        /// <value>
        /// The check digit.
        /// </value>
        [JsonProperty( "Check Digit" )]
        public string CheckDigit { get; set; }

        /// <summary>
        /// Gets or sets the delivery point verification. Refer to Delivery Point Verification Codes Table in the NCOA Data Dictionary document
        /// </summary>
        /// <value>
        /// The delivery point verification.
        /// </value>
        [JsonProperty( "Delivery Point Verification" )]
        public string DeliveryPointVerification { get; set; }

        /// <summary>
        /// Gets or sets the delivery point verification notes that identify what took place during DPV.
        /// </summary>
        /// <value>
        /// The delivery point verification notes.
        /// </value>
        [JsonProperty( "Delivery Point Verification Notes" )]
        public string DeliveryPointVerificationNotes { get; set; }

        /// <summary>
        /// Gets or sets the vacant.Y – Vacant  N - Occupied
        /// </summary>
        /// <value>
        /// The vacant.
        /// </value>
        [JsonProperty( "Vacant" )]
        public string Vacant { get; set; }

        /// <summary>
        /// Gets or sets the 2-digit congressional district code.
        /// </summary>
        /// <value>
        /// The congressional district code.
        /// </value>
        [JsonProperty( "Congressional District Code" )]
        public string CongressionalDistrictCode { get; set; }

        /// <summary>
        /// Gets or sets the area code.
        /// </summary>
        /// <value>
        /// The area code.
        /// </value>
        [JsonProperty( "Area Code" )]
        public string AreaCode { get; set; }

        /// <summary>
        /// Gets or sets the latitude.
        /// </summary>
        /// <value>
        /// The latitude.
        /// </value>
        [JsonProperty( "Latitude" )]
        public string Latitude { get; set; }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        /// <value>
        /// The first name.
        /// </value>
        [JsonProperty( "First Name" )]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the longitude.
        /// </summary>
        /// <value>
        /// The longitude.
        /// </value>
        [JsonProperty( "Longitude" )]
        public string Longitude { get; set; }

        /// <summary>
        /// Gets or sets the time zone.
        /// </summary>
        /// <value>
        /// The time zone.
        /// </value>
        [JsonProperty( "Time Zone" )]
        public string TimeZone { get; set; }

        /// <summary>
        /// Gets or sets the name of the county.
        /// </summary>
        /// <value>
        /// The name of the county.
        /// </value>
        [JsonProperty( "County Name" )]
        public string CountyName { get; set; }

        /// <summary>
        /// Gets or sets the 3-digit county federal information processing standards (FIPS) code.
        /// </summary>
        /// <value>
        /// The county FIPS.
        /// </value>
        [JsonProperty( "County FIPS" )]
        public string CountyFIPS { get; set; }

        /// <summary>
        /// Gets or sets the 2-digit state federal information processing standards (FIPS) code.
        /// </summary>
        /// <value>
        /// The state FIPS.
        /// </value>
        [JsonProperty( "State FIPS" )]
        public string StateFIPS { get; set; }

        /// <summary>
        /// Gets or sets the barcode. Delivery point barcode gives a 12-digit number that can be used in the creation of a barcode used for mailings.
        /// </summary>
        /// <value>
        /// The barcode.
        /// </value>
        [JsonProperty( "Barcode" )]
        public string Barcode { get; set; }

        /// <summary>
        /// Gets or sets the Locatable Address Conversion System (LACS); the change of rural-style addresses to city-style addresses.
        /// </summary>
        /// <value>
        /// The Locatable Address Conversion System (LACS).
        /// </value>
        [JsonProperty( "Locatable Address Conversion System" )]
        public object LACS { get; set; }

        /// <summary>
        /// Gets or sets the Line of Travel. The Line of Travel number indicates the first occurrence of delivery made to the add-on range within the carrier route.
        /// </summary>
        /// <value>
        /// The line of travel.
        /// </value>
        [JsonProperty( "Line of Travel" )]
        public string LineOfTravel { get; set; }

        /// <summary>
        /// Gets or sets the ascending descending. LOT order indicator gives the approximate delivery order within the sequence number.
        /// </summary>
        /// <value>
        /// The ascending descending.
        /// </value>
        [JsonProperty( "Ascending/Descending" )]
        public string AscendingDescending { get; set; }

        /// <summary>
        /// Gets or sets the move applied. Contains the date (YYYYMMDD) the record was processed through NCOA.
        /// </summary>
        /// <value>
        /// The move applied.
        /// </value>
        [JsonProperty( "Move Applied" )]
        public string MoveApplied { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        /// <value>
        /// The last name.
        /// </value>
        [JsonProperty( "Last Name" )]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the type of the move. I - Individual Match  F - Family Match  B - Business Name Match
        /// </summary>
        /// <value>
        /// The type of the move.
        /// </value>
        [JsonProperty( "Move Type" )]
        public string MoveType { get; set; }

        /// <summary>
        /// Gets or sets the move date. The date the move was filed with the USPS, in YYYYMM format.
        /// </summary>
        /// <value>
        /// The move date.
        /// </value>
        [JsonProperty( "Move Date" )]
        public string MoveDate { get; set; }

        /// <summary>
        /// Gets or sets the move distance. This is measured in miles; with up to 9,999 miles and two decimal points included from centroid zip.
        /// </summary>
        /// <value>
        /// The move distance.
        /// </value>
        [JsonProperty( "Move Distance" )]
        public double? MoveDistance { get; set; }

        /// <summary>
        /// Gets or sets the match flag. M – Moved  G - PO Box Closed K - Moved, left no forwarding address  F - Moved to a Foreign Country
        /// </summary>
        /// <value>
        /// The match flag.
        /// </value>
        [JsonProperty( "Match Flag" )]
        public string MatchFlag { get; set; }

        /// <summary>
        /// Gets or sets the NXI. Status code returned during NCOA processing; This code identifies if a new address was provided and gives description of why or why not.
        /// <list type="table">
        /// <listheader>
        /// <term>Return code</term>
        /// <term>Caption</term>
        /// <term>Description</term>
        /// </listheader>
        /// <item>
        /// <term>A</term>
        /// <term>Full Address Match</term>
        /// <term>Matched record and a new address has been provided (NCOAlink 18Month Service code only).</term>
        /// </item>
        /// <item>
        /// <term>00</term>
        /// <term>No Matching Address - No new address provided</term>
        /// <term>No Match (NCOAlink 18-month Service code only)</term>
        /// </item>
        /// <item>
        /// <term>01</term>
        /// <term>New address outside US - no new address provided</term>
        /// <term>Found COA: Foreign Move - The input record matched to a business, individual or family type master file record but the new address was outside USPS delivery area.</term>
        /// </item>
        /// <item>
        /// <term>02</term>
        /// <term>No forwarding address</term>
        /// <term>Found COA: Moved Left No Address - The input record matched to a business, individual or family type master file record and the new address was not provided to USPS.</term>
        /// </item>
        /// <item>
        /// <term>03</term>
        /// <term>Closed PO box - No new address provided</term>
        /// <term>Found COA: Box Closed No - The Input record matched to a business, individual or family type master file record which contains an old address of PO BOX that has been closed without a forwarding address provided.</term>
        /// </item>
        /// <item>
        /// <term>04</term>
        /// <term>Address 2 Missing for family move - No new address provided</term>
        /// <term>Cannot match COA: Street Address with Secondary - The input record matched to a family record type on master file with an old address that contained secondary information. The input record does not contain secondary information. This address match situation requires individual name matching logic to obtain a match and individual names do not match.</term>
        /// </item>
        /// <item>
        /// <term>05</term>
        /// <term>Too many matches - No new address provided</term>
        /// <term>Found COA: New 11-digit DPBC (Delivery Point Barcode) is Ambiguous - The input record matched to a business, individual or family type master file record. The new address on the master file record could not be converted to a deliverable address because the DPBC represents more than one delivery point.</term>
        /// </item>
        /// <item>
        /// <term>06</term>
        /// <term>Partial Match - No new address provided</term>
        /// <term>Cannot Match COA: Conflicting Directions: Middle Name Related -There is more than one COA (individual or family type) record for the match algorithm and the middle names or initials on the COAs are different. Therefore, a single match result could not be determined.</term>
        /// </item>
        /// <item>
        /// <term>07</term>
        /// <term>Partial Match - No new address provided</term>
        /// <term>Cannot Match COA: Conflicting Directions: Gender Related -There is more than one COA (individual or family type) record for the match algorithm and the genders of the names on the COAs are different. Therefore, a single match result could not be determined.</term>
        /// </item>
        /// <item>
        /// <term>08</term>
        /// <term>Too many possible matches - No new address provided</term>
        /// <term>Cannot Match COA: Other Conflicting Instructions - The input record matched to two master file (business, individual or family type) records. The two records in the master file were compared and due to differences in the new addresses, a match could not be made.</term>
        /// </item>
        /// <item>
        /// <term>09</term>
        /// <term>Family move address does not match individual name - No new address provided</term>
        /// <term>Cannot Match COA: High-rise Default - The input record matched to a family record on the master file from a High- rise address ZIP+4 coded to the building default. This address match situation requires individual name matching logic to obtain a match and individual names do not match.</term>
        /// </item>
        /// <item>
        /// <term>10</term>
        /// <term>Family move address does not match individual name</term>
        /// <term>Cannot Match COA: Rural Default - The input record matched to a family record on the master file from a Rural Route or Highway Contract Route address ZIP+4 coded to the route default. This address situation requires individual name matching logic to obtain a match and individual names do not match.</term>
        /// </item>
        /// <item>
        /// <term>11</term>
        /// <term>Only last name was matched - No new address provided</term>
        /// <term>Cannot Match COA: Individual Match: Insufficient COA Name for Match - There is a master file (individual or family type) record with the same surname and address but there is insufficient name information on the master file record to produce a match using individual matching logic.</term>
        /// </item>
        /// <item>
        /// <term>12</term>
        /// <term>Middle name does not match - No new address provided</term>
        /// <term>Cannot Match COA: Middle Name Test Failed - The input record matched to an individual or family record on the master file with the same address and surname. However, a match cannot be made because the input name contains a conflict with the middle name or initials on the master file record.</term>
        /// </item>
        /// <item>
        /// <term>13</term>
        /// <term>Gender does not match - No new address provided</term>
        /// <term>Cannot Match COA: Gender Test Failed - The input record matched to a master file (individual or family type) record.A match cannot be made because the gender of the name on the input record conflicts with the gender of the name on the master file record.</term>
        /// </item>
        /// <item>
        /// <term>14</term>
        /// <term>Undeliverable Address - No new address provided</term>
        /// <term>Found COA: New Address Would Not Convert at Run Time - The input record matched to a master file (business, individual or family type) record. The new address could not be converted to a deliverable address.</term>
        /// </item>
        /// <item>
        /// <term>15</term>
        /// <term>First name is missing - No new address provided</term>
        /// <term>Cannot Match COA: Individual Name Insufficient - There is a master file record with the same address and surname. A match cannot be made because the input record does not contain a first name or contains initials only.</term>
        /// </item>
        /// <item>
        /// <term>16</term>
        /// <term>No matching apt number - No new address provided</term>
        /// <term>Cannot Match COA: Secondary Number Discrepancy - The input record matched to a street level individual or family type record. However, a match is prohibited based on I of the following reasons: 1) There is conflicting secondary information on the input and master file record; 2) the input record contained secondary information and matched to a family record that does not contain secondary information. In item 2, this address match situation requires individual name matching logic to obtain a COA match and individual names do not match.</term>
        /// </item>
        /// <item>
        /// <term>17</term>
        /// <term>First name doesn't match - No new address provided</term>
        /// <term>Cannot Match COA: Other Insufficient Name - The input record matched to an individual or family master file record. The input name is different or not sufficient enough to produce a match.</term>
        /// </item>
        /// <item>
        /// <term>18</term>
        /// <term>Family move with General address - No new address provided</term>
        /// <term>Cannot Match COA: General Delivery - The input record matched to a family record on the master file from a General Delivery address. This address situation requires individual name matching logic to obtain a match and individual names do not match.</term>
        /// </item>
        /// <item>
        /// <term>19</term>
        /// <term>No Zip Code found - No new address provided</term>
        /// <term>Found COA: New Address not ZIP+4 coded - There is a change of address on file but the new address cannot be ZIP+4 coded and therefore there is no 11 -digit DPBC to store or return.</term>
        /// </item>
        /// <item>
        /// <term>20</term>
        /// <term>Cannot determine single address - No new address provided</term>
        /// <term>Cannot Match COA: Conflicting Directions after re-chaining - Multiple master file records were potential matches for the input record. The master file records contained different new addresses and a single match result could not be determined.</term>
        /// </item>
        /// <item>
        /// <term>66</term>
        /// <term>Deleted address with no forwarding address.</term>
        /// <term>Daily Delete - The input record matched to a business, individual or family type master file record with an old address that is present in the daily delete file. The presence of an address in the daily delete file means that a COA with this address is pending deletion from the master file and that no mail may be forwarded from this address.</term>
        /// </item>
        /// <item>
        /// <term>91</term>
        /// <term>Matched; secondary address may be missing</term>
        /// <term>Found COA: Secondary Number dropped from COA. The input record matched to a master file record. The master file record had a secondary number and the input address did not. A new address was provided.</term>
        /// </item>
        /// <item>
        /// <term>92</term>
        /// <term>Matched; secondary address may be incorrect</term>
        /// <term>Found COA: Secondary Number Dropped from input address. The input record matched to a master file record, but the input address had a secondary number and the master file record did not. The record is a ZIP + 4® street level match. A new address was provided.</term>
        /// </item>
        /// </list>
        /// </summary>
        /// <value>
        /// The NXI
        /// </value>
        [JsonProperty( "NXI" )]
        public string NXI { get; set; }

        /// <summary>
        /// Gets or sets the Address Not Known (ANK).
        /// <list type="table">
        /// <listheader>
        /// <term>Return code</term>
        /// <term>Caption</term>
        /// <term>Description</term>
        /// </listheader>
        /// <item>
        /// <term>77</term>
        /// <term>ANK - Address Not Known</term>
        /// <term>The record was not found. You should suppress these records from your database or flag these records for deletion and not mail to them.</term>
        /// </item>
        /// <item>
        /// <term>48</term>
        /// <term>48 Month NCOA</term>
        /// <term>The record was found in the 48 month NCOA. This record moved between 19-48 months ago.</term>
        /// </item>
        /// </list>
        /// </summary>
        /// <value>
        /// The ANK.
        /// </value>
        [JsonProperty( "ANK" )]
        public string ANK { get; set; }

        /// <summary>
        /// Gets or sets the residential delivery indicator. Y – Residential  N - Business
        /// </summary>
        /// <value>
        /// The residential delivery indicator.
        /// </value>
        [JsonProperty( "Residential Delivery Indicator" )]
        public string ResidentialDeliveryIndicator { get; set; }

        /// <summary>
        /// Gets or sets the type of the record. A - Active; no move was applied; may or may not be the current address  C - Current; move was applied, and this is the current record  H - Historical or previous address
        /// </summary>
        /// <value>
        /// The type of the record.
        /// </value>
        [JsonProperty( "Record Type" )]
        public string RecordType { get; set; }

        /// <summary>
        /// Gets or sets the country code. 2-digit country abbreviation.
        /// </summary>
        /// <value>
        /// The country code.
        /// </value>
        [JsonProperty( "Country Code" )]
        public string CountryCode { get; set; }

        /// <summary>
        /// Gets or sets the name of the company.
        /// </summary>
        /// <value>
        /// The name of the company.
        /// </value>
        [JsonProperty( "Company Name" )]
        public object CompanyName { get; set; }

        /// <summary>
        /// Gets or sets the address line 1.
        /// </summary>
        /// <value>
        /// The address line1.
        /// </value>
        [JsonProperty( "Address Line 1" )]
        public string AddressLine1 { get; set; }

        /// <summary>
        /// Gets or sets the address line 2.
        /// </summary>
        /// <value>
        /// The address line2.
        /// </value>
        [JsonProperty( "Address Line 2" )]
        public string AddressLine2 { get; set; }

        /// <summary>
        /// Gets or sets the address identifier.
        /// </summary>
        /// <value>
        /// The address identifier.
        /// </value>
        [JsonProperty( "Address Id" )]
        public int? AddressId { get; set; }

        /// <summary>
        /// Gets or sets the household identifier.
        /// </summary>
        /// <value>
        /// The household identifier.
        /// </value>
        [JsonProperty( "Household Id" )]
        public int? HouseholdId { get; set; }

        /// <summary>
        /// Gets or sets the individual identifier.
        /// </summary>
        /// <value>
        /// The individual identifier.
        /// </value>
        [JsonProperty( "Individual Id" )]
        public int? IndividualId { get; set; }

        /// <summary>
        /// Gets or sets the street number.
        /// </summary>
        /// <value>
        /// The street number.
        /// </value>
        [JsonProperty( "Street Number" )]
        public string StreetNumber { get; set; }

        /// <summary>
        /// Gets or sets the street pre-direction.
        /// </summary>
        /// <value>
        /// The street pre-direction.
        /// </value>
        [JsonProperty( "Street Pre Direction" )]
        public string StreetPreDirection { get; set; }

        /// <summary>
        /// Gets or sets the name of the street.
        /// </summary>
        /// <value>
        /// The name of the street.
        /// </value>
        [JsonProperty( "Street Name" )]
        public string StreetName { get; set; }

        /// <summary>
        /// Gets or sets the street post direction.
        /// </summary>
        /// <value>
        /// The street post direction.
        /// </value>
        [JsonProperty( "Street Post Direction" )]
        public string StreetPostDirection { get; set; }

        /// <summary>
        /// Convert NCOA Date to DateTime
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>DataTime</returns>
        private DateTime? NcoaDateToDateTime( string date )
        {
            if ( date == null )
            {
                return null;
            }

            try
            {
                if ( date.Length == 6 )
                {
                    return new DateTime( date.Substring( 0, 4 ).AsInteger(), date.Substring( 4, 2 ).AsInteger(), 0 );
                }
                else if ( date.Length == 8 )
                {
                    return new DateTime( date.Substring( 0, 4 ).AsInteger(), date.Substring( 4, 2 ).AsInteger(), date.Substring( 6, 2 ).AsInteger() );
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Convert NCOA return record to NcoaHistory.
        /// </summary>
        /// <returns>NcoaHistory</returns>
        public NcoaHistory ToNcoaHistory()
        {
            var ids = InputIndividualId?.Split( '_' );
            var ncoaHistory = new NcoaHistory()
            {
                //PersonId = ( ids?[0] ).AsInteger(),
                PersonAliasId = ( ids?[1] ).AsInteger(),
                FamilyId = ( ids?[2] ).AsInteger(),
                LocationId = ( ids?[3] ).AsInteger(),
                AddressStatus = AddressStatus == "V" ? Model.AddressStatus.Valid : Model.AddressStatus.Invalid,
                MoveDate = NcoaDateToDateTime( MoveDate ),
                MoveDistance = (decimal?)MoveDistance,
                NcoaNote = DeliveryPointVerificationNotes,
                NcoaRunDateTime = NcoaRunDateTime,
                OriginalCity = InputAddressCity,
                OriginalPostalCode = InputAddressPostalCode,
                OriginalState = InputAddressStateCode,
                OriginalStreet1 = InputAddressLine1,
                OriginalStreet2 = InputAddressLine2,
                Processed = Processed.NotProcessed,
                UpdatedBarcode = Barcode,
                UpdatedCity = CityName,
                UpdatedCountry = CountryCode,
                UpdatedPostalCode = PostalCode,
                UpdatedState = StateCode,
                UpdatedStreet1 = AddressLine1,
                UpdatedStreet2 = AddressLine2
            };

            switch ( ResidentialDeliveryIndicator )
            {
                case "Y":
                    ncoaHistory.UpdatedAddressType = UpdatedAddressType.Residential;
                    break;
                case "N":
                    ncoaHistory.UpdatedAddressType = UpdatedAddressType.Business;
                    break;
                default:
                    ncoaHistory.UpdatedAddressType = UpdatedAddressType.None;
                    break;
            };

            if ( ANK == "48" )
            {
                ncoaHistory.NcoaType = NcoaType.Month48Move;
            }
            else if ( MatchFlag.IsNotNullOrWhiteSpace() && RecordType == "C" )
            {
                ncoaHistory.NcoaType = NcoaType.Move;
            }
            else
            {
                ncoaHistory.NcoaType = NcoaType.NoMove;

                if ( AddressStatus == "V" )
                {
                    ncoaHistory.Processed = Processed.Complete;
                }
            }

            if ( AddressStatus != "V" )
            {
                ncoaHistory.AddressInvalidReason = AddressInvalidReason.NotFound;
                ncoaHistory.AddressStatus = Model.AddressStatus.Invalid;
            }
            else if ( Vacant == "Y" )
            {
                ncoaHistory.AddressInvalidReason = AddressInvalidReason.Vacant;
            }
            else
            {
                ncoaHistory.AddressInvalidReason = AddressInvalidReason.None;
            }

            switch ( MatchFlag )
            {
                case "M":
                    ncoaHistory.MatchFlag = Model.MatchFlag.Moved;
                    break;
                case "G":
                    ncoaHistory.MatchFlag = Model.MatchFlag.POBoxClosed;
                    break;
                case "K":
                    ncoaHistory.MatchFlag = Model.MatchFlag.MovedNoForwarding;
                    break;
                case "F":
                    ncoaHistory.MatchFlag = Model.MatchFlag.MovedToForeignCountry;
                    break;
                default:
                    ncoaHistory.MatchFlag = Model.MatchFlag.None;
                    break;
            }

            switch ( MoveType )
            {
                case "I":
                    ncoaHistory.MoveType = Model.MoveType.Individual;
                    break;
                case "F":
                    ncoaHistory.MoveType = Model.MoveType.Family;
                    break;
                case "B":
                    ncoaHistory.MoveType = Model.MoveType.Business;
                    break;
                default:
                    ncoaHistory.MoveType = Model.MoveType.None;
                    break;
            }

            return ncoaHistory;
        }
    }
}
