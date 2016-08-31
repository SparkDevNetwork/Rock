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
    public partial class BlockPrePostHtml : Rock.Migrations.RockMigration1
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attrib for BlockType: HTML Content:Use Code Editor
            AddBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Code Editor", "UseCodeEditor", "", "Use the code editor instead of the WYSIWYG editor", 0, @"False", "0673E015-F8DD-4A52-B380-C758011331B2" );

            // Set the checklit block to hide when empty
            AddBlockAttributeValue( "62B1DBE6-B3D9-4C0B-BD12-1DD8C4F2C6EB", "42A9404C-835C-469C-AD85-D77573F76C3D", "True" );

            AddColumn( "dbo.Block", "PreHtml", c => c.String() );
            AddColumn( "dbo.Block", "PostHtml", c => c.String() );

            Sql( @"
    UPDATE B SET
	    [PreHtml] = AV.[Value]
    FROM [AttributeValue] AV
    INNER JOIN [Attribute] A ON A.[Id] = AV.[AttributeId]
    INNER JOIN [Block] B ON B.[Id] = AV.[EntityId]
    WHERE A.[Guid] IN ('15E874B8-FF76-40FB-8713-6D0C98609734','075679A0-A811-47BC-B19C-1052374F9436')
    AND AV.[Value] IS NOT NULL
    AND AV.[Value] <> ''

    UPDATE B SET
	    [PostHtml] = AV.[Value]
    FROM [AttributeValue] AV
    INNER JOIN [Attribute] A ON A.[Id] = AV.[AttributeId]
    INNER JOIN [Block] B ON B.[Id] = AV.[EntityId]
    WHERE A.[Guid] IN ('2E9AF795-68FE-4BD6-AF8B-8848CD796AF5','15192E4B-D2C7-4949-84C1-5D1D65EA98FB')
    AND AV.[Value] IS NOT NULL
    AND AV.[Value] <> ''
" );
            DeleteAttribute( "15E874B8-FF76-40FB-8713-6D0C98609734" );
            DeleteAttribute( "2E9AF795-68FE-4BD6-AF8B-8848CD796AF5" );
            DeleteAttribute( "075679A0-A811-47BC-B19C-1052374F9436" );
            DeleteAttribute( "15192E4B-D2C7-4949-84C1-5D1D65EA98FB" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Pre-Text", "PreText", "", "HTML text to render before the blocks main content.", 1, "", "15E874B8-FF76-40FB-8713-6D0C98609734" );
            AddBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Post-Text", "PostText", "", "HTML text to render after the blocks main content.", 2, "", "2E9AF795-68FE-4BD6-AF8B-8848CD796AF5" );
            AddBlockTypeAttribute( "15572974-DD86-43C8-BBBF-5181EE76E2C9", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Pre-Text", "PreText", "", "Html to wrap the control in to provide advanced UI's like panels.", 0, "", "075679A0-A811-47BC-B19C-1052374F9436" );
            AddBlockTypeAttribute( "15572974-DD86-43C8-BBBF-5181EE76E2C9", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Post-Text", "PostText", "", "Html to wrap the control in to provide advanced UI's like panels.", 1, "", "15192E4B-D2C7-4949-84C1-5D1D65EA98FB" );

            Sql( @"
    INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid])
    SELECT
        0,
        A.[Id],
        B.[Id],
        0,
        B.[PreHtml],
        NEWID()
    FROM [BlockType] BT 
    INNER JOIN [Block] B ON B.[BlockTypeId] = BT.[Id]
    INNER JOIN [Attribute] A ON A.[EntityTypeQualifierColumn] = 'BlockTypeId' AND A.[EntityTypeQualifierValue] = BT.[Id] AND A.[Key] = 'PreText'
    WHERE BT.[Guid] IN ('19B61D65-37E3-459F-A44F-DEF0089118A3', '15572974-DD86-43C8-BBBF-5181EE76E2C9')
    AND B.[PreHtml] IS NOT NULL
    AND B.[PreHtml] <> ''

    INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid])
    SELECT
        0,
        A.[Id],
        B.[Id],
        0,
        B.[PostHtml],
        NEWID()
    FROM [BlockType] BT 
    INNER JOIN [Block] B ON B.[BlockTypeId] = BT.[Id]
    INNER JOIN [Attribute] A ON A.[EntityTypeQualifierColumn] = 'BlockTypeId' AND A.[EntityTypeQualifierValue] = BT.[Id] AND A.[Key] = 'PostText'
    WHERE BT.[Guid] IN ('19B61D65-37E3-459F-A44F-DEF0089118A3', '15572974-DD86-43C8-BBBF-5181EE76E2C9')
    AND B.[PostHtml] IS NOT NULL
    AND B.[PostHtml] <> ''" );

            DropColumn( "dbo.Block", "PostHtml" );
            DropColumn("dbo.Block", "PreHtml");
        }
    }
}
