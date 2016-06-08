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
    public partial class AddForeignId : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.AttendanceCode", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.Attendance", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.PersonAlias", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.Person", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.DefinedValue", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.DefinedType", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.FieldType", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.Group", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.Campus", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.Location", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.GroupLocation", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.Schedule", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.Category", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.EntityType", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.Device", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.GroupType", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.GroupTypeRole", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.GroupMember", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.PhoneNumber", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.BinaryFile", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.BinaryFileType", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.BinaryFileData", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.UserLogin", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.AttributeQualifier", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.Attribute", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.AttributeValue", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.Audit", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.AuditDetail", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.Auth", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.Block", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.BlockType", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.Layout", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.Page", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.PageContext", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.PageRoute", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.Site", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.SiteDomain", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.CommunicationRecipient", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.CommunicationRecipientActivity", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.Communication", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.CommunicationTemplate", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.DataViewFilter", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.DataView", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.SystemEmail", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.ExceptionLog", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.FinancialAccount", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.FinancialBatch", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.FinancialTransaction", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.FinancialTransactionImage", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.FinancialTransactionRefund", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.FinancialScheduledTransaction", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.FinancialScheduledTransactionDetail", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.FinancialTransactionDetail", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.FinancialPersonBankAccount", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.FinancialPersonSavedAccount", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.FinancialPledge", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.Following", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.History", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.HtmlContent", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.MarketingCampaignAd", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.MarketingCampaign", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.MarketingCampaignAudience", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.MarketingCampaignCampus", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.MarketingCampaignAdType", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.Metric", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.MetricValue", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.Note", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.NoteType", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.PageView", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.PersonBadge", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.PersonViewed", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.PrayerRequest", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.ReportField", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.Report", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.RestAction", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.RestController", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.ServiceJob", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.ServiceLog", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.TaggedItem", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.Tag", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.WorkflowAction", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.WorkflowActionType", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.WorkflowActivityType", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.WorkflowType", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.WorkflowActivity", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.Workflow", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.WorkflowLog", "ForeignId", c => c.String(maxLength: 50));
            AddColumn("dbo.WorkflowTrigger", "ForeignId", c => c.String(maxLength: 50));

            DropForeignKey( "dbo.ReportField", "DataSelectComponentEntityTypeId", "dbo.EntityType" );
            DropIndex( "dbo.ReportField", new[] { "DataSelectComponentEntityTypeId" } );

            CreateIndex( "dbo.ReportField", "DataSelectComponentEntityTypeId" );
            AddForeignKey("dbo.ReportField", "DataSelectComponentEntityTypeId", "dbo.EntityType", "Id", cascadeDelete: true);
            
            Sql( @"
    UPDATE [Block] SET [Zone] = REPLACE([Zone],'BadgBar','BadgeBar')
    WHERE [Zone] LIKE 'BadgBar%'
" );
            Sql( @"
/*
<doc>
	<summary>
 		This procedure merges the data from the non-primary person to the primary person.  It
		is used when merging people in Rock and should never be used outside of that process. 
	</summary>

	<returns>
	</returns>
	<param name=""Old Id"" datatype=""int"">The person id of the non-primary Person being merged</param>
	<param name=""New Id"" datatype=""int"">The person id of the rimary Person being merged</param>
	<remarks>	
		Uses the following constants:
			* Group Type - Family: 790E3215-3B10-442B-AF69-616C0DCB998E
			* Group Role - Known Relationship Owner: 7BC6C12E-0CD1-4DFD-8D5B-1B35AE714C42
			* Group Role - Implied Relationship Owner: CB9A0E14-6FCF-4C07-A49A-D7873F45E196
	</remarks>
	<code>
	</code>
</doc>
*/

ALTER PROCEDURE [dbo].[spCrm_PersonMerge]
    @OldId int
	, @NewId int

AS
BEGIN

	DECLARE @OldGuid uniqueidentifier
	DECLARE @NewGuid uniqueidentifier

	SET @OldGuid = ( SELECT [Guid] FROM [Person] WHERE [Id] = @OldId )
	SET @NewGuid = ( SELECT [Guid] FROM [Person] WHERE [Id] = @NewId )

	IF @OldGuid IS NOT NULL AND @NewGuid IS NOT NULL
	BEGIN

		DECLARE @PersonEntityTypeId INT = ( SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Person' )
		DECLARE @PersonFieldTypeId INT = ( SELECT [Id] FROM [FieldType] WHERE [Class] = 'Rock.Field.Types.PersonFieldType' )

		-- Authorization
		-----------------------------------------------------------------------------------------------
		-- Update any authorizations associated to old person that do not already have a matching 
		-- authorization for the new person
		UPDATE AO
			SET [PersonId] = @NewId
		FROM [Auth] AO
			LEFT OUTER JOIN [Auth] AN
				ON AN.[PersonId] = @NewId
				AND AN.[EntityTypeId] = AO.[EntityTypeId]
				AND AN.[EntityId] = AO.[EntityId]
				AND AN.[Action] = AO.[Action]
				AND AN.[AllowOrDeny] = AO.[AllowOrDeny]
				AND AN.[SpecialRole] = AO.[SpecialRole]
		WHERE AO.[PersonId] = @OldId
			AND AN.[Id] IS NULL

		-- Delete any authorizations not updated to new person
		DELETE [Auth]
		WHERE [PersonId] = @OldId

		-- Category
		-----------------------------------------------------------------------------------------------
		-- Currently UI does not allow categorizing people, but if it does in the future, would need 
		-- to add script to handle merge


		-- Communication Recipient
		-----------------------------------------------------------------------------------------------
		-- Update any communication recipients associated to old person to the new person where the new
		-- person does not already have the recipient record
		UPDATE CRO
			SET [PersonId] = @NewId
		FROM [CommunicationRecipient] CRO
			LEFT OUTER JOIN [CommunicationRecipient] CRN
				ON CRN.[CommunicationId] = CRO.[CommunicationId]
				AND CRN.[PersonId] = @NewId
		WHERE CRO.[PersonId] = @OldId
			AND CRN.[Id] IS NULL

		-- Delete any remaining recipents that were not updated
		DELETE [CommunicationRecipient]
		WHERE [PersonId] = @OldId

		SELECT DISTINCT G.[ID]
		INTO #FamilyIDs
		FROM [GroupMember] GM
			INNER JOIN [Group] G ON G.[Id] = GM.[GroupId]
			INNER JOIN [GroupType] GT ON GT.[Id] = G.[GroupTypeId]
		WHERE GM.[PersonId] = @OldId
			AND GT.[Guid] = '790E3215-3B10-442B-AF69-616C0DCB998E'

		-- Move/Update Known Relationships
		EXEC [dbo].[spCrm_PersonMergeRelationships] @OldId, @NewId, '7BC6C12E-0CD1-4DFD-8D5B-1B35AE714C42'

		-- Move/Update Implied Relationships
		EXEC [dbo].[spCrm_PersonMergeRelationships] @OldId, @NewId, 'CB9A0E14-6FCF-4C07-A49A-D7873F45E196'

		-- Group Member
		-----------------------------------------------------------------------------------------------
		-- Update any group members associated to old person to the new person where the new is not 
		-- already in the group with the same role
		UPDATE GMO
			SET [PersonId] = @NewId
		FROM [GroupMember] GMO
			INNER JOIN [GroupTypeRole] GTR
				ON GTR.[Id] = GMO.[GroupRoleId]
			LEFT OUTER JOIN [GroupMember] GMN
				ON GMN.[GroupId] = GMO.[GroupId]
				AND GMN.[PersonId] = @NewId
				AND (GTR.[MaxCount] <= 1 OR GMN.[GroupRoleId] = GMO.[GroupRoleId])
		WHERE GMO.[PersonId] = @OldId
			AND GMN.[Id] IS NULL

		-- Delete any group members not updated (already existed with new id)
		DELETE [GroupMember]
		WHERE [PersonId] = @OldId
		
		-- Note
		-----------------------------------------------------------------------------------------------
		-- Update any note that is associated to the old person to be associated to the new person
		UPDATE N
			SET [EntityId] = @NewId
		FROM [NoteType] NT
			INNER JOIN [Note] N
				ON N.[NoteTypeId] = NT.[Id]
				AND N.[EntityId] = @OldId
		WHERE NT.[EntityTypeId] = @PersonEntityTypeId


		-- History
		-----------------------------------------------------------------------------------------------
		-- Update any history that is associated to the old person to be associated to the new person
		UPDATE [History] SET [EntityId] = @NewId
		WHERE [EntityTypeId] = @PersonEntityTypeId
		AND [EntityId] = @OldId

		-- Tags
		-----------------------------------------------------------------------------------------------
		-- Update any tags associated to the old person to be associated to the new person as long as 
		-- same tag does not already exist for new person
		UPDATE TIO
			SET [EntityGuid] = @NewGuid
		FROM [Tag] T
			INNER JOIN [TaggedItem] TIO
				ON TIO.[TagId] = T.[Id]
				AND TIO.[EntityGuid] = @OldGuid
			LEFT OUTER JOIN [TaggedItem] TIN
				ON TIN.[TagId] = T.[Id]
				AND TIN.[EntityGuid] = @NewGuid
		WHERE T.[EntityTypeId] = @PersonEntityTypeId
			AND TIN.[Id] IS NULL

		-- Delete any tagged items still associated with old person (new person had same tag)
		DELETE TIO
		FROM [Tag] T
			INNER JOIN [TaggedItem] TIO
				ON TIO.[TagId] = T.[Id]
				AND TIO.[EntityGuid] = @OldGuid
		WHERE T.[EntityTypeId] = @PersonEntityTypeId

		-- If old person and new person have tags with the same name for the same entity type,
		-- update the old person's tagged items to use the new person's tag
		UPDATE TIO
			SET [TagId] = TIN.[Id]
		FROM [Tag] T
			INNER JOIN [Tag] TN
				ON TN.[EntityTypeId] = T.[EntityTypeId]
				AND TN.[EntityTypeQualifierColumn] = T.[EntityTypeQualifierColumn]
				AND TN.[EntityTypeQualifierValue] = T.[EntityTypeQualifierValue]
				AND TN.[Name] = T.[Name]
				AND TN.[OwnerId] = @NewId
			INNER JOIN [TaggedItem] TIO
				ON TIO.[TagId] = T.[Id]
			LEFT OUTER JOIN [TaggedItem] TIN
				ON TIN.[TagId] = TN.[Id]
		WHERE T.[OwnerId] = @OldId
			AND TIN.[Id] IS NULL

		-- Delete any of the old person's tags that have the same name and are associated to same 
		-- entity type as a tag used bo the new person
		DELETE T
		FROM [Tag] T
			INNER JOIN [Tag] TN
				ON TN.[EntityTypeId] = T.[EntityTypeId]
				AND TN.[EntityTypeQualifierColumn] = T.[EntityTypeQualifierColumn]
				AND TN.[EntityTypeQualifierValue] = T.[EntityTypeQualifierValue]
				AND TN.[Name] = T.[Name]
				AND TN.[OwnerId] = @NewId
		WHERE T.[OwnerId] = @OldId


		-- Remaining Tables
		-----------------------------------------------------------------------------------------------
		-- Update any column on any table that has a foreign key relationship to the Person table's Id
		-- column  

		DECLARE @Sql varchar(max)

		DECLARE ForeignKeyCursor INSENSITIVE CURSOR FOR
		SELECT 
			' UPDATE ' + tso.name +
			' SET ' + tac.name + ' = ' + CAST(@NewId as varchar) +
			' WHERE ' + tac.name + ' = ' + CAST(@OldId as varchar) 
		FROM sys.foreign_key_columns kc
			INNER JOIN sys.foreign_keys k ON kc.constraint_object_id = k.object_id
			INNER JOIN sys.all_objects so ON so.object_id = kc.referenced_object_id
			INNER JOIN sys.all_columns rac ON rac.column_id = kc.referenced_column_id AND rac.object_id = so.object_id
			INNER JOIN sys.all_objects tso ON tso.object_id = kc.parent_object_id
			INNER JOIN sys.all_columns tac ON tac.column_id = kc.parent_column_id AND tac.object_id = tso.object_id
		WHERE so.name = 'Person'
			AND rac.name = 'Id'
			AND tso.name NOT IN (
					'Auth'
				,'CommunicationRecipient'
				,'GroupMember'
				,'PhoneNumber'
			)

		OPEN ForeignKeyCursor

		FETCH NEXT
		FROM ForeignKeyCursor
		INTO @Sql

		WHILE (@@FETCH_STATUS <> -1)
		BEGIN

			IF (@@FETCH_STATUS = 0)
			BEGIN

				EXEC(@Sql)
			
			END
		
			FETCH NEXT
			FROM ForeignKeyCursor
			INTO @Sql

		END

		CLOSE ForeignKeyCursor
		DEALLOCATE ForeignKeyCursor


		-- Person
		-----------------------------------------------------------------------------------------------
		-- Delete the old person record.  By this time it should not have any relationships 
		-- with other tables 

		DELETE Person
		WHERE [Id] = @OldId

	END

END
" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.ReportField", "DataSelectComponentEntityTypeId", "dbo.EntityType");
            DropIndex("dbo.ReportField", new[] { "DataSelectComponentEntityTypeId" });

            CreateIndex( "dbo.ReportField", "DataSelectComponentEntityTypeId" );
            AddForeignKey( "dbo.ReportField", "DataSelectComponentEntityTypeId", "dbo.EntityType", "Id" );

            DropColumn("dbo.WorkflowTrigger", "ForeignId");
            DropColumn("dbo.WorkflowLog", "ForeignId");
            DropColumn("dbo.Workflow", "ForeignId");
            DropColumn("dbo.WorkflowActivity", "ForeignId");
            DropColumn("dbo.WorkflowType", "ForeignId");
            DropColumn("dbo.WorkflowActivityType", "ForeignId");
            DropColumn("dbo.WorkflowActionType", "ForeignId");
            DropColumn("dbo.WorkflowAction", "ForeignId");
            DropColumn("dbo.Tag", "ForeignId");
            DropColumn("dbo.TaggedItem", "ForeignId");
            DropColumn("dbo.ServiceLog", "ForeignId");
            DropColumn("dbo.ServiceJob", "ForeignId");
            DropColumn("dbo.RestController", "ForeignId");
            DropColumn("dbo.RestAction", "ForeignId");
            DropColumn("dbo.Report", "ForeignId");
            DropColumn("dbo.ReportField", "ForeignId");
            DropColumn("dbo.PrayerRequest", "ForeignId");
            DropColumn("dbo.PersonViewed", "ForeignId");
            DropColumn("dbo.PersonBadge", "ForeignId");
            DropColumn("dbo.PageView", "ForeignId");
            DropColumn("dbo.NoteType", "ForeignId");
            DropColumn("dbo.Note", "ForeignId");
            DropColumn("dbo.MetricValue", "ForeignId");
            DropColumn("dbo.Metric", "ForeignId");
            DropColumn("dbo.MarketingCampaignAdType", "ForeignId");
            DropColumn("dbo.MarketingCampaignCampus", "ForeignId");
            DropColumn("dbo.MarketingCampaignAudience", "ForeignId");
            DropColumn("dbo.MarketingCampaign", "ForeignId");
            DropColumn("dbo.MarketingCampaignAd", "ForeignId");
            DropColumn("dbo.HtmlContent", "ForeignId");
            DropColumn("dbo.History", "ForeignId");
            DropColumn("dbo.Following", "ForeignId");
            DropColumn("dbo.FinancialPledge", "ForeignId");
            DropColumn("dbo.FinancialPersonSavedAccount", "ForeignId");
            DropColumn("dbo.FinancialPersonBankAccount", "ForeignId");
            DropColumn("dbo.FinancialTransactionDetail", "ForeignId");
            DropColumn("dbo.FinancialScheduledTransactionDetail", "ForeignId");
            DropColumn("dbo.FinancialScheduledTransaction", "ForeignId");
            DropColumn("dbo.FinancialTransactionRefund", "ForeignId");
            DropColumn("dbo.FinancialTransactionImage", "ForeignId");
            DropColumn("dbo.FinancialTransaction", "ForeignId");
            DropColumn("dbo.FinancialBatch", "ForeignId");
            DropColumn("dbo.FinancialAccount", "ForeignId");
            DropColumn("dbo.ExceptionLog", "ForeignId");
            DropColumn("dbo.SystemEmail", "ForeignId");
            DropColumn("dbo.DataView", "ForeignId");
            DropColumn("dbo.DataViewFilter", "ForeignId");
            DropColumn("dbo.CommunicationTemplate", "ForeignId");
            DropColumn("dbo.Communication", "ForeignId");
            DropColumn("dbo.CommunicationRecipientActivity", "ForeignId");
            DropColumn("dbo.CommunicationRecipient", "ForeignId");
            DropColumn("dbo.SiteDomain", "ForeignId");
            DropColumn("dbo.Site", "ForeignId");
            DropColumn("dbo.PageRoute", "ForeignId");
            DropColumn("dbo.PageContext", "ForeignId");
            DropColumn("dbo.Page", "ForeignId");
            DropColumn("dbo.Layout", "ForeignId");
            DropColumn("dbo.BlockType", "ForeignId");
            DropColumn("dbo.Block", "ForeignId");
            DropColumn("dbo.Auth", "ForeignId");
            DropColumn("dbo.AuditDetail", "ForeignId");
            DropColumn("dbo.Audit", "ForeignId");
            DropColumn("dbo.AttributeValue", "ForeignId");
            DropColumn("dbo.Attribute", "ForeignId");
            DropColumn("dbo.AttributeQualifier", "ForeignId");
            DropColumn("dbo.UserLogin", "ForeignId");
            DropColumn("dbo.BinaryFileData", "ForeignId");
            DropColumn("dbo.BinaryFileType", "ForeignId");
            DropColumn("dbo.BinaryFile", "ForeignId");
            DropColumn("dbo.PhoneNumber", "ForeignId");
            DropColumn("dbo.GroupMember", "ForeignId");
            DropColumn("dbo.GroupTypeRole", "ForeignId");
            DropColumn("dbo.GroupType", "ForeignId");
            DropColumn("dbo.Device", "ForeignId");
            DropColumn("dbo.EntityType", "ForeignId");
            DropColumn("dbo.Category", "ForeignId");
            DropColumn("dbo.Schedule", "ForeignId");
            DropColumn("dbo.GroupLocation", "ForeignId");
            DropColumn("dbo.Location", "ForeignId");
            DropColumn("dbo.Campus", "ForeignId");
            DropColumn("dbo.Group", "ForeignId");
            DropColumn("dbo.FieldType", "ForeignId");
            DropColumn("dbo.DefinedType", "ForeignId");
            DropColumn("dbo.DefinedValue", "ForeignId");
            DropColumn("dbo.Person", "ForeignId");
            DropColumn("dbo.PersonAlias", "ForeignId");
            DropColumn("dbo.Attendance", "ForeignId");
            DropColumn("dbo.AttendanceCode", "ForeignId");
        }
    }
}
