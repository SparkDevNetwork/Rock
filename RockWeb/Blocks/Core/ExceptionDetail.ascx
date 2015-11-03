<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ExceptionDetail.ascx.cs" Inherits="RockWeb.Blocks.Core.ExceptionDetail" %>

<asp:UpdatePanel ID="upExcpetionDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlSummary" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-bug"></i> <asp:Literal ID="lPageTitle" runat="server" /></h1>
            </div>

            <div class="panel-body">
                
                <fieldset>
                    <h4>Summary</h4>
                    <div class="row">
                        <div class="col-md-12">
                            <asp:Literal ID="lExceptionSummary" runat="server" />
                        </div>
                    </div>
                    <div class="row actions">
                        <div class="col-md-3">
                            <a href="#" id="btnShowCookies" runat="server" class="btn-show-cookies btn btn-action"><i class="fa fa-laptop"></i> Show Cookies</a>
                        </div>
                        <div class="col-md-3">
                            <a href="#" id="btnShowVariables" runat="server" class="btn-show-servervars btn btn-action"><i class="fa fa-hdd-o"></i> Show Server Variables</a>
                        </div>
                        <div class="col-md-3">
                            <a href="#" id="btnShowFormData" runat="server" class="btn-show-formdata btn btn-action"><i class="fa fa-hdd-o"></i> Show Form Data</a>
                        </div>
                        <div class="col-md-6"></div>
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
                <div id="divFormData" style="display: none">
                    <fieldset>
                        <h4>Form Data</h4>
                        <asp:Literal ID="lFormData" runat="server" />
                    </fieldset>
                </div>
                <div id="divExceptionDetails">
                    <fieldset>
                        <h4>Details</h4>

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
                                    <td onclick="<%# string.Format("redirectToPage('{0}')", GetExceptionDetailUrl((int)Eval("Id"))) %>"><%# EncodeHtml( Eval("ExceptionType") ) %></td>
                                    <td onclick="<%# string.Format("redirectToPage('{0}')", GetExceptionDetailUrl((int)Eval("Id"))) %>"><%# EncodeHtml( Eval("Source") )%></td>
                                    <td onclick="<%# string.Format("redirectToPage('{0}')", GetExceptionDetailUrl((int)Eval("Id"))) %>"><%# EncodeHtml( Eval("Description") ) %></td>
                                    <td style="text-align: center;">
                                        <a id="<%# "lbToggleStackTrace_" + Eval("Id").ToString()  %>" href="#" onclick="<%# string.Format("return toggleStackTrace({0});", Eval("Id")) %>" class="btn btn-default">
                                            <i class="fa fa-file-o"></i> Show Stack Trace
                                        </a>
                                    </td>
                                </tr>
                                <tr id="<%# "trStackTrace_" + Eval("Id").ToString() %>" class="exceptionDetail-stackTrace-hide" onclick="<%# string.Format("redirectToPage('{0}')", GetExceptionDetailUrl((int)Eval("Id"))) %>">
                                    <td colspan="4">
                                        <pre><%#Eval("StackTrace") %></pre>
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

    $(".btn-show-servervars").click(function () {
        $("#divServerVariables").slideToggle();
        return false;
    });

    $(".btn-show-formdata").click(function () {
        $("#divFormData").slideToggle();
        return false;
    });

    $(".btn-show-cookies").click(function () {
        $("#divCookies").slideToggle();
        return false;
    });



</script>
