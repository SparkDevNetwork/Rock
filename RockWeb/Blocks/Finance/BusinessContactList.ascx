<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BusinessContactList.ascx.cs" Inherits="RockWeb.Blocks.Finance.BusinessContactList" %>
<asp:UpdatePanel ID="upnlBusinesses" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlContactList" runat="server" CssClass="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-users"></i> Business Contacts</h1>
                </div>
                <asp:HiddenField ID="hfBusinessId" runat="server" />
                <div class="panel-body">
                    <div class="grid grid-panel">
                        <Rock:Grid ID="gContactList" runat="server" RowItemText="Contact" EmptyDataText="No Contacts Found" AllowSorting="true" OnRowSelected="gContactList_RowSelected" ShowConfirmDeleteDialog="false">
                            <Columns>
                                <Rock:RockBoundField DataField="FullName" HeaderText="Contact Name" SortExpression="FullName" />
                                <Rock:DeleteField OnClick="gContactList_Delete" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>
            </asp:Panel>

            <Rock:ModalDialog ID="mdAddContact" runat="server" Title="Add Contact" ValidationGroup="AddContact">
                <Content>
                    <asp:HiddenField ID="hfModalOpen" runat="server" />
                    <asp:ValidationSummary ID="valSummaryAddContact" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="AddContact" />
                    <Rock:PersonPicker ID="ppContact" runat="server" Label="Contact" Required="true" ValidationGroup="AddContact" />
                </Content>
            </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>