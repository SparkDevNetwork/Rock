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
using System;

namespace Rock.SystemGuid
{
    /// <summary>
    /// System Pages.  NOTE: Some of these are referenced in Migrations to avoid string-typos.
    /// </summary>
    public static class Page
    {

        /// <summary>
        /// Gets the Ability Levels page guid
        /// ParentPage: Check-in
        /// </summary>
        public const string ABILITY_LEVELS = "9DD78A23-BE4B-474E-BCBC-F06AAABB67FA";

        /// <summary>
        /// Gets the Ability Select page guid
        /// ParentPage: Check-in
        /// </summary>
        public const string ABILITY_SELECT = "A1CBDAA4-94DD-4156-8260-5A3781E39FD0";

        /// <summary>
        /// Gets the Account page guid
        /// ParentPage: Rock Shop
        /// </summary>
        public const string ACCOUNT = "DADE879D-8DF5-4367-89EF-FEECD12B81AB";

        /// <summary>
        /// Gets the Account Detail page guid
        /// ParentPage: Accounts
        /// </summary>
        public const string ACCOUNT_DETAIL = "75130E27-405A-4935-AB27-0EDE11F6E8B3";

        /// <summary>
        /// Gets the Accounts page guid
        /// ParentPage: Administration
        /// </summary>
        public const string ACCOUNTS = "2B630A3B-E081-4204-A3E4-17BB3A5F063D";

        /// <summary>
        /// Gets the Add Transaction page guid
        /// ParentPage: Contributions
        /// </summary>
        public const string ADD_TRANSACTION = "B1CA86DC-9890-4D26-8EBD-488044E1B3DD";

        /// <summary>
        /// Gets the Admin page guid
        /// ParentPage: Check-in
        /// </summary>
        public const string ADMIN_CHECK_IN = "7B7207D0-B905-4836-800E-A24DDC6FE445";

        /// <summary>
        /// Gets the Admin Tools page guid
        /// ParentPage: Internal Homepage
        /// </summary>
        public const string ADMIN_TOOLS = "84E12152-E456-478E-AF68-BA640E9CE65B";

        /// <summary>
        /// Gets the Administration page guid
        /// ParentPage: Finance
        /// </summary>
        public const string ADMINISTRATION_FINANCE = "18C9E5C3-3E28-4AA3-84F6-78CD4EA2DD3C";

        /// <summary>
        /// Gets the Application Group Detail page guid
        /// ParentPage: Application Groups
        /// </summary>
        public const string APPLICATION_GROUP_DETAIL = "E9737442-E6A9-47D5-A842-11C1AE1CF43F";

        /// <summary>
        /// Gets the Application Groups page guid
        /// ParentPage: System Settings
        /// </summary>
        public const string APPLICATION_GROUPS = "BA078BB8-7205-46F4-9530-B2FB9EAD3E57";

        /// <summary>
        /// Gets the Attendance page guid
        /// ParentPage: Group Attendance
        /// </summary>
        public const string ATTENDANCE = "D2A75147-B031-4DF7-8E04-FDDEAE2575F1";

        /// <summary>
        /// Gets the Attendance Analysis page guid
        /// ParentPage: Reporting
        /// </summary>
        public const string ATTENDANCE_ANALYSIS = "7A3CF259-1090-403C-83B7-2DB3A53DEE26";

        /// <summary>
        /// Gets the Attribute Categories page guid
        /// ParentPage: General Settings
        /// </summary>
        public const string ATTRIBUTE_CATEGORIES = "220D72F5-B589-4378-9852-BBB6F145AD7F";

        /// <summary>
        /// Gets the Audit Information page guid
        /// ParentPage: Security
        /// </summary>
        public const string AUDIT_INFORMATION = "4D7F3953-0BD9-4B4B-83F9-5FCC6B2BBE30";

        /// <summary>
        /// Gets the Authentication Services page guid
        /// ParentPage: Security
        /// </summary>
        public const string AUTHENTICATION_SERVICES = "CE2170A9-2C8E-40B1-A42E-DFA73762D01D";

        /// <summary>
        /// Gets the Background Check Providers page guid
        /// ParentPage: System Settings
        /// </summary>
        public const string BACKGROUND_CHECK_PROVIDERS = "53F1B7D9-806A-4541-93BC-4CCF5DFF90B3";

        /// <summary>
        /// Gets the Batches page guid
        /// ParentPage: Functions
        /// </summary>
        public const string BATCHES = "EF65EFF2-99AC-4081-8E09-32A04518683A";

        /// <summary>
        /// Gets the Benevolence page guid
        /// ParentPage: Functions
        /// </summary>
        public const string BENEVOLENCE_FUNCTIONS = "D893CCCC-368A-42CF-B36E-69991128F016";

        /// <summary>
        /// Gets the Benevolence page guid
        /// ParentPage: Person Pages
        /// </summary>
        public const string BENEVOLENCE_PERSON_PAGES = "15FA4176-1C8E-409D-8B47-85ADA35DE5D2";

        /// <summary>
        /// Gets the Benevolence Detail page guid
        /// ParentPage: Benevolence
        /// </summary>
        public const string BENEVOLENCE_DETAIL = "6DC7BAED-CA01-4703-B679-EC81143CDEDD";

        /// <summary>
        /// Gets the Benevolence Request Detail page guid
        /// ParentPage: Benevolence
        /// </summary>
        public const string BENEVOLENCE_REQUEST_DETAIL = "648CA58C-EB12-4479-9994-F064070E3A32";

        /// <summary>
        /// Gets the Block Properties page guid
        /// ParentPage: System Dialogs
        /// </summary>
        public const string BLOCK_PROPERTIES = "F0B34893-9550-4864-ADB4-EE860E4E427C";

        /// <summary>
        /// Gets the Block Type Detail page guid
        /// ParentPage: Block Types
        /// </summary>
        public const string BLOCK_TYPE_DETAIL = "C694AD7C-46DD-47FE-B2AC-1CF158FA6504";

        /// <summary>
        /// Gets the Block Types page guid
        /// ParentPage: CMS Configuration
        /// </summary>
        public const string BLOCK_TYPES = "5FBE9019-862A-41C6-ACDC-287D7934757D";

        /// <summary>
        /// Gets the Blog page guid
        /// ParentPage: External Homepage
        /// </summary>
        public const string BLOG = "4857A6C9-F194-4B64-A9FB-6A8DC7A1A671";

        /// <summary>
        /// Gets the Blog Details page guid
        /// ParentPage: Blog
        /// </summary>
        public const string BLOG_DETAILS = "2D0D0FB0-68C4-47E1-8BC6-98F931497F5E";

        /// <summary>
        /// Gets the Bulk Update page guid
        /// ParentPage: Support Pages
        /// </summary>
        public const string BULK_UPDATE = "B6BFDE54-0EFA-4499-847D-BE1259F83535";

        /// <summary>
        /// Gets the Business Detail page guid
        /// ParentPage: Businesses
        /// </summary>
        public const string BUSINESS_DETAIL = "D2B43273-C64F-4F57-9AAE-9571E1982BAC";

        /// <summary>
        /// Gets the Businesses page guid
        /// ParentPage: Administration
        /// </summary>
        public const string BUSINESSES = "F4DF4899-2D44-4997-BA9B-9D2C64958A20";

        /// <summary>
        /// Gets the Campus Detail page guid
        /// ParentPage: Campuses
        /// </summary>
        public const string CAMPUS_DETAIL = "BDD7B906-4D42-43C0-8DBB-B89A566734D8";

