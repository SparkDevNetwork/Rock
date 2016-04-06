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
    public partial class FollowingEventsAndRegistrations : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateEntityType( "Rock.Model.EventItem", "6A58AD11-3491-84AE-4896-8F39906EA65E", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.RegistrationInstance", "5CD9C0C8-C047-61A0-4E36-0FDB8496F066", true, true );

            RockMigrationHelper.AddPageRoute( "844DC54B-DAEC-47B3-A63A-712DD6D57793", "RegistrationInstance/{RegistrationInstanceId}", "564A51F5-DA47-35AF-4278-E8C810FA8D25" );
            RockMigrationHelper.AddPageRoute( "7FB33834-F40A-4221-8849-BB8C06903B04", "EventItem/{EventItemId}", "8AE37E76-9B0B-2AB1-461A-CE9B70F8F2C5" );

            // Add Block to Page: My Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlock( "AE1818D8-581C-4599-97B9-509EA450376A", "", "36B56055-7AA2-4169-82DD-CCFBD2C7B4CC", "Follow Events", "Sidebar1", @"<div class='panel panel-block'> 
    <div class='panel-heading'>
       <h4 class='panel-title'><i class='fa fa-flag'></i> Followed Items</h4>
    </div>
    <div class='panel-body'>","",3,"A88BDBC1-C03A-4517-BBE4-151D0ADE8913"); 
            // Add Block to Page: My Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlock("AE1818D8-581C-4599-97B9-509EA450376A","","36B56055-7AA2-4169-82DD-CCFBD2C7B4CC","Follow Registrations","Sidebar1","",@"    </div>
</div>",4,"0D680545-3435-4C09-8A1A-FA0FFB022F49"); 

            // update block order for pages with new blocks if the page,zone has multiple blocks
            Sql(@"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = 'D54B70C3-B964-459D-B5B6-39BB49AE4E7A'");  // Page: My Dashboard,  Zone: Sidebar1,  Block: Person Suggestion Notice
            Sql(@"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = 'AA11F703-FF26-4DA3-8CAE-E95989013135'");  // Page: My Dashboard,  Zone: Sidebar1,  Block: Following Groups
            Sql(@"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = 'A88BDBC1-C03A-4517-BBE4-151D0ADE8913'");  // Page: My Dashboard,  Zone: Sidebar1,  Block: Follow Events
            Sql(@"UPDATE [Block] SET [Order] = 3 WHERE [Guid] = '0D680545-3435-4C09-8A1A-FA0FFB022F49'");  // Page: My Dashboard,  Zone: Sidebar1,  Block: Follow Registrations
            Sql(@"UPDATE [Block] SET [Order] = 4 WHERE [Guid] = '8111124F-8201-4F54-8A2C-CDC9D7CEA1BC'");  // Page: My Dashboard,  Zone: Sidebar1,  Block: My Open Workflows

          
            Sql( @"  DECLARE @BlockId int = (SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = 'A88BDBC1-C03A-4517-BBE4-151D0ADE8913')
  DECLARE @AttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'C896AA82-F8D7-4A94-9730-D4030DA3DBFA')
  DECLARE @EntityGuid nvarchar(50) = (SELECT TOP 1 convert(nvarchar(50),[Guid]) FROM [EntityType] WHERE [Name] = 'Rock.Model.EventItem')

  INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Value], [Guid])
  VALUES (0, @AttributeId, @BlockId, @EntityGuid, '3DEC8071-92D3-1BBF-4ACB-A2A9D36B7DBF')" );

            // Attrib Value for Block:Follow Events, Attribute:Lava Template Page: My Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("A88BDBC1-C03A-4517-BBE4-151D0ADE8913","58CA8CF4-6C86-4D27-92BE-C687E74D014F",@"{% if FollowingItems != empty %}
    <strong>Events</strong>
    <ul class='list-unstyled margin-b-md'>
    {% for item in FollowingItems %}
        {% if LinkUrl != '' %}
            <li><i class='fa fa-calendar icon-fw'></i><a href='{{ LinkUrl | Replace:'[Id]',item.Id}}'> {{ item.Name }}</a></li>
        {% else %}
            <li>{{ item.Name }}</li>
        {% endif %}
    {% endfor %}
    
    {% if HasMore %}
        <li><i class='fa icon-fw''></i> <small>(showing top {{ Quantity }})</small></li>
    {% endif %}
    
    </ul>
{% endif %}");

            // Attrib Value for Block:Follow Events, Attribute:Enable Debug Page: My Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("A88BDBC1-C03A-4517-BBE4-151D0ADE8913","D7094B16-B237-4D97-B1FC-F680C0701583",@"False");
            // Attrib Value for Block:Follow Events, Attribute:Max Results Page: My Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("A88BDBC1-C03A-4517-BBE4-151D0ADE8913","D2BB9E7D-9DCC-4432-88F5-D5A3058F6DEB",@"35");
            // Attrib Value for Block:Follow Events, Attribute:Link URL Page: My Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("A88BDBC1-C03A-4517-BBE4-151D0ADE8913","9981ABB3-7130-41DB-87AF-973722FBD54E",@"EventItem/[Id]");
            // Attrib Value for Block:Follow Registrations, Attribute:Link URL Page: My Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("0D680545-3435-4C09-8A1A-FA0FFB022F49","9981ABB3-7130-41DB-87AF-973722FBD54E",@"RegistrationInstance/[Id]");
            // Attrib Value for Block:Follow Registrations, Attribute:Enable Debug Page: My Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("0D680545-3435-4C09-8A1A-FA0FFB022F49","D7094B16-B237-4D97-B1FC-F680C0701583",@"False");
            // Attrib Value for Block:Follow Registrations, Attribute:Max Results Page: My Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("0D680545-3435-4C09-8A1A-FA0FFB022F49","D2BB9E7D-9DCC-4432-88F5-D5A3058F6DEB",@"35");
            
            Sql( @"  DECLARE @BlockId int = (SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = '0D680545-3435-4C09-8A1A-FA0FFB022F49')
  DECLARE @AttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'C896AA82-F8D7-4A94-9730-D4030DA3DBFA')
  DECLARE @EntityGuid nvarchar(50) = (SELECT TOP 1 convert(nvarchar(50),[Guid]) FROM [EntityType] WHERE [Name] = 'Rock.Model.RegistrationInstance')

  INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Value], [Guid])
  VALUES (0, @AttributeId, @BlockId, @EntityGuid, 'D25F32F8-8183-5AB4-4027-5307842A08B2')" );
            
            // Attrib Value for Block:Follow Registrations, Attribute:Lava Template Page: My Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("0D680545-3435-4C09-8A1A-FA0FFB022F49","58CA8CF4-6C86-4D27-92BE-C687E74D014F",@"{% if FollowingItems != empty %}
    <strong>Registrations</strong>
    <ul class='list-unstyled margin-b-md'>
    {% for item in FollowingItems %}
        {% if LinkUrl != '' %}
            <li><i class='fa fa-clipboard icon-fw'></i><a href='{{ LinkUrl | Replace:'[Id]',item.Id }}'> {{ item.Name }}</a></li>
        {% else %}
            <li>{{ item.Name }}</li>
        {% endif %}
    {% endfor %}
    
    {% if HasMore %}
        <li><i class='fa icon-fw''></i> <small>(showing top {{ Quantity }})</small></li>
    {% endif %}
    
    </ul>
{% endif %}");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"DELETE FROM [PageRoute] WHERE [Guid] IN ('564A51F5-DA47-35AF-4278-E8C810FA8D25','8AE37E76-9B0B-2AB1-461A-CE9B70F8F2C5')" );

            // Remove Block: Follow Registrations, from Page: My Dashboard, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "0D680545-3435-4C09-8A1A-FA0FFB022F49" );
            // Remove Block: Follow Events, from Page: My Dashboard, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "A88BDBC1-C03A-4517-BBE4-151D0ADE8913" );
        }
    }
}
