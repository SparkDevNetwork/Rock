// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class ConfigureCors : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddDefinedType( "Global", "REST CORS Domains (Advanced)", @"
Lists the external domains that are authorized to access the REST API through ""cross-origin resource sharing"" (CORS).", "DF7C8DF7-49F9-4858-9E5D-20842AF65AD8", @"
When a browser encounters a script that originated from another domain trying to access the Rock REST API, the browser will query Rock (using CORS) to check 
if that request should be allowed. By default Rock will deny access to the API through this type of cross-site request. To override this behavior, add the domains that
should be allowed to make this type of request to the list of values below. This will enable CORS for just those domains. Note: This only applies to REST calls made 
through scripts downloaded from another domain to a browser. It does not apply to REST calls made directly from another external server or application. That type of
request is allowed by default (but still controlled through security).");

            RockMigrationHelper.AddPage("91CCB1C9-5F9F-44F5-8BE2-9EC3A3CFD46F","D65F783D-87A9-4CC9-8110-E83466A0EADB","REST CORS Domains","","B03A8C4E-E394-44B0-B7CC-89B74C79C325","fa fa-sign-in"); // Site:Rock RMS
            // Add Block to Page: REST CORS Domains, Site: Rock RMS
            RockMigrationHelper.AddBlock("B03A8C4E-E394-44B0-B7CC-89B74C79C325","","0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE","Defined Value List","Main","","",1,"BC6CE880-382B-4DF2-8C10-B7F7C1DEC9FB"); 
            // Add Block to Page: REST CORS Domains, Site: Rock RMS
            RockMigrationHelper.AddBlock("B03A8C4E-E394-44B0-B7CC-89B74C79C325","","08C35F15-9AF7-468F-9D50-CDFD3D21220C","Defined Type Detail","Main","","",0,"EF27C0E7-9D1A-41AB-970B-C854299CE667"); 
            // Attrib Value for Block:Defined Value List, Attribute:Defined Type Page: REST CORS Domains, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("BC6CE880-382B-4DF2-8C10-B7F7C1DEC9FB","9280D61F-C4F3-4A3E-A9BB-BCD67FF78637",@"df7c8df7-49f9-4858-9e5d-20842af65ad8");
            // Attrib Value for Block:Defined Type Detail, Attribute:Defined Type Page: REST CORS Domains, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("EF27C0E7-9D1A-41AB-970B-C854299CE667","0305EF98-C791-4626-9996-F189B9BB674C",@"df7c8df7-49f9-4858-9e5d-20842af65ad8");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Block: Defined Type Detail, from Page: REST CORS Domains, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("EF27C0E7-9D1A-41AB-970B-C854299CE667");
            // Remove Block: Defined Value List, from Page: REST CORS Domains, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("BC6CE880-382B-4DF2-8C10-B7F7C1DEC9FB");

            RockMigrationHelper.DeletePage("B03A8C4E-E394-44B0-B7CC-89B74C79C325"); // Page: REST CORS DomainsLayout: Full Width, Site: Rock RMS        
        }
    }
}
