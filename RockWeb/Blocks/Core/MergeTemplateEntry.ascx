<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MergeTemplateEntry.ascx.cs" Inherits="RockWeb.Blocks.Core.MergeTemplateEntry" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <Triggers>
        <asp:PostBackTrigger ControlID="btnMerge" />
    </Triggers>
    <ContentTemplate>
        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

        <asp:Panel ID="pnlEntry" runat="server" CssClass="panel panel-block">

            <asp:HiddenField ID="hfEntitySetId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-files-o"></i>
                    <asp:Literal ID="lActionTitle" runat="server" Text="Create Merge Document" /></h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlblInfo" runat="server" LabelType="Info" Text="" />
                </div>
            </div>

            <div class="panel-body">

                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />


                <fieldset>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:NotificationBox ID="nbNumberOfRecords" runat="server" NotificationBoxType="Info" />
                            <Rock:RockCheckBox ID="cbCombineFamilyMembers" runat="server" Text="Combine Family Members" Help="Set this to combine family members into a single row. For example, Ted Decker and Cindy Decker would be combined into 'Ted & Cindy Decker'" />
                            <Rock:MergeTemplatePicker ID="mtPicker" runat="server" Label="Merge Template" Required="true" OnSelectItem="mtPicker_SelectItem" />
                        </div>
                        <div class="col-md-6">
                            <asp:LinkButton ID="btnShowDataPreview" runat="server" CssClass="btn btn-action btn-xs" OnClick="btnShowDataPreview_Click" Text="Show Data Rows" CausesValidation="false" />
                            <asp:Panel ID="pnlPreview" runat="server" Visible="false">
                                <h4>Preview (top 15 rows)</h4>
                                <div class="grid">
                                    <Rock:Grid ID="gPreview" runat="server" AllowSorting="true" EmptyDataText="No Results" ShowActionRow="false" DisplayType="Light" />
                                </div>
                            </asp:Panel>

                            <asp:LinkButton ID="btnShowMergeFieldsHelp" runat="server" CssClass="btn btn-default btn-xs" OnClick="btnShowMergeFieldsHelp_Click" CausesValidation="false"> Show Merge Fields <i class="fa fa-chevron-down"></i></asp:LinkButton>
                            <asp:Panel ID="pnlMergeFieldsHelp" runat="server" Visible="false">
                                <Rock:RockLiteral ID="lShowMergeFields" runat="server" />
                            </asp:Panel>

                        </div>
                    </div>
                    

                    <Rock:NotificationBox ID="nbMergeError" runat="server" NotificationBoxType="Warning" Visible="false" CssClass="js-merge-error"/>

                    <div class="actions">
                        <asp:LinkButton ID="btnMerge" runat="server" Text="Merge" CssClass="btn btn-primary" OnClientClick="$('.js-merge-error').hide()" OnClick="btnMerge_Click" />
                    </div>
                </fieldset>
            </div>

        </asp:Panel>



    </ContentTemplate>
</asp:UpdatePanel>
