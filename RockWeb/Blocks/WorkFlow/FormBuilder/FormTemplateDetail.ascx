<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FormTemplateDetail.ascx.cs" Inherits="RockWeb.Blocks.WorkFlow.FormBuilder.FormTemplateDetail" %>

<asp:UpdatePanel ID="upFormTemplateDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">

            <asp:HiddenField ID="hfFormTemplateId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-building-o"></i><asp:Literal ID="lActionTitle" runat="server" /></h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlStatus" runat="server" />
                </div>
            </div>

            <div class="panel-body">

                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <div id="pnlEditDetails" runat="server">

                    <fieldset>

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:DataTextBox ID="tbFormBuilderTemplateName" runat="server" SourceTypeName="Rock.Model.WorkflowFormBuilderTemplate, Rock" PropertyName="Name" ValidateRequestMode="Disabled" />
                            </div>

                            <div class="col-md-6">
                                <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Active" />
                            </div>
                        </div>

                        <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.WorkflowFormBuilderTemplate, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />

                        <Rock:RockCheckBox ID="cbIsLoginRequired" runat="server" Label="Is Login Required" Help="Determines if a person needs to be logged in to complete the form." />

                        <h4 class="step-title text-break">Form Headers and Footers</h4>
                        <div class="row">
                            <div class="col-sm-8">
                                The headers and footers below will be displayed on all pages of the forms that use this template. 
                            </div>
                        </div>
                        
                        <hr />
                        
                        <Rock:HtmlEditor ID="htmlFormHeader" runat="server" Label="FormHeader" Toolbar="Light" />
                        <Rock:HtmlEditor ID="htmlFormFooter" runat="server" Label="FormFooter" Toolbar="Light" />

                    <Rock:Switch ID="swEnablePersonEntry" runat="server" Text="Enable Person Entry" />

                    </fieldset>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>

                </div>

                <fieldset id="fieldsetViewDetails" runat="server" >
                    <div class="row">
                        <asp:Literal ID="lMainDetails" runat="server" ></asp:Literal>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" AccessKey="e" ToolTip="Alt+e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" />
                    </div>
                </fieldset>

            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>