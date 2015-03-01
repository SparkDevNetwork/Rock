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
    public partial class AttendanceReminderJob : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // NA: Update SampleData block attribute for 3.0 changes
            RockMigrationHelper.AddBlockAttributeValue( "34CA1FA0-F8F1-449F-9788-B5E6315DC058", "5E26439E-4E98-45B1-B19B-D5B2F3405963", @"http://storage.rockrms.com/sampledata/sampledata_1_3_0.xml" );

            // JE: Admin Checklist Item
            RockMigrationHelper.AddDefinedValue( "4BF34677-37E9-4E71-BD03-252B66C9373D", "Update External Applications", @"The following external applications have been updated in v3.0. You can download the installers under <span class=""navigation-tip"">Admins Tools > Power Tools > External Applications</span>.
<ul>
<li>Rock Check Scanner</li>
<li>Rock Statement Generator</li>
<li>Rock Windows Check-in Client</li>
</ul>", "CC9B250C-859D-3899-4730-9C5EFAEC504C" );

            // Increase the size of attribute key columns
            DropIndex("dbo.AttributeQualifier", "IX_AttributeIdKey");
            AlterColumn("dbo.AttributeQualifier", "Key", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.Attribute", "Key", c => c.String(nullable: false, maxLength: 200));
            CreateIndex("dbo.AttributeQualifier", new[] { "AttributeId", "Key" }, unique: true, name: "IX_AttributeIdKey");

            // Add page for managing system email categories
            RockMigrationHelper.AddPage( "199DC522-F4D6-4D82-AF44-3C16EE9D2CDA", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "System Email Categories", "", "B55323CD-F494-43E7-97BF-4E13DAB58E0B", "fa fa-folder" ); // Site:Rock RMS
            // Add Block to Page: System Email Categories, Site: Rock RMS
            RockMigrationHelper.AddBlock( "B55323CD-F494-43E7-97BF-4E13DAB58E0B", "", "620FC4A2-6587-409F-8972-22065919D9AC", "Categories", "Main", "", "", 0, "550D7229-2788-4C0E-BFE6-4AAE95D28267" );
            // Attrib Value for Block:Categories, Attribute:Entity Type Page: System Email Categories, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "550D7229-2788-4C0E-BFE6-4AAE95D28267", "C405A507-7889-4287-8342-105B89710044", @"b21fd119-893e-46c0-b42d-e4cdd5c8c49d" );

            // Set the system email category page to not show name in breadcrumbs
            Sql( @"
    UPDATE [Page]
    SET [BreadCrumbDisplayName] = 0
    WHERE [Guid] = 'B55323CD-F494-43E7-97BF-4E13DAB58E0B'
" );

            // Update guid and icons for all system email categories
            RockMigrationHelper.UpdateCategoryByName( "B21FD119-893E-46C0-B42D-E4CDD5C8C49D", "Finance", "fa fa-money", "", "673D13E6-0161-4AC2-B265-DF3783DE3B41", 0 );
            RockMigrationHelper.UpdateCategoryByName( "B21FD119-893E-46C0-B42D-E4CDD5C8C49D", "Groups", "fa fa-users", "", "B31064D2-F2EF-43AA-8BEA-14DF257CBC59", 1 );
            RockMigrationHelper.UpdateCategoryByName( "B21FD119-893E-46C0-B42D-E4CDD5C8C49D", "Security", "fa fa-lock", "", "AEB302FF-A40B-4EDF-9F4E-7E4292C03A47", 2 );
            RockMigrationHelper.UpdateCategoryByName( "B21FD119-893E-46C0-B42D-E4CDD5C8C49D", "System", "fa fa-wrench", "", "9AF1FA93-089B-44B7-83A3-48E0031CCC1D", 3 );
            RockMigrationHelper.UpdateCategoryByName( "B21FD119-893E-46C0-B42D-E4CDD5C8C49D", "Workflow", "fa fa-cogs", "", "C7B9B5F1-9D90-485F-93E4-5D7D81EC2B12", 4 );

            // Add the system email for group reminders
            RockMigrationHelper.UpdateSystemEmail( "Groups", "Group Attendance Reminder", "", "", "", "", "", "Group Attendance", @"
{% capture today %}{{ 'Now' | Date:'yyyyMMdd' }}{% endcapture %}
{% capture occurrenceDate %}{{ Occurrence | Date:'yyyyMMdd' }}{% endcapture %}
{% capture attendanceLink %}{{ GlobalAttribute.PublicApplicationRoot }}page/<<AttendanceDetailPage>>?GroupId={{ Group.Id }}&Occurrence={{ Occurrence | Date:'yyyy-MM-ddTHH\%3amm\%3ass' }}{% endcapture %}
{{ GlobalAttribute.EmailHeader }}

{{ Person.NickName }},<br/>
<br/>
Please remember to enter attendance for your group meeting {% if today == occurrenceDate %}today{% else %}on {{ Occurrence | Date:'dddd' }}{% endif %}.<br/>
<br/>
Thank-you!<br/>
<br/>
<table align=""left"" style=""width: 29%; min-width: 190px; margin-bottom: 12px;"" cellpadding=""0"" cellspacing=""0"">
 <tr>
   <td>

		<div><!--[if mso]>
		  <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""{{ attendanceLink }}"" style=""height:38px;v-text-anchor:middle;width:175px;"" arcsize=""11%"" strokecolor=""#e76812"" fillcolor=""#ee7624"">
			<w:anchorlock/>
			<center style=""color:#ffffff;font-family:sans-serif;font-size:13px;font-weight:normal;"">Enter Attendance</center>
		  </v:roundrect>
		<![endif]--><a href=""{{ attendanceLink }}""
		style=""background-color:#ee7624;border:1px solid #e76812;border-radius:4px;color:#ffffff;display:inline-block;font-family:sans-serif;font-size:13px;font-weight:normal;line-height:38px;text-align:center;text-decoration:none;width:175px;-webkit-text-size-adjust:none;mso-hide:all;"">Enter Attendance</a></div>

	</td>
 </tr>
</table>

{{ GlobalAttribute.EmailFooter }}
", "ED567FDE-A3B4-4827-899D-C2740DF3E5DA" );

            // Update the system email to have correct page id
            Sql( @"
    DECLARE @PageId int = ( SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '0C00CD89-BF4C-4B19-9B0D-E1FA2CFF5DD7' )
    UPDATE [SystemEmail] SET [Body] = REPLACE( [Body], '<<AttendanceDetailPage>>', CAST(@PageId as varchar) )
    WHERE [Guid] = 'ED567FDE-A3B4-4827-899D-C2740DF3E5DA'
" );
            // Add attributes for the send reminder job
            RockMigrationHelper.AddEntityAttribute( "Rock.Model.ServiceJob", "18E29E23-B43B-4CF7-AE41-C85672C09F50", "Class", "Rock.Jobs.SendAttendanceReminder",
                "Group Type", "", "The Group type to send attendance reminders for.", 0, "50FCFB30-F51A-49DF-86F4-2B176EA1820B", "F48F7422-E128-48F9-B429-027C573119FB" );
            RockMigrationHelper.AddEntityAttribute( "Rock.Model.ServiceJob", "08F3003B-F3E2-41EC-BDF1-A2B7AC2908CF", "Class", "Rock.Jobs.SendAttendanceReminder",
                "System Email", "", "The system email to use when sending reminder.", 1, "ED567FDE-A3B4-4827-899D-C2740DF3E5DA", "2A9A7991-D1F5-43CB-9566-552F6D2A6D95" );
            RockMigrationHelper.AddEntityAttribute( "Rock.Model.ServiceJob", "18E29E23-B43B-4CF7-AE41-C85672C09F50", "Class", "Rock.Jobs.SendAttendanceReminder",
                "Send Reminders", "", "Comma delimited list of days after a group meets to send an additional reminder. For example, a value of '2,4' would result in an additional reminder getting sent two and four days after group meets is attendance was not entered.", 2, "", "069A5B85-75B5-4F5F-86A7-9602B1760FE5" );

            // Add the job and it's attribute values
            Sql( @"  
    DECLARE @EntityId int
    DECLARE @AttributeId int

    INSERT INTO [ServiceJob]
        ([IsSystem]
        ,[IsActive]
        ,[Name]
        ,[Description]
        ,[Class]
        ,[CronExpression]
        ,[NotificationStatus]
        ,[Guid])
     VALUES
        (0
        ,1
        ,'Send Attendance Reminders'
        ,'Sends a reminder to group leaders about entering attendance for their group meeting.'
        ,'Rock.Jobs.SendAttendanceReminder'
        ,'0 0 16 1/1 * ? *'
        ,3
        ,'88674EB2-3ABC-4C1B-94B1-6E2E1EDF9BB9')
    SET @EntityId = SCOPE_IDENTITY()

    SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'F48F7422-E128-48F9-B429-027C573119FB')
    INSERT INTO [AttributeValue] ( [IsSystem],[AttributeId],[EntityId],[Value],[Guid] )
    VALUES ( 1, @AttributeId, @EntityId, '50fcfb30-f51a-49df-86f4-2b176ea1820b', NEWID() )

    SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '2A9A7991-D1F5-43CB-9566-552F6D2A6D95')
    INSERT INTO [AttributeValue] ( [IsSystem],[AttributeId],[EntityId],[Value],[Guid] )
    VALUES ( 1, @AttributeId, @EntityId, 'ed567fde-a3b4-4827-899d-c2740df3e5da', NEWID() )

    SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '069A5B85-75B5-4F5F-86A7-9602B1760FE5')
    INSERT INTO [AttributeValue] ( [IsSystem],[AttributeId],[EntityId],[Value],[Guid] )
    VALUES ( 1, @AttributeId, @EntityId, '2,4', NEWID() )
" );
        }


        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( "DELETE [ServiceJob] WHERE [Guid] = '88674EB2-3ABC-4C1B-94B1-6E2E1EDF9BB9'" );

            RockMigrationHelper.DeleteAttribute( "069A5B85-75B5-4F5F-86A7-9602B1760FE5" );
            RockMigrationHelper.DeleteAttribute( "2A9A7991-D1F5-43CB-9566-552F6D2A6D95" );
            RockMigrationHelper.DeleteAttribute( "F48F7422-E128-48F9-B429-027C573119FB" );

            RockMigrationHelper.DeleteSystemEmail( "ED567FDE-A3B4-4827-899D-C2740DF3E5DA" );

            // Remove Block: Categories, from Page: System Email Categories, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "550D7229-2788-4C0E-BFE6-4AAE95D28267" );
            RockMigrationHelper.DeletePage( "B55323CD-F494-43E7-97BF-4E13DAB58E0B" ); //  Page: System Email Categories, Layout: Full Width, Site: Rock RMS

            DropIndex("dbo.AttributeQualifier", "IX_AttributeIdKey");
            AlterColumn("dbo.Attribute", "Key", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.AttributeQualifier", "Key", c => c.String(nullable: false, maxLength: 50));
            CreateIndex("dbo.AttributeQualifier", new[] { "AttributeId", "Key" }, unique: true, name: "IX_AttributeIdKey");
        }
    }
}
