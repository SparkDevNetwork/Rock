<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BatchList.ascx.cs" Inherits="RockWeb.Blocks.Finance.BatchList" %>

<asp:UpdatePanel ID="upnlFinancialBatch" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Danger" Visible="true" />

        <asp:Panel ID="pnlBatchList" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-archive"></i> Batch List</h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:GridFilter ID="gfBatchFilter" runat="server">
                        <Rock:DatePicker ID="dpBatchDate" runat="server" Label="Date" />
                        <Rock:RockTextBox ID="tbTitle" runat="server" Label="Title"></Rock:RockTextBox>
                        <Rock:RockDropDownList ID="ddlStatus" runat="server" Label="Status" />
                        <Rock:CampusPicker ID="campCampus" runat="server" />
                    </Rock:GridFilter>

                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                    <Rock:Grid ID="gBatchList" runat="server" OnRowDataBound="gBatchList_RowDataBound" ShowConfirmDeleteDialog="true" OnRowSelected="gBatchList_Edit" AllowSorting="true">
                        <Columns>
                            <asp:BoundField DataField="Id" HeaderText="Id" SortExpression="Id" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                            <asp:TemplateField HeaderText="Date">
                                <ItemTemplate>
                                    <span><%# Eval("BatchStartDateTime") %></span>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="Name" HeaderText="Title" SortExpression="Name" />
                            <asp:TemplateField HeaderText="Transaction Total">
                                <ItemTemplate>
                                    <asp:Literal ID="TransactionTotal" runat="server" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Var from Control">
                                <ItemTemplate>
                                    <asp:Label ID="lblVariance" runat="server"></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Transaction Count" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right">
                                <ItemTemplate>
                                    <asp:Literal ID="TransactionCount" runat="server" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="Campus" HeaderText="Campus" SortExpression="Campus"  />
                            <asp:BoundField DataField="Status" HeaderText="Status" SortExpression="Status" />
                            <asp:TemplateField HeaderText="Messages">
                                <ItemTemplate>
                                    <asp:Label ID="lblWarnings" runat="server"></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <Rock:DeleteField OnClick="gBatchList_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>

            

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>





