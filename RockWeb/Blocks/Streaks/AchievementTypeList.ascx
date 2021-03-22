<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AchievementTypeList.ascx.cs" Inherits="RockWeb.Blocks.Streaks.AchievementTypeList" %>

<asp:UpdatePanel ID="upAchievementTypeList" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-medal"></i>
                    Achievement Types
                </h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gAchievements" runat="server" AllowSorting="false" OnRowSelected="gAchievements_Edit">
                        <Columns>
                            <Rock:RockTemplateField ExcelExportBehavior="NeverInclude" HeaderStyle-Width="48px">
                                <ItemTemplate>
                                   <i class="fa-fw <%# Eval( "IconCssClass" ) %>"></i>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" />
                            <Rock:BoolField DataField="IsActive" HeaderText="Active" />
                            <Rock:RockBoundField DataField="ComponentName" HeaderText="Achievement Type" />
                            <Rock:RockBoundField DataField="SourceName" HeaderText="Source" />
                            <Rock:DeleteField OnClick="gAchievements_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
