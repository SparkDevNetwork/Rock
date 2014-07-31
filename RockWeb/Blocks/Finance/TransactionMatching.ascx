<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TransactionMatching.ascx.cs" Inherits="RockWeb.Blocks.Finance.TransactionMatching" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <!-- TODO talk about this... -->
        <style>
            .check-image {
                max-width: 500px;
                margin: 5px;
            }

            .check-image-thumbnail {
                max-width: 100px;
                margin: 5px;
            }
        </style>

        <asp:HiddenField ID="hfBackNextHistory" runat="server" />
        <asp:HiddenField ID="hfHistoryPosition" runat="server" />
        <asp:HiddenField ID="hfTransactionId" runat="server" />
        <asp:HiddenField ID="hfBatchId" runat="server" />
        <asp:HiddenField ID="hfCheckMicrHashed" runat="server" />
        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lPanelTitle" runat="server" /></h1>
                <asp:LinkButton ID="btnFilter" runat="server" CssClass="btn pull-right" OnClick="btnFilter_Click"><i class="fa fa-gear" title="Filter Accounts"></i></asp:LinkButton>
                <Rock:HighlightLabel ID="hlRemainingCount" runat="server" />
            </div>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbNoUnmatchedTransactionsRemaining" runat="server" NotificationBoxType="Success" Text="<i class='fa fa-2x fa-check-circle'></i> There are no more unmatched checks in this batch." />
                <asp:Panel ID="pnlEdit" runat="server">
                    <div class="row">
                        <div class="col-md-6">
                            <asp:Literal ID="lTransactionInfo" runat="server" />
                            <Rock:NotificationBox ID="nbNoCheckImageWarning" runat="server" NotificationBoxType="Warning" Text="Warning. No Check Images found for this transaction." />
                            <Rock:NotificationBox ID="nbIsInProcess" runat="server" NotificationBoxType="Warning" Text="Warning. This check is getting processed by ...." />
                            <Rock:NotificationBox ID="nbNoMicrWarning" runat="server" NotificationBoxType="Danger" Text="Warning. Unable to determine Checking Account Number" />
                            <asp:Image ID="imgCheck" runat="server" CssClass="check-image" AlternateText="Unable to display check. Verify that user is authorized to view Check Images." />
                            <br />
                            <asp:Image ID="imgCheckOtherSideThumbnail" runat="server" CssClass="check-image-thumbnail" ToolTip="Click to toggle" onclick="javascript: toggleCheckImages();" />
                        </div>
                        <div class="col-md-6">

                            <Rock:RockDropDownList ID="ddlIndividual" runat="server" Label="Individual" Help="Select a person that has previously been matched to the checking account." />
                            <a id="hlAddNewFamily" runat="server" href="#">Add Family</a>
                            <Rock:PersonPicker ID="ppSelectNew" runat="server" Label="Assign to New" Help="Select a new person to match to the checking account." />

                            <Rock:NotificationBox ID="nbSaveError" runat="server" NotificationBoxType="Danger" Text="Warning. Unable to save..." />
                            <Rock:RockControlWrapper ID="rcwAccountSplit" runat="server" Label="Account Split" Help="Enter the amount that should be allocated to each account. The total must match the amount shown on the check">
                                <asp:Repeater ID="rptAccounts" runat="server">
                                    <ItemTemplate>
                                        <Rock:CurrencyBox ID="cbAccountAmount" runat="server" Label='<%#Eval( "Name" )%>' data-account-id='<%#Eval("Id")%>' CssClass="js-account-amount" onkeyup="javascript: updateRemainingAccountAllocation()" />
                                    </ItemTemplate>
                                </asp:Repeater>
                            </Rock:RockControlWrapper>

                            <%-- note: using disabled instead of readonly so that we can set the postback value in javascript --%>
                            <Rock:CurrencyBox ID="cbTotalAmount" runat="server" Label="Total Amount" CssClass="js-total-amount" disabled="disabled" Text="0.00"></Rock:CurrencyBox>

                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <asp:LinkButton ID="btnPrevious" runat="server" CssClass="btn" OnClick="btnPrevious_Click">Previous</asp:LinkButton>
                            <asp:LinkButton ID="btnNext" runat="server" CssClass="btn btn-primary pull-right" OnClick="btnNext_Click">Next <i class="fa fa-chevron-right"></i></asp:LinkButton>
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-12" style="text-align: right">
                            <Rock:HighlightLabel ID="hlUnmatchedRemaining" runat="server" LabelType="Info" ToolTip="Number of unmatched transactions remaining in this batch." />
                            <Rock:HighlightLabel ID="hlUnvisitedRemainingDebug" runat="server" LabelType="Default" ToolTip="Number of unvisited transactions remaining in this batch." />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-12">
                            <div style="text-align: center">
                                <asp:Literal ID="lBookmarkDebug" runat="server" Text="1|2|<u>3</u>|4|5" />
                            </div>
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
            function updateRemainingAccountAllocation() {
                // do currency math in Cents instead of Dollars to avoid floating point math issues
                var checkTotalAmountCents = null;

                $('#<%=pnlView.ClientID%> .js-account-amount :input').each(function (index, elem) {
                    var accountAmountDollar = $(elem).val();
                    if (!isNaN(accountAmountDollar) && accountAmountDollar != "") {
                        checkTotalAmountCents = (checkTotalAmountCents || 0.00) + Number(accountAmountDollar) * 100;
                    }
                });

                var checkTotalAmountDollars = checkTotalAmountCents != null ? (checkTotalAmountCents / 100).toFixed(2) : null;

                $('#<%=pnlView.ClientID%>').find('.js-total-amount :input').val(checkTotalAmountDollars);
            }

            function toggleCheckImages() {
                var image1src = $('#<%=imgCheck.ClientID%>').attr("src");
                var image2src = $('#<%=imgCheckOtherSideThumbnail.ClientID%>').attr("src");

                $('#<%=imgCheck.ClientID%>').attr("src", image2src);
                $('#<%=imgCheckOtherSideThumbnail.ClientID%>').attr("src", image1src);
            }

            Sys.Application.add_load(function () {
                $('#<%=pnlView.ClientID%>').rockFadeIn();
            })
        </script>

    </ContentTemplate>
</asp:UpdatePanel>
