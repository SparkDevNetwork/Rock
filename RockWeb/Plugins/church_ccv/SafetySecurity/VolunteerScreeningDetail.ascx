<%@ Control Language="C#" AutoEventWireup="true" CodeFile="VolunteerScreeningDetail.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.SafetySecurity.VolunteerScreeningDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pNewScreening" runat="server" class="panel panel-block">
            <%-- Header with global info --%>
            <div class="panel-heading">
                <div class="row col-sm-4">
                    <h4 class="panel-title">Volunteer Screening</h4>
                    <br />
                </div>
            </div>

            <div class="panel-body">
                <h4 class="panel-title"><strong>Person</strong></h4>

                <div class="row">
                    <div class="col-md-6">
                        <asp:Literal ID="lPersonName" runat="server"></asp:Literal>
                    </div>
                </div>
            </div>

            <%-- Application info --%>
            <div class="panel-body">
                <h4 class="panel-title"><strong>Application</strong></h4>
                
                <div class="row">
                    <div class="col-md-6">
                        <asp:Literal ID="lDate" runat="server"></asp:Literal>
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <asp:Literal ID="lApplicationWorkflow" runat="server"></asp:Literal>
                    </div>
                </div>
            </div>

            <%-- Background Check info --%>
            <div class="panel-body">
                <h4 class="panel-title"><strong>Background Check</strong></h4>

                <div>
                    <asp:Literal ID="lBGCheck_Link" runat="server"></asp:Literal>
                </div>
                
                <div>
                    Date: <asp:Literal ID="lBGCheck_Date" runat="server">Pending</asp:Literal>
                </div>

                <div>
                    Doc: <asp:Literal ID="lBGCheck_Doc" runat="server">Pending</asp:Literal>
                </div>

                <div>
                    Result: <asp:Literal ID="lBGCheck_Result" runat="server">Pending</asp:Literal>
                </div>
            </div>
        </asp:Panel>
        
        <%-- Character Reference info --%>
        <asp:Panel ID="pCharReferences" runat="server" class="panel panel-block">
            <%-- Header with global info --%>
            <div class="panel-heading">
                <div class="row col-sm-4">
                    <h4 class="panel-title"><strong>Character References</strong></h4>
                    <br />
                </div>
            </div>


            <div class="panel-body">
                <Rock:Grid ID="gCharacterRefs" Title="Character References" runat="server" DisplayType="Light" AllowSorting="true" RowItemText="Result" AllowPaging="false">
                    <Columns>
                        <asp:HyperLinkField DataNavigateUrlFields="PersonId" DataTextField="PersonText" DataNavigateUrlFormatString="~/Person/{0}" HeaderText="Person" />
                        <asp:HyperLinkField DataNavigateUrlFields="WorkflowId" DataTextField="WorkflowText" DataNavigateUrlFormatString="~/page/1492?CharacterReferenceWorkflowId={0}" HeaderText="Review" />
                        <Rock:RockBoundField DataField="State" HeaderText="State" SortExpression="State" />
                    </Columns>
                </Rock:Grid>

                <asp:Literal ID="lNoCharacterRefs" runat="server">No Character References have been Issued</asp:Literal>
            </div>
        </asp:Panel>
        
        <%-- Legacy Application Doc info --%>
        <asp:Panel ID="pLegacy" runat="server" class="panel panel-block">
            <div class="panel-body">
                <h4 class="panel-title">Legacy Documents</h4>

                <div class="row col-sm-4">
                    <Rock:FileUploader ID="fu_legAppFile" runat="server" Label="Application Document" IsBinary="true" OnFileUploaded="fu_legAppFile_FileUploaded" OnFileRemoved="fu_legAppFile_FileRemoved" />
                    
                    <Rock:FileUploader ID="FileUploader1" runat="server" Label="Character Reference 1" IsBinary="true" OnFileUploaded="fu_legAppFile_FileUploaded" OnFileRemoved="fu_legAppFile_FileRemoved" />
                    <Rock:FileUploader ID="FileUploader2" runat="server" Label="Character Reference 2" IsBinary="true" OnFileUploaded="fu_legAppFile_FileUploaded" OnFileRemoved="fu_legAppFile_FileRemoved" />
                    <Rock:FileUploader ID="FileUploader3" runat="server" Label="Character Reference 3" IsBinary="true" OnFileUploaded="fu_legAppFile_FileUploaded" OnFileRemoved="fu_legAppFile_FileRemoved" />
                </div>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
