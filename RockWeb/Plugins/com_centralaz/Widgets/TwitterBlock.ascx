<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TwitterBlock.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Widgets.TwitterBlock" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <script>
            Sys.Application.add_load(function () {
                var config1 = {
                    "id": '626813522828132352',
                    "domId": 'tw-widget1',
                    "maxTweets": 1,
                    "enableLinks": true
                };
                twitterFetcher.fetch(config1);
            });
        </script>
        <div id="tw-widget1">
        </div>
    </ContentTemplate>
</asp:UpdatePanel>

