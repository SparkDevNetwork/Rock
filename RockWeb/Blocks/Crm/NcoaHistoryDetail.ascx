<%@ Control Language="C#" AutoEventWireup="true" CodeFile="NcoaHistoryDetail.ascx.cs" Inherits="RockWeb.Blocks.Crm.NcoaHistoryDetail" %>

<asp:UpdatePanel ID="upNcoaHistory" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server"  CssClass="panel panel-block">
            <asp:HiddenField ID="hfBinaryFileId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-file-text-o"></i> NCOA History </h1>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">

                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger"  />
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <div class="row">
                    <div class="col-md-12">
                        <Rock:FileUploader ID="FileUploader1" runat="server" Label="Upload CSV" Required="true" BinaryFileTypeGuid="C1142570-8CD6-4A20-83B1-ACB47C1CD377"  />
                    </div>
                </div>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                </div>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
