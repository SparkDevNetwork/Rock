<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ConnectionCard.ascx.cs" Inherits="RockWeb.Plugins.com_visitgracechurch.Connection.ConnectionCard" %>

<script type="text/javascript">
    Sys.Application.add_load(function () {
        $('div.js-family-select').click(function () {

            var $familySelectDiv = $(this);

            // get the postback url
            var postbackUrl = $familySelectDiv.attr('data-target');

            // remove the postbackUrls from the other div to prevent multiple clicks
            $('div.js-family-select').attr('data-target', '');

            // if the postbackUrl has been cleared, another button has already been pressed, so ignore
            if (!postbackUrl || postbackUrl == '') {
                return;
            }

            // make the btn in the template a bootstrap button to show the loading... message
            var $familySelectBtn = $familySelectDiv.find('a');
            $familySelectBtn.attr('data-loading-text', 'Loading...');
            Rock.controls.bootstrapButton.showLoading($familySelectBtn);

            window.location = 'javascript: ' + postbackUrl;
        });

        $('a.btn-checkin-select').click(function () {
            $(this).siblings().attr('onclick', 'return false;');
        });
    });
</script>

<style>
    .btn-checkin-select {
        white-space: normal;
        border: 1px solid transparent;
        border-radius: 4px;
    }

    .btn-select {
        background-color: #205171;
    }

    .connectCardSelectedData {
        margin: 0 0 20px 0;
        padding: 5px 25px;
        background-color: #e4e4e4;
        border-radius: 14px;
    }

    .form-inline .input-group .input-group-addon, .form-inline .input-group .input-group-btn, .form-inline .input-group .form-control {
        width: auto;
    }
    .form-inline .input-group {
        display: inline-table;
        vertical-align: middle;
    }

