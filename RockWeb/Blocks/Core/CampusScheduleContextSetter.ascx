<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CampusScheduleContextSetter.ascx.cs" Inherits="RockWeb.Blocks.Core.CampusScheduleContextSetter" %>

<style>
    .setter-state {
        padding: 5px 15px;
    }

    .setter-toggle {
        padding-top: 15px; 
        padding-bottom: 15px;
    }
</style>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <div class="setter js-setter">
            <div class="setter-content pull-left">
                <div class="setter-state js-setter-state">
                    <asp:Literal ID="lCurrentSelections" runat="server" /> 
                </div>
            
                <div class="setter-options js-setter-options" style="display: none;">
                    <!-- campus picker -->
                    <ul class="nav navbar-nav contextsetter contextsetter-campus">
                        <li class="dropdown">

                            <a class="dropdown-toggle navbar-link" href="#" data-toggle="dropdown">
                                <asp:Literal ID="lCurrentCampusSelection" runat="server" />
                                <b class="fa fa-caret-down"></b>
                            </a>

                            <ul class="dropdown-menu">
                                <asp:Repeater runat="server" ID="rptCampuses" OnItemCommand="rptCampuses_ItemCommand">
                                    <ItemTemplate>
                                        <li>
                                            <asp:LinkButton ID="btnCampus" runat="server" Text='<%# Eval("Name") %>' CommandArgument='<%# Eval("Id") %>' />
                                        </li>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </ul>
                        </li>
                    </ul>

                    <!-- schedule picker -->
                    <ul class="nav navbar-nav contextsetter contextsetter-schedule">
                        <li class="dropdown">

                            <a class="dropdown-toggle navbar-link" href="#" data-toggle="dropdown">
                                <asp:Literal ID="lCurrentScheduleSelection" runat="server" />
                                <b class="fa fa-caret-down"></b>
                            </a>

                            <ul class="dropdown-menu">
                                <asp:Repeater runat="server" ID="rptSchedules" OnItemCommand="rptSchedules_ItemCommand">
                                    <ItemTemplate>
                                        <li>
                                            <asp:LinkButton ID="btnSchedule" runat="server" Text='<%# Eval("Name") %>' CommandArgument='<%# Eval("Id") %>' />
                                        </li>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </ul>
                        </li>
                    </ul>
                </div>
            </div>

            <div class="setter-toggle pull-right">
                <i class="fa fa-chevron-down cursor-pointer margin-h-sm js-setter-show-options"></i>
            </div>
        </div>

        <script>
            Sys.Application.add_load( function () {
                $('.js-setter-show-options, .js-setter-state').on('click', function () {

                    var toggleControl = $(this).parents('.js-setter').find('.js-setter-show-options');

                    $(this).parents('.js-setter').find('.js-setter-options').toggle();
                    $(this).parents('.js-setter').find('.js-setter-state').toggle();
                    toggleControl.toggleClass('fa-chevron-up fa-chevron-down');
                });
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>