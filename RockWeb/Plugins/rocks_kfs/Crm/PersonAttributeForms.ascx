<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonAttributeForms.ascx.cs" Inherits="RockWeb.Plugins.rocks_kfs.Crm.PersonAttributeForms" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
        var postbackArg = 'cancel-dlg-field:0;0'
        window.location = "javascript:__doPostBack('<%=upnlContent.ClientID %>', '" + postbackArg + "')";
    }
</script>

<script type="text/javascript">
    Sys.Application.add_load(function () {
        var fixHelper = function (e, ui) {
            ui.children().each(function () {
                $(this).width($(this).width());
            });
            return ui;
        };

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
                    var postbackArg = 're-order-form:' + ui.item.attr('data-key') + ';' + ui.item.index();
                    window.location = "javascript:__doPostBack('<%=upnlContent.ClientID %>', '" + postbackArg + "')";
                }
            }
        });
    });
</script>

<asp:UpdatePanel ID="upnlContent" runat="server" ChildrenAsTriggers="false" UpdateMode="Conditional">
    <ContentTemplate>

        <asp:HiddenField ID="hfTriggerScroll" runat="server" Value="" />

        <asp:ValidationSummary ID="vsSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
        <Rock:NotificationBox ID="nbMain" runat="server" Visible="false"></Rock:NotificationBox>
        <asp:Literal ID="litConfirmationText" runat="server" Visible="false"></asp:Literal>

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
                                    <div class="col-md-4">
                                        <Rock:RockCheckBox ID="cbDisplayProgressBar" runat="server" Label="Display Progress Bar" Text="Yes"
                                            Help="Display a progress to the user that indicates how far along they are in filling out multiple forms. Not shown when there is only one form." />
                                        <Rock:RockDropDownList ID="ddlSaveValues" runat="server" Label="Save Values"
                                            Help="Determines if values should be saved each time user navigates to next form or not saved until the very end. An advantage to saving them on each form, is that those values can then be used in the header or footer of the next page using Lava.">
                                            <asp:ListItem Value="PAGE" Text="After Each Page" />
                                            <asp:ListItem Value="END" Text="At the End" />
                                        </Rock:RockDropDownList>
                                    </div>
                                    <div class="col-md-4">
                                        <Rock:WorkflowTypePicker ID="wtpWorkflow" runat="server" Label="Workflow"
                                            Help="An optional workflow to launch after the person has filled out all of the forms." />
                                         <Rock:RockDropDownList ID="ddlWorkflowEntity" runat="server" Label="Workflow Entity"
                                            Help="The entity that should be used to initiate the workflow.">
                                            <asp:ListItem Value="Person" Text="Person" />
                                            <asp:ListItem Value="ConnectionRequest" Text="Connection Request" />
                                            <asp:ListItem Value="GroupMember" Text="Group Member" />
                                        </Rock:RockDropDownList>
                                    </div>
                                    <div class="col-md-4">
                                        <Rock:PagePicker ID="ppDonePage" runat="server" Label="Done Page"
                                            Help="An optional page to redirect user to after they have finished entering information on all the forms." />
                                    </div>
                                    <div class="col-md-12">
                                        <Rock:CodeEditor ID="ceConfirmationText" runat="server" Label="Confirmation Text" EditorMode="Html" EditorTheme="Rock" Height="300"
                                            Help="The message to display after form is completed if Done Page is not set." />
                                    </div>
                                </div>

                                <div class="panel panel-default contribution-info">
                                    <div class="panel-heading">
                                        <h3 class="panel-title">Forms</h3>
                                    </div>
                                    <div class="panel-body">
                                        <div class="form-list ui-sortable">
                                            <asp:PlaceHolder ID="phForms" runat="server" />
                                        </div>
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
                <asp:ValidationSummary ID="ValidationSummaryAttribute" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="Field" />
                <div class="row">
                    <div class="col-md-4">
                        <Rock:RockLiteral ID="lFieldSource" runat="server" Label="Source" Visible="false" />
                        <Rock:RockDropDownList ID="ddlFieldSource" runat="server" Label="Source" AutoPostBack="true" OnSelectedIndexChanged="ddlFieldSource_SelectedIndexChanged" ValidationGroup="Field" />
                        <Rock:RockDropDownList ID="ddlPersonField" runat="server" Label="Person Field" Visible="false" ValidationGroup="Field" />
                        <Rock:RockDropDownList ID="ddlPersonAttributes" runat="server" Label="Person Attribute" Visible="false" ValidationGroup="Field" />
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
                <Rock:CodeEditor ID="ceAttributePreText" runat="server" Label="Pre-Text" EditorMode="Lava" EditorTheme="Rock" EditorHeight="100" ValidationGroup="Field"
                    Help="Any HTML to display directly above this field <span class='tip tip-lava'></span>." />
                <Rock:CodeEditor ID="ceAttributePostText" runat="server" Label="Post-Text" EditorMode="Lava" EditorTheme="Rock" EditorHeight="100" ValidationGroup="Field"
                    Help="Any HTML to display directly below this field <span class='tip tip-lava'></span>." />
            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>
