<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ReportDetail.ascx.cs" Inherits="RockWeb.Blocks.Reporting.ReportDetail" %>
<asp:UpdatePanel ID="upReport" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server" Visible="false">
            <asp:HiddenField ID="hfReportId" runat="server" />

                <div id="pnlEditDetails" runat="server">
                    <div class="panel panel-block">
                        <div class="panel-heading">
                            <h1 class="panel-title"><i class="fa fa-list-alt"></i> <asp:Literal ID="lActionTitle" runat="server" /></h1>
                        </div>
                        <div class="panel-body">

                            <asp:ValidationSummary ID="vsDetails" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.Report, Rock" PropertyName="Name" Label="Name" />
                                </div>
                                <div class="col-md-6">
                                </div>
                            </div>

                            <div class="row">
                                <div class="col-md-12">
                                    <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.Report, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                                </div>
                            </div>

                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:CategoryPicker ID="cpCategory" runat="server" Required="true" EntityTypeName="Rock.Model.Report" Label="Category" />
                                    <Rock:EntityTypePicker ID="etpEntityType" runat="server" Label="Applies To" Required="true" AutoPostBack="true" OnSelectedIndexChanged="etpEntityType_SelectedIndexChanged"/>
                                    <Rock:DataViewPicker ID="ddlDataView" runat="server" Label="Data View" Required="false" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:KeyValueList ID="kvSortFields" runat="server" Label="Sorting" />
                                </div>
                            </div>

                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:NumberBox ID="nbFetchTop" runat="server" NumberType="Integer" Required="false" Label="Resulting Row Limit" MinimumValue="0" MaxLength="9"
                                        Help="Limits the number of rows returned in the report. Leave blank to show all rows." />
                                </div>
                                <div class="col-md-6">
                                </div>

                            </div>

                            <section class="panel panel-widget">
                                <header class="panel-heading clearfix">
                                    <div class="pull-left">
                                        <h3 class="panel-title margin-t-sm">
                                            <span>Fields</span>
                                        </h3>
                                    </div>
                                    <div class="pull-right">
                                        <div class="btn-group btn-group-xs pull-right">
                                            <asp:LinkButton runat="server" ID="btnAddField" CssClass="btn btn-action" CausesValidation="false" OnClick="btnAddField_Click"><i class="fa fa-plus"></i> Add Field</asp:LinkButton></td>
                                        </div>
                                    </div>
                                </header>
                                <div class="panel-body panel-widget-sort-container">
                                    <asp:PlaceHolder runat="server" ID="phReportFields" ViewStateMode="Disabled" />
                                </div>
                            </section>

                            <div class="actions">
                                <asp:LinkButton ID="btnSave" runat="server" Text="Save" AccessKey="s" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" AccessKey="c" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                            </div>
                        </div>
                    </div>
                </div>

                <div id="pnlViewDetails" runat="server">
                    <div class="panel panel-block">
                        <div class="panel-heading">
                            <h1 class="panel-title"><i class="fa fa-list-alt"></i> <asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>
                        </div>
                        <div class="panel-body">
                            <p class="description">
                                <asp:Literal ID="lReportDescription" runat="server"></asp:Literal>
                            </p>

                            <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                            <div class="row">
                                <asp:Literal ID="lblMainDetails" runat="server" />
                            </div>

                            <div class="actions">
                                <asp:LinkButton ID="btnEdit" runat="server" AccessKey="m" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" />
                                <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                                <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" />
                                <Rock:SecurityButton ID="btnSecurity" runat="server" class="btn btn-sm btn-security pull-right" />
                            </div>
                        </div>
                    </div>
                    
                    <div class="panel panel-block">
                        <div class="panel-heading">
                            <h1 class="panel-title"><i class="fa fa-table"></i> Report Data</h1>
                        </div>
                        <div class="panel-body">
                            <div class="grid grid-panel"><Rock:Grid ID="gReport" runat="server" AllowSorting="true" EmptyDataText="No Results" /></div>
                        </div>
                    </div>

            </div>

        </asp:Panel>
        <script>

            Sys.Application.add_load(function () {
                var fixHelper = function (e, ui) {
                    ui.children().each(function () {
                        $(this).width($(this).width());
                    });
                    return ui;
                };

                // javascript to make the Reorder buttons work on the panel-widget controls
                $('.panel-widget-sort-container').sortable({
                    helper: fixHelper,
                    handle: '.panel-widget-reorder',
                    containment: 'parent',
                    tolerance: 'pointer',
                    start: function (event, ui) {
                        {
                            var start_pos = ui.item.index();
                            ui.item.data('start_pos', start_pos);
                        }
                    },
                    update: function (event, ui) {
                        {
                            $('#' + '<%=btnSave.ClientID %>').addClass('disabled');
                            var newItemIndex = $(ui.item).prevAll('.panel-widget').length;
                            __doPostBack('<%=upReport.ClientID %>', 're-order-panel-widget:' + ui.item.attr('id') + ';' + newItemIndex);
                        }
                    }
                });

                function htmlEncode(value) {
                    // from http://stackoverflow.com/questions/1219860/html-encoding-in-javascript-jquery
                    // create a in-memory div, set it's inner text(which jQuery automatically encodes)
                    // then grab the encoded contents back out.  The div never exists on the page.
                    return $('<div/>').text(value).html();
                }


                // javascript to set the widget panel title based on the defined column header text when collapsed
                $('.panel-widget .panel-heading').on('click', function (e, data) {
                    if ($(this).find('.fa-chevron-down').length) {
                        var title = $(this).closest('section').find('.js-column-header-textbox').val();
                        var reportFieldGuid = $(this).closest('section').find('.js-report-field-guid').val();

                        // set hidden value of title
                        $(this).find('.js-header-title-hidden').val(htmlEncode(title));

                        // set displayed text of title
                        $(this).find('.js-header-title').text(title);
                        
                        // update displayed sorting field names to match updated title
                        var $kvSortFields = $('#<%=kvSortFields.ClientID %>');
                        $kvSortFields.find('.key-value-key').find('option[value="' + reportFieldGuid + '"]').text(title);

                        // update the HTML for when the next sorting field is added
                        var valueHtml = $kvSortFields.find('.js-value-html').val();
                        var $fakeDiv = $('<div/>').append(valueHtml);
                        $fakeDiv.find('.key-value-key').find('option[value="' + reportFieldGuid + '"]').text(title);
                        var updatedValueHtml = $fakeDiv.html();
                        $kvSortFields.find('.js-value-html').val(updatedValueHtml);
                    }
                })

            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
