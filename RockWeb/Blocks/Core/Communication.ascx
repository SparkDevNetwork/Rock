<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Communication.ascx.cs" Inherits="RockWeb.Blocks.Core.Communication" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlEdit" runat="server">

        <asp:HiddenField ID="hfCommunicationId" runat="server" />
        <asp:HiddenField ID="hfChannelId" runat="server" />

        <asp:Panel ID="pnlStatus" runat="server" CssClass="pull-right">
            <h4><asp:Literal ID="lStatus" runat="server" /></h4>
            <h5><asp:Literal ID="lRecipientStatus" runat="server" /></h5>
        </asp:Panel>

        <ul class="nav nav-pills">
            <asp:Repeater ID="rptChannels" runat="server">
                <ItemTemplate>
                    <li class='<%# (int)Eval("Key") == ChannelEntityTypeId ? "active" : "" %>'>
                        <asp:LinkButton ID="lbChannel" runat="server" Text='<%# Eval("Value") %>' CommandArgument='<%# Eval("Key") %>' OnClick="lbChannel_Click" CausesValidation="false">
                        </asp:LinkButton>
                    </li>
                </ItemTemplate>
            </asp:Repeater>
        </ul>
        
        <asp:ValidationSummary ID="ValidationSummary" runat="server" CssClass="alert alert-error" />

        <div class="control-group">
            <div class="control-label">
                To: <asp:Literal ID="lNumRecipients" runat="server" /> <Rock:PersonPicker ID="ppAddPerson" runat="server" PersonName="Add Person" OnSelectPerson="ppAddPerson_SelectPerson" />
                <asp:CustomValidator ID="valRecipients" runat="server" OnServerValidate="valRecipients_ServerValidate" Display="None" ErrorMessage="At least one recipient is required." />
            </div>
            <div class="recipient">
                <ul class="recipient-content">
                    <asp:Repeater ID="rptRecipients" runat="server" OnItemCommand="rptRecipients_ItemCommand" OnItemDataBound="rptRecipients_ItemDataBound">
                        <ItemTemplate>
                            <li class='<%# Eval("Status").ToString().ToLower() %>'><%# Eval("PersonName") %> <asp:LinkButton ID="lbRemoveRecipient" runat="server" CommandArgument='<%# Eval("PersonId") %>' CausesValidation="false"><i class="icon-remove"></i></asp:LinkButton></li>
                        </ItemTemplate>
                    </asp:Repeater>
                </ul>
            </div>
        </div>

        <div class="pull-right">
            <asp:LinkButton ID="lbShowAllRecipients" runat="server" Text="Show All" OnClick="lbShowAllRecipients_Click" CausesValidation="false"/>
            <asp:LinkButton ID="lbRemoveAllRecipients" runat="server" Text="Remove All Pending Recipients" CssClass="remove-all-recipients" OnClick="lbRemoveAllRecipients_Click" CausesValidation="false"/>
        </div>

        <asp:PlaceHolder ID="phContent" runat="server" />

        <Rock:DateTimePicker ID="dtpFutureSend" runat="server" LabelText="Delay Send Until" SourceTypeName="Rock.Model.Communication" PropertyName="FutureSendDateTime" />

        <div class="actions">
            <asp:LinkButton ID="btnSubmit" runat="server" Text="Submit" CssClass="btn btn-primary" OnClick="btnSubmit_Click" />
            <asp:LinkButton ID="btnApprove" runat="server" Text="Approve" CssClass="btn btn-primary" OnClick="btnApprove_Click" />
            <asp:LinkButton ID="btnDeny" runat="server" Text="Deny" CssClass="btn" OnClick="btnDeny_Click" />
            <asp:LinkButton ID="btnSave" runat="server" Text="Save as Draft" CssClass="btn" OnClick="btnSave_Click" />
            <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel Send" CssClass="btn" OnClick="btnCancel_Click" />
            <asp:LinkButton ID="btnCopy" runat="server" Text="Copy Communication" CssClass="btn" OnClick="btnCopy_Click" CausesValidation="false" />
        </div>

        </asp:Panel>

        <asp:Panel ID="pnlResult" runat="server" Visible="false">
            <Rock:NotificationBox ID="nbResult" runat="server" NotificationBoxType="Success" />
            <br />
            <asp:HyperLink ID="hlViewCommunication" runat="server" Text="View Communication" />
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>