        /// <summary>
        /// Gets the Campuses page guid
        /// ParentPage: General Settings
        /// </summary>
        public const string CAMPUSES = "5EE91A54-C750-48DC-9392-F1F0F0581C3A";

        /// <summary>
        /// Gets the Change Password page guid
        /// ParentPage: My Settings
        /// </summary>
        public const string CHANGE_PASSWORD_MY_SETTINGS = "4508223C-2989-4592-B764-B3F372B6051B";

        /// <summary>
        /// Gets the Check-in page guid
        /// ParentPage: 
        /// </summary>
        public const string CHECK_IN_ROOT = "CDF2C599-D341-42FD-B7DC-CD402EA96050";

        /// <summary>
        /// Gets the Check-in page guid
        /// ParentPage: Rock Settings
        /// </summary>
        public const string CHECK_IN_ROCK_SETTINGS = "66C5DD58-094C-4FF9-9AFB-44801FCFCC2D";

        /// <summary>
        /// Gets the Check-in Configuration page guid
        /// ParentPage: Check-in
        /// </summary>
        public const string CHECK_IN_CONFIGURATION = "C646A95A-D12D-4A67-9BE6-C9695C0267ED";

        /// <summary>
        /// Gets the Check-in Label page guid
        /// ParentPage: Check-in Labels
        /// </summary>
        public const string CHECK_IN_LABEL = "B565FDF8-959F-4AC8-ACDF-3B1B5CFE79F5";

        /// <summary>
        /// Gets the Check-in Labels page guid
        /// ParentPage: Check-in
        /// </summary>
        public const string CHECK_IN_LABELS = "7C093A63-F2AC-4FE3-A826-8BF06D204EA2";

        /// <summary>
        /// Gets the Check-in Manager page guid
        /// ParentPage: Check-in Type
        /// </summary>
        public const string CHECK_IN_MANAGER = "A4DCE339-9C11-40CA-9A02-D2FE64EA164B";

        /// <summary>
        /// Gets the Check-in Type page guid
        /// ParentPage: 
        /// </summary>
        public const string CHECK_IN_TYPE = "62C70118-0A6F-432A-9D84-A5296655CB9E";

        /// <summary>
        /// Gets the ChildPages page guid
        /// ParentPage: System Dialogs
        /// </summary>
        public const string CHILDPAGES = "D58F205E-E9CC-4BD9-BC79-F3DA86F6E346";

        /// <summary>
        /// Gets the CKEditor RockFileBrowser Plugin Frame page guid
        /// ParentPage: System Dialogs
        /// </summary>
        public const string CKEDITOR_ROCKFILEBROWSER_PLUGIN_FRAME = "4A4995CA-24F6-4D33-B861-A24274F53AA6";

        /// <summary>
        /// Gets the CKEditor RockMergeField Plugin Frame page guid
        /// ParentPage: System Dialogs
        /// </summary>
        public const string CKEDITOR_ROCKMERGEFIELD_PLUGIN_FRAME = "1FC09F0D-72F2-44E6-9D16-2884F9AF33DD";

        /// <summary>
        /// Gets the CMS Configuration page guid
        /// ParentPage: Rock Settings
        /// </summary>
        public const string CMS_CONFIGURATION = "B4A24AB7-9369-4055-883F-4F4892C39AE3";

        /// <summary>
        /// Gets the Communication page guid
        /// ParentPage: Support Pages
        /// </summary>
        public const string COMMUNICATION = "60002BC0-790A-4052-8F8D-B08C2C5D261C";

        /// <summary>
        /// Gets the Communication History page guid
        /// ParentPage: Communications
        /// </summary>
        public const string COMMUNICATION_HISTORY = "CADB44F2-2453-4DB5-AB11-DADA5162AB79";

        /// <summary>
        /// Gets the Communication Mediums page guid
        /// ParentPage: Communications
        /// </summary>
        public const string COMMUNICATION_MEDIUMS = "6FF35C53-F89F-4601-8543-2E2328C623F8";

        /// <summary>
        /// Gets the Communication Templates page guid
        /// ParentPage: Communications
        /// </summary>
        public const string COMMUNICATION_TEMPLATES_COMMUNICATIONS = "39F75137-90D2-4E6F-8613-F19344767594";

        /// <summary>
        /// Gets the Communication Templates page guid
        /// ParentPage: My Settings
        /// </summary>
        public const string COMMUNICATION_TEMPLATES_MY_SETTINGS = "EA611245-7A5E-4995-A3C6-EB97C6FD7C8D";

        /// <summary>
        /// Gets the Communication Transports page guid
        /// ParentPage: Communications
        /// </summary>
        public const string COMMUNICATION_TRANSPORTS = "29CC8A0B-6476-4200-8B93-DC9BA8767D59";

        /// <summary>
        /// Gets the Communications page guid
        /// ParentPage: People
        /// </summary>
        public const string COMMUNICATIONS_PEOPLE = "7F79E512-B9DB-4780-9887-AD6D63A39050";

        /// <summary>
        /// Gets the Communications page guid
        /// ParentPage: Rock Settings
        /// </summary>
        public const string COMMUNICATIONS_ROCK_SETTINGS = "199DC522-F4D6-4D82-AF44-3C16EE9D2CDA";

        /// <summary>
        /// Gets the Configuration page guid
        /// ParentPage: Check-in Configuration
        /// </summary>
        public const string CONFIGURATION = "4AB679AF-C8CC-427C-A615-0BF9F52E8E3E";

        /// <summary>
        /// Gets the Confirm page guid
        /// ParentPage: Security
        /// </summary>
        public const string CONFIRM = "D73F83B4-E20E-4F95-9A2C-511FB669F44C";

        /// <summary>
        /// Gets the Contact Us page guid
        /// ParentPage: Connect
        /// </summary>
        public const string CONTACT_US = "B1E63FE3-779C-4388-AFE4-FD6DFC034932";

        /// <summary>
        /// Gets the Content page guid
        /// ParentPage: Website
        /// </summary>
        public const string CONTENT = "117B547B-9D71-4EE9-8047-176676F5DC8C";

        /// <summary>
        /// Gets the Content Channel Detail page guid
        /// ParentPage: Content Channels
        /// </summary>
        public const string CONTENT_CHANNEL_DETAIL = "4AE244F5-A5BF-48CF-B53B-785148EC367D";

        /// <summary>
        /// Gets the Content Channel Types page guid
        /// ParentPage: CMS Configuration
        /// </summary>
        public const string CONTENT_CHANNEL_TYPES = "37E3D602-5D7D-4818-BCAA-C67EBB301E55";

        /// <summary>
        /// Gets the Content Channels page guid
        /// ParentPage: CMS Configuration
        /// </summary>
        public const string CONTENT_CHANNELS = "8ADCE4B2-8E95-4FA3-89C4-06A883E8145E";

        /// <summary>
        /// Gets the Content Detail page guid
        /// ParentPage: Content
        /// </summary>
        public const string CONTENT_DETAIL = "D18E837C-9E65-4A38-8647-DFF04A595D97";

        /// <summary>
        /// Gets the Content Item Detail page guid
        /// ParentPage: Content Channel Detail
        /// </summary>
        public const string CONTENT_ITEM_DETAIL = "ABF26679-1051-4F4F-8A67-5958E5BF71F8";

        /// <summary>
        /// Gets the Content Type Detail page guid
        /// ParentPage: Content Channel Types
        /// </summary>
        public const string CONTENT_TYPE_DETAIL = "91EAB2A2-4D44-4701-9ABE-37AE3E7A1B8F";

