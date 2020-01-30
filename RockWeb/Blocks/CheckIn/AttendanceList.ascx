<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AttendanceList.ascx.cs" Inherits="RockWeb.Blocks.Checkin.AttendanceList" %>

<asp:UpdatePanel ID="upAttendanceList" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlContent" CssClass="panel panel-block" runat="server">

            <Rock:ModalAlert ID="mdGridWarning" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-check-square"></i> Attendance List</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="rFilter" runat="server">
                        <Rock:RockDropDownList ID="ddlDidAttend" runat="server" Label="Attended">
                            <asp:ListItem Text="[All]" Value=""></asp:ListItem>
                            <asp:ListItem Text="Did Attend" Value="1"></asp:ListItem>
                            <asp:ListItem Text="Did Not Attend" Value="0"></asp:ListItem>
                        </Rock:RockDropDownList>
                        <Rock:PersonPicker ID="ppEnteredBy" runat="server" Label="Entered By" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="gAttendees" runat="server" DataKeyNames="Id" DisplayType="Full" AllowSorting="true" EmptyDataText="No Attendance Found">
                        <Columns>
                            <asp:BoundField DataField="PersonAlias.Person.FullNameReversed" HeaderText="Name" />
                            <Rock:BoolField DataField="DidAttend" HeaderText="Attended" SortExpression="DidAttend" />
                            <Rock:RockBoundField DataField="Note" HeaderText="Note" HtmlEncode="false" SortExpression="Note"/>
                            <asp:BoundField DataField="CreatedByPersonName" HeaderText="Entered By" />
                            <Rock:DateTimeField DataField="CreatedOn" HeaderText="Entered On" SortExpression="CreatedOn" />
                            <Rock:DeleteField OnClick="gAttendeesDelete_Click"></Rock:DeleteField>
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>              

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
