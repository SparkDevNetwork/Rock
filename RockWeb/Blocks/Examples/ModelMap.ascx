<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ModelMap.ascx.cs" Inherits="RockWeb.Blocks.Examples.ModelMap" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-gears"></i>Model Map</h1>
            </div>
            <div class="panel-body">
                <div class="list-as-blocks clearfix">
                    <ul>
                        <asp:Repeater ID="rptCategory" runat="server">
                            <ItemTemplate>
                                <li class='<%# Eval("Class") %>'>
                                    <asp:LinkButton ID="lbCategory" runat="server" CommandArgument='<%# Eval("Category.Name") %>' CommandName="Display">
                                        <i class='<%# Eval("Category.IconCssClass") %>'></i>
                                        <h3><%# Eval("Category.Name") %> </h3>
                                    </asp:LinkButton>
                                </li>
                            </ItemTemplate>
                        </asp:Repeater>
                    </ul>
                </div>
                <div class="row">
                    <div class="col-md-4 margin-t-md">
                        <div class="panel-body">
                            <ul>
                                <asp:Repeater ID="rptModel" runat="server">
                                    <ItemTemplate>
                                        <li class='<%# Eval("Class") %>'>
                                            <asp:LinkButton ID="lbModel" runat="server" CommandArgument='<%# Eval("GuidId") %>' CommandName="Display">
                                        <%# Eval("Name") %> 
                                            </asp:LinkButton>
                                        </li>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </ul>
                        </div>
                    </div>
                    <div class="col-md-8">
                        <asp:Literal ID="lClasses" runat="server" ViewStateMode="Disabled"></asp:Literal>
                    </div>
                </div>

            </div>
        </div>


        <h2>Key</h2>

        <table class="table">
            <tr>
                <td class="text-center"><strong class="text-danger">*</strong></td>
                <td>A required field.</td>
            </tr>
            <tr>
                <td class="text-center"><span class="fa-stack small"><i class="fa fa-database fa-stack-1x"></i><i class="fa fa-ban fa-stack-2x text-danger"></i></span></td>
                <td>Not mapped to the database.  These fields are computed and are only available in the object.</td>
            </tr>
            <tr>
                <td class="text-center"><small><span class="tip tip-lava"></span></small></td>
                <td>These fields are available where Lava is supported.</td>
            </tr>
        </table>

        <script type="text/javascript">
            Sys.Application.add_load(function ()
            {
                // Hide and unhide inherited properties and methods
                $('.js-model-inherited').on('click', function ()
                {
                    $(this).find('i.js-model-check').toggleClass('fa-check-square-o fa-square-o');
                    $(this).parent().find('li.js-model').toggleClass('non-hidden hidden ');
                });

            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>

