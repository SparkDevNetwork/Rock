<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonalizationSegmentList.ascx.cs" Inherits="RockWeb.Blocks.Cms.PersonalizationSegmentList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-star"></i> 
                    Personalization Segments
                </h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="gFilter" runat="server" OnApplyFilterClick="gfList_ApplyFilterClick" OnClearFilterClick="gfList_ClearFilterClick" >
                        <Rock:RockTextBox ID="tbNameFilter" runat="server" Label="Name" />
                        <Rock:RockCheckBox ID="cbShowInactive" runat="server" Checked="false" Label="Include Inactive" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="gList" runat="server" AllowSorting="true" RowItemText="Personalization Segments" OnRowSelected="gList_RowSelected">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:RockBoundField DataField="FilterDataViewName" HeaderText="Data View" SortExpression="FilterDataViewName" />
                            <Rock:RockBoundField DataField="KnownIndividualsCount" HeaderText="Known Individuals" SortExpression="KnownIndividualsCount" />
                            <Rock:RockBoundField DataField="AnonymousIndividualsCount" HeaderText="Anonymous Individuals" SortExpression="AnonymousIndividualsCount" />
                            <Rock:RockBoundField DataField="TimeToUpdateDurationMilliseconds" HeaderText="Time To Update (ms)" SortExpression="TimeToUpdateDurationMilliseconds" DataFormatString="{0:#,##0.0}" />
                            <Rock:BoolField DataField="IsActive" HeaderText="Active" SortExpression="IsActive" />
                            <asp:TemplateField HeaderStyle-CssClass="grid-columncommand" ItemStyle-CssClass="grid-columncommand">
                                <ItemTemplate>
                                    <a class="btn btn-default" href='<%# string.Format( "{0}{1}", ResolveRockUrl( "~/admin/cms/personalization-segments/" ), Eval("Guid") ) %>'><i class="fa fa-users"></i></a>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <Rock:DeleteField OnClick="gList_DeleteClick" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
