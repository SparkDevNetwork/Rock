<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonSignalTypeList.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonSignalTypeList" %>

<asp:UpdatePanel ID="upPersonSignalType" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-flag"></i> Person Signal Type List</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gPersonSignalType" runat="server" AllowSorting="false">
                        <Columns>
                            <Rock:ReorderField />
                            <Rock:ColorField DataField="SignalColor" HeaderText="Color" />
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:RockBoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                            <Rock:SecurityField TitleField="Name" />
                            <Rock:DeleteField OnClick="gPersonSignalType_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
