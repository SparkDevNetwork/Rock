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
using System;

namespace Rock.Tests.Shared
{
    /// <summary>
    /// Well-known Guids that can be used to identify specific test records.
    /// Values should be declared as string types where possible.
    /// Some Guid entries have been imported from previous code.
    /// </summary>
    public static class TestGuids
    {
        // A Guid Reference that does not match any entity in a Rock database.
        public const string NoMatch = "00000000-0000-0000-0000-000000000001";

        public static class Crm
        {
            public const string CampusMain = "76882AE3-1CE8-42A6-A2B6-8C0B29CF8CF8";
            public const string CampusSouth = "5434F90B-BB91-4A34-8B09-8C117A9E5D95";
            public const string CampusSteppingStone = "089844AF-6310-4C20-9434-A845F982B0C5";
        }

        public static class Category
        {
            public const string DataViewLocations = "1D45C0A7-3DE8-428C-94A8-14E5ED5E2E36";

            public const string PrayerRequestAllChurch = "5A94E584-35F0-4214-91F1-D72531CC6325";
            public const string PrayerRequestFinancesAndJob = "C35752E2-A575-4182-8B5A-8AE9D0694B97";
            public const string PrayerRequestComfortAndGrief = "D12837CF-EEE7-4822-9899-36E8884CD848";
            public const string PrayerRequestHealthIssues = "5C63A5B0-B2CF-4476-8F0E-D415BEE163D8";
            public const string PrayerRequestFinancesOther = "B74A8CF7-5B19-4F2A-B527-9665829F6369";
            public const string PrayerRequestFinancesOnly = "DA235296-0E77-4352-A5F7-08C66AF92726";
            public const string PrayerRequestJobOnly = "99136D63-6ACD-4496-8D08-793EE917978B";

            public const string SystemEmailSystem = "9AF1FA93-089B-44B7-83A3-48E0031CCC1D";
        }

        public static class Communications
        {
            public static string TestCommunicationSingleEmail1Guid = "{BDCDD3ED-22FF-43E8-9860-65D26DBD5B9B}";
            public static string TestCommunicationBulkEmail1Guid = "{D3EA6513-372D-4192-8E8F-DCA2707AA572}";
            public static string TestCommunicationBulkEmailNoInteractionsGuid = "{42AECB98-76BE-4CD5-A9FA-14FE2CC27887}";
            public static string TestCommunicationBulkSmsGuid = "{79E18AFD-DB14-485A-80D9-CDAC44CA1098}";
            public static string TestTwilioTransportEntityTypeGuid = "{CF9FD146-8623-4D9A-98E6-4BD710F071A4}";

            public static string TestSmsSenderGuid = "{DB8CFE73-4209-4109-8FC5-3C5013AFC290}";
            public static string MobilePhoneTedDecker = "6235553322";
            public static string TedDeckerPersonGuid = "{8FEDC6EE-8630-41ED-9FC5-C7157FD1EAA4}";

            public static string NamelessPerson1MobileNumber = "4807770001";
            public static string NamelessPerson2MobileNumber = "4807770002";

            public static string UnknownPerson1MobileNumber = "4807770101";
        }

        public static class DataViews
        {
            public static string MembersAndAttendees = "CB4BB264-A1F4-4EDB-908F-2CCF3A534BC7";
            public static string AdultMembersAndAttendees = "0da5f82f-cffe-45af-b725-49b3899a1f72";
            public static string AdultMembersAndAttendeesMales = "D0846DB2-7AE8-4897-867F-1AF5CC1DE95A";
            public static string AdultMembersAndAttendeesFemales = "1D144B43-F985-4122-B2E3-D548CC3B2A82";
            public static string Males = "c43983d7-1f22-4e94-9f5c-342da3a0e168";
            public static string Females = "e2cc2258-bf35-4db2-91e2-9be1b68156a3";
            public static string UnderThirtyFive = "ff608842-bb10-4c9c-aa18-9d5c407590d3";
            public static string ThirtyFiveAndOlder = "5537d54c-1b9b-4b81-aa63-f10d676fae77";

            public static string LocationsOutsideArizona = "14B1854D-4F45-4F4D-AFFF-C0A1E06353DF";
            public static string LocationsInsideArizona = "C39B353E-3E44-42C0-9D85-2107FB5E8C04";
        }

