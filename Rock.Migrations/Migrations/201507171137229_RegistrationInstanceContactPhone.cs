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
    public partial class RegistrationInstanceContactPhone : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.RegistrationInstance", "ContactPhone", c => c.String( maxLength: 50 ) );

            Sql( MigrationSQL._201507171137229_RegistrationInstanceContactPhone );

            RockMigrationHelper.AddPageRoute( "F7CA6E0F-C319-47AB-9A6D-247C5716D846", "Registration/{CampusId}/{GroupId}" );
            RockMigrationHelper.AddPageRoute( "F7CA6E0F-C319-47AB-9A6D-247C5716D846", "Registration/{Slug}" );

            RockMigrationHelper.AddPage( "FC81099A-2F98-4EBA-AC5A-8300B2FE46C4", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Registrant", "", "52CA0336-FC25-4131-BB5A-94A628C0EE77", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "FC81099A-2F98-4EBA-AC5A-8300B2FE46C4", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Payment", "", "D5049786-CD67-45FA-83A9-4EE663F8FC5A", "" ); // Site:Rock RMS

            RockMigrationHelper.UpdateBlockType( "Registrant Detail", "Displays interface for editing the registration attribute values and fees for a given registrant.", "~/Blocks/Event/RegistrantDetail.ascx", "Event", "D72A1A61-43D1-4D5D-92EC-BAECA02EAC43" );

            // Add Block to Page: Payment, Site: Rock RMS
            RockMigrationHelper.AddBlock( "D5049786-CD67-45FA-83A9-4EE663F8FC5A", "", "1DE16F87-4A49-4A3C-A03E-B8488ECBEEBE", "Transaction Detail", "Main", "", "", 0, "64C35D4A-736D-4A9B-A2AB-79BD61ACF298" );
            // Add Block to Page: Registrant, Site: Rock RMS
            RockMigrationHelper.AddBlock( "52CA0336-FC25-4131-BB5A-94A628C0EE77", "", "D72A1A61-43D1-4D5D-92EC-BAECA02EAC43", "Registrant Detail", "Main", "", "", 0, "2F125E9E-5027-4EFE-B9F4-0FA047D7CF45" );

            // Attrib for BlockType: Registration Detail:Batch Name Prefix
            RockMigrationHelper.AddBlockTypeAttribute( "A1C967B2-EEDA-416F-A53C-7BE46D6DA4E1", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Batch Name Prefix", "BatchNamePrefix", "", "The batch prefix name to use when creating a new batch", 5, @"Event Registration", "03770487-DC57-4DEE-A0DB-F8A5B22F96C2" );
            // Attrib for BlockType: Registration Detail:Transaction Page
            RockMigrationHelper.AddBlockTypeAttribute( "A1C967B2-EEDA-416F-A53C-7BE46D6DA4E1", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Transaction Page", "TransactionPage", "", "The page for viewing transaction details", 1, @"", "352DE424-2299-404E-B86F-FEA46D588D2D" );
            // Attrib for BlockType: Registration Detail:Registrant Page
            RockMigrationHelper.AddBlockTypeAttribute( "A1C967B2-EEDA-416F-A53C-7BE46D6DA4E1", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Registrant Page", "RegistrantPage", "", "The page for viewing details about a registrant", 0, @"", "DA8437F4-B8A6-4C52-9111-8ADD868E0392" );
            // Attrib for BlockType: Registration Detail:Group Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "A1C967B2-EEDA-416F-A53C-7BE46D6DA4E1", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Detail Page", "GroupDetailPage", "", "The page for viewing details about a group", 2, @"", "B936B357-776B-438F-B6BE-06CB26AFDFB4" );
            // Attrib for BlockType: Registration Detail:Group Member Page
            RockMigrationHelper.AddBlockTypeAttribute( "A1C967B2-EEDA-416F-A53C-7BE46D6DA4E1", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Member Page", "GroupMemberPage", "", "The page for viewing details about a group member", 3, @"", "C19CCA3D-48E8-4CF3-BADE-C509CC3C3434" );
            // Attrib for BlockType: Registration Detail:Source
            RockMigrationHelper.AddBlockTypeAttribute( "A1C967B2-EEDA-416F-A53C-7BE46D6DA4E1", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Source", "Source", "", "The Financial Source Type to use when creating transactions", 4, @"BE7ECF50-52BC-4774-808D-574BA842DB98", "429649E9-B86D-4C15-B392-0A87D3AFC31A" );

            // Attrib for BlockType: Registration Template Detail:Default Success Text
            RockMigrationHelper.AddBlockTypeAttribute( "91354899-304E-44C7-BD0D-55F42E6505D3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Default Success Text", "DefaultSuccessText", "", "The success text default to use for a new template", 2, @"
{% capture currencySymbol %}{{ 'Global' | Attribute:'CurrencySymbol' }}{% endcapture %}
{% assign registrantCount = Registration.Registrants | Size %}
<p>
    You have succesfully registered the following 
    {{ RegistrationInstance.RegistrationTemplate.RegistrantTerm | PluralizeForQuantity:registrantCount | Downcase }}
    for {{ RegistrationInstance.Name }}:
</p>

<ul>
{% for registrant in Registration.Registrants %}
    <li>
    
        <strong>{{ registrant.PersonAlias.Person.FullName }}</strong>
        
        {% if registrant.Cost > 0 %}
            - {{ currencySymbol }}{{ registrant.Cost | Format:'#,##0.00' }}
        {% endif %}
        
        {% assign feeCount = registrant.Fees | Size %}
        {% if feeCount > 0 %}
            <br/>{{ RegistrationInstance.RegistrationTemplate.FeeTerm | PluralizeForQuantity:registrantCount }}:
            <ul class='list-unstyled'>
            {% for fee in registrant.Fees %}
                <li>
                    {{ fee.RegistrationTemplateFee.Name }} {{ fee.Option }}
                    {% if fee.Quantity > 1 %} ({{ fee.Quantity }} @ {{ currencySymbol }}{{ fee.Cost | Format:'#,##0.00' }}){% endif %}: {{ currencySymbol }}{{ fee.TotalCost | Format:'#,##0.00' }}
                </li>
            {% endfor %}
            </ul>
        {% endif %}
        
    </li>
{% endfor %}
</ul>

{% if Registration.TotalCost > 0 %}
<p>
    Total Cost: {{ currencySymbol }}{{ Registration.TotalCost | Format:'#,##0.00' }}<br/>
    {% if Registration.DiscountedCost != Registration.TotalCost %}
        Discounted Cost: {{ currencySymbol }}{{ Registration.DiscountedCost | Format:'#,##0.00' }}<br/>
    {% endif %}
    {% for payment in Registration.Payments %}
        Paid {{ currencySymbol }}{{ payment.Amount | Format:'#,##0.00' }} on {{ payment.Transaction.TransactionDateTime| Date:'M/d/yyyy' }} 
        <small>(Ref #: {{ payment.Transaction.TransactionCode }})</small><br/>
    {% endfor %}
    {% assign paymentCount = Registration.Payments | Size %}
    {% if paymentCount > 1 %}
        Total Paid: {{ currencySymbol }}{{ Registration.TotalPaid | Format:'#,##0.00' }}<br/>
    {% endif %}
    Balance Due: {{ currencySymbol }}{{ Registration.BalanceDue | Format:'#,##0.00' }}
</p>
{% endif %}

<p>
    A confirmation email has been sent to {{ Registration.ConfirmationEmail }}. If you have any questions 
    please contact {{ RegistrationInstance.ContactName }} at {{ RegistrationInstance.ContactEmail }}.
</p>
", "B72F8C64-0AD5-4E9F-8DB0-00DE14E9BCCD" );

            // Attrib for BlockType: Registration Template Detail:Default Confirmation Email
            RockMigrationHelper.AddBlockTypeAttribute( "91354899-304E-44C7-BD0D-55F42E6505D3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Default Confirmation Email", "DefaultConfirmationEmail", "", "The default Confirmation Email Template value to use for a new template", 0, @"
{{ 'Global' | Attribute:'EmailHeader' }}
{% capture currencySymbol %}{{ 'Global' | Attribute:'CurrencySymbol' }}{% endcapture %}
{% assign registrantCount = Registration.Registrants | Size %}

<h1>{{ RegistrationInstance.RegistrationTemplate.RegistrationTerm }} Confirmation: {{ RegistrationInstance.Name }}</h1>

<p>
    The following {{ RegistrationInstance.RegistrationTemplate.RegistrantTerm | PluralizeForQuantity:registrantCount | Downcase }}
    {% if registrantCount > 1 %}have{% else %}has{% endif %} been registered for {{ RegistrationInstance.Name }}:
</p>

<ul>
{% for registrant in Registration.Registrants %}
    <li>
    
        {{ registrant.PersonAlias.Person.FullName }}
        
        {% if registrant.Cost > 0 %}
            - {{ currencySymbol }}{{ registrant.Cost | Format:'#,##0.00' }}
        {% endif %}
        
        {% assign feeCount = registrant.Fees | Size %}
        {% if feeCount > 0 %}
            <br/>{{ RegistrationInstance.RegistrationTemplate.FeeTerm | PluralizeForQuantity:registrantCount }}:
            <ul>
            {% for fee in registrant.Fees %}
                <li>
                    {{ fee.RegistrationTemplateFee.Name }} {{ fee.Option }}
                    {% if fee.Quantity > 1 %} ({{ fee.Quantity }} @ {{ currencySymbol }}{{ fee.Cost | Format:'#,##0.00' }}){% endif %}: {{ currencySymbol }}{{ fee.TotalCost | Format:'#,##0.00' }}
                </li>
            {% endfor %}
            </ul>
        {% endif %}

    </li>
{% endfor %}
</ul>

{% if Registration.TotalCost > 0 %}
<p>
    Total Cost: {{ currencySymbol }}{{ Registration.TotalCost | Format:'#,##0.00' }}<br/>
    {% if Registration.DiscountedCost != Registration.TotalCost %}
        Discounted Cost: {{ currencySymbol }}{{ Registration.DiscountedCost | Format:'#,##0.00' }}<br/>
    {% endif %}
    {% for payment in Registration.Payments %}
        Paid {{ currencySymbol }}{{ payment.Amount | Format:'#,##0.00' }} on {{ payment.Transaction.TransactionDateTime| Date:'M/d/yyyy' }} <small>(Ref #: {{ payment.Transaction.TransactionCode }})</small><br/>
    {% endfor %}
    
    {% assign paymentCount = Registration.Payments | Size %}
    
    {% if paymentCount > 1 %}
        Total Paid: {{ currencySymbol }}{{ Registration.TotalPaid | Format:'#,##0.00' }}<br/>
    {% endif %}
    
    Balance Due: {{ currencySymbol }}{{ Registration.BalanceDue | Format:'#,##0.00' }}
</p>
{% endif %}

{{ RegistrationInstance.AdditionalConfirmationDetails }}

<p>
    If you have any questions please contact {{ RegistrationInstance.ContactName }} at {{ RegistrationInstance.ContactEmail }}.
</p>

{{ 'Global' | Attribute:'EmailFooter' }}
", "EBD8EB51-5514-43B5-8AA6-E0A509D865E5" );
            // Attrib for BlockType: Registration Template Detail:Default Reminder Email
            RockMigrationHelper.AddBlockTypeAttribute( "91354899-304E-44C7-BD0D-55F42E6505D3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Default Reminder Email", "DefaultReminderEmail", "", "The default Reminder Email Template value to use for a new template", 1, @"
", "10FED7FA-8E42-4A28-B13F-0DC65D1F7BE5" );
            // Attrib Value for Block:Registration Detail, Attribute:Transaction Page Page: Registration Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "0E0AE4CB-E348-435E-A0CA-9E3B2FC6BA19", "352DE424-2299-404E-B86F-FEA46D588D2D", @"d5049786-cd67-45fa-83a9-4ee663f8fc5a" );
            // Attrib Value for Block:Registration Detail, Attribute:Batch Name Prefix Page: Registration Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "0E0AE4CB-E348-435E-A0CA-9E3B2FC6BA19", "03770487-DC57-4DEE-A0DB-F8A5B22F96C2", @"Event Registration" );
            // Attrib Value for Block:Registration Detail, Attribute:Registrant Page Page: Registration Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "0E0AE4CB-E348-435E-A0CA-9E3B2FC6BA19", "DA8437F4-B8A6-4C52-9111-8ADD868E0392", @"52ca0336-fc25-4131-bb5a-94a628c0ee77" );
            // Attrib Value for Block:Registration Detail, Attribute:Group Detail Page Page: Registration Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "0E0AE4CB-E348-435E-A0CA-9E3B2FC6BA19", "B936B357-776B-438F-B6BE-06CB26AFDFB4", @"4e237286-b715-4109-a578-c1445ec02707" );
            // Attrib Value for Block:Registration Detail, Attribute:Group Member Page Page: Registration Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "0E0AE4CB-E348-435E-A0CA-9E3B2FC6BA19", "C19CCA3D-48E8-4CF3-BADE-C509CC3C3434", @"3905c63f-4d57-40f0-9721-c60a2f681911" );
            // Attrib Value for Block:Registration Detail, Attribute:Source Page: Registration Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "0E0AE4CB-E348-435E-A0CA-9E3B2FC6BA19", "429649E9-B86D-4C15-B392-0A87D3AFC31A", @"be7ecf50-52bc-4774-808d-574ba842db98" );

        }
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Registration Detail:Source
            RockMigrationHelper.DeleteAttribute( "429649E9-B86D-4C15-B392-0A87D3AFC31A" );
            // Attrib for BlockType: Registration Detail:Group Member Page
            RockMigrationHelper.DeleteAttribute( "C19CCA3D-48E8-4CF3-BADE-C509CC3C3434" );
            // Attrib for BlockType: Registration Detail:Group Detail Page
            RockMigrationHelper.DeleteAttribute( "B936B357-776B-438F-B6BE-06CB26AFDFB4" );
            // Attrib for BlockType: Registration Detail:Registrant Page
            RockMigrationHelper.DeleteAttribute( "DA8437F4-B8A6-4C52-9111-8ADD868E0392" );
            // Attrib for BlockType: Registration Detail:Batch Name Prefix
            RockMigrationHelper.DeleteAttribute( "03770487-DC57-4DEE-A0DB-F8A5B22F96C2" );
            // Attrib for BlockType: Registration Detail:Transaction Page
            RockMigrationHelper.DeleteAttribute( "352DE424-2299-404E-B86F-FEA46D588D2D" );
            // Attrib for BlockType: Registration Template Detail:Default Reminder Email
            RockMigrationHelper.DeleteAttribute( "10FED7FA-8E42-4A28-B13F-0DC65D1F7BE5" );
            // Attrib for BlockType: Registration Template Detail:Default Confirmation Email
            RockMigrationHelper.DeleteAttribute( "EBD8EB51-5514-43B5-8AA6-E0A509D865E5" );
            // Attrib for BlockType: Registration Template Detail:Default Success Text
            RockMigrationHelper.DeleteAttribute( "B72F8C64-0AD5-4E9F-8DB0-00DE14E9BCCD" );
            // Remove Block: Registrant Detail, from Page: Registrant, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "2F125E9E-5027-4EFE-B9F4-0FA047D7CF45" );
            // Remove Block: Transaction Detail, from Page: Payment, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "64C35D4A-736D-4A9B-A2AB-79BD61ACF298" );

            RockMigrationHelper.DeleteBlockType( "D72A1A61-43D1-4D5D-92EC-BAECA02EAC43" ); // Registrant Detail

            RockMigrationHelper.DeletePage( "D5049786-CD67-45FA-83A9-4EE663F8FC5A" ); //  Page: Payment, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "52CA0336-FC25-4131-BB5A-94A628C0EE77" ); //  Page: Registrant, Layout: Full Width, Site: Rock RMS
            
            DropColumn("dbo.RegistrationInstance", "ContactPhone");
        }
    }
}
