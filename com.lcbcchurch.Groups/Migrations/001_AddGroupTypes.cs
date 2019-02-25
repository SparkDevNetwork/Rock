// <copyright>
// Copyright by LCBC Church
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;
namespace com.lcbcchurch.Groups.Migrations
{
    [MigrationNumber( 1, "1.0.14" )]
    public class AddGroupTypes : Migration
    {
        public override void Up()
        {
            // Delete Hours Serving Attribute
            RockMigrationHelper.DeleteAttribute( "9DD24AE7-966C-47C3-B6D5-E52B24A0AADF" );

            // Add Group Types
            RockMigrationHelper.AddGroupType( "Campus Group", "", "Group", "Member", false, true, true, "", 0, "", 0, "", "EB306DC0-C9FD-4672-84C3-7BB02ECF484A" );
            RockMigrationHelper.AddGroupType( "Ministry Group", "", "Group", "Member", false, true, true, "", 0, "", 0, "", "F12057FA-F0CE-41A7-A199-7C7754E01E10" );
            RockMigrationHelper.AddGroupType( "Student Ministry Small Group", "", "Group", "Member", false, true, true, "", 0, "50FCFB30-F51A-49DF-86F4-2B176EA1820B", 0, "", "3EC71DB5-3A20-4B06-97FA-5B764AD3DBA3" );
            RockMigrationHelper.AddGroupType( "Collide Small Groups", "", "Group", "Member", false, true, true, "", 0, "3EC71DB5-3A20-4B06-97FA-5B764AD3DBA3", 0, "", "77E49198-2670-4744-939F-534AA8275915" );
            RockMigrationHelper.AddGroupType( "HSM Small Groups", "", "Group", "Member", false, true, true, "", 0, "3EC71DB5-3A20-4B06-97FA-5B764AD3DBA3", 0, "", "0C425414-F311-4818-9D1E-92A0D2DFF3B0" );
            RockMigrationHelper.AddGroupType( "kidCrew Adult Volunteers", "", "Group", "Member", false, true, true, "", 0, "2C42B2D4-1C5F-4AD5-A9AD-08631B872AC4", 0, "", "093DAFC7-2F18-46D3-99F9-C3BEC4A8C07F" );
            RockMigrationHelper.AddGroupType( "kidCrew Student Volunteers", "", "Group", "Member", false, true, true, "", 0, "2C42B2D4-1C5F-4AD5-A9AD-08631B872AC4", 0, "", "ABD85A23-17F5-4EE0-A0C6-A88B6F8DFD77" );
            RockMigrationHelper.AddGroupType( "kidCrew Guest Services Adult Volunteers", "", "Group", "Member", false, true, true, "", 0, "2C42B2D4-1C5F-4AD5-A9AD-08631B872AC4", 0, "", "3ECF1EE2-693E-4820-8706-86C02287D4D9" );
            RockMigrationHelper.AddGroupType( "kidCrew Guest Services Student Volunteers", "", "Group", "Member", false, true, true, "", 0, "2C42B2D4-1C5F-4AD5-A9AD-08631B872AC4", 0, "", "A4EE4A54-B971-463E-99D2-1A40FFC70FA6" );
            RockMigrationHelper.AddGroupType( "Collide Adult Volunteers", "", "Group", "Member", false, true, true, "", 0, "2C42B2D4-1C5F-4AD5-A9AD-08631B872AC4", 0, "", "C62147B0-1B28-45F9-940C-2D95938922E0" );
            RockMigrationHelper.AddGroupType( "Collide Student Volunteers", "", "Group", "Member", false, true, true, "", 0, "2C42B2D4-1C5F-4AD5-A9AD-08631B872AC4", 0, "", "B43DB1BD-DA52-4201-89C1-9444F02C9F5D" );
            RockMigrationHelper.AddGroupType( "HSM Adult Volunteers", "", "Group", "Member", false, true, true, "", 0, "2C42B2D4-1C5F-4AD5-A9AD-08631B872AC4", 0, "", "C0A6E32C-D35C-4744-97E3-3409E6D9D034" );
            RockMigrationHelper.AddGroupType( "HSM Student Volunteers", "", "Group", "Member", false, true, true, "", 0, "2C42B2D4-1C5F-4AD5-A9AD-08631B872AC4", 0, "", "5EB34C64-DD0C-4561-BE78-59BADD8684AA" );

            // Add Group Type Associations
            AddGroupTypeAssociation( "EB306DC0-C9FD-4672-84C3-7BB02ECF484A", "EB306DC0-C9FD-4672-84C3-7BB02ECF484A" );
            AddGroupTypeAssociation( "EB306DC0-C9FD-4672-84C3-7BB02ECF484A", "F12057FA-F0CE-41A7-A199-7C7754E01E10" );
            AddGroupTypeAssociation( "F12057FA-F0CE-41A7-A199-7C7754E01E10", "2C42B2D4-1C5F-4AD5-A9AD-08631B872AC4" );
            AddGroupTypeAssociation( "F12057FA-F0CE-41A7-A199-7C7754E01E10", "F12057FA-F0CE-41A7-A199-7C7754E01E10" );
            AddGroupTypeAssociation( "F12057FA-F0CE-41A7-A199-7C7754E01E10", "3EC71DB5-3A20-4B06-97FA-5B764AD3DBA3" );
            AddGroupTypeAssociation( "F12057FA-F0CE-41A7-A199-7C7754E01E10", "093DAFC7-2F18-46D3-99F9-C3BEC4A8C07F" );
            AddGroupTypeAssociation( "F12057FA-F0CE-41A7-A199-7C7754E01E10", "ABD85A23-17F5-4EE0-A0C6-A88B6F8DFD77" );
            AddGroupTypeAssociation( "F12057FA-F0CE-41A7-A199-7C7754E01E10", "3ECF1EE2-693E-4820-8706-86C02287D4D9" );
            AddGroupTypeAssociation( "F12057FA-F0CE-41A7-A199-7C7754E01E10", "A4EE4A54-B971-463E-99D2-1A40FFC70FA6" );
            AddGroupTypeAssociation( "F12057FA-F0CE-41A7-A199-7C7754E01E10", "C62147B0-1B28-45F9-940C-2D95938922E0" );
            AddGroupTypeAssociation( "F12057FA-F0CE-41A7-A199-7C7754E01E10", "B43DB1BD-DA52-4201-89C1-9444F02C9F5D" );
            AddGroupTypeAssociation( "F12057FA-F0CE-41A7-A199-7C7754E01E10", "C0A6E32C-D35C-4744-97E3-3409E6D9D034" );
            AddGroupTypeAssociation( "F12057FA-F0CE-41A7-A199-7C7754E01E10", "5EB34C64-DD0C-4561-BE78-59BADD8684AA" );
            AddGroupTypeAssociation( "3EC71DB5-3A20-4B06-97FA-5B764AD3DBA3", "77E49198-2670-4744-939F-534AA8275915" );
            AddGroupTypeAssociation( "3EC71DB5-3A20-4B06-97FA-5B764AD3DBA3", "0C425414-F311-4818-9D1E-92A0D2DFF3B0" );

            // Add Group Type Roles
            RockMigrationHelper.UpdateGroupTypeRole( "EB306DC0-C9FD-4672-84C3-7BB02ECF484A", "Member", "", 0, null, null, "95ACEDFF-6392-4127-9F86-0ABC0A42D5DE", false, false, true );
            RockMigrationHelper.UpdateGroupTypeRole( "F12057FA-F0CE-41A7-A199-7C7754E01E10", "Member", "", 0, null, null, "9D086E39-B675-40A8-B223-28F186C7B64C", false, false, true );
            RockMigrationHelper.UpdateGroupTypeRole( "3EC71DB5-3A20-4B06-97FA-5B764AD3DBA3", "Member", "", 0, null, null, "19A0F339-B99B-4706-B2B8-58B5E8B288CD", false, false, true );
            RockMigrationHelper.UpdateGroupTypeRole( "77E49198-2670-4744-939F-534AA8275915", "Member", "", 0, null, null, "83109BF4-CF19-4710-BFEB-BC54E9C1C172", false, false, true );
            RockMigrationHelper.UpdateGroupTypeRole( "0C425414-F311-4818-9D1E-92A0D2DFF3B0", "Member", "", 0, null, null, "2784326F-B3E8-4782-BD45-79A9742F3788", false, false, true );
            RockMigrationHelper.UpdateGroupTypeRole( "093DAFC7-2F18-46D3-99F9-C3BEC4A8C07F", "Member", "", 0, null, null, "A1C9DFDF-968A-4BE9-BD89-4A3DC95F5D4C", false, false, true );
            RockMigrationHelper.UpdateGroupTypeRole( "ABD85A23-17F5-4EE0-A0C6-A88B6F8DFD77", "Member", "", 0, null, null, "50DA779F-23FE-46C2-9A91-0BA309E84987", false, false, true );
            RockMigrationHelper.UpdateGroupTypeRole( "3ECF1EE2-693E-4820-8706-86C02287D4D9", "Member", "", 0, null, null, "3F9EF71C-C777-4EC5-9F34-F6DF3921D229", false, false, true );
            RockMigrationHelper.UpdateGroupTypeRole( "A4EE4A54-B971-463E-99D2-1A40FFC70FA6", "Member", "", 0, null, null, "F45905C5-6F65-4E63-93E2-4083CCDE6999", false, false, true );
            RockMigrationHelper.UpdateGroupTypeRole( "C62147B0-1B28-45F9-940C-2D95938922E0", "Member", "", 0, null, null, "4ADED0CF-31DD-4715-AC49-B4C4CE478E0F", false, false, true );
            RockMigrationHelper.UpdateGroupTypeRole( "B43DB1BD-DA52-4201-89C1-9444F02C9F5D", "Member", "", 0, null, null, "1193E46B-0AE8-44B1-8EF7-0978C1B3E986", false, false, true );
            RockMigrationHelper.UpdateGroupTypeRole( "C0A6E32C-D35C-4744-97E3-3409E6D9D034", "Member", "", 0, null, null, "4581AB2F-83FB-41BE-8D8A-6B315FF762BA", false, false, true );
            RockMigrationHelper.UpdateGroupTypeRole( "5EB34C64-DD0C-4561-BE78-59BADD8684AA", "Member", "", 0, null, null, "ACC50A56-A196-420B-805C-D26AA9F066DB", false, false, true );

            // Add Group Type Attributes
            //KidCrew Adult Volunteers
            RockMigrationHelper.AddGroupTypeGroupMemberAttribute( "093DAFC7-2F18-46D3-99F9-C3BEC4A8C07F", "EC6A5CAF-F6A2-47A4-9CBA-6E1C53D7E59B", "Schedules", @"", 0, "", "7C2B37E1-A943-4E4A-ACB5-53E041A3C9C9" );
            RockMigrationHelper.AddGroupTypeGroupMemberAttribute( "093DAFC7-2F18-46D3-99F9-C3BEC4A8C07F", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Job", @"", 1, "", "CBFF02EB-A7B0-4211-8807-64B13AC34A4F" );
            RockMigrationHelper.AddGroupTypeGroupMemberAttribute( "093DAFC7-2F18-46D3-99F9-C3BEC4A8C07F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Environment", @"", 2, "", "A68B61CF-1D10-4331-B4F2-69E80421DCAC" );
            RockMigrationHelper.AddAttributeQualifier( "CBFF02EB-A7B0-4211-8807-64B13AC34A4F", "values", "Captain,Leader,Production", "36123D22-A785-479D-82C2-BF4A3BBDDCEB" );
            RockMigrationHelper.AddAttributeQualifier( "CBFF02EB-A7B0-4211-8807-64B13AC34A4F", "enhancedselection", "False", "4171D3D0-0143-4A77-A7C2-59A3B2503B2D" );

            //KidCrew Student Volunteers
            RockMigrationHelper.AddGroupTypeGroupMemberAttribute( "ABD85A23-17F5-4EE0-A0C6-A88B6F8DFD77", "EC6A5CAF-F6A2-47A4-9CBA-6E1C53D7E59B", "Schedules", @"", 0, "", "A04A385A-DEFA-447C-A0A2-9185F5A71C4F" );
            RockMigrationHelper.AddGroupTypeGroupMemberAttribute( "ABD85A23-17F5-4EE0-A0C6-A88B6F8DFD77", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Job", @"", 1, "", "A75BC2B4-EA22-4FFF-A657-1BB4CB95214B" );
            RockMigrationHelper.AddGroupTypeGroupMemberAttribute( "ABD85A23-17F5-4EE0-A0C6-A88B6F8DFD77", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Environment", @"", 2, "", "9D8AE363-D5DA-4759-9686-837D8069A942" );
            RockMigrationHelper.AddAttributeQualifier( "A75BC2B4-EA22-4FFF-A657-1BB4CB95214B", "values", "Leader,Production", "5D79F787-5F73-4D72-A966-02BC420B0E32" );
            RockMigrationHelper.AddAttributeQualifier( "A75BC2B4-EA22-4FFF-A657-1BB4CB95214B", "enhancedselection", "False", "FA706CF7-7B2E-459A-A024-7DE60221944E" );

            //KidCrew Adult Guest Services Volunteers
            RockMigrationHelper.AddGroupTypeGroupMemberAttribute( "3ECF1EE2-693E-4820-8706-86C02287D4D9", "EC6A5CAF-F6A2-47A4-9CBA-6E1C53D7E59B", "Schedules", @"", 0, "", "1A8C9D2E-0576-4055-BDB2-D2298C9E56AB" );
            RockMigrationHelper.AddGroupTypeGroupMemberAttribute( "3ECF1EE2-693E-4820-8706-86C02287D4D9", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Job", @"", 1, "", "8C332971-1131-44FA-89A8-CB3574402006" );
            RockMigrationHelper.AddAttributeQualifier( "8C332971-1131-44FA-89A8-CB3574402006", "values", "Buddy,Buddy Captain,Campus Captain,Guest Services", "612D43AD-9CDB-434E-9E70-12611AD212EB" );
            RockMigrationHelper.AddAttributeQualifier( "8C332971-1131-44FA-89A8-CB3574402006", "enhancedselection", "False", "CBD804B0-E77D-4D19-A5F1-C869B0ED9AE2" );

            //KidCrew Student Guest Services Volunteers
            RockMigrationHelper.AddGroupTypeGroupMemberAttribute( "A4EE4A54-B971-463E-99D2-1A40FFC70FA6", "EC6A5CAF-F6A2-47A4-9CBA-6E1C53D7E59B", "Schedules", @"", 0, "", "23AAE077-06DF-4F7E-93A3-E606B26E15C2" );
            RockMigrationHelper.AddGroupTypeGroupMemberAttribute( "A4EE4A54-B971-463E-99D2-1A40FFC70FA6", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Job", @"", 1, "", "6FD4FAC2-467E-4E1A-B4B0-8ABBB1C21E2A" );
            RockMigrationHelper.AddAttributeQualifier( "6FD4FAC2-467E-4E1A-B4B0-8ABBB1C21E2A", "values", "Buddy,Guest Services", "516CF9A5-C64D-47B1-A8C9-66DC2B2FE958" );
            RockMigrationHelper.AddAttributeQualifier( "6FD4FAC2-467E-4E1A-B4B0-8ABBB1C21E2A", "enhancedselection", "False", "FCAB1E65-6E92-44F0-A214-0C7BF8556C42" );

            //Collide Adult Volunteers
            RockMigrationHelper.AddGroupTypeGroupMemberAttribute( "C62147B0-1B28-45F9-940C-2D95938922E0", "EC6A5CAF-F6A2-47A4-9CBA-6E1C53D7E59B", "Schedules", @"", 0, "", "C95C3414-7780-48BC-85DA-645F0ECCD691" );
            RockMigrationHelper.AddGroupTypeGroupMemberAttribute( "C62147B0-1B28-45F9-940C-2D95938922E0", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Job", @"", 1, "", "B81242F5-22CF-4FE3-9C04-5C2BD50C19CE" );
            RockMigrationHelper.AddGroupTypeGroupMemberAttribute( "C62147B0-1B28-45F9-940C-2D95938922E0", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Grade", @"", 2, "", "9E1441FA-3ED1-4E5A-9130-0494FDCABC5E" );
            RockMigrationHelper.AddAttributeQualifier( "B81242F5-22CF-4FE3-9C04-5C2BD50C19CE", "values", "Band,Buddy,Guest Services,LIFE Group Leader,Production", "FC121128-9BC0-4055-A06C-173D48502DBA" );
            RockMigrationHelper.AddAttributeQualifier( "9E1441FA-3ED1-4E5A-9130-0494FDCABC5E", "values", "5th,6th,7th,8th", "C7915F44-15C6-439F-8211-451D1AAD9535" );
            RockMigrationHelper.AddAttributeQualifier( "B81242F5-22CF-4FE3-9C04-5C2BD50C19CE", "enhancedselection", "False", "9429EA22-6625-4780-8AD8-9443A44EC856" );
            RockMigrationHelper.AddAttributeQualifier( "9E1441FA-3ED1-4E5A-9130-0494FDCABC5E", "enhancedselection", "False", "8702A8DB-2AD0-48C1-BE6F-1ABD1E18DDB9" );

            //Collide Student Volunteers
            RockMigrationHelper.AddGroupTypeGroupMemberAttribute( "B43DB1BD-DA52-4201-89C1-9444F02C9F5D", "EC6A5CAF-F6A2-47A4-9CBA-6E1C53D7E59B", "Schedules", @"", 0, "", "D6A7DCD4-643E-47C6-A79D-7A9384FC3A08" );
            RockMigrationHelper.AddGroupTypeGroupMemberAttribute( "B43DB1BD-DA52-4201-89C1-9444F02C9F5D", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Job", @"", 1, "", "E6E18CF7-2E85-45FE-8D08-665178A0E998" );
            RockMigrationHelper.AddGroupTypeGroupMemberAttribute( "B43DB1BD-DA52-4201-89C1-9444F02C9F5D", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Grade", @"", 2, "", "AC83852A-50E5-428B-ABA6-471EB5B405AC" );
            RockMigrationHelper.AddAttributeQualifier( "E6E18CF7-2E85-45FE-8D08-665178A0E998", "values", "Band,Buddy,Guest Services,LIFE Group Leader,Production", "CDA60F30-C9A7-4DA1-8653-96C3834BBC27" );
            RockMigrationHelper.AddAttributeQualifier( "AC83852A-50E5-428B-ABA6-471EB5B405AC", "values", "5th,6th,7th,8th", "C247FF7C-BD4D-4E3F-868F-0DBEA3547D08" );
            RockMigrationHelper.AddAttributeQualifier( "E6E18CF7-2E85-45FE-8D08-665178A0E998", "enhancedselection", "False", "5C84FC49-AA97-46B4-AE1A-9CF656428CF7" );
            RockMigrationHelper.AddAttributeQualifier( "AC83852A-50E5-428B-ABA6-471EB5B405AC", "enhancedselection", "False", "5FAA643A-F361-4702-B0FC-0D4C81057930" );

            //HSM Adult Volunteers
            RockMigrationHelper.AddGroupTypeGroupMemberAttribute( "C0A6E32C-D35C-4744-97E3-3409E6D9D034", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Job", @"", 1, "", "F963DA61-80AB-4296-811B-B86D85371C2A" );
            RockMigrationHelper.AddGroupTypeGroupMemberAttribute( "C0A6E32C-D35C-4744-97E3-3409E6D9D034", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Grade", @"", 2, "", "59C74C65-C816-4622-8C9D-EDA79BF0A158" );
            RockMigrationHelper.AddAttributeQualifier( "F963DA61-80AB-4296-811B-B86D85371C2A", "values", "Band,Buddy,Guest Services,LIFE Group Leader,Production", "201BD3F2-BE1F-4D4B-8558-7991DC3CC0FB" );
            RockMigrationHelper.AddAttributeQualifier( "59C74C65-C816-4622-8C9D-EDA79BF0A158", "values", "9th,10th,11th,12th", "58F4630B-733B-493D-A49B-F0F23BD65120" );
            RockMigrationHelper.AddAttributeQualifier( "F963DA61-80AB-4296-811B-B86D85371C2A", "enhancedselection", "False", "1D71B392-D876-425E-AF9F-AD6E18F133DF" );
            RockMigrationHelper.AddAttributeQualifier( "59C74C65-C816-4622-8C9D-EDA79BF0A158", "enhancedselection", "False", "9A0C49BA-0AE3-4643-B401-74897730651D" );

            //HSM Student Volunteers
            RockMigrationHelper.AddGroupTypeGroupMemberAttribute( "5EB34C64-DD0C-4561-BE78-59BADD8684AA", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Job", @"", 1, "", "85B0BDC6-2286-4C6B-A310-17BF643F2216" );
            RockMigrationHelper.AddGroupTypeGroupMemberAttribute( "5EB34C64-DD0C-4561-BE78-59BADD8684AA", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Grade", @"", 2, "", "C159B8D1-1200-4DA7-97DD-A175F00659FB" );
            RockMigrationHelper.AddAttributeQualifier( "85B0BDC6-2286-4C6B-A310-17BF643F2216", "values", "Band,Guest Services,Production", "B7FAA110-B23A-455A-BC22-3AC2580F8519" );
            RockMigrationHelper.AddAttributeQualifier( "C159B8D1-1200-4DA7-97DD-A175F00659FB", "values", "9th,10th,11th,12th", "544E32E9-78F2-46A1-B1C3-670BFD837D68" );
            RockMigrationHelper.AddAttributeQualifier( "85B0BDC6-2286-4C6B-A310-17BF643F2216", "enhancedselection", "False", "3C2F3F36-38B0-4F7E-919F-CE40B2878EFA" );
            RockMigrationHelper.AddAttributeQualifier( "C159B8D1-1200-4DA7-97DD-A175F00659FB", "enhancedselection", "False", "734B1687-A245-477F-8BC4-3C4E567FFA07" );

            //Collide Small Groups
            RockMigrationHelper.AddGroupTypeGroupAttribute( "77E49198-2670-4744-939F-534AA8275915", "EC6A5CAF-F6A2-47A4-9CBA-6E1C53D7E59B", "Schedule", @"", 0, "", "718EB363-AF54-4841-B4BA-69993250A015" );
            RockMigrationHelper.AddGroupTypeGroupAttribute( "77E49198-2670-4744-939F-534AA8275915", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Name", @"", 1, "", "960F2823-F29C-47B4-BC46-B824823ADB63" );

            //HSM Small Groups
            RockMigrationHelper.AddGroupTypeGroupAttribute( "0C425414-F311-4818-9D1E-92A0D2DFF3B0", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Name", @"", 1, "", "57847E65-D3F9-4116-B991-61D91F27274A" );

            // Set the attributes as IsGridColumn = true
            Sql( @" Update Attribute
                    Set IsGridColumn = 1
                    Where Guid in ('718EB363-AF54-4841-B4BA-69993250A015',
'960F2823-F29C-47B4-BC46-B824823ADB63',
'57847E65-D3F9-4116-B991-61D91F27274A',
'7C2B37E1-A943-4E4A-ACB5-53E041A3C9C9',
'CBFF02EB-A7B0-4211-8807-64B13AC34A4F',
'A68B61CF-1D10-4331-B4F2-69E80421DCAC',
'A04A385A-DEFA-447C-A0A2-9185F5A71C4F',
'A75BC2B4-EA22-4FFF-A657-1BB4CB95214B',
'9D8AE363-D5DA-4759-9686-837D8069A942',
'1A8C9D2E-0576-4055-BDB2-D2298C9E56AB',
'8C332971-1131-44FA-89A8-CB3574402006',
'23AAE077-06DF-4F7E-93A3-E606B26E15C2',
'6FD4FAC2-467E-4E1A-B4B0-8ABBB1C21E2A',
'C95C3414-7780-48BC-85DA-645F0ECCD691',
'B81242F5-22CF-4FE3-9C04-5C2BD50C19CE',
'D6A7DCD4-643E-47C6-A79D-7A9384FC3A08',
'E6E18CF7-2E85-45FE-8D08-665178A0E998',
'F963DA61-80AB-4296-811B-B86D85371C2A',
'85B0BDC6-2286-4C6B-A310-17BF643F2216',
'9E1441FA-3ED1-4E5A-9130-0494FDCABC5E',
'AC83852A-50E5-428B-ABA6-471EB5B405AC',
'59C74C65-C816-4622-8C9D-EDA79BF0A158',
'C159B8D1-1200-4DA7-97DD-A175F00659FB')
" );


        }
        public override void Down()
        {
        }
        public void AddGroupTypeAssociation( string parentGroupTypeGuid, string childGroupTypeGuid )
        {
            Sql( string.Format( @"

                -- Insert a group type association...

                DECLARE @ParentGroupTypeId int = ( SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = '{0}' )
                DECLARE @ChildGroupTypeId int = ( SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = '{1}' )

                IF NOT EXISTS (
                    SELECT [GroupTypeId]
                    FROM [GroupTypeAssociation]
                    WHERE [GroupTypeId] = @ParentGroupTypeId
                    AND [ChildGroupTypeId] = @ChildGroupTypeId)
                BEGIN
                    INSERT INTO [GroupTypeAssociation] (
                        [GroupTypeId]
                        ,[ChildGroupTypeId])
                    VALUES(
                        @ParentGroupTypeId
                        ,@ChildGroupTypeId)
                END
",
                   parentGroupTypeGuid,
                   childGroupTypeGuid
           ) );
        }

    }
}