        public static class DataFilters
        {
            public static string ContentChannel = "4049b6d7-ba63-494d-b0fd-8171617a1db9";
        }

        public static class Events
        {
            public const string EventIdentifierRockSolidFinancesClass = "Rock Solid Finances Class";
        }

        public static class Groups
        {
            public static string CategoryGroupsGuid = "5CF5224C-F01D-4904-91D0-E58B723F0D2A";
            public static string CategoryLocationsGuid = "1D45C0A7-3DE8-428C-94A8-14E5ED5E2E36";

            public static string SmallGroupDeckerGuid = "62dc3753-01d5-48b5-b22d-d2825d92900b";
            public static string SmallGroupAlishaMarbleGuid = "10b60f8d-0f23-4faa-b35f-9a5f19f5f995";
            public static string SmallGroupMarbleGuid = "90b2ceda-ae3a-4c10-a2e1-b987020379ad";
        }

        public static class TestPeople
        {
            public static string TedDecker = "8FEDC6EE-8630-41ED-9FC5-C7157FD1EAA4";
            public static string CindyDecker = "B71494DB-D809-451A-A950-28898D0FD92C";
            public static string AlexDecker = "27919690-3CCE-4FA6-95C4-CD21419EB51F";
            public static string NoahDecker = "32aab9e4-970d-4551-a17e-385e66113bd5";

            public static string BillMarble = "1EA811BB-3118-42D1-B020-32A82BC8081A";
            public static string AlishaMarble = "69DC0FDC-B451-4303-BD91-EF17C0015D23";
            public static string ThomasMiller = "955bf3e9-d38a-4dcb-a6f2-edf5ec2571c5";

            public static string SarahSimmons = "FC6B9819-EF2E-44C9-93DB-05571B39E58F";

            public static string BenJones = "3C402382-3BD2-4337-A996-9E62F1BAB09D";
            public static string BrianJones = "3D7F6605-3666-4AB5-9F4E-D7FEBF93278E";
            public static string MariahJackson = "9C2A020B-CF34-403E-A948-3E91FDFB958B";

            public static string CraigLowe = "b701e5d2-adbd-4c66-91e8-3d6cd298d9a0";
            public static string MaddieLowe = "C398A8E3-C9BC-4017-A3F6-7C2BFF654056";

            public static string SamHanks = "1E6E66C7-A487-48E6-B064-7F9F4DDE6680";
        }

        public static class PrayerRequestGuid
        {
            public const string AllChurchForEmployment = "197F4092-B593-4A4D-9A89-74FACE0052A2";
            public const string TedDeckerForEmployment = "07F335E8-167C-42E5-B6BF-39756092BBB0";
            public const string MariahJacksonForMother = "DAAB8E38-A5C2-4132-A64E-B17ADE9E40B5";
            public const string BenJonesForCarFinance = "D049FA48-1558-49B9-BD8D-A4E3F9D0B0CF";
            public const string SarahSimmonsForFinancialWisdom = "3DEDEED8-D260-4121-8F0F-52E420A799DB";
        }

        public static class PrayerRequestCommentGuid
        {
            public const string FinancesAndJobW1C1 = "C3C97B7A-06E4-4553-8EC4-84D9033B122E";
            public const string FinancesAndJobW1C2 = "21308403-037A-41CC-B059-92DE2E908838";
            public const string FinancesAndJobW2C1 = "BF723E1F-0B6C-44F3-8B40-78245A0D6F31";
            public const string FinancesAndJobWeek4 = "ED0C2D67-EBB4-4711-81B7-1288C5E63B1A";

            public const string TedDecker11 = "6724CC3F-7381-4C71-8861-3FA18675C6A0";
            public const string TedDecker12 = "F8EC1128-EF03-4754-8B73-44C26751CB72";
            public const string TedDecker21 = "2F97ED48-3077-4BF5-A7A6-EA10316F3B90";
            public const string TedDecker22 = "A3D247DF-58F9-41CB-8FE9-1628E4F3B0B5";

            public const string MariahJacksonComment1 = "2EA84150-C6F5-4CC9-BCFB-023D49FC5B3F";
            public const string MariahJacksonComment2 = "5AAA63D8-13BA-4573-BBD5-7BB22508E648";
            public const string MariahJacksonComment3 = "88DDC7F4-0E25-4302-AA8E-632608C5A294";

