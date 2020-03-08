<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CampusStaffPersonalContacts.ascx.cs" Inherits="RockWeb.Plugins.com_lcbcchurch.NewVisitor.CampusStaffPersonalContacts" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <div class="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fas fa-building"></i> <asp:Literal ID="lCampus" runat="server" /> Staff Personal Contacts</h1>
            </div>
            <div class="panel-body">
                <asp:HiddenField ID="hfCampusId" runat="server" />
                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" Visible="false" />
                <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                <div class="grid grid-panel">
                    <Rock:Grid ID="gPersons" runat="server" CssClass="js-grid-requests" AllowSorting="true" ExportSource="ColumnOutput">
                        <Columns>
                            <asp:HyperLinkField DataTextField="FullName" DataNavigateUrlFields="Id" SortExpression="Person.LastName,Person.NickName" DataNavigateUrlFormatString="~/Person/{0}" HeaderText="Name" />
                            <Rock:RockTemplateField HeaderText="Last Week" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center" SortExpression="LastWeek">
                                <ItemTemplate>
                                    <%# GetPercentageColumnHtml((int)Eval("LastWeek") ) %>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockBoundField DataField="FaceToFaceConnections" HeaderText="Face-to-Face <br/> Connections Last Week" HtmlEncode="false" SortExpression="FaceToFaceConnections" DataFormatString="{0:N0}" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" />
                            <Rock:RockBoundField DataField="OtherConnections" HeaderText="Other Connections Last Week" SortExpression="OtherConnections" DataFormatString="{0:N0}" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" />
                            <Rock:RockTemplateField HeaderText="Previous 4 Weeks" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center" SortExpression="Previous4Weeks">
                                <ItemTemplate>
                                    <%# GetPercentageColumnHtml((int)Eval("Previous4Weeks") ) %>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
