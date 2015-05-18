<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TransactionMatching.ascx.cs" Inherits="RockWeb.Blocks.Finance.TransactionMatching" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfBackNextHistory" runat="server" />
        <asp:HiddenField ID="hfHistoryPosition" runat="server" />
        <asp:HiddenField ID="hfTransactionId" runat="server" />
        <asp:HiddenField ID="hfBatchId" runat="server" />
        <asp:HiddenField ID="hfCheckMicrHashed" runat="server" />
        <asp:HiddenField ID="hfDoFadeIn" runat="server" />
        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lPanelTitle" runat="server" /></h1>

                <asp:Literal ID="lProgressBar" runat="server"></asp:Literal>
                
                <asp:LinkButton ID="btnFilter" runat="server" CssClass="btn btn-xs btn-default pull-right margin-l-sm" OnClick="btnFilter_Click"><i class="fa fa-gear" title="Filter Accounts"></i></asp:LinkButton>
                
                <Rock:RockControlWrapper ID="rcwAddNewBusiness" runat="server"  Visible="false">
                    <a id="hlAddNewBusiness" class="btn btn-default btn-xs margin-r-sm pull-right" runat="server" href="#">Add Business</a>
                </Rock:RockControlWrapper>
                
                <Rock:RockControlWrapper ID="rcwAddNewFamily" runat="server"  Visible="false">
                    <a id="hlAddNewFamily" class="btn btn-default btn-xs margin-r-sm pull-right" runat="server" href="#">Add Family</a>
                </Rock:RockControlWrapper>
            </div>

            <div class="panel-body">
                <Rock:NotificationBox ID="nbNoUnmatchedTransactionsRemaining" runat="server" NotificationBoxType="Success" Text="<i class='fa fa-2x fa-check-circle'></i> There are no more unmatched transactions in this batch." />
                <asp:Panel ID="pnlEdit" runat="server">
                    <div class="row">
                        <div class="col-md-7">
                            <Rock:NotificationBox ID="nbNoTransactionImageWarning" runat="server" NotificationBoxType="Warning" Text="Warning. No Images found for this transaction." />
                            <Rock:NotificationBox ID="nbIsInProcess" runat="server" NotificationBoxType="Warning" Text="Warning. This transaction is getting processed by ...." />
                            <Rock:NotificationBox ID="nbNoMicrWarning" runat="server" NotificationBoxType="Danger" Text="Warning. Unable to determine Checking Account Number" />
                            <div>
                                <asp:Image ID="imgPrimary" runat="server" CssClass="transaction-image" />
                            </div>
                            <div>
                                <asp:Repeater ID="rptrImages" runat="server">
                                    <ItemTemplate>
                                        <asp:Image ID="imgOther" ImageUrl='<%# ImageUrl( (int)Eval("BinaryFileId") ) %>' runat="server" CssClass="transaction-image-thumbnail" ToolTip="Click to toggle" />
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                        </div>
                        <div class="col-md-5">
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockDropDownList ID="ddlIndividual" runat="server" Label="Individual" Help="Select a person that has previously been matched to the bank account. If the person isn't in this list, use the 'Assign to New' to select the matching person." AutoPostBack="true" OnSelectedIndexChanged="ddlIndividual_SelectedIndexChanged" />
                                    <Rock:PersonPicker ID="ppSelectNew" runat="server" Label="Assign to New" Help="Select a new person to match to the bank account." IncludeBusinesses="true" OnSelectPerson="ppSelectNew_SelectPerson"/>
                                </div>

                                <div class="col-md-6">
                                    <asp:Panel ID="pnlPreview" CssClass="contents" runat="server">
                                        <asp:Literal ID="lPersonName" runat="server" />
                                        <p>
                                            <asp:Literal ID="lSpouseName" runat="server" />
                                        </p>

                                        <!-- List of addresses associated with this person -->

                                        <ul class="list-unstyled">
                                            <asp:Repeater ID="rptrAddresses" runat="server">
                                                <ItemTemplate>
                                                    <li class="address clearfix">

                                                        <strong><%# Eval("GroupLocationTypeValue.Value") %></strong>
                                                        <p>
                                                            <%# Eval("Location.FormattedHtmlAddress") %>
                                                        </p>
                                                    </li>
                                                </ItemTemplate>
                                            </asp:Repeater>
                                        </ul>


                                    </asp:Panel>
                                </div>
                            </div>

                            
                            <Rock:NotificationBox ID="nbSaveError" runat="server" NotificationBoxType="Danger" Dismissable="true" Text="Warning. Unable to save..." />
                            <Rock:RockControlWrapper ID="rcwAccountSplit" runat="server" Label="Account Split" Help="Enter the amount that should be allocated to each account. The total must match the amount shown on the transaction image">
                                <asp:Repeater ID="rptAccounts" runat="server">
                                    <ItemTemplate>
                                        <Rock:CurrencyBox ID="cbAccountAmount" runat="server" Label='<%#Eval( "Name" )%>' data-account-id='<%#Eval("Id")%>' CssClass="js-account-amount" onkeypress="javascript:handleAmountBoxKeyPress(event.keyCode)" />
                                    </ItemTemplate>
                                </asp:Repeater>
                            </Rock:RockControlWrapper>

                            <%-- note: using disabled instead of readonly so that we can set the postback value in javascript --%>
                            <Rock:CurrencyBox ID="cbTotalAmount" runat="server" Label="Total Amount" CssClass="js-total-amount" Help="Allocates amounts to the above account(s) until the total amount matches what is shown on the transaction image." disabled="disabled" Text="0.00"></Rock:CurrencyBox>

                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <asp:LinkButton ID="btnPrevious" runat="server" CssClass="btn" OnClick="btnPrevious_Click">Previous</asp:LinkButton>
                            <asp:LinkButton ID="btnNext" runat="server" AccessKey="" CssClass="btn btn-primary pull-right" OnClick="btnNext_Click">Next <i class="fa fa-chevron-right"></i></asp:LinkButton>
                        </div>
                    </div>

                </asp:Panel>
            </div>

            <Rock:ModalDialog ID="mdAccountsPersonalFilter" runat="server" Title="Accounts Filter" OnSaveClick="mdAccountsPersonalFilter_SaveClick">
                <Content>
                    <Rock:AccountPicker ID="apPersonalAccounts" runat="server" AllowMultiSelect="true" Label="Select Account(s)" />
                </Content>
            </Rock:ModalDialog>

        </asp:Panel>

        <script>
            // update the Total Amount UI text as amounts are edited
            function updateRemainingAccountAllocation() {
                // do currency math in Cents instead of Dollars to avoid floating point math issues
                var transactionTotalAmountCents = null;

                $('#<%=pnlView.ClientID%> .js-account-amount :input').each(function (index, elem) {
                    var accountAmountDollar = $(elem).val();
                    if (!isNaN(accountAmountDollar) && accountAmountDollar != "") {
                        transactionTotalAmountCents = (transactionTotalAmountCents || 0.00) + Number(accountAmountDollar) * 100;
                    }
                });

                var transactionTotalAmountDollars = transactionTotalAmountCents != null ? (transactionTotalAmountCents / 100).toFixed(2) : null;

                $('#<%=pnlView.ClientID%>').find('.js-total-amount :input').val(transactionTotalAmountDollars);
            }

            Sys.Application.add_load(function () {
                if ($('#<%=hfDoFadeIn.ClientID%>').val() == "1") {
                    $('#<%=pnlView.ClientID%>').rockFadeIn();
                }
            })

            // handle onkeypress for the account amount input boxes
            function handleAmountBoxKeyPress(keyCode)
            {
                // if Enter was pressed when in one of the Amount boxes, click the Next button.  Otherwise, updateRemainingAccountAllocation()
                if (keyCode == 13)
                {
                    $('#<%=btnNext.ClientID%>')[0].click();
                } else {
                    updateRemainingAccountAllocation();
                }
            }
        </script>

    </ContentTemplate>
</asp:UpdatePanel>
