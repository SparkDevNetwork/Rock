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
    using System.Linq;
    using Data;
    using Model;
    using Rock;
    /// <summary>
    ///
    /// </summary>
    public partial class DiscEnhancements : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            var rockContext = new RockContext();

            var discWorkflowGuid = new Guid( "885CBA61-44EA-4B4A-B6E1-289041B6A195" );
            var discWorkflowNotEdited = new WorkflowTypeService( rockContext ).Queryable()
                                                .Where( w => w.Guid == discWorkflowGuid && w.ModifiedDateTime == null )
                                                .Any();

            if ( discWorkflowNotEdited )
            {
                // add person attribute to store last DISC request date
                RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DATE, "0B187C81-2106-4875-82B6-FBF1277AE23B", "Last DISC Request Date", "LastDiscRequestDate", "fa fa-bar-chart", "The date the last DISC request was sent.", 9, string.Empty, "41B73365-A984-879E-4749-7DB4FC720138" );


                RockMigrationHelper.UpdateWorkflowTypeAttribute( "885CBA61-44EA-4B4A-B6E1-289041B6A195", "9C204CD0-1233-41C5-818A-C5DA439445AA", "No Email Warning", "NoEmailWarning", "Warning message when the person does not have an email address.", 5, @"", "BB2B3CF8-7C81-4EBD-9589-A961E341F70C" ); // DISC Request:No Email Warning}


                RockMigrationHelper.UpdateWorkflowActionType( "4A8E2E61-FE0D-4AB3-BA48-9419A20E4541", "Set No Email Warning", 5, "A41216D6-6FB0-4019-B222-2C29B4519CF4", true, false, "", "", 1, "", "940E88E7-1294-4DD7-A626-E1345A41A2D1" ); // DISC Request:Launch From Person Profile:Set No Email Warning
                RockMigrationHelper.AddActionTypeAttributeValue( "940E88E7-1294-4DD7-A626-E1345A41A2D1", "FA7C685D-8636-41EF-9998-90FFF3998F76", @"" ); // DISC Request:Launch From Person Profile:Set No Email Warning:Order
                RockMigrationHelper.AddActionTypeAttributeValue( "940E88E7-1294-4DD7-A626-E1345A41A2D1", "F3B9908B-096F-460B-8320-122CF046D1F9", @"DECLARE @PersonAliasGuid uniqueidentifier = '{{ Workflow.Person_unformatted }}'

SELECT  CASE
    WHEN EXISTS ( SELECT 1
        FROM [Person] P
        INNER JOIN [PersonAlias] PA ON PA.[Guid] = @PersonAliasGuid AND P.[Id] = PA.[PersonId]
        WHERE P.[IsEmailActive] <> 0 AND P.[Email] IS NOT NULL AND P.[Email] != '' )
    THEN 'True'
    ELSE 'False'
    END" ); // DISC Request:Launch From Person Profile:Set No Email Warning:SQLQuery
                RockMigrationHelper.AddActionTypeAttributeValue( "940E88E7-1294-4DD7-A626-E1345A41A2D1", "A18C3143-0586-4565-9F36-E603BC674B4E", @"False" ); // DISC Request:Launch From Person Profile:Set No Email Warning:Active
                RockMigrationHelper.AddActionTypeAttributeValue( "940E88E7-1294-4DD7-A626-E1345A41A2D1", "56997192-2545-4EA1-B5B2-313B04588984", @"bb2b3cf8-7c81-4ebd-9589-a961e341f70c" ); // DISC Request:Launch From Person Profile:Set No Email Warning:Result Attribute


                RockMigrationHelper.UpdateWorkflowActionType( "4A8E2E61-FE0D-4AB3-BA48-9419A20E4541", "Set No Email Warning Message", 6, "C789E457-0783-44B3-9D8F-2EBAB5F11110", true, false, "", "BB2B3CF8-7C81-4EBD-9589-A961E341F70C", 1, "False", "3FAC4206-D572-4BB7-84A8-9FF817C96302" ); // DISC Request:Launch From Person Profile:Set No Email Warning Message
                RockMigrationHelper.AddActionTypeAttributeValue( "3FAC4206-D572-4BB7-84A8-9FF817C96302", "57093B41-50ED-48E5-B72B-8829E62704C8", @"" ); // DISC Request:Launch From Person Profile:Set No Email Warning Message:Order
                RockMigrationHelper.AddActionTypeAttributeValue( "3FAC4206-D572-4BB7-84A8-9FF817C96302", "D7EAA859-F500-4521-9523-488B12EAA7D2", @"False" ); // DISC Request:Launch From Person Profile:Set No Email Warning Message:Active
                RockMigrationHelper.AddActionTypeAttributeValue( "3FAC4206-D572-4BB7-84A8-9FF817C96302", "44A0B977-4730-4519-8FF6-B0A01A95B212", @"bb2b3cf8-7c81-4ebd-9589-a961e341f70c" ); // DISC Request:Launch From Person Profile:Set No Email Warning Message:Attribute
                RockMigrationHelper.AddActionTypeAttributeValue( "3FAC4206-D572-4BB7-84A8-9FF817C96302", "E5272B11-A2B8-49DC-860D-8D574E2BC15C", @"<div class=""alert alert-warning margin-t-sm"">{{ Workflow.Person }} does not have an active email address. Please add an address to their record.</div>" ); // DISC Request:Launch From Person Profile:Set No Email Warning Message:Text Value|Attribute Value

                RockMigrationHelper.UpdateWorkflowActionType( "4A8E2E61-FE0D-4AB3-BA48-9419A20E4541", "Update Request Person Attribute", 9, "320622DA-52E0-41AE-AF90-2BF78B488552", true, false, "", "", 1, "", "42401767-F313-4AAE-BE46-AD557F970847" ); // DISC Request:Launch From Person Profile:Update Request Person Attribute
                RockMigrationHelper.AddActionTypeAttributeValue( "42401767-F313-4AAE-BE46-AD557F970847", "E5BAC4A6-FF7F-4016-BA9C-72D16CB60184", @"False" ); // DISC Request:Launch From Person Profile:Update Request Person Attribute:Active
                RockMigrationHelper.AddActionTypeAttributeValue( "42401767-F313-4AAE-BE46-AD557F970847", "E456FB6F-05DB-4826-A612-5B704BC4EA13", @"c0bc984c-84c3-494b-a861-49840e452f68" ); // DISC Request:Launch From Person Profile:Update Request Person Attribute:Person
                RockMigrationHelper.AddActionTypeAttributeValue( "42401767-F313-4AAE-BE46-AD557F970847", "3F3BF3E6-AD53-491E-A40F-441F2AFCBB5B", @"" ); // DISC Request:Launch From Person Profile:Update Request Person Attribute:Order
                RockMigrationHelper.AddActionTypeAttributeValue( "42401767-F313-4AAE-BE46-AD557F970847", "8F4BB00F-7FA2-41AD-8E90-81F4DFE2C762", @"41b73365-a984-879e-4749-7db4fc720138" ); // DISC Request:Launch From Person Profile:Update Request Person Attribute:Person Attribute
                RockMigrationHelper.AddActionTypeAttributeValue( "42401767-F313-4AAE-BE46-AD557F970847", "94689BDE-493E-4869-A614-2D54822D747C", @"{{ 'Now' | Date:'M/d/yyyy' }}" ); // DISC Request:Launch From Person Profile:Update Request Person Attribute:Value|Attribute Value

                // reorder actions
                Sql( @"   
-- set no email warning flag
  UPDATE  [WorkflowActionType]
	SET [Order] = 6
    WHERE [Guid] = '940E88E7-1294-4DD7-A626-E1345A41A2D1'

  -- set no email warning message
  UPDATE  [WorkflowActionType]
	SET [Order] = 7
    WHERE [Guid] = '3FAC4206-D572-4BB7-84A8-9FF817C96302'

  -- custom messgae
  UPDATE  [WorkflowActionType]
	SET [Order] = 8
    WHERE [Guid] = '1A08A4EC-B2C6-43B5-926F-2F86CFA35102'

  -- persist workflow
  UPDATE  [WorkflowActionType]
	SET [Order] = 9
    WHERE [Guid] = '555A729C-5EC4-4C83-B35F-036234E5EFCC'

-- send email
UPDATE  [WorkflowActionType]
	SET [Order] = 10
    WHERE [Guid] = '666FC137-BC95-49BE-A976-0BFF2417F44C'

-- update person attribute
UPDATE  [WorkflowActionType]
	SET [Order] = 11
    WHERE [Guid] = '42401767-F313-4AAE-BE46-AD557F970847'

-- workflow complete
UPDATE  [WorkflowActionType]
	SET [Order] = 12
    WHERE [Guid] = 'EA4FF8DD-0E66-4C98-84FC-845DEAB76A61'" );


                // update form header with new warning message
                Sql( @"  UPDATE [WorkflowActionForm]
  SET [Header] = 'Hi &#123;&#123; Person.NickName &#125;&#125;!

{{ Workflow.WarningMessage }}
{{ Workflow.NoEmailWarning }}'
  WHERE [Guid] = '4AFAB342-D584-4B79-B038-A99C0C469D74'" );

            }
        }
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // add person attribute to store last DISC request date
            RockMigrationHelper.DeleteAttribute( "41B73365-A984-879E-4749-7DB4FC720138" );

            RockMigrationHelper.DeleteAttribute( "BB2B3CF8-7C81-4EBD-9589-A961E341F70C" );

            RockMigrationHelper.DeleteWorkflowActionType( "940E88E7-1294-4DD7-A626-E1345A41A2D1" );
            RockMigrationHelper.DeleteWorkflowActionType( "3FAC4206-D572-4BB7-84A8-9FF817C96302" );
            RockMigrationHelper.DeleteWorkflowActionType( "42401767-F313-4AAE-BE46-AD557F970847" );

            // put old message back
            Sql( @"UPDATE[WorkflowActionForm]
  SET[Header] = 'Hi &#123;&#123; Person.NickName &#125;&#125;!

{{ Workflow.WarningMessage }}'
  WHERE[Guid] = '4AFAB342-D584-4B79-B038-A99C0C469D74'" );
        }
    }
}
