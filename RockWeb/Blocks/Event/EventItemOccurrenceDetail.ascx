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
                <h1 class="panel-title"><i class="fa fa-clock-o"></i> <asp:Literal ID="lActionTitle" runat="server" /></h1>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">

                <asp:ValidationSummary ID="vsSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <asp:Panel id="pnlViewDetails" runat="server">

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
                        <asp:LinkButton ID="btnEdit" runat="server" AccessKey="e" ToolTip="Alt+e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" CausesValidation="false" />
                    </div>

                </asp:Panel>

                <asp:Panel id="pnlEditDetails" runat="server">

                    <div class="row">

                        <div class="col-md-6">

                            <Rock:RockDropDownList ID="ddlCampus" runat="server" Label="Campus" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Model.Campus, Rock" PropertyName="Name" />
                            <Rock:RockTextBox ID="tbLocation" runat="server" Label="Location Description" />
                            <Rock:RockControlWrapper ID="rcwSchedule" runat="server" Label="Schedule" >
                                <Rock:ScheduleBuilder ID="sbSchedule" runat="server" ValidationGroup="Schedule" AllowMultiSelect="true" Required="true" OnSaveSchedule="sbSchedule_SaveSchedule"/>
                                <asp:Literal ID="lScheduleText" runat="server" />
                            </Rock:RockControlWrapper>
                            <Rock:RockLiteral ID="lRegistration" runat="server" Label="Registration Instance - Group" CssClass="margin-b-none" />
                            <asp:LinkButton ID="lbEditRegistration" runat="server" CssClass="btn btn-default btn-xs margin-b-md" OnClick="lbEditRegistration_Click" ><i class="fa fa-pencil"></i> Edit</asp:LinkButton>
                            <asp:LinkButton ID="lbDeleteRegistration" runat="server" CssClass="btn btn-danger btn-xs margin-b-md" OnClick="lbDeleteRegistration_Click" ><i class="fa fa-times"></i> Remove</asp:LinkButton>
                            <asp:LinkButton ID="lbCreateNewRegistration" runat="server" CssClass="btn btn-primary btn-xs margin-b-md" Text="Add New Registration Instance" OnClick="lbCreateNewRegistration_Click" />
                            <asp:LinkButton ID="lbLinkToExistingRegistration" runat="server" CssClass="btn btn-default btn-xs margin-b-md" Text="Use Existing Registration Instance" OnClick="lbLinkToExistingRegistration_Click" />

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
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>

                </asp:Panel>

            </div>

        </asp:Panel>

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="dlgNewLinkage" runat="server" Title="New Registration Instance" SaveButtonText="OK" OnSaveClick="dlgNewLinkage_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="NewLinkage">
            <Content>
                <asp:ValidationSummary ID="vsNewLinkage" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="NewLinkage" />
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlNewLinkageTemplate" runat="server" Label="Registration Template" ValidationGroup="NewLinkage" Required="true" AutoPostBack="true" OnSelectedIndexChanged="ddlNewLinkageTemplate_SelectedIndexChanged" />
                    </div>
                    <div class="col-md-6">
                        <Rock:GroupPicker ID="gpNewLinkageGroup" runat="server" Label="Group" ValidationGroup="NewLinkage" />
                    </div>
                </div>
                <Rock:RegistrationInstanceEditor ID="rieNewLinkage" runat="server" ValidationGroup="NewLinkage" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgEditLinkage" runat="server" Title="Edit Registration Instance" SaveButtonText="OK" OnSaveClick="dlgEditLinkage_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="EditLinkage">
            <Content>
                <asp:ValidationSummary ID="vsEditLinkage" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="EditLinkage" />
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockLiteral ID="lEditLinkageTemplate" runat="server" Label="Registration Template" />
                    </div>
                    <div class="col-md-6">
                        <Rock:GroupPicker ID="gpEditLinkageGroup" runat="server" Label="Group" ValidationGroup="EditLinkage" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbEditLinkagePublicName" runat="server" Label="Public Name" Required="true" ValidationGroup="EditLinkage" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbEditLinkageUrlSlug" runat="server" Label="URL Slug" ValidationGroup="EditLinkage" Help="When creating an event occurrence that specifies a campus, a URL Slug MUST be used when registering in order for the registrant to be placed into the linked group." />
                    </div>
                </div>
                <Rock:RegistrationInstanceEditor ID="rieEditLinkage" runat="server" ValidationGroup="EditLinkage" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgExistingLinkage" runat="server" Title="Existing Registration Instance" SaveButtonText="OK" OnSaveClick="dlgExistingLinkage_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="ExistingLinkage">
            <Content>
                <asp:ValidationSummary ID="vsExistingLinkage" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="ExistingLinkage" />
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlExistingLinkageTemplate" runat="server" Label="Registration Template" ValidationGroup="ExistingLinkage"
                            AutoPostBack="true" OnSelectedIndexChanged="ddlExistingLinkageTemplate_SelectedIndexChanged" CausesValidation="false"
                            Required="true" EnhanceForLongLists="true" />
                        <Rock:RockDropDownList ID="ddlExistingLinkageInstance" runat="server" Label="Registration Instance" ValidationGroup="ExistingLinkage"
                            Required="true" EnhanceForLongLists="true" />
                    </div>
                    <div class="col-md-6">
                        <Rock:GroupPicker ID="gpExistingLinkageGroup" runat="server" Label="Group" ValidationGroup="ExistingLinkage" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbExistingLinkagePublicName" runat="server" Label="Public Name" Required="true" ValidationGroup="ExistingLinkage" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbExistingLinkageUrlSlug" runat="server" Label="URL Slug" ValidationGroup="ExistingLinkage" />
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
