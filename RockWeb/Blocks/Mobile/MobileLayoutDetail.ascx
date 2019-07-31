<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MobileLayoutDetail.ascx.cs" Inherits="RockWeb.Blocks.Mobile.MobileLayoutDetail" %>

<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbError" runat="server" NotificationBoxType="Danger" />

        <asp:Panel ID="pnlDetails" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h3 class="panel-title">Layout</h3>
            </div>

            <div class="panel-body">
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbName" runat="server" Label="Name" Required="true" />
                    </div>
                </div>

                <Rock:RockTextBox ID="tbDescription" runat="server" Label="Description" TextMode="MultiLine" />

                <Rock:CodeEditor ID="cePhoneLayout" runat="server" Label="Phone Layout XAML" EditorMode="Xml" />

                <Rock:CodeEditor ID="ceTabletLayout" runat="server" Label="Tablet Layout XAML" EditorMode="Xml" />

                <div class="actions margin-t-md">
                    <asp:LinkButton ID="lbSave" runat="server" CssClass="btn btn-primary" Text="Save" OnClick="lbSave_Click" />
                    <asp:LinkButton ID="lbCancel" runat="server" CssClass="btn btn-link" Text="Cancel" OnClick="lbCancel_Click" CausesValidation="false" />
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>