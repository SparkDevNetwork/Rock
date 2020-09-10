<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SmsPipelineList.ascx.cs" Inherits="RockWeb.Blocks.Communication.SmsPipelineList" %>

<asp:UpdatePanel ID="upSettings" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Danger" Visible="false" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-sms"></i> SMS Pipeline List</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gSmsPipelines" runat="server" AllowSorting="true" OnRowSelected="gSmsPipelines_Edit">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:RockBoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                            <Rock:RockBoundField DataField="IsActive" HeaderText="Is Active" SortExpression="IsActive" />
                            <Rock:DeleteField OnClick="gSmsPipelines_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