            public const string BenJonesComment1 = "7C5173DF-B11B-4EF2-8CCB-30DC53A4522B";
            public const string BenJonesComment2 = "4619DEFF-DA42-4A75-AA24-77A7F3292EBA";

            public const string SarahSimmonsComment1 = "1CD643A0-DED5-4550-81C7-A6C6CACF0C87";
        }

        public static class Schedules
        {

            public const string ScheduleSat1630Guid = "7883CAC8-6E30-482B-95A7-2F0DEE859BE1";
            public const string ScheduleSat1800Guid = "33FF69E9-059B-4702-B1E5-4D499CB7B07A";
            public const string ScheduleSun0900Guid = "FF6FB240-0C32-4542-BE40-159C522F7E51";
            public const string ScheduleSun1030Guid = "4628D917-EC9C-4269-B39C-5BCBEF3658E7";
            public const string ScheduleSun1200Guid = "1F6C15DA-982F-43B1-BDE9-D4E70CFBCB45";

            public const string ScheduleDaily1630Guid = "A5C81078-EB8C-46AA-BB91-1E2BA8BA76AE";
        }

        public static class Steps
        {
            // Constants
            public static Guid CategoryAdultsGuid = new Guid( "43DC43A8-420B-4012-BAA0-0A0DDF2D4A9A" );
            public static Guid CategoryYouthGuid = new Guid( "EAB10217-F288-4B29-B56F-50BD4BA5FB08" );
            public static Guid CategoryStepsGuid = new Guid( "31A23665-1C2E-4901-9E20-A7A0C7E6DF70" );

            public static Guid ProgramSacramentsGuid = new Guid( "2CAFBB12-901F-4880-A3E4-848F25CAF1A6" );
            public static Guid StepTypeBaptismGuid = new Guid( "23E73F78-587A-483F-99EF-855FD6AD1B11" );
            public static Guid StepTypeEucharistGuid = new Guid( "5EA01E79-5D17-4E87-A94F-8C4DD22131B5" );
            public static Guid StepTypeAnnointingGuid = new Guid( "48141631-38F6-40C0-8470-5253208CEA9A" );
            public static Guid StepTypeConfessionGuid = new Guid( "5F754006-7F8C-4BED-94A8-FC61CEEC6B43" );
            public static Guid StepTypeMarriageGuid = new Guid( "D03B3C65-C128-4918-A300-509B94B90175" );
            public static Guid StepTypeHolyOrdersGuid = new Guid( "0099C701-6C1E-418E-A94F-C247A2FE4BA5" );
            public static Guid StepTypeConfirmationGuid = new Guid( "F109169F-C1F6-46ED-9091-274540E3F3E2" );
            public static Guid StatusSacramentsSuccessGuid = new Guid( "A5C2A14F-9ED9-4DF4-A1C8-8ADF75E18833" );
            public static Guid StatusSacramentsPendingGuid = new Guid( "B591240C-4D4D-49DA-82E3-F8C1738B8EC6" );
            public static Guid StatusSacramentsIncompleteGuid = new Guid( "D0CDE46C-46D6-4F8D-B48D-55D7ACE6C3BA" );
            public static Guid PrerequisiteHolyOrdersGuid = new Guid( "D96A2C67-2D76-4697-838F-3514CA11485E" );

            public static Guid ProgramAlphaGuid = new Guid( "F7C2BA07-579B-4800-BBE1-B14B73E21E12" );
            public static Guid StepTypeAttenderGuid = new Guid( "24A70E5C-9871-452E-9403-3B43C003AA87" );
            public static Guid StepTypeVolunteerGuid = new Guid( "31DB3478-840B-4541-972D-059690AD623E" );
            public static Guid StepTypeLeaderGuid = new Guid( "95DF3AC6-AFBF-4BEA-AF86-805335A1C4CB" );
            public static Guid StatusAlphaStartedGuid = new Guid( "F29CF8A1-76A9-4436-9726-810DF0BC95C7" );
            public static Guid StatusAlphaCompletedGuid = new Guid( "7BA5F14D-BB38-4AAB-AB69-3A7F49494A55" );

