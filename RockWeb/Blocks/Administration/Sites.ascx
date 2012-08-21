<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Sites.ascx.cs" Inherits="RockWeb.Blocks.Administration.Sites" %>

<asp:UpdatePanel ID="upSites" runat="server">
<ContentTemplate>

    <asp:Panel ID="pnlList" runat="server">
        
        <Rock:Grid ID="gSites" runat="server" EmptyDataText="No Sites Found">
            <Columns>
                <asp:BoundField DataField="Name" HeaderText="Name" />
                <asp:BoundField HeaderText="Description" DataField="Description" />
                <asp:BoundField HeaderText="Theme" DataField="Theme" />
                <Rock:SecurityField/>
                <Rock:EditField OnClick="gSites_Edit" />
                <Rock:DeleteField OnClick="gSites_Delete" />
            </Columns>
        </Rock:Grid>

    </asp:Panel>

    <asp:Panel ID="pnlDetails" runat="server" Visible="false">
    
        <asp:HiddenField ID="hfSiteId" runat="server" />

        <asp:ValidationSummary runat="server" CssClass="failureNotification"/>

        <div class="row-fluid">

            <div class="span6">

                <fieldset>
                    <legend><asp:Literal ID="lAction" runat="server"></asp:Literal> Site</legend>
                    <Rock:DataTextBox ID="tbSiteName" runat="server" SourceTypeName="Rock.CMS.Site, Rock" PropertyName="Name" Tip="Hi There!" />
                    <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.CMS.Site, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                    <Rock:DataDropDownList ID="ddlTheme" runat="server" SourceTypeName="Rock.CMS.Site, Rock" PropertyName="Theme" />
                    <Rock:DataDropDownList ID="ddlDefaultPage" runat="server" SourceTypeName="Rock.CMS.Site, Rock" PropertyName="DefaultPageId" LabelText="Default Page" />
                </fieldset>

            </div>

            <div class="span6">

                <fieldset>
                    <legend>&nbsp;</legend>
                    <Rock:DataTextBox ID="tbSiteDomains" runat="server" SourceTypeName="Rock.CMS.Site, Rock" PropertyName="SiteDomains" TextMode="MultiLine" />
                    <Rock:DataTextBox ID="tbFaviconUrl" runat="server" SourceTypeName="Rock.CMS.Site, Rock" PropertyName="FaviconUrl" />
                    <Rock:DataTextBox ID="tbAppleTouchIconUrl" runat="server" SourceTypeName="Rock.CMS.Site, Rock" PropertyName="AppleTouchIconUrl" />
                    <Rock:DataTextBox ID="tbFacebookAppId" runat="server" SourceTypeName="Rock.CMS.Site, Rock" PropertyName="FacebookAppId" />
                    <Rock:DataTextBox ID="tbFacebookAppSecret" runat="server" SourceTypeName="Rock.CMS.Site, Rock" PropertyName="FacebookAppSecret" />
                </fieldset>

            </div>

        </div>

        <div class="actions">
            <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn primary" onclick="btnSave_Click" />
            <asp:LinkButton id="btnCancel" runat="server" Text="Cancel" CssClass="btn secondary" CausesValidation="false" OnClick="btnCancel_Click" />
        </div>

    </asp:Panel>

    <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Error" Visible="false" />

</ContentTemplate>
</asp:UpdatePanel>

