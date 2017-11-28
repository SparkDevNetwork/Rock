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
    public partial class PageShortLinkUrl : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AlterColumn("dbo.PageShortLink", "Url", c => c.String(nullable: false));

            // SK: Added a security rule to block view for RSR - Benevolence Group on this page
            RockMigrationHelper.AddSecurityAuthForPage( "A3EF32AC-B0FE-4140-A6F4-134FDD247CBD", 0, "View", false, "02FA0881-3552-42B8-A519-D021139B800F", 0, "25BF7D65-1642-463E-8081-C3637B9ABC02" ); // Page:Fundraising Matching

            // DT: Add icon for meta models
            RockMigrationHelper.AddBlockAttributeValue( "2583DE89-F028-4ACE-9E1F-2873340726AC", "75A0C88F-7F5B-48A2-88A4-C3A62F0EDF9A", @"CMS^fa fa-code|Communication^fa fa-comment|Connection^fa fa-plug|Core^fa fa-gear|Event^fa fa-clipboard|Finance^fa fa-money|Group^fa fa-users|Prayer^fa fa-cloud-upload|Reporting^fa fa-list-alt|Workflow^fa fa-gears|Other^fa fa-question-circle|CRM^fa fa-user|Meta^fa fa-table" );

            // MP: Impersonate Action Attributes on Bio Block
            RockMigrationHelper.UpdateBlockTypeAttribute( "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Impersonation", "EnableImpersonation", "", "Should the Impersonate custom action be enabled? Note: If enabled, it is only visible to users that are authorized to administrate the person.", 3, @"False", "A57178A8-A531-42D2-BCC6-90204ADB7A40" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Impersonation Start Page", "ImpersonationStartPage", "", "The page to navigate to after clicking the Impersonate action.", 4, @"", "98066864-F4C0-4997-87BB-A35DD8916421" );
            RockMigrationHelper.AddBlockAttributeValue( "B5C1FDB6-0224-43E4-8E26-6B2EAF86253A", "98066864-F4C0-4997-87BB-A35DD8916421", @"" );

            // DT: Add TextToWorkflow to core
            RockMigrationHelper.AddDefinedType( "Workflow", "Text To Workflow", "Matches SMS phones and keywords to launch workflows of various types", "2CACB86F-D811-4483-98E1-272F1FF8FF5D" );
            RockMigrationHelper.AddDefinedTypeAttribute( "2CACB86F-D811-4483-98E1-272F1FF8FF5D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Keyword Expression", "KeywordExpression", "The incoming keyword regular expression to match to (e.g. 'regist.*' would match 'register' or 'reg')", 2, "", "3A526D6C-06FC-46CD-A447-9A6D9A74BB4F" );
            RockMigrationHelper.AddDefinedTypeAttribute( "2CACB86F-D811-4483-98E1-272F1FF8FF5D", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow Type", "WorkflowType", "The type of workflow to launch.", 3, "", "0097D00F-1F29-4217-8E67-D37A619A6FA3" );
            RockMigrationHelper.AddDefinedTypeAttribute( "2CACB86F-D811-4483-98E1-272F1FF8FF5D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Workflow Name Template", "WorkflowNameTemplate", "The lava template to use for setting the workflow name. See the defined type's help text for a listing of merge fields. <span class='tip tip-lava'></span>", 4, "", "67E09C64-3558-48B7-9A27-A9499D0826E8" );
            RockMigrationHelper.AddDefinedTypeAttribute( "2CACB86F-D811-4483-98E1-272F1FF8FF5D", "73B02051-0D38-4AD9-BF81-A2D477DE4F70", "Workflow Attributes", "WorkflowAttributes", "Key/value list of workflow attributes to set with the given lava merge template. See the defined type’s help text for a listing of merge fields. <span class='tip tip-lava'></span>", 5, "", "836CFC0B-6750-4A93-8309-EAB868B845AF" );
            RockMigrationHelper.UpdateAttributeQualifier( "836CFC0B-6750-4A93-8309-EAB868B845AF", "keyprompt", "Attribute Key", "5F71B89A-44DC-48C6-A12D-CF0C056B6929" );
            RockMigrationHelper.UpdateAttributeQualifier( "836CFC0B-6750-4A93-8309-EAB868B845AF", "valueprompt", "Merge Template", "0660523C-E12E-44B3-A0BA-8BC4D866204E" );

            Sql( @"UPDATE [Attribute] SET [IsGridColumn] = 'True' WHERE [Guid] = '0097D00F-1F29-4217-8E67-D37A619A6FA3'" );

            Sql( @"UPDATE [DefinedType] SET [HelpText] = '

The following merge fields are available for both the ''Name Template'' and ''Workflow Attributes'' attributes.
<p>
    <a data-toggle=""collapse""  href=""#collapsefields"" class=''btn btn-action btn-xs''>show/hide fields</a>
</p>

<div id=""collapsefields"" class=""panel-collapse collapse"">
<pre>
{
   ""FromPhone"":""+15555555555"",
   ""ToPhone"":""+15555555555"",
   ""MessageBody"":""keyword"",
   ""ReceivedTime"":""10:02 PM"",
   ""ReceivedDate"":""7/29/2014"",
   ""FromPerson"":{
      ""FullName"":""Ted Decker"",
      ""IsDeceased"":false,
      ""TitleValueId"":null,
      ""FirstName"":""Theodore"",
      ""NickName"":""Ted"",
      ""MiddleName"":"""",
      ""LastName"":""Decker"",
      ""SuffixValueId"":null,
      ""PhotoId"":36,
      ""BirthDay"":10,
      ""BirthMonth"":2,
      ""BirthYear"":1976,
      ""Gender"":1,
      ""MaritalStatusValueId"":143,
      ""AnniversaryDate"":null,
      ""GraduationDate"":""1994-06-01T00:00:00"",
      ""GivingGroupId"":41,
      ""Email"":""ted@rocksoliddemochurch.com"",
      ""IsEmailActive"":true,
      ""EmailNote"":null,
      ""EmailPreference"":0,
      ""ReviewReasonNote"":null,
      ""InactiveReasonNote"":null,
      ""SystemNote"":null,
      ""ViewedCount"":null,
      ""PrimaryAliasId"":2,
      ""BirthdayDayOfWeek"":""Monday"",
      ""BirthdayDayOfWeekShort"":""Mon"",
      ""PhotoUrl"":""/GetImage.ashx?id=36"",
      ""BirthDate"":""1976-02-10T00:00:00"",
      ""Age"":38,
      ""DaysToBirthday"":196,
      ""Grade"":33,
      ""GradeFormatted"":"""",
      ""CreatedDateTime"":null,
      ""ModifiedDateTime"":""2014-07-23T23:26:18.357"",
      ""CreatedByPersonAliasId"":null,
      ""ModifiedByPersonAliasId"":1,
      ""Attributes"":null,
      ""AttributeValues"":null,
      ""Id"":2,
      ""Guid"":""8fedc6ee-8630-41ed-9fc5-c7157fd1eaa4"",
      ""UrlEncodedKey"":""EAAAAHkbB7e!2bYK0Xdq9Ib9ePIpblOpW9jAghYRMyMWe9vjb3BF8mvzaL6CCNVZrs6zk4nNCgX9JkXkmY3KRudX!2bKO!2fg!3d"",
	  ""ConnectionStatusValue"":{
         ""Order"":0,
         ""Name"":""Member"",
         ""Description"":""Applied to individuals who have completed all requirements established to become a member."",
         ""Id"":65,
         ""Guid"":""41540783-d9ef-4c70-8f1d-c9e83d91ed5f"",
      },
      ""MaritalStatusValue"":{
         ""Order"":0,
         ""Name"":""Married"",
         ""Description"":""Used with an individual is married."",
         ""Id"":143,
         ""Guid"":""5fe5a540-7d9f-433e-b47e-4229d1472248"",
         ""UrlEncodedKey"":""EAAAAHiZvJY3Dkl85B1F8SJ2AnS8onTRarYspmUq5VOIkKRWurg4E913MdwkRq2tzQWF7qQoraHlMey24opvgDMvNQ8!3d""
      },
      ""PhoneNumbers"":[
         {
            ""NumberTypeValue"":{
               ""Order"":1,
               ""Name"":""Home"",
               ""Description"":""Home phone number"",
               ""Id"":13,
               ""Guid"":""aa8732fb-2cea-4c76-8d6d-6aaa2c6a4303"",
               ""UrlEncodedKey"":""EAAAABRiGGLD6CTiCKqb29OBr2KkOwimKb0BQp8oKV0gvKIzSAwNzMXg!2bwvQUHvY!2brdPIJZ7nBdJ1QcwbUsUwB7ITYc!3d""
            },
            ""CountryCode"":""1"",
            ""Number"":""6235553322"",
            ""NumberFormatted"":""(623) 555-3322"",
            ""Extension"":null,
            ""NumberTypeValueId"":13,
            ""IsMessagingEnabled"":false,
            ""IsUnlisted"":false,
            ""Description"":null,
            ""NumberFormattedWithCountryCode"":""+1 (623) 555-3322"",
            ""Id"":1,
            ""Guid"":""fdfbd202-a67e-4ea5-9cff-2b939593d054"",
            ""UrlEncodedKey"":""EAAAALquQ6scz74JCg3iV50!2bnF0LIDQHyjyJOYxwRn7BSbFX4Z6Y76g7eBbFoXpkSQ67FjHd8VZ2M4!2bZzzbAHhKLYtA!3d""
         },
         {
            ""NumberTypeValue"":{
               ""Order"":2,
               ""Name"":""Work"",
               ""Description"":""Work phone number"",
               ""Guid"":""2cc66d5a-f61c-4b74-9af9-590a9847c13c"",
               ""UrlEncodedKey"":""EAAAAAXigHXhetFtBvzV!2bFC7PbJC!2bYcv77nHDYssZobfHDgyvd004RG7xxctsdccwZIHFajKAcfzgw2mwH7IS8iCJ8Y!3d""
            },
            ""CountryCode"":""1"",
            ""Number"":""6235552444"",
            ""NumberFormatted"":""(623) 555-2444"",
            ""Extension"":null,
            ""NumberTypeValueId"":136,
            ""IsMessagingEnabled"":false,
            ""IsUnlisted"":false,
            ""Description"":null,
            ""NumberFormattedWithCountryCode"":""+1 (623) 555-2444"",
            ""Id"":3,
            ""Guid"":""e26f602c-a742-4ee5-b332-5583b2bd31c0"",
            ""UrlEncodedKey"":""EAAAADItUMe9!2bgCRZ3uugX9STAPz7fllpXVUFdK65R5DlIMYjyjCFDwQIJ7rp3tZrztTsAMsRk5AbuocEH3TvXY3Cgk!3d""
         },
         {
            ""NumberTypeValue"":{
               ""Order"":0,
               ""Name"":""Mobile"",
               ""Description"":""Mobile/Cell phone number"",
               ""Id"":12,
               ""Guid"":""407e7e45-7b2e-4fcd-9605-ecb1339f2453"",
               ""UrlEncodedKey"":""EAAAANaN5diFoMocUiXgsoPb7g!2fwZ0NA8lvD8HPIevtFueL1YrEBGCM9GQF4FANqYrid4yQZm5nFUR!2bjt9JfpMf12Rg!3d""
            },
            ""CountryCode"":""1"",
            ""Number"":""6238662792"",
            ""NumberFormatted"":""(623) 866-2792"",
            ""Extension"":null,
            ""NumberTypeValueId"":12,
            ""IsMessagingEnabled"":true,
            ""IsUnlisted"":false,
            ""Description"":null,
            ""NumberFormattedWithCountryCode"":""+1 (623) 866-2792"",
            ""Id"":15,
            ""Guid"":""96e6b17e-7a18-4231-915a-27a98c02d4c4"",
            ""UrlEncodedKey"":""EAAAADHSjNg82gKW!2bII0kmO3Wd3bKBOAOJjW8Mb!2buKiNGhzr4ElEd4G4PGz7YHoRWqb4ozsiOr3A7mhzJw0VZg!2fD7RY!3d""
         }
      ],
      ""Photo"":{
         ""BinaryFileType"":{
            ""Name"":""Person Image"",
            ""Description"":""Image of a Person"",
            ""IconCssClass"":""fa fa-camera"",
	    },
         ""IsTemporary"":false,
         ""BinaryFileTypeId"":5,
         ""Url"":null,
         ""FileName"":""decker_ted.jpg"",
         ""MimeType"":""image/jpeg"",
         ""Description"":null,
         ""StorageEntityTypeId"":51,
         ""CreatedDateTime"":null,
         ""ModifiedDateTime"":null,
         ""CreatedByPersonAliasId"":null,
         ""ModifiedByPersonAliasId"":null,
         ""Attributes"":null,
         ""AttributeValues"":null,
         ""Id"":36,
         ""Guid"":""5c875f30-7e0e-4d2f-a14b-51aa01e1d887"",
         ""UrlEncodedKey"":""EAAAAGGE!2budoDA82FsBBemJhMyckBepZzs6zblCc0RNJ1j!2bjtnP!2bbzTTGDffFM7brwVOaM!2f6i0Aa2iyc6U1DyorqpIw!3d""
      },
      ""RecordStatusReasonValue"":null,
      ""RecordStatusValue"":{
         ""Order"":0,
         ""Name"":""Active"",
         ""Description"":""Denotes an individual that is actively participating in the activities or services of the organization."",
         ""Id"":3,
         ""Guid"":""618f906c-c33d-4fa3-8aef-e58cb7b63f1e"",
         ""UrlEncodedKey"":""EAAAABP8y!2bgB9EHvyeR2Tr7miJ9SXQRqyr7lNsl98PUWXOqXclMLzmnQVRm8Msmz0!2f1FwMsTsNjPLz6t!2fy1GRuB!2fNl4!3d""
      },
      ""RecordTypeValue"":{
         ""Order"":0,
         ""Name"":""Person"",
         ""Description"":""A Person Record"",
         ""Id"":1,
         ""Guid"":""36cf10d6-c695-413d-8e7c-4546efef385e"",
         ""UrlEncodedKey"":""EAAAAPmuNdsg7fGQlwQdnyA7NVA3cihjY2nm9crF18A629Vz33FJ8p7SARghTWQ8YJ2D!2fY5g8xsLCk6ImNq3UUczuno!3d""
      },
      ""ReviewReasonValue"":null,
      ""SuffixValue"":null,
      ""TitleValue"":null,
   }
}
</pre>


</div>
' WHERE [Guid] = '2CACB86F-D811-4483-98E1-272F1FF8FF5D'" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AlterColumn("dbo.PageShortLink", "Url", c => c.String(nullable: false, maxLength: 200));
        }
    }
}
