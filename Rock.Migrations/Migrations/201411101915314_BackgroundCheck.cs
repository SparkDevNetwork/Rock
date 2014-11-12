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
    public partial class BackgroundCheck : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Update the EntityType qualifier on Attribute attributes to use Guid instead of Id
            Sql( @"
    UPDATE AQ
    SET [Value] = CAST(ET.[Guid] AS VARCHAR(50))
    FROM [Attribute] A
    INNER JOIN [FieldType] FT ON FT.[Id] = A.[FieldTypeId] AND FT.[Guid] = '99B090AA-4D7E-46D8-B393-BF945EA1BA8B'
    INNER JOIN [AttributeQualifier] AQ ON AQ.[AttributeId] = A.[Id] AND AQ.[Key] = 'entitytype'
    INNER JOIN [EntityType] ET ON ET.[Id] = CAST(AQ.[Value] AS int)
" );

            // Update the BinaryFileType qualifier on Binary file attributes to use Guid instead of Id
            Sql( @"
    UPDATE AQ
    SET [Value] = CAST(BFT.[Guid] AS VARCHAR(50))
    FROM [Attribute] A
    INNER JOIN [FieldType] FT ON FT.[Id] = A.[FieldTypeId] AND FT.[Guid] = 'C403E219-A56B-439E-9D50-9302DFE760CF'
    INNER JOIN [AttributeQualifier] AQ ON AQ.[AttributeId] = A.[Id] AND AQ.[Key] = 'binaryFileType'
    INNER JOIN [BinaryFileType] BFT ON BFT.[Id] = CAST(AQ.[Value] AS int)
" );

            // Add Background check binary file type
            RockMigrationHelper.UpdateBinaryFileType( "0AA42802-04FD-4AEC-B011-FEB127FC85CD", "Background Check", "The background check result", "fa fa-search", "5C701472-8A6B-4BBE-AEC6-EC833C859F2D", false, true );

            // Add new person attribute category for background check data
            RockMigrationHelper.UpdatePersonAttributeCategory( "Background Check", "fa fa-search", "Details of last Background Check Request", "4D1E1EBA-ABF2-4A7C-8ADF-65CB5AAE94E2" );
            
            RockMigrationHelper.UpdatePersonAttribute( "6B6AA175-4758-453F-8D83-FCD8044B5F36", "4D1E1EBA-ABF2-4A7C-8ADF-65CB5AAE94E2", "Background Check Date", "BackgroundCheckDate", "", "Date person last passed/failed a background check", 0, "", "3DAFF000-7F74-47D7-8CB0-E4A4E6C81F5F" );
            RockMigrationHelper.AddAttributeQualifier( "3DAFF000-7F74-47D7-8CB0-E4A4E6C81F5F", "format", "", "2DF6459A-CBAE-4F8D-9870-873EFF77ACD6" );
            RockMigrationHelper.AddAttributeQualifier( "3DAFF000-7F74-47D7-8CB0-E4A4E6C81F5F", "displayDiff", "False", "7EF8C117-2743-4420-8BC7-7B8F5149A00F" );
            
            RockMigrationHelper.UpdatePersonAttribute( "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "4D1E1EBA-ABF2-4A7C-8ADF-65CB5AAE94E2", "Background Check Result", "BackgroundCheckResult", "", "Result of last background check", 0, "", "44490089-E02C-4E54-A456-454845ABBC9D" );
            RockMigrationHelper.AddAttributeQualifier( "44490089-E02C-4E54-A456-454845ABBC9D", "values", "Pass,Fail", "65F25E53-576A-445B-B67D-5CE2373A2D85" );
            RockMigrationHelper.AddAttributeQualifier( "44490089-E02C-4E54-A456-454845ABBC9D", "fieldtype", "ddl", "62A411F9-5CEC-492D-9BAB-98F23B4DF44C" );

            RockMigrationHelper.UpdatePersonAttribute( "C403E219-A56B-439E-9D50-9302DFE760CF", "4D1E1EBA-ABF2-4A7C-8ADF-65CB5AAE94E2", "Background Check Document", "BackgroundCheckDocument", "", "The last background check", 0, "", "F3931952-460D-43E0-A6E0-EB6B5B1F9167" );
            RockMigrationHelper.AddAttributeQualifier( "F3931952-460D-43E0-A6E0-EB6B5B1F9167", "binaryFileType", "5C701472-8A6B-4BBE-AEC6-EC833C859F2D", "43248635-B07D-49D8-885D-DF46C041CC04" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Delete the background check person attributes/category
            RockMigrationHelper.DeleteAttribute( "F3931952-460D-43E0-A6E0-EB6B5B1F9167" );
            RockMigrationHelper.DeleteAttribute( "44490089-E02C-4E54-A456-454845ABBC9D" );
            RockMigrationHelper.DeleteAttribute( "3DAFF000-7F74-47D7-8CB0-E4A4E6C81F5F" );
            RockMigrationHelper.DeleteCategory( "4D1E1EBA-ABF2-4A7C-8ADF-65CB5AAE94E2" );

            // Update the BinaryFileType qualifier on Binary file attributes to use Id instead of Guid 
            Sql( @"
    UPDATE AQ
    SET [Value] = CAST(BFT.[Id] AS VARCHAR)
    FROM [Attribute] A
    INNER JOIN [FieldType] FT ON FT.[Id] = A.[FieldTypeId] AND FT.[Guid] = 'C403E219-A56B-439E-9D50-9302DFE760CF'
    INNER JOIN [AttributeQualifier] AQ ON AQ.[AttributeId] = A.[Id] AND AQ.[Key] = 'binaryFileType'
    INNER JOIN [BinaryFileType] BFT ON CAST(BFT.[Guid] AS varchar(50)) = AQ.[Value]
" );

            // Update the EntityType qualifier on Attribute attributes to use Id instead of Guid 
            Sql( @"
    UPDATE AQ
    SET [Value] = CAST(ET.[Id] AS VARCHAR)
    FROM [Attribute] A
    INNER JOIN [FieldType] FT ON FT.[Id] = A.[FieldTypeId] AND FT.[Guid] = 'C403E219-A56B-439E-9D50-9302DFE760CF'
    INNER JOIN [AttributeQualifier] AQ ON AQ.[AttributeId] = A.[Id] AND AQ.[Key] = 'binaryFileType'
    INNER JOIN [EntityType] ET ON CAST(ET.[Guid] AS varchar(50)) = AQ.[Value]
" );

        }
    }
}
