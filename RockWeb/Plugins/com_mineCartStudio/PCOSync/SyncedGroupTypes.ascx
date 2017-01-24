<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SyncedGroupTypes.ascx.cs" Inherits="RockWeb.Plugins.com_mineCartStudio.PCOSync.SyncedGroupTypes" %>

<asp:UpdatePanel ID="upSettings" runat="server">
    <ContentTemplate>
        
        <asp:Panel ID="pnlContent" runat="server">

            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-sitemap"></i> Group Types</h1>
                </div>
                <div class="panel-body">
                    <div class="grid grid-panel">
                        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                        <Rock:Grid ID="gGroupType" runat="server" AllowSorting="true" TooltipField="Description" RowItemText="Group Type" CssClass="js-grid-grouptypes">
                            <Columns>
                                <Rock:RockBoundField DataField="Name" HeaderText="Group Type" SortExpression="Name" />
                                <Rock:RockBoundField DataField="GroupCount" HeaderText ="Active Groups" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:N0}" />
                                <Rock:RockBoundField DataField="MemberCount" HeaderText ="Active Members" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:N0}" />
                                <Rock:DeleteField OnClick="gGroupType_Delete" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>
            </div>

            <Rock:ModalDialog ID="mdlgGroupType" runat="server" Title="Add Group Type" ValidationGroup="GroupType" >
                <Content>

                    <p>
                        Select a new group type that should be configured to allow groups to sync to Planning Center Online. Adding a group type will result in the 
                        neccessary group attributes being created for that group type to allow syncing.
                    </p>

                    <asp:ValidationSummary ID="valSummaryValue" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="GroupType" />

                    <Rock:RockDropDownList Id="ddlGroupType" runat="server" Label="Group Type" Required="true" ValidationGroup="GroupType" />

                </Content>
            </Rock:ModalDialog>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
