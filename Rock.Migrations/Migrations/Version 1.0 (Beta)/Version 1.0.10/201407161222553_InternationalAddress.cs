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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class InternationalAddress : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {

            RockMigrationHelper.AddGlobalAttribute( "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "", "", "Support International Addresses", "Should user's be able to select a country when entering addresses?", 0, "False", "14D56DC9-7A64-4210-97D9-BF1ABE409DC7" );

            Sql( @"
    -- Default all existing addresses to include country of 'US'
    UPDATE [Location] SET [Country] = 'US' 
    WHERE [Country] IS NULL 
	    AND [State] IS NOT NULL
	    AND [Zip] IS NOT NULL

    -- Remove the 'Abbreviation' Attribute from Country defined type and update Name to be the abbreviation (Description is full name)
    UPDATE DV	
	    SET [Name] = AV.[Value]
    FROM [Attribute] A
	    INNER JOIN [AttributeValue] AV ON AV.[AttributeId] = A.[Id]
	    INNER JOIN [DefinedValue] DV ON AV.[EntityId] = DV.[Id]
    WHERE A.[Guid] = 'DA46DC37-5398-4520-B6A5-6E57C9C46F7A'
    DELETE [Attribute] WHERE [Guid] = 'DA46DC37-5398-4520-B6A5-6E57C9C46F7A'

    UPDATE [Attribute]
    SET [DefaultValue] = 'Province'
    WHERE [Guid] = 'A4E00B14-8CFF-4719-A43F-462851C7BBEF'

    UPDATE [Attribute]
    SET [DefaultValue] = 'Postal Code'
    WHERE [Guid] = '7D785A5D-53CA-4FEC-BC88-DFBD7439B547'

    -- Update the default address format
    DECLARE @crlf varchar(2) = char(13) + char(10)
    UPDATE [Attribute]
    SET [DefaultValue] = '{{ Street1 }}' + @crlf + '{{ Street2 }}' + @crlf + '{{ City }}, {{ State }} {{ Zip }}' + @crlf + '{{ Country }}'
    WHERE [Guid] = 'B6EF4138-C488-4043-A628-D35F91503843'
" );
            RockMigrationHelper.AddDefinedTypeAttribute( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "9C204CD0-1233-41C5-818A-C5DA439445AA", "City Label", "CityLabel", "The label to use for the 'city' field", 0, "City", "F5FDC3C6-9900-4BED-8FBD-CB2173EB585E" );

            Sql( @"
    UPDATE [Attribute] SET [IsGridColumn] = 1  WHERE [Guid] = 'F5FDC3C6-9900-4BED-8FBD-CB2173EB585E'
" );
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "AF", "Afghanistan", 0, false ); // Afghanistan
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "AX", "Aland Islands", 1, false ); // Aland Islands
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "AL", "Albania", 2, false ); // Albania
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "DZ", "Algeria", 3, false ); // Algeria
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "AS", "American Samoa", 4, false ); // American Samoa
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "AD", "Andorra", 5, false ); // Andorra
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "AO", "Angola", 6, false ); // Angola
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "AI", "Anguilla", 7, false ); // Anguilla
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "AQ", "Antarctica", 8, false ); // Antarctica
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "AG", "Antigua and Barbuda", 9, false ); // Antigua and Barbuda
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "AR", "Argentina", 10, false ); // Argentina
            RockMigrationHelper.AddDefinedValueAttributeValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "AR", "CityLabel", @"Complements" ); // Argentina City Label
            RockMigrationHelper.AddDefinedValueAttributeValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "AR", "StateLabel", @"Municipality" ); // Argentina State Label
            RockMigrationHelper.AddDefinedValueAttributeValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "AR", "PostalCodeLabel", @"Postal Code" ); // Argentina PostalCode Label
            RockMigrationHelper.AddDefinedValueAttributeValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "AR", "AddressFormat", @"{{ Street1 }}
{{ Street2 }} {{ City }}
{{ Zip }} {{ State }}
{{ Country }}" ); // Argentina Address Format
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "AM", "Armenia", 11, false ); // Armenia
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "AW", "Aruba", 12, false ); // Aruba
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "AU", "Australia", 13, false ); // Australia
            RockMigrationHelper.AddDefinedValueAttributeValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "AU", "CityLabel", @"Locality" ); // Australia City Label
            RockMigrationHelper.AddDefinedValueAttributeValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "AU", "StateLabel", @"State" ); // Australia State Label
            RockMigrationHelper.AddDefinedValueAttributeValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "AU", "PostalCodeLabel", @"Postcode" ); // Australia PostalCode Label
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "AT", "Austria", 14, false ); // Austria
            RockMigrationHelper.AddDefinedValueAttributeValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "AT", "CityLabel", @"Town" ); // Austria City Label
            RockMigrationHelper.AddDefinedValueAttributeValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "AT", "StateLabel", @"" ); // Austria State Label
            RockMigrationHelper.AddDefinedValueAttributeValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "AT", "PostalCodeLabel", @"Postal Code" ); // Austria PostalCode Label
            RockMigrationHelper.AddDefinedValueAttributeValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "AT", "AddressFormat", @"{{ Street1 }}
{{ Street2 }}
{{ Zip }} {{ City }}
{{ Country }}" ); // Austria Address Format
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "AZ", "Azerbaijan", 15, false ); // Azerbaijan
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "BS", "Bahamas", 16, false ); // Bahamas
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "BH", "Bahrain", 17, false ); // Bahrain
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "BD", "Bangladesh", 18, false ); // Bangladesh
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "BB", "Barbados", 19, false ); // Barbados
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "BY", "Belarus", 20, false ); // Belarus
            RockMigrationHelper.AddDefinedValueAttributeValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "BY", "CityLabel", @"Village" ); // Belarus City Label
            RockMigrationHelper.AddDefinedValueAttributeValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "BY", "StateLabel", @"Region" ); // Belarus State Label
            RockMigrationHelper.AddDefinedValueAttributeValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "BY", "PostalCodeLabel", @"Postal Code" ); // Belarus PostalCode Label
            RockMigrationHelper.AddDefinedValueAttributeValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "BY", "AddressFormat", @"{{ Street1 }}
{{ Street2 }}
{{ City }}
{{ Zip }}
{{ State }}
{{ Country }}" ); // Belarus Address Format
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "BE", "Belgium", 21, false ); // Belgium
            RockMigrationHelper.AddDefinedValueAttributeValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "BE", "CityLabel", @"Spatial" ); // Belgium City Label
            RockMigrationHelper.AddDefinedValueAttributeValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "BE", "StateLabel", @"Town" ); // Belgium State Label
            RockMigrationHelper.AddDefinedValueAttributeValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "BE", "PostalCodeLabel", @"Postal Code" ); // Belgium PostalCode Label
            RockMigrationHelper.AddDefinedValueAttributeValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "BE", "AddressFormat", @"{{ Street1 }}
{{ Street2 }}
{{ City }}
{{ Zip }} {{ State }}
{{ Country }}" ); // Belgium Address Format
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "BZ", "Belize", 22, false ); // Belize
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "BJ", "Benin", 23, false ); // Benin
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "BM", "Bermuda", 24, false ); // Bermuda
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "BT", "Bhutan", 25, false ); // Bhutan
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "BO", "Bolivia", 26, false ); // Bolivia
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "BQ", "Bonaire, Saint Eustatius and Saba ", 27, false ); // Bonaire, Saint Eustatius and Saba 
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "BA", "Bosnia and Herzegovina", 28, false ); // Bosnia and Herzegovina
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "BW", "Botswana", 29, false ); // Botswana
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "BV", "Bouvet Island", 30, false ); // Bouvet Island
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "BR", "Brazil", 31, false ); // Brazil
            RockMigrationHelper.AddDefinedValueAttributeValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "BR", "CityLabel", @"Neighbourhood" ); // Brazil City Label
            RockMigrationHelper.AddDefinedValueAttributeValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "BR", "StateLabel", @"Municipality, State" ); // Brazil State Label
            RockMigrationHelper.AddDefinedValueAttributeValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "BR", "PostalCodeLabel", @"Postal Code" ); // Brazil PostalCode Label
            RockMigrationHelper.AddDefinedValueAttributeValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "BR", "AddressFormat", @"{{ Street1 }}
{{ Street2 }}
{{ City }}
{{ State }}
{{ Zip }}
{{ Country }}" ); // Brazil Address Format
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "IO", "British Indian Ocean Territory", 32, false ); // British Indian Ocean Territory
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "VG", "British Virgin Islands", 33, false ); // British Virgin Islands
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "BN", "Brunei", 34, false ); // Brunei
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "BG", "Bulgaria", 35, false ); // Bulgaria
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "BF", "Burkina Faso", 36, false ); // Burkina Faso
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "BI", "Burundi", 37, false ); // Burundi
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "KH", "Cambodia", 38, false ); // Cambodia
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "CM", "Cameroon", 39, false ); // Cameroon
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "CA", "Canada", 40, false ); // Canada
            RockMigrationHelper.AddDefinedValueAttributeValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "CA", "CityLabel", @"City" ); // Canada City Label
            RockMigrationHelper.AddDefinedValueAttributeValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "CA", "StateLabel", @"Province/Territory" ); // Canada State Label
            RockMigrationHelper.AddDefinedValueAttributeValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "CA", "PostalCodeLabel", @"Postal Code" ); // Canada PostalCode Label
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "CV", "Cape Verde", 41, false ); // Cape Verde
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "KY", "Cayman Islands", 42, false ); // Cayman Islands
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "CF", "Central African Republic", 43, false ); // Central African Republic
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "TD", "Chad", 44, false ); // Chad
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "CL", "Chile", 45, false ); // Chile
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "CN", "China", 46, false ); // China
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "CX", "Christmas Island", 47, false ); // Christmas Island
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "CC", "Cocos Islands", 48, false ); // Cocos Islands
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "CO", "Colombia", 49, false ); // Colombia
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "KM", "Comoros", 50, false ); // Comoros
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "CK", "Cook Islands", 51, false ); // Cook Islands
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "CR", "Costa Rica", 52, false ); // Costa Rica
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "HR", "Croatia", 53, false ); // Croatia
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "CU", "Cuba", 54, false ); // Cuba
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "CW", "Curacao", 55, false ); // Curacao
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "CY", "Cyprus", 56, false ); // Cyprus
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "CZ", "Czech Republic", 57, false ); // Czech Republic
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "CD", "Democratic Republic of the Congo", 58, false ); // Democratic Republic of the Congo
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "DK", "Denmark", 59, false ); // Denmark
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "DJ", "Djibouti", 60, false ); // Djibouti
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "DM", "Dominica", 61, false ); // Dominica
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "DO", "Dominican Republic", 62, false ); // Dominican Republic
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "TL", "East Timor", 63, false ); // East Timor
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "EC", "Ecuador", 64, false ); // Ecuador
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "EG", "Egypt", 65, false ); // Egypt
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "SV", "El Salvador", 66, false ); // El Salvador
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "GQ", "Equatorial Guinea", 67, false ); // Equatorial Guinea
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "ER", "Eritrea", 68, false ); // Eritrea
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "EE", "Estonia", 69, false ); // Estonia
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "ET", "Ethiopia", 70, false ); // Ethiopia
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "FK", "Falkland Islands", 71, false ); // Falkland Islands
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "FO", "Faroe Islands", 72, false ); // Faroe Islands
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "FJ", "Fiji", 73, false ); // Fiji
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "FI", "Finland", 74, false ); // Finland
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "FR", "France", 75, false ); // France
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "GF", "French Guiana", 76, false ); // French Guiana
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "PF", "French Polynesia", 77, false ); // French Polynesia
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "TF", "French Southern Territories", 78, false ); // French Southern Territories
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "GA", "Gabon", 79, false ); // Gabon
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "GM", "Gambia", 80, false ); // Gambia
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "GE", "Georgia", 81, false ); // Georgia
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "DE", "Germany", 82, false ); // Germany
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "GH", "Ghana", 83, false ); // Ghana
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "GI", "Gibraltar", 84, false ); // Gibraltar
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "GR", "Greece", 85, false ); // Greece
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "GL", "Greenland", 86, false ); // Greenland
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "GD", "Grenada", 87, false ); // Grenada
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "GP", "Guadeloupe", 88, false ); // Guadeloupe
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "GU", "Guam", 89, false ); // Guam
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "GT", "Guatemala", 90, false ); // Guatemala
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "GG", "Guernsey", 91, false ); // Guernsey
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "GN", "Guinea", 92, false ); // Guinea
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "GW", "Guinea-Bissau", 93, false ); // Guinea-Bissau
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "GY", "Guyana", 94, false ); // Guyana
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "HT", "Haiti", 95, false ); // Haiti
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "HM", "Heard Island and McDonald Islands", 96, false ); // Heard Island and McDonald Islands
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "HN", "Honduras", 97, false ); // Honduras
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "HK", "Hong Kong", 98, false ); // Hong Kong
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "HU", "Hungary", 99, false ); // Hungary
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "IS", "Iceland", 100, false ); // Iceland
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "IN", "India", 101, false ); // India
            RockMigrationHelper.AddDefinedValueAttributeValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "IN", "CityLabel", @"Locality" ); // India City Label
            RockMigrationHelper.AddDefinedValueAttributeValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "IN", "StateLabel", @"State" ); // India State Label
            RockMigrationHelper.AddDefinedValueAttributeValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "IN", "PostalCodeLabel", @"City - Postal Code" ); // India PostalCode Label
            RockMigrationHelper.AddDefinedValueAttributeValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "IN", "AddressFormat", @"{{ Street1 }}
{{ Street2 }}
{{ City }}
{{ Zip }}
{{ State }}
{{ Country }}" ); // India Address Format
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "ID", "Indonesia", 102, false ); // Indonesia
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "IR", "Iran", 103, false ); // Iran
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "IQ", "Iraq", 104, false ); // Iraq
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "IE", "Ireland", 105, false ); // Ireland
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "IM", "Isle of Man", 106, false ); // Isle of Man
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "IL", "Israel", 107, false ); // Israel
            RockMigrationHelper.AddDefinedValueAttributeValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "IL", "CityLabel", @"Town" ); // Israel City Label
            RockMigrationHelper.AddDefinedValueAttributeValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "IL", "PostalCodeLabel", @"Postal Code" ); // Israel PostalCode Label
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "IT", "Italy", 108, false ); // Italy
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "CI", "Ivory Coast", 109, false ); // Ivory Coast
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "JM", "Jamaica", 110, false ); // Jamaica
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "JP", "Japan", 111, false ); // Japan
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "JE", "Jersey", 112, false ); // Jersey
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "JO", "Jordan", 113, false ); // Jordan
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "KZ", "Kazakhstan", 114, false ); // Kazakhstan
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "KE", "Kenya", 115, false ); // Kenya
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "KI", "Kiribati", 116, false ); // Kiribati
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "XK", "Kosovo", 117, false ); // Kosovo
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "KW", "Kuwait", 118, false ); // Kuwait
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "KG", "Kyrgyzstan", 119, false ); // Kyrgyzstan
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "LA", "Laos", 120, false ); // Laos
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "LV", "Latvia", 121, false ); // Latvia
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "LB", "Lebanon", 122, false ); // Lebanon
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "LS", "Lesotho", 123, false ); // Lesotho
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "LR", "Liberia", 124, false ); // Liberia
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "LY", "Libya", 125, false ); // Libya
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "LI", "Liechtenstein", 126, false ); // Liechtenstein
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "LT", "Lithuania", 127, false ); // Lithuania
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "LU", "Luxembourg", 128, false ); // Luxembourg
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "MO", "Macao", 129, false ); // Macao
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "MK", "Macedonia", 130, false ); // Macedonia
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "MG", "Madagascar", 131, false ); // Madagascar
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "MW", "Malawi", 132, false ); // Malawi
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "MY", "Malaysia", 133, false ); // Malaysia
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "MV", "Maldives", 134, false ); // Maldives
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "ML", "Mali", 135, false ); // Mali
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "MT", "Malta", 136, false ); // Malta
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "MH", "Marshall Islands", 137, false ); // Marshall Islands
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "MQ", "Martinique", 138, false ); // Martinique
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "MR", "Mauritania", 139, false ); // Mauritania
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "MU", "Mauritius", 140, false ); // Mauritius
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "YT", "Mayotte", 141, false ); // Mayotte
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "MX", "Mexico", 142, false ); // Mexico
            RockMigrationHelper.AddDefinedValueAttributeValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "MX", "CityLabel", @"City" ); // Mexico City Label
            RockMigrationHelper.AddDefinedValueAttributeValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "MX", "StateLabel", @"State" ); // Mexico State Label
            RockMigrationHelper.AddDefinedValueAttributeValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "MX", "PostalCodeLabel", @"Postal Code" ); // Mexico PostalCode Label
            RockMigrationHelper.AddDefinedValueAttributeValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "MX", "AddressFormat", @"{{ Street1 }}
{{ Street2 }}
{{ Zip }} {{ City }}, {{ State }}
{{ Country }}" ); // Mexico Address Format
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "FM", "Micronesia", 143, false ); // Micronesia
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "MD", "Moldova", 144, false ); // Moldova
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "MC", "Monaco", 145, false ); // Monaco
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "MN", "Mongolia", 146, false ); // Mongolia
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "ME", "Montenegro", 147, false ); // Montenegro
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "MS", "Montserrat", 148, false ); // Montserrat
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "MA", "Morocco", 149, false ); // Morocco
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "MZ", "Mozambique", 150, false ); // Mozambique
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "MM", "Myanmar", 151, false ); // Myanmar
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "NA", "Namibia", 152, false ); // Namibia
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "NR", "Nauru", 153, false ); // Nauru
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "NP", "Nepal", 154, false ); // Nepal
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "NL", "Netherlands", 155, false ); // Netherlands
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "AN", "Netherlands Antilles", 156, false ); // Netherlands Antilles
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "NC", "New Caledonia", 157, false ); // New Caledonia
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "NZ", "New Zealand", 158, false ); // New Zealand
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "NI", "Nicaragua", 159, false ); // Nicaragua
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "NE", "Niger", 160, false ); // Niger
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "NG", "Nigeria", 161, false ); // Nigeria
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "NU", "Niue", 162, false ); // Niue
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "NF", "Norfolk Island", 163, false ); // Norfolk Island
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "KP", "North Korea", 164, false ); // North Korea
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "MP", "Northern Mariana Islands", 165, false ); // Northern Mariana Islands
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "NO", "Norway", 166, false ); // Norway
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "OM", "Oman", 167, false ); // Oman
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "PK", "Pakistan", 168, false ); // Pakistan
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "PW", "Palau", 169, false ); // Palau
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "PS", "Palestinian Territory", 170, false ); // Palestinian Territory
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "PA", "Panama", 171, false ); // Panama
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "PG", "Papua New Guinea", 172, false ); // Papua New Guinea
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "PY", "Paraguay", 173, false ); // Paraguay
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "PE", "Peru", 174, false ); // Peru
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "PH", "Philippines", 175, false ); // Philippines
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "PN", "Pitcairn", 176, false ); // Pitcairn
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "PL", "Poland", 177, false ); // Poland
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "PT", "Portugal", 178, false ); // Portugal
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "PR", "Puerto Rico", 179, false ); // Puerto Rico
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "QA", "Qatar", 180, false ); // Qatar
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "CG", "Republic of the Congo", 181, false ); // Republic of the Congo
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "RE", "Reunion", 182, false ); // Reunion
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "RO", "Romania", 183, false ); // Romania
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "RU", "Russia", 184, false ); // Russia
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "RW", "Rwanda", 185, false ); // Rwanda
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "BL", "Saint Barthelemy", 186, false ); // Saint Barthelemy
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "SH", "Saint Helena", 187, false ); // Saint Helena
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "KN", "Saint Kitts and Nevis", 188, false ); // Saint Kitts and Nevis
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "LC", "Saint Lucia", 189, false ); // Saint Lucia
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "MF", "Saint Martin", 190, false ); // Saint Martin
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "PM", "Saint Pierre and Miquelon", 191, false ); // Saint Pierre and Miquelon
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "VC", "Saint Vincent and the Grenadines", 192, false ); // Saint Vincent and the Grenadines
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "WS", "Samoa", 193, false ); // Samoa
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "SM", "San Marino", 194, false ); // San Marino
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "ST", "Sao Tome and Principe", 195, false ); // Sao Tome and Principe
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "SA", "Saudi Arabia", 196, false ); // Saudi Arabia
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "SN", "Senegal", 197, false ); // Senegal
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "RS", "Serbia", 198, false ); // Serbia
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "CS", "Serbia and Montenegro", 199, false ); // Serbia and Montenegro
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "SC", "Seychelles", 200, false ); // Seychelles
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "SL", "Sierra Leone", 201, false ); // Sierra Leone
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "SG", "Singapore", 202, false ); // Singapore
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "SX", "Sint Maarten", 203, false ); // Sint Maarten
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "SK", "Slovakia", 204, false ); // Slovakia
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "SI", "Slovenia", 205, false ); // Slovenia
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "SB", "Solomon Islands", 206, false ); // Solomon Islands
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "SO", "Somalia", 207, false ); // Somalia
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "ZA", "South Africa", 208, false ); // South Africa
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "GS", "South Georgia and the South Sandwich Islands", 209, false ); // South Georgia and the South Sandwich Islands
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "KR", "South Korea", 210, false ); // South Korea
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "SS", "South Sudan", 211, false ); // South Sudan
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "ES", "Spain", 212, false ); // Spain
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "LK", "Sri Lanka", 213, false ); // Sri Lanka
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "SD", "Sudan", 214, false ); // Sudan
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "SR", "Suriname", 215, false ); // Suriname
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "SJ", "Svalbard and Jan Mayen", 216, false ); // Svalbard and Jan Mayen
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "SZ", "Swaziland", 217, false ); // Swaziland
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "SE", "Sweden", 218, false ); // Sweden
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "CH", "Switzerland", 219, false ); // Switzerland
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "SY", "Syria", 220, false ); // Syria
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "TW", "Taiwan", 221, false ); // Taiwan
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "TJ", "Tajikistan", 222, false ); // Tajikistan
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "TZ", "Tanzania", 223, false ); // Tanzania
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "TH", "Thailand", 224, false ); // Thailand
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "TG", "Togo", 225, false ); // Togo
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "TK", "Tokelau", 226, false ); // Tokelau
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "TO", "Tonga", 227, false ); // Tonga
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "TT", "Trinidad and Tobago", 228, false ); // Trinidad and Tobago
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "TN", "Tunisia", 229, false ); // Tunisia
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "TR", "Turkey", 230, false ); // Turkey
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "TM", "Turkmenistan", 231, false ); // Turkmenistan
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "TC", "Turks and Caicos Islands", 232, false ); // Turks and Caicos Islands
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "TV", "Tuvalu", 233, false ); // Tuvalu
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "VI", "U.S. Virgin Islands", 234, false ); // U.S. Virgin Islands
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "UG", "Uganda", 235, false ); // Uganda
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "UA", "Ukraine", 236, false ); // Ukraine
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "AE", "United Arab Emirates", 237, false ); // United Arab Emirates
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "GB", "United Kingdom", 238, false ); // United Kingdom
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "US", "United States", 239, false ); // United States
            RockMigrationHelper.AddDefinedValueAttributeValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "US", "CityLabel", @"City" ); // United States City Label
            RockMigrationHelper.AddDefinedValueAttributeValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "US", "StateLabel", @"State" ); // United States State Label
            RockMigrationHelper.AddDefinedValueAttributeValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "US", "PostalCodeLabel", @"Zip" ); // United States PostalCode Label
            RockMigrationHelper.AddDefinedValueAttributeValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "US", "AddressFormat", @"{{ Street1 }}
{{ Street2 }}
{{ City }}, {{ State }} {{ Zip }}" ); // United States Address Format
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "UM", "United States Minor Outlying Islands", 240, false ); // United States Minor Outlying Islands
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "UY", "Uruguay", 241, false ); // Uruguay
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "UZ", "Uzbekistan", 242, false ); // Uzbekistan
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "VU", "Vanuatu", 243, false ); // Vanuatu
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "VA", "Vatican", 244, false ); // Vatican
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "VE", "Venezuela", 245, false ); // Venezuela
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "VN", "Vietnam", 246, false ); // Vietnam
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "WF", "Wallis and Futuna", 247, false ); // Wallis and Futuna
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "EH", "Western Sahara", 248, false ); // Western Sahara
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "YE", "Yemen", 249, false ); // Yemen
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "ZM", "Zambia", 250, false ); // Zambia
            RockMigrationHelper.UpdateDefinedValueByName_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "ZW", "Zimbabwe", 251, false ); // Zimbabwe


            // From Migration Rollup List
            Sql( @"
    -- Add icon to following page
    UPDATE [Page] SET [IconCssClass] = 'fa fa-flag' WHERE [Guid] = 'A6AE67F7-0B46-4F9A-9C96-054E1E82F784'

    -- Change icon for 'Childhood Information' 
    UPDATE [Category] SET [IconCssClass] = 'fa fa-child' WHERE [Guid] = '752DC692-836E-4A3E-B670-4325CD7724BF'

    -- Change the sites for check-in to be non-system so themes can be changed
    UPDATE [Site] SET [IsSystem] = 0 WHERE [Guid] IN ('15AEFC01-ACB3-4F5D-B83E-AB3AB7F2A54A', '30FB46F7-4814-4691-852A-04FB56CC07F0', 'A5FA7C3C-A238-4E0B-95DE-B540144321EC')
" );



        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
