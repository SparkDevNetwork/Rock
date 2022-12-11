<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonalizationSegmentResults.ascx.cs" Inherits="RockWeb.Blocks.Cms.PersonalizationSegmentResults" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title pull-left">
                    <i class="fa fa-users"></i>
                    <asp:Literal ID="lTitle" runat="server" />
                </h1>
            </div>

            <div class="panel-body">
                <Rock:NotificationBox ID="nbRoleWarning" runat="server" CssClass="alert-grid" NotificationBoxType="Warning" Title="No roles!" Visible="false" />

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="gfFilter" runat="server">
                        <Rock:RockTextBox ID="tbNickName" runat="server" Label="Nick Name" />
                        <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="gResults" runat="server" DisplayType="Full" AllowSorting="true">
                        <Columns>
                            <Rock:RockBoundField DataField="FullName" HeaderText="Name" SortExpression="LastName,NickName" />
                            <Rock:DefinedValueField DataField="ConnectionStatusValueId" HeaderText="Connection Status" SortExpression="ConnectionStatusValueId" />
                            <Rock:RockBoundField
                                DataField="Gender"
                                HeaderText="Gender"
                                SortExpression="Gender"
                                ColumnPriority="Desktop" />
                            <Rock:RockBoundField
                                HeaderText="Age Classification"
                                DataField="AgeClassification"
                                SortExpression="AgeClassification" />
                            <Rock:DefinedValueField
                                DataField="RecordStatusValueId"
                                HeaderText="Record Status"
                                ColumnPriority="Desktop" />
                            <asp:HyperLinkField ItemStyle-CssClass="grid-columncommand" ControlStyle-CssClass="btn btn-default btn-sm" DataTextFormatString="<i class='fa fa-user'></i>" DataTextField="FullName" DataNavigateUrlFormatString="/Person/{0}" DataNavigateUrlFields="Id" /> 
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
