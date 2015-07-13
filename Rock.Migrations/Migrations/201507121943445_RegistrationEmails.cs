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
    public partial class RegistrationEmails : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.RegistrationTemplate", "Notify", c => c.Int(nullable: false));
            DropColumn("dbo.RegistrationTemplate", "NotifyGroupLeaders");

            RockMigrationHelper.UpdateCategoryByName( "B21FD119-893E-46C0-B42D-E4CDD5C8C49D", "Event Registration", "fa fa-folder", "", "4A7D0D1F-E160-445E-9D29-AEBD140DA242", 5 );
            Sql( MigrationSQL._201507121943445_RegistrationEmails );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteSystemEmail( "7B0F4F06-69BD-4CB4-BD04-8DA3779D5259" );
            RockMigrationHelper.DeleteSystemEmail( "158607D1-0772-4947-ADD6-EA31AB6ABC2F" );

            AddColumn( "dbo.RegistrationTemplate", "NotifyGroupLeaders", c => c.Boolean( nullable: false ) );
            DropColumn("dbo.RegistrationTemplate", "Notify");
        }
    }
}
