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
    public partial class PersonMarketingBinaryFileTypes : Rock.Migrations.RockMigration1
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // add new BinaryFileTypes for Person and Marketing Campaign Images (both are AllowCaching=True)
            Sql( @"
INSERT INTO [BinaryFileType]
           ([IsSystem]
           ,[Name]
           ,[Description]
           ,[IconCssClass]
           ,[StorageEntityTypeId]
           ,[AllowCaching]
           ,[Guid])
     VALUES
           (1
           ,'Person Image'
           ,'Image of a Person'
           ,'fa fa-camera'
           ,51
           ,1
           ,'03BD8476-8A9F-4078-B628-5B538F967AFC')

INSERT INTO [BinaryFileType]
           ([IsSystem]
           ,[Name]
           ,[Description]
           ,[IconCssClass]
           ,[StorageEntityTypeId]
           ,[AllowCaching]
           ,[Guid])
     VALUES
           (1
           ,'Marketing Campaign Ad Image'
           ,'Image used for Marketing Campaign Ads'
           ,'fa fa-bullhorn'
           ,51
           ,1
           ,'8DBF874C-F3C2-4848-8137-C963C431EB0B')
" );

            // Update BinaryFile BinaryFileType for Person and MarketingCampaign Images
            Sql( @"
declare
@binaryFileTypePersonImage int,
@binaryFileTypeMarketingCampaignAdImage int

select @binaryFileTypePersonImage = [Id] from [BinaryFileType] where [Guid] = '03BD8476-8A9F-4078-B628-5B538F967AFC';
select @binaryFileTypeMarketingCampaignAdImage = [Id] from [BinaryFileType] where [Guid] = '8DBF874C-F3C2-4848-8137-C963C431EB0B';

update [BinaryFile] set [BinaryFileTypeId] = @binaryFileTypePersonImage where [Id] in (select [PhotoId] from [Person] where [PhotoId] is not null)

update [BinaryFile] set [BinaryFileTypeId] = @binaryFileTypeMarketingCampaignAdImage where [Id] in (
select [av].[Value]
from [Attribute] [a]
join [FieldType] [ft] on [ft].[Id] = [a].[FieldTypeId]
join [AttributeValue] [av] on [av].[AttributeId] = [a].[Id]
join [MarketingCampaignAd] [mca] on [mca].[Id] = [av].[EntityId]
where [ft].[Guid] = '97F8157D-A8C8-4AB3-96A2-9CB2A9049E6D' /*ImageFieldType*/
and [a].[EntityTypeId] = (select Id from EntityType where Name = 'Rock.Model.MarketingCampaignAd')
)
" );

            // Update MarketingCampaignAd Image Attributes configuration values for BinaryFileType
            Sql( @"
declare
@binaryFileTypeMarketingCampaignAdImage int

-- delete the AttributeQualifier just in case it is already there
delete from AttributeQualifier where AttributeId in (
select a.Id
    from [Attribute] [a]
    join [FieldType] [ft] on [ft].[Id] = [a].[FieldTypeId]
    where [ft].[Guid] = '97F8157D-A8C8-4AB3-96A2-9CB2A9049E6D' /*ImageFieldType*/
    and [a].[EntityTypeId] = (select Id from EntityType where Name = 'Rock.Model.MarketingCampaignAd')
) and [Key] = 'binaryFileType'

select @binaryFileTypeMarketingCampaignAdImage = [Id] from [BinaryFileType] where [Guid] = '8DBF874C-F3C2-4848-8137-C963C431EB0B';

-- Add an AttributeQualifier of BinaryFileType=@binaryFileTypeMarketingCampaignAdImage for MarketingCampaignAd Image attributes 
insert into [AttributeQualifier] ([IsSystem], [AttributeId], [Key], [Value], [Guid])
select 1, a.Id, 'binaryFileType', @binaryFileTypeMarketingCampaignAdImage, NEWID()
    from [Attribute] [a]
    join [FieldType] [ft] on [ft].[Id] = [a].[FieldTypeId]
    where [ft].[Guid] = '97F8157D-A8C8-4AB3-96A2-9CB2A9049E6D' /*ImageFieldType*/
    and [a].[EntityTypeId] = (select Id from EntityType where Name = 'Rock.Model.MarketingCampaignAd')
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove MarketingCampaignAd Image Attributes configuration values for BinaryFileType
            Sql( @"
delete from AttributeQualifier where AttributeId in (
select a.Id
    from [Attribute] [a]
    join [FieldType] [ft] on [ft].[Id] = [a].[FieldTypeId]
    where [ft].[Guid] = '97F8157D-A8C8-4AB3-96A2-9CB2A9049E6D' /*ImageFieldType*/
    and [a].[EntityTypeId] = (select Id from EntityType where Name = 'Rock.Model.MarketingCampaignAd')
) and [Key] = 'binaryFileType'
" );

            // Update BinaryFile BinaryFileType back to Default for Person and MarketingCampaign Images
            Sql( @"
declare
@binaryFileTypeDefault int,
@binaryFileTypePersonImage int,
@binaryFileTypeMarketingCampaignAdImage int

select @binaryFileTypeDefault = [Id] from [BinaryFileType] where [Guid] = 'C1142570-8CD6-4A20-83B1-ACB47C1CD377';
select @binaryFileTypePersonImage = [Id] from [BinaryFileType] where [Guid] = '03BD8476-8A9F-4078-B628-5B538F967AFC';
select @binaryFileTypeMarketingCampaignAdImage = [Id] from [BinaryFileType] where [Guid] = '8DBF874C-F3C2-4848-8137-C963C431EB0B';

update [BinaryFile] set [BinaryFileTypeId] = @binaryFileTypeDefault where BinaryFileTypeId = @binaryFileTypePersonImage;
update [BinaryFile] set [BinaryFileTypeId] = @binaryFileTypeDefault where BinaryFileTypeId = @binaryFileTypeMarketingCampaignAdImage;

" );

            // delete BinaryFileTypes for Person and Marketing Campaign Images
            Sql( @"delete from BinaryFileType where Guid in ('03BD8476-8A9F-4078-B628-5B538F967AFC', '8DBF874C-F3C2-4848-8137-C963C431EB0B')" );
        }
    }
}
