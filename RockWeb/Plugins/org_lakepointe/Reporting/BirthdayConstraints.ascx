<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BirthdayConstraints.ascx.cs" Inherits="RockWeb.Plugins.org_lakepointe.Reporting.BirthdayConstraints" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">
            <div class="panel-body">
                <form>
                    <div class="form-row">
                        <div class="form-group col-md-1">
                            <Rock:ButtonDropDownList ID="bddlMonth" runat="server" Label="Month" />
                        </div>
                        <div class="form-group col-md-3">
                            <Rock:GroupPicker ID="gpGroups" runat="server" Label="Groups" AllowMultiSelect="true" Required="true" />
                        </div>
                        <div class="form-group col-md-3">
                            <Rock:GroupPicker ID="gpSaturdayGroups" runat="server" Label="Saturday Groups" Help="Select top-level group for Saturday" />
                        </div>
                        <div class="form-group col-md-3">
                            <Rock:GroupPicker ID="gpSundayGroups" runat="server" Label="Sunday Groups" Help="Select top-level group for Sunday"/>
                        </div>
                    </div>
                    <div class="form-row">
                        <div class="form-group col-md-6">
                            <Rock:RockCheckBoxList ID="cblGroupOptions" runat="server" Label="Selection Constraints" RepeatDirection="Horizontal">
                            </Rock:RockCheckBoxList>
                        </div>
                    </div>
                </form>
                <Rock:BootstrapButton ID="lbGenerate" runat="server" Text="Generate Report" CssClass="btn btn-primary" OnClick="LbGenerate_Click"
                    DataLoadingText="&lt;i class='fa fa-refresh fa-spin'&gt;&lt;/i&gt; Thinking" />
                <br />
                <Rock:Grid ID="grData" runat="server" AllowSorting="true" RowItemText="Person" ExportSource="ColumnOutput" ExportFilename="Birthdays">
                    <Columns>
                        <asp:BoundField HeaderText="Name" DataField="Name" SortExpression="Name" />
                        <asp:BoundField HeaderText="Birth Month" DataField="BirthMonth" SortExpression="BirthMonth" />
                        <asp:BoundField HeaderText="Birth Day" DataField="BirthDay" SortExpression="BirthDay" />
                        <asp:BoundField HeaderText="Age" DataField="Age" SortExpression="Age" />
                        <asp:BoundField HeaderText="Grade" DataField="Grade" SortExpression="Grade" />
                        <asp:BoundField HeaderText="Most Recent Attendance" DataField="MostRecentAttendance" SortExpression="MostRecentAttendance" />
                        <asp:BoundField HeaderText="Sunday Groups" DataField="Sunday" SortExpression="Sunday" />
                        <asp:BoundField HeaderText="Saturday Groups" DataField="Saturday" SortExpression="Saturday" />
                    </Columns>
                </Rock:Grid>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>