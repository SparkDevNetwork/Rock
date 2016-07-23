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
    public partial class AddPersonLinkSelect : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateEntityType( "Rock.Reporting.DataSelect.Person.PersonLinkSelect", "Person Link Select", "Rock.Reporting.DataSelect.Person.PersonLinkSelect, Rock, Version=1.0.12.0, Culture=neutral, PublicKeyToken=null", false, true, "6301F6B4-B2EF-469A-8EC2-7D5F06B55C60" );

            Sql( @"
DECLARE @PendingIndividualReportId INT = (
        SELECT TOP 1 [Id]
        FROM [Report]
        WHERE [Guid] = '4E3ECAE0-9D36-4C22-994D-AD31DE0F6FB7'
        )
        ,@SelfInactivatedReportId INT = (
        SELECT TOP 1 [Id]
        FROM [Report]
        WHERE [Guid] = '87D3E118-ADA8-4424-B63B-9482A7D9E609'
        )
        ,@PersonLinkDataSelectComponentEntityTypeId INT = (
        SELECT TOP 1 [Id]
        FROM [EntityType] where [Guid] = '6301F6B4-B2EF-469A-8EC2-7D5F06B55C60'
        )

-- LastName field for Pending Individuals report
DELETE from [ReportField] where [Guid] = 'B6DC8215-B7EF-4928-BCCA-DB35CC1C97F8'

-- NickName field for Pending Individuals report
DELETE from [ReportField] where [Guid] = '47AA0671-F1BF-4864-8007-A5DE9512EF0C'

-- NickName field for Self Inactivated report
DELETE from [ReportField] where [Guid] = 'c1b78a6c-eb40-4eaa-be75-c6de8c00629c'

-- LastName field for Self Inactivated report
DELETE from [ReportField] where [Guid] = '6113132a-bd29-4e25-8c6d-22bac0fd795b'

-- Name (Person Link) field for Pending Individual Report
INSERT INTO [dbo].[ReportField] (
        [ReportId]
        ,[ReportFieldType]
        ,[ShowInGrid]
        ,[DataSelectComponentEntityTypeId]
        ,[Selection]
        ,[Order]
        ,[Guid]
        ,[ColumnHeaderText]
        )
    VALUES (
        @PendingIndividualReportId
        ,2
        ,1
        ,@PersonLinkDataSelectComponentEntityTypeId
        ,'Name'
        ,0
        ,'A5543E82-373D-403B-92A1-7F513C93D254'
        ,'Name'
        )

-- Name (Person Link) field for Self Inactivated Report
INSERT INTO [dbo].[ReportField] (
        [ReportId]
        ,[ReportFieldType]
        ,[ShowInGrid]
        ,[DataSelectComponentEntityTypeId]
        ,[Selection]
        ,[Order]
        ,[Guid]
        ,[ColumnHeaderText]
        )
    VALUES (
        @SelfInactivatedReportId
        ,2
        ,1
        ,@PersonLinkDataSelectComponentEntityTypeId
        ,'Name'
        ,0
        ,'E369B151-B334-4BB0-84A0-CDD7E4A27DBC'
        ,'Name'
        )

" );

            #region Create UrlMask attributes for person history categories

            Sql( @"
    DECLARE @AttributeId int
    DECLARE @CategoryEntityTypeId int = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Category')
    DECLARE @HistoryEntityTypeId int = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.History')
    DECLARE @FieldTypeId int = (SELECT TOP 1 [Id] FROM [FieldType] WHERE [Guid] = '9C204CD0-1233-41C5-818A-C5DA439445AA')

    DELETE [Attribute] 
    WHERE [EntityTypeId] = @CategoryEntityTypeId
    AND [Key] = 'UrlMask'
    AND [EntityTypeQualifierColumn] = 'EntityTypeId'
    AND [EntityTypeQualifierValue] = CAST(@HistoryEntityTypeId as varchar)

    INSERT INTO [Attribute] (
        [IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],
        [Key],[Name],[Description],
        [Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],
        [Guid])
    VALUES(
        1,@FieldTypeId,@CategoryEntityTypeid,'EntityTypeId',CAST(@HistoryEntityTypeId as varchar),
        'UrlMask','Url Mask','The URL to use when displaying history items of this category type.  Any occurrence of {0} will be replaced with the RelatedEntityId from history record.',
        0,0,'',0,0,
        '0C405062-72BB-4362-9738-90C9ED5ACDDE')
    SET @AttributeId = SCOPE_IDENTITY()

    DECLARE @CommunicationCategoryId int = ( SELECT TOP 1 [Id] FROM [Category] WHERE [Guid] = 'F291034B-7581-48F3-B522-E31B8534D529' )
    IF @CommunicationCategoryId IS NOT NULL
    BEGIN
        INSERT INTO [AttributeValue] (
            [IsSystem],[AttributeId],[EntityId],[Value],[Guid])
        VALUES(
            1,@AttributeId,@CommunicationCategoryId,'~/Communication/{0}', NEWID())
    END

    DECLARE @GroupPageId int = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '4E237286-B715-4109-A578-C1445EC02707')
    DECLARE @GroupMemberCategoryId int = ( SELECT TOP 1 [Id] FROM [Category] WHERE [Guid] = '325278A4-FACA-4F38-A405-9C090B3BAA34' )
    IF @GroupPageId IS NOT NULL AND @GroupMemberCategoryId IS NOT NULL
    BEGIN
        INSERT INTO [AttributeValue] (
            [IsSystem],[AttributeId],[EntityId],[Value],[Guid])
        VALUES(
            1,@AttributeId,@GroupMemberCategoryId,'~/page/' + CAST(@GroupPageId as varchar) + '?GroupId={0}', NEWID())
    END
" );
            // update the photo request page route
            Sql( @"
    UPDATE [PageRoute]
    SET [Route] = 'PhotoRequest/Upload/{rckipid}'
    WHERE [Route] = 'PhotoRequest/Upload/{Person}'
");

            RockMigrationHelper.AddSecurityAuthForPage( "8559A9F1-C6A4-4945-B393-74F6706A8FA2", 0, "View", true, null, Model.SpecialRole.AllAuthenticatedUsers.ConvertToInt(), "2CDAA4DC-53B4-4A19-B168-9051E4E52DBC" );
            RockMigrationHelper.AddSecurityAuthForPage( "8559A9F1-C6A4-4945-B393-74F6706A8FA2", 1, "View", false, null, Model.SpecialRole.AllUsers.ConvertToInt(), "9658AFEE-2849-46BF-9EC8-5CF30B97EED8" );

            #endregion

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
DELETE FROM [ReportField] where [Guid] = 'A5543E82-373D-403B-92A1-7F513C93D254'
DELETE FROM [ReportField] where [Guid] = 'E369B151-B334-4BB0-84A0-CDD7E4A27DBC'
" );
        }
    }
}
