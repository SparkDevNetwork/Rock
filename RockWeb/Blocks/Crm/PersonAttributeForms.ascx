<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonAttributeForms.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonAttributeForms" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upnlContent" runat="server" ChildrenAsTriggers="false" UpdateMode="Conditional">
    <ContentTemplate>

        <asp:HiddenField ID="hfTriggerScroll" runat="server" Value="" />

        <asp:ValidationSummary ID="vsSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
        <Rock:NotificationBox ID="nbMain" runat="server" Visible="false"></Rock:NotificationBox>

        <%-- View Panel --%>
        <asp:Panel ID="pnlView" runat="server" Visible="false">

            <h1><asp:Literal ID="lTitle" runat="server" /></h1>
        
            <asp:Panel ID="pnlProgressBar" runat="server">
                <div class="progress">
                    <div class="progress-bar" role="progressbar" aria-valuenow="<%=this.PercentComplete%>" aria-valuemin="0" aria-valuemax="100" style="width: <%=this.PercentComplete%>%;">
                        <span class="sr-only"><%=this.PercentComplete%>% Complete</span>
                    </div>
                </div>
            </asp:Panel>

            <asp:Literal ID="lHeader" runat="server" />

            <asp:PlaceHolder ID="phContent" runat="server" />

            <asp:Literal ID="lFooter" runat="server" />

            <div class="actions">
                <asp:LinkButton ID="lbPrev" runat="server" AccessKey="p" ToolTip="Alt+p" Text="Previous" CssClass="btn btn-default" CausesValidation="false" OnClick="lbPrev_Click" />
                <Rock:BootstrapButton ID="lbNext" runat="server" AccessKey="n" ToolTip="Alt+n" Text="Next" DataLoadingText="Next" CssClass="btn btn-primary pull-right" CausesValidation="true" OnClick="lbNext_Click" />
            </div>

        </asp:Panel>

        <%-- Edit Panel --%>

        <asp:Panel ID="pnlEditModal" runat="server" Visible="false" CssClass="panel panel-block">
            <Rock:ModalDialog ID="mdEdit" runat="server" OnSaveClick="btnSave_Click" Title="Person Attribute Forms Configuration" OnCancelScript="">
                <Content>

                    <asp:UpdatePanel ID="upnlEdit" runat="server">
                        <ContentTemplate>
                            <div class="panel-heading"></div>
                            <div class="panel-body">

                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:RockCheckBox ID="cbDisplayProgressBar" runat="server" Label="Display Progress Bar" Text="Yes"
                                            Help="Display a progress to the user that indicates how far along they are in filling out multiple forms. Not shown when there is only one form." />
                                        <Rock:RockDropDownList ID="ddlSaveValues" runat="server" Label="Save Values"
                                            Help="Determines if values should be saved each time user navigates to next form or not saved until the very end. An advantage to saving them on each form, is that those values can then be used in the header or footer of the next page using Lava.">
                                            <asp:ListItem Value="PAGE" Text="After Each Page" />
                                            <asp:ListItem Value="END" Text="At the End" />
                                        </Rock:RockDropDownList>
                                    </div>
                                    <div class="col-md-6">
                                        <Rock:WorkflowTypePicker ID="wtpWorkflow" runat="server" Label="Workflow"
                                            Help="An optional workflow to launch after the person has filled out all of the forms." />
                                        <Rock:PagePicker ID="ppDonePage" runat="server" Label="Done Page"
                                            Help="An optional page to redirect user to after they have finished entering information on all the forms." />
                                    </div>
                                </div>

                                <div class="panel panel-default contribution-info">
                                    <div class="panel-heading">
                                        <h3 class="panel-title">Forms</h3>
                                    </div>
                                    <div class="panel-body">

                                        <asp:PlaceHolder ID="phForms" runat="server" />

                                        <div class="pull-right">
                                            <asp:LinkButton ID="lbAddForm" runat="server" CssClass="btn btn-action btn-xs" OnClick="lbAddForm_Click" CausesValidation="false"><i class="fa fa-plus"></i> Add Form</asp:LinkButton>
                                        </div>

                                    </div>
                                </div>
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </Content>
            </Rock:ModalDialog>
        </asp:Panel>

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="dlgField" runat="server" Title="Form Field" OnSaveClick="dlgField_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="Field">
            <Content>
                <asp:HiddenField ID="hfFormGuid" runat="server" />
                <asp:HiddenField ID="hfAttributeGuid" runat="server" />
                <asp:ValidationSummary ID="ValidationSummaryAttribute" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="Field" />
                <div class="row">
                    <div class="col-md-4">
                        <Rock:RockDropDownList ID="ddlPersonAttributes" runat="server" Label="Person Attribute" ValidationGroup="Field" />
                    </div>
                    <div class="col-md-4">
                        <Rock:RockCheckBox ID="cbUsePersonCurrentValue" runat="server" Label="Use Current Value" Text="Yes" ValidationGroup="Field"
                            Help="Should the person's current value for this field be displayed (pre-filled)?" />
                    </div>
                    <div class="col-md-4">
                        <Rock:RockCheckBox ID="cbRequireInInitialEntry" runat="server" Label="Required" Text="Yes" ValidationGroup="Field"
                            Help="Should a value for this attribute be required?" />
                    </div>
                </div>
                <Rock:CodeEditor ID="ceAttributePreText" runat="server" Label="Pre-Text" EditorMode="Html" EditorTheme="Rock" EditorHeight="100" ValidationGroup="Field"
                    Help="Any HTML to display directly above this field <span class='tip tip-lava'></span>." />
                <Rock:CodeEditor ID="ceAttributePostText" runat="server" Label="Post-Text" EditorMode="Html" EditorTheme="Rock" EditorHeight="100" ValidationGroup="Field"
                    Help="Any HTML to display directly below this field <span class='tip tip-lava'></span>." />
            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>
