<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContentItemDetail.ascx.cs" Inherits="RockWeb.Blocks.Cms.ContentItemDetail" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>
        <!-- Item Details Controls -->
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server" Visible="false">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-certificate"></i> <asp:Literal ID="lActionTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">

                <asp:ValidationSummary ID="ValidationSummary2" runat="server" CssClass="alert alert-danger" />
                <asp:HiddenField ID="hfContentChannelId" runat="server" />
                <asp:HiddenField ID="hfContentItemId" runat="server" />

                <!-- Approval -->
                <asp:UpdatePanel ID="upnlApproval" runat="server">
                    <ContentTemplate>

                        <div class="alert alert-action">

                            <asp:Label ID="lblApprovalStatus" runat="server" />

                            <asp:Label ID="lblApprovalStatusPerson" runat="server" />

                            <div class="pull-right">
                                <asp:LinkButton ID="lbApprove" runat="server" OnClick="lbApprove_Click" CssClass="btn btn-primary btn-xs" Text="Approve" />
                                <asp:LinkButton ID="lbDeny" runat="server" OnClick="lbDeny_Click" CssClass="btn btn-xs btn-link" Text="Deny" />
                            </div>

                            <asp:HiddenField ID="hfApprovalStatusPersonId" runat="server" />
                            <asp:HiddenField ID="hfApprovalStatus" runat="server" />
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>

                <fieldset>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbTitle" runat="server" SourceTypeName="Rock.Model.ContentItem, Rock" PropertyName="Title" Label="Title" />
                        </div>
                        <div class="col-md-6">
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:CodeEditor ID="ceContent" runat="server" Label="Content" EditorMode="Html" EditorTheme="Rock" EditorHeight="400" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockDropDownList ID="ddlContentType" runat="server" DataTextField="Name" DataValueField="Id" Label="Content Type" 
                                AutoPostBack="true" OnSelectedIndexChanged="ddlContentType_SelectedIndexChanged" />
                            <Rock:DateTimePicker ID="dpStartDateTime" runat="server" Label="Start" Required="true" />
                            <Rock:DateTimePicker ID="dpExpireDateTime" runat="server" Label="Expire"  />
                            <Rock:DataTextBox ID="tbPriority" runat="server" SourceTypeName="Rock.Model.ContentItem, Rock" PropertyName="Priority" Label="Priority" />
                            <Rock:DataTextBox ID="tbPermalink" runat="server" SourceTypeName="Rock.Model.ContentItem, Rock" PropertyName="Permalink" Label="Permalink" />
                        </div>

                        <div class="col-md-6 attributes">
                            <asp:PlaceHolder ID="phAttributes" runat="server" EnableViewState="false"></asp:PlaceHolder>
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click"></asp:LinkButton>
                        <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" OnClick="btnCancel_Click" CausesValidation="false"></asp:LinkButton>
                    </div>
                </fieldset>

            </div>

            
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
