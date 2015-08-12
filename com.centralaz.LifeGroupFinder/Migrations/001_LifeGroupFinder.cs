using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;
using Rock.Web.Cache;

namespace com.centralaz.LifeGroupFinder.Migrations
{
    [MigrationNumber( 1, "1.0.14" )]
    public class LifeGroupFinder : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            //Add Children's age groups defined type
            RockMigrationHelper.AddDefinedType( "Global", "Child Age Groups", "", "512F355E-9441-4C47-BE47-7FFE19209496", @"" );
            RockMigrationHelper.AddDefinedValue( "512F355E-9441-4C47-BE47-7FFE19209496", "Children (PreK-5th)", "", "B1A13599-85E1-428B-AA99-7F041D05E62A", false );
            RockMigrationHelper.AddDefinedValue( "512F355E-9441-4C47-BE47-7FFE19209496", "Early Childhood (0-3yrs)", "", "7DF06A41-4AD0-47EB-9352-D9AFA1AB3CFC", false );
            RockMigrationHelper.AddDefinedValue( "512F355E-9441-4C47-BE47-7FFE19209496", "High School", "", "EBB29BCA-531D-439E-AB45-C6D6A54FF50A", false );
            RockMigrationHelper.AddDefinedValue( "512F355E-9441-4C47-BE47-7FFE19209496", "Junior High", "", "C08626C9-6516-424D-B09A-C43010BC65CC", false );

