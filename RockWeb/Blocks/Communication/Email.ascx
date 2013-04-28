<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Email.ascx.cs" Inherits="RockWeb.Blocks.Communication.Email" %>

<div class="row-fluid">
    <div class="span6">
        <Rock:LabeledTextBox ID="tbFromName" runat="server" LabelText="From Name" />
        <Rock:LabeledTextBox ID="tbFromAddress" runat="server" LabelText="From Address" />
        <Rock:LabeledTextBox ID="tbReplyToAddress" runat="server" LabelText="Reply To Address" />
        <Rock:LabeledTextBox ID="tbSubject" runat="server" LabelText="Subject" />
        <Rock:LabeledHtmlEditor ID="htmlMessage" runat="server" LabelText="Message" />
        <Rock:LabeledTextBox ID="tbTextMessage" runat="server" LabelText="Message (Text Version)" TextMode="MultiLine" Rows="8" />
    </div>
    <div class="span6">

        <asp:HiddenField id="hfAttachments" runat="server" />
        <Rock:LabeledFileUploader ID="fuAttachments" runat="server" LabelText="Attachments" />

        <div class="attachment">
            <ul class="attachment-content">
                <asp:Repeater ID="rptAttachments" runat="server" OnItemCommand="rptAttachments_ItemCommand">
                    <ItemTemplate>
                        <li>
                            <a target="_blank" href='<%# string.Format( "{0}File.ashx?{1}", System.Web.VirtualPathUtility.ToAbsolute( "~" ), Eval("Key") ) %>'><%# Eval("Value") %></a>
                            <asp:LinkButton ID="lbRemoveAttachment" runat="server" CommandArgument='<%# Eval("Key") %>'><i class="icon-remove"></i></asp:LinkButton>                        
                        </li>
                    </ItemTemplate>
                </asp:Repeater>
            </ul>
        </div>

    </div>
</div>
