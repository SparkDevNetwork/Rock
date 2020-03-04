﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DynamicData.ascx.cs" Inherits="RockWeb.Blocks.Reporting.DynamicData" %>

<asp:UpdatePanel ID="upnlContent" runat="server" ChildrenAsTriggers="false" UpdateMode="Conditional">
    <ContentTemplate>

        <%-- View Panel --%>
        <asp:Panel ID="pnlView" runat="server">
            <Rock:NotificationBox ID="nbError" runat="server" Title="Query Error!" NotificationBoxType="Danger" Visible="false" />
            <asp:PlaceHolder ID="phContent" runat="server" Visible="false" />
        </asp:Panel>

        <%-- Edit Panel --%>
        <asp:Panel ID="pnlEditModel" runat="server" Visible="false">
            <Rock:ModalDialog ID="mdEdit" runat="server" OnSaveClick="lbSave_Click" Title="Dynamic Data Block">
                <Content>

                    <asp:UpdatePanel runat="server" ID="upnlEdit">
                        <ContentTemplate>

                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockTextBox ID="tbName" runat="server" Label="Page Name" CssClass="input-large" Help="The current page's title" />
                                </div>
                                <div class="col-md-6">
                                </div>
                            </div>

                            <div class="row">
                                <div class="col-md-12">
                                    <Rock:RockTextBox ID="tbDesc" runat="server" Label="Page Description" TextMode="MultiLine" Rows="1" CssClass="input-xlarge" Help="The current page's description" />
                                    <Rock:CodeEditor ID="ceQuery" EditorHeight="200" EditorMode="Sql" EditorTheme="Rock" runat="server" Label="Query"
                                        Help="The SQL query or stored procedure name to execute.  If parameters are included below, this should be the name of a stored procedure, otherwise it can be any SQL text.
                                            By default, a grid will be displayed showing all the rows and columns returned by the query.  However, if a 'Formatted Ouput' value is included below, the results will be formatted
                                            according to the 'Formatted Ouput' value." />
                                    <Rock:RockCheckBox ID="cbStoredProcedure" runat="server" Label="Query is a Stored Procedure" Text="Yes" />

                                </div>
                            </div>

                            <div class="row">
                                <div class="col-md-6">
                                    <div class="form-group">
                                        <div class="control-label">
                                            <asp:DropDownList ID="ddlHideShow" runat="server" CssClass="input-small">
                                                <asp:ListItem Text="Hide" Value="False"></asp:ListItem>
                                                <asp:ListItem Text="Show" Value="True"></asp:ListItem>
                                            </asp:DropDownList>
                                            Columns<Rock:HelpBlock ID="hbHideShow" runat="server" Text="If using 'Show Columns,' only the columns specified will be displayed.  If using 'Hide Columns,' all columns except the columns specified will be displayed." />
                                        </div>
                                        <div class="controls">
                                            <Rock:RockTextBox ID="tbColumns" runat="server" TextMode="MultiLine" Rows="1" CssClass="input-xlarge" />
                                        </div>
                                    </div>
                                    <Rock:RockTextBox ID="tbParams" runat="server" Label="Parameters" TextMode="MultiLine" Rows="1" CssClass="input-xlarge"
                                        Help="The parameters that the stored procedure expects in the format of 'param1=value;param2=value'.  Any parameter with the same name as a page parameter (i.e. querystring,
                                            form, or page route) will have its value replaced with the page's current value.  A parameter with the name of 'CurrentPersonId' will have its value replaced with the currently logged in person's id." />
                                    <div class="js-grid-options-container">
                                        <Rock:RockCheckBox ID="cbPersonReport" runat="server" Text="Person Report" CssClass="js-checkbox-person-report"
                                            Help="Does this query return a list of people? If it does, then additional options will be available from the result grid.  (i.e. Communicate, etc).  Note: A column named 'Id' that contains the person's Id is required for a person report." />
                                        <Rock:RockControlWrapper ID="rcwGridOptions" runat="server" Label="Show Grid Actions">
                                            <div class="margin-l-md">
                                                <Rock:RockCheckBox ID="cbShowCommunicate" runat="server" ContainerCssClass="js-checkbox-person-grid-action" Text="Communicate" />
                                                <Rock:RockCheckBox ID="cbShowMergePerson" runat="server" ContainerCssClass="js-checkbox-person-grid-action" Text="Merge Person" />
                                                <Rock:RockCheckBox ID="cbShowBulkUpdate" runat="server" ContainerCssClass="js-checkbox-person-grid-action" Text="Bulk Update" />
                                                <Rock:RockCheckBox ID="cbShowExcelExport" runat="server" Text="Excel Export" />
                                                <Rock:RockCheckBox ID="cbShowMergeTemplate" runat="server" Text="Merge Template" />
                                            </div>
                                        </Rock:RockControlWrapper>
                                        <Rock:RockCheckBox ID="cbShowGridFilter" runat="server" Label="Show Grid Filter" Text="Yes" />
                                    </div>
                                </div>
                                <div class="col-md-6">
                                    <Rock:RockTextBox ID="tbUrlMask" runat="server" Label="Selection URL" CssClass="input-large"
                                        Help="The URL to redirect user to when they click on a row in the grid.  Any column's value can be used in the URL by including it in braces.  For example if the grid includes an 'Id' column that contains Person Ids, you can link to the Person view, by specifying a value here of '~/Person/{Id}" />

                                    <Rock:RockTextBox ID="tbMergeFields" runat="server" Label="Communication Merge Fields" TextMode="MultiLine" Rows="1" CssClass="input-xlarge"
                                        Help="When creating a new communication from a person report, additional fields from the report can be used as merge fields on the communication. Enter any column names that you'd like to be available for the communication. If the same recipient has multiple results in this report, each result will be included in an 'AdditionalFields' list. These can be accessed using Lava in the communication. For example: {% for field in AdditionalFields %}{{ field.columnName }}{% endfor %}" />

                                    <Rock:RockTextBox ID="tbCommunicationRecipientPersonIdFields" runat="server" Label="Communication Recipient Fields"
                                        Help="The column name(s) that contain a person id field to use as the recipient for a communication. If left blank, it will assume a column named 'Id' contains the recipient's person Id." />

                                    <Rock:NumberBox ID="nbTimeout" runat="server" Label="Timeout" Help="In seconds." />

                                    <Rock:RockTextBox ID="tbEncryptedFields" runat="server" Label="Encrypted Fields" TextMode="MultiLine" Rows="1" CssClass="input-xlarge"
                                        Help="Any fields that need to be decrypted before displaying their value." />

                                </div>

                                <div class="col-md-12">
                                    <Rock:CodeEditor ID="ceFormattedOutput" runat="server" Label="Formatted Output" CssClass="input-large" EditorHeight="400"
                                        Help="Optional formatting to apply to the returned results.  If left blank, a grid will be displayed. Example: {% for row in rows %} {{ row.FirstName }} {% endfor %} or if the query returns multiple result sets: {% for row in table1.rows %} {{ row.FirstName }} {% endfor %} " />
                                </div>

                                <div class="col-md-12">
                                    <Rock:CodeEditor ID="cePageTitleLava" runat="server" Label="Page Title Lava" EditorMode="Lava" CssClass="input-large" EditorHeight="200"
                                        Help="Optional Lava for setting the page title. If nothing is provided then the page's title will be used. Example '{{rows[0].FullName}}' or if the query returns multiple result sets '{{table1.rows[0].FullName}}'." />
                                </div>
                            </div>

                        </ContentTemplate>
                    </asp:UpdatePanel>

                </Content>
            </Rock:ModalDialog>
        </asp:Panel>
        <script type="text/javascript">
            function enablePersonGridActions($personGridCheckbox) {
                $personGridCheckbox.each(function (index, element) {
                    if ($(element).is(':checked')) {
                        $('.js-checkbox-person-grid-action').removeAttr('disabled');
                    }
                    else {
                        $('.js-checkbox-person-grid-action').attr('disabled', true);
                    }
                })
            };

            Sys.Application.add_load(function () {
                enablePersonGridActions($('.js-checkbox-person-report'));

                $('.js-checkbox-person-report').on('change', function () {
                    enablePersonGridActions($(this));
                })
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
