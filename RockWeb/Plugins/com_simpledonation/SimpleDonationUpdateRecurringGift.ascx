<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SimpleDonationUpdateRecurringGift.ascx.cs" Inherits="Plugins.com_simpledonation.SimpleDonationUpdateRecurringGift" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
			
        <asp:Panel ID="pnlView" runat="server"> 
            <div class="panel-body" style="margin:0; padding:0;">
                <Rock:Lava runat="server" ID="updateRecurringGift">
                    {% if CurrentPerson %}
                    {% assign currentPersonId = CurrentPerson.Id %}

                    {% comment %}{{ currentPersonId }}{% endcomment %}

                    {% sql return:'scheduledtransactions' %}
                        SELECT
                            st.TransactionCode [transactionCode],
                            std.Amount [amount],
                            std.AccountId [account],
                            CASE WHEN fpd.AccountNumberMasked IS NULL THEN 'zzz' ELSE fpd.AccountNumberMasked END [updated]
                        FROM FinancialScheduledTransaction st
                        JOIN FinancialScheduledTransactionDetail std
                            ON std.ScheduledTransactionId=st.Id
                        JOIN PersonAlias pa
                            ON pa.Id = st.AuthorizedPersonAliasId
                        JOIN Person p
                            ON p.ID = pa.PersonId
                        LEFT JOIN FinancialPaymentDetail fpd
                            ON fpd.Id = st.FinancialPaymentDetailId
                        WHERE p.ID = {{ currentPersonId }}
                            AND (fpd.AccountNumberMasked IS NULL OR fpd.Guid = '54487E50-64C1-420E-8A87-5BBD6AC9BA82')
                    {% endsql %}


                    {% sql return:'SDsubDomain' %}
                        SELECT 
	                        Value
                        FROM    [AttributeValue] av
                        JOIN    [Attribute] a
	                        ON a.Id = av.AttributeId
                        JOIN	[FieldType] ft
	                        ON a.FieldTypeId = ft.Id
                        WHERE
                            a.[Name] = 'Domain'
                        AND a.[Description] like '%Simple Donation%'
                        AND av.[Value] like '%simpledonation.com'
                        AND ft.Name = 'Url Link'
                    {% endsql %}

                    {% for item in scheduledtransactions %}
                        {% assign txs = scheduledtransactions | Size %}
                    {% endfor %}

                    {% if txs >= 2 %}
                        <div class="p-4 mb-4 text-sm text-blue-700 bg-blue-100 rounded-lg dark:bg-blue-200 dark:text-blue-800" role="alert">
                            <p class="p-0 m-0" style="font-size: 1.1em;">Note that you have a total of <b><u>{{ txs }}</u></b> recurring gifts to confirm. Please scroll down to confirm payment method on all gifts.</p>
                        </div>
                    {% endif %}

                    {% for item in scheduledtransactions %}
                        {% assign firstItem = item in SDsubDomain | First %}
                            {% for firstItem in SDsubDomain | Remove:'https://' %}
                                <iframe src="https://{{ firstItem.Value | Remove:'https://' }}/recurring_donations/{{ item.transactionCode }}/edit" width="100%" frameborder="0" scrolling="no" allowtransparency="true"></iframe>
                            {% endfor %}
                    {% endfor %}

                    <script type="text/javascript" src="//simpledonation.com/assets/iframeResizer.js"></script>
                    <script type="text/javascript">iFrameResize({checkOrigin: false});</script>

                    {% assign alldone = scheduledtransactions %}
                        {% if alldone != empty  %}
                            {% else %}
                                <p>You must be logged in and have a recurring gift scheduled that hasn't been updated.  If you're seeing this message, you're all done.</p>
                        {% endif %}

                            {% else %}
                                <p>You must be logged in to use this page </p>
                    {% endif %}{% comment %}from if CurrentPerson{% endcomment %}
                </Rock:Lava>

            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>