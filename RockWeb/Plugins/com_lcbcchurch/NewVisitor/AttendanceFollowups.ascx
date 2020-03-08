<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AttendanceFollowups.ascx.cs" Inherits="RockWeb.Plugins.com_lcbcchurch.NewVisitor.AttendanceFollowups" %>

<style type="text/css">
.table thead th.total {
  border-right: 1px solid #000 !important;
}

.table td.total {
  border-right: 1px solid #000 !important;
}
</style>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <script>
            Sys.Application.add_load(function () {
                $(document).ready(function () {

                    $('.grid-label').hover(function () {
                        $(this).find('span').toggle("slide");
                    });
                });
            });
        </script>
        <div class="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fas fa-building"></i>Attendance Follow-ups</h1>
                <div class="pull-right">
                    <Rock:Toggle ID="tglMetricType" runat="server" ButtonSizeCssClass="btn-xs" OnCssClass="btn-warning" OffCssClass="btn-warning" OnText="Last Week" OffText="4 Weeks" CssClass="btn" Checked="true" OnCheckedChanged="tglMetricType_CheckedChanged" />
                </div>
            </div>
            <div class="panel-body">
                <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                <div class="grid grid-panel">
                    <Rock:Grid ID="gAttendances" runat="server" CssClass="js-grid-requests" AllowSorting="true">
                        <Columns>
                            <Rock:CampusField DataField="CampusId" HeaderText="Campus" SortExpression="CampusName" />
                            <Rock:RockTemplateField HeaderText="Total" ItemStyle-HorizontalAlign="Center" HeaderStyle-CssClass="col-md-2 total" HeaderStyle-HorizontalAlign="Center" SortExpression="Total" ItemStyle-CssClass="total">
                                <ItemTemplate>
                                    <%# GetMetricColumnHtml( (decimal)Eval("TotalFulfilled"), (decimal)Eval("TotalQualified")) %>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockTemplateField HeaderText="Welcome Email" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center" SortExpression="WelcomeEmail">
                                <ItemTemplate>
                                    <%# GetMetricColumnHtml( (decimal)Eval("WelcomeEmailFulfilled"), (decimal)Eval("WelcomeEmailQualified")) %>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockTemplateField HeaderText="Welcome Card" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center" SortExpression="WelcomeCard">
                                <ItemTemplate>
                                    <%# GetMetricColumnHtml( (decimal)Eval("WelcomeCardFulfilled"), (decimal)Eval("WelcomeCardQualified")) %>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockTemplateField HeaderText="No Return 2nd Week" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center" SortExpression="NoReturn2ndWeek">
                                <ItemTemplate>
                                    <%# GetMetricColumnHtml( (decimal)Eval("NoReturn2ndWeekFulfilled"), (decimal)Eval("NoReturn2ndWeekQualified")) %>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockTemplateField HeaderText="Cookie Drop" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center" SortExpression="CookieDrop">
                                <ItemTemplate>
                                    <%# GetMetricColumnHtml( (decimal)Eval("CookieDropFulfilled"), (decimal)Eval("CookieDropQualified")) %>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockTemplateField HeaderText="Serve Card" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center" SortExpression="ServeCard">
                                <ItemTemplate>
                                    <%# GetMetricColumnHtml( (decimal)Eval("ServeCardFulfilled"), (decimal)Eval("ServeCardQualified")) %>
                                </ItemTemplate>
                            </Rock:RockTemplateField>

                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
