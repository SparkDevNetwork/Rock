<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AttendanceSelfEntry.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.AttendanceSelfEntry" %>
<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>
        <div class="panel panel-block">
            <div class="panel-body">
                <asp:Panel runat="server" CssClass="js-navigation-panel" ID="pnlPrimaryWatcher" Visible="false">
                    <h2><asp:Literal ID="lPanel1Title" runat="server" /></h2>
                    <asp:Literal ID="lPanel1Text" runat="server" />
                    <asp:ValidationSummary ID="valValidation" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="vgPrimary" />
                    <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" Title="Warning" Visible="false" />
                    <div class="row">
                        <div class="col-md-6">
                            <fieldset>
                                <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" Required="true" ValidationGroup="vgPrimary" />
                                <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" Required="false" ValidationGroup="vgPrimary" />
                                <Rock:EmailBox ID="tbEmail" runat="server" Label="Email" Required="false" ValidationGroup="vgPrimary" />
                                <Rock:BirthdayPicker ID="bpBirthDay" runat="server" Label="Birthday" ValidationGroup="vgPrimary" />
                                <asp:Panel ID="pnlPhone" runat="server" CssClass="margin-b-lg">
                                    <Rock:PhoneNumberBox ID="pnbPhone" runat="server" Label="Mobile Phone" ValidationGroup="vgPrimary" />
                                    <Rock:RockCheckBox ID="cbIsMessagingEnabled" runat="server" Text="May we message you on occassion" DisplayInline="true" ValidationGroup="vgPrimary" />
                                </asp:Panel>
                                <Rock:AddressControl ID="acAddress" runat="server" Label="Address" RequiredErrorMessage="Your Address is Required" ValidationGroup="vgPrimary" />
                            </fieldset>


                        </div>
                        <asp:Panel ID="pnlLogin" runat="server" CssClass="col-md-6">
                            <p>
                                Already have an account on our website?
                                <br />
                                Login Instead.
                            </p>
                            <asp:LinkButton ID="btnLogin" CssClass="btn btn-default" runat="server" Text="Login" OnClick="btnLogin_Click" />
                        </asp:Panel>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnPrimaryNext" runat="server" AccessKey="s" ToolTip="Alt+n" Text="Next" CssClass="btn btn-primary pull-right" OnClick="btnPrimaryNext_Click" ValidationGroup="vgPrimary" />
                    </div>
                </asp:Panel>
                <asp:Panel runat="server" ID="pnlOtherWatcher" Visible="false" CssClass="js-navigation-panel">
                    <h2><asp:Literal ID="lPanel2Title" runat="server" /></h2>
                    <asp:Literal ID="lPanel2Text" runat="server" />
                    <asp:ValidationSummary ID="valOtherWatcher" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="vgOther" />
                    <Rock:NotificationBox ID="nbOtherWarning" runat="server" NotificationBoxType="Warning" Title="Warning" Visible="false" />
                    <div class="row">
                        <div class="col-md-6">
                            <fieldset>
                                <Rock:RockDropDownList ID="ddlRelation" runat="server" Label="Relation" ValidationGroup="vgOther" OnSelectedIndexChanged="ddlRelation_SelectedIndexChanged" AutoPostBack="true"/>
                                <Rock:RockTextBox ID="tbOtherFirstName" runat="server" Label="First Name" Required="true" ValidationGroup="vgOther" />
                                <Rock:RockTextBox ID="tbOtherLastName" runat="server" Label="Last Name" Required="false" ValidationGroup="vgOther" />
                                <Rock:EmailBox ID="tbOtherEmail" runat="server" Label="Email" Required="false" ValidationGroup="vgOther" />
                                <Rock:BirthdayPicker ID="bpOtherBirthDay" runat="server" Label="Birthday" ValidationGroup="vgOther" />
                                <asp:Panel ID="pnlOtherPhone" runat="server" CssClass="margin-b-lg">
                                    <Rock:PhoneNumberBox ID="pnOtherMobile" runat="server" Label="Mobile Phone" ValidationGroup="vgOther" />
                                    <Rock:RockCheckBox ID="cbOtherMessagingEnabled" runat="server" Text="May we message you on occassion" DisplayInline="true" ValidationGroup="vgOther" />
                                </asp:Panel>
                                <asp:LinkButton ID="lbAddIndividual" runat="server" CssClass="btn btn-default btn-sm" ValidationGroup="vgOther" OnClick="lbAddIndividual_Click"> Add Additional Individual</asp:LinkButton>
                            </fieldset>
                        </div>
                        <div class="col-md-4 col-md-offset-1 padding-all-xl">
                            <Rock:RockControlWrapper ID="rcwListed" runat="server" Label="Currently Listed">
                                <hr class="margin-t-sm" />
                                <asp:Repeater ID="rptListed" runat="server" OnItemCommand="rptListed_ItemCommand">
                                    <ItemTemplate>
                                        <div class="form-inline rollover-container margin-b-sm">
                                            <asp:HiddenField ID="hfRowId" runat="server" Value='<%# Eval("Guid") %>' />
                                            <b><%# Eval("Name") %></b> <span><%#Eval("Relationship") %></span>
                                            <div class="rollover-item actions pull-right">
                                                <asp:LinkButton ID="lbDelete" runat="server" CommandName="delete" CommandArgument='<%# Eval("Guid") %>'><i class="fa fa-times"></i></asp:LinkButton>
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </Rock:RockControlWrapper>
                        </div>
                    </div>
                    <div class="actions">
                        <asp:LinkButton ID="btnOtherNext" runat="server" AccessKey="s" ToolTip="Alt+n" Text="Next" CssClass="btn btn-primary pull-right" OnClick="btnOtherNext_Click" CausesValidation="false" />
                    </div>
                </asp:Panel>
                <asp:Panel ID="pnlAccount" runat="server" Visible="false" CssClass="js-navigation-panel">
                    <h2><asp:Literal ID="lPanel3Title" runat="server" /></h2>
                    <asp:Literal ID="lPanel3Text" runat="server" />
                    <Rock:NotificationBox ID="nbAccountWarning" runat="server" NotificationBoxType="Warning" Title="Warning" Visible="false" />
                    <Rock:RockTextBox ID="txtUserName" runat="server" Label="Username" CssClass="input-medium" />
                    <Rock:RockTextBox ID="txtPassword" runat="server" Label="Password" CssClass="input-medium" TextMode="Password" FormGroupCssClass="margin-b-none" />
                    <div class="row margin-b-md">
                        <div class="col-md-12">
                            <a class="btn btn-link btn-xs pull-right js-show-password" href="#">Show Password</a>
                        </div>
                    </div>
                    <div class="actions">
                        <asp:LinkButton ID="btnAccountNext" runat="server" AccessKey="s" ToolTip="Alt+n" Text="Next" CssClass="btn btn-primary pull-right" OnClick="btnAccountNext_Click" />
                    </div>

                </asp:Panel>
                <asp:Panel ID="pnlKnownIndividual" runat="server" Visible="false" CssClass="js-navigation-panel">
                    <h2><asp:Literal ID="lKnownIndividualTitle" runat="server" /></h2>
                    <asp:Literal ID="lKnownIndividualText" runat="server" />
                    <Rock:RockCheckBoxList ID="cblIndividuals" runat="server" RepeatDirection="Horizontal" Required="true" ValidationGroup="vgCheckIn"></Rock:RockCheckBoxList>
                    <asp:LinkButton ID="lbAdjustWatchers" runat="server" Text="Adjust Remembered Watchers" CssClass="btn btn-link" OnClick="lbAdjustWatchers_Click" CausesValidation="false" />
                    <div class="actions">
                        <asp:LinkButton ID="btnCheckIn" runat="server" AccessKey="s" ToolTip="Alt+n" Text="Check-In" ValidationGroup="vgCheckIn" CssClass="btn btn-primary" OnClick="btnCheckIn_Click" />
                    </div>
                </asp:Panel>
                <asp:Panel ID="pnlThanks" runat="server" Visible="false" CssClass="js-navigation-panel">
                    <h2><asp:Literal ID="lSuccessTitle" runat="server" /></h2>
                    <asp:Literal ID="lSuccessText" runat="server" />
                    <asp:Literal ID="lSuccessDebug" runat="server" Visible="false" />
                </asp:Panel>
            </div>
        </div>

        <script>

            Sys.Application.add_load(function () {

                $('.js-show-password').click(function (e) {
                    e.preventDefault();
                    var textbox = $('#<%=txtPassword.ClientID%>');
                    if (textbox.prop('type') === 'password') {
                        $(textbox).prop('type', 'text');
                        $(this).text('Hide Password');
                    } else {
                        $(textbox).prop('type', 'password');
                        $(this).text('Show Password');
                    }
                });
            })
        </script>
    </ContentTemplate>
</asp:UpdatePanel>


