<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ReportDetail.ascx.cs" Inherits="RockWeb.Blocks.Reporting.ReportDetail" %>
<asp:UpdatePanel ID="upReport" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server" Visible="false">
            <asp:HiddenField ID="hfReportId" runat="server" />

            <div class="banner">
                <h1>
                    <asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>
            </div>

            <asp:ValidationSummary ID="vsDetails" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

            <div id="pnlEditDetails" runat="server">

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
                        <Rock:DataDropDownList ID="ddlEntityType" runat="server" SourceTypeName="Rock.Model.Report, Rock" PropertyName="EntityTypeId" DataTextField="FriendlyName" Label="Applies To" DataValueField="Id" AutoPostBack="true" OnSelectedIndexChanged="ddlEntityType_SelectedIndexChanged" />
                        <Rock:DataDropDownList ID="ddlDataView" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Model.DataView, Rock" PropertyName="Name" Label="Data View" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:NumberBox ID="nbFetchTop" runat="server" NumberType="Integer" Required="false" SourceTypeName="Rock.Model.Report, Rock" PropertyName="FetchTop" Label="Result Row Limit" MinimumValue="0" MaxLength="9"
                            Help="Limits the number of rows returned in the report. Leave blank to show all rows." />
                    </div>
                    <div class="col-md-6">
                    </div>
                </div>

                <section class="panel panel-widget">
                    <header class="panel-heading clearfix">
                        <div class="pull-left">
                            <h3 class="panel-title">
                                <span>Fields</span>
                            </h3>
                        </div>
                        <div class="pull-right">
                            <div class="form-control-group">
                                <asp:LinkButton runat="server" ID="btnAddField" CssClass="btn btn-primary btn-sm" Text="Add" CausesValidation="false" OnClick="btnAddField_Click" /></td>
                            </div>
                        </div>
                    </header>
                    <div class="panel-body panel-widget-sort-container">
                        <asp:PlaceHolder runat="server" ID="phReportFields" ViewStateMode="Disabled" />
                    </div>
                </section>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </div>

            <div id="pnlViewDetails" runat="server">

                <p class="description">
                    <asp:Literal ID="lReportDescription" runat="server"></asp:Literal>
                </p>

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <div class="row">
                    <asp:Literal ID="lblMainDetails" runat="server" />
                </div>

                <div class="actions">
                    <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-primary btn-sm" OnClick="btnEdit_Click" />
                    <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                    <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link btn-sm" OnClick="btnDelete_Click" />
                    <Rock:SecurityButton ID="btnSecurity" runat="server" class="btn btn-sm btn-action pull-right" />
                </div>

                <h4>Results</h4>
                <div class="grid"><Rock:Grid ID="gReport" runat="server" AllowSorting="true" EmptyDataText="No Results" /></div>

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

                // javascript to set the widget panel title based on the defined column header text when collapsed
                $('.panel-widget .panel-heading').on('click', function (e, data) {
                    if ($(this).find('.fa-chevron-down').length) {
                        var title = $(this).closest('section').find('.js-column-header-textbox').val();

                        // set hidden value of title
                        $(this).find('.js-header-title-hidden').val(title);

                        // set displayed text of title
                        $(this).find('.js-header-title').text(title);
                    }
                })

            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
