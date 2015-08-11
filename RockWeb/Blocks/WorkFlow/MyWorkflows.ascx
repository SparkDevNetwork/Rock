<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MyWorkflows.ascx.cs" Inherits="RockWeb.Blocks.WorkFlow.MyWorkflows" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-gears"></i> My Workflows</h1>

                <div class="pull-right">
                    <Rock:Toggle ID="tglRole" CssClass="margin-r-md pull-left"  runat="server" OnText="Initiated By Me" ActiveButtonCssClass="btn-info" ButtonSizeCssClass="btn-xs" OffText="Assigned To Me" AutoPostBack="true" OnCheckedChanged="tgl_CheckedChanged" />
                    <Rock:Toggle ID="tglDisplay" CssClass="pull-left" runat="server" OnText="Active Types" ActiveButtonCssClass="btn-success" ButtonSizeCssClass="btn-xs" OffText="All Types" AutoPostBack="true" OnCheckedChanged="tgl_CheckedChanged" />
                </div>

            </div>
            <div class="panel-body">
                <div class="list-as-blocks clearfix">
                    <ul>
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
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:RockBoundField DataField="Status" HeaderText="Status" SortExpression="Status" />
                            <Rock:RockBoundField DataField="ActiveActivityNames" HeaderText="Active Activities" HtmlEncode="false" />
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
