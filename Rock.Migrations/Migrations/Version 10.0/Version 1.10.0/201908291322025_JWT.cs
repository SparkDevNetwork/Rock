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
    public partial class JWT : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddDefinedType(
                "Global",
                "JSON Web Token Configuration",
                "These values represent validation parameters for JSON Web Tokens.  The actual defined value is an OpenID configuration URL.  An example value if you are using Auth0 would be https://xxxxxx.auth0.com/.well-known/openid-configuration.  Attributes for these defined values define additional parameters for token verification and then Person Search Key association.",
                SystemGuid.DefinedType.JWT_CONFIGURATION,
                "" );

            RockMigrationHelper.AddDefinedTypeAttribute(
                SystemGuid.DefinedType.JWT_CONFIGURATION,
                "59D5A94C-94A0-4630-B80A-BB25697D74C7",
                "Person Search Key",
                "PersonSearchTypeValue",
                "The person search keys to use when looking for the bearer of a token using a person search key",
                1037,
                "",
                Rock.SystemGuid.Attribute.DEFINED_VALUE_JWT_SEARCH_KEY );

            RockMigrationHelper.AddDefinedTypeAttribute( SystemGuid.DefinedType.JWT_CONFIGURATION, "9C204CD0-1233-41C5-818A-C5DA439445AA", "Audience", "Audience", "The Audience for the JSON Web Token", 1034, "", SystemGuid.Attribute.DEFINED_VALUE_JWT_AUDIENCE );
            RockMigrationHelper.AddDefinedTypeAttribute( SystemGuid.DefinedType.JWT_CONFIGURATION, "9C204CD0-1233-41C5-818A-C5DA439445AA", "Issuer", "Issuer", "The Issuer for the JSON Web Token", 1035, "", SystemGuid.Attribute.DEFINED_VALUE_JWT_ISSUER );
            RockMigrationHelper.AddDefinedTypeAttributeQualifier( SystemGuid.Attribute.DEFINED_VALUE_JWT_SEARCH_KEY, SystemGuid.DefinedType.PERSON_SEARCH_KEYS, "E1F99CD5-881D-485F-87F1-E74E713D43A1" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.DEFINED_VALUE_JWT_ISSUER ); // Issuer
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.DEFINED_VALUE_JWT_AUDIENCE ); // Audience
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.DEFINED_VALUE_JWT_SEARCH_KEY ); // PersonSearchTypeValue
            RockMigrationHelper.DeleteDefinedType( SystemGuid.DefinedType.JWT_CONFIGURATION ); // JSON Web Token Configuration
        }
    }
}
