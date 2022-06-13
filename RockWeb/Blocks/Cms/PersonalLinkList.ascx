<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonalLinkList.ascx.cs" Inherits="RockWeb.Blocks.Cms.PersonalLinkList" %>
<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlContent" CssClass="panel panel-block" runat="server">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-bookmark"></i>
                    <asp:Literal ID="lTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="gfFilter" runat="server">
                        <Rock:RockTextBox ID="txtLinkName" runat="server" Label="Name" />
                    </Rock:GridFilter>
                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                    <Rock:Grid ID="gLinkList" runat="server" RowItemText="Link">
                        <Columns>
                            <Rock:ReorderField />
                            <Rock:RockBoundField DataField="Name" HeaderText="Link Name" />
                            <Rock:RockBoundField DataField="Url" HeaderText="Link URL" TruncateLength="65" />
                            <Rock:EditField OnClick="gLinkList_Edit" OnDataBound="EditButton_OnDataBound" />
                            <Rock:DeleteField OnClick="gLinkList_Delete" OnDataBound="DeleteButton_OnDataBound" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
            <Rock:ModalDialog ID="mdAddPersonalLink" runat="server" Title="Add Personal Link" OnSaveClick="mdAddPersonalLink_SaveClick" ValidationGroup="AddPersonalLink">
                <Content>
                    <asp:HiddenField ID="hfPersonalLinkId" runat="server" />
                    <asp:ValidationSummary ID="vsEditPersonal" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="AddPersonalLink" />
                    <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.PersonalLink, Rock" PropertyName="Name" Label="Link Name" />
                    <Rock:UrlLinkBox ID="urlLink" runat="server" Label="Link URL" ValidationGroup="AddPersonalLink" Required="true" />
                </Content>
            </Rock:ModalDialog>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>



