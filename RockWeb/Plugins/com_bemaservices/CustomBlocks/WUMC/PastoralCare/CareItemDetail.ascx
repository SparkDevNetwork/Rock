<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CareItemDetail.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.PastoralCare.CareItemDetail" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <Triggers>
        <asp:AsyncPostBackTrigger ControlID="cblCareTypes" EventName="SelectedIndexChanged" />
    </Triggers>
    <ContentTemplate>

        <asp:HiddenField ID="hfCareTypeId" runat="server" />
        <asp:HiddenField ID="hfCareItemId" runat="server" />
        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:NotificationBox ID="nbNoParameterMessage" runat="server" NotificationBoxType="Warning" Heading="Missing Parameter(s)"
            Text="This block requires a valid care item id and/or a care type id as query string parameters." />

        <asp:Panel ID="pnlContents" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lTitle" runat="server" />
                </h1>
            </div>
            <asp:Panel ID="pnlReadDetails" runat="server">

                <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>

                <div class="panel-body">
                    <div class="row">
                        <div class="col-md-2">
                            <div class="photo">
                                <asp:Literal ID="lPortrait" runat="server" />
                            </div>
                        </div>
                        <div class="col-md-8">
                            <asp:Panel runat="server" CssClass="margin-b-sm" ID="pnlBadges">
                                <Rock:PersonProfileBadgeList ID="blStatus" runat="server" />
                            </asp:Panel>

                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockLiteral ID="lContactInfo" runat="server" Label="Contact Info" />
                                    <Rock:RockLiteral ID="lContactor" runat="server" Label="Requester" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:RockLiteral ID="lCareType" runat="server" Label="Care Types" />
                                    <Rock:RockLiteral ID="lContactDate" runat="server" Label="Request Date" />
                                </div>
                            </div>

                        </div>

                        <div class="col-md-2 text-right">
                            <asp:LinkButton ID="lbProfilePage" runat="server" CssClass="btn btn-default btn-xs"><i class="fa fa-user"></i> Profile</asp:LinkButton>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:RockLiteral ID="lDescription" runat="server" />
                            <Rock:DynamicPlaceholder ID="phAttributesReadOnly" runat="server" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="lbEdit" runat="server" Text="Edit" CssClass="btn btn-primary" OnClick="lbEdit_Click"></asp:LinkButton>
                    </div>

                </div>

            </asp:Panel>

            <asp:Panel ID="pnlEditDetails" runat="server" Visible="false">

                <div class="panel-body">
                    <Rock:NotificationBox ID="nbValidation" runat="server" NotificationBoxType="Danger" Visible="false" />
                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                    <Rock:NotificationBox ID="nbErrorMessage" runat="server" NotificationBoxType="Danger" />
                    <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

                    <div class="row">
                        <div class="col-md-3">
                            <Rock:PersonPicker runat="server" ID="ppPerson" Label="Person" Required="true" />
                        </div>
                        <div class="col-md-3">
                            <Rock:PersonPicker ID="ppContactorEdit" runat="server" Label="Requester" Required="True" />
                        </div>
                        <div class="col-md-3">
                            <Rock:DateTimePicker ID="dtpContactDate" runat="server" Label="Request Date" Required="true" />
                        </div>
                        <div class="col-md-3">
                            <Rock:RockCheckBoxList ID="cblCareTypes" runat="server" Label="Care Types"
                                Help="Care Types that this item is tied to (at least one is required)."
                                OnSelectedIndexChanged="cblCareTypes_SelectedIndexChanged" AutoPostBack="true"
                                RepeatDirection="Horizontal" Required="true" />
                        </div>
                    </div>

                    <Rock:RockTextBox ID="tbDescription" Label="Description" runat="server" TextMode="MultiLine" Rows="4" ValidateRequestMode="Disabled" />

                    <Rock:DynamicPlaceholder ID="phAttributes" runat="server" />

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click"></asp:LinkButton>
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" OnClick="btnCancel_Click" CausesValidation="false"></asp:LinkButton>
                    </div>

                </div>

            </asp:Panel>

        </asp:Panel>
        <Rock:PanelWidget ID="wpCareContacts" runat="server" Title="Contacts" Expanded="true" CssClass="clickable">
            <div class="grid">
                <Rock:Grid ID="gCareContacts" runat="server" AllowPaging="false" DisplayType="Light"
                    RowItemText="Contact" OnRowDataBound="gCareContacts_RowDataBound" OnRowSelected="gCareContacts_Edit">
                    <Columns>
                        <Rock:RockBoundField DataField="Date" HeaderText="Date" />
                        <Rock:RockBoundField DataField="Contactor" HeaderText="Contactor" />
                        <Rock:RockBoundField DataField="Description" HeaderText="Description" />
                        <Rock:DeleteField OnClick="gCareContacts_Delete" />
                    </Columns>
                </Rock:Grid>
            </div>
        </Rock:PanelWidget>

        <Rock:ModalDialog ID="dlgCareContacts" runat="server" SaveButtonText="Add" OnSaveClick="btnAddCareContact_Click" Title="Add Contact" ValidationGroup="Contact">
            <Content>
                <asp:ValidationSummary ID="valConnectorGroup" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="Contact" />
                <asp:HiddenField ID="hfAddCareContactGuid" runat="server" />
                <div class="row">
                    <div class="col-md-6">
                        <Rock:PersonPicker ID="ppContactor" runat="server" Label="Contactor" Required="true" EnableSelfSelection="true" ValidationGroup="Contact" />
                    </div>
                    <div class="col-md-6">
                        <Rock:DateTimePicker ID="dtpVisitDate" runat="server" Label="Visit Date" Required="true" EnableSelfSelection="true" ValidationGroup="Contact" />
                    </div>
                </div>
                <Rock:RockTextBox ID="tbNote" runat="server" Label="Description" TextMode="MultiLine" Rows="4" ValidationGroup="Contact" />
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
