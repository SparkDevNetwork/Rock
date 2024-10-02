<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ThemeList.ascx.cs" Inherits="RockWeb.Blocks.Cms.ThemeList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
            <asp:HiddenField ID="hfClonedThemeName" runat="server" />
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-picture-o "></i> Themes</h1>
            </div>
            <div class="panel-body">

                <Rock:NotificationBox ID="nbMessages" runat="server" CssClass="alert-grid" />

                <div class="grid grid-panel">
                    <Rock:Grid ID="gThemes" runat="server" AllowSorting="true" DataKeyNames="Name" OnRowSelected="gThemes_RowSelected" RowItemText="Theme">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:RockBoundField DataField="Purpose" HeaderText="Purpose" SortExpression="Purpose" />
                            <Rock:BoolField DataField="IsActive" HeaderText="Active" HeaderStyle-HorizontalAlign="Center"  ItemStyle-HorizontalAlign="Center"/>
                            <Rock:BoolField DataField="IsSystem" HeaderText="System" HeaderStyle-HorizontalAlign="Center"  ItemStyle-HorizontalAlign="Center"/>
                            <Rock:LinkButtonField HeaderText="Compile" CssClass="btn btn-default btn-sm" Text="<i class='fa fa-refresh'></i>" OnClick="gCompileTheme_Click" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center"/>
                            <Rock:LinkButtonField HeaderText="Copy" CssClass="btn btn-default btn-sm" Text="<i class='fa fa-clone'></i>" OnClick="gCloneTheme_Click" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center"/>
                            <Rock:DeleteField OnClick="gThemes_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>

            <Rock:ModalAlert ID="mdThemeCompile" runat="server" />

            <Rock:ModalDialog ID="mdThemeClone" runat="server" Title="Clone Theme" ValidationGroup="vgClone" SaveButtonText="Clone" OnSaveClick="mdThemeClone_SaveClick" OnCancelScript="clearActiveDialog();">
                <Content>
                    <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Enter A New Theme Name" CssClass="alert alert-validation" />
                    <Rock:RockTextBox ID="tbNewThemeName" runat="server" Label="New Theme Name" Required="true" ValidationGroup="vgClone" />
                    <small>Note: Spaces and special characters will be removed.</small>
                </Content>
            </Rock:ModalDialog>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
