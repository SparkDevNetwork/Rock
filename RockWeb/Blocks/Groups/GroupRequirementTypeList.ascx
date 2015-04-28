<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupRequirementTypeList.ascx.cs" Inherits="RockWeb.Blocks.Groups.GroupRequirementTypeList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-check-square-o"></i>&nbsp;Group Requirement Types</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gList" runat="server" AllowSorting="true" OnRowSelected="gList_Edit">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:BoolField DataField="CanExpire" HeaderText="Can Expire" SortExpression="CanExpire" />
                            <Rock:EnumField DataField="RequirementCheckType" HeaderText="Type" SortExpression="RequirementCheckType" />
                            <Rock:DeleteField OnClick="gList_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
