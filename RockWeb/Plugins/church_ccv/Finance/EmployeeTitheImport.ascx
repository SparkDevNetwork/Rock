<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EmployeeTitheImport.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Finance.EmployeeTitheImport" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-cloud-upload"></i>&nbsp;Employee Tithe Import</h1>
            </div>
            <div class="panel-body">
                <%-- Start Panel --%>
                <asp:Panel ID="pnlStart" runat="server">
                    <Rock:FileUploader ID="fuImport" runat="server" Label="Import File" OnFileUploaded="fuImport_FileUploaded" />
                </asp:Panel>

                <%-- Configure Import Panel --%>
                <asp:Panel ID="pnlConfigure" runat="server" Visible="false">
                    <h4>Account Mapping</h4>
                    <Rock:Grid ID="gMapAccounts" runat="server" DisplayType="Light" OnRowCreated="gMapAccounts_RowCreated" OnRowDataBound="gMapAccounts_RowDataBound">
                        <Columns>
                            <Rock:RockBoundField HeaderText="Column" DataField="CampusCode" />
                            <Rock:RockTemplateField HeaderText="Account">
                                <ItemTemplate>
                                    <asp:HiddenField ID="hfCampusCode" runat="server" />
                                    <Rock:RockDropDownList ID="ddlAccount" runat="server" />
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                        </Columns>
                    </Rock:Grid>


                    <h4>Currency Type</h4>
                    <Rock:RockDropDownList ID="ddlCurrencyType" runat="server" />

                    <br />
                    <div class="actions pull-right">
                        <asp:LinkButton ID="btnNext" runat="server" AccessKey="n" Text="Next" CssClass="btn btn-primary" OnClick="btnNext_Click" />
                    </div>

                </asp:Panel>

                <%-- Import Preview Panel --%>
                <asp:Panel ID="pnlImportPreview" runat="server" Visible="false">
                    <div class="pull-right">
                        <asp:Literal ID="lUnmatchedRecords" runat="server" Text="" /></div>
                    <h4>Preview</h4>
                    <Rock:Grid ID="gImportPreview" runat="server" AllowPaging="false">
                        <Columns>
                            <Rock:RockBoundField DataField="EmployeeId" HeaderText="Employee Id" />
                            <Rock:RockBoundField DataField="ImportPersonName" HeaderText="Import Name" />
                            <Rock:PersonField DataField="RockPerson" HeaderText="Rock Name" NullDisplayText="<span class='badge badge-danger'>NOT FOUND</span>" />
                            <Rock:DateField DataField="PayDate" HeaderText="Date" />
                        </Columns>
                    </Rock:Grid>

                    <div class="row">
                        <div class="col-md-4 col-md-offset-8 margin-t-md">
                            <asp:Panel ID="pnlSummary" runat="server" CssClass="panel panel-block">
                                <div class="panel-heading">
                                    <h1 class="panel-title">Account Totals</h1>
                                </div>
                                <div class="panel-body">
                                    <asp:Repeater ID="rptAccountSummary" runat="server">
                                        <ItemTemplate>
                                            <div class='row'>
                                                <div class='col-xs-8'><%#Eval("Name")%></div>
                                                <div class='col-xs-4 text-right'><%#Eval("TotalAmount")%></div>
                                            </div>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                    <div class='row'>
                                        <div class='col-xs-8'><b>Total: </div>
                                        <div class='col-xs-4 text-right'>
                                            <asp:Literal ID="lGrandTotal" runat="server" /></b>
                                        </div>
                                    </div>
                                </div>
                            </asp:Panel>
                        </div>
                    </div>

                    <div class="actions pull-right">
                        <asp:LinkButton ID="btnImport" runat="server" AccessKey="i" Text="Import" CssClass="btn btn-primary" OnClick="btnImport_Click" />
                    </div>
                </asp:Panel>


                <%-- Done Panel --%>
                <asp:Panel ID="pnlDone" runat="server" Visible="false">
                    <Rock:NotificationBox ID="nbSuccess" runat="server" Title="Success" Text="n records imported" />
                    <div class="actions pull-right">
                        <asp:LinkButton ID="btnViewBatch" runat="server" AccessKey="v" Text="View Batch" CssClass="btn btn-primary" OnClick="btnViewBatch_Click" />
                    </div>
                </asp:Panel>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
