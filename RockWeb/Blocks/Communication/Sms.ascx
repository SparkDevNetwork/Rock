<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Sms.ascx.cs" Inherits="RockWeb.Blocks.Communication.Email" %>

<fieldset>
    <Rock:LabeledTextBox ID="tbFromPhone" runat="server" LabelText="From Phone Number" />
    <Rock:LabeledTextBox ID="tbMessage" runat="server" LabelText="Message" TextMode="MultiLine" Rows="4" />
</fieldset>