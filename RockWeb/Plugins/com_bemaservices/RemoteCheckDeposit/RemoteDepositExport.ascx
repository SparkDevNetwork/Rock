<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RemoteDepositExport.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.RemoteCheckDeposit.RemoteDepositExport" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbWarningMessage" runat="server" Dismissable="true" NotificationBoxType="Danger" Visible="true" />

        
            
        <asp:Panel ID="pnlBatches" runat="server" >
            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-download"></i>&nbsp;Export Batches</h1>
                    <div class="pull-right">
                        <Rock:BootstrapButton ID="lbExportSelected" runat="server" CssClass="btn btn-sm btn-primary pull-right"  OnClick="lbSelectBatches_Click">
                            <i class="fa fa-download"></i> Export Selected
                        </Rock:BootstrapButton>
                    </div>
                </div>

                <div class="panel-body">
                    <div class="grid grid-panel">
                        <Rock:ModalAlert ID="maWarningDialog" runat="server" />
                        <Rock:GridFilter ID="gfBatches" runat="server" OnApplyFilterClick="gfBatches_ApplyFilterClick" OnClearFilterClick="gfBatches_ClearFilterClick" OnDisplayFilterValue="gfBatches_DisplayFilterValue">
                            <Rock:RockDropDownList ID="ddlStatus" runat="server" Label="Status" />
                            <Rock:DateRangePicker ID="drpBatchDate" runat="server" Label="Date Range" />
                            <Rock:CampusPicker ID="campCampus" runat="server" />
                            <Rock:RockTextBox ID="tbTitle" runat="server" Label="Title"></Rock:RockTextBox>
                            <Rock:RockDropDownList ID="ddlDeposited" runat="server" Label="Deposited" />
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
                </div>
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-download"></i>&nbsp;Export Batches</h1>
                    <div class="pull-right">
                        <Rock:BootstrapButton ID="lbExportBottom" runat="server" CssClass="btn btn-sm btn-primary pull-right"  OnClick="lbSelectBatches_Click">
                            <i class="fa fa-download"></i> Export Selected
                        </Rock:BootstrapButton>
                    </div>
                </div>
                </div>
            </asp:Panel>

            <asp:Panel ID="pnlFixMicr" runat="server" Visible="false">
                <div class="panel panel-block">
                    <div class="panel-heading">
                        <h1 class="panel-title"><i class="fa fa-download"></i>&nbsp;Check Details</h1>
                        
                    </div>

                    <div class="panel-body">
                    <asp:Repeater ID="rptMicrDetail" runat="server">
                        <ItemTemplate>
                            <div class="row">
                                <Rock:NotificationBox ID="nbMicrWarning" runat="server" Dismissable="true" NotificationBoxType="Warning" Text='<%# Eval("IsValidMessage") %>' Visible='<%# !(bool)Eval("IsValid") %>' />
                                    
                                <div class="col-md-4">
                                    <label>Image</label>
                                    <asp:Image ID="imgFront" runat="server" CssClass="transaction-image" ImageUrl='<%#Eval("ImageURL") %>' />
                                </div>
                                <div class="col-md-4">
                                    <asp:HiddenField ID="hfFixMicrId" runat="server" Value='<%#Eval("TransactionId") %>' />
                                    <Rock:RockTextBox ID="tbRoutingNumber" runat="server" Label="Routing Number" Required="true" ValidationGroup="FixMicr" Text='<%#Eval("RoutingNumber") %>' />
                                    <Rock:RockTextBox ID="tbAccountNumber" runat="server" Label="Account Number" Required="true" ValidationGroup="FixMicr" Text='<%#Eval("AccountNumber") %>' />
                                </div>
                                <div class="col-md-4">
                                    <Rock:RockTextBox ID="tbCheckNumber" runat="server" Label="Check Number" Required="true" ValidationGroup="FixMicr" Text='<%#Eval("CheckNumber") %>' />
                                    <Rock:CurrencyBox ID="tbAmount" runat="server" Label="Amount" Enabled="false" ValidationGroup="FixMicr" Text='<%# this.FormatValueAsCurrency((decimal)Eval("Amount")) %>' />
                                </div>
                            </div>
                            <br /><hr />
                        </ItemTemplate>
                    </asp:Repeater>
                    <div class="actions margin-t-md">
                                <asp:LinkButton ID="lbFixMicr" runat="server" Text="Save and Continue" CssClass="btn btn-primary" ValidationGroup="FixMicr" OnClick="lbFixMicr_Click" />
                                <asp:LinkButton ID="lbCancelMicr" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="lbCancel_Click" />
                            </div>
                </div>
                    </div>
            </asp:Panel>

            <asp:Panel ID="pnlOptions" runat="server" Visible="false">
                <div class="panel panel-block">
                    <div class="panel-heading">
                        <h1 class="panel-title"><i class="fa fa-download"></i>&nbsp;Create Export</h1>
                        
                    </div>

                    <div class="panel-body">

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
                    </div>
                </div>
            </asp:Panel>

            <asp:Panel ID="pnlSuccess" runat="server" Visible="false">
                <div class="panel panel-block">
                    <div class="panel-heading">
                        <h1 class="panel-title"><i class="fa fa-download"></i>&nbsp;Download</h1>
                        
                    </div>

                    <div class="panel-body">
                        <div class="alert alert-success">
                        <p>Data has been successfully exported.</p>
                            <p>
                                <asp:HyperLink ID="hlDownload" runat="server" Text="Download" CssClass="btn btn-success" />
                            </p>
                        </div>

                        <div class="actions margin-t-md">
                            <asp:LinkButton ID="lbFinished" runat="server" Text="Finished" CssClass="btn btn-default" OnClick="lbFinished_Click" CausesValidation="false" />
                        </div>
                    </div>

                  </div>  
            </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
