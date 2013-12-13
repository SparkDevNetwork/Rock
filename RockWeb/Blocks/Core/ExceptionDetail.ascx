<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ExceptionDetail.ascx.cs" Inherits="RockWeb.Blocks.Core.ExceptionDetail" %>

<asp:UpdatePanel ID="upExcpetionDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlSummary" runat="server" CssClass="panel panel-default">
            <div class="panel-body">
                <div class="banner">
                    <h1>
                        <asp:Literal ID="lPageTitle" runat="server" />
                    </h1>
                </div>
                <fieldset>
                    <legend>Summary</legend>
                    <div class="row">
                        <div class="col-md-12">
                            <asp:Literal ID="lExceptionSummary" runat="server" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-3">
                            <Rock:RockCheckBox ID="chkShowCookies" runat="server" />
                        </div>
                        <div class="col-md-3">
                            <Rock:RockCheckBox ID="chkShowServerVariables" runat="server" />
                        </div>
                        <div class="col-md-6"></div>
                    </div>
                </fieldset>
                <div id="divCookies" style="display: none;">
                    <fieldset>
                        <legend>Cookies</legend>
                        <asp:Literal ID="lCookies" runat="server" />
                    </fieldset>
                </div>
                <div id="divServerVariables" style="display: none">
                    <fieldset>
                        <legend>Server Variables</legend>
                        <asp:Literal ID="lServerVariables" runat="server" />
                    </fieldset>
                </div>
                <div id="divExceptionDetails">
                    <fieldset>
                        <legend>Details</legend>

                        <asp:Repeater ID="rptExcpetionDetails" runat="server">

                            <HeaderTemplate>
                                <table class="table table-bordered table-striped table-hover">
                                    <thead>
                                        <tr>
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
                                    <td onclick="<%# string.Format("redirectToPage('{0}')", GetExceptionDetailUrl((int)Eval("Id"))) %>"><%# Eval("ExceptionType") %></td>
                                    <td onclick="<%# string.Format("redirectToPage('{0}')", GetExceptionDetailUrl((int)Eval("Id"))) %>"><%# Eval("Source") %></td>
                                    <td onclick="<%# string.Format("redirectToPage('{0}')", GetExceptionDetailUrl((int)Eval("Id"))) %>"><%# Eval("Description") %></td>
                                    <td style="text-align: center;">
                                        <a id="<%# "lbToggleStackTrace_" + Eval("Id").ToString()  %>" href="#" onclick="<%# string.Format("return toggleStackTrace({0});", Eval("Id")) %>" class="btn btn-default">
                                            <i class="fa fa-file-o"></i> Show Stack Trace
                                        </a>
                                    </td>
                                </tr>
                                <tr id="<%# "trStackTrace_" + Eval("Id").ToString() %>" class="exceptionDetail-stackTrace-hide" onclick="<%# string.Format("redirectToPage('{0}')", GetExceptionDetailUrl((int)Eval("Id"))) %>">
                                    <td colspan="4">
                                        <%#Eval("StackTrace") %>
                                    </td>
                                </tr>
                            </ItemTemplate>
                            <FooterTemplate>
                                </tbody>
                                </table>
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
        $("[id*=trStackTrace_" + exceptionId + "]:first").toggleClass("exceptionDetail-stackTrace-hide");
        $("[id*=trStackTrace_" + exceptionId + "]:first").toggleClass("exceptionDetail-stackTrace-show");

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

    $("[id*=chkShowServerVariables]").click(function () {
        $("#divServerVariables").slideToggle();
    });

    $("[id*=chkShowCookies]").click(function () {
        $("#divCookies").slideToggle();
    });



</script>
