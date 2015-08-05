<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MyConnectionOpportunities.ascx.cs" Inherits="RockWeb.Blocks.Connection.MyConnectionOpportunities" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class='fa fa-plug'></i>
                    My Connection Requests</h1>

                <div class="pull-right">
                    <Rock:Toggle ID="tglMyOpportunities" CssClass="margin-r-md pull-left" runat="server" OnText="My Opportunities" ActiveButtonCssClass="btn-info" ButtonSizeCssClass="btn-xs" OffText="All Opportunities" AutoPostBack="true" OnCheckedChanged="tglMyOpportunities_CheckedChanged" Checked="true" />
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
                                        <li class='<%# SelectedOpportunityId.HasValue && (int)Eval("Id") == SelectedOpportunityId.Value ? "active" : "" %>'>
                                            <asp:LinkButton ID="lbConnectionOpportunity" runat="server" CommandArgument='<%# Eval("Id") %>' CommandName="Display">
                                        <i class='<%# Eval("IconCssClass") %>'></i>
                                        <h3><%# Eval("Name") %> </h3>
                                        <div class="notification">
                                            <span class="label label-danger"><%# ((int)Eval("ActiveCount")).ToString("#,###,###") %></span>
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
        <asp:Panel ID="pnlGrid" runat="server" CssClass="panel panel-block" Visible="false">
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
                        <Rock:RockCheckBoxList ID="cblState" runat="server" Label="State" RepeatDirection="Horizontal" />
                        <Rock:RockCheckBoxList ID="cblCampus" runat="server" Label="Campus" DataTextField="Name" DataValueField="Id" RepeatDirection="Horizontal" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="gRequests" runat="server" OnRowSelected="gRequests_Edit" CssClass="js-grid-requests" >
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:RockBoundField DataField="Campus" HeaderText="Campus" SortExpression="Campu" />
                            <Rock:RockBoundField DataField="Group" HeaderText="Group" SortExpression="Group" />
                            <Rock:RockBoundField DataField="Connector" HeaderText="Connector" />
                            <Rock:RockBoundField DataField="Activities" HeaderText="Activities" HtmlEncode="false" />
                            <asp:TemplateField HeaderText="State">
                                <ItemTemplate>
                                    <span class='label label-<%# Eval("StateLabel") %>'><%# Eval("State") %></span>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Status">
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
