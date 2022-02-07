<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EventList.ascx.cs" Inherits="RockWeb.Blocks.Follow.EventList" %>

<asp:UpdatePanel ID="pnlEventListUpdatePanel" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-flag"></i> Event List</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="rGridEvent" runat="server"  RowItemText="Event" OnRowSelected="rGridEvent_Edit" TooltipField="Description" >
                        <Columns>
                            <Rock:ReorderField />
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" />
                            <asp:TemplateField HeaderText="Type" >
                                <ItemTemplate><%# GetComponentName( Eval( "EntityType") )%></ItemTemplate>
                            </asp:TemplateField>
                            <Rock:BoolField DataField="IsActive" HeaderText="Active" />
                            <Rock:SecurityField TitleField="Name" />
                            <Rock:DeleteField OnClick="rGridEvent_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
