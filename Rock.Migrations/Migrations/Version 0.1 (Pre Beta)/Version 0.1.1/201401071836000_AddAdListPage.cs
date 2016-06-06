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
    public partial class AddAdListPage : Rock.Migrations.RockMigration1
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add an Ad List page under Tools > Website & Communication.
            AddPage( "F7105BFE-B28C-41B6-9CE6-F1018D77DD8F", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Ad List", "A list of all the ads in the system", "78D470E9-221B-4EBD-9FF6-995B45FB9CD5" );
            // Add the MarketingCampaignAdList block to the Ad List page.
            AddBlock( "78D470E9-221B-4EBD-9FF6-995B45FB9CD5", "", "0A690902-A0A1-4AB1-AFEC-001BA5FD124B", "Ad List", "Main", 0, "4CB0738F-E76D-4BAD-92D5-BA3C680E672B" );
            // Set the Show Marketing Campaign Title attribute to True for this instance of the Marketing Campaign Ad List block.
            AddBlockAttributeValue( "4CB0738F-E76D-4BAD-92D5-BA3C680E672B", "F78D9781-DD8F-4AF7-A3E3-9ADC289C829B", "True" );

            // Add an Ad Detail page as the detail page for the Ad List page.
            AddPage( "78D470E9-221B-4EBD-9FF6-995B45FB9CD5", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Ad Detail", "Detail page for managing the Ad information", "5B4A4DF6-17BB-4C99-B5B7-6DC9C896BC8E" );
            // Add the MarketingCampaignAdDetail block to the Ad Detail page.
            AddBlock( "5B4A4DF6-17BB-4C99-B5B7-6DC9C896BC8E", "", "3025D1FF-8022-4E0F-8918-515D07D50335", "Ad Detail", "Main", 0, "C610D250-8B84-4B11-B57A-EB24C27BD989" );
            // Set the Detail Page attribute to the Ad Detail page for this instance of the Marketing Campaign Ad List block.
            AddBlockAttributeValue( "4CB0738F-E76D-4BAD-92D5-BA3C680E672B", "0FDE1CB9-0E7D-4275-A6FF-670ABB6BE8CA", @"5B4A4DF6-17BB-4C99-B5B7-6DC9C896BC8E" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Delete the Detail Page attribute value from the Marketing Campaign Ad List block.
            DeleteBlockAttributeValue( "4CB0738F-E76D-4BAD-92D5-BA3C680E672B", "0FDE1CB9-0E7D-4275-A6FF-670ABB6BE8CA" );
            // Delete the Marketing Campaign Ad Detail block from the Ad Detail page.
            DeleteBlock( "C610D250-8B84-4B11-B57A-EB24C27BD989" );
            // Delete the Ad Detail page.
            DeletePage( "5B4A4DF6-17BB-4C99-B5B7-6DC9C896BC8E" );

            // Delete the Show Marketing Campaign Title attribute value from the Marketing Campaign Ad List block.
            DeleteBlockAttributeValue( "4CB0738F-E76D-4BAD-92D5-BA3C680E672B", "F78D9781-DD8F-4AF7-A3E3-9ADC289C829B" );
            // Delete the Marketing Campaing Ad List block from the Ad List page.
            DeleteBlock( "4CB0738F-E76D-4BAD-92D5-BA3C680E672B" );
            // Delete the Ad List page.
            DeletePage( "78D470E9-221B-4EBD-9FF6-995B45FB9CD5" );
        }
    }
}
