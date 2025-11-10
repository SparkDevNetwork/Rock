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
    public partial class AddGoogleApiKeyServerGlobalAttribute : Rock.Migrations.RockMigration
    {
        private const string GLOBAL_GOOGLE_API_KEY_SERVER = "B2CAB2A1-BFFD-ADB4-4DEC-7D762BC2793F";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add new global attribute for storing a Google API key that can be used from the server. The existing one
            // is configured to be used from javascript and therefore can not be locked down by the server's IP address.
            RockMigrationHelper.AddGlobalAttribute(
                SystemGuid.FieldType.TEXT
                , String.Empty // qualifier column
                , String.Empty // qualifier value
                , "Google API Key Server"
                , "Use this key for server-side requests to Google APIs, such as geocoding or directions. This should be a separate key from the one used for client-side calls and should be restricted by your server’s IP address. The  \"Google API Key\" global attribute is intended for browser-based requests."
                , 0 // order 
                , string.Empty // default value
                , GLOBAL_GOOGLE_API_KEY_SERVER
                , "GoogleApiKeyServer" );

            // Add new global attribute to the 'Config' category
            Sql( $@" DECLARE @GoogleApiKeyServerAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '{GLOBAL_GOOGLE_API_KEY_SERVER}')
                     DECLARE @ConfigCategoryId int = (SELECT TOP 1 [Id] FROM [Category] WHERE [Guid] = 'bb40b563-18d1-4133-94b9-d7f67d95e4e3')

                     INSERT INTO [AttributeCategory]
                        ([AttributeId],[CategoryId])
                     VALUES 
                        (@GoogleApiKeyServerAttributeId, @ConfigCategoryId)" );

            // Update the current client side global attribute to have a clear description
            Sql( @"UPDATE [Attribute]
                    SET [Description] = 'Use this key for client-side requests to Google APIs, such as Maps JavaScript. This key should be restricted by HTTP referrers (e.g., your domain). For server-side requests, use the ""Google API Key Server"" global attribute.'
                    WHERE [Guid] = 'd8b02008-b672-4414-95b1-616d62676056'
                    " );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( GLOBAL_GOOGLE_API_KEY_SERVER );
        }
    }
}
