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
    public partial class LinkList : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateBlockType( "Link List Lava", "Displays a list of links.", "~/Blocks/Cms/LinkListLava.ascx", "CMS", "BBA9210E-80E1-486A-822D-F8842FE09F99" );
            // Attrib for BlockType: Link List Lava:Enable Debug
            RockMigrationHelper.AddBlockTypeAttribute( "BBA9210E-80E1-486A-822D-F8842FE09F99", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Debug", "EnableDebug", "", "Show merge data to help you see what's available to you.", 2, @"False", "0368A01D-975B-4FCA-BB70-7FB699BE5704" );
            // Attrib for BlockType: Link List Lava:Defined Type
            RockMigrationHelper.AddBlockTypeAttribute( "BBA9210E-80E1-486A-822D-F8842FE09F99", "BC48720C-3610-4BCF-AE66-D255A17F1CDF", "Defined Type", "DefinedType", "", "The defined type to use when saving link information.", 0, @"7E7969BD-945C-4472-8A80-889EF5833776", "53189258-9E59-4236-8B31-F375C9BB3E18" );
            // Attrib for BlockType: Search:Search Type
            RockMigrationHelper.AddBlockTypeAttribute( "E3A99534-6FD9-49AD-AC52-32D53B2CEDD7", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Search Type", "SearchType", "", "The type of search to use for check-in (default is phone number).", 4, @"F3F66040-C50F-4D13-9652-780305FFFE23", "E5BD71C5-1D30-40F4-8E62-D3A4E68A7F86" );
            // Attrib for BlockType: Link List Lava:Edit Footer
            RockMigrationHelper.AddBlockTypeAttribute( "BBA9210E-80E1-486A-822D-F8842FE09F99", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Edit Footer", "EditFooter", "", "The HTML to display above list when editing values.", 4, @"
    </div>
</div>
", "EFDFAD19-43A9-4D01-80EC-41CAA6235E9D" );
            // Attrib for BlockType: Link List Lava:Edit Header
            RockMigrationHelper.AddBlockTypeAttribute( "BBA9210E-80E1-486A-822D-F8842FE09F99", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Edit Header", "EditHeader", "", "The HTML to display above list when editing values.", 3, @"
<div class='panel panel-block'>
    <div class='panel-heading'>
        <h4 class='panel-title'>Links</div>
    <div>
    <div class='panel-body'>
", "569B27B8-9EEB-40C7-96C6-6B45F3E61611" );
            // Attrib for BlockType: Link List Lava:Lava Template
            RockMigrationHelper.AddBlockTypeAttribute( "BBA9210E-80E1-486A-822D-F8842FE09F99", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "", "Lava template to use to display content", 1, @"
<div class=""panel panel-block""> 
    <div class=""panel-heading"">
        <h4 class=""panel-title"">Links</h4>
        {% if AllowedActions.Edit == true %}
            <span class=""pull-right""><a href=""#"" onclick=""{{ '' | Postback:'EditList' }}""><i class='fa fa-gear'></i></a></span>
        {% endif %}
    </div>
    <div class=""panel-body"">

        {% for definedValue in DefinedValues %}
            {% assign IsLink = definedValue | Attribute:'IsLink','RawValue' %}
            {% if IsLink == 'True' %}
                {% if forloop.first == true %}
        <ul class='list-unstyled'>
                {% endif %}                
            <li><a href='{{ definedValue.Description }}'>{{ definedValue.Value }}</a></li>
            {% else %}
                {% if forloop.first == false %}
        </ul>
                {% endif %}
        <strong>{{ definedValue.Value }}</strong>
        <ul class='list-unstyled'>
            {% endif %}
            {% if forloop.last == true %}
        </ul>
            {% endif %}
        {% endfor %}

    </div>
</div>
", "37758E21-23CB-4836-AAF6-1AAD9E36198F" );

            RockMigrationHelper.AddDefinedType( "Link Lists", "Links", "Default list of links used by the 'Link List Lava' block.", "7E7969BD-945C-4472-8A80-889EF5833776" );
            RockMigrationHelper.AddDefinedTypeAttribute( "7E7969BD-945C-4472-8A80-889EF5833776", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Is Link", "IsLink", "Is this item a Link (vs. Heading)?", 0, "true", "2D3317AE-00CE-47E9-92A2-D28DDE72DBB2" );
            RockMigrationHelper.AddAttributeQualifier( "2D3317AE-00CE-47E9-92A2-D28DDE72DBB2", "truetext", "Yes", "D56ECD08-DB47-47D3-ABF8-5A7A1229A80E" );
            RockMigrationHelper.AddAttributeQualifier( "2D3317AE-00CE-47E9-92A2-D28DDE72DBB2", "falsetext", "No", "94148AD8-0234-4C0D-BF55-EC7133A954F2" );

            RockMigrationHelper.AddDefinedValue( "7E7969BD-945C-4472-8A80-889EF5833776", "Rock", "", "69B8FE33-F2FA-4619-9C9B-CC4ABB81D6FC" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "69B8FE33-F2FA-4619-9C9B-CC4ABB81D6FC", "2D3317AE-00CE-47E9-92A2-D28DDE72DBB2", "False" );

            RockMigrationHelper.AddDefinedValue( "7E7969BD-945C-4472-8A80-889EF5833776", "Website", "http://www.rockrms.com", "FD1FBDFF-691C-4A7F-A1BC-E5B91206520B" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FD1FBDFF-691C-4A7F-A1BC-E5B91206520B", "2D3317AE-00CE-47E9-92A2-D28DDE72DBB2", "True" );

            RockMigrationHelper.AddDefinedValue( "7E7969BD-945C-4472-8A80-889EF5833776", "Documentation", "http://www.rockrms.com/Learn/Documentation", "FE0B4254-46F5-4FE6-87D9-0482A1BB4E84" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FE0B4254-46F5-4FE6-87D9-0482A1BB4E84", "2D3317AE-00CE-47E9-92A2-D28DDE72DBB2", "True" );

            RockMigrationHelper.AddDefinedValue( "7E7969BD-945C-4472-8A80-889EF5833776", "Questions", "http://www.rockrms.com/Rock/Ask", "1A40715B-7B4D-42CE-8CBC-C76DDE67BED1" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "1A40715B-7B4D-42CE-8CBC-C76DDE67BED1", "2D3317AE-00CE-47E9-92A2-D28DDE72DBB2", "True" );

            RockMigrationHelper.AddDefinedValue( "7E7969BD-945C-4472-8A80-889EF5833776", "Community", "http://www.rockrms.com/slack", "755D4091-253E-499B-AB15-A7E86BF423F1" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "755D4091-253E-499B-AB15-A7E86BF423F1", "2D3317AE-00CE-47E9-92A2-D28DDE72DBB2", "True" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
