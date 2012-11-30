<%@ Control Language="C#" AutoEventWireup="false" CodeFile="HtmlContent.ascx.cs" Inherits="RockWeb.Blocks.Cms.HtmlContent" %>
<%@ Register TagPrefix="asp" Namespace="AjaxControlToolkit" Assembly="AjaxControlToolkit"%>

<script type="text/javascript">

    Sys.Application.add_load(function () {

        var modalPopup = $find('<%=mpeContent.BehaviorID%>');
        modalPopup.add_hidden(modalHidden_<%=CurrentBlock.Id %>);
    
        $('#<%=tbStartDate.ClientID %>').kendoDatePicker({ open:function(e){
            window.setTimeout(function(){ $('.k-calendar-container').parent('.k-animation-container').css('zIndex', '200000'); }, 1);
        } });

        $('#<%=tbExpireDate.ClientID %>').kendoDatePicker({ open:function(e){
            window.setTimeout(function(){ $('.k-calendar-container').parent('.k-animation-container').css('zIndex', '200000'); }, 1);
        } });

        $('#html-content-version-<%=CurrentBlock.Id %>').click(function () {
            $('#html-content-versions-<%=CurrentBlock.Id %>').show();
            $(this).hide();
            $('#html-content-edit-<%=CurrentBlock.Id %>').hide();
            $find('<%=mpeContent.BehaviorID%>')._layout(); 
            return false;
        });

        $('#html-content-versions-cancel-<%=CurrentBlock.Id %>').click(function () {
            $('#html-content-edit-<%=CurrentBlock.Id %>').show();
            $('#html-content-version-<%=CurrentBlock.Id %>').show();
            $('#html-content-versions-<%=CurrentBlock.Id %>').hide();
            $find('<%=mpeContent.BehaviorID%>')._layout(); 
            return false;
        });

        if ($('#<%=hfAction.ClientID %>').val() == 'Edit')
        {
            $('#html-content-edit-<%=CurrentBlock.Id %> textarea.html-content-editor').ckeditor(function() {
                $find('<%=mpeContent.BehaviorID%>').show(); 
            }, ckoptionsAdv).end();
        }

        $('a.html-content-show-version-<%=CurrentBlock.Id %>').click(function () {

            if (CKEDITOR.instances['<%=txtHtmlContentEditor.ClientID %>'].checkDirty() == false ||
                confirm('Loading a previous version will cause any changes you\'ve made to the existing text to be lost.  Are you sure you want to continue?'))
            {
                $.ajax({
                    type: 'GET',
                    contentType: 'application/json',
                    dataType: 'json',
                    url: rock.baseUrl + 'REST/Cms/HtmlContent/' + $(this).attr('html-id'),
                    success: function (getData, status, xhr) {

                        htmlContent = getData;
                        
                        $('#html-content-version-<%=CurrentBlock.Id %>').text('Version ' + htmlContent.Version);
                        $('#<%=hfVersion.ClientID %>').val(htmlContent.Version);
                        $('#<%=tbStartDate.ClientID %>').val(htmlContent.StartDateTime);
                        $('#<%=tbExpireDate.ClientID %>').val(htmlContent.ExpireDateTime);
                        $('#<%=cbApprove.ClientID %>').attr('checked', htmlContent.Approved);

                        CKEDITOR.instances['<%=txtHtmlContentEditor.ClientID %>'].setData(htmlContent.Content, function() {
                            CKEDITOR.instances['<%=txtHtmlContentEditor.ClientID %>'].resetDirty();
                            $('#html-content-edit-<%=CurrentBlock.Id %>').show();
                            $('#html-content-version-<%=CurrentBlock.Id %>').show();
                            $('#html-content-versions-<%=CurrentBlock.Id %>').hide();
                            $find('<%=mpeContent.BehaviorID%>')._layout(); 
                        });

                    },
                    error: function (xhr, status, error) {
                        alert(status + ' [' + error + ']: ' + xhr.responseText);
                    }
                });
            }
        });

    });

    function modalHidden_<%=CurrentBlock.Id %>(){
        CKEDITOR.instances['<%=txtHtmlContentEditor.ClientID %>'].destroy();
    }

    function saveHtmlContent_<%=CurrentBlock.Id %>(){
        $('#<%=btnSave.ClientID %>').click();
    }

