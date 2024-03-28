<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PowerBiReportViewer.ascx.cs" Inherits="RockWeb.Blocks.Reporting.PowerBiReportViewer" %>

<asp:UpdatePanel ID="upnlContent" runat="server" UpdateMode="Conditional">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbError" runat="server" NotificationBoxType="Warning" />

        <%-- Edit Panel --%>
        <asp:Panel ID="pnlEditModal" runat="server" Visible="false">
            <Rock:ModalDialog ID="mdEdit" runat="server" OnSaveClick="lbSave_Click" Title="Report Configuration" OnCancelScript="clearDialog();">
                <Content>

                    <asp:UpdatePanel ID="upnlEdit" runat="server">
                        <ContentTemplate>

                            <Rock:RockDropDownList ID="ddlSettingPowerBiAccount" runat="server" Label="Power BI Account" AutoPostBack="true" OnSelectedIndexChanged="ddlSettingPowerBiAccount_SelectedIndexChanged" />
                            <Rock:RockDropDownList ID="ddlSettingPowerBiGroup" runat="server" Label="Power BI Workspace"  AutoPostBack="true" OnSelectedIndexChanged="ddlSettingPowerBiGroup_SelectedIndexChanged"/>
                            <Rock:RockDropDownList ID="ddlSettingPowerBiReportUrl" runat="server" Label="Report" />
                            <Rock:RockCheckBox ID="cbSettingPowerBIFullsizeBtn" runat="server" Label="Show Fullsize Button" />
                            <Rock:RockCheckBox ID="cbSettingPowerBIRightPane" runat="server" Label="Show Right Pane" />
                            <Rock:RockCheckBox ID="cbSettingPowerBINavPane" runat="server" Label="Show Navigation Pane" />

                            <Rock:CodeEditor ID="ceAppendUrlTemplate" runat="server" Label="Append URL Template"
                                Help="The results of this Lava template will be appended to the end of the URL for the iframe. As an example, if you wanted to provide a filter you could provide '&filter=ModelName/PropertyName eq 12'. See <a href='https://learn.microsoft.com/en-us/power-bi/collaborate-share/service-url-filters'>Power BI docs</a> for details on filters." />

                        </ContentTemplate>
                    </asp:UpdatePanel>
                </Content>
            </Rock:ModalDialog>
        </asp:Panel>

        <asp:Panel ID="pnlView" runat="server">
            <asp:HiddenField ID="hfAccessToken" runat="server" />
            <asp:HiddenField ID="hfReportEmbedUrl" runat="server" />
            <div class="js-report-container">
                <div class="report-wrapper js-report-wrapper">
                    <div style="position: relative; width: 100%; height: 100%;">
                        <a id="fullsizer" href="#" class="btn btn-action report-fullsize" title="View Fullsize" style="position: absolute; top: 0; left: 0; border-radius: 0;" runat="server"><i class="fa fa-arrows-alt"></i></a>
                        <iframe id="report-iframe" src="" frameborder="1" style="border: 0;" seamless></iframe>
                    </div>
                </div>
            </div>

            <script type="text/javascript">
                    Sys.Application.add_load(function () {
                        iframe = document.getElementById('report-iframe');
                        if (!iframe) {
                            return;
                        }

                        iframe.src = $('#<%= hfReportEmbedUrl.ClientID%>').val();
                        iframe.onload = postActionLoadReport;

                        var height = $(window).height() - 200;
                        var width = $('#report-iframe').offsetParent().width();
                        $('#report-iframe').css('height', height);
                        $('#report-iframe').css('width', width);

                        $(".report-fullsize").on("click", function (event) {
                            event.preventDefault();

                            var reportWrapper = $(this).closest('.js-report-wrapper');
                            var reportIframe = reportWrapper.find('#report-iframe');

                            if (reportWrapper.hasClass("open")) {
                                $('#body').css("overflow","visible");
                                reportWrapper.prependTo(".js-report-container");
                                reportWrapper.css("position", "static");
                                reportWrapper.css("width", "100%");
                                reportWrapper.css("height", "100%");
                                reportWrapper.css("z-index", "auto");
                                reportWrapper.css("top", "auto");
                                reportWrapper.css("left", "auto");
                                reportWrapper.removeClass("open");


                                var height = $(window).height() - 200; //$('#report-iframe').offsetParent().height();
                                var width = $('#report-iframe').offsetParent().width();
                                reportIframe.css("width", width);
                                reportIframe.css("height", height);
                            } else {
                                $('#body').css("overflow","hidden");
                                reportWrapper.prependTo("#page-wrapper");
                                reportWrapper.css("position", "fixed");
                                reportWrapper.css("width", "100%");
                                reportWrapper.css("height", "100%");
                                reportWrapper.css("z-index", "9999");
                                reportWrapper.css("top", "0");
                                reportWrapper.css("left", "0");
                                reportWrapper.addClass("open");

                                reportIframe.css("width", "100%");
                                reportIframe.css("height", "100%");
                            }
                        });
                    });

                // post the auth token to the iFrame.
                function postActionLoadReport() {

                    // get the access token.
                    accessToken = $('#<%= hfAccessToken.ClientID%>').val();

                    // return if no a
                    if ("" === accessToken)
                        return;

                    // construct the push message structure
                    // this structure also supports setting the reportId, groupId, height, and width.
                    // when using a report in a group, you must provide the groupId on the iFrame SRC
                    var m = { action: "loadReport", accessToken: accessToken };
                    message = JSON.stringify(m);

                    // push the message.
                    iframe = document.getElementById('report-iframe');
                    iframe.contentWindow.postMessage(message, "*");;
                }

                //And if the outer div has no set specific height set..
                $(window).resize(function () {
                    if (!$('.js-report-wrapper').hasClass("open")) {
                        var height = $(window).height() - 200; //$('#report-iframe').offsetParent().height();;
                        var width = $('#report-iframe').offsetParent().width();
                        $('#report-iframe').css('height', height);
                        $('#report-iframe').css('width', width);
                    }
                });
            </script>
        </asp:Panel>

        <asp:Panel ID="pnlLogin" runat="server" Visible="false">
            <p>
                The connection to Power BI could not be established. Please attempt to log in again.
            </p>
            <asp:Button ID="btnLogin" runat="server" Text="Log In" CssClass="btn btn-primary" OnClick="btnLogin_Click" />
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>

