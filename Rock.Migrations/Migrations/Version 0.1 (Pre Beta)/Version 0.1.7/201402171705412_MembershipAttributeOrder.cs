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
    public partial class MembershipAttributeOrder : Rock.Migrations.RockMigration3
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
            UPDATE [Attribute] SET [Order] = [Order] + 1 WHERE [Guid] = 'D42763FA-28E9-4A55-A25A-48998D7D7FEF'
            UPDATE [Attribute] SET [Order] = [Order] + 2 WHERE [Guid] = '87986F92-4AC3-49D2-9753-328588F3BB7A'
            " );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
            UPDATE [Attribute] SET [Order] = 0 WHERE [Guid] = 'D42763FA-28E9-4A55-A25A-48998D7D7FEF'
            UPDATE [Attribute] SET [Order] = 0 WHERE [Guid] = '87986F92-4AC3-49D2-9753-328588F3BB7A'
            " );
        }
    }
}
