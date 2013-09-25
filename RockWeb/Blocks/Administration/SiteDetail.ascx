<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SiteDetail.ascx.cs" Inherits="RockWeb.Blocks.Administration.SiteDetail" %>

<asp:UpdatePanel ID="upSites" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server" >

            <asp:HiddenField ID="hfSiteId" runat="server" />

            <asp:ValidationSummary runat="server" CssClass="alert alert-error" />

            <fieldset>
                <legend>
                    <asp:Literal ID="lActionTitle" runat="server" />
                </legend>

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <div class="row-fluid">
                    <div class="span6">
                        <Rock:DataTextBox ID="tbSiteName" runat="server" SourceTypeName="Rock.Model.Site, Rock" PropertyName="Name" />
                        <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.Site, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                        <Rock:DataDropDownList ID="ddlTheme" runat="server" SourceTypeName="Rock.Model.Site, Rock" PropertyName="Theme" />
                        <Rock:PagePicker ID="ppDefaultPage" runat="server" Label="Default Page" Required="true" PromptForPageRoute="true"/>
                        <Rock:DataTextBox ID="tbLoginPageReference" runat="server" SourceTypeName="Rock.Model.Site, Rock" PropertyName="LoginPageReference" />
                    </div>
                    <div class="span6">
                        <Rock:DataTextBox ID="tbSiteDomains" runat="server" SourceTypeName="Rock.Model.Site, Rock" PropertyName="SiteDomains" TextMode="MultiLine" />
                        <Rock:DataTextBox ID="tbFaviconUrl" runat="server" SourceTypeName="Rock.Model.Site, Rock" PropertyName="FaviconUrl" />
                        <Rock:DataTextBox ID="tbAppleTouchIconUrl" runat="server" SourceTypeName="Rock.Model.Site, Rock" PropertyName="AppleTouchIconUrl" />
                        <Rock:DataTextBox ID="tbFacebookAppId" runat="server" SourceTypeName="Rock.Model.Site, Rock" PropertyName="FacebookAppId" />
                        <Rock:DataTextBox ID="tbFacebookAppSecret" runat="server" SourceTypeName="Rock.Model.Site, Rock" PropertyName="FacebookAppSecret" />
                        
                        <Rock:DataTextBox ID="tbRegistrationPageReference" runat="server" SourceTypeName="Rock.Model.Site, Rock" PropertyName="RegistrationPageReference" />
                        <Rock:DataTextBox ID="tbErrorPage" runat="server" SourceTypeName="Rock.Model.Site, Rock" PropertyName="ErrorPage" />
                    </div>
                </div>

            </fieldset>

            <div class="actions">
                <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>

