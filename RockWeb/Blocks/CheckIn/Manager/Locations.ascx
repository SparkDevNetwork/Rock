<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Locations.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Manager.Locations" %>

<script type="text/javascript">
    Sys.Application.add_load(function () {
        $('.js-cancel-checkin').click(function (event) {
            event.stopImmediatePropagation();
            var personName = $(this).closest(".list-group-item").find(".js-checkin-person-name").first().text();
            return Rock.dialogs.confirmDelete(event, 'Checkin for ' + personName);
        });
    });
</script>
<Rock:RockUpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbWarning" runat="server" NotificationBoxType="Warning" Dismissable="true" />

        <asp:Panel ID="pnlContent" runat="server" CssClass="checkin-manager">

            <asp:HiddenField ID="hfChartData" runat="server" />
            <asp:HiddenField ID="hfChartOptions" runat="server" />
            <asp:Panel ID="pnlChart" runat="server" style="width:100%;height:170px" CssClass="clickable" />

            <br />

            <div class="input-group">
                <Rock:RockTextBox ID="tbSearch" runat="server" Placeholder="Person Search..." />
                <span class="input-group-btn">
                    <asp:LinkButton ID="lbSearch" runat="server" CssClass="btn btn-default" OnClick="lbSearch_Click"><i class="fa fa-search"></i> Search</asp:LinkButton>
                </span>
            </div>

            <br />

            <div class="panel panel-default">

                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <asp:Panel ID="pnlNavHeading" runat="server" CssClass="panel-heading clickable clearfix" >
                    <asp:PlaceHolder runat="server">
                        <div class="margin-t-sm pull-left">
                            <i class="fa fa-chevron-left"></i> 
                            <asp:Literal ID="lNavHeading" runat="server" />
                        </div>
                        <div class="pull-right margin-v-sm">
                            <Rock:Toggle ID="tglHeadingRoom" runat="server" OnText="Open" OffText="Close" ButtonSizeCssClass="btn-xs" OnCssClass="btn-success" OffCssClass="btn-danger" OnCheckedChanged="tglRoom_CheckedChanged" />
                        </div>
                        <asp:Panel ID="pnlThreshold" runat="server" CssClass="pull-right margin-r-md margin-t-sm js-threshold paneleditor">
                            <span class="paneleditor-label">Threshold:</span> 
                            <Rock:HiddenFieldWithClass ID="hfThreshold" runat="server" CssClass="js-threshold-hf" />
                            <asp:Label ID="lThreshold" runat="server" CssClass="js-threshold-view js-threshold-l" /> 
                            <a class="btn btn-default btn-xs js-threshold-view js-threshold-btn-edit"><i class="fa fa-edit"></i></a>
                            <Rock:NumberBox ID="nbThreshold" runat="server" CssClass="input-width-xs js-threshold-edit js-threshold-nb paneleditor-input" NumberType="Integer" style="display:none"></Rock:NumberBox>
                            <asp:LinkButton id="lbUpdateThreshold" runat="server" CssClass="btn btn-success btn-xs js-threshold-edit js-threshold-btn-save paneleditor-button" OnClick="lbUpdateThreshold_Click" style="display:none"><i class="fa fa-check"></i></asp:LinkButton>
                            <a class="btn btn-warning btn-xs js-threshold-edit js-threshold-btn-cancel paneleditor-button" style="display:none"><i class="fa fa-ban"></i></a>
                        </asp:Panel>
                    </asp:PlaceHolder>
                </asp:Panel>

                <ul class="list-group">

                    <asp:Repeater ID="rptNavItems" runat="server">
                        <ItemTemplate>
                            <li id="liNavItem" runat="server" class="list-group-item clickable" >
                                <div class="content margin-v-sm"><%# Eval("Name") %></div>
                                <div class="pull-right margin-v-sm">
                                    <asp:Label ID="lblCurrentCount" runat="server" CssClass="badge"/>
                                    &nbsp;&nbsp;
                                    <Rock:Toggle ID="tglRoom" runat="server" OnText="Open" OffText="Close" ButtonSizeCssClass="btn-xs" OnCssClass="btn-success" OffCssClass="btn-danger" OnCheckedChanged="tglRoom_CheckedChanged"  />
                                    <i class='fa fa-fw fa-chevron-right'></i>
                                </div>
                            </li>
                        </ItemTemplate>
                    </asp:Repeater>

                    <asp:Repeater ID="rptPeople" runat="server" OnItemCommand="rptPeople_ItemCommand">
                        <ItemTemplate>
                            <li id="liNavItem" runat="server" class="list-group-item clickable clearfix" >
                                <div class="photoframe pull-left margin-r-md"><asp:Literal ID="imgPerson" runat="server" /></div>
                                <div class="pull-left margin-t-sm">
                                    <span class="js-checkin-person-name"><%# Eval("Name") %></span><asp:Literal ID="lAge" runat="server" />
                                    <%# Eval("ScheduleGroupNames") %>
                                </div>
                                <span class="pull-right margin-t-sm">
                                    <asp:Literal ID="lStatus" runat="server" />
                                    <asp:LinkButton ID="lbRemoveAttendance" runat="server" CssClass="js-cancel-checkin btn btn-xs btn-danger" 
                                        CommandArgument='<%# Eval("Id") %>' CommandName="Delete" Visible='<%# (bool)Eval("ShowCancel") %>'><i class="fa fa-times"></i></asp:LinkButton>
                                </span>
                            </li>
                        </ItemTemplate>
                    </asp:Repeater>

                </ul>

            </div>

        </asp:Panel>

    </ContentTemplate>
</Rock:RockUpdatePanel>
