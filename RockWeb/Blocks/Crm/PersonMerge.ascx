<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonMerge.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonMerge" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-copy"></i> Merge Records</h1>
            </div>
            <div class="panel-body">

                <Rock:PersonPicker ID="ppAdd" runat="server" Label="Add Another Person" OnSelectPerson="ppAdd_SelectPerson" />

                <Rock:NotificationBox ID="nbPeople" runat="server" NotificationBoxType="Warning" Visible="false"
                    Text="You need to select at least two people to merge." />

                <asp:HiddenField ID="hfSelectedColumn" runat="server" />
        
                <div class="grid">
                    <Rock:Grid ID="gValues" runat="server" EnableResponsiveTable="false" AllowSorting="false" EmptyDataText="No Results" />
                </div>
            
                <Rock:NotificationBox ID="nbSecurityNotice" runat="server" NotificationBoxType="danger" Visible="false" Title="Account Hijack Possible:" Heading="Security Alert" 
                    Text="Because there are two different emails associated with this merge, and at least one of the records has a login, be sure to proceed with caution.
                    It is possible that the new record was created in an attempt to gain access to the account through the merge process. While this person will be
                    prompted to reconfirm their logins using the email address you select, you may wish to manually confirm the validity of the request before
                    completing this merge." />

                <div class="actions pull-right">
                    <asp:LinkButton ID="lbMerge" runat="server" Text="Merge Records" CssClass="btn btn-primary" OnClick="lbMerge_Click" />
                </div>
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