</style>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:HiddenField ID="hfGroupId" runat="server" />

        <h3>Connect Card</h3>
        <hr />

        <asp:Panel ID="pnSelectedData" runat="server" Visible="false" CssClass="connectCardSelectedData">
            <div class="clearfix">
                <Rock:BootstrapButton ID="btnRemoveFamily" runat="server" CssClass="pull-right" OnClick="btnRemoveFamily_Click"><i class="fa fa-times"></i></Rock:BootstrapButton>
                <asp:Label ID="lFamily" runat="server" />
            </div>
            <asp:Panel ID="pnCheckInType" runat="server" Visible="false">
                <div class="clearfix">
                    <Rock:BootstrapButton ID="btnRemoveCheckInType" runat="server" CssClass="pull-right" OnClick="btnRemoveCheckInType_Click"><i class="fa fa-times"></i></Rock:BootstrapButton>
                    <asp:Label ID="lCheckInType" runat="server" />
                </div>
            </asp:Panel>
            <asp:Panel ID="pnCheckingInMem" runat="server" Visible="false">
                <div class="clearfix">
                    <Rock:BootstrapButton ID="btnRemoveMem" runat="server" CssClass="pull-right" OnClick="btnRemoveMem_Click"><i class="fa fa-times"></i></Rock:BootstrapButton>
                    <asp:Label ID="lCheckingIn" runat="server" />
                </div>
            </asp:Panel>
        </asp:Panel>

        <asp:Panel ID="pnSearch" runat="server">
            <Rock:NotificationBox ID="nbSearchValidation" runat="server" NotificationBoxType="Danger" Visible="false"/>
            <label>Please enter your phone number:</label>
            <div class="form-inline">
                <Rock:PhoneNumberBox ID="pnbPhoneSearch" runat="server" OnTextChanged="btnGo_Click" CssClass="input-width-md" />
                <Rock:BootstrapButton ID="btnGo" runat="server" CssClass="btn btn-success" Text="Go" OnClick="btnGo_Click" />
            </div>
            <asp:Panel ID="pnlSelectFamily" runat="server" Visible="false" CssClass="margin-t-md">
				<h4>Select your household:</h4>
                <div class="row">
                    <asp:Repeater ID="rSelection" runat="server" OnItemDataBound="rSelection_ItemDataBound">
                        <ItemTemplate>
                            <%-- pnlSelectFamilyPostback will take care of firing the postback, and lSelectFamilyButtonHtml will be the button HTML from Lava  --%>
                            <asp:Panel ID="pnlSelectFamilyPostback" runat="server" CssClass="col-xs-12 js-family-select">
                                <asp:Literal ID="lSelectFamilyButtonHtml" runat="server" />
                            </asp:Panel>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </asp:Panel>
        </asp:Panel>

        <asp:Panel ID="pnCheckingIn" runat="server" Visible="false">

            <div class="row margin-t-md">
                <div class="col-xs-12 text-left">
                    <label>Where are you worshiping today?</label>
                    <div class="btn-group" role="group" aria-label="Wher are you worshiping today">
                        <Rock:BootstrapButton ID="btnOnline" runat="server" CssClass="btn btn-sm btn-primary" Text="Online" OnClick="btnOnline_Click" />
                        <Rock:BootstrapButton ID="btnInPerson" runat="server" CssClass="btn btn-sm btn-primary" Text="In Person" OnClick="btnInPerson_Click" />
                    </div>
                </div>
            </div>

            <asp:Panel ID="pnCampus" runat="server" Visible="false">
                <div class="row margin-t-md">
                    <div class="col-xs-12 text-left">
                        <label>Which campus?</label>
                        <div class="btn-group" role="group" aria-label="Campus">
                            <asp:Repeater ID="rCampuses" runat="server" OnItemCommand="rCampuses_ItemCommand">
                                <ItemTemplate>
                                    <asp:LinkButton ID="lbCampus" runat="server" CssClass="btn btn-sm btn-primary" CommandName="SelectCampus" CommandArgument='<%# Eval("Id" )%>' Text='<%# Eval("Name") %>'></asp:LinkButton>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                    </div>
                </div>
            </asp:Panel>

            <asp:Panel ID="pnVenue" runat="server" Visible="false">
                <div class="row margin-t-md">
                    <div class="col-xs-12 text-left">
                        <label>Which venue?</label>
                        <div class="btn-group" role="group" aria-label="Venue">
                            <asp:Repeater ID="rVenues" runat="server" OnItemCommand="rVenues_ItemCommand" >
                                <ItemTemplate>
                                    <asp:LinkButton ID="lbVenue" runat="server" CssClass="btn btn-sm btn-primary" CommandName="SelectVenue" CommandArgument='<%# Eval("Id" )%>' Text='<%# Eval("Name") %>'></asp:LinkButton>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                    </div>
                </div>
            </asp:Panel>

            <asp:Panel ID="pnService" runat="server" Visible="false">
                <div class="row margin-t-md">
                    <div class="col-xs-12 text-left">
                        <label>Which service?</label>
                        <div class="btn-group" role="group" aria-label="Service">
                            <asp:Repeater ID="rServices" runat="server" OnItemCommand="rServices_ItemCommand" >
                                <ItemTemplate>
                                    <asp:LinkButton ID="lbService" runat="server" CssClass="btn btn-sm btn-primary" CommandName="SelectService" CommandArgument='<%# Eval("Id" )%>' Text='<%# Eval("Name") %>'></asp:LinkButton>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                    </div>
                </div>
            </asp:Panel>

            <asp:Panel ID="pnNextCheckIn" runat="server" Visible="false">
                <hr />
                <Rock:BootstrapButton ID="btnNextCheckIn" runat="server" CssClass="btn btn-primary pull-right" OnClick="btnNextCheckIn_Click" Text="Next">
						Next
						<i class="fa fa-chevron-right"></i>
                </Rock:BootstrapButton>
            </asp:Panel>

        </asp:Panel>

        <asp:Panel ID="pnFamilyMembers" runat="server" Visible="false">
            <div class="row margin-t-md">
                <div class="col-xs-12 text-left">
                    <asp:Literal ID="Literal1" runat="server"><h4>Who would you like to check in from your household? (Select yourself first)</h4></asp:Literal>
                    <asp:Repeater ID="rSelectionMembers" runat="server" OnItemCommand="rSelectionMembers_ItemCommand">
                        <ItemTemplate>
                            <Rock:BootstrapButton ID="lbSelect" runat="server" CommandArgument='<%# Eval("Id") %>' CssClass="btn btn-primary btn-block btn-checkin-select margin-b-sm text-left" DataLoadingText="Loading...">
                                <asp:Literal ID="lSquare" runat="server"><i class='fa fa-square-o'></i></asp:Literal>
                                <asp:Literal ID="lSquareCheck" runat="server" Visible="false"><i class='fa fa-check-square-o'></i></asp:Literal>
                                &nbsp;<%# Container.DataItem.ToString() %>
                                <asp:Label id="lblMe" runat="server" class="pull-right" visible="false"><i class="fa fa-user"></i> Me</asp:Label>
                            </Rock:BootstrapButton>
                        </ItemTemplate>
                    </asp:Repeater>

                    <Rock:BootstrapButton ID="lbEditFamily" runat="server" CssClass="btn btn-primary btn-block btn-checkin-select text-left" OnClick="lbEditFamily_Click"><i class='fa fa-plus'></i> Edit Family</Rock:BootstrapButton>

                    <hr />
                    <Rock:BootstrapButton ID="btnBackMembers" runat="server" CssClass="btn btn-primary left-span" OnClick="btnBackMembers_Click">
						<i class="fa fa-chevron-left"></i>
						Back
                    </Rock:BootstrapButton>
                    <Rock:BootstrapButton ID="btnNextMembers" runat="server" CssClass="btn btn-primary pull-right" OnClick="btnNextMembers_Click" Visible="false">
						Next
						<i class="fa fa-chevron-right"></i>
                    </Rock:BootstrapButton>

                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnFinalStep" runat="server" Visible="false">
            <div class="row margin-t-md">
                <div class="col-xs-12 text-left">
                    <Rock:BootstrapButton ID="btnSalvation" runat="server" CssClass="btn btn-primary btn-block btn-checkin-select margin-b-sm text-left" OnClick="btnSalvation_Click" >
                        <i class='fa fa-square-o'></i> I made a decision to follow Jesus today
                    </Rock:BootstrapButton>	
                    
					<div class="margin-b-sm"">
						I would like to:
					</div>					
					<div class="margin-l-lg margin-b-sm">
						<Rock:BootstrapButton ID="btnExplore" runat="server" CssClass="btn btn-primary btn-block btn-checkin-select margin-b-sm text-left" OnClick="btnExplore_Click" >
							<i class='fa fa-square-o'></i> Learn more about Grace Church at Explore
						</Rock:BootstrapButton>
						
						<Rock:BootstrapButton ID="btnServe" runat="server" CssClass="btn btn-primary btn-block btn-checkin-select margin-b-sm text-left" OnClick="btnServe_Click" >
							<i class='fa fa-square-o'></i> Join a Team
						</Rock:BootstrapButton>
						
						<Rock:BootstrapButton ID="btnConnect" runat="server" CssClass="btn btn-primary btn-block btn-checkin-select margin-b-sm text-left" OnClick="btnConnect_Click" >
							<i class='fa fa-square-o'></i> I'm ready to Discover Community with others
						</Rock:BootstrapButton>

						<Rock:BootstrapButton ID="btnContact" runat="server" CssClass="btn btn-primary btn-block btn-checkin-select margin-b-sm text-left" OnClick="btnContact_Click" >
							<i class='fa fa-square-o'></i> Please have a Staff Member contact me
						</Rock:BootstrapButton>
						
						<asp:Panel ID="pnContactRegarding" runat="server" Visible="false">
							<div style="margin-bottom: 10px;">
								<Rock:RockTextBox ID="tbContactRegarding" runat="server" TextMode="MultiLine" Rows="3" CssClass="margin-t-sm" Placeholder="Enter message..." />
							</div>
						</asp:Panel>
					   
						<Rock:BootstrapButton ID="btnBaptism" runat="server" CssClass="btn btn-primary btn-block btn-checkin-select margin-b-sm text-left" OnClick="btnBaptism_Click" >
							<i class='fa fa-square-o'></i> Find out more information about baptism
						</Rock:BootstrapButton>
						
						<Rock:BootstrapButton ID="btnUpdate" runat="server" CssClass="btn btn-primary btn-block btn-checkin-select margin-b-sm text-left" OnClick="btnUpdate_Click" >
							<i class='fa fa-square-o'></i> Receive the Weekly Campus Update
						</Rock:BootstrapButton>
					</div>
                    
                    <Rock:BootstrapButton ID="btnNew" runat="server" CssClass="btn btn-primary btn-block btn-checkin-select margin-b-sm text-left" OnClick="btnNew_Click" >
                        <i class='fa fa-square-o'></i> I am new to the area
                    </Rock:BootstrapButton>

                    <asp:Panel ID="pnSubmitWarning" runat="server" Visible="false">
                        <label class="label label-danger" style="margin-bottom: 10px;">Please fill out comment or prayer info</label>
                    </asp:Panel>

                    <Rock:BootstrapButton ID="btnComment" runat="server" CssClass="btn btn-primary btn-block btn-checkin-select margin-b-sm text-left " OnClick="btnComment_Click">
                        <i class='fa fa-square-o'></i> I would like to leave a comment or submit a prayer request
                    </Rock:BootstrapButton>

                    <asp:Panel ID="pnComment" runat="server" Visible="false">
                        <div style="margin-bottom: 10px;">
                            <Rock:RockRadioButtonList ID="rblComment" runat="server" RepeatDirection="Horizontal">
                                <asp:ListItem Text="Comment" Value="0" Selected="True" />
                                <asp:ListItem Text="Prayer" Value="1" />
                                <asp:ListItem Text="Praise" Value="2" />
                            </Rock:RockRadioButtonList>
                            <Rock:RockTextBox ID="tbComment" runat="server" TextMode="MultiLine" Rows="3" CssClass="margin-t-sm" Placeholder="Enter message..."/>
                        </div>
                    </asp:Panel>

                    <asp:Panel ID="pnSubmit" runat="server">
                        <hr />
                        <Rock:BootstrapButton ID="btnBackFinal" runat="server" CssClass="btn btn-primary left-span" OnClick="btnBackFinal_Click">
								<i class="fa fa-chevron-left"></i>
								Back
                        </Rock:BootstrapButton>
                        <Rock:BootstrapButton ID="btnSubmit" runat="server" CssClass="btn btn-success pull-right" Text="Submit" OnClick="btnSubmit_Click" />
                    </asp:Panel>
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnThanks" runat="server" Visible="false">
            <div class="row margin-t-md">
                <div class="col-xs-12 text-left">
                    <h4>Thank you!</h4>
                    <label>Thanks for submitting your connect card and helping us shepherd our church.</label>
                    <br />
                    <label>You're all set!</label>
                </div>
            </div>
        </asp:Panel>


    </ContentTemplate>
