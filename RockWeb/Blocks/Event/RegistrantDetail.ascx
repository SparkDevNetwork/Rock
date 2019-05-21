<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RegistrantDetail.ascx.cs" Inherits="RockWeb.Blocks.Event.RegistrantDetail" %>

<asp:UpdatePanel ID="upnlRegistrantDetail" runat="server">
    <ContentTemplate>

        <div class="wizard">

            <div class="wizard-item complete">
                <asp:LinkButton ID="lbWizardTemplate" runat="server" OnClick="lbWizardTemplate_Click" CausesValidation="false">
                    <%-- Placeholder needed for bug. See: http://stackoverflow.com/questions/5539327/inner-image-and-text-of-asplinkbutton-disappears-after-postback--%>
                    <asp:PlaceHolder runat="server">
                        <div class="wizard-item-icon">
                            <i class="fa fa-fw fa-clipboard"></i>
                        </div>
                        <div class="wizard-item-label">
                            <asp:Literal ID="lWizardTemplateName" runat="server" Text="Template" />
                        </div>
                    </asp:PlaceHolder>
                </asp:LinkButton>
            </div>

            <div class="wizard-item complete">
                <asp:LinkButton ID="lbWizardInstance" runat="server" OnClick="lbWizardInstance_Click" CausesValidation="false">
                    <asp:PlaceHolder runat="server">
                        <div class="wizard-item-icon">
                            <i class="fa fa-fw fa-file-o"></i>
                        </div>
                        <div class="wizard-item-label">
                            <asp:Literal ID="lWizardInstanceName" runat="server" Text="Instance" />
                        </div>
                    </asp:PlaceHolder>
                </asp:LinkButton>
            </div>

            <div class="wizard-item complete">
                <asp:LinkButton ID="lbWizardRegistration" runat="server" OnClick="lbWizardRegistration_Click" CausesValidation="false">
                    <asp:PlaceHolder runat="server">
                        <div class="wizard-item-icon">
                            <i class="fa fa-fw fa-group"></i>
                        </div>
                        <div class="wizard-item-label">
                            <asp:Literal ID="lWizardRegistrationName" runat="server" Text="Registration" />
                        </div>
                    </asp:PlaceHolder>
                </asp:LinkButton>
            </div>

            <div class="wizard-item active">
                <div class="wizard-item-icon">
                    <i class="fa fa-fw fa-user"></i>
                </div>
                <div class="wizard-item-label">
                    <asp:Literal ID="lWizardRegistrantName" runat="server" Text="Registration" />
                </div>
            </div>
        </div>

        <asp:Panel ID="pnlDetails" runat="server">

            <div class="panel panel-block">

                <div class="panel-heading">
                    <h1 class="panel-title pull-left"><asp:Literal ID="lTitle" runat="server"></asp:Literal></h1>
                    <div class="panel-labels">
                        <Rock:HighlightLabel ID="hlCost" runat="server" LabelType="Info" ToolTip="Cost" />
                    </div>
                </div>

                <div class="panel-body">

                    <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:PersonPicker ID="ppPerson" runat="server" Label="Person" Required="true" />
                            <asp:PlaceHolder ID="phFields" runat="server" />
                        </div>
                        <div class="col-md-6">
                            <Rock:Toggle ID="tglWaitList" runat="server" Checked="true" OnText="Registrant" OffText="Wait List" Label="Status" ActiveButtonCssClass="btn-info" ButtonSizeCssClass="btn-xs" Help="Moving someone from a wait list here will not send any emails to them. To send a formatted email use the Wait List tab." />
                            <div class="row">
                                <div class="col-xs-6">
                                    <Rock:CurrencyBox ID="cbCost" runat="server" Label="Cost" />
                                </div>
                                <div class="col-xs-6">
                                    <Rock:RockCheckBox ID="cbDiscountApplies" runat="server" Label="Discount Applies" Text="Yes" Help="If there was a discount code used for the registration, should it apply to this registrant?" />
                                </div>
                            </div>
                            <div id="divFees" runat="server" class="well registration-additional-options">
                                <asp:PlaceHolder ID="phFees" runat="server" />
                            </div>
                            <asp:HiddenField ID="hfSignedDocumentId" runat="server" />
                            <Rock:FileUploader ID="fuSignedDocument" runat="server" Label="Signed Document" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary js-edit-registrant" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>

                </div>
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
