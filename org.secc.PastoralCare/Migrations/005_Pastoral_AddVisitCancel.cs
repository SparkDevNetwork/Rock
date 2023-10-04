// <copyright>
// Copyright Southeast Christian Church
//
// Licensed under the  Southeast Christian Church License (the "License");
// you may not use this file except in compliance with the License.
// A copy of the License shoud be included with this file.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock;
using Rock.Plugin;
using Rock.Web.Cache;

namespace org.secc.PastoralCare.Migrations
{
    [MigrationNumber( 5, "1.2.0" )]
    class Pastoral_AddVisitCancel : Migration
    {
        public override void Up()
        {

            // Update the footer on the Summary User Entry Forms to only show valid visits
            RockMigrationHelper.UpdateWorkflowActionForm( @"<div class=""panel panel-default"">  <div class=""panel-heading"">   <div class=""panel-title"">    <i class=""fa fa-info-circle""></i> Homebound Resident Information   </div>  </div>  <div class=""panel-body"">      <div class=""col-md-6"">          <div class=""form-group static-control"" style=""margin-top: 57px;"">      <label class=""control-label"">Age</label><div class=""control-wrapper"">       <p class=""form-control-static"">{{ Workflow | Attribute:'HomeboundPerson','Age' }}</p>      </div>     </div>", @"    </div> </div>{% capture visitList %} <div class=""panel panel-default"">     <div class=""panel-heading"">         <div class=""panel-title"">             <i class=""fa fa-calendar""></i> Visit History         </div>     </div>     <div class=""panel-body"">         <table class=""table table-striped table-bordered"">             <thead>                 <tr>                     <th>Visitor</th>                     <th>Date</th>                     <th>Visit Notes</th>                 </tr>             </thead>             <tbody>             {% for activity in Workflow.Activities reversed %}                 {% assign visitDateSize = activity | Attribute:'VisitDate' | Size %}{% if activity.ActivityType.Name == 'Visitation Info' and visitDateSize > 0   %}{% assign showVisits = true %}                     <tr>                         <td>{{ activity.Visitor }}</td>                         <td>{{ activity.VisitDate }}</td>                         <td>{{ activity.VisitNote }}</td>                     </tr>                 {% endif %}             {% endfor %}             </tbody>         </table>     </div> </div>{% endcapture %}{% if showVisits == 'true' %}{{visitList}}{% endif %}", "Edit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^F5F74E70-3643-4122-9221-5FAAB0DBF47F^Your information has been submitted successfully.|Add Visit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^61E356D3-2A02-457B-9A43-B67806C67A45^|Close^fdc397cd-8b4a-436e-bea1-bce2e6717c03^9F9FEF18-1259-43D1-B922-C37C17B7D9D8^|", "", true, "", "87FDE195-3C3E-408C-BD42-19A9FE01D224" ); // Homebound Resident:Homebound Resident Summary:Entry Form  
            RockMigrationHelper.UpdateWorkflowActionForm( @"<div class=""panel panel-default"">  <div class=""panel-heading"">   <div class=""panel-title"">    <i class=""fa fa-info-circle""></i> Resident Information   </div>  </div>  <div class=""panel-body"">      <div class=""col-md-6"">          <div class=""form-group static-control"" style=""margin-top: 57px;"">      <label class=""control-label"">Age</label><div class=""control-wrapper"">       <p class=""form-control-static"">{{ Workflow | Attribute:'PersonToVisit','Age' }}&nbsp;</p>      </div>     </div>", @"    </div> </div>{% capture visitList %} <div class=""panel panel-default"">     <div class=""panel-heading"">         <div class=""panel-title"">             <i class=""fa fa-calendar""></i> Visit History         </div>     </div>     <div class=""panel-body"">         <table class=""table table-striped table-bordered"">             <thead>                 <tr>                     <th>Visitor</th>                     <th>Date</th>                     <th>Visit Notes</th>                 </tr>             </thead>             <tbody>             {% for activity in Workflow.Activities reversed %}                 {% assign visitDateSize = activity | Attribute:'VisitDate' | Size %}{% if activity.ActivityType.Name == 'Visitation Info' and visitDateSize > 0   %}{% assign showVisits = true %}                     <tr>                         <td>{{ activity.Visitor }}</td>                         <td>{{ activity.VisitDate }}</td>                         <td>{{ activity.VisitNote }}</td>                     </tr>                 {% endif %}             {% endfor %}             </tbody>         </table>     </div> </div>{% endcapture %}{% if showVisits == 'true' %}{{visitList}}{% endif %}", "Edit Request^fdc397cd-8b4a-436e-bea1-bce2e6717c03^c5bc1151-7a01-4422-8a67-fc495c2d937c^Your information has been submitted successfully.|Add Visit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^6dcf3214-b88f-4d4e-9251-3cf4a8cbea6b^|Discharge^fdc397cd-8b4a-436e-bea1-bce2e6717c03^8a694803-6cce-4be1-b3c5-5ff23e0e5539^", "", true, "", "4992F4AB-998B-4939-B442-AF93A11485F0" ); // Nursing Home Resident:Resident Summary Info:Entry Form 
            RockMigrationHelper.UpdateWorkflowActionForm( @"<div class=""panel panel-default"">  <div class=""panel-heading"">   <div class=""panel-title"">    <i class=""fa fa-info-circle""></i> Patient Information   </div>  </div>  <div class=""panel-body"">      <div class=""col-md-6"">          <div class=""form-group static-control"" style=""margin-top: 57px;"">      <label class=""control-label"">Age</label><div class=""control-wrapper"">       <p class=""form-control-static"">{{ Workflow | Attribute:'PersonToVisit','Age' }}&nbsp;</p>      </div>     </div>", @"    </div> </div>{% capture visitList %} <div class=""panel panel-default"">     <div class=""panel-heading"">         <div class=""panel-title"">             <i class=""fa fa-calendar""></i> Visit History         </div>     </div>     <div class=""panel-body"">         <table class=""table table-striped table-bordered"">             <thead>                 <tr>                     <th>Visitor</th>                     <th>Date</th>                     <th>Visit Notes</th>                 </tr>             </thead>             <tbody>             {% for activity in Workflow.Activities reversed %}                 {% assign visitDateSize = activity | Attribute:'VisitDate' | Size %}{% if activity.ActivityType.Name == 'Visitation Info' and visitDateSize > 0   %}{% assign showVisits = true %}                     <tr>                         <td>{{ activity.Visitor }}</td>                         <td>{{ activity.VisitDate }}</td>                         <td>{{ activity.VisitNote }}</td>                     </tr>                 {% endif %}             {% endfor %}             </tbody>         </table>     </div> </div>{% endcapture %}{% if showVisits == 'true' %}{{visitList}}{% endif %}", "Edit Request^fdc397cd-8b4a-436e-bea1-bce2e6717c03^00E9E98E-2027-4B7D-AAAD-87D11B0EC06B^Your information has been submitted successfully.|Add Visit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^8E7746D0-5920-434B-BC61-78333100740A^|Discharge^fdc397cd-8b4a-436e-bea1-bce2e6717c03^03FCE7A7-82BE-49E2-9A67-05D31AE0E1CA^|", "", true, "", "7E6636A3-52C3-4727-8B9E-0DB50406BCF8" ); // Hospital Admission:Patient Summary Info:Entry Form  

            // Create the Attribute Command attributes
            RockMigrationHelper.UpdateWorkflowActivityTypeAttribute( "61E356D3-2A02-457B-9A43-B67806C67A45", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Activity Command", "ActivityCommand", "", 3, @"", "7ACF21F1-D8B8-4B51-A040-95B7E4FFA74B" ); // Homebound Resident:Visitation Info:Activity Command
            RockMigrationHelper.AddAttributeQualifier( "7ACF21F1-D8B8-4B51-A040-95B7E4FFA74B", "ispassword", @"False", "90CDEA76-75FC-4BB7-9789-A202CEB7B153" ); // Hospital Admission:Activity Command:ispassword
            RockMigrationHelper.UpdateWorkflowActivityTypeAttribute( "6DCF3214-B88F-4D4E-9251-3CF4A8CBEA6B", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Activity Command", "ActivityCommand", "", 3, @"", "7ACF21F1-D8B8-4B51-A040-95B7E4FFA74C" ); // Nursing Home Resident:Visitation Info:Activity Command
            RockMigrationHelper.AddAttributeQualifier( "7ACF21F1-D8B8-4B51-A040-95B7E4FFA74C", "ispassword", @"False", "90CDEA76-75FC-4BB7-9789-A202CEB7B154" ); // Hospital Admission:Activity Command:ispassword
            RockMigrationHelper.UpdateWorkflowActivityTypeAttribute( "8E7746D0-5920-434B-BC61-78333100740A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Activity Command", "ActivityCommand", "", 3, @"", "7ACF21F1-D8B8-4B51-A040-95B7E4FFA74D" ); // Hospital Admission:Visitation Info:Activity Command
            RockMigrationHelper.AddAttributeQualifier( "7ACF21F1-D8B8-4B51-A040-95B7E4FFA74D", "ispassword", @"False", "90CDEA76-75FC-4BB7-9789-A202CEB7B155" ); // Hospital Admission:Activity Command:ispassword

            // Add the cancel button
            RockMigrationHelper.UpdateWorkflowActionForm( @"", @"", "Save Visit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^3b165553-0d50-4d1d-a53f-f4fff50cbe3f^Your information has been submitted successfully.|Cancel^5683e775-b9f3-408c-80ac-94de0e51cf3a^3b165553-0d50-4d1d-a53f-f4fff50cbe3f^|", "", true, "7ACF21F1-D8B8-4B51-A040-95B7E4FFA74B", "D8CC1582-5F0A-4270-931D-C9AE1BF91672" ); // Homebound Resident:Visitation Info:Form Entry  
            RockMigrationHelper.UpdateWorkflowActionForm( @"", @"", "Save Visit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^b55f9a75-d6fa-4854-b67d-4c84afa0883d^Your information has been submitted successfully.|Cancel^5683e775-b9f3-408c-80ac-94de0e51cf3a^b55f9a75-d6fa-4854-b67d-4c84afa0883d^|", "", true, "7ACF21F1-D8B8-4B51-A040-95B7E4FFA74C", "41C3DD25-249D-42E7-90E4-F2ED09A40938" ); // Nursing Home Resident:Visitation Info:Form Entry  
            RockMigrationHelper.UpdateWorkflowActionForm( @"", @"", "Save Visit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^A24F45F9-CDCC-42C9-8457-7F4438F9C0F9^Your information has been submitted successfully.|Cancel^5683e775-b9f3-408c-80ac-94de0e51cf3a^A24F45F9-CDCC-42C9-8457-7F4438F9C0F9^|", "", true, "7ACF21F1-D8B8-4B51-A040-95B7E4FFA74D", "62E4A560-CBBA-4ABB-9BA0-36E3408B7B49" ); // Hospital Admission:Visitation Info:Form Entry

            // Add actions to clear the visit date
            RockMigrationHelper.UpdateWorkflowActionType( "61E356D3-2A02-457B-9A43-B67806C67A45", "Clear the Visit Date", 2, "C789E457-0783-44B3-9D8F-2EBAB5F11110", true, false, "", "7ACF21F1-D8B8-4B51-A040-95B7E4FFA74B", 1, "Cancel", "044E0BC8-FDE1-44A2-8796-264E8D1B3489" ); // Homebound Resident:Visitation Info:Clear the Visit Date
            RockMigrationHelper.AddActionTypeAttributeValue( "044E0BC8-FDE1-44A2-8796-264E8D1B3489", "D7EAA859-F500-4521-9523-488B12EAA7D2", @"False" ); // Homebound Resident:Visitation Info:Clear the Visit Date:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "044E0BC8-FDE1-44A2-8796-264E8D1B3489", "44A0B977-4730-4519-8FF6-B0A01A95B212", @"5F961235-C39A-41E1-BA6E-A030815F8AAB" ); // Homebound Resident:Visitation Info:Clear the Visit Date:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "044E0BC8-FDE1-44A2-8796-264E8D1B3489", "57093B41-50ED-48E5-B72B-8829E62704C8", @"" ); // Homebound Resident:Visitation Info:Clear the Visit Date:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "044E0BC8-FDE1-44A2-8796-264E8D1B3489", "E5272B11-A2B8-49DC-860D-8D574E2BC15C", @"" ); // Homebound Resident:Visitation Info:Clear the Visit Date:Text Value|Attribute Value

            RockMigrationHelper.UpdateWorkflowActionType( "6DCF3214-B88F-4D4E-9251-3CF4A8CBEA6B", "Clear the Visit Date", 2, "C789E457-0783-44B3-9D8F-2EBAB5F11110", true, false, "", "7ACF21F1-D8B8-4B51-A040-95B7E4FFA74C", 1, "Cancel", "044E0BC8-FDE1-44A2-8796-264E8D1B3490" ); // Nursing Home Resident:Visitation Info:Clear the Visit Date
            RockMigrationHelper.AddActionTypeAttributeValue( "044E0BC8-FDE1-44A2-8796-264E8D1B3490", "D7EAA859-F500-4521-9523-488B12EAA7D2", @"False" ); // Nursing Home Resident:Visitation Info:Clear the Visit Date:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "044E0BC8-FDE1-44A2-8796-264E8D1B3490", "44A0B977-4730-4519-8FF6-B0A01A95B212", @"6800068F-D799-4787-8241-948F1F910949" ); // Nursing Home Resident:Visitation Info:Clear the Visit Date:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "044E0BC8-FDE1-44A2-8796-264E8D1B3490", "57093B41-50ED-48E5-B72B-8829E62704C8", @"" ); // Nursing Home Resident:Visitation Info:Clear the Visit Date:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "044E0BC8-FDE1-44A2-8796-264E8D1B3490", "E5272B11-A2B8-49DC-860D-8D574E2BC15C", @"" ); // Nursing Home Resident:Visitation Info:Clear the Visit Date:Text Value|Attribute Value

            RockMigrationHelper.UpdateWorkflowActionType( "8E7746D0-5920-434B-BC61-78333100740A", "Clear the Visit Date", 2, "C789E457-0783-44B3-9D8F-2EBAB5F11110", true, false, "", "7ACF21F1-D8B8-4B51-A040-95B7E4FFA74D", 1, "Cancel", "044E0BC8-FDE1-44A2-8796-264E8D1B3491" ); // Hospital Admission:Visitation Info:Clear the Visit Date
            RockMigrationHelper.AddActionTypeAttributeValue( "044E0BC8-FDE1-44A2-8796-264E8D1B3491", "D7EAA859-F500-4521-9523-488B12EAA7D2", @"False" ); // Hospital Admission:Visitation Info:Clear the Visit Date:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "044E0BC8-FDE1-44A2-8796-264E8D1B3491", "44A0B977-4730-4519-8FF6-B0A01A95B212", @"A411555B-D3F9-4341-A27A-69C767C8EE83" ); // Hospital Admission:Visitation Info:Clear the Visit Date:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "044E0BC8-FDE1-44A2-8796-264E8D1B3491", "57093B41-50ED-48E5-B72B-8829E62704C8", @"" ); // Hospital Admission:Visitation Info:Clear the Visit Date:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "044E0BC8-FDE1-44A2-8796-264E8D1B3491", "E5272B11-A2B8-49DC-860D-8D574E2BC15C", @"" ); // Hospital Admission:Visitation Info:Clear the Visit Date:Text Value|Attribute Value

            // Add an action to make sure the activity is complete
            RockMigrationHelper.UpdateWorkflowActionType( "61E356D3-2A02-457B-9A43-B67806C67A45", "Activity Complete", 3, "0D5E33A5-8700-4168-A42E-74D78B62D717", true, true, "", "", 1, "", "A4EA2FB5-75F6-4F44-9D73-B0CB7E9C58C8" ); // Homebound Resident:Visitation Info:Activity Complete
            RockMigrationHelper.AddActionTypeAttributeValue( "A4EA2FB5-75F6-4F44-9D73-B0CB7E9C58C8", "27BCE39D-9E9B-4704-AAD0-4E8ADC114F72", @"" ); // Homebound Resident:Visitation Info:Activity Complete:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "A4EA2FB5-75F6-4F44-9D73-B0CB7E9C58C8", "1CFB9BFA-6665-4058-8E28-1FD9D5F8F69A", @"False" ); // Homebound Resident:Visitation Info:Activity Complete:Active

            RockMigrationHelper.UpdateWorkflowActionType( "6DCF3214-B88F-4D4E-9251-3CF4A8CBEA6B", "Activity Complete", 3, "0D5E33A5-8700-4168-A42E-74D78B62D717", true, true, "", "", 1, "", "A4EA2FB5-75F6-4F44-9D73-B0CB7E9C58C9" ); // Nursing Home Resident:Visitation Info:Activity Complete
            RockMigrationHelper.AddActionTypeAttributeValue( "A4EA2FB5-75F6-4F44-9D73-B0CB7E9C58C9", "27BCE39D-9E9B-4704-AAD0-4E8ADC114F72", @"" ); // Nursing Home Resident:Visitation Info:Activity Complete:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "A4EA2FB5-75F6-4F44-9D73-B0CB7E9C58C9", "1CFB9BFA-6665-4058-8E28-1FD9D5F8F69A", @"False" ); // Nursing Home Resident:Visitation Info:Activity Complete:Active

            RockMigrationHelper.UpdateWorkflowActionType( "8E7746D0-5920-434B-BC61-78333100740A", "Activity Complete", 3, "0D5E33A5-8700-4168-A42E-74D78B62D717", true, true, "", "", 1, "", "A4EA2FB5-75F6-4F44-9D73-B0CB7E9C58CA" ); // Hospital Admission:Visitation Info:Activity Complete
            RockMigrationHelper.AddActionTypeAttributeValue( "A4EA2FB5-75F6-4F44-9D73-B0CB7E9C58CA", "27BCE39D-9E9B-4704-AAD0-4E8ADC114F72", @"" ); // Hospital Admission:Visitation Info:Activity Complete:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "A4EA2FB5-75F6-4F44-9D73-B0CB7E9C58CA", "1CFB9BFA-6665-4058-8E28-1FD9D5F8F69A", @"False" ); // Hospital Admission:Visitation Info:Activity Complete:Active

        }
        public override void Down()
        {
            // There is no going back.
        }
    }
}

