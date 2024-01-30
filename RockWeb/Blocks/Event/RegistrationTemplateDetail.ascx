<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RegistrationTemplateDetail.ascx.cs" Inherits="RockWeb.Blocks.Event.RegistrationTemplateDetail" %>

<script type="text/javascript">

    function clearActiveDialog() {
        $('#<%=hfActiveDialogID.ClientID %>').val('');
    }

    Sys.Application.add_load(function () {
        $('div.js-same-family').find('input:radio').on('click', function () {
            if ($(this).val() > 0) {
                $('.js-current-family-members').slideDown();
            } else {
                var $div = $('.js-current-family-members');
                $div.find('input:checkbox').prop('checked', false);
                $div.slideUp();
            }
        });
    });
</script>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">
            <asp:HiddenField ID="hfRegistrationTemplateId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-clipboard"></i>
                    <asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlInactive" runat="server" LabelType="Danger" Text="Inactive" />
                    <Rock:HighlightLabel ID="hlType" runat="server" LabelType="Type" />
                </div>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">

                <asp:ValidationSummary ID="vsDetails" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                <Rock:NotificationBox ID="nbValidationError" runat="server" NotificationBoxType="Danger" Heading="Please correct the following:" Visible="false" />

                <div id="pnlEditDetails" runat="server">

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.RegistrationTemplate, Rock" PropertyName="Name" />
                            <Rock:CategoryPicker ID="cpCategory" runat="server" Required="true" Label="Category" EntityTypeName="Rock.Model.RegistrationTemplate" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Active" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-12">
                            <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.RegistrationTemplate, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
                        </div>
                    </div>

                    <Rock:PanelWidget ID="pwDetails" runat="server" Title="Details" Expanded="true">
                        <div class="row">
                            <div class="col-md-4">
                                <Rock:GroupTypePicker ID="gtpGroupType" runat="server" Label="Group Type" AutoPostBack="true" OnSelectedIndexChanged="gtpGroupType_SelectedIndexChanged" IsSortedByName="true" />
                                <Rock:GroupRolePicker ID="rpGroupTypeRole" runat="server" Label="Group Member Role"
                                    Help="The group member role that new registrants should be added to group with." />
                                <Rock:RockDropDownList ID="ddlGroupMemberStatus" runat="server" Label="Group Member Status"
                                    Help="The group member status that new registrants should be added to group with." />
                                <Rock:DefinedValuePicker ID="dvpConnectionStatus" runat="server" Label="Connection Status"
                                    Help="The connection status to use for new individuals. Setting this here will override the setting on the Registration Entry block." />
                            </div>
                            <div class="col-md-8">
                                <div class="row">
                                    <div class="col-xs-6">
                                        <Rock:RockCheckBox ID="cbMultipleRegistrants" runat="server" Label="Allow Multiple Registrants" Text="Yes"
                                            Help="Should user be allowed to register multiple registrants at the same time?"
                                            AutoPostBack="true" OnCheckedChanged="cbMultipleRegistrants_CheckedChanged" />
                                    </div>
                                    <div class="col-xs-6">
                                        <Rock:NumberBox MinimumValue="1" ID="nbMaxRegistrants" runat="server" Label="Max Registrants Per Registration"
                                            Help="The maximum number of registrants that a person is allowed to register at one time. Leave blank for unlimited." Visible="false" />
                                    </div>
                                </div>

                                <div class="row">
                                    <div class="col-xs-6">
                                        <Rock:RockRadioButtonList ID="rblRegistrantsInSameFamily" runat="server" Label="Registrants In Same Family" RepeatDirection="Horizontal" CssClass="js-same-family"
                                            Help="Typical relationship of registrants that user would register." />
                                        <div id="divCurrentFamilyMembers" runat="server" class="js-current-family-members">
                                            <Rock:RockCheckBox ID="cbShowCurrentFamilyMembers" runat="server" Label="Show Family Members" Text="Yes"
                                                Help="If Registrants in Same Family option is set to 'Yes' or 'Ask', should the person registering be able to select people from their family when registering (vs. having to enter the family member's information manually)?" />
                                        </div>

                                        <Rock:RockCheckBox ID="cbWaitListEnabled" runat="server" Label="Enable Wait List" Text="Yes" Help="Check to enable a 'wait list' once the registration's maximum attendees has been reached. The 'Maximum Attendees' must be set on the registration instance for this feature to work." />
                                    </div>
                                    <div class="col-xs-6">
                                        <Rock:RockDropDownList ID="ddlRegistrarOption" runat="server" Label="Registrar Options">
                                            <asp:ListItem Value="0" Text="Prompt For Registrar" />
                                            <asp:ListItem Value="1" Text="Pre-fill First Registrant" />
                                            <asp:ListItem Value="2" Text="Use First Registrant" />
                                            <asp:ListItem Value="3" Text="Use Logged In Person" />
                                        </Rock:RockDropDownList>
                                        <Rock:RockCheckBox ID="cbShowSmsOptIn" runat="server" Label="Show SMS Opt-In" Text="Yes" Help="When enabled a checkbox will be shown next to each mobile phone number for registrants allowing the registrar to enable SMS messaging for this number." />
                                    </div>
                                </div>
                            </div>
                        </div>

                        <hr />

                        <div class="row">
                            <div class="col-md-6">
                                <div class="row">
                                    <div class="col-xs-6">
                                        <Rock:RockCheckBoxList ID="cblNotify" runat="server" Label="Notify" RepeatDirection="Vertical"
                                            Help="Who should be notified when new people are registered?">
                                            <asp:ListItem Value="1" Text="Registration Contact" />
                                            <asp:ListItem Value="2" Text="Group Followers" />
                                            <asp:ListItem Value="4" Text="Group Leaders" />
                                        </Rock:RockCheckBoxList>
                                    </div>
                                    <div class="col-xs-6">
                                        <Rock:RockCheckBox ID="cbAddPersonNote" runat="server" Label="Add Person Note" Help="Should a note be added to a person's record whenever they register?" Text="Yes" />
                                    </div>
                                </div>
                            </div>
                            <div class="col-md-6">
                                <div class="row">
                                    <div class="col-xs-6">
                                        <Rock:RockCheckBox ID="cbLoginRequired" runat="server" Label="Login Required" Text="Yes"
                                            Help="Is user required to be logged in when registering?" />
                                    </div>
                                    <div class="col-xs-6">
                                    </div>
                                </div>
                            </div>
                        </div>

                        <hr />

                        <div class="well">
                            <div class="row">
                                <div class="col-sm-6">
                                    <Rock:Toggle ID="tglSetCostOnTemplate" runat="server" Label="Set Cost On" OnText="Template" OffText="Instance"
                                        ActiveButtonCssClass="btn-info" OnCheckedChanged="tglSetCost_CheckedChanged" ButtonSizeCssClass="btn-xs" />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:FinancialGatewayPicker ID="fgpFinancialGateway" runat="server" Label="Financial Gateway"
                                        Help="The financial gateway to use for processing registration payments." IncludeInactive="false" ShowAllGatewayComponents="true" AutoPostBack="true" OnSelectedIndexChanged="fgpFinancialGateway_SelectedIndexChanged" />
                                    <Rock:RockTextBox ID="txtBatchNamePrefix" runat="server" Label="Batch Prefix" Help="Optional prefix to add the the financial batches. If left blank the prefix from the registration block will be used." />
                                    <Rock:CurrencyBox ID="cbCost" runat="server" Label="Cost"
                                        Help="The cost per registrant." />
                                </div>
                                <div class="col-md-6">
                                    <Rock:RockCheckBox ID="cbEnablePaymentPlans" runat="server" Label="Enable Payment Plans" AutoPostBack="true" OnCheckedChanged="cbEnablePaymentPlans_CheckedChanged" Enabled="false"
                                        Help="Determines if individuals should be able to pay their registration costs in multiple, scheduled installments. Not all payment gateways support this feature." />
                                    <Rock:RockCheckBoxList ID="cblSelectablePaymentFrequencies" runat="server" Label="Selectable Payment Frequencies" Visible="false"
                                        Help="The payment frequencies that the individual can select from." RepeatDirection="Horizontal" />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:CurrencyBox ID="cbMinimumInitialPayment" runat="server" Label="Minimum Initial Payment"
                                        Help="The minimum amount required per registrant. Leave value blank if full amount is required." />
                                </div>
                                <div class="col-md-6">
                                    <Rock:CurrencyBox ID="cbDefaultPaymentAmount" runat="server" Label="Default Payment Amount"
                                        Help="The default payment amount per registrant. Leave value blank to default to the full amount. NOTE: This requires that a Minimum Initial Payment is greater than 0." />
                                </div>
                            </div>
                        </div>

                        <hr />

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:WorkflowTypePicker ID="wtpRegistrationWorkflow" runat="server" Label="Registration Workflow"
                                    Help="An optional workflow type to launch when a new registration is completed." />
                                 <Rock:WorkflowTypePicker ID="wtpRegistrantWorkflow" runat="server" Label="Registrant Workflow"
                                    Help="An optional workflow type to launch for each Registrant when a new registration is completed. Both the 'RegistrationRegistrantId' and the 'RegistrationId' will be passed to the workflow." />
                                <Rock:RockCheckBox ID="cbAllowExternalUpdates" runat="server" Label="Allow External Updates to Saved Registrations" Text="Yes"
                                    Help="Allow saved registrations to be updated online. If false, the individual will be able to make additional payments but will
                                            not be allowed to change any of the registrant information and attributes." />
                            </div>
                            <div class="col-md-6">
                                <div class="row">
                                    <div class="col-xs-6">
                                        <Rock:RockDropDownList ID="ddlSignatureDocumentTemplate" runat="server" Label="Required Signature Document" AutoPostBack="true"
                                            Help="A document that needs to be signed for registrations of this type." OnSelectedIndexChanged="ddlSignatureDocumentTemplate_SelectedIndexChanged" />
                                    </div>
                                    <div class="col-xs-6">
                                        <Rock:RockCheckBox ID="cbDisplayInLine" runat="server" Label="In-Line Signature" Text="Yes" Visible="false"
                                            Help="When registering for this type of event, should the Required Signature Document be displayed during the registration steps? If not, a request will be sent after the registration is completed." />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </Rock:PanelWidget>

                    <%-- Registrant Forms --%>
                    <Rock:PanelWidget ID="wpRegistrantForms" runat="server" Title="Registrant Form(s)">
                        <div class="form-list">
                            <asp:PlaceHolder ID="phForms" runat="server" />
                        </div>
                        <div class="pull-right">
                            <asp:LinkButton ID="lbAddForm" runat="server" CssClass="btn btn-action btn-xs" OnClick="lbAddForm_Click" CausesValidation="false"><i class="fa fa-plus"></i> Add Form</asp:LinkButton>
                        </div>
                    </Rock:PanelWidget>

                    <%-- Registration Attributes --%>
                    <Rock:PanelWidget ID="wpRegistrationAttributes" runat="server" Title="Registration Attributes">
                        <div class="grid">
                            <Rock:Grid ID="gRegistrationAttributes" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Registration Attribute">
                                <Columns>
                                    <Rock:ReorderField />
                                    <Rock:RockBoundField DataField="Name" HeaderText="Attribute" />
                                    <Rock:RockBoundField DataField="Description" HeaderText="Description" />
                                    <Rock:RockLiteralField HeaderText="Category" OnDataBound="gRegistrationAttributesCategory_DataBound" />
                                    <Rock:BoolField DataField="IsRequired" HeaderText="Required" />
                                    <Rock:SecurityField TitleField="Name" />
                                    <Rock:EditField OnClick="gRegistrationAttributes_Edit" />
                                    <Rock:DeleteField OnClick="gRegistrationAttributes_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </Rock:PanelWidget>

                    <Rock:ModalDialog ID="dlgRegistrationAttribute" runat="server" Title="Registration Attributes" OnSaveClick="dlgRegistrationAttribute_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="RegistrationAttributes">
                        <Content>
                            <Rock:AttributeEditor ID="edtRegistrationAttributes" runat="server" ShowActions="false" ValidationGroup="RegistrationAttributes" />
                        </Content>
                    </Rock:ModalDialog>

                    <%-- Fees --%>
                    <Rock:PanelWidget ID="wpFees" runat="server" Title="Fees">
                        <Rock:Grid ID="gFees" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Fees">
                            <Columns>
                                <Rock:ReorderField />
                                <Rock:RockBoundField DataField="Name" HeaderText="Fee" />
                                <Rock:EnumField DataField="FeeType" HeaderText="Options" />
                                <Rock:RockBoundField DataField="Cost" HeaderText="Cost" ItemStyle-Wrap="true" />
                                <Rock:BoolField DataField="AllowMultiple" HeaderText="Enable<br />Quantity" HtmlEncode="false" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" />
                                <Rock:BoolField DataField="DiscountApplies" HeaderText="Discount<br />Applies" HtmlEncode="false" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" />
                                <Rock:BoolField DataField="IsActive" HeaderText="Is<br />Active" HtmlEncode="false" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" />
                                <Rock:BoolField DataField="IsRequired" HeaderText="Is<br />Required" HtmlEncode="false" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" />
                                <Rock:EditField OnClick="gFees_Edit" />
                                <Rock:DeleteField OnClick="gFees_Delete" />
                            </Columns>
                        </Rock:Grid>
                    </Rock:PanelWidget>

                    <%-- Discounts --%>
                    <Rock:PanelWidget ID="wpDiscounts" runat="server" Title="Discounts">
                        <Rock:Grid ID="gDiscounts" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Discounts">
                            <Columns>
                                <Rock:ReorderField />
                                <Rock:RockBoundField DataField="Code" HeaderText="Code" />
                                <Rock:RockBoundField DataField="Discount" HeaderText="Discount" />
                                <Rock:RockBoundField DataField="Limits" HeaderText="Limits" />
                                <Rock:EditField OnClick="gDiscounts_Edit" />
                                <Rock:DeleteField OnClick="gDiscounts_Delete" />
                            </Columns>
                        </Rock:Grid>
                    </Rock:PanelWidget>

                    <%-- Placement Configuration Grid --%>
                    <Rock:PanelWidget ID="pwPlacementConfiguration" runat="server" Title="Placement Configuration">
                        <Rock:Grid ID="gPlacementConfigurations" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Placement Configuration" OnRowDataBound="gPlacementConfigurations_RowDataBound">
                            <Columns>
                                <Rock:ReorderField />
                                <Rock:RockLiteralField ID="lPlacementName" HeaderText="Placement Name" />
                                <Rock:RockLiteralField ID="lGroupTypeName" HeaderText="Group Type" />
                                <Rock:RockLiteralField ID="lSharedGroupNames" HeaderText="Shared Groups" />
                                <Rock:RockBoundField DataField="AllowMultiplePlacements" HeaderText="Allow Multiple" />
                                <Rock:EditField OnClick="gPlacementConfigurations_EditClick" />
                                <Rock:DeleteField OnClick="gPlacementConfigurations_DeleteClick" />
                            </Columns>
                        </Rock:Grid>
                    </Rock:PanelWidget>

                    <%-- Terms/Text --%>
                    <Rock:PanelWidget ID="wpTerms" runat="server" Title="Terms/Text">
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockTextBox ID="tbRegistrationTerm" runat="server" Label="Registration Term" Placeholder="Registration" MaxLength="100" />
                                <Rock:RockTextBox ID="tbRegistrantTerm" runat="server" Label="Registrant Term" Placeholder="Person" MaxLength="100" />
                                <Rock:RockTextBox ID="tbRegistrationAttributeTitleStart" runat="server" Label="Registration Attribute Title - Start" Placeholder="Registration Information" Help="The section title for attributes that are collected at the start of the registration entry process." MaxLength="200" />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockTextBox ID="tbFeeTerm" runat="server" Label="Fee Term" Placeholder="Additional Options" MaxLength="100" />
                                <Rock:RockTextBox ID="tbDiscountCodeTerm" runat="server" Label="Discount Code Term" Placeholder="Discount Code" MaxLength="100" />
                                <Rock:RockTextBox ID="tbRegistrationAttributeTitleEnd" runat="server" Label="Registration Attribute Title - End" Placeholder="Registration Information" Help="The section title for attributes that are collected at the end of the registration entry process." MaxLength="200" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-12">
                                <Rock:RockTextBox ID="tbSuccessTitle" runat="server" Label="Success Title" Placeholder="Congratulations"
                                    Help="The heading to display to user after successfully completing a registration of this type." />
                                <Rock:HtmlEditor ID="heInstructions" runat="server" Label="Registration Instructions" ResizeMaxWidth="720" Height="300" Help="These instructions will appear at the beginning of the registration process." Toolbar="Light" />
                                <Rock:CodeEditor ID="ceSuccessText" runat="server" Label="Registration Confirmation Text" EditorMode="Lava" EditorTheme="Rock" EditorHeight="300"
                                    Help="The text to display to user after successfully completing a registration of this type. If there are costs or fees for this registration, a summary of those will be displayed after this text." />
                            </div>
                        </div>
                    </Rock:PanelWidget>

                    <%-- Show Communication Settings --%>
                    <div class="justify-content-end" id="registration-detailscheckbox" style="display:flex;">
                        <div class="checkbox pull-right">
                            <label>
                                <input id="cb-showdetails" type="checkbox" />
                                <span class="label-text">Show Communication Settings</span>
                            </label>
                        </div>
                    </div>

                    <div id="registration-details" style="display: none;">
                        <Rock:PanelWidget ID="wpConfirmation" runat="server" Title="Confirmation Email">
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockTextBox ID="tbConfirmationFromName" runat="server" Label="From Name" />
                                    <Rock:RockTextBox ID="tbConfirmationFromEmail" runat="server" Label="From Email" />
                                </div>
                                <div class="col-md-6">
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-12">
                                    <Rock:RockTextBox ID="tbConfirmationSubject" runat="server" Label="Subject" />
                                    <Rock:CodeEditor ID="ceConfirmationEmailTemplate" runat="server" Label="Email Template" EditorMode="Lava" EditorTheme="Rock" EditorHeight="300" />
                                </div>
                            </div>
                        </Rock:PanelWidget>

                        <Rock:PanelWidget ID="wpReminder" runat="server" Title="Reminder Email">
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockTextBox ID="tbReminderFromName" runat="server" Label="From Name" />
                                    <Rock:RockTextBox ID="tbReminderFromEmail" runat="server" Label="From Email" />
                                </div>
                                <div class="col-md-6">
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-12">
                                    <Rock:RockTextBox ID="tbReminderSubject" runat="server" Label="Subject" />
                                    <Rock:CodeEditor ID="ceReminderEmailTemplate" runat="server" Label="Email Template" EditorMode="Lava" EditorTheme="Rock" EditorHeight="300" />
                                </div>
                            </div>
                        </Rock:PanelWidget>

                        <Rock:PanelWidget ID="wpPaymentReminder" runat="server" Title="Payment Reminder Email">
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockTextBox ID="tbPaymentReminderFromName" runat="server" Label="From Name" />
                                    <Rock:RockTextBox ID="tbPaymentReminderFromEmail" runat="server" Label="From Email" />
                                </div>
                                <div class="col-md-6">
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-12">
                                    <Rock:RockTextBox ID="tbPaymentReminderSubject" runat="server" Label="Subject" />
                                    <Rock:CodeEditor ID="cePaymentReminderEmailTemplate" runat="server" Label="Email Template" EditorMode="Lava" EditorTheme="Rock" EditorHeight="300" />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:NumberBox ID="nbPaymentReminderTimeSpan" runat="server" Help="The number of days in between automatic payment reminders." CssClass="input-width-xs" Label="Payment Reminder Time Span" />
                                </div>
                                <div class="col-md-6">
                                </div>
                            </div>
                        </Rock:PanelWidget>

                        <Rock:PanelWidget ID="wpWaitListTransition" runat="server" Title="Wait List Transition Email">
                            <div class="alert alert-info">
                                This email template will be used when the individual needs to be notified that they are no longer on the wait list and have been transitioned to a full registrant.
                            </div>
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockTextBox ID="tbWaitListTransitionFromName" runat="server" Label="From Name" />
                                    <Rock:RockTextBox ID="tbWaitListTransitionFromEmail" runat="server" Label="From Email" />
                                </div>
                                <div class="col-md-6">
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-12">
                                    <Rock:RockTextBox ID="tbWaitListTransitionSubject" runat="server" Label="Subject" />
                                    <Rock:CodeEditor ID="ceWaitListTransitionEmailTemplate" runat="server" Label="Email Template" EditorMode="Lava" EditorTheme="Rock" EditorHeight="300" />
                                </div>
                            </div>
                        </Rock:PanelWidget>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>
                </div>

                <fieldset id="fieldsetViewDetails" runat="server">

                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockLiteral ID="lGroupType" runat="server" Label="Group Type" />
                            <Rock:RockLiteral ID="lWorkflowType" runat="server" Label="Registration Workflow" />
                            <Rock:RockLiteral ID="lRequiredSignedDocument" runat="server" Label="Required Signature Document" />
                            <Rock:RockControlWrapper ID="rcwRegistrantFormsSummary" runat="server" Label="Registrant Forms" CssClass="js-expandable-summary-wrapper">
                                <div class="js-expandable-summary" style="display: none">
                                    <asp:Literal ID="lRegistrantFormsSummary" runat="server" />
                                </div>
                            </Rock:RockControlWrapper>
                            <Rock:RockControlWrapper ID="rcwRegistrationAttributesSummary" runat="server" Label="Registration Attributes" CssClass="js-expandable-summary-wrapper">
                                <div class="js-expandable-summary" style="display: none">
                                    <asp:Literal ID="lRegistrationAttributesSummary" runat="server" />
                                </div>
                            </Rock:RockControlWrapper>
                        </div>
                        <div class="col-md-6">
                            <Rock:RockLiteral ID="lCost" runat="server" Label="Cost" />
                            <Rock:RockLiteral ID="lMinimumInitialPayment" runat="server" Label="Minimum Initial Payment" />
                            <Rock:RockLiteral ID="lDefaultPaymentAmount" runat="server" Label="Default Payment Amount" />
                            <Rock:RockControlWrapper ID="rcwFees" runat="server" Label="Fees">
                                <asp:Repeater ID="rFees" runat="server">
                                    <ItemTemplate>
                                        <div class="row">
                                            <div class="col-xs-4"><%# Eval("Name") %></div>
                                            <div class="col-xs-8"><%# FormatFeeItems( Eval("FeeItems") as ICollection<Rock.Model.RegistrationTemplateFeeItem> ) %></div>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </Rock:RockControlWrapper>
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-12">
                            <Rock:RockLiteral ID="lDescription" runat="server" Label="Description" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" AccessKey="m" ToolTip="Alt+m" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <Rock:HiddenFieldWithClass ID="hfHasRegistrations" runat="server" CssClass="js-has-registrations" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link js-delete-template" OnClick="btnDelete_Click" CausesValidation="false" />
                        <span class="pull-right">
                            <asp:LinkButton ID="btnPlacements" runat="server" CssClass="btn btn-default btn-sm btn-square" OnClick="btnPlacements_Click">
                                <i class="fa fa-random"></i>
                            </asp:LinkButton>
                            <asp:LinkButton ID="btnCopy" runat="server" CssClass="btn btn-default btn-sm btn-square" Text="<i class='fa fa-clone'></i>" OnClick="btnCopy_Click" />
                            <Rock:SecurityButton ID="btnSecurity" runat="server" class="btn btn-sm btn-square btn-security" />
                        </span>
                    </div>
                </fieldset>
            </div>
        </asp:Panel>

        <asp:HiddenField ID="hfActiveDialogID" runat="server" />

        <%-- Field Filter Dialog --%>
        <Rock:ModalDialog ID="dlgFieldFilter" runat="server" Title="Form Field Filter" OnSaveClick="dlgFieldFilter_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="FieldFilter">
            <Content>
                <asp:HiddenField ID="hfFormGuidFilter" runat="server" />
                <asp:HiddenField ID="hfFormFieldGuidFilter" runat="server" />
                <asp:ValidationSummary ID="ValidationSummaryFieldFilter" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="FieldFilter" />

                <Rock:FieldVisibilityRulesEditor ID="fvreFieldVisibilityRulesEditor" runat="server" />

            </Content>
        </Rock:ModalDialog>

        <%-- Registrant Form Field Dialog --%>
        <Rock:ModalDialog ID="dlgRegistrantFormField" runat="server" Title="Registrant Form Field" OnSaveClick="dlgRegistrantFormField_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="Field">
            <Content>
                <asp:HiddenField ID="hfFormGuid" runat="server" />
                <asp:HiddenField ID="hfAttributeGuid" runat="server" />
                <Rock:NotificationBox ID="nbFormField" runat="server" Visible="false" NotificationBoxType="Validation"></Rock:NotificationBox>
                <asp:ValidationSummary ID="ValidationSummaryAttribute" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="Field" />
                <div class="row">
                    <div class="col-md-3">
                        <Rock:RockLiteral ID="lFieldSource" runat="server" Label="Source" Visible="false" />
                        <Rock:RockDropDownList ID="ddlFieldSource" runat="server" Label="Source" AutoPostBack="true" OnSelectedIndexChanged="ddlFieldSource_SelectedIndexChanged" ValidationGroup="Field" />
                        <Rock:RockLiteral ID="lPersonField" runat="server" Label="Person Field" Visible="false" />
                        <Rock:RockDropDownList ID="ddlPersonField" runat="server" Label="Person Field" Visible="false" ValidationGroup="Field" />
                        <Rock:RockDropDownList ID="ddlPersonAttributes" runat="server" Label="Person Attribute" Visible="false" ValidationGroup="Field" EnhanceForLongLists="true" />
                        <Rock:RockDropDownList ID="ddlGroupTypeAttributes" runat="server" Label="Group Member Attribute" Visible="false" ValidationGroup="Field" EnhanceForLongLists="true" Required="true" DisplayRequiredIndicator="true" />
                    </div>
                    <div class="col-md-3">
                        <Rock:RockCheckBox ID="cbInternalField" runat="server" Label="Internal Field" Text="Yes" Visible="false" ValidationGroup="Field"
                            Help="Should this field be hidden on the public registration page(s) and only visible internally?" />
                        <Rock:RockCheckBox ID="cbRequireInInitialEntry" runat="server" Label="Required" Text="Yes" Visible="false" ValidationGroup="Field"
                            Help="Should a value for this attribute be required when registering?" />
                    </div>
                    <div class="col-md-3">
                        <Rock:RockCheckBox ID="cbCommonValue" runat="server" Label="Common Value" Text="Yes" Visible="false" ValidationGroup="Field"
                            Help="When registering more than one person, should the value of this attribute default to the value entered for first person registered?" />
                        <Rock:RockCheckBox ID="cbShowOnGrid" runat="server" Label="Show on Grid" Text="Yes" Visible="false" ValidationGroup="Field"
                            Help="Should this value be displayed on the list of registrants? Note: Some person fields cannot be shown in grids, and group member attributes are only shown in group member lists." />
                    </div>
                    <div class="col-md-3">
                        <Rock:RockCheckBox ID="cbUsePersonCurrentValue" runat="server" Label="Use Current Value" Text="Yes" Visible="false" ValidationGroup="Field"
                            Help="Should the person's current value for this field be displayed when they register?" />
                        <Rock:RockCheckBox ID="cbShowOnWaitList" runat="server" Label="Show On Wait List" Text="Yes" Visible="true" ValidationGroup="Field"
                            Help="Should this field be shown for a person registering on the waitlist?" />
                        <Rock:RockCheckBox ID="cbLockExistingValue" runat="server" Label="Lock Existing Value" Text="Yes" Visible="true" ValidationGroup="Field"
                            Help="When enabled, this option restricts editing the field when a value is already on the person's record." />
                    </div>
                </div>
                <Rock:AttributeEditor ID="edtRegistrantAttribute" runat="server" ShowActions="false" ValidationGroup="Field" Visible="false" />
                <Rock:CodeEditor ID="ceFormFieldPreHtml" runat="server" Label="Pre-HTML" EditorMode="Lava" EditorTheme="Rock" EditorHeight="100" ValidationGroup="Field" />
                <Rock:CodeEditor ID="ceFormFieldPostHtml" runat="server" Label="Post-HTML" EditorMode="Lava" EditorTheme="Rock" EditorHeight="100" ValidationGroup="Field" />
            </Content>
        </Rock:ModalDialog>

        <%-- Discounts Dialog --%>
        <Rock:ModalDialog ID="dlgDiscount" runat="server" Title="Discount Code" OnSaveClick="dlgDiscount_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="Discount">
            <Content>
                <asp:HiddenField ID="hfDiscountGuid" runat="server" />
                <asp:ValidationSummary ID="ValidationSummaryDiscount" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="Discount" />
                <Rock:NotificationBox ID="nbDuplicateDiscountCode" runat="server" NotificationBoxType="Warning" Visible="false" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbDiscountCode" runat="server" CssClass="input-width-xl" Label="Discount Code" ValidationGroup="Discount" Required="true" />
                        <Rock:RockRadioButtonList ID="rblDiscountType" runat="server" Label="Discount Type" RepeatDirection="Horizontal" AutoPostBack="true" OnSelectedIndexChanged="rblDiscountType_SelectedIndexChanged">
                            <asp:ListItem Text="Percentage" Value="Percentage" />
                            <asp:ListItem Text="Amount" Value="Amount" />
                        </Rock:RockRadioButtonList>
                        <Rock:NumberBox ID="nbDiscountPercentage" runat="server" AppendText="%" CssClass="input-width-md" Label="Discount Percentage" NumberType="Integer" ValidationGroup="Discount" />
                        <Rock:CurrencyBox ID="cbDiscountAmount" runat="server" CssClass="input-width-md" Label="Discount Amount" ValidationGroup="Discount" />
                        <Rock:RockCheckBox ID="cbcAutoApplyDiscount" runat="server" Label="Auto Apply Discount" Help="Will automatically apply the discount if the registration meets the criteria. If multiple automatic discounts exist, only the first one that meets the criteria will be applied." />
                    </div>
                    <div class="col-md-6">
                        <Rock:NumberBox ID="nbDiscountMaxUsage" runat="server" NumberType="Integer" MinimumValue="0" Label="Maximum Usage" Help="The maximum number of registrations that the discount code can be used on (leave blank for none)." />
                        <Rock:NumberBox ID="nbDiscountMaxRegistrants" runat="server" NumberType="Integer" MinimumValue="0" Label="Maximum Registrants" Help="The maximum number of registrants in a single registration that the discount code can be used on." />
                        <Rock:NumberBox ID="nbDiscountMinRegistrants" runat="server" NumberType="Integer" MinimumValue="0" Label="Minimum Registrants" Help="The minimum number of registrants in the registration that are required in order to use the discount code." />
                        <Rock:DateRangePicker ID="drpDiscountDateRange" runat="server" Label="Effective Dates" Help="The beginning and/or ending date that the discount code can be used." />
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>

        <%-- Placement Configuration Dialog --%>
        <Rock:ModalDialog ID="dlgPlacementConfiguration" runat="server" Title="Placement Configuration" OnSaveClick="dlgPlacementConfiguration_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="PlacementConfiguration">
            <Content>
                <asp:HiddenField ID="hfRegistrationPlacementConfigurationGuid" runat="server" />
                <asp:ValidationSummary ID="vsPlacementConfiguration" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="PlacementConfiguration" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbPlacementConfigurationName" runat="server" Label="Name" Required="true" MaxLength="100" ValidationGroup="PlacementConfiguration" />
                    </div>
                    <div class="col-md-6">
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <Rock:GroupTypePicker ID="gtpPlacementConfigurationGroupTypeEdit" runat="server" Label="Group Type" Help="The group type to limit placement groups by." ValidationGroup="PlacementConfiguration" Required="true" />
                        <Rock:RockLiteral ID="lPlacementConfigurationGroupTypeReadOnly" runat="server" Label="Group Type" />
                        <Rock:RockCheckBox ID="cbPlacementConfigurationAllowMultiple" runat="server" Label="Allow Multiple" Help="Determines if a registrant can be in more than one group for this placement." />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbPlacementConfigurationIconCssClass" runat="server" Label="Icon CSS Class" Help="CSS Icon to use for the placement. If not provided, the icon for the group type will be used." ValidationGroup="PlacementConfiguration" />
                    </div>
                </div>

                <Rock:RockControlWrapper ID="rcwPlacementConfigurationSharedGroups" runat="server" Label="Shared Groups">
                    <asp:HiddenField ID="hfPlacementConfigurationSharedGroupIdList" runat="server" />
                    <p>The groups below will be linked as placement groups to each registration instance of this template.</p>
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:Grid ID="gPlacementConfigurationSharedGroups" runat="server" DisplayType="Light" DataKeyNames="Id" AllowPaging="false" AllowSorting="false">
                                <Columns>
                                    <Rock:RockBoundField DataField="Name" HeaderText="Group" />
                                    <Rock:DeleteField OnClick="gPlacementConfigurationSharedGroups_DeleteClick" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                        <div class="col-md-6">
                            <asp:Panel ID="pnlPlacementConfigurationAddSharedGroup" runat="server" Visible="false">
                                <Rock:NotificationBox ID="nbAddPlacementGroupWarning" runat="server" NotificationBoxType="Warning" />
                                <Rock:GroupPicker ID="gpPlacementConfigurationAddSharedGroup" runat="server" Label="Add Group" />
                                <asp:LinkButton ID="btnPlacementConfigurationAddSharedGroup" runat="server" CssClass="btn btn-xs btn-primary" Text="Add" OnClick="btnPlacementConfigurationAddSharedGroup_Click" />
                                <asp:LinkButton ID="btnPlacementConfigurationAddSharedGroupCancel" runat="server" CssClass="btn btn-xs btn-link" Text="Cancel" OnClick="btnPlacementConfigurationAddSharedGroupCancel_Click" />
                            </asp:Panel>
                        </div>
                    </div>
                </Rock:RockControlWrapper>
            </Content>
        </Rock:ModalDialog>

        <%-- Fees Dialog --%>
        <Rock:ModalDialog ID="dlgFee" runat="server" Title="Fee" OnSaveClick="dlgFee_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="Fee">
            <Content>
                <asp:HiddenField ID="hfFeeGuid" runat="server" />
                <asp:ValidationSummary ID="ValidationSummaryFee" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="Fee" />
                <div class="row">
                    <div class="col-md-12">
                        <Rock:RockTextBox ID="tbFeeName" runat="server" Label="Name" ValidationGroup="Fee" Required="true" MaxLength="100" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockRadioButtonList ID="rblFeeType" runat="server" Label="Options" ValidationGroup="Fee" RepeatDirection="Horizontal" AutoPostBack="true" OnSelectedIndexChanged="rblFeeType_SelectedIndexChanged" />
                        <Rock:NotificationBox ID="nbFeeItemsConfigurationWarning" runat="server" NotificationBoxType="Warning" Visible="false" />

                        <Rock:RockControlWrapper ID="rcwFeeItemsSingle" runat="server" Label="">
                            <asp:HiddenField ID="hfFeeItemSingleGuid" runat="server" />
                            <Rock:CurrencyBox ID="cbFeeItemSingleCost" runat="server" Label="Cost" ValidationGroup="Fee" />
                            <Rock:NumberBox ID="nbFeeItemSingleMaximumUsageCount" runat="server" Label="Maximum Available" Help="The maximum number of times this fee can be used per registration instance." ValidationGroup="Fee" CssClass="input-width-md" />
                        </Rock:RockControlWrapper>
                        <Rock:RockControlWrapper ID="rcwFeeItemsMultiple" runat="server" Label="Costs" Help="Enter the name, cost, and the maximum number of times this fee can be used per registration instance.">
                            <asp:Repeater ID="rptFeeItemsMultiple" runat="server" OnItemDataBound="rptFeeItemsMultiple_ItemDataBound">
                                <ItemTemplate>
                                    <div class="controls controls-row form-control-group">
                                        <%-- Note: If the FeeItem isn't in the database yet, feeItemId will be 0, so use Guid to identify it --%>
                                        <asp:HiddenField ID="hfFeeItemId" runat="server" />

                                        <asp:HiddenField ID="hfFeeItemGuid" runat="server" />
                                        <asp:Panel ID="pnlFeeItemNameContainer" runat="server">
                                            <Rock:NotificationBox ID="nbFeeItemWarning" runat="server" NotificationBoxType="Default" />
                                            <Rock:RockTextBox ID="tbFeeItemName" runat="server" CssClass="input-width-md margin-b-sm" Placeholder="Option" ValidationGroup="Fee" Required="true" MaxLength="100" />
                                        </asp:Panel>
                                        <Rock:CurrencyBox ID="cbFeeItemCost" runat="server" CssClass="input-width-md margin-b-sm" Placeholder="Cost" ValidationGroup="Fee" NumberType="Currency" Required="false" />
                                        <Rock:NumberBox ID="nbMaximumUsageCount" runat="server" CssClass="input-width-md margin-b-sm" Placeholder="Max Available" ValidationGroup="Fee" Required="false" NumberType="Integer" />
                                        <asp:LinkButton ID="btnDeleteFeeItem" runat="server" CssClass="btn btn-danger btn-sm btn-square margin-b-sm" OnClick="btnDeleteFeeItem_Click"><i class="fa fa-times"></i></asp:LinkButton>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>

                            <div class="actions">
                                <asp:LinkButton ID="btnAddFeeItem" runat="server" CssClass="btn btn-action btn-sm btn-square" OnClick="btnAddFeeItem_Click"><i class="fa fa-plus-circle"></i></asp:LinkButton>
                            </div>
                        </Rock:RockControlWrapper>

                    </div>
                    <div class="col-md-3">
                        <Rock:RockCheckBox ID="cbAllowMultiple" runat="server" Label="Enable Quantity" ValidationGroup="Fee" Text="Yes" Help="Should registrants be able to select more than one of this item?" CssClass="form-check" />
                        <Rock:RockCheckBox ID="cbDiscountApplies" runat="server" Label="Discount Applies" ValidationGroup="Fee" Text="Yes" Help="Should discounts be applied to this fee?" />
                        <Rock:RockCheckBox ID="cbFeeIsActive" runat="server" Label="Is Active" ValidationGroup="Fee" Text="Yes" Help="Unchecking this will remove the fee for new registrations but will not effect the existing registrants." />
                    </div>
                    <div class="col-md-3">
                        <Rock:RockCheckBox ID="cbFeeIsRequired" runat="server" Label="Is Required" ValidationGroup="Fee" Text="Yes" Help="Checking this will mark the fee for new registrations required." />
                        <Rock:RockCheckBox ID="cbHideWhenNoneRemaining" runat="server" Label="Hide When None Remaining" ValidationGroup="Fee" Text="Yes" Help="If checked then items that have 0 remaining will not display. If not checked then the items will display but will not be selectable." />
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>

        <script>

            Sys.Application.add_load(function () {

                var fixHelper = function (e, ui) {
                    ui.children().each(function () {
                        $(this).width($(this).width());
                    });
                    return ui;
                };

                $('#cb-showdetails').on('change', function () {
                    $('#registration-details').slideDown();
                    $('#registration-detailscheckbox').slideUp();
                });

                $('.js-expandable-summary-wrapper > label.control-label').on('click', function () {
                    $(this).closest('.js-expandable-summary-wrapper').find('.js-expandable-summary').toggle(500);
                })

                // NOTE: js-optional-form-list is a div created in codebehind around the optional forms
                var $formList = $('.js-optional-form-list');

                if ($formList.length > 0) {
                    $formList.sortable({
                        helper: fixHelper,
                        handle: '.form-reorder',
                        containment: 'parent',
                        tolerance: 'pointer',
                        start: function (event, ui) {
                            {
                                var start_pos = ui.item.index();
                                ui.item.data('start_pos', start_pos);
                            }
                        },
                        update: function (event, ui) {
                            {
                                var postbackArg = 're-order-form:' + ui.item.attr('data-key') + ';' + ui.item.index();
                                window.location = "javascript:__doPostBack('<%=upDetail.ClientID %>', '" +  postbackArg + "')";
                            }
                        }
                    });
                }

            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
