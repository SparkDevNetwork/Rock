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
    public partial class AddResponseRecipient : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"DECLARE @SmsDefinedTypeEntityTypeId int = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = '611BDE1F-7405-4D16-8626-CCFEDB0E62BE')
DECLARE @DefinedValueEntityTypeId int = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.DefinedValue')
DECLARE @PersonFieldType int = (SELECT [Id] FROM [FieldType] WHERE [Guid] = 'E4EAB7B2-0B76-429B-AFE4-AD86D7428C70')

INSERT INTO [dbo].[Attribute]
           ([IsSystem]
           ,[FieldTypeId]
           ,[EntityTypeId]
           ,[EntityTypeQualifierColumn]
           ,[EntityTypeQualifierValue]
           ,[Key]
           ,[Name]
           ,[Description]
           ,[Order]
           ,[IsGridColumn]
           ,[DefaultValue]
           ,[IsMultiValue]
           ,[IsRequired]
           ,[Guid])
     VALUES
           (1
           ,@PersonFieldType
           ,@DefinedValueEntityTypeId
           ,'DefinedTypeId'
           ,@SmsDefinedTypeEntityTypeId
           ,'ResponseRecipient'
           ,'Response Recipient'
		   ,'The person who should receive responses to the SMS number. This person must have a phone number with SMS enabled or no response will be sent.'
           ,24
           ,1
           ,null
           ,0
           ,1
           ,'E9E82709-5506-4339-8F6A-C2259329A71F')" );


            // move the layout for the login status
            Sql( @" UPDATE [Block]
  SET [Zone] = 'Login'
  WHERE [LayoutId] IN 
			(SELECT [Id] FROM [Layout] WHERE [Guid] IN ('F66758C6-3E3D-4598-AF4C-B317047B5987', 'D65F783D-87A9-4CC9-8110-E83466A0EADB', '195BCD57-1C10-4969-886F-7324B6287B75', '0CB60906-6B74-44FD-AB25-026050EF70EB','6AC471A3-9B0E-459B-ADA2-F6E18F970803', '22D220B5-0D34-429A-B9E3-59D80AE423E7', 'BACA6FF2-A228-4C47-9577-2BBDFDFD26BA', 'EDFE06F4-D329-4340-ACD8-68A60CD112E6'))
		AND [BlockTypeId] = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '04712F3D-9667-4901-A49D-4507573EF7AD')" );

            // add page route for communication
            Sql( @"  DECLARE @PageId int = (SELECT [Id] FROM [Page] WHERE [Guid] = '2A22D08D-73A8-4AAF-AC7E-220E8B2E7857')
  INSERT INTO [PageRoute] ([IsSystem], [PageId], [Route], [Guid])
  VALUES
	(1, @PageId, 'Communication', '61BAF008-56D1-4F61-8C42-9BB672580420')" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"DELETE FROM [Attribute] WHERE [Guid] = 'E9E82709-5506-4339-8F6A-C2259329A71F'" );
            Sql( @"DELETE FROM [PageRoute] WHERE [Guid] = '61BAF008-56D1-4F61-8C42-9BB672580420'" );
        }
    }
}
