<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CommunicationEntryWizard.ascx.cs" Inherits="RockWeb.Blocks.Communication.CommunicationEntryWizard" %>

<asp:UpdatePanel ID="upnlContent" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
    <ContentTemplate>
        <asp:HiddenField ID="hfCommunicationId" runat="server" />
        <asp:HiddenField ID="hfNavigationHistoryInstance" runat="server" Value="" />
        <asp:HiddenField ID="hfSelectedCommunicationTemplateId" runat="server" />
        <asp:HiddenField ID="hfEmailEditorHtml" runat="server" />
        <asp:HiddenField ID="hfEmailEditorHtml_dvrm" runat="server" Value="True" />
        <asp:HiddenField ID="hfDisablePredictableIds" runat="server" Value="" />


        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading panel-follow">
                <h1 class="panel-title"><i class="fa fa-comment"></i>&nbsp;<asp:Literal ID="lTitle" runat="server" /></h1>

                <div class="pull-right">
                    <asp:Panel ID="pnlHeadingLabels" runat="server">
                        <div class="label label-default">
                            <asp:LinkButton ID="btnUseSimpleEditor" runat="server" Text="Use Simple Editor" OnClick="btnUseSimpleEditor_Click" />
                        </div>
                    </asp:Panel>

                    <%-- Put the email send test and preview button in an updatepanel to avoid flicker with the email editor --%>
                    <asp:UpdatePanel ID="upEmailSendTest" runat="server" Visible="false">
                        <ContentTemplate>
                            <Rock:NotificationBox ID="nbEmailTestResult" CssClass="margin-t-md" runat="server" NotificationBoxType="Success" Text="Test Email has been sent." Visible="false" Dismissable="true" />
                            <a class="btn btn-xs btn-default js-email-sendtest" href="#">Send Test</a>
                            <asp:LinkButton ID="btnEmailPreview" runat="server" CssClass="btn btn-xs btn-default js-saveeditorhtml" Text="Preview" OnClick="btnEmailPreview_Click" />
                            <asp:LinkButton ID="btnEmailEditorSaveDraft" runat="server" CssClass="btn btn-xs btn-default js-saveeditorhtml" Text="Save" OnClick="btnEmailEditorSaveDraft_Click" />
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
                <div class="rock-fullscreen-toggle js-fullscreen-trigger"></div>
            </div>

            <div class="panel-body">
                <%-- List Selection --%>
                <asp:Panel ID="pnlListSelection" CssClass="js-navigation-panel h-100 d-flex flex-column" runat="server">
                    <div class="panel-fill-body position-relative flex-fill styled-scroll">
                        <div class="position-absolute inset-0 overflow-auto">
                            <div class="panel-body">
                                <div>
                                    <h1 class="step-title text-break">List Selection</h1>

                                    <Rock:NotificationBox ID="nbCommunicationNotWizardCompatible" runat="server" NotificationBoxType="Info">
                                        This communication uses a template that is not compatible with the email wizard. You can continue with the email wizard, but the main content of the email will be replaced when the Email Wizard compatible template is selected. To keep the content, click 'Use Simple Editor' to use the simple communication editor.
                                    </Rock:NotificationBox>

                                    <div class="row">
                                        <div class="col-sm-8">
                                            Select the communication list you would like use for your audience. You can further customize your recipient list by applying segments.
                                        </div>
                                        <div class="col-sm-4">
                                            <asp:LinkButton
                                                ID="btnRecipientList"
                                                CausesValidation="false"
                                                runat="server"
                                                CssClass="btn btn-link btn-xs text-primary pull-right"
                                                OnClick="btnRecipientList_Click" />
                                        </div>
                                    </div>

                                    <hr />
                                </div>

                                <asp:ValidationSummary ID="vsRecipientSelection" runat="server" HeaderText="Please correct the following:" ValidationGroup="vgRecipientSelection" CssClass="alert alert-validation" />

                                <Rock:NotificationBox ID="nbRecipientsAlert" runat="server" NotificationBoxType="Validation" />

                                <div class="row">
                                    <asp:Panel ID="pnlListSelectionList" runat="server" CssClass="col-lg-8">
                                        <Rock:RockDropDownList
                                            ID="ddlCommunicationGroupList"
                                            runat="server"
                                            Label="Communication List"
                                            CssClass="input-width-xxl"
                                            ValidationGroup="vgRecipientSelection"
                                            Required="true"
                                            OnSelectedIndexChanged="ddlCommunicationGroupList_SelectedIndexChanged"
                                            AutoPostBack="true" />

                                        <asp:Panel ID="pnlCommunicationGroupSegments" runat="server">
                                            <label class="control-label mb-0">Segments</label>
                                            <p class="text-sm">Optionally, further refine your recipients by filtering by segment.</p>
                                            <div class="margin-b-lg">
                                                <Rock:RockCheckBoxList
                                                    ID="cblCommunicationGroupSegments"
                                                    runat="server"
                                                    RepeatDirection="Horizontal"
                                                    ValidationGroup="vgRecipientSelection"
                                                    OnSelectedIndexChanged="cblCommunicationGroupSegments_SelectedIndexChanged"
                                                    AutoPostBack="true" />
                                            </div>

                                            <Rock:RockRadioButtonList
                                                ID="rblCommunicationGroupSegmentFilterType"
                                                runat="server"
                                                Label="Recipients Must Meet"
                                                RepeatDirection="Horizontal"
                                                ValidationGroup="vgRecipientSelection"
                                                AutoPostBack="true"
                                                OnSelectedIndexChanged="rblCommunicationGroupSegmentFilterType_SelectedIndexChanged" />

                                            <asp:Panel ID="pnlRecipientFromListCount" runat="server" CssClass="label label-info">
                                                <asp:Literal ID="lRecipientFromListCount" runat="server" Text="" />
                                            </asp:Panel>
                                        </asp:Panel>
                                    </asp:Panel>
                                    <div class="col-lg-6">
                                        <Rock:RockCheckBox
                                            ID="cbDuplicatePreventionOption"
                                            runat="server"
                                            Label="Prevent Duplicate Email/SMS Addresses"
                                            Help="Check this option to prevent communications from being sent to people with the same email/SMS addresses.
                                            This will mean two people who share an address will not receive a personalized communication, only one of them will." />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>


                    <div class="actions mt-3">
                        <asp:LinkButton
                            ID="btnRecipientSelectionNext"
                            runat="server"
                            data-shortcut-key="arrowright"
                            Text="Next"
                            ToolTip="Alt+🡆"
                            DataLoadingText="Next"
                            CssClass="btn btn-primary pull-right js-wizard-navigation"
                            ValidationGroup="vgRecipientSelection"
                            CausesValidation="true"
                            OnClick="btnRecipientSelectionNext_Click" />
                    </div>

                     <%-- Recipient Selection: Communication List Recipients Modal --%>
                    <Rock:ModalDialog Id="mdCommunicationListRecipients" runat="server" Title="Communication List Recipients">
                        <Content>
                            <Rock:NotificationBox ID="nbListWarning" runat="server" NotificationBoxType="Info" />
                            <Rock:Grid ID="gRecipientList" runat="server" OnRowDataBound="gRecipientList_RowDataBound">
                                <Columns>
                                    <asp:BoundField DataField="NickName" HeaderText="First Name" SortExpression="NickName" />
                                    <asp:BoundField DataField="LastName" HeaderText="Last Name" SortExpression="LastName" />
                                    <Rock:RockLiteralField ID="lRecipientListAlert" HeaderText="Notes" />
                                    <Rock:RockLiteralField ID="lRecipientListAlertEmail" HeaderText="Email" />
                                    <Rock:RockLiteralField ID="lRecipientListAlertSMS" HeaderText="SMS" />
                                </Columns>
                            </Rock:Grid>
                        </Content>
                    </Rock:ModalDialog>

                </asp:Panel>

                <%-- Recipient List --%>
                <asp:Panel ID="pnlIndividualRecipientPanel" CssClass="js-navigation-panel d-flex flex-column h-100" runat="server" Visible="false">
                    <div>
                        <h1 class="step-title text-break">Recipient List</h1>
                        <p>
                            <asp:Literal ID="lIndividualRecipientPanelCaption" runat="server" />
                        </p>
                        <hr />

                        <asp:ValidationSummary
                            ID="vsIndividualRecipientList"
                            runat="server"
                            HeaderText="Please correct the following:"
                            ValidationGroup="vsIndividualRecipientList"
                            CssClass="alert alert-warning" />

                        <Rock:NotificationBox
                            ID="nbIndividualListWarning"
                            runat="server"
                            NotificationBoxType="Validation" />

                        <div class="d-flex margin-b-md">
                            <div class="mr-sm-auto">
                                <Rock:PersonPicker
                                    ID="ppAddPerson"
                                    runat="server"
                                    CssClass="picker-menu-left"
                                    Label="Person"
                                    PersonName="Add Person"
                                    OnSelectPerson="ppAddPerson_SelectPerson"
                                    EnableSelfSelection="true" />
                            </div>
                            <div class="ml-sm-auto mt-auto form-group">
                                <asp:Panel ID="pnlIndividualRecipientListCount" runat="server" CssClass="label label-info">
                                    <asp:Literal ID="lIndividualRecipientListCount" runat="server" Text="" />
                                </asp:Panel>
                            </div>
                        </div>
                    </div>

                    <%-- Recipient List Panel --%>
                    <asp:Panel ID="pnlIndividualRecipientList" runat="server" Visible="false">
                        <div class="position-relative flex-fill styled-scroll" style="min-height: 200px;">
                            <Rock:Grid
                                ID="gIndividualRecipients"
                                runat="server"
                                DisplayType="Light"
                                OnRowDataBound="gIndividualRecipients_RowDataBound"
                                HideDeleteButtonForIsSystem="false"
                                ShowConfirmDeleteDialog="false">
                                <Columns>
                                    <Rock:SelectField></Rock:SelectField>
                                    <asp:BoundField DataField="FullName" HeaderText="Name" SortExpression="FullName" />
                                    <Rock:RockLiteralField ID="lRecipientAlertEmail" HeaderText="Email" />
                                    <Rock:RockLiteralField ID="lRecipientAlertSMS" HeaderText="SMS" />
                                    <Rock:RockLiteralField ID="lRecipientAlert" HeaderText="Notes" />
                                    <Rock:DeleteField OnClick="gIndividualRecipients_DeleteClick" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                        <div class="my-3">
                            <asp:LinkButton ID="btnDeleteSelectedRecipients"
                                runat="server"
                                CssClass="btn btn-xs btn-outline-primary"
                                OnClick="btnDeleteSelectedRecipients_Click"
                                Text="Remove Selected" />
                        </div>
                    </asp:Panel>

                    <%-- Recipient Summary Panel --%>
                    <asp:Panel ID="pnlIndividualRecipientSummary" runat="server" Visible="false" CssClass="margin-t-md">
                        <div class="position-relative flex-fill" style="min-height: 200px;">
                            
                                <asp:Literal ID="lRecipientSummary" runat="server" />
                            
                        </div>
                    </asp:Panel>

                    <%-- Panel Actions  --%>
                    <div class="actions panel-actions-bordered">
                        <asp:LinkButton ID="btnRecipientListNext"
                            runat="server"
                            data-shortcut-key="arrowright"
                            Text="Next"
                            ToolTip="Alt+🡆"
                            DataLoadingText="Next"
                            CssClass="btn btn-primary pull-right js-wizard-navigation"
                            ValidationGroup="vsIndividualRecipientList"
                            CausesValidation="true"
                            OnClick="btnRecipientListNext_Click" />
                    </div>
                </asp:Panel>

                <%-- Communication Delivery, Medium Selection --%>
                <asp:Panel ID="pnlCommunicationDelivery" CssClass="js-navigation-panel h-100 d-flex flex-column" runat="server" Visible="false">
                    <div class="panel-fill-body position-relative flex-fill styled-scroll">
                        <div class="position-absolute inset-0 overflow-auto">
                            <div class="panel-body">
                                <div>
                                    <h1 class="step-title text-break">Communication Details</h1>
                                    <p>Provide additional information about your communication including when and how it should be sent.</p>
                                    <hr />
                                </div>

                                <Rock:NotificationBox ID="nbNoCommunicationTransport" runat="server" CssClass="margin-t-md" NotificationBoxType="Warning" Title="Warning" />

                                <asp:ValidationSummary ID="vsCommunicationDelivery" runat="server" HeaderText="Please correct the following:" ValidationGroup="vgCommunicationDelivery" CssClass="alert alert-validation" />
                                <div class="row">
                                    <div class="col-xs-12 col-sm-9 col-md-6">
                                        <Rock:RockTextBox ID="tbCommunicationName" runat="server" Label="Communication Name" Help="This name is used internally to describe the communication. It is not sent as a part of the communication." Required="true" ValidationGroup="vgCommunicationDelivery" MaxLength="100" />
                                    </div>
                                    <div class="col-xs-12 col-sm-3 col-md-6">
                                        <Rock:Switch
                                            ID="swBulkCommunication"
                                            runat="server"
                                            label=" "
                                            Checked="false"
                                            FormGroupCssClass="custom-switch-centered hide-label-sm"
                                            Text="Bulk"
                                            Visible="false" />
                                    </div>
                                </div>

                                <div class="row">
                                    <div class="col-md-12">
                                        <Rock:RockRadioButtonList
                                            runat="server"
                                            ID="rblCommunicationMedium"
                                            Label="Communication Medium"
                                            Help="The communication medium that you would like to send your message through."
                                            RepeatDirection="Horizontal"
                                            CssClass="js-medium-radio"
                                            AutoPostBack="true"
                                            OnSelectedIndexChanged="rblCommunicationMedium_SelectedIndexChanged" />

                                        <span runat="server" ID="spnRecipientPreferenceNotification" class="small help-block js-medium-recipientpreference-notification" Visible="false">Selecting 'Recipient Preference' will require adding content for email and SMS mediums.</span>
                                    </div>
                                </div>

                                <Rock:ModalDialog runat="server" ID="mdScheduleSend" Title="Schedule Send" OnSaveClick="mdScheduleSend_SaveClick">
                                    <Content>
                                        <Rock:NotificationBox ID="nbSendCommunicationDateTimeWarning" runat="server" NotificationBoxType="Danger" Visible="false" />
                                        <Rock:DateTimePicker runat="server" CssClass="js-send-communications-date" ID="dtpSendCommunicationDateTime" Label="Send Communications Date and Time" />
                                        <Rock:RockCheckBox runat="server" CssClass="js-send-immediately" ID="chkSendImmediately" Text="Send Immediately" Checked="true" />
                                    </Content>
                                </Rock:ModalDialog>
                                <div>
                                <asp:LinkButton runat="server" ID="lbScheduleSend" CssClass="btn btn-link pl-0 mb-4" OnClick="lbScheduleSend_Click">
                                    <i class='fa fa-calendar' aria-hidden='true'></i> Send: Immediately
                                </asp:LinkButton>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="actions mt-3">
                        <asp:LinkButton ID="btnCommunicationDeliveryPrevious" runat="server" data-shortcut-key="arrowleft" Text="Previous" ToolTip="Alt+🡄" CssClass="btn btn-default js-wizard-navigation" CausesValidation="false" OnClick="btnCommunicationDeliveryPrevious_Click" />
                        <asp:LinkButton ID="btnCommunicationDeliveryNext" runat="server" data-shortcut-key="arrowright" Text="Next" ToolTip="Alt+🡆" DataLoadingText="Next" CssClass="btn btn-primary pull-right js-wizard-navigation" ValidationGroup="vgCommunicationDelivery" CausesValidation="true" OnClick="btnCommunicationDeliveryNext_Click" />
                    </div>

                </asp:Panel>

                <%-- Template Selection --%>
                <asp:Panel ID="pnlTemplateSelection" CssClass="js-navigation-panel h-100 d-flex flex-column" runat="server" Visible="false">
                    <div class="panel-fill-body position-relative flex-fill styled-scroll">
                        <div class="position-absolute inset-0 overflow-auto">
                            <div class="panel-body">

                                <div>
                                    <div class="row">
                                        <div class="col-sm-8">
                                            <h1 class="step-title text-break">Communication Template</h1>
                                            <p>Templates allow you to speed up the communication creation and provide consistency. Administrate Templates</p>
                                        </div>
                                        <div class="d-flex col-sm-4">
                                            <div class="ml-sm-auto">
                                                <Rock:CategoryPicker ID="cpCommunicationTemplate" runat="server" AllowMultiSelect="false" Label="Category Filter" EntityTypeName="Rock.Model.CommunicationTemplate" OnSelectItem="cpCommunicationTemplate_SelectItem" />
                                            </div>
                                        </div>
                                    </div>
                                    <hr/>
                                </div>

                                <Rock:NotificationBox ID="nbTemplateSelectionWarning" runat="server" NotificationBoxType="Validation" Visible="false" />
                                <div class="row template-selection">
                                    <asp:Repeater ID="rptSelectTemplate" runat="server" OnItemDataBound="rptSelectTemplate_ItemDataBound">
                                        <ItemTemplate>
                                            <div class="col-md-4 col-sm-6">
                                                <asp:LinkButton ID="btnSelectTemplate" CssClass="communication-template" runat="server" OnClick="btnSelectTemplate_Click">
                                                    <div class="row">
                                                        <div class="col-xs-5">
                                                            <asp:Literal ID="lTemplateImagePreview" runat="server"></asp:Literal>
                                                        </div>
                                                        <div class="col-xs-7">
                                                            <label>
                                                                <asp:Literal ID="lTemplateName" runat="server"></asp:Literal></label>
                                                            <p>
                                                                <asp:Literal ID="lTemplateDescription" runat="server"></asp:Literal>
                                                            </p>
                                                        </div>
                                                    </div>
                                                </asp:LinkButton>
                                            </div>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="actions mt-3">
                        <asp:LinkButton ID="btnTemplateSelectionPrevious" runat="server" data-shortcut-key="arrowleft" Text="Previous" ToolTip="Alt+🡄" CssClass="btn btn-default js-wizard-navigation" CausesValidation="false" OnClick="btnTemplateSelectionPrevious_Click" />
                        <asp:LinkButton ID="btnTemplateSelectionNext" runat="server" data-shortcut-key="arrowright" Text="Next" ToolTip="Alt+🡆" DataLoadingText="Next" CssClass="btn btn-primary pull-right js-wizard-navigation" ValidationGroup="vgTemplateSelection" CausesValidation="true" OnClick="btnTemplateSelectionNext_Click" />
                    </div>
                </asp:Panel>

                <%-- Email Editor --%>
                <asp:Panel ID="pnlEmailEditor" CssClass="js-navigation-panel emaileditor-wrapper d-flex flex-column h-100 " runat="server" Visible="false">

                        <section id="emaileditor" class="panel-fill-body position-relative flex-fill styled-scroll">
                            <div id="emaileditor-designer" style="visibility:hidden;opacity: 0;">
                                <iframe id="ifEmailDesigner" name="emaileditor-iframe" class="emaileditor-iframe js-emaileditor-iframe" runat="server" src="javascript: window.frameElement.getAttribute('srcdoc');" frameborder="0" border="0" cellspacing="0"></iframe>
                            </div>
                            <div id="emaileditor-properties" class="position-absolute top-0 right-0 bottom-0 overflow-auto">
                                    <div class="js-email-sendtest-inputs" style="display: none">
                                        <Rock:EmailBox ID="tbTestEmailAddress" runat="server" Label="Email" ValidationGroup="vgEmailEditorSendTest" Required="true" AllowMultiple="false" Help="This will temporarily change your email address during the test, but it will be changed back after the test is complete." />
                                        <asp:LinkButton ID="btnEmailSendTest" runat="server" CssClass="btn btn-xs btn-primary js-saveeditorhtml" Text="Send Test" CausesValidation="true" ValidationGroup="vgEmailEditorSendTest" OnClick="btnEmailSendTest_Click" />
                                        <a class="btn btn-xs btn-link js-email-sendtest-cancel" href="#">Cancel</a>
                                        <hr/>
                                    </div>

                                <div class="emaileditor-propertypanels js-propertypanels">
                                    <!-- Text/Html Properties -->
                                    <div class="propertypanel propertypanel-text" data-component="text" style="display: none;">
                                        <h4 class="propertypanel-title">Text</h4>

                                        <Rock:HtmlEditor ID="htmlEditor" CssClass="js-component-text-htmlEditor" runat="server" Height="350" CallbackOnChangeScript="updateTextComponent(this, contents);" />

                                        <div class="row">
                                            <div class="col-md-6">
                                                <div class="form-group">
                                                    <label class="control-label" for="component-text-backgroundcolor">Background Color</label>
                                                    <div id="component-text-backgroundcolor" class="input-group colorpicker-component">
                                                        <input type="text" value="" class="form-control" />
                                                        <span class="input-group-addon"><i></i></span>
                                                    </div>
                                                </div>

                                            </div>
                                            <div class="col-md-6">
                                                <Rock:RockDropDownList ID="ddlLineHeight" CssClass="js-component-text-lineheight" ClientIDMode="Static" runat="server" Label="Line Height">
                                                    <asp:ListItem />
                                                    <asp:ListItem Text="Normal" Value="100%" />
                                                    <asp:ListItem Text="Slight" Value="125%" />
                                                    <asp:ListItem Text="1 &frac12; spacing" Value="150%" />
                                                    <asp:ListItem Text="Double space" Value="200%" />
                                                </Rock:RockDropDownList>
                                            </div>
                                        </div>

                                        <div class="row">
                                            <div class="col-md-6">
                                                <div class="form-group">
                                                    <label class="control-label" for="component-text-border-width">Border Width</label>
                                                    <div class="input-group input-width-md">
                                                        <input class="form-control" id="component-text-border-width" type="number"><span class="input-group-addon">px</span>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="col-md-6">
                                                <div class="form-group">
                                                    <label class="control-label" for="component-text-border-color">Border Color</label>
                                                    <div id="component-text-border-color" class="input-group colorpicker-component">
                                                        <input type="text" value="" class="form-control" />
                                                        <span class="input-group-addon"><i></i></span>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>

                                        <div class="row">
                                            <div class="col-md-6">
                                                <div class="form-group">
                                                    <label class="control-label" for="component-text-padding-top">Padding Top</label>
                                                    <div class="input-group input-width-md">
                                                        <input class="form-control" id="component-text-padding-top" type="number"><span class="input-group-addon">px</span>
                                                    </div>
                                                </div>
                                                <div class="form-group">
                                                    <label class="control-label" for="component-text-padding-left">Padding Left</label>
                                                    <div class="input-group input-width-md">
                                                        <input class="form-control" id="component-text-padding-left" type="number"><span class="input-group-addon">px</span>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="col-md-6">
                                                <div class="form-group">
                                                    <label class="control-label" for="component-text-padding-bottom">Padding Bottom</label>
                                                    <div class="input-group input-width-md">
                                                        <input class="form-control" id="component-text-padding-bottom" type="number"><span class="input-group-addon">px</span>
                                                    </div>
                                                </div>
                                                <div class="form-group">
                                                    <label class="control-label" for="component-text-padding-right">Padding Right</label>
                                                    <div class="input-group input-width-md">
                                                        <input class="form-control" id="component-text-padding-right" type="number"><span class="input-group-addon">px</span>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>

                                    <!-- Image Properties -->
                                    <div class="propertypanel propertypanel-image" data-component="image" style="display: none;">
                                        <h4 class="propertypanel-title">Image</h4>

                                        <div class="row">
                                            <div class="col-md-6">
                                                <div class="btn-group toggle-container js-btn-group-image-type">
                                                    <asp:HyperLink ID="aImagePickerTypeImage" runat="server" CssClass="js-image-picker-type-image btn btn-toggle btn-xs btn-primary">Image</asp:HyperLink>
                                                    <asp:HyperLink ID="aImagePickerTypeAsset" runat="server" CssClass="js-image-picker-type-asset btn btn-toggle btn-xs btn-default">Asset</asp:HyperLink>
                                                </div>
                                                <Rock:ImageUploader ID="componentImageUploader" ClientIDMode="Static" runat="server" Label="Image" UploadAsTemporary="false" DoneFunctionClientScript="handleImageUpdate(e, data)" DeleteFunctionClientScript="handleImageUpdate()" />

                                                <asp:UpdatePanel ID="assetPickerPanel" runat="server">
                                                    <ContentTemplate>
                                                    <Rock:ItemFromBlockPicker
                                                        ID="componentAssetManager"
                                                        runat="server"
                                                        BlockTypePath="~/Blocks/CMS/AssetManager.ascx"
                                                        ShowInModal="true"
                                                        SelectControlCssClass="imageupload-group"
                                                        CssClass="js-component-asset-manager picker-asset"
                                                        ModalSaveButtonText="Select"
                                                        ModalSaveButtonCssClass="js-singleselect aspNetDisabled"
                                                        ModalCssClass="js-AssetManager-modal"
                                                        ButtonTextTemplate="Select Asset"
                                                        ModalTitle="Asset Manager" 
                                                        ShowSelectNoneButton="true">
                                                    </Rock:ItemFromBlockPicker>
                                                    </ContentTemplate>
                                                </asp:UpdatePanel>

                                            </div>
                                            <div class="col-md-6">
                                                <div class="form-group">
                                                    <label class="control-label" for="component-image-imgcsswidth">Width</label>
                                                    <select id="component-image-imgcsswidth" class="form-control">
                                                        <option value="0">Image Width</option>
                                                        <option value="1">Full Width</option>
                                                    </select>
                                                </div>

                                                <div class="form-group">
                                                    <label class="control-label" for="component-image-imagealign">Align</label>
                                                    <select id="component-image-imagealign" class="form-control">
                                                        <option value="left">Left</option>
                                                        <option value="center">Center</option>
                                                        <option value="right">Right</option>
                                                    </select>
                                                </div>

                                                <div class="form-group">
                                                    <label class="control-label" for="component-image-resizemode">Resize Mode</label>
                                                    <select id="component-image-resizemode" class="form-control">
                                                        <option value="crop">Crop</option>
                                                        <option value="pad">Pad</option>
                                                        <option value="stretch">Stretch</option>
                                                    </select>
                                                </div>
                                            </div>
                                        </div>

                                        <div class="row">
                                            <div class="col-md-6">
                                                <div class="form-group">
                                                    <label class="control-label" for="component-image-imagewidth">Image Width</label>
                                                    <div class="input-group input-width-md">
                                                        <input class="form-control" id="component-image-imagewidth" type="number"><span class="input-group-addon">px</span>
                                                    </div>
                                                </div>
                                                <div class="form-group">
                                                    <label class="control-label" for="component-image-margin-top">Margin Top</label>
                                                    <div class="input-group input-width-md">
                                                        <input class="form-control" id="component-image-margin-top" type="number"><span class="input-group-addon">px</span>
                                                    </div>
                                                </div>
                                                <div class="form-group">
                                                    <label class="control-label" for="component-image-margin-left">Margin Left</label>
                                                    <div class="input-group input-width-md">
                                                        <input class="form-control" id="component-image-margin-left" type="number"><span class="input-group-addon">px</span>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="col-md-6">
                                                <div class="form-group">
                                                    <label class="control-label" for="component-image-imageheight">Image Height</label>
                                                    <div class="input-group input-width-md">
                                                        <input class="form-control" id="component-image-imageheight" type="number"><span class="input-group-addon">px</span>
                                                    </div>
                                                </div>
                                                <div class="form-group">
                                                    <label class="control-label" for="component-image-margin-bottom">Margin Bottom</label>
                                                    <div class="input-group input-width-md">
                                                        <input class="form-control" id="component-image-margin-bottom" type="number"><span class="input-group-addon">px</span>
                                                    </div>
                                                </div>
                                                <div class="form-group">
                                                    <label class="control-label" for="component-image-margin-right">Margin Right</label>
                                                    <div class="input-group input-width-md">
                                                        <input class="form-control" id="component-image-margin-right" type="number"><span class="input-group-addon">px</span>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>

                                        <div class="row">
                                            <div class="col-md-6">
                                                <div class="form-group">
                                                    <label class="control-label" for="component-image-link">Link</label>
                                                    <input type="url" id="component-image-link" class="form-control" />
                                                </div>
                                            </div>
                                            <div class="col-md-6">
                                                <div class="form-group">
                                                    <label class="control-label" for="component-image-alt">Alt Text</label>
                                                    <input type="text" id="component-image-alt" class="form-control" />
                                                </div>
                                            </div>
                                        </div>
                                    </div>

                                    <!-- Video Properties -->
                                    <div class="propertypanel propertypanel-video" data-component="video" style="display: none;">
						                <h4 class="propertypanel-title font-weight-semibold mt-0">Video</h4>
                                        <div class="row">
                                            <div class="col-md-12">
                                                <div class="form-group">
                                                    <label class="control-label" for="component-video-url">
                                                        Source Video URL
                                                        <asp:LinkButton ID="lbVideoUrlHelpText" Text="<i class='fa fa-info-circle'></i>" TabIndex="-1"  runat="server" CssClass="help"
                                                            data-toggle="tooltip" data-placement="auto" data-container="body" data-html="true"
                                                            data-original-title="The URL to the source video. This will be used to generate the preview thumbnail and will be the default target link. Preview images are generated automatically for "/>
                                                    </label>
                                                    <div class="input-group">
                                                        <span class="input-group-addon" id="component-video-addon-pre"><i class="fa fa-link"></i></span>
                                                        <input class="form-control" id="component-video-url" type="url" />
                                                        <span class="input-group-addon" id="component-video-addon"><i class="fa fa-arrow-right"></i></span>
                                                    </div>
                                                </div>
                                                <div id="component-video-error" class="alert alert-warning" role="alert">
                                                    Sorry, we couldn't generate a preview image for that URL. Please upload an image.
                                                </div>
                                            </div>
                                            <div class="col-md-6">
                                                <Rock:ImageUploader ID="componentVideoImageUploader" ClientIDMode="Static" runat="server" Label="Preview Image" UploadAsTemporary="false" DoneFunctionClientScript="handleVideoImageUpdate(e, data)" DeleteFunctionClientScript="handleVideoImageUpdate()" />
                                            </div>
                                            <div class="col-md-12">
                                                <div class="form-group">
                                                    <label class="control-label" for="component-video-url">
                                                        Link Target URL
                                                        <asp:LinkButton ID="lbTargetUrlHelpText" Text="<i class='fa fa-info-circle'></i>" TabIndex="-1"  runat="server" CssClass="help"
                                                            data-toggle="tooltip" data-placement="auto" data-container="body" data-html="true"
                                                            data-original-title="The URL the video will load when it is clicked. This can be different from the source video if you're embedding the video on your website."/>
                                                    </label>
                                                    <div class="input-group">
                                                        <span class="input-group-addon" id="component-target-addon"><i class="fa fa-link"></i></span>
                                                        <input class="form-control" id="component-target-url" type="url" />
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
					                </div>

                                    <!-- Section Properties -->
                                    <div class="propertypanel propertypanel-section" data-component="section" style="display: none;">
                                        <h4 class="propertypanel-title">Section</h4>
                                        <div id="component-section-column1">
                                            <span class="label label-default">Column 1</span>
                                            <div class="row">
                                                <div class="col-md-6">
                                                    <div class="form-group">
                                                        <label class="control-label" for="component-section-backgroundcolor-1">Background Color</label>
                                                        <div id="component-section-backgroundcolor-1" class="input-group colorpicker-component">
                                                            <input type="text" value="" class="form-control" />
                                                            <span class="input-group-addon"><i></i></span>
                                                        </div>
                                                    </div>
                                                </div>
                                                <div class="col-md-6">
                                                </div>
                                            </div>
                                            <div class="row">
                                                <div class="col-md-6">
                                                    <div class="form-group">
                                                        <label class="control-label" for="component-section-padding-top-1">Padding Top</label>
                                                        <div class="input-group input-width-md">
                                                            <input class="form-control js-component-section-padding-input" id="component-section-padding-top-1" type="number"><span class="input-group-addon">px</span>
                                                        </div>
                                                    </div>
                                                    <div class="form-group">
                                                        <label class="control-label" for="component-section-padding-left-1">Padding Left</label>
                                                        <div class="input-group input-width-md">
                                                            <input class="form-control js-component-section-padding-input" id="component-section-padding-left-1" type="number"><span class="input-group-addon">px</span>
                                                        </div>
                                                    </div>
                                                </div>
                                                <div class="col-md-6">
                                                    <div class="form-group">
                                                        <label class="control-label" for="component-section-padding-bottom-1">Padding Bottom</label>
                                                        <div class="input-group input-width-md">
                                                            <input class="form-control js-component-section-padding-input" id="component-section-padding-bottom-1" type="number"><span class="input-group-addon">px</span>
                                                        </div>
                                                    </div>
                                                    <div class="form-group">
                                                        <label class="control-label" for="component-section-padding-right-1">Padding Right</label>
                                                        <div class="input-group input-width-md">
                                                            <input class="form-control js-component-section-padding-input" id="component-section-padding-right-1" type="number"><span class="input-group-addon">px</span>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>

                                        <div id="component-section-column2">
                                            <hr class="margin-all-sm">
                                            <span class="label label-default">Column 2</span>

                                            <div class="row">
                                                <div class="col-md-6">
                                                    <div class="form-group">
                                                        <label class="control-label" for="component-section-backgroundcolor-2">Background Color</label>
                                                        <div id="component-section-backgroundcolor-2" class="input-group colorpicker-component">
                                                            <input type="text" value="" class="form-control" />
                                                            <span class="input-group-addon"><i></i></span>
                                                        </div>
                                                    </div>
                                                </div>
                                                <div class="col-md-6">
                                                </div>
                                            </div>
                                            <div class="row">
                                                <div class="col-md-6">
                                                    <div class="form-group">
                                                        <label class="control-label" for="component-section-padding-top-2">Padding Top</label>
                                                        <div class="input-group input-width-md">
                                                            <input class="form-control js-component-section-padding-input" id="component-section-padding-top-2" type="number"><span class="input-group-addon">px</span>
                                                        </div>
                                                    </div>
                                                    <div class="form-group">
                                                        <label class="control-label" for="component-section-padding-left-2">Padding Left</label>
                                                        <div class="input-group input-width-md">
                                                            <input class="form-control js-component-section-padding-input" id="component-section-padding-left-2" type="number"><span class="input-group-addon">px</span>
                                                        </div>
                                                    </div>
                                                </div>
                                                <div class="col-md-6">
                                                    <div class="form-group">
                                                        <label class="control-label" for="component-section-padding-bottom-2">Padding Bottom</label>
                                                        <div class="input-group input-width-md">
                                                            <input class="form-control js-component-section-padding-input" id="component-section-padding-bottom-2" type="number"><span class="input-group-addon">px</span>
                                                        </div>
                                                    </div>
                                                    <div class="form-group">
                                                        <label class="control-label" for="component-section-padding-right-2">Padding Right</label>
                                                        <div class="input-group input-width-md">
                                                            <input class="form-control js-component-section-padding-input" id="component-section-padding-right-2" type="number"><span class="input-group-addon">px</span>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>

                                        <div id="component-section-column3">
                                            <hr class="margin-all-sm">
                                            <span class="label label-default">Column 3</span>

                                            <div class="row">
                                                <div class="col-md-6">
                                                    <div class="form-group">
                                                        <label class="control-label" for="component-section-backgroundcolor-3">Background Color</label>
                                                        <div id="component-section-backgroundcolor-3" class="input-group colorpicker-component">
                                                            <input type="text" value="" class="form-control" />
                                                            <span class="input-group-addon"><i></i></span>
                                                        </div>
                                                    </div>
                                                </div>
                                                <div class="col-md-6">
                                                </div>
                                            </div>
                                            <div class="row">
                                                <div class="col-md-6">
                                                    <div class="form-group">
                                                        <label class="control-label" for="component-section-padding-top-3">Padding Top</label>
                                                        <div class="input-group input-width-md">
                                                            <input class="form-control js-component-section-padding-input" id="component-section-padding-top-3" type="number"><span class="input-group-addon">px</span>
                                                        </div>
                                                    </div>
                                                    <div class="form-group">
                                                        <label class="control-label" for="component-section-padding-left-3">Padding Left</label>
                                                        <div class="input-group input-width-md">
                                                            <input class="form-control js-component-section-padding-input" id="component-section-padding-left-3" type="number"><span class="input-group-addon">px</span>
                                                        </div>
                                                    </div>
                                                </div>
                                                <div class="col-md-6">
                                                    <div class="form-group">
                                                        <label class="control-label" for="component-section-padding-bottom-3">Padding Bottom</label>
                                                        <div class="input-group input-width-md">
                                                            <input class="form-control js-component-section-padding-input" id="component-section-padding-bottom-3" type="number"><span class="input-group-addon">px</span>
                                                        </div>
                                                    </div>
                                                    <div class="form-group">
                                                        <label class="control-label" for="component-section-padding-right-3">Padding Right</label>
                                                        <div class="input-group input-width-md">
                                                            <input class="form-control js-component-section-padding-input" id="component-section-padding-right-3" type="number"><span class="input-group-addon">px</span>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>

                                    <!-- Divider Properties -->
                                    <div class="propertypanel propertypanel-divider" id="component-divider-panel" data-component="divider" style="display: none;">
                                        <h4 class="propertypanel-title">Divider</h4>
                                        <div class="row">
                                            <div class="col-md-6">
                                                <div class="form-group">
                                                    <label class="control-label" for="component-divider-height">Height</label>
                                                    <div class="input-group input-width-md">
                                                        <input class="form-control" id="component-divider-height" type="number"><span class="input-group-addon">px</span>
                                                    </div>
                                                </div>
                                                <div class="form-group">
                                                    <label class="control-label" for="component-divider-margin-top">Margin Top</label>
                                                    <div class="input-group input-width-md">
                                                        <input class="form-control" id="component-divider-margin-top" type="number"><span class="input-group-addon">px</span>
                                                    </div>
                                                </div>
                                                <div class="form-group">
                                                    <Rock:RockCheckBox ID="cbComponentDividerDivideWithLine" CssClass="js-component-divider-divide-with-line" runat="server" Text="Divide With Line" />
                                                </div>
                                            </div>
                                            <div class="col-md-6">
                                                <div class="form-group">
                                                    <label class="control-label" for="component-divider-color">Color</label>
                                                    <div id="component-divider-color" class="input-group colorpicker-component">
                                                        <input type="text" value="" class="form-control" />
                                                        <span class="input-group-addon"><i></i></span>
                                                    </div>
                                                </div>
                                                <div class="form-group">
                                                    <label class="control-label" for="component-divider-margin-bottom">Margin Bottom</label>
                                                    <div class="input-group input-width-md">
                                                        <input class="form-control" id="component-divider-margin-bottom" type="number"><span class="input-group-addon">px</span>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>

                                    <!-- Code Properties -->
                                    <div class="propertypanel propertypanel-code" data-component="code" style="display: none;">
                                        <h4 class="propertypanel-title">HTML</h4>
                                        <Rock:CodeEditor ID="codeEditor" CssClass="js-component-code-codeEditor" runat="server" Height="350" EditorTheme="Rock" EditorMode="Lava" OnChangeScript="updateCodeComponent(this, contents);" />
                                        <div class="alert alert-danger" id="component-code-codeEditor-error" style="display: none"></div>


                                        <div class="row">
                                            <div class="col-md-6">
                                                <div class="form-group">
                                                    <label class="control-label" for="component-code-margin-top">Margin Top</label>
                                                    <div class="input-group input-width-md">
                                                        <input class="form-control" id="component-code-margin-top" type="number"><span class="input-group-addon">px</span>
                                                    </div>
                                                </div>
                                                <div class="form-group">
                                                    <label class="control-label" for="component-code-margin-left">Margin Left</label>
                                                    <div class="input-group input-width-md">
                                                        <input class="form-control" id="component-code-margin-left" type="number"><span class="input-group-addon">px</span>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="col-md-6">
                                                <div class="form-group">
                                                    <label class="control-label" for="component-code-margin-bottom">Margin Bottom</label>
                                                    <div class="input-group input-width-md">
                                                        <input class="form-control" id="component-code-margin-bottom" type="number"><span class="input-group-addon">px</span>
                                                    </div>
                                                </div>
                                                <div class="form-group">
                                                    <label class="control-label" for="component-code-margin-right">Margin Right</label>
                                                    <div class="input-group input-width-md">
                                                        <input class="form-control" id="component-code-margin-right" type="number"><span class="input-group-addon">px</span>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>

                                    <!-- Button Properties -->
                                    <div id="component-button-panel" class="propertypanel propertypanel-button" data-component="button" style="display: none;">
                                        <h4 class="propertypanel-title">Button</h4>
                                        <hr />
                                        <div class="form-group">
                                            <label class="control-label" for="component-button-buttontext">Button Text</label>
                                            <input class="form-control" id="component-button-buttontext" placeholder="Press Me">
                                        </div>

                                        <div class="form-group">
                                            <label class="control-label" for="component-button-buttonurl">URL</label>
                                            <div class="input-group">
                                                <span class="input-group-addon"><i class="fa fa-link"></i></span>
                                                <input class="form-control" id="component-button-buttonurl" placeholder="http://yourlink.com">
                                            </div>
                                        </div>

                                        <div class="row">
                                            <div class="col-md-6">
                                                <div class="form-group">
                                                    <label class="control-label" for="component-button-buttonbackgroundcolor">Background Color</label>
                                                    <div id="component-button-buttonbackgroundcolor" class="input-group colorpicker-component">
                                                        <input type="text" value="" class="form-control" />
                                                        <span class="input-group-addon"><i></i></span>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="col-md-6">
                                                <div class="form-group">
                                                    <label class="control-label" for="component-button-buttonfontcolor">Font Color</label>
                                                    <div id="component-button-buttonfontcolor" class="input-group colorpicker-component">
                                                        <input type="text" value="" class="form-control" />
                                                        <span class="input-group-addon"><i></i></span>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>

                                        <div class="row">
                                            <div class="col-md-6">
                                                <div class="form-group">
                                                    <label class="control-label" for="component-button-buttonwidth">Width</label>
                                                    <select id="component-button-buttonwidth" class="form-control">
                                                        <option value="0">Fit To Text</option>
                                                        <option value="1">Full Width</option>
                                                        <option value="2">Fixed Width</option>
                                                    </select>
                                                </div>
                                                <div class="form-group js-buttonfixedwidth">
                                                    <label class="control-label" for="component-button-buttonfixedwidth">Fixed Width</label>
                                                    <input class="form-control" id="component-button-buttonfixedwidth">
                                                </div>
                                            </div>
                                            <div class="col-md-6">
                                                <div class="form-group">
                                                    <label class="control-label" for="component-button-buttonalign">Align</label>
                                                    <select id="component-button-buttonalign" class="form-control">
                                                        <option value="left">Left</option>
                                                        <option value="center">Center</option>
                                                        <option value="right">Right</option>
                                                    </select>
                                                </div>
                                            </div>
                                        </div>

                                        <div class="form-group">
                                            <label class="control-label" for="component-button-buttonfont">Font</label>
                                            <select id="component-button-buttonfont" class="form-control">
                                                <option value=""></option>
                                                <option value="Arial, Helvetica, sans-serif">Arial</option>
                                                <option value='"Arial Black", Gadget, sans-serif'>Arial Black</option>
                                                <option value='"Courier New", Courier, monospace'>Courier New</option>
                                                <option value="Georgia, serif">Georgia</option>
                                                <option value="Helvetica, Arial, sans-serif">Helvetica</option>
                                                <option value="Impact, Charcoal, sans-serif">Impact</option>
                                                <option value='"Lucida Sans Unicode", "Lucida Grande", sans-serif'>Lucida</option>
                                                <option value='"Lucida Console", Monaco, monospace'>Lucida Console</option>
                                                <option value="Tahoma, Geneva, sans-serif">Tahoma</option>
                                                <option value='Times New Roman", Times, serif'>Times New Roman</option>
                                                <option value='Trebuchet MS", Helvetica, sans-serif'>Trebuchet MS</option>
                                                <option value="Verdana, Geneva, sans-serif">Verdana</option>
                                            </select>
                                        </div>

                                        <div class="row">
                                            <div class="col-md-6">
                                                <div class="form-group">
                                                    <label class="control-label" for="component-button-buttonfontweight">Font Weight</label>
                                                    <select id="component-button-buttonfontweight" class="form-control">
                                                        <option value="normal">Normal</option>
                                                        <option value="bold">Bold</option>
                                                        <option value="bolder">Bolder</option>
                                                        <option value="lighter">Lighter</option>
                                                    </select>
                                                </div>
                                            </div>
                                            <div class="col-md-6">
                                                <div class="form-group">
                                                    <label class="control-label" for="component-button-buttonfontsize">Font Size</label>
                                                    <input class="form-control" id="component-button-buttonfontsize">
                                                </div>
                                            </div>
                                        </div>

                                        <div class="row">
                                            <div class="col-md-6">
                                                <div class="form-group">
                                                    <label class="control-label" for="component-button-buttonpadding">Button Padding</label>
                                                    <input class="form-control" id="component-button-buttonpadding">
                                                </div>
                                            </div>
                                            <div class="col-md-6">
                                                <div class="form-group">
                                                    <label class="control-label" for="component-button-buttonradius">Border Radius<a class="help" href="#" tabindex="-1" data-toggle="tooltip" data-placement="auto" data-container="body" data-html="true" title="Outlook and Windows Mail email clients do not support rounded corners."><i class="fa fa-info-circle"></i></a></label>
                                                    <input class="form-control" id="component-button-buttonradius">
                                                </div>
                                            </div>
                                        </div>
                                    </div>

                                    <!-- RSVP Properties -->
                                    <div id="component-rsvp-panel" class="propertypanel propertypanel-rsvp" data-component="rsvp" style="display: none;">
                                        <h4 class="propertypanel-title">RSVP</h4>
                                        <div class="row">
                                            <div class="col-md-12">
                                                <div class="form-group" id="component-rsvp-group">
                                                    <Rock:GroupPicker ID="gpRSVPGroup" runat="server" CssClass="rsvp-group" LimitToRSVPEnabledGroups="true" />
                                                </div>
                                            </div>
                                        </div>
                                        <div class="row">
                                            <div class="col-md-12">
                                                <div class="form-group">
                                                    <select id="component-rsvp-occurrence" class="form-control" disabled="disabled">
                                                        <option value=""></option>
                                                        <option value="1">Test Value</option>
                                                    </select>
                                                </div>
                                            </div>
                                            <div class="col-md-12">
                                                <div class="form-group">
                                                    <button id="component-rsvp-registerbutton" type="button" class="btn btn-xs btn-default disabled">Register Recipients</button>
                                                    <span class="js-rsvp-person-ids">
                                                        <asp:HiddenField ID="hfRSVPPersonIDs" runat="server" Value="" />
                                                    </span>

                                                    <asp:HiddenField ID="hfRSVPShowAdvancedSettings" runat="server" Value="false" />
                                                    <div class="pull-right">
                                                        <a href="#" class="btn btn-xs btn-link js-rsvp-show-advanced-settings disabled">Show Advanced Settings</a>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>


                                        <div class="form-group js-rsvp-advanced-settings">
                                            <label class="control-label" for="component-rsvp-accepttext">Accept Button Label</label>
                                            <input class="form-control" id="component-rsvp-accepttext" placeholder="Accept">
                                        </div>

                                        <div class="row js-rsvp-advanced-settings">
                                            <div class="col-md-6">
                                                <div class="form-group">
                                                    <label class="control-label" for="component-rsvp-acceptbackgroundcolor">Accept Button Color</label>
                                                    <div id="component-rsvp-acceptbackgroundcolor" class="input-group colorpicker-component">
                                                        <input type="text" value="" class="form-control" />
                                                        <span class="input-group-addon"><i></i></span>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="col-md-6">
                                                <div class="form-group">
                                                    <label class="control-label" for="component-rsvp-acceptfontcolor">Accept Button Text Color</label>
                                                    <div id="component-rsvp-acceptfontcolor" class="input-group colorpicker-component">
                                                        <input type="text" value="" class="form-control" />
                                                        <span class="input-group-addon"><i></i></span>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>

                                        <div class="row js-rsvp-advanced-settings">
                                            <div class="col-md-9">
                                                <div class="form-group">
                                                    <label class="control-label" for="component-rsvp-declinetext">Decline Button Label</label>
                                                    <input class="form-control" id="component-rsvp-declinetext" placeholder="Decline">
                                                </div>
                                            </div>
                                            <div class="col-md-3">
                                                <div class="form-group">
                                                    <Rock:RockCheckBox ID="rcbIncludeDecline" runat="server" CssClass="js-rsvp-include-decline" Checked="true" Label="Show Decline" />
                                                </div>
                                            </div>
                                        </div>

                                        <div class="row js-rsvp-advanced-settings">
                                            <div class="col-md-6">
                                                <div class="form-group">
                                                    <label class="control-label" for="component-rsvp-declinebackgroundcolor">Decline Button Color</label>
                                                    <div id="component-rsvp-declinebackgroundcolor" class="input-group colorpicker-component">
                                                        <input type="text" value="" class="form-control" />
                                                        <span class="input-group-addon"><i></i></span>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="col-md-6">
                                                <div class="form-group">
                                                    <label class="control-label" for="component-rsvp-declinefontcolor">Decline Button Text Color</label>
                                                    <div id="component-rsvp-declinefontcolor" class="input-group colorpicker-component">
                                                        <input type="text" value="" class="form-control" />
                                                        <span class="input-group-addon"><i></i></span>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>

                                        <div class="form-group js-rsvp-advanced-settings">
                                            <label class="control-label" for="component-rsvp-buttonfont">Font Face</label>
                                            <select id="component-rsvp-buttonfont" class="form-control">
                                                <option value=""></option>
                                                <option value="Arial, Helvetica, sans-serif">Arial</option>
                                                <option value='"Arial Black", Gadget, sans-serif'>Arial Black</option>
                                                <option value='"Courier New", Courier, monospace'>Courier New</option>
                                                <option value="Georgia, serif">Georgia</option>
                                                <option value="Helvetica, Arial, sans-serif">Helvetica</option>
                                                <option value="Impact, Charcoal, sans-serif">Impact</option>
                                                <option value='"Lucida Sans Unicode", "Lucida Grande", sans-serif'>Lucida</option>
                                                <option value='"Lucida Console", Monaco, monospace'>Lucida Console</option>
                                                <option value="Tahoma, Geneva, sans-serif">Tahoma</option>
                                                <option value='Times New Roman", Times, serif'>Times New Roman</option>
                                                <option value='Trebuchet MS", Helvetica, sans-serif'>Trebuchet MS</option>
                                                <option value="Verdana, Geneva, sans-serif">Verdana</option>
                                            </select>
                                        </div>

                                        <div class="row js-rsvp-advanced-settings">
                                            <div class="col-md-6">
                                                <div class="form-group">
                                                    <label class="control-label" for="component-rsvp-buttonfontweight">Font Weight</label>
                                                    <select id="component-rsvp-buttonfontweight" class="form-control">
                                                        <option value="normal">Normal</option>
                                                        <option value="bold">Bold</option>
                                                        <option value="bolder">Bolder</option>
                                                        <option value="lighter">Lighter</option>
                                                    </select>
                                                </div>
                                            </div>
                                            <div class="col-md-6">
                                                <div class="form-group">
                                                    <label class="control-label" for="component-rsvp-buttonfontsize">Font Size</label>
                                                    <input class="form-control" id="component-rsvp-buttonfontsize">
                                                </div>
                                            </div>
                                        </div>

                                        <div class="row js-rsvp-advanced-settings">
                                            <div class="col-md-6">
                                                <div class="form-group">
                                                    <label class="control-label" for="component-rsvp-buttonpadding">Button Padding</label>
                                                    <input class="form-control" id="component-rsvp-buttonpadding">
                                                </div>
                                            </div>
                                            <div class="col-md-6">
                                                <div class="form-group">
                                                    <label class="control-label" for="component-rsvp-buttonalign">Align</label>
                                                    <select id="component-rsvp-buttonalign" class="form-control">
                                                        <option value="left">Left</option>
                                                        <option value="center">Center</option>
                                                        <option value="right">Right</option>
                                                    </select>
                                                </div>
                                            </div>
                                        </div>

                                    </div>

                                    <div class="js-propertypanel-actions actions mt-2" style="display: none">
                                        <a href="#" class="btn btn-primary" onclick="clearPropertyPane(event); return false;">Complete</a>
                                        <a href="#" class="btn btn-link" onclick="deleteCurrentComponent(); return false;">Delete</a>
                                    </div>
                                </div>

                            </div>
                        </section>

                        <%-- the contents editor-controls-markup is copied into the iframe --%>
                        <div id="editor-controls-markup" style="display: none;">
                            <div id="editor-controls" class="editor-toolbar-container js-emaileditor-addon" style="min-height:72px;">
                                <div class="js-editor-toolbar-content">

                                    <div class="component component-text" data-content="<h1>Title</h1><p> Can't wait to see what you have to say!</p>" data-state="template">
                                        <i class="fa fa-align-justify"></i>
                                        <br />
                                        Text
                                    </div>
                                    <div class="component component-image" data-content="<img src='<%= VirtualPathUtility.ToAbsolute("~/Assets/Images/image-placeholder.jpg") %>' style='width: 100%;' data-imgcsswidth='full' alt='' />" data-state="template">
                                        <i class="fa fa-picture-o"></i>
                                        <br />
                                        Image
                                    </div>
                                    <div class="component component-video" data-content="<a href=''><img src='<%= VirtualPathUtility.ToAbsolute("~/Assets/Images/video-placeholder.jpg") %>' style='width: 100%;' data-imgcsswidth='full' /></a>" data-state="template">
					                    <i class="fa fa-play-circle-o"></i> <br /> Video
				                    </div>
                                    <div class="component component-divider" data-content="<hr style='margin-top: 20px; margin-bottom: 20px; border: 0; height: 4px; background: #c4c4c4;' />" data-state="template">
                                        <i class="fa fa-minus"></i>
                                        <br />
                                        Divider
                                    </div>
                                    <div class="component component-code" data-content="Add your code here..." data-state="template">
                                        <i class="fa fa-code"></i>
                                        <br />
                                        HTML
                                    </div>
                                    <div class="component component-button v2" data-content="<table class='button-outerwrap' border='0' cellpadding='0' cellspacing='0' width='100%' style='min-width:100%;'><tbody><tr><td valign='top' align='center' class='button-innerwrap'><table border='0' cellpadding='0' cellspacing='0' class='button-shell'><tbody><tr><td align='center' valign='middle' class='button-content' style='border-radius: 3px;background-color:#2baadf' ><a class='button-link' title='Push Me' href='http://' target='_blank' rel='noopener noreferrer' style='display: inline-block; font-weight: bold; letter-spacing: normal; line-height: 100%; text-align: center; text-decoration: none; color: #ffffff;background-color: #2baadf; padding: 15px; border: 1px solid #2baadf; border-radius: 3px;'>Push Me</a></td></tr></tbody></table></td></tr></tbody></table>" data-state="template">
                                        <i class="fa fa-square-o"></i>
                                        <br />
                                        Button
                                    </div>
                                    <div class="component component-rsvp" data-content="<table class='rsvp-outerwrap' border='0' cellpadding='0' width='100%' style='min-width:100%;'><tbody><tr><td style='padding-top:0; padding-right:0; padding-bottom:0; padding-left:0;' valign='top' align='center' class='rsvp-innerwrap'><table border='0' cellpadding='0' cellspacing='0'><tr><td><table border='0' cellpadding='0' cellspacing='0' class='accept-button-shell' style='display: inline-table; border-collapse: separate !important; border-radius: 3px; background-color: #16C98D;'><tbody><tr><td align='center' valign='middle' class='rsvp-accept-content' style='font-family: Arial; font-size: 16px; padding: 15px;'><a class='rsvp-accept-link' title='Accept' href='http://' target='_blank' rel='noopener noreferrer' style='font-weight: bold; letter-spacing: normal; line-height: 100%; text-align: center; text-decoration: none; color: #FFFFFF;'>Accept</a></td></tr></tbody></table></td><td style='padding-left: 10px;'><table border='0' cellpadding='0' cellspacing='0' class='decline-button-shell' style='display: inline-table; border-collapse: separate !important; border-radius: 3px; background-color: #D4442E;'><tbody><tr><td align='center' valign='middle' class='rsvp-decline-content' style='font-family: Arial; font-size: 16px; padding: 15px;'><a class='rsvp-decline-link' title='Decline' href='http://' target='_blank' style='font-weight: bold; letter-spacing: normal; line-height: 100%; text-align: center; text-decoration: none; color: #FFFFFF;'>Decline</a></td></tr></tbody></table></td></tr></table></td></tr></tbody></table><input type='hidden' class='rsvp-group-id' /><input type='hidden' class='rsvp-occurrence-value' />" data-state="template">
                                        <i class="fa fa-user-check"></i>
                                        <br />
                                        RSVP
                                    </div>

                                    <div class="component-separator"></div>
                                </div>
                                <div class="js-editor-toolbar-structure">

                                    <div class="component component-section" data-content="<div class='dropzone'></div>" data-state="template">
                                        <i class="rk rk-one-column"></i>
                                        <br />
                                        One
                                    </div>
                                    <div class="component component-section" data-content="<table class='row'width='100%' ><tr><td class='dropzone columns large-6 small-12 first' width='50%' valign='top'></td><td class='dropzone columns large-6 small-12 last' width='50%' valign='top'></td></tr></table>" data-state="template">
                                        <i class="rk rk-two-column"></i>
                                        <br />
                                        Two
                                    </div>
                                    <div class="component component-section" data-content="<table class='row'width='100%' ><tr><td class='dropzone columns large-4 small-12 first' width='33%' valign='top'></td><td class='dropzone columns large-4 small-12' width='34%' valign='top'></td><td class='dropzone columns large-4 small-12 last' width='33%' valign='top'></td></tr></table>" data-state="template">
                                        <i class="rk rk-three-column"></i>
                                        <br />
                                        Three
                                    </div>
                                    <div class="component component-section" data-content="<table class='row'width='100%' ><tr><td class='dropzone columns large-4 small-12 first' width='33%' valign='top'></td><td class='dropzone columns large-8 small-12 last' width='67%' valign='top'></td></tr></table>" data-state="template">
                                        <i class="rk rk-left-column"></i>
                                        <br />
                                        Left
                                    </div>
                                    <div class="component component-section" data-content="<table class='row'width='100%' ><tr><td class='dropzone columns large-8 small-12 first' width='67%' valign='top'></td><td class='dropzone columns large-4 small-12 last' width='33%' valign='top'></td></tr></table>" data-state="template">
                                        <i class="rk rk-right-column"></i>
                                        <br />
                                        Right
                                    </div>
                                </div>
                            </div>
                        </div>

                    <div class="actions clearfix mt-0">
                        <asp:LinkButton ID="btnEmailEditorPrevious" runat="server" data-shortcut-key="arrowleft" Text="Previous" ToolTip="Alt+🡄" CssClass="btn btn-default js-saveeditorhtml js-wizard-navigation" CausesValidation="false" OnClick="btnEmailEditorPrevious_Click" />
                        <asp:LinkButton ID="btnEmailEditorNext" runat="server" data-shortcut-key="arrowright" Text="Next" ToolTip="Alt+🡆" DataLoadingText="Next" CssClass="btn btn-primary pull-right js-saveeditorhtml js-wizard-navigation" ValidationGroup="vgEmailEditor" CausesValidation="true" OnClick="btnEmailEditorNext_Click" />
                    </div>

                </asp:Panel>

                <asp:UpdatePanel ID="upnlEmailPreview" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <asp:Panel ID="pnlEmailPreview" runat="server" Visible="false">
                            <Rock:ModalDialog ID="mdEmailPreview" runat="server" Title="Email Preview">
                                <Content>
                                    <div class="text-center margin-v-md">
                                        <div class="btn-group" role="group">
                                            <button type="button" class="btn btn-default js-preview-desktop"><i class="fa fa-desktop"></i> Desktop</button>
                                            <button type="button" class="btn btn-default js-preview-mobile"><i class="fa fa-mobile"></i> Mobile</button>
                                        </div>
                                    </div>
                                    <div id="pnlEmailPreviewContainer" runat="server" class="email-preview js-email-preview device-browser center-block">
                                        <iframe id="ifEmailPreview" name="emailpreview-iframe" class="emaileditor-iframe js-emailpreview-iframe email-wrapper" runat="server" src="javascript: window.frameElement.getAttribute('srcdoc');" frameborder="0" border="0" cellspacing="0" scrolling="yes"></iframe>
                                    </div>
                                </Content>

                            </Rock:ModalDialog>
                        </asp:Panel>
                    </ContentTemplate>
                </asp:UpdatePanel>

                <%-- Email Summary --%>
                <asp:Panel ID="pnlEmailSummary" CssClass="js-navigation-panel h-100 d-flex flex-column" runat="server" Visible="false">
                    <div class="panel-fill-body position-relative flex-fill styled-scroll">
                        <div class="position-absolute inset-0 overflow-auto">
                            <div class="panel-body">
                                <div>
                                    <h1 class="step-title text-break">Email Summary</h1>
                                    <p>Provide the basic information about your email.</p>
                                    <hr/>
                                </div>

                                <asp:ValidationSummary ID="vsEmailSummary" runat="server" HeaderText="Please correct the following:" ValidationGroup="vgEmailSummary" CssClass="alert alert-validation" />

                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:RockTextBox ID="tbFromName" runat="server" Label="From Name" Required="true" ValidationGroup="vgEmailSummary" MaxLength="100" Help="<span class='tip tip-lava'></span>" />
                                    </div>
                                    <div class="col-md-6">
                                        <Rock:EmailBox ID="ebFromAddress" runat="server" Label="From Address" Required="true" AllowLava="true" ValidationGroup="vgEmailSummary" MaxLength="100" Help="<span class='tip tip-lava'>" />
                                        <asp:HiddenField ID="hfShowAdditionalFields" runat="server" />
                                        <div class="pull-right">
                                            <a href="#" class="btn btn-xs btn-link js-show-additional-fields">Show Additional Fields</a>
                                        </div>
                                    </div>
                                </div>

                                <asp:Panel ID="pnlEmailSummaryAdditionalFields" runat="server" CssClass="js-additional-fields" Style="display: none">
                                    <div class="row">
                                        <div class="col-md-6">
                                            <Rock:EmailBox ID="ebReplyToAddress" runat="server" Label="Reply To Address" AllowLava="true" MaxLength="100" Help="<span class='tip tip-lava'>" />
                                        </div>
                                        <div class="col-md-6">
                                        </div>
                                    </div>
                                    <div class="well">
                                        <p><strong>Note:</strong> Because Rock personalizes emails, CC and BCC recipients will receive one email per recipient.</p>
                                        <div class="row">
                                            <div class="col-md-6">
                                                <Rock:EmailBox ID="ebCCList" runat="server" Label="CC List" AllowMultiple="true" Help="Comma-delimited list of email addresses that will be copied on the email sent to every recipient. Lava can be used to access recipent data. <span class='tip tip-lava'></span>" AllowLava="true" />
                                            </div>
                                            <div class="col-md-6">
                                                <Rock:EmailBox ID="ebBCCList" runat="server" Label="BCC List" AllowMultiple="true" Help="Comma-delimited list of email addresses that will be blind copied on the email sent to every recipient. Lava can be used to access recipent data. <span class='tip tip-lava'></span>" AllowLava="true" />
                                            </div>
                                        </div>
                                    </div>
                                </asp:Panel>

                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:RockTextBox ID="tbEmailSubject" runat="server" Label="Email Subject" Help="<span class='tip tip-lava'></span>" Required="true" ValidationGroup="vgEmailSummary" MaxLength="100" />
                                        <asp:UpdatePanel ID="upEmailFileAttachments" runat="server">
                                            <ContentTemplate>
                                                <asp:HiddenField ID="hfEmailAttachedBinaryFileIds" runat="server" />
                                                <Rock:FileUploader ID="fupEmailAttachments" runat="server" Label="Attachments" OnFileUploaded="fupEmailAttachments_FileUploaded" />
                                                <asp:Literal ID="lEmailAttachmentListHtml" runat="server" />
                                            </ContentTemplate>
                                        </asp:UpdatePanel>
                                    </div>
                                    <div class="col-md-6">
                                        <Rock:RockTextBox ID="tbEmailPreview" runat="server" Label="Email Preview" TextMode="MultiLine" Rows="4" Help="A short summary of the email which will show in the inbox before the email is opened." />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="actions clearfix mt-3">
                        <asp:LinkButton ID="btnEmailSummaryPrevious" runat="server" data-shortcut-key="arrowleft" Text="Previous" ToolTip="Alt+🡄" CssClass="btn btn-default js-wizard-navigation" CausesValidation="false" OnClick="btnEmailSummaryPrevious_Click" />
                        <asp:LinkButton ID="btnEmailSummaryNext" runat="server" data-shortcut-key="arrowright" Text="Next" ToolTip="Alt+🡆" DataLoadingText="Next" CssClass="btn btn-primary pull-right js-wizard-navigation" ValidationGroup="vgEmailSummary" CausesValidation="true" OnClick="btnEmailSummaryNext_Click" />
                    </div>
                </asp:Panel>

                <%-- Mobile Text Editor --%>
                <asp:Panel ID="pnlMobileTextEditor" CssClass="js-navigation-panel h-100 d-flex flex-column" runat="server" Visible="false">
                    <h1 class="step-title text-break">Mobile Text Editor</h1>
                    <asp:HiddenField ID="hfSMSSampleRecipientPersonId" runat="server" />

                    <asp:ValidationSummary ID="vsMobileTextEditor" runat="server" HeaderText="Please correct the following:" ValidationGroup="vgMobileTextEditor" CssClass="alert alert-validation" />
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockDropDownList ID="ddlSMSFrom" runat="server" Label="From" Help="The number to originate message from (configured under Admin Tools > Communications > SMS Phone Numbers)." Required="true" ValidationGroup="vgMobileTextEditor" />
                            <Rock:RockControlWrapper ID="rcwSMSMessage" runat="server" Label="Message" Help="<span class='tip tip-lava'></span>">
                                <Rock:MergeFieldPicker ID="mfpSMSMessage" runat="server" CssClass="margin-b-sm pull-right" OnSelectItem="mfpMessage_SelectItem" ValidationGroup="vgMobileTextEditor" />
                                <asp:HiddenField ID="hfSMSCharLimit" runat="server" />
                                <asp:Label ID="lblSMSMessageCount" runat="server" CssClass="badge margin-all-sm pull-right" />
                                <Rock:RockTextBox ID="tbSMSTextMessage" runat="server" CssClass="js-sms-text-message" TextMode="MultiLine" Rows="3" Required="true" ValidationGroup="vgMobileTextEditor" RequiredErrorMessage="Message is required" ValidateRequestMode="Disabled" />
                                <Rock:NotificationBox ID="nbSMSTestResult" CssClass="margin-t-md" runat="server" NotificationBoxType="Success" Text="Test SMS has been sent." Visible="false" />
                                <div class="control-actions margin-t-sm pull-right">
                                    <a class="btn btn-xs btn-default js-sms-sendtest" href="#">Send Test</a>
                                    <asp:LinkButton ID="btnSMSEditorSaveDraft" runat="server" CssClass="btn btn-xs btn-default" Text="Save" OnClick="btnSMSEditorSaveDraft_Click" />
                                    <div class="js-sms-sendtest-inputs" style="display: none">
                                        <Rock:RockTextBox ID="tbTestSMSNumber" runat="server" Label="SMS Number" ValidationGroup="vgMobileTextEditorSendTest" Required="true" Help="This will temporarily change your SMS number during the test, but it will be changed back after the test is complete." />
                                        <asp:Button ID="btnSMSSendTest" runat="server" CssClass="btn btn-xs btn-primary" Text="Send" CausesValidation="true" ValidationGroup="vgMobileTextEditorSendTest" OnClick="btnSMSSendTest_Click" />
                                        <a class="btn btn-xs btn-link js-sms-sendtest-cancel" href="#">Cancel</a>
                                    </div>
                                </div>
                            </Rock:RockControlWrapper>
                            <Rock:FileUploader ID="fupMobileAttachment" runat="server" Label="Attachment" OnFileUploaded="fupMobileAttachment_FileUploaded" OnFileRemoved="fupMobileAttachment_FileRemoved" />
                            <Rock:NotificationBox ID="nbMobileAttachmentSizeWarning" runat="server" NotificationBoxType="Warning" Text="" Dismissable="true" Visible="false" />
                            <Rock:NotificationBox ID="nbMobileAttachmentFileTypeWarning" runat="server" NotificationBoxType="Warning" Text="" Dismissable="true" Visible="false" />
                        </div>
                        <div class="col-md-6">
                            <div class="device device-mobile hidden-sm hidden-xs mb-5">
                                <div class="sms">
                                    <header>
                                        <span class="left">Messages</span><h2>
                                            <asp:Literal ID="lSMSChatPerson" runat="server" Text="Ted Decker" /></h2>
                                        <span class="right">Contacts</span>
                                    </header>
                                    <div class="messages-wrapper">
                                        <div class="js-sms-chatoutput message to">
                                            <asp:Label ID="lblSMSPreview" runat="server" CssClass="js-sms-preview" />
                                        </div>

                                        <div id="divAttachmentLoadError" runat="server" style="display: none" class="alert alert-danger margin-all-md" />
                                        <asp:Image ID="imgSMSImageAttachment" runat="server" CssClass="pull-right margin-r-md" onerror="showSMSAttachmentLoadError()" Width="50%" />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="actions clearfix mt-auto">
                        <asp:LinkButton ID="btnMobileTextEditorPrevious" runat="server" data-shortcut-key="arrowleft" Text="Previous" ToolTip="Alt+🡄" CssClass="btn btn-default js-wizard-navigation" CausesValidation="false" OnClick="btnMobileTextEditorPrevious_Click" />
                        <asp:LinkButton ID="btnMobileTextEditorNext" runat="server" data-shortcut-key="arrowright" Text="Next" ToolTip="Alt+🡆" DataLoadingText="Next" CssClass="btn btn-primary pull-right js-wizard-navigation" ValidationGroup="vgMobileTextEditor" CausesValidation="true" OnClick="btnMobileTextEditorNext_Click" />
                    </div>
                </asp:Panel>

                <%-- Push Editor --%>
                <asp:Panel ID="pnlPushEditor" CssClass="js-navigation-panel h-100 d-flex flex-column" runat="server" Visible="false">
                    <h1 class="step-title text-break">Push Notification Editor</h1>
                    <asp:ValidationSummary ID="vsPushEditor" runat="server" HeaderText="Please correct the following:" ValidationGroup="vgPushEditor" CssClass="alert alert-validation" />
                    <div class="row">
                        <div class="col-md-12">
                            <asp:PlaceHolder ID="phPushControl" runat="server" />
                        </div>
                    </div>
                    <div class="actions clearfix mt-auto">
                        <asp:LinkButton ID="btnPushEditorPrevious" runat="server" data-shortcut-key="arrowleft" ToolTip="Alt+p" Text="Previous" CssClass="btn btn-default js-wizard-navigation" CausesValidation="false" OnClick="btnPushEditorPrevious_Click" />
                        <asp:LinkButton ID="btnPushEditorNext" runat="server" data-shortcut-key="arrowright" Text="Next" ToolTip="Alt+🡆" DataLoadingText="Next" CssClass="btn btn-primary pull-right js-wizard-navigation" ValidationGroup="vgPushEditor" CausesValidation="true" OnClick="btnPushEditorNext_Click" />
                    </div>
                </asp:Panel>

                <%-- Confirmation --%>
                <asp:Panel ID="pnlConfirmation" CssClass="js-navigation-panel h-100 d-flex flex-column" runat="server" Visible="false">
                    <div class="panel-fill-body position-relative flex-fill styled-scroll">
                        <div class="position-absolute inset-0 overflow-auto">
                            <div class="panel-body h-100 d-flex flex-column">
                                <div>
                                <h1 class="step-title text-break">Confirmation</h1>
                                <p>Your communication is ready to be sent. Please review these final details before sending.</p>

                                <hr />
                                </div>
                                <Rock:NotificationBox ID="nbConfirmation" runat="server" NotificationBoxType="Info" />
                                <div class="row d-flex flex-wrap flex-fill">
                                    <div class="col-xs-12 col-md-4">
                                        <Rock:RockLiteral runat="server" ID="litRecipientCount" Label="Recipient Count" Text="234" />
                                        <Rock:RockLiteral runat="server" ID="litCommunicationName" Label="Communication Name" Text="234" />
                                        <Rock:RockLiteral runat="server" ID="litSchedule" Label="Schedule" Text="234" />
                                        <Rock:RockLiteral runat="server" ID="litRecipientList" Label="Recipient List" Text="234" />
                                        <Rock:RockLiteral runat="server" ID="litTemplate" Label="Template" Text="234" />
                                        <Rock:RockLiteral runat="server" ID="litCommunicationMedium" Label="Communication Medium" Text="234" />
                                    </div>
                                    <div class="col-xs-12 col-md-8 d-flex flex-column">
                                        <div>
                                            <ul class="nav nav-pills">
                                                <li id="tabEmail" runat="server" class="active">
                                                    <asp:LinkButton ID="lnkEmail" runat="server" Text="Email" CssClass="show-pill" OnClick="lnkEmail_Click" pill="email-tab" />
                                                </li>
                                                <li id="tabSMS" runat="server">
                                                    <asp:LinkButton ID="lnkSms" runat="server" Text="SMS" CssClass="show-pill" OnClick="lnkSms_Click" pill="sms-tab" />
                                                </li>
                                                <li id="tabPush" runat="server">
                                                    <asp:LinkButton ID="lnkPush" runat="server" Text="Push" CssClass="show-pill" OnClick="lnkPush_Click" pill="push-tab" />
                                                </li>
                                            </ul>
                                            <hr />
                                        </div>
                                        <div class="tabContent flex-fill">
                                            <asp:Panel runat="server" ID="pnlEmailTab" CssClass="tab-panel d-flex flex-column h-100">
                                                <div class="row">
                                                    <div class="col-sm-4">
                                                        <Rock:RockLiteral runat="server" ID="litEmailConfirmationFrom" Label="From" />
                                                    </div>
                                                    <div class="col-sm-8">
                                                        <Rock:RockLiteral runat="server" ID="litEmailConfirmationSubject" Label="Subject" />
                                                    </div>
                                                </div>
                                                <div class="row">
                                                    <div class="col-sm-4">
                                                        <Rock:RockLiteral runat="server" ID="litEmailConfirmationReplyTo" Label="Reply To" />
                                                    </div>
                                                    <div class="col-sm-4">
                                                        <Rock:RockLiteral runat="server" ID="litEmailConfirmationCc" Label="CC" />
                                                    </div>
                                                    <div class="col-sm-4">
                                                        <Rock:RockLiteral runat="server" ID="litEmailConfirmationBcc" Label="BCC" />
                                                    </div>
                                                </div>
                                                <div class="bg-gray-100 flex-fill position-relative mb-3 mb-sm-0 styled-scroll" style="min-height:300px;">
                                                    <div class="position-absolute w-100 h-100 inset-0 overflow-auto">
                                                        <iframe
                                                            runat="server"
                                                            id="ifConfirmationEmailPreview"
                                                            name="confirmationemailpreview-iframe"
                                                            src="javascript: window.frameElement.getAttribute('srcdoc');"
                                                            frameborder="0"
                                                            border="0"
                                                            cellspacing="0"
                                                            scrolling="yes"
                                                            class="w-100" />
                                                    </div>
                                                </div>

                                            </asp:Panel>
                                            <asp:Panel runat="server" ID="pnlSmsTab" CssClass="tab-panel">
                                                <div class="row">
                                                    <div class="col-sm-4">
                                                        <Rock:RockLiteral runat="server" ID="litConfirmationSmsFromNumber" Label="From" />
                                                    </div>
                                                </div>
                                                <div class="col-md-6">
                                                    <div class="device device-mobile hidden-sm hidden-xs mb-5">
                                                        <div class="sms">
                                                            <header>
                                                                <span class="left">Messages</span><h2>
                                                                    <asp:Literal ID="lblConfirmationSmsTo" runat="server" Text="Ted Decker" /></h2>
                                                                <span class="right">Contacts</span>
                                                            </header>
                                                            <div class="messages-wrapper">
                                                                <div class="message to">
                                                                    <asp:Label ID="lblConfirmationSmsMessage" runat="server" />
                                                                </div>

                                                                <div id="divConfirmationSmsImageAttachmentLoadError" runat="server" style="display: none" class="alert alert-danger margin-all-md" />
                                                                <asp:Image ID="imgConfirmationSmsImageAttachment" runat="server" CssClass="pull-right margin-r-md" onerror="showConfirmationSMSAttachmentLoadError()" Width="50%" />
                                                            </div>
                                                        </div>
                                                    </div>
                                                </div>
                                            </asp:Panel>
                                            <asp:Panel runat="server" ID="pnlPush" CssClass="tab-panel">
                                                <div class="row">
                                                    <div class="col-sm-4">
                                                        <Rock:RockLiteral runat="server" ID="litConfirmationPushTitle" Label="Title" />
                                                        <Rock:RockLiteral runat="server" ID="litConfirmationPushMessage" Label="Message" />
                                                    </div>
                                                    <div class="col-sm-4">
                                                        <Rock:RockLiteral runat="server" ID="litConfirmationPushOpenAction" Label="Open Action" />
                                                    </div>
                                                    <div class="col-sm-4">
                                                        <Rock:RockLiteral runat="server" ID="litConfirmationPushOpenActionDetails" Label="Open Action Details" />
                                                    </div>
                                                </div>
                                            </asp:Panel>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="actions clearfix mt-3">
                            <asp:LinkButton ID="btnConfirmationPrevious" runat="server" data-shortcut-key="arrowleft" Text="Previous" ToolTip="Alt+🡄" CssClass="btn btn-default mb-2 mb-sm-0 " CausesValidation="false" OnClick="btnConfirmationPrevious_Click" />
                            <div class="pull-right">
                                <asp:LinkButton ID="btnSaveAsDraft" runat="server" Text="Save as Draft" CssClass="btn btn-default mb-2 mb-sm-0" CausesValidation="true" ValidationGroup="vgConfirmation" OnClick="btnSaveAsDraft_Click" />
                                <asp:LinkButton ID="btnSend" runat="server" Text="Send" CssClass="btn btn-primary mb-2 mb-sm-0" CausesValidation="true" ValidationGroup="vgConfirmation" OnClick="btnSend_Click" />
                            </div>
                    </div>
                </asp:Panel>

                <%-- Panel: Result --%>
                <asp:Panel ID="pnlResult" runat="server" Visible="false">
                    <%-- Task Activity --%>
                    <asp:Panel ID="pnlTaskActivity" runat="server" CssClass="panel js-global-task-progress">
                        <div class="panel-heading">
                            <p>Your communication is being created. This may take some time for a large number of recipients...</p>
                        </div>
                        <div class="panel-body">
                            <div>
                                <span class="mr-1"><i class="fa fa-spinner fa-spin"></i></span>
                                <span id="_TaskActivityMessage">Working...</span>
                            </div>
                            <div id="_TaskActivityBar" class="mt-1 js-global-task-progress-long-running" style="display: none">
                                <div class="progress" style="position: relative">
                                    <div id="_TaskActivityBarFill" class="progress-bar progress-bar-info progress-bar-striped active" role="progressbar"
                                        aria-valuenow="0" aria-valuemin="0" aria-valuemax="100" style="width: 0%">
                                        <span style="position: absolute; display: block; color: black; width: 100%; text-align: center">
                                            <span id="_TaskActivityPercentage">0.0% complete</span>
                                            <span id="_TaskActivityTime" class="mr-1"></span>
                                        </span>
                                    </div>
                                </div>
                            </div>
                            <div id="_TaskActivityLog" style="display: none">
                                <h3>Activity Log</h3>
                                <div class="alert alert-info scrollableContainer" id="messageContainer" runat="server" />
                            </div>
                        </div>
                    </asp:Panel>

                    <%-- Task Result --%>
                    <asp:Panel ID="pnlTaskResult" runat="server" CssClass="panel js-global-task-result">
                        <div class="panel-body">
                            <Rock:NotificationBox ID="TaskActivityNotificationBox" runat="server" NotificationBoxType="Success" Text="Task complete." />
                            <br />
                            <asp:HyperLink ID="hlViewCommunication" runat="server" Text="View Communication" />
                        </div>
                    </asp:Panel>

                </asp:Panel>

            </div>
        </asp:Panel>

        <script type="text/javascript">
            Sys.Application.add_load(function () {
                Rock.controls.fullScreen.initialize('body');

                if ($('#<%=pnlEmailEditor.ClientID%>').length) {
                    loadEmailEditor();
                }

                if ($('#<%=pnlEmailPreview.ClientID%>').length) {
                    var $emailPreviewIframe = $('.js-emailpreview-iframe');

                    var $previewModal = $emailPreviewIframe.closest('.modal-content');

                    // set opacity to 0 to hide flicker when loading
                    $previewModal.fadeTo(0, 0);

                    $emailPreviewIframe.height('auto');

                    $emailPreviewIframe.load(function () {
                        new ResizeSensor($('#<%=pnlEmailPreviewContainer.ClientID%>'), function () {
                            $('#<%=ifEmailPreview.ClientID%>', window.parent.document).height($('#<%=pnlEmailPreviewContainer.ClientID%>').height());
                        });

                        var newHeight = $(this.contentWindow.document).height();
                        if ($(this).height() != newHeight) {
                            $(this).height(newHeight);
                        }

                        $('#<%=pnlEmailPreviewContainer.ClientID%>').height(newHeight);

                        if ($previewModal.is(':visible')) {
                            // set opacity back to 1 now that it is loaded and sized
                            $previewModal.fadeTo(0, 1);
                        }
                    });
                }

                $(".js-send-communications-date input").on('change', function (e) {
                    var ctrl = $(this);
                    if (!ctrl || !ctrl.length || !$(ctrl[0]).val()) {
                        return;
                    }

                    ctrl = $('.js-send-immediately');
                    if (!ctrl || !ctrl.length || !ctrl[0].checked) {
                        return;
                    }

                    ctrl[0].checked = false;
                });

                $('.js-send-immediately').on('click', function (e) {
                    var ctrl = $(this);
                    if (!ctrl || !ctrl.length) {
                        return;
                    }
                    var isChecked = $(this)[0].checked;
                    if (isChecked) {
                        var dateInputControls = $(".js-send-communications-date input")

                        for (var i = 0; i < dateInputControls.length; i++) {
                            $(dateInputControls[i]).val('')
                        }

                    }

                });

                $('.js-show-additional-fields').off('click').on('click', function () {
                    var isVisible = !$('.js-additional-fields').is(':visible');
                    $('#<%=hfShowAdditionalFields.ClientID %>').val(isVisible);
                    $('.js-show-additional-fields').text(isVisible ? 'Hide Additional Fields' : 'Show Additional Fields');
                    $('.js-additional-fields').slideToggle();
                    return false;
                });

                if ($('#<%=hfShowAdditionalFields.ClientID %>').val() == "true") {
                    $('.js-additional-fields').show();
                    $('.js-show-additional-fields').text('Hide Additional Fields');
                }

                $('.js-rsvp-show-advanced-settings').off('click').on('click', function () {
                    var isVisible = !$('.js-rsvp-advanced-settings').is(':visible');
                    $('#<%=hfRSVPShowAdvancedSettings.ClientID %>').val(isVisible);
                    $('.js-rsvp-show-advanced-settings').text(isVisible ? 'Hide Advanced Settings' : 'Show Advanced Settings');
                    $('.js-rsvp-advanced-settings').slideToggle();
                    return false;
                });

                $('.js-rsvp-advanced-settings').hide();
                $('.js-rsvp-show-advanced-settings').text('Show Advanced Settings');

                $('.js-email-sendtest').off('click').on('click', function () {
                    // $('#<%=btnEmailEditorSaveDraft.ClientID%>').hide();
                    $('#<%=nbEmailTestResult.ClientID%>').hide();
                    $(this).hide();
                    // $('#<%=btnEmailPreview.ClientID%>').hide();

                    $('.js-email-sendtest-inputs').slideDown();
                    return false;
                });

                $('.js-email-sendtest-cancel').off('click').on('click', function () {
                    $('.js-email-sendtest').show();
                    // $('#<%=btnEmailPreview.ClientID%>').show();
                    //$('#<%=btnEmailEditorSaveDraft.ClientID%>').show();
                    $('.js-email-sendtest-inputs').hide();
                    return false;
                });

                $('.js-sms-sendtest').off('click').on('click', function () {
                    $('#<%=btnSMSEditorSaveDraft.ClientID%>').hide();
                    $(this).hide();

                    $('.js-sms-sendtest-inputs').slideDown();
                    return false;
                });

                $('.js-sms-sendtest-cancel').off('click').on('click', function () {
                    $('#<%=btnSMSEditorSaveDraft.ClientID%>').show();
                    $('.js-sms-sendtest').show();
                    $('.js-sms-sendtest-inputs').hide();
                    return false;
                });

                var $smsTextMessage = $('#<%=tbSMSTextMessage.ClientID %>');

                if ($smsTextMessage.length) {

                    setSMSBubbleText();
                    $smsTextMessage.off('input').on('input', function () {
                        setSMSBubbleText();
                    });
                }

                var smsCharLimit = $('#<%=hfSMSCharLimit.ClientID%>').val();
                if (smsCharLimit && smsCharLimit > 0) {
                    $('#<%=tbSMSTextMessage.ClientID%>').limit(
                        {
                            maxChars: smsCharLimit,
                            counter: '#<%=lblSMSMessageCount.ClientID%>',
                            normalClass: 'badge',
                            warningClass: 'badge-warning',
                            overLimitClass: 'badge-danger'
                        });
                }

                $('.js-saveeditorhtml').off('click');
                $('.js-wizard-navigation').off('click');

                // save the editor html (minus the editor components) to a hidden field
                $('.js-saveeditorhtml').on('click', function () {
                    var $editorIframe = $('#<%=ifEmailDesigner.ClientID%>');
                    var $editorHtml = $editorIframe.contents().find('HTML').clone();

                    // remove all the email editor stuff
                    $editorHtml.find('.js-emaileditor-addon').remove();

                    var emailHtmlContent = $editorHtml[0].outerHTML;
                    var doctype = '<!DOCTYPE html>' + '\n';

                    $('#<%=hfEmailEditorHtml.ClientID%>').val(doctype + emailHtmlContent);
                });

                // make sure scroll position is set to top after navigating (so that stuff doesn't roll out of view if navigating from a tall to a short height )
                $('.js-wizard-navigation').on('click', function () {
                    $('html, body').animate({
                        scrollTop: $('#<%=upnlContent.ClientID%>')
                    }, 'fast');
                });

                // resize the email preview when the Mobile/Desktop modes are clicked
                $('.js-preview-mobile, .js-preview-desktop').off('click').on('click', function (a, b, c) {
                    var $emailPreviewIframe = $('.js-emailpreview-iframe');

                    if ($(this).hasClass('js-preview-mobile')) {
                        var mobileContainerHeight = '585px';

                        $('.js-email-preview').removeClass("device-browser").addClass("device-mobile");
                        var mobilePreviewHeight = $('.js-email-preview').height();

                        $emailPreviewIframe.height(mobileContainerHeight);
                        $('#<%=pnlEmailPreviewContainer.ClientID%>').height(mobileContainerHeight);
                    }
                    else {
                        $('.js-email-preview').removeClass("device-mobile").addClass("device-browser");
                        $emailPreviewIframe.height('auto');

                        var emailPreviewIframe = $emailPreviewIframe[0];
                        var newHeight = $(emailPreviewIframe.contentWindow.document).height();
                        if ($(emailPreviewIframe).height() != newHeight) {
                            $(emailPreviewIframe).height(newHeight);
                        }

                        $('#<%=pnlEmailPreviewContainer.ClientID%>').height(newHeight);
                    }
                });
            });

            function resizeIframe(el) {
                el.style.height = el.contentWindow.document.documentElement.scrollHeight + 'px';
            }

            function removeAttachment(source, hf, fileId) {
                // Get the attachment list
                var $hf = $('#' + hf);
                var fileIds = $hf.val().split(',');

                // Remove the selected attachment
                var removeAt = $.inArray(fileId, fileIds);
                fileIds.splice(removeAt, 1);
                $hf.val(fileIds.join());

                // Remove parent <li>
                $(source).closest($('li')).remove();
            }

            function loadEmailEditor() {
                var $editorIframe = $('.js-emaileditor-iframe');
                if ($editorIframe[0].contentWindow.Rock) {
                    // already loaded (probably because the btnEmailSendTest triggered a postback)
                    return;
                }

                // load in editor styles and scripts
                var cssLink = document.createElement("link")
                cssLink.className = "js-emaileditor-addon";
                cssLink.href = '<%=RockPage.ResolveRockUrl("~/Themes/Rock/Styles/email-editor.css", true ) %>';
                cssLink.rel = "stylesheet";
                cssLink.type = "text/css";

                var jqueryLoaderScript = document.createElement("script");
                jqueryLoaderScript.async = false;
                jqueryLoaderScript.className = "js-emaileditor-addon";
                jqueryLoaderScript.type = "text/javascript";
                jqueryLoaderScript.src = '<%=System.Web.Optimization.Scripts.Url("~/Scripts/Bundles/RockJQueryLatest" ) %>';

                var dragulaLoaderScript = document.createElement("script");
                dragulaLoaderScript.async = false;
                dragulaLoaderScript.setAttribute('defer', '');
                dragulaLoaderScript.className = "js-emaileditor-addon";
                dragulaLoaderScript.type = "text/javascript";
                dragulaLoaderScript.src = '<%=RockPage.ResolveRockUrl("~/Scripts/dragula.min.js", true ) %>';

                var editorScript = document.createElement("script");
                editorScript.async = false;
                editorScript.setAttribute('defer', '');
                editorScript.className = "js-emaileditor-addon";
                editorScript.type = "text/javascript";
                editorScript.src = '<%=RockPage.ResolveRockUrl("~/Scripts/Rock/Controls/EmailEditor/email-editor.js", true ) %>';
                editorScript.onload = function () {
                    $editorIframe[0].contentWindow.Rock.controls.emailEditor.initialize({
                        id: 'editor-controls',
                        componentSelected: loadPropertiesPage
                    });
                };

                $editorIframe.load(function () {
                    frames['emaileditor-iframe'].document.head.appendChild(cssLink)
                        .appendChild(jqueryLoaderScript)
                        .appendChild(dragulaLoaderScript)
                        .appendChild(editorScript);

                    var $this = $(this);
                    var contents = $this.contents();
                    var editorMarkup = $('#editor-controls-markup').contents();

                    $(contents).find('body').prepend(editorMarkup);

                    $('#emaileditor-designer').delay(500).css('visibility', 'visible').css('opacity', '1');
                });

                if ($editorIframe.length) {
                    $editorIframe[0].src = 'javascript: window.frameElement.getAttribute("srcdoc")';

                    // initialize component helpers
                    Rock.controls.emailEditor.rsvpComponentHelper.initializeEventHandlers();
                    Rock.controls.emailEditor.buttonComponentHelper.initializeEventHandlers();
                    Rock.controls.emailEditor.codeComponentHelper.initializeEventHandlers();
                    Rock.controls.emailEditor.dividerComponentHelper.initializeEventHandlers();
                    Rock.controls.emailEditor.imageComponentHelper.initializeEventHandlers();
                    Rock.controls.emailEditor.videoComponentHelper.initializeEventHandlers();
                    Rock.controls.emailEditor.textComponentHelper.initializeEventHandlers();
                    Rock.controls.emailEditor.sectionComponentHelper.initializeEventHandlers();
                }
            }

            var $currentComponent;

            function loadPropertiesPage(componentType, $component) {
                $currentComponent = $component;
                var $currentPropertiesPanel = $('.js-propertypanels').find("[data-component='" + componentType + "']");

                // hide all property panels
                $('.propertypanel').hide();

                // hide any summernote popovers that might be hanging out
                $('.note-popover.popover').hide();

                // temp - set text of summernote
                switch (componentType) {
                    case 'text':
                        $currentComponent = Rock.controls.emailEditor.textComponentHelper.setProperties($currentComponent);
                        break;
                    case 'rsvp':
                        $currentComponent = Rock.controls.emailEditor.rsvpComponentHelper.setProperties($currentComponent);
                        break;
                    case 'button':
                        $currentComponent = Rock.controls.emailEditor.buttonComponentHelper.setProperties($currentComponent);
                        break;
                    case 'image':
                        $currentComponent = Rock.controls.emailEditor.imageComponentHelper.setProperties($currentComponent);
                        break;
                    case 'video':
                        $currentComponent = Rock.controls.emailEditor.videoComponentHelper.setProperties($currentComponent);
				        break;
                    case 'section':
                        $currentComponent = Rock.controls.emailEditor.sectionComponentHelper.setProperties($currentComponent);
                        break;
                    case 'divider':
                        $currentComponent = Rock.controls.emailEditor.dividerComponentHelper.setProperties($currentComponent);
                        break;
                    case 'code':
                        $currentComponent = Rock.controls.emailEditor.codeComponentHelper.setProperties($currentComponent);
                        break;
                    default:
                        clearPropertyPane(null);
                        $currentComponent = null;
                }

                // show proper panel
                $currentPropertiesPanel.show();

                // show panel actions
                $('.js-propertypanel-actions').show();
            }

            // function that components will call after they have processed their own save and close logic
            function clearPropertyPane(e) {

                // hide all property panes, hide panel actions and set current as not selected
                $('.propertypanel').hide();
                $('.js-propertypanel-actions').hide();
                $currentComponent.removeClass('selected');

                // hide any summernote popovers that might be hanging out
                $('.note-popover.popover').hide();

                if (e != null) {
                    e.preventDefault();
                }
            }

            // function that will remove the currently selected component from the email html
            function deleteCurrentComponent() {
                $currentComponent.remove();
                clearPropertyPane(null);
            }

            function updateTextComponent(el, contents) {
                Rock.controls.emailEditor.textComponentHelper.updateTextComponent(el, contents);
            }

            function updateCodeComponent(el, contents) {
                Rock.controls.emailEditor.codeComponentHelper.updateCodeComponent(el, contents);
            }

            function handleImageUpdate(e, data) {
                Rock.controls.emailEditor.imageComponentHelper.handleImageUpdate(e, data);
            }

            function handleAssetUpdate(e, data) {
                Rock.controls.emailEditor.imageComponentHelper.handleAssetUpdate(e, data);
            }

            function handleVideoImageUpdate(e, data) {
			    Rock.controls.emailEditor.videoComponentHelper.handleVideoImageUpdate(e, data);
            }

            // debouncing function from John Hann
            // http://unscriptable.com/index.php/2009/03/20/debouncing-javascript-methods/
            var debounceLavaRender = function (func, threshold, execAsap) {
                var timeout;

                return function debounced() {
                    var obj = this, args = arguments;
                    function delayed() {
                        if (!execAsap)
                            func.apply(obj, args);
                        timeout = null;
                    };

                    if (timeout) {
                        clearTimeout(timeout);
                    }
                    else if (execAsap)
                        func.apply(obj, args);

                    timeout = setTimeout(delayed, threshold || 250);
                };
            }

            // debounce Timeout for rendering SMS using Lava
            var smsBubbleTextLavaTimeout;

            function setSMSBubbleText() {
                var updatedText = $('#<%=tbSMSTextMessage.ClientID %>').val();

                if (updatedText) {
                    $('.js-sms-chatoutput').show();

                    // only send to api/Lava/RenderTemplate if it has lava fields in it
                    if (/.*\{.*\}.*/.test(updatedText)) {
                        // add lava that will assign the Person mergeField if there are merge fields in the text
                        var additionalMergeObjects = '15|Person|' + $('#<%=hfSMSSampleRecipientPersonId.ClientID%>').val();

                        if (smsBubbleTextLavaTimeout) {
                            clearTimeout(smsBubbleTextLavaTimeout);
                        }

                        smsBubbleTextLavaTimeout = setTimeout(function () {
                            $.post('<%=this.ResolveUrl("~/api/Lava/RenderTemplate")%>' + '?additionalMergeObjects=' + additionalMergeObjects, updatedText, function (data) {
                                if (data.startsWith('Error resolving Lava merge fields:')) {
                                    $('.js-sms-chatoutput').text(updatedText);
                                }
                                else {
                                    $('.js-sms-chatoutput').text(data);
                                }
                            }).fail(function (a, b, c) {
                                $('.js-sms-chatoutput').text(updatedText);
                            })
                        }, 100);
                    }
                    else {
                        $('.js-sms-chatoutput').text(updatedText);
                    }
                }
                else {
                    $('.js-sms-chatoutput').hide();
                }
            }

            function showSMSAttachmentLoadError(ctrlName) {
                $('#<%=divAttachmentLoadError.ClientID%>').show();
            }

            function showConfirmationSMSAttachmentLoadError(ctrlName) {
                $('#<%=divConfirmationSmsImageAttachmentLoadError.ClientID%>').show();
            }

            function setActiveMediumTypeButton($activeBtn) {
                if (!$activeBtn || !$activeBtn.length) {
                    return;
                }
                var val = $activeBtn.val();
                if (val === '<%=(int)Rock.Model.CommunicationType.RecipientPreference%>') {
                    $('.js-medium-recipientpreference-notification').show();
                } else {
                    $('.js-medium-recipientpreference-notification').hide();
                }
            }

        </script>

        <!-- Text Component -->
        <script src='<%=RockPage.ResolveRockUrl("~/Scripts/Rock/Controls/EmailEditor/textComponentHelper.js", true)%>'></script>

        <!-- Section Component -->
        <script src='<%=RockPage.ResolveRockUrl("~/Scripts/Rock/Controls/EmailEditor/sectionComponentHelper.js", true)%>'></script>

        <!-- Button Component -->
        <script src='<%=RockPage.ResolveRockUrl("~/Scripts/Rock/Controls/EmailEditor/buttonComponentHelper.js", true)%>'></script>

        <!-- RSVP Component -->
        <script src='<%=RockPage.ResolveRockUrl("~/Scripts/Rock/Controls/EmailEditor/rsvpComponentHelper.js", true)%>'></script>

        <!-- Image Component -->
        <script src='<%=RockPage.ResolveRockUrl("~/Scripts/Rock/Controls/EmailEditor/imageComponentHelper.js", true)%>'></script>

        <!-- Video Component -->
        <script src='<%=RockPage.ResolveRockUrl("~/Scripts/Rock/Controls/EmailEditor/videoComponentHelper.js", true)%>' ></script>

        <!-- Divider Component -->
        <script src='<%=RockPage.ResolveRockUrl("~/Scripts/Rock/Controls/EmailEditor/dividerComponentHelper.js", true)%>'></script>

        <!-- Code Component -->
        <script src='<%=RockPage.ResolveRockUrl("~/Scripts/Rock/Controls/EmailEditor/codeComponentHelper.js", true)%>'></script>
    </ContentTemplate>
</asp:UpdatePanel>
