<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AttendanceSelfEntry.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.AttendanceSelfEntry" %>
<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>
        <div class="panel panel-block">
            <div class="panel-body">
                <asp:Panel runat="server" CssClass="js-navigation-panel" ID="pnlPrimaryWatcher" Visible="false">
                    <div class="margin-b-lg">
                        <h2 class="margin-t-none">
                            <asp:Literal ID="lPanel1Title" runat="server" />
                        </h2>
                        <asp:Literal ID="lPanel1Text" runat="server" />
                    </div>
                    <asp:ValidationSummary ID="valValidation" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="vgPrimary" />
                    <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" Title="Warning" Visible="false" />
                    <div class="row">
                        <asp:Panel ID="pnlLogin" runat="server" CssClass="col-sm-5 col-sm-push-7 margin-b-lg">
                            <p>
                                Already have an account on our website?
                                <br />
                                Log in instead.
                            </p>
                            <asp:LinkButton ID="btnLogin" CssClass="btn btn-default" runat="server" Text="Login" OnClick="btnLogin_Click" />
                        </asp:Panel>

                        <div class="col-sm-7 col-sm-pull-5">
                            <fieldset>
                                <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" Required="true" ValidationGroup="vgPrimary" NoSpecialCharacters="true" NoEmojisOrSpecialFonts="true" />
                                <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" Required="true" ValidationGroup="vgPrimary" NoSpecialCharacters="true" NoEmojisOrSpecialFonts="true" />
                                <Rock:EmailBox ID="tbEmail" runat="server" Label="Email" Required="false" ValidationGroup="vgPrimary" />
                                <Rock:BirthdayPicker ID="bpBirthDay" runat="server" Label="Birthday" ValidationGroup="vgPrimary" />
                                <asp:Panel ID="pnlPhone" runat="server" CssClass="margin-b-lg">
                                    <Rock:PhoneNumberBox ID="pnbPhone" runat="server" Label="Mobile Phone" ValidationGroup="vgPrimary" />
                                    <Rock:RockCheckBox ID="cbIsMessagingEnabled" runat="server" Text="May we message you on occassion" DisplayInline="true" ValidationGroup="vgPrimary" />
                                </asp:Panel>
                                <Rock:AddressControl ID="acAddress" runat="server" Label="Address" RequiredErrorMessage="Your Address is Required" ValidationGroup="vgPrimary" />
                            </fieldset>
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnPrimaryNext" runat="server" AccessKey="s" ToolTip="Alt+n" Text="Next" CssClass="btn btn-primary pull-right" OnClick="btnPrimaryNext_Click" ValidationGroup="vgPrimary" />
                    </div>
                </asp:Panel>
                <asp:Panel runat="server" ID="pnlOtherWatcher" Visible="false" CssClass="js-navigation-panel">
                    <div class="margin-b-lg">
                        <h2 class="margin-t-none">
                            <asp:Literal ID="lPanel2Title" runat="server" /></h2>
                        <asp:Literal ID="lPanel2Text" runat="server" />
                    </div>
                    <asp:ValidationSummary ID="valOtherWatcher" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="vgOther" />
                    <Rock:NotificationBox ID="nbOtherWarning" runat="server" NotificationBoxType="Warning" Title="Warning" Visible="false" />
                    <div class="row">
                        <div class="col-sm-6 col-md-7 margin-b-lg">
                            <fieldset>
                                <Rock:RockDropDownList ID="ddlRelation" runat="server" Label="Relation" ValidationGroup="vgOther" OnSelectedIndexChanged="ddlRelation_SelectedIndexChanged" AutoPostBack="true" />
                                <Rock:RockTextBox ID="tbOtherFirstName" runat="server" Label="First Name" Required="true" ValidationGroup="vgOther" />
                                <Rock:RockTextBox ID="tbOtherLastName" runat="server" Label="Last Name" Required="true" ValidationGroup="vgOther" />
                                <Rock:EmailBox ID="tbOtherEmail" runat="server" Label="Email" Required="false" ValidationGroup="vgOther" />
                                <Rock:BirthdayPicker ID="bpOtherBirthDay" runat="server" Label="Birthday" ValidationGroup="vgOther" />
                                <asp:Panel ID="pnlOtherPhone" runat="server" CssClass="margin-b-lg">
                                    <Rock:PhoneNumberBox ID="pnOtherMobile" runat="server" Label="Mobile Phone" ValidationGroup="vgOther" />
                                    <Rock:RockCheckBox ID="cbOtherMessagingEnabled" runat="server" Text="May we message you on occassion" DisplayInline="true" ValidationGroup="vgOther" />
                                </asp:Panel>
                                <asp:LinkButton ID="lbAddIndividual" runat="server" CssClass="btn btn-default btn-sm" ValidationGroup="vgOther" OnClick="lbAddIndividual_Click"> Add Additional Individual</asp:LinkButton>
                            </fieldset>
                        </div>
                        <div class="col-sm-6 col-md-5 margin-b-lg">
                            <Rock:RockControlWrapper ID="rcwListed" runat="server" Label="Currently Listed">
                                <hr class="margin-t-sm" />
                                <asp:Repeater ID="rptListed" runat="server" OnItemCommand="rptListed_ItemCommand" OnItemDataBound="rptListed_ItemDataBound">
                                    <ItemTemplate>
                                        <div class="form-inline rollover-container margin-b-sm">
                                            <asp:HiddenField ID="hfRowId" runat="server" Value='<%# Eval("Guid") %>' />
                                            <b><%# Eval("FullName") %></b> <span><%#Eval("RelationshipType") %></span>
                                            <div class="rollover-item control-actions pull-right">
                                                <asp:LinkButton ID="lbDelete" runat="server" CommandName="delete" CommandArgument='<%# Eval("Guid") %>'><i class="fa fa-times"></i></asp:LinkButton>
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </Rock:RockControlWrapper>
                        </div>
                    </div>
                    <div class="actions">
                        <asp:LinkButton ID="btnOtherNext" runat="server" AccessKey="s" ToolTip="Alt+n" Text="Next" CssClass="btn btn-primary pull-right" OnClick="btnOtherNext_Click" />
                    </div>
                </asp:Panel>
                <asp:Panel ID="pnlAccount" runat="server" Visible="false" CssClass="js-navigation-panel">
                    <div class="margin-b-lg">
                        <h2 class="margin-t-none">
                            <asp:Literal ID="lPanel3Title" runat="server" /></h2>
                        <asp:Literal ID="lPanel3Text" runat="server" />
                    </div>
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
                    <div class="margin-b-lg">
                        <h2 class="margin-t-none">
                            <asp:Literal ID="lKnownIndividualTitle" runat="server" />
                        </h2>
                        <asp:Literal ID="lKnownIndividualText" runat="server" />
                    </div>

                            <div class="controls checkin-family checkin-buttons d-flex" data-toggle="buttons-checkbox">
                                <asp:Repeater ID="rptFamilyMembers" runat="server">
                                    <ItemTemplate>
                                        <button type="button" person-id='<%# Eval("Id") %>' class='<%# "btn btn-default btn-lg btn-checkbox" + ((bool)Eval("Selected") ? " active" : "") %>'>
                                            <i class="fa <%# ((bool)Eval("Selected") ? "fa-check-circle-o" : "fa-circle-o") %>"></i>
                                            <span class="name"><%# Eval("FullName") %></span>
                                        </button>
                                    </ItemTemplate>
                                </asp:Repeater>
                                <asp:HiddenField ID="hfSelectedFamilyMembers" runat="server"></asp:HiddenField>
                            </div>

                            <div class="controls checkin-other checkin-buttons d-flex margin-b-lg" data-toggle="buttons-checkbox">
                                <asp:Repeater ID="rptOtherMembers" runat="server">
                                    <ItemTemplate>
                                        <button type="button" person-id='<%# Eval("Id") %>' class='<%# "btn btn-default btn-lg btn-checkbox" + ((bool)Eval("Selected") ? " active" : "") %>'>
                                            <i class="fa <%# ((bool)Eval("Selected") ? "fa-check-circle-o" : "fa-circle-o") %>"></i>
                                            <span class="name"><%# Eval("FullName") %></span>
                                            <span class="small"><%# Eval("RelationshipType") %></span>
                                        </button>
                                    </ItemTemplate>
                                </asp:Repeater>
                                <asp:HiddenField ID="hfSelectedOtherMembers" runat="server"></asp:HiddenField>
                            </div>

                            <asp:LinkButton ID="lbAdjustWatchers" runat="server" Text="Adjust Remembered Watchers" OnClick="lbAdjustWatchers_Click" OnClientClick="return GetSelection();" CausesValidation="false" />

                    <div class="actions margin-t-xl">
                        <asp:LinkButton ID="btnCheckIn" runat="server" AccessKey="s" ToolTip="Alt+n" Text="Check-In" ValidationGroup="vgCheckIn" CssClass="btn btn-primary" OnClientClick="return GetSelection(true);" OnClick="btnCheckIn_Click" />
                    </div>
                </asp:Panel>
                <asp:Panel ID="pnlThanks" runat="server" Visible="false" CssClass="js-navigation-panel">
                    <h2 class="margin-t-none">
                        <asp:Literal ID="lSuccessTitle" runat="server" />
                    </h2>
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

                $('button.btn-checkbox').click( function() {{
                    $(this).find('i').toggleClass('fa-check-circle-o').toggleClass('fa-circle-o');
                }});
            })

            function GetSelection(isCheckIn) {
                var familyIds = '';
                $('div.checkin-family button.active').each(function () {
                    familyIds += $(this).attr('person-id') + ',';
                });
                if (isCheckIn && familyIds == '') {
                    bootbox.alert('Please select at least one family member');
                    return false;
                } else {
                    $('#<%=hfSelectedFamilyMembers.ClientID%>').val(familyIds);

                }

                var otherIds = '';
                $('div.checkin-other button.active').each(function () {
                    otherIds += $(this).attr('person-id') + ',';
                });
                $('#<%=hfSelectedOtherMembers.ClientID%>').val(otherIds);
                return true;
            }
        </script>
    </ContentTemplate>
</asp:UpdatePanel>


