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
            AddPage( "20F97A93-7949-4C2A-8A5E-C756FE8585CA", "Attended Checkin", "Screens for managing Attended Checkin", "32A132A6-63A2-4840-B4A5-23D80994CCBD" );
            Sql( " UPDATE [Page] SET [ParentPageId] = NULL WHERE [Guid] = '32A132A6-63A2-4840-B4A5-23D80994CCBD' " );
            AddPage( "32A132A6-63A2-4840-B4A5-23D80994CCBD", "Admin", "Admin screen for Attended Checkin", "Checkin", "771E3CF1-63BD-4880-BC43-AC29B4CCE963" );
            AddPage( "32A132A6-63A2-4840-B4A5-23D80994CCBD", "Search", "Search screen for Attended Checkin", "Checkin", "8F618315-F554-4751-AB7F-00CC5658120A" );
            AddPage( "32A132A6-63A2-4840-B4A5-23D80994CCBD", "Family Select", "Family select for Attended Checkin", "Checkin", "AF83D0B2-2995-4E46-B0DF-1A4763637A68" );
            AddPage( "32A132A6-63A2-4840-B4A5-23D80994CCBD", "Activity Select", "Activity select for Attended Checkin", "Checkin", "C87916FE-417E-4A11-8831-5CFA7678A228" );
            AddPage( "32A132A6-63A2-4840-B4A5-23D80994CCBD", "Confirmation", "Confirmation screen for Attended Checkin", "Checkin", "BE996C9B-3DFE-407F-BD53-D6F58D85A035" );

            AddBlockType( "Attended Check In - Admin", "Admin screen for Attended Checkin", "~/Blocks/CheckIn/Attended/Admin.ascx", "2C51230E-BA2E-4646-BB10-817B26C16218" );
            AddBlockType( "Attended Check In - Search", "Search screen for Attended Checkin", "~/Blocks/CheckIn/Attended/Search.ascx", "645D3F2F-0901-44FE-93E9-446DBC8A1680" );
            AddBlockType( "Attended Check In - Family Select", "Family select for Attended Checkin", "~/Blocks/CheckIn/Attended/FamilySelect.ascx", "4D48B5F0-F0B2-4C10-8498-DAF690761A80" );

            AddBlock( "771E3CF1-63BD-4880-BC43-AC29B4CCE963", "2C51230E-BA2E-4646-BB10-817B26C16218", "Attended Search", "", "Content", 0, "9F8731AB-07DB-406F-A344-45E31D0DE301" );
            AddBlock( "771E3CF1-63BD-4880-BC43-AC29B4CCE963", "0DF27F26-691D-41F8-B0F7-987E4FEC375C", "Idle Redirect", "", "Content", 1, "C3167514-93FA-45EF-BA8B-0E9EFB6C575C" );

            AddBlock( "8F618315-F554-4751-AB7F-00CC5658120A", "645D3F2F-0901-44FE-93E9-446DBC8A1680", "Attended Search", "", "Content", 0, "182C9AA0-E76F-4AAF-9F61-5418EE5A0CDB" );
            AddBlock( "8F618315-F554-4751-AB7F-00CC5658120A", "0DF27F26-691D-41F8-B0F7-987E4FEC375C", "Idle Redirect", "", "Content", 1, "2A0996EB-1E5A-4AC0-87C0-64AF682FF669" );

            AddBlock( "AF83D0B2-2995-4E46-B0DF-1A4763637A68", "4D48B5F0-F0B2-4C10-8498-DAF690761A80", "Attended Family Select", "", "Content", 0, "82929409-8551-413C-972A-98EDBC23F420" );
            AddBlock( "AF83D0B2-2995-4E46-B0DF-1A4763637A68", "0DF27F26-691D-41F8-B0F7-987E4FEC375C", "Idle Redirect", "", "Content", 1, "BDD502FF-40D2-42E6-845E-95C49C3505B3" );

            // add new block type attributes
            AddBlockTypeAttribute( "2C51230E-BA2E-4646-BB10-817B26C16218", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Admin Page Url", "AdminPageUrl", "Page Routes", "The url of the Check-In admin page", 0, "~/attendedcheckin/admin", "E78AD186-8100-4275-81E1-42542F7423BE" );
            AddBlockTypeAttribute( "2C51230E-BA2E-4646-BB10-817B26C16218", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Welcome Page Url", "WelcomePageUrl", "Page Routes", "The url of the Check-In welcome page", 1, "~/attendedcheckin/search", "402113AE-A293-4E9C-8AA3-AD8AA084EDA1" );
            AddBlockTypeAttribute( "2C51230E-BA2E-4646-BB10-817B26C16218", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Search Page Url", "SearchPageUrl", "Page Routes", "The url of the Check-In search page", 2, "~/attendedcheckin/search", "EB7DFEAF-7C02-4F12-BA26-F7F4B3FD3094" );
            AddBlockTypeAttribute( "2C51230E-BA2E-4646-BB10-817B26C16218", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Family Select Page Url", "FamilySelectPageUrl", "Page Routes", "The url of the Check-In family select page", 3, "~/attendedcheckin/family", "43BD9007-1808-4886-B869-B26762CF7C80" );
            AddBlockTypeAttribute( "2C51230E-BA2E-4646-BB10-817B26C16218", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Person Select Page Url", "PersonSelectPageUrl", "Page Routes", "The url of the Check-In person select page", 4, "~/attendedcheckin/family", "6EBF491B-2C83-4154-AF6B-04AC04E8245A" );
            AddBlockTypeAttribute( "2C51230E-BA2E-4646-BB10-817B26C16218", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Group Type Select Page Url", "GroupTypeSelectPageUrl", "Page Routes", "The url of the Check-In group type select page", 5, "~/attendedcheckin/family", "0853E29D-A107-47D2-BED3-4408CED60353" );
            AddBlockTypeAttribute( "2C51230E-BA2E-4646-BB10-817B26C16218", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Location Select Page Url", "LocationSelectPageUrl", "Page Routes", "The url of the Check-In location select page", 6, "~/attendedcheckin/activity", "9C56761D-3BB6-42A2-94F3-F0D204B6538C" );
            AddBlockTypeAttribute( "2C51230E-BA2E-4646-BB10-817B26C16218", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Group Select Page Url", "GroupSelectPageUrl", "Page Routes", "The url of the Check-In group select page", 7, "~/attendedcheckin/activity", "1695E760-D395-4CFD-A83B-12F28C773BE1" );
            AddBlockTypeAttribute( "2C51230E-BA2E-4646-BB10-817B26C16218", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Time Select Page Url", "TimeSelectPageUrl", "Page Routes", "The url of the Check-In group select page", 8, "~/attendedcheckin/activity", "B7FAC026-2F8D-4C70-9AE3-7E4206621CFF" );
            AddBlockTypeAttribute( "2C51230E-BA2E-4646-BB10-817B26C16218", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Success Page Url", "SuccessPageUrl", "Page Routes", "The url of the Check-In success page", 9, "~/attendedcheckin/confirm", "CC63BC77-69A3-4FA4-9C8F-61F4136C1955" );
            AddBlockTypeAttribute( "2C51230E-BA2E-4646-BB10-817B26C16218", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Workflow Type Id", "WorkflowTypeId", "", "The Id of the workflow type to activate for check-in", 0, "3", "1159207C-EA8B-4066-A044-233C6446412D" );
            

            AddBlockTypeAttribute( "645D3F2F-0901-44FE-93E9-446DBC8A1680", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Admin Page Url", "AdminPageUrl", "Page Routes", "The url of the Check-In admin page", 0, "~/attendedcheckin", "C880596F-933E-4210-AE3C-6BCAD1269FA8" ); 
            AddBlockTypeAttribute( "645D3F2F-0901-44FE-93E9-446DBC8A1680", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Welcome Page Url", "WelcomePageUrl", "Page Routes", "The url of the Check-In welcome page", 1, "~/attendedcheckin/search", "E0177B74-788A-4D97-BCE5-6B21EE2E7C13" );
            AddBlockTypeAttribute( "645D3F2F-0901-44FE-93E9-446DBC8A1680", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Search Page Url", "SearchPageUrl", "Page Routes", "The url of the Check-In search page", 2, "~/attendedcheckin/search", "ABDC1A06-B91A-4562-A8A8-163C53654BFC" );
            AddBlockTypeAttribute( "645D3F2F-0901-44FE-93E9-446DBC8A1680", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Family Select Page Url", "FamilySelectPageUrl", "Page Routes", "The url of the Check-In family select page", 3, "~/attendedcheckin/family", "4EDBFEF9-8013-4BB8-88E8-09B10A3F94CA" );
            AddBlockTypeAttribute( "645D3F2F-0901-44FE-93E9-446DBC8A1680", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Person Select Page Url", "PersonSelectPageUrl", "Page Routes", "The url of the Check-In person select page", 4, "~/attendedcheckin/family", "5CEDEC9E-2320-49AC-999C-E0F91B674757" );
            AddBlockTypeAttribute( "645D3F2F-0901-44FE-93E9-446DBC8A1680", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Group Type Select Page Url", "GroupTypeSelectPageUrl", "Page Routes", "The url of the Check-In group type select page", 5, "~/attendedcheckin/family", "E58FA6BE-0F21-4851-B1FF-0E7660E76D9C" );            
            AddBlockTypeAttribute( "645D3F2F-0901-44FE-93E9-446DBC8A1680", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Location Select Page Url", "LocationSelectPageUrl", "Page Routes", "The url of the Check-In location select page", 6, "~/attendedcheckin/activity", "7A763F9A-1249-4993-B662-FFBA3E90CD68" );
            AddBlockTypeAttribute( "645D3F2F-0901-44FE-93E9-446DBC8A1680", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Group Select Page Url", "GroupSelectPageUrl", "Page Routes", "The url of the Check-In group select page", 7, "~/attendedcheckin/activity", "D516A80F-A22B-48CE-BF3E-7FDFC51CAEE7" );
            AddBlockTypeAttribute( "645D3F2F-0901-44FE-93E9-446DBC8A1680", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Time Select Page Url", "TimeSelectPageUrl", "Page Routes", "The url of the Check-In group select page", 8, "~/attendedcheckin/activity", "3612F81E-0942-4B9F-AB9E-DA8E379E3D29" );
            AddBlockTypeAttribute( "645D3F2F-0901-44FE-93E9-446DBC8A1680", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Success Page Url", "SuccessPageUrl", "Page Routes", "The url of the Check-In success page", 9, "~/attendedcheckin/confirm", "9E43E8AB-A869-4F5A-97FE-68F64F1A4BC6" );
            AddBlockTypeAttribute( "645D3F2F-0901-44FE-93E9-446DBC8A1680", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Workflow Type Id", "WorkflowTypeId", "", "The Id of the workflow type to activate for check-in", 0, "3", "A1FCD0DD-81D1-4E9D-964A-8481EF77F16D" );
            

            AddBlockTypeAttribute( "4D48B5F0-F0B2-4C10-8498-DAF690761A80", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Admin Page Url", "AdminPageUrl", "Page Routes", "The url of the Check-In admin page", 0, "~/attendedcheckin", "0F709573-54E2-42DF-B16A-5F1DD1CF6435" );            
            AddBlockTypeAttribute( "4D48B5F0-F0B2-4C10-8498-DAF690761A80", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Welcome Page Url", "WelcomePageUrl", "Page Routes", "The url of the Check-In welcome page", 1, "~/attendedcheckin/search", "1799B5D4-89E5-41D7-BBC3-51BBB9F1C071" );
            AddBlockTypeAttribute( "4D48B5F0-F0B2-4C10-8498-DAF690761A80", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Search Page Url", "SearchPageUrl", "Page Routes", "The url of the Check-In search page", 2, "~/attendedcheckin/search", "27BD473A-0D7C-4D21-94D6-BCB3E212683C" );
            AddBlockTypeAttribute( "4D48B5F0-F0B2-4C10-8498-DAF690761A80", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Family Select Page Url", "FamilySelectPageUrl", "Page Routes", "The url of the Check-In family select page", 3, "~/attendedcheckin/family", "6C5CF4C5-3AED-4D6C-A5AA-9DEDCB504223" );
            AddBlockTypeAttribute( "4D48B5F0-F0B2-4C10-8498-DAF690761A80", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Person Select Page Url", "PersonSelectPageUrl", "Page Routes", "The url of the Check-In person select page", 4, "~/attendedcheckin/family", "32F86FAF-665A-40A4-A048-2D32A1AFA139" );
            AddBlockTypeAttribute( "4D48B5F0-F0B2-4C10-8498-DAF690761A80", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Group Type Select Page Url", "GroupTypeSelectPageUrl", "Page Routes", "The url of the Check-In group type select page", 5, "~/attendedcheckin/family", "CD3F6DCC-E674-4600-90CD-22D4FA068B33" );
            AddBlockTypeAttribute( "4D48B5F0-F0B2-4C10-8498-DAF690761A80", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Location Select Page Url", "LocationSelectPageUrl", "Page Routes", "The url of the Check-In location select page", 6, "~/attendedcheckin/activity", "03A950FD-BB79-449D-A5D4-1A32E12CFB8E" );            
            AddBlockTypeAttribute( "4D48B5F0-F0B2-4C10-8498-DAF690761A80", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Group Select Page Url", "GroupSelectPageUrl", "Page Routes", "The url of the Check-In group select page", 7, "~/attendedcheckin/activity", "156D1EC8-1682-4D81-A056-03E966457B90" );
            AddBlockTypeAttribute( "4D48B5F0-F0B2-4C10-8498-DAF690761A80", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Time Select Page Url", "TimeSelectPageUrl", "Page Routes", "The url of the Check-In group select page", 8, "~/attendedcheckin/activity", "97EA2C35-9E48-4B12-9EB3-E2B5700264C6" );
            AddBlockTypeAttribute( "4D48B5F0-F0B2-4C10-8498-DAF690761A80", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Success Page Url", "SuccessPageUrl", "Page Routes", "The url of the Check-In success page", 9, "~/attendedcheckin/confirm", "96552CC1-319D-477B-8391-48E0AF00FF10" );
            AddBlockTypeAttribute( "4D48B5F0-F0B2-4C10-8498-DAF690761A80", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Workflow Type Id", "WorkflowTypeId", "", "The Id of the workflow type to activate for check-in", 0, "3", "9889D576-58BC-49AB-A0FE-0CD141E70305" );
            
            AddBlockTypeAttribute( "E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Refresh Interval", "RefreshInterval", "", "How often (seconds) should page automatically query server for new check-in data", 0, "10", "3043FDBE-4B3D-4276-890C-CED7C560E26F" );

            // Page Routes for Attended Checkin
            AddPageRoute( "771E3CF1-63BD-4880-BC43-AC29B4CCE963", "attendedcheckin" );
            AddPageRoute( "771E3CF1-63BD-4880-BC43-AC29B4CCE963", "attendedcheckin/admin" );
            AddPageRoute( "8F618315-F554-4751-AB7F-00CC5658120A", "attendedcheckin/search" );
            AddPageRoute( "AF83D0B2-2995-4E46-B0DF-1A4763637A68", "attendedcheckin/family" );
            AddPageRoute( "C87916FE-417E-4A11-8831-5CFA7678A228", "attendedcheckin/activity" );
            AddPageRoute( "BE996C9B-3DFE-407F-BD53-D6F58D85A035", "attendedcheckin/confirm" );

            AddDefinedValue( "1EBCDB30-A89A-4C14-8580-8289EC2C7742", "Name", "Search for family based on name", "071D6DAA-3063-463A-B8A1-7D9A1BE1BB31" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteDefinedValue( "071D6DAA-3063-463A-B8A1-7D9A1BE1BB31" ); // DefinedValue search by name

            DeleteAttribute( "9B15F727-FAEA-4226-B2A7-339848D300A9" ); // Order
            DeleteAttribute( "79148A67-14FF-4E1F-AE92-03327D753BA8" ); // Active
            DeleteAttribute( "2D6F50D6-B67D-479E-B659-08B890A3B0C9" ); // Order
            DeleteAttribute( "B9848D7E-7995-47ED-80AB-C769DA68A5C5" ); // Active
            DeleteAttribute( "ACEE10EE-BAEA-4659-8991-29ADC539D60C" ); // Order
            DeleteAttribute( "79DF17E4-28BF-4D4D-BAFF-3E97E601D5FE" ); // Active
            DeleteAttribute( "DD9C2E51-0CB4-43C5-9E93-2722BFA9BBFD" ); // Order
            DeleteAttribute( "283F3C9C-3BBE-4D8C-B167-13E634D7E4B2" ); // Active
            DeleteAttribute( "90E48C3D-382A-45AB-A852-8E916F693575" ); // Order
            DeleteAttribute( "BB627950-DE5A-4A10-83A5-4B468F83832E" ); // Active
            DeleteAttribute( "313651AF-913C-4796-AD59-0EE78C9F3267" ); // Security Code Length
            DeleteAttribute( "9A7AFBC8-F009-437B-A437-ED9F3686EBA8" ); // Active
            DeleteAttribute( "A8C425B7-BBE0-4101-8BA8-C89E970190F6" ); // Order
            DeleteAttribute( "E49A02FA-7681-4D7C-823D-8B74869FD1DA" ); // Active
            DeleteAttribute( "2AB24831-D0BF-486A-8953-BCA655B454D6" ); // Order
            DeleteAttribute( "05406657-6012-4174-9C64-B87C74944C4F" ); // Active
            DeleteAttribute( "1F3341B4-D736-4796-BC8D-866855D219BC" ); // Order
            DeleteAttribute( "1D404017-075D-47A2-979A-5331A2051AC6" ); // Active
            DeleteAttribute( "CCB0455E-6B6F-47D5-94A0-67CAD44291B1" ); // Order
            DeleteAttribute( "D3B3DE47-C2F5-4A16-AB5A-456A5358E6DC" ); // Active
            DeleteAttribute( "53D02B00-B117-434B-8DCC-A13CF28C5BF3" ); // Order
            DeleteAttribute( "72A7792C-4895-4C2D-BAE9-AB4C597E8C73" ); // Active
            DeleteAttribute( "D92C65BE-C9B8-41C1-9C25-5CAE1CC2FFF1" ); // Order
            DeleteAttribute( "729BFD30-6F70-46AD-8C5E-EBFD4A36244A" ); // Active
            DeleteAttribute( "784EB3EA-63FB-4CE6-8E3C-32EEB7EAB671" ); // Order
            DeleteAttribute( "D4EE6306-5550-4B3F-8925-603406716A1F" ); // Active
            DeleteAttribute( "97C86416-FD18-4B85-BE9C-62ED73504D4E" ); // Order
            DeleteAttribute( "D9F5C3CC-5BAB-4222-9440-645DA1189A86" ); // Active
            DeleteAttribute( "1BB10189-9CAC-416C-B970-A396D078026C" ); // Order
            DeleteAttribute( "4DF96B5A-ABC5-4895-B74E-E87F24AED9CC" ); // Active
            DeleteAttribute( "5640AEB4-9232-4249-8F88-A4DF420CAFDA" ); // Order
            DeleteAttribute( "E7A57275-0CBD-4ABD-8F32-FC5478BFA986" ); // Active
            DeleteAttribute( "54694DCA-68A3-4ABD-B433-5EAD87BC3DD8" ); // Order
            DeleteAttribute( "272AEAEF-5244-47EA-B1AE-0A0AA7D00D87" ); // Active
            DeleteAttribute( "A82D0747-DE6C-46AF-B98E-3311BFEBDFB0" ); // Order
            DeleteAttribute( "E2B843BF-4E92-478C-851C-A112C8A80D65" ); // Active
            DeleteAttribute( "80CE4879-A346-4E8A-9A27-19FB42C64285" ); // Activity Type
            DeleteAttribute( "B21D1EB9-45BD-437D-B8C0-D105AD104DBB" ); // Active
            DeleteAttribute( "8494BF4D-7E68-45BE-82CE-5385ED7E2DDB" ); // Recipient
            DeleteAttribute( "4DF5BA0C-5296-4958-AA2E-770276AC1396" ); // EmailTemplate
            DeleteAttribute( "8FBC9415-115F-497B-AF46-8568EE47EC69" ); // Order
            DeleteAttribute( "26546BF5-D930-4FCD-88AD-D110330694E1" ); // Active
            DeleteAttribute( "C63DAA5B-A4FA-4EC8-B76E-996EF00D14AB" ); // Status
            DeleteAttribute( "9F10EBBD-4F3B-44A5-B3AF-097C520E1D44" ); // Order
            DeleteAttribute( "06A8CF9D-3A76-43C0-B7F3-45EFACDB2B42" ); // Active
            DeleteAttribute( "1ED50F74-D061-47DE-A1B6-13D7B9F37BB8" ); // Check In State
            DeleteAttribute( "47159C4B-5105-4971-BDDC-AFA9FE7F1074" ); // Welcome Page Url
            DeleteAttribute( "7EB400CF-42C1-4069-BDE1-4E0415FE99F9" ); // Group Select Page Url
            DeleteAttribute( "B6CC88EE-6AF9-4936-941C-136A326EF09B" ); // Workflow Type Id
            DeleteAttribute( "C3A1656E-B689-446A-B0E0-7B5EE03203B8" ); // Success Page Url
            DeleteAttribute( "CE3DBDDA-4A44-4136-BBC3-BE08144A0719" ); // Location Select Page Url
            DeleteAttribute( "CF3EF7D2-1CBB-40B4-BC3D-7FDD7119212F" ); // Time Select Page Url
            DeleteAttribute( "E8A08BC8-C0CA-43A1-9217-A445295099CF" ); // Admin Page Url
            DeleteAttribute( "98216B21-9684-4565-9AF1-72434F99F86D" ); // Search Page Url
            DeleteAttribute( "C662A17F-4647-48CB-8478-2028F488F7AA" ); // Family Select Page Url
            DeleteAttribute( "27DB2614-0478-4FBA-AF71-E7FFF6D1E85D" ); // Person Select Page Url
            DeleteAttribute( "31B26CC5-79D4-4284-BF0E-79E07CE141B6" ); // Group Type Select Page Url
            DeleteAttribute( "402113AE-A293-4E9C-8AA3-AD8AA084EDA1" ); // Welcome Page Url
            DeleteAttribute( "1695E760-D395-4CFD-A83B-12F28C773BE1" ); // Group Select Page Url
            DeleteAttribute( "1159207C-EA8B-4066-A044-233C6446412D" ); // Workflow Type Id
            DeleteAttribute( "CC63BC77-69A3-4FA4-9C8F-61F4136C1955" ); // Success Page Url
            DeleteAttribute( "9C56761D-3BB6-42A2-94F3-F0D204B6538C" ); // Location Select Page Url
            DeleteAttribute( "B7FAC026-2F8D-4C70-9AE3-7E4206621CFF" ); // Time Select Page Url
            DeleteAttribute( "E78AD186-8100-4275-81E1-42542F7423BE" ); // Admin Page Url
            DeleteAttribute( "EB7DFEAF-7C02-4F12-BA26-F7F4B3FD3094" ); // Search Page Url
            DeleteAttribute( "43BD9007-1808-4886-B869-B26762CF7C80" ); // Family Select Page Url
            DeleteAttribute( "6EBF491B-2C83-4154-AF6B-04AC04E8245A" ); // Person Select Page Url
            DeleteAttribute( "0853E29D-A107-47D2-BED3-4408CED60353" ); // Group Type Select Page Url


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
            DeleteBlock( "9F8731AB-07DB-406F-A344-45E31D0DE301" ); // Attended Admin
            DeleteBlock( "C3167514-93FA-45EF-BA8B-0E9EFB6C575C" ); // Idle Redirect
            DeleteBlock( "182C9AA0-E76F-4AAF-9F61-5418EE5A0CDB" ); // Attended Search
            DeleteBlock( "2A0996EB-1E5A-4AC0-87C0-64AF682FF669" ); // Idle Redirect
            DeleteBlock( "82929409-8551-413C-972A-98EDBC23F420" ); // Attended Family Select
            DeleteBlock( "BDD502FF-40D2-42E6-845E-95C49C3505B3" ); // Idle Redirect
            DeleteBlockType( "2C51230E-BA2E-4646-BB10-817B26C16218" ); // Attended Check In - Admin
            DeleteBlockType( "645D3F2F-0901-44FE-93E9-446DBC8A1680" ); // Attended Check In - Search
            DeleteBlockType( "4D48B5F0-F0B2-4C10-8498-DAF690761A80" ); // Attended Check In - Family Select
            DeletePage( "771E3CF1-63BD-4880-BC43-AC29B4CCE963" ); // Admin
            DeletePage( "8F618315-F554-4751-AB7F-00CC5658120A" ); // Search
            DeletePage( "AF83D0B2-2995-4E46-B0DF-1A4763637A68" ); // Family Select
            DeletePage( "C87916FE-417E-4A11-8831-5CFA7678A228" ); // Activity Select
            DeletePage( "BE996C9B-3DFE-407F-BD53-D6F58D85A035" ); // Confirmation
            DeletePage( "32A132A6-63A2-4840-B4A5-23D80994CCBD" ); // Attended Checkin
        }
    }
}
