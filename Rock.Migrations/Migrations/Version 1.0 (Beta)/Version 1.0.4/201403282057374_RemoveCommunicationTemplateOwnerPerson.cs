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
    public partial class RemoveCommunicationTemplateOwnerPerson : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey("dbo.CommunicationTemplate", "OwnerPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.CommunicationTemplate", new[] { "OwnerPersonAliasId" });
            DropColumn("dbo.CommunicationTemplate", "OwnerPersonAliasId");

            // Rollup of other stuff to put in Migration
            Sql( @"
-- Change MapStyles DefinedValues to IsSystem=False
update [DefinedValue] set [IsSystem] = 0 where [Guid] in ('B1B95FDC-BB41-429F-A5D0-04D4D8284E2C','C67A6551-C3A7-451A-AA64-9F5159D63D3D','BFC46259-FB66-4427-BF05-2B030A582BEA','54DB5C2B-D099-4A89-A1C1-60FB2EF4EFE6','0965072D-D7D5-41FC-A70B-6F69AA4E9EEB','9CB28B88-57A1-484F-BA2D-641CAF727CB2','E00AC5FE-C8FF-4499-82B7-507932DE2308','FDC5D6BA-A818-4A06-96B1-9EF31B4087AC')" );

            Sql( @"
-- Update these InternalHomePage blocks to IsSystem=False
Update [Block] set [IsSystem]=0 where Guid in (
'6A648E77-ABA9-4AAF-A8BB-027A12261ED9', -- 	HTML Content (Quick Links)
'CB8F9152-08BB-4576-B7A1-B0DDD9880C44', -- 	Active Users (Internal Users)
'03FCBF5A-42E0-4F45-B670-BC8E324BD573' -- 	Active Users (External Users)
)
" );

            // Add the HideNewAccount attribute to the Login block type
            UpdateBlockTypeAttribute( "7B83D513-1178-429E-93FF-E76430E038E4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide New Account Option", "HideNewAccount", "", "Should 'New Account' option be hidden?  For site's that require user to be in a role (Internal Rock Site for example), users shouldn't be able to create their own accont.", 6, "False", "7D47046D-5D66-45BB-ACFA-7460DE112FC2" );
            AddBlockAttributeValue( "3D325BB3-E1C9-4194-8E9B-11BFFC347DC3", "7D47046D-5D66-45BB-ACFA-7460DE112FC2", "True" );
            AddBlockAttributeValue( "A8E221F0-DE4E-4B0F-B660-BC7AC2298EF8", "7D47046D-5D66-45BB-ACFA-7460DE112FC2", "False" );

            // Add Job for Location Services Verify
            Sql( @"
INSERT INTO [dbo].[ServiceJob]
           ([IsSystem]
           ,[IsActive]
           ,[Name]
           ,[Description]
           ,[Class]
           ,[CronExpression]
           ,[NotificationStatus]
           ,[Guid])
     VALUES
        (0	
         ,1	
         ,'Location Services Verify'
         ,'Attempts to standardize and geocode addresses that have not been verified yet. It also attempts to re-verify address that failed in the past after a given wait period.'
         ,'Rock.Jobs.LocationServicesVerify'
         ,'0 0 3 1/1 * ? *'
         ,3
         ,'9384778B-7669-4C72-A073-81E046807A90')
" );

            Sql( "UPDATE [Page] set [IconCssClass] = 'fa fa-credit-card' WHERE [Guid] = '7CA317B5-5C47-465D-B407-7D614F2A568F'" );
            
            
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove the HideNewAccount attribute from the login block type
            DeleteBlockAttribute( "7D47046D-5D66-45BB-ACFA-7460DE112FC2" );
            
            AddColumn("dbo.CommunicationTemplate", "OwnerPersonAliasId", c => c.Int());
            CreateIndex("dbo.CommunicationTemplate", "OwnerPersonAliasId");
            AddForeignKey("dbo.CommunicationTemplate", "OwnerPersonAliasId", "dbo.PersonAlias", "Id");

            Sql( "DELETE FROM [ServiceJob] WHERE [Guid] = '9384778B-7669-4C72-A073-81E046807A90'" );
        }
    }
}
