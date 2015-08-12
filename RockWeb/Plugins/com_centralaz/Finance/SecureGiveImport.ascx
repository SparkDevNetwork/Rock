<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SecureGiveImport.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Finance.SecureGiveImport" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlImport" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">Import SecureGive</h1>
            </div>
            <div class="panel-body">

                <Rock:RockTextBox runat="server" ID="tbBatchName" Label="Batch Name" ToolTip="The name you wish to use for this batch import."></Rock:RockTextBox>

                <p>
                    <asp:FileUpload runat="server" ID="fuImport" CssClass="input-small" />
                </p>

                <p>
                    <asp:LinkButton runat="server" ID="lbImport" CssClass="btn btn-default btn-sm" OnClick="lbImport_Click">
                                    <i class="fa fa-arrow-up"></i> Import
                    </asp:LinkButton>
                </p>

                <Rock:NotificationBox ID="nbMessage" runat="server" NotificationBoxType="Danger" />

                <asp:Panel ID="pnlErrors" runat="server" Visible="false" CssClass="alert alert-danger block-message error">
                        <Rock:Grid ID="gErrors" runat="server" AllowSorting="false" OnRowDataBound="gErrors_RowDataBound" RowItemText="error" AllowPaging="false" RowStyle-CssClass="danger" AlternatingRowStyle-CssClass="danger" ShowActionRow="false" >
                            <Columns>
                                <asp:TemplateField SortExpression="ReferenceNumber" HeaderText="Reference Number">
                                            <ItemTemplate>
                                                <asp:Literal ID="lReferenceNumber" runat="server" />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                <asp:TemplateField SortExpression="ChurchCode" HeaderText="Church Code" Visible="false">
                                            <ItemTemplate>
                                                <asp:Literal ID="lChurchCode" runat="server" />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                <asp:TemplateField SortExpression="IndividualId" HeaderText="Individual ID">
                                            <ItemTemplate>
                                                <asp:Literal ID="lIndividualId" runat="server" />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                <asp:TemplateField SortExpression="ContributorName" HeaderText="Contributor Name">
                                            <ItemTemplate>
                                                <asp:Literal ID="lContributorName" runat="server" />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                <asp:TemplateField SortExpression="FundName" HeaderText="Fund Name">
                                            <ItemTemplate>
                                                <asp:Literal ID="lFundName" runat="server" />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                <asp:TemplateField SortExpression="FundCode" HeaderText="Fund Code">
                                            <ItemTemplate>
                                                <asp:Literal ID="lFundCode" runat="server" />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                <asp:TemplateField SortExpression="ReceivedDate" HeaderText="Received Date">
                                            <ItemTemplate>
                                                <asp:Literal ID="lReceivedDate" runat="server" />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                <asp:TemplateField SortExpression="Amount" HeaderText="Amount">
                                            <ItemTemplate>
                                                <asp:Literal ID="lAmount" runat="server" />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                <asp:TemplateField SortExpression="TransactionId" HeaderText="Transaction ID">
                                            <ItemTemplate>
                                                <asp:Literal ID="lTransactionId" runat="server" />
                                            </ItemTemplate>
                                        </asp:TemplateField>                            
                                <asp:TemplateField SortExpression="ContributionType" HeaderText="Contribution Type">
                                            <ItemTemplate>
                                                <asp:Literal ID="lContributionType" runat="server" />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                            </Columns>
                        </Rock:Grid>
                    </asp:Panel>

                <Rock:NotificationBox ID="nbBatch" runat="server" NotificationBoxType="Success" />

            </div>
        </asp:Panel>

        <asp:Panel ID="pnlGrid" runat="server" CssClass="panel panel-block" Visible="false">
            <div class="panel-heading">
                <h1 class="panel-title">Batch Summary</h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:Grid ID="gContributions" runat="server" AllowSorting="false" AllowPaging="false" >
                        <Columns>
                            <asp:TemplateField SortExpression="TransactionId" HeaderText="Transaction ID">
                                        <ItemTemplate>
                                            <asp:Literal ID="lTransactionID" runat="server" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                            <asp:BoundField DataField="Transaction.ProcessedDateTime" HeaderText="Transaction Date" SortExpression="TransactionDate" />
                            <asp:TemplateField SortExpression="FullName" HeaderText="Full Name">
                                        <ItemTemplate>
                                            <asp:Literal ID="lFullName" runat="server" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                            <asp:BoundField DataField="Transaction.CurrencyTypeValue" HeaderText="Transaction Type" SortExpression="TransactionType" />
                            <asp:BoundField DataField="Account" HeaderText="Fund Name" SortExpression="FundName" />
                            <asp:BoundField DataField="Amount" HeaderText="Amount" SortExpression="Amount" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