        /// <summary>
        /// Gets the Contributions page guid
        /// ParentPage: Person Pages
        /// </summary>
        public const string CONTRIBUTIONS = "53CF4CBE-85F9-4A50-87D7-0D72A3FB2892";

        /// <summary>
        /// Gets the Data Filters page guid
        /// ParentPage: System Settings
        /// </summary>
        public const string DATA_FILTERS = "5537F375-B652-4603-8E04-119C74414CD7";

        /// <summary>
        /// Gets the Data Integrity page guid
        /// ParentPage: Reporting
        /// </summary>
        public const string DATA_INTEGRITY = "84FD84DF-F58B-4B9D-A407-96276C40AB7E";

        /// <summary>
        /// Gets the Data Selects page guid
        /// ParentPage: System Settings
        /// </summary>
        public const string DATA_SELECTS = "227FDFB9-8C29-4B34-ABE5-E0579A3A6018";

        /// <summary>
        /// Gets the Data Transformations page guid
        /// ParentPage: System Settings
        /// </summary>
        public const string DATA_TRANSFORMATIONS = "9C569E6B-F745-40E4-B91B-A518CD6C2922";

        /// <summary>
        /// Gets the Data Views page guid
        /// ParentPage: Reporting
        /// </summary>
        public const string DATA_VIEWS = "4011CB37-28AA-46C4-99D5-826F4A9CADF5";

        /// <summary>
        /// Gets the Defined Type Detail page guid
        /// ParentPage: Defined Types
        /// </summary>
        public const string DEFINED_TYPE_DETAIL = "60C0C193-61CF-4B34-A0ED-67EF8FD44867";

        /// <summary>
        /// Gets the Defined Types page guid
        /// ParentPage: General Settings
        /// </summary>
        public const string DEFINED_TYPES = "E0E1DE66-B825-4BFB-A0B3-6E069AA9AA40";

        /// <summary>
        /// Gets the Devices page guid
        /// ParentPage: Check-in
        /// </summary>
        public const string DEVICES_CHECK_IN = "5A06C807-251C-4155-BBE7-AAC73D0745E3";

        /// <summary>
        /// Gets the Devices page guid
        /// ParentPage: General Settings
        /// </summary>
        public const string DEVICES_GENERAL_SETTINGS = "7E660A4D-72C5-4CF8-B144-16CFC2ADD4D7";

        /// <summary>
        /// Gets the DISC Assessment page guid
        /// ParentPage: Support Pages
        /// </summary>
        public const string DISC_ASSESSMENT = "C8CEF4B0-4A09-46D2-9B6B-CD2B6D3078B1";

        /// <summary>
        /// Gets the DISC Result page guid
        /// ParentPage: Person Profile
        /// </summary>
        public const string DISC_RESULT = "039F770B-5734-4735-ABF1-B39B77C84AD0";

        /// <summary>
        /// Gets the Download Payments page guid
        /// ParentPage: Administration
        /// </summary>
        public const string DOWNLOAD_PAYMENTS = "720819FC-1730-444A-9DE8-C98D29954170";

        /// <summary>
        /// Gets the Duplicate Detail page guid
        /// ParentPage: Duplicate Finder
        /// </summary>
        public const string DUPLICATE_DETAIL = "6F9CE971-75DF-4F2A-BD5E-A12B149A442E";

        /// <summary>
        /// Gets the Duplicate Finder page guid
        /// ParentPage: Data Integrity
        /// </summary>
        public const string DUPLICATE_FINDER = "21E94BF1-C594-44B6-AD91-939ABD04D36E";

        /// <summary>
        /// Gets the Edit Family page guid
        /// ParentPage: Person Profile
        /// </summary>
        public const string EDIT_FAMILY = "E9E1E5F2-467D-47CB-AF41-B4D9EF8B0B27";

        /// <summary>
        /// Gets the Edit My Account page guid
        /// ParentPage: My Account
        /// </summary>
        public const string EDIT_MY_ACCOUNT = "4A4655D1-BDD9-4ECE-A3F6-B655F0BDF9F5";

        /// <summary>
        /// Gets the Edit Person page guid
        /// ParentPage: Person Profile
        /// </summary>
        public const string EDIT_PERSON = "AD899394-13AD-4CAB-BCB3-CFFD79C9ADCC";

        /// <summary>
        /// Gets the Edit Scheduled Transaction page guid
        /// ParentPage: Contributions
        /// </summary>
        public const string EDIT_SCHEDULED_TRANSACTION_CONTRIBUTIONS = "D360B64F-1267-4518-95CD-99CD5AB87D88";

        /// <summary>
        /// Gets the Edit Scheduled Transaction page guid
        /// ParentPage: Scheduled Transaction
        /// </summary>
        public const string EDIT_SCHEDULED_TRANSACTION_SCHEDULED_TRANSACTION = "F1C3BBD3-EE91-4DDD-8880-1542EBCD8041";

        /// <summary>
        /// Gets the Email Preference page guid
        /// ParentPage: Support Pages
        /// </summary>
        public const string EMAIL_PREFERENCE = "74317DFD-F10D-4347-8E6A-CCD0FAFE31D7";

        /// <summary>
        /// Gets the Employee Details page guid
        /// ParentPage: Org Chart
        /// </summary>
        public const string EMPLOYEE_DETAILS = "DA8E33F3-2EEF-4C4B-87F3-715C3F107CAF";

        /// <summary>
        /// Gets the Entity Administration page guid
        /// ParentPage: Security
        /// </summary>
        public const string ENTITY_ADMINISTRATION = "F7F41856-F7EA-49A8-9D9B-917AC1964602";

        /// <summary>
        /// Gets the Entity Attributes page guid
        /// ParentPage: Security
        /// </summary>
        public const string ENTITY_ATTRIBUTES = "23507C90-3F78-40D4-B847-6FE8941FCD32";

        /// <summary>
        /// Gets the Exception Detail page guid
        /// ParentPage: Exception List
        /// </summary>
        public const string EXCEPTION_DETAIL = "F1F58172-E03E-4299-910A-ED34F857DAFB";

        /// <summary>
        /// Gets the Exception List page guid
        /// ParentPage: System Settings
        /// </summary>
        public const string EXCEPTION_LIST = "21DA6141-0A03-4F00-B0A8-3B110FBE2438";

        /// <summary>
        /// Gets the Extended Attributes page guid
        /// ParentPage: Person Pages
        /// </summary>
        public const string EXTENDED_ATTRIBUTES = "1C737278-4CBA-404B-B6B3-E3F0E05AB5FE";

        /// <summary>
        /// Gets the External Applications page guid
        /// ParentPage: Power Tools
        /// </summary>
        public const string EXTERNAL_APPLICATIONS = "5A676DCC-37F0-4624-8CCD-408A5A471D8A";

        /// <summary>
        /// Gets the External Homepage page guid
        /// ParentPage: 
        /// </summary>
        public const string EXTERNAL_HOMEPAGE = "85F25819-E948-4960-9DDF-00F54D32444E";

        /// <summary>
        /// Gets the Family Select page guid
        /// ParentPage: Check-in
        /// </summary>
        public const string FAMILY_SELECT = "10C97379-F719-4ACB-B8C6-651957B660A4";

        /// <summary>
        /// Gets the File Storage Providers page guid
        /// ParentPage: System Settings
        /// </summary>
        public const string FILE_STORAGE_PROVIDERS = "FEA8D6FC-B26F-48D5-BE69-6BCEF7CDC4E5";

        /// <summary>
        /// Gets the File Type page guid
        /// ParentPage: File Types
        /// </summary>
        public const string FILE_TYPE = "19CAC4D5-FE82-4AE0-BFD3-3C12E3024574";

