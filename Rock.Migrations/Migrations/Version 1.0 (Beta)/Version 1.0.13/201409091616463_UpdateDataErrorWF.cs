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
    public partial class UpdateDataErrorWF : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add Organization abbreviation global attribute
            RockMigrationHelper.AddGlobalAttribute( "9C204CD0-1233-41C5-818A-C5DA439445AA", "", "", "Organization Abbreviation", "The abbreviated name of your organization", 0, "", "555306F1-6117-48B9-B184-D48DC1EC445F" );

            // Add WorkflowTypeDetail block attributes for manage page and launch page
            RockMigrationHelper.AddBlockTypeAttribute( "E1FF677D-5E52-4259-90C7-5560ECBBD82B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Manage Workflows Page", "ManageWorkflowsPage", "", "Page used to manage workflows.", 0, "", "9E710496-17F7-4F22-9DD1-3A7563E9E906", true );
            RockMigrationHelper.AddBlockTypeAttribute( "E1FF677D-5E52-4259-90C7-5560ECBBD82B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Workflow Launch Page", "WorkflowLaunchPage", "", "Page used to launch a workflow.", 0, "", "8023344B-C247-4A23-A191-880ABF391A34", true );
            RockMigrationHelper.AddBlockAttributeValue( "2C330A26-1A1C-4B36-80FA-4CB96198F985", "9E710496-17F7-4F22-9DD1-3A7563E9E906", "61e1b4b6-eace-42e8-a2fb-37465e6d0004" );
            RockMigrationHelper.AddBlockAttributeValue( "2C330A26-1A1C-4B36-80FA-4CB96198F985", "8023344B-C247-4A23-A191-880ABF391A34", "5da89bc9-a185-4749-a843-314b72170d82,e1471bfe-17dc-483f-af8f-7dcc4f168948" );

            // Update SetName attribute to indicate supports liquid
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "36005473-BD5D-470B-B28D-98E6D7ED808D", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Text Value|Attribute Value", "NameValue", "The value to use for the workflow's name. <span class='tip tip-lava'></span>", 1, @"", "93852244-A667-4749-961A-D47F88675BE4" ); // Rock.Workflow.Action.SetWorkflowName:Text Value|Attribute Value

            // SendEmail action updated attributes
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "From Email Address|Attribute Value", "From", "The email address or an attribute that contains the person or email address that email should be sent from (will default to organization email). <span class='tip tip-lava'></span>", 0, @"", "9F5F7CEC-F369-4FDF-802A-99074CE7A7FC" ); // Rock.Workflow.Action.SendEmail:From Email Address|Attribute Value
            RockMigrationHelper.DeleteAttribute( "15B0A910-3335-438F-A761-88939BDD3BF7" );
            RockMigrationHelper.DeleteAttribute( "985E9A65-CD7B-42F5-9589-A25A9BF23B91" );

            // Update Person Data Error workflow type so that it only has one entry activity type
            Sql( @"
    DELETE [WorkflowActivity]
    WHERE [ActivityTypeId] IN (
        SELECT [Id] FROM [WorkflowActivityType] 
        WHERE GUID = '6AA06B90-5BC1-464B-A660-C481853C1B32'
    )
    DELETE [WorkflowActivityType] WHERE GUID = '6AA06B90-5BC1-464B-A660-C481853C1B32'

" );
            RockMigrationHelper.UpdateWorkflowActivityType( "221BF486-A82C-40A7-85B7-BB44DA45582F", true, "Entry", "Prompts user for specific details about the data error on the selected person's profile.", true, 0, "F238E2D8-AAE0-481F-BABF-955C0A3343CC" ); // Person Data Error:Entry
            RockMigrationHelper.UpdateWorkflowActivityType( "221BF486-A82C-40A7-85B7-BB44DA45582F", true, "Pending", "Reports the error to the person responsible for addressing data errors on person profile records and waits for them to complete the resolution.", false, 1, "31D6A610-9F0B-4136-8FEA-8F4E192D75B8" ); // Person Data Error:Pending
            RockMigrationHelper.UpdateWorkflowActivityType( "221BF486-A82C-40A7-85B7-BB44DA45582F", true, "Complete", "Notifies the person who reported the error that issue has been completed, and marks the workflow complete.", false, 2, "E9F9E0BE-D38B-4017-954A-5B15C1EED2DE" ); // Person Data Error:Complete

            RockMigrationHelper.UpdateWorkflowActionType( "F238E2D8-AAE0-481F-BABF-955C0A3343CC", "Set the Reported By value", 2, "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", true, false, "", "", 1, "", "D56341AE-7A81-445F-B2CC-21C84A767D6C" ); // Person Data Error:Entry:Set the Reported By value
            RockMigrationHelper.UpdateWorkflowActionType( "F238E2D8-AAE0-481F-BABF-955C0A3343CC", "Enter Details", 3, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, false, "CB8C9C9D-C81D-4B31-92CA-3BCEAE1DBA87", "", 1, "", "341569AB-76F6-44E8-A033-CE29FDE8035F" ); // Person Data Error:Entry:Enter Details
            RockMigrationHelper.UpdateWorkflowActionType( "F238E2D8-AAE0-481F-BABF-955C0A3343CC", "Persist the Workflow", 4, "F1A39347-6FE0-43D4-89FB-544195088ECF", true, false, "", "", 1, "", "5169CBED-08DF-4ABD-91AB-83A2637C5515" ); // Person Data Error:Entry:Persist the Workflow
            RockMigrationHelper.UpdateWorkflowActionType( "F238E2D8-AAE0-481F-BABF-955C0A3343CC", "Set Person", 0, "972F19B9-598B-474B-97A4-50E56E7B59D2", true, false, "", "", 1, "", "12999084-68F8-4A36-AFB2-B851DE391DF1" ); // Person Data Error:Entry:Set Person
            RockMigrationHelper.UpdateWorkflowActionType( "F238E2D8-AAE0-481F-BABF-955C0A3343CC", "Persist Workflow", 1, "F1A39347-6FE0-43D4-89FB-544195088ECF", true, false, "", "", 1, "", "28DA965F-BE88-4BE1-BF81-7863DD65BDF4" ); // Person Data Error:Entry:Persist Workflow

            RockMigrationHelper.AddActionTypeAttributeValue( "12999084-68F8-4A36-AFB2-B851DE391DF1", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1", @"False" ); // Person Data Error:Entry:Set Person:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "12999084-68F8-4A36-AFB2-B851DE391DF1", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7", @"fd89f2c8-cbc8-4ed1-96d1-891ab9616c9e" ); // Person Data Error:Entry:Set Person:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "12999084-68F8-4A36-AFB2-B851DE391DF1", "AD4EFAC4-E687-43DF-832F-0DC3856ABABB", @"" ); // Person Data Error:Entry:Set Person:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "28DA965F-BE88-4BE1-BF81-7863DD65BDF4", "50B01639-4938-40D2-A791-AA0EB4F86847", @"False" ); // Person Data Error:Entry:Persist Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "28DA965F-BE88-4BE1-BF81-7863DD65BDF4", "86F795B0-0CB6-4DA4-9CE4-B11D0922F361", @"" ); // Person Data Error:Entry:Persist Workflow:Order

            #region Pending Individual Report

            Sql( @"
    -----------------------------------------------------------------------------------------------
    -- START script for Report: Pending Individuals
    -----------------------------------------------------------------------------------------------
    -- add Report: Pending Individuals
    DECLARE @CategoryId INT = (
            SELECT [Id]
            FROM [Category]
            WHERE [Guid] = 'D738D12D-BC3B-47B0-8A90-F7924D137595' /* Data Integrity */
            )
        ,@DataViewId INT = (
            SELECT [Id]
            FROM [DataView]
            WHERE [Guid] = '6374C50D-4BBF-4B12-AA2C-9C2C92E5F32E' /* Pending People */
            )
        ,@EntityTypeId INT = (
            SELECT TOP 1 [Id]
            FROM [EntityType]
            WHERE [Guid] = '72657ED8-D16E-492E-AC12-144C5E7567E7'
            ) /* Rock.Model.Person */
        ,@PendingIndividualReportGuid UNIQUEIDENTIFIER = '4E3ECAE0-9D36-4C22-994D-AD31DE0F6FB7'

    INSERT INTO [Report] (
        [IsSystem]
        ,[Name]
        ,[Description]
        ,[CategoryId]
        ,[EntityTypeId]
        ,[DataViewId]
        ,[Guid]
        ,[FetchTop]
        )
    VALUES (
        0
        ,N'Pending Individuals'
        ,N'People who have have a status of pending.'
        ,@CategoryId
        ,@EntityTypeId
        ,@DataViewId
        ,@PendingIndividualReportGuid
        ,NULL
        )

    DECLARE @ReportId INT = SCOPE_IDENTITY()

    INSERT INTO [dbo].[ReportField] (
        [ReportId]
        ,[ReportFieldType]
        ,[ShowInGrid]
        ,[DataSelectComponentEntityTypeId]
        ,[Selection]
        ,[Order]
        ,[Guid]
        ,[ColumnHeaderText]
        )
    VALUES (
        @ReportId
        ,0
        ,1
        ,NULL
        ,'LastName'
        ,0
        ,'B6DC8215-B7EF-4928-BCCA-DB35CC1C97F8'
        ,'Last Name'
        )
        ,(
        @ReportId
        ,0
        ,1
        ,NULL
        ,'NickName'
        ,1
        ,'47AA0671-F1BF-4864-8007-A5DE9512EF0C'
        ,'Nick Name'
        )
        ,(
        @ReportId
        ,0
        ,1
        ,NULL
        ,'CreatedDateTime'
        ,2
        ,'AADE6679-0B54-4CCB-BD5F-6486FA833A1A'
        ,'Created Date Time'
        )
    -----------------------------------------------------------------------------------------------
    -- END script for Report: Pending Individuals
    -----------------------------------------------------------------------------------------------
" );

            #endregion

            #region Set Report Security

            // Set auth on Rock.Reporting.DataSelect.Person.FirstContributionSelect to only allow view to Finance Admins, Finance Users and Rock Admins
            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Reporting.DataSelect.Person.FirstContributionSelect ).FullName,
                0,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "C58E5152-2330-4D34-9C53-606554341D53" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Reporting.DataSelect.Person.FirstContributionSelect ).FullName,
                1,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_FINANCE_USERS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "43C4483B-3266-4E0B-9CCE-FFAA1EBFC519" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Reporting.DataSelect.Person.FirstContributionSelect ).FullName,
                2,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_ADMINISTRATORS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "CC03214B-5F01-4619-A62E-45A4C8A2E766" );

            // deny everybody else
            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Reporting.DataSelect.Person.FirstContributionSelect ).FullName,
                3,
                Rock.Security.Authorization.VIEW,
                false,
                null,
                Rock.Model.SpecialRole.AllUsers.ConvertToInt(),
                "486E3BA8-9FD9-4FA3-96B5-FDAB8FD156F0" );

            // Set auth on Rock.Reporting.DataSelect.Person.LastContributionSelect to only allow view to Finance Admins, Finance Users and Rock Admins
            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Reporting.DataSelect.Person.LastContributionSelect ).FullName,
                0,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "5F45DB5F-3196-4458-A1FF-2517A41482E9" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Reporting.DataSelect.Person.LastContributionSelect ).FullName,
                1,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_FINANCE_USERS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "8CCCAB09-741E-4A61-93B0-E31028E0AC43" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Reporting.DataSelect.Person.LastContributionSelect ).FullName,
                2,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_ADMINISTRATORS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "EB3F90C2-4A44-4B42-A296-61D61C4D1183" );

            // deny everybody else
            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Reporting.DataSelect.Person.LastContributionSelect ).FullName,
                3,
                Rock.Security.Authorization.VIEW,
                false,
                null,
                Rock.Model.SpecialRole.AllUsers.ConvertToInt(),
                "0F939CC5-F0E2-4DCB-AB5F-6E5111801291" );


            // Set auth on Rock.Reporting.DataSelect.Person.TotalGivingAmountSelect to only allow view to Finance Admins, Finance Users and Rock Admins
            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Reporting.DataSelect.Person.TotalGivingAmountSelect ).FullName,
                0,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "5BB65E0E-49CF-45B7-984B-9B0228000590" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Reporting.DataSelect.Person.TotalGivingAmountSelect ).FullName,
                1,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_FINANCE_USERS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "8E5A555A-EAB1-4C46-A270-62D8633264D5" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Reporting.DataSelect.Person.TotalGivingAmountSelect ).FullName,
                2,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_ADMINISTRATORS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "BB4F04BC-F984-4F0D-BAD9-F1162970E669" );

            // deny everybody else
            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Reporting.DataSelect.Person.TotalGivingAmountSelect ).FullName,
                3,
                Rock.Security.Authorization.VIEW,
                false,
                null,
                Rock.Model.SpecialRole.AllUsers.ConvertToInt(),
                "C4A6BE0B-48ED-4901-9EC4-F4ED1D84DBA6" );

            // Set auth on Rock.Reporting.DataFilter.Person.FirstContributionDateFilter to only allow view to Finance Admins, Finance Users and Rock Admins
            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Reporting.DataFilter.Person.FirstContributionDateFilter ).FullName,
                0,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "71DDE91A-B4F6-4698-B9E3-0EF4D9097745" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Reporting.DataFilter.Person.FirstContributionDateFilter ).FullName,
                1,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_FINANCE_USERS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "404BD7D1-EC18-473E-94A4-00B22AAAD19F" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Reporting.DataFilter.Person.FirstContributionDateFilter ).FullName,
                2,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_ADMINISTRATORS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "DB6409C7-7200-4505-B87B-CAE47FF02CD9" );

            // deny everybody else
            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Reporting.DataFilter.Person.FirstContributionDateFilter ).FullName,
                3,
                Rock.Security.Authorization.VIEW,
                false,
                null,
                Rock.Model.SpecialRole.AllUsers.ConvertToInt(),
                "AC650DF8-579F-4262-98B5-2B27A24D2101" );

            // Set auth on Rock.Reporting.DataFilter.Person.GivingAmountFilter to only allow view to Finance Admins, Finance Users and Rock Admins
            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Reporting.DataFilter.Person.GivingAmountFilter ).FullName,
                0,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "933C0B3C-9E96-484A-B8ED-F5EF205291D4" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Reporting.DataFilter.Person.GivingAmountFilter ).FullName,
                1,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_FINANCE_USERS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "08C1964E-F2BC-4DC4-B80D-17E720E921E3" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Reporting.DataFilter.Person.GivingAmountFilter ).FullName,
                2,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_ADMINISTRATORS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "35738ED0-6A9C-497E-955C-EFBF0BA00100" );

            // deny everybody else
            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Reporting.DataFilter.Person.GivingAmountFilter ).FullName,
                3,
                Rock.Security.Authorization.VIEW,
                false,
                null,
                Rock.Model.SpecialRole.AllUsers.ConvertToInt(),
                "2C782356-FCCA-49CA-9B76-53A9AF5B1ECA" );

            #endregion

            #region Sample Workflow

            RockMigrationHelper.UpdateEntityType("Rock.Model.Workflow", "3540E9A7-FE30-43A9-8B0A-A372B63DFC93", true, true);
            RockMigrationHelper.UpdateEntityType("Rock.Model.WorkflowActivity", "2CB52ED0-CB06-4D62-9E2C-73B60AFA4C9F", true, true);
            RockMigrationHelper.UpdateEntityType("Rock.Model.WorkflowActionType", "23E3273A-B137-48A3-9AFF-C8DC832DDCA6", true, true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.ActivateActivity","38907A90-1634-4A93-8017-619326A4A582",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.AssignActivityFromAttributeValue","F100A31F-E93A-4C7A-9E55-0FAF41A101C4",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.AssignActivityToPerson","FB2981B7-7922-42E1-8ACF-7F63BB7989E6",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.CompleteWorkflow","EEDA4318-F014-4A46-9C76-4C052EF81AA1",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.PersistWorkflow","F1A39347-6FE0-43D4-89FB-544195088ECF",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.SendEmail","66197B01-D1F0-4924-A315-47AD54E030DE",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.SetAttributeToCurrentPerson","24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.SetAttributeValue","C789E457-0783-44B3-9D8F-2EBAB5F11110",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.SetWorkflowName","36005473-BD5D-470B-B28D-98E6D7ED808D",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.UserEntryForm","486DC4FA-FCBC-425F-90B0-E606DA8A9F68",false,true);
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","DE9CB292-4785-4EA3-976D-3826F91E9E98"); // Rock.Workflow.Action.SetAttributeToCurrentPerson:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A","33E6DF69-BDFA-407A-9744-C175B60643AE","Person Attribute","PersonAttribute","The attribute to set to the currently logged in person.",0,@"","BBED8A83-8BB2-4D35-BAFB-05F67DCAD112"); // Rock.Workflow.Action.SetAttributeToCurrentPerson:Person Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","89E9BCED-91AB-47B0-AD52-D78B0B7CB9E8"); // Rock.Workflow.Action.SetAttributeToCurrentPerson:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("36005473-BD5D-470B-B28D-98E6D7ED808D","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","0A800013-51F7-4902-885A-5BE215D67D3D"); // Rock.Workflow.Action.SetWorkflowName:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("36005473-BD5D-470B-B28D-98E6D7ED808D","3B1D93D7-9414-48F9-80E5-6A3FC8F94C20","Text Value|Attribute Value","NameValue","The value to use for the workflow's name. <span class='tip tip-lava'></span>",1,@"","93852244-A667-4749-961A-D47F88675BE4"); // Rock.Workflow.Action.SetWorkflowName:Text Value|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("36005473-BD5D-470B-B28D-98E6D7ED808D","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","5D95C15A-CCAE-40AD-A9DD-F929DA587115"); // Rock.Workflow.Action.SetWorkflowName:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("38907A90-1634-4A93-8017-619326A4A582","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","E8ABD802-372C-47BE-82B1-96F50DB5169E"); // Rock.Workflow.Action.ActivateActivity:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("38907A90-1634-4A93-8017-619326A4A582","739FD425-5B8C-4605-B775-7E4D9D4C11DB","Activity","Activity","The activity type to activate",0,@"","02D5A7A5-8781-46B4-B9FC-AF816829D240"); // Rock.Workflow.Action.ActivateActivity:Activity
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("38907A90-1634-4A93-8017-619326A4A582","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","3809A78C-B773-440C-8E3F-A8E81D0DAE08"); // Rock.Workflow.Action.ActivateActivity:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("486DC4FA-FCBC-425F-90B0-E606DA8A9F68","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","234910F2-A0DB-4D7D-BAF7-83C880EF30AE"); // Rock.Workflow.Action.UserEntryForm:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("486DC4FA-FCBC-425F-90B0-E606DA8A9F68","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","C178113D-7C86-4229-8424-C6D0CF4A7E23"); // Rock.Workflow.Action.UserEntryForm:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("66197B01-D1F0-4924-A315-47AD54E030DE","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","Body","Body","The body of the email that should be sent. <span class='tip tip-lava'></span> <span class='tip tip-html'></span>",3,@"","4D245B9E-6B03-46E7-8482-A51FBA190E4D"); // Rock.Workflow.Action.SendEmail:Body
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("66197B01-D1F0-4924-A315-47AD54E030DE","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","36197160-7D3D-490D-AB42-7E29105AFE91"); // Rock.Workflow.Action.SendEmail:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("66197B01-D1F0-4924-A315-47AD54E030DE","3B1D93D7-9414-48F9-80E5-6A3FC8F94C20","From Email Address|Attribute Value","From","The email address or an attribute that contains the person or email address that email should be sent from (will default to organization email). <span class='tip tip-lava'></span>",0,@"","9F5F7CEC-F369-4FDF-802A-99074CE7A7FC"); // Rock.Workflow.Action.SendEmail:From Email Address|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("66197B01-D1F0-4924-A315-47AD54E030DE","3B1D93D7-9414-48F9-80E5-6A3FC8F94C20","Send To Email Address|Attribute Value","To","The email address or an attribute that contains the person or email address that email should be sent to",1,@"","0C4C13B8-7076-4872-925A-F950886B5E16"); // Rock.Workflow.Action.SendEmail:Send To Email Address|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("66197B01-D1F0-4924-A315-47AD54E030DE","9C204CD0-1233-41C5-818A-C5DA439445AA","Subject","Subject","The subject that should be used when sending email. <span class='tip tip-lava'></span>",2,@"","5D9B13B6-CD96-4C7C-86FA-4512B9D28386"); // Rock.Workflow.Action.SendEmail:Subject
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("66197B01-D1F0-4924-A315-47AD54E030DE","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","D1269254-C15A-40BD-B784-ADCC231D3950"); // Rock.Workflow.Action.SendEmail:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("C789E457-0783-44B3-9D8F-2EBAB5F11110","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","D7EAA859-F500-4521-9523-488B12EAA7D2"); // Rock.Workflow.Action.SetAttributeValue:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("C789E457-0783-44B3-9D8F-2EBAB5F11110","33E6DF69-BDFA-407A-9744-C175B60643AE","Attribute","Attribute","The attribute to set the value of.",0,@"","44A0B977-4730-4519-8FF6-B0A01A95B212"); // Rock.Workflow.Action.SetAttributeValue:Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("C789E457-0783-44B3-9D8F-2EBAB5F11110","3B1D93D7-9414-48F9-80E5-6A3FC8F94C20","Text Value|Attribute Value","Value","The text or attribute to set the value from. <span class='tip tip-lava'></span>",1,@"","E5272B11-A2B8-49DC-860D-8D574E2BC15C"); // Rock.Workflow.Action.SetAttributeValue:Text Value|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("C789E457-0783-44B3-9D8F-2EBAB5F11110","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","57093B41-50ED-48E5-B72B-8829E62704C8"); // Rock.Workflow.Action.SetAttributeValue:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("EEDA4318-F014-4A46-9C76-4C052EF81AA1","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","0CA0DDEF-48EF-4ABC-9822-A05E225DE26C"); // Rock.Workflow.Action.CompleteWorkflow:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("EEDA4318-F014-4A46-9C76-4C052EF81AA1","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","25CAD4BE-5A00-409D-9BAB-E32518D89956"); // Rock.Workflow.Action.CompleteWorkflow:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("F100A31F-E93A-4C7A-9E55-0FAF41A101C4","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","E0F7AB7E-7761-4600-A099-CB14ACDBF6EF"); // Rock.Workflow.Action.AssignActivityFromAttributeValue:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("F100A31F-E93A-4C7A-9E55-0FAF41A101C4","33E6DF69-BDFA-407A-9744-C175B60643AE","Attribute","Attribute","The person or group attribute value to assign this activity to.",0,@"","FBADD25F-D309-4512-8430-3CC8615DD60E"); // Rock.Workflow.Action.AssignActivityFromAttributeValue:Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("F100A31F-E93A-4C7A-9E55-0FAF41A101C4","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","7A6B605D-7FB1-4F48-AF35-5A0683FB1CDA"); // Rock.Workflow.Action.AssignActivityFromAttributeValue:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("F1A39347-6FE0-43D4-89FB-544195088ECF","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","50B01639-4938-40D2-A791-AA0EB4F86847"); // Rock.Workflow.Action.PersistWorkflow:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("F1A39347-6FE0-43D4-89FB-544195088ECF","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","86F795B0-0CB6-4DA4-9CE4-B11D0922F361"); // Rock.Workflow.Action.PersistWorkflow:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("FB2981B7-7922-42E1-8ACF-7F63BB7989E6","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","0B768E17-C64A-4212-BAD5-8A16B9F05A5C"); // Rock.Workflow.Action.AssignActivityToPerson:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("FB2981B7-7922-42E1-8ACF-7F63BB7989E6","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","5C5F7DB4-51DE-4293-BD73-CABDEB6564AC"); // Rock.Workflow.Action.AssignActivityToPerson:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("FB2981B7-7922-42E1-8ACF-7F63BB7989E6","E4EAB7B2-0B76-429B-AFE4-AD86D7428C70","Person","Person","The person to assign this activity to.",0,@"","7ED2571D-B1BF-4DB6-9D04-9B5D064F51D8"); // Rock.Workflow.Action.AssignActivityToPerson:Person
            RockMigrationHelper.UpdateCategory("C9F3C4A5-1526-474D-803F-D6C7A45CBBAE","Samples","fa fa-star","","CB99421E-9ADC-488E-8C71-94BB14F27F56",0); // Samples
            RockMigrationHelper.UpdateWorkflowType(false,true,"Position Approval","This workflow will be used by the staff of Rock Solid Church to request the opening of a new position on staff.","CB99421E-9ADC-488E-8C71-94BB14F27F56","Position Request","fa fa-user",0,false,3,"655BE2A4-2735-4CF9-AEC8-7EF5BE92724C"); // Position Approval
            RockMigrationHelper.UpdateWorkflowTypeAttribute("655BE2A4-2735-4CF9-AEC8-7EF5BE92724C","C28C7BF3-A552-4D77-9408-DEDCF760CED0","Approval Notes","ApprovalNotes","Optional notes for the approver to provide more details.",9,@"","1701E7CA-0E2D-43C4-B865-6BA4241AF65D"); // Position Approval:Approval Notes
            RockMigrationHelper.UpdateWorkflowTypeAttribute("655BE2A4-2735-4CF9-AEC8-7EF5BE92724C","E4EAB7B2-0B76-429B-AFE4-AD86D7428C70","Approver","Approver","The person who is required to approve the request.",7,@"","1ECDC402-4935-485D-908E-9E88B883D9E9"); // Position Approval:Approver
            RockMigrationHelper.UpdateWorkflowTypeAttribute("655BE2A4-2735-4CF9-AEC8-7EF5BE92724C","9C204CD0-1233-41C5-818A-C5DA439445AA","Approved","ApproverResult","Whether the request was approved.",8,@"Pending","508A909D-CCAD-4646-842A-FBBE3B375DB2"); // Position Approval:Approved
            RockMigrationHelper.UpdateWorkflowTypeAttribute("655BE2A4-2735-4CF9-AEC8-7EF5BE92724C","6F9E2DD0-E39E-4602-ADF9-EB710A75304A","Job Description","JobDescription","Document containing the official job description. You can download the template <a href=''>from the HR Intranet</a>",5,@"","F125EB97-0C1C-4AFB-84DF-0E1A43AF8B31"); // Position Approval:Job Description
            RockMigrationHelper.UpdateWorkflowTypeAttribute("655BE2A4-2735-4CF9-AEC8-7EF5BE92724C","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Number of Hours","NumberofHours","Number of hours per week that the employee will work.",4,@"","E5ED7172-802B-4E09-B115-08D6DAC354B8"); // Position Approval:Number of Hours
            RockMigrationHelper.UpdateWorkflowTypeAttribute("655BE2A4-2735-4CF9-AEC8-7EF5BE92724C","C28C7BF3-A552-4D77-9408-DEDCF760CED0","Position Description","PositionDescription","Short description of the role of the position.",2,@"","6AF9BF76-9E43-49F5-AC77-B02F59C65549"); // Position Approval:Position Description
            RockMigrationHelper.UpdateWorkflowTypeAttribute("655BE2A4-2735-4CF9-AEC8-7EF5BE92724C","9C204CD0-1233-41C5-818A-C5DA439445AA","Position Title","PositionTitle","",1,@"","8C050069-2A21-4545-9D36-ED04C2168AE6"); // Position Approval:Position Title
            RockMigrationHelper.UpdateWorkflowTypeAttribute("655BE2A4-2735-4CF9-AEC8-7EF5BE92724C","E4EAB7B2-0B76-429B-AFE4-AD86D7428C70","Requestor","Requestor","The person who is making the request.",0,@"","BA94430C-DB98-48CA-A078-39743B722AFE"); // Position Approval:Requestor
            RockMigrationHelper.UpdateWorkflowTypeAttribute("655BE2A4-2735-4CF9-AEC8-7EF5BE92724C","758D9648-573E-4800-B5AF-7CC29F4BE170","Salary Range","SalaryRange","The salary range to be provided by HR.",6,@",","8E2956D4-4765-4A36-8434-2AD0D33FBB0F"); // Position Approval:Salary Range
            RockMigrationHelper.UpdateWorkflowTypeAttribute("655BE2A4-2735-4CF9-AEC8-7EF5BE92724C","7525C4CB-EE6B-41D4-9B64-A08048D5A5C0","Type","Type","If the employee will be full-time or part-time.",3,@"","3703041B-2F0D-49B2-8C45-BE0769802C7E"); // Position Approval:Type
            RockMigrationHelper.AddAttributeQualifier("F125EB97-0C1C-4AFB-84DF-0E1A43AF8B31","binaryFileType",@"3","24FB6E92-1308-4F7D-BAD0-DFA305E66547"); // Position Approval:Job Description:binaryFileType
            RockMigrationHelper.AddAttributeQualifier("3703041B-2F0D-49B2-8C45-BE0769802C7E","fieldtype",@"rb","D210C4E0-80BC-4EDF-8EC1-01FFD41F1A9E"); // Position Approval:Type:fieldtype
            RockMigrationHelper.AddAttributeQualifier("3703041B-2F0D-49B2-8C45-BE0769802C7E","values",@"Full-time, Part-time","E1930B6B-A40F-49DA-BACD-83CA7E602BCB"); // Position Approval:Type:values
            RockMigrationHelper.UpdateWorkflowActivityType("655BE2A4-2735-4CF9-AEC8-7EF5BE92724C",true,"Initial Entry","This activity handles the initial entry of the request by the staff person.",true,0,"5CE3E346-3365-40E4-8C88-EF1D352FFC76"); // Position Approval:Initial Entry
            RockMigrationHelper.UpdateWorkflowActivityType("655BE2A4-2735-4CF9-AEC8-7EF5BE92724C",true,"HR Entry","In this step an HR staff member will add information to the request (like salary range) and select the appropriate person in the organization to act as the approver.",false,1,"2003F919-4623-49C9-91F2-FD5A0CA6FA6A"); // Position Approval:HR Entry
            RockMigrationHelper.UpdateWorkflowActivityType("655BE2A4-2735-4CF9-AEC8-7EF5BE92724C",true,"Approval Process","This activity is used to get approval from the select approver.",false,2,"411E66BD-5527-457C-AB80-5C524A584E5B"); // Position Approval:Approval Process
            RockMigrationHelper.UpdateWorkflowActivityType("655BE2A4-2735-4CF9-AEC8-7EF5BE92724C",true,"Approved","This activity is activated if the request has been approved.",false,3,"0511AD1D-72B0-4B3A-AE28-3291D24CE498"); // Position Approval:Approved
            RockMigrationHelper.UpdateWorkflowActivityType("655BE2A4-2735-4CF9-AEC8-7EF5BE92724C",true,"Denied","This activity is activated if the request is denied.",false,4,"8CDD18F5-4279-46CA-886C-128571D868EE"); // Position Approval:Denied
            RockMigrationHelper.UpdateWorkflowActionForm(@"<h1>Position Details</h1>
<p>
    {{CurrentPerson.NickName}}, please complete the entry below to start the 
    approval process.
</p>",@"","Submit^^^Your information has been submitted successfully.","",true,"","1D4A540E-6D7C-4D58-9E54-754380DF6272"); // Position Approval:Initial Entry:Get Initial Input From User
            RockMigrationHelper.UpdateWorkflowActionForm(@"<h1>Full-time Details</h1>
<p>
    Since this is a full-time position please provide a job description on the 
    offical HR template. This template can be downloaded from <a href"">the HR 
    Intranet page</a>.
</p>",@"","Submit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^^Your position request has been submitted to HR.|","88C7D1CC-3478-4562-A301-AE7D4D7FFF6D",true,"","B01ECD9E-B4D4-4A24-8BAC-95306609A89C"); // Position Approval:Initial Entry:Get Full-time Details
            RockMigrationHelper.UpdateWorkflowActionForm(@"<h1>Part-time Details</h1>
<p>
    Please enter the number of hours this employee will be working per week.
</p>",@"","Submit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^^Your position request has been submitted to HR.|","",true,"","E8D6EB74-48E0-4AEA-A1CA-B6B860969A74"); // Position Approval:Initial Entry:Get Part-time Details
            RockMigrationHelper.UpdateWorkflowActionForm(@"<h1>HR Entry</h1>
<p>
    {{ Workflow.Requestor }} has requested a new {{ Workflow.PositionTitle }} 
    position. Please complete the addition items below so that this position 
    can move on for approval.
</p>",@"","Submit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^411E66BD-5527-457C-AB80-5C524A584E5B^Your information has been saved and sent on for approval.|","88C7D1CC-3478-4562-A301-AE7D4D7FFF6D",true,"","17083ABD-C595-4FE8-BCC6-4CF2CF739AF3"); // Position Approval:HR Entry:HR Entry
            RockMigrationHelper.UpdateWorkflowActionForm(@"<h1>Position Approval</h1>
<p>
    {{ Workflow.Requestor }} has requested a new {{ Workflow.PositionTitle }} position. 
    Please approve or deny this request.
</p>",@"","Approve^c88fef94-95b9-444a-bc93-58e983f3c047^0511AD1D-72B0-4B3A-AE28-3291D24CE498^You have approved this request. HR and the requester will be notified.|Deny^d6b809a9-c1cc-4ebb-816e-33d8c1e53ea4^0511AD1D-72B0-4B3A-AE28-3291D24CE498^You have denied this request. HR and the requester will be notified.|","88C7D1CC-3478-4562-A301-AE7D4D7FFF6D",true,"","59102A3B-24C8-4B4F-BA76-EEB692087ADC"); // Position Approval:Approval Process:Approval Entry
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("1D4A540E-6D7C-4D58-9E54-754380DF6272","8C050069-2A21-4545-9D36-ED04C2168AE6",0,true,false,true,"4A7C03C3-2CBC-41A9-9356-220C4A76319A"); // Position Approval:Initial Entry:Get Initial Input From User:Position Title
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("1D4A540E-6D7C-4D58-9E54-754380DF6272","6AF9BF76-9E43-49F5-AC77-B02F59C65549",1,true,false,true,"6D7D4AA0-7A74-4260-91B2-93590098AAC9"); // Position Approval:Initial Entry:Get Initial Input From User:Position Description
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("1D4A540E-6D7C-4D58-9E54-754380DF6272","3703041B-2F0D-49B2-8C45-BE0769802C7E",2,true,false,true,"0A8BDB37-7BF0-4238-8E14-404F4B7224AA"); // Position Approval:Initial Entry:Get Initial Input From User:Type
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("1D4A540E-6D7C-4D58-9E54-754380DF6272","E5ED7172-802B-4E09-B115-08D6DAC354B8",3,false,true,false,"77FB8576-F3C6-48BE-9E68-D3B34A30589B"); // Position Approval:Initial Entry:Get Initial Input From User:Number of Hours
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("1D4A540E-6D7C-4D58-9E54-754380DF6272","F125EB97-0C1C-4AFB-84DF-0E1A43AF8B31",4,false,true,false,"56123D3F-9FF3-47AF-9750-DF8A460FEE91"); // Position Approval:Initial Entry:Get Initial Input From User:Job Description
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("1D4A540E-6D7C-4D58-9E54-754380DF6272","BA94430C-DB98-48CA-A078-39743B722AFE",5,false,true,false,"49BC433B-5EDE-43C8-B605-2AE778B97C14"); // Position Approval:Initial Entry:Get Initial Input From User:Requestor
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("1D4A540E-6D7C-4D58-9E54-754380DF6272","8E2956D4-4765-4A36-8434-2AD0D33FBB0F",6,false,true,false,"E21EFEBF-6478-4655-B27A-DA74E60C5DCD"); // Position Approval:Initial Entry:Get Initial Input From User:Salary Range
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("1D4A540E-6D7C-4D58-9E54-754380DF6272","1ECDC402-4935-485D-908E-9E88B883D9E9",7,false,true,false,"03F1A90A-7D01-4F22-BE72-23D7EFD9BC39"); // Position Approval:Initial Entry:Get Initial Input From User:Approver
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("1D4A540E-6D7C-4D58-9E54-754380DF6272","508A909D-CCAD-4646-842A-FBBE3B375DB2",8,false,true,false,"914605EA-8C70-4470-B11A-0FF70B735FE8"); // Position Approval:Initial Entry:Get Initial Input From User:Approved
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("1D4A540E-6D7C-4D58-9E54-754380DF6272","1701E7CA-0E2D-43C4-B865-6BA4241AF65D",9,false,true,false,"473A0E10-12A2-4F84-99DA-A3CD8732562B"); // Position Approval:Initial Entry:Get Initial Input From User:Approval Notes
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("B01ECD9E-B4D4-4A24-8BAC-95306609A89C","8C050069-2A21-4545-9D36-ED04C2168AE6",0,true,true,false,"59BFE3E8-F2C3-45AB-B2BD-8C9363D25EC4"); // Position Approval:Initial Entry:Get Full-time Details:Position Title
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("B01ECD9E-B4D4-4A24-8BAC-95306609A89C","6AF9BF76-9E43-49F5-AC77-B02F59C65549",1,true,true,false,"34DF650E-8AAD-4BB2-B8A5-7903E646AEEF"); // Position Approval:Initial Entry:Get Full-time Details:Position Description
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("B01ECD9E-B4D4-4A24-8BAC-95306609A89C","3703041B-2F0D-49B2-8C45-BE0769802C7E",2,true,true,false,"85BAC78D-EF8E-4C4B-A37D-9C9AD6BB5BC6"); // Position Approval:Initial Entry:Get Full-time Details:Type
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("B01ECD9E-B4D4-4A24-8BAC-95306609A89C","E5ED7172-802B-4E09-B115-08D6DAC354B8",3,false,true,false,"4DDBC7DE-5C03-46AD-81D6-5B80051FF29B"); // Position Approval:Initial Entry:Get Full-time Details:Number of Hours
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("B01ECD9E-B4D4-4A24-8BAC-95306609A89C","F125EB97-0C1C-4AFB-84DF-0E1A43AF8B31",4,true,false,true,"6E705F1B-A813-4B57-9C14-EC73B9B6C1B3"); // Position Approval:Initial Entry:Get Full-time Details:Job Description
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("B01ECD9E-B4D4-4A24-8BAC-95306609A89C","BA94430C-DB98-48CA-A078-39743B722AFE",5,false,true,false,"D5790DA7-0FB4-47E5-80CF-F30EC3B21CF7"); // Position Approval:Initial Entry:Get Full-time Details:Requestor
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("B01ECD9E-B4D4-4A24-8BAC-95306609A89C","8E2956D4-4765-4A36-8434-2AD0D33FBB0F",6,false,true,false,"89B67E37-E2CE-4AE2-BBFB-49EB79DD9B03"); // Position Approval:Initial Entry:Get Full-time Details:Salary Range
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("B01ECD9E-B4D4-4A24-8BAC-95306609A89C","1ECDC402-4935-485D-908E-9E88B883D9E9",7,false,true,false,"19C64E15-F36C-4EEB-8C28-DA274FA154FE"); // Position Approval:Initial Entry:Get Full-time Details:Approver
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("B01ECD9E-B4D4-4A24-8BAC-95306609A89C","508A909D-CCAD-4646-842A-FBBE3B375DB2",8,false,true,false,"F454E67C-12D6-4A3C-AA41-EE97E4C8E627"); // Position Approval:Initial Entry:Get Full-time Details:Approved
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("B01ECD9E-B4D4-4A24-8BAC-95306609A89C","1701E7CA-0E2D-43C4-B865-6BA4241AF65D",9,false,true,false,"CB14EA84-DED9-476D-AC4F-63EE2CAE3622"); // Position Approval:Initial Entry:Get Full-time Details:Approval Notes
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("E8D6EB74-48E0-4AEA-A1CA-B6B860969A74","8C050069-2A21-4545-9D36-ED04C2168AE6",0,true,true,false,"51C800DE-402A-400F-9F35-17DC70B0148E"); // Position Approval:Initial Entry:Get Part-time Details:Position Title
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("E8D6EB74-48E0-4AEA-A1CA-B6B860969A74","6AF9BF76-9E43-49F5-AC77-B02F59C65549",1,true,true,false,"C00877DB-520E-4148-A8D0-80338E0DD7B3"); // Position Approval:Initial Entry:Get Part-time Details:Position Description
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("E8D6EB74-48E0-4AEA-A1CA-B6B860969A74","3703041B-2F0D-49B2-8C45-BE0769802C7E",2,true,true,false,"33272587-FAA7-4C76-834A-64EC16633912"); // Position Approval:Initial Entry:Get Part-time Details:Type
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("E8D6EB74-48E0-4AEA-A1CA-B6B860969A74","E5ED7172-802B-4E09-B115-08D6DAC354B8",3,true,false,true,"243273CF-9EED-48A2-B66D-F1A6AA7BA7EE"); // Position Approval:Initial Entry:Get Part-time Details:Number of Hours
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("E8D6EB74-48E0-4AEA-A1CA-B6B860969A74","F125EB97-0C1C-4AFB-84DF-0E1A43AF8B31",4,false,true,false,"FE4E0563-0893-4046-AB78-06B8A1D316BD"); // Position Approval:Initial Entry:Get Part-time Details:Job Description
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("E8D6EB74-48E0-4AEA-A1CA-B6B860969A74","BA94430C-DB98-48CA-A078-39743B722AFE",5,false,true,false,"C6419EBC-8026-4332-BCE0-CC72911F143E"); // Position Approval:Initial Entry:Get Part-time Details:Requestor
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("E8D6EB74-48E0-4AEA-A1CA-B6B860969A74","8E2956D4-4765-4A36-8434-2AD0D33FBB0F",6,false,true,false,"DB03F73A-2850-4346-8696-99DF3109B8CB"); // Position Approval:Initial Entry:Get Part-time Details:Salary Range
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("E8D6EB74-48E0-4AEA-A1CA-B6B860969A74","1ECDC402-4935-485D-908E-9E88B883D9E9",7,false,true,false,"E39FBEBB-7F94-401D-B27C-2BDFB93D88F0"); // Position Approval:Initial Entry:Get Part-time Details:Approver
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("E8D6EB74-48E0-4AEA-A1CA-B6B860969A74","508A909D-CCAD-4646-842A-FBBE3B375DB2",8,false,true,false,"4A3ED5B5-94E2-4472-A829-54CDE92A1C0F"); // Position Approval:Initial Entry:Get Part-time Details:Approved
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("E8D6EB74-48E0-4AEA-A1CA-B6B860969A74","1701E7CA-0E2D-43C4-B865-6BA4241AF65D",9,false,true,false,"F0CC64F9-31E1-4FF5-935E-05DC69FFB511"); // Position Approval:Initial Entry:Get Part-time Details:Approval Notes
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("17083ABD-C595-4FE8-BCC6-4CF2CF739AF3","8C050069-2A21-4545-9D36-ED04C2168AE6",1,true,true,false,"F0071C16-62FA-46E7-BEED-19954A88A519"); // Position Approval:HR Entry:HR Entry:Position Title
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("17083ABD-C595-4FE8-BCC6-4CF2CF739AF3","6AF9BF76-9E43-49F5-AC77-B02F59C65549",2,true,true,false,"1FF296C1-B89D-4BD3-8C0C-C93E16E77334"); // Position Approval:HR Entry:HR Entry:Position Description
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("17083ABD-C595-4FE8-BCC6-4CF2CF739AF3","3703041B-2F0D-49B2-8C45-BE0769802C7E",3,true,true,false,"6F150B2B-BAD9-44A6-9ABF-FEE7D04A096E"); // Position Approval:HR Entry:HR Entry:Type
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("17083ABD-C595-4FE8-BCC6-4CF2CF739AF3","E5ED7172-802B-4E09-B115-08D6DAC354B8",4,true,true,false,"DF161F22-19BF-46FE-B43E-09536BFBB3A7"); // Position Approval:HR Entry:HR Entry:Number of Hours
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("17083ABD-C595-4FE8-BCC6-4CF2CF739AF3","F125EB97-0C1C-4AFB-84DF-0E1A43AF8B31",5,true,true,false,"56E63C7C-84DB-4F71-93FB-592C0FD203A3"); // Position Approval:HR Entry:HR Entry:Job Description
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("17083ABD-C595-4FE8-BCC6-4CF2CF739AF3","BA94430C-DB98-48CA-A078-39743B722AFE",0,true,true,false,"87ABC867-D9F7-48DB-85F6-D02A5A5D7C65"); // Position Approval:HR Entry:HR Entry:Requestor
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("17083ABD-C595-4FE8-BCC6-4CF2CF739AF3","8E2956D4-4765-4A36-8434-2AD0D33FBB0F",6,true,false,true,"26D52D12-B610-4BFF-973D-EE92D6D9CC26"); // Position Approval:HR Entry:HR Entry:Salary Range
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("17083ABD-C595-4FE8-BCC6-4CF2CF739AF3","1ECDC402-4935-485D-908E-9E88B883D9E9",7,true,false,true,"E1C3E47C-4290-4065-9FCF-D010E6418892"); // Position Approval:HR Entry:HR Entry:Approver
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("17083ABD-C595-4FE8-BCC6-4CF2CF739AF3","508A909D-CCAD-4646-842A-FBBE3B375DB2",8,false,true,false,"C8FD95DA-6C29-44A6-862D-760B5C4825E6"); // Position Approval:HR Entry:HR Entry:Approved
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("17083ABD-C595-4FE8-BCC6-4CF2CF739AF3","1701E7CA-0E2D-43C4-B865-6BA4241AF65D",9,false,true,false,"21A65390-5076-4B0E-A94D-A9418F1AD23D"); // Position Approval:HR Entry:HR Entry:Approval Notes
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("59102A3B-24C8-4B4F-BA76-EEB692087ADC","BA94430C-DB98-48CA-A078-39743B722AFE",0,true,true,false,"6B674218-6AF2-41DB-A9BC-61714221DE08"); // Position Approval:Approval Process:Approval Entry:Requestor
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("59102A3B-24C8-4B4F-BA76-EEB692087ADC","8C050069-2A21-4545-9D36-ED04C2168AE6",1,true,true,false,"E2130B9E-7342-4B32-B4B2-C4A8891235F9"); // Position Approval:Approval Process:Approval Entry:Position Title
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("59102A3B-24C8-4B4F-BA76-EEB692087ADC","6AF9BF76-9E43-49F5-AC77-B02F59C65549",2,true,true,false,"080FE2C7-0036-42B0-BFEB-EA340B267193"); // Position Approval:Approval Process:Approval Entry:Position Description
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("59102A3B-24C8-4B4F-BA76-EEB692087ADC","3703041B-2F0D-49B2-8C45-BE0769802C7E",3,true,true,false,"C3DB2C91-1840-4DE5-BE21-42DDA4268926"); // Position Approval:Approval Process:Approval Entry:Type
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("59102A3B-24C8-4B4F-BA76-EEB692087ADC","E5ED7172-802B-4E09-B115-08D6DAC354B8",4,true,true,false,"39BA7953-D8D6-4A55-96D4-0604EB6BE24C"); // Position Approval:Approval Process:Approval Entry:Number of Hours
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("59102A3B-24C8-4B4F-BA76-EEB692087ADC","F125EB97-0C1C-4AFB-84DF-0E1A43AF8B31",5,true,true,false,"BF437D06-D345-4808-B373-818515E6EB5A"); // Position Approval:Approval Process:Approval Entry:Job Description
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("59102A3B-24C8-4B4F-BA76-EEB692087ADC","8E2956D4-4765-4A36-8434-2AD0D33FBB0F",6,true,true,false,"DA09E717-F7BA-4E2F-B0E6-AA8AC11783DD"); // Position Approval:Approval Process:Approval Entry:Salary Range
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("59102A3B-24C8-4B4F-BA76-EEB692087ADC","1ECDC402-4935-485D-908E-9E88B883D9E9",7,false,true,false,"7D56F1CF-CA72-4533-AEC5-9738E2BC6A64"); // Position Approval:Approval Process:Approval Entry:Approver
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("59102A3B-24C8-4B4F-BA76-EEB692087ADC","508A909D-CCAD-4646-842A-FBBE3B375DB2",8,false,true,false,"A799F771-342E-4475-84DC-A052C130C2D2"); // Position Approval:Approval Process:Approval Entry:Approved
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("59102A3B-24C8-4B4F-BA76-EEB692087ADC","1701E7CA-0E2D-43C4-B865-6BA4241AF65D",9,true,false,false,"134BFE0D-CC1D-4C34-BE2D-10C68D308875"); // Position Approval:Approval Process:Approval Entry:Approval Notes
            RockMigrationHelper.UpdateWorkflowActionType("5CE3E346-3365-40E4-8C88-EF1D352FFC76","Get Initial Input From User",0,"486DC4FA-FCBC-425F-90B0-E606DA8A9F68",true,false,"1D4A540E-6D7C-4D58-9E54-754380DF6272","",1,"","99EDA7DC-483A-4977-8542-0016403D896C"); // Position Approval:Initial Entry:Get Initial Input From User
            RockMigrationHelper.UpdateWorkflowActionType("5CE3E346-3365-40E4-8C88-EF1D352FFC76","Get Full-time Details",4,"486DC4FA-FCBC-425F-90B0-E606DA8A9F68",true,false,"B01ECD9E-B4D4-4A24-8BAC-95306609A89C","3703041B-2F0D-49B2-8C45-BE0769802C7E",1,"Full-time","6113A58E-9746-4835-8C36-52A04EAE7969"); // Position Approval:Initial Entry:Get Full-time Details
            RockMigrationHelper.UpdateWorkflowActionType("5CE3E346-3365-40E4-8C88-EF1D352FFC76","Get Part-time Details",6,"486DC4FA-FCBC-425F-90B0-E606DA8A9F68",true,false,"E8D6EB74-48E0-4AEA-A1CA-B6B860969A74","3703041B-2F0D-49B2-8C45-BE0769802C7E",1,"Part-time","0B614F2B-1CD1-4851-A4C0-BFFFA913DBC4"); // Position Approval:Initial Entry:Get Part-time Details
            RockMigrationHelper.UpdateWorkflowActionType("5CE3E346-3365-40E4-8C88-EF1D352FFC76","Set Full-time Hours to 40",5,"C789E457-0783-44B3-9D8F-2EBAB5F11110",true,false,"","3703041B-2F0D-49B2-8C45-BE0769802C7E",1,"Full-time","F90096F5-F88B-4B9C-8251-C4C321418390"); // Position Approval:Initial Entry:Set Full-time Hours to 40
            RockMigrationHelper.UpdateWorkflowActionType("5CE3E346-3365-40E4-8C88-EF1D352FFC76","Set Requestor",2,"24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A",true,false,"","",1,"","C85E9810-E0E7-41BE-A19E-68C76AFE8925"); // Position Approval:Initial Entry:Set Requestor
            RockMigrationHelper.UpdateWorkflowActionType("5CE3E346-3365-40E4-8C88-EF1D352FFC76","Persist Workflow",1,"F1A39347-6FE0-43D4-89FB-544195088ECF",true,false,"","",1,"","ED0812D1-BA80-4EBC-88D7-E0AEBAC9F06A"); // Position Approval:Initial Entry:Persist Workflow
            RockMigrationHelper.UpdateWorkflowActionType("5CE3E346-3365-40E4-8C88-EF1D352FFC76","Send to HR for Additional Fields",7,"38907A90-1634-4A93-8017-619326A4A582",true,true,"","",1,"","AC0FD5EA-A0FE-40DA-8702-3FB144EC49D6"); // Position Approval:Initial Entry:Send to HR for Additional Fields
            RockMigrationHelper.UpdateWorkflowActionType("5CE3E346-3365-40E4-8C88-EF1D352FFC76","Set Workflow Name",3,"36005473-BD5D-470B-B28D-98E6D7ED808D",true,false,"","",1,"","213AC950-595D-40DE-8CEE-AA16D65F21E3"); // Position Approval:Initial Entry:Set Workflow Name
            RockMigrationHelper.UpdateWorkflowActionType("2003F919-4623-49C9-91F2-FD5A0CA6FA6A","HR Entry",1,"486DC4FA-FCBC-425F-90B0-E606DA8A9F68",true,true,"17083ABD-C595-4FE8-BCC6-4CF2CF739AF3","",1,"","F283EDEC-64BD-479C-8F2E-103359AA47CF"); // Position Approval:HR Entry:HR Entry
            RockMigrationHelper.UpdateWorkflowActionType("2003F919-4623-49C9-91F2-FD5A0CA6FA6A","Assign To HR Member",0,"FB2981B7-7922-42E1-8ACF-7F63BB7989E6",true,false,"","",1,"","42B16D80-E722-4690-AB19-842CD666AC35"); // Position Approval:HR Entry:Assign To HR Member
            RockMigrationHelper.UpdateWorkflowActionType("411E66BD-5527-457C-AB80-5C524A584E5B","Assign to Approver",0,"F100A31F-E93A-4C7A-9E55-0FAF41A101C4",true,false,"","",1,"","E01064E5-229F-4A64-ACC8-28612910E219"); // Position Approval:Approval Process:Assign to Approver
            RockMigrationHelper.UpdateWorkflowActionType("411E66BD-5527-457C-AB80-5C524A584E5B","Approval Entry",1,"486DC4FA-FCBC-425F-90B0-E606DA8A9F68",true,true,"59102A3B-24C8-4B4F-BA76-EEB692087ADC","",1,"","C93403A1-C753-4579-A083-D74568E21977"); // Position Approval:Approval Process:Approval Entry
            RockMigrationHelper.UpdateWorkflowActionType("0511AD1D-72B0-4B3A-AE28-3291D24CE498","Email Requester",1,"66197B01-D1F0-4924-A315-47AD54E030DE",true,false,"","",1,"","FF1538E8-2357-4E47-9735-D382622FF3FD"); // Position Approval:Approved:Email Requester
            RockMigrationHelper.UpdateWorkflowActionType("0511AD1D-72B0-4B3A-AE28-3291D24CE498","Email HR",2,"66197B01-D1F0-4924-A315-47AD54E030DE",true,false,"","",1,"","72269A5F-AB1C-4756-9D28-A3CE9051AD13"); // Position Approval:Approved:Email HR
            RockMigrationHelper.UpdateWorkflowActionType("0511AD1D-72B0-4B3A-AE28-3291D24CE498","Mark Request Approved",0,"C789E457-0783-44B3-9D8F-2EBAB5F11110",true,false,"","",1,"","F407C8B0-FD05-43AD-9801-A1C8C5450A53"); // Position Approval:Approved:Mark Request Approved
            RockMigrationHelper.UpdateWorkflowActionType("0511AD1D-72B0-4B3A-AE28-3291D24CE498","Mark Workflow Complete",3,"EEDA4318-F014-4A46-9C76-4C052EF81AA1",true,true,"","",1,"","3209423E-B0ED-4D4C-89B2-02F1D5D5E126"); // Position Approval:Approved:Mark Workflow Complete
            RockMigrationHelper.UpdateWorkflowActionType("8CDD18F5-4279-46CA-886C-128571D868EE","Mark Workflow Complete",3,"EEDA4318-F014-4A46-9C76-4C052EF81AA1",true,true,"","",1,"","701A39A7-8FC1-4249-9273-E0B3BB9E7BCB"); // Position Approval:Denied:Mark Workflow Complete
            RockMigrationHelper.UpdateWorkflowActionType("8CDD18F5-4279-46CA-886C-128571D868EE","Mark Position Denied",0,"C789E457-0783-44B3-9D8F-2EBAB5F11110",true,false,"","",1,"","098380A7-3B88-43FE-95DE-821F7958EC29"); // Position Approval:Denied:Mark Position Denied
            RockMigrationHelper.UpdateWorkflowActionType("8CDD18F5-4279-46CA-886C-128571D868EE","Email Requester",1,"66197B01-D1F0-4924-A315-47AD54E030DE",true,false,"","",1,"","C3486002-E6B9-478F-B17A-78B7C8DABDF8"); // Position Approval:Denied:Email Requester
            RockMigrationHelper.UpdateWorkflowActionType("8CDD18F5-4279-46CA-886C-128571D868EE","Email HR",2,"66197B01-D1F0-4924-A315-47AD54E030DE",true,false,"","",1,"","1CFB5480-8BE7-481E-B7C0-9C707D6CDB49"); // Position Approval:Denied:Email HR
            RockMigrationHelper.AddActionTypeAttributeValue("99EDA7DC-483A-4977-8542-0016403D896C","234910F2-A0DB-4D7D-BAF7-83C880EF30AE",@"False"); // Position Approval:Initial Entry:Get Initial Input From User:Active
            RockMigrationHelper.AddActionTypeAttributeValue("99EDA7DC-483A-4977-8542-0016403D896C","C178113D-7C86-4229-8424-C6D0CF4A7E23",@""); // Position Approval:Initial Entry:Get Initial Input From User:Order
            RockMigrationHelper.AddActionTypeAttributeValue("ED0812D1-BA80-4EBC-88D7-E0AEBAC9F06A","50B01639-4938-40D2-A791-AA0EB4F86847",@"False"); // Position Approval:Initial Entry:Persist Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue("ED0812D1-BA80-4EBC-88D7-E0AEBAC9F06A","86F795B0-0CB6-4DA4-9CE4-B11D0922F361",@""); // Position Approval:Initial Entry:Persist Workflow:Order
            RockMigrationHelper.AddActionTypeAttributeValue("C85E9810-E0E7-41BE-A19E-68C76AFE8925","DE9CB292-4785-4EA3-976D-3826F91E9E98",@"False"); // Position Approval:Initial Entry:Set Requestor:Active
            RockMigrationHelper.AddActionTypeAttributeValue("C85E9810-E0E7-41BE-A19E-68C76AFE8925","89E9BCED-91AB-47B0-AD52-D78B0B7CB9E8",@""); // Position Approval:Initial Entry:Set Requestor:Order
            RockMigrationHelper.AddActionTypeAttributeValue("C85E9810-E0E7-41BE-A19E-68C76AFE8925","BBED8A83-8BB2-4D35-BAFB-05F67DCAD112",@"ba94430c-db98-48ca-a078-39743b722afe"); // Position Approval:Initial Entry:Set Requestor:Person Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("213AC950-595D-40DE-8CEE-AA16D65F21E3","0A800013-51F7-4902-885A-5BE215D67D3D",@"False"); // Position Approval:Initial Entry:Set Workflow Name:Active
            RockMigrationHelper.AddActionTypeAttributeValue("213AC950-595D-40DE-8CEE-AA16D65F21E3","5D95C15A-CCAE-40AD-A9DD-F929DA587115",@""); // Position Approval:Initial Entry:Set Workflow Name:Order
            RockMigrationHelper.AddActionTypeAttributeValue("213AC950-595D-40DE-8CEE-AA16D65F21E3","93852244-A667-4749-961A-D47F88675BE4",@"8c050069-2a21-4545-9d36-ed04c2168ae6"); // Position Approval:Initial Entry:Set Workflow Name:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("6113A58E-9746-4835-8C36-52A04EAE7969","234910F2-A0DB-4D7D-BAF7-83C880EF30AE",@"False"); // Position Approval:Initial Entry:Get Full-time Details:Active
            RockMigrationHelper.AddActionTypeAttributeValue("6113A58E-9746-4835-8C36-52A04EAE7969","C178113D-7C86-4229-8424-C6D0CF4A7E23",@""); // Position Approval:Initial Entry:Get Full-time Details:Order
            RockMigrationHelper.AddActionTypeAttributeValue("F90096F5-F88B-4B9C-8251-C4C321418390","D7EAA859-F500-4521-9523-488B12EAA7D2",@"False"); // Position Approval:Initial Entry:Set Full-time Hours to 40:Active
            RockMigrationHelper.AddActionTypeAttributeValue("F90096F5-F88B-4B9C-8251-C4C321418390","44A0B977-4730-4519-8FF6-B0A01A95B212",@"e5ed7172-802b-4e09-b115-08d6dac354b8"); // Position Approval:Initial Entry:Set Full-time Hours to 40:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("F90096F5-F88B-4B9C-8251-C4C321418390","57093B41-50ED-48E5-B72B-8829E62704C8",@""); // Position Approval:Initial Entry:Set Full-time Hours to 40:Order
            RockMigrationHelper.AddActionTypeAttributeValue("F90096F5-F88B-4B9C-8251-C4C321418390","E5272B11-A2B8-49DC-860D-8D574E2BC15C",@"40"); // Position Approval:Initial Entry:Set Full-time Hours to 40:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("0B614F2B-1CD1-4851-A4C0-BFFFA913DBC4","234910F2-A0DB-4D7D-BAF7-83C880EF30AE",@"False"); // Position Approval:Initial Entry:Get Part-time Details:Active
            RockMigrationHelper.AddActionTypeAttributeValue("0B614F2B-1CD1-4851-A4C0-BFFFA913DBC4","C178113D-7C86-4229-8424-C6D0CF4A7E23",@""); // Position Approval:Initial Entry:Get Part-time Details:Order
            RockMigrationHelper.AddActionTypeAttributeValue("AC0FD5EA-A0FE-40DA-8702-3FB144EC49D6","E8ABD802-372C-47BE-82B1-96F50DB5169E",@"False"); // Position Approval:Initial Entry:Send to HR for Additional Fields:Active
            RockMigrationHelper.AddActionTypeAttributeValue("AC0FD5EA-A0FE-40DA-8702-3FB144EC49D6","02D5A7A5-8781-46B4-B9FC-AF816829D240",@"2003F919-4623-49C9-91F2-FD5A0CA6FA6A"); // Position Approval:Initial Entry:Send to HR for Additional Fields:Activity
            RockMigrationHelper.AddActionTypeAttributeValue("AC0FD5EA-A0FE-40DA-8702-3FB144EC49D6","3809A78C-B773-440C-8E3F-A8E81D0DAE08",@""); // Position Approval:Initial Entry:Send to HR for Additional Fields:Order
            RockMigrationHelper.AddActionTypeAttributeValue("42B16D80-E722-4690-AB19-842CD666AC35","0B768E17-C64A-4212-BAD5-8A16B9F05A5C",@"False"); // Position Approval:HR Entry:Assign To HR Member:Active
            RockMigrationHelper.AddActionTypeAttributeValue("42B16D80-E722-4690-AB19-842CD666AC35","5C5F7DB4-51DE-4293-BD73-CABDEB6564AC",@""); // Position Approval:HR Entry:Assign To HR Member:Order
            RockMigrationHelper.AddActionTypePersonAttributeValue("42B16D80-E722-4690-AB19-842CD666AC35","7ED2571D-B1BF-4DB6-9D04-9B5D064F51D8",@"19016194-9a27-4d10-935b-e317548d75fb"); // Position Approval:HR Entry:Assign To HR Member:Person
            RockMigrationHelper.AddActionTypeAttributeValue("F283EDEC-64BD-479C-8F2E-103359AA47CF","234910F2-A0DB-4D7D-BAF7-83C880EF30AE",@"False"); // Position Approval:HR Entry:HR Entry:Active
            RockMigrationHelper.AddActionTypeAttributeValue("F283EDEC-64BD-479C-8F2E-103359AA47CF","C178113D-7C86-4229-8424-C6D0CF4A7E23",@""); // Position Approval:HR Entry:HR Entry:Order
            RockMigrationHelper.AddActionTypeAttributeValue("E01064E5-229F-4A64-ACC8-28612910E219","E0F7AB7E-7761-4600-A099-CB14ACDBF6EF",@"False"); // Position Approval:Approval Process:Assign to Approver:Active
            RockMigrationHelper.AddActionTypeAttributeValue("E01064E5-229F-4A64-ACC8-28612910E219","FBADD25F-D309-4512-8430-3CC8615DD60E",@"1ecdc402-4935-485d-908e-9e88b883d9e9"); // Position Approval:Approval Process:Assign to Approver:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("E01064E5-229F-4A64-ACC8-28612910E219","7A6B605D-7FB1-4F48-AF35-5A0683FB1CDA",@""); // Position Approval:Approval Process:Assign to Approver:Order
            RockMigrationHelper.AddActionTypeAttributeValue("C93403A1-C753-4579-A083-D74568E21977","234910F2-A0DB-4D7D-BAF7-83C880EF30AE",@"False"); // Position Approval:Approval Process:Approval Entry:Active
            RockMigrationHelper.AddActionTypeAttributeValue("C93403A1-C753-4579-A083-D74568E21977","C178113D-7C86-4229-8424-C6D0CF4A7E23",@""); // Position Approval:Approval Process:Approval Entry:Order
            RockMigrationHelper.AddActionTypeAttributeValue("F407C8B0-FD05-43AD-9801-A1C8C5450A53","D7EAA859-F500-4521-9523-488B12EAA7D2",@"False"); // Position Approval:Approved:Mark Request Approved:Active
            RockMigrationHelper.AddActionTypeAttributeValue("F407C8B0-FD05-43AD-9801-A1C8C5450A53","44A0B977-4730-4519-8FF6-B0A01A95B212",@"508a909d-ccad-4646-842a-fbbe3b375db2"); // Position Approval:Approved:Mark Request Approved:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("F407C8B0-FD05-43AD-9801-A1C8C5450A53","57093B41-50ED-48E5-B72B-8829E62704C8",@""); // Position Approval:Approved:Mark Request Approved:Order
            RockMigrationHelper.AddActionTypeAttributeValue("F407C8B0-FD05-43AD-9801-A1C8C5450A53","E5272B11-A2B8-49DC-860D-8D574E2BC15C",@"Approved"); // Position Approval:Approved:Mark Request Approved:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("FF1538E8-2357-4E47-9735-D382622FF3FD","36197160-7D3D-490D-AB42-7E29105AFE91",@"False"); // Position Approval:Approved:Email Requester:Active
            RockMigrationHelper.AddActionTypeAttributeValue("FF1538E8-2357-4E47-9735-D382622FF3FD","9F5F7CEC-F369-4FDF-802A-99074CE7A7FC",@"jonathan.edmiston@gmail.com"); // Position Approval:Approved:Email Requester:From Email Address|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("FF1538E8-2357-4E47-9735-D382622FF3FD","D1269254-C15A-40BD-B784-ADCC231D3950",@""); // Position Approval:Approved:Email Requester:Order
            RockMigrationHelper.AddActionTypeAttributeValue("FF1538E8-2357-4E47-9735-D382622FF3FD","0C4C13B8-7076-4872-925A-F950886B5E16",@"ba94430c-db98-48ca-a078-39743b722afe"); // Position Approval:Approved:Email Requester:Send To Email Address|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("FF1538E8-2357-4E47-9735-D382622FF3FD","5D9B13B6-CD96-4C7C-86FA-4512B9D28386",@"UPDATE: {{Workflow.PositionTitle}}"); // Position Approval:Approved:Email Requester:Subject
            RockMigrationHelper.AddActionTypeAttributeValue("FF1538E8-2357-4E47-9735-D382622FF3FD","4D245B9E-6B03-46E7-8482-A51FBA190E4D",@"{{ GlobalAttribute.EmailHeader }}
<p>
    {{Person.NickName}}, your request for the {{Workflow.PositionTitle}} position has been 
    approved by {{Workflow.Approver}}. HR will be getting with you soon to arrange next steps
    for the posting process.
</p>
{% if Workflow.ApprovalNotes != null %}
    <b>Approval Notes:</b>
    <p>
        {{Workflow.ApprovalNotes}}
    </p>
{% endif %}
{{ GlobalAttribute.EmailFooter }}"); // Position Approval:Approved:Email Requester:Body
            RockMigrationHelper.AddActionTypeAttributeValue("72269A5F-AB1C-4756-9D28-A3CE9051AD13","36197160-7D3D-490D-AB42-7E29105AFE91",@"False"); // Position Approval:Approved:Email HR:Active
            RockMigrationHelper.AddActionTypeAttributeValue("72269A5F-AB1C-4756-9D28-A3CE9051AD13","9F5F7CEC-F369-4FDF-802A-99074CE7A7FC",@"info@rocksolidchurchdemo.com"); // Position Approval:Approved:Email HR:From Email Address|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("72269A5F-AB1C-4756-9D28-A3CE9051AD13","D1269254-C15A-40BD-B784-ADCC231D3950",@""); // Position Approval:Approved:Email HR:Order
            RockMigrationHelper.AddActionTypeAttributeValue("72269A5F-AB1C-4756-9D28-A3CE9051AD13","0C4C13B8-7076-4872-925A-F950886B5E16",@"jonedmiston@ccvonline.com"); // Position Approval:Approved:Email HR:Send To Email Address|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("72269A5F-AB1C-4756-9D28-A3CE9051AD13","5D9B13B6-CD96-4C7C-86FA-4512B9D28386",@"UPDATE: {{Workflow.PositionTitle}}"); // Position Approval:Approved:Email HR:Subject
            RockMigrationHelper.AddActionTypeAttributeValue("72269A5F-AB1C-4756-9D28-A3CE9051AD13","4D245B9E-6B03-46E7-8482-A51FBA190E4D",@"{{ GlobalAttribute.EmailHeader }}
<p>
    The request for the {{Workflow.PositionTitle}} position has been 
    approved by {{Workflow.Approver}}. Please follow up with {{Workflow.Requester}} with
    next steps for posting the position.
</p>
{% if Workflow.ApprovalNotes != null %}
    <b>Approval Notes:</b>
    <p>
        {{Workflow.ApprovalNotes}}
    </p>
{% endif %}
{{ GlobalAttribute.EmailFooter }}"); // Position Approval:Approved:Email HR:Body
            RockMigrationHelper.AddActionTypeAttributeValue("3209423E-B0ED-4D4C-89B2-02F1D5D5E126","0CA0DDEF-48EF-4ABC-9822-A05E225DE26C",@"False"); // Position Approval:Approved:Mark Workflow Complete:Active
            RockMigrationHelper.AddActionTypeAttributeValue("3209423E-B0ED-4D4C-89B2-02F1D5D5E126","25CAD4BE-5A00-409D-9BAB-E32518D89956",@""); // Position Approval:Approved:Mark Workflow Complete:Order
            RockMigrationHelper.AddActionTypeAttributeValue("098380A7-3B88-43FE-95DE-821F7958EC29","D7EAA859-F500-4521-9523-488B12EAA7D2",@"False"); // Position Approval:Denied:Mark Position Denied:Active
            RockMigrationHelper.AddActionTypeAttributeValue("098380A7-3B88-43FE-95DE-821F7958EC29","44A0B977-4730-4519-8FF6-B0A01A95B212",@"508a909d-ccad-4646-842a-fbbe3b375db2"); // Position Approval:Denied:Mark Position Denied:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("098380A7-3B88-43FE-95DE-821F7958EC29","57093B41-50ED-48E5-B72B-8829E62704C8",@""); // Position Approval:Denied:Mark Position Denied:Order
            RockMigrationHelper.AddActionTypeAttributeValue("098380A7-3B88-43FE-95DE-821F7958EC29","E5272B11-A2B8-49DC-860D-8D574E2BC15C",@"Denied"); // Position Approval:Denied:Mark Position Denied:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("C3486002-E6B9-478F-B17A-78B7C8DABDF8","36197160-7D3D-490D-AB42-7E29105AFE91",@"False"); // Position Approval:Denied:Email Requester:Active
            RockMigrationHelper.AddActionTypeAttributeValue("C3486002-E6B9-478F-B17A-78B7C8DABDF8","9F5F7CEC-F369-4FDF-802A-99074CE7A7FC",@"info@rocksolidchurchdemo.com"); // Position Approval:Denied:Email Requester:From Email Address|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("C3486002-E6B9-478F-B17A-78B7C8DABDF8","D1269254-C15A-40BD-B784-ADCC231D3950",@""); // Position Approval:Denied:Email Requester:Order
            RockMigrationHelper.AddActionTypeAttributeValue("C3486002-E6B9-478F-B17A-78B7C8DABDF8","0C4C13B8-7076-4872-925A-F950886B5E16",@"ba94430c-db98-48ca-a078-39743b722afe"); // Position Approval:Denied:Email Requester:Send To Email Address|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("C3486002-E6B9-478F-B17A-78B7C8DABDF8","5D9B13B6-CD96-4C7C-86FA-4512B9D28386",@"UPDATE: {{Workflow.PositionTitle}}"); // Position Approval:Denied:Email Requester:Subject
            RockMigrationHelper.AddActionTypeAttributeValue("C3486002-E6B9-478F-B17A-78B7C8DABDF8","4D245B9E-6B03-46E7-8482-A51FBA190E4D",@"{{ GlobalAttribute.EmailHeader }}
<p>
    {{Person.NickName}}, your request for the {{Workflow.PositionTitle}} position was not approved by
    {{Workflow.Approver}}. HR will be getting with you soon to arrange next steps
    for this process.
</p>
{% if Workflow.ApprovalNotes != null %}
    <b>Approval Notes:</b>
    <p>
        {{Workflow.ApprovalNotes}}
    </p>
{% endif %}
{{ GlobalAttribute.EmailFooter }}"); // Position Approval:Denied:Email Requester:Body
            RockMigrationHelper.AddActionTypeAttributeValue("1CFB5480-8BE7-481E-B7C0-9C707D6CDB49","36197160-7D3D-490D-AB42-7E29105AFE91",@"False"); // Position Approval:Denied:Email HR:Active
            RockMigrationHelper.AddActionTypeAttributeValue("1CFB5480-8BE7-481E-B7C0-9C707D6CDB49","9F5F7CEC-F369-4FDF-802A-99074CE7A7FC",@"info@rocksolidchurchdemo.com"); // Position Approval:Denied:Email HR:From Email Address|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("1CFB5480-8BE7-481E-B7C0-9C707D6CDB49","D1269254-C15A-40BD-B784-ADCC231D3950",@""); // Position Approval:Denied:Email HR:Order
            RockMigrationHelper.AddActionTypeAttributeValue("1CFB5480-8BE7-481E-B7C0-9C707D6CDB49","0C4C13B8-7076-4872-925A-F950886B5E16",@"hr@rocksolidchurchdemo.com"); // Position Approval:Denied:Email HR:Send To Email Address|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("1CFB5480-8BE7-481E-B7C0-9C707D6CDB49","5D9B13B6-CD96-4C7C-86FA-4512B9D28386",@"UPDATE: {{Workflow.PositionTitle}}"); // Position Approval:Denied:Email HR:Subject
            RockMigrationHelper.AddActionTypeAttributeValue("1CFB5480-8BE7-481E-B7C0-9C707D6CDB49","4D245B9E-6B03-46E7-8482-A51FBA190E4D",@"{{ GlobalAttribute.EmailHeader }}
<p>
    The request for the {{Workflow.PositionTitle}} position was not 
    approved by {{Workflow.Approver}}. Please follow up with {{Workflow.Requester}} with
    next steps for this process.
</p>
{% if Workflow.ApprovalNotes != null %}
    <b>Approval Notes:</b>
    <p>
        {{Workflow.ApprovalNotes}}
    </p>
{% endif %}
{{ GlobalAttribute.EmailFooter }}"); // Position Approval:Denied:Email HR:Body
            RockMigrationHelper.AddActionTypeAttributeValue("701A39A7-8FC1-4249-9273-E0B3BB9E7BCB","0CA0DDEF-48EF-4ABC-9822-A05E225DE26C",@"False"); // Position Approval:Denied:Mark Workflow Complete:Active
            RockMigrationHelper.AddActionTypeAttributeValue("701A39A7-8FC1-4249-9273-E0B3BB9E7BCB","25CAD4BE-5A00-409D-9BAB-E32518D89956",@""); // Position Approval:Denied:Mark Workflow Complete:Order

            #endregion
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "8023344B-C247-4A23-A191-880ABF391A34" );
            RockMigrationHelper.DeleteAttribute( "9E710496-17F7-4F22-9DD1-3A7563E9E906" );
            RockMigrationHelper.DeleteAttribute( "555306F1-6117-48B9-B184-D48DC1EC445F" );

            Sql( @"
    DELETE FROM [Report] where [Guid] = '4E3ECAE0-9D36-4C22-994D-AD31DE0F6FB7'
" );

            RockMigrationHelper.DeleteSecurityAuth( "C58E5152-2330-4D34-9C53-606554341D53" );
            RockMigrationHelper.DeleteSecurityAuth( "43C4483B-3266-4E0B-9CCE-FFAA1EBFC519" );
            RockMigrationHelper.DeleteSecurityAuth( "CC03214B-5F01-4619-A62E-45A4C8A2E766" );
            RockMigrationHelper.DeleteSecurityAuth( "486E3BA8-9FD9-4FA3-96B5-FDAB8FD156F0" );
            
            RockMigrationHelper.DeleteSecurityAuth( "5F45DB5F-3196-4458-A1FF-2517A41482E9" );
            RockMigrationHelper.DeleteSecurityAuth( "8CCCAB09-741E-4A61-93B0-E31028E0AC43" );
            RockMigrationHelper.DeleteSecurityAuth( "EB3F90C2-4A44-4B42-A296-61D61C4D1183" );
            RockMigrationHelper.DeleteSecurityAuth( "0F939CC5-F0E2-4DCB-AB5F-6E5111801291" );
            
            RockMigrationHelper.DeleteSecurityAuth( "5BB65E0E-49CF-45B7-984B-9B0228000590" );
            RockMigrationHelper.DeleteSecurityAuth( "8E5A555A-EAB1-4C46-A270-62D8633264D5" );
            RockMigrationHelper.DeleteSecurityAuth( "BB4F04BC-F984-4F0D-BAD9-F1162970E669" );
            RockMigrationHelper.DeleteSecurityAuth( "C4A6BE0B-48ED-4901-9EC4-F4ED1D84DBA6" );
            
            RockMigrationHelper.DeleteSecurityAuth( "71DDE91A-B4F6-4698-B9E3-0EF4D9097745" );
            RockMigrationHelper.DeleteSecurityAuth( "404BD7D1-EC18-473E-94A4-00B22AAAD19F" );
            RockMigrationHelper.DeleteSecurityAuth( "DB6409C7-7200-4505-B87B-CAE47FF02CD9" );
            RockMigrationHelper.DeleteSecurityAuth( "AC650DF8-579F-4262-98B5-2B27A24D2101" );
            
            RockMigrationHelper.DeleteSecurityAuth( "933C0B3C-9E96-484A-B8ED-F5EF205291D4" );
            RockMigrationHelper.DeleteSecurityAuth( "08C1964E-F2BC-4DC4-B80D-17E720E921E3" );
            RockMigrationHelper.DeleteSecurityAuth( "35738ED0-6A9C-497E-955C-EFBF0BA00100" );
            RockMigrationHelper.DeleteSecurityAuth( "2C782356-FCCA-49CA-9B76-53A9AF5B1ECA" );
        }
    }
}
