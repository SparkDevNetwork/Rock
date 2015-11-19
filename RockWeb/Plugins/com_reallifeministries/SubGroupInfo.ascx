<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SubGroupInfo.ascx.cs" Inherits="com.reallifeministries.SubGroupInfo" %>

<asp:UpdatePanel runat="server">
    <ContentTemplate>
        <asp:panel ID="pnlContent" runat="server">
            <div class="panel panel-block">
                
                <div class="panel-heading clearfix">
                    <h1 class="panel-title pull-left">
                        <i class="fa fa-users"></i>
                        <asp:Literal ID="lHeading" runat="server" Text="Sub Groups" />
                    </h1>
                </div>

                <div class="panel-body">
                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                    <div class="grid grid-panel">
                        <Rock:Grid runat="server" ID="gSubGroups" OnGridRebind="gSubGroups_GridRebind"
                            OnRowSelected="gSubGroups_RowSelected" DataKeyNames="GroupId">
                            <Columns>
                                <Rock:RockBoundField DataField="Group.Name" HeaderText="Name" />
                                <asp:TemplateField HeaderText="Groups">
                                    <ItemTemplate>
                                        <i class="fa fa-info-circle" title="Coming Soon!"></i>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <Rock:RockBoundField DataField="ActiveMembers" HeaderText="Active" />
                                <Rock:RockBoundField DataField="PendingMembers" HeaderText="Pending" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>

            </div>
        </asp:panel>
    </ContentTemplate>
</asp:UpdatePanel>