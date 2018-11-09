<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ViewStateViewer.ascx.cs" Inherits="RockWeb.Blocks.Utility.ViewStateViewer" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <style>
            .xmlviewer pre.ace_editor {
                margin-bottom: 0;
            }
        </style>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-default">
        
            <div class="panel-heading clearfix">
                <h1 class="panel-title pull-left">
                    <i class="fa fa-binoculars"></i> 
                    ViewState Viewer
                </h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlblViewStateSize" runat="server" LabelType="Info" />
                </div>
            </div>
            <div class="panel-body" style="padding: 0;">
                <Rock:CodeEditor ID="ceViewState" runat="server" EditorHeight="800" EditorMode="Xml" CssClass="xmlviewer" EnableViewState="false" LineWrap="false" />
            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>