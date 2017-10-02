﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupList.ascx.cs" Inherits="RockWeb.Blocks.Groups.GroupList" %>

<asp:UpdatePanel ID="upnlGroupList" runat="server">
    <ContentTemplate>
        
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i runat="server" id="iIcon"></i> <asp:Literal ID="lTitle" runat="server" Text="Group List" /></h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="gfSettings" runat="server" OnDisplayFilterValue="rFilter_DisplayFilterValue">
                        <Rock:GroupTypePicker ID="gtpGroupType" runat="server" Label="Group Type" />
                        <Rock:RockDropDownList ID="ddlGroupTypePurpose" runat="server" Label="Group Type Purpose"/>
                        <Rock:RockDropDownList ID="ddlActiveFilter" runat="server" Label="Active Status">
                            <asp:ListItem Text="[All]" Value="all"></asp:ListItem>
                            <asp:ListItem Text="Active" Value="active"></asp:ListItem>
                            <asp:ListItem Text="Inactive" Value="inactive"></asp:ListItem>
                        </Rock:RockDropDownList>
                    </Rock:GridFilter>
                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                    <Rock:Grid ID="gGroups" runat="server" RowItemText="Group" AllowSorting="true" OnRowSelected="gGroups_Edit">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:RockTemplateField HeaderText="Name" SortExpression="Name" Visible="false">
                                <ItemTemplate>
                                    <%# Eval("Name") %><br /><small><%# Eval("Path") %></small>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockBoundField DataField="GroupTypeName" HeaderText="Group Type" SortExpression="GroupTypeName"/>
                            <Rock:RockBoundField DataField="Description" HeaderText="Description" ColumnPriority="Desktop" SortExpression="Description" />
                            <Rock:RockBoundField DataField="GroupRole" HeaderText="Role" SortExpression="Role" />
                            <Rock:RockBoundField DataField="MemberCount" HeaderText="Members" SortExpression="MemberCount" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                            <Rock:DateTimeField DataField="DateAdded" HeaderText="Added" SortExpression="DateAdded" FormatAsElapsedTime="true" />
                            <Rock:BoolField DataField="IsSystem" HeaderText="System" SortExpression="IsSystem" />
                            <Rock:BoolField DataField="IsActive" HeaderText="Active" SortExpression="IsActiveOrder" />
                            <Rock:DeleteField OnClick="gGroups_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>
          <Rock:ModalDialog ID="modalDetails" runat="server" Title="Add to Group" ValidationGroup="GroupName">
            <Content>
                <div class="row">
                    <div class="col-md-4">
                        <Rock:RockDropDownList ID="ddlGroup" runat="server" Label="Group" DataTextField="Name" DataValueField="Id" ValidationGroup="GroupName" EnhanceForLongLists="true" />
                    </div>
                </div>

            </Content>
        </Rock:ModalDialog>

        <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Danger" Visible="false" />
    </ContentTemplate>
</asp:UpdatePanel>
