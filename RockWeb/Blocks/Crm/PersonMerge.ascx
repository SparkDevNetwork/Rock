<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonMerge.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonMerge" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-copy"></i>&nbsp;Merge Records</h1>
            </div>
            <div class="panel-body">
                
                <%-- View Panel for merge that have only have VIEW auth to the block --%>
                <asp:Panel runat="server" ID="pnlView">
                    <Rock:NotificationBox runat="server" ID="nbNotAuthorized" NotificationBoxType="Warning" Title="Sorry" Text="You are not authorized to merge people. If you find duplicates through a search, a report, or any other list of people, you can submit a request from that list to have the duplicates merged." Visible="false"/>
                    <Rock:NotificationBox runat="server" ID="nbMergeRequestSuccess" NotificationBoxType="Success" Title="Success" Text="Your merge request has been sent to the data integrity team to process." Visible="false" />
                    <Rock:NotificationBox runat="server" ID="nbMergeRequestAlreadySubmitted" NotificationBoxType="Info" Title="" Text="This merge has already been requested. You might be seeing this message because you are not authorized to merge records. You can update the optional note if you have more information to add." Visible="false" />

                    <Rock:RockTextBox runat="server" ID="tbEntitySetNote" Label="Optional Note" Rows="4" TextMode="MultiLine" Visible="false" />
                    <div class="actions">
                        <asp:LinkButton ID="btnSaveRequestNote" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSaveRequestNote_Click" Visible="false" />
                    </div>

                    <Rock:NotificationBox runat="server" ID="nbNoteSavedSuccess" NotificationBoxType="Success" Title="Success" Text="Your note has been saved." Visible="false" />
                </asp:Panel>
                
                <%-- Edit Panel for merge that have EDIT auth to the block --%>
                <asp:Panel runat="server" ID="pnlEdit">
                    <Rock:PersonPicker ID="ppAdd" runat="server" Label="Add Another Person" OnSelectPerson="ppAdd_SelectPerson" />

                    <Rock:NotificationBox ID="nbPeople" runat="server" NotificationBoxType="Warning" Visible="false"
                        Text="You need to select at least two people to merge." />

                    <asp:HiddenField ID="hfSelectedColumn" runat="server" />

                    <div class="grid">
                        <Rock:Grid ID="gValues" runat="server" EnableResponsiveTable="false" AllowSorting="false" EmptyDataText="No Results" />
                    </div>

                    <Rock:NotificationBox ID="nbSecurityNotice" runat="server" NotificationBoxType="danger" Visible="false" Heading="Security Alert, Account Hijack Possible:" />

                    <Rock:NotificationBox ID="nbError" runat="server" NotificationBoxType="Danger" Visible ="false" />

                    <div class="actions pull-right">
                        <asp:LinkButton ID="lbMerge" runat="server" Text="Merge Records" CssClass="btn btn-primary" OnClick="lbMerge_Click" />
                    </div>

                </asp:Panel>
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