        /// <summary>
        /// Gets the File Types page guid
        /// ParentPage: General Settings
        /// </summary>
        public const string FILE_TYPES = "66031C31-B397-4F78-8AB2-389B7D8731AA";

        /// <summary>
        /// Gets the Finance page guid
        /// ParentPage: Internal Homepage
        /// </summary>
        public const string FINANCE = "7BEB7569-C485-40A0-A609-B0678F6F7240";

        /// <summary>
        /// Gets the Financial Batch Detail page guid
        /// ParentPage: Batches
        /// </summary>
        public const string FINANCIAL_BATCH_DETAIL = "606BDA31-A8FE-473A-B3F8-A00ECF7E06EC";

        /// <summary>
        /// Gets the Financial Gateways page guid
        /// ParentPage: System Settings
        /// </summary>
        public const string FINANCIAL_GATEWAYS = "F65AA215-8B46-4E34-B709-FA956BF62C30";

        /// <summary>
        /// Gets the Following page guid
        /// ParentPage: Manage
        /// </summary>
        public const string FOLLOWING = "A6AE67F7-0B46-4F9A-9C96-054E1E82F784";

        /// <summary>
        /// Gets the Forgot User Name page guid
        /// ParentPage: Security
        /// </summary>
        public const string FORGOT_USER_NAME = "C6628FBD-F297-4C23-852E-40F1369C23A8";

        /// <summary>
        /// Gets the Functions page guid
        /// ParentPage: Finance
        /// </summary>
        public const string FUNCTIONS_FINANCE = "142627AE-6590-48E3-BFCA-3669260B8CF2";

        /// <summary>
        /// Gets the Gateway Detail page guid
        /// ParentPage: Financial Gateways
        /// </summary>
        public const string GATEWAY_DETAIL = "24DE6092-CE91-468C-8E49-94DB3875B9B7";

        /// <summary>
        /// Gets the General Settings page guid
        /// ParentPage: Rock Settings
        /// </summary>
        public const string GENERAL_SETTINGS = "0B213645-FA4E-44A5-8E4C-B2D8EF054985";

        /// <summary>
        /// Gets the Give Now page guid
        /// ParentPage: Give
        /// </summary>
        public const string GIVE_NOW = "1615E090-1889-42FF-AB18-5F7BE9F24498";

        /// <summary>
        /// Gets the Global Attributes page guid
        /// ParentPage: General Settings
        /// </summary>
        public const string GLOBAL_ATTRIBUTES = "A2753E03-96B1-4C83-AA11-FCD68C631571";

        /// <summary>
        /// Gets the Group Attendance page guid
        /// ParentPage: Group Toolbox
        /// </summary>
        public const string GROUP_ATTENDANCE_GROUP_TOOLBOX = "00D2DCE6-D9C0-47A0-BAE1-4591779AE2E1";

        /// <summary>
        /// Gets the Group Attendance page guid
        /// ParentPage: Group Viewer
        /// </summary>
        public const string GROUP_ATTENDANCE_GROUP_VIEWER = "7EA94B4F-013B-4A79-8D01-86994EB04604";

        /// <summary>
        /// Gets the Group Attendance Detail page guid
        /// ParentPage: Group Attendance
        /// </summary>
        public const string GROUP_ATTENDANCE_DETAIL = "0C00CD89-BF4C-4B19-9B0D-E1FA2CFF5DD7";

        /// <summary>
        /// Gets the Group Map page guid
        /// ParentPage: Group Viewer
        /// </summary>
        public const string GROUP_MAP = "60995C8C-862F-40F5-AFBB-13B49CDA77EB";

        /// <summary>
        /// Gets the Group Member Detail page guid
        /// ParentPage: Application Group Detail
        /// </summary>
        public const string GROUP_MEMBER_DETAIL_APPLICATION_GROUP_DETAIL = "C920AA8F-A8CA-4984-95EC-58B7309E670E";

        /// <summary>
        /// Gets the Group Member Detail page guid
        /// ParentPage: Group Viewer
        /// </summary>
        public const string GROUP_MEMBER_DETAIL_GROUP_VIEWER = "3905C63F-4D57-40F0-9721-C60A2F681911";

        /// <summary>
        /// Gets the Group Member Detail page guid
        /// ParentPage: Photo Request Application Group
        /// </summary>
        public const string GROUP_MEMBER_DETAIL_PHOTO_REQUEST_APPLICATION_GROUP = "34491B77-E94D-4DA6-9E74-0F6086522E4C";

        /// <summary>
        /// Gets the Group Member Detail page guid
        /// ParentPage: Security Roles Detail
        /// </summary>
        public const string GROUP_MEMBER_DETAIL_SECURITY_ROLES_DETAIL = "45899E6A-7CEC-44EC-8DBA-BD8850262C04";

        /// <summary>
        /// Gets the Group Registration page guid
        /// ParentPage: Small Groups
        /// </summary>
        public const string GROUP_REGISTRATION = "7D24FE9A-710C-4B25-B1C7-76161ED78DB8";

        /// <summary>
        /// Gets the Group Search Results page guid
        /// ParentPage: Support Pages
        /// </summary>
        public const string GROUP_SEARCH_RESULTS = "9C9CAD94-095E-4CC9-BC29-24BDE30492B2";

        /// <summary>
        /// Gets the Group Select page guid
        /// ParentPage: Check-in
        /// </summary>
        public const string GROUP_SELECT = "6F0CB22B-E05B-42F1-A329-9219E81F6C34";

        /// <summary>
        /// Gets the Group Toolbox page guid
        /// ParentPage: My Account
        /// </summary>
        public const string GROUP_TOOLBOX = "4D84E1B1-6BA0-4F04-A9F3-DD07A6CF3F38";

        /// <summary>
        /// Gets the Group Type Detail page guid
        /// ParentPage: Group Types
        /// </summary>
        public const string GROUP_TYPE_DETAIL = "5CD8E024-710B-4EDE-8C8C-4C9E15E6AFAB";

        /// <summary>
        /// Gets the Group Type Select page guid
        /// ParentPage: Check-in
        /// </summary>
        public const string GROUP_TYPE_SELECT = "60E3EA1F-FD6B-4F0E-9C72-A9960E13427C";

        /// <summary>
        /// Gets the Group Types page guid
        /// ParentPage: General Settings
        /// </summary>
        public const string GROUP_TYPES = "40899BCD-82B0-47F2-8F2A-B6AA3877B445";

        /// <summary>
        /// Gets the Group Viewer page guid
        /// ParentPage: Manage
        /// </summary>
        public const string GROUP_VIEWER = "4E237286-B715-4109-A578-C1445EC02707";

        /// <summary>
        /// Gets the Groups page guid
        /// ParentPage: Person Pages
        /// </summary>
        public const string GROUPS = "183B7B7E-105A-4C9A-A4BC-06CD26B7FE6D";

        /// <summary>
        /// Gets the History page guid
        /// ParentPage: Person Pages
        /// </summary>
        public const string HISTORY = "BC8E5377-0F6C-457A-9CF0-0F0A0AB2A418";

        /// <summary>
        /// Gets the History Categories page guid
        /// ParentPage: System Settings
        /// </summary>
        public const string HISTORY_CATEGORIES = "95ACFF8C-B9EE-41C6-BAC0-D117D6E1FADC";

        /// <summary>
        /// Gets the HTML Content Approval page guid
        /// ParentPage: Website
        /// </summary>
        public const string HTML_CONTENT_APPROVAL = "9DF95EFF-88B4-401A-8F5F-E3B8DB02A308";

