<%@ Control Language="C#" AutoEventWireup="true" CodeFile="InteractiveExperienceList.ascx.cs" Inherits="RockWeb.Blocks.Event.InteractiveExperiences.InteractiveExperienceList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-list"></i> Experiences
                </h1>
            </div>

            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:GridFilter ID="gfExperiences" runat="server">
                        <Rock:CampusPicker ID="cpCampus" runat="server" Label="Campus" ForceVisible="true" />
                        <Rock:RockCheckBox ID="cbShowInactive" runat="server" Checked="false" Label="Include Inactive" />
                    </Rock:GridFilter>

                    <Rock:Grid ID="gExperienceList" runat="server" AllowSorting="true" OnRowSelected="gExperienceList_RowSelected" CssClass="js-experience-list">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" ItemStyle-CssClass="js-experience-name" />
                            <Rock:DateTimeField DataField="NextStartDateTime" HeaderText="Next Start" SortExpression="NextStartDateTime" />
                            <Rock:RockBoundField DataField="ActionCount" HeaderText="Actions" SortExpression="ActionCount" />
                            <Rock:RockBoundField DataField="Campus" HeaderText="Campus" />
                            <Rock:BoolField DataField="IsActive" HeaderText="Active" SortExpression="IsActive" />
                            <Rock:RockTemplateField HeaderStyle-CssClass="grid-columncommand" ItemStyle-CssClass="grid-columncommand">
                                <ItemTemplate>
                                    <asp:LinkButton ID="lbShowManager" runat="server" CssClass="btn btn-default btn-sm" ToolTip="Open Experience Manager" Visible='<%# Eval( "HasOccurrences" ) %>' OnCommand="lbShowManager_Command" CommandArgument='<%# Eval( "Id" ) %>'>
                                        <i class="fa fa-desktop"></i>
                                    </asp:LinkButton>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:DeleteField OnClick="gExperienceList_DeleteClick" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
