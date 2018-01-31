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
    public partial class IndexDocumentUrlRollup : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.EntityType", "IndexDocumentUrl", c => c.String());

            Sql( @"UPDATE [EntityType]
  SET [IndexDocumentUrl] = '{{ IndexDocument.Url }}'
  WHERE [Name] = 'Rock.Model.Site'

  UPDATE [EntityType]
  SET [IndexDocumentUrl] = '{% if IndexDocument.IndexModelType == ""Rock.UniversalSearch.IndexModels.PersonIndex"" %}
    {% assign url = ""~/Person/"" | ResolveRockUrl %}
    
    {% if DisplayOptions.Person-Url and DisplayOptions.Person-Url != null and DisplayOptions.Person-Url != '''' %}
        {% assign url = DisplayOptions.Person-Url | ResolveRockUrl %}
    {% endif %}
{% elseif IndexDocument.IndexModelType == ""Rock.UniversalSearch.IndexModels.BusinessIndex"" %}
    {% assign url = ""~/Business/"" | ResolveRockUrl %}
    
    {% if DisplayOptions.Business-Url and DisplayOptions.Business-Url != null and DisplayOptions.Business-Url != '''' %}
        {% assign url = DisplayOptions.Business-Url | ResolveRockUrl %}
    {% endif %}
{% endif %}

{{ url }}/{{ IndexDocument.Id }}'
  WHERE [Name] = 'Rock.Model.Person'


UPDATE [EntityType]
  SET [IndexDocumentUrl] = '{% assign url = ""~/Group/"" | ResolveRockUrl %}

{% if DisplayOptions.Group-Url and DisplayOptions.Group-Url != null and DisplayOptions.Group-Url != '''' %}
    {% assign url = DisplayOptions.Group-Url | ResolveRockUrl %}
{% endif %}

{{ url }}/{{ IndexDocument.Id }}'
  WHERE [Name] = 'Rock.Model.ContentChannelItem'

UPDATE [EntityType]
  SET [IndexDocumentUrl] = '{% assign url = ""~/Group/"" | ResolveRockUrl %}

{% if DisplayOptions.Group-Url and DisplayOptions.Group-Url != null and DisplayOptions.Group-Url != '' %}
    {% assign url = DisplayOptions.Group-Url | ResolveRockUrl %}
{% endif %}

{{ url }}/{{ IndexDocument.Id }}'
  WHERE [Name] = 'Rock.Model.Group'




 UPDATE [EntityType]
  SET [IndexResultTemplate] = '{% if IndexDocument.IndexModelType == ""Rock.UniversalSearch.IndexModels.PersonIndex"" %}

    {% assign url = ""~/Person/"" | ResolveRockUrl %}
    
    {% if DisplayOptions.Person-Url and DisplayOptions.Person-Url != null and DisplayOptions.Person-Url != '''' %}
        {% assign url = DisplayOptions.Person-Url | ResolveRockUrl %}
    {% endif %}
    
    
    <div class=""row model-cannavigate"" data-href=""{{ url }}{{ IndexDocument.Id }}"">
        <div class=""col-sm-1 text-center"">
            <i class=""{{ IndexDocument.IconCssClass }} fa-2x""></i>
        </div>
        <div class=""col-md-3 col-sm-10"">
            <strong>{{ IndexDocument.NickName}} {{ IndexDocument.LastName}} {{ IndexDocument.Suffix }}</strong> 
            <br>
            {% if IndexDocument.Email != null and IndexDocument.Email != '' %}
                {{ IndexDocument.Email }} <br>
            {% endif %}
    
            {% if IndexDocument.StreetAddress != '' and IndexDocument.StreetAddress != null %}
                {{ IndexDocument.StreetAddress }}<br>
            {% endif %}
            
            {% if IndexDocument.City != '' and IndexDocument.City != null %}
                {{ IndexDocument.City }}, {{ IndexDocument.State }} {{ IndexDocument.PostalCode }}
            {% endif %}
        </div>
        <div class=""col-md-2"">
            Connection Status: <br> 
            {{ IndexDocument.ConnectionStatusValueId | FromCache:''DefinedValue'' | Property:''Value'' }}
        </div>
        <div class=""col-md-2"">
            Age: <br> 
            {{ IndexDocument.Age }}
        </div>
        <div class=""col-md-2"">
            Record Status: <br> 
            {{ IndexDocument.RecordStatusValueId | FromCache:''DefinedValue'' | Property:''Value'' }}
        </div>
        <div class=""col-md-2"">
            Campus: <br> 
            {{ IndexDocument.CampusId | FromCache:''Campus'' | Property:''Name'' }}
        </div>
    </div>

{% elseif IndexDocument.IndexModelType == ""Rock.UniversalSearch.IndexModels.BusinessIndex"" %}
    {% assign url = ""~/Business/"" | ResolveRockUrl %}
    
    {% if DisplayOptions.Business-Url and DisplayOptions.Business-Url != null and DisplayOptions.Business-Url != '''' %}
        {% assign url = DisplayOptions.Business-Url | ResolveRockUrl %}
    {% endif %}
    
    
    <div class=""row model-cannavigate"" data-href=""{{ url }}{{ IndexDocument.Id }}"">
        <div class=""col-sm-1 text-center"">
            <i class=""{{ IndexDocument.IconCssClass }} fa-2x""></i>
        </div>
        <div class=""col-sm-11"">
            <strong>{{ IndexDocument.Name}}</strong> 

            {% if IndexDocument.Contacts != null and IndexDocument.Contacts != '''' %}
                <br>Contacts: {{ IndexDocument.Contacts }}
            {% endif %}
        </div>
    </div>
{% endif %}'
  WHERE [Name] = 'Rock.Model.Person'


" );

            // rollups
            RockMigrationHelper.AddBlockTypeAttribute( "C0D58DEE-D266-4AA8-8750-414A3CC26C07", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Inactive Campuses", "DisplayInactiveCampuses", "", "Include inactive campuses in the Campus Filter", 0, @"True", "67AB1406-38A4-4473-9BDF-734E38463A4C" );

            RockMigrationHelper.UpdateBlockType( "Giving Type Context Setter", "Block that can be used to set the Person context to either the current person or one of the current person's businesses.", "~/Blocks/Finance/GivingTypeContextPicker.ascx", "Finance", "57B00D03-1CDC-4492-95CF-7BD127CE61F0" );

            // Add Block to Page: Giving History, Site: External Website
            RockMigrationHelper.AddBlock( "621E03C5-6586-450B-A340-CC7D9727B69A", "", "57B00D03-1CDC-4492-95CF-7BD127CE61F0", "Giving Type Context Setter", "Main", @"<div class='pull-right'>
    Show Giving For", @"</div>", 0, "51555696-AA72-4BE1-84C5-6C783F4B26F3" );
            // update block order for pages with new blocks if the page,zone has multiple blocks
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '51555696-AA72-4BE1-84C5-6C783F4B26F3'" );  // Page: Giving History,  Zone: Main,  Block: Giving Type Context Setter
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '8A5E5144-3054-4FC9-AD8A-B0F4813C94E4'" );  // Page: Giving History,  Zone: Main,  Block: Transaction Report Intro Text
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = '639943D6-75C2-46B4-B044-F4FD7E42E936'" );  // Page: Giving History,  Zone: Main,  Block: Contribution Statement List Lava
            Sql( @"UPDATE [Block] SET [Order] = 3 WHERE [Guid] = '0B62A727-1AEB-4134-AFAE-1EBB73A6B098'" );  // Page: Giving History,  Zone: Main,  Block: Transaction Report


            // Attrib for BlockType: Transaction Report:Use Person Context
            RockMigrationHelper.AddBlockTypeAttribute( "1FAEE5A2-5005-4BD8-A2BD-B7D9030A894D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Person Context", "UsePersonContext", "", "Determines if the person context should be used instead of the CurrentPerson.", 5, @"False", "CCC536BC-D8FB-4AA0-ADF4-944E8ACE9FB4" );

            // Attrib for BlockType: Transaction Report:Entity Type
            RockMigrationHelper.AddBlockTypeAttribute( "1FAEE5A2-5005-4BD8-A2BD-B7D9030A894D", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", "The type of entity that will provide context for this block", 0, @"", "1DA0E94F-F362-4724-BE62-FFB7E7D9A9CA" );

            // Attrib Value for Block:Transaction Report, Attribute:Entity Type Page: Giving History, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "0B62A727-1AEB-4134-AFAE-1EBB73A6B098", "1DA0E94F-F362-4724-BE62-FFB7E7D9A9CA", @"72657ed8-d16e-492e-ac12-144c5e7567e7" );

            // Attrib Value for Block:Transaction Report, Attribute:Use Person Context Page: Giving History, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "0B62A727-1AEB-4134-AFAE-1EBB73A6B098", "CCC536BC-D8FB-4AA0-ADF4-944E8ACE9FB4", @"True" );

            // Attrib Value for Block:Contribution Statement List Lava, Attribute:Use Person Context Page: Giving History, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "639943D6-75C2-46B4-B044-F4FD7E42E936", "F37EB885-416A-4B70-B48E-8A25557C7B12", @"True" );

            // Attrib Value for Block:Contribution Statement List Lava, Attribute:Entity Type Page: Giving History, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "639943D6-75C2-46B4-B044-F4FD7E42E936", "F9A168F1-3E59-4C5F-8019-7B17D00B94C9", @"72657ed8-d16e-492e-ac12-144c5e7567e7" );

            // --------
            // Add Block to Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( "D2B43273-C64F-4F57-9AAE-9571E1982BAC", "", "694FF260-8C6F-4A59-93C9-CF3793FE30E6", "Scheduled Transaction List", "Main", @"", @"", 2, "91850A29-BB1A-4E92-A798-DE7D6E09E671" );

            RockMigrationHelper.AddPage( "D2B43273-C64F-4F57-9AAE-9571E1982BAC", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Scheduled Transaction", "", "591204DA-B586-454C-8BD5-85652CEAA553", "fa fa-credit-card" ); // Site:Rock RMS

            // Add Block to Page: Scheduled Transaction, Site: Rock RMS
            RockMigrationHelper.AddBlock( "591204DA-B586-454C-8BD5-85652CEAA553", "", "85753750-7465-4241-97A6-E5F27EA38C8B", "Scheduled Transaction View", "Main", @"", @"", 0, "B3521F04-88B1-4AD7-A51A-7CD0276797FF" );
            // Add Block to Page: Scheduled Transaction, Site: Rock RMS
            RockMigrationHelper.AddBlock( "591204DA-B586-454C-8BD5-85652CEAA553", "", "E04320BC-67C3-452D-9EF6-D74D8C177154", "Transaction List", "Main", @"", @"", 1, "3070AE21-0EC0-45C0-989B-87C00E7F75DB" );

            // update block order for pages with new blocks if the page,zone has multiple blocks
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '77AB2D30-FCBE-45E9-9757-401AE2676A7F'" );  // Page: Business Detail,  Zone: Main,  Block: Business Detail
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '5322C1C2-0387-4752-9E87-67700F485C5E'" );  // Page: Business Detail,  Zone: Main,  Block: Transaction Yearly Summary Lava
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = '91850A29-BB1A-4E92-A798-DE7D6E09E671'" );  // Page: Business Detail,  Zone: Main,  Block: Scheduled Transaction List
            Sql( @"UPDATE [Block] SET [Order] = 3 WHERE [Guid] = '0A567E24-80BE-4906-B303-77D1A5FB89DE'" );  // Page: Business Detail,  Zone: Main,  Block: Transaction List

            // Attrib Value for Block:Scheduled Transaction List, Attribute:View Page Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "91850A29-BB1A-4E92-A798-DE7D6E09E671", "47B99CD1-FB63-44D7-8586-45BDCDF51137", @"591204da-b586-454c-8bd5-85652ceaa553" );
            // Attrib Value for Block:Scheduled Transaction List, Attribute:Add Page Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "91850A29-BB1A-4E92-A798-DE7D6E09E671", "9BCE3FD8-9014-4120-9DCC-06C4936284BA", @"" );
            // Attrib Value for Block:Scheduled Transaction List, Attribute:Entity Type Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "91850A29-BB1A-4E92-A798-DE7D6E09E671", "375F7220-04C6-4E41-B99A-A2CE494FD74A", @"72657ed8-d16e-492e-ac12-144c5e7567e7" );

            // Attrib Value for Block:Scheduled Transaction View, Attribute:Update Page Page: Scheduled Transaction, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B3521F04-88B1-4AD7-A51A-7CD0276797FF", "98FE689B-DCBC-4E29-9269-A96FE8066C50", @"591204da-b586-454c-8bd5-85652ceaa553" );
            // Attrib Value for Block:Transaction List, Attribute:Detail Page Page: Scheduled Transaction, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3070AE21-0EC0-45C0-989B-87C00E7F75DB", "C6D07A89-84C9-412A-A584-E37E59506566", @"591204da-b586-454c-8bd5-85652ceaa553" );
            // Attrib Value for Block:Transaction List, Attribute:Show Only Active Accounts on Filter Page: Scheduled Transaction, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3070AE21-0EC0-45C0-989B-87C00E7F75DB", "81AD58EA-F94B-42A1-AC57-16902B717092", @"False" );
            // Attrib Value for Block:Transaction List, Attribute:Transaction Types Page: Scheduled Transaction, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3070AE21-0EC0-45C0-989B-87C00E7F75DB", "293F8A3E-020A-4260-8817-3E368CF31ABB", @"" );
            // Attrib Value for Block:Transaction List, Attribute:Image Height Page: Scheduled Transaction, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3070AE21-0EC0-45C0-989B-87C00E7F75DB", "00EBFDFE-C6AE-48F2-B284-809D1765D489", @"200" );
            // Attrib Value for Block:Transaction List, Attribute:Show Options Page: Scheduled Transaction, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3070AE21-0EC0-45C0-989B-87C00E7F75DB", "0227D124-D207-4F68-8B77-4A4A88CBBE6F", @"False" );
            // Attrib Value for Block:Transaction List, Attribute:Entity Type Page: Scheduled Transaction, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3070AE21-0EC0-45C0-989B-87C00E7F75DB", "29A6C37A-EFB3-41CC-A522-9CEFAAEEA910", @"76824e8a-ccc4-4085-84d9-8af8c0807e20" );
            // Attrib Value for Block:Transaction List, Attribute:Title Page: Scheduled Transaction, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3070AE21-0EC0-45C0-989B-87C00E7F75DB", "A4E3B5C6-B386-45B5-A929-8FD9379BABBC", @"Processed Transactions" );

            // Add/Update PageContext for Page:Scheduled Transaction, Entity: Rock.Model.FinancialScheduledTransaction, Parameter: ScheduledTransactionId
            RockMigrationHelper.UpdatePageContext( "591204DA-B586-454C-8BD5-85652CEAA553", "Rock.Model.FinancialScheduledTransaction", "ScheduledTransactionId", "169E1FA7-823C-49E4-A1FA-C6AB65A57D6E" );

            // Attrib Value for Block:Scheduled Transaction List, Attribute:Add Page Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "91850A29-BB1A-4E92-A798-DE7D6E09E671", "9BCE3FD8-9014-4120-9DCC-06C4936284BA", @"b1ca86dc-9890-4d26-8ebd-488044e1b3dd" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.EntityType", "IndexDocumentUrl");

            // Attrib for BlockType: Transaction Report:Use Person Context
            RockMigrationHelper.DeleteAttribute( "CCC536BC-D8FB-4AA0-ADF4-944E8ACE9FB4" );

            // Attrib for BlockType: Transaction Report:Entity Type
            RockMigrationHelper.DeleteAttribute( "1DA0E94F-F362-4724-BE62-FFB7E7D9A9CA" );

            // Remove Block: Giving Type Context Setter, from Page: Giving History, Site: External Website
            RockMigrationHelper.DeleteBlock( "51555696-AA72-4BE1-84C5-6C783F4B26F3" );

            RockMigrationHelper.DeleteBlockType( "57B00D03-1CDC-4492-95CF-7BD127CE61F0" ); // Giving Type Context Setter

            // ---------

            // Add Block to Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "91850A29-BB1A-4E92-A798-DE7D6E09E671" );

            RockMigrationHelper.DeletePage( "591204DA-B586-454C-8BD5-85652CEAA553" ); // Site:Rock RMS

            // Add Block to Page: Scheduled Transaction, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "B3521F04-88B1-4AD7-A51A-7CD0276797FF" );
            // Add Block to Page: Scheduled Transaction, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "3070AE21-0EC0-45C0-989B-87C00E7F75DB" );

            RockMigrationHelper.DeletePageContext( "169E1FA7-823C-49E4-A1FA-C6AB65A57D6E" );
        }
    }
}
