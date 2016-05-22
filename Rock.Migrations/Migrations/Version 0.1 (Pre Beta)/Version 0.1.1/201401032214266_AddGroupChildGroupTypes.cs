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
    public partial class AddGroupChildGroupTypes : Rock.Migrations.RockMigration1
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( "IF NOT EXISTS (SELECT 1 FROM [GroupTypeAssociation] WHERE [GroupTypeId] = 13 AND [ChildGroupTypeId] = 13) INSERT INTO [GroupTypeAssociation] ([GroupTypeId], [ChildGroupTypeId]) VALUES (13, 13)" );
            Sql( "IF NOT EXISTS (SELECT 1 FROM [GroupTypeAssociation] WHERE [GroupTypeId] = 23 AND [ChildGroupTypeId] = 23) INSERT INTO [GroupTypeAssociation] ([GroupTypeId], [ChildGroupTypeId]) VALUES (23, 23)" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"DELETE FROM [GroupTypeAssociation] WHERE [GroupTypeId] = 13 AND [ChildGroupTypeId] = 13" );
            Sql( @"DELETE FROM [GroupTypeAssociation] WHERE [GroupTypeId] = 23 AND [ChildGroupTypeId] = 23" );
        }
    }
}
