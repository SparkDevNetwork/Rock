<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MergeTemplateList.ascx.cs" Inherits="RockWeb.Blocks.Core.MergeTemplateList" %>

<asp:UpdatePanel ID="upMergeTemplateList" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-files-o"></i>&nbsp;Merge Template List</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    
                    <Rock:GridFilter ID="gfSettings" runat="server" OnApplyFilterClick="gfSettings_ApplyFilterClick">
                        <Rock:PersonPicker ID="ppPersonFilter" runat="server" Label="Person" />
                        <Rock:RockCheckBox ID="cbShowGlobalMergeTemplates" runat="server" Label="Show Global Merge Templates" Text="Yes" />
                    </Rock:GridFilter>

                    <Rock:Grid ID="gMergeTemplates" runat="server" AllowSorting="true" OnRowSelected="gMergeTemplates_Edit">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:PersonField DataField="PersonAlias.Person"  HeaderText="Owner" SortExpression="PersonAlias.Person.LastName,PersonAlias.Person.NickName" NullDisplayText="(Global)" />
                            <Rock:DeleteField OnClick="gMergeTemplates_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>

