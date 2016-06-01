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
    public partial class OnlineContributionStatementPages : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            
RockMigrationHelper.AddPage("621E03C5-6586-450B-A340-CC7D9727B69A","55E19934-762D-48E5-BD07-ACB1249ACBDC","Contribution Statement","","FC44FC7F-5EA2-4F0E-8182-D8D6C9C75E28",""); // Site:External Website

RockMigrationHelper.UpdateBlockType("Contribution Statement Lava","Block for displaying a Lava based contribution statement.","~/Blocks/Finance/ContributionStatementLava.ascx","Finance","AF986B72-ADD9-4E05-971F-1DE4EBED8667");
RockMigrationHelper.UpdateBlockType("Contribution Statement List Lava","Block for displaying a listing of years where contribution statements are available.","~/Blocks/Finance/ContributionStatementListLava.ascx","Finance","22BF5B51-6511-4D31-8A48-4978A454C386");


// Add Block to Page: Contribution Statement, Site: External Website
RockMigrationHelper.AddBlock("FC44FC7F-5EA2-4F0E-8182-D8D6C9C75E28","","AF986B72-ADD9-4E05-971F-1DE4EBED8667","Contribution Statement Lava","Main","","",0,"A1C9B68E-BD41-43E1-A40F-BAD33EBD4124"); 
// Add Block to Page: Giving History, Site: External Website
RockMigrationHelper.AddBlock("621E03C5-6586-450B-A340-CC7D9727B69A","","22BF5B51-6511-4D31-8A48-4978A454C386","Contribution Statement List Lava","Main","","",1,"639943D6-75C2-46B4-B044-F4FD7E42E936"); 
// update block order for pages with new blocks if the page,zone has multiple blocks
Sql(@"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '8A5E5144-3054-4FC9-AD8A-B0F4813C94E4'");  // Page: Giving History,  Zone: Main,  Block: Transaction Report Intro Text
Sql(@"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '639943D6-75C2-46B4-B044-F4FD7E42E936'");  // Page: Giving History,  Zone: Main,  Block: Contribution Statement List Lava
Sql(@"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = '0B62A727-1AEB-4134-AFAE-1EBB73A6B098'");  // Page: Giving History,  Zone: Main,  Block: Transaction Report



