<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PdfMerger.ascx.cs" Inherits="RockWeb.Plugins.org_lakepointe.CMS.PdfMerger" %>
<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>
        <div class="panel panel-block" runat="server" id="pnlMain">
            <div class="panel-heading">
                <h1 class="panel-title">PDF Merger</h1>
            </div>
            <div class="panel-body">
                <div class="row">
                    <div class="col-xs-12">
                        <asp:Panel CssClass="alert alert-warning" runat="server" ID="pnlError" Visible="false">
                            <h4 class="alert-heading">Error</h4>
                            <asp:Literal runat="server" ID="lError" />
                        </asp:Panel>
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-8">
                        <Rock:RockDropDownList runat="server" ID="ddlQuery" Label="Query" />
                    </div>
                    <div class="col-md-4">
                        <Rock:Toggle runat="server" ID="tglDoubleSidedMode" Label="Double-Sided Mode"
                            Help="When this is on, extra blank pages are added when needed to allow for proper double-sided printing." />
                    </div>
                </div>
                <div class="row">
                    <div class="col-xs-12">
                        <asp:Button runat="server" ID="btnDownload" Text="Download" OnClick="btnDownload_Click" CssClass="btn btn-primary" />
                    </div>
                </div>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
