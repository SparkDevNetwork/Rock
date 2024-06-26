<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TextToGiveSettings.ascx.cs" Inherits="RockWeb.Blocks.Finance.TextToGiveSettings" %>

<asp:UpdatePanel ID="upStreakTypeDetail" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfIsEditMode" runat="server" />

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-mobile-alt"></i>
                    <asp:Literal ID="lTitle" runat="server" />
                </h1>
            </div>
            <div class="panel-body">

                <Rock:NotificationBox ID="nbMessage" runat="server" NotificationBoxType="Warning" />

                <div id="pnlViewDetails" runat="server">
                    <div class="row">
                        <div class="col-sm-6">
                            <asp:Literal ID="lDescriptionLeft" runat="server" />
                        </div>
                        <div class="col-sm-6">
                            <asp:Literal ID="lDescriptionRight" runat="server" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" data-shortcut-key="e" ToolTip="Alt+e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                        <asp:LinkButton ID="btnCancelOnView" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancelOnView_Click" />
                    </div>
                </div>

                <div id="pnlEditDetails" runat="server">
                    <div class="row">
                        <div class="col-md-5 col-sm-12">
                            <Rock:AccountPicker ID="apAccountPicker" runat="server" Label="Default Account" />
                        </div>
                        <div class="col-md-7 col-sm-12">
                            <Rock:RockDropDownList ID="ddlSavedAccount" label="Saved Account" runat="server" DataTextField="Name" DataValueField="Id" />
                            <asp:LinkButton ID="btnAddSavedAccount" runat="server" Text="Add" CssClass="btn btn-link" CausesValidation="false" OnClick="btnAddSavedAccount_Click" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" data-shortcut-key="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancelOnEdit" runat="server" data-shortcut-key="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancelOnEdit_Click" />
                    </div>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
