<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SqlCommand.ascx.cs" Inherits="RockWeb.Blocks.Reporting.SqlCommand" %>

<asp:UpdatePanel ID="upReport" runat="server">
    <ContentTemplate>

        <div class="sql-comand">

            <div class="panel panel-default">
                <div class="panel-body">
                    <fieldset>
                        

                        <Rock:CodeEditor ID="tbQuery" runat="server" Label="SQL Text" Height="400" EditorMode="Sql" EditorTheme="Rock" Help="The SQL query or stored procedure name to execute.">
SELECT
    TOP 10 *
FROM
    person
                        </Rock:CodeEditor>

                        <Rock:Toggle ID="tQuery" runat="server" Label="Selection Query?" OnText="Yes" OffText="No" Checked="true"
                            Help="Will the SQL Text above return rows? If so, a grid will be displayed containing the results of the query." />
                    </fieldset>

                    <div class="actions">
                        <asp:LinkButton ID="btnExec" runat="server" Text="Run" CssClass="btn btn-primary" OnClick="btnExec_Click" />
                    </div>
                </div>

            

                <Rock:NotificationBox ID="nbSuccess" runat="server" Heading="Success" Title="Command run successfully!" NotificationBoxType="Success" Visible="false" />
                <Rock:NotificationBox ID="nbError" runat="server" Heading="Error" Title="SQL Error!" NotificationBoxType="Danger" Visible="false" />
                <Rock:Grid ID="gReport" runat="server" AllowSorting="true" EmptyDataText="No Results" Visible="false" />
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
