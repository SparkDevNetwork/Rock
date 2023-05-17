<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SystemPhoneNumberList.ascx.cs" Inherits="RockWeb.Blocks.Communication.SystemPhoneNumberList" %>
<asp:UpdatePanel ID="upSettings" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Danger" Visible="false" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-sms"></i><asp:Literal ID="lTitle" Text="System Phone Numbers List" runat="server" /></h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="rFilter" runat="server">
                        <Rock:RockDropDownList ID="ddlActiveFilter" runat="server" Label="Active Status" />
                        <Rock:RockDropDownList ID="ddlSmsEnabledFilter" runat="server" Label="SMS Enabled" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="gSystemPhoneNumbers" runat="server" RowItemText="Phone Numbers">
                        <Columns>
                            <Rock:ReorderField />
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" />
                            <Rock:RockBoundField DataField="Number" HeaderText="Number" />
                            <Rock:BoolField DataField="IsActive" HeaderText="Active" />
                            <Rock:BoolField DataField="IsSmsEnabled" HeaderText="SMS Enabled" />
                            <Rock:RockBoundField DataField="AssignedToPersonAlias.Person.FullName" HeaderText="Response Recipient" />
                            <Rock:SecurityField TitleField="Name" />
                            <Rock:DeleteField OnClick="gSystemPhoneNumbers_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
