<%@ control language="C#" autoeventwireup="true" inherits="RockWeb.Blocks.Finance.BusinessList, RockWeb" %>

<asp:UpdatePanel ID="upnlBusinesses" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Danger" Visible="true" />

        <asp:Panel ID="pnlBusinessList" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-briefcase"></i> Business List</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">

                    <Rock:GridFilter ID="gfBusinessFilter" runat="server">
                        <Rock:RockTextBox ID="tbBusinessName" runat="server" Label="Business Name"></Rock:RockTextBox>  <!-- this should search by "contains" not necessarily an exact match -->
                        <Rock:RockDropDownList ID="ddlActiveFilter" runat="server" Label="Active Status">
                            <asp:ListItem Text="[All]" Value="all"></asp:ListItem>
                            <asp:ListItem Text="Active" Value="active"></asp:ListItem>
                            <asp:ListItem Text="Inactive" Value="inactive"></asp:ListItem>
                        </Rock:RockDropDownList>
                    </Rock:GridFilter>

                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                    <Rock:Grid ID="gBusinessList" runat="server" RowItemText="Business" EmptyDataText="No Businesses Found" AllowSorting="true" OnRowDataBound="gBusinessList_RowDataBound" OnRowSelected="gBusinessList_RowSelected">
                        <Columns>
                            <asp:BoundField DataField="FirstName" HeaderText="Business Name" SortExpression="FirstName" />
                            <asp:TemplateField>
                                <HeaderTemplate>Contact</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="lblPhoneNumber" runat="server" />
                                    <%# Eval("Email") %>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <HeaderTemplate>Address</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="lblAddress" runat="server" />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </Rock:Grid>

                </div>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
