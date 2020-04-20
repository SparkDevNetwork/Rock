using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock;
using Rock.Model;
using Rock.Plugin;

namespace com.bemaservices.SurveyGizmo.Migrations
{
    [MigrationNumber( 1, "1.8.0" )]
    class Setup : Migration
    {
        public override void Up()
        {
            RockMigrationHelper.UpdateEntityAttribute("Rock.Model.Category","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","EntityTypeId","234","Link All Children Events","",1035,@"","B8C11009-7A46-4214-B9E9-4CD5857491A2","LinkAllChildrenEvents");  
            RockMigrationHelper.UpdateEntityAttribute("Rock.Model.Category","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","EntityTypeId","234","Force Pay Later Payments","",1036,@"","07CB2258-4B23-4CEA-B182-4B4403197999","ForcePayLaterPayments");  
            RockMigrationHelper.UpdateEntityAttribute("Rock.Model.Category","7B34F9D8-6BBA-423E-B50E-525ABB3A1013","EntityTypeId","234","Financial Gateway","",1037,@"369f58a0-de75-4beb-8a9c-ca02f86c3252","00005ED7-97B7-4C63-B35F-8C46BAD405EE","FinancialGateway");  
            RockMigrationHelper.UpdateEntityAttribute("Rock.Model.Category","434D7B6F-F8DD-45B7-8C3E-C76EF10BE56A","EntityTypeId","234","Financial Account","",1038,@"","45DAC967-0C86-4C07-8F28-297972BAC379","FinancialAccount");  
            RockMigrationHelper.UpdateEntityAttribute("Rock.Model.Category","27718256-C1EB-4B1F-9B4B-AC53249F78DF","EntityTypeId","234","Registration Confirmation Text","",1039,@"{% capture currencySymbol %}{{ 'Global' | Attribute:'CurrencySymbol' }}{% endcapture %} {% assign registrantCount = Registrants | Size %} <p>     You have successfully registered the following      {{ Registrant | PluralizeForQuantity:registrantCount | Downcase }} for {{Category.Name}}: </p>  {% for registration in Registrations %} <h3>{{registration.RegistrationInstance.Name}}</h3> <ul> {% for registrant in registration.Registrants %}     <li>              <strong>{{ registrant.PersonAlias.Person.FullName }}</strong>                  {% if registrant.Cost > 0 %}             - {{ currencySymbol }}{{ registrant.Cost | Format'#,##0.00' }}         {% endif %}                  {% assign feeCount = registrant.Fees | Size %}         {% if feeCount > 0 %}             <br/>{{ registration.RegistrationInstance.RegistrationTemplate.FeeTerm | PluralizeForQuantity:registrantCount }}:             <ul class='list-unstyled'>             {% for fee in registrant.Fees %}                 <li>                     {{ fee.RegistrationTemplateFee.Name }} {{ fee.Option }}                     {% if fee.Quantity > 1 %} ({{ fee.Quantity }} @ {{ currencySymbol }}{{ fee.Cost | Format'#,##0.00' }}){% endif %}: {{ currencySymbol }}{{ fee.TotalCost | Format'#,##0.00' }}                 </li>             {% endfor %}             </ul>         {% endif %}              </li> {% endfor %} </ul>  {% if registration.TotalCost > 0 %} <p>     Total Due: {{ currencySymbol }}{{ registration.TotalCost | Format''#,##0.00'' }}<br/>     {% for payment in registration.Payments %}         Paid {{ currencySymbol }}{{ payment.Amount | Format''#,##0.00'' }} on {{ payment.Transaction.TransactionDateTime| Date:'M/d/yyyy' }}          <small>(Ref #: {{ payment.Transaction.TransactionCode }})</small><br/>     {% endfor %}     {% assign paymentCount = registration.Payments | Size %}     {% if paymentCount > 1 %}         Total Paid: {{ currencySymbol }}{{ registration.TotalPaid | Format''#,##0.00'' }}<br/>     {% endif %}     Balance Due: {{ currencySymbol }}{{ registration.BalanceDue | Format''#,##0.00'' }} </p> {% endif %} {% if forloop.last %} <p>     A confirmation email has been sent to {{ registration.ConfirmationEmail }}. If you have any questions      please contact {{ registration.RegistrationInstance.ContactPersonAlias.Person.FullName }} at {{ registration.RegistrationInstance.ContactEmail }}. </p> {% endif %} {% endfor %}   ","8E5E1A5D-FCDB-4061-9395-40ED9443B3D7","RegistrationConfirmationText");  
            RockMigrationHelper.UpdateEntityAttribute("Rock.Model.Category","9C204CD0-1233-41C5-818A-C5DA439445AA","EntityTypeId","234","Batch Name Prefix","",1040,@"","89290AE8-683F-480F-9B91-4A04B52BEC96","BatchNamePrefix");

            // LinkAllChildrenEvents
            RockMigrationHelper.UpdateAttributeQualifier("B8C11009-7A46-4214-B9E9-4CD5857491A2","falsetext",@"No","1712CCFE-E9FD-4134-9954-54C5AC2BA91D");  
            // LinkAllChildrenEvents
            RockMigrationHelper.UpdateAttributeQualifier("B8C11009-7A46-4214-B9E9-4CD5857491A2","truetext",@"Yes","9F57B5C2-C8BA-441E-B3BA-DA5DFAA0E34F");  
            // ForcePayLaterPayments
            RockMigrationHelper.UpdateAttributeQualifier("07CB2258-4B23-4CEA-B182-4B4403197999","falsetext",@"No","9EACE315-5477-4E4E-8C5C-F862A6A9FB98");  
            // ForcePayLaterPayments
            RockMigrationHelper.UpdateAttributeQualifier("07CB2258-4B23-4CEA-B182-4B4403197999","truetext",@"Yes","0226E079-6A22-4C32-B515-A8A67487ED68");  
            // FinancialAccount
            RockMigrationHelper.UpdateAttributeQualifier("45DAC967-0C86-4C07-8F28-297972BAC379","displaypublicname",@"True","8D0BBE56-4B2F-43A8-AFCF-5812B6859C81");  
            // RegistrationConfirmationText
            RockMigrationHelper.UpdateAttributeQualifier("8E5E1A5D-FCDB-4061-9395-40ED9443B3D7","editorHeight",@"","93FEA08F-A487-43AC-8B07-54B7F382A195");  
            // RegistrationConfirmationText
            RockMigrationHelper.UpdateAttributeQualifier("8E5E1A5D-FCDB-4061-9395-40ED9443B3D7","editorMode",@"0","DDC547DC-8195-41F5-8CC2-5B636F962119");  
            // RegistrationConfirmationText
            RockMigrationHelper.UpdateAttributeQualifier("8E5E1A5D-FCDB-4061-9395-40ED9443B3D7","editorTheme",@"0","C0A363BB-EC35-43E5-A9AB-400A998BB507");  
            // BatchNamePrefix
            RockMigrationHelper.UpdateAttributeQualifier("89290AE8-683F-480F-9B91-4A04B52BEC96","ispassword",@"False","C5E55502-4AC8-46A2-830B-55E0C9D06670");  
            // BatchNamePrefix
            RockMigrationHelper.UpdateAttributeQualifier("89290AE8-683F-480F-9B91-4A04B52BEC96","maxcharacters",@"","008C5D5C-FC42-4CBC-A3AD-A383A3C330D8");  
            // BatchNamePrefix
            RockMigrationHelper.UpdateAttributeQualifier("89290AE8-683F-480F-9B91-4A04B52BEC96","showcountdown",@"False","CF0465BE-ECF4-482A-B091-2ADA03EFBC7E");  

            // Page: Multi Event Registration
            RockMigrationHelper.AddPage( "2E6FED28-683F-4726-8CF1-2822E8E73B03", "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Multi Event Registration", "", "81BA3CDD-220F-45AA-AD45-43F53FFD68DA", "" ); // Site:External Website
            RockMigrationHelper.UpdateBlockType( "Multi-Event Registration Wizard", "Block used to register for multiple registration instances.", "~/Plugins/com_bemaservices/Event/MultiEventRegistrationWizard.ascx", "BEMA Services > Event", "BD314E14-D57D-4874-80C2-60680278E767" );
            // Add Block to Page: Multi Event Registration, Site: External Website
            RockMigrationHelper.AddBlock( true, "81BA3CDD-220F-45AA-AD45-43F53FFD68DA", "", "BD314E14-D57D-4874-80C2-60680278E767", "Multi-Event Registration Wizard", "Main", "", "", 0, "1B3F7E66-BB5C-4D91-B9F2-5A29578A9461" );
            // Attrib for BlockType: Multi-Event Registration Wizard:Connection Status
            RockMigrationHelper.UpdateBlockTypeAttribute( "BD314E14-D57D-4874-80C2-60680278E767", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "", "The connection status to use for new individuals (default: 'Web Prospect'.)", 0, @"368DD475-242C-49C4-A42C-7278BE690CC2", "8BB56FC9-928F-4E94-A4B3-BBA3F2B7AD74" );
            // Attrib for BlockType: Multi-Event Registration Wizard:Record Status
            RockMigrationHelper.UpdateBlockTypeAttribute( "BD314E14-D57D-4874-80C2-60680278E767", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "", "The record status to use for new individuals (default: 'Pending'.)", 1, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "52317BE0-D201-4B5E-8907-D5890D961AC8" );
            // Attrib for BlockType: Multi-Event Registration Wizard:Source
            RockMigrationHelper.UpdateBlockTypeAttribute( "BD314E14-D57D-4874-80C2-60680278E767", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Source", "Source", "", "The Financial Source Type to use when creating transactions", 2, @"7D705CE7-7B11-4342-A58E-53617C5B4E69", "E8BA4BA7-CE79-46B2-A046-CC6BFCFE37CE" );
            // Attrib for BlockType: Multi-Event Registration Wizard:Batch Name Prefix
            RockMigrationHelper.UpdateBlockTypeAttribute( "BD314E14-D57D-4874-80C2-60680278E767", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Batch Name Prefix", "BatchNamePrefix", "", "The batch prefix name to use when creating a new batch", 3, @"Event Registration", "D44753E3-DA00-481E-98F1-EC63E1EB8F53" );
            // Attrib for BlockType: Multi-Event Registration Wizard:Display Progress Bar
            RockMigrationHelper.UpdateBlockTypeAttribute( "BD314E14-D57D-4874-80C2-60680278E767", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Progress Bar", "DisplayProgressBar", "", "Display a progress bar for the registration.", 4, @"True", "A327B788-0918-498A-997E-79953309590B" );
            // Attrib for BlockType: Multi-Event Registration Wizard:Allow InLine Digital Signature Documents
            RockMigrationHelper.UpdateBlockTypeAttribute( "BD314E14-D57D-4874-80C2-60680278E767", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow InLine Digital Signature Documents", "SignInline", "", "Should inline digital documents be allowed? This requires that the registration template is configured to display the document inline", 6, @"True", "8013AE5B-E34D-4600-975B-677E2EA6125D" );
            // Attrib for BlockType: Multi-Event Registration Wizard:Confirm Account Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "BD314E14-D57D-4874-80C2-60680278E767", "08F3003B-F3E2-41EC-BDF1-A2B7AC2908CF", "Confirm Account Template", "ConfirmAccountTemplate", "", "Confirm Account Email Template", 7, @"17aaceef-15ca-4c30-9a3a-11e6cf7e6411", "DAF15B89-71CE-48A8-832E-2D7B76D4B456" );
            // Attrib for BlockType: Multi-Event Registration Wizard:Family Term
            RockMigrationHelper.UpdateBlockTypeAttribute( "BD314E14-D57D-4874-80C2-60680278E767", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Family Term", "FamilyTerm", "", "The term to use for specifying which household or family a person is a member of.", 8, @"immediate family", "B634815C-D8EF-4468-8A8A-6ADFC14FE03A" );
            // Attrib for BlockType: Multi-Event Registration Wizard:Force Email Update
            RockMigrationHelper.UpdateBlockTypeAttribute( "BD314E14-D57D-4874-80C2-60680278E767", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Force Email Update", "ForceEmailUpdate", "", "Force the email to be updated on the person's record.", 9, @"False", "BE23BE0A-B358-4167-AA44-327F8E1DB6ED" );
            // Attrib for BlockType: Multi-Event Registration Wizard:Show Field Descriptions

            RockMigrationHelper.UpdateBlockTypeAttribute( "BD314E14-D57D-4874-80C2-60680278E767", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Field Descriptions", "ShowFieldDescriptions", "", "Show the field description as help text", 10, @"True", "DBC04D65-FBE9-4D15-B030-D65E8DABFE0C" );
            // Attrib for BlockType: Multi-Event Registration Wizard:Hide First and Last Name fields
            RockMigrationHelper.UpdateBlockTypeAttribute( "BD314E14-D57D-4874-80C2-60680278E767", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide First and Last Name fields", "HideFirstLastNameFields", "", "Hide first and last name fields, and skip any forms that are only those fields.", 10, @"True", "7FAD1694-ED26-435D-AB81-7FF42116925B" );
        }

        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "7FAD1694-ED26-435D-AB81-7FF42116925B" );
            RockMigrationHelper.DeleteAttribute( "D44753E3-DA00-481E-98F1-EC63E1EB8F53" );
            RockMigrationHelper.DeleteAttribute( "E8BA4BA7-CE79-46B2-A046-CC6BFCFE37CE" );
            RockMigrationHelper.DeleteAttribute( "52317BE0-D201-4B5E-8907-D5890D961AC8" );
            RockMigrationHelper.DeleteAttribute( "8BB56FC9-928F-4E94-A4B3-BBA3F2B7AD74" );
            RockMigrationHelper.DeleteAttribute( "DBC04D65-FBE9-4D15-B030-D65E8DABFE0C" );
            RockMigrationHelper.DeleteAttribute( "BE23BE0A-B358-4167-AA44-327F8E1DB6ED" );
            RockMigrationHelper.DeleteAttribute( "B634815C-D8EF-4468-8A8A-6ADFC14FE03A" );
            RockMigrationHelper.DeleteAttribute( "8013AE5B-E34D-4600-975B-677E2EA6125D" );
            RockMigrationHelper.DeleteAttribute( "A327B788-0918-498A-997E-79953309590B" );
            RockMigrationHelper.DeleteAttribute( "DAF15B89-71CE-48A8-832E-2D7B76D4B456" );
            RockMigrationHelper.DeleteBlock( "1B3F7E66-BB5C-4D91-B9F2-5A29578A9461" );
            RockMigrationHelper.DeleteBlockType( "BD314E14-D57D-4874-80C2-60680278E767" );
            RockMigrationHelper.DeletePage( "81BA3CDD-220F-45AA-AD45-43F53FFD68DA" ); //  Page: Multi Event Registration

            RockMigrationHelper.DeleteAttribute("B8C11009-7A46-4214-B9E9-4CD5857491A2");    // Rock.Model.Category: Link All Children Events  
            RockMigrationHelper.DeleteAttribute("07CB2258-4B23-4CEA-B182-4B4403197999");    // Rock.Model.Category: Force Pay Later Payments  
            RockMigrationHelper.DeleteAttribute("00005ED7-97B7-4C63-B35F-8C46BAD405EE");    // Rock.Model.Category: Financial Gateway  
            RockMigrationHelper.DeleteAttribute("45DAC967-0C86-4C07-8F28-297972BAC379");    // Rock.Model.Category: Financial Account  
            RockMigrationHelper.DeleteAttribute("8E5E1A5D-FCDB-4061-9395-40ED9443B3D7");    // Rock.Model.Category: Registration Confirmation Text  
            RockMigrationHelper.DeleteAttribute("89290AE8-683F-480F-9B91-4A04B52BEC96");    // Rock.Model.Category: Batch Name Prefix  
        }
    }
}