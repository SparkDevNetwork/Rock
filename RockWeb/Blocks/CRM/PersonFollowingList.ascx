<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonFollowingList.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonFollowingList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="maGridWarning" runat="server" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-flag"></i> Your Following List</h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:Grid ID="gFollowings" runat="server" AllowSorting="true" RowItemText="Following">
                        <Columns>
                            <Rock:RockBoundField DataField="LastName" HeaderText="Last Name" SortExpression="LastName" />
                            <Rock:RockBoundField DataField="NickName" HeaderText="First Name" SortExpression="NickName" />
                            <Rock:DateField DataField="BirthDate" HeaderText="Birthdate" SortExpression="BirthDate" ColumnPriority="Desktop" />
                            <Rock:RockBoundField DataField="Email" HeaderText="Email" SortExpression="Email" ColumnPriority="DesktopLarge" />
                            <Rock:RockBoundField DataField="HomePhone" HeaderText="Home Phone" SortExpression="HomePhone" ColumnPriority="DesktopLarge" />
                            <Rock:RockBoundField DataField="CellPhone" HeaderText="Cell Phone" SortExpression="CellPhone" ColumnPriority="DesktopLarge" />
                            <Rock:RockBoundField DataField="SpouseName" HeaderText="Spouse's Name" SortExpression="SpouseName" ColumnPriority="Desktop" />
                            <Rock:DeleteField OnClick="gFollowings_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
