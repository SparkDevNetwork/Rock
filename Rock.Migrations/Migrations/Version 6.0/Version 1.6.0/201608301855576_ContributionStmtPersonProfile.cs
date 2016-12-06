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
    public partial class ContributionStmtPersonProfile : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
                        // Page: Contribution Statement
            RockMigrationHelper.AddPage("53CF4CBE-85F9-4A50-87D7-0D72A3FB2892","2E169330-D7D7-4ECA-B417-72C64BE150F0","Contribution Statement","","98EBADAF-CCA9-4893-9DD3-D8201D8BD7FA",""); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType("Contribution Statement Lava","Block for displaying a Lava based contribution statement.","~/Blocks/Finance/ContributionStatementLava.ascx","Finance","AF986B72-ADD9-4E05-971F-1DE4EBED8667");
            RockMigrationHelper.AddBlock("98EBADAF-CCA9-4893-9DD3-D8201D8BD7FA","","AF986B72-ADD9-4E05-971F-1DE4EBED8667","Contribution Statement Lava","Main","","",0,"680D8BC7-9F39-45AA-A89E-D542BC7AC57D"); 
            RockMigrationHelper.AddBlockTypeAttribute("AF986B72-ADD9-4E05-971F-1DE4EBED8667","17033CDD-EF97-4413-A483-7B85A787A87F","Accounts","Accounts","","A selection of accounts to include on the statement. If none are selected all accounts that are tax-deductible will be uses.",0,@"","EA87DB7F-F053-40A9-B8FB-064D691ACA9D");
            RockMigrationHelper.AddBlockTypeAttribute("AF986B72-ADD9-4E05-971F-1DE4EBED8667","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Display Pledges","DisplayPledges","","Determines if pledges should be shown.",1,@"True","20ABF562-D44A-47D1-9C34-BCA15A44D64E");
            RockMigrationHelper.AddBlockTypeAttribute("AF986B72-ADD9-4E05-971F-1DE4EBED8667","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","Lava Template","LavaTemplate","","The Lava template to use for the contribution statement.",2,@"{% capture pageTitle %}{{ 'Global' | Attribute:'OrganizationName' }} | Contribution Statement{%endcapture%}
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
</p>","22DEED01-F70B-4AF7-AF34-887E0C18E8FD");
            RockMigrationHelper.AddBlockTypeAttribute("AF986B72-ADD9-4E05-971F-1DE4EBED8667","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Enable Debug","EnableDebug","","Shows the merge fields available for the Lava",3,@"False","D35A9BB5-F85D-4C50-A7A2-5F9A797DF17C");
            RockMigrationHelper.AddBlockTypeAttribute("AF986B72-ADD9-4E05-971F-1DE4EBED8667","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Allow Person Querystring","AllowPersonQuerystring","","Determines if a person is allowed to be passed through the querystring. For security reasons this is not allowed by default.",4,@"False","9291B6FD-7ABE-453A-BD9F-7C3CF916C757");
            RockMigrationHelper.AddBlockAttributeValue("680D8BC7-9F39-45AA-A89E-D542BC7AC57D","D35A9BB5-F85D-4C50-A7A2-5F9A797DF17C",@"False"); // Enable Debug
            RockMigrationHelper.AddBlockAttributeValue("680D8BC7-9F39-45AA-A89E-D542BC7AC57D","20ABF562-D44A-47D1-9C34-BCA15A44D64E",@"True"); // Display Pledges
            RockMigrationHelper.AddBlockAttributeValue("680D8BC7-9F39-45AA-A89E-D542BC7AC57D","EA87DB7F-F053-40A9-B8FB-064D691ACA9D",@""); // Accounts
            RockMigrationHelper.AddBlockAttributeValue("680D8BC7-9F39-45AA-A89E-D542BC7AC57D","22DEED01-F70B-4AF7-AF34-887E0C18E8FD",@"{% capture pageTitle %}{{ 'Global' | Attribute:'OrganizationName' }} | Contribution Statement{%endcapture%}
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
</p>"); // Lava Template
            RockMigrationHelper.AddBlockAttributeValue("680D8BC7-9F39-45AA-A89E-D542BC7AC57D","9291B6FD-7ABE-453A-BD9F-7C3CF916C757",@"True"); // Allow Person Querystring


            RockMigrationHelper.AddBlock( "53CF4CBE-85F9-4A50-87D7-0D72A3FB2892", "", "22BF5B51-6511-4D31-8A48-4978A454C386", "Contribution Statement List Lava", "SectionA2", "", "", 1, "96599B45-E080-44AE-8CB7-CCCCA4873398" );

            // Attrib for BlockType: Contribution Statement List Lava:Use Person Context
            RockMigrationHelper.AddBlockTypeAttribute( "22BF5B51-6511-4D31-8A48-4978A454C386", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Person Context", "UsePersonContext", "", "Determines if the person context should be used instead of the CurrentPerson.", 5, @"False", "F37EB885-416A-4B70-B48E-8A25557C7B12" );

            // Attrib for BlockType: Contribution Statement List Lava:Entity Type
            RockMigrationHelper.AddBlockTypeAttribute( "22BF5B51-6511-4D31-8A48-4978A454C386", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", "The type of entity that will provide context for this block", 0, @"", "F9A168F1-3E59-4C5F-8019-7B17D00B94C9" );

            // Attrib Value for Block:Contribution Statement List Lava, Attribute:Use Person Context Page: Contributions, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "96599B45-E080-44AE-8CB7-CCCCA4873398", "F37EB885-416A-4B70-B48E-8A25557C7B12", @"True" );

            // Attrib Value for Block:Contribution Statement List Lava, Attribute:Entity Type Page: Contributions, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("96599B45-E080-44AE-8CB7-CCCCA4873398","F9A168F1-3E59-4C5F-8019-7B17D00B94C9",@"72657ed8-d16e-492e-ac12-144c5e7567e7");
            // Attrib Value for Block:Contribution Statement List Lava, Attribute:Lava Template Page: Contributions, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("96599B45-E080-44AE-8CB7-CCCCA4873398","7B554631-3CD5-40C4-8E67-ECED56D4D7C1", @"{% assign yearCount = StatementYears | Size %}
{% if yearCount > 0 %}<hr /><p class=""margin-t-md"">
    <strong><i class='fa fa-file-text-o'></i> Available Contribution Statements</strong>
</p>



{% assign currentYear = 'Now' | Date:'yyyy' %}

<div>
{% for statementyear in StatementYears %}
    {% if currentYear == statementyear.Year %}
        <a href=""{{ DetailPage }}?PersonGuid={{ PersonGuid }}&StatementYear={{ statementyear.Year }}"" class=""btn btn-sm btn-default"">{{ statementyear.Year }} <small>YTD</small></a>
    {% else %}
        <a href=""{{ DetailPage }}?PersonGuid={{ PersonGuid }}&StatementYear={{ statementyear.Year }}"" class=""btn btn-sm btn-default"">{{ statementyear.Year }}</a>
    {% endif %}
{% endfor %}
</div>
{% endif %}" );
            // Attrib Value for Block:Contribution Statement List Lava, Attribute:Accounts Page: Contributions, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("96599B45-E080-44AE-8CB7-CCCCA4873398","AC1EF7F3-7B06-4978-84DD-B38025FC2E7B",@"");
            // Attrib Value for Block:Contribution Statement List Lava, Attribute:Max Years To Display Page: Contributions, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("96599B45-E080-44AE-8CB7-CCCCA4873398","346384B5-1ECE-4949-BFF4-712E1FAA4335",@"3");
            // Attrib Value for Block:Contribution Statement List Lava, Attribute:Detail Page Page: Contributions, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("96599B45-E080-44AE-8CB7-CCCCA4873398","5B439A86-D2AD-4223-8D1E-A50FF883D7C2",@"98ebadaf-cca9-4893-9dd3-d8201d8bd7fa");
            // Attrib Value for Block:Contribution Statement List Lava, Attribute:Enable Debug Page: Contributions, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("96599B45-E080-44AE-8CB7-CCCCA4873398","D8865D7D-F3FA-48FF-8339-8E2649D8CC29",@"False");

            Sql( @"  UPDATE [HtmlContent]
  SET [Content] = '<a href=""../../AddTransaction?Person={{ Context.Person.UrlEncodedKey }}"" class=""btn btn-default btn-block"">Add One-time Gift</a>
        <a href=""../../AddTransaction?Person={{ Context.Person.UrlEncodedKey }}"" class=""btn btn-default btn-block"">New Scheduled Transaction</a>'
  WHERE [Guid] = 'B20065F8-2FC9-4BE2-B0B3-7E8612393AAE'" );

            Sql( @"  UPDATE [Block] 
  SET [PreHtml] = '<div class=""panel panel-block""><div class=""panel-body"">'
  WHERE [Guid] = '6F7F97D3-6C7D-4B58-A6E7-9A21BF55428A'" );

            Sql( @"UPDATE[Block]
  SET[PostHtml] = '</div></div>'
  WHERE[Guid] = '96599B45-E080-44AE-8CB7-CCCCA4873398'" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "9291B6FD-7ABE-453A-BD9F-7C3CF916C757" );
            RockMigrationHelper.DeleteAttribute( "22DEED01-F70B-4AF7-AF34-887E0C18E8FD" );
            RockMigrationHelper.DeleteAttribute( "EA87DB7F-F053-40A9-B8FB-064D691ACA9D" );
            RockMigrationHelper.DeleteAttribute( "20ABF562-D44A-47D1-9C34-BCA15A44D64E" );
            RockMigrationHelper.DeleteAttribute( "D35A9BB5-F85D-4C50-A7A2-5F9A797DF17C" );
            RockMigrationHelper.DeleteBlock( "680D8BC7-9F39-45AA-A89E-D542BC7AC57D" );
            RockMigrationHelper.DeleteBlockType( "AF986B72-ADD9-4E05-971F-1DE4EBED8667" );
            RockMigrationHelper.DeletePage( "98EBADAF-CCA9-4893-9DD3-D8201D8BD7FA" ); //  Page: Contribution Statement

            // Attrib for BlockType: Contribution Statement List Lava:Entity Type
            RockMigrationHelper.DeleteAttribute( "F9A168F1-3E59-4C5F-8019-7B17D00B94C9" );
            // Attrib for BlockType: Contribution Statement List Lava:Use Person Context
            RockMigrationHelper.DeleteAttribute( "F37EB885-416A-4B70-B48E-8A25557C7B12" );

            // Remove Block: Contribution Statement List Lava, from Page: Contributions, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "96599B45-E080-44AE-8CB7-CCCCA4873398" );
        }
    }
}
