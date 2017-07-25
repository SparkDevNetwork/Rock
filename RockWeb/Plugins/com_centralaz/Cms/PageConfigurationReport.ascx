<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PageConfigurationReport.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Cms.PageConfigurationReport" %>
    <style>
        #diagramContainer {
            padding: 20px;
            width:80%; height: 200px;
            border: 1px solid gray;
        }

        .diagram-block {
            min-height:20px; width: 70px;
            border: 1px solid #555;
            margin: auto;
            margin-bottom: 2px;
            font-size: x-small;
            padding: 2px;
            background-color:#eee;
        }

        #cy {
          position: absolute;
          min-height: 2000px;
          left: 0;
          top: 0;
          right: 0;
          bottom: 0;
          z-index: 999;
        }
    </style>

<asp:UpdatePanel ID="upnlContent" runat="server" UpdateMode="Always">
    <ContentTemplate>
        <div id="divPageConfigurationReport" style="min-height:2000px;">
            <Rock:NotificationBox ID="nbMessage" runat="server" NotificationBoxType="Danger"></Rock:NotificationBox>
            <asp:Panel runat="server" ID="pnlReport">
                <asp:LinkButton runat="server" ID="lbToggleDiagram" CssClass="btn btn-primary" OnClick="lbToggleDiagram_Click">Show Diagram</asp:LinkButton>
                <asp:Literal ID="lPages" runat="server" ViewStateMode="Disabled" Visible="true"></asp:Literal>

                <table class="table table-condensed">
                    <tr><th>Key</th></tr>
                    <tr class="danger"><td>Indicates unused by other pages in this report.</td></tr>
                </table>
            </asp:Panel>
            
            <!-- Diagram stuff -->
            <asp:Panel runat="server" ID="pnlDiagram" Visible="false">
                <div id="cy"></div>
            </asp:Panel>

            </div>
        <script>
            function AfterPartialPostback()
            {
                try
                {
                    if ( $("[id$='pnlDiagram']").is(":visible") )
                    {
                        var cy = window.cy = cytoscape({
                            container: document.getElementById('cy'),

                            boxSelectionEnabled: false,
                            autounselectify: true,

                            style: [
                              {
                                  selector: 'node',
                                  css: {
                                      'content': 'data(name)',
                                      'text-valign': 'center',
                                      'text-halign': 'center',
                                      'font-size': '10pt',
                                      'shape': 'rectangle'
                                  }
                              },
                              {
                                  selector: '$node > node',
                                  css: {
                                      'padding-top': '10px',
                                      'padding-left': '10px',
                                      'padding-bottom': '10px',
                                      'padding-right': '10px',
                                      'text-valign': 'top',
                                      'text-halign': 'center',
                                      'background-color': '#bbb',
                                      'font-size': '20pt'
                                  }
                              },
                              {
                                  selector: '.block',
                                  css: {
                                      'padding-top': '2px',
                                      'padding-left': '2px',
                                      'padding-bottom': '2px',
                                      'padding-right': '2px',
                                      'text-halign': 'center',
                                      'background-color': '#dde',
                                      'font-size': '12pt',
                                      'width': 'label'
                                  }
                              },
                              {
                                  selector: '.setting',
                                  css: {
                                      'padding-top': '2px',
                                      'padding-left': '2px',
                                      'padding-bottom': '2px',
                                      'padding-right': '2px',
                                      'background-color': '#dfdfef',
                                      'font-size': '12pt',
                                      'text-wrap': 'wrap',
                                      'text-halign': 'center'
                                  }
                              },
                              {
                                  selector: 'edge',
                                  css: {
                                      'arrow-scale': 2,
                                      'target-arrow-shape': 'triangle',
                                      'target-arrow-color': '#6FB1FC',
                                      'line-color': '#6FB1FC',
                                      'color': '#000',
                                      'curve-style': 'bezier',
                                      'control-point-step-size': 50,
                                      'opacity': 0.7,
                                      'label': 'data(label)',
                                  }
                              },
                              {
                                  selector: '.nextPage',
                                  css: {
                                      'target-arrow-color': '#55FF55',
                                      'line-color': '#55FF55',
                                  }
                              },
                              {
                                  selector: '.prevPage',
                                  css: {
                                      'target-arrow-color': '#CCCC44',
                                      'line-color': '#CCCC44',
                                      'line-style': 'dotted',
                                  }
                              },
                              {
                                  selector: '.homePage',
                                  css: {
                                      'target-arrow-color': '#999',
                                      'line-color': '#999',
                                      'line-style': 'dashed',
                                  }
                              },
                              {
                                  selector: ':selected',
                                  css: {
                                      'background-color': 'black',
                                      'line-color': 'black',
                                      'target-arrow-color': 'black',
                                      'source-arrow-color': 'black'
                                  }
                              },
                            {
                                selector: '.multiline-manual',
                                style: {
                                    'text-wrap': 'wrap'
                                }
                            },
                            {
                                selector: '.multiline-auto',
                                style: {
                                    'text-wrap': 'wrap',
                                    'text-max-width': 80
                                }
                            },
                            {
                                selector: '.autorotate',
                                style: {
                                    'edge-text-rotation': 'autorotate'
                                }
                            },
                            {
                                selector: '.background',
                                style: {
                                    'text-background-opacity': 1,
                                    'text-background-color': '#ccc',
                                    'text-background-shape': 'roundrectangle',
                                    'text-border-color': '#000',
                                    'text-border-width': 1,
                                    'text-border-opacity': 1
                                }
                            },
                            {
                                selector: '.outline',
                                style: {
                                    'text-outline-color': '#ccc',
                                    'text-outline-width': 3
                                }
                            }
                            ],

                            elements: {
                                nodes: [
                                    <%= NodeData() %>
                                ],
                                edges: [
                                    <%= EdgeData() %>
                                ]
                            },

                            layout: {
                                name: 'circle',
                                padding: 5,
                                cols: 5
                            }

                        });

                    }
                } catch (ex)
                {
                    console.log( ex.message )
                }
            };

            //Re-bind for callbacks
            Sys.Application.add_load(function ()
            {
                AfterPartialPostback();
            });


        </script>

    </ContentTemplate>

</asp:UpdatePanel>