        /// <summary>
        /// Gets the Internal Homepage page guid
        /// ParentPage: 
        /// </summary>
        public const string INTERNAL_HOMEPAGE = "20F97A93-7949-4C2A-8A5E-C756FE8585CA";

        /// <summary>
        /// Gets the Item Detail page guid
        /// ParentPage: Support Pages
        /// </summary>
        public const string ITEM_DETAIL = "56F1DC05-3D7D-49B6-9A30-5CF271C687F4";

        /// <summary>
        /// Gets the Jobs Administration page guid
        /// ParentPage: System Settings
        /// </summary>
        public const string JOBS_ADMINISTRATION = "C58ADA1A-6322-4998-8FED-C3565DE87EFA";

        /// <summary>
        /// Gets the Label Merge Fields page guid
        /// ParentPage: Check-in
        /// </summary>
        public const string LABEL_MERGE_FIELDS = "1DED4B72-1784-4781-A836-83D705B153FC";

        /// <summary>
        /// Gets the LaunchWorkflow page guid
        /// ParentPage: Workflows
        /// </summary>
        public const string LAUNCHWORKFLOW = "5DA89BC9-A185-4749-A843-314B72170D82";

        /// <summary>
        /// Gets the Layout Detail page guid
        /// ParentPage: Site Detail
        /// </summary>
        public const string LAYOUT_DETAIL = "E6217A2B-B16F-4E84-BF67-795CA7F5F9AA";

        /// <summary>
        /// Gets the Link Organization page guid
        /// ParentPage: Rock Shop
        /// </summary>
        public const string LINK_ORGANIZATION = "6E029432-56F4-46AD-9D9C-C122F3D3C55C";

        /// <summary>
        /// Gets the Location Detail page guid
        /// ParentPage: Location Editor
        /// </summary>
        public const string LOCATION_DETAIL = "1602C1CA-2EC7-4163-B0E1-1FE7306AC2B4";

        /// <summary>
        /// Gets the Location Editor page guid
        /// ParentPage: Power Tools
        /// </summary>
        public const string LOCATION_EDITOR = "47BFA50A-68D8-4841-849B-75AB3E5BCD6D";

        /// <summary>
        /// Gets the Location Select page guid
        /// ParentPage: Check-in
        /// </summary>
        public const string LOCATION_SELECT = "043BB717-5799-446F-B8DA-30E575110B0C";

        /// <summary>
        /// Gets the Location Services page guid
        /// ParentPage: System Settings
        /// </summary>
        public const string LOCATION_SERVICES = "1FD5698F-7279-463F-9637-9A80DB86BB86";

        /// <summary>
        /// Gets the Login page guid
        /// ParentPage: Security
        /// </summary>
        public const string LOGIN_SECURITY = "03CB988A-138C-448B-A43D-8891844EEB18";

        /// <summary>
        /// Gets the Manage page guid
        /// ParentPage: People
        /// </summary>
        public const string MANAGE = "B0F4B33D-DD11-4CCC-B79D-9342831B8701";

        /// <summary>
        /// Gets the Manage Workflows page guid
        /// ParentPage: Workflows
        /// </summary>
        public const string MANAGE_WORKFLOWS = "61E1B4B6-EACE-42E8-A2FB-37465E6D0004";

        /// <summary>
        /// Gets the Merge People page guid
        /// ParentPage: Manage
        /// </summary>
        public const string MERGE_PEOPLE = "F0C4E25F-83DF-44FF-AB5A-EF6C3044FAD3";

        /// <summary>
        /// Gets the Merge Template Detail page guid
        /// ParentPage: Merge Templates
        /// </summary>
        public const string MERGE_TEMPLATE_DETAIL = "F29C7AF7-6436-4C4B-BD17-330A487A4BF4";

        /// <summary>
        /// Gets the Merge Template Entry page guid
        /// ParentPage: Support Pages
        /// </summary>
        public const string MERGE_TEMPLATE_ENTRY = "B864FBFD-3699-4DB5-A8A9-12B2FEA86C7A";

        /// <summary>
        /// Gets the Merge Template Types page guid
        /// ParentPage: System Settings
        /// </summary>
        public const string MERGE_TEMPLATE_TYPES = "42717D07-3744-4187-89EC-F01EDD0FF5AD";

        /// <summary>
        /// Gets the Merge Templates page guid
        /// ParentPage: General Settings
        /// </summary>
        public const string MERGE_TEMPLATES_GENERAL_SETTINGS = "679AF013-0093-435E-AA49-E73B99EB9710";

        /// <summary>
        /// Gets the Merge Templates page guid
        /// ParentPage: My Settings
        /// </summary>
        public const string MERGE_TEMPLATES_MY_SETTINGS = "23F81A62-617A-498B-AAAC-D748F721176A";

        /// <summary>
        /// Gets the Metric Value Detail page guid
        /// ParentPage: Metrics
        /// </summary>
        public const string METRIC_VALUE_DETAIL = "64E16878-D5AE-40A5-94FE-C2E8BE62DF61";

        /// <summary>
        /// Gets the Metrics page guid
        /// ParentPage: Reporting
        /// </summary>
        public const string METRICS = "78D84825-EB1A-43C6-9AD5-5F0F84CC9A53";

        /// <summary>
        /// Gets the My Account page guid
        /// ParentPage: Security
        /// </summary>
        public const string MY_ACCOUNT_SECURITY = "290C53DC-0960-484C-B314-8301882A454C";

        /// <summary>
        /// Gets the My Settings page guid
        /// ParentPage: Internal Homepage
        /// </summary>
        public const string MY_SETTINGS = "CF54E680-2E02-4F16-B54B-A2F2D29CD932";

        /// <summary>
        /// Gets the My Workflows page guid
        /// ParentPage: Workflow
        /// </summary>
        public const string MY_WORKFLOWS = "F3FA9EBE-A540-4106-90E5-2DFB2D72BBF0";

        /// <summary>
        /// Gets the Named Locations page guid
        /// ParentPage: Check-in
        /// </summary>
        public const string NAMED_LOCATIONS_CHECK_IN = "96501070-BB46-4432-AA3C-A8C496691629";

        /// <summary>
        /// Gets the Named Locations page guid
        /// ParentPage: General Settings
        /// </summary>
        public const string NAMED_LOCATIONS_GENERAL_SETTINGS = "2BECFB85-D566-464F-B6AC-0BE90189A418";

        /// <summary>
        /// Gets the New Account page guid
        /// ParentPage: Security
        /// </summary>
        public const string NEW_ACCOUNT = "7D4E2142-D24E-4DD2-84BC-B34C5C3D0D46";

        /// <summary>
        /// Gets the New Communication page guid
        /// ParentPage: Communications
        /// </summary>
        public const string NEW_COMMUNICATION = "2A22D08D-73A8-4AAF-AC7E-220E8B2E7857";

        /// <summary>
        /// Gets the New Family page guid
        /// ParentPage: Manage
        /// </summary>
        public const string NEW_FAMILY = "6A11A13D-05AB-4982-A4C2-67A8B1950C74";

        /// <summary>
        /// Gets the Org Chart page guid
        /// ParentPage: Office Information
        /// </summary>
        public const string ORG_CHART = "C3909F1A-6908-4035-BB93-EC4FBFDCC536";

        /// <summary>
        /// Gets the Package Detail page guid
        /// ParentPage: Rock Shop
        /// </summary>
        public const string PACKAGE_DETAIL = "D6DC6AFE-70D9-43CF-9D76-EAEE2317FB14";

