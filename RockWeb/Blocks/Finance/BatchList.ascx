<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BatchList.ascx.cs" Inherits="RockWeb.Blocks.Finance.BatchList" %>

<asp:UpdatePanel ID="upnlFinancialBatch" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Danger" Visible="true" />

        <asp:Panel ID="pnlBatchList" runat="server">

            <Rock:GridFilter ID="gfBatchFilter" runat="server">
                <Rock:DatePicker ID="dpBatchDate" runat="server" Label="Date" />
                <Rock:RockTextBox ID="tbTitle" runat="server" Label="Title"></Rock:RockTextBox>
                <Rock:RockDropDownList ID="ddlStatus" runat="server" Label="Status" />
                <Rock:CampusPicker ID="campCampus" runat="server" />
            </Rock:GridFilter>

            <Rock:Grid ID="gBatchList" runat="server" OnRowDataBound="gBatchList_RowDataBound" ShowConfirmDeleteDialog="true" OnRowSelected="gBatchList_Edit">
                <Columns>
                    <asp:BoundField DataField="Id" HeaderText="Id" SortExpression="Id" />
                    <asp:TemplateField HeaderText="Date">
                        <ItemTemplate>
                            <span><%# Eval("BatchStartDateTime") %></span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="Name" HeaderText="Title" SortExpression="Name" />
                    
                    <asp:BoundField DataField="ControlAmount" HeaderText="Control Amount" />
                    <asp:TemplateField HeaderText="Transaction Total">
                        <ItemTemplate>
                            <asp:Literal ID="TransactionTotal" runat="server" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Variance">
                        <ItemTemplate>
                            <asp:Literal ID="Variance" runat="server" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Transaction Count">
                        <ItemTemplate>
                            <asp:Literal ID="TransactionCount" runat="server" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="Campus" HeaderText="Campus" />
                    <asp:BoundField DataField="Status" HeaderText="Status" SortExpression="Status" />
                    <Rock:DeleteField OnClick="gBatchList_Delete" />
                </Columns>
            </Rock:Grid>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>





