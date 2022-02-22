<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ExceptionDetail.ascx.cs" Inherits="RockWeb.Blocks.Core.ExceptionDetail" %>

<asp:UpdatePanel ID="upExcpetionDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlSummary" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-bug"></i>
                    <asp:Literal ID="lPageTitle" runat="server" /></h1>
            </div>

            <div class="panel-body">

                <fieldset>
                    <div class="row">
                        <div class="col-md-12">
                            <asp:Literal ID="lExceptionSummary" runat="server" />
                        </div>
                    </div>

                    <div class="panel-actions">
                        <a href="#" id="btnShowCookies" runat="server" class="js-btn-show-cookies btn btn-default">
                            <i class="fa fa-laptop"></i>
                            Show Cookies
                        </a>
                        <a href="#" id="btnShowVariables" runat="server" class="js-btn-show-servervars btn btn-default">
                            <i class="fa fa-hdd-o"></i>
                            Show Server Variables

                        </a>
                    </div>

                </fieldset>

                <div id="divCookies" style="display: none;">
                    <fieldset>
                        <h4>Cookies</h4>
                        <asp:Literal ID="lCookies" runat="server" />
                    </fieldset>
                </div>
                <div id="divServerVariables" style="display: none">
                    <fieldset>
                        <h4>Server Variables</h4>
                        <asp:Literal ID="lServerVariables" runat="server" />
                    </fieldset>
                </div>
                <div id="divExceptionDetails">
                    <fieldset>
                        <h4>Details</h4>
                        <p>
                            This list shows the complete exception hierarchy with a full call-stack trace. The top-level exception is listed first, and subsequent entries provide more specific detail about the origin of the error.
                        </p>
                        <asp:Repeater ID="rptExceptionDetails" runat="server" OnItemDataBound="rptExceptionDetails_ItemDataBound">

                            <HeaderTemplate>
                                <div class="table-responsive">
                                    <table class="table table-bordered table-striped">
                                        <thead>
                                            <tr>
                                                <th>Id</th>
                                                <th>Exception Type</th>
                                                <th>Source</th>
                                                <th>Description</th>
                                                <th>&nbsp;</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr>
                                    <td><%# EncodeHtml( Eval("Id") ) %></td>
                                    <td><%# EncodeHtml( Eval("ExceptionType") ) %></td>
                                    <td><%# EncodeHtml( Eval("Source") )%></td>
                                    <td class="wrap-contents"><%# EncodeHtml( Eval("Description") ) %></td>

                                    <td style="text-align: center;">
                                        <asp:PlaceHolder ID="phStackTraceButton" runat="server">
                                            <a id="<%# "lbToggleStackTrace_" + Eval("Id").ToString()  %>" href="#" onclick="<%# string.Format("return toggleStackTrace({0});", Eval("Id")) %>" class="btn btn-default">
                                                <i class="fa fa-layer-group"></i>
                                                Show Stack Trace
                                            </a>
                                        </asp:PlaceHolder>
                                    </td>
                                </tr>
                                <tr id="<%# "trStackTrace_" + Eval("Id").ToString() %>" class="exceptionDetail-stackTrace-hide">
                                    <td colspan="5">
                                        <pre style="white-space: pre-wrap;"><asp:Literal ID="lStackTrace" runat="server" /></pre>
                                    </td>
                                </tr>
                            </ItemTemplate>
                            <FooterTemplate>
                                </tbody>
                                </table>
                                </div>
                            </FooterTemplate>
                        </asp:Repeater>
                    </fieldset>
                </div>

            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>

<script type="text/javascript">
    function toggleStackTrace(exceptionId) {
        $("[id*=trStackTrace_" + exceptionId + "]").first().toggleClass("exceptionDetail-stackTrace-hide");
        $("[id*=trStackTrace_" + exceptionId + "]").first().toggleClass("exceptionDetail-stackTrace-show");

        var link = $("#lbToggleStackTrace_" + exceptionId);

        if ($(link).html().indexOf("Show") !== -1) {
            $(link).html($(link).html().replace("Show", "Hide"));
        }
        else {
            $(link).html($(link).html().replace("Hide", "Show"));
        }
        return false;
    }

    function redirectToPage(pageUrl) {
        if (pageUrl != undefined && pageUrl != "") {
            var host = document.location.protocol + "//" + document.location.hostname;
            if (document.location.port != 80 && document.location.port != 443) {
                host = host + ":" + document.location.port;
            }
            document.location.href = pageUrl;
        }
    }

    $(".js-btn-show-servervars").on("click", function () {
        $(this).toggleClass("btn-default btn-action");
        $("#divServerVariables").slideToggle();
        return false;
    });

    $(".js-btn-show-formdata").on("click", function () {
        $(this).toggleClass("btn-default btn-action");
        $("#divFormData").slideToggle();
        return false;
    });

    $(".js-btn-show-cookies").on("click", function () {
        $(this).toggleClass("btn-default btn-action");
        $("#divCookies").slideToggle();
        return false;
    });
</script>
