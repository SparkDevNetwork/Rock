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
    public partial class Rollup_20251022 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
IF EXISTS (SELECT 1 FROM dbo.[Person] WHERE [Guid] = 'ad28da19-4af1-408f-9090-2672f8376f27')
BEGIN

    DECLARE @NewPersonGuid uniqueidentifier = NEWID()
    DECLARE @NewPersonAliasGuid uniqueidentifier = NEWID()

    UPDATE [Person]
    SET [Guid] = @NewPersonGuid
    WHERE [Guid] = 'ad28da19-4af1-408f-9090-2672f8376f27'

    DECLARE @PersonFieldTypeId int = (SELECT TOP 1 [Id] FROM [FieldType] WHERE [Guid] = 'E4EAB7B2-0B76-429B-AFE4-AD86D7428C70')

    -- Update built-in admin user person alias guid (if it still exists)
    UPDATE [PersonAlias]
    SET [Guid] = @NewPersonAliasGuid
        ,[AliasPersonGuid] = @NewPersonGuid
    WHERE [Guid] = '996c8b72-c255-40e6-bb98-b1d5cf345f3b' AND [AliasPersonGuid] = 'ad28da19-4af1-408f-9090-2672f8376f27'

    -- Update the old PersonAlias.[AliasPersonGuid] with their new @NewPersonGuid
    UPDATE [PersonAlias]
    SET [AliasPersonGuid] = @NewPersonGuid
    WHERE [PersonId] = 1 AND [AliasPersonGuid] = 'ad28da19-4af1-408f-9090-2672f8376f27'

    -- Update the Person with the new top primary alias  (NOT AVAILABLE IN v16.x)
    UPDATE [Person]
    SET [PrimaryAliasGuid] = (SELECT TOP 1 [Guid] FROM [PersonAlias] WHERE Id = Person.PrimaryAliasId)
    WHERE [Guid] = @NewPersonGuid

    -- Update attribute values that reference the old person alias guid
    UPDATE av
        SET av.[Value] = CAST(@NewPersonAliasGuid AS nvarchar(36))
    FROM [AttributeValue] av
    INNER JOIN [Attribute] a ON av.[AttributeId] = a.[Id]
    WHERE a.[FieldTypeId] = @PersonFieldTypeId AND av.[ValueChecksum] = '817829624' AND av.[Value] = '996c8b72-c255-40e6-bb98-b1d5cf345f3b'
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
