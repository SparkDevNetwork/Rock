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
using System.Data.Entity.Migrations;

using Rock.Field.Types;

namespace Rock.Migrations
{
    
    /// <summary>
    ///
    /// </summary>
    public partial class AddAddressFieldRequirements : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add Field Type: Data Entry Requirement Level
            RockMigrationHelper.UpdateFieldType( "Data Entry Requirement Level", "Indicates the availability and necessity of a data entry item.", "Rock", "Rock.Field.Types.DataEntryRequirementLevelFieldType", SystemGuid.FieldType.DATA_ENTRY_REQUIREMENT_LEVEL );


            // Add Attributes for Defined Type "Country".
            RockMigrationHelper.AddDefinedTypeAttribute( SystemGuid.DefinedType.LOCATION_COUNTRIES, SystemGuid.FieldType.TEXT, "Locality Label", SystemKey.CountryAttributeKey.AddressLocalityLabel, "The label to use for the Locality field, which describes a subdivision of a state or region.", 2, true, "County", false, false, SystemGuid.Attribute.COUNTRY_LOCALITY_LABEL );

            RockMigrationHelper.AddDefinedTypeAttribute( SystemGuid.DefinedType.LOCATION_COUNTRIES, SystemGuid.FieldType.DATA_ENTRY_REQUIREMENT_LEVEL, "Address Line 1 Requirement", SystemKey.CountryAttributeKey.AddressLine1Requirement, "Sets the requirement for the Address Line 1 component of a postal address in this Country.", 6, true, DataEntryRequirementLevelSpecifier.Required.ConvertToInt().ToString(), false, false, SystemGuid.Attribute.COUNTRY_ADDRESS_LINE_1_REQUIREMENT );
            RockMigrationHelper.AddDefinedTypeAttribute( SystemGuid.DefinedType.LOCATION_COUNTRIES, SystemGuid.FieldType.DATA_ENTRY_REQUIREMENT_LEVEL, "Address Line 2 Requirement", SystemKey.CountryAttributeKey.AddressLine2Requirement, "Sets the requirement for the Address Line 2 component of a postal address in this Country.", 7, true, DataEntryRequirementLevelSpecifier.Optional.ConvertToInt().ToString(), false, false, SystemGuid.Attribute.COUNTRY_ADDRESS_LINE_2_REQUIREMENT );
            RockMigrationHelper.AddDefinedTypeAttribute( SystemGuid.DefinedType.LOCATION_COUNTRIES, SystemGuid.FieldType.DATA_ENTRY_REQUIREMENT_LEVEL, "City Requirement", SystemKey.CountryAttributeKey.AddressCityRequirement, "Sets the requirement for the City component of a postal address in this Country.", 8, true, DataEntryRequirementLevelSpecifier.Required.ConvertToInt().ToString(), false, false, SystemGuid.Attribute.COUNTRY_ADDRESS_CITY_REQUIREMENT );
            RockMigrationHelper.AddDefinedTypeAttribute( SystemGuid.DefinedType.LOCATION_COUNTRIES, SystemGuid.FieldType.DATA_ENTRY_REQUIREMENT_LEVEL, "Locality Requirement", SystemKey.CountryAttributeKey.AddressLocalityRequirement, "Sets the requirement for the Locality component of a postal address in this Country.", 9, true, DataEntryRequirementLevelSpecifier.Optional.ConvertToInt().ToString(), false, false, SystemGuid.Attribute.COUNTRY_ADDRESS_LOCALITY_REQUIREMENT );
            RockMigrationHelper.AddDefinedTypeAttribute( SystemGuid.DefinedType.LOCATION_COUNTRIES, SystemGuid.FieldType.DATA_ENTRY_REQUIREMENT_LEVEL, "State Requirement", SystemKey.CountryAttributeKey.AddressStateRequirement, "Sets the requirement for the State component of a postal address in this Country.", 10, true, DataEntryRequirementLevelSpecifier.Required.ConvertToInt().ToString(), false, false, SystemGuid.Attribute.COUNTRY_ADDRESS_STATE_REQUIREMENT );
            RockMigrationHelper.AddDefinedTypeAttribute( SystemGuid.DefinedType.LOCATION_COUNTRIES, SystemGuid.FieldType.DATA_ENTRY_REQUIREMENT_LEVEL, "Postal Code Requirement", SystemKey.CountryAttributeKey.AddressPostalCodeRequirement, "Sets the requirement for the Postal Code component of a postal address in this Country.", 11, true, DataEntryRequirementLevelSpecifier.Optional.ConvertToInt().ToString(), false, false, SystemGuid.Attribute.COUNTRY_ADDRESS_POSTCODE_REQUIREMENT );

            // Set Attribute Values for Country Defined Type "United States".
            RockMigrationHelper.AddDefinedValueAttributeValueByValue( SystemGuid.DefinedType.LOCATION_COUNTRIES, "US", SystemKey.CountryAttributeKey.AddressLocalityLabel, "County" );
            RockMigrationHelper.AddDefinedValueAttributeValueByValue( SystemGuid.DefinedType.LOCATION_COUNTRIES, "US", SystemKey.CountryAttributeKey.AddressLine1Requirement, DataEntryRequirementLevelSpecifier.Required.ConvertToInt().ToString() );
            RockMigrationHelper.AddDefinedValueAttributeValueByValue( SystemGuid.DefinedType.LOCATION_COUNTRIES, "US", SystemKey.CountryAttributeKey.AddressLine2Requirement, DataEntryRequirementLevelSpecifier.Optional.ConvertToInt().ToString() );
            RockMigrationHelper.AddDefinedValueAttributeValueByValue( SystemGuid.DefinedType.LOCATION_COUNTRIES, "US", SystemKey.CountryAttributeKey.AddressCityRequirement, DataEntryRequirementLevelSpecifier.Required.ConvertToInt().ToString() );
            RockMigrationHelper.AddDefinedValueAttributeValueByValue( SystemGuid.DefinedType.LOCATION_COUNTRIES, "US", SystemKey.CountryAttributeKey.AddressLocalityRequirement, DataEntryRequirementLevelSpecifier.Optional.ConvertToInt().ToString() );
            RockMigrationHelper.AddDefinedValueAttributeValueByValue( SystemGuid.DefinedType.LOCATION_COUNTRIES, "US", SystemKey.CountryAttributeKey.AddressStateRequirement, DataEntryRequirementLevelSpecifier.Required.ConvertToInt().ToString() );
            RockMigrationHelper.AddDefinedValueAttributeValueByValue( SystemGuid.DefinedType.LOCATION_COUNTRIES, "US", SystemKey.CountryAttributeKey.AddressPostalCodeRequirement, DataEntryRequirementLevelSpecifier.Optional.ConvertToInt().ToString() );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Attributes for Defined Type "Country".
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.COUNTRY_LOCALITY_LABEL );

            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.COUNTRY_ADDRESS_LINE_1_REQUIREMENT );
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.COUNTRY_ADDRESS_LINE_2_REQUIREMENT );
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.COUNTRY_ADDRESS_CITY_REQUIREMENT );
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.COUNTRY_ADDRESS_LOCALITY_REQUIREMENT );
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.COUNTRY_ADDRESS_STATE_REQUIREMENT );
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.COUNTRY_ADDRESS_POSTCODE_REQUIREMENT );

            // Remove Field Type: Data Entry Requirement Level
            RockMigrationHelper.DeleteFieldType( SystemGuid.FieldType.DATA_ENTRY_REQUIREMENT_LEVEL );
        }
    }
}
