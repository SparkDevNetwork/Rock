<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Locations.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Manager.Locations" %>

<Rock:RockUpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="checkin-manager">

            <Rock:NotificationBox ID="nbGroupTypeWarning" runat="server" NotificationBoxType="Warning" Text="Please select a Check-in type in the block settings." Dismissable="true" />

            <asp:HiddenField ID="hfChartData" runat="server" />
            <asp:HiddenField ID="hfChartOptions" runat="server" />
            <asp:Panel ID="pnlChart" runat="server" style="width:100%;height:170px" />

            <br />

            <div class="row">
                <div class="col-xs-8 col-sm-9 col-md-10 col-lg-11">
                    <Rock:RockTextBox ID="tbSearch" runat="server" Placeholder="Person Search..." />
                </div>
                <div class="col-xs-4 col-sm-3 col-md-2 col-lg-1">
                    <span class="pull-right">
                        <asp:LinkButton ID="lbSearch" runat="server" CssClass="btn btn-default" OnClick="lbSearch_Click"><i class="fa fa-search"></i> Search</asp:LinkButton>
                    </span>
                </div>
            </div>

            <br />

            <div class="panel panel-default">

                <asp:Panel ID="pnlNavHeading" runat="server" CssClass="panel-heading clickable">
                    <asp:PlaceHolder runat="server">
                        <i class="fa fa-chevron-left"></i> <asp:Literal ID="lNavHeading" runat="server" />
                        <span class="pull-right">
                            <Rock:Toggle ID="tglHeadingRoom" runat="server" OnText="Open" OffText="Close" ButtonSizeCssClass="btn-xs" OnCheckedChanged="tglRoom_CheckedChanged" />
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
                                        <Rock:Toggle ID="tglRoom" runat="server" OnText="Open" OffText="Close" ButtonSizeCssClass="btn-xs" OnCheckedChanged="tglRoom_CheckedChanged"  />
                                        <i class='fa fa-fw fa-chevron-right'></i>
                                    </span>
                                </asp:PlaceHolder>
                            </li>
                        </ItemTemplate>
                    </asp:Repeater>

                    <asp:Repeater ID="rptPeople" runat="server">
                        <ItemTemplate>
                            <li id="liNavItem" runat="server" class="list-group-item clickable" >
                                <asp:PlaceHolder runat="server">
                                    <asp:Literal ID="imgPerson" runat="server" /> 
                                    &nbsp; 
                                    <%# Eval("Name") %>
                                    <span class="pull-right">
                                        <asp:Literal ID="lStatus" runat="server" />
                                    </span>
                                </asp:PlaceHolder>
                            </li>
                        </ItemTemplate>
                    </asp:Repeater>

                </ul>

            </div>

        </div>

    </ContentTemplate>
</Rock:RockUpdatePanel>
