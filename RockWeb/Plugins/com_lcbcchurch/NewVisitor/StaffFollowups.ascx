<%@ Control Language="C#" AutoEventWireup="true" CodeFile="StaffFollowups.ascx.cs" Inherits="RockWeb.Plugins.com_lcbcchurch.NewVisitor.StaffFollowups" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-user"></i><asp:Literal ID="lTitle" runat="server" />'s Engagements</h1>
            </div>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" Visible="false" />
                <asp:HiddenField ID="hfPersonId" runat="server" />
                <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                <div class="grid grid-panel">
                    <Rock:Grid ID="gNotes" runat="server" CssClass="js-grid-requests" AllowSorting="true">
                        <Columns>
                            <Rock:DateField HeaderText="Date" DataField="CreatedDateTime" SortExpression="CreatedDateTime" ItemStyle-HorizontalAlign="Left"  HeaderStyle-HorizontalAlign="Left"/>
                            <asp:HyperLinkField DataTextField="FullName" DataNavigateUrlFields="PersonId" SortExpression="Person.LastName,Person.NickName" DataNavigateUrlFormatString="~/Person/{0}" HeaderText="Name" />
                            <Rock:RockTemplateField HeaderText="Type">
                                <ItemTemplate>
                                    <i class='<%# Eval("TypeIconCssClass") %>' aria-hidden='true' title='<%# Eval("NoteTypeName") %>'></i>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockBoundField DataField="Text" HeaderText="Text" SortExpression="Text" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>

