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
    public partial class DiscDefinedType : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddDefinedType( "Global", "DISC Results", "Used to store information needed to drive Rock's DISC assessment.", "F06DDAD8-6058-4182-AD0A-B523BB7A2D78", @"" );
            RockMigrationHelper.AddDefinedTypeAttribute( "F06DDAD8-6058-4182-AD0A-B523BB7A2D78", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Relationship Matrix", "RelationshipMatrix", "", 8, "", "A500116E-37CB-400F-996B-A56940FD24E9" );
            RockMigrationHelper.AddDefinedTypeAttribute( "F06DDAD8-6058-4182-AD0A-B523BB7A2D78", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title", "Description1", "", 0, "", "9AFC7328-2E1D-4F63-9BCB-438BB48B3379" );
            RockMigrationHelper.AddDefinedTypeAttribute( "F06DDAD8-6058-4182-AD0A-B523BB7A2D78", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Challenges", "Challenges", "", 2, "", "BE5497AF-8575-4619-8E53-CCED2C237465" );
            RockMigrationHelper.AddDefinedTypeAttribute( "F06DDAD8-6058-4182-AD0A-B523BB7A2D78", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Follower Style", "FollowerStyle", "", 6, "", "4005EA8E-4243-4EBA-B04D-7D54860BD61E" );
            RockMigrationHelper.AddDefinedTypeAttribute( "F06DDAD8-6058-4182-AD0A-B523BB7A2D78", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Leadership Style", "LeadershipStyle", "", 5, "", "EE6796B4-6A7B-49D6-AF40-3E4A01A0C8E8" );
            RockMigrationHelper.AddDefinedTypeAttribute( "F06DDAD8-6058-4182-AD0A-B523BB7A2D78", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Motivation", "Motivation", "", 4, "", "54167D4D-2890-4477-87BB-073353665685" );
            RockMigrationHelper.AddDefinedTypeAttribute( "F06DDAD8-6058-4182-AD0A-B523BB7A2D78", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Strengths", "Strengths", "", 1, "", "8454BAE8-154D-4F38-B151-B4FAC159A39C" );
            RockMigrationHelper.AddDefinedTypeAttribute( "F06DDAD8-6058-4182-AD0A-B523BB7A2D78", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Team Contribution", "TeamContribution", "", 7, "", "9BDA13BB-2DEB-47F5-86CA-71C2C1454FEF" );
            RockMigrationHelper.AddDefinedTypeAttribute( "F06DDAD8-6058-4182-AD0A-B523BB7A2D78", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Under Pressure", "UnderPressure", "", 3, "", "49AA57E0-AF90-491A-A80D-CC7383634A30" );
            RockMigrationHelper.AddAttributeQualifier( "4005EA8E-4243-4EBA-B04D-7D54860BD61E", "numberofrows", "", "5E692C56-FC81-4117-888D-E4DD74576C11" );
            RockMigrationHelper.AddAttributeQualifier( "49AA57E0-AF90-491A-A80D-CC7383634A30", "numberofrows", "", "9A283777-1447-4D64-8C11-C407147A9FD7" );
            RockMigrationHelper.AddAttributeQualifier( "54167D4D-2890-4477-87BB-073353665685", "numberofrows", "", "E64D9E63-301B-4924-9327-401699F95823" );
            RockMigrationHelper.AddAttributeQualifier( "8454BAE8-154D-4F38-B151-B4FAC159A39C", "numberofrows", "", "803CD975-E9F4-492B-BB19-185BDA88E6BC" );
            RockMigrationHelper.AddAttributeQualifier( "9AFC7328-2E1D-4F63-9BCB-438BB48B3379", "ispassword", "False", "265A3E82-E7B9-4387-949C-1D1EB04B16DB" );
            RockMigrationHelper.AddAttributeQualifier( "9BDA13BB-2DEB-47F5-86CA-71C2C1454FEF", "numberofrows", "", "FF72DB70-CEDA-4B69-867A-F04838F21130" );
            RockMigrationHelper.AddAttributeQualifier( "A500116E-37CB-400F-996B-A56940FD24E9", "editorHeight", "", "124A8ACA-DE8B-4786-A15F-DB2F258BD7D8" );
            RockMigrationHelper.AddAttributeQualifier( "A500116E-37CB-400F-996B-A56940FD24E9", "editorMode", "4", "7F2A3EAC-5F82-498D-876E-A0B67E2D3736" );
            RockMigrationHelper.AddAttributeQualifier( "A500116E-37CB-400F-996B-A56940FD24E9", "editorTheme", "0", "A49AF878-5C9E-48C2-9BAD-1AE555A5C030" );
            RockMigrationHelper.AddAttributeQualifier( "BE5497AF-8575-4619-8E53-CCED2C237465", "numberofrows", "", "3BFA6535-A8FF-4FF4-8737-9B50A0EC109D" );
            RockMigrationHelper.AddAttributeQualifier( "EE6796B4-6A7B-49D6-AF40-3E4A01A0C8E8", "numberofrows", "", "67ABAEC0-A58C-4939-A6C6-1F62A33A06A7" );
            RockMigrationHelper.AddDefinedValue( "F06DDAD8-6058-4182-AD0A-B523BB7A2D78", "C", "C - Description", "B428B0CA-2E41-4B3E-9A37-5B618C116CA3", false );
            RockMigrationHelper.AddDefinedValue( "F06DDAD8-6058-4182-AD0A-B523BB7A2D78", "CD", "CD - Description", "C742748E-E86F-4145-962B-3468CC563D71", false );
            RockMigrationHelper.AddDefinedValue( "F06DDAD8-6058-4182-AD0A-B523BB7A2D78", "CI", "CI - Description", "227A4C9A-F92B-48D7-9E2B-62D328369C3D", false );
            RockMigrationHelper.AddDefinedValue( "F06DDAD8-6058-4182-AD0A-B523BB7A2D78", "CS", "CS - Description", "3FDC0A3C-A826-4252-90A4-4BF710C12311", false );
            RockMigrationHelper.AddDefinedValue( "F06DDAD8-6058-4182-AD0A-B523BB7A2D78", "D", "D - Desc", "8B33090D-DD62-4BBB-BFDA-2DC67F26745D", false );
            RockMigrationHelper.AddDefinedValue( "F06DDAD8-6058-4182-AD0A-B523BB7A2D78", "DC", "DC - Description", "20706B1C-B8CC-45AB-AF41-4D52E55583BB", false );
            RockMigrationHelper.AddDefinedValue( "F06DDAD8-6058-4182-AD0A-B523BB7A2D78", "DI", "DI - Description", "43164D7B-9521-40FF-8877-FAFA1C753284", false );
            RockMigrationHelper.AddDefinedValue( "F06DDAD8-6058-4182-AD0A-B523BB7A2D78", "DS", "DS - Descritpion", "09D96C14-1F68-4454-954B-7EC8898B0A86", false );
            RockMigrationHelper.AddDefinedValue( "F06DDAD8-6058-4182-AD0A-B523BB7A2D78", "I", "I - Description", "AD076E20-ADAB-4DF3-9598-8F9FCC19B9F0", false );
            RockMigrationHelper.AddDefinedValue( "F06DDAD8-6058-4182-AD0A-B523BB7A2D78", "IC", "IC - Description", "879149E8-D396-41D9-92C2-EC9E62BE8916", false );
            RockMigrationHelper.AddDefinedValue( "F06DDAD8-6058-4182-AD0A-B523BB7A2D78", "ID", "ID - Description", "03B518C2-F9EB-4B84-A470-45EBEBAB7F17", false );
            RockMigrationHelper.AddDefinedValue( "F06DDAD8-6058-4182-AD0A-B523BB7A2D78", "IS", "IS - Description", "99D3129C-295C-471A-8E8A-804A9EBF36B9", false );
            RockMigrationHelper.AddDefinedValue( "F06DDAD8-6058-4182-AD0A-B523BB7A2D78", "S", "S - Description", "03DE220E-7924-47A5-90D4-D6383285340C", false );
            RockMigrationHelper.AddDefinedValue( "F06DDAD8-6058-4182-AD0A-B523BB7A2D78", "SC", "SC - Description", "AC246F37-0858-4692-A373-C0F125036355", false );
            RockMigrationHelper.AddDefinedValue( "F06DDAD8-6058-4182-AD0A-B523BB7A2D78", "SD", "SD - Description", "5580070A-3D01-4CB6-BC45-2645E222DC21", false );
            RockMigrationHelper.AddDefinedValue( "F06DDAD8-6058-4182-AD0A-B523BB7A2D78", "SI", "SI - Description", "05868DFC-BED9-497D-8998-6B5474280568", false );
            RockMigrationHelper.AddDefinedValueAttributeValue( "03B518C2-F9EB-4B84-A470-45EBEBAB7F17", "4005EA8E-4243-4EBA-B04D-7D54860BD61E", @"ID - Follower Style" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "03B518C2-F9EB-4B84-A470-45EBEBAB7F17", "49AA57E0-AF90-491A-A80D-CC7383634A30", @"ID - Under Pressure" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "03B518C2-F9EB-4B84-A470-45EBEBAB7F17", "54167D4D-2890-4477-87BB-073353665685", @"ID - Motivation" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "03B518C2-F9EB-4B84-A470-45EBEBAB7F17", "8454BAE8-154D-4F38-B151-B4FAC159A39C", @"ID - Strengths" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "03B518C2-F9EB-4B84-A470-45EBEBAB7F17", "9AFC7328-2E1D-4F63-9BCB-438BB48B3379", @"ID - Title" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "03B518C2-F9EB-4B84-A470-45EBEBAB7F17", "9BDA13BB-2DEB-47F5-86CA-71C2C1454FEF", @"ID - Team Contribution" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "03B518C2-F9EB-4B84-A470-45EBEBAB7F17", "A500116E-37CB-400F-996B-A56940FD24E9", @"" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "03B518C2-F9EB-4B84-A470-45EBEBAB7F17", "BE5497AF-8575-4619-8E53-CCED2C237465", @"ID - Challenges" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "03B518C2-F9EB-4B84-A470-45EBEBAB7F17", "EE6796B4-6A7B-49D6-AF40-3E4A01A0C8E8", @"ID - Leadership Style" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "03DE220E-7924-47A5-90D4-D6383285340C", "4005EA8E-4243-4EBA-B04D-7D54860BD61E", @"S - Follower Style" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "03DE220E-7924-47A5-90D4-D6383285340C", "49AA57E0-AF90-491A-A80D-CC7383634A30", @"S - Under Pressure" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "03DE220E-7924-47A5-90D4-D6383285340C", "54167D4D-2890-4477-87BB-073353665685", @"S - Motivation" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "03DE220E-7924-47A5-90D4-D6383285340C", "8454BAE8-154D-4F38-B151-B4FAC159A39C", @"S - Strengths" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "03DE220E-7924-47A5-90D4-D6383285340C", "9AFC7328-2E1D-4F63-9BCB-438BB48B3379", @"S - Title" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "03DE220E-7924-47A5-90D4-D6383285340C", "9BDA13BB-2DEB-47F5-86CA-71C2C1454FEF", @"S - Team Contribution" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "03DE220E-7924-47A5-90D4-D6383285340C", "A500116E-37CB-400F-996B-A56940FD24E9", @"{
  ""d"": {
    ""T"": ""4"",
    ""P"": ""-2""
  },
  ""i"": {
    ""T"": ""4"",
    ""P"": ""-1""
  },
  ""s"": {
    ""T"": ""2"",
    ""P"": ""4""
  },
  ""c"": {
    ""T"": ""3"",
    ""P"": ""3""
  }
}" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "03DE220E-7924-47A5-90D4-D6383285340C", "BE5497AF-8575-4619-8E53-CCED2C237465", @"S - Challenges" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "03DE220E-7924-47A5-90D4-D6383285340C", "EE6796B4-6A7B-49D6-AF40-3E4A01A0C8E8", @"S - Leadership Style" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "05868DFC-BED9-497D-8998-6B5474280568", "4005EA8E-4243-4EBA-B04D-7D54860BD61E", @"SI - Follower Style" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "05868DFC-BED9-497D-8998-6B5474280568", "49AA57E0-AF90-491A-A80D-CC7383634A30", @"SI - Under Pressure" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "05868DFC-BED9-497D-8998-6B5474280568", "54167D4D-2890-4477-87BB-073353665685", @"SI - Motivation" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "05868DFC-BED9-497D-8998-6B5474280568", "8454BAE8-154D-4F38-B151-B4FAC159A39C", @"SI - Strengths" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "05868DFC-BED9-497D-8998-6B5474280568", "9AFC7328-2E1D-4F63-9BCB-438BB48B3379", @"SI - Title" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "05868DFC-BED9-497D-8998-6B5474280568", "9BDA13BB-2DEB-47F5-86CA-71C2C1454FEF", @"SI - Team Contribution" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "05868DFC-BED9-497D-8998-6B5474280568", "A500116E-37CB-400F-996B-A56940FD24E9", @"" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "05868DFC-BED9-497D-8998-6B5474280568", "BE5497AF-8575-4619-8E53-CCED2C237465", @"SI - Challenges" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "05868DFC-BED9-497D-8998-6B5474280568", "EE6796B4-6A7B-49D6-AF40-3E4A01A0C8E8", @"SI - Leadership Style" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "09D96C14-1F68-4454-954B-7EC8898B0A86", "4005EA8E-4243-4EBA-B04D-7D54860BD61E", @"DS - Follower Style" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "09D96C14-1F68-4454-954B-7EC8898B0A86", "49AA57E0-AF90-491A-A80D-CC7383634A30", @"DS - Under Pressure" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "09D96C14-1F68-4454-954B-7EC8898B0A86", "54167D4D-2890-4477-87BB-073353665685", @"DS - Motivation" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "09D96C14-1F68-4454-954B-7EC8898B0A86", "8454BAE8-154D-4F38-B151-B4FAC159A39C", @"DS - Strengths" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "09D96C14-1F68-4454-954B-7EC8898B0A86", "9AFC7328-2E1D-4F63-9BCB-438BB48B3379", @"DS - Title" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "09D96C14-1F68-4454-954B-7EC8898B0A86", "9BDA13BB-2DEB-47F5-86CA-71C2C1454FEF", @"DS - Team Contribution" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "09D96C14-1F68-4454-954B-7EC8898B0A86", "A500116E-37CB-400F-996B-A56940FD24E9", @"" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "09D96C14-1F68-4454-954B-7EC8898B0A86", "BE5497AF-8575-4619-8E53-CCED2C237465", @"DS - Challenges" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "09D96C14-1F68-4454-954B-7EC8898B0A86", "EE6796B4-6A7B-49D6-AF40-3E4A01A0C8E8", @"DS - Leadership Style" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "20706B1C-B8CC-45AB-AF41-4D52E55583BB", "4005EA8E-4243-4EBA-B04D-7D54860BD61E", @"DC - Follower Style" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "20706B1C-B8CC-45AB-AF41-4D52E55583BB", "49AA57E0-AF90-491A-A80D-CC7383634A30", @"DC - Under pressure" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "20706B1C-B8CC-45AB-AF41-4D52E55583BB", "54167D4D-2890-4477-87BB-073353665685", @"DC - Motivation" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "20706B1C-B8CC-45AB-AF41-4D52E55583BB", "8454BAE8-154D-4F38-B151-B4FAC159A39C", @"DC - Strengths" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "20706B1C-B8CC-45AB-AF41-4D52E55583BB", "9AFC7328-2E1D-4F63-9BCB-438BB48B3379", @"DC - Title" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "20706B1C-B8CC-45AB-AF41-4D52E55583BB", "9BDA13BB-2DEB-47F5-86CA-71C2C1454FEF", @"DC - Team Contribution" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "20706B1C-B8CC-45AB-AF41-4D52E55583BB", "A500116E-37CB-400F-996B-A56940FD24E9", @"" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "20706B1C-B8CC-45AB-AF41-4D52E55583BB", "BE5497AF-8575-4619-8E53-CCED2C237465", @"DC - Challenges" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "20706B1C-B8CC-45AB-AF41-4D52E55583BB", "EE6796B4-6A7B-49D6-AF40-3E4A01A0C8E8", @"DC - Leadership Style" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "227A4C9A-F92B-48D7-9E2B-62D328369C3D", "4005EA8E-4243-4EBA-B04D-7D54860BD61E", @"CI - Follower Style" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "227A4C9A-F92B-48D7-9E2B-62D328369C3D", "49AA57E0-AF90-491A-A80D-CC7383634A30", @"CI - Under Pressure" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "227A4C9A-F92B-48D7-9E2B-62D328369C3D", "54167D4D-2890-4477-87BB-073353665685", @"CI - Motivation" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "227A4C9A-F92B-48D7-9E2B-62D328369C3D", "8454BAE8-154D-4F38-B151-B4FAC159A39C", @"CI - Strengths" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "227A4C9A-F92B-48D7-9E2B-62D328369C3D", "9AFC7328-2E1D-4F63-9BCB-438BB48B3379", @"CI - Title" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "227A4C9A-F92B-48D7-9E2B-62D328369C3D", "9BDA13BB-2DEB-47F5-86CA-71C2C1454FEF", @"CI - Team Contribution" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "227A4C9A-F92B-48D7-9E2B-62D328369C3D", "A500116E-37CB-400F-996B-A56940FD24E9", @"" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "227A4C9A-F92B-48D7-9E2B-62D328369C3D", "BE5497AF-8575-4619-8E53-CCED2C237465", @"CI - Challenges" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "227A4C9A-F92B-48D7-9E2B-62D328369C3D", "EE6796B4-6A7B-49D6-AF40-3E4A01A0C8E8", @"CI - Leadership Style" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "3FDC0A3C-A826-4252-90A4-4BF710C12311", "4005EA8E-4243-4EBA-B04D-7D54860BD61E", @"CS - Follower" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "3FDC0A3C-A826-4252-90A4-4BF710C12311", "49AA57E0-AF90-491A-A80D-CC7383634A30", @"CS - Under Pressure" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "3FDC0A3C-A826-4252-90A4-4BF710C12311", "54167D4D-2890-4477-87BB-073353665685", @"CS - Motivation" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "3FDC0A3C-A826-4252-90A4-4BF710C12311", "8454BAE8-154D-4F38-B151-B4FAC159A39C", @"Strengths" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "3FDC0A3C-A826-4252-90A4-4BF710C12311", "9AFC7328-2E1D-4F63-9BCB-438BB48B3379", @"CS - Title" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "3FDC0A3C-A826-4252-90A4-4BF710C12311", "9BDA13BB-2DEB-47F5-86CA-71C2C1454FEF", @"CS - Team Contribution" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "3FDC0A3C-A826-4252-90A4-4BF710C12311", "A500116E-37CB-400F-996B-A56940FD24E9", @"" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "3FDC0A3C-A826-4252-90A4-4BF710C12311", "BE5497AF-8575-4619-8E53-CCED2C237465", @"CS - Challenges" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "3FDC0A3C-A826-4252-90A4-4BF710C12311", "EE6796B4-6A7B-49D6-AF40-3E4A01A0C8E8", @"CS - Leadership" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "43164D7B-9521-40FF-8877-FAFA1C753284", "4005EA8E-4243-4EBA-B04D-7D54860BD61E", @"DI - Follower Style" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "43164D7B-9521-40FF-8877-FAFA1C753284", "49AA57E0-AF90-491A-A80D-CC7383634A30", @"DI - Under Pressure" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "43164D7B-9521-40FF-8877-FAFA1C753284", "54167D4D-2890-4477-87BB-073353665685", @"DI - Motivation" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "43164D7B-9521-40FF-8877-FAFA1C753284", "8454BAE8-154D-4F38-B151-B4FAC159A39C", @"DI - Strengths" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "43164D7B-9521-40FF-8877-FAFA1C753284", "9AFC7328-2E1D-4F63-9BCB-438BB48B3379", @"DI - Title" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "43164D7B-9521-40FF-8877-FAFA1C753284", "9BDA13BB-2DEB-47F5-86CA-71C2C1454FEF", @"DI - Team Contribution" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "43164D7B-9521-40FF-8877-FAFA1C753284", "A500116E-37CB-400F-996B-A56940FD24E9", @"" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "43164D7B-9521-40FF-8877-FAFA1C753284", "BE5497AF-8575-4619-8E53-CCED2C237465", @"DI - Challenges" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "43164D7B-9521-40FF-8877-FAFA1C753284", "EE6796B4-6A7B-49D6-AF40-3E4A01A0C8E8", @"DI - Leadership Style" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "5580070A-3D01-4CB6-BC45-2645E222DC21", "4005EA8E-4243-4EBA-B04D-7D54860BD61E", @"SD - Follower Style" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "5580070A-3D01-4CB6-BC45-2645E222DC21", "49AA57E0-AF90-491A-A80D-CC7383634A30", @"SD - Under Pressure" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "5580070A-3D01-4CB6-BC45-2645E222DC21", "54167D4D-2890-4477-87BB-073353665685", @"SD - Motivation" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "5580070A-3D01-4CB6-BC45-2645E222DC21", "8454BAE8-154D-4F38-B151-B4FAC159A39C", @"SD - Strengths" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "5580070A-3D01-4CB6-BC45-2645E222DC21", "9AFC7328-2E1D-4F63-9BCB-438BB48B3379", @"SD - Title" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "5580070A-3D01-4CB6-BC45-2645E222DC21", "9BDA13BB-2DEB-47F5-86CA-71C2C1454FEF", @"SD - Team Contribution" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "5580070A-3D01-4CB6-BC45-2645E222DC21", "A500116E-37CB-400F-996B-A56940FD24E9", @"" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "5580070A-3D01-4CB6-BC45-2645E222DC21", "BE5497AF-8575-4619-8E53-CCED2C237465", @"SD - Challenges" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "5580070A-3D01-4CB6-BC45-2645E222DC21", "EE6796B4-6A7B-49D6-AF40-3E4A01A0C8E8", @"SD - Leadership Style" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "879149E8-D396-41D9-92C2-EC9E62BE8916", "4005EA8E-4243-4EBA-B04D-7D54860BD61E", @"IC - Follower Style" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "879149E8-D396-41D9-92C2-EC9E62BE8916", "49AA57E0-AF90-491A-A80D-CC7383634A30", @"IC - Under Pressure" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "879149E8-D396-41D9-92C2-EC9E62BE8916", "54167D4D-2890-4477-87BB-073353665685", @"IC - Motivation" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "879149E8-D396-41D9-92C2-EC9E62BE8916", "8454BAE8-154D-4F38-B151-B4FAC159A39C", @"IC - Strengths" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "879149E8-D396-41D9-92C2-EC9E62BE8916", "9AFC7328-2E1D-4F63-9BCB-438BB48B3379", @"IC - Title" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "879149E8-D396-41D9-92C2-EC9E62BE8916", "9BDA13BB-2DEB-47F5-86CA-71C2C1454FEF", @"IC - Team Contribution" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "879149E8-D396-41D9-92C2-EC9E62BE8916", "A500116E-37CB-400F-996B-A56940FD24E9", @"" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "879149E8-D396-41D9-92C2-EC9E62BE8916", "BE5497AF-8575-4619-8E53-CCED2C237465", @"IC - Challenges" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "879149E8-D396-41D9-92C2-EC9E62BE8916", "EE6796B4-6A7B-49D6-AF40-3E4A01A0C8E8", @"IC - Leadership Style" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8B33090D-DD62-4BBB-BFDA-2DC67F26745D", "4005EA8E-4243-4EBA-B04D-7D54860BD61E", @"D - Follower Style" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8B33090D-DD62-4BBB-BFDA-2DC67F26745D", "49AA57E0-AF90-491A-A80D-CC7383634A30", @"D - Under Pressure" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8B33090D-DD62-4BBB-BFDA-2DC67F26745D", "54167D4D-2890-4477-87BB-073353665685", @"D - Motivation" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8B33090D-DD62-4BBB-BFDA-2DC67F26745D", "8454BAE8-154D-4F38-B151-B4FAC159A39C", @"D - Strength" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8B33090D-DD62-4BBB-BFDA-2DC67F26745D", "9AFC7328-2E1D-4F63-9BCB-438BB48B3379", @"D- Title" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8B33090D-DD62-4BBB-BFDA-2DC67F26745D", "9BDA13BB-2DEB-47F5-86CA-71C2C1454FEF", @"D - Team Contribution" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8B33090D-DD62-4BBB-BFDA-2DC67F26745D", "A500116E-37CB-400F-996B-A56940FD24E9", @"{
  ""d"": {
    ""T"": ""-1"",
    ""P"": ""1""
  },
  ""i"": {
    ""T"": ""-2"",
    ""P"": ""2""
  },
  ""s"": {
    ""T"": ""4"",
    ""P"": ""-2""
  },
  ""c"": {
    ""T"": ""-2"",
    ""P"": ""-4""
  }
}" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8B33090D-DD62-4BBB-BFDA-2DC67F26745D", "BE5497AF-8575-4619-8E53-CCED2C237465", @"D - Challenge" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8B33090D-DD62-4BBB-BFDA-2DC67F26745D", "EE6796B4-6A7B-49D6-AF40-3E4A01A0C8E8", @"D - Leadership style" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "99D3129C-295C-471A-8E8A-804A9EBF36B9", "4005EA8E-4243-4EBA-B04D-7D54860BD61E", @"IS - Follower Style" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "99D3129C-295C-471A-8E8A-804A9EBF36B9", "49AA57E0-AF90-491A-A80D-CC7383634A30", @"IS - Under Pressure" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "99D3129C-295C-471A-8E8A-804A9EBF36B9", "54167D4D-2890-4477-87BB-073353665685", @"IS - Motivation" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "99D3129C-295C-471A-8E8A-804A9EBF36B9", "8454BAE8-154D-4F38-B151-B4FAC159A39C", @"IS - Strengths" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "99D3129C-295C-471A-8E8A-804A9EBF36B9", "9AFC7328-2E1D-4F63-9BCB-438BB48B3379", @"IS - Title" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "99D3129C-295C-471A-8E8A-804A9EBF36B9", "9BDA13BB-2DEB-47F5-86CA-71C2C1454FEF", @"IS - Team Contribution" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "99D3129C-295C-471A-8E8A-804A9EBF36B9", "A500116E-37CB-400F-996B-A56940FD24E9", @"" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "99D3129C-295C-471A-8E8A-804A9EBF36B9", "BE5497AF-8575-4619-8E53-CCED2C237465", @"IS - Challenges" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "99D3129C-295C-471A-8E8A-804A9EBF36B9", "EE6796B4-6A7B-49D6-AF40-3E4A01A0C8E8", @"IS - Leadership Style" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "AC246F37-0858-4692-A373-C0F125036355", "4005EA8E-4243-4EBA-B04D-7D54860BD61E", @"SC - Follower Style" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "AC246F37-0858-4692-A373-C0F125036355", "49AA57E0-AF90-491A-A80D-CC7383634A30", @"SC - Under Pressure" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "AC246F37-0858-4692-A373-C0F125036355", "54167D4D-2890-4477-87BB-073353665685", @"SC - Motivation" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "AC246F37-0858-4692-A373-C0F125036355", "8454BAE8-154D-4F38-B151-B4FAC159A39C", @"SC - Strength" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "AC246F37-0858-4692-A373-C0F125036355", "9AFC7328-2E1D-4F63-9BCB-438BB48B3379", @"SC - Title" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "AC246F37-0858-4692-A373-C0F125036355", "9BDA13BB-2DEB-47F5-86CA-71C2C1454FEF", @"SC - Team Contribution" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "AC246F37-0858-4692-A373-C0F125036355", "A500116E-37CB-400F-996B-A56940FD24E9", @"" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "AC246F37-0858-4692-A373-C0F125036355", "BE5497AF-8575-4619-8E53-CCED2C237465", @"SC - Challenges" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "AC246F37-0858-4692-A373-C0F125036355", "EE6796B4-6A7B-49D6-AF40-3E4A01A0C8E8", @"SC - Leadership Style" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "AD076E20-ADAB-4DF3-9598-8F9FCC19B9F0", "4005EA8E-4243-4EBA-B04D-7D54860BD61E", @"I - Follower Style" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "AD076E20-ADAB-4DF3-9598-8F9FCC19B9F0", "49AA57E0-AF90-491A-A80D-CC7383634A30", @"I - Under Pressure" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "AD076E20-ADAB-4DF3-9598-8F9FCC19B9F0", "54167D4D-2890-4477-87BB-073353665685", @"I - Motivation" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "AD076E20-ADAB-4DF3-9598-8F9FCC19B9F0", "8454BAE8-154D-4F38-B151-B4FAC159A39C", @"I - Strengths" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "AD076E20-ADAB-4DF3-9598-8F9FCC19B9F0", "9AFC7328-2E1D-4F63-9BCB-438BB48B3379", @"I - Title" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "AD076E20-ADAB-4DF3-9598-8F9FCC19B9F0", "9BDA13BB-2DEB-47F5-86CA-71C2C1454FEF", @"I - Team Contribution" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "AD076E20-ADAB-4DF3-9598-8F9FCC19B9F0", "A500116E-37CB-400F-996B-A56940FD24E9", @"{
  ""d"": {
    ""T"": ""-2"",
    ""P"": ""2""
  },
  ""i"": {
    ""T"": ""-3"",
    ""P"": ""4""
  },
  ""s"": {
    ""T"": ""4"",
    ""P"": ""-1""
  },
  ""c"": {
    ""T"": ""1"",
    ""P"": ""-3""
  }
}" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "AD076E20-ADAB-4DF3-9598-8F9FCC19B9F0", "BE5497AF-8575-4619-8E53-CCED2C237465", @"I - Challenges" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "AD076E20-ADAB-4DF3-9598-8F9FCC19B9F0", "EE6796B4-6A7B-49D6-AF40-3E4A01A0C8E8", @"I - Leadership Style" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "B428B0CA-2E41-4B3E-9A37-5B618C116CA3", "4005EA8E-4243-4EBA-B04D-7D54860BD61E", @"C - Follower Style" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "B428B0CA-2E41-4B3E-9A37-5B618C116CA3", "49AA57E0-AF90-491A-A80D-CC7383634A30", @"C - Under Pressure" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "B428B0CA-2E41-4B3E-9A37-5B618C116CA3", "54167D4D-2890-4477-87BB-073353665685", @"C - Motivation" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "B428B0CA-2E41-4B3E-9A37-5B618C116CA3", "8454BAE8-154D-4F38-B151-B4FAC159A39C", @"C - Strengths" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "B428B0CA-2E41-4B3E-9A37-5B618C116CA3", "9AFC7328-2E1D-4F63-9BCB-438BB48B3379", @"C - Title" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "B428B0CA-2E41-4B3E-9A37-5B618C116CA3", "9BDA13BB-2DEB-47F5-86CA-71C2C1454FEF", @"C - Team Contribution" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "B428B0CA-2E41-4B3E-9A37-5B618C116CA3", "A500116E-37CB-400F-996B-A56940FD24E9", @"{
  ""d"": {
    ""T"": ""-1"",
    ""P"": ""-4""
  },
  ""i"": {
    ""T"": ""2"",
    ""P"": ""-3""
  },
  ""s"": {
    ""T"": ""3"",
    ""P"": ""3""
  },
  ""c"": {
    ""T"": ""2"",
    ""P"": ""4""
  }
}" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "B428B0CA-2E41-4B3E-9A37-5B618C116CA3", "BE5497AF-8575-4619-8E53-CCED2C237465", @"C - Challenges" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "B428B0CA-2E41-4B3E-9A37-5B618C116CA3", "EE6796B4-6A7B-49D6-AF40-3E4A01A0C8E8", @"C - Leadership Style" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C742748E-E86F-4145-962B-3468CC563D71", "4005EA8E-4243-4EBA-B04D-7D54860BD61E", @"CD - Follower" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C742748E-E86F-4145-962B-3468CC563D71", "49AA57E0-AF90-491A-A80D-CC7383634A30", @"CD - Under Pressure" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C742748E-E86F-4145-962B-3468CC563D71", "54167D4D-2890-4477-87BB-073353665685", @"CD - Motivation" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C742748E-E86F-4145-962B-3468CC563D71", "8454BAE8-154D-4F38-B151-B4FAC159A39C", @"CD - Strengths" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C742748E-E86F-4145-962B-3468CC563D71", "9AFC7328-2E1D-4F63-9BCB-438BB48B3379", @"CD - Title" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C742748E-E86F-4145-962B-3468CC563D71", "9BDA13BB-2DEB-47F5-86CA-71C2C1454FEF", @"CD - Team Contribution" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C742748E-E86F-4145-962B-3468CC563D71", "A500116E-37CB-400F-996B-A56940FD24E9", @"" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C742748E-E86F-4145-962B-3468CC563D71", "BE5497AF-8575-4619-8E53-CCED2C237465", @"CD - Challenges" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C742748E-E86F-4145-962B-3468CC563D71", "EE6796B4-6A7B-49D6-AF40-3E4A01A0C8E8", @"CD - Leadership Style" );
        
            // update to computed column to more safely do ValueAsDateTime
            Sql( @"
    ALTER TABLE [dbo].[AttributeValue] DROP COLUMN [ValueAsDateTime]
    ALTER TABLE [dbo].[AttributeValue] ADD [ValueAsDateTime] AS CASE WHEN [value] LIKE '____-__-__T__:__:__________' THEN CONVERT(datetime, CONVERT(datetimeoffset, [value])) ELSE NULL END
" );
            // fix typo in badges
            Sql( @"UPDATE PersonBadge SET [Description] = 'Show if individual is in a serving team.' WHERE [Guid] = 'E0455598-82B0-4F49-B806-C3A41C71E9DA'");
        
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "4005EA8E-4243-4EBA-B04D-7D54860BD61E" );
            RockMigrationHelper.DeleteAttribute( "49AA57E0-AF90-491A-A80D-CC7383634A30" );
            RockMigrationHelper.DeleteAttribute( "54167D4D-2890-4477-87BB-073353665685" );
            RockMigrationHelper.DeleteAttribute( "8454BAE8-154D-4F38-B151-B4FAC159A39C" );
            RockMigrationHelper.DeleteAttribute( "9AFC7328-2E1D-4F63-9BCB-438BB48B3379" );
            RockMigrationHelper.DeleteAttribute( "9BDA13BB-2DEB-47F5-86CA-71C2C1454FEF" );
            RockMigrationHelper.DeleteAttribute( "A500116E-37CB-400F-996B-A56940FD24E9" );
            RockMigrationHelper.DeleteAttribute( "BE5497AF-8575-4619-8E53-CCED2C237465" );
            RockMigrationHelper.DeleteAttribute( "EE6796B4-6A7B-49D6-AF40-3E4A01A0C8E8" );
            RockMigrationHelper.DeleteDefinedValue( "03B518C2-F9EB-4B84-A470-45EBEBAB7F17" );
            RockMigrationHelper.DeleteDefinedValue( "03DE220E-7924-47A5-90D4-D6383285340C" );
            RockMigrationHelper.DeleteDefinedValue( "05868DFC-BED9-497D-8998-6B5474280568" );
            RockMigrationHelper.DeleteDefinedValue( "09D96C14-1F68-4454-954B-7EC8898B0A86" );
            RockMigrationHelper.DeleteDefinedValue( "20706B1C-B8CC-45AB-AF41-4D52E55583BB" );
            RockMigrationHelper.DeleteDefinedValue( "227A4C9A-F92B-48D7-9E2B-62D328369C3D" );
            RockMigrationHelper.DeleteDefinedValue( "3FDC0A3C-A826-4252-90A4-4BF710C12311" );
            RockMigrationHelper.DeleteDefinedValue( "43164D7B-9521-40FF-8877-FAFA1C753284" );
            RockMigrationHelper.DeleteDefinedValue( "5580070A-3D01-4CB6-BC45-2645E222DC21" );
            RockMigrationHelper.DeleteDefinedValue( "879149E8-D396-41D9-92C2-EC9E62BE8916" );
            RockMigrationHelper.DeleteDefinedValue( "8B33090D-DD62-4BBB-BFDA-2DC67F26745D" );
            RockMigrationHelper.DeleteDefinedValue( "99D3129C-295C-471A-8E8A-804A9EBF36B9" );
            RockMigrationHelper.DeleteDefinedValue( "AC246F37-0858-4692-A373-C0F125036355" );
            RockMigrationHelper.DeleteDefinedValue( "AD076E20-ADAB-4DF3-9598-8F9FCC19B9F0" );
            RockMigrationHelper.DeleteDefinedValue( "B428B0CA-2E41-4B3E-9A37-5B618C116CA3" );
            RockMigrationHelper.DeleteDefinedValue( "C742748E-E86F-4145-962B-3468CC563D71" );
            RockMigrationHelper.DeleteDefinedType( "F06DDAD8-6058-4182-AD0A-B523BB7A2D78" );
        }
    }
}
