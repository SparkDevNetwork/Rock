<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BatchCopier.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Finance.BatchCopier" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:LinkButton ID="lbDisplay" runat="server" CssClass="btn btn-default hidden" Text="Copy Batch" OnClick="lbDisplay_Click"><i class="fa fa-files-o"></i> Copy Batch</asp:LinkButton>
        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block" Visible="false">
            <div class="panel-body">
                <asp:ValidationSummary ID="valSummaryBatch" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                <asp:CustomValidator ID="cvBatch" runat="server" />
                <h3>
                    <div class="col-md-6">
                        To copy this batch, type in a new date and click 'Create Copy'
                    </div>
                    <div class="col-md-3">
                        <Rock:DateTimePicker ID="dtpBatchDate" runat="server" Label="New Batch Date" />
                    </div>
                    <div class="col-md-3">
                        <asp:LinkButton ID="lbCopyBatch" runat="server" Text="Create Copy" OnClick="lbCopyBatch_Click" CssClass="btn btn-primary btn-lg" />
                    </div>
                </h3>
            </div>
        </asp:Panel>
        <script>
            Sys.Application.add_load(function () {
                $('#<%=lbDisplay.ClientID%>').insertBefore($("[id $='_lbMatch']")).after(" ").removeClass('hidden');
            });
        </script>
    </ContentTemplate>

</asp:UpdatePanel>
