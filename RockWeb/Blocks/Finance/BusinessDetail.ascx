<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BusinessDetail.ascx.cs" Inherits="RockWeb.Blocks.Finance.BusinessDetail" %>

<asp:UpdatePanel ID="upnlBusinesses" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">

            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-briefcase"></i>
                        <asp:Literal ID="lTitle" runat="server" /></h1>
                    <div class="panel-labels">
                        <Rock:HighlightLabel ID="hlStatus" runat="server" />

                        <asp:Panel ID="pnlActionWrapper" runat="server" CssClass="js-button-dropdownlist panel-options pull-right dropdown-right btn-group">
                            <button type="button" class="btn btn-default dropdown-toggle js-buttondropdown-btn-select" data-toggle="dropdown">Actions <i class="fa fa-caret-down"></i></button>
                            <ul class="dropdown-menu">
                                <asp:Literal ID="lActions" runat="server" />
                            </ul>
                        </asp:Panel>
                    </div>
                </div>
                <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
                <div class="panel-body">

                    <div id="pnlEditDetails" runat="server">
                        <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                        <asp:HiddenField ID="hfBusinessId" runat="server" />
                        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

                        <div class="row">
                            <div class="col-md-3">
                                <fieldset>
                                    <Rock:DefinedValuePicker ID="dvpRecordStatus" runat="server" Label="Record Status" AutoPostBack="true" OnSelectedIndexChanged="ddlRecordStatus_SelectedIndexChanged" />
                                    <Rock:DefinedValuePicker ID="dvpReason" runat="server" Label="Reason" Visible="false"></Rock:DefinedValuePicker>
                                </fieldset>
                            </div>
                            <div class="col-md-9">
                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:RockDropDownList ID="ddlCampus" runat="server" Label="Campus" />
                                    </div>
                                </div>

                                <Rock:DataTextBox ID="tbBusinessName" runat="server" Label="Name" SourceTypeName="Rock.Model.Person, Rock" PropertyName="FirstName" Required="true" />

                                <%-- Address Section --%>
                                <div class="panel panel-section">
                                    <div class="panel-heading">
                                        <h3 class="panel-title">Address</h3>
                                    </div>
                                    <div class="panel-body">
                                        <Rock:AddressControl ID="acAddress" runat="server" UseStateAbbreviation="true" UseCountryAbbreviation="false" />
                                        <Rock:RockCheckBox ID="cbSaveFormerAddressAsPreviousAddress" runat="server" Text="Save Former Address As Previous Address" />
                                    </div>
                                </div>

                                <%-- Contact Information Section --%>
                                <div class="panel panel-section">
                                    <div class="panel-heading">
                                        <h3 class="panel-title">Contact Information</h3>
                                    </div>
                                    <div class="panel-body">
                                        <div class="row">
                                            <div class="col-xs-12 col-sm-6">
                                                <Rock:PhoneNumberBox ID="pnbPhone" runat="server" Label="Phone Number" CountryCode='<%# Eval("CountryCode") %>' Number='<%# Eval("NumberFormatted")  %>' />
                                            </div>
                                            <div class="col-xs-3">
                                                <Rock:RockCheckBox ID="cbSms" runat="server" Text="SMS" Label="&nbsp;" Checked='<%# (bool)Eval("IsMessagingEnabled") %>' />
                                            </div>
                                            <div class="col-xs-3">
                                                <Rock:RockCheckBox ID="cbUnlisted" runat="server" Text="Unlisted" Label="&nbsp;" Checked='<%# (bool)Eval("IsUnlisted") %>' />
                                            </div>
                                        </div>
                                        <Rock:EmailBox ID="tbEmail" runat="server" Label="Email Address" />
                                        <Rock:RockRadioButtonList ID="rblEmailPreference" runat="server" RepeatDirection="Horizontal" Label="Email Preference">
                                            <asp:ListItem Text="Email Allowed" Value="EmailAllowed" Selected="True" />
                                            <asp:ListItem Text="No Mass Emails" Value="NoMassEmails" />
                                            <asp:ListItem Text="Do Not Email" Value="DoNotEmail" />
                                        </Rock:RockRadioButtonList>
                                    </div>
                                </div>

                                <%-- Attributes Section this will only show if there are attributes configured. --%>
                                <div id="pnlEditAttributes" runat="server" class="panel panel-section">
                                    <div class="panel-heading">
                                        <h3 class="panel-title">Attributes</h3>
                                    </div>
                                    <div class="panel-body">
                                        <Rock:AttributeValuesContainer ID="avcEditAttributes" runat="server"></Rock:AttributeValuesContainer>
                                    </div>
                                </div>

                                <%-- Advanced Settings Section --%>
                                <Rock:PanelWidget runat="server" ID="pwAdvanced" Title="Advanced Settings">
                                    <div class="row">
                                        <div class="col-md-12">
                                            <Rock:RockControlWrapper ID="rcwSearchKeys" runat="server" Label="Search Keys" Help="Search keys provide alternate ways to search for an individual.">
                                                <Rock:Grid ID="gSearchKeys" runat="server" DisplayType="Light" DataKeyNames="Guid" RowItemText="Search Key" ShowConfirmDeleteDialog="false">
                                                    <Columns>
                                                        <Rock:DefinedValueField DataField="SearchTypeValueId" HeaderText="Search Type" />
                                                        <Rock:RockBoundField DataField="SearchValue" HeaderText="Search Value" />
                                                        <Rock:DeleteField OnClick="gSearchKeys_Delete" />
                                                    </Columns>
                                                </Rock:Grid>
                                            </Rock:RockControlWrapper>
                                        </div>
                                    </div>
                                </Rock:PanelWidget>

                                <%-- Buttons --%>
                                <div class="actions">
                                    <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" />
                                    <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="lbCancel_Click" />
                                </div>

                            </div>
                        </div>
                    </div>

                    <%-- View Only panel --%>
                    <div id="pnlViewDetails" runat="server">
                        <h1 class="title name mt-0"><asp:Literal ID="lViewPanelBusinessName" runat="server" /></h1>
                        <Rock:BadgeListControl ID="blStatus" runat="server" />
                        <Rock:TagList ID="taglPersonTags" runat="server" CssClass="tag-list mt-3 mb-2 clearfix" />

                        <fieldset id="fieldsetViewSummary" runat="server">
                            <div class="row">
                                <div class="col-md-12">
                                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-6">
                                    <asp:Literal ID="lDetailsLeft" runat="server" />
                                </div>
                                <div class="col-md-6">
                                    <asp:Literal ID="lDetailsRight" runat="server" />
                                </div>
                            </div>

                            <%-- Attributes Section this will only show if there are attributes configured. --%>
                                <div id="pnlViewAttributes" runat="server" class="panel panel-section">
                                    <div class="panel-heading">
                                        <h3 class="panel-title">Attributes</h3>
                                    </div>
                                    <div class="panel-body">
                                        <Rock:AttributeValuesContainer ID="avcViewAttributes" runat="server"></Rock:AttributeValuesContainer>
                                    </div>
                                </div>

                            <div class="actions">
                                <asp:LinkButton ID="lbEdit" runat="server" Text="Edit" CssClass="btn btn-primary" CausesValidation="false" OnClick="lbEdit_Click" />
                            </div>
                        </fieldset>
                    </div>
                </div>
            </div>

            <Rock:ModalDialog runat="server" ID="mdSearchKey" Title="Add Search Key" ValidationGroup="vgSearchKey" OnSaveClick="mdSearchKey_SaveClick">
                <Content>
                    <Rock:RockDropDownList ID="ddlSearchValueType" runat="server" Label="Search Type" Required="true" ValidationGroup="vgSearchKey" />
                    <Rock:RockTextBox ID="tbSearchValue" runat="server" Label="Search Value" Required="true" ValidationGroup="vgSearchKey" autocomplete="off" />
                </Content>
            </Rock:ModalDialog>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
