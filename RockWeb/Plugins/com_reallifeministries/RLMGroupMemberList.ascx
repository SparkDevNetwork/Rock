<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RLMGroupMemberList.ascx.cs" Inherits="com.reallifeministries.RLMGroupMemberList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlContent" runat="server">

            <div id="pnlGroupMembers" runat="server">

                <div class="panel panel-block">
                
                    <div class="panel-heading clearfix">
                        <h1 class="panel-title pull-left">
                            <i class="fa fa-users"></i>
                            <asp:Literal ID="lHeading" runat="server" Text="Group Members" />
                        </h1>
                    </div>

                    <div class="panel-body">
                        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

                        <Rock:NotificationBox ID="nbRoleWarning" runat="server" NotificationBoxType="Warning" Title="No roles!" Visible="false" />

                        <div class="grid grid-panel">
                            <Rock:GridFilter ID="rFilter" runat="server" OnDisplayFilterValue="rFilter_DisplayFilterValue" >
                                <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" />
                                <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" />
                                <Rock:Toggle ID="tglSubGroups" runat="server" Label="Show Sub Groups" />

                                <Rock:RockCheckBoxList ID="cblRole" runat="server" Label="Role" RepeatDirection="Horizontal" />
                                <Rock:RockCheckBoxList ID="cblStatus" runat="server" Label="Status" RepeatDirection="Horizontal" />
                            </Rock:GridFilter>
                            <Rock:Grid ID="gGroupMembers" runat="server" DisplayType="Full" AllowPaging="true" AllowSorting="true" 
                                OnRowSelected="gGroupMembers_Edit" OnRowDataBound="gGroupMembers_RowDataBound">
                                <Columns>
                                    <Rock:SelectField></Rock:SelectField>
                                    <asp:TemplateField HeaderText="Name" SortExpression="Person.LastName,Person.NickName">
                                        <ItemTemplate>
                                            <%#Eval("Person.FullName") %>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField ItemStyle-CssClass="grid-columncommand" HeaderStyle-CssClass="grid-columncommand">
                                        <ItemTemplate>
                                            <asp:HyperLink runat="server" ID="lnkProfile">
                                                <div class='btn btn-default'><i class='fa fa-user'></i></div>
                                            </asp:HyperLink>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Group" SortExpression="Group.Name">
                                        <ItemTemplate>
                                            <%#Eval("Group.Name") %>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Role" SortExpression="GroupRole.Name">
                                        <ItemTemplate>
                                            <%#Eval("GroupRole.Name") %>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Status" SortExpression="GroupMemberStatus">
                                        <ItemTemplate>
                                            <%#Eval("GroupMemberStatus") %>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </div>
                </div>
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
