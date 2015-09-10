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
    using Rock.Security;
    
    /// <summary>
    ///
    /// </summary>
    public partial class ExternalGivingPageCleanup : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"DECLARE @LayoutId int = (SELECT TOP 1 [Id] FROM [Layout] WHERE [Guid] = '325B7BFD-8B80-44FD-A951-4E4763DA6C0D')

              UPDATE [Page]
              SET [LayoutId] = @LayoutId
              WHERE [Guid] = 'D0ADBAF0-3DC9-4123-BEE3-4E8E34BCD520'" );

            // change order of current block
            Sql( @"  UPDATE [Block]
	            SET [Order] = 1
	            WHERE [Guid] = '4DA02EC9-75EA-4031-AC1F-79805638E4FD'" );

            // add nav
            RockMigrationHelper.AddBlock( "D0ADBAF0-3DC9-4123-BEE3-4E8E34BCD520", "", "CACB9D1A-A820-4587-986A-D66A69EE9948", "Page Menu", "Sidebar1", "", "", 0, "F3D9D85B-DB57-2A8A-4E60-4CBE3577DC80" );

            // add html content
            RockMigrationHelper.AddBlock( "D0ADBAF0-3DC9-4123-BEE3-4E8E34BCD520", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "Saved Payment Accounts Intro Text", "Main", "", "", 0, "D51881E8-4B93-2496-4D90-24C2EAB792EC" );

            // add html content
            Sql( @"DECLARE @PaymentContentBlock int
SET @PaymentContentBlock = (SELECT TOP 1 [ID] FROM [Block] WHERE [Guid] = 'D51881E8-4B93-2496-4D90-24C2EAB792EC')

INSERT INTO [dbo].[HtmlContent]
           ([BlockId]
           ,[Version]
           ,[Content]
           ,[IsApproved]
           ,[ApprovedDateTime]
           ,[StartDateTime]
           ,[ExpireDateTime]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId])
     VALUES
           (@PaymentContentBlock
           ,1
           ,'<p>Below are payment accounts that you have saved in the past. Click the red ''x'' to delete these saved accounts.</p> <hr />'
           ,1
           ,getdate()
           ,null
           ,null
           ,'D8EC4715-598B-36A4-4A63-A3AAE271185E'
           ,getdate()
           ,getdate()
           ,null
           ,null)" );



            // Attrib Value for Block:Page Menu, Attribute:CSS File Page: View My Giving, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "F3D9D85B-DB57-2A8A-4E60-4CBE3577DC80", "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22", @"" );

            // Attrib Value for Block:Page Menu, Attribute:Enable Debug Page: View My Giving, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "F3D9D85B-DB57-2A8A-4E60-4CBE3577DC80", "2EF904CD-976E-4489-8C18-9BA43885ACD9", @"False" );

            // Attrib Value for Block:Page Menu, Attribute:Include Current Parameters Page: View My Giving, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "F3D9D85B-DB57-2A8A-4E60-4CBE3577DC80", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"False" );

            // Attrib Value for Block:Page Menu, Attribute:Include Current QueryString Page: View My Giving, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "F3D9D85B-DB57-2A8A-4E60-4CBE3577DC80", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" );

            // Attrib Value for Block:Page Menu, Attribute:Is Secondary Block Page: View My Giving, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "F3D9D85B-DB57-2A8A-4E60-4CBE3577DC80", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" );

            // Attrib Value for Block:Page Menu, Attribute:Number of Levels Page: View My Giving, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "F3D9D85B-DB57-2A8A-4E60-4CBE3577DC80", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"3" );

            // Attrib Value for Block:Page Menu, Attribute:Root Page Page: View My Giving, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "F3D9D85B-DB57-2A8A-4E60-4CBE3577DC80", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"8bb303af-743c-49dc-a7ff-cc1236b4b1d9" );

            // Attrib Value for Block:Page Menu, Attribute:Template Page: View My Giving, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "F3D9D85B-DB57-2A8A-4E60-4CBE3577DC80", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageSubNav.lava'  %}" );

            // fix giving leader function
            Sql( @"ALTER TABLE PERSON DROP COLUMN [GivingLeaderId] 
                    GO

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

                    ALTER FUNCTION [dbo].[ufnFinancial_GetGivingLeader]( @PersonId int, @GivingGroupId int ) 

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
			                    AND p.[GivingGroupId] = @GivingGroupId
			                    ORDER BY r.[Order], p.[Gender], p.[BirthYear], p.[BirthMonth], p.[BirthDay]
		                    )
	                    END

	                    RETURN COALESCE( @GivingLeaderPersonId, @PersonId )

                    END
                    GO

                    ALTER TABLE PERSON ADD [GivingLeaderId] as (
	                    [dbo].[ufnFinancial_GetGivingLeader] ( [Id], [GivingGroupId] )
                    )
                    GO" );

            // Fix description of Person Search Page
            Sql( @"Update [Page] set [Description] = 'Displays person search results', [PageDisplayDescription] = 0 where [Guid] = '5e036ade-c2a4-4988-b393-dac58230f02e' and [Description] = 'Screen to administrate campuses.'" );

            // fix security on benevolence security page
            // add security to the person profile benevolence pages
            RockMigrationHelper.AddSecurityAuthForPage( Rock.SystemGuid.Page.BENEVOLENCE_PERSON_PAGES, 0, Authorization.VIEW, true, Rock.SystemGuid.Group.GROUP_BENEVOLENCE, 0, "1E737BCB-1776-019F-4593-731275BE8A53" );
            RockMigrationHelper.AddSecurityAuthForPage( Rock.SystemGuid.Page.BENEVOLENCE_PERSON_PAGES, 1, Authorization.VIEW, true, Rock.SystemGuid.Group.GROUP_ADMINISTRATORS, 0, "4CFDB157-AE82-EEAB-4BF4-480396A957D4" );
            RockMigrationHelper.AddSecurityAuthForPage( Rock.SystemGuid.Page.BENEVOLENCE_PERSON_PAGES, 2, Authorization.VIEW, false, null, Rock.Model.SpecialRole.AllUsers.ConvertToInt(), "8C0CCA16-2314-FAB2-455A-3AA459052320" );

            // add security to the benevolence block on the person profile pages
            RockMigrationHelper.AddSecurityAuthForBlock( "52EDDE7F-6808-4912-A73B-94AE0939DD48", 0, Authorization.EDIT, true, Rock.SystemGuid.Group.GROUP_BENEVOLENCE, 0, "E61ADD2C-302C-5398-4DFD-0C1782C02411" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"DELETE FROM [HtmlContent] WHERE [Guid] = 'D8EC4715-598B-36A4-4A63-A3AAE271185E'" );
            
            RockMigrationHelper.DeleteBlock( "F3D9D85B-DB57-2A8A-4E60-4CBE3577DC80" );
            RockMigrationHelper.DeleteBlock( "D51881E8-4B93-2496-4D90-24C2EAB792EC" );

            RockMigrationHelper.DeleteSecurityAuth( "1E737BCB-1776-019F-4593-731275BE8A53" );
            RockMigrationHelper.DeleteSecurityAuth( "4CFDB157-AE82-EEAB-4BF4-480396A957D4" );
            RockMigrationHelper.DeleteSecurityAuth( "8C0CCA16-2314-FAB2-455A-3AA459052320" );
            RockMigrationHelper.DeleteSecurityAuth( "E61ADD2C-302C-5398-4DFD-0C1782C02411" );
        }
    }
}