        /// <summary>
        /// Gets the Package Install page guid
        /// ParentPage: Rock Shop
        /// </summary>
        public const string PACKAGE_INSTALL = "6029A6D6-0059-4CC1-9751-1F012BC267F1";

        /// <summary>
        /// Gets the Packages By Category page guid
        /// ParentPage: Rock Shop
        /// </summary>
        public const string PACKAGES_BY_CATEGORY = "50D17FE7-88DB-46B2-9C58-DF8C0DE376A4";

        /// <summary>
        /// Gets the Page Map page guid
        /// ParentPage: CMS Configuration
        /// </summary>
        public const string PAGE_MAP = "EC7A06CD-AAB5-4455-962E-B4043EA2440E";

        /// <summary>
        /// Gets the Page Properties page guid
        /// ParentPage: System Dialogs
        /// </summary>
        public const string PAGE_PROPERTIES = "37759B50-DB4A-440D-A83B-4EF3B4727B1E";

        /// <summary>
        /// Gets the Page Route Detail page guid
        /// ParentPage: Routes
        /// </summary>
        public const string PAGE_ROUTE_DETAIL = "649A2B1E-7A15-4DA8-AF67-17874B6FE98F";

        /// <summary>
        /// Gets the People page guid
        /// ParentPage: Internal Homepage
        /// </summary>
        public const string PEOPLE = "97ECDC48-6DF6-492E-8C72-161F76AE111B";

        /// <summary>
        /// Gets the Person Attributes page guid
        /// ParentPage: General Settings
        /// </summary>
        public const string PERSON_ATTRIBUTES = "7BA1FAF4-B63C-4423-A818-CC794DDB14E3";

        /// <summary>
        /// Gets the Person Page Views page guid
        /// ParentPage: Person Profile
        /// </summary>
        public const string PERSON_PAGE_VIEWS = "82E9CDDB-A60E-4C0E-9306-C07BEAAD5F70";

        /// <summary>
        /// Gets the Person Pages page guid
        /// ParentPage: Support Pages
        /// </summary>
        public const string PERSON_PAGES = "BF04BB7E-BE3A-4A38-A37C-386B55496303";

        /// <summary>
        /// Gets the Person Profile page guid
        /// ParentPage: Check-in Manager
        /// </summary>
        public const string PERSON_PROFILE_CHECK_IN_MANAGER = "F3062622-C6AD-48F3-ADD7-7F58E4BD4EF3";

        /// <summary>
        /// Gets the Person Profile page guid
        /// ParentPage: Person Pages
        /// </summary>
        public const string PERSON_PROFILE_PERSON_PAGES = "08DBD8A5-2C35-4146-B4A8-0F7652348B25";

        /// <summary>
        /// Gets the Person Profile Badge Detail page guid
        /// ParentPage: Person Profile Badges
        /// </summary>
        public const string PERSON_PROFILE_BADGE_DETAIL = "D376EFD7-5B0D-44BF-A44D-03C466D2D30D";

        /// <summary>
        /// Gets the Person Profile Badges page guid
        /// ParentPage: General Settings
        /// </summary>
        public const string PERSON_PROFILE_BADGES = "26547B83-A92D-4D7E-82ED-691F403F16B6";

        /// <summary>
        /// Gets the Person Search page guid
        /// ParentPage: Support Pages
        /// </summary>
        public const string PERSON_SEARCH = "5E036ADE-C2A4-4988-B393-DAC58230F02E";

        /// <summary>
        /// Gets the Person Select page guid
        /// ParentPage: Check-in
        /// </summary>
        public const string PERSON_SELECT = "BB8CF87F-680F-48F9-9147-F4951E033D17";

        /// <summary>
        /// Gets the Person Viewed Detail page guid
        /// ParentPage: Security
        /// </summary>
        public const string PERSON_VIEWED_DETAIL = "48A9DF54-CC19-42FA-BDC6-97AF3E63029D";

        /// <summary>
        /// Gets the Photo Opt-Out page guid
        /// ParentPage: Support Pages
        /// </summary>
        public const string PHOTO_OPT_OUT = "04141667-1A08-4E15-8BB7-E3E312233E11";

        /// <summary>
        /// Gets the Photo Request Application Group page guid
        /// ParentPage: Photo Requests
        /// </summary>
        public const string PHOTO_REQUEST_APPLICATION_GROUP = "372BAF1A-F619-46FC-A69A-61E2A0A82F0E";

        /// <summary>
        /// Gets the Photo Requests page guid
        /// ParentPage: Data Integrity
        /// </summary>
        public const string PHOTO_REQUESTS = "325B50D6-545D-461A-9CB7-72B001E82F21";

        /// <summary>
        /// Gets the Photo Upload page guid
        /// ParentPage: Support Pages
        /// </summary>
        public const string PHOTO_UPLOAD = "8559A9F1-C6A4-4945-B393-74F6706A8FA2";

        /// <summary>
        /// Gets the Pledge Detail page guid
        /// ParentPage: Pledge List
        /// </summary>
        public const string PLEDGE_DETAIL = "EF7AA296-CA69-49BC-A28B-901A8AAA9466";

        /// <summary>
        /// Gets the Pledge List page guid
        /// ParentPage: Functions
        /// </summary>
        public const string PLEDGE_LIST = "1570D2AF-4FE2-4FC7-BED9-F20EBCBE9867";

        /// <summary>
        /// Gets the Power Tools page guid
        /// ParentPage: Rock Settings
        /// </summary>
        public const string POWER_TOOLS = "7F1F4130-CB98-473B-9DE1-7A886D2283ED";

        /// <summary>
        /// Gets the Prayer page guid
        /// ParentPage: Manage
        /// </summary>
        public const string PRAYER_MANAGE = "1A3437C8-D4CB-4329-A366-8D6A4CBF79BF";

        /// <summary>
        /// Gets the Prayer Categories page guid
        /// ParentPage: General Settings
        /// </summary>
        public const string PRAYER_CATEGORIES = "FA2A1171-9308-41C7-948C-C9EBEA5BD668";

        /// <summary>
        /// Gets the Prayer Request Detail page guid
        /// ParentPage: Prayer
        /// </summary>
        public const string PRAYER_REQUEST_DETAIL = "89C3DB4A-BAFD-45C8-88C6-45D8FEC48B48";

        /// <summary>
        /// Gets the (Public) Create Pledge page guid
        /// ParentPage: Pledge List
        /// </summary>
        public const string PUBLIC_CREATE_PLEDGE = "60051DAF-2986-406D-A78B-1609CBF2256D";

        /// <summary>
        /// Gets the Purchases page guid
        /// ParentPage: Rock Shop
        /// </summary>
        public const string PURCHASES = "6A163569-2826-4EF2-8208-879DDBDC0896";

        /// <summary>
        /// Gets the Report Detail page guid
        /// ParentPage: Reports
        /// </summary>
        public const string REPORT_DETAIL = "DB58BC69-01FA-4F3E-832B-B1D0DE915C21";

        /// <summary>
        /// Gets the Reporting page guid
        /// ParentPage: Tools
        /// </summary>
        public const string REPORTING = "BB0ACD18-24FB-42BA-B89A-2FFD80472F5B";

        /// <summary>
        /// Gets the Reports page guid
        /// ParentPage: Data Integrity
        /// </summary>
        public const string REPORTS_DATA_INTEGRITY = "134D8730-6AF5-4518-89EE-7370FA78676E";

