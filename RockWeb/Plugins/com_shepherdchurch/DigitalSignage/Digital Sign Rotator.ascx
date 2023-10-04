<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Digital Sign Rotator.ascx.cs" Inherits="RockWeb.Plugins.com_shepherdchurch.DigitalSignage.DigitalSignRotator" %>

<script src="https://player.vimeo.com/api/player.js"></script>
<script src="https://www.youtube.com/iframe_api"></script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlContent" runat="server" Visible="false">
            <asp:HiddenField ID="hfDevice" runat="server" />
            <asp:HiddenField ID="hfSlideInterval" runat="server" />
            <asp:HiddenField ID="hfUpdateInterval" runat="server" />
            <asp:HiddenField ID="hfTransitions" runat="server" />
            <asp:HiddenField ID="hfContentChannel" runat="server" />
            <asp:HiddenField ID="hfAudio" runat="server" />

            <div class="dsrContainer"></div>
        </asp:Panel>

        <asp:Panel ID="pnlError" runat="server" Visible="false">
            <div class="dsrContainer">
                <Rock:NotificationBox ID="nbError" runat="server" CssClass="dsr-alert" NotificationBoxType="Danger"></Rock:NotificationBox>

                <asp:LinkButton ID="lbReload" runat="server" CssClass="hidden" OnClick="lbReload_Click"></asp:LinkButton>
            </div>
        </asp:Panel>

        <script>
            Sys.Application.add_load(function ()
            {
                if ($('#<%= lbReload.ClientID %>').length)
                {
                    setTimeout(function () { $('#<%= lbReload.ClientID %>').get(0).click(); }, 10000);
                }
                else
                {
                    var options = {};
                    var val;

                    if ($('#<%= hfDevice.ClientID %>').val())
                    {
                        options.device = $('#<%= hfDevice.ClientID %>').val();
                    }

                    if ($('#<%= hfSlideInterval.ClientID %>').val())
                    {
                        options.slideInterval = $('#<%= hfSlideInterval.ClientID %>').val();
                    }

                    if ($('#<%= hfUpdateInterval.ClientID %>').val())
                    {
                        options.updateInterval = $('#<%= hfUpdateInterval.ClientID %>').val();
                    }

                    if ($('#<%= hfTransitions.ClientID %>').val())
                    {
                        options.transitions = $('#<%= hfTransitions.ClientID %>').val().split(',');
                    }

                    if ($('#<%= hfContentChannel.ClientID %>').val())
                    {
                        options.contentChannel = $('#<%= hfContentChannel.ClientID %>').val();
                    }

                    if ($('#<%= hfAudio.ClientID %>').val())
                    {
                        options.audio = $('#<%= hfAudio.ClientID %>').val() == 'true';
                    }

                    $('.dsrContainer').digitalSign(options);
                }

                /* Hide the mouse after 2.5 seconds of idle time. */
                (function () {
                    var idleMouseTimer;
                    var forceMouseHide = false;
                    var $panel = $('#<%= pnlContent.ClientID %>');

                    $panel.css('cursor', 'none');

                    $panel.mousemove(function (ev) {
                        if (!forceMouseHide) {
                            $panel.css('cursor', '');

                            clearTimeout(idleMouseTimer);

                            idleMouseTimer = setTimeout(function () {
                                $panel.css('cursor', 'none');

                                forceMouseHide = true;
                                setTimeout(function () {
                                    forceMouseHide = false;
                                }, 200);
                            }, 2500);
                        }
                    });
                })();
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
