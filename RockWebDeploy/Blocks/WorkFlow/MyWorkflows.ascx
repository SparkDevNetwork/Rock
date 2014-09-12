<%@ control language="C#" autoeventwireup="true" inherits="RockWeb.Blocks.WorkFlow.MyWorkflows, RockWeb" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-gears"></i> My Workflows</h1>
            </div>
            <div class="panel-body">

                <div class="clearfix margin-b-md">
                    <Rock:Toggle ID="tglRole" CssClass="pull-left"  runat="server" OnText="Initiated By Me" ActiveButtonCssClass="btn-info" OffText="Assigned To Me" AutoPostBack="true" OnCheckedChanged="tgl_CheckedChanged" />
                    <Rock:Toggle ID="tglDisplay" CssClass="pull-right" runat="server" OnText="Active Types" ActiveButtonCssClass="btn-success" OffText="All Types" AutoPostBack="true" OnCheckedChanged="tgl_CheckedChanged" />
                </div>

                <div class="list-as-blocks margin-t-lg clearfix">
                    <ul class="list-unstyled">
                        <asp:Repeater ID="rptWorkflowTypes" runat="server">
                            <ItemTemplate>
                                <li class='<%# Eval("Class") %>'>
                                    <asp:LinkButton ID="lbWorkflowType" runat="server" CommandArgument='<%# Eval("WorkflowType.Id") %>' CommandName="Display">
                                        <i class='<%# Eval("WorkflowType.IconCssClass") %>'></i>
                                        <h3><%# Eval("WorkflowType.Name") %> </h3>
                                        <div class="notification">
                                            <span class="label label-danger"><%# ((int)Eval("Count")).ToString("#,###,###") %></span>
                                        </div>
                                    </asp:LinkButton>
                                </li>
                            </ItemTemplate>
                        </asp:Repeater>
                    </ul>
                </div>

                <h4><asp:Literal ID="lWorkflow" runat="server"></asp:Literal></h4>
                <div class="grid">
                    <Rock:Grid ID="gWorkflows" runat="server" OnRowSelected="gWorkflows_Edit" >
                        <Columns>
                            <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <asp:BoundField DataField="Status" HeaderText="Status" SortExpression="Status" />
                            <asp:BoundField DataField="ActiveActivityNames" HeaderText="Active Activities" HtmlEncode="false" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>
        <script>
            $(".my-workflows .list-as-blocks li").on("click", function () {
                $(".my-workflows .list-as-blocks li").removeClass('active');
                $(this).addClass('active');
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
