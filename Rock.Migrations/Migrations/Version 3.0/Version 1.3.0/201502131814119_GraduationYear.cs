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
    public partial class GraduationYear : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Person", "GraduationYear", c => c.Int());
            Sql( "Update Person set GraduationYear = DATEPART(year, GraduationDate)" );
            DropColumn("dbo.Person", "GraduationDate");

            RockMigrationHelper.UpdateFieldType( "Defined Value Range", "", "Rock", "Rock.Field.Types.DefinedValueRangeFieldType", "B5C07B16-844D-4620-82E3-4CCA8F5FC350" );

            Sql( @"
-- Stock GradeRange attribute Guid='C7C028C2-6582-45E8-839D-5C4467C6FDF4' for Checkin Group Types. Change to DefinedValueRange attribute

-- change the field type to DefinedValueRange with definedtype of SchoolGrade and ShowDescription=true
update [Attribute] set [FieldTypeId] = (select top 1 Id from FieldType where [Guid] = 'B5C07B16-844D-4620-82E3-4CCA8F5FC350') where [Guid] = 'C7C028C2-6582-45E8-839D-5C4467C6FDF4'
delete from AttributeQualifier where AttributeId in (select top 1 Id from Attribute where [Guid] = 'C7C028C2-6582-45E8-839D-5C4467C6FDF4')
insert into AttributeQualifier ([IsSystem], [AttributeId], [Key], [Value], [Guid])
  select top 1 1, Id, 'definedtype', '24e5a79f-1e62-467a-ad5d-0d10a2328b4d', '8D1595BA-D15B-4006-92E8-DEB07354DE92' from Attribute where [Guid] = 'C7C028C2-6582-45E8-839D-5C4467C6FDF4'
insert into AttributeQualifier ([IsSystem], [AttributeId], [Key], [Value], [Guid])
  select top 1 1, Id, 'displaydescription', 'True', '71DB83F9-5E34-4F50-A1CF-B333D954A5FF' from Attribute where [Guid] = 'C7C028C2-6582-45E8-839D-5C4467C6FDF4'

-- prefix existing values with '_' so the replace can find the old values (vs replacing parts of the new guid values)
update AttributeValue set Value = REPLACE(Value, ',', '_,') + '_' where [AttributeId] in (select Id from [Attribute] where [Guid] = 'C7C028C2-6582-45E8-839D-5C4467C6FDF4') and len(value) < 6

-- Senior
update AttributeValue set Value = replace(Value, '12_', 'C49BD3AF-FF94-4A7C-99E1-08503A3C746E') where [AttributeId] in (select Id from [Attribute] where [Guid] = 'C7C028C2-6582-45E8-839D-5C4467C6FDF4')
-- Junior
update AttributeValue set Value = replace(Value, '11_', '78F7D773-8244-4995-8BC4-AD6F6A7B7820') where [AttributeId] in (select Id from [Attribute] where [Guid] = 'C7C028C2-6582-45E8-839D-5C4467C6FDF4')
-- Sophomore
update AttributeValue set Value = replace(Value, '10_', 'E04E3F62-EF5C-4860-8F32-1C152CA1700A') where [AttributeId] in (select Id from [Attribute] where [Guid] = 'C7C028C2-6582-45E8-839D-5C4467C6FDF4')
-- Freshman
update AttributeValue set Value = replace(Value, '9_', '2A130E04-3712-427A-8BB0-473EB8FF8924')  where [AttributeId] in (select Id from [Attribute] where [Guid] = 'C7C028C2-6582-45E8-839D-5C4467C6FDF4')
-- 8th
update AttributeValue set Value = replace(Value, '8_', 'D58D70AF-3CCC-4D4E-BFAF-2014D8579D60')  where [AttributeId] in (select Id from [Attribute] where [Guid] = 'C7C028C2-6582-45E8-839D-5C4467C6FDF4')
-- 7th
update AttributeValue set Value = replace(Value, '7_', '3FE728AC-BE25-409A-98CB-3CFCE5FA063B')  where [AttributeId] in (select Id from [Attribute] where [Guid] = 'C7C028C2-6582-45E8-839D-5C4467C6FDF4')
-- 6th
update AttributeValue set Value = replace(Value, '6_', '2D702ED8-7046-4DA5-AFFA-9633A211F594')  where [AttributeId] in (select Id from [Attribute] where [Guid] = 'C7C028C2-6582-45E8-839D-5C4467C6FDF4')
-- 5th
update AttributeValue set Value = replace(Value, '5_', '3D8CDBC8-8840-4A7E-85D0-B7C29A019EBB')  where [AttributeId] in (select Id from [Attribute] where [Guid] = 'C7C028C2-6582-45E8-839D-5C4467C6FDF4')
-- 4th
update AttributeValue set Value = replace(Value, '4_', 'F0F98B9C-E6BE-4C42-B8F4-0D8AB1A18847')  where [AttributeId] in (select Id from [Attribute] where [Guid] = 'C7C028C2-6582-45E8-839D-5C4467C6FDF4')
-- 3rd
update AttributeValue set Value = replace(Value, '3_', '23CC6288-78ED-4849-AFC9-417E0DA5A4A9')  where [AttributeId] in (select Id from [Attribute] where [Guid] = 'C7C028C2-6582-45E8-839D-5C4467C6FDF4')
-- 2nd
update AttributeValue set Value = replace(Value, '2_', 'E475D0CA-5979-4C76-8788-D91ADF595E10')  where [AttributeId] in (select Id from [Attribute] where [Guid] = 'C7C028C2-6582-45E8-839D-5C4467C6FDF4')
-- 1st
update AttributeValue set Value = replace(Value, '1_', '6B5CDFBD-9882-4EBB-A01A-7856BCD0CF61')  where [AttributeId] in (select Id from [Attribute] where [Guid] = 'C7C028C2-6582-45E8-839D-5C4467C6FDF4')
-- K
update AttributeValue set Value = replace(Value, '0_', '0FED3291-51F3-4EED-886D-1D3DF826BEAC')  where [AttributeId] in (select Id from [Attribute] where [Guid] = 'C7C028C2-6582-45E8-839D-5C4467C6FDF4')
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.Person", "GraduationDate", c => c.DateTime(storeType: "date"));
            DropColumn("dbo.Person", "GraduationYear");
        }
    }
}
