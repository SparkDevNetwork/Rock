<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CacheViewer.ascx.cs" Inherits="RockWeb.Blocks.Cms.CacheManager" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-tachometer"></i>
                    Cache Viewer
                </h1>
            </div>
            <div class="panel-body">
                <div class="row">
                    <div class="col-sm-6">
                        <Rock:RockDropDownList ID="ddlCacheTypes" runat="server" DataTextField="Name" DataValueField="Id" Label="Cache Type" />
                    </div>
                    <div class="col-sm-6 col-md-3 col-lg-2">
                        <Rock:RockTextBox ID="rtbEntityId" runat="server" Label="Entity Id" />
                    </div>
                </div>
                <div class="actions">
                        <Rock:BootstrapButton ID="btnRefresh" runat="server" CssClass="btn btn-primary" ToolTip="Refresh" OnClick="btnRefresh_Click">
                            <i class="fa fa-refresh"></i>
                            Load
                        </Rock:BootstrapButton>
                    </div>
                <hr />
                <Rock:NotificationBox ID="nbNotification" runat="server" Visible="false" />
                <pre runat="server" id="preResult"></pre>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
