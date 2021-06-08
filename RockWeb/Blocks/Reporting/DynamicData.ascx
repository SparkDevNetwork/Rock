<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DynamicData.ascx.cs" Inherits="RockWeb.Blocks.Reporting.DynamicData" %>

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
                                <div class="col-md-6">
                                    <Rock:RockTextBox ID="tbDesc" runat="server" Label="Page Description" TextMode="MultiLine" Rows="1" CssClass="input-xlarge" Help="The current page's description" />
                                </div>
                                <div class="col-md-6">
                                </div>
                            </div>

                            <h3>Query Logic</h3>
                            <hr class="mt-0" />
                            <div class="row">
                                <div class="col-md-12">
                                    <Rock:CodeEditor ID="ceQuery" EditorHeight="212" EditorMode="Sql" EditorTheme="Rock" runat="server" Label="Query"
                                        Help="The SQL query or stored procedure name to execute.  If parameters are included they will also need to be in the Parameters field below.
                                            By default, a grid will be displayed showing all the rows and columns returned by the query.  However, if a 'Formatted Output' value is included below, the results will be formatted
                                            according to the 'Formatted Output' value." />
                                </div>
                            </div>

                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockCheckBox ID="cbStoredProcedure" runat="server" Label="Query is a Stored Procedure" Help="Provide only the name of the stored procedure in the Query field. The parameters (if any) for the stored procedure should be configured using the Parameters field." />
                                </div>
                                <div class="col-md-6">
                                    <div class="input-group">
                                        <label>Timeout</label>
                                        <div class="input-group input-width-md">
                                            <Rock:NumberBox ID="nbTimeout" CssClass="form-control" runat="server" /><span class="input-group-addon">(sec)</span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockTextBox ID="tbParams" runat="server" Label="Parameters" TextMode="MultiLine" Rows="1" CssClass="input-xlarge"
                                        Help="The parameters that the query expects in the format of 'param1=value;param2=value'. The equals sign must be provided for each parameter, but you don't have to provide a default value if you want it to default to blank.  Any parameter with the same name as a page parameter (i.e. querystring,
                                            form, or page route) will have its value replaced with the page's current value.  A parameter with the name of 'CurrentPersonId' will have its value replaced with the currently logged in person's id." />
                                </div>
                            </div>

                            <h3>Formatting</h3>
                            <hr class="mt-0" />
                            <div class="row">
                                <div class="col-md-6">
                                    <div class="mb-3">
                                        <div class="control-label">
                                            <asp:DropDownList ID="ddlHideShow" runat="server" CssClass="input-small">
                                                <asp:ListItem Text="Hide" Value="False"></asp:ListItem>
                                                <asp:ListItem Text="Show" Value="True"></asp:ListItem>
                                            </asp:DropDownList>
                                            Columns<Rock:HelpBlock ID="hbHideShow" runat="server" Text="If using 'Show Columns,' only the columns specified will be displayed.  If using 'Hide Columns,' all columns except the columns specified will be displayed." />
                                        </div>
                                        <div>
                                            <Rock:RockTextBox ID="tbColumns" runat="server" TextMode="MultiLine" Rows="1" CssClass="input-xlarge" />
                                        </div>
                                    </div>
                                    <Rock:RockTextBox ID="tbUrlMask" runat="server" Label="Selection URL" CssClass="input-large"
                                        Help="The URL to redirect user to when they click on a row in the grid.  Any column's value can be used in the URL by including it in braces.  For example if the grid includes an 'Id' column that contains Person Ids, you can link to the Person view, by specifying a value here of '~/Person/{Id}" />
                                    <Rock:Switch ID="cbShowGridFilter" runat="server" Text="Show Grid Filter" TextAlign="Right" />
                                    <Rock:Switch ID="swWrapInPanel" runat="server" Text="Wrap in Panel" CssClass="js-checkbox-wrap-in-panel" />
                                </div>
                                <div class="col-md-6">
                                    <div class="js-grid-options-container">
                                        <Rock:Switch ID="cbPersonReport" runat="server" Text="Person Report" CssClass="js-checkbox-person-report"
                                            Help="Does this query return a list of people? If it does, then additional options will be available from the result grid.  (i.e. Communicate, etc).  Note: A column named 'Id' that contains the person's Id is required for a person report." />
                                        <Rock:RockControlWrapper ID="rcwGridOptions" runat="server" Label="Show Grid Actions">
                                            <div class="margin-l-md">
                                                <Rock:RockCheckBox ID="cbShowCommunicate" runat="server" ContainerCssClass="js-checkbox-person-grid-action" Text="Communicate" />
                                                <Rock:RockCheckBox ID="cbShowMergePerson" runat="server" ContainerCssClass="js-checkbox-person-grid-action" Text="Merge Person" />
                                                <Rock:RockCheckBox ID="cbShowBulkUpdate" runat="server" ContainerCssClass="js-checkbox-person-grid-action" Text="Bulk Update" />
                                                <Rock:RockCheckBox ID="cbShowLaunchWorkflow" runat="server" ContainerCssClass="js-checkbox-person-grid-action" Text="Launch Workflow" />

                                                <Rock:RockCheckBox ID="cbShowExcelExport" runat="server" Text="Excel Export" />
                                                <Rock:RockCheckBox ID="cbShowMergeTemplate" runat="server" Text="Merge Template" />
                                            </div>
                                        </Rock:RockControlWrapper>
                                    </div>
                                </div>
                            </div>

                           <div class="row js-wrap-in-panel">
                                <div class="col-md-6">
                                    <Rock:RockTextBox ID="tbPanelTitle" runat="server" Label="Panel Title" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:RockTextBox ID="tbPanelIcon" runat="server" Label="Panel Icon CSS Class" />
                                </div>
                            </div>

                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:Switch ID="swLavaCustomization" runat="server" Text="Customize Results with Lava" CssClass="js-checkbox-lava-customization" />
                                </div>
                            </div>
                            <div class="row js-lava-customization">
                                <div class="col-md-12">
                                    <Rock:CodeEditor
                                        ID="ceFormattedOutput"
                                        runat="server"
                                        Label="Formatted Output"
                                        CssClass="input-large"
                                        EditorHeight="400"
                                        Help="Optional formatting to apply to the returned results.  If left blank, a grid will be displayed. Example: {% for row in rows %} {{ row.FirstName }} {% endfor %} or if the query returns multiple result sets: {% for row in table1.rows %} {{ row.FirstName }} {% endfor %} " />
                                </div>
                            </div>

                            <Rock:PanelWidget CssClass="mt-4" runat="server" ID="pwAdvancedSettings" Title="Advanced Settings">
                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:RockTextBox ID="tbMergeFields" runat="server" Label="Communication Merge Fields" TextMode="MultiLine" Rows="1" CssClass="input-xlarge"
                                            Help="When creating a new communication from a person report, additional fields from the report can be used as merge fields on the communication. Enter any column names that you'd like to be available for the communication. If the same recipient has multiple results in this report, each result will be included in an 'AdditionalFields' list. These can be accessed using Lava in the communication. For example: {% for field in AdditionalFields %}{{ field.columnName }}{% endfor %}" />

                                    </div>
                                    <div class="col-md-6">
                                        <Rock:RockTextBox ID="tbCommunicationRecipientPersonIdFields" runat="server" Label="Communication Recipient Fields"
                                            Help="The column name(s) that contain a person id field to use as the recipient for a communication. If left blank, it will assume a column named 'Id' contains the recipient's person Id." />
                                    </div>
                                    <div class="col-md-6">
                                        <Rock:RockTextBox ID="tbEncryptedFields" runat="server" Label="Encrypted Fields" TextMode="MultiLine" Rows="1" CssClass="input-xlarge"
                                            Help="Any fields that need to be decrypted before displaying their value." />
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-md-12">
                                        <Rock:CodeEditor ID="cePageTitleLava" runat="server" Label="Page Title Lava" EditorMode="Lava" CssClass="input-large" EditorHeight="200"
                                            Help="Optional Lava for setting the page title. If nothing is provided then the page's title will be used. Example '{{rows[0].FullName}}' or if the query returns multiple result sets '{{table1.rows[0].FullName}}'." />
                                    </div>
                                </div>
                            </Rock:PanelWidget>
                        </ContentTemplate>
                    </asp:UpdatePanel>

                </Content>
            </Rock:ModalDialog>
        </asp:Panel>
        <script type="text/javascript">
            function showHidePersonGridActions($personGridCheckbox) {
                $personGridCheckbox.each(function (index, element) {
                    if ($(element).is(':checked')) {
                        $('.js-checkbox-person-grid-action').show();
                    }
                    else {
                        $('.js-checkbox-person-grid-action').hide();
                    }
                })
            };

            function showHideItems(checkboxControl, controlsToHide) {
                if (checkboxControl && checkboxControl.is(':checked')) {
                    controlsToHide.show();
                } else {
                    controlsToHide.hide();
                }
            }

            Sys.Application.add_load(function () {
                showHidePersonGridActions($('.js-checkbox-person-report'));
                showHideItems($('.js-checkbox-wrap-in-panel'), $(".js-wrap-in-panel"));
                showHideItems($('.js-checkbox-lava-customization'), $(".js-lava-customization"));

                $('.js-checkbox-person-report').on('change', function () {
                    showHidePersonGridActions($(this));
                })

                $('.js-checkbox-wrap-in-panel').on('change', function () {
                    showHideItems($(this), $(".js-wrap-in-panel"));
                })

                $('.js-checkbox-lava-customization').on('change', function () {
                    showHideItems($(this), $(".js-lava-customization"));
                })
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
