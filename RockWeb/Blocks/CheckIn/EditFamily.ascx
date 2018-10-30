<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EditFamily.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.EditFamily" %>
<%@ Reference Control="~/Blocks/CheckIn/Search.ascx" %>

<%-- Wrap with a 'Conditional' Update Panel so that we don't loose the modal effect on postback from mdEditFamily --%>
<asp:UpdatePanel ID="upContent" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <asp:HiddenField ID="hfGroupId" runat="server" />
        <asp:HiddenField ID="hfShowCancelEditPrompt" runat="server" Value="0" />

        <script>
            Sys.Application.add_load(function () {

                // the number of ms since the last time keyboard input was received when in 'capture alternateid' mode'
                var lastKeyPress = 0;

                // the keyboard input capture in 'capture alternateid' mode prior to the alternate-id field having focus
                var keyboardBuffer = '';

                // the element that was active when 'capture alternateid' mode detected wedge input
                var originalTarget = null;

                var $alternateId = $('.js-alternate-id');
                if ($alternateId.is(':visible')) {
                    var alternateIdElementId = $alternateId.prop('id');

                    $alternateId.on('keypress', function (e) {
                        if (e.which == 13) {
                            var date = new Date();
                            var timeDiff = (date.getTime() - lastKeyPress);

                            if (timeDiff < 500) {
                                // restore focus back to the element that was active when 'capture alternateid' mode detected wedge input

                                if (originalTarget) {
                                    originalTarget.focus();

                                    // clear the originalTarget
                                    originalTarget = $(false);
                                }

                                // don't let a carriage return in the AlternateID field cause the form to submit. It is probably from the Keyboard Wedge if it has been less than 500ms
                                // NOTE: a Carriage Return from the Wedge takes longer than other input, so we have to wait a little longer (but not too long).
                                e.preventDefault();
                                return false;
                            }
                        }
                    });

                    $(document).off('keypress').on('keypress', function (e) {
                        var date = new Date();

                        if (e.target.id != alternateIdElementId) {
                            var timeDiff = (date.getTime() - lastKeyPress);
                            if (timeDiff > 500) {
                                // if it's been more than 500ms, assume it is either a new wedge read or just normal keyboard input
                                keyboardBuffer = String.fromCharCode(e.which);
                            } else if (timeDiff < 75) {
                                // if it's been more less than 75ms, assume a wedge read is coming in and append to the keyboardBuffer
                                var targetBuffer = keyboardBuffer;
                                keyboardBuffer += String.fromCharCode(e.which);
                                if (keyboardBuffer.length > 6) {
                                    // if we've gotton more than 6 chars of 'fast input', pretty good chance it is a wedge
                                    // A fast typist might be able to cause this to happen too
                                    var $target = $(e.target);
                                    var targetVal = $target.val();
                                    if (targetVal.includes(targetBuffer)) {
                                        targetVal = targetVal.replace(targetBuffer, '');
                                        $target.val(targetVal);
                                    }

                                    $alternateId.val(keyboardBuffer);
                                    originalTarget = $(e.target);
                                    $alternateId.focus();
                                    e.preventDefault();
                                    return false;
                                }
                            }
                        }

                        lastKeyPress = date.getTime();
                    });
                }

                if ($('#<%=hfShowCancelEditPrompt.ClientID%>').val() == "1") {

                bootbox.confirm({
                    message: 'Changes have been made to this family. Do you want to leave without saving?',
                    buttons: {
                        cancel: {
                            label: 'Stay on This Page'
                        },
                        confirm: {
                            label: 'Leave This Page'
                        }
                    },
                    callback: function (result) {
                        if (result) {
                            window.location = "javascript:__doPostBack('<%=upContent.ClientID %>', 'ConfirmCancelFamily')";
                        }
                    }
                });

            }
            });
        </script>

        <%-- Edit Family Modal --%>
        <Rock:ModalDialog ID="mdEditFamily" runat="server" Title="Add Family" CancelLinkVisible="false">
            <Content>

                <%-- Have an inner UpdatePanel wrapper by a 'Conditional' Update Panel so that we don't loose the modal effect on postback from mdEditFamily --%>
                <asp:UpdatePanel ID="upEditFamily" runat="server">
                    <ContentTemplate>
                        <%-- Edit Family View --%>
                        <asp:Panel ID="pnlEditFamily" runat="server">
                            <asp:ValidationSummary ID="vsEditFamily" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="vgEditFamily" />
                            <%-- Grid --%>
                            <div class="control-group edit-family-header">
                                <label class="control-label">Family Members</label>
                                <asp:LinkButton ID="btnAddPerson" runat="server" CssClass="btn btn-link btn-add-person pull-right" Text="<i class='fas fa-plus'></i> Add Person" CausesValidation="false" OnClick="btnAddPerson_Click" />
                            </div>
                            <Rock:Grid ID="gFamilyMembers" runat="server" CssClass="edit-family-grid" DisplayType="Light" ShowActionRow="false" ShowActionsInHeader="false" ShowHeader="false" ShowFooter="false" OnRowDataBound="gFamilyMembers_RowDataBound" RowItemText="Person">
                                <Columns>
                                    <asp:BoundField DataField="FullName" />
                                    <Rock:RockLiteralField ID="lGroupRoleAndRelationship" />
                                    <asp:BoundField DataField="Gender" />
                                    <asp:BoundField DataField="Age" />
                                    <asp:BoundField DataField="GradeFormatted" />
                                    <Rock:RockLiteralField ID="lRequiredAttributes" />
                                    <Rock:EditField OnClick="EditFamilyMember_Click" />
                                    <Rock:DeleteField OnClick="DeleteFamilyMember_Click" />
                                </Columns>
                            </Rock:Grid>

                            <%-- Family Attributes --%>

                            <Rock:DynamicPlaceholder ID="phFamilyAttributes" runat="server" />

                            <%-- Edit Family Buttons --%>
                            <div class="actions">
                                <Rock:BootstrapButton ID="btnSaveFamily" runat="server" CssClass="btn btn-primary" Text="Save" CausesValidation="true" ValidationGroup="vgEditFamily" OnClick="btnSaveFamily_Click" DataLoadingText="Saving..." />
                                <asp:LinkButton ID="btnCancelFamily" runat="server" CssClass="btn btn-default btn-cancel" Text="Cancel" CausesValidation="false" OnClick="btnCancelFamily_Click" />
                            </div>
                        </asp:Panel>

                        <%-- Edit Person View --%>
                        <asp:Panel ID="pnlEditPerson" runat="server" DefaultButton="btnDonePerson">


                            <asp:HiddenField ID="hfGroupMemberGuid" runat="server" />
                            <asp:ValidationSummary ID="vsEditPerson" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="vgEditPerson" />
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:Toggle ID="tglAdultChild" runat="server" OnText="Adult" OffText="Child" ActiveButtonCssClass="btn-primary" OnCheckedChanged="tglAdultChild_CheckedChanged" />
                                    <Rock:DefinedValuePicker ID="dvpRecordStatus" runat="server" Label="Record Status" ValidationGroup="vgEditPerson" />

                                    <%-- keep a hidden field for connectionstatus since we need to keep the state, but don't want it to be editable or viewable --%>
                                    <asp:HiddenField ID="hfConnectionStatus" runat="server" />
                                </div>
                                <div class="col-md-6">
                                    <%-- Fields to be shown when editing a Child --%>
                                    <asp:Panel ID="pnlChildRelationshipToAdult" runat="server">
                                        <Rock:RockDropDownList ID="ddlChildRelationShipToAdult" runat="server" Label="Relationship to Adult" />
                                        <Rock:RockLiteral ID="lChildRelationShipToAdultReadOnly" runat="server" Label="Relationship" />
                                    </asp:Panel>

                                    <%-- Fields to be shown when editing an Adult --%>
                                    <Rock:Toggle ID="tglAdultMaritalStatus" runat="server" OnText="Married" OffText="Single" ActiveButtonCssClass="btn-primary" />
                                </div>
                            </div>

                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:DataTextBox ID="tbFirstName" runat="server" SourceTypeName="Rock.Model.Person" PropertyName="NickName" Label="First Name" Required="true" ValidationGroup="vgEditPerson" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:DataTextBox ID="tbLastName" runat="server" SourceTypeName="Rock.Model.Person" PropertyName="LastName" Label="Last Name" Required="true" ValidationGroup="vgEditPerson" />
                                </div>
                            </div>

                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:DefinedValuePicker ID="dvpSuffix" runat="server" Label="Suffix" ValidationGroup="vgEditPerson" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:ButtonGroup ID="bgGender" runat="server" FormGroupCssClass="toggle-container" SelectedItemClass="btn btn-primary active" UnselectedItemClass="btn btn-default" Label="&nbsp;" Required="true" ValidationGroup="vgEditPerson" RequiredErrorMessage="Gender is required.">
                                        <asp:ListItem Text="Male" Value="1" />
                                        <asp:ListItem Text="Female" Value="2" />
                                    </Rock:ButtonGroup>
                                </div>
                                <div class="col-md-6">
                                    <Rock:DatePicker ID="dpBirthDate" runat="server" Label="Birthdate" AllowFutureDateSelection="False" RequireYear="True" ShowOnFocus="false" StartView="decade" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:GradePicker ID="gpGradePicker" runat="server" Label="Grade" UseGradeOffsetAsValue="true" UseAbbreviation="true" ValidationGroup="vgEditPerson" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:PhoneNumberBox ID="pnMobilePhone" runat="server" Label="Mobile Phone" ValidationGroup="vgEditPerson" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:EmailBox ID="tbEmail" runat="server" Label="Email" ValidationGroup="vgEditPerson" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:RockTextBox ID="tbAlternateID" runat="server" Label="Alternate ID" CssClass="js-alternate-id" ValidationGroup="vgEditPerson" />
                                </div>

                            </div>

                            <%-- Person Attributes editing an Adult --%>
                            <Rock:HtmlGenericContainer ID="pnlAdultFields" runat="server">
                                <Rock:DynamicPlaceholder ID="phAdultAttributes" runat="server" />
                            </Rock:HtmlGenericContainer>

                            <%-- Person Attributes when editing a Child --%>
                            <Rock:HtmlGenericContainer ID="pnlChildFields" runat="server">
                                <Rock:DynamicPlaceholder ID="phChildAttributes" runat="server" />
                            </Rock:HtmlGenericContainer>

                            <%-- Person Actions --%>
                            <div class="actions">
                            <asp:LinkButton ID="btnDonePerson" runat="server" CssClass="btn btn-primary" Text="Done" CausesValidation="true" ValidationGroup="vgEditPerson" OnClick="btnDonePerson_Click" />
                                <asp:LinkButton ID="btnCancelPerson" runat="server" CssClass="btn btn-default btn-cancel" Text="Cancel" CausesValidation="false" OnClick="btnCancelPerson_Click" />

                            </div>
                        </asp:Panel>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>
