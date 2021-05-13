<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WorkflowEntry.ascx.cs" Inherits="RockWeb.Blocks.WorkFlow.WorkflowEntry" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="row">
            <div id="divForm" runat="server" class="col-md-6">

                <div class="panel panel-block">

                    <div class="panel-heading">
                        <h1 class="panel-title">
                            <asp:Literal ID="lIconHtml" runat="server"><i class="fa fa-gear"></i></asp:Literal>
                            <asp:Literal ID="lTitle" runat="server">Workflow Entry</asp:Literal>
                        </h1>
                        <div class="panel-labels">
                            <Rock:HighlightLabel ID="hlblWorkflowId" runat="server" LabelType="Info" />
                            <Rock:HighlightLabel ID="hlblDateAdded" runat="server" LabelType="Default" />
                        </div>
                    </div>
                    <div class="panel-body">

                        <asp:Literal ID="lSummary" runat="server" Visible="false" />

                        <asp:Panel ID="pnlForm" CssClass="workflow-entry-panel" runat="server">

                            <asp:ValidationSummary ID="vsDetails" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                            <asp:Literal ID="lFormHeaderText" runat="server" />

                            <%-- Person Entry --%>
                            <asp:Panel ID="pnlPersonEntry" runat="server">
                                <asp:Literal ID="lPersonEntryPreHtml" runat="server" />

                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:CampusPicker ID="cpPersonEntryCampus" runat="server" Required="true" />
                                    </div>
                                    <div class="col-md-6">
                                    </div>
                                </div>
                                
                                <%-- Special input with rock-fullname class --%>
                                <Rock:RockTextBox ID="tbRockFullName" runat="server" CssClass="rock-fullname" ValidationGroup="vgRockFullName" Placeholder="Please enter name (Required)" />
                                <Rock:NotificationBox ID="nbRockFullName" runat="server" NotificationBoxType="Validation" />

                                <asp:Panel ID="pnlPersonEntryRow1" runat="server" CssClass="row">
                                    <%-- Person 1 --%>
                                    <asp:Panel ID="pnlPersonEntryRow1Column1" runat="server" CssClass="col-md-6">
                                        <Rock:PersonBasicEditor ID="pePerson1" runat="server" />
                                    </asp:Panel>

                                    <%-- Person 2 (Spouse) --%>
                                    <asp:Panel ID="pnlPersonEntryRow1Column2" runat="server" CssClass="col-md-6">
                                        <Rock:PersonBasicEditor ID="pePerson2" runat="server" />
                                    </asp:Panel>
                                </asp:Panel>

                                <Rock:RockCheckBox ID="cbShowPerson2" runat="server" Text="Show Person2" Checked="false" AutoPostBack="true" OnCheckedChanged="cbShowPerson2_CheckedChanged" />

                                <%-- Person Entry Address and Marital Status --%>
                                <asp:Panel ID="pnlPersonEntryRow2" CssClass="row" runat="server">
                                    <asp:Panel ID="pnlPersonEntryRow2Column1" runat="server" CssClass="col-md-6">
                                        <Rock:AddressControl ID="acPersonEntryAddress" runat="server" Label="Address" />
                                    </asp:Panel>
                                    <asp:Panel ID="pnlPersonEntryRow2Column2" runat="server" CssClass="col-md-6">
                                        <Rock:DefinedValuePicker runat="server" ID="dvpMaritalStatus" Label="Marital Status" />
                                    </asp:Panel>
                                </asp:Panel>

                                <asp:Literal ID="lPersonEntryPostHtml" runat="server" />
                            </asp:Panel>

                            <%-- Workflow Attribute Controls  --%>
                            <asp:PlaceHolder ID="phAttributes" runat="server" />

                            <asp:Literal ID="lFormFooterText" runat="server" />

                            <div class="actions">
                                <asp:PlaceHolder ID="phActions" runat="server" />
                            </div>

                        </asp:Panel>

                        <Rock:NotificationBox ID="nbMessage" runat="server" Dismissable="true" CssClass="margin-t-lg" />
                    </div>

                </div>

            </div>

            <div id="divNotes" runat="server" class="col-md-6">
                <Rock:NoteContainer ID="ncWorkflowNotes" runat="server" NoteLabel="Note"
                    ShowHeading="true" Title="Notes" TitleIconCssClass="fa fa-comment"
                    DisplayType="Full" UsePersonIcon="false" ShowAlertCheckBox="true"
                    ShowPrivateCheckBox="false" ShowSecurityButton="false"
                    AllowAnonymousEntry="false" AddAlwaysVisible="false"
                    SortDirection="Descending" />
            </div>

        </div>

        <script>
            function handleWorkflowActionButtonClick(validationGroup, causesValidation) {
                if (causesValidation) {
                    // make sure page is valid before doing the postback (from this button's href)
                    if (!Page_ClientValidate(validationGroup)) {
                        return false;
                    }
                }

                //$(this).button('loading');
                return true;
            }
        </script>

    </ContentTemplate>
</asp:UpdatePanel>
