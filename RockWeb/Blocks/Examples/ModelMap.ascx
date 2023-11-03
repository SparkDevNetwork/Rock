<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ModelMap.ascx.cs" Inherits="RockWeb.Blocks.Examples.ModelMap" %>
<%@ Import namespace="Rock" %>
<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfSelectedCategoryGuid" runat="server" />
        <asp:HiddenField ID="hfSelectedEntityId" runat="server" />
        <style>
        @media (max-width: 767px){
            .table-properties > tbody > tr > td {
                border: 0;
            }

            .table-properties > tbody > tr > td:empty {
                display: none !important;
            }

            .table-properties > tbody > tr {
                border-top: 1px solid #dbdbdb;
            }
        }
        </style>
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
                                        <h3><%# Eval("Name").ToString().SplitCase() %> </h3>
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

                <asp:Panel ID="pnlKey" runat="server" CssClass="well" Visible="false" >
                    <h5 class="mt-0">Key</h5>
                    <table class="table table-condensed">
                        <tr>
                            <td class="w-0 text-center"><span class="required-indicator"></span></td>
                            <td>A required field.</td>
                        </tr>
                        <tr>
                            <td class="w-0 text-center"><i class='fa fa-database fa-fw'></i></td>
                            <td>A property on the database.</td>
                        </tr>
                        <tr>
                            <td class="w-0 text-center"><i class='fa fa-square fa-fw o-20'></i></td>
                            <td>Not mapped to the database.  These fields are computed and are only available in the object.</td>
                        </tr>
                        <tr>
                            <td class="w-0 text-center"><small><i class='fa fa-bolt fa-fw text-warning'></i></small></td>
                            <td>These fields are available where Lava is supported.</td>
                        </tr>
                        <tr>
                            <td class="text-center"><small><i class='fa fa-ban fa-fw text-danger'></i></small></td>
                            <td>These methods or fields are obsolete and should not be used.</td>
                        </tr>
                    </table>
                </asp:Panel>
            </div>
            <div class="col-md-8">

                <Rock:NotificationBox ID="nbClassesWarning" runat="server" NotificationBoxType="Warning" Visible="false" />

                <asp:Panel ID="pnlClassDetail" runat="server" CssClass="panel panel-block">
                    <div class="panel-heading">
                        <h1 class="panel-title">Model Details</h1>
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


                        <div class="clearfix">
                            <small class="pull-right">Show:
                                <span class="js-model-inherited"><i class="js-model-check fa fa-fw fa-square-o"></i> Methods</span>
                            </small>
                        </div>

                        <div id="divClass" runat="server">
                            <h4 class="font-weight-medium rollover-container border-bottom border-gray-400 pb-1 mb-2"><asp:Literal ID="lClassName" runat="server" /> <asp:Literal ID="lActualTableName" runat="server" /> <asp:HyperLink ID="hlAnchor" runat="server" CssClass="text-color margin-l-sm small rollover-item"><i class="fa fa-xs fa-link"></i></asp:HyperLink></h4>
                            <asp:Literal ID="lClassDescription" runat="server" />
                            <asp:Literal ID="lClassExample" runat="server" />
                        </div>

                        <asp:Literal ID="lClasses" runat="server" ViewStateMode="Disabled"></asp:Literal>
                    </div>
                </asp:Panel>
            </div>
        </div>


        <script type="text/javascript">
            Sys.Application.add_load(function () {
                // Hide and unhide inherited properties and methods
                $('.js-model-inherited').on('click', function () {
                    $(this).find('i.js-model-check').toggleClass('fa-check-square-o fa-square-o');
                    $(this).closest('.panel-body').find('h4.js-model').toggleClass('visible hidden');
                    $(this).closest('.panel-body').find('li.js-model').toggleClass('visible hidden');
                });

                $('.js-show-values').on('click', function () {
                    var valueTable = $(this).next('.js-value-table');
                    var txt = $(valueTable).is(':visible') ? 'Show Values' : 'Hide Values';
                    $(this).find('span').text(txt);
                    $(this).find('i').toggleClass('fa-chevron-down fa-chevron-up');
                    $(valueTable).slideToggle();
                });
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>

