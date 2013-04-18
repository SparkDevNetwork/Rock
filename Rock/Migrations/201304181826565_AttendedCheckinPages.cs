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
    public partial class AttendedCheckinPages : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddPage( "32A132A6-63A2-4840-B4A5-23D80994CCBD", "Admin", "Admin screen for Attended Checkin", "Checkin", "771E3CF1-63BD-4880-BC43-AC29B4CCE963" );
            AddPage( "32A132A6-63A2-4840-B4A5-23D80994CCBD", "Search", "Search screen for Attended Checkin", "Checkin", "8F618315-F554-4751-AB7F-00CC5658120A" );
            AddPage( "32A132A6-63A2-4840-B4A5-23D80994CCBD", "Family Select", "Family select for Attended Checkin", "Checkin", "AF83D0B2-2995-4E46-B0DF-1A4763637A68" );
            AddPage( "32A132A6-63A2-4840-B4A5-23D80994CCBD", "Activity Select", "Activity select for Attended Checkin", "Checkin", "C87916FE-417E-4A11-8831-5CFA7678A228" );
            AddPage( "32A132A6-63A2-4840-B4A5-23D80994CCBD", "Confirmation", "Confirmation screen for Attended Checkin", "Checkin", "BE996C9B-3DFE-407F-BD53-D6F58D85A035" );
            AddBlockType( "Attended Check In - Search", "Search screen for Attended Checkin", "~/Blocks/CheckIn/Attended/Search.ascx", "645D3F2F-0901-44FE-93E9-446DBC8A1680" );
            AddBlockType( "Attended Check In - Family Select", "Family select for Attended Checkin", "~/Blocks/CheckIn/Attended/FamilySelect.ascx", "4D48B5F0-F0B2-4C10-8498-DAF690761A80" );
            AddBlock( "8F618315-F554-4751-AB7F-00CC5658120A", "645D3F2F-0901-44FE-93E9-446DBC8A1680", "Attended Search", "", "Content", 0, "182C9AA0-E76F-4AAF-9F61-5418EE5A0CDB" );
            AddBlock( "8F618315-F554-4751-AB7F-00CC5658120A", "0DF27F26-691D-41F8-B0F7-987E4FEC375C", "Idle Redirect", "", "Content", 1, "2A0996EB-1E5A-4AC0-87C0-64AF682FF669" );
            AddBlock( "AF83D0B2-2995-4E46-B0DF-1A4763637A68", "4D48B5F0-F0B2-4C10-8498-DAF690761A80", "Attended Family Select", "", "Content", 0, "82929409-8551-413C-972A-98EDBC23F420" );
            AddBlock( "AF83D0B2-2995-4E46-B0DF-1A4763637A68", "0DF27F26-691D-41F8-B0F7-987E4FEC375C", "Idle Redirect", "", "Content", 1, "BDD502FF-40D2-42E6-845E-95C49C3505B3" );
            AddBlockTypeAttribute( "645D3F2F-0901-44FE-93E9-446DBC8A1680", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Welcome Page Url", "WelcomePageUrl", "Page Routes", "The url of the Check-In welcome page", 1, "~/checkin/welcome", "E0177B74-788A-4D97-BCE5-6B21EE2E7C13" );
            AddBlockTypeAttribute( "645D3F2F-0901-44FE-93E9-446DBC8A1680", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Group Select Page Url", "GroupSelectPageUrl", "Page Routes", "The url of the Check-In group select page", 7, "~/checkin/group", "D516A80F-A22B-48CE-BF3E-7FDFC51CAEE7" );
            AddBlockTypeAttribute( "645D3F2F-0901-44FE-93E9-446DBC8A1680", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Workflow Type Id", "WorkflowTypeId", "", "The Id of the workflow type to activate for check-in", 0, "0", "A1FCD0DD-81D1-4E9D-964A-8481EF77F16D" );
            AddBlockTypeAttribute( "645D3F2F-0901-44FE-93E9-446DBC8A1680", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Success Page Url", "SuccessPageUrl", "Page Routes", "The url of the Check-In success page", 9, "~/checkin/success", "9E43E8AB-A869-4F5A-97FE-68F64F1A4BC6" );
            AddBlockTypeAttribute( "645D3F2F-0901-44FE-93E9-446DBC8A1680", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Location Select Page Url", "LocationSelectPageUrl", "Page Routes", "The url of the Check-In location select page", 6, "~/checkin/location", "7A763F9A-1249-4993-B662-FFBA3E90CD68" );
            AddBlockTypeAttribute( "645D3F2F-0901-44FE-93E9-446DBC8A1680", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Time Select Page Url", "TimeSelectPageUrl", "Page Routes", "The url of the Check-In group select page", 8, "~/checkin/time", "3612F81E-0942-4B9F-AB9E-DA8E379E3D29" );
            AddBlockTypeAttribute( "645D3F2F-0901-44FE-93E9-446DBC8A1680", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Admin Page Url", "AdminPageUrl", "Page Routes", "The url of the Check-In admin page", 0, "~/checkin", "C880596F-933E-4210-AE3C-6BCAD1269FA8" );
            AddBlockTypeAttribute( "645D3F2F-0901-44FE-93E9-446DBC8A1680", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Search Page Url", "SearchPageUrl", "Page Routes", "The url of the Check-In search page", 2, "~/checkin/search", "ABDC1A06-B91A-4562-A8A8-163C53654BFC" );
            AddBlockTypeAttribute( "645D3F2F-0901-44FE-93E9-446DBC8A1680", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Family Select Page Url", "FamilySelectPageUrl", "Page Routes", "The url of the Check-In family select page", 3, "~/checkin/family", "4EDBFEF9-8013-4BB8-88E8-09B10A3F94CA" );
            AddBlockTypeAttribute( "645D3F2F-0901-44FE-93E9-446DBC8A1680", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Person Select Page Url", "PersonSelectPageUrl", "Page Routes", "The url of the Check-In person select page", 4, "~/checkin/person", "5CEDEC9E-2320-49AC-999C-E0F91B674757" );
            AddBlockTypeAttribute( "645D3F2F-0901-44FE-93E9-446DBC8A1680", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Group Type Select Page Url", "GroupTypeSelectPageUrl", "Page Routes", "The url of the Check-In group type select page", 5, "~/checkin/grouptype", "E58FA6BE-0F21-4851-B1FF-0E7660E76D9C" );
            AddBlockTypeAttribute( "E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Refresh Interval", "RefreshInterval", "", "How often (seconds) should page automatically query server for new check-in data", 0, "10", "3043FDBE-4B3D-4276-890C-CED7C560E26F" );
            AddBlockTypeAttribute( "4D48B5F0-F0B2-4C10-8498-DAF690761A80", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Welcome Page Url", "WelcomePageUrl", "Page Routes", "The url of the Check-In welcome page", 1, "~/checkin/welcome", "1799B5D4-89E5-41D7-BBC3-51BBB9F1C071" );
            AddBlockTypeAttribute( "4D48B5F0-F0B2-4C10-8498-DAF690761A80", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Group Select Page Url", "GroupSelectPageUrl", "Page Routes", "The url of the Check-In group select page", 7, "~/checkin/group", "156D1EC8-1682-4D81-A056-03E966457B90" );
            AddBlockTypeAttribute( "4D48B5F0-F0B2-4C10-8498-DAF690761A80", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Workflow Type Id", "WorkflowTypeId", "", "The Id of the workflow type to activate for check-in", 0, "0", "9889D576-58BC-49AB-A0FE-0CD141E70305" );
            AddBlockTypeAttribute( "4D48B5F0-F0B2-4C10-8498-DAF690761A80", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Success Page Url", "SuccessPageUrl", "Page Routes", "The url of the Check-In success page", 9, "~/checkin/success", "96552CC1-319D-477B-8391-48E0AF00FF10" );
            AddBlockTypeAttribute( "4D48B5F0-F0B2-4C10-8498-DAF690761A80", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Location Select Page Url", "LocationSelectPageUrl", "Page Routes", "The url of the Check-In location select page", 6, "~/checkin/location", "03A950FD-BB79-449D-A5D4-1A32E12CFB8E" );
            AddBlockTypeAttribute( "4D48B5F0-F0B2-4C10-8498-DAF690761A80", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Time Select Page Url", "TimeSelectPageUrl", "Page Routes", "The url of the Check-In group select page", 8, "~/checkin/time", "97EA2C35-9E48-4B12-9EB3-E2B5700264C6" );
            AddBlockTypeAttribute( "4D48B5F0-F0B2-4C10-8498-DAF690761A80", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Admin Page Url", "AdminPageUrl", "Page Routes", "The url of the Check-In admin page", 0, "~/checkin", "0F709573-54E2-42DF-B16A-5F1DD1CF6435" );
            AddBlockTypeAttribute( "4D48B5F0-F0B2-4C10-8498-DAF690761A80", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Search Page Url", "SearchPageUrl", "Page Routes", "The url of the Check-In search page", 2, "~/checkin/search", "27BD473A-0D7C-4D21-94D6-BCB3E212683C" );
            AddBlockTypeAttribute( "4D48B5F0-F0B2-4C10-8498-DAF690761A80", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Family Select Page Url", "FamilySelectPageUrl", "Page Routes", "The url of the Check-In family select page", 3, "~/checkin/family", "6C5CF4C5-3AED-4D6C-A5AA-9DEDCB504223" );
            AddBlockTypeAttribute( "4D48B5F0-F0B2-4C10-8498-DAF690761A80", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Person Select Page Url", "PersonSelectPageUrl", "Page Routes", "The url of the Check-In person select page", 4, "~/checkin/person", "32F86FAF-665A-40A4-A048-2D32A1AFA139" );
            AddBlockTypeAttribute( "4D48B5F0-F0B2-4C10-8498-DAF690761A80", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Group Type Select Page Url", "GroupTypeSelectPageUrl", "Page Routes", "The url of the Check-In group type select page", 5, "~/checkin/grouptype", "CD3F6DCC-E674-4600-90CD-22D4FA068B33" );
            // Attrib Value for Idle Redirect:New Location
            AddBlockAttributeValue( "2A0996EB-1E5A-4AC0-87C0-64AF682FF669", "C4204D6E-715E-4E3A-BA1B-949D20D26487", "~/attendedcheckin/search" );

            // Attrib Value for Idle Redirect:New Location
            AddBlockAttributeValue( "BDD502FF-40D2-42E6-845E-95C49C3505B3", "C4204D6E-715E-4E3A-BA1B-949D20D26487", "~/attendedcheckin/search" );

            // Attrib Value for Idle Redirect:Idle Seconds
            AddBlockAttributeValue( "BDD502FF-40D2-42E6-845E-95C49C3505B3", "A7F99980-BED4-4A80-AB83-DDAB5C7D7AAD", "30" );

            // Attrib Value for Idle Redirect:Idle Seconds
            AddBlockAttributeValue( "2A0996EB-1E5A-4AC0-87C0-64AF682FF669", "A7F99980-BED4-4A80-AB83-DDAB5C7D7AAD", "30" );

            // Attrib Value for Attended Search:Welcome Page Url
            AddBlockAttributeValue( "182C9AA0-E76F-4AAF-9F61-5418EE5A0CDB", "E0177B74-788A-4D97-BCE5-6B21EE2E7C13", "~/attendedcheckin/search" );

            // Attrib Value for Attended Search:Group Select Page Url
            AddBlockAttributeValue( "182C9AA0-E76F-4AAF-9F61-5418EE5A0CDB", "D516A80F-A22B-48CE-BF3E-7FDFC51CAEE7", "~/attendedcheckin/activity" );

            // Attrib Value for Attended Search:Workflow Type Id
            AddBlockAttributeValue( "182C9AA0-E76F-4AAF-9F61-5418EE5A0CDB", "A1FCD0DD-81D1-4E9D-964A-8481EF77F16D", "2" );

            // Attrib Value for Attended Search:Success Page Url
            AddBlockAttributeValue( "182C9AA0-E76F-4AAF-9F61-5418EE5A0CDB", "9E43E8AB-A869-4F5A-97FE-68F64F1A4BC6", "~/attendedcheckin/confirm" );

            // Attrib Value for Attended Search:Location Select Page Url
            AddBlockAttributeValue( "182C9AA0-E76F-4AAF-9F61-5418EE5A0CDB", "7A763F9A-1249-4993-B662-FFBA3E90CD68", "~/attendedcheckin/activity" );

            // Attrib Value for Attended Search:Time Select Page Url
            AddBlockAttributeValue( "182C9AA0-E76F-4AAF-9F61-5418EE5A0CDB", "3612F81E-0942-4B9F-AB9E-DA8E379E3D29", "~/attendedcheckin/activity" );

            // Attrib Value for Attended Search:Admin Page Url
            AddBlockAttributeValue( "182C9AA0-E76F-4AAF-9F61-5418EE5A0CDB", "C880596F-933E-4210-AE3C-6BCAD1269FA8", "~/attendedcheckin/admin" );

            // Attrib Value for Attended Search:Search Page Url
            AddBlockAttributeValue( "182C9AA0-E76F-4AAF-9F61-5418EE5A0CDB", "ABDC1A06-B91A-4562-A8A8-163C53654BFC", "~/attendedcheckin/search" );

            // Attrib Value for Attended Search:Family Select Page Url
            AddBlockAttributeValue( "182C9AA0-E76F-4AAF-9F61-5418EE5A0CDB", "4EDBFEF9-8013-4BB8-88E8-09B10A3F94CA", "~/attendedcheckin/family" );

            // Attrib Value for Attended Search:Person Select Page Url
            AddBlockAttributeValue( "182C9AA0-E76F-4AAF-9F61-5418EE5A0CDB", "5CEDEC9E-2320-49AC-999C-E0F91B674757", "~/attendedcheckin/person" );

            // Attrib Value for Attended Search:Group Type Select Page Url
            AddBlockAttributeValue( "182C9AA0-E76F-4AAF-9F61-5418EE5A0CDB", "E58FA6BE-0F21-4851-B1FF-0E7660E76D9C", "~/attendedcheckin/activity" );

            // Attrib Value for Attended Family Select:Welcome Page Url
            AddBlockAttributeValue( "82929409-8551-413C-972A-98EDBC23F420", "1799B5D4-89E5-41D7-BBC3-51BBB9F1C071", "~/attendedcheckin/search" );

            // Attrib Value for Attended Family Select:Group Select Page Url
            AddBlockAttributeValue( "82929409-8551-413C-972A-98EDBC23F420", "156D1EC8-1682-4D81-A056-03E966457B90", "~/attendedcheckin/activity" );

            // Attrib Value for Attended Family Select:Workflow Type Id
            AddBlockAttributeValue( "82929409-8551-413C-972A-98EDBC23F420", "9889D576-58BC-49AB-A0FE-0CD141E70305", "2" );

            // Attrib Value for Attended Family Select:Success Page Url
            AddBlockAttributeValue( "82929409-8551-413C-972A-98EDBC23F420", "96552CC1-319D-477B-8391-48E0AF00FF10", "~/attendedcheckin/confirm" );

            // Attrib Value for Attended Family Select:Location Select Page Url
            AddBlockAttributeValue( "82929409-8551-413C-972A-98EDBC23F420", "03A950FD-BB79-449D-A5D4-1A32E12CFB8E", "~/attendedcheckin/activity" );

            // Attrib Value for Attended Family Select:Time Select Page Url
            AddBlockAttributeValue( "82929409-8551-413C-972A-98EDBC23F420", "97EA2C35-9E48-4B12-9EB3-E2B5700264C6", "~/attendedcheckin/activity" );

            // Attrib Value for Attended Family Select:Admin Page Url
            AddBlockAttributeValue( "82929409-8551-413C-972A-98EDBC23F420", "0F709573-54E2-42DF-B16A-5F1DD1CF6435", "~/attendedcheckin/admin" );

            // Attrib Value for Attended Family Select:Search Page Url
            AddBlockAttributeValue( "82929409-8551-413C-972A-98EDBC23F420", "27BD473A-0D7C-4D21-94D6-BCB3E212683C", "~/attendedcheckin/search" );

            // Attrib Value for Attended Family Select:Family Select Page Url
            AddBlockAttributeValue( "82929409-8551-413C-972A-98EDBC23F420", "6C5CF4C5-3AED-4D6C-A5AA-9DEDCB504223", "~/attendedcheckin/family" );

            // Attrib Value for Attended Family Select:Person Select Page Url
            AddBlockAttributeValue( "82929409-8551-413C-972A-98EDBC23F420", "32F86FAF-665A-40A4-A048-2D32A1AFA139", "~/attendedcheckin/person" );

            // Attrib Value for Attended Family Select:Group Type Select Page Url
            AddBlockAttributeValue( "82929409-8551-413C-972A-98EDBC23F420", "CD3F6DCC-E674-4600-90CD-22D4FA068B33", "~/attendedcheckin/activity" );

            AddDefinedValue( "1EBCDB30-A89A-4C14-8580-8289EC2C7742", "Name", "Search for family based on name", "071D6DAA-3063-463A-B8A1-7D9A1BE1BB31" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteDefinedValue( "071D6DAA-3063-463A-B8A1-7D9A1BE1BB31" ); // DefinedValue search by name
                
            DeleteAttribute( "E0177B74-788A-4D97-BCE5-6B21EE2E7C13" ); // Welcome Page Url
            DeleteAttribute( "D516A80F-A22B-48CE-BF3E-7FDFC51CAEE7" ); // Group Select Page Url
            DeleteAttribute( "A1FCD0DD-81D1-4E9D-964A-8481EF77F16D" ); // Workflow Type Id
            DeleteAttribute( "9E43E8AB-A869-4F5A-97FE-68F64F1A4BC6" ); // Success Page Url
            DeleteAttribute( "7A763F9A-1249-4993-B662-FFBA3E90CD68" ); // Location Select Page Url
            DeleteAttribute( "3612F81E-0942-4B9F-AB9E-DA8E379E3D29" ); // Time Select Page Url
            DeleteAttribute( "C880596F-933E-4210-AE3C-6BCAD1269FA8" ); // Admin Page Url
            DeleteAttribute( "ABDC1A06-B91A-4562-A8A8-163C53654BFC" ); // Search Page Url
            DeleteAttribute( "4EDBFEF9-8013-4BB8-88E8-09B10A3F94CA" ); // Family Select Page Url
            DeleteAttribute( "5CEDEC9E-2320-49AC-999C-E0F91B674757" ); // Person Select Page Url
            DeleteAttribute( "E58FA6BE-0F21-4851-B1FF-0E7660E76D9C" ); // Group Type Select Page Url
            DeleteAttribute( "3043FDBE-4B3D-4276-890C-CED7C560E26F" ); // Refresh Interval
            DeleteAttribute( "63FA25AA-7796-4302-BF05-D96A1C390BD7" ); // Minimum Age
            DeleteAttribute( "D05368C9-5069-49CD-B7E8-9CE8C46BB75D" ); // Maximum Age
            DeleteAttribute( "9D2BFE8A-41F3-4A02-B3CF-9193F0C8419E" ); // Check In State
            DeleteAttribute( "1799B5D4-89E5-41D7-BBC3-51BBB9F1C071" ); // Welcome Page Url
            DeleteAttribute( "156D1EC8-1682-4D81-A056-03E966457B90" ); // Group Select Page Url
            DeleteAttribute( "9889D576-58BC-49AB-A0FE-0CD141E70305" ); // Workflow Type Id
            DeleteAttribute( "96552CC1-319D-477B-8391-48E0AF00FF10" ); // Success Page Url
            DeleteAttribute( "03A950FD-BB79-449D-A5D4-1A32E12CFB8E" ); // Location Select Page Url
            DeleteAttribute( "97EA2C35-9E48-4B12-9EB3-E2B5700264C6" ); // Time Select Page Url
            DeleteAttribute( "0F709573-54E2-42DF-B16A-5F1DD1CF6435" ); // Admin Page Url
            DeleteAttribute( "27BD473A-0D7C-4D21-94D6-BCB3E212683C" ); // Search Page Url
            DeleteAttribute( "6C5CF4C5-3AED-4D6C-A5AA-9DEDCB504223" ); // Family Select Page Url
            DeleteAttribute( "32F86FAF-665A-40A4-A048-2D32A1AFA139" ); // Person Select Page Url
            DeleteAttribute( "CD3F6DCC-E674-4600-90CD-22D4FA068B33" ); // Group Type Select Page Url
            DeleteAttribute( "185E147C-7F33-4168-BC44-EB92758D4417" ); // Order
            DeleteAttribute( "E26D90DB-4BFC-435A-9B72-78D8BDD14861" ); // Active
            DeleteAttribute( "3B484581-11AF-493D-A373-D7A005915EE0" ); // Order
            DeleteAttribute( "5C4AD928-21DD-4E79-9B14-71DECE533C9A" ); // Active
            DeleteAttribute( "6AB0E56B-DCE4-42AF-B040-8D97F5372D7D" ); // Order
            DeleteAttribute( "1F36456C-2F51-4F7B-9384-D67341D1B1A6" ); // Active
            DeleteAttribute( "13ED921C-C10C-4FB2-9240-FD038054F656" ); // Order
            DeleteAttribute( "529F4C5B-222C-48E8-ABD9-7F5DDD8D69C8" ); // Active
            DeleteAttribute( "9DB6E1D9-E4DD-4AC1-B923-912DEFFF7891" ); // Order
            DeleteAttribute( "BF60B1A0-A698-44C3-BD10-B2D55E978F54" ); // Active
            DeleteAttribute( "D67E4A1C-46E2-4A5F-9DD8-38369B3F566F" ); // Security Code Length
            DeleteAttribute( "CC7A475C-9B2D-4C9D-B345-C3F9008BF4CD" ); // Active
            DeleteAttribute( "F741C8FE-8417-4C64-B3A8-580C18DD38D2" ); // Order
            DeleteAttribute( "F7D97084-9D1E-4EA5-97FD-86F459B4A405" ); // Active
            DeleteAttribute( "628394D4-CFF1-4F68-9390-C7A4677DABCA" ); // Order
            DeleteAttribute( "D9143727-9FDE-4274-A372-27BA6902552B" ); // Active
            DeleteAttribute( "49818043-5E37-4E95-A98F-12D991BF63E9" ); // Order
            DeleteAttribute( "13641503-85E0-4A1D-B07D-E792A500D203" ); // Active
            DeleteAttribute( "0E228D41-BFB1-4368-964A-4786F26159DF" ); // Order
            DeleteAttribute( "8D4B5AB7-71E0-4B52-83AB-8DEBD11FFA3A" ); // Active
            DeleteAttribute( "C3191F56-1BC1-4422-B328-54F992244EFF" ); // Order
            DeleteAttribute( "8A2F589E-9BE7-42A6-B4F2-A66758CB8E4C" ); // Active
            DeleteAttribute( "57179E5B-E5E7-49E2-8F7A-2922567D0587" ); // Order
            DeleteAttribute( "E807D7E0-E7EE-41FC-926C-7EBD3E1C2A13" ); // Active
            DeleteAttribute( "DCA9E612-AB42-478B-90CA-CA7A3A61BD3F" ); // Order
            DeleteAttribute( "FFE511C4-FE70-4AC1-89D5-D1FF9D55DD72" ); // Active
            DeleteAttribute( "3E3DDF3C-1455-42D2-9C9D-40804A26E51E" ); // Order
            DeleteAttribute( "D3E7F620-0138-41DD-93A6-A65190D87E99" ); // Active
            DeleteAttribute( "005E24AE-4365-4D0B-A909-29EE159A8983" ); // Order
            DeleteAttribute( "75C53CEF-A3AB-42EB-BF77-AEFC29D25F9D" ); // Active
            DeleteAttribute( "E40B5CAB-CB61-45AB-B850-5B9D1AE7515E" ); // Order
            DeleteAttribute( "284C500B-F222-4FED-A9BA-EB4E66B57C4A" ); // Active
            DeleteAttribute( "2FB3A787-4764-49EF-8E79-0D617E8EC934" ); // Order
            DeleteAttribute( "9E5CDB2B-ED76-4B63-ABDA-F159DDF6C98C" ); // Active
            DeleteAttribute( "5E43BF1C-A908-4B5E-AB63-8D373FDD7C0C" ); // Order
            DeleteAttribute( "4ED9A90A-19FA-4153-AD3A-496BDC383B22" ); // Active
            DeleteAttribute( "27B1327E-3918-4876-BC7B-071690D9AE8A" ); // Activity Type
            DeleteAttribute( "A6F8E96B-E581-4467-A32A-1CFD921E04A2" ); // Active
            DeleteAttribute( "908ACAE3-D315-492F-BE28-E40E0579BC99" ); // Recipient
            DeleteAttribute( "D8BD67B9-F063-47A4-A425-01B784D7A25C" ); // EmailTemplate
            DeleteAttribute( "DB9CE4F6-F205-42FB-9ACB-1522B3473E32" ); // Order
            DeleteAttribute( "FDA75A9D-C757-4068-812B-5BBEF02CCE95" ); // Active
            DeleteAttribute( "13F28FA5-B5B2-456D-9EB0-F09084DDCC1F" ); // Status
            DeleteAttribute( "64510635-C346-43C0-AD97-38A9762E6584" ); // Order
            DeleteAttribute( "624E445B-91EF-4B93-AFB9-F7B63904164E" ); // Active
            DeleteBlock( "182C9AA0-E76F-4AAF-9F61-5418EE5A0CDB" ); // Attended Search
            DeleteBlock( "2A0996EB-1E5A-4AC0-87C0-64AF682FF669" ); // Idle Redirect
            DeleteBlock( "82929409-8551-413C-972A-98EDBC23F420" ); // Attended Family Select
            DeleteBlock( "BDD502FF-40D2-42E6-845E-95C49C3505B3" ); // Idle Redirect
            DeleteBlockType( "645D3F2F-0901-44FE-93E9-446DBC8A1680" ); // Attended Check In - Search
            DeleteBlockType( "4D48B5F0-F0B2-4C10-8498-DAF690761A80" ); // Attended Check In - Family Select
            DeletePage( "32A132A6-63A2-4840-B4A5-23D80994CCBD" ); // Attended Checkin
            DeletePage( "771E3CF1-63BD-4880-BC43-AC29B4CCE963" ); // Admin
            DeletePage( "8F618315-F554-4751-AB7F-00CC5658120A" ); // Search
            DeletePage( "AF83D0B2-2995-4E46-B0DF-1A4763637A68" ); // Family Select
            DeletePage( "C87916FE-417E-4A11-8831-5CFA7678A228" ); // Activity Select
            DeletePage( "BE996C9B-3DFE-407F-BD53-D6F58D85A035" ); // Confirmation
        }
    }
}
