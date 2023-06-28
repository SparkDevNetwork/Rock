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
                <div class="d-flex flex-wrap">
                    <asp:Literal ID="lName" runat="server" />
                    <a href="#!" class="btn btn-sm" onclick="$('.js-namePronunciationPanel').slideToggle()">
                        <span ID="spNamePronunciationIcon" runat="server" class="fa-stack fa-md text-primary">
                            <i class="fa fa-circle fa-stack-2x"></i>
                            <i class="fa fa-volume-up fa-stack-1x fa-inverse"></i>
                        </span>
                    </a>
                </div>
                <%-- Name Pronunciation Panel --%>
                <asp:HiddenField ID="hfNamePronunciationVisible" runat="server"/>
                <div id="divNamePronunciationPanel" runat="server" class="bg-default mt-1 p-1 rounded js-namePronunciationPanel" style="display:none">
                    <div runat="server" class="d-flex justify-content-between">
                        <a ID="aPlayAllNamePronunciations" runat="server" CssClass="text-primary"/>
                        <div runat="server" class="d-flex flex-wrap justify-content-left align-items-left ml-2 mr-2 mt-0" >
                            <span ID="spFirstNamePronunciation" runat="server" class="text-break"></span>
                            <span ID="spNickNamePronunciation" runat="server" class="text-break"></span>
                            <span ID="spLastNamePronunciation" runat="server" class="text-break"></span>
                        </div>
                        <asp:LinkButton ID="lbEditNamePronunciation" runat="server" OnClick="lbEditNamePronunciation_Click"/>
                    </div>
                    <div runat="server" class="d-flex flex-wrap justify-content-left align-items-center">
                        <asp:Label ID="lPronunciationNote" runat="server" />
                    </div>
                </div>
                <%-- Badges --%>
                <div class="d-flex flex-wrap justify-content-left align-items-center gap mt-3">
                    <Rock:BadgeListControl ID="blStatus" runat="server" />
                    <asp:LinkButton ID="lbFollowing" runat="server" CssClass="btn btn-default btn-xs btn-follow" OnClick="lbFollowing_Click"></asp:LinkButton>
                </div>
                <%-- Buttons --%>
                <div class="profile-actions" style="justify-content:left; padding-right:10px; padding-left:10px">
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
                    <asp:Literal ID="lRaceAndEthnicity" runat="server" />
                    <asp:Literal ID="lAge" runat="server" />
                    <asp:Literal ID="lMaritalStatus" runat="server" />
                    <asp:Literal ID="lGrade" runat="server" />
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
                    <a href='<%# Eval("url") %>' class='text-link p-2' target="_blank" rel="noopener noreferrer">
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
        <%-- Name Pronunciation Modal --%>
        <Rock:ModalDialog ID="dlgNamePronunciation" runat="server" Title="Pronunciation Override" OnSaveClick="dlgSavePronunciation_Click" ValidationGroup="PageAttributes" Visible="false">
            <Content>
                <div class="block-content">
                    <span>You can override the pronunciation for this individual below. If you have any feedback regarding the system's pronunciation, please don't hesitate to </span>
                    <a id="formLink" runat="server" href="https://community.rockrms.com">share it with us.</a>
                </div>
                <div class="margin-t-md">
                    <label class="control-label" for="tbFirstName">First Name</label>
                    <div class="control-wrapper">
                        <input id="tbFirstName" runat="server" type="text" value="" maxlength="200" class="form-control mb-3">
                        <span id="tbFirstName_rfv" class="validation-error help-inline" style="display:none"></span>
                    </div>
                    <label class="control-label" for="tbNickName">Nick Name</label>
                    <div class="control-wrapper">
                        <input id="tbNickName" runat="server" name="tbNickName" type="text" value="" maxlength="200" class="form-control mb-3">
                        <span id="tbFNickName_rfv" class="validation-error help-inline" style="display:none"></span>
                    </div>
                    <label class="control-label" for="tbLastName">Last Name</label>
                    <div class="control-wrapper">
                        <input id="tbLastName" runat="server" name="tbLastName" type="text" value="" maxlength="200" class="form-control mb-3">
                        <span id="tbLastName_rfv" class="validation-error help-inline" style="display:none"></span>
                    </div>
                    <label class="control-label" for="tbPronunciationNote">Pronunciation Notes</label>
                    <div class="control-wrapper">
                        <textarea id="tbPronunciationNote" runat="server" name="tbPronunciationNote" type="text" value="" maxlength="1000" class="form-control"></textarea>
                        <span id="tbPronunciationNote_rfv" class="validation-error help-inline" style="display:none"></span>
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>



