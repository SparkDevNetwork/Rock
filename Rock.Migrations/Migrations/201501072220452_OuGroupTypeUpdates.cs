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
    public partial class OuGroupTypeUpdates : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // create new purpose
            RockMigrationHelper.AddDefinedValue( "B23F1E45-BC26-4E82-BEB3-9B191FE5CCC3", "Staff", "Used to designate that a group type contains staff members.", "B76244EB-4F61-4B7C-9595-41D23650386F" );
        
            // update ou group type to assign purpose
            Sql( @"DECLARE @PurposeValueId int = (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = 'B76244EB-4F61-4B7C-9595-41D23650386F')

                UPDATE [GroupType] SET
                [GroupTypePurposeValueId] = @PurposeValueId
                WHERE [Guid] = 'AAB2E9F4-E828-4FEE-8467-73DC9DAB784C'" );

            // update missing block setting on Org Chart page
            RockMigrationHelper.AddBlockAttributeValue( "3FB66841-811D-4298-823B-06F8AFC95047", "ADCC4391-8D8B-4A28-80AF-24CD6D3F77E2", "C3909F1A-6908-4035-BB93-EC4FBFDCC536" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
