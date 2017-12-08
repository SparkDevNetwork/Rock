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
namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// 
    /// </summary>
    [MigrationNumber( 1, "1.6.10" )]
    public class StatementGenerator1 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            //  TODO: RockMigrationHelper.AddDefinedType()

            string rockDefaultLavaTemplate = @"
{% assign publicApplicationRoot = 'Global' | Attribute:'PublicApplicationRoot' %}
{% assign organizationName = 'Global' | Attribute:'OrganizationName' %}
{% assign organizationAddress = 'Global' | Attribute:'OrganizationAddress' %}
{% assign organizationWebsite = 'Global' | Attribute:'OrganizationWebsite' %}
{% assign organizationEmail = 'Global' | Attribute:'OrganizationEmail' %}
{% assign organizationPhone = 'Global' | Attribute:'OrganizationPhone' %}
{% assign currencySymbol = 'Global' | Attribute:'CurrencySymbol' %}
<!DOCTYPE html>
<html>
<head>
    <title>
    	{{ organizationName }} | Contribution Statement
    </title>

    <!-- Included CSS Files -->
    <link rel=""stylesheet"" href=""https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css"" integrity=""sha384-BVYiiSIFeK1dGmJRAkycuHAHRg32OmUcww7on3RYdg4Va+PmSTsz/K68vbdEjh4u"" crossorigin=""anonymous"">

   <style>
        html, body {
            height: auto;
            width: 100%;
            min-width: 100%;
            margin: 0 0 0 0;
            padding: 0 0 0 0;
            vertical-align: top;
			background-color: #FFFFFF
        }
        
        .footer {
            font-size: 8px
        }
        
        /* helper classes not included in stock bs3 */
        
        .margin-t-md {
            margin-top: 15px; !important
        }
        .margin-b-md {
            margin-bottom: 15px; !important
        }
        .padding-t-md {
            padding-top: 15px; !important
        }
        .padding-b-md {
            padding-bottom: 15px; !important
        }
    </style>

<body>

<!-- set top padding to help align logo and return address with envelope window -->
<div style='padding-top:50px'>
<div class=""row"">
    <div class=""col-md-6 pull-left"">
        <div>
            <img src=""{{ LavaTemplate | Attribute:'Logo','Url' }}"" width=""240px"" height=""80px"" />
        </div>
        
        <div>
            {{ organizationAddress }}<br />
            {{ organizationWebsite }}
        </div>
    </div>
    <div class=""col-md-6 text-right"">
        <h4>Charitable Contributions for the Year</h4>
        <p>{{ StatementStartDate | Date:'M/d/yyyy' }} - {{ StatementEndDate | Date:'M/d/yyyy' }}<p>
    </div>
</div>

<!-- set top margin to help align recipient address with envelope window -->
<div style='margin-top:130px'>
<h4>
    {{ Salutation }} <br />
    {{ StreetAddress1 }} <br />
    {% if StreetAddress2 != '' %}
        {{ StreetAddress2 }} <br />
    {% endif %}
    {{ City }}, {{ State }} {{ PostalCode }}
</h4>
</div>

<hr style=""opacity: .5;"" />

<h4>Gift List</h4>


    <table class=""table table-bordered table-striped table-condensed"">
        <thead>
            <tr>
                <th>Date</th>
                <th>Giving Area</th>
                <th>Check/Trans #</th>
                <th align=""right"">Amount</th>
            </tr>
        </thead>    
    
        <tbody>
        {% for transaction in TransactionDetails %}
            <tr>
                <td>{{ transaction.Transaction.TransactionDateTime | Date:'M/d/yyyy' }}</td>
                <td>{{ transaction.Account.Name }}</td>
                <td>{{ transaction.Transaction.TransactionCode }}</td>
                <td align=""right"">{{ currencySymbol }}{{ transaction.Amount }}</td>
            </tr>
        {% endfor %}
        </tbody>
        <tfoot>
        </tfoot>
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
                <div class=""col-xs-6 text-right"">{{ currencySymbol }}{{ accountsummary.Total }}</div>
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
                    Amt Pledged: {{ currencySymbol }}{{ pledge.AmountPledged }} <br />
                    Amt Given: {{ currencySymbol }}{{ pledge.AmountGiven }} <br />
                    Amt Remaining: {{ currencySymbol }}{{ pledge.AmountRemaining }}
                </p>
            </div>
            <div class=""col-xs-6 padding-t-md"">
                <div>
                    Percent Complete <br />
                    {{ pledge.PercentComplete }}%
                </div>
            </div>
        </div>
    {% endfor %}
{% endif %}

<hr style=""opacity: .5;"" />
<p class=""text-center"">
    Thank you for your continued support of the {{ organizationName }}. If you have any questions about your statement,
    email {{ organizationEmail }} or call {{ organizationPhone }}.
</p>

<p class=""text-center"">
    <em>Unless otherwise noted, the only goods and services provided are intangible religious benefits.</em>
</p>

</body>
</html>
";

            string rockDefaultFooterHtml = @"
<!DOCTYPE html>
<html>

<head>
    <script>
        function subst() {
            var vars = {};
            var query_strings_from_url = document.location.search.substring(1).split('&');
            for (var query_string in query_strings_from_url) {
                if (query_strings_from_url.hasOwnProperty(query_string)) {
                    var temp_var = query_strings_from_url[query_string].split('=', 2);
                    vars[temp_var[0]] = decodeURI(temp_var[1]);
                }
            }
            var css_selector_classes = ['page', 'frompage', 'topage'];
            for (var css_class in css_selector_classes) {
                if (css_selector_classes.hasOwnProperty(css_class)) {
                    var element = document.getElementsByClassName(css_selector_classes[css_class]);
                    for (var j = 0; j < element.length; ++j) {
                        element[j].textContent = vars[css_selector_classes[css_class]];
                    }
                }
            }
        }
    </script>
</head>

<body style=""border:0; margin: 0;"" onload=""subst()"">
    <table class='footer' style='width: 100%'>
        <tr>
            <td style=""text-align:right; font-size:10px; opacity:.5"">
                Page <span class=""page""></span> of <span class=""topage""></span>
            </td>
        </tr>
    </table>
</body>

</html>
";


        // RockMigrationHelper.AddDefinedValueAttributeValue()
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
