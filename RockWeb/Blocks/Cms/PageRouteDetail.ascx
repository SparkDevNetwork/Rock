<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PageRouteDetail.ascx.cs" Inherits="RockWeb.Blocks.Cms.PageRouteDetail" %>

<asp:UpdatePanel ID="upPageRoutes" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server" Visible="false">

            <asp:HiddenField ID="hfPageRouteId" runat="server" />

            <div class="banner">
                <h1><asp:Literal ID="lActionTitle" runat="server" /></h1>
            </div>

            <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-error" />

            <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

            <fieldset>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:PagePicker ID="ppPage" runat="server" Label="Page" Required="true" PromptForPageRoute="false"/>
                    </div>
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbRoute" runat="server" SourceTypeName="Rock.Model.PageRoute, Rock" PropertyName="Route" />
                    </div>
                </div>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>     
                         
            </fieldset>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>


