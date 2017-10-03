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
    public partial class Analytics2 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Drop views that were pointing to AnalyticsDimDate and re-create them after the rename
            Sql( @"
IF OBJECT_ID(N'[dbo].[AnalyticsDimAttendanceDate]', 'V') IS NOT NULL
    DROP VIEW AnalyticsDimAttendanceDate
GO

IF OBJECT_ID(N'[dbo].[AnalyticsDimFinancialTransactionDate]', 'V') IS NOT NULL
    DROP VIEW AnalyticsDimFinancialTransactionDate
GO

IF OBJECT_ID(N'[dbo].[AnalyticsDimPersonBirthDate]', 'V') IS NOT NULL
    DROP VIEW AnalyticsDimPersonBirthDate
GO
" );

            RenameTable(name: "dbo.AnalyticsDimDate", newName: "AnalyticsSourceDate");


            Sql( MigrationSQL._201709191729044_Analytics2_AnalyticsDimAttendanceDate );
            Sql( MigrationSQL._201709191729044_Analytics2_AnalyticsDimFamilyHeadOfHouseholdBirthDate );
            Sql( MigrationSQL._201709191729044_Analytics2_AnalyticsDimFinancialTransactionDate );
            Sql( MigrationSQL._201709191729044_Analytics2_AnalyticsDimPersonCurrentBirthDate );
            Sql( MigrationSQL._201709191729044_Analytics2_AnalyticsDimPersonHistoricalBirthDate );

            // DT: vCard Format
            RockMigrationHelper.AddGlobalAttribute( Rock.SystemGuid.FieldType.CODE_EDITOR, "", "", "vCard Format", "Lava format to use when creating a vCard for a person", 0, @"
BEGIN:VCARD
VERSION:2.1
N:{{ Person.LastName }};{{ Person.NickName }}
FN:{{ Person.FullName }} 
{% assign cellPhone = Person | PhoneNumber:''Mobile'', true %}{% if cellPhone and cellPhone != '''' %}TEL;PREF;CELL:{{ cellPhone }}{% endif %}
{% assign homePhone = Person | PhoneNumber:''Home'', true %}{% if homePhone and homePhone != '''' %}TEL;HOME:{{ homePhone }}{% endif %}
{% assign workPhone = Person | PhoneNumber:''Work'', true %}{% if workPhone and workPhone != '''' %}TEL;WORK:{{ workPhone }}{% endif %}
{% if Person.Email and Person.Email != '''' %}EMAIL;PREF;HOME:{{ Person.Email}}{% endif %}
{{ Person | Address:''Home'',''ADR;HOME;PREF:;;[[Street1]];[[City]];[[State]];[[PostalCode]]'' }}
{{ Person | Address:''Work'',''ADR;WORK:;;[[Street1]];[[City]];[[State]];[[PostalCode]]'' }}
{% assign employer = Person | Attribute:''Employer'' %}{% if employer and employer != '''' %}ORG:{{ employer }}{% endif %}
{% assign position = Person | Attribute:''Position'' %}{% if position and position != '''' %}TITLE:{{ position }}{% endif %}
{% if Person.PhotoId %}PHOTO;ENCODING=BASE64;TYPE=JPEG:{{ Person.PhotoId | Base64Encode:''h=92&w=92&mode=max&format=jpg'' }}{% endif %}


END:VCARD
", "DC0C0ED9-3347-4EBA-A6B5-CC5A8F547F92", "VCardFormat" );
            RockMigrationHelper.AddAttributeQualifier( "DC0C0ED9-3347-4EBA-A6B5-CC5A8F547F92", "editorHeight", "400", "41C08E15-2B78-4F50-9BC6-5A3217B542BD" );
            RockMigrationHelper.AddAttributeQualifier( "DC0C0ED9-3347-4EBA-A6B5-CC5A8F547F92", "editorMode", "3", "C28D3C9C-586B-40F2-BDEA-5BF67F2ED0C9" );
            RockMigrationHelper.AddAttributeQualifier( "DC0C0ED9-3347-4EBA-A6B5-CC5A8F547F92", "editorTheme", "0", "A07ED4B6-9391-459A-AA4D-472926F259C9" );
        }



        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RenameTable(name: "dbo.AnalyticsSourceDate", newName: "AnalyticsDimDate");
        }
    }
}
