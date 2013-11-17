<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CheckInGroupTypeList.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.CheckinGroupTypeList" %>

<asp:UpdatePanel ID="upGroupType" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        <Rock:Grid ID="gGroupType" runat="server" AllowSorting="true" OnRowCommand="gGroupType_RowCommand">
            
            <Columns>
                <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                <asp:BoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                <asp:TemplateField ItemStyle-CssClass="grid-col-actions" HeaderStyle-CssClass="grid-col-actions" HeaderText="Actions">
                    <ItemTemplate>
                        <asp:LinkButton runat="server" ID="btnSchedule" CssClass="btn btn-action" CausesValidation="false" CommandName="schedule" CommandArgument="<%# Container.DataItemIndex %>"><i class="fa fa-calendar"></i> Schedule</asp:LinkButton>
                        <asp:LinkButton runat="server" ID="btnConfigure" CssClass="btn btn-action" CausesValidation="false" CommandName="configure" CommandArgument="<%# Container.DataItemIndex %>"><i class="fa fa-cog"></i> Groups/Locations</asp:LinkButton>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>

        </Rock:Grid>

        <Rock:ModalDialog runat="server" ID="mdAddCheckinGroupType" SaveButtonText="Save" Title="Add Check-in Configuration Type" OnSaveClick="mdAddCheckinGroupType_SaveClick" ValidationGroup="CheckinGroupTypeListAdd">
            <Content>
                <Rock:RockTextBox runat="server" ID="tbGroupTypeName" Label="Name" Required="true" ValidationGroup="CheckinGroupTypeListAdd" />
            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>
