<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Bio.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.Bio" %>

<asp:UpdatePanel ID="pnlContent" runat="server">
    <ContentTemplate>

        <div id="divBio" runat="server" class="card card-profile card-profile-bio">

            <div id="profile-image" class="img-card-top profile-squish">
                <asp:Literal ID="lImage" runat="server" />
            </div>

            <%-- Name and actions --%>
            <div class="card-section position-relative">

                <%-- Account Protection Level --%>
                <asp:Literal ID="litAccountProtectionLevel" runat="server" />

                <%-- Person Name --%>
                <asp:Literal ID="lName" runat="server" />

                <%-- Badges --%>
                <div class="d-flex flex-wrap justify-content-center align-items-center mt-2">

                    <Rock:BadgeListControl ID="blStatus" runat="server" />
                    <asp:Panel ID="pnlFollowing" runat="server" >
                        <asp:LinkButton ID="lbFollowing" runat="server" class="btn btn-default btn-xs btn-follow m-1" OnClick="lbFollowing_Click"></asp:LinkButton>
                    </asp:Panel>
                    
                </div>
                
                <%-- Buttons --%>
                <div class="expand-section d-flex flex-row flex-wrap justify-content-center mt-3">

                    <div id="divSmsButton" runat="server" class="action-container" title="Send a SMS">
                        <asp:LinkButton ID="lbSendText" runat="server" class="btn btn-default btn-go btn-square " OnClick="lbSendText_Click"><i class="fa fa-comment-alt"></i></asp:LinkButton>
                        <div><span>Text</span></div>
                    </div>

                    <div id="divEmailButton" runat="server" class="action-container" title="Send an email">
                        <asp:LinkButton ID="lbSendEmail" runat="server" class="btn btn-default btn-go btn-square "><i class="fa fa-envelope"></i></asp:LinkButton>
                        <div><span>Email</span></div>
                    </div>

                    <div class="action-container" title="">
                        <a class="dropdown-toggle btn btn-default btn-go btn-square" data-toggle="dropdown" href="#" tabindex="0">
                            <i class="fa fa-bolt"></i>
                        </a>
                        <ul class="dropdown-menu dropdown-menu-right">
                            <li><asp:LinkButton ID="lbImpersonate" runat="server" Visible="false" OnClick="lbImpersonate_Click"><i class='fa-fw fa fa-unlock'></i>&nbsp;Impersonate</asp:LinkButton></li>
                            <li><asp:HyperLink ID="hlVCard" runat="server"><i class='fa fa-address-card'></i>&nbsp;Download vCard</asp:HyperLink></li>
                            <asp:Literal ID="lActions" runat="server" />
                        </ul>
                        <div><span>Actions</span></div>
                    </div>

                    <div class="action-container" title="Edit Person">
                        <asp:LinkButton ID="lbEditPerson" runat="server" AccessKey="I" ToolTip="Alt+I" CssClass="btn btn-default btn-go btn-square " OnClick="lbEditPerson_Click"><i class="fa fa-pencil"></i></asp:LinkButton>
                        <div><span>Edit</span></div>
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
            <div class="card-section">
                <div class="expand-section">

                    <asp:Repeater ID="rptPhones" runat="server" OnItemDataBound="rptPhones_ItemDataBound">
                        <ItemTemplate>
                            <asp:Literal ID="litPhoneNumber" runat="server"></asp:Literal>
                        </ItemTemplate>
                    </asp:Repeater>
                    
                    <div class="email">
                        <asp:Literal ID="lEmail" runat="server" />
                    </div>
                </div>
            </div>

            <%-- Social Media Accounts --%>
            <div class="card-section py-0">
                <div class="d-flex flex-wrap justify-content-center">

                    <asp:Repeater ID="rptSocial" runat="server">
                        <ItemTemplate>
                            <a href='<%# Eval("url") %>' class='text-link p-2' target="_blank">
                                <i class='<%# Eval("icon") %>'></i>
                            </a>
                        </ItemTemplate>
                    </asp:Repeater>
                    
                
                </div>
            </div>

            <%-- Custom Content --%>
            <div id="divCustomContent" runat="server" class="card-section">
                <div class="col-sm-9 col-md-10 text-center sm-text-left">
                    <asp:PlaceHolder ID="phCustomContent" runat="server" />
                </div>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>


