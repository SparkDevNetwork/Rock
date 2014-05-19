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
namespace com.ccvonline.CommandCenter.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class RecordingControl : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddPage( "1AFDA740-8119-45B8-AF4D-58856D469BE5", "Service Recordings", "", "Default", "2BAAF392-2FE6-4D83-B949-122E6B97E5BB", "" );
            AddPage( "2BAAF392-2FE6-4D83-B949-122E6B97E5BB", "Recording Details", "", "Default", "6E7ACDFC-0297-473E-8990-9C96CC49394C", "" );

            AddBlockType( "com .ccvonline - Command Center - Recording Detail", "", "~/Plugins/com.ccvonline/CommandCenter/RecordingDetail.ascx", "1A054FCC-2E0E-4AD1-BA36-21991DB479AB" );
            AddBlockType( "com .ccvonline - Command Center - Recording List", "", "~/Plugins/com.ccvonline/CommandCenter/RecordingList.ascx", "AF4EB7C5-9121-4765-BEF2-558499BD0D6C" );
            
            AddBlock( "2BAAF392-2FE6-4D83-B949-122E6B97E5BB", "AF4EB7C5-9121-4765-BEF2-558499BD0D6C", "Recording List", "", "Content", 0, "7591B01B-8F22-47E3-BEFB-076338A3F24A" );
            AddBlock( "6E7ACDFC-0297-473E-8990-9C96CC49394C", "1A054FCC-2E0E-4AD1-BA36-21991DB479AB", "Recording Detail", "", "Content", 0, "FF6657ED-7A19-4869-A887-32EF70F84EDB" );

            // Attrib for BlockType: com .ccvonline - Command Center - Recording List:Detail Page
            AddBlockTypeAttribute( "AF4EB7C5-9121-4765-BEF2-558499BD0D6C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPageGuid", "", "", 0, "", "D23CF945-F440-4821-8496-C35035E1C3FE" );
            
            // Attrib Value for Recording List:Detail Page
            AddBlockAttributeValue( "7591B01B-8F22-47E3-BEFB-076338A3F24A", "D23CF945-F440-4821-8496-C35035E1C3FE", "6e7acdfc-0297-473e-8990-9c96cc49394c" );

            UpdateFieldType( "Accounts Field Type", "", "Rock", "Rock.Field.Types.AccountsFieldType", "CC009E89-CE40-42F6-9D7C-D117ADF8DCD0" );
            UpdateFieldType( "Category Field Type", "", "Rock", "Rock.Field.Types.CategoryFieldType", "AB6B4F30-F535-41E9-A4B6-63CE12C9C3CB" );
            
            // Create the Wowza Server Global Attribute and the Command Center attribute category
            Sql( string.Format( @"
    DECLARE @AttributeId INT
    DECLARE @CategoryId INT

    SET @AttributeId = ( SELECT [Id] FROM [Attribute] WHERE [Guid] = '{0}' )
    IF @AttributeId IS NULL
    BEGIN

        DECLARE @TextFieldType int
        SET @TextFieldType = ( SELECT [Id] FROM [FieldType] WHERE [Guid] = '{1}' )

        INSERT INTO [Attribute] ( [IsSystem], [FieldTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Description], [Order], [IsGridColumn], [IsMultiValue], [IsRequired], [Guid] )
        VALUES ( 1, @TextFieldType, '', '', 'ccvonlineWowzaServer', 'Wowza Server', 'Url of Wowza Server', 0, 0, 0, 0, '{0}' )
        SET @AttributeId = SCOPE_IDENTITY()

    END

    SET @CategoryId = ( SELECT [Id] FROM [Category] WHERE [Guid] = '{2}' )
    IF @CategoryId IS NULL
    BEGIN

	    DECLARE @AttributeEntityId INT
	    SET @AttributeEntityId = ( SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Attribute' )

        INSERT INTO [Category] ( [IsSystem], [EntityTypeId], [EntityTypeQualifierColumn], [Name], [Guid] )
        VALUES ( 1, @AttributeEntityId, 'EntityTypeId', 'Command Center', '{2}' )
        SET @CategoryId = SCOPE_IDENTITY()
        
    END

    IF NOT EXISTS (SELECT [AttributeId] FROM [AttributeCategory] WHERE [AttributeId] = @AttributeId AND [CategoryId] = @CategoryId)
    BEGIN
        
        INSERT INTO [AttributeCategory] ( [AttributeId], [CategoryId] )
        VALUES ( @AttributeId, @CategoryId )

    END
", com.ccvonline.SystemGuid.Attribute.WOWZA_SERVER_URL, Rock.SystemGuid.FieldType.TEXT, com.ccvonline.SystemGuid.Category.COMMAND_CENTER ) );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( string.Format( @"

    DELETE [Attribute] WHERE [Guid] = '{0}'
    DELETE [Category] WHERE [Guid] = '{1}'

", com.ccvonline.SystemGuid.Attribute.WOWZA_SERVER_URL, com.ccvonline.SystemGuid.Category.COMMAND_CENTER ) );

            DeleteAttribute( "D23CF945-F440-4821-8496-C35035E1C3FE" ); // Detail Page

            DeleteBlock( "FF6657ED-7A19-4869-A887-32EF70F84EDB" ); // Recording Detail
            DeleteBlock( "7591B01B-8F22-47E3-BEFB-076338A3F24A" ); // Recording List

            DeleteBlockType( "AF4EB7C5-9121-4765-BEF2-558499BD0D6C" ); // com .ccvonline - Command Center - Recording List
            DeleteBlockType( "1A054FCC-2E0E-4AD1-BA36-21991DB479AB" ); // com .ccvonline - Command Center - Recording Detail

            DeletePage( "6E7ACDFC-0297-473E-8990-9C96CC49394C" ); // Recording Details
            DeletePage( "2BAAF392-2FE6-4D83-B949-122E6B97E5BB" ); // Service Recordings
        }
    }
}
