<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonalLinkSectionList.ascx.cs" Inherits="RockWeb.Blocks.Cms.PersonalLinkSectionList" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-bookmark"></i>
                    Link Sections</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="gfFilter" runat="server">
                        <Rock:RockTextBox ID="txtSectionName" runat="server" Label="Name" />
                    </Rock:GridFilter>
                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                    <Rock:Grid ID="gSectionList" runat="server" RowItemText="Section" OnRowDataBound="gSectionList_RowDataBound">
                        <Columns>
                            <Rock:ReorderField />
                            <Rock:RockBoundField DataField="Name" HeaderText="Section Name" />
                            <Rock:RockBoundField DataField="LinkCount" HeaderText="Link Count" />
                            <Rock:BoolField DataField="IsShared" HeaderText="Shared" />
                            <Rock:DeleteField OnClick="gSectionList_Delete" OnDataBound="DeleteButton_OnDataBound" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>


