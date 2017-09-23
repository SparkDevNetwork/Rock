﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CommunicationRecipientList.ascx.cs" Inherits="RockWeb.Blocks.Communication.CommunicationRecipientList" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-comments-o"></i> Communication List</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="rFilter" runat="server">
                        <Rock:RockTextBox ID="tbSubject" runat="server" Label="Subject" />
                        <Rock:RockDropDownList ID="ddlType" runat="server" Label="Communication Type" />
                        <Rock:RockDropDownList ID="ddlStatus" runat="server" Label="Status" />
                        <Rock:PersonPicker ID="ppSender" runat="server" Label="Created By" />
                        <Rock:DateRangePicker ID="drpDates" runat="server" Label="Date Range" />
                        <Rock:RockTextBox ID="tbContent" runat="server" Label="Content" />
                    </Rock:GridFilter>

                    <Rock:Grid ID="gCommunication" runat="server" AllowSorting="true" >
                        <Columns>
                            <Rock:RockBoundField DataField="Subject" SortExpression="Subject" HeaderText="Subject" />
                            <Rock:EnumField DataField="CommunicationType" SortExpression="CommunicationType" HeaderText="Type" />
                            <Rock:EnumField DataField="Status" SortExpression="Status" HeaderText="Status" />
                            <Rock:DateTimeField DataField="CreatedDateTime" SortExpression="CreatedDateTime" ColumnPriority="DesktopLarge" HeaderText="Created" />
                            <Rock:RockBoundField DataField="SenderPersonAlias.Person.FullName" HeaderText="Created By" SortExpression="SenderPersonAlias.Person.LastName,SenderPersonAlias.Person.NickName" />
                            <Rock:LinkButtonField CssClass="btn btn-default btn-sm fa fa-file-text-o" OnClick="gCommunication_RowSelected" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

    </ContentTemplate>
</asp:UpdatePanel>
