<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Communication.ascx.cs" Inherits="RockWeb.Blocks.Core.Communication" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <asp:HiddenField ID="hfCommunicationId" runat="server" />
        <asp:HiddenField ID="hfChannelId" runat="server" />

        <ul class="nav nav-pills">
            <asp:Repeater ID="rptChannels" runat="server">
                <ItemTemplate>
                    <li class='<%# (int)Eval("Key") == ChannelEntityTypeId ? "active" : "" %>'>
                        <asp:LinkButton ID="lbChannel" runat="server" Text='<%# Eval("Value") %>' CommandArgument='<%# Eval("Key") %>' OnClick="lbChannel_Click">
                        </asp:LinkButton>
                    </li>
                </ItemTemplate>
            </asp:Repeater>
        </ul>
        
        <div class="control-group">
            <div class="control-label">
                To: <asp:Literal ID="lNumRecipients" runat="server" /> <Rock:PersonPicker ID="ppAddPerson" runat="server" PersonName="Add Person" OnSelectPerson="ppAddPerson_SelectPerson" />
            </div>
            <div class="recipient">
                <ul class="recipient-content">
                    <asp:Repeater ID="rptRecipients" runat="server" OnItemCommand="rptRecipients_ItemCommand">
                        <ItemTemplate>
                            <li><%# Eval("Value") %> <asp:LinkButton ID="lbRemoveRecipient" runat="server" CommandArgument='<%# Eval("Key") %>'><i class="icon-remove"></i></asp:LinkButton></li>
                        </ItemTemplate>
                    </asp:Repeater>
                </ul>
            </div>
        </div>

        <div class="pull-right">
            <asp:LinkButton ID="lbShowAllRecipients" runat="server" Text="Show All" OnClick="lbShowAllRecipients_Click" />
            <asp:LinkButton ID="lbRemoveAllRecipients" runat="server" Text="Remove All" CssClass="remove-all-recipients" OnClick="lbRemoveAllRecipients_Click" />
        </div>

        <asp:PlaceHolder ID="phContent" runat="server" />

        <div class="actions">
            <asp:LinkButton ID="btnSubmit" runat="server" Text="Submit" CssClass="btn btn-primary" OnClick="btnSubmit_Click" />
            <asp:LinkButton ID="btnApprove" runat="server" Text="Approve" CssClass="btn btn-primary" OnClick="btnApprove_Click" />
            <asp:LinkButton ID="btnDeny" runat="server" Text="Deny" CssClass="btn" OnClick="btnDeny_Click" />
            <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn" OnClick="btnSave_Click" />
            <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel Send" CssClass="btn" OnClick="btnCancel_Click" />
            <asp:LinkButton ID="btnCopy" runat="server" Text="Copy Communication" CssClass="btn" OnClick="btnCopy_Click" />
        </div>


    </ContentTemplate>
</asp:UpdatePanel>


