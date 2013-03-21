<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CreatePledge.ascx.cs" Inherits="RockWeb.Blocks.Finance.CreatePledge" %>

<asp:UpdatePanel ID="upCreatePledge" runat="server">
    <ContentTemplate>
        <fieldset>
            <legend><asp:Literal ID="lLegendText" runat="server"/></legend>
            <Rock:DataDropDownList ID="ddlFrequencyType" runat="server"/>
        </fieldset>
        <div class="actions">
            <asp:Button ID="btnSave" runat="server" Text="Save Pledge" OnClick="btnSave_Click"/>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>