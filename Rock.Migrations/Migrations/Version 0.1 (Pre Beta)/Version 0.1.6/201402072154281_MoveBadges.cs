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
    public partial class MoveBadges : Rock.Migrations.RockMigration3
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql(@"

    -- Set first badge list badges
    UPDATE [AttributeValue] SET
    VALUE = '452CF317-D3A1-49B5-84B1-4206DDADC653'
    WHERE [Guid] = '4F41CA56-BF42-4601-8123-EC71737C4E36'

    -- Set second badge list badges
    UPDATE [AttributeValue] SET
    VALUE = '3f7d648d-d6ba-4f03-931c-afbdfa24bbd8'
    WHERE [Guid] = 'F14BE42A-D356-4F06-9EE0-0396B1C487F6'

");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
