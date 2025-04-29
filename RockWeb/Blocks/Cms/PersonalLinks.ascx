<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonalLinks.ascx.cs" Inherits="RockWeb.Blocks.Cms.PersonalLinks" %>
<%@ Import Namespace="Rock" %>

<script type="text/javascript">
    Sys.Application.add_load(function () {
        var clientId = $('#<%= upnlContent.ClientID %>');
        Rock.personalLinks.initialize(clientId);
    })
</script>

<asp:LinkButton runat="server" ID="lbBookmark" Visible="false" class="rock-bookmark js-rock-bookmark"><i class="fa fa-bookmark"></i></asp:LinkButton>


<asp:UpdatePanel ID="upnlContent" runat="server" UpdateMode="Conditional" class="popover rock-popover styled-scroll js-bookmark-panel js-personal-link-popover position-fixed d-none" role="tooltip">
    <ContentTemplate>
        <%-- Html for Bookmark button and link containers --%>
        <asp:Panel ID="pnlView" runat="server" CssClass="rock-popover js-personal-links-container">
            <div class="popover-panel">
                <h3 class="popover-title">Personal Links

                    <div class="ml-auto">
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
                    </div>
                </h3>
                <div class="popover-content">

                    <%-- Container for personalLinks.js to put the personalLinks links html into --%>
                    <div id="divPersonalLinks" runat="server" class="js-personal-links-links" data-last-shared-link-update-utc="01-01-1900"></div>
                </div>
            </div>
            <div class="popover-panel">
                <h3 class="popover-title">Quick Returns</h3>

                <%-- Container for personalLinks.js to put the quickReturns html into --%>
                <div id="divQuickReturn" class="popover-content js-personal-links-quick-return"></div>
            </div>
        </asp:Panel>

        <%-- Configuration panel for Adding Personal (non-shared) Section --%>
        <asp:Panel ID="pnlAddSection" runat="server" CssClass="popover-panel w-100 js-bookmark-configuration js-bookmark-add-section" Visible="false">
            <h3 class="popover-title">Personal Link Section</h3>
            <div class="panel-body">
                <fieldset>
                    <Rock:DataTextBox ID="tbSectionName" runat="server" SourceTypeName="Rock.Model.PersonalLinkSection, Rock" PropertyName="Name" ValidationGroup="vgAddSection" />
                </fieldset>
                <div class="actions">
                    <asp:LinkButton ID="btnSectionSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary btn-xs js-rebuild-links" OnClick="btnSectionSave_Click" ValidationGroup="vgAddSection" />
                    <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link btn-xs js-rebuild-links" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>
            </div>
        </asp:Panel>

        <%-- Configuration panel for adding Personal (non-shared) Links --%>
        <asp:Panel ID="pnlAddLink" runat="server" CssClass="popover-panel w-100 js-bookmark-configuration js-bookmark-add-link" Visible="false">
            <h3 class="popover-title">Personal Link</h3>
            <div class="panel-body">
                <fieldset>
                    <Rock:DataTextBox ID="tbLinkName" runat="server" SourceTypeName="Rock.Model.PersonalLink, Rock" PropertyName="Name" ValidationGroup="vgAddLink" />
                    <Rock:UrlLinkBox ID="urlLink" runat="server" Label="Link URL" ValidationGroup="vgAddLink" Required="true" />
                    <Rock:RockDropDownList ID="ddlSection" runat="server" ValidationGroup="vgAddLink" Label="Section" />
                </fieldset>

                <div class="actions">
                    <asp:LinkButton ID="btnLinkSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary btn-xs js-rebuild-links" OnClick="btnLinkSave_Click" ValidationGroup="vgAddLink" />
                    <asp:LinkButton ID="btnLinkCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link btn-xs js-rebuild-links" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
