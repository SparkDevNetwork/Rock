<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonalLinks.ascx.cs" Inherits="RockWeb.Blocks.Cms.PersonalLinks" %>

<%@ Import Namespace="Rock" %>
<style>
    .rock-top-header .navbar-zone-header .zone-content > .block-instance {
        display: block !important;
    }

    .rock-top-header .smartsearch {
        width: calc(100% - 210px);
    }

    @media (min-width: 768px) {
        .rock-top-header .smartsearch {
            width: 340px;
        }
    }

    .rock-bookmark {
        display: inline-block;
        color: #fff !important;
        font-size: 20px;
        padding: 10px 20px 10px 10px;
        margin: 15px 2px 15px 0;
        opacity: .75;
    }

        .rock-bookmark:hover,
        .rock-bookmark:active {
            color: #fff;
            opacity: .5;
        }

    .rock-popover {
        display: flex !important;
        flex-wrap: wrap;
        width: 530px;
        max-width: 100%;
        font-size: 14px;
    }

    @media (min-width: 480px) {
        .rock-popover {
            flex-wrap: nowrap;
            max-height: 350px;
        }
    }

    .rock-popover .popover-title {
        min-height: 40px;
    }

    .rock-popover .dropdown-menu a {
        font-size: 14px;
        line-height: 24px;
    }

    .rock-popover > .popover-panel:first-child {
        border-right: 1px solid #eee;
    }

    .rock-popover .popover-panel {
        display: flex;
        flex-direction: column;
        width: 100%;
    }

    @media (min-width: 480px) {
        .rock-popover .popover-panel {
            width: 50%;
        }
    }

    .popover-panel > .popover-content {
        overflow: auto;
    }
</style>

