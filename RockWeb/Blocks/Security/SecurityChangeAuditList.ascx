<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SecurityChangeAuditList.ascx.cs" Inherits="RockWeb.Blocks.Security.SecurityChangeAuditList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <Rock:ModalAlert ID="maGridWarning" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-file"></i> Security Change Audit</h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:GridFilter ID="gfFiler" runat="server">
                        <Rock:DateRangePicker ID="drpDate" runat="server" Label="Date" />
                        <Rock:EntityTypePicker ID="etpEntityType" runat="server" Label="Entity Type" />
                        <Rock:NumberBox ID="nbEntityId" runat="server" Label="Entity Id" />
                        <Rock:PersonPicker ID="ppChangedBy" runat="server" Label="Changed By" />
                    </Rock:GridFilter>

                    <Rock:Grid ID="gSecurityChangeAudit" runat="server" AllowSorting="true" RowItemText="Security Change" OnRowDataBound="gSecurityChangeAudit_RowDataBound">
                        <Columns>
                            <Rock:DateField DataField="ChangeDateTime" HeaderText="Date" SortExpression="ChangeDateTime" />
                            <Rock:RockBoundField DataField="EntityType.FriendlyName" HeaderText="Entity Type" SortExpression="EntityType.FriendlyName" />
                            <Rock:RockBoundField DataField="EntityId" HeaderText="EntityId" SortExpression="EntityId" />
                            <Rock:PersonField DataField="ChangeByPersonAlias" HeaderText="Changed By" SortExpression="ChangeByPersonAlias.Person.LastName, ChangeByPersonAlias.Person.NickName" />
                            <Rock:RockBoundField DataField="Action" HeaderText="Action" SortExpression="Action" />
                            <Rock:RockLiteralField ID="lAccess" HeaderText="Access" />
                            <Rock:RockBoundField DataField="Group.Name" HeaderText="Group" SortExpression="Group.Name" />
                            <Rock:EnumField DataField="SpecialRole" SortExpression="SpecialRole" HeaderText="Special Role" />
                            <Rock:RockLiteralField ID="lOrder" HeaderText="Order" />
                            <Rock:RockLiteralField ID="lChange" HeaderText="Change" SortExpression="ChangeType" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>

        </div>

    </ContentTemplate>
</asp:UpdatePanel>
