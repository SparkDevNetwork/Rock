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
    public partial class AddAdministratorForGroup : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Group", "GroupAdministratorPersonAliasId", c => c.Int());
            AddColumn("dbo.GroupType", "AdministratorTerm", c => c.String(maxLength: 100));
            AddColumn("dbo.GroupType", "ShowAdministrator", c => c.Boolean(nullable: false));
            CreateIndex("dbo.Group", "GroupAdministratorPersonAliasId");
            AddForeignKey("dbo.Group", "GroupAdministratorPersonAliasId", "dbo.PersonAlias", "Id");
            Sql( @" UPDATE [GroupType]
                    SET [AdministratorTerm]='Administrator'" );
            string lavaTemplate = @"{% if Group.Schedule != null %}

            <dt> Schedule </dt>
            <dd>{{ Group.Schedule.FriendlyScheduleText }}</ dd >
            {% endif %}
            {% if Group.GroupCapacity != null and Group.GroupCapacity != '' %}

            <dt> Capacity </dt>

            <dd>{{ Group.GroupCapacity }}</dd>
            {% endif %}";
            string newLavaTemplate = @"{% if Group.Schedule != null %}

            <dt> Schedule </dt>
            <dd>{{ Group.Schedule.FriendlyScheduleText }}</ dd >
            {% endif %}
            {% if Group.GroupCapacity != null and Group.GroupCapacity != '' %}

            <dt> Capacity </dt>

            <dd>{{ Group.GroupCapacity }}</dd>
            {% endif %}
            {% if Group.GroupType.ShowAdministrator and Group.GroupAdministratorPersonAlias != null and Group.GroupAdministratorPersonAlias != '' %}

            <dt> {{ Group.GroupType.AdministratorTerm }}</dt>

            <dd>{{ Group.GroupAdministratorPersonAlias.Person.FullName }}</dd>
            {% endif %}";
            Sql( string.Format( @"UPDATE [GroupType] SET [GroupViewLavaTemplate] = REPLACE([GroupViewLavaTemplate],'{0}','{1}')
                    WHERE [GroupViewLavaTemplate] like '%{0}%'", lavaTemplate.Replace( "'", "''" ), newLavaTemplate.Replace( "'", "''" ) ) );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.Group", "GroupAdministratorPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.Group", new[] { "GroupAdministratorPersonAliasId" });
            DropColumn("dbo.GroupType", "ShowAdministrator");
            DropColumn("dbo.GroupType", "AdministratorTerm");
            DropColumn("dbo.Group", "GroupAdministratorPersonAliasId");
        }
    }
}
