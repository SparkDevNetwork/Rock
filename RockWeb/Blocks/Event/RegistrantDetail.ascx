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

                <div class="panel-heading clearfix">
                    <h1 class="panel-title pull-left"><asp:Literal ID="lTitle" runat="server"></asp:Literal></h1>
                    <div class="panel-labels">
                        <Rock:HighlightLabel ID="hlCost" runat="server" LabelType="Info" ToolTip="Cost" />
                    </div>
                </div>

                <div class="panel-body">

                    <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:PersonPicker ID="ppPerson" runat="server" Label="Person" Required="true" />
                            <asp:PlaceHolder ID="phFields" runat="server" />
                        </div>
                        <div class="col-md-6">
                            <Rock:CurrencyBox ID="cbCost" runat="server" Label="Cost" />
                            <div id="divFees" runat="server" class="well registration-additional-options">
                                <asp:PlaceHolder ID="phFees" runat="server" />
                            </div>
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>

                </div>
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
