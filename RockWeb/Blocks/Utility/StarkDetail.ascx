<%@ Control Language="C#" AutoEventWireup="true" CodeFile="StarkDetail.ascx.cs" Inherits="RockWeb.Blocks.Utility.StarkDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block panel-next">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-star"></i>
                    <span>Blank Detail Block</span>
                </h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlblTest" runat="server" LabelType="Info" Text="Label" />
                </div>

                <div class="panel-toolbar" role="menu">
                    <div id="ctl00_main_ctl33_ctl01_ctl06_pnlFollowing" class="panel-follow-status js-follow-status" data-toggle="tooltip" data-placement="top" title="" data-original-title="Click to Follow">
                    </div>
                </div>

                <div class="panel-toolbar" role="menu">
                    <a href="#" class="btn btn-toolbar-master" data-toggle="dropdown" aria-expanded="false"><i class="fa fa-ellipsis-v"></i></a>
                    <ul id="menu1" class="dropdown-menu" aria-labelledby="drop4">
                    <li><a href="#">Settings</a></li>
                    <li><a href="#">Fullscreen</a></li>
                    <li role="separator" class="divider"></li>
                    <li><a href="#">Show Details</a></li>
                    </ul>
                </div>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-subheading">
                <div class="pull-left">
                <Rock:TagList ID="taglPersonTags" runat="server" CssClass="clearfix" />
                </div>

                <div class="badges pull-right">
                    <i class="fa fa-check-circle" style="color:#83758F"></i>
                    <i class="fa fa-flag-checkered" style="color:#16C98D"></i>
                </div>
            </div>
            <div class="panel-body">

                <span class="label label-default">Default</span>
                <span class="label label-primary">Primary</span>
                <span class="label label-success">Success</span>
                <span class="label label-info">Info</span>
                <span class="label label-warning">Warning</span>
                <span class="label label-danger">Danger</span>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>