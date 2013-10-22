<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CampusDetail.ascx.cs" Inherits="RockWeb.Blocks.Administration.CampusDetail" %>

<asp:UpdatePanel ID="upCampusDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">
            
            <asp:HiddenField ID="hfCampusId" runat="server" />

            <div class="banner">
                <h1><asp:Literal ID="lActionTitle" runat="server" /></h1>
            </div>

            <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="alert alert-danger" />
            <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

            <fieldset>

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <div class="row">
                    <div class="col-md-6"><Rock:DataTextBox ID="tbCampusName" runat="server" SourceTypeName="Rock.Model.Campus, Rock" PropertyName="Name" /></div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbCampusCode" runat="server" SourceTypeName="Rock.Model.Campus, Rock" PropertyName="ShortCode" />
                    </div>
                </div>

            </fieldset>

            <div class="actions">
                <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-default" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
