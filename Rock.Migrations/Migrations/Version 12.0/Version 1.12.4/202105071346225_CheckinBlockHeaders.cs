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
    public partial class CheckinBlockHeaders : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DeleteOldBlockTitleAttributes();
            CreateAbilityLevelSelectHeaderTemplate();
            CreateActionSelectHeaderTemplate();
            CreateCheckoutPersonSelectHeaderTemplate();
            CreateGroupSelectHeaderTemplate();
            CreateGroupTypeSelectHeaderTemplate();
            CreateLocationSelectHeaderTemplate();
            CreateMultiPersonSelectHeaderTemplate();
            CreatePersonSelectHeaderTemplate();
            CreateTimeSelectHeaderTemplate();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // remove the new attributes
            RockMigrationHelper.DeleteAttribute( "1A0F70F8-77F9-4712-A214-C2C4B68736C5"); // Rock.Model.GroupType: Ability Level Select Header Template
            RockMigrationHelper.DeleteAttribute( "91DAAB8F-34F0-46FB-AEE9-2E1405A4FE28"); // Rock.Model.GroupType: Action Select Header Template
            RockMigrationHelper.DeleteAttribute( "389906D5-9C0A-421B-BFB4-2950257F322A"); // Rock.Model.GroupType: Checkout Person Select Header Template
            RockMigrationHelper.DeleteAttribute( "81236834-ED01-4377-95C7-923EA4A6B803"); // Rock.Model.GroupType: Group Select Header Template
            RockMigrationHelper.DeleteAttribute( "DE20567B-BB5D-4E12-8B83-6ADCB92FB4CA"); // Rock.Model.GroupType: Group Type Select Header Template
            RockMigrationHelper.DeleteAttribute( "44A8BBF1-354D-4581-B6E8-189FEEBFF45F"); // Rock.Model.GroupType: Location Select Header Template
            RockMigrationHelper.DeleteAttribute( "72B1EFCB-6073-46EE-856B-65EDDFECF344"); // Rock.Model.GroupType: Multi Person Select Header Template
            RockMigrationHelper.DeleteAttribute( "0734CBDD-57D8-4CE1-9E78-226A41D5E09C"); // Rock.Model.GroupType: Person Select Header Template
            RockMigrationHelper.DeleteAttribute( "90D9CAEA-843B-4D32-84CF-E25A3258087F"); // Rock.Model.GroupType: Time Select Header Template

            // Put back the old ones
            Sql( @"
                SET IDENTITY_INSERT [Attribute] ON

                INSERT [dbo].[Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [ForeignKey], [IconCssClass], [AllowSearch], [ForeignGuid], [ForeignId], [IsIndexEnabled], [IsAnalytic], [IsAnalyticHistory])
                VALUES
                    (2716, 1, 1, 9, N'BlockTypeId', N'112', N'Title', N'Title', N'Title to display. Use {0} for family name.', 8, 0, N'{0}', 0, 0, N'8ecb1e83-97bb-435e-bfe6-40b4a33ecc9b', NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, 0, 0, 0),
                    (2720, 1, 1, 9, N'BlockTypeId', N'113', N'Title', N'Title', N'Title to display. Use {0} for person/schedule.', 9, 0, N'{0}', 0, 0, N'9ddf3190-e07f-4964-9cdc-69af675fcf2e', NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, 0, 0, 0),
                    (2725, 1, 1, 9, N'BlockTypeId', N'114', N'Title', N'Title', N'Title to display. Use {0} for person/schedule.', 8, 0, N'{0}', 0, 0, N'f95cab1d-37a4-4a53-b63f-bf9d275fba27', NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, 0, 0, 0),
                    (2728, 1, 1, 9, N'BlockTypeId', N'115', N'Title', N'Title', N'Title to display. Use {0} for person/schedule.', 8, 0, N'{0}', 0, 0, N'256d4cc4-9f09-47d3-b167-46f876f0acd3', NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, 0, 0, 0),
                    (2732, 1, 1, 9, N'BlockTypeId', N'116', N'Title', N'Title', N'Title to display. Use {0} for family/person name.', 5, 0, N'{0}', 0, 0, N'b5cf8a58-92e8-4473-be73-63fb3b6ff49e', NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, 0, 0, 0),
                    (2737, 1, 1, 9, N'BlockTypeId', N'187', N'Title', N'Title', N'Title to display. Use {0} for person''s name.', 9, 0, N'{0}', 0, 0, N'085d8ca9-82ee-40c2-8985-f3db36dc4370', NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, 0, 0, 0),
                    (2746, 1, 1, 9, N'BlockTypeId', N'414', N'Title', N'Title', N'Title to display. Use {0} for family name.', 7, 0, N'{0}', 0, 0, N'88d0b3a3-9efa-45df-97bf-7f200afa80bd', NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, 0, 0, 0),
                    (2748, 1, 1, 9, N'BlockTypeId', N'472', N'Title', N'Title', N'Title to display. Use {0} for family name', 7, 0, N'{0}', 0, 0, N'0d5b5b30-e1ce-402e-b7fb-760f8e4975b2', NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, 0, 0, 0),
                    (2750, 1, 1, 9, N'BlockTypeId', N'473', N'Title', N'Title', N'Title to display. Use {0} for family name', 5, 0, N'{0} Check Out', 0, 0, N'05ca1b42-c5f1-407c-9f35-2cf4104bc96d', NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, 0, 0, 0)

                SET IDENTITY_INSERT [Attribute] OFF" );
        }

        private void CreateAbilityLevelSelectHeaderTemplate()
        {
            // Entity: Rock.Model.GroupType Attribute: Ability Level Select Header Template
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.GroupType",
                "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5",
                "GroupTypePurposeValueId",
                "142",
                "Ability Level Select Header Template",
                "Ability Level Select Header Template",
                @"Lava template to use for the 'Ability Level Select' check-in block header. The available merge fields are:
{{ Family }} which is a Group object and is the current family.
{{ Individual }} which is a Person object and is the current selected person.
{{ SelectedArea }} is a GroupType object and corresponds to the selected check-in Area listed in Areas and Groups.",
                1058,
                @"{{ Individual.FullName }}",
                "1A0F70F8-77F9-4712-A214-C2C4B68736C5",
                "core_checkin_AbilityLevelSelectHeaderLavaTemplate");

            // Qualifier for attribute: core_checkin_AbilityLevelSelectHeaderLavaTemplate
            RockMigrationHelper.UpdateAttributeQualifier( "1A0F70F8-77F9-4712-A214-C2C4B68736C5", "editorHeight", @"", "1C22174D-53BA-4B49-94B3-991C4C9E2160");
            // Qualifier for attribute: core_checkin_AbilityLevelSelectHeaderLavaTemplate
            RockMigrationHelper.UpdateAttributeQualifier( "1A0F70F8-77F9-4712-A214-C2C4B68736C5", "editorMode", @"3", "D8900899-3967-4157-9BAE-A1B9C7C3CB5C");
            // Qualifier for attribute: core_checkin_AbilityLevelSelectHeaderLavaTemplate
            RockMigrationHelper.UpdateAttributeQualifier( "1A0F70F8-77F9-4712-A214-C2C4B68736C5", "editorTheme", @"0", "353247CB-DEB9-4D44-AB41-6358D4168A8A");
        }

        private void CreateActionSelectHeaderTemplate()
        {
            // Entity: Rock.Model.GroupType Attribute: Action Select Header Template
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.GroupType",
                "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5",
                "GroupTypePurposeValueId",
                "142",
                "Action Select Header Template",
                "Action Select Header Template",
                @"Lava template to use for the 'Action Select' check-in block header. The available merge fields are:
{{ Family }} which is a Group object and is the current family.",
                1059,
                @"{{ Family.Name }}",
                "91DAAB8F-34F0-46FB-AEE9-2E1405A4FE28",
                "core_checkin_ActionSelectHeaderLavaTemplate");
            
            // Qualifier for attribute: core_checkin_ActionSelectHeaderLavaTemplate
            RockMigrationHelper.UpdateAttributeQualifier( "91DAAB8F-34F0-46FB-AEE9-2E1405A4FE28", "editorHeight", @"", "0C024FE8-47D7-4D74-8354-FFEA05354D8C");
            // Qualifier for attribute: core_checkin_ActionSelectHeaderLavaTemplate
            RockMigrationHelper.UpdateAttributeQualifier( "91DAAB8F-34F0-46FB-AEE9-2E1405A4FE28", "editorMode", @"3", "811CBCB6-0929-454B-B35C-5709A6378572");
            // Qualifier for attribute: core_checkin_ActionSelectHeaderLavaTemplate
            RockMigrationHelper.UpdateAttributeQualifier( "91DAAB8F-34F0-46FB-AEE9-2E1405A4FE28", "editorTheme", @"0", "6BD268F1-124E-423A-AEF0-5117189E0B2C");

        }

        private void CreateCheckoutPersonSelectHeaderTemplate()
        {
            // Entity: Rock.Model.GroupType Attribute: Checkout Person Select Header Template
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.GroupType",
                "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5",
                "GroupTypePurposeValueId",
                "142",
                "Checkout Person Select Header Template",
                "Checkout Person Select Header Template",
                @"Lava template to use for the 'Checkout Person Select' check-in block header. The available merge fields are:
{{ Family }} which is a Group object and is the current family.",
                1060,
                @"{{ Family.Name }} Check Out",
                "389906D5-9C0A-421B-BFB4-2950257F322A",
                "core_checkin_CheckoutPersonSelectHeaderLavaTemplate");
            
            // Qualifier for attribute: core_checkin_CheckoutPersonSelectHeaderLavaTemplate
            RockMigrationHelper.UpdateAttributeQualifier( "389906D5-9C0A-421B-BFB4-2950257F322A", "editorHeight", @"", "28AAAF5E-0B0B-4653-8E54-AD6518753100");
            // Qualifier for attribute: core_checkin_CheckoutPersonSelectHeaderLavaTemplate
            RockMigrationHelper.UpdateAttributeQualifier( "389906D5-9C0A-421B-BFB4-2950257F322A", "editorMode", @"3", "B3D1203B-7C21-423B-9E9E-BE75FEEB18A1");
            // Qualifier for attribute: core_checkin_CheckoutPersonSelectHeaderLavaTemplate
            RockMigrationHelper.UpdateAttributeQualifier( "389906D5-9C0A-421B-BFB4-2950257F322A", "editorTheme", @"0", "C83F98C9-9DF7-4535-8989-409BE71E5793");

        }

        private void CreateGroupSelectHeaderTemplate()
        {
            // Entity: Rock.Model.GroupType Attribute: Group Select Header Template
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.GroupType",
                "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5",
                "GroupTypePurposeValueId",
                "142",
                "Group Select Header Template",
                "Group Select Header Template",
                @"Lava template to use for the 'Group Select' check-in block header. The available merge fields are:
{{ Family }} which is a Group object and is the current family.
{{ Individual }} which is a Person object and is the current selected person.
{{ SelectedArea }} is a GroupType object and corresponds to the selected check-in Area listed in Areas and Groups.
{{ SelectedGroup }} is a Group object and corresponds to the selected check-in group listed in Areas and Groups.",
                1061,
                @"{% if SelectedSchedule != empty %}
    {{ Individual.FullName }} @ {{ SelectedSchedule.Name }}
{% else %}}
    {{ Individual.FullName }}
{% endif %}",
                "81236834-ED01-4377-95C7-923EA4A6B803",
                "core_checkin_GroupSelectHeaderLavaTemplate");

            // Qualifier for attribute: core_checkin_GroupSelectHeaderLavaTemplate
            RockMigrationHelper.UpdateAttributeQualifier( "81236834-ED01-4377-95C7-923EA4A6B803", "editorHeight", @"", "833AA128-6471-4160-93D7-6D339B75E38C");
            // Qualifier for attribute: core_checkin_GroupSelectHeaderLavaTemplate
            RockMigrationHelper.UpdateAttributeQualifier( "81236834-ED01-4377-95C7-923EA4A6B803", "editorMode", @"3", "5B9EB078-18DF-45F1-A3C6-2816A506811F");
            // Qualifier for attribute: core_checkin_GroupSelectHeaderLavaTemplate
            RockMigrationHelper.UpdateAttributeQualifier( "81236834-ED01-4377-95C7-923EA4A6B803", "editorTheme", @"0", "44282521-8CF5-4B5C-9ABB-67473AEBB5BF");
        }

        private void CreateGroupTypeSelectHeaderTemplate()
        {
            // Entity: Rock.Model.GroupType Attribute: Group Type Select Header Template
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.GroupType",
                "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5",
                "GroupTypePurposeValueId",
                "142",
                "Group Type Select Header Template",
                "Group Type Select Header Template",
                @"Lava template to use for the 'Group Type Select' check-in block header. The available merge fields are:
{{ Family }} which is a Group object and is the current family.
{{ Individual }} which is a Person object and is the current selected person.
{{ SelectedSchedule}} is a Schedule object and is the current selected schedule.",
                1062,
                @"{% if SelectedSchedule != empty %}
    {{ Individual.FullName }} @ {{ SelectedSchedule.Name }}
{% else %}}
    {{ Individual.FullName }}
{% endif %}",
                "DE20567B-BB5D-4E12-8B83-6ADCB92FB4CA",
                "core_checkin_GroupTypeSelectHeaderLavaTemplate");
            
            // Qualifier for attribute: core_checkin_GroupTypeSelectHeaderLavaTemplate
            RockMigrationHelper.UpdateAttributeQualifier( "DE20567B-BB5D-4E12-8B83-6ADCB92FB4CA", "editorHeight", @"", "608AB7AF-5C8B-477E-981C-9E1BF6EC83F1");
            // Qualifier for attribute: core_checkin_GroupTypeSelectHeaderLavaTemplate
            RockMigrationHelper.UpdateAttributeQualifier( "DE20567B-BB5D-4E12-8B83-6ADCB92FB4CA", "editorMode", @"3", "C8C43E84-7C7C-485B-B513-D41A302F3235");
            // Qualifier for attribute: core_checkin_GroupTypeSelectHeaderLavaTemplate
            RockMigrationHelper.UpdateAttributeQualifier( "DE20567B-BB5D-4E12-8B83-6ADCB92FB4CA", "editorTheme", @"0", "926684B4-EADA-473D-B18B-896DB20821CB");
        }

        private void CreateLocationSelectHeaderTemplate()
        {
            // Entity: Rock.Model.GroupType Attribute: Location Select Header Template
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.GroupType",
                "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5",
                "GroupTypePurposeValueId",
                "142",
                "Location Select Header Template",
                "Location Select Header Template",
                @"Lava template to use for the 'Location Select' check-in block header. The available merge fields are:
{{ Family }} which is a Group object and is the current family.
{{ Individual }} which is a Person object and is the current selected person.
{{ SelectedGroup }} is a Group object and corresponds to the selected check-in group listed in Areas and Groups.
{{ SelectedSchedule}} is a Schedule object and is the current selected schedule.",
                1063,
                @"{% if SelectedSchedule != empty %}
    {{ Individual.FullName }} @ {{ SelectedSchedule.Name }}
{% else %}}
    {{ Individual.FullName }}
{% endif %}",
                "44A8BBF1-354D-4581-B6E8-189FEEBFF45F",
                "core_checkin_LocationSelectHeaderLavaTemplate");

            // Qualifier for attribute: core_checkin_LocationSelectHeaderLavaTemplate
            RockMigrationHelper.UpdateAttributeQualifier( "44A8BBF1-354D-4581-B6E8-189FEEBFF45F", "editorHeight", @"", "D61490A6-4441-4831-9424-37BC1AA28486");
            // Qualifier for attribute: core_checkin_LocationSelectHeaderLavaTemplate
            RockMigrationHelper.UpdateAttributeQualifier( "44A8BBF1-354D-4581-B6E8-189FEEBFF45F", "editorMode", @"3", "AFC1FF73-15C8-4505-9913-9B80C1A4760E");
            // Qualifier for attribute: core_checkin_LocationSelectHeaderLavaTemplate
            RockMigrationHelper.UpdateAttributeQualifier( "44A8BBF1-354D-4581-B6E8-189FEEBFF45F", "editorTheme", @"0", "5316055E-D633-4827-B461-4128A48E52B6");
        }

        private void CreateMultiPersonSelectHeaderTemplate()
        {
            // Entity: Rock.Model.GroupType Attribute: Multi Person Select Header Template
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.GroupType",
                "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5",
                "GroupTypePurposeValueId",
                "142",
                "Multi Person Select Header Template",
                "Multi Person Select Header Template",
                @"Lava template to use for the 'Multi Person Select' check-in block header. The available merge fields are:
{{ Family }} which is a Group object and is the current family.",
                1064,
                @"{{ Family.Name }}",
                "72B1EFCB-6073-46EE-856B-65EDDFECF344",
                "core_checkin_MultiPersonSelectHeaderLavaTemplate");
            
            // Qualifier for attribute: core_checkin_MultiPersonSelectHeaderLavaTemplate
            RockMigrationHelper.UpdateAttributeQualifier( "72B1EFCB-6073-46EE-856B-65EDDFECF344", "editorHeight", @"", "16B66DF7-45C2-4261-8DD9-B51D6A007643");
            // Qualifier for attribute: core_checkin_MultiPersonSelectHeaderLavaTemplate
            RockMigrationHelper.UpdateAttributeQualifier( "72B1EFCB-6073-46EE-856B-65EDDFECF344", "editorMode", @"3", "0314B28E-C39A-46A7-A1BD-1FCD935AD7D5");
            // Qualifier for attribute: core_checkin_MultiPersonSelectHeaderLavaTemplate
            RockMigrationHelper.UpdateAttributeQualifier( "72B1EFCB-6073-46EE-856B-65EDDFECF344", "editorTheme", @"0", "F9FA819A-0B79-44D3-8F46-1F927ADCD357");
        }

        private void CreatePersonSelectHeaderTemplate()
        {
            // Entity: Rock.Model.GroupType Attribute: Person Select Header Template
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.GroupType",
                "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5",
                "GroupTypePurposeValueId",
                "142",
                "Person Select Header Template",
                "Person Select Header Template",
                @"Lava template to use for the 'Person Select' check-in block header. The available merge fields are:
{{ Family }} which is a Group object and is the current family.",
                1065,
                @"{{ Family.Name }}",
                "0734CBDD-57D8-4CE1-9E78-226A41D5E09C",
                "core_checkin_PersonSelectHeaderLavaTemplate");
            
            // Qualifier for attribute: core_checkin_PersonSelectHeaderLavaTemplate
            RockMigrationHelper.UpdateAttributeQualifier( "0734CBDD-57D8-4CE1-9E78-226A41D5E09C", "editorHeight", @"", "F874F44A-97A3-4F8E-AD02-7CA7C88930C3");
            // Qualifier for attribute: core_checkin_PersonSelectHeaderLavaTemplate
            RockMigrationHelper.UpdateAttributeQualifier( "0734CBDD-57D8-4CE1-9E78-226A41D5E09C", "editorMode", @"3", "0508211A-1138-4A91-8EBA-2A7786D78A4D");
            // Qualifier for attribute: core_checkin_PersonSelectHeaderLavaTemplate
            RockMigrationHelper.UpdateAttributeQualifier( "0734CBDD-57D8-4CE1-9E78-226A41D5E09C", "editorTheme", @"0", "C6884DD9-18A3-4323-8355-498DC2217710");
        }

        private void CreateTimeSelectHeaderTemplate()
        {
            // Entity: Rock.Model.GroupType Attribute: Time Select Header Template
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.GroupType",
                "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", 
                "GroupTypePurposeValueId", 
                "142", 
                "Time Select Header Template", 
                "Time Select Header Template",
                @"Lava template to use for the 'Time Select' check-in block header. The available merge fields are:
{{ Family }} which is a Group object and is the current family.
{{ SelectedIndividuals }} is a list of Person objects which contains all of the currently selected persons.
{{ CheckinType }} is the type of check-in given as a string which will be either 'Family' or 'Individual'.", 
                1066, 
                @"{% if CheckinType == 'Family' %}
    {{ Family.Name }}
{% else %}
    {% assign selectedIndividual = SelectedIndividuals | First %}
    {{ selectedIndividual.FullName }}
{% endif %}",
                "90D9CAEA-843B-4D32-84CF-E25A3258087F", 
                "core_checkin_TimeSelectHeaderLavaTemplate");

            // Qualifier for attribute: core_checkin_TimeSelectHeaderLavaTemplate
            RockMigrationHelper.UpdateAttributeQualifier( "90D9CAEA-843B-4D32-84CF-E25A3258087F", "editorHeight", @"", "CDD797B8-6027-4DDA-AC86-FE531820A73B");
            // Qualifier for attribute: core_checkin_TimeSelectHeaderLavaTemplate
            RockMigrationHelper.UpdateAttributeQualifier( "90D9CAEA-843B-4D32-84CF-E25A3258087F", "editorMode", @"3", "1B3C1913-5432-4993-AED4-469A71A2B76B");
            // Qualifier for attribute: core_checkin_TimeSelectHeaderLavaTemplate
            RockMigrationHelper.UpdateAttributeQualifier( "90D9CAEA-843B-4D32-84CF-E25A3258087F", "editorTheme", @"0", "503AD87F-8622-4C62-8349-D13D059FB1B5");
        }

        private void DeleteOldBlockTitleAttributes()
        {
            Sql( @"
                DELETE FROM [Attribute] WHERE [Id] IN (
                SELECT a.[Id]
                FROM [Attribute] a
                JOIN [BlockType] bt on a.[EntityTypeQualifierValue] = bt.[Id]
                WHERE a.[EntityTypeQualifierColumn] = 'BlockTypeId'
	                AND a.[Key] = 'Title'
	                AND a.[EntityTypeId] = (SELECT [Id] FROM [EntityType] WHERE [Guid] = 'D89555CA-9AE4-4D62-8AF1-E5E463C1EF65')
	                AND  bt.[Path] IN (
		                '~/Blocks/CheckIn/AbilityLevelSelect.ascx',
		                '~/Blocks/CheckIn/ActionSelect.ascx',
		                '~/Blocks/CheckIn/CheckOutPersonSelect.ascx',
		                '~/Blocks/CheckIn/GroupSelect.ascx',
		                '~/Blocks/CheckIn/GroupTypeSelect.ascx',
		                '~/Blocks/CheckIn/LocationSelect.ascx',
		                '~/Blocks/CheckIn/MultiPersonSelect.ascx',
		                '~/Blocks/CheckIn/PersonSelect.ascx',
		                '~/Blocks/CheckIn/TimeSelect.ascx')
                )" );
        }
    }
}
