﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MergeTemplateDetail.ascx.cs" Inherits="RockWeb.Blocks.Core.MergeTemplateDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server">

            <asp:Panel ID="pnlEditDetails" runat="server" CssClass="panel panel-block">
                <asp:HiddenField ID="hfMergeTemplateId" runat="server" />
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-files-o"></i>
                        <asp:Literal ID="lActionTitle" runat="server" /></h1>
                </div>
                <div class="panel-body">

                    <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                    <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

                    <fieldset>

                        <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.MergeTemplate, Rock" PropertyName="Name" />
                                <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.MergeTemplate, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                                <Rock:RockDropDownList ID="ddlMergeTemplateType" runat="server" Label="Type" Required="true" />
                            </div>
                            <div class="col-md-6">
                                <Rock:NotificationBox ID="nbFileTypeWarning" runat="server" NotificationBoxType="Warning" Visible="true" Dismissable="true" />
                                <Rock:FileUploader ID="fuTemplateBinaryFile" runat="server" Label="Template File" Required="true" OnFileUploaded="fuTemplateBinaryFile_FileUploaded" />
                                <Rock:CategoryPicker ID="cpCategory" runat="server" Label="Category" Required="true" />
                                <Rock:PersonPicker ID="ppPerson" runat="server" Label="Person" Help="Set this to make it a personal merge template. Leave it blank to make it a global." />
                            </div>

                        </div>

                    </fieldset>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>

                </div>

            </asp:Panel>

            <div id="pnlViewDetails" class="panel panel-block" runat="server">

                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-files-o"></i>
                        <asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>
                </div>
                <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
                <div class="panel-body">

                    <div class="row">
                        <Rock:NotificationBox ID="NotificationBox1" runat="server" NotificationBoxType="Info" />
                    </div>

                    <fieldset>
                        <div class="row">
                            <div class="col-md-6">
                                <asp:Literal ID="lblMainDetailsCol1" runat="server" />
                            </div>
                            <div class="col-md-6">
                                <asp:Literal ID="lblMainDetailsCol2" runat="server" />
                            </div>
                        </div>
                        <div class="actions">
                            <asp:LinkButton ID="btnEdit" runat="server" AccessKey="e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" />
                            <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClientClick="Rock.dialogs.confirmDelete(event, 'merge template');" OnClick="btnDelete_Click" />
                            <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        </div>
                    </fieldset>

                </div>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
