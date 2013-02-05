//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    /// 
    /// </summary>
    public partial class DefinedTypesSweep01 : RockMigration_3
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // delete old DefinedTypes block and blocktype
            Sql( @"
delete from [Block] where [Guid] = 'B8D83A2C-31F2-48C6-BEBC-753BCDC2A30C'
delete from [BlockType] where [Guid] = '0A43050F-BF0B-49F7-9D8C-2761976F160F'" );

            // fix up null FieldTypeIds in DefinedTypes table
            Sql( @"
declare
  @fieldTypeId int = (select Id from FieldType where Guid = '9C204CD0-1233-41C5-818A-C5DA439445AA')

begin
  update [DefinedType] set [FieldTypeId] = @fieldTypeId where [FieldTypeId] is null
end" );

            AddPage( "E0E1DE66-B825-4BFB-A0B3-6E069AA9AA40", "Defined Type Detail", "", "Default", "60C0C193-61CF-4B34-A0ED-67EF8FD44867" );

            AddBlockType( "Administration - Defined Type Detail", "", "~/Blocks/Administration/DefinedTypeDetail.ascx", "08C35F15-9AF7-468F-9D50-CDFD3D21220C" );
            AddBlockType( "Administration - Defined Type List", "", "~/Blocks/Administration/DefinedTypeList.ascx", "5470C9C4-09C1-439F-AA56-3524047497EE" );

            AddBlock( "E0E1DE66-B825-4BFB-A0B3-6E069AA9AA40", "5470C9C4-09C1-439F-AA56-3524047497EE", "Defined Type List", "", "Content", 1, "BA80CB0F-ED7B-49A3-8E07-CA493AC4B39C" );
            AddBlock( "60C0C193-61CF-4B34-A0ED-67EF8FD44867", "08C35F15-9AF7-468F-9D50-CDFD3D21220C", "Defined Type Detail", "", "Content", 0, "70B52ED8-4EB6-403F-B2E5-835CBF884921" );

            AddBlockTypeAttribute( "5470C9C4-09C1-439F-AA56-3524047497EE", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page Guid", "DetailPageGuid", "Advanced", "", 0, "", "E75A3154-22F5-4E35-BF86-B57EFA460BD6" );

            // Attrib Value for Defined Type List:Detail Page Guid
            AddBlockAttributeValue( "BA80CB0F-ED7B-49A3-8E07-CA493AC4B39C", "E75A3154-22F5-4E35-BF86-B57EFA460BD6", "60c0c193-61cf-4b34-a0ed-67ef8fd44867" );        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteAttribute( "E75A3154-22F5-4E35-BF86-B57EFA460BD6" );
            DeleteBlock( "BA80CB0F-ED7B-49A3-8E07-CA493AC4B39C" );
            DeleteBlock( "70B52ED8-4EB6-403F-B2E5-835CBF884921" );
            DeleteBlockType( "08C35F15-9AF7-468F-9D50-CDFD3D21220C" );
            DeleteBlockType( "5470C9C4-09C1-439F-AA56-3524047497EE" );
            DeletePage( "60C0C193-61CF-4B34-A0ED-67EF8FD44867" );        }
    }
}
