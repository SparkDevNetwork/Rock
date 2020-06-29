using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

using com.bemaservices.HrManagement.SystemGuid;
using Rock.Web.Cache;
using Rock.Lava.Blocks;
using System.Security.AccessControl;

namespace com.bemaservices.HrManagement.Migrations
{
    [MigrationNumber( 3, "1.9.4" )]
    public class Workflows : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            #region FieldTypes


            #endregion

            #region EntityTypes

            RockMigrationHelper.UpdateEntityType( "Rock.Model.Workflow", "3540E9A7-FE30-43A9-8B0A-A372B63DFC93", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.WorkflowActivity", "2CB52ED0-CB06-4D62-9E2C-73B60AFA4C9F", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.WorkflowActionType", "23E3273A-B137-48A3-9AFF-C8DC832DDCA6", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.ActivateActivity", "38907A90-1634-4A93-8017-619326A4A582", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.RunLava", "BC21E57A-1477-44B3-A7C2-61A806118945", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.RunSQL", "A41216D6-6FB0-4019-B222-2C29B4519CF4", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SetAttributeToCurrentPerson", "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.UserEntryForm", "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", false, true );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "DE9CB292-4785-4EA3-976D-3826F91E9E98" ); // Rock.Workflow.Action.SetAttributeToCurrentPerson:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Person Attribute", "PersonAttribute", "The attribute to set to the currently logged in person.", 0, @"", "BBED8A83-8BB2-4D35-BAFB-05F67DCAD112" ); // Rock.Workflow.Action.SetAttributeToCurrentPerson:Person Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "89E9BCED-91AB-47B0-AD52-D78B0B7CB9E8" ); // Rock.Workflow.Action.SetAttributeToCurrentPerson:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "38907A90-1634-4A93-8017-619326A4A582", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "E8ABD802-372C-47BE-82B1-96F50DB5169E" ); // Rock.Workflow.Action.ActivateActivity:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "38907A90-1634-4A93-8017-619326A4A582", "739FD425-5B8C-4605-B775-7E4D9D4C11DB", "Activity", "Activity", "The activity type to activate", 0, @"", "02D5A7A5-8781-46B4-B9FC-AF816829D240" ); // Rock.Workflow.Action.ActivateActivity:Activity
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "38907A90-1634-4A93-8017-619326A4A582", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "3809A78C-B773-440C-8E3F-A8E81D0DAE08" ); // Rock.Workflow.Action.ActivateActivity:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE" ); // Rock.Workflow.Action.UserEntryForm:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "C178113D-7C86-4229-8424-C6D0CF4A7E23" ); // Rock.Workflow.Action.UserEntryForm:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A41216D6-6FB0-4019-B222-2C29B4519CF4", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "SQLQuery", "SQLQuery", "The SQL query to run. <span class='tip tip-lava'></span>", 0, @"", "F3B9908B-096F-460B-8320-122CF046D1F9" ); // Rock.Workflow.Action.RunSQL:SQLQuery
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A41216D6-6FB0-4019-B222-2C29B4519CF4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "A18C3143-0586-4565-9F36-E603BC674B4E" ); // Rock.Workflow.Action.RunSQL:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A41216D6-6FB0-4019-B222-2C29B4519CF4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Continue On Error", "ContinueOnError", "Should processing continue even if SQL Error occurs?", 3, @"False", "D992DB0A-B528-4833-ADCE-61C5BD9BD156" ); // Rock.Workflow.Action.RunSQL:Continue On Error
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A41216D6-6FB0-4019-B222-2C29B4519CF4", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Result Attribute", "ResultAttribute", "An optional attribute to set to the scaler result of SQL query.", 2, @"", "56997192-2545-4EA1-B5B2-313B04588984" ); // Rock.Workflow.Action.RunSQL:Result Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A41216D6-6FB0-4019-B222-2C29B4519CF4", "73B02051-0D38-4AD9-BF81-A2D477DE4F70", "Parameters", "Parameters", "The parameters to supply to the SQL query. <span class='tip tip-lava'></span>", 1, @"", "45C97B53-B45E-44CD-8BD9-12DB8302BE38" ); // Rock.Workflow.Action.RunSQL:Parameters
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A41216D6-6FB0-4019-B222-2C29B4519CF4", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "FA7C685D-8636-41EF-9998-90FFF3998F76" ); // Rock.Workflow.Action.RunSQL:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava", "Value", "The <span class='tip tip-lava'></span> to run.", 0, @"", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4" ); // Rock.Workflow.Action.RunLava:Lava
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "F1924BDC-9B79-4018-9D4A-C3516C87A514" ); // Rock.Workflow.Action.RunLava:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attribute", "Attribute", "The attribute to store the result in.", 1, @"", "431273C6-342D-4030-ADC7-7CDEDC7F8B27" ); // Rock.Workflow.Action.RunLava:Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "The Lava commands that should be enabled for this action.", 2, @"", "F3E380BF-AAC8-4015-9ADC-0DF56B5462F5" ); // Rock.Workflow.Action.RunLava:Enabled Lava Commands
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "1B833F48-EFC2-4537-B1E3-7793F6863EAA" ); // Rock.Workflow.Action.RunLava:Order

            #endregion

            #region Categories

            RockMigrationHelper.UpdateCategory( "C9F3C4A5-1526-474D-803F-D6C7A45CBBAE", "PTO Requests", "fa fa-clock", "", "CD21D1FD-B9DB-4122-B252-86E8FD85CEEC", 0 ); // PTO Requests

            #endregion

            #region Request PTO

            RockMigrationHelper.UpdateWorkflowType( false, true, "Request PTO", "", "CD21D1FD-B9DB-4122-B252-86E8FD85CEEC", "Work", "fa fa-list-ol", 28800, true, 0, "25CA07A0-7662-42C8-9BD6-E2EDCE157795", 0 ); // Request PTO
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "25CA07A0-7662-42C8-9BD6-E2EDCE157795", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Person", "Person", "", 0, @"", "CBA976C2-3B3B-4E06-B258-0896CF02163B", false ); // Request PTO:Person
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "25CA07A0-7662-42C8-9BD6-E2EDCE157795", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Supervisor", "Supervisor", "", 1, @"", "2574FCAB-22F5-4D91-B217-84AFA2B4A022", false ); // Request PTO:Supervisor
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "25CA07A0-7662-42C8-9BD6-E2EDCE157795", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Supervisor Attribute", "SupervisorAttribute", "", 2, @"67afd5a3-28f3-404f-a3b8-88630061f294", "37BABFB2-60C3-4A21-82CA-154F731D4EDE", false ); // Request PTO:Supervisor Attribute
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "25CA07A0-7662-42C8-9BD6-E2EDCE157795", "6B6AA175-4758-453F-8D83-FCD8044B5F36", "Start Date", "StartDate", "", 3, @"", "8F85DA13-2602-4438-B58A-CFAA88FBFDDD", false ); // Request PTO:Start Date
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "25CA07A0-7662-42C8-9BD6-E2EDCE157795", "6B6AA175-4758-453F-8D83-FCD8044B5F36", "End Date", "EndDate", "", 4, @"", "2266B6AB-419F-45F8-880B-F8085F0BCD11", false ); // Request PTO:End Date
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "25CA07A0-7662-42C8-9BD6-E2EDCE157795", "81FD3C9B-E22A-492F-9212-93546BBE6677", "PTO Type", "PTOType", "", 5, @"", "598E70BB-8A92-483B-834B-2FAF6572C8EE", false ); // Request PTO:PTO Type
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "25CA07A0-7662-42C8-9BD6-E2EDCE157795", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Hours / Day", "HoursDay", "", 6, @"", "DBC30BCF-E5E8-4F4B-BEB0-7D1C226C317A", false ); // Request PTO:Hours / Day
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "25CA07A0-7662-42C8-9BD6-E2EDCE157795", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Reason", "Reason", "", 7, @"", "66868B80-D1EB-417F-BCAE-F8D75E68FC8C", false ); // Request PTO:Reason
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "25CA07A0-7662-42C8-9BD6-E2EDCE157795", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Hours Available", "HoursAvailable", "", 8, @"", "593E3676-C196-4265-8BED-7199C9E94833", false ); // Request PTO:Hours Available
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "25CA07A0-7662-42C8-9BD6-E2EDCE157795", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Error Message", "ErrorMessage", "", 9, @"", "2C4F2FBE-6E72-48FA-B26E-B1C9D4A11DA1", false ); // Request PTO:Error Message
            RockMigrationHelper.AddAttributeQualifier( "CBA976C2-3B3B-4E06-B258-0896CF02163B", "EnableSelfSelection", @"False", "C85491DE-C2D4-4553-809A-2972927C6A32" ); // Request PTO:Person:EnableSelfSelection
            RockMigrationHelper.AddAttributeQualifier( "2574FCAB-22F5-4D91-B217-84AFA2B4A022", "EnableSelfSelection", @"False", "F52FA34A-9CA5-4F7B-88C1-0A343654D23A" ); // Request PTO:Supervisor:EnableSelfSelection
            RockMigrationHelper.AddAttributeQualifier( "37BABFB2-60C3-4A21-82CA-154F731D4EDE", "allowmultiple", @"False", "C7BE1893-9D8D-4418-A552-C1D2BA1F6C7A" ); // Request PTO:Supervisor Attribute:allowmultiple
            RockMigrationHelper.AddAttributeQualifier( "37BABFB2-60C3-4A21-82CA-154F731D4EDE", "entitytype", @"72657ed8-d16e-492e-ac12-144c5e7567e7", "836EF542-5A5D-4A33-9E19-38C630BF0CCE" ); // Request PTO:Supervisor Attribute:entitytype
            RockMigrationHelper.AddAttributeQualifier( "37BABFB2-60C3-4A21-82CA-154F731D4EDE", "qualifierColumn", @"", "3983E5D1-4E68-457B-8804-89EF4B61C5D2" ); // Request PTO:Supervisor Attribute:qualifierColumn
            RockMigrationHelper.AddAttributeQualifier( "37BABFB2-60C3-4A21-82CA-154F731D4EDE", "qualifierValue", @"", "BBA0FDA4-3F22-48CC-B456-5EB8E859401F" ); // Request PTO:Supervisor Attribute:qualifierValue
            RockMigrationHelper.AddAttributeQualifier( "8F85DA13-2602-4438-B58A-CFAA88FBFDDD", "datePickerControlType", @"Date Picker", "56E069BD-56BF-46B2-8B60-74916951548F" ); // Request PTO:Start Date:datePickerControlType
            RockMigrationHelper.AddAttributeQualifier( "8F85DA13-2602-4438-B58A-CFAA88FBFDDD", "displayCurrentOption", @"False", "D386A292-C400-41C5-966D-4A1E62DD6650" ); // Request PTO:Start Date:displayCurrentOption
            RockMigrationHelper.AddAttributeQualifier( "8F85DA13-2602-4438-B58A-CFAA88FBFDDD", "displayDiff", @"False", "925B2F43-23D4-4493-923B-30A1DA7D7AFC" ); // Request PTO:Start Date:displayDiff
            RockMigrationHelper.AddAttributeQualifier( "8F85DA13-2602-4438-B58A-CFAA88FBFDDD", "format", @"", "72BFAE7E-D8F9-4A86-A9D9-697731D36661" ); // Request PTO:Start Date:format
            RockMigrationHelper.AddAttributeQualifier( "8F85DA13-2602-4438-B58A-CFAA88FBFDDD", "futureYearCount", @"", "67CDD506-7DF1-46A9-8621-ACE3E5701063" ); // Request PTO:Start Date:futureYearCount
            RockMigrationHelper.AddAttributeQualifier( "2266B6AB-419F-45F8-880B-F8085F0BCD11", "datePickerControlType", @"Date Picker", "B00B19DF-CD67-443B-A7AD-E4DBF9D5076E" ); // Request PTO:End Date:datePickerControlType
            RockMigrationHelper.AddAttributeQualifier( "2266B6AB-419F-45F8-880B-F8085F0BCD11", "displayCurrentOption", @"False", "174A12BE-3020-4F58-92FD-7187EE8C1CB1" ); // Request PTO:End Date:displayCurrentOption
            RockMigrationHelper.AddAttributeQualifier( "2266B6AB-419F-45F8-880B-F8085F0BCD11", "displayDiff", @"False", "65B15184-45D2-4FE1-A062-5C12FD26B8B6" ); // Request PTO:End Date:displayDiff
            RockMigrationHelper.AddAttributeQualifier( "2266B6AB-419F-45F8-880B-F8085F0BCD11", "format", @"", "DB4CFACB-7E8A-44EC-934C-996E1E58A114" ); // Request PTO:End Date:format
            RockMigrationHelper.AddAttributeQualifier( "2266B6AB-419F-45F8-880B-F8085F0BCD11", "futureYearCount", @"", "C9AC4204-85FD-47BE-9770-09EF88367831" ); // Request PTO:End Date:futureYearCount
            RockMigrationHelper.AddAttributeQualifier( "DBC30BCF-E5E8-4F4B-BEB0-7D1C226C317A", "fieldtype", @"ddl", "FCA3B571-0B27-47E6-8C12-A72801D67BD2" ); // Request PTO:Hours / Day:fieldtype
            RockMigrationHelper.AddAttributeQualifier( "DBC30BCF-E5E8-4F4B-BEB0-7D1C226C317A", "repeatColumns", @"", "C4ED813D-68D0-414D-A6AE-37C3513D369B" ); // Request PTO:Hours / Day:repeatColumns
            RockMigrationHelper.AddAttributeQualifier( "DBC30BCF-E5E8-4F4B-BEB0-7D1C226C317A", "values", @"0.5,1.0,1.5,2.0,2.5,3.0,3.5,4.0,4.5,5.0,5.5,6.0,6.5,7.0,7.5,8.0", "53CA4D9F-B589-40B4-87E3-875A4CCD4DBF" ); // Request PTO:Hours / Day:values
            RockMigrationHelper.AddAttributeQualifier( "66868B80-D1EB-417F-BCAE-F8D75E68FC8C", "allowhtml", @"False", "47E2EC0C-C8E7-47FF-BE05-1EE1C22BC1B1" ); // Request PTO:Reason:allowhtml
            RockMigrationHelper.AddAttributeQualifier( "66868B80-D1EB-417F-BCAE-F8D75E68FC8C", "maxcharacters", @"", "E419467C-30A9-466B-8F0A-4BB739D21617" ); // Request PTO:Reason:maxcharacters
            RockMigrationHelper.AddAttributeQualifier( "66868B80-D1EB-417F-BCAE-F8D75E68FC8C", "numberofrows", @"", "A5705060-77EB-4ABF-878B-5D695933BAFF" ); // Request PTO:Reason:numberofrows
            RockMigrationHelper.AddAttributeQualifier( "66868B80-D1EB-417F-BCAE-F8D75E68FC8C", "showcountdown", @"False", "471D4E90-D2CA-42EA-AF78-876F1C733D1C" ); // Request PTO:Reason:showcountdown
            RockMigrationHelper.AddAttributeQualifier( "593E3676-C196-4265-8BED-7199C9E94833", "ispassword", @"False", "7F3E549D-CCBB-4B32-BD2D-F0AB49706147" ); // Request PTO:Hours Available:ispassword
            RockMigrationHelper.AddAttributeQualifier( "593E3676-C196-4265-8BED-7199C9E94833", "maxcharacters", @"", "3B078BC4-9A31-450E-AE3F-B81B398843C7" ); // Request PTO:Hours Available:maxcharacters
            RockMigrationHelper.AddAttributeQualifier( "593E3676-C196-4265-8BED-7199C9E94833", "showcountdown", @"False", "8ABE1F0C-D115-4A58-9C0B-1ADB646C370B" ); // Request PTO:Hours Available:showcountdown
            RockMigrationHelper.AddAttributeQualifier( "2C4F2FBE-6E72-48FA-B26E-B1C9D4A11DA1", "ispassword", @"False", "55722CE9-732E-45F9-B30D-BD61C0A3D3A5" ); // Request PTO:Error Message:ispassword
            RockMigrationHelper.AddAttributeQualifier( "2C4F2FBE-6E72-48FA-B26E-B1C9D4A11DA1", "maxcharacters", @"", "5BC184CA-20FA-4E09-8414-0AD043149A33" ); // Request PTO:Error Message:maxcharacters
            RockMigrationHelper.AddAttributeQualifier( "2C4F2FBE-6E72-48FA-B26E-B1C9D4A11DA1", "showcountdown", @"False", "8A395D11-5CF2-420C-8F76-3B9B6331084A" ); // Request PTO:Error Message:showcountdown
            RockMigrationHelper.UpdateWorkflowActivityType( "25CA07A0-7662-42C8-9BD6-E2EDCE157795", true, "Start", "", true, 0, "4429A350-7D7D-4D4D-BF19-A584020CEEFE" ); // Request PTO:Start
            RockMigrationHelper.UpdateWorkflowActivityType( "25CA07A0-7662-42C8-9BD6-E2EDCE157795", true, "Validate Request", "", false, 1, "FDEFD76B-AEB9-46FB-927A-4E1AAB91CE04" ); // Request PTO:Validate Request
            RockMigrationHelper.UpdateWorkflowActionForm( @"{% assign errorMsg = Workflow | Attribute:'ErrorMessage' | Trim %}
{% if errorMsg and errorMsg != '' %}
    <div class='alert alert-warning'>{{ errorMsg }}</div>
{% endif %}
<div id='vbsSlotDiv' class='panel panel-block'>
            <div class=""panel-heading panel-follow clearfix"">
                <h1 class=""panel-title"">
                    Available Hours
                </h1>
            </div>
            <div class=""panel-body"">
                <div class='row'>
                    <div class='col-md-3'>
                        <b>Type</b>
                    </div>
                    <div class='col-md-3'>
                    <b> Accrued Time</b>
                    </div>
                    <div class='col-md-3'>
                        <b>Time Spent</b>
                    </div>
                    <div class='col-md-3'>
                        <b>Time Remaining</b>
                    </div>
                </div>
                
                    <div class='row'>
                        <div class='col-md-3'>
                            <b>PTO</b>
                        </div>
                        <div class='col-md-3'>
                            8
                        </div>
                        <div class='col-md-3'>
                            5
                        </div>
                        <div class='col-md-3'>
                        3
                        </div>
                    </div>
                
                    <div class='row'>
                        <div class='col-md-3'>
                            <b>Sick Time</b>
                        </div>
                        <div class='col-md-3'>
                            8
                        </div>
                        <div class='col-md-3'>
                            5
                        </div>
                        <div class='col-md-3'>
                        3
                        </div>
                    </div>
                
                    <div class='row'>
                        <div class='col-md-3'>
                            <b>Sabbatical</b>
                        </div>
                        <div class='col-md-3'>
                            8
                        </div>
                        <div class='col-md-3'>
                            5
                        </div>
                        <div class='col-md-3'>
                        3
                        </div>
                    </div>
            </div>
        </div>", @"", "Submit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^FDEFD76B-AEB9-46FB-927A-4E1AAB91CE04^Your information has been submitted successfully.|", "", false, "", "9A35017A-63F2-4367-8320-B29CE911B136" ); // Request PTO:Start:Form
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "9A35017A-63F2-4367-8320-B29CE911B136", "CBA976C2-3B3B-4E06-B258-0896CF02163B", 0, true, true, false, false, @"<div class='row'><div class='col-md-6'>", @"</div>", "D130BA8F-547E-41C9-92C2-8321586C0F31" ); // Request PTO:Start:Form:Person
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "9A35017A-63F2-4367-8320-B29CE911B136", "2574FCAB-22F5-4D91-B217-84AFA2B4A022", 1, true, true, false, false, @"<div class='col-md-6'>", @"</div></div>", "CBC760C0-BAC5-45AB-A9BB-09F011CD6263" ); // Request PTO:Start:Form:Supervisor
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "9A35017A-63F2-4367-8320-B29CE911B136", "37BABFB2-60C3-4A21-82CA-154F731D4EDE", 2, false, true, false, false, @"", @"", "E761C590-FF7A-40AD-B3DD-B3D0D679D8AF" ); // Request PTO:Start:Form:Supervisor Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "9A35017A-63F2-4367-8320-B29CE911B136", "8F85DA13-2602-4438-B58A-CFAA88FBFDDD", 3, true, false, true, false, @"<div class='row'><div class='col-md-6'>", @"</div>", "2673F3AA-62CF-4F4A-9CAA-307438E4D2BC" ); // Request PTO:Start:Form:Start Date
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "9A35017A-63F2-4367-8320-B29CE911B136", "2266B6AB-419F-45F8-880B-F8085F0BCD11", 4, true, false, true, false, @"<div class='col-md-6'>", @"</div></div>", "A05B2AD8-2A2F-49FE-9F62-8F8A63D2D979" ); // Request PTO:Start:Form:End Date
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "9A35017A-63F2-4367-8320-B29CE911B136", "598E70BB-8A92-483B-834B-2FAF6572C8EE", 5, true, false, true, false, @"<div class='row'><div class='col-md-6'>", @"</div>", "0343AD88-6F3E-4739-9412-595AFE5585BD" ); // Request PTO:Start:Form:PTO Type
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "9A35017A-63F2-4367-8320-B29CE911B136", "DBC30BCF-E5E8-4F4B-BEB0-7D1C226C317A", 6, true, false, true, false, @"<div class='col-md-6'>", @"</div></div>", "FADD2817-B4FC-4C85-B43A-842245AF3C92" ); // Request PTO:Start:Form:Hours / Day
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "9A35017A-63F2-4367-8320-B29CE911B136", "66868B80-D1EB-417F-BCAE-F8D75E68FC8C", 7, true, false, true, false, @"", @"", "46F3D5C2-A011-49DF-A704-ACFC01FF431F" ); // Request PTO:Start:Form:Reason
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "9A35017A-63F2-4367-8320-B29CE911B136", "593E3676-C196-4265-8BED-7199C9E94833", 8, false, true, false, false, @"", @"", "749FB89D-305F-4781-9812-624542251EB4" ); // Request PTO:Start:Form:Hours Available
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "9A35017A-63F2-4367-8320-B29CE911B136", "2C4F2FBE-6E72-48FA-B26E-B1C9D4A11DA1", 9, false, true, false, false, @"", @"", "23CFD428-FE05-4D31-B650-E2FE9547E218" ); // Request PTO:Start:Form:Error Message
            RockMigrationHelper.UpdateWorkflowActionType( "4429A350-7D7D-4D4D-BF19-A584020CEEFE", "Set Person", 0, "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", true, false, "", "CBA976C2-3B3B-4E06-B258-0896CF02163B", 32, "", "5773DFF8-5E54-4AD2-93FD-074097E7C1D8" ); // Request PTO:Start:Set Person
            RockMigrationHelper.UpdateWorkflowActionType( "4429A350-7D7D-4D4D-BF19-A584020CEEFE", "Set Supervisor", 1, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 1, "", "52D4E40D-B4B8-4521-91FD-8446F11B40CD" ); // Request PTO:Start:Set Supervisor
            RockMigrationHelper.UpdateWorkflowActionType( "4429A350-7D7D-4D4D-BF19-A584020CEEFE", "Form", 2, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, true, "9A35017A-63F2-4367-8320-B29CE911B136", "", 1, "", "E5DD2A27-30F5-42D4-9AE6-C437C4D3EDA6" ); // Request PTO:Start:Form
            RockMigrationHelper.UpdateWorkflowActionType( "FDEFD76B-AEB9-46FB-927A-4E1AAB91CE04", "Clear Error Message", 0, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 1, "", "D1BA6262-931B-4B91-9C56-E59D913D1E20" ); // Request PTO:Validate Request:Clear Error Message
            RockMigrationHelper.UpdateWorkflowActionType( "FDEFD76B-AEB9-46FB-927A-4E1AAB91CE04", "Get Hours Available", 1, "A41216D6-6FB0-4019-B222-2C29B4519CF4", true, false, "", "", 1, "", "6F0A6C57-5733-4431-9F82-BE1CB1AEB15F" ); // Request PTO:Validate Request:Get Hours Available
            RockMigrationHelper.UpdateWorkflowActionType( "FDEFD76B-AEB9-46FB-927A-4E1AAB91CE04", "Validate Entry", 2, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 1, "", "2B143418-0DC2-468C-BDB9-524DCC310C0E" ); // Request PTO:Validate Request:Validate Entry
            RockMigrationHelper.UpdateWorkflowActionType( "FDEFD76B-AEB9-46FB-927A-4E1AAB91CE04", "Capture Information Again", 3, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "2C4F2FBE-6E72-48FA-B26E-B1C9D4A11DA1", 64, "", "A3E36BF8-A808-42C2-91EF-AAE00F34F632" ); // Request PTO:Validate Request:Capture Information Again
            RockMigrationHelper.UpdateWorkflowActionType( "FDEFD76B-AEB9-46FB-927A-4E1AAB91CE04", "Save Pto Request", 4, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 1, "", "3902166F-2243-4543-9B66-235858D4F036" ); // Request PTO:Validate Request:Save Pto Request
            RockMigrationHelper.UpdateWorkflowActionType( "FDEFD76B-AEB9-46FB-927A-4E1AAB91CE04", "Launch PTO Approval", 5, "BC21E57A-1477-44B3-A7C2-61A806118945", true, true, "", "", 1, "", "B0DC7B15-7B09-493C-80AA-BA456190786B" ); // Request PTO:Validate Request:Launch PTO Approval
            RockMigrationHelper.AddActionTypeAttributeValue( "5773DFF8-5E54-4AD2-93FD-074097E7C1D8", "DE9CB292-4785-4EA3-976D-3826F91E9E98", @"False" ); // Request PTO:Start:Set Person:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "5773DFF8-5E54-4AD2-93FD-074097E7C1D8", "BBED8A83-8BB2-4D35-BAFB-05F67DCAD112", @"cba976c2-3b3b-4e06-b258-0896cf02163b" ); // Request PTO:Start:Set Person:Person Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "52D4E40D-B4B8-4521-91FD-8446F11B40CD", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{% assign supervisorAttribute = Workflow | Attribute:'SupervisorAttribute','Object' %}
{% assign person = Workflow | Attribute:'Person','Object' %}
{% assign supervisor = person | Attribute:supervisorAttribute.Key,'Object' %}
{{supervisor.PrimaryAlias.Guid}}" ); // Request PTO:Start:Set Supervisor:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "52D4E40D-B4B8-4521-91FD-8446F11B40CD", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // Request PTO:Start:Set Supervisor:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "52D4E40D-B4B8-4521-91FD-8446F11B40CD", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"2574fcab-22f5-4d91-b217-84afa2b4a022" ); // Request PTO:Start:Set Supervisor:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "E5DD2A27-30F5-42D4-9AE6-C437C4D3EDA6", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE", @"False" ); // Request PTO:Start:Form:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "D1BA6262-931B-4B91-9C56-E59D913D1E20", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{{ }}" ); // Request PTO:Validate Request:Clear Error Message:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "D1BA6262-931B-4B91-9C56-E59D913D1E20", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // Request PTO:Validate Request:Clear Error Message:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "D1BA6262-931B-4B91-9C56-E59D913D1E20", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"2c4f2fbe-6e72-48fa-b26e-b1c9d4a11da1" ); // Request PTO:Validate Request:Clear Error Message:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "6F0A6C57-5733-4431-9F82-BE1CB1AEB15F", "F3B9908B-096F-460B-8320-122CF046D1F9", @"Select 4