            // Known Step Achievements
            // These steps are specifically identified to enable verification of query and filtering operations.
            public static Guid TedDeckerPersonGuid = new Guid( "8FEDC6EE-8630-41ED-9FC5-C7157FD1EAA4" );
            public static Guid TedDeckerStepBaptismGuid = new Guid( "02BB71C9-5FE9-45B8-B230-51C7A8475B6B" );
            public static Guid TedDeckerStepConfirmationGuid = new Guid( "15412166-1DE0-41C0-9F8D-F07160513CAE" );
            public static Guid TedDeckerStepMarriage1Guid = new Guid( "414EBE88-2CD2-40E1-893B-216DEA2CB25E" );
            public static Guid TedDeckerStepMarriage2Guid = new Guid( "314F5303-C803-442B-AE39-DAA7BC30CCEE" );

            public static Guid BillMarblePersonGuid = new Guid( "1EA811BB-3118-42D1-B020-32A82BC8081A" );
            public static Guid BillMarbleStepBaptismGuid = new Guid( "CBD25E24-FD14-4B69-884B-F78DA8BF8FD8" );
            public static Guid BillMarbleStepConfirmationGuid = new Guid( "FE6B8F0D-450B-4737-80EE-E2BC4FA4EAB5" );
            public static Guid BillMarbleStepMarriage1Guid = new Guid( "E6F37E9D-177F-4CEA-91EA-2C2B45C4B1FD" );
            public static Guid BillMarbleStepAlphaAttenderGuid = new Guid( "680F64F1-C579-4216-B08C-68AF3FC762EC" );

            public static Guid AlishaMarblePersonGuid = new Guid( "69DC0FDC-B451-4303-BD91-EF17C0015D23" );
            public static Guid AlishaMarbleStepBaptismGuid = new Guid( "12DF97EE-52E8-49CE-B804-2568B8837B81" );
            public static Guid AlishaMarbleStepConfirmationGuid = new Guid( "6B3037D8-23AC-478C-BCC9-7483B86976AA" );
            public static Guid AlishaMarbleStepMarriage1Guid = new Guid( "FB2BA405-4233-4E7E-83AB-8A94B5E2A80A" );
            public static Guid AlishaMarbleStepAlphaAttenderGuid = new Guid( "87976C94-C9E4-4839-9EFF-1E0B3F4EF5A0" );

            public static Guid SarahSimmonsPersonGuid = new Guid( "FC6B9819-EF2E-44C9-93DB-05571B39E58F" );
            public static Guid SarahSimmonsStepConfirmationGuid = new Guid( "E41A74DE-72C7-40B5-B05C-C1A72E09F5C0" );
            public static Guid SarahSimmonsStepMarriage1Guid = new Guid( "94342ACA-744B-41A0-9076-0BCB808514CF" );
            public static Guid SarahSimmonsStepAlphaAttenderGuid = new Guid( "B5907E4F-6197-4A9F-A1DB-046DE7BB2035" );

            public static Guid BrianJonesPersonGuid = new Guid( "3D7F6605-3666-4AB5-9F4E-D7FEBF93278E" );
            public static Guid BrianJonesStepBaptismGuid = new Guid( "92171B1E-25C8-408B-AA93-AB7263D02B5A" );

            public static Guid BenJonesPersonGuid = new Guid( "3C402382-3BD2-4337-A996-9E62F1BAB09D" );
            public static Guid BenJonesStepAlphaAttenderGuid = new Guid( "D5DDBBE4-9D62-4EDE-840D-E9DAB8F99430" );

            public static string ColorCodeRed = "#f00";
            public static string ColorCodeGreen = "#0f0";
            public static string ColorCodeBlue = "#00f";

            // A DataView that returns all of the people who have been baptised in 2001.
            public static Guid DataViewStepsCompleted2001Guid = new Guid( "E52D89CD-19A0-44B3-8052-D4FD206AB499" );
        }

        public static class SystemEmailGuid
        {
            public const string PrayerCommentsNotification = "FAEA9DE5-62CE-4EEE-960B-C06103E97AA9";
        }

        public static class Shortcodes
        {
            public const string ShortcodeTest1 = "B60A5547-FA5C-49C3-AC14-D631E54208AE";
        }

        /// <summary>
        /// Well-known Guids to identify specific Lava Engine framework implementations.
        /// </summary>
        public static class LavaEngines
        {
            public const string RockLiquid = "7CE5A54A-54DC-464A-83EE-659658134239";
            public const string DotLiquid = "165E5E74-4923-4E25-B330-A27DB3A59CE7";
            public const string Fluid = "605445FE-6ECC-4E67-9A95-98F7173F7389";

        }
    }
}
