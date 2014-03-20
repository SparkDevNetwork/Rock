<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BusinessList.ascx.cs" Inherits="RockWeb.Blocks.Finance.BusinessList" %>

<asp:UpdatePanel ID="upnlBusinesses" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Danger" Visible="true" />

        <asp:Panel ID="pnlBusinessList" runat="server">

            <Rock:GridFilter ID="gfBusinessFilter" runat="server">
                <Rock:RockTextBox ID="tbBusinessName" runat="server" Label="Business Name"></Rock:RockTextBox>  <!-- this should search by "contains" not necessarily an exact match -->
                <Rock:PersonPicker ID="ppBusinessOwner" runat="server" Label="Owner" />
            </Rock:GridFilter>

            <Rock:ModalAlert ID="mdGridWarning" runat="server" />
            <Rock:Grid ID="gBusinessList" runat="server" OnRowDataBound="gBusinessList_RowDataBound" ShowConfirmDeleteDialog="true" OnRowSelected="gBusinessList_RowSelected">
                <Columns>
                    <asp:BoundField DataField="Id" HeaderText="Id" SortExpression="Id" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                    <asp:BoundField DataField="BusinessName" HeaderText="Business Name" SortExpression="BusinessName" />
                    <asp:BoundField DataField="Contacts" HeaderText="Contacts" SortExpression="Contacts" />
                    <asp:BoundField DataField="Address" HeaderText="Address" SortExpression="Address" />
                    <Rock:EditField OnClick="gBusinessList_Edit" />
                    <Rock:DeleteField OnClick="gBusinessList_Delete" />
                </Columns>
            </Rock:Grid>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
