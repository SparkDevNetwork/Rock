<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupMemberScheduleTemplateList.ascx.cs" Inherits="RockWeb.Blocks.GroupScheduling.GroupMemberScheduleTemplateList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <Rock:ModalAlert ID="mdGridWarning" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-calendar-day"></i>
                    Group Member Schedule Templates
                </h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gList" runat="server" AllowSorting="true" OnRowSelected="gList_RowSelected">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="FullName" />
                            <Rock:DeleteField OnClick="gList_DeleteClick" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
