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
    public partial class WaitlistConfirmationFields : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.RegistrationTemplate", "WaitlistConfirmationSubject", c => c.String( maxLength: 200 ) );
            AddColumn( "dbo.RegistrationTemplate", "WaitlistConfirmationEmailTemplate", c => c.String() );
            AddColumn( "dbo.RegistrationTemplate", "WaitlistConfirmationMessage", c => c.String() );
            AddColumn("dbo.RegistrationTemplateFormField", "ShowOnWaitlist", c => c.Boolean(nullable: false));
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.RegistrationTemplateFormField", "ShowOnWaitlist");
            DropColumn( "dbo.RegistrationTemplate", "WaitlistConfirmationMessage" );
            DropColumn( "dbo.RegistrationTemplate", "WaitlistConfirmationEmailTemplate" );
            DropColumn( "dbo.RegistrationTemplate", "WaitlistConfirmationSubject" );
        }
    }
}
