<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DataViewDetail.ascx.cs" Inherits="RockWeb.Blocks.Reporting.DataViewDetail" %>
<asp:UpdatePanel ID="upDataViewDetail" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server" Visible="false">
            <asp:HiddenField ID="hfDataViewId" runat="server" />

            <div id="pnlEditDetails" class="panel panel-block" runat="server">

                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-filter"></i> <asp:Literal ID="lActionTitle" runat="server" /></h1>

                    <div class="panel-labels">
                        <Rock:HighlightLabel ID="hlblEditDataViewId" runat="server" />
                        <div class="label label-primary"><asp:LinkButton ID="lbCreateReport" runat="server" OnClick="lbCreateReport_Click" ><i class="fa fa-plus"></i> Create Report</asp:LinkButton></div>
                    </div>
                </div>
                <div class="panel-body">

                    <asp:ValidationSummary ID="vsDetails" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                    <asp:CustomValidator ID="cvSecurityError" runat="server" Display="None"></asp:CustomValidator>

                    <fieldset>
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.DataView, Rock" PropertyName="Name" AutoPostBack="true" OnTextChanged="tbName_TextChanged" CssClass="" />
                                <Rock:RockCheckBox ID="cbDisableUseOfReadOnlyContext" runat="server" Visible="false" Label="Disable Use Of Read Only Context" Help="Enabling this option will cause the data view to run with a normal database context instead of using a read-only context. This may be needed if the results of the data view will be used for updating data, or for specific plugin filters that update data during the query process." />
                            </div>
                            <div class="col-md-6">
                                <Rock:CategoryPicker ID="cpCategory" runat="server" EntityTypeName="Rock.Model.DataView" Label="Category" Required="true" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-12">
                                <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.DataView, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:EntityTypePicker ID="etpEntityType" runat="server" Label="Applies To" OnSelectedIndexChanged="etpEntityType_SelectedIndexChanged" AutoPostBack="true" EnhanceForLongLists="true" Required="true" />
                                <Rock:RockDropDownList ID="ddlTransform" runat="server" Label="Post-filter Transformation" />
                            </div>
                            <div class="col-md-6">
                                <Rock:Toggle ID="tglIncludeDeceased" runat="server" Label="Include Deceased" OnText="Yes" OffText="No" Visible="false" />
                            </div>
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:DataTextBox ID="tbIconCssClass" runat="server" SourceTypeName="Rock.Model.DataView, Rock" PropertyName="IconCssClass" Label="Icon CSS Class"
                                        Help="The Font Awesome icon class to use when an Entity is returned by this DataView." />
                                    <Rock:ColorPicker ID="cpIconColor" runat="server" Label="Highlight Color"
                                        Help="The highlight color of the icon in the Icon CSS Class." />
                                 </div>
                            </div>
                        </div>

                        <%-- Persistence Schedule Settings --%>
                        <asp:UpdatePanel runat="server" UpdateMode="Conditional" class="panel panel-waterfall">
                            <ContentTemplate>
                                <div class="panel-heading">
                                    <Rock:Switch ID="swPersistDataView" runat="server" CssClass="form-group" Text="Persistence Schedule" BoldText="true" AutoPostBack="true" OnCheckedChanged="swPersistDataView_CheckedChanged" Help="Persisting this dataview may improve performance, especially for complex filters. The results of a persisted dataview are stored and re-used until the scheduled interval has elapsed." />
                                </div>

                                <asp:Panel runat="server" ID="pnlPersistenceSchedule" class="panel-body">
                                    <div class="row">
                                        <div class="col-xs-12 col-md-3 show-divider">
                                            <Rock:RockRadioButtonList ID="rblPersistenceType" runat="server" Label="Type"
                                                OnSelectedIndexChanged="rblPersistenceType_SelectedIndexChanged" AutoPostBack="true">
                                                <asp:ListItem Text="Interval" Value="0"></asp:ListItem>
                                                <asp:ListItem Text="Schedule" Value="1"></asp:ListItem>
                                            </Rock:RockRadioButtonList>
                                        </div>
                                        
                                        <div id="divPersistenceSchedule" runat="server" class="col-xs-12 col-md-3" Visible="false">
                                            <Rock:RockRadioButtonList ID="rblPersistenceSchedule" runat="server" Label="Persistence Schedule"
                                                OnSelectedIndexChanged="rblPersistenceSchedule_SelectedIndexChanged" AutoPostBack="true">
                                                <asp:ListItem Text="Unique" Value="0"></asp:ListItem>
                                                <asp:ListItem Text="Named Schedule" Value="1"></asp:ListItem>
                                            </Rock:RockRadioButtonList>
                                        </div>
                                        <div id="divScheduleSelect" runat="server" class="col-xs-12 col-md-6" Visible="false">
                                            <Rock:RockDropDownList ID="ddlNamedSchedule" runat="server" Label="Named" Required="true" RequiredErrorMessage="A Named Schedule is required when Persistence is 'Named Schedule'" Visible="false" />
                                            <Rock:ScheduleBuilder ID="sbUniqueSchedule" runat="server" Label="Unique" ShowScheduleFriendlyTextAsToolTip="true" Visible="false" />
                                        </div>
                                        <asp:Panel runat="server" ID="pnlScheduleIntervalControls" class="col-xs-12 col-md-9" Visible="false">
                                            <Rock:IntervalPicker ID="ipPersistedScheduleInterval" runat="server" Label="Persistence Interval" DefaultValue="12" DefaultInterval="Hour" />
                                        </asp:Panel>
                                    </div>
                                </asp:Panel>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </fieldset>


                    <Rock:NotificationBox ID="nbFiltersError" runat="server" NotificationBoxType="Danger" Visible="false" />

                    <asp:PlaceHolder ID="phFilters" runat="server"></asp:PlaceHolder>

                    <Rock:NotificationBox ID="nbPersistError" runat="server" NotificationBoxType="Warning" />

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                        <asp:LinkButton ID="btnPreview" runat="server" Text="Preview" CssClass="btn btn-default pull-right" CausesValidation="false" OnClick="btnPreview_Click" />
                    </div>

                </div>

            </div>

            <div id="pnlViewDetails" runat="server">
                <div class="panel panel-block" runat="server">
                    <div class="panel-heading">
                        <h1 class="panel-title"><i class="fa fa-filter"></i> <asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>
                        <div class="panel-labels">
                             <Rock:HighlightLabel ID="hlblDataViewId" runat="server" />
                             <Rock:HighlightLabel ID="hlblPersisted" LabelType="Info" Text="Persisted" Visible="false" runat="server" />
                             <Rock:HighlightLabel ID="hlblReadOnlyContext" LabelType="Warning" runat="server" Text="Read Only Context Disabled" Visible="false" />
                            <div class="label label-primary"><asp:LinkButton ID="lbViewCreateReport" runat="server" OnClick="lbCreateReport_Click" ><i class="fa fa-plus"></i> Create Report</asp:LinkButton></div>
                        </div>
                    </div>
                    <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
                    <div class="panel-body">

                        <fieldset>
                            <div class="clearfix">
                                <div class="pull-right ml-1 mb-1">
                                    <Rock:HighlightLabel runat="server" ID="hlTimeToRun" />
                                    <Rock:HighlightLabel runat="server" ID="hlRunSince" CustomClass="" />

                                    <Rock:BootstrapButton CssClass="btn btn-default btn-label" ID="lbResetRunCount" runat="server" OnClick="lbResetRunCount_Click" ToolTip="Reset Counter" ><i class="fa fa-undo"></i></Rock:BootstrapButton>

                                    <Rock:HighlightLabel runat="server" ID="hlLastRun" />
                                </div>

                                <div class="description">
                                    <asp:Literal ID="lDescription" runat="server"></asp:Literal>
                                </div>
                            </div>

                            <div class="row">
                                <div class="col-md-6">
                                    <asp:Literal ID="lblMainDetails" runat="server" />
                                </div>
                                <div class="col-md-6">
                                    <asp:Literal ID="lFilters" runat="server" />
                                    <asp:Literal ID="lPersisted" runat="server" />
                                    <asp:Literal ID="lDataViews" runat="server" />
                                    <asp:Literal ID="lReports" runat="server" />
                                    <asp:Literal ID="lGroups" runat="server" />
                                </div>
                            </div>

                            <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Warning" />

                            <div class="actions">
                                <asp:LinkButton ID="btnEdit" runat="server" AccessKey="m" ToolTip="Alt+m" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" />
                                <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" />
                                <div class="pull-right">
                                    <asp:LinkButton ID="btnCopy" runat="server" Text="<i class='fa fa-clone'></i>" Tooltip="Copy Data View" CssClass="btn btn-default btn-square btn-sm" OnClick="btnCopy_Click" />
                                    <Rock:SecurityButton ID="btnSecurity" runat="server" class="btn btn-sm btn-square btn-security" />
                                </div>
                                <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                            </div>

                        </fieldset>
                    </div>

                </div>
            </div>

            <Rock:ModalDialog ID="modalPreview" runat="server" Title="Preview (top 15 rows )" ValidationGroup="Preview">
                <Content>
                    <Rock:NotificationBox ID="nbPreviewError" runat="server" NotificationBoxType="Warning" />
                    <div class="grid"><Rock:Grid ID="gPreview" runat="server" AllowSorting="true" EmptyDataText="No Results" ShowActionRow="false" DisplayType="Light" /></div>
                </Content>
            </Rock:ModalDialog>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
