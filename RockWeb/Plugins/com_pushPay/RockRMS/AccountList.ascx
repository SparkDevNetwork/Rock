<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AccountList.ascx.cs" Inherits="RockWeb.Plugins.com_pushPay.RockRMS.AccountList" %>

<asp:UpdatePanel ID="upnlList" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlNew" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-bank"></i> Pushpay Integration</h1>
            </div>
            <div class="panel-body">
                <div class="col-md-6">
                    <asp:Image ID="imgPromotion" runat="server" CssClass="img-responsive" />
                    <div class="actions margin-t-md">
                        <asp:HyperLink ID="hlRequestAPI" runat="server" CssClass="btn btn-primary btn-block" >
                            <strong>Existing Pushpay Customer</strong><br />
                            <small>( Request API Keys From Pushpay )</small>
                        </asp:HyperLink>
                        <asp:HyperLink ID="hlGetStarted" runat="server" CssClass="btn btn-default btn-block">
                            <strong>New Pushpay Customer</strong><br />
                            <small>( Register For a Pushpay Account )</small>
                        </asp:HyperLink>
                    </div>
                </div>
                <div class="col-md-1">
                </div>
                <div class="col-md-5">
                    <asp:ValidationSummary ID="ValidationSummaryNew" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="NewAccount" />
                    <Rock:RockTextBox ID="tbNewClientId" runat="server" Label="Pushpay API Client Id" Required="true"  ValidationGroup="NewAccount"
                        Help="The Client ID provided by Pushpay to access the Pushpay API" />
                    <Rock:RockTextBox ID="tbNewClientSecret" runat="server" Label="Pushpay API Client Secret" TextMode="Password" Required="true"  ValidationGroup="NewAccount"
                        Help="The Client Secret provided by Pushpay to access the Pushpay API" />
                    <div class="actions">
                        <asp:LinkButton ID="lbSaveNew" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="lbSaveNew_Click" ValidationGroup="NewAccount" />
                    </div>
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlAccounts" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-bank"></i> Pushpay Accounts</h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:Grid ID="gAccounts" runat="server" AllowSorting="true" OnRowSelected="gAccounts_RowSelected" RowItemText="Account">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Account Name" SortExpression="Name" />
                            <Rock:RockBoundField DataField="ClientId" HeaderText="API Client Id" SortExpression="" />
                            <Rock:BoolField DataField="IsActive" HeaderText="Active" SortExpression="IsActive" />
                            <Rock:BoolField DataField="Authorized" HeaderText="Authorized" SortExpression="Authorized" />
                            <Rock:RockBoundField DataField="MerchantHtmlBadge" HeaderText="Merchant Listings" HtmlEncode="false" />
                            <Rock:EditField OnClick="gAccounts_Edit" />
                            <Rock:DeleteField OnClick="gAccounts_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>

        <Rock:ModalDialog runat="server" ID="mdEditAccountSettings" SaveButtonText="Save" Title="Pushpay Account Settings" OnSaveClick="mdEditAccountSettings_SaveClick" ValidationGroup="AccountEdit">
            <Content>
                <asp:HiddenField runat="server" ID="hfAccountId" />
                <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="AccountEdit" />

                <ul class="nav nav-pills margin-b-md">
                    <li id="liSettings" runat="server"><a href='#<%=divSettings.ClientID%>' data-toggle="pill">Settings</a></li>
                    <li id="liAdvancedSettings" runat="server"><a href='#<%=divAdvancedSettings.ClientID%>' data-toggle="pill">Advanced Settings</a></li>
                </ul>

                <div class="tab-content">

                    <div id="divSettings" runat="server" class="tab-pane">
                        <Rock:RockTextBox ID="tbName" runat="server" Label="Account Name" Required="true" ValidationGroup="AccountEdit" />
                        <Rock:RockTextBox ID="tbClientId" runat="server" Label="API Client Id" Required="true" ValidationGroup="AccountEdit"
                            Help="The Client ID provided by Pushpay to access the Pushpay API" />
                        <Rock:RockTextBox ID="tbClientSecret" runat="server" Label="API Client Secret" TextMode="Password" Required="true" ValidationGroup="AccountEdit"
                            Help="The Client Secret provided by Pushpay to access the Pushpay API" />
                    </div>

                    <div id="divAdvancedSettings" runat="server" class="tab-pane">
                        <Rock:RockCheckBox ID="cbActive" runat="server" Label="Active" Text="Yes" />
                        <Rock:RockTextBox ID="tbAuthorizationUrl" runat="server" Label="Authorization Server URL" Required="true" ValidationGroup="AccountEdit"
                            Help="The URL of the Pushpay Authorization Server (default: https://auth.pushpay.com/pushpay/)" />
                        <Rock:RockTextBox ID="tbApiUrl" runat="server" Label="API Server URL" Required="true" ValidationGroup="AccountEdit"
                            Help="The URL of the Pushpay API (default: https://api.pushpay.com)"  />
                        <Rock:RockTextBox ID="tbAuthorizationRedirectUrl" runat="server" Label="Redirect URL" Required="true" ValidationGroup="AccountEdit"
                            Help="The URL on your server that Pushpay should redirect user back to after granting access on the Pushpay site (i.e. https://rock.rocksolidchurchdemo.com/pushpayredirect)."  />
                    </div>

                </div>

            </Content>
        </Rock:ModalDialog>

        <script type="text/javascript">
            Sys.Application.add_load(function () {
                $('[data-toggle="tooltip"]').tooltip()
            })
        </script>

    </ContentTemplate>
</asp:UpdatePanel>