        /// <summary>
        /// Gets the Reports page guid
        /// ParentPage: Reporting
        /// </summary>
        public const string REPORTS_REPORTING = "0FDF1F63-CFB3-4F8E-AC5D-A5312B522D6D";

        /// <summary>
        /// Gets the REST Controller Actions page guid
        /// ParentPage: REST Controllers
        /// </summary>
        public const string REST_CONTROLLER_ACTIONS = "7F5EF1AA-0E27-4AA1-A5E1-1CD6DDDCDDC5";

        /// <summary>
        /// Gets the REST Controllers page guid
        /// ParentPage: Security
        /// </summary>
        public const string REST_CONTROLLERS = "0D51F443-1C0D-4C71-8BAE-E5F5A35E8B79";

        /// <summary>
        /// Gets the REST CORS Domains page guid
        /// ParentPage: Security
        /// </summary>
        public const string REST_CORS_DOMAINS = "B03A8C4E-E394-44B0-B7CC-89B74C79C325";

        /// <summary>
        /// Gets the REST Key Detail page guid
        /// ParentPage: REST Keys
        /// </summary>
        public const string REST_KEY_DETAIL = "594692AA-5647-4F9A-9488-AADB990FDE56";

        /// <summary>
        /// Gets the REST Keys page guid
        /// ParentPage: Security
        /// </summary>
        public const string REST_KEYS = "881AB1C2-4E00-4A73-80CC-9886B3717A20";

        /// <summary>
        /// Gets the Rock Settings page guid
        /// ParentPage: Admin Tools
        /// </summary>
        public const string ROCK_SETTINGS = "550A898C-EDEA-48B5-9C58-B20EC13AF13B";

        /// <summary>
        /// Gets the Rock Shop page guid
        /// ParentPage: Rock Settings
        /// </summary>
        public const string ROCK_SHOP = "B093E7A0-5E7E-4A5F-AEF3-CE397D342BFA";

        /// <summary>
        /// Gets the Rock Update page guid
        /// ParentPage: General Settings
        /// </summary>
        public const string ROCK_UPDATE = "A3990266-CB0D-4FB5-882C-3852ED5D96AB";

        /// <summary>
        /// Gets the Routes page guid
        /// ParentPage: CMS Configuration
        /// </summary>
        public const string ROUTES = "4A833BE3-7D5E-4C38-AF60-5706260015EA";

        /// <summary>
        /// Gets the Safe Sender Domains page guid
        /// ParentPage: Communications
        /// </summary>
        public const string SAFE_SENDER_DOMAINS = "B90576B0-110E-4DC0-8EB8-4668C5238508";

        /// <summary>
        /// Gets the Sample Data page guid
        /// ParentPage: Power Tools
        /// </summary>
        public const string SAMPLE_DATA = "844ABF2A-D085-4370-945B-86C89580C6D5";

        /// <summary>
        /// Gets the Schedule Builder page guid
        /// ParentPage: Check-in Configuration
        /// </summary>
        public const string SCHEDULE_BUILDER = "A286D16B-FDDF-4D89-B98F-D51188B611E6";

        /// <summary>
        /// Gets the Scheduled Job Detail page guid
        /// ParentPage: Jobs Administration
        /// </summary>
        public const string SCHEDULED_JOB_DETAIL = "E18AC09D-45CD-49CF-8874-157B32556B7D";

        /// <summary>
        /// Gets the Scheduled Transaction page guid
        /// ParentPage: Scheduled Transactions
        /// </summary>
        public const string SCHEDULED_TRANSACTION = "996F5541-D2E1-47E4-8078-80A388203CEC";

        /// <summary>
        /// Gets the Scheduled Transactions page guid
        /// ParentPage: Functions
        /// </summary>
        public const string SCHEDULED_TRANSACTIONS = "F23C5BF7-4F52-448F-8445-6BAEEE3030AB";

        /// <summary>
        /// Gets the Schedules page guid
        /// ParentPage: Check-in
        /// </summary>
        public const string SCHEDULES_CHECK_IN = "AFFFB245-A0EB-4002-B736-A2D52DD692CF";

        /// <summary>
        /// Gets the Schedules page guid
        /// ParentPage: General Settings
        /// </summary>
        public const string SCHEDULES_GENERAL_SETTINGS = "F5D6D7DD-FD5F-494C-83DC-E2AF63C705D1";

        /// <summary>
        /// Gets the Search page guid
        /// ParentPage: Check-in
        /// </summary>
        public const string SEARCH = "D47858C0-0E6E-46DC-AE99-8EC84BA5F45F";

        /// <summary>
        /// Gets the Search Services page guid
        /// ParentPage: System Settings
        /// </summary>
        public const string SEARCH_SERVICES = "1719F597-5BA9-458D-9362-9C3E558E5C82";

        /// <summary>
        /// Gets the Search Type page guid
        /// ParentPage: Check-in
        /// </summary>
        public const string SEARCH_TYPE = "3E0327B1-EE0E-41DC-87DB-C4C14922A7CA";

        /// <summary>
        /// Gets the Security page guid
        /// ParentPage: Admin Tools
        /// </summary>
        public const string SECURITY_ADMIN_TOOLS = "8C71A7E2-18A8-41C0-AB40-AD85CF90CA81";

        /// <summary>
        /// Gets the Security page guid
        /// ParentPage: Person Pages
        /// </summary>
        public const string SECURITY_PERSON_PAGES = "0E56F56E-FB32-4827-A69A-B90D43CB47F5";

        /// <summary>
        /// Gets the Security page guid
        /// ParentPage: Rock Settings
        /// </summary>
        public const string SECURITY_ROCK_SETTINGS = "91CCB1C9-5F9F-44F5-8BE2-9EC3A3CFD46F";

        /// <summary>
        /// Gets the Security page guid
        /// ParentPage: System Dialogs
        /// </summary>
        public const string SECURITY_SYSTEM_DIALOGS = "86D5E33E-E351-4CA5-9925-849C6C467257";

        /// <summary>
        /// Gets the Security Roles page guid
        /// ParentPage: Security
        /// </summary>
        public const string SECURITY_ROLES = "D9678FEF-C086-4232-972C-5DBAC14BFEE6";

        /// <summary>
        /// Gets the Security Roles Detail page guid
        /// ParentPage: Security Roles
        /// </summary>
        public const string SECURITY_ROLES_DETAIL = "48AAD428-A9C9-4BBB-A80F-B85F28D31240";

        /// <summary>
        /// Gets the Send Photo Requests page guid
        /// ParentPage: Communications
        /// </summary>
        public const string SEND_PHOTO_REQUESTS = "B64D0429-488C-430E-8C32-5C7F32589F73";

        /// <summary>
        /// Gets the Sites page guid
        /// ParentPage: CMS Configuration
        /// </summary>
        public const string SITES = "7596D389-4EAB-4535-8BEE-229737F46F44";

        /// <summary>
        /// Gets the SMS From Values page guid
        /// ParentPage: Communications
        /// </summary>
        public const string SMS_FROM_VALUES = "3F1EA6E5-6C61-444A-A80E-5B66F96F521B";

        /// <summary>
        /// Gets the SQL Command page guid
        /// ParentPage: Power Tools
        /// </summary>
        public const string SQL_COMMAND = "03C49950-9C4C-4668-9C65-9A0DF43D1B33";

        /// <summary>
        /// Gets the Success page guid
        /// ParentPage: Check-in
        /// </summary>
        public const string SUCCESS = "E08230B8-35A4-40D6-A0BB-521418314DA9";

