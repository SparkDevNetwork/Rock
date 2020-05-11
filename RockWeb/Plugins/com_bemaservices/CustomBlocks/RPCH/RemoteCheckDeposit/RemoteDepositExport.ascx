<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RemoteDepositExport.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.RemoteCheckDeposit.RemoteDepositExport" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Danger" Visible="true" />

        <asp:Panel ID="pnlDetails" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-download"></i> Export Batches
                </h1>
            </div>

            <div class="panel-body">
                <asp:Panel ID="pnlBatches" runat="server">
                    <div class="grid grid-panel">
                        <Rock:GridFilter ID="gfBatches" runat="server" OnApplyFilterClick="gfBatches_ApplyFilterClick" OnClearFilterClick="gfBatches_ClearFilterClick" OnDisplayFilterValue="gfBatches_DisplayFilterValue">
                            <Rock:RockDropDownList ID="ddlStatus" runat="server" Label="Status" />
                            <Rock:DateRangePicker ID="drpBatchDate" runat="server" Label="Date Range" />
                            <Rock:CampusPicker ID="campCampus" runat="server" />
                            <Rock:RockTextBox ID="tbTitle" runat="server" Label="Title"></Rock:RockTextBox>
                        </Rock:GridFilter>

                        <Rock:Grid ID="gBatches" runat="server" OnGridRebind="gBatches_GridRebind" OnRowCreated="gBatches_RowCreated">
                            <Columns>
                                <Rock:SelectField />
                                <Rock:RockBoundField DataField="Id" HeaderText="Id" SortExpression="Id" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" />
                                <Rock:DateField DataField="BatchStartDateTime" HeaderText="Date" SortExpression="BatchStartDateTime" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" />
                                <Rock:RockBoundField DataField="Name" HeaderText="Title" SortExpression="Name" />
                                <Rock:RockBoundField DataField="TransactionCount" HeaderText="Transaction Count" SortExpression="TransactionCount" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" />
                                <Rock:CurrencyField DataField="TransactionAmount" HeaderText="Transaction Total" SortExpression="TransactionAmount" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" />
                                <Rock:RockTemplateField HeaderText="Variance" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right">
                                    <ItemTemplate>
                                        <span class='<%# (decimal)Eval("Variance") != 0 ? "label label-danger" : "" %>'><%# this.FormatValueAsCurrency((decimal)Eval("Variance")) %></span>
                                    </ItemTemplate>
                                </Rock:RockTemplateField>
                                <Rock:RockBoundField DataField="CampusName" HeaderText="Campus" SortExpression="CampusName" />
                                <Rock:RockTemplateField HeaderText="Status" SortExpression="Status" HeaderStyle-CssClass="grid-columnstatus" ItemStyle-CssClass="grid-columnstatus" FooterStyle-CssClass="grid-columnstatus" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center">
                                    <ItemTemplate>
                                        <span class='<%# Eval("StatusLabelClass") %>'><%# Eval("StatusText") %></span>
                                    </ItemTemplate>
                                </Rock:RockTemplateField>
                                <Rock:RockBoundField DataField="Notes" HeaderText="Note" HtmlEncode="false" />
                                <Rock:BoolField DataField="Deposited" HeaderText="Deposited" SortExpression="Deposited" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlFixMicr" runat="server" Visible="false">
                    <div class="alert alert-warning">
                        <p>This check did not properly scan the MICR line and must be fixed before it can be exported.</p>
                        <p>Original scan data: <asp:Literal ID="ltBadMicr" runat="server" /></p>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <asp:HiddenField ID="hfFixMicrId" runat="server" />
                            <Rock:RockTextBox ID="tbRoutingNumber" runat="server" Label="Routing Number" Required="true" ValidationGroup="FixMicr" />
                            <Rock:RockTextBox ID="tbAccountNumber" runat="server" Label="Account Number" Required="true" ValidationGroup="FixMicr" />
                            <Rock:RockTextBox ID="tbCheckNumber" runat="server" Label="Check Number" Required="true" ValidationGroup="FixMicr" />

                            <div class="actions margin-t-md">
                                <asp:LinkButton ID="lbFixMicr" runat="server" Text="Save" CssClass="btn btn-primary" ValidationGroup="FixMicr" OnClick="lbFixMicr_Click" />
                                <asp:LinkButton ID="lbCancelMicr" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="lbCancel_Click" />
                            </div>
                        </div>

                        <div class="col-md-6">
                            <label>Image</label>
                            <asp:Image ID="imgFront" runat="server" CssClass="transaction-image" />
                        </div>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlOptions" runat="server" Visible="false">
                    <asp:HiddenField ID="hfBatchIds" runat="server" />
                    <Rock:RockDropDownList ID="ddlFileFormat" runat="server" Label="File Format" Required="true" ValidationGroup="Options" />

                    <Rock:DateTimePicker ID="dpBusinessDate" runat="server" Label="Business Date" Required="true" ValidationGroup="Options" />

                    <dl>
                        <dt>
                            Total Amount
                        </dt>
                        <dd><asp:Literal ID="lTotalDeposit" runat="server" /></dd>
                    </dl>

                    <div class="actions margin-t-md">
                        <asp:LinkButton ID="lbExport" runat="server" Text="Export" CssClass="btn btn-primary" OnClick="lbExport_Click" ValidationGroup="Options" />
                        <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-link" OnClick="lbCancel_Click" CausesValidation="false" />
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlSuccess" runat="server" Visible="false">
                    <div class="alert alert-success">
                    <p>Data has been successfully exported.</p>
                        <p>
                            <asp:HyperLink ID="hlDownload" runat="server" Text="Download" CssClass="btn btn-success" />
                        </p>
                    </div>

                    <div class="actions margin-t-md">
                        <asp:LinkButton ID="lbFinished" runat="server" Text="Finished" CssClass="btn btn-default" OnClick="lbFinished_Click" CausesValidation="false" />
                    </div>
                </asp:Panel>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
