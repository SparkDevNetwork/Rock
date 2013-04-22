<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Email.ascx.cs" Inherits="RockWeb.Blocks.Communication.Email" %>

<fieldset>
    <Rock:LabeledTextBox ID="tbFromName" runat="server" LabelText="From Name" />
    <Rock:LabeledTextBox ID="tbFromAddress" runat="server" LabelText="From Address" />
    <Rock:LabeledTextBox ID="tbReplyToAddress" runat="server" LabelText="Reply To Address" />
    <Rock:LabeledTextBox ID="tbSubject" runat="server" LabelText="Subject" />
</fieldset>