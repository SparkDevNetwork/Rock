<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FundList.ascx.cs" Inherits="RockWeb.Plugins.com_pushPay.RockRMS.FundList" %>

<asp:UpdatePanel ID="upnlList" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlFunds" runat="server" CssClass="panel panel-block" >
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-building-o"></i> <asp:Literal ID="lMerchantName" runat="server" Text="Pushpay Merchant Listing" /> Funds</h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:Grid ID="gFunds" runat="server" AllowSorting="true" >
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Pushpay Fund Name" SortExpression="Name" />
                            <Rock:RockBoundField DataField="FinancialAccount" HeaderText="Rock Financial Account" HtmlEncode="false" />
                            <Rock:EditField OnClick="gFunds_Edit" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>

        <Rock:ModalDialog runat="server" ID="mdEditFundSettings" SaveButtonText="Save" Title="Pushpay Fund Settings" OnSaveClick="mdEditFundSettings_SaveClick" ValidationGroup="FundEdit">
            <Content>
                <asp:HiddenField runat="server" ID="hfFundId" />
                <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="MerchantEdit" />
                <div class="well">
                    <asp:RadioButtonList ID="rblFundCampusOption" runat="server" RepeatDirection="Horizontal" OnSelectedIndexChanged="rblFundCampusOption_SelectedIndexChanged" AutoPostBack="true" >
                        <asp:ListItem Value="False" Text="Use one account for this fund" Selected="True" />
                        <asp:ListItem Value="True" Text="Select accounts by campus" />
                    </asp:RadioButtonList>
                </div>
                <asp:Panel ID="pnlSameAccount" runat="server" CssClass="row">
                    <div class="col-md-6">
                        <Rock:AccountPicker ID="apRockAccount" runat="server" Label="Rock Financial Account" Help="The Rock Financial Account that should be used when downloading transactions from Pushpay with this fund." />
                    </div>
                    <div class="col-md-6">
                    </div>
                </asp:Panel>
                <asp:Panel ID="pnlCampusAccounts" runat="server" Visible="false">
                    <Rock:NotificationBox ID="nbCampusAccountWarning" runat="server" Heading="Unsupported Configuration" 
                        Text="<p>The merchant listing for this fund does not have a campus field, or you have not selected the campus field when configuring the merchant listing.</p>" 
                        Visible="false" NotificationBoxType="Warning" />
                    <asp:Repeater ID="rptCampusAccounts" runat="server">
                        <HeaderTemplate>
                            <div class="row">
                                <div class="col-sm-6">
                                    <strong>Campus</strong>
                                </div>
                                <div class="col-sm-6">
                                    <strong>Financial Account</strong>
                                </div>
                            </div>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <hr />
                            <asp:HiddenField ID="hfCampusValue" runat="server" Value='<%# Eval("CampusKey") %>' />
                            <div class="row">
                                <div class="col-sm-6">
                                    <%# Eval("Name") %>
                                </div>
                                <div class="col-sm-6">
                                    <Rock:AccountPicker ID="apCampusAccount" runat="server" />
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </asp:Panel>
            </Content>
        </Rock:ModalDialog>

        <script type="text/javascript">
            Sys.Application.add_load(function () {
                $('[data-toggle="tooltip"]').tooltip()
            })
        </script>

    </ContentTemplate>
</asp:UpdatePanel>
