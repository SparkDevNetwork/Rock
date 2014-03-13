<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonMerge.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonMerge" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:PersonPicker ID="ppAdd" runat="server" Label="Add Another Person" OnSelectPerson="ppAdd_SelectPerson" />

        <Rock:NotificationBox ID="nbPeople" runat="server" NotificationBoxType="Warning" Visible="false"
            Text="You need to select at least two people to merge." />

        <asp:HiddenField ID="hfSelectedColumn" runat="server" />
        <Rock:Grid ID="gValues" runat="server" AllowSorting="false" EmptyDataText="No Results" />
            
        <Rock:NotificationBox ID="nbSecurityNotice" runat="server" NotificationBoxType="danger" Visible="false" Title="Multiple Email Addresses Exist" Heading="Security Alert" 
            Text="Please choose the correct email address carefully!  After the merge, this person will be prompted to reconfirm their logins using the email address you select.  If it appears that a user created a new email address, you may want to consider manually confirming the validity of that request before selecting the new email address and merging these records." />

        <div class="actions pull-right">
            <asp:LinkButton ID="lbMerge" runat="server" Text="Merge Records" CssClass="btn btn-primary" OnClick="lbMerge_Click" />
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