// Attrib for BlockType: Contribution Statement Lava:Enable Debug
RockMigrationHelper.AddBlockTypeAttribute("AF986B72-ADD9-4E05-971F-1DE4EBED8667","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Enable Debug","EnableDebug","","Shows the merge fields available for the Lava",3,@"False","D35A9BB5-F85D-4C50-A7A2-5F9A797DF17C");
// Attrib for BlockType: Contribution Statement Lava:Display Pledges
RockMigrationHelper.AddBlockTypeAttribute("AF986B72-ADD9-4E05-971F-1DE4EBED8667","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Display Pledges","DisplayPledges","","Determines if pledges should be shown.",1,@"True","20ABF562-D44A-47D1-9C34-BCA15A44D64E");
// Attrib for BlockType: Contribution Statement Lava:Accounts
RockMigrationHelper.AddBlockTypeAttribute("AF986B72-ADD9-4E05-971F-1DE4EBED8667","17033CDD-EF97-4413-A483-7B85A787A87F","Accounts","Accounts","","A selection of accounts to include on the statement. If none are selected all accounts that are tax-deductible will be uses.",0,@"","EA87DB7F-F053-40A9-B8FB-064D691ACA9D");
// Attrib for BlockType: Contribution Statement Lava:Lava Template
RockMigrationHelper.AddBlockTypeAttribute("AF986B72-ADD9-4E05-971F-1DE4EBED8667","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","Lava Template","LavaTemplate","","The Lava template to use for the contribution statement.",2,@"{% assign currentYear = 'Now' | Date:'yyyy' %}
{% assign statementCount = StatementYears | Size %}

{% if statementCount > 0 %}
    <h4>Available Contribution Statements</h4>
    
    <div class=""margin-b-md"">
    {% for statementyear in StatementYears %}
        {% if currentYear == statementyear.Year %}
            <a href=""{{ DetailPage }}?StatementYear={{ statementyear.Year }}"" class=""btn btn-primary"">{{ statementyear.Year }} <small>YTD</small></a>
        {% else %}
            <a href=""{{ DetailPage }}?StatementYear={{ statementyear.Year }}"" class=""btn btn-primary"">{{ statementyear.Year }}</a>
        {% endif %}
    {% endfor %}
    </div>
{% endif %}","22DEED01-F70B-4AF7-AF34-887E0C18E8FD");

// Attrib for BlockType: Contribution Statement List Lava:Lava Template
RockMigrationHelper.AddBlockTypeAttribute("22BF5B51-6511-4D31-8A48-4978A454C386","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","Lava Template","LavaTemplate","","The Lava template to use for the contribution statement.",3,@"","7B554631-3CD5-40C4-8E67-ECED56D4D7C1");
// Attrib for BlockType: Contribution Statement List Lava:Accounts
RockMigrationHelper.AddBlockTypeAttribute("22BF5B51-6511-4D31-8A48-4978A454C386","17033CDD-EF97-4413-A483-7B85A787A87F","Accounts","Accounts","","A selection of accounts to use for checking if transactions for the current user exist. If no accounts are provided then all tax-deductible accounts will be considered.",0,@"","AC1EF7F3-7B06-4978-84DD-B38025FC2E7B");
// Attrib for BlockType: Contribution Statement List Lava:Max Years To Display
RockMigrationHelper.AddBlockTypeAttribute("22BF5B51-6511-4D31-8A48-4978A454C386","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Max Years To Display","MaxYearsToDisplay","","The maximum number of years to display (including the current year).",1,@"3","346384B5-1ECE-4949-BFF4-712E1FAA4335");
// Attrib for BlockType: Contribution Statement List Lava:Detail Page
RockMigrationHelper.AddBlockTypeAttribute("22BF5B51-6511-4D31-8A48-4978A454C386","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Detail Page","DetailPage","","The statement detail page.",2,@"","5B439A86-D2AD-4223-8D1E-A50FF883D7C2");
// Attrib for BlockType: Contribution Statement List Lava:Enable Debug
RockMigrationHelper.AddBlockTypeAttribute("22BF5B51-6511-4D31-8A48-4978A454C386","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Enable Debug","EnableDebug","","Shows the merge fields available for the Lava",4,@"False","D8865D7D-F3FA-48FF-8339-8E2649D8CC29");


// Attrib Value for Block:Contribution Statement Lava, Attribute:Accounts Page: Contribution Statement, Site: External Website
RockMigrationHelper.AddBlockAttributeValue("A1C9B68E-BD41-43E1-A40F-BAD33EBD4124","EA87DB7F-F053-40A9-B8FB-064D691ACA9D",@"");
// Attrib Value for Block:Contribution Statement Lava, Attribute:Enable Debug Page: Contribution Statement, Site: External Website
RockMigrationHelper.AddBlockAttributeValue("A1C9B68E-BD41-43E1-A40F-BAD33EBD4124","D35A9BB5-F85D-4C50-A7A2-5F9A797DF17C",@"False");
// Attrib Value for Block:Contribution Statement Lava, Attribute:Display Pledges Page: Contribution Statement, Site: External Website
RockMigrationHelper.AddBlockAttributeValue("A1C9B68E-BD41-43E1-A40F-BAD33EBD4124","20ABF562-D44A-47D1-9C34-BCA15A44D64E",@"True");
// Attrib Value for Block:Contribution Statement Lava, Attribute:Lava Template Page: Contribution Statement, Site: External Website
RockMigrationHelper.AddBlockAttributeValue("A1C9B68E-BD41-43E1-A40F-BAD33EBD4124","22DEED01-F70B-4AF7-AF34-887E0C18E8FD",@"{% capture pageTitle %}{{ 'Global' | Attribute:'OrganizationName' }} | Contribution Statement{%endcapture%}
{{ pageTitle | SetPageTitle }}

<div class=""row margin-b-xl"">
<div class=""col-md-6"">
<div class=""pull-left"">
<img src=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}{{ 'Global' | Attribute:'EmailHeaderLogo' }}"" width=""100px"" />
</div>

<div class=""pull-left margin-l-md margin-t-sm"">
<strong>{{ 'Global' | Attribute:'OrganizationName' }}</strong><br />
{{ 'Global' | Attribute:'OrganizationAddress' }}<br />
{{ 'Global' | Attribute:'OrganizationWebsite' }}
</div>
</div>
<div class=""col-md-6 text-right"">
<h4>Charitable Contributions for the Year {{ StatementStartDate | Date:'yyyy' }}</h4>
<p>{{ StatementStartDate | Date:'M/d/yyyy' }} - {{ StatementEndDate | Date:'M/d/yyyy' }}<p>
</div>
</div>

<h4>
{{ Salutation }} <br />
{{ StreetAddress1 }} <br />
{% if StreetAddress2 != '' %}
{{ StreetAddress2 }} <br />
{% endif %}
{{ City }}, {{ State }} {{ PostalCode }}
</h4>


<div class=""clearfix"">
<div class=""pull-right"">
<a href=""#"" class=""btn btn-primary hidden-print"" onClick=""window.print();""><i class=""fa fa-print""></i> Print Statement</a> 
</div>
</div>

<hr style=""opacity: .5;"" />

<h4 class=""margin-t-md margin-b-md"">Gift List</h4>


<table class=""table table-bordered table-striped table-condensed"">
<tr>
<th>Date</th>
<th>Giving Area</th>
<th>Check/Trans #</th>
<th align=""right"">Amount</th>
</tr>


{% for transaction in TransactionDetails %}
<tr>
	<td>{{ transaction.Transaction.TransactionDateTime | Date:'M/d/yyyy' }}</td>
	<td>{{ transaction.Account.Name }}</td>
	<td>{{ transaction.Transaction.TransactionCode }}</td>
	<td align=""right"">{{ 'Global' | Attribute:'CurrencySymbol' }}{{ transaction.Amount }}</td>
</tr>
{% endfor %}

</table>




<div class=""row"">
<div class=""col-xs-6 col-xs-offset-6"">
<h4 class=""margin-t-md margin-b-md"">Fund Summary</h4>
<div class=""row"">
<div class=""col-xs-6"">
	<strong>Fund Name</strong>
</div>
<div class=""col-xs-6 text-right"">
	<strong>Total Amount</strong>
</div>
</div>

{% for accountsummary in AccountSummary %}
<div class=""row"">
	<div class=""col-xs-6"">{{ accountsummary.AccountName }}</div>
	<div class=""col-xs-6 text-right"">{{ 'Global' | Attribute:'CurrencySymbol' }}{{ accountsummary.Total }}</div>
</div>
{% endfor %}
</div>
</div>

{% assign pledgeCount = Pledges | Size %}

{% if pledgeCount > 0 %}
<hr style=""opacity: .5;"" />
<h4 class=""margin-t-md margin-b-md"">Pledges</h4>

{% for pledge in Pledges %}
<div class=""row"">
<div class=""col-xs-6"">
	<strong>{{ pledge.AccountName }}</strong>
	
	<p>
		Amt Pledged: {{ 'Global' | Attribute:'CurrencySymbol' }}{{ pledge.AmountPledged }} <br />
		Amt Given: {{ 'Global' | Attribute:'CurrencySymbol' }}{{ pledge.AmountGiven }} <br />
		Amt Remaining: {{ 'Global' | Attribute:'CurrencySymbol' }}{{ pledge.AmountRemaining }}
	</p>
</div>
<div class=""col-xs-6 padding-t-md"">
	<div class=""hidden-print"">
		Pledge Progress
		<div class=""progress"">
		  <div class=""progress-bar"" role=""progressbar"" aria-valuenow=""{{ pledge.PercentComplete }}"" aria-valuemin=""0"" aria-valuemax=""100"" style=""width: {{ pledge.PercentComplete }}%;"">
			{{ pledge.PercentComplete }}%
		  </div>
		</div>
	</div>
	<div class=""visible-print-block"">
		Percent Complete <br />
		{{ pledge.PercentComplete }}%
	</div>
</div>
</div>
{% endfor %}
{% endif %}

<hr style=""opacity: .5;"" />
<p class=""text-center"">
Thank you for your continued support of the {{ 'Global' | Attribute:'OrganizationName' }}. If you have any questions about your statement,
email {{ 'Global' | Attribute:'OrganizationEmail' }} or call {{ 'Global' | Attribute:'OrganizationPhone' }}.
</p>

<p class=""text-center"">
<em>Unless otherwise noted, the only goods and services provided are intangible religious benefits.</em>
</p>");
// Attrib Value for Block:Contribution Statement List Lava, Attribute:Enable Debug Page: Giving History, Site: External Website
RockMigrationHelper.AddBlockAttributeValue("639943D6-75C2-46B4-B044-F4FD7E42E936","D8865D7D-F3FA-48FF-8339-8E2649D8CC29",@"False");
// Attrib Value for Block:Contribution Statement List Lava, Attribute:Accounts Page: Giving History, Site: External Website
RockMigrationHelper.AddBlockAttributeValue("639943D6-75C2-46B4-B044-F4FD7E42E936","AC1EF7F3-7B06-4978-84DD-B38025FC2E7B",@"");
// Attrib Value for Block:Contribution Statement List Lava, Attribute:Max Years To Display Page: Giving History, Site: External Website
RockMigrationHelper.AddBlockAttributeValue("639943D6-75C2-46B4-B044-F4FD7E42E936","346384B5-1ECE-4949-BFF4-712E1FAA4335",@"3");

// Attrib Value for Block:Contribution Statement List Lava, Attribute:Detail Page Page: Giving History, Site: External Website
RockMigrationHelper.AddBlockAttributeValue("639943D6-75C2-46B4-B044-F4FD7E42E936","5B439A86-D2AD-4223-8D1E-A50FF883D7C2",@"fc44fc7f-5ea2-4f0e-8182-d8d6c9c75e28");
RockMigrationHelper.UpdateFieldType("Data View","","Rock","Rock.Field.Types.DataViewFieldType","BD72BBF1-0269-407E-BDBE-EEED4F1F207F");
RockMigrationHelper.UpdateFieldType("Phone Number","","Rock","Rock.Field.Types.PhoneNumberFieldType","6B1908EC-12A2-463A-A7BD-970CE0FAF097");

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

            // Attrib for BlockType: Contribution Statement List Lava:Detail Page
            RockMigrationHelper.DeleteAttribute( "5B439A86-D2AD-4223-8D1E-A50FF883D7C2" );
            // Attrib for BlockType: Contribution Statement List Lava:Lava Template
            RockMigrationHelper.DeleteAttribute( "7B554631-3CD5-40C4-8E67-ECED56D4D7C1" );
            // Attrib for BlockType: Contribution Statement List Lava:Max Years To Display
            RockMigrationHelper.DeleteAttribute( "346384B5-1ECE-4949-BFF4-712E1FAA4335" );
            // Attrib for BlockType: Contribution Statement List Lava:Accounts
            RockMigrationHelper.DeleteAttribute( "AC1EF7F3-7B06-4978-84DD-B38025FC2E7B" );
            // Attrib for BlockType: Contribution Statement List Lava:Enable Debug
            RockMigrationHelper.DeleteAttribute( "D8865D7D-F3FA-48FF-8339-8E2649D8CC29" );

            // Attrib for BlockType: Contribution Statement Lava:Lava Template
            RockMigrationHelper.DeleteAttribute( "22DEED01-F70B-4AF7-AF34-887E0C18E8FD" );
            // Attrib for BlockType: Contribution Statement Lava:Display Pledges
            RockMigrationHelper.DeleteAttribute( "20ABF562-D44A-47D1-9C34-BCA15A44D64E" );
            // Attrib for BlockType: Contribution Statement Lava:Enable Debug
            RockMigrationHelper.DeleteAttribute( "D35A9BB5-F85D-4C50-A7A2-5F9A797DF17C" );
            // Attrib for BlockType: Contribution Statement Lava:Accounts
            RockMigrationHelper.DeleteAttribute( "EA87DB7F-F053-40A9-B8FB-064D691ACA9D" );

            // Remove Block: Contribution Statement List Lava, from Page: Giving History, Site: External Website
            RockMigrationHelper.DeleteBlock( "639943D6-75C2-46B4-B044-F4FD7E42E936" );
            // Remove Block: Contribution Statement Lava, from Page: Contribution Statement, Site: External Website
            RockMigrationHelper.DeleteBlock( "A1C9B68E-BD41-43E1-A40F-BAD33EBD4124" );

            RockMigrationHelper.DeleteBlockType( "22BF5B51-6511-4D31-8A48-4978A454C386" ); // Contribution Statement List Lava
            RockMigrationHelper.DeleteBlockType( "AF986B72-ADD9-4E05-971F-1DE4EBED8667" ); // Contribution Statement Lava

            RockMigrationHelper.DeletePage( "FC44FC7F-5EA2-4F0E-8182-D8D6C9C75E28" ); //  Page: Contribution Statement, Layout: Blank, Site: External Website

        }
    }
}
