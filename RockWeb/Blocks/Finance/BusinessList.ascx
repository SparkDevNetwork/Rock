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
            <Rock:Grid ID="gBusinessList" runat="server" EmptyDataText="No Businesses Found" AllowSorting="true" OnRowDataBound="gBusinessList_RowDataBound" ShowConfirmDeleteDialog="true" OnRowSelected="gBusinessList_RowSelected">
                <Columns>
                    <asp:BoundField DataField="FirstName" HeaderText="Business Name" SortExpression="FirstName" />
                    <asp:TemplateField>
                        <HeaderTemplate>Contact</HeaderTemplate>
                        <ItemTemplate>
                            <%# Eval("PhoneNumbers[0].NumberFormatted") %><br />
                            <%# Eval("Email") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <HeaderTemplate>Address</HeaderTemplate>
                        <ItemTemplate>
                            <%# Eval("GivingGroup.GroupLocations[0].Location.Street1") %><br />
                            <asp:PlaceHolder ID="phStreet2" runat="server" />
                            <%# Eval("GivingGroup.GroupLocations[0].Location.City") %>, <%# Eval("GivingGroup.GroupLocations[0].Location.State") %> <%# Eval("GivingGroup.GroupLocations[0].Location.Zip") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <Rock:EditField OnClick="gBusinessList_Edit" />
                    <Rock:DeleteField OnClick="gBusinessList_Delete" />
                </Columns>
            </Rock:Grid>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
