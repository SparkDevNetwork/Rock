<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CreatePledge.ascx.cs" Inherits="RockWeb.Blocks.Finance.CreatePledge" %>

<asp:UpdatePanel ID="upCreatePledge" runat="server">
    <ContentTemplate>
        <fieldset>
            <legend><asp:Literal ID="lLegendText" runat="server"/></legend>
            <Rock:DataTextBox ID="tbFirstName" runat="server" LabelText="First Name"/>
            <Rock:DataTextBox ID="tbLastName" runat="server" LabelText="Last Name"/>
            <Rock:DataTextBox ID="tbAmount" runat="server" PrependText="$" LabelText="Total Amount"/>
            <Rock:DataTextBox ID="tbEmail" runat="server" LabelText="Email" TextMode="Email"/>
            <Rock:DateTimePicker ID="dtpStartDate" runat="server" LabelText="Start Date"/>
            <Rock:DateTimePicker ID="dtpEndDate" runat="server" LabelText="End Date"/>
            <Rock:DataTextBox ID="tbFrequencyAmount" runat="server" PrependText="$" LabelText="Amount"/>
            <Rock:DataDropDownList ID="ddlFrequencyType" runat="server"/>
        </fieldset>
        <div class="actions">
            <asp:Button ID="btnSave" runat="server" Text="Save Pledge" OnClick="btnSave_Click"/>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>