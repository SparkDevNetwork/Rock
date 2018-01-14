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
    public partial class AddedGroupConfigAndRegistationInstructions : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.GroupType", "AllowSpecificGroupMemberAttributes", c => c.Boolean(nullable: false));
            AddColumn("dbo.GroupType", "EnableSpecificGroupRequirements", c => c.Boolean(nullable: false));
            AddColumn("dbo.GroupType", "AllowGroupSync", c => c.Boolean(nullable: false));
            AddColumn("dbo.GroupType", "AllowSpecificGroupMemberWorkflows", c => c.Boolean(nullable: false));
            AddColumn("dbo.RegistrationInstance", "RegistrationInstructions", c => c.String());
            AddColumn("dbo.RegistrationTemplate", "RegistrationInstructions", c => c.String());
            Sql(
                @"DECLARE @securityRoleId INT, @groupMemberEntityType INT;
        SELECT @securityRoleId = [Id] FROM [GroupType] WHERE [Guid]='AECE949F-704C-483E-A4FB-93D5E4720C4C'

        SELECT @groupMemberEntityType = [Id] FROM [EntityType] WHERE [Guid]='49668B95-FEDC-43DD-8085-D2B0D6343C48'

        UPDATE 
	        [GroupType] 
        SET 
	        [AllowGroupSync] = 1 
        WHERE [Id] IN 
			        (SELECT Distinct [GroupTypeId] FROM [Group] WHERE [SyncDataViewId] IS NOT NULL) OR
			        [Id] IN (SELECT [Id] FROM [GroupType] WHERE [InheritedGroupTypeId]=@securityRoleId ) OR 
			        [Id]=@securityRoleId

        UPDATE 
	        [GroupType] 
        SET 
	        [EnableSpecificGroupRequirements] = 1 
        WHERE 
	        [Id] IN  (
	        SELECT A.[GroupTypeId] FROM [Group] A INNER JOIN [GroupRequirement] B ON A.[Id]=B.[GroupId]
	        ) 

        UPDATE 
	        [GroupType]
        SET 
	        [AllowSpecificGroupMemberAttributes] = 1 
        WHERE 
	        [Id] IN  (
	        SELECT A.[GroupTypeId]
	        FROM 
		        [Group] A INNER JOIN 
		        [Attribute] B  
	        ON 
		        A.[Id] = CONVERT(INT,B.[EntityTypeQualifierValue]) 
	        WHERE 
		        B.[EntityTypeQualifierColumn]='GroupId' AND
		        B.[EntityTypeId]=@groupMemberEntityType) 



        UPDATE 
	        [GroupType] 
        SET 
	        [AllowSpecificGroupMemberWorkflows] = 1 
        WHERE 
	        [Id] IN  (
	        SELECT A.[GroupTypeId] FROM [Group] A INNER JOIN [GroupMemberWorkflowTrigger] B ON A.[Id]=B.[GroupId]
        ) 
");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.RegistrationTemplate", "RegistrationInstructions");
            DropColumn("dbo.RegistrationInstance", "RegistrationInstructions");
            DropColumn("dbo.GroupType", "AllowSpecificGroupMemberWorkflows");
            DropColumn("dbo.GroupType", "AllowGroupSync");
            DropColumn("dbo.GroupType", "EnableSpecificGroupRequirements");
            DropColumn("dbo.GroupType", "AllowSpecificGroupMemberAttributes");
        }
    }
}
