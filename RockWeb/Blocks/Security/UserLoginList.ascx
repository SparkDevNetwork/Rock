<%@ Control Language="C#" AutoEventWireup="true" CodeFile="UserLoginList.ascx.cs" Inherits="RockWeb.Blocks.Security.UserLoginList" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlList" CssClass="panel panel-block" runat="server">

            <Rock:ModalAlert ID="maGridWarning" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-users"></i> User Account List</h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:GridFilter ID="gfSettings" runat="server">
                        <Rock:RockTextBox ID="tbUserNameFilter" runat="server" Label="Username"></Rock:RockTextBox>
                        <Rock:ComponentPicker ID="compProviderFilter" runat="server" Label="Authentication Provider" ContainerType="Rock.Security.AuthenticationContainer, Rock" />
                        <Rock:DateRangePicker ID="drpCreated" runat="server" Label="Created" />
                        <Rock:DateRangePicker ID="drpLastLogin" runat="server" Label="Last Login" />
                        <Rock:RockDropDownList ID="ddlIsConfirmedFilter" runat="server" Label="Is Confirmed">
                            <asp:ListItem Value="" Text="" />
                            <asp:ListItem Value="true" Text="Yes" />
                            <asp:ListItem Value="false" Text="No" />
                        </Rock:RockDropDownList>
                        <Rock:RockDropDownList ID="ddlLockedOutFilter" runat="server" Label="Is Locked Out">
                            <asp:ListItem Value="" Text="" />
                            <asp:ListItem Value="true" Text="Yes" />
                            <asp:ListItem Value="false" Text="No" />
                        </Rock:RockDropDownList>
                    </Rock:GridFilter>

                    <Rock:Grid ID="gUserLogins" runat="server" AllowSorting="true" RowItemText="Login" OnRowDataBound="gUserLogins_RowDataBound">
                        <Columns>
                            <Rock:RockLiteralField ID="lUserNameOrRemoteProvider" HeaderText="Username" SortExpression="UserName" />
                            <Rock:PersonField DataField="Person" HeaderText="Person" SortExpression="Person.LastName, Person.NickName" />
                            <Rock:RockLiteralField ID="lProviderName" HeaderText="Provider" SortExpression="EntityType.FriendlyName" />
                            <Rock:DateField DataField="CreatedDateTime" HeaderText="Created" SortExpression="CreatedDateTime" />
                            <Rock:DateField DataField="LastLoginDateTime" HeaderText="Last Login" SortExpression="LastLoginDateTime" />
                            <Rock:BoolField DataField="IsConfirmed" HeaderText="Confirmed" SortExpression="IsConfirmed" />
                            <Rock:BoolField DataField="IsLockedOut" HeaderText="Locked Out" SortExpression="IsLockedOut" />
                            <Rock:BoolField DataField="IsPasswordChangeRequired" HeaderText="Password Change Required" SortExpression="IsPasswordChangeRequired" />
                            <Rock:DeleteField OnClick="gUserLogins_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>

        </asp:Panel>

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="dlgDetails" runat="server" Title="Login" OnSaveClick="dlgDetails_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="Login">
            <Content>

                <asp:HiddenField ID="hfIdValue" runat="server" />
                <asp:ValidationSummary ID="valUserLoginSummary" runat="server" ValidationGroup="Login"  CssClass="alert alert-validation" HeaderText="Please correct the following:"/>
                <Rock:NotificationBox ID="nbErrorMessage" runat="server" NotificationBoxType="Danger" />
                <div class="row">
                    <div class="col-md-6">
                        <Rock:PersonPicker ID="ppPerson" runat="server" Label="Person" Required="true" />
                    </div>
                    <div class="col-md-6">
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbUserNameEdit" runat="server" SourceTypeName="Rock.Model.UserLogin, Rock" PropertyName="UserName" ValidationGroup="Login" />
                        <Rock:RockCheckBox ID="cbIsConfirmed" runat="server" Label="Confirmed" Help="Has the user confirmed this login?" />
                        <Rock:RockCheckBox ID="cbIsLockedOut" runat="server" Label="Locked Out" Help="Has the user been locked out of using this login?" />
                        <Rock:RockCheckBox ID="cbIsRequirePasswordChange" runat="server" Label="Require Password Change" Help="Require the user to change the password on next log in." Visible="false" />
                    </div>
                    <div class="col-md-6">
                        <Rock:ComponentPicker ID="compProvider" runat="server" Label="Authentication Provider" ContainerType="Rock.Security.AuthenticationContainer, Rock" Required="true" AutoPostBack="true" ValidationGroup="Login" />
                        <Rock:RockControlWrapper ID="rcwPassword" runat="server" Label="Set Password">
                            <Rock:RockTextBox ID="tbPassword" runat="server" Label="Password" ValidateRequestMode="Disabled" TextMode="Password" Enabled="false" />
                            <Rock:RockTextBox ID="tbPasswordConfirm" runat="server" Label="Confirm" ValidateRequestMode="Disabled" TextMode="Password" Enabled="false" />
                        </Rock:RockControlWrapper>
                    </div>
                </div>

            </Content>
        </Rock:ModalDialog>

        <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Danger" Visible="false" />

    </ContentTemplate>
</asp:UpdatePanel>