            //Life group attributes
            RockMigrationHelper.AddGroupTypeGroupAttribute( "50FCFB30-F51A-49DF-86F4-2B176EA1820B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Has Pets", "Whether the group has pets.", 1, "False", "504D75BC-5735-437D-BC30-5981B614B92B" );
            RockMigrationHelper.AddGroupTypeGroupAttribute( "50FCFB30-F51A-49DF-86F4-2B176EA1820B", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Has Children", "Whether the group has children.", 2, "", "D6E4B310-FFDC-4166-8803-04C084277F68" );
            RockMigrationHelper.AddGroupTypeGroupAttribute( "50FCFB30-F51A-49DF-86F4-2B176EA1820B", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Crossroads", "The closest crossroads of the group", 3, "", "F246F843-1F7B-4FFB-9252-635668F0002B" );
            RockMigrationHelper.AddGroupTypeGroupAttribute( "50FCFB30-F51A-49DF-86F4-2B176EA1820B", "9C204CD0-1233-41C5-818A-C5DA439445AA", "List Description", "", 4, "", "8F0A6B55-8DA5-42EC-9369-E1BF11C903E8" );
            RockMigrationHelper.AddGroupTypeGroupAttribute( "50FCFB30-F51A-49DF-86F4-2B176EA1820B", "97F8157D-A8C8-4AB3-96A2-9CB2A9049E6D", "Main Photo", "", 5, "", "36F6FFFE-6BD9-4DC6-81F3-F125492EECD5" );
            RockMigrationHelper.AddGroupTypeGroupAttribute( "50FCFB30-F51A-49DF-86F4-2B176EA1820B", "F1F5B59D-F086-4627-A94A-DFA7E67950F3", "Main Video", "", 6, null, "6775AD18-BA1A-4696-9D62-F950751537B2" );
            RockMigrationHelper.AddGroupTypeGroupAttribute( "50FCFB30-F51A-49DF-86F4-2B176EA1820B", "97F8157D-A8C8-4AB3-96A2-9CB2A9049E6D", "Group Photo 1", "", 7, "", "D578D2B5-A549-4E7E-98C9-C44A4E5A654D" );
            RockMigrationHelper.AddGroupTypeGroupAttribute( "50FCFB30-F51A-49DF-86F4-2B176EA1820B", "97F8157D-A8C8-4AB3-96A2-9CB2A9049E6D", "Group Photo 2", "", 8, "", "667C780E-1543-499A-82FC-7B415820977D" );
            RockMigrationHelper.AddGroupTypeGroupAttribute( "50FCFB30-F51A-49DF-86F4-2B176EA1820B", "97F8157D-A8C8-4AB3-96A2-9CB2A9049E6D", "Group Photo 3", "", 9, "", "9DA055F3-9B8A-4399-BB93-9783502273DF" );
            RockMigrationHelper.AddAttributeQualifier( "D6E4B310-FFDC-4166-8803-04C084277F68", "allowmultiple", "True", "7092B70D-08BC-4B3B-9E64-227D53DAD149" );
            RockMigrationHelper.AddAttributeQualifier( "D6E4B310-FFDC-4166-8803-04C084277F68", "definedtype", "55", "9AF45EBE-3154-402D-B5C5-03BA338170AC" );
            RockMigrationHelper.AddAttributeQualifier( "D6E4B310-FFDC-4166-8803-04C084277F68", "displaydescription", "False", "B406ED42-7770-422D-9AF1-CEEF22AF3E48" );
            RockMigrationHelper.AddAttributeQualifier( "36F6FFFE-6BD9-4DC6-81F3-F125492EECD5", "binaryFileType", "", "3464C219-A171-43A5-8924-518A94407F05" );
            RockMigrationHelper.AddAttributeQualifier( "6775AD18-BA1A-4696-9D62-F950751537B2", "binaryFileType", "", "EF9CE0CD-6F5B-4AC3-85EB-2C516425B886" );
            RockMigrationHelper.AddAttributeQualifier( "D578D2B5-A549-4E7E-98C9-C44A4E5A654D", "binaryFileType", "", "19FDE740-AA2E-4567-976B-52C5AD979DB0" );
            RockMigrationHelper.AddAttributeQualifier( "667C780E-1543-499A-82FC-7B415820977D", "binaryFileType", "", "643ED060-4BAF-4FD8-B974-0FFDCC22EC7E" );
            RockMigrationHelper.AddAttributeQualifier( "9DA055F3-9B8A-4399-BB93-9783502273DF", "binaryFileType", "", "2163AF41-D1D8-4E43-BC9E-D15D215844D4" );

            GroupTypeCache theGroupType = GroupTypeCache.Read( "50FCFB30-F51A-49DF-86F4-2B176EA1820B" );
            int id = theGroupType.Id;
            String theId = id.ToString();
            RockMigrationHelper.AddEntityAttribute( "Rock.Model.GroupMember", Rock.SystemGuid.FieldType.BOOLEAN, "GroupTypeId", theId, "Information Request", "", "Is this someone just wanting more information?", 0, "False", "94DBC590-26B4-4A7F-B555-520D57D10C12" );
            RockMigrationHelper.AddAttributeQualifier( "94DBC590-26B4-4A7F-B555-520D57D10C12", "falsetext", "No", "73CC9D86-0988-435B-A9DB-EA559050E196" );
            RockMigrationHelper.AddAttributeQualifier( "94DBC590-26B4-4A7F-B555-520D57D10C12", "truetext", "Yes", "69D4E5B4-5370-4D03-A18E-E254B0BDDB14" );

            RockMigrationHelper.UpdateSystemEmail( "Groups", "New member - To member", "", "", "", "", "", "New member - To member", "New member - To member", "B83EF8B4-67DF-4824-B38F-B7CF5527A381" );
            RockMigrationHelper.UpdateSystemEmail( "Groups", "New member - To leader", "", "", "", "", "", "New member - To leader", "New member - To leader", "CAAFA7D3-B8F2-4FB5-9C57-EBCB91DE5A2E" );
            RockMigrationHelper.UpdateSystemEmail( "Groups", "Information Request - To Leader", "", "", "", "", "", "Information Request - To Leader", "Information Request - To Leader", "8091E0D6-1F54-4015-A298-F87D36982DAF" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "94DBC590-26B4-4A7F-B555-520D57D10C12" );
            RockMigrationHelper.DeleteAttribute( "504D75BC-5735-437D-BC30-5981B614B92B" );    // GroupType - Small Group: Has Pets
            RockMigrationHelper.DeleteAttribute( "D6E4B310-FFDC-4166-8803-04C084277F68" );    // GroupType - Small Group: Has Children
            RockMigrationHelper.DeleteAttribute( "F246F843-1F7B-4FFB-9252-635668F0002B" );    // GroupType - Small Group: Crossroads
            RockMigrationHelper.DeleteAttribute( "8F0A6B55-8DA5-42EC-9369-E1BF11C903E8" );    // GroupType - Small Group: List Description

            RockMigrationHelper.DeleteDefinedValue( "7DF06A41-4AD0-47EB-9352-D9AFA1AB3CFC" );
            RockMigrationHelper.DeleteDefinedValue( "B1A13599-85E1-428B-AA99-7F041D05E62A" );
            RockMigrationHelper.DeleteDefinedValue( "C08626C9-6516-424D-B09A-C43010BC65CC" );
            RockMigrationHelper.DeleteDefinedValue( "EBB29BCA-531D-439E-AB45-C6D6A54FF50A" );
            RockMigrationHelper.DeleteDefinedType( "512F355E-9441-4C47-BE47-7FFE19209496" );
        }
    }
}
