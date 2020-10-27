<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Locations.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Manager.Locations" %>

<script type="text/javascript">
    Sys.Application.add_load(function () {
        $('.js-cancel-checkin').on('click', function (event) {
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

            <div class="panel-heading hidden">
                <h1 class="panel-title"> <i class="fa fa-sitemap"></i>&nbsp;<asp:Literal ID="lGroupTypeName" runat="server" /></h1>
            </div>

            <div class="panel">
                <asp:HiddenField ID="hfChartData" runat="server" />
                <asp:HiddenField ID="hfChartOptions" runat="server" />

                <asp:Panel ID="pnlChart" runat="server" style="height:180px" CssClass="cursor-pointer" />
            </div>

            <div class="input-group mb-4">
                <Rock:RockTextBox ID="tbSearch" runat="server" Placeholder="Person Search..." />
                <span class="input-group-btn">
                    <asp:LinkButton ID="lbSearch" runat="server" CssClass="btn btn-default" OnClick="lbSearch_Click"><i class="fa fa-search"></i> Search</asp:LinkButton>
                </span>
            </div>

            <div class="panel panel-default">

                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                <asp:Panel ID="pnlNavHeading" runat="server" CssClass="panel-heading cursor-pointer clearfix" >
                    <asp:PlaceHolder runat="server">
                        <div class="pull-left">
                            <i class="fa fa-chevron-left"></i>
                            <asp:Literal ID="lNavHeading" runat="server" />
                        </div>
                        <div class="pull-right">
                            <Rock:Toggle ID="tglHeadingRoom" runat="server" OnText="Open" OffText="Close" ButtonSizeCssClass="btn-xs" OnCssClass="btn-success" OffCssClass="btn-danger" OnCheckedChanged="tglRoom_CheckedChanged" />
                        </div>
                        <asp:Panel ID="pnlThreshold" runat="server" CssClass="location-threshold pull-right d-flex mr-2 js-threshold">
                            <span class="small align-self-center mr-1">Threshold: </span>
                            <Rock:HiddenFieldWithClass ID="hfThreshold" runat="server" CssClass="js-threshold-hf" />
                            <asp:Label ID="lThreshold" runat="server" CssClass="js-threshold-view js-threshold-l small align-self-center mr-2" />
                            <a class="btn btn-default btn-xs btn-square js-threshold-view js-threshold-btn-edit mr-1"><i class="fa fa-edit"></i></a>
                            <Rock:NumberBox ID="nbThreshold" runat="server" CssClass="input-xs input-width-xs js-threshold-edit js-threshold-nb mr-1" NumberType="Integer" Style="display: none"></Rock:NumberBox>
                            <asp:LinkButton ID="lbUpdateThreshold" runat="server" CssClass="btn btn-primary btn-xs btn-square js-threshold-edit js-threshold-btn-save paneleditor-button mr-1" OnClick="lbUpdateThreshold_Click" Style="display: none"><i class="fa fa-check"></i></asp:LinkButton>
                            <a class="btn btn-default btn-xs btn-square js-threshold-edit js-threshold-btn-cancel paneleditor-button mr-1" style="display: none"><i class="fa fa-ban"></i></a>
                        </asp:Panel>
                    </asp:PlaceHolder>
                </asp:Panel>

                <ul class="list-group">
                    <asp:Repeater ID="rptNavItems" runat="server">
                        <ItemTemplate>
                            <li id="liNavItem" runat="server" class="list-group-item cursor-pointer">
                                <div class="content"><%# Eval("Name") %></div>
                                <div class="pull-right d-flex align-items-center">
                                    <asp:Label ID="lblCurrentCount" runat="server" CssClass="badge"/>
                                    <Rock:Toggle ID="tglRoom" runat="server" OnText="Open" OffText="Close" ButtonSizeCssClass="btn-xs" OnCssClass="btn-success" OffCssClass="btn-danger" OnCheckedChanged="tglRoom_CheckedChanged"  />
                                    <i class='fa fa-fw fa-chevron-right'></i>
                                </div>
                            </li>
                        </ItemTemplate>
                    </asp:Repeater>

                    <asp:Repeater ID="rptPeople" runat="server" OnItemCommand="rptPeople_ItemCommand">
                        <ItemTemplate>
                            <li id="liNavItem" runat="server" class="list-group-item d-flex align-items-center cursor-pointer clearfix" >
                                <div class="d-flex align-items-center"><asp:Literal ID="imgPerson" runat="server" />
                                <div>
                                    <span class="js-checkin-person-name"><%# Eval("Name") %></span><asp:Literal ID="lAge" runat="server" />
                                    <%# Eval("ScheduleGroupNames") %>
                                </div>
                                </div>
                                <div class="ml-auto">
                                    <asp:Literal ID="lStatus" runat="server" />
                                    <asp:LinkButton ID="lbRemoveAttendance" runat="server" CssClass="js-cancel-checkin btn btn-xs btn-square btn-danger"
                                        CommandArgument='<%# Eval("Id") %>' CommandName="Delete" Visible='<%# (bool)Eval("ShowCancel") %>'><i class="fa fa-times"></i></asp:LinkButton>
                                </div>
                            </li>
                        </ItemTemplate>
                    </asp:Repeater>

                </ul>

            </div>

        </asp:Panel>

    </ContentTemplate>
</Rock:RockUpdatePanel>
