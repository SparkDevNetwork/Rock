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
    public partial class ExtendDiscountCodes : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.RegistrationRegistrant", "DiscountApplies", c => c.Boolean(nullable: false));

            AddColumn("dbo.RegistrationTemplateDiscount", "MaxUsage", c => c.Int());
            AddColumn("dbo.RegistrationTemplateDiscount", "MaxRegistrants", c => c.Int());
            AddColumn("dbo.RegistrationTemplateDiscount", "MinRegistrants", c => c.Int());
            AddColumn("dbo.RegistrationTemplateDiscount", "StartDate", c => c.DateTime());
            AddColumn("dbo.RegistrationTemplateDiscount", "EndDate", c => c.DateTime());

            Sql( @"
    UPDATE [RegistrationRegistrant] SET [DiscountApplies] = 1
" );
            // Fix issue with Waitlist not getting enabled for all existing fields by default
            Sql( @"
    UPDATE [RegistrationTemplateFormField] SET [ShowOnWaitlist] = 1
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.RegistrationTemplateDiscount", "EndDate");
            DropColumn("dbo.RegistrationTemplateDiscount", "StartDate");
            DropColumn("dbo.RegistrationTemplateDiscount", "MinRegistrants");
            DropColumn("dbo.RegistrationTemplateDiscount", "MaxRegistrants");
            DropColumn("dbo.RegistrationTemplateDiscount", "MaxUsage");

            DropColumn("dbo.RegistrationRegistrant", "DiscountApplies");
        }
    }
}
