<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ModelMap.ascx.cs" Inherits="RockWeb.Blocks.Examples.ModelMap" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfSelectedCategoryGuid" runat="server" />
        <asp:HiddenField ID="hfSelectedEntityId" runat="server" />
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-object-ungroup"></i> Model Map</h1>
            </div>
            <div class="panel-body">
                <div class="list-as-blocks clearfix">
                    <ul>
                        <asp:Repeater ID="rptCategory" runat="server">
                            <ItemTemplate>
                                <li class='<%# GetCategoryClass( Eval("Guid") ) %>'>
                                    <asp:LinkButton ID="lbCategory" runat="server" CommandArgument='<%# Eval("Guid") %>' CommandName="Display">
                                        <i class='<%# Eval("IconCssClass") %>'></i>
                                        <h3><%# Eval("Name") %> </h3>
                                    </asp:LinkButton>
                                </li>
                            </ItemTemplate>
                        </asp:Repeater>
                    </ul>
                </div>
            </div>


        </div>

        <div class="row">
            <div class="col-md-4">
                <asp:Panel ID="pnlModels" runat="server" CssClass="panel panel-block" Visible="false" >
                    <div class="panel-heading">
                        <h1 class="panel-title"> <asp:Literal ID="lCategoryName" runat="server" /></h1>
                    </div>
                    <div class="panel-body">
                        <ul class="list-unstyled">
                            <asp:Repeater ID="rptModel" runat="server">
                                <ItemTemplate>
                                    <li class='<%# GetEntityClass( Eval("Id") ) %>'>
                                        <asp:LinkButton ID="lbModel" runat="server" CommandArgument='<%# Eval("Id") %>' CommandName="Display">
                                            <%# Eval("FriendlyName") %> 
                                        </asp:LinkButton>
                                    </li>
                                </ItemTemplate>
                            </asp:Repeater>
                        </ul>
                    </div>
                </asp:Panel>
            </div>
            <div class="col-md-8">

                <Rock:NotificationBox ID="nbClassesWarning" runat="server" NotificationBoxType="Warning" Visible="false" />

                <asp:Panel ID="pnlClassDetail" runat="server" CssClass="panel panel-block">
                    <div class="panel-heading">
                        <h1 class="panel-title rollover-container"><asp:Literal ID="lClassName" runat="server" /></h1>
                        <asp:HyperLink ID="hlAnchor" runat="server" CssClass="text-color pull-left margin-l-sm"><i class="fa fa-link"></i></asp:HyperLink>
                        <p class='description'><asp:Literal ID="lClassDescription" runat="server" /></p>
                    </div>

                    <Rock:GridFilter ID="gfSettings" runat="server" OnApplyFilterClick="gfSettings_ApplyFilterClick" OnClearFilterClick="gfSettings_ClearFilterClick">
                        <Rock:RockDropDownList ID="ddlIsRequired" runat="server" Label="Is Required">
                            <asp:ListItem Value="" Text="" />
                            <asp:ListItem Value="True" Text="Yes" />
                            <asp:ListItem Value="False" Text="No" />
                        </Rock:RockDropDownList>

                        <Rock:RockDropDownList ID="ddlIsDatabase" runat="server" Label="Database Property">
                            <asp:ListItem Value="" Text="" />
                            <asp:ListItem Value="True" Text="Yes" />
                            <asp:ListItem Value="False" Text="No" />
                        </Rock:RockDropDownList>

                        <Rock:RockDropDownList ID="ddlIsLava" runat="server" Label="Lava Supported">
                            <asp:ListItem Value="" Text="" />
                            <asp:ListItem Value="True" Text="Yes" />
                            <asp:ListItem Value="False" Text="No" />
                        </Rock:RockDropDownList>
                    </Rock:GridFilter>
                    
                    <div class="panel-body">
                        <small class="pull-right">Show: 
                            <span class="js-model-inherited"><i class="js-model-check fa fa-fw fa-square-o"></i> inherited</span>
                        </small>

                        <asp:Literal ID="lClasses" runat="server" ViewStateMode="Disabled"></asp:Literal>
                    </div>
                </asp:Panel>
            </div>
        </div>

        <asp:Panel ID="pnlKey" runat="server" CssClass="well" Visible="false" >
   
                    <h4>Key</h4>
                    <div class="row">
                            <div class="col-xs-5 col-sm-3 col-md-1 text-center"><strong class="text-danger">*</strong></div>
                            <div class="col-xs-7 col-sm-9 col-md-10">A required field.</div>
                        </div>
                        <hr />
                        <div class="row">
                            <div class="col-xs-5 col-sm-3 col-md-1 text-center"><i class='fa fa-database fa-fw'></i></div>
                            <div class="col-xs-7 col-sm-9 col-md-10">A property on the database.</div>
                        </div>
                         <hr />
                        <div class="row">
                            <div class="col-xs-5 col-sm-3 col-md-1 text-center"><i class='fa fa-square-o fa-fw'></i></div>
                            <div class="col-xs-7 col-sm-9 col-md-10">Not mapped to the database.  These fields are computed and are only available in the object.</div>
                        </div>
                        <hr />
                        <div class="row">
                            <div class="col-xs-5 col-sm-3 col-md-1 text-center"><small><i class='fa fa-bolt fa-fw text-warning'></i></small></div>
                            <div class="col-xs-7 col-sm-9 col-md-10">These fields are available where Lava is supported.</div>
                        </div>
                        <hr />
                        <div class="row">
                            <div class="col-xs-5 col-sm-3 col-md-1 text-center"><small><i class='fa fa-ban fa-fw text-danger'></i></small></div>
                            <div class="col-xs-7 col-sm-9 col-md-10">These methods or fields are obsolete and should not be used anymore.</div>
                        </div>
            </asp:Panel>


        <script type="text/javascript">
            Sys.Application.add_load(function () {
                // Hide and unhide inherited properties and methods
                $('.js-model-inherited').on('click', function () {
                    $(this).find('i.js-model-check').toggleClass('fa-check-square-o fa-square-o');
                    $(this).closest('.panel-body').find('li.js-model').toggleClass('non-hidden hidden ');
                });

            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>

