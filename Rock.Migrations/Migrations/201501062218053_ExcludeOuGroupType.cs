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
    public partial class ExcludeOuGroupType : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // exclude ou group type from group viewer
            RockMigrationHelper.AddBlockTypeAttribute( "2D26A2C4-62DC-4680-8219-A52EB2BC0F65", "F725B854-A15E-46AE-9D4C-0608D4154F1E", "Group Types Exclude", "GroupTypesExclude", "", "Select group types to exclude from this block.", 3, "", "D8EEB91B-745E-4D63-911B-728D8F1B0B6E" );
            RockMigrationHelper.AddBlockAttributeValue( "95612FCE-C40B-4CBB-AE26-800B52BE5FCD", "D8EEB91B-745E-4D63-911B-728D8F1B0B6E", "aab2e9f4-e828-4fee-8467-73dc9dab784c", true );

            // Rename the Defined Type 'Liquid Templates' -> 'Lava Templates
            Sql( @"  UPDATE [DefinedType] SET [Name] = 'Lava Templates' WHERE [Guid] = 'C3D44004-6951-44D9-8560-8567D705A48B'" );

            // Fix checkin label merge fields
            Sql( @"
    DECLARE @AttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'CE57450F-634A-420A-BF5A-B43E9B20ABF2' )
	IF @AttributeId IS NOT NULL
	BEGIN

		UPDATE [AttributeValue]
		SET [Value] = REPLACE([Value], '|Notes:^|', '')
		WHERE [AttributeId] = @AttributeId
		AND [Value] LIKE '%|Notes:^|%'

		UPDATE [AttributeValue]
		SET [Value] = REPLACE([Value], '|Child Pick-up Receipt^|up your child. If you lose this please see the area director.^|', '')
		WHERE [AttributeId] = @AttributeId
		AND [Value] LIKE '%|Child Pick-up Receipt^|up your child. If you lose this please see the area director.^|%'

		DECLARE @ChildLabelIconId int = ( SELECT TOP 1 [Id] FROM [BinaryFile] WHERE [Guid] = 'C2E6B0A0-3991-4FAF-9E2F-49CF321CBB0D' )
		IF @ChildLabelIconId IS NOT NULL
		BEGIN

			DECLARE @BirthdayIconValueId int = ( SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = 'C6AA76B5-3E7F-4E14-905E-36173E60949D' ) 
			IF @BirthdayIconValueId IS NOT NULL
			BEGIN
				UPDATE [AttributeValue]
				SET [Value] = REPLACE([Value], '|2^' + CAST(@BirthdayIconValueId AS VARCHAR), '|B^' + CAST(@BirthdayIconValueId AS VARCHAR))
				WHERE [AttributeId] = @AttributeId
				AND [EntityId] = @ChildLabelIconId
				AND [Value] LIKE '%|2^' + CAST(@BirthdayIconValueId AS VARCHAR) + '%'
			END

			DECLARE @FirsttimeIconValueId int = ( SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = 'BD1B1FEA-D4A9-45EB-9958-01EF75D5A949' ) 
			IF @FirsttimeIconValueId IS NOT NULL
			BEGIN
				UPDATE [AttributeValue]
				SET [Value] = REPLACE([Value], '|3^' + CAST(@FirsttimeIconValueId AS VARCHAR), '|F^' + CAST(@FirsttimeIconValueId AS VARCHAR))
				WHERE [AttributeId] = @AttributeId
				AND [EntityId] = @ChildLabelIconId
				AND [Value] LIKE '%|3^' + CAST(@FirsttimeIconValueId AS VARCHAR) + '%'
			END

		END
    END
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
