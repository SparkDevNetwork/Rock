<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SimpleDonationRecurringMigrationProgressBars.ascx.cs" Inherits="Plugins.com_simpledonation.SimpleDonationRecurringMigrationProgressBars" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
			
        <asp:Panel ID="pnlView" runat="server"> 
            <div class="panel-body" style="margin:0; padding: 0 0 20px 0;">
                    
				<style>
					.bg-error { background-color: #ff0000 !important; }
					.bg-success { background-color: #e6e600!important; }
					.bg-info    { background-color: green!important;}
					.legend { width: 25px; height: 25px; margin-right: 20px;}
					table.status-legend td:nth-of-type(2) { padding-left: 20px; }
				</style>

				<Rock:Lava runat="server" ID="ProgressBars">
					{% sql return:'totals' %}
						SELECT
							COUNT(DISTINCT(st.Id)) [total],
							COALESCE(SUM(std.Amount),0) [totalDollars]
						FROM FinancialScheduledTransaction st
						JOIN FinancialScheduledTransactionDetail std
							ON std.ScheduledTransactionId=st.Id
						JOIN PersonAlias pa
							ON pa.Id = st.AuthorizedPersonAliasId
						JOIN Person p
							ON p.ID = pa.PersonId
						JOIN DefinedValue dv
							ON dv.Id = st.TransactionFrequencyValueId
						LEFT JOIN Attribute Att
							ON Att."Key" = 'SimpleDonationMigration'
						LEFT JOIN AttributeValue av
							ON av.AttributeId = Att.Id AND av.EntityId = st.Id
						LEFT JOIN Attribute sdstatusatt
							ON sdstatusatt."Key" = 'SimpleDonationRecurringStatus'
						LEFT JOIN AttributeValue sdstatusav
							ON sdstatusav.AttributeId = sdstatusatt.Id AND sdstatusav.EntityId = st.Id
						WHERE av.Value = 'true'
					{% endsql %}

					{% assign totalvalues = totals | First  %}

					{% sql return:'toBeUpdated' %}
						SELECT
							COUNT(DISTINCT(st.Id)) [total],
							COALESCE(SUM(std.Amount),0) [totalDollars]
						FROM FinancialScheduledTransaction st
						JOIN FinancialScheduledTransactionDetail std
							ON std.ScheduledTransactionId=st.Id
						JOIN PersonAlias pa
							ON pa.Id = st.AuthorizedPersonAliasId
						JOIN Person p
							ON p.ID = pa.PersonId
						JOIN DefinedValue dv
							ON dv.Id = st.TransactionFrequencyValueId
						LEFT JOIN Attribute Att
							ON Att."Key" = 'SimpleDonationMigration'
						LEFT JOIN AttributeValue av
							ON av.AttributeId = Att.Id AND av.EntityId = st.Id
						LEFT JOIN Attribute sdstatusatt
							ON sdstatusatt."Key" = 'SimpleDonationRecurringStatus'
						LEFT JOIN AttributeValue sdstatusav
							ON sdstatusav.AttributeId = sdstatusatt.Id AND sdstatusav.EntityId = st.Id
						LEFT JOIN FinancialPaymentDetail fpd
							ON fpd.Id = st.FinancialPaymentDetailId
						WHERE av.Value = 'true'
							AND (fpd.Guid = '54487E50-64C1-420E-8A87-5BBD6AC9BA82' OR fpd.AccountNumberMasked IS NULL)
					{% endsql %}

					{% assign toUpdate = toBeUpdated | First  %}

					{% sql return:'toBeActivated' %}
						SELECT
							COUNT(DISTINCT(st.Id)) [total],
							COALESCE(SUM(std.Amount),0) [totalDollars]
						FROM FinancialScheduledTransaction st
						JOIN FinancialScheduledTransactionDetail std
							ON std.ScheduledTransactionId=st.Id
						JOIN PersonAlias pa
							ON pa.Id = st.AuthorizedPersonAliasId
						JOIN Person p
							ON p.ID = pa.PersonId
						JOIN DefinedValue dv
							ON dv.Id = st.TransactionFrequencyValueId
						LEFT JOIN Attribute Att
							ON Att."Key" = 'SimpleDonationMigration'
						LEFT JOIN AttributeValue av
							ON av.AttributeId = Att.Id AND av.EntityId = st.Id
						LEFT JOIN Attribute sdstatusatt
							ON sdstatusatt."Key" = 'SimpleDonationRecurringStatus'
						LEFT JOIN AttributeValue sdstatusav
							ON sdstatusav.AttributeId = sdstatusatt.Id AND sdstatusav.EntityId = st.Id
						LEFT JOIN FinancialPaymentDetail fpd
							ON fpd.Id = st.FinancialPaymentDetailId
						WHERE av.Value = 'true'
							AND (fpd.Guid != '54487E50-64C1-420E-8A87-5BBD6AC9BA82' OR fpd.AccountNumberMasked IS NOT NULL)
							AND sdstatusav.Value IN ('Inactive','Cancelled')
					{% endsql %}

					{% assign toActivate = toBeActivated | First  %}

					{% sql return:'alreadyActivated' %}
						SELECT
							COUNT(DISTINCT(st.Id)) [total],
							COALESCE(SUM (std.Amount),0) [totalDollars]
						FROM FinancialScheduledTransaction st
						JOIN FinancialScheduledTransactionDetail std
							ON std.ScheduledTransactionId=st.Id
						JOIN PersonAlias pa
							ON pa.Id = st.AuthorizedPersonAliasId
						JOIN Person p
							ON p.ID = pa.PersonId
						JOIN DefinedValue dv
							ON dv.Id = st.TransactionFrequencyValueId
						LEFT JOIN Attribute Att
							ON Att."Key" = 'SimpleDonationMigration'
						LEFT JOIN AttributeValue av
							ON av.AttributeId = Att.Id AND av.EntityId = st.Id
						LEFT JOIN Attribute sdstatusatt
							ON sdstatusatt."Key" = 'SimpleDonationRecurringStatus'
						LEFT JOIN AttributeValue sdstatusav
							ON sdstatusav.AttributeId = sdstatusatt.Id AND sdstatusav.EntityId = st.Id
						LEFT JOIN FinancialPaymentDetail fpd
							ON fpd.Id = st.FinancialPaymentDetailId
						WHERE av.Value = 'true'
							AND st.FinancialPaymentDetailId IS NOT NULL		
							AND (fpd.Guid != '54487E50-64C1-420E-8A87-5BBD6AC9BA82' OR sdstatusav.Value = 'Active')
					{% endsql %}

					{% assign activated = alreadyActivated | First  %}

					{% assign percentToUpdate = toUpdate.total | Times:100 | DividedBy:totalvalues.total,2 %}
					{% assign percentToActivate = toActivate.total | Times:100 | DividedBy:totalvalues.total,2 %}
					{% assign percentActivated = activated.total | Times:100 | DividedBy:totalvalues.total,2 %}
					{% assign percentDollarsToUpdate = toUpdate.totalDollars | Times:100 | DividedBy:totalvalues.totalDollars,2 %}
					{% assign percentDollarsToActivate = toActivate.totalDollars | Times:100 | DividedBy:totalvalues.totalDollars,2 %}
					{% assign percentDollarsActivated = activated.totalDollars | Times:100 | DividedBy:totalvalues.totalDollars,2 %}


					<h4>Status of Transactions Migrated</h4>
						<div class="progress">
							<div class="progress-bar bg-error" role="progressbar" style="width:{{ percentToUpdate }}%" aria-valuenow="15" aria-valuemin="0" aria-valuemax="100">{{ percentToUpdate }}%</div>
							<div class="progress-bar bg-success" role="progressbar" style="width: {{ percentToActivate }}%" aria-valuenow="30" aria-valuemin="0" aria-valuemax="100">{{ percentToActivate }}%</div>
							<div class="progress-bar bg-info" role="progressbar" style="width: {{ percentToUpdate | Plus:percentToActivate | Minus:100.00 | Times:-1.00 }}%" aria-valuenow="20" aria-valuemin="0" aria-valuemax="100">{{ percentToUpdate | Plus:percentToActivate | Minus:100.00 | Times:-1.00 }}%</div>
						</div>
						<table class="status-legend">
							<tr>
								<td class="legend bg-error"></td>
								<td>Percent of transactions without payments</td>
							</tr>
							<tr>
								<td class="legend bg-success"></td>
								<td>Percent of transactions to be activated</td>
							</tr>
							<tr>
								<td class="legend bg-info"></td>
								<td>Percent of transactions migrated</td>
							</tr>
						</table>

					<h4>Status of Dollars Migrated</h4>
						<div class="progress">
							<div class="progress-bar" role="progressbar" style="background-color:red;width: {{ percentDollarsToUpdate }}%" aria-valuenow="15" aria-valuemin="0" aria-valuemax="100">{{percentDollarsToUpdate}}%</div>
							<div class="progress-bar bg-success" role="progressbar" style="width: {{ percentDollarsToActivate }}%" aria-valuenow="30" aria-valuemin="0" aria-valuemax="100">{{ percentDollarsToActivate }}%</div>
							<div class="progress-bar bg-info" role="progressbar" style="width: {{ percentDollarsToUpdate | Plus:percentDollarsToActivate | | Minus:100.00 | Times:-1.00  }}%" aria-valuenow="20" aria-valuemin="0" aria-valuemax="100">{{ percentDollarsToUpdate | Plus:percentDollarsToActivate | | Minus:100.00 | Times:-1.00  }}%</div>
						</div>
						<table class="status-legend">
							<tr>
								<td class="legend bg-error"></td>
								<td>Percent of Dollars without Payments</td>
							</tr>
							<tr>
								<td class="legend bg-success"></td>
								<td>Percent of Dollars to be Activated</td>
							</tr>
							<tr>
								<td class="legend bg-info"></td>
								<td>Percent of Dollars Migrated</td>
							</tr>
						</table>
				</Rock:Lava>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>