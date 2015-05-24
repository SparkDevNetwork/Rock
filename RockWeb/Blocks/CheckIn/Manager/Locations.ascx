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

                <asp:Panel ID="pnlNavHeading" runat="server" CssClass="panel-heading clickable" >
                    <asp:PlaceHolder runat="server">
                        <i class="fa fa-chevron-left"></i> <asp:Literal ID="lNavHeading" runat="server" />
                        <span class="pull-right">
                            <Rock:Toggle ID="tglHeadingRoom" runat="server" OnText="Open" OffText="Close" ButtonSizeCssClass="btn-xs" OnCssClass="btn-success" OffCssClass="btn-danger" OnCheckedChanged="tglRoom_CheckedChanged" />
                        </span>
                    </asp:PlaceHolder>
                </asp:Panel>

                <ul class="list-group">

                    <asp:Repeater ID="rptNavItems" runat="server">
                        <ItemTemplate>
                            <li id="liNavItem" runat="server" class="list-group-item clickable" >
                                <asp:PlaceHolder runat="server">
                                    <%# Eval("Name") %>
                                    <span class="pull-right">
                                        <span class='badge<%# (int)Eval("CurrentCount") > 0 ? " badge-success" : "" %> '><%# ((int)Eval("CurrentCount")).ToString("N0") %></span>
                                        &nbsp;&nbsp;
                                        <Rock:Toggle ID="tglRoom" runat="server" OnText="Open" OffText="Close" ButtonSizeCssClass="btn-xs" OnCssClass="btn-success" OffCssClass="btn-danger" OnCheckedChanged="tglRoom_CheckedChanged"  />
                                        <i class='fa fa-fw fa-chevron-right'></i>
                                    </span>
                                </asp:PlaceHolder>
                            </li>
                        </ItemTemplate>
                    </asp:Repeater>

                    <asp:Repeater ID="rptPeople" runat="server" OnItemCommand="rptPeople_ItemCommand">
                        <ItemTemplate>
                            <li id="liNavItem" runat="server" class="list-group-item clickable clearfix" >
                                <asp:PlaceHolder runat="server">
                                    <div class="photoframe pull-left margin-r-md"><asp:Literal ID="imgPerson" runat="server" /></div>
                                    <div class="pull-left margin-t-sm">
                                        <span class="js-checkin-person-name"><%# Eval("Name") %></span> <br /> 
                                        <asp:Literal ID="lAge" runat="server" />
                                    </div>
                                    <span class="pull-right margin-t-md">
                                        <%# Eval("GroupName") %><asp:Literal ID="lStatus" runat="server" /><br />
                                        <asp:LinkButton ID="lbRemoveAttendance" runat="server" Text="Cancel Checkin" CssClass="js-cancel-checkin btn btn-xs btn-default" 
                                            CommandArgument='<%# Eval("Id") %>' CommandName="Delete" />
                                    </span>
                                </asp:PlaceHolder>
                            </li>
                        </ItemTemplate>
                    </asp:Repeater>

                </ul>

            </div>

        </asp:Panel>

    </ContentTemplate>
</Rock:RockUpdatePanel>
