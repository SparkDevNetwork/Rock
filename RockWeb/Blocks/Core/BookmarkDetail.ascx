<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BookmarkDetail.ascx.cs" Inherits="RockWeb.Blocks.Core.BookmarkDetail" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">
            <asp:HiddenField ID="hfPersonBookmarkId" runat="server" />
            <asp:HiddenField ID="hfCategoryId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-bookmark-o"></i> 
                    <asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>
            </div>

            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>

            <div class="panel-body">
                <Rock:NotificationBox ID="NotificationBox1" runat="server" NotificationBoxType="Info" />
                <div id="pnlEditDetails" runat="server">

                    <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.PersonBookmark, Rock" PropertyName="Name" />
                    <Rock:DataTextBox ID="tbUrl" runat="server" SourceTypeName="Rock.Model.PersonBookmark, Rock" PropertyName="Url" />
                    <Rock:PersonPicker ID="ppPerson" runat="server" Label="Person" />

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>

                </div>

                <fieldset id="fieldsetViewDetails" runat="server">
                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" EnableViewState="false" />

                    <Rock:RockControlWrapper ID="rcWrapper" runat="server" Label="Url">
                        <asp:HyperLink ID="hlUrl" runat="server" Target="_blank" />
                    </Rock:RockControlWrapper>

                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" AccessKey="m" ToolTip="Alt+m" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" CausesValidation="false" />
                        <span class="pull-right">
                            <Rock:SecurityButton ID="btnSecurity" runat="server" class="btn btn-sm btn-security" />
                        </span>
                    </div>
                </fieldset>

            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
