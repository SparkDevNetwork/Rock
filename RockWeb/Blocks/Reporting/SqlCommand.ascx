<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SqlCommand.ascx.cs" Inherits="RockWeb.Blocks.Reporting.SqlCommand" %>

<asp:UpdatePanel ID="upReport" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-exclamation-triangle"></i>SQL Command</h1>
            </div>

            <div class="panel-body">
                <fieldset>
                    <Rock:HiddenFieldWithClass ID="hfSelectedText" runat="server" CssClass="js-selected-text" />
                    <Rock:HiddenFieldWithClass ID="hfSelectionRange" runat="server" CssClass="js-selected-range" />
                    <Rock:CodeEditor ID="tbQuery" runat="server" Label="SQL Text" Height="400" EditorMode="Sql" CssClass="js-code-editor" EditorTheme="Rock" Help="The SQL query or stored procedure name to execute." />

                    <Rock:Toggle ID="tQuery" runat="server" Label="Selection Query?" OnText="Yes" OffText="No" Checked="true"
                        Help="Will the SQL Text above return rows? If so, a grid will be displayed containing the results of the query." />
                </fieldset>

                <div class="actions">
                    <asp:LinkButton ID="btnExec" runat="server" Text="Run" CssClass="btn btn-primary js-exec-btn" OnClick="btnExec_Click" />
                </div>

                <div class="margin-t-md">
                    <Rock:NotificationBox ID="nbSuccess" runat="server" Heading="Success" Title="Command completed successfully." NotificationBoxType="Success" Visible="false" />
                    <Rock:NotificationBox ID="nbError" runat="server" Heading="Error" Title="SQL Error" NotificationBoxType="Danger" Visible="false" />
                </div>
            </div>
            
            <div class="grid">
                <Rock:Grid ID="gReport" runat="server" AllowSorting="true" EmptyDataText="No Results" Visible="false" />
            </div>

            <p id="pQueryTime" runat="server" class="text-right margin-r-md" visible="false" />
        </div>

        <script>
            Sys.Application.add_load(function () {
                var $mainPanel = $('#<%=upReport.ClientID%>');
                var aceEditor = $mainPanel.find('.js-code-editor .ace_editor').data('aceEditor');

                // restore the selected range (if there was any)
                var selectionRangeJSON = $mainPanel.find('.js-selected-range').val();
                if (selectionRangeJSON) { 
                    var selectionRange = JSON.parse(selectionRangeJSON);
                    if (selectionRange) {
                        aceEditor.selection.setRange(selectionRange);
                    }
                }

                $('.js-exec-btn').click(function () {
                    // Save the selected 'range' - Ace stores this in a Range object
                    var selectedRangeJSON = JSON.stringify(aceEditor.getSelectionRange());
                    $selectionRange.val(selectedRangeJSON);
                    
                    // Set the selected text to our hidden field
                    var selectedText = aceEditor.getSelectedText();
                    $('.js-selected-text').val(selectedText);
                });
            })

        </script>

    </ContentTemplate>
</asp:UpdatePanel>