<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MyConnectionOpportunities.ascx.cs" Inherits="RockWeb.Blocks.Connection.MyConnectionOpportunities" %>
<%@ Import namespace="Rock" %>
<script>
    Sys.Application.add_load(function () {
        $('.js-legend-badge').tooltip({ html: true, container: 'body', delay: { show: 200, hide: 100 } });
    });

    //Sys.WebForms.PageRequestManager.getInstance().add_endRequest(scrollToGrid);
    function scrollToGrid() {
        if (!$('.js-grid-header').visible(true)) {
            $('html, body').animate({
                scrollTop: $('.js-grid-header').offset().top + 'px'
            }, 'fast');
        }
    }
</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class='fa fa-plug'></i>
                    My Connection Requests</h1>

                <div class="pull-right">
                    <div class="pull-left badge-legend padding-r-md">
                        <span class="pull-left badge badge-info js-legend-badge" data-toggle="tooltip" data-original-title="Assigned To You">&nbsp;</span>
                        <span class="pull-left badge badge-warning js-legend-badge" data-toggle="tooltip" data-original-title="Unassigned Item">&nbsp;</span>
                        <span class="pull-left badge badge-critical js-legend-badge" data-toggle="tooltip" data-original-title="Critical Status">&nbsp;</span>
                        <asp:Literal ID="lIdleToolTip" runat="server" />
                    </div>
                    <Rock:Toggle ID="tglMyOpportunities" CssClass="margin-r-md pull-left" runat="server" OnText="My Requests" ActiveButtonCssClass="btn-info" ButtonSizeCssClass="btn-xs" OffText="All Requests" AutoPostBack="true" OnCheckedChanged="tglMyOpportunities_CheckedChanged" Checked="true" />
                    <asp:LinkButton ID="lbConnectionTypes" runat="server" CssClass=" pull-right" OnClick="lbConnectionTypes_Click" CausesValidation="false"><i class="fa fa-gear"></i></asp:LinkButton>
                </div>

            </div>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbNoOpportunities" runat="server" NotificationBoxType="Info" Text="There are no current connection requests." Visible="" />

                <asp:Repeater ID="rptConnnectionTypes" runat="server" OnItemDataBound="rptConnnectionTypes_ItemDataBound">
                    <ItemTemplate>
                        <asp:Literal ID="lConnectionTypeName" runat="server" />
                        <div class="list-as-blocks clearfix">    
                            <ul>
                                <asp:Repeater ID="rptConnectionOpportunities" runat="server" OnItemCommand="rptConnectionOpportunities_ItemCommand">
                                    <ItemTemplate>
                                        <li class='<%# SelectedOpportunityId.HasValue && (int)Eval("Id") == SelectedOpportunityId.Value ? "active" : "" %> block-status'>
                                            <asp:LinkButton ID="lbConnectionOpportunity" runat="server" CommandArgument='<%# Eval("Id") %>' CommandName="Display">
                                                <i class='<%# Eval("IconCssClass") %>'></i>
                                                <h3><%# Eval("Name") %> </h3>
                                                <div class="status-list">
                                                    <span class="badge badge-info"><%# ((int)Eval("AssignedToYou")).ToString("#,###,###") %></span>
                                                    <span class="badge badge-warning"><%# ((int)Eval("UnassignedCount")).ToString("#,###,###") %></span>
                                                    <span class="badge badge-critical"><%# ((int)Eval("CriticalCount")).ToString("#,###,###") %></span>
                                                    <span class="badge badge-danger"><%# ((int)Eval("IdleCount")).ToString("#,###,###") %></span>
                                                </div>
                                            </asp:LinkButton>
                                        </li>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </ul>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>

            </div>
        </div>
        <asp:Panel ID="pnlGrid" runat="server" CssClass="panel panel-block js-grid-header" Visible="false">
            <div class="panel-heading">
                <h1 class="panel-title"><asp:Literal ID="lOpportunityIcon" runat="server" /> <asp:Literal ID="lConnectionRequest" runat="server"></asp:Literal></h1>
            </div>
            <div class="panel-body">
                <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                <div class="grid grid-panel">
                    <Rock:GridFilter ID="rFilter" runat="server" OnDisplayFilterValue="rFilter_DisplayFilterValue">
                        <Rock:PersonPicker ID="ppRequester" runat="server" Label="Requester" />
                        <Rock:PersonPicker ID="ppConnector" runat="server" Label="Connector" />
                        <Rock:RockCheckBoxList ID="cblStatus" runat="server" Label="Status" DataTextField="Name" DataValueField="Id" RepeatDirection="Horizontal" />
                        <Rock:RockCheckBoxList ID="cblState" runat="server" Label="State" RepeatDirection="Horizontal">
                            <asp:ListItem Text="Active" Value="0" />
                            <asp:ListItem Text="Inactive" Value="1" />
                            <asp:ListItem Text="Future Follow Up" Value="2" />
                            <asp:ListItem Text="Future Follow Up (Past Due)" Value="-2" />
                            <asp:ListItem Text="Connected" Value="3" />
                        </Rock:RockCheckBoxList>
                        <Rock:RockCheckBoxList ID="cblCampus" runat="server" Label="Campus" DataTextField="Name" DataValueField="Id" RepeatDirection="Horizontal" />
                        <Rock:RockCheckBoxList ID="cblLastActivity" runat="server" Label="Last Activity" DataTextField="Name" DataValueField="Id" RepeatDirection="Horizontal" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="gRequests" runat="server" OnRowSelected="gRequests_Edit" CssClass="js-grid-requests" AllowSorting="true" >
                        <Columns>
                            <Rock:SelectField />
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="PersonAlias.Person.LastName,PersonAlias.Person.NickName" />
                            <Rock:RockBoundField DataField="Campus" HeaderText="Campus" SortExpression="Campus.Name" />
                            <Rock:RockBoundField DataField="Group" HeaderText="Group" SortExpression="AssignedGroup.Name" />
                            <Rock:RockBoundField DataField="Connector" HeaderText="Connector" SortExpression="Connector.PersonAlias.Person.LastName,Connector.PersonAlias.Person.NickName" />
                            <Rock:RockBoundField DataField="LastActivity" HeaderText="Last Activity" HtmlEncode="false" />
                            <Rock:RockBoundField DataField="LastActivityNote" HeaderText="Last Activity Note" HtmlEncode="false" />
                            <asp:TemplateField HeaderText="State" SortExpression="ConnectionState" >
                                <ItemTemplate>
                                    <%# Eval("StateLabel") %>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Status" SortExpression="ConnectionStatus.Name">
                                <ItemTemplate>
                                    <span class='label label-<%# Eval("StatusLabel") %>'><%# Eval("Status") %></span>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <Rock:DeleteField OnClick="gRequests_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>
        <script>
            $(".my-workflows .list-as-blocks li").on("click", function () {
                $(".my-workflows .list-as-blocks li").removeClass('active');
                $(this).addClass('active');
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
