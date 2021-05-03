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
    public partial class Rollup_0919 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddGroupTypeEntityAttributes();
            FixNonCashDefinedValue();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// ED: Add GroupType Entity attributes for Check-in Edit Family
        /// </summary>
        private void AddGroupTypeEntityAttributes()
        {
            Sql( @"
                DECLARE @GroupTypeEntityId INT = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.GroupType')
                DECLARE @CheckInTemplateGroupTypeId INT = (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '4A406CB0-495B-4795-B788-52BDFDE00B01')
                DECLARE @BooleanFieldTypeId INT = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '1EDAFDED-DFE6-4334-B019-6EECBA89E05A')

                IF NOT EXISTS (SELECT * FROM [Attribute] WHERE [Key] = 'core_checkin_registration_DisplaySmsButton')
                BEGIN
	                INSERT INTO Attribute(
		                  [IsSystem]
		                , [FieldTypeId]
		                , [EntityTypeId]
		                , [EntityTypeQualifierColumn]
		                , [EntityTypeQualifierValue]
		                , [Key]
		                , [Name]
		                , [Order]
		                , [IsGridColumn]
		                , [IsMultiValue]
		                , [IsRequired]
		                , [Guid]
		                , [AllowSearch]
		                , [IsIndexEnabled]
		                , [IsAnalytic]
		                , [IsAnalyticHistory]
		                , [IsActive]
		                , [EnableHistory]
		                , [ShowOnBulk]
		                , [IsPublic])
	                VALUES(
	                      1
	                    , @BooleanFieldTypeId
	                    , @GroupTypeEntityId
	                    , 'GroupTypePurposeValueId'
	                    , @CheckInTemplateGroupTypeId
	                    , 'core_checkin_registration_DisplaySmsButton'
	                    , 'Display SMS Enabled Selection for Phone Number'
	                    , 0
	                    , 0
	                    , 0
	                    , 0
	                    , '85AF0B4D-4D37-467B-9A82-B98D13D3555D'
	                    , 0
	                    , 0
	                    , 0
	                    , 0
	                    , 1
	                    , 0
	                    , 0
	                    , 0)
                END

                IF NOT EXISTS (SELECT * FROM [Attribute] WHERE [Key] = 'core_checkin_registration_DefaultSmsEnabled')
                BEGIN
	                INSERT INTO Attribute(
		                  [IsSystem]
		                , [FieldTypeId]
		                , [EntityTypeId]
		                , [EntityTypeQualifierColumn]
		                , [EntityTypeQualifierValue]
		                , [Key]
		                , [Name]
		                , [Order]
		                , [IsGridColumn]
		                , [IsMultiValue]
		                , [IsRequired]
		                , [Guid]
		                , [AllowSearch]
		                , [IsIndexEnabled]
		                , [IsAnalytic]
		                , [IsAnalyticHistory]
		                , [IsActive]
		                , [EnableHistory]
		                , [ShowOnBulk]
		                , [IsPublic])
	                VALUES(
                          1
	                    , @BooleanFieldTypeId
	                    , @GroupTypeEntityId
	                    , 'GroupTypePurposeValueId'
	                    , @CheckInTemplateGroupTypeId
	                    , 'core_checkin_registration_DefaultSmsEnabled'
	                    , 'Set the SMS Enabled for the phone number by default'
	                    , 0
	                    , 0
	                    , 0
	                    , 0
	                    , '8777BFD7-33EF-4F34-AD87-260755AFF529'
	                    , 0
	                    , 0
	                    , 0
	                    , 0
	                    , 1
	                    , 0
	                    , 0
	                    , 0)
                END" );
        }

        /// <summary>
        /// ED: Fix Non-Cash defined value for currency type defined type
        /// </summary>
        private void FixNonCashDefinedValue()
        {
            Sql( @"
                -- Known GUID FINANCIAL_CURRENCY_TYPE   1D1304DE-E83A-44AF-B11D-0C66DD600B81;
                -- Known GUID CURRENCY_TYPE_NONCASH     7950FF66-80EE-E8AB-4A77-4A13EDEB7513;

                -- First get the currency type defined type ID
                DECLARE @currencyTypeDefinedTypeId INT = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = '1D1304DE-E83A-44AF-B11D-0C66DD600B81')

				-- Check if the Non-Cash DefinedValue exists
                IF EXISTS (SELECT * FROM [DefinedValue] WHERE [Guid] = '7950FF66-80EE-E8AB-4A77-4A13EDEB7513')
                BEGIN
                 -- Make sure this is flagged as a system DefinedValue so it cannot be deleted using the UI
                 UPDATE [DefinedValue] SET [IsSystem] = 1, [IsActive] = 1, [Value] = 'Non-Cash Asset' WHERE [Guid] = '7950FF66-80EE-E8AB-4A77-4A13EDEB7513'
                END
                ELSE 
				BEGIN
                 -- Insert the core version of the Non-Cash Asset currency type
                 INSERT INTO [DefinedValue] ([IsSystem], [DefinedTypeId], [Order], [Value], [Description], [Guid], [IsActive])
                 VALUES(1, @currencyTypeDefinedTypeId, 99, 'Non-Cash Asset', 'Used to track non-cash transactions.', '7950FF66-80EE-E8AB-4A77-4A13EDEB7513', 1)
                END" );
        }
    }
}