</script>

<asp:UpdatePanel runat="server" class="html-content-block">
<ContentTemplate>

    <asp:Literal ID="lPreText" runat="server"></asp:Literal><asp:Literal ID="lHtmlContent" runat="server"></asp:Literal><asp:Literal ID="lPostText" runat="server"></asp:Literal>

    <asp:HiddenField ID="hfAction" runat="server" />
    <asp:Button ID="btnDefault" runat="server" Text="Show" style="display:none"/>
    <asp:Panel ID="pnlContentEditor" runat="server" CssClass="modal2" style="display:none">

        <div class="modal-header">
            <a id="aClose" runat="server" href="#" class="close">&times;</a>
            <h3>HTML Content</h3>
            <asp:PlaceHolder ID="phCurrentVersion" runat="server"><a id="html-content-version-<%=CurrentBlock.Id %>" 
                class="html-content-version-label">Version <asp:Literal ID="lVersion" runat="server"></asp:Literal></a></asp:PlaceHolder>
        </div>

        <div id="html-content-versions-<%=CurrentBlock.Id %>" style="display:none">
            <div class="modal-body">
                <Rock:Grid ID="rGrid" runat="server" AllowPaging="false" >
                    <Columns>
                        <asp:TemplateField SortExpression="Version" HeaderText="Version">
                            <ItemTemplate>
                                <a html-id='<%# Eval("Id") %>' class="html-content-show-version-<%=CurrentBlock.Id %>" href="#">Version <%# Eval("Version") %></a>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="ModifiedDateTime" HeaderText="Modified" SortExpression="ModifiedDateTime" />
                        <asp:BoundField DataField="ModifiedByPerson" HeaderText="By" SortExpression="ModifiedByPerson" />
                        <Rock:BoolField DataField="Approved" HeaderText="Approved" SortExpression="Approved" />
                        <asp:BoundField DataField="ApprovedByPerson" HeaderText="By" SortExpression="ApprovedByPerson" />
                        <asp:BoundField DataField="StartDateTime" DataFormatString="MM/dd/yy" HeaderText="Start" SortExpression="StartDateTime" />
                        <asp:BoundField DataField="ExpireDateTime" DataFormatString="MM/dd/yy" HeaderText="Expire" SortExpression="ExpireDateTime" />
                    </Columns>
                </Rock:Grid>
            </div>
            <div class="modal-footer">
                <button id="html-content-versions-cancel-<%=CurrentBlock.Id %>" class="btn">Cancel</button>
            </div>
        </div>

        <div id="html-content-edit-<%=CurrentBlock.Id %>">
            <asp:panel ID="pnlVersioningHeader" runat="server" class="html-content-edit-header">
                <asp:HiddenField ID="hfVersion" runat="server" />
                Start: <asp:TextBox ID="tbStartDate" runat="server"></asp:TextBox>
                Expire: <asp:TextBox ID="tbExpireDate" runat="server"></asp:TextBox>
                <div class="html-content-approve inline-form"><asp:CheckBox ID="cbApprove" runat="server" TextAlign="Right" Text="Approve" /></div>
            </asp:panel>
            <div class="modal-body">
                <asp:TextBox ID="txtHtmlContentEditor" CssClass="html-content-editor" TextMode="MultiLine" runat="server"></asp:TextBox>
            </div>
            <div class="modal-footer">
                <span class="inline-form">
                    <asp:CheckBox ID="cbOverwriteVersion" runat="server" TextAlign="Right" Text="don't save a new version" />
                </span>
                <asp:LinkButton ID="lbCancel" runat="server" CssClass="btn" Text="Cancel" />
                <asp:LinkButton ID="lbOk" runat="server" CssClass="btn btn-primary" Text="Save" />
            </div>
        </div>

        <asp:Button ID="btnSave" runat="server" OnClick="btnSave_Click" Text="Save" style="display:none" />

    </asp:Panel>
    
    <asp:ModalPopupExtender ID="mpeContent" runat="server" BackgroundCssClass="modal-backdrop"  
        PopupControlID="pnlContentEditor" CancelControlID="lbCancel" OkControlID="lbOk" TargetControlID="btnDefault"></asp:ModalPopupExtender>

</ContentTemplate>
</asp:UpdatePanel>



