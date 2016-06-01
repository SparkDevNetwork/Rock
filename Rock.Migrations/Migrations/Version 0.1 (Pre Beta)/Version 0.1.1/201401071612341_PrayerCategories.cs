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
    public partial class PrayerCategories : Rock.Migrations.RockMigration1
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddPage( "0B213645-FA4E-44A5-8E4C-B2D8EF054985", "195BCD57-1C10-4969-886F-7324B6287B75", "Prayer Categories", "", "FA2A1171-9308-41C7-948C-C9EBEA5BD668", "fa fa-cloud-upload" ); // Site:Rock Internal
            AddBlockType( "Core > Categories", "", "~/Blocks/Core/Categories.ascx", "620FC4A2-6587-409F-8972-22065919D9AC" );
            AddBlockTypeAttribute( "620FC4A2-6587-409F-8972-22065919D9AC", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "EntityType", "", "The entity type to manage categories for.", 0, @"", "C405A507-7889-4287-8342-105B89710044" );
            AddBlock( "FA2A1171-9308-41C7-948C-C9EBEA5BD668", "", "620FC4A2-6587-409F-8972-22065919D9AC", "Categories", "Main", 0, "E0EF4ED1-A621-43A1-A75D-C6319F7F10D2" );
            AddBlockAttributeValue( "E0EF4ED1-A621-43A1-A75D-C6319F7F10D2", "C405A507-7889-4287-8342-105B89710044", @"f13c8fd2-7702-4c79-a6a9-86440dd5de13" );

            Sql( @"
    UPDATE [Page] SET 
        [MenuDisplayIcon] = 1,
        [MenuDisplayChildPages] = 1,
        [BreadCrumbDisplayName] = 0
    WHERE [Guid] = 'FA2A1171-9308-41C7-948C-C9EBEA5BD668'
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteBlock( "E0EF4ED1-A621-43A1-A75D-C6319F7F10D2" );
            DeleteAttribute( "C405A507-7889-4287-8342-105B89710044" );
            DeleteBlockType( "620FC4A2-6587-409F-8972-22065919D9AC" );
            DeletePage( "FA2A1171-9308-41C7-948C-C9EBEA5BD668" );

        }
    }
}
