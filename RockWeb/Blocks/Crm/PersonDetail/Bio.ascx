<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Bio.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.Bio" %>

<asp:UpdatePanel ID="pnlContent" runat="server">
    <ContentTemplate>
        <script>
            $(function () {
                $("#profile-image a").fluidbox({ rescale: true });
            });
        </script>

        <div id="profile-image" class="img-card-top profile-squish">
            <div class="fluid-crop">
                <a href="#" class="fluidbox fluidbox-closed">
                    <asp:Literal ID="lImage" runat="server" />
                </a>
            </div>
        </div>

        <div class="bio-data">
            <%-- Name and actions --%>
            <div class="card-section position-relative">
                <%-- Account Protection Level --%>
                <asp:Literal ID="litAccountProtectionLevel" runat="server" />
                <%-- Person Name --%>
                <asp:Literal ID="lName" runat="server" />
                <%-- Badges --%>
                <div class="d-flex flex-wrap justify-content-center align-items-center gap mt-3">
                    <Rock:BadgeListControl ID="blStatus" runat="server" />
                    <asp:LinkButton ID="lbFollowing" runat="server" CssClass="btn btn-default btn-xs btn-follow" OnClick="lbFollowing_Click"></asp:LinkButton>
                </div>
                <%-- Buttons --%>
                <div class="profile-actions">
                    <div id="divSmsButton" runat="server" class="action-container">
                        <asp:Literal ID="lSmsButton" runat="server" />
                    </div>
                    <div id="divEmailButton" runat="server" class="action-container">
                        <asp:Literal ID="lEmailButton" runat="server" />
                    </div>
                    <div class="action-container">
                        <button type="button" class="dropdown-toggle btn btn-default btn-go btn-square stretched-link" data-toggle="dropdown" title="Actions" aria-label="Actions">
                            <i class="fa fa-bolt"></i>
                        </button>
                        <ul class="dropdown-menu">
                            <li><asp:LinkButton ID="lbImpersonate" runat="server" Visible="false" OnClick="lbImpersonate_Click"><i class='fa-fw fa fa-unlock'></i>&nbsp;Impersonate</asp:LinkButton></li>
                            <li><asp:HyperLink ID="hlVCard" runat="server"><i class='fa fa-address-card'></i>&nbsp;Download vCard</asp:HyperLink></li>
                            <asp:Literal ID="lActions" runat="server" />
                        </ul>
                        <span>Actions</span>
                    </div>
                    <div id="divEditButton" runat="server" class="action-container">
                        <asp:LinkButton ID="lbEditPerson" runat="server" AccessKey="I" ToolTip="Alt+I" CssClass="btn btn-default btn-go btn-square stretched-link" OnClick="lbEditPerson_Click" aria-label="Edit Person"><i class="fa fa-pencil"></i></asp:LinkButton>
                        <span>Edit</span>
                    </div>
                </div>
            </div>
            <%-- Demographic info --%>
            <div class="card-section">
                <dl class="reversed-label">
                    <asp:Literal ID="lGender" runat="server" />
                    <asp:Literal ID="lAge" runat="server" />
                    <asp:Literal ID="lMaritalStatus" runat="server" />
                    <asp:Literal ID="lGraduation" runat="server" />
                </dl>
            </div>
            <%-- Phone Numbers. Email also in this section. --%>
            <div ID="divContactSection" runat="server" class="card-section">
                <asp:Repeater ID="rptPhones" runat="server" OnItemDataBound="rptPhones_ItemDataBound">
                    <HeaderTemplate>
                        <div class="expand-section">
                    </HeaderTemplate>
                    <ItemTemplate>
                        <asp:Literal ID="litPhoneNumber" runat="server"></asp:Literal>
                    </ItemTemplate>
                    <FooterTemplate>
                        </div>
                    </FooterTemplate>
                </asp:Repeater>
                <asp:Literal ID="lEmail" runat="server" />
            </div>
            <%-- Social Media Accounts --%>
            <asp:Repeater ID="rptSocial" runat="server">
                <HeaderTemplate>
                    <div class="card-section py-0">
                        <div class="d-flex flex-wrap justify-content-center">
                </HeaderTemplate>
                <ItemTemplate>
                    <a href='<%# Eval("url") %>' class='text-link p-2' target="_blank">
                        <i class='<%# Eval("icon") %>'></i>
                    </a>
                </ItemTemplate>
                <FooterTemplate>
                    </div>
                </div>
                </FooterTemplate>
            </asp:Repeater>
            <%-- Custom Content --%>
            <div id="divCustomContent" runat="server" class="card-section">
                <div class="col-sm-9 col-md-10 text-center sm-text-left">
                    <asp:PlaceHolder ID="phCustomContent" runat="server" />
                </div>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>


