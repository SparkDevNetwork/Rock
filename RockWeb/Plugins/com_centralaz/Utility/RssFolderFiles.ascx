<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RssFolderFiles.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Utility.RssFolderFiles" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-rss"></i> RSS Block</h1>
            </div>
            <div class="panel-body">

                <div class="alert alert-info">
                    <p>
                        Because you're logged in as an admin, the RSS feed is not being sent so you can 
                        configure the block settings.  However, below is what the RSS content would
                        look like.
                    </p>
                    <asp:LinkButton ID="lbClearCache" runat="server" OnClick="lbClearCache_Click" CssClass="btn btn-sm btn-info" Text="<i class='fa fa-recycle'></i> Clear Cache"></asp:LinkButton>
                    <hr />
                    <pre>
                        <asp:Literal ID="lDebug" runat="server"></asp:Literal>
                    </pre>
                </div>

            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
