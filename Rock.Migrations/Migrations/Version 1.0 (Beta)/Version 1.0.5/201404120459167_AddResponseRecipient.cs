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
    public partial class AddResponseRecipient : Rock.Migrations.RockMigration4
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

            // add location editor pages
            AddPage( "7F1F4130-CB98-473B-9DE1-7A886D2283ED", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Location Editor", "", "47BFA50A-68D8-4841-849B-75AB3E5BCD6D", "fa fa-building-o" ); // Site:Rock RMS
            AddPage( "47BFA50A-68D8-4841-849B-75AB3E5BCD6D", "195BCD57-1C10-4969-886F-7324B6287B75", "Location Detail", "", "1602C1CA-2EC7-4163-B0E1-1FE7306AC2B4", "" ); // Site:Rock RMS
            UpdateBlockType( "Location List", "Block for viewing a list of locations.", "~/Blocks/Core/LocationList.ascx", "Core", "5144ED5B-89A9-4D77-B0E5-695070BE0C8E" );

            // Add Block to Page: Location Editor, Site: Rock RMS
            AddBlock( "47BFA50A-68D8-4841-849B-75AB3E5BCD6D", "", "5144ED5B-89A9-4D77-B0E5-695070BE0C8E", "Location List", "Main", "", "", 0, "843137AA-7CA3-41F0-8AC7-5CEDCA391BD5" );

            // Add Block to Page: Location Detail, Site: Rock RMS
            AddBlock( "1602C1CA-2EC7-4163-B0E1-1FE7306AC2B4", "", "08189564-1245-48F8-86CC-560F4DD48733", "Location Detail", "Main", "", "", 0, "15C9F680-4C46-4962-94B1-BD1FF1B8F831" );

            // Attrib for BlockType: Location Detail:Map Style
            AddBlockTypeAttribute( "08189564-1245-48F8-86CC-560F4DD48733", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Map Style", "MapStyle", "", "The map theme that should be used for styling the GeoPicker map.", 0, @"FDC5D6BA-A818-4A06-96B1-9EF31B4087AC", "3475B7C5-3B57-4617-A1CA-CBACF20E7463" );

            // Attrib for BlockType: Location List:Detail Page
            AddBlockTypeAttribute( "5144ED5B-89A9-4D77-B0E5-695070BE0C8E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "8AF2347F-BC7B-4C9E-BB5E-25A34D1238C6" );

            // Attrib Value for Block:Location List, Attribute:Detail Page Page: Location Editor, Site: Rock RMS
            AddBlockAttributeValue( "843137AA-7CA3-41F0-8AC7-5CEDCA391BD5", "8AF2347F-BC7B-4C9E-BB5E-25A34D1238C6", @"1602c1ca-2ec7-4163-b0e1-1fe7306ac2b4" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"DELETE FROM [Attribute] WHERE [Guid] = 'E9E82709-5506-4339-8F6A-C2259329A71F'" );
            Sql( @"DELETE FROM [PageRoute] WHERE [Guid] = '61BAF008-56D1-4F61-8C42-9BB672580420'" );

            // Attrib for BlockType: Location List:Detail Page
            DeleteAttribute( "8AF2347F-BC7B-4C9E-BB5E-25A34D1238C6" );
            // Attrib for BlockType: Location Detail:Map Style
            DeleteAttribute( "3475B7C5-3B57-4617-A1CA-CBACF20E7463" );

            // Remove Block: Location Detail, from Page: Location Detail, Site: Rock RMS
            DeleteBlock( "15C9F680-4C46-4962-94B1-BD1FF1B8F831" );
            // Remove Block: Location List, from Page: Location Editor, Site: Rock RMS
            DeleteBlock( "843137AA-7CA3-41F0-8AC7-5CEDCA391BD5" );

            DeleteBlockType( "5144ED5B-89A9-4D77-B0E5-695070BE0C8E" ); // Location List

            DeletePage( "1602C1CA-2EC7-4163-B0E1-1FE7306AC2B4" ); // Page: Location DetailLayout: Full Width Panel, Site: Rock RMS
            DeletePage( "47BFA50A-68D8-4841-849B-75AB3E5BCD6D" ); // Page: Location EditorLayout: Full Width, Site: Rock RMS
        }
    }
}
