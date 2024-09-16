<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EventItemOccurrenceDetail.ascx.cs" Inherits="RockWeb.Blocks.Event.EventItemOccurrenceDetail" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upnlEventItemList" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server" CssClass="panel panel-block">

            <asp:HiddenField ID="hfEventItemOccurrenceId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-clock-o"></i>
                    <asp:Literal ID="lActionTitle" runat="server" /></h1>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">

                <asp:ValidationSummary ID="vsSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <asp:Panel ID="pnlViewDetails" runat="server">

                    <div class="row">
                        <div class="col-md-6">
                            <asp:Literal ID="lLeftDetails" runat="server" />
                        </div>
                        <div class="col-md-6">
                            <asp:Literal ID="lRightDetails" runat="server" />
                            <asp:PlaceHolder ID="phAttributes" runat="server"></asp:PlaceHolder>
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-12">
                            <Rock:RockLiteral ID="lOccurrenceNotes" Label="Occurrence Note" runat="server" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" data-shortcut-key="e" ToolTip="Alt+e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link js-delete-event" OnClick="btnDelete_Click" CausesValidation="false" />
                    </div>

                </asp:Panel>

                <asp:Panel ID="pnlEditDetails" runat="server">

                    <div class="row">

                        <div class="col-md-6">

                            <Rock:RockDropDownList ID="ddlCampus" runat="server" Label="Campus" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Model.Campus, Rock" PropertyName="Name" />
                            <Rock:RockTextBox ID="tbLocation" runat="server" Label="Location Description" />
                            <Rock:RockControlWrapper ID="rcwSchedule" runat="server" Label="Schedule">
                                <Rock:ScheduleBuilder ID="sbSchedule" runat="server" ValidationGroup="Schedule" AllowMultiSelect="true" Required="true" OnSaveSchedule="sbSchedule_SaveSchedule" />
                                <asp:Literal ID="lScheduleText" runat="server" />
                            </Rock:RockControlWrapper>

                            <asp:Repeater ID="rptRegistrations" runat="server" OnItemDataBound="rptRegistrations_ItemDataBound">
                                <HeaderTemplate>
                                    <label class="control-label">Event / Registration / Group Linkages</label>
                                    <table class="w-100">
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <tr>
                                        <td>
                                            <asp:HiddenField ID="hfGroupMapId" runat="server" Value='<%# Eval("GroupMapId") %>' />
                                            <span class="margin-b-none margin-r-md"><%# Eval("PublicName") %></span>
                                        </td>
                                        <td>
                                            <%# Eval("GroupName") %>
                                        </td>
                                        <td>
                                            <%# Eval("UrlSlug") %>
                                        </td>
                                        <td class="text-right">
                                            <asp:LinkButton ID="lbEditRegistration"
                                                runat="server"
                                                CssClass="btn btn-default btn-xs"
                                                OnCommand="lbEditRegistration_Command"
                                                CommandArgument='<%# Eval("GroupMapId") %>'><i class="fa fa-pencil"></i></asp:LinkButton>
                                            <asp:LinkButton ID="lbDeleteRegistration"
                                                runat="server"
                                                CssClass="btn btn-danger btn-xs"
                                                OnCommand="lbDeleteRegistration_Command"
                                                CommandArgument='<%# Eval("GroupMapId") %>'><i class="fa fa-times"></i></asp:LinkButton>
                                        </td>
                                    </tr>
                                </ItemTemplate>
                                <FooterTemplate>
                                    </table>
                                    <div class="row">
                                        <div class="col-sm-12">
                                            <asp:LinkButton ID="lbCreateNewRegistration"
                                                runat="server"
                                                CssClass="btn btn-primary btn-xs margin-t-sm margin-b-md"
                                                Text="<i class='fa fa-plus'></i> Add Linkage"
                                                OnClick="lbCreateNewRegistration_Click" />
                                        </div>
                                    </div>
                                </FooterTemplate>
                            </asp:Repeater>
                        </div>

                        <div class="col-md-6">
                            <Rock:PersonPicker ID="ppContact" runat="server" Label="Contact" EnableSelfSelection="true" OnSelectPerson="ppContact_SelectPerson" />
                            <Rock:PhoneNumberBox ID="pnPhone" runat="server" Label="Phone" />
                            <Rock:EmailBox ID="tbEmail" runat="server" Label="Email" />
                        </div>
                    </div>

                    <Rock:HtmlEditor ID="htmlOccurrenceNote" runat="server" Label="Occurrence Note" />

                    <Rock:PanelWidget ID="wpAttributes" runat="server" Title="Attribute Values">
                        <Rock:DynamicPlaceholder ID="phAttributeEdits" runat="server"></Rock:DynamicPlaceholder>
                    </Rock:PanelWidget>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" data-shortcut-key="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" data-shortcut-key="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>
                </asp:Panel>
            </div>
        </asp:Panel>

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog runat="server"
            ID="dlgNewEventRegistrationGroupLinkage"
            Title="New Event/Registration/Group Linkage"
            SaveButtonText="Save"
            OnSaveClick="dlgNewEventRegistrationGroupLinkage_SaveClick"
            OnCancelScript="clearActiveDialog();"
            ValidationGroup="NewEventRegistrationGroupLinkage">
            <Content>
                <asp:ValidationSummary runat="server"
                    ID="vsNewEventRegistrationGroupLinkage"
                    HeaderText="Please correct the following:"
                    CssClass="alert alert-validation"
                    ValidationGroup="NewEventRegistrationGroupLinkage" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox runat="server"
                            ID="tbNewLinkageUrlSlug"
                            Label="URL Slug"
                            Help=" Optional URL key to use to retrieve the linkage details on your external website. You will need to configure blocks on your site to use this key." />
                        <asp:CustomValidator runat="server"
                            ID="cvUrlSlug"
                            ErrorMessage="URL Slug must be unique across all events."
                            ControlToValidate="tbNewLinkageUrlSlug"
                            OnServerValidate="cvUrlSlug_ServerValidate"
                            ValidationGroup="NewEventRegistrationGroupLinkage" />
                        <asp:CustomValidator runat="server"
                            ID="rvUrlSlugForNewLinkage"
                            ErrorMessage="URL Slug must be lowercase and cannot contain any special characters other than -"
                            ControlToValidate="tbNewLinkageUrlSlug"
                            OnServerValidate="rvUrlSlug_ServerValidate"
                            ValidationGroup="NewEventRegistrationGroupLinkage"
                            Display="None" />
                    </div>
                    <div class="col-md-6">
                        <Rock:GroupPicker ID="gpNewLinkageGroup" runat="server" Label="Group" ValidationGroup="NewEventRegistrationGroupLinkage" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-12">
                        <Rock:ButtonGroup runat="server"
                            ID="bgLinkageOptions"
                            AutoPostBack="true"
                            OnSelectedIndexChanged="bgLinkageOptions_SelectedIndexChanged"
                            CssClass="text-small"
                            SelectedItemClass="btn btn-xs btn-primary"
                            UnselectedItemClass="btn btn-xs btn-default">
                            <asp:ListItem Text="No Registration Required" Value="None" Selected="True" />
                            <asp:ListItem Text="Create Registration Instance" Value="New" />
                            <asp:ListItem Text="Use Existing Registration Instance" Value="Existing" />
                        </Rock:ButtonGroup>
                    </div>
                </div>
                <div class="row mt-3">
                    <div class="col-md-12">
                        <asp:Panel runat="server" ID="pnlEditRegistrationInstance" Visible="false" CssClass="panel panel-section">
                            <div class="panel-heading">
                                <h5 class="panel-title">Registration Type</h5>
                            </div>

                            <div class="panel-body">
                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:RockDropDownList ID="ddlExistingLinkageTemplate" runat="server" Label="Registration Template" ValidationGroup="NewEventRegistrationGroupLinkage"
                                            AutoPostBack="true" OnSelectedIndexChanged="ddlExistingLinkageTemplate_SelectedIndexChanged" CausesValidation="false"
                                            Required="true" EnhanceForLongLists="true" />
                                    </div>
                                    <div class="col-md-6">
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:RockDropDownList ID="ddlExistingLinkageInstance" runat="server" Label="Registration Instance" ValidationGroup="NewEventRegistrationGroupLinkage" Required="true" EnhanceForLongLists="true" />
                                    </div>
                                    <div class="col-md-6">
                                        <Rock:RockTextBox ID="tbExistingLinkagePublicName" runat="server" Label="Public Name" Required="true" ValidationGroup="NewEventRegistrationGroupLinkage" />
                                    </div>
                                </div>
                            </div>
                        </asp:Panel>
                        <asp:Panel runat="server" ID="pnlNewRegistrationInstance" Visible="false" CssClass="panel panel-section">
                            <div class="panel-heading">
                                <h5 class="panel-title">Registration Type</h5>
                            </div>

                            <div class="panel-body">
                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:RockDropDownList ID="ddlNewLinkageTemplate" runat="server" Label="Registration Template" ValidationGroup="NewLinkage" Required="true" AutoPostBack="true" OnSelectedIndexChanged="ddlNewLinkageTemplate_SelectedIndexChanged" />
                                    </div>
                                    <div class="col-md-6">
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:RockTextBox ID="tbRegistrationInstanceName" runat="server" Label="Registration Instance Name" Required="true" ValidationGroup="NewEventRegistrationGroupLinkage" />
                                    </div>
                                    <div class="col-md-6">
                                        <Rock:RockTextBox ID="tbNewLinkagePublicName" runat="server" Label="Public Name" Required="true" ValidationGroup="NewEventRegistrationGroupLinkage" />
                                    </div>
                                </div>
                            </div>
                        </asp:Panel>
                        <Rock:RegistrationInstanceEditor ID="rieNewLinkage" runat="server" ValidationGroup="NewEventRegistrationGroupLinkage" ShowRegistrationTypeSection="false" />

                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgEditLinkage" runat="server" Title="Edit Registration Instance" SaveButtonText="OK" OnSaveClick="dlgEditLinkage_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="EditLinkage">
            <Content>
                <asp:HiddenField ID="hfEditLinkageGroupMapId" runat="server" />
                <asp:ValidationSummary ID="vsEditLinkage" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="EditLinkage" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbEditLinkageUrlSlug" runat="server" Label="URL Slug" ValidationGroup="EditLinkage" Help="When creating an event occurrence that specifies a campus, a URL Slug MUST be used when registering in order for the registrant to be placed into the linked group." />
                        <asp:CustomValidator runat="server"
                            ID="cvEditUrlSlug"
                            ErrorMessage="URL Slug must be unique across all events."
                            ControlToValidate="tbEditLinkageUrlSlug"
                            OnServerValidate="cvUrlSlug_ServerValidate"
                            ValidationGroup="EditLinkage" />
                        <asp:CustomValidator runat="server"
                            ID="rvEditUrlSlug"
                            ErrorMessage="URL Slug must be lowercase and cannot contain any special characters other than -"
                            ControlToValidate="tbEditLinkageUrlSlug"
                            OnServerValidate="rvUrlSlug_ServerValidate"
                            ValidationGroup="EditLinkage"
                            Display="None"/>
                    </div>
                    <div class="col-md-6">
                        <Rock:GroupPicker ID="gpEditLinkageGroup" runat="server" Label="Group" ValidationGroup="EditLinkage" />
                    </div>
                </div>

                <asp:Panel runat="server" ID="pnlEditLinkageRegistrationType" class="panel panel-section">
                    <div class="panel-heading">
                        <h5 class="panel-title">Registration Type</h5>
                    </div>

                    <div class="panel-body">
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockLiteral ID="lEditLinkageTemplate" runat="server" Label="Registration Template" />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockTextBox ID="tbEditLinkagePublicName" runat="server" Label="Public Name" Required="true" ValidationGroup="EditLinkage" />
                            </div>
                        </div>
                    </div>
                </asp:Panel>
                <Rock:RegistrationInstanceEditor ID="rieEditLinkage" runat="server" ValidationGroup="EditLinkage" ShowRegistrationTypeSection="false" />
            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>
