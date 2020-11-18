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

    using Rock.Model;

    /// <summary>
    ///
    /// </summary>
    public partial class WorkflowActionFormAllowPersonEntry : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.WorkflowActionForm", "AllowPersonEntry", c => c.Boolean( nullable: false ) );
            AddColumn( "dbo.WorkflowActionForm", "PersonEntryPreHtml", c => c.String() );
            AddColumn( "dbo.WorkflowActionForm", "PersonEntryPostHtml", c => c.String() );
            
            // set SQL defaults for these 
            AddColumn( "dbo.WorkflowActionForm", "PersonEntryCampusIsVisible", c => c.Boolean( nullable: false, defaultValue: true ) );
            AddColumn( "dbo.WorkflowActionForm", "PersonEntryAutofillCurrentPerson", c => c.Boolean( nullable: false, defaultValue: true ) );
            AddColumn( "dbo.WorkflowActionForm", "PersonEntryHideIfCurrentPersonKnown", c => c.Boolean( nullable: false, defaultValue: true ) );
            AddColumn( "dbo.WorkflowActionForm", "PersonEntrySpouseEntryOption", c => c.Int( nullable: false, defaultValue: ( int ) WorkflowActionFormPersonEntryOption.Hidden ) );
            AddColumn( "dbo.WorkflowActionForm", "PersonEntryEmailEntryOption", c => c.Int( nullable: false, defaultValue: ( int ) WorkflowActionFormPersonEntryOption.Required ) );
            AddColumn( "dbo.WorkflowActionForm", "PersonEntryMobilePhoneEntryOption", c => c.Int( nullable: false, defaultValue: ( int ) WorkflowActionFormPersonEntryOption.Hidden ) );
            AddColumn( "dbo.WorkflowActionForm", "PersonEntryBirthdateEntryOption", c => c.Int( nullable: false, defaultValue: ( int ) WorkflowActionFormPersonEntryOption.Hidden ) );
            AddColumn( "dbo.WorkflowActionForm", "PersonEntryAddressEntryOption", c => c.Int( nullable: false, defaultValue: ( int ) WorkflowActionFormPersonEntryOption.Hidden ) );
            AddColumn( "dbo.WorkflowActionForm", "PersonEntryMaritalStatusEntryOption", c => c.Int( nullable: false, defaultValue: ( int ) WorkflowActionFormPersonEntryOption.Hidden ) );
            AddColumn( "dbo.WorkflowActionForm", "PersonEntrySpouseLabel", c => c.String( maxLength: 50, nullable:false, defaultValue: "Spouse" ) );
            
            AddColumn( "dbo.WorkflowActionForm", "PersonEntryConnectionStatusValueId", c => c.Int() );
            AddColumn( "dbo.WorkflowActionForm", "PersonEntryRecordStatusValueId", c => c.Int() );
            AddColumn( "dbo.WorkflowActionForm", "PersonEntryGroupLocationTypeValueId", c => c.Int() );
            AddColumn( "dbo.WorkflowActionForm", "PersonEntryPersonAttributeGuid", c => c.Guid() );
            AddColumn( "dbo.WorkflowActionForm", "PersonEntrySpouseAttributeGuid", c => c.Guid() );
            AddColumn( "dbo.WorkflowActionForm", "PersonEntryFamilyAttributeGuid", c => c.Guid() );
            CreateIndex( "dbo.WorkflowActionForm", "PersonEntryConnectionStatusValueId" );
            CreateIndex( "dbo.WorkflowActionForm", "PersonEntryRecordStatusValueId" );
            CreateIndex( "dbo.WorkflowActionForm", "PersonEntryGroupLocationTypeValueId" );
            AddForeignKey( "dbo.WorkflowActionForm", "PersonEntryConnectionStatusValueId", "dbo.DefinedValue", "Id" );
            AddForeignKey( "dbo.WorkflowActionForm", "PersonEntryGroupLocationTypeValueId", "dbo.DefinedValue", "Id" );
            AddForeignKey( "dbo.WorkflowActionForm", "PersonEntryRecordStatusValueId", "dbo.DefinedValue", "Id" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey( "dbo.WorkflowActionForm", "PersonEntryRecordStatusValueId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.WorkflowActionForm", "PersonEntryGroupLocationTypeValueId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.WorkflowActionForm", "PersonEntryConnectionStatusValueId", "dbo.DefinedValue" );
            DropIndex( "dbo.WorkflowActionForm", new[] { "PersonEntryGroupLocationTypeValueId" } );
            DropIndex( "dbo.WorkflowActionForm", new[] { "PersonEntryRecordStatusValueId" } );
            DropIndex( "dbo.WorkflowActionForm", new[] { "PersonEntryConnectionStatusValueId" } );
            DropColumn( "dbo.WorkflowActionForm", "PersonEntryFamilyAttributeGuid" );
            DropColumn( "dbo.WorkflowActionForm", "PersonEntrySpouseAttributeGuid" );
            DropColumn( "dbo.WorkflowActionForm", "PersonEntryPersonAttributeGuid" );
            DropColumn( "dbo.WorkflowActionForm", "PersonEntryGroupLocationTypeValueId" );
            DropColumn( "dbo.WorkflowActionForm", "PersonEntryRecordStatusValueId" );
            DropColumn( "dbo.WorkflowActionForm", "PersonEntryConnectionStatusValueId" );
            DropColumn( "dbo.WorkflowActionForm", "PersonEntrySpouseLabel" );
            DropColumn( "dbo.WorkflowActionForm", "PersonEntryMaritalStatusEntryOption" );
            DropColumn( "dbo.WorkflowActionForm", "PersonEntryAddressEntryOption" );
            DropColumn( "dbo.WorkflowActionForm", "PersonEntryBirthdateEntryOption" );
            DropColumn( "dbo.WorkflowActionForm", "PersonEntryMobilePhoneEntryOption" );
            DropColumn( "dbo.WorkflowActionForm", "PersonEntryEmailEntryOption" );
            DropColumn( "dbo.WorkflowActionForm", "PersonEntrySpouseEntryOption" );
            DropColumn( "dbo.WorkflowActionForm", "PersonEntryHideIfCurrentPersonKnown" );
            DropColumn( "dbo.WorkflowActionForm", "PersonEntryAutofillCurrentPerson" );
            DropColumn( "dbo.WorkflowActionForm", "PersonEntryCampusIsVisible" );
            DropColumn( "dbo.WorkflowActionForm", "PersonEntryPostHtml" );
            DropColumn( "dbo.WorkflowActionForm", "PersonEntryPreHtml" );
            DropColumn( "dbo.WorkflowActionForm", "AllowPersonEntry" );
        }
    }
}
