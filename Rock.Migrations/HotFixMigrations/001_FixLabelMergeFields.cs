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

using Rock.Plugin;

namespace Rock.Migrations.HotFixMigrations
{
    /// <summary>
    /// 
    /// </summary>
    [MigrationNumber( 1, "1.2.0" )]
    public class FixLabelMergeFields : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // NOTE: This was moved to normal migration after being merged back to develop branch 
/*
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
*/
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
