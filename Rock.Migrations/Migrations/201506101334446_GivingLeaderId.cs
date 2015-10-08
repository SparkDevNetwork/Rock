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
    public partial class GivingLeaderId : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
-- create function for giving lead
/*
<doc>
	<summary>
 		This function returns the lead giving person id for a given person id and giving group id.
	</summary>

	<returns>
		Person Id.
	</returns>
	<remarks>
		
	</remarks>
	<code>
		SELECT [dbo].[ufnFinancial_GetGivingLeader]( 3, 53 )
	</code>
</doc>
*/

CREATE FUNCTION [dbo].[ufnFinancial_GetGivingLeader]( @PersonId int, @GivingGroupId int ) 

RETURNS int AS

BEGIN
	
	DECLARE @GivingLeaderPersonId int = @PersonId

	IF @GivingGroupId IS NOT NULL 
	BEGIN
		SET @GivingLeaderPersonId = (
			SELECT TOP 1 p.[Id]
			FROM [GroupMember] gm
			INNER JOIN [GroupTypeRole] r on r.[Id] = gm.[GroupRoleId]
			INNER JOIN [Person] p ON p.[Id] = gm.[PersonId]
			WHERE gm.[GroupId] = @GivingGroupId
          AND p.[IsDeceased] = 0
			ORDER BY r.[Order], p.[Gender], p.[BirthYear], p.[BirthMonth], p.[BirthDay]
		)
	END

	RETURN COALESCE( @GivingLeaderPersonId, @PersonId )

END
" );

            Sql( @"
ALTER TABLE PERSON ADD [GivingLeaderId] as (
	[dbo].[ufnFinancial_GetGivingLeader] ( [Id], [GivingGroupId] )
)
" );

            RockMigrationHelper.AddPage( "18C9E5C3-3E28-4AA3-84F6-78CD4EA2DD3C", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Giving Analytics", "", "D34B3916-1ABD-4F16-B820-5AAAA761F77F", "", "F4DF4899-2D44-4997-BA9B-9D2C64958A20" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Giving Analysis", "Shows a graph of giving statistics which can be configured for specific date range, amounts, currency types, campus, etc.", "~/Blocks/Finance/GivingAnalytics.ascx", "Finance", "48E4225F-8948-4FB0-8F00-1B43D3D9B3C3" );

            // Add Block to Page: Giving Analytics, Site: Rock RMS
            RockMigrationHelper.AddBlock( "D34B3916-1ABD-4F16-B820-5AAAA761F77F", "", "48E4225F-8948-4FB0-8F00-1B43D3D9B3C3", "Giving Analysis", "Main", "", "", 0, "784C58EF-B1B8-4237-BF12-E04DE8271A5A" );

            // Attrib for BlockType: Giving Analysis:Chart Style
            RockMigrationHelper.AddBlockTypeAttribute( "48E4225F-8948-4FB0-8F00-1B43D3D9B3C3", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Chart Style", "ChartStyle", "", "", 0, @"2ABB2EA0-B551-476C-8F6B-478CD08C2227", "5B40D794-DDE8-492F-99E2-D77F0ABDF558" );
            // Attrib for BlockType: Giving Analysis:Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "48E4225F-8948-4FB0-8F00-1B43D3D9B3C3", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "Select the page to navigate to when the chart is clicked", 0, @"", "2686B03C-CE03-44FD-AE01-39FFB0A934AE" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Giving Analysis:Detail Page
            RockMigrationHelper.DeleteAttribute( "2686B03C-CE03-44FD-AE01-39FFB0A934AE" );
            // Attrib for BlockType: Giving Analysis:Chart Style
            RockMigrationHelper.DeleteAttribute( "5B40D794-DDE8-492F-99E2-D77F0ABDF558" );
            // Remove Block: Giving Analysis, from Page: Giving Analytics, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "784C58EF-B1B8-4237-BF12-E04DE8271A5A" );
            RockMigrationHelper.DeleteBlockType( "48E4225F-8948-4FB0-8F00-1B43D3D9B3C3" ); // Giving Analysis
            RockMigrationHelper.DeletePage( "D34B3916-1ABD-4F16-B820-5AAAA761F77F" ); //  Page: Giving Analytics, Layout: Full Width, Site: Rock RMS

            DropColumn( "dbo.Person", "GivingLeaderId" );
        }
    }
}
