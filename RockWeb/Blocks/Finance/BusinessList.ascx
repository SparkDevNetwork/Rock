<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BusinessList.ascx.cs" Inherits="RockWeb.Blocks.Finance.BusinessList" %>

<asp:UpdatePanel ID="upnlBusinesses" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Danger" Visible="true" />

        <asp:Panel ID="pnlBusinessList" runat="server">

            <div class="grid">
                <Rock:GridFilter ID="gfBusinessFilter" runat="server">
                    <Rock:RockTextBox ID="tbBusinessName" runat="server" Label="Business Name"></Rock:RockTextBox>  <!-- this should search by "contains" not necessarily an exact match -->
                </Rock:GridFilter>

                <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                <Rock:Grid ID="gBusinessList" runat="server" EmptyDataText="No Businesses Found" AllowSorting="true" OnRowDataBound="gBusinessList_RowDataBound" ShowConfirmDeleteDialog="false" OnRowSelected="gBusinessList_RowSelected">
                    <Columns>
                        <asp:BoundField DataField="FirstName" HeaderText="Business Name" SortExpression="FirstName" />
                        <asp:TemplateField>
                            <HeaderTemplate>Contact</HeaderTemplate>
                            <ItemTemplate>
                                <asp:Label ID="lblPhoneNumber" runat="server" />
                                <asp:Label ID="lblEmail" runat="server" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField>
                            <HeaderTemplate>Address</HeaderTemplate>
                            <ItemTemplate>
                                <asp:Label ID="lblStreet1" runat="server" />
                                <asp:Label ID="lblStreet2" runat="server" />
                                <asp:Label ID="lblCityStateZip" runat="server" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <Rock:EditField OnClick="gBusinessList_Edit" />
                        <Rock:DeleteField OnClick="gBusinessList_Delete" />
                    </Columns>
                </Rock:Grid>
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
