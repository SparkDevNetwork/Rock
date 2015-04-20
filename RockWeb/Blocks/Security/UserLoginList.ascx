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
                        <Rock:RockDropDownList ID="ddlIsConfirmedFilter" runat="server" Label="Is Confirmed" >
                            <asp:ListItem Value="" Text="" />
                            <asp:ListItem Value="true" Text="Yes" />
                            <asp:ListItem Value="false" Text="No" />
                        </Rock:RockDropDownList>
                        <Rock:RockDropDownList ID="ddlLockedOutFilter" runat="server" Label="Is Locked Out" >
                            <asp:ListItem Value="" Text="" />
                            <asp:ListItem Value="true" Text="Yes" />
                            <asp:ListItem Value="false" Text="No" />
                        </Rock:RockDropDownList>
                    </Rock:GridFilter>

                    <Rock:Grid ID="gUserLogins" runat="server" AllowSorting="true" RowItemText="Login">
                        <Columns>
                            <Rock:RockBoundField DataField="UserName" HeaderText="Username" SortExpression="Name" />
                            <asp:HyperLinkField DataNavigateUrlFields="PersonId" DataTextField="PersonName" DataNavigateUrlFormatString="~/Person/{0}" HeaderText="Person" />
                            <Rock:RockBoundField DataField="ProviderName" HeaderText="Provider" SortExpression="EntityType.FriendlyName" />
                            <Rock:DateField DataField="CreatedDateTime" HeaderText="Created" SortExpression="CreatedDateTime" />
                            <Rock:DateField DataField="LastLoginDateTime" HeaderText="Last Login" SortExpression="LastLoginDateTime" />
                            <Rock:BoolField DataField="IsConfirmed" HeaderText="Confirmed" SortExpression="IsConfirmed" />
                            <Rock:BoolField DataField="IsLockedOut" HeaderText="Locked Out" SortExpression="IsLockedOut" />
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

                <Rock:NotificationBox ID="nbErrorMessage" runat="server" NotificationBoxType="Danger" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbUserName" runat="server" SourceTypeName="Rock.Model.UserLogin, Rock" PropertyName="UserName" />
                        <Rock:RockCheckBox ID="cbIsConfirmed" runat="server" Label="Confirmed" Text="Yes" Help="Has the user confirmed this login?" />
                        <Rock:RockCheckBox ID="cbIsLockedOut" runat="server" Label="Locked Out" Text="Yes" Help="Has the user been locked out of using this login?" />
                    </div>
                    <div class="col-md-6">
                        <Rock:ComponentPicker ID="compProvider" runat="server" Label="Authentication Provider" ContainerType="Rock.Security.AuthenticationContainer, Rock" Required="true" AutoPostBack="true" />
                        <Rock:RockTextBox ID="tbPassword" runat="server" Label="Password" TextMode="Password" Enabled="false"></Rock:RockTextBox>
                        <Rock:RockTextBox ID="tbPasswordConfirm" runat="server" Label="Confirm" TextMode="Password" Enabled="false"></Rock:RockTextBox>
                    </div>
                </div>

            </Content>
        </Rock:ModalDialog>

        <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Danger" Visible="false" />

    </ContentTemplate>
</asp:UpdatePanel>