/* DECLARE @WorkflowId int = {{ Workflow.Id }}
DECLARE @WorkflowTypeId int = {{ Workflow.WorkflowTypeId }}
DECLARE @CampusValue varchar(36) = '{{ Workflow | Attribute:'Campus','RawValue' }}'
DECLARE @DateValue datetime = '{{ Workflow | Attribute:'Date' | Date:'yyyy-MM-dd' }}'
DECLARE @LocationValue varchar(36) = '{{ Workflow | Attribute:'Location','RawValue' }}'
DECLARE @ScheduleValue varchar(36) = '{{ Workflow | Attribute:'Schedule','RawValue' }}'

DECLARE @Capacity INT = ( SELECT TOP 1 [FirmRoomThreshold] FROM [Location] WHERE [Guid] = @LocationValue )
IF @Capacity IS NULL
BEGIN
	SELECT 999
END
ELSE
BEGIN

	DECLARE @WorkflowEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Workflow' )
	DECLARE @CampusAttrId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [EntityTypeId] = @WorkflowEntityTypeId AND [EntityTypeQualifierColumn] = 'WorkflowTypeId' AND [EntityTypeQualifierValue] = @WorkflowTypeId AND [Key] = 'Campus' )
	DECLARE @DateAttrId int = ( SELECT TOP 1 [Id]  FROM [Attribute] WHERE [EntityTypeId] = @WorkflowEntityTypeId AND [EntityTypeQualifierColumn] = 'WorkflowTypeId' AND [EntityTypeQualifierValue] = @WorkflowTypeId AND [Key] = 'Date' )
	DECLARE @LocationAttrId int = ( SELECT TOP 1 [Id]  FROM [Attribute] WHERE [EntityTypeId] = @WorkflowEntityTypeId AND [EntityTypeQualifierColumn] = 'WorkflowTypeId' AND [EntityTypeQualifierValue] = @WorkflowTypeId AND [Key] = 'Location' )
	DECLARE @ScheduleAttrId int = ( SELECT TOP 1 [Id]  FROM [Attribute] WHERE [EntityTypeId] = @WorkflowEntityTypeId AND [EntityTypeQualifierColumn] = 'WorkflowTypeId' AND [EntityTypeQualifierValue] = @WorkflowTypeId AND [Key] = 'Schedule' )
	DECLARE @HowManyAttrId int = ( SELECT TOP 1 [Id]  FROM [Attribute] WHERE [EntityTypeId] = @WorkflowEntityTypeId AND [EntityTypeQualifierColumn] = 'WorkflowTypeId' AND [EntityTypeQualifierValue] = @WorkflowTypeId AND [Key] = 'HowMany' )

	DECLARE @SpotsReserved int = (
		SELECT SUM(HV.[ValueAsNumeric]) 
		FROM [Workflow] W
		INNER JOIN [AttributeValue] CV ON CV.[EntityId] = W.[Id] AND CV.[AttributeId] = @CampusAttrId
		INNER JOIN [AttributeValue] DV ON DV.[EntityId] = W.[Id] AND DV.[AttributeId] = @DateAttrId
		INNER JOIN [AttributeValue] LV ON LV.[EntityId] = W.[Id] AND LV.[AttributeId] = @LocationAttrId
		INNER JOIN [AttributeValue] SV ON SV.[EntityId] = W.[Id] AND SV.[AttributeId] = @ScheduleAttrId
		INNER JOIN [AttributeValue] HV ON HV.[EntityId] = W.[Id] AND HV.[AttributeId] = @HowManyAttrId
		WHERE W.[WorkflowTypeId] = @WorkflowTypeId
		AND W.[Id] <> @WorkflowId
		AND CV.[Value] = @CampusValue
		AND DV.[ValueAsDateTime] = @DateValue
		AND LV.[Value] = @LocationValue
		AND SV.[Value] = @ScheduleValue 
	)

	IF @SpotsReserved IS NULL SET @SpotsReserved = 0

	SELECT @Capacity - @SpotsReserved 

END
*/" ); // Request PTO:Validate Request:Get Hours Available:SQLQuery
            RockMigrationHelper.AddActionTypeAttributeValue( "6F0A6C57-5733-4431-9F82-BE1CB1AEB15F", "A18C3143-0586-4565-9F36-E603BC674B4E", @"False" ); // Request PTO:Validate Request:Get Hours Available:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "6F0A6C57-5733-4431-9F82-BE1CB1AEB15F", "56997192-2545-4EA1-B5B2-313B04588984", @"593e3676-c196-4265-8bed-7199c9e94833" ); // Request PTO:Validate Request:Get Hours Available:Result Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "6F0A6C57-5733-4431-9F82-BE1CB1AEB15F", "D992DB0A-B528-4833-ADCE-61C5BD9BD156", @"True" ); // Request PTO:Validate Request:Get Hours Available:Continue On Error
            RockMigrationHelper.AddActionTypeAttributeValue( "2B143418-0DC2-468C-BDB9-524DCC310C0E", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{% assign hoursDay = Workflow | Attribute:'HoursDay' | AsInteger %}
{% assign hoursAvailable = Workflow | Attribute:'HoursAvailable' | AsInteger %}
{%- if hoursDay > hoursAvailable -%}
    {% capture msg %}{{ msg }}
        <p><strong>Hours Not Available.</strong> There are only {{ hoursAvailable }} hours available.</p>
    {% endcapture %}
{%- endif -%}
{{ msg }}" ); // Request PTO:Validate Request:Validate Entry:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "2B143418-0DC2-468C-BDB9-524DCC310C0E", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // Request PTO:Validate Request:Validate Entry:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "2B143418-0DC2-468C-BDB9-524DCC310C0E", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"2c4f2fbe-6e72-48fa-b26e-b1c9d4a11da1" ); // Request PTO:Validate Request:Validate Entry:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "A3E36BF8-A808-42C2-91EF-AAE00F34F632", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Request PTO:Validate Request:Capture Information Again:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "A3E36BF8-A808-42C2-91EF-AAE00F34F632", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"4429A350-7D7D-4D4D-BF19-A584020CEEFE" ); // Request PTO:Validate Request:Capture Information Again:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "3902166F-2243-4543-9B66-235858D4F036", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"test" ); // Request PTO:Validate Request:Save Pto Request:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "3902166F-2243-4543-9B66-235858D4F036", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // Request PTO:Validate Request:Save Pto Request:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "B0DC7B15-7B09-493C-80AA-BA456190786B", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"test" ); // Request PTO:Validate Request:Launch PTO Approval:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "B0DC7B15-7B09-493C-80AA-BA456190786B", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // Request PTO:Validate Request:Launch PTO Approval:Active

            #endregion

            #region DefinedValue AttributeType qualifier helper

            Sql( @"
			UPDATE [aq] SET [key] = 'definedtype', [Value] = CAST( [dt].[Id] as varchar(5) )
			FROM [AttributeQualifier] [aq]
			INNER JOIN [Attribute] [a] ON [a].[Id] = [aq].[AttributeId]
			INNER JOIN [FieldType] [ft] ON [ft].[Id] = [a].[FieldTypeId]
			INNER JOIN [DefinedType] [dt] ON CAST([dt].[guid] AS varchar(50) ) = [aq].[value]
			WHERE [ft].[class] = 'Rock.Field.Types.DefinedValueFieldType'
			AND [aq].[key] = 'definedtypeguid'
		" );

            #endregion
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
