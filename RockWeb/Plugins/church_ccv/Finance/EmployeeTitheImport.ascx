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
                    <Rock:Grid ID="gMapAccounts" runat="server" DisplayType="Light" OnRowCreated="gMapAccounts_RowCreated">
                        <Columns>
                            <Rock:RockBoundField HeaderText="Column" DataField="CampusCode" />
                            <Rock:RockTemplateField HeaderText="Account">
                                <ItemTemplate>
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