<asp:UpdatePanel ID="upnlContent" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <script type="text/javascript">
            function Show() {
                //javascript:window.open("http://www.microsoft.com");
                $('#<%= quickreturns.ClientID %>').toggleClass('d-none');

                var quickReturnsByType = personalLinks.getQuickReturns();
                var quickReturnHtml = '';
                if (quickReturnsByType.length > 0) {
                    for (var i = 0; i < quickReturnsByType.length; i++) {
                        if (quickReturnsByType[i].items.length > 0) {
                            var itemsHtml = '<li><strong>' + quickReturnsByType[i].type + '</strong></li>';
                            for (var item in quickReturnsByType[i].items) {
                                itemsHtml += ' <li><a href="' + quickReturnsByType[i].items[item].url + '">' + quickReturnsByType[i].items[item].itemName + '</a></li>';
                            }
                            quickReturnHtml += '<ul class="list-unstyled">' + itemsHtml + '</ul>';
                        }
                    }
                }

                $('#divQuickReturn').html(quickReturnHtml);
            }

            Sys.Application.add_load(function () {
                $(".js-bookmark").on("click", function (a) {
                    //$('#quick-returns').toggleClass('d-none');
                });
            })

            // Hide when clicking outside.
            $(document).mouseup(function (e) {
                var container = $('#<%= upnlContent.ClientID %>');

                // if the target of the click isn't the container or a descendant of the container
                if (!container.is(e.target) && container.has(e.target).length === 0) {
                    //container.hide();
                    $('#<%= quickreturns.ClientID %>').addClass('d-none');
                }
            });
        </script>

        <asp:LinkButton runat="server" ID="lbBookmark" Visible="false" class="rock-bookmark pull-right js-bookmark"
            href="#" OnClientClick="Show()"><i class="fa fa-bookmark"></i></asp:LinkButton>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div runat="server" id="quickreturns" class="d-none" style="width: 500px;">

                <div class="popover rock-popover styled-scroll" role="tooltip">
                    <div class="popover-panel">
                        <h3 class="popover-title">Personal Links

                            <div class="dropdown pull-right">
                                <a class="btn btn-xs py-0 px-1 text-muted" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false" role="button"><i class="fa fa-ellipsis-v"></i></a>
                                <ul class="dropdown-menu">
                                    <li><asp:LinkButton runat="server" ID="lbManageLinks" OnClick="lbManageLinks_Click">Manage Links</asp:LinkButton></li>
                                </ul>
                            </div>
                            <div class="dropdown pull-right">
                                <a class="btn btn-xs py-0 px-1 text-muted" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false" role="button"><i class="fa fa-plus"></i></a>
                                <ul class="dropdown-menu">
                                    <li><asp:LinkButton runat="server" ID="lbAddLink" OnClick="lbAddLink_Click">Add Link</asp:LinkButton></li>
                                    <li><asp:LinkButton runat="server" ID="lbAddSection" OnClick="lbAddSection_Click">Add Section</asp:LinkButton></li>
                                </ul>
                            </div>
                        </h3>
                        <div class="popover-content">
                            <asp:Repeater ID="rptPersonalLinkSection" runat="server" OnItemDataBound="rptPersonalLinkSection_ItemDataBound">
                                <ItemTemplate>
                                    <ul class="list-unstyled">
                                        <li><strong><%# (bool)Eval("IsShared") ? "<i class='fa fa-users'></i>" : string.Empty %> <%# ((string)Eval( "Name" )).FixCase() %></strong></li>
                                        <asp:Repeater ID="rptLinks" runat="server">
                                            <ItemTemplate>
                                                <li><a href="<%#Eval("Url")%>"><%#((string)Eval( "Name" )).FixCase()%></a></li>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                    </ul>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                    </div>
                    <div class="popover-panel">
                        <h3 class="popover-title">Quick Returns</h3>
                        <div id="divQuickReturn" class="popover-content">
                            <ul class="list-unstyled">
                                <li><strong>People</strong></li>
                                <li><a href="#">Ted Decker</a></li>
                                <li><a href="#">Bill Marble</a></li>
                                <li><a href="#">Jenny Michaels</a></li>
                                <li><a href="#">Alisha Marble</a></li>
                                <li><a href="#">Pete Foster</a></li>
                            </ul>
                            <ul class="list-unstyled">
                                <li><strong>Groups</strong></li>
                                <li><a href="#">Usher’s Group</a></li>
                                <li><a href="#">Greeter’s Group</a></li>
                                <li><a href="#">Ted Decker Small Group</a></li>
                                <li><a href="#">Volunteer On-Boarding</a></li>
                            </ul>
                            <ul class="list-unstyled">
                                <li><strong>Data Views</strong></li>
                                <li><a href="#">Foundational Dataviews</a></li>
                                <li><a href="#">Active Adults</a></li>
                                <li><a href="#">Large Givers</a></li>
                                <li><a href="#">Campus Accounts</a></li>
                            </ul>
                            <ul class="list-unstyled">
                                <li><strong>Reports</strong></li>
                                <li><a href="#">All Volunteers</a></li>
                                <li><a href="#">All Volunteers</a></li>
                                <li><a href="#">All Volunteers</a></li>
                                <li><a href="#">All Volunteers</a></li>
                            </ul>
                        </div>
                    </div>
                    <div class="tooltip-inner d-none"></div>
                </div>

            </div>

        </asp:Panel>

        <asp:Panel ID="pnlAddSection" runat="server" CssClass="panel panel-block" Visible="false">
            <div style="width: 500px;">

                <div class="popover rock-popover styled-scroll" role="tooltip">

                    <div class="popover-panel" style="width: 100%">
                        <h3 class="popover-title">Personal Link Section</h3>
                        <div class="panel-body">
                            <fieldset>
                                <Rock:DataTextBox ID="tbSectionName" runat="server" SourceTypeName="Rock.Model.PersonalLinkSection, Rock" PropertyName="Name" ValidationGroup="vgAddSection" />
                            </fieldset>
                            <div class="actions">
                                <asp:LinkButton ID="btnSectionSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSectionSave_Click" ValidationGroup="vgAddSection" />
                                <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                            </div>
                        </div>
                    </div>
        </asp:Panel>
        <asp:Panel ID="pnlAddLink" runat="server" CssClass="panel panel-block" Visible="false">
            <div style="width: 500px;">

                <div class="popover rock-popover styled-scroll" role="tooltip">

                    <div class="popover-panel" style="width: 100%">
                        <h3 class="popover-title">Personal Link</h3>
                        <div class="panel-body">
                            <fieldset>
                                <Rock:DataTextBox ID="tbLinkName" runat="server" SourceTypeName="Rock.Model.PersonalLink, Rock" PropertyName="Name" ValidationGroup="vgAddLink" />
                                <Rock:UrlLinkBox ID="urlLink" runat="server" Label="Link Url" ValidationGroup="vgAddLink" Required="true" />
                                <Rock:RockDropDownList ID="ddlSection" runat="server" ValidationGroup="vgAddLink" Label="Section" />
                            </fieldset>

                            <div class="actions">
                                <asp:LinkButton ID="btnLinkSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnLinkSave_Click" ValidationGroup="vgAddLink" />
                                <asp:LinkButton ID="btnLinkCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                            </div>
                        </div>
                    </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
