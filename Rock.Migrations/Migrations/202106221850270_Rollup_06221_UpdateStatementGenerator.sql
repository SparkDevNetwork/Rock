UPDATE [FinancialStatementTemplate] SET [ReportTemplate]=N'{% assign publicApplicationRoot = ''Global'' | Attribute:''PublicApplicationRoot'' %}
{% assign organizationName = ''Global'' | Attribute:''OrganizationName'' %}
{% assign organizationAddress = ''Global'' | Attribute:''OrganizationAddress'' %}
{% assign organizationWebsite = ''Global'' | Attribute:''OrganizationWebsite'' %}
{% assign organizationEmail = ''Global'' | Attribute:''OrganizationEmail'' %}
{% assign organizationPhone = ''Global'' | Attribute:''OrganizationPhone'' %}
{% assign currencySymbol = ''Global'' | Attribute:''CurrencySymbol'' %}
{% assign bodyPadding = ''0'' %}
{% assign fontSizeBase = ''9px'' %}
{% if RenderMedium == ''Html'' %}
  {% assign bodyPadding = ''0'' %}
  {% assign fontSizeBase = ''12px'' %}
{% endif %}
<!DOCTYPE html>
<html>
<head>
    <title>
    	{{ organizationName }} | Contribution Statement
    </title>

    <!-- Included CSS Files -->
    <link rel="stylesheet" href="https://stackpath.bootstrapcdn.com/bootstrap/3.4.1/css/bootstrap.min.css" integrity="sha384-HSMxcRTRxnN+Bdg0JdbxYKrThecOKuH5zCYotlSAcp1+c8xmyTe9GYg1l9a69psu" crossorigin="anonymous">

   <style>
        html, body {
            height: auto;
            width: 100%;
            min-width: 100%;
            font-size: {{ fontSizeBase }};
            margin: 0 0 0 0;
            padding: {{ bodyPadding }};
            vertical-align: top;
            background-color: #FFFFFF;
        }
        
         /* helper classes not included in stock bs3 */
        
        .margin-t-md {
            margin-top: 15px; !important
        }
		.margin-r-md {
            margin-right: 15px; !important
        }
        .margin-b-md {
            margin-bottom: 15px; !important
        }
		.margin-l-md {
            margin-left: 15px; !important
        }
        .padding-t-md {
            padding-top: 15px; !important
        }
		.padding-r-md {
            padding-right: 15px; !important
        }
        .padding-b-md {
            padding-bottom: 15px; !important
        }
		.padding-l-md {
            padding-left: 15px; !important
		}
    </style>

