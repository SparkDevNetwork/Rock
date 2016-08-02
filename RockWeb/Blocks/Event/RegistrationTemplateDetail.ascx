<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RegistrationTemplateDetail.ascx.cs" Inherits="RockWeb.Blocks.Event.RegistrationTemplateDetail" %>


<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>
              
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server" Visible="false">
            <asp:HiddenField ID="hfRegistrationTemplateId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-clipboard"></i> <asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>
                
                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlInactive" runat="server" LabelType="Danger" Text="Inactive" />
                    <Rock:HighlightLabel ID="hlType" runat="server" LabelType="Type" />
                </div>
            </div>
            <div class="panel-body container-fluid">

                <asp:ValidationSummary ID="vsDetails" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                <Rock:NotificationBox ID="nbValidationError" runat="server" NotificationBoxType="Danger" Heading="Please Correct the Following" Visible="false" />

                <div id="pnlEditDetails" runat="server">

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.RegistrationTemplate, Rock" PropertyName="Name" />
                            <Rock:CategoryPicker ID="cpCategory" runat="server" Required="true" Label="Category" EntityTypeName="Rock.Model.RegistrationTemplate" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbIsActive" runat="server" Text="Active" />
                        </div>
                    </div>

                    <Rock:PanelWidget ID="pwDetails" runat="server" Title="Details" Expanded="true">
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:GroupTypePicker ID="gtpGroupType" runat="server" Label="Group Type" AutoPostBack="true" OnSelectedIndexChanged="gtpGroupType_SelectedIndexChanged" />
                                <Rock:GroupRolePicker ID="rpGroupTypeRole" runat="server" Label="Group Member Role"
                                    Help="The group member role that new registrants should be added to group with." />
                                <Rock:RockDropDownList ID="ddlGroupMemberStatus" runat="server" Label="Group Member Status" 
                                    Help="The group member status that new registrants should be added to group with."/>
                                
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
                                <div class="row">
                                    <div class="col-xs-6">
                                        <Rock:RockCheckBox ID="cbLoginRequired" runat="server" Label="Login Required" Text="Yes"
                                            Help="Is user required to be logged in when registering?" />
                                    </div>
                                    <div class="col-xs-6">
                                        <Rock:RockCheckBox ID="cbAllowGroupPlacement" runat="server" Label="Allow Group Placement" Text="Yes"
                                            Help="If enabled, the registration instance will include a Group Placement option for 
                                                adding registrants to specific child groups of a selected parent group." />
                                    </div>
                                </div>

                                <Rock:RockCheckBox ID="cbAllowExternalUpdates" runat="server" Label="Allow External Updates to Saved Registrations" Text="Yes"
                                            Help="Allow saved registrations to be updated online. If false the individual will be able to make additional payments, but will
                                            not be allow to change any of the registrant information and attributes." />

                                <Rock:WorkflowTypePicker ID="wtpRegistrationWorkflow" runat="server" Label="Registration Workflow"
                                    Help="An optional workflow type to launch when a new registration is completed." />

                                <Rock:RockDropDownList ID="ddlSignatureDocumentTemplate" runat="server" Label="Required Signature Document" 
                                    Help="A document that needs to be signed for registrations of this type."/>

                            </div>
                            <div class="col-md-6">
                                <div class="row">
                                    <div class="col-xs-6">
                                        <Rock:RockCheckBox ID="cbMultipleRegistrants" runat="server" Label="Allow Multiple Registrants" Text="Yes"
                                            Help="Should user be allowed to register multiple registrants at the same time?"
                                            AutoPostBack="true" OnCheckedChanged="cbMultipleRegistrants_CheckedChanged" />
                                    </div>
                                    <div class="col-xs-6">
                                        <Rock:NumberBox ID="nbMaxRegistrants" runat="server" Label="Maximum Registrants"
                                            Help="The maximum number of registrants that user is allowed to register" Visible="false" />
                                    </div>
                                </div>
                                <Rock:RockRadioButtonList ID="rblRegistrantsInSameFamily" runat="server" Label="Registrants in same Family" RepeatDirection="Horizontal"
                                    Help="Typical relationship of registrants that user would register." />
                                <div class="well">
                                    <Rock:Toggle ID="tglSetCostOnTemplate" runat="server" Label="Set Cost On" OnText="Template" OffText="Instance" 
                                        ActiveButtonCssClass="btn-info" OnCheckedChanged="tglSetCost_CheckedChanged" ButtonSizeCssClass="btn-xs" />
                                    <Rock:CurrencyBox ID="cbCost" runat="server" Label="Cost"
                                        Help="The cost per registrant." />
                                    <Rock:CurrencyBox ID="cbMinimumInitialPayment" runat="server" Label="Minimum Initial Payment"
                                        Help="The minimum amount required per registrant. Leave value blank if full amount is required." />
                                    <Rock:FinancialGatewayPicker ID="fgpFinancialGateway" runat="server" Label="Financial Gateway"
                                        Help="The financial gateway to use for processing registration payments." />
                                    <Rock:RockTextBox ID="txtBatchNamePrefix" runat="server" Label="Batch Prefix" Help="Optional prefix to add the the financial batches. If left blank the prefix from the registration block will be used." />
                                </div>
                            </div>
                        </div>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="wpPersonFields" runat="server" Title="Form(s)">
                        <Rock:PanelWidget ID="wpDefaultForm" runat="server" Title="Default Form" Expanded="true">
                            <Rock:Grid ID="gFields" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Field">
                                <Columns>
                                    <Rock:ReorderField />
                                    <Rock:RockBoundField DataField="Name" HeaderText="Field" />
                                    <Rock:EnumField DataField="FieldSource" HeaderText="Source" />
                                    <Rock:FieldTypeField DataField="FieldType" HeaderText="Type" />
                                    <Rock:BoolField DataField="IsInternal" HeaderText="Internal" />
                                    <Rock:BoolField DataField="IsSharedValue" HeaderText="Common" />
                                    <Rock:BoolField DataField="ShowCurrentValue" HeaderText="Use Current Value" />
                                    <Rock:BoolField DataField="IsRequired" HeaderText="Required" />
                                    <Rock:BoolField DataField="IsGridField" HeaderText="Show on Grid" />
                                    <Rock:EditField OnClick="gFields_Edit" />
                                    <Rock:DeleteField OnClick="gFields_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </Rock:PanelWidget>
                        <div class="form-list">
                            <asp:PlaceHolder ID="phForms" runat="server" />
                        </div>
                        <div class="pull-right">
                            <asp:LinkButton ID="lbAddForm" runat="server" CssClass="btn btn-action btn-xs" OnClick="lbAddForm_Click" CausesValidation="false"><i class="fa fa-plus"></i> Add Form</asp:LinkButton>
                        </div>

                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="wpFees" runat="server" Title="Fees">
                        <Rock:Grid ID="gFees" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Fees">
                            <Columns>
                                <Rock:ReorderField />
                                <Rock:RockBoundField DataField="Name" HeaderText="Fee" />
                                <Rock:EnumField DataField="FeeType" HeaderText="Options" />
                                <Rock:RockBoundField DataField="Cost" HeaderText="Cost" />
                                <Rock:BoolField DataField="AllowMultiple" HeaderText="Enable Quantity" />
                                <Rock:BoolField DataField="DiscountApplies" HeaderText="Discount Applies" />
                                <Rock:EditField OnClick="gFees_Edit" />
                                <Rock:DeleteField OnClick="gFees_Delete" />
                            </Columns>
                        </Rock:Grid>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="wpDiscounts" runat="server" Title="Discounts">
                        <Rock:Grid ID="gDiscounts" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Discounts">
                            <Columns>
                                <Rock:ReorderField />
                                <Rock:RockBoundField DataField="Code" HeaderText="Code" />
                                <Rock:RockBoundField DataField="Discount" HeaderText="Discount" ItemStyle-HorizontalAlign="Right" />
                                <Rock:EditField OnClick="gDiscounts_Edit" />
                                <Rock:DeleteField OnClick="gDiscounts_Delete" />
                            </Columns>
                        </Rock:Grid>
                    </Rock:PanelWidget>

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
                                <Rock:CodeEditor ID="ceConfirmationEmailTemplate" runat="server" Label="Confirmation Email Template" EditorMode="Lava" EditorTheme="Rock" EditorHeight="300" />
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
                                <Rock:CodeEditor ID="ceReminderEmailTemplate" runat="server" Label="Reminder Email Template" EditorMode="Lava" EditorTheme="Rock" EditorHeight="300" />
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
                                <Rock:CodeEditor ID="cePaymentReminderEmailTemplate" runat="server" Label="Confirmation Email Template" EditorMode="Lava" EditorTheme="Rock" EditorHeight="300" />
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

                    <Rock:PanelWidget ID="wpTerms" runat="server" Title="Terms/Text">
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockTextBox ID="tbRegistrationTerm" runat="server" Label="Registration Term" Placeholder="Registration" />
                                <Rock:RockTextBox ID="tbRegistrantTerm" runat="server" Label="Registrant Term" Placeholder="Registrant" />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockTextBox ID="tbFeeTerm" runat="server" Label="Fee Term" Placeholder="Additional Options" />
                                <Rock:RockTextBox ID="tbDiscountCodeTerm" runat="server" Label="Discount Code Term" Placeholder="Discount Code" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-12">
                                <Rock:RockTextBox ID="tbSuccessTitle" runat="server" Label="Success Title" Placeholder="Congratulations"
                                    Help="The heading to display to user after successfully completing a registration of this type." />
                                <Rock:CodeEditor ID="ceSuccessText" runat="server" Label="Success Text" EditorMode="Lava" EditorTheme="Rock" EditorHeight="300" 
                                    Help="The text to display to user after successfully completing a registration of this type. If there are costs or fees for this registration, a summary of those will be displayed after this text." />
                            </div>
                        </div>
                    </Rock:PanelWidget>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>

                </div>

                <fieldset id="fieldsetViewDetails" runat="server">

                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockLiteral ID="lGroupType" runat="server" Label="Group Type" />
                            <Rock:RockLiteral ID="lWorkflowType" runat="server" Label="Registration Workflow" />
                            <Rock:RockLiteral ID="lRequiredSignedDocument" runat="server" Label="Required Signed Document" />
                            <Rock:RockControlWrapper ID="rcwForms" runat="server" Label="Forms" CssClass="js-forms-wrapper">
                                <div class="forms-readonly-list" style="display: none">
                                    <asp:Literal ID="lFormsReadonly" runat="server" />
                                </div>
                            </Rock:RockControlWrapper>
                        </div>
                        <div class="col-md-6">
                            <Rock:RockLiteral ID="lCost" runat="server" Label="Cost" />
                            <Rock:RockLiteral ID="lMinimumInitialPayment" runat="server" Label="Minimum Initial Payment" />
                            <Rock:RockControlWrapper ID="rcwFees" runat="server" Label="Fees">
                                <asp:Repeater ID="rFees" runat="server">
                                    <ItemTemplate>
                                        <div class="row">
                                            <div class="col-xs-4"><%# Eval("Name") %></div>
                                            <div class="col-xs-8"><%# FormatFeeCost( Eval("CostValue").ToString() ) %></div>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </Rock:RockControlWrapper>
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" AccessKey="m" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <Rock:HiddenFieldWithClass ID="hfHasRegistrations" runat="server" CssClass="js-has-registrations" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link js-delete-template" OnClick="btnDelete_Click" CausesValidation="false" />
                        <span class="pull-right">
                            <asp:LinkButton ID="btnCopy" runat="server" CssClass="btn btn-default btn-sm fa fa-clone" OnClick="btnCopy_Click"/>
                            <Rock:SecurityButton ID="btnSecurity" runat="server" class="btn btn-sm btn-security" />
                        </span>

                    </div>
                
                </fieldset>

            </div>

        </asp:Panel>

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="dlgField" runat="server" Title="Form Field" OnSaveClick="dlgField_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="Field">
            <Content>
                <asp:HiddenField ID="hfFormGuid" runat="server" />
                <asp:HiddenField ID="hfAttributeGuid" runat="server" />
                <asp:ValidationSummary ID="ValidationSummaryAttribute" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="Field" />
                <div class="row">
                    <div class="col-md-3">
                        <Rock:RockDropDownList ID="ddlFieldSource" runat="server" Label="Source" AutoPostBack="true" OnSelectedIndexChanged="ddlFieldSource_SelectedIndexChanged" ValidationGroup="Field" />
                        <Rock:RockLiteral ID="lPersonField" runat="server" Label="Person Field" Visible="false" />
                        <Rock:RockDropDownList ID="ddlPersonField" runat="server" Label="Person Field" Visible="false" ValidationGroup="Field" />
                        <Rock:RockDropDownList ID="ddlPersonAttributes" runat="server" Label="Person Attribute" Visible="false" ValidationGroup="Field" />
                        <Rock:RockDropDownList ID="ddlGroupTypeAttributes" runat="server" Label="Group Member Attribute" Visible="false" ValidationGroup="Field" />
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
                            Help="Should this value be displayed on the list of registrants?" />
                    </div>
                    <div class="col-md-3">
                        <Rock:RockCheckBox ID="cbUsePersonCurrentValue" runat="server" Label="Use Current Value" Text="Yes" Visible="false" ValidationGroup="Field"
                            Help="Should the person's current value for this field be displayed when they register?" />
                    </div>
                </div>
                <Rock:AttributeEditor ID="edtRegistrationAttribute" runat="server" ShowActions="false" ValidationGroup="Field" Visible="false" />
                <Rock:CodeEditor ID="ceAttributePreText" runat="server" Label="Pre-Text" EditorMode="Html" EditorTheme="Rock" EditorHeight="100" ValidationGroup="Field" />
                <Rock:CodeEditor ID="ceAttributePostText" runat="server" Label="Post-Text" EditorMode="Html" EditorTheme="Rock" EditorHeight="100" ValidationGroup="Field" />
           </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgDiscount" runat="server" Title="Discount Code" OnSaveClick="dlgDiscount_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="Discount">
            <Content>
                <asp:HiddenField ID="hfDiscountGuid" runat="server" />
                <asp:ValidationSummary ID="ValidationSummaryDiscount" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="Discount" />
                <Rock:RockTextBox ID="tbDiscountCode" runat="server" CssClass="input-width-xl" Label="Discount Code" ValidationGroup="Discount" Required="true" />
                <Rock:RockRadioButtonList ID="rblDiscountType" runat="server" Label="Discount Type" RepeatDirection="Horizontal" AutoPostBack="true" OnSelectedIndexChanged="rblDiscountType_SelectedIndexChanged">
                    <asp:ListItem Text="Percentage" Value="Percentage" />
                    <asp:ListItem Text="Amount" Value="Amount" />
                </Rock:RockRadioButtonList>
                <Rock:NumberBox ID="nbDiscountPercentage" runat="server" AppendText="%" CssClass="input-width-md" Label="Discount Percentage" NumberType="Integer" ValidationGroup="Discount"  />
                <Rock:CurrencyBox ID="cbDiscountAmount" runat="server" CssClass="input-width-md" Label="Discount Amount" ValidationGroup="Discount" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgFee" runat="server" Title="Fee" OnSaveClick="dlgFee_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="Fee">
            <Content>
                <asp:HiddenField ID="hfFeeGuid" runat="server" />
                <asp:ValidationSummary ID="ValidationSummaryFee" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="Fee" />
                <Rock:RockTextBox ID="tbFeeName" runat="server" Label="Name" ValidationGroup="Fee" Required="true" />
                <Rock:RockRadioButtonList ID="rblFeeType" runat="server" Label="Options" ValidationGroup="Fee" RepeatDirection="Horizontal" AutoPostBack="true" OnSelectedIndexChanged="rblFeeType_SelectedIndexChanged" />
                <Rock:CurrencyBox ID="cCost" runat="server" Label="Cost" ValidationGroup="Fee" />
                <Rock:KeyValueList ID="kvlMultipleFees" runat="server" Label="Costs" ValidationGroup="Fee" KeyPrompt="Option" ValuePrompt="Cost" />
                <Rock:RockCheckBox ID="cbAllowMultiple" runat="server" Label="Enable Quantity" ValidationGroup="Fee" Text="Yes"
                    Help="Should registrants be able to select more than one of this item?" />
                <Rock:RockCheckBox ID="cbDiscountApplies" runat="server" Label="Discount Applies" ValidationGroup="Fee" Text="Yes"
                    Help="Should discounts be applied to this fee?" />
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

                $('.js-forms-wrapper > label.control-label').click( function(){
                    $('.forms-readonly-list').toggle(500);
                })

                $('.form-list').sortable({
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
                            __doPostBack('<%=upDetail.ClientID %>', 're-order-form:' + ui.item.attr('data-key') + ';' + ui.item.index());
                        }
                    }
                });

            });

        </script>
    </ContentTemplate>
</asp:UpdatePanel>
