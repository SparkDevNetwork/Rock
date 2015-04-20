<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CheckInGroupTypeList.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.CheckinGroupTypeList" %>

<asp:UpdatePanel ID="upGroupType" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-check-square-o"></i> Check-in Configurations List</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gGroupType" runat="server" AllowSorting="true" OnRowCommand="gGroupType_RowCommand" OnRowSelected="gGroupType_RowSelected">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:RockBoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                            <Rock:RockTemplateField ItemStyle-CssClass="grid-col-actions" HeaderStyle-CssClass="grid-col-actions" HeaderText="Actions">
                                <ItemTemplate>
                                    <div class="btn-group">
                                        <asp:LinkButton runat="server" ID="btnSchedule" CssClass="btn btn-action" CausesValidation="false" CommandName="schedule" CommandArgument="<%# Container.DataItemIndex %>"><i class="fa fa-calendar"></i> Schedule</asp:LinkButton>
                                        <asp:LinkButton runat="server" ID="btnConfigure" CssClass="btn btn-action" CausesValidation="false" CommandName="configure" CommandArgument="<%# Container.DataItemIndex %>"><i class="fa fa-cog"></i> Groups/Locations</asp:LinkButton>
                                    </div>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>

        <Rock:ModalDialog runat="server" ID="mdAddEditCheckinGroupType" SaveButtonText="Save" Title="Check-in Configuration Type" OnSaveClick="mdAddCheckinGroupType_SaveClick" ValidationGroup="CheckinGroupTypeListAdd">
            <Content>
                <asp:HiddenField runat="server" ID="hfGroupTypeId" />
                <Rock:RockTextBox runat="server" ID="tbGroupTypeName" Label="Name" Required="true" ValidationGroup="CheckinGroupTypeListAdd" />
                <Rock:RockTextBox runat="server" ID="tbGroupTypeDescription" Label="Description" ValidationGroup="CheckinGroupTypeListAdd" TextMode="MultiLine" Rows="4" />
            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>
