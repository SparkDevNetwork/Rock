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

    using Rock.Security;
    
    /// <summary>
    ///
    /// </summary>
    public partial class GroupLocationPersonId : Rock.Migrations.RockMigration1
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Update the Admin's password to be encoded using the current passwordKey setting in web.config
            string encryptionKey = System.Configuration.ConfigurationManager.AppSettings["PasswordKey"];
            if ( !String.IsNullOrWhiteSpace( encryptionKey ) )
            {
                var hash = new System.Security.Cryptography.HMACSHA1();
                hash.Key = Encryption.HexToByte( encryptionKey );
                string newPassword = Convert.ToBase64String( hash.ComputeHash( System.Text.Encoding.Unicode.GetBytes( "admin" ) ) );

                Sql( string.Format( "Update [UserLogin] SET [Password] = '{0}' WHERE [UserName] = 'Admin'", newPassword ) );
            }

            AddColumn("dbo.GroupLocation", "GroupMemberPersonId", c => c.Int());
            CreateIndex("dbo.GroupLocation", "GroupMemberPersonId");
            AddForeignKey("dbo.GroupLocation", "GroupMemberPersonId", "dbo.Person", "Id", cascadeDelete: true);
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.GroupLocation", "GroupMemberPersonId", "dbo.Person");
            DropIndex("dbo.GroupLocation", new[] { "GroupMemberPersonId" });
            DropColumn("dbo.GroupLocation", "GroupMemberPersonId");
        }
    }
}
