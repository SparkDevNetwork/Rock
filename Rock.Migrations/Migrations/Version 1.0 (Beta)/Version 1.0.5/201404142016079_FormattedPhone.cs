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
    public partial class FormattedPhone : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.PhoneNumber", "NumberFormatted", c => c.String(maxLength: 50));

            // Add Smart Search block
            UpdateBlockType( "Smart Search", "Provides extensible options for searching in Rock.", "~/Blocks/Core/SmartSearch.ascx", "Core", "9D406BD5-88C1-45E5-AFEA-70F9CFB66C74" );
            AddBlock( "", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "9D406BD5-88C1-45E5-AFEA-70F9CFB66C74", "Smart Search", "Header", "", "", 0, "043CDF73-42DB-4F3A-B2BC-17BE8931097C" );    // Full Width
            AddBlock( "", "195BCD57-1C10-4969-886F-7324B6287B75", "9D406BD5-88C1-45E5-AFEA-70F9CFB66C74", "Smart Search", "Header", "", "", 0, "90D6CEFA-DD25-431A-B9AF-A3ED60A244B8" );    // Full Width Panel
            AddBlock( "", "0CB60906-6B74-44FD-AB25-026050EF70EB", "9D406BD5-88C1-45E5-AFEA-70F9CFB66C74", "Smart Search", "Header", "", "", 0, "7CB706DC-9B70-4E45-A2DE-E44C3C678CC1" );    // Left Sidebar
            AddBlock( "", "6AC471A3-9B0E-459B-ADA2-F6E18F970803", "9D406BD5-88C1-45E5-AFEA-70F9CFB66C74", "Smart Search", "Header", "", "", 0, "191CF341-94CA-4AFE-9DE1-4764FCEB51BF" );    // Left Sidebar Panel
            AddBlock( "", "22D220B5-0D34-429A-B9E3-59D80AE423E7", "9D406BD5-88C1-45E5-AFEA-70F9CFB66C74", "Smart Search", "Header", "", "", 0, "46621E1C-85A0-4D68-8B9E-A14832C1EDFB" );    // Right Sidebar
            AddBlock( "", "BACA6FF2-A228-4C47-9577-2BBDFDFD26BA", "9D406BD5-88C1-45E5-AFEA-70F9CFB66C74", "Smart Search", "Header", "", "", 0, "078B9ECD-FC7D-4376-80BD-CBCCD19F5A41" );    // Right Sidebar Panel
            AddBlock( "", "EDFE06F4-D329-4340-ACD8-68A60CD112E6", "9D406BD5-88C1-45E5-AFEA-70F9CFB66C74", "Smart Search", "Header", "", "", 0, "35EFE2C5-BFDA-4AF1-8D90-B96F53CA1EA8" );    // Three Column
            AddBlock( "", "F66758C6-3E3D-4598-AF4C-B317047B5987", "9D406BD5-88C1-45E5-AFEA-70F9CFB66C74", "Smart Search", "Header", "", "", 0, "B01F3EF1-ECB9-4C7E-AB92-B45C4C29D5C0" );    // PersonDetail

            Sql( @"
    UPDATE [PersonBadge] SET [Order] = 0 WHERE [Guid] = '66972BFF-42CD-49AB-9A7A-E1B9DECA4EBF' -- Connection Status
    UPDATE [PersonBadge] SET [Order] = 1 WHERE [Guid] = 'B21DCD49-AC35-4B2B-9857-75213209B643' -- Campus
    UPDATE [PersonBadge] SET [Order] = 2 WHERE [Guid] = '66972BFF-42CD-49AB-9A7A-E1B9DECA4ECA' -- Inactive Record Status
    UPDATE [PersonBadge] SET [Order] = 3 WHERE [Guid] = '8A9AD88E-359F-46FD-9BA1-8B0603644F17' -- Last Visit on External Site
    UPDATE [PersonBadge] SET [Order] = 4 WHERE [Guid] = '260EAD7D-5073-4F88-A6A9-427F6E95985E' -- Attending Duration
    UPDATE [PersonBadge] SET [Order] = 5 WHERE [Guid] = '3F7D648D-D6BA-4F03-931C-AFBDFA24BBD8' -- Family Attendance
    UPDATE [PersonBadge] SET [Order] = 6 WHERE [Guid] = '66972BFF-42CD-49AB-9A7A-E1B9DECA4EBA' -- Family 16 Week Attendance
    UPDATE [PersonBadge] SET [Order] = 7 WHERE [Guid] = '66972BFF-42CD-49AB-9A7A-E1B9DECA4EBE' -- Baptism
    UPDATE [PersonBadge] SET [Order] = 8 WHERE [Guid] = 'E0455598-82B0-4F49-B806-C3A41C71E9DA' -- In Serving Team

    UPDATE [AttributeValue] SET
    VALUE = '66972BFF-42CD-49AB-9A7A-E1B9DECA4EBA,260EAD7D-5073-4F88-A6A9-427F6E95985E,8A9AD88E-359F-46FD-9BA1-8B0603644F17'
    WHERE [Guid] = '4F41CA56-BF42-4601-8123-EC71737C4E36'
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
    UPDATE [AttributeValue] SET
    VALUE = '66972BFF-42CD-49AB-9A7A-E1B9DECA4EBA,260EAD7D-5073-4F88-A6A9-427F6E95985E'
    WHERE [Guid] = '4F41CA56-BF42-4601-8123-EC71737C4E36'
" );

            DeleteBlockType( "9D406BD5-88C1-45E5-AFEA-70F9CFB66C74" );

            DropColumn( "dbo.PhoneNumber", "NumberFormatted" );
        }
    }
}