</asp:UpdatePanel>

<%-- Wrap with a 'Conditional' Update Panel so that we don't loose the modal effect on postback from mdEditFamily --%>
<asp:UpdatePanel ID="upContent" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <asp:HiddenField ID="HiddenField1" runat="server" />
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
                                    <Rock:EditField OnClick="EditFamilyMember_Click" />
                                    <Rock:DeleteField OnClick="DeleteFamilyMember_Click" />
                                </Columns>
                            </Rock:Grid>

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
                            <asp:Panel ID="pnAddress" runat="server" CssClass="alert alert-validation" Visible="false">
                                <p>Address is required.</p>
                            </asp:Panel>
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:Toggle ID="tglAdultChild" runat="server" OnText="Adult" OffText="Child" ActiveButtonCssClass="btn-primary" OnCheckedChanged="tglAdultChild_CheckedChanged" />
                                    <Rock:DefinedValuePicker ID="dvpRecordStatus" runat="server" Label="Record Status" ValidationGroup="vgEditPerson" />

                                    <%-- keep a hidden field for connectionstatus since we need to keep the state, but don't want it to be editable or viewable --%>
                                    <asp:HiddenField ID="hfConnectionStatus" runat="server" />
                                </div>
                                <div class="col-md-6">
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
                                    <Rock:AddressControl ID="acHomeAddress" runat="server" Label="Home Address" Required="true" />
                                </div>

                            </div>

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
