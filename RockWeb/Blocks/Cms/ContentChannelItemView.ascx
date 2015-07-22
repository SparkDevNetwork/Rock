<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContentChannelItemView.ascx.cs" Inherits="RockWeb.Blocks.Cms.ContentChannelItemView" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block list-as-blocks">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-bullhorn"></i> My Content</h1>

                <div class="pull-right">
                    <Rock:Toggle ID="tglStatus" runat="server" OnText="Pending" ActiveButtonCssClass="btn-success" ButtonSizeCssClass="btn-xs" OffText="All Items" AutoPostBack="true" OnCheckedChanged="tgl_CheckedChanged" />
                </div>

            </div>
            <div class="panel-body">

                <ul>
                    <asp:Repeater ID="rptChannels" runat="server">
                        <ItemTemplate>
                            <li class='<%# Eval("Class") %>'>
                                <asp:LinkButton ID="lbChannel" runat="server" CommandArgument='<%# Eval("Channel.Id") %>' CommandName="Display">
                                    <i class='<%# Eval("Channel.IconCssClass") %>'></i>
                                    <h3><%# Eval("Channel.Name") %> </h3>
                                    <div class="notification">
                                        <span class="label label-danger"><%# ((int)Eval("Count")).ToString("#,###,###") %></span>
                                    </div>
                                </asp:LinkButton>
                            </li>
                        </ItemTemplate>
                    </asp:Repeater>
                </ul>

            </div>
        </div>

        <div class="panel panel-block" id="divItemPanel" runat="server" visible="false">
            <div class="panel-heading">
                <div class="panel-title">
                    <i class="fa fa-bullhorn"></i> <asp:Literal ID="lContentChannelItems" runat="server"></asp:Literal></h1>
                </div>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">

                    <Rock:GridFilter ID="gfFilter" runat="server">
                        <Rock:RockDropDownList ID="ddlStatus" runat="server" Label="Status" />
                        <Rock:DateRangePicker ID="drpDateRange" runat="server" Label="Date Range" />
                        <Rock:RockTextBox ID="tbTitle" runat="server" Label="Title" />
                    </Rock:GridFilter>               

                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />

                    <Rock:Grid ID="gContentChannelItems" runat="server" AllowSorting="true" OnRowSelected="gContentChannelItems_Edit" />

                </div>
            </div>
        </div>

        <script>
            $(".my-contentItems .list-as-blocks li").on("click", function () {
                $(".my-contentItems .list-as-blocks li").removeClass('active');
                $(this).addClass('active');
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
