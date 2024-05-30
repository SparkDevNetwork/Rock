<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonalLinkSectionDetail.ascx.cs" Inherits="RockWeb.Blocks.Cms.PersonalLinkSectionDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading ">
                <h1 class="panel-title">
                    <i class="fa fa-bookmark"></i> <asp:Literal ID="lActionTitle" runat="server" />
                </h1>
                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlShared" runat="server" Text="Shared" LabelType="Info" Visible="false" />
                </div>
            </div>
            <div class="panel-body">
                <asp:HiddenField ID="hfPersonalLinkSectionId" runat="server" />
                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                <div id="pnlViewDetails" runat="server">
                    <div class="row">
                        <div class="col-sm-6 col-md-7 col-lg-8">
                            <div class="margin-b-lg">
                                <asp:Literal ID="lDescription" runat="server" />
                            </div>
                        </div>
                    </div>
                    <div class="actions margin-t-lg">
                        <asp:LinkButton ID="btnEdit" runat="server" data-shortcut-key="e" ToolTip="Alt+e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                        <Rock:SecurityButton ID="btnSecurity" runat="server" class="btn btn-sm btn-square btn-security pull-right" />
                    </div>
                </div>
                <div id="pnlEditDetails" runat="server">
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.PersonalLinkSection, Rock" PropertyName="Name" />
                        </div>
                    </div>
                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" data-shortcut-key="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" data-shortcut-key="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>
                </div>
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