<body>

    <!-- set top padding to help align logo and return address with envelope window -->
    <div style=''padding-top:44px''>

    <!-- set fixed height to help align recipient address with envelope window -->
    <div class="row" style=''{% if RenderMedium == ''Html'' %}height:155px{% else %}height:175px{% endif %}''>
        <div class="col-md-6 pull-left">
            <div>
                <img src="{{ publicApplicationRoot }}GetImage.ashx?Id={{ FinancialStatementTemplate.LogoBinaryFileId }}" width="170px" height="56px" />
            </div>
            
            <div>
                {{ organizationAddress }}<br />
                {{ organizationWebsite }}
            </div>
        </div>
        <div class="col-md-6 text-right">
            <h5>Contribution Summary for {{ Salutation }}</h5>
            <p>{{ StatementStartDate | Date:''M/d/yyyy'' }} - {{ StatementEndDate | Date:''M/d/yyyy'' }}<p>
        </div>
    </div>

    <h5>
        {{ Salutation }} <br />
        {{ StreetAddress1 }} <br />
        {% if StreetAddress2 != '''' %}
            {{ StreetAddress2 }} <br />
        {% endif %}
        {{ City }}, {{ State }} {{ PostalCode }}
    </h5>
</div>

<hr style="opacity: .5;" />

<div class=''well'' style=''padding: 8px''>
    <div class="row">
        <div class="col-xs-6 pull-left">
            <strong style=''margin-left: 5px''>Total Cash Gifts This Period</strong>
        </div>
        <div class="col-xs-6 text-right">
            <strong>{{ currencySymbol }}{{ TotalContributionAmount }}</strong>
        </div>
    </div>
</div>

<table class="table table-bordered table-striped table-condensed">
    <thead>
        <tr>
            <th>Date</th>
            <th>Type</th>
            <th>Account</th>
            <th style="text-align:right">Amount</th>
        </tr>
    </thead>    

    <tbody>
    {% for transactionDetail in TransactionDetails %}
        <tr>
            <td>{{ transactionDetail.Transaction.TransactionDateTime | Date:''M/d/yyyy'' }}</td>
            <td>{{ transactionDetail.Transaction.FinancialPaymentDetail.CurrencyTypeValue.Value }}</td>
            <td>{{ transactionDetail.Account.Name }}</td>
            <td style="text-align:right">{{ currencySymbol }}{{ transactionDetail.Amount }}</td>
        </tr>
    {% endfor %}
    </tbody>
    <tfoot>
    </tfoot>
</table>

{% assign nonCashCount = TransactionDetailsNonCash | Size %}

{% if nonCashCount > 0 %}
	<hr style="opacity: .5;" />

	<h3>Non-Cash Gifts</h3>

	<table class="table table-condensed">
		<thead>
			<tr>
				<th>Date</th>
				<th>Fund</th>
				<th>Description</th>
				<th style="text-align:right">Amount</th>
			</tr>
		</thead>    

		<tbody>
		{% for transactionDetailNonCash in TransactionDetailsNonCash %}
			<tr>
				<td>{{ transactionDetailNonCash.Transaction.TransactionDateTime | Date:''M/d/yyyy'' }}</td>
				<td>{{ transactionDetailNonCash.Account.Name }}</td>
				<td>{{ transactionDetailNonCash.Transaction.Summary }} {{ transactionDetailNonCash.Summary }}</td>
				<td style="text-align:right">{{ currencySymbol }}{{ transactionDetailNonCash.Amount }}</td>
			</tr>
		{% endfor %}
		</tbody>
		<tfoot>
		</tfoot>
	</table>
{% endif %}

{% assign accountSummaryCount = AccountSummary | Size %}

{% if accountSummaryCount > 0 %}
<hr style="opacity: .5;" />

{% if RenderMedium == ''Html'' %}
<div class="row">
    <div class="col-xs-6 col-xs-offset-6">
        <h4 class="margin-t-md margin-b-md">Fund Summary</h4>
        <div class="row">
            <div class="col-xs-6">
                <strong>Fund Name</strong>
            </div>
            <div class="col-xs-6 text-right">
                <strong>Total Amount</strong>
            </div>
        </div>
        
        {% for accountsummary in AccountSummary %}
            <div class="row">
                <div class="col-xs-6">{{ accountsummary.AccountName }}</div>
                <div class="col-xs-6 text-right">{{ accountsummary.Total | FormatAsCurrency }}</div>
            </div>
         {% endfor %}
    </div>
</div>
{% else %}
	<h3>Account Totals</h3>
	{% for accountsummary in AccountSummary %}
		<div class="row">
			<div class="col-xs-3 pull-left">{{ accountsummary.AccountName }}</div>
			<div class="col-xs-3 text-right">{{ currencySymbol }}{{ accountsummary.Total }}</div>
			<div class="col-xs-6"></div>
		</div>
	{% endfor %}
{% endif %}

{% endif %}
 
{% assign pledgeCount = Pledges | Size %}

{% if pledgeCount > 0 %}
    <hr style="opacity: .5;" />

    <h3>Pledges</h3>
 
    {% for pledge in Pledges %}
        <div class="row">
            <div class="col-xs-3">
                <strong>{{ pledge.AccountName }}</strong>
                
                <p>
                    Amt Pledged: {{ currencySymbol }}{{ pledge.AmountPledged }} <br />
                    Amt Given: {{ currencySymbol }}{{ pledge.AmountGiven }} <br />
                    Amt Remaining: {{ currencySymbol }}{{ pledge.AmountRemaining }}
                </p>
            </div>
            <div class="col-xs-3">
                <br />
                <p>
                    Percent Complete <br />
                    {{ pledge.PercentComplete }}%
                    <br />
                </p>
            </div>
        </div>
    {% endfor %}
{% endif %}

<hr style="opacity: .5;" />
<p class="text-center">
    Thank you for your continued support of the {{ organizationName }}. If you have any questions about your statement,
    email {{ organizationEmail }} or call {{ organizationPhone }}.
</p>

<p class="text-center">
    <em>Unless otherwise noted, the only goods and services provided are intangible religious benefits.</em>
</p>

</body>
</html>' WHERE ([Guid]='4B93657A-DD5F-4D8A-A13F-1B4E9ADBDAD0')