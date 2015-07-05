<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupRequirementTypeDetail.ascx.cs" Inherits="RockWeb.Blocks.Groups.GroupRequirementTypeDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server" CssClass="panel panel-block">

            <asp:HiddenField ID="hfGroupRequirementTypeId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-check-square-o"></i>
                    <asp:Literal ID="lActionTitle" runat="server" /></h1>

                <div class="panel-labels">
                    
                </div>
            </div>
            <div class="panel-body">

                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

                <fieldset>

                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.GroupRequirementType, Rock" PropertyName="Name" />
                        </div>
                        <div class="col-md-6">
                        </div>
                    </div>

                    <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.GroupRequirementType, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbPositiveLabel" runat="server" SourceTypeName="Rock.Model.GroupRequirementType, Rock" PropertyName="PositiveLabel" Help="The text that is displayed when the requirement is met."/>
                            <Rock:DataTextBox ID="tbNegativeLabel" runat="server" SourceTypeName="Rock.Model.GroupRequirementType, Rock" PropertyName="NegativeLabel" Help="The text that is displayed when the requirement is not met." />
                            <Rock:DataTextBox ID="tbWarningLabel" runat="server" SourceTypeName="Rock.Model.GroupRequirementType, Rock" PropertyName="WarningLabel" Help="The text that is displayed when the requirement in a warning state." />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbCanExpire" runat="server" Label="Can Expire" CssClass="js-can-expire-checkbox" />
                            <div class="js-can-expire-days">
                                <Rock:NumberBox ID="nbExpireInDays" runat="server" Label="Expire in Days" Help="The number of days after the requirement is met before it expires (If CanExpire is true). Leave blank if it never expires." />
                            </div>
                        </div>
                    </div>

                    <Rock:RockControlWrapper ID="rcwRequirementCheckType" runat="server" Label="Check Type">
                        <div class="controls">
                            <div class="js-requirement-check-type">
                                <Rock:HiddenFieldWithClass ID="hfRequirementCheckType" CssClass="js-hidden-selected" runat="server" />
                                <div class="btn-group">
                                    <asp:HyperLink ID="btnRequirementCheckTypeSQL" runat="server" CssClass="btn btn-default active" Text="SQL" data-val="0" />
                                    <asp:HyperLink ID="btnRequirementCheckTypeDataview" runat="server" CssClass="btn btn-default" Text="Dataview" data-val="1" />
                                    <asp:HyperLink ID="btnRequirementCheckTypeManual" runat="server" CssClass="btn btn-default" Text="Manual" data-val="2" />
                                </div>
                            </div>
                        </div>
                    </Rock:RockControlWrapper>

                    <div class="row">
                        <div class="col-md-12">
                            <div class="js-dataview-mode-div">
                                <Rock:DataViewPicker ID="dpDataView" runat="server" Label="Dataview" Help="The dataview that will return a list of people that meet the criteria." />
                                <Rock:DataViewPicker ID="dpWarningDataView" runat="server" Label="Warning Dataview" Help="Optional dataview that will return a list of people that should be marked as in a warning status." />
                            </div>
                            <div class="js-sql-mode-div">
                                <Rock:CodeEditor ID="ceSqlExpression" runat="server" Label="SQL Expression" Help="A SQL expression that returns a list of Person Ids that meet the criteria." EditorMode="Sql" />
                                <Rock:CodeEditor ID="ceWarningSqlExpression" runat="server" Label="Warning SQL Expression" Help="Optional SQL expression that returns a list of Person Ids that should be marked as in a warning status." EditorMode="Sql" />
                            </div>
                            <div class="js-manual-mode-div">
                                <Rock:DataTextBox ID="tbCheckboxLabel" runat="server" SourceTypeName="Rock.Model.GroupRequirementType, Rock" PropertyName="CheckboxLabel" Help="The label that is used for the checkbox when the requirement is manually set." />
                            </div>
                        </div>
                    </div>

                </fieldset>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </div>

        </asp:Panel>

        <script>
            function setActiveButtonGroupButton($activeBtn) {
                $activeBtn.addClass('active');
                $activeBtn.siblings('.btn').removeClass('active');

                var requirementCheckType = $activeBtn.data('val');

                // set hidden field of what requirementchecktype mode we are in
                $activeBtn.closest('.btn-group').siblings('.js-hidden-selected').val(requirementCheckType);

                // Sql
                if (requirementCheckType == '0') {
                    $('.js-sql-mode-div').show();
                }
                else {
                    $('.js-sql-mode-div').hide();
                }

                // Dataview
                if (requirementCheckType == '1') {
                    $('.js-dataview-mode-div').show();
                }
                else {
                    $('.js-dataview-mode-div').hide();
                }

                // Manual
                if (requirementCheckType == '2') {
                    $('.js-manual-mode-div').show();
                }
                else {
                    $('.js-manual-mode-div').hide();
                }
            }

            function setExpireDaysVisibility() {
                var checked = $('.js-can-expire-checkbox').is(':checked');
                if (checked) {
                    $('.js-can-expire-days').show();
                } else {
                    $('.js-can-expire-days').hide();
                }
            }

            Sys.Application.add_load(function () {
                $('.js-requirement-check-type .btn').on('click', function (e) {
                    setActiveButtonGroupButton($(this));
                });

                setActiveButtonGroupButton($('.js-requirement-check-type').find("[data-val='" + $('.js-requirement-check-type .js-hidden-selected').val() + "']"));

                $('.js-can-expire-checkbox').change(setExpireDaysVisibility);

                setExpireDaysVisibility();
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
