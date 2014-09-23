<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MyContentItems.ascx.cs" Inherits="RockWeb.Blocks.Cms.MyContentItems" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-bullhorn"></i> My Content</h1>

                <div class="pull-right">
                    <Rock:Toggle ID="tglStatus" runat="server" OnText="Pending" ActiveButtonCssClass="btn-success" ButtonSizeCssClass="btn-xs" OffText="All" AutoPostBack="true" OnCheckedChanged="tgl_CheckedChanged" />
                </div>

            </div>
            <div class="panel-body">

                <div class="list-as-blocks margin-t-lg clearfix">
                    <ul class="list-unstyled">
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

                <h4><asp:Literal ID="lContentItem" runat="server"></asp:Literal></h4>

                <div class="grid">
                    <Rock:GridFilter ID="gfFilter" runat="server">
                        <Rock:RockDropDownList ID="ddlStatus" runat="server" Label="Status" />
                        <Rock:DateRangePicker ID="drpDateRange" runat="server" Label="Date Range" />
                        <Rock:RockTextBox ID="tbTitle" runat="server" Label="Title" />
                    </Rock:GridFilter>               
                    <Rock:Grid ID="gContentItems" runat="server" OnRowSelected="gContentItems_Edit" >
                        <Columns>
                            <asp:BoundField DataField="Title" HeaderText="Title" SortExpression="Title" />
                            <Rock:DateTimeField DataField="StartDateTime" HeaderText="Start" SortExpression="StartDateTime" />
                            <Rock:DateTimeField DataField="ExpireDateTime" HeaderText="Expire" SortExpression="ExpireDateTime" />
                            <asp:BoundField DataField="Priority" HeaderText="Priority" SortExpression="Priority" DataFormatString="{0:N0}" ItemStyle-HorizontalAlign="Right" />
                        </Columns>
                    </Rock:Grid>
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