        /// <summary>
        /// Gets the Support Pages page guid
        /// ParentPage: Internal Homepage
        /// </summary>
        public const string SUPPORT_PAGES_INTERNAL_HOMEPAGE = "936C90C4-29CF-4665-A489-7C687217F7B8";

        /// <summary>
        /// Gets the System Configuration page guid
        /// ParentPage: System Settings
        /// </summary>
        public const string SYSTEM_CONFIGURATION = "7BFD28F2-6B90-4191-B7C1-2CDBFA23C3FA";

        /// <summary>
        /// Gets the System Dialogs page guid
        /// ParentPage: Internal Homepage
        /// </summary>
        public const string SYSTEM_DIALOGS = "E7BD353C-91A6-4C15-A6C8-F44D0B16D16E";

        /// <summary>
        /// Gets the System Email Categories page guid
        /// ParentPage: Communications
        /// </summary>
        public const string SYSTEM_EMAIL_CATEGORIES_COMMUNICATIONS = "B55323CD-F494-43E7-97BF-4E13DAB58E0B";

        /// <summary>
        /// Gets the System Email Categories page guid
        /// ParentPage: System Emails
        /// </summary>
        public const string SYSTEM_EMAIL_CATEGORIES_SYSTEM_EMAILS = "66FAF7A6-7523-475C-A88D-51C75178A785";

        /// <summary>
        /// Gets the System Email Details page guid
        /// ParentPage: System Emails
        /// </summary>
        public const string SYSTEM_EMAIL_DETAILS = "588C72A8-7DEC-405F-BA4A-FE64F87CB817";

        /// <summary>
        /// Gets the System Emails page guid
        /// ParentPage: Communications
        /// </summary>
        public const string SYSTEM_EMAILS = "89B7A631-EA6F-4DA3-9380-04EE67B63E9E";

        /// <summary>
        /// Gets the System Information page guid
        /// ParentPage: System Dialogs
        /// </summary>
        public const string SYSTEM_INFORMATION = "8A97CC93-3E93-4286-8440-E5217B65A904";

        /// <summary>
        /// Gets the System Settings page guid
        /// ParentPage: Rock Settings
        /// </summary>
        public const string SYSTEM_SETTINGS = "C831428A-6ACD-4D49-9B2D-046D399E3123";

        /// <summary>
        /// Gets the Tag page guid
        /// ParentPage: Tags
        /// </summary>
        public const string TAG = "F3BD2F37-F16A-4C98-8A4C-C14A16AAFA3A";

        /// <summary>
        /// Gets the Tag Details page guid
        /// ParentPage: Tags
        /// </summary>
        public const string TAG_DETAILS = "D258BF5B-B585-4C5B-BDCD-99F7519D45E2";

        /// <summary>
        /// Gets the Tags page guid
        /// ParentPage: General Settings
        /// </summary>
        public const string TAGS_GENERAL_SETTINGS = "F111791B-6A58-4388-8533-00E913F48F41";

        /// <summary>
        /// Gets the Tags page guid
        /// ParentPage: Manage
        /// </summary>
        public const string TAGS_MANAGE = "2654EBE9-F585-4E64-93F3-102357F89660";

        /// <summary>
        /// Gets the Time Select page guid
        /// ParentPage: Check-in
        /// </summary>
        public const string TIME_SELECT = "C0AFA081-B64E-4006-BFFC-A350A51AE4CC";

        /// <summary>
        /// Gets the Tools page guid
        /// ParentPage: Internal Homepage
        /// </summary>
        public const string TOOLS = "98163C8B-5C91-4A68-BB79-6AD948A604CE";

        /// <summary>
        /// Gets the Transaction Detail page guid
        /// ParentPage: Financial Batch Detail
        /// </summary>
        public const string TRANSACTION_DETAIL_FINANCIAL_BATCH_DETAIL = "97716641-D003-4663-9EA2-D9BB94E7955B";

        /// <summary>
        /// Gets the Transaction Detail page guid
        /// ParentPage: Transactions
        /// </summary>
        public const string TRANSACTION_DETAIL_TRANSACTIONS = "B67E38CB-2EF1-43EA-863A-37DAA1C7340F";

        /// <summary>
        /// Gets the Transaction Matching page guid
        /// ParentPage: Financial Batch Detail
        /// </summary>
        public const string TRANSACTION_MATCHING = "CD18FE52-8D6A-49C9-81BF-DF97C5BA0302";

        /// <summary>
        /// Gets the Transactions page guid
        /// ParentPage: Functions
        /// </summary>
        public const string TRANSACTIONS = "7CA317B5-5C47-465D-B407-7D614F2A568F";

        /// <summary>
        /// Gets the User Accounts page guid
        /// ParentPage: Security
        /// </summary>
        public const string USER_ACCOUNTS = "306BFEF8-596C-482A-8DEC-34A7B622E688";

        /// <summary>
        /// Gets the Verify Photos page guid
        /// ParentPage: Photo Requests
        /// </summary>
        public const string VERIFY_PHOTOS = "07E4BA19-614A-42D0-9D75-DFB31374844D";

        /// <summary>
        /// Gets the Website page guid
        /// ParentPage: Tools
        /// </summary>
        public const string WEBSITE = "F7105BFE-B28C-41B6-9CE6-F1018D77DD8F";

        /// <summary>
        /// Gets the Welcome page guid
        /// ParentPage: Check-in
        /// </summary>
        public const string WELCOME = "432B615A-75FF-4B14-9C99-3E769F866950";

        /// <summary>
        /// Gets the Workflow page guid
        /// ParentPage: Tools
        /// </summary>
        public const string WORKFLOW = "CDB27DB2-977C-415A-AED5-D0751DFD5DF2";

        /// <summary>
        /// Gets the Workflow Configuration page guid
        /// ParentPage: General Settings
        /// </summary>
        public const string WORKFLOW_CONFIGURATION = "DCB18A76-6DFF-48A5-A66E-2CAA10D2CA1A";

        /// <summary>
        /// Gets the Workflow Detail page guid
        /// ParentPage: Manage Workflows
        /// </summary>
        public const string WORKFLOW_DETAIL = "BA547EED-5537-49CF-BD4E-C583D760788C";

        /// <summary>
        /// Gets the Workflow Entry page guid
        /// ParentPage: Workflows
        /// </summary>
        public const string WORKFLOW_ENTRY = "0550D2AA-A705-4400-81FF-AB124FDF83D7";

        /// <summary>
        /// Gets the Workflow Trigger page guid
        /// ParentPage: Workflow Triggers
        /// </summary>
        public const string WORKFLOW_TRIGGER = "04D844EA-7780-427B-8912-FA5EB7C74439";

        /// <summary>
        /// Gets the Workflow Triggers page guid
        /// ParentPage: General Settings
        /// </summary>
        public const string WORKFLOW_TRIGGERS = "1A233978-5BF4-4A09-9B86-6CC4C081F48B";

        /// <summary>
        /// Gets the Workflows page guid
        /// ParentPage: Data Integrity
        /// </summary>
        public const string WORKFLOWS_DATA_INTEGRITY = "90C32D5E-A5D5-4CE4-AAB0-E31B43B585E4";

        /// <summary>
        /// Gets the Workflows page guid
        /// ParentPage: Workflow
        /// </summary>
        public const string WORKFLOWS_WORKFLOW = "6510AB6B-DFB4-4DBF-9F0F-7EA598E4AC54";

        /// <summary>
        /// Gets the ZoneBlocks page guid
        /// ParentPage: System Dialogs
        /// </summary>
        public const string ZONEBLOCKS = "9F36531F-C1B5-4E23-8FA3-18B6DAFF1B0B";
    }
}