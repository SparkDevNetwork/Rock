<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FinancialStatementTemplateList.ascx.cs" Inherits="RockWeb.Blocks.Finance.FinancialStatementTemplateList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <asp:Panel ID="pnlView" runat="server" CssClass="">
            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title">
                        <i class="fa fa-file-invoice-dollar"></i>
                        Statement Templates</h1>
                </div>
                <div class="panel-body">

                    <div class="grid grid-panel">
                        <Rock:GridFilter ID="rFilter" runat="server">
                            <Rock:RockTextBox ID="txtAccountName" runat="server" Label="Name" />
                            <Rock:RockCheckBox ID="cbShowInactive" runat="server" Checked="false" Label="Include Inactive" />
                        </Rock:GridFilter>
                        <Rock:Grid ID="gList" runat="server" AllowSorting="true" OnRowSelected="gList_RowSelected">
                            <Columns>
                                <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                                <Rock:RockBoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                                <Rock:BoolField DataField="IsActive" HeaderText="Active" />
                                <Rock:DeleteField OnClick="gList_DeleteClick" />
                            </Columns>
                        </Rock:Grid>
                    </div>

                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
