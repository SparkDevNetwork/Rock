<%@ Control Language="C#" AutoEventWireup="true" CodeFile="StreakTypeList.ascx.cs" Inherits="RockWeb.Blocks.Streaks.StreakTypeList" %>

<asp:UpdatePanel ID="upStreakTypeList" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-list-ol"></i>
                    Streak Types
                </h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="rFilter" runat="server" OnClearFilterClick="rFilter_ClearFilterClick">
                        <Rock:RockDropDownList ID="ddlActiveFilter" runat="server" Label="Active Status">
                            <asp:ListItem></asp:ListItem>
                            <asp:ListItem Text="Active" Value="Active"></asp:ListItem>
                            <asp:ListItem Text="Inactive" Value="Inactive"></asp:ListItem>
                        </Rock:RockDropDownList>
                    </Rock:GridFilter>
                    <Rock:Grid ID="gStreakTypes" runat="server" AllowSorting="true">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:BoolField DataField="IsActive" HeaderText="Active" SortExpression="IsActive" />
                            <Rock:RockBoundField DataField="OccurrenceFrequency" HeaderText="Frequency" SortExpression="OccurrenceFrequency" />
                            <Rock:DateTimeField HeaderText="Start Date" DataField="StartDate" SortExpression="StartDate" DataFormatString="{0:d}"  />
                            <Rock:RockBoundField DataField="EnrollmentCount" HeaderText="Enrollments" SortExpression="EnrollmentCount" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" />
                            <Rock:SecurityField TitleField="Name" />
                            <Rock:PersonProfileLinkField LinkedPageAttributeKey="PersonProfilePage" />
                            <Rock:DeleteField OnClick="gStreakTypes_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
