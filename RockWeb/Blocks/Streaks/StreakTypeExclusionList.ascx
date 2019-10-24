<%@ Control Language="C#" AutoEventWireup="true" CodeFile="StreakTypeExclusionList.ascx.cs" Inherits="RockWeb.Blocks.Streaks.StreakTypeExclusionList" %>

<asp:UpdatePanel ID="upMain" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbBlockStatus" runat="server" NotificationBoxType="Info" />

        <asp:Panel ID="pnlContent" runat="server">

            <div id="pnlExclusions" runat="server">

                <div class="panel panel-block">

                    <div class="panel-heading clearfix">
                        <h1 class="panel-title pull-left">
                            <i class="fa fa-calendar-times"></i>
                            <asp:Literal ID="lHeading" runat="server" Text="Exclusions" />
                        </h1>
                    </div>

                    <div class="panel-body">
                        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

                        <div class="grid grid-panel">
                            <Rock:Grid ID="gExclusions" runat="server" AllowSorting="true" OnRowSelected="gExclusions_Edit">
                                <Columns>
                                    <Rock:RockBoundField HeaderText="Location" DataField="LocationName" SortExpression="LocationName" />
                                    <Rock:DeleteField OnClick="gExclusions_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </div>
                </div>
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
