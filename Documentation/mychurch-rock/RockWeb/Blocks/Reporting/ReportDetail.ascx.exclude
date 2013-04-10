<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ReportDetail.ascx.cs" Inherits="RockWeb.Blocks.Reporting.ReportDetail" %>
<script>
    Sys.Application.add_load(function () {
        $('div.report-filter-caption').click(function () {
            $(this).slideToggle();
            $(this).next('div').slideToggle();
        });
    });
</script>
<asp:UpdatePanel ID="upReporting" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">

            <div class="row-fluid">
                <div class="span3 well">

                    <fieldset>
                        <legend>Details</legend>
                        <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.Report, Rock" PropertyName="Name" CssClass="" />
                        <Rock:DataTextBox  ID="tbDescription" runat="server" SourceTypeName="Rock.Model.Report, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                    </fieldset>

                </div>

                <div class="span9 well">

                    <fieldset>
                        <legend>Filter</legend>
                        <asp:PlaceHolder ID="phFilters" runat="server"></asp:PlaceHolder>
                    </fieldset>

                </div>
            </div>

            <div class="actions">
                <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>
        
            <div>
                <asp:Label runat="server" ID="lblGridTitle" Text="Results" />
                <Rock:Grid ID="gResults" runat="server">
                    <Columns>
                        <asp:BoundField DataField="GivenName" HeaderText="Given Name" SortExpression="GivenName" />
                        <asp:BoundField DataField="LastName" HeaderText="Last Name" SortExpression="LastName" />
                        <asp:BoundField DataField="Gender" HeaderText="Gender" SortExpression="Gender" />
                        <asp:BoundField DataField="Email" HeaderText="Email" SortExpression="Email" />
                    </Columns>
                </Rock:Grid>
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
