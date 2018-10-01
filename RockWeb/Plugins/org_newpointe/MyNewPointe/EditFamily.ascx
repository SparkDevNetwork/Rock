<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EditFamily.ascx.cs" Inherits="RockWeb.Plugins.org_newpointe.MyNewpointe.EditFamily" %>

<asp:UpdatePanel ID="upAddGroup" runat="server">
    <ContentTemplate>

        <asp:Literal ID="lMessageContent" runat="server"></asp:Literal>

        <asp:Panel ID="editPanel" runat="server" Visible="false">

            <asp:ValidationSummary ID="vsValidationErrors" runat="server" />

            <Rock:DataTextBox ID="dtbFamilyName" runat="server" Label="Family Name" SourceTypeName="Rock.Model.Group, Rock" PropertyName="Name" Required="true"></Rock:DataTextBox>
            <Rock:CampusPicker ID="cpFamilyCampus" runat="server" Label="Campus" Required="true"></Rock:CampusPicker>
            <Rock:AddressControl ID="acMailingAddress" runat="server" Label="Mailing Address" />
            <Rock:RockCheckBox ID="cbHomeIsMailing" runat="server" Label="Home Address" Text="Same As Mailing" AutoPostBack="true" OnCheckedChanged="cbHomeIsMailing_CheckedChanged" />
            <Rock:AddressControl ID="acHomeAddress" runat="server" />

            <div class="actions clearfix">
                <asp:LinkButton runat="server" ID="lbCancel" CssClass="btn btn-lg btn-default pull-left" Text="Cancel" OnClientClick="history.back();" CausesValidation="false"></asp:LinkButton>
                <asp:LinkButton runat="server" ID="lbSubmit" CssClass="btn btn-lg btn-success pull-right" Text="Save" OnClick="saveFamily"></asp:LinkButton>
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
