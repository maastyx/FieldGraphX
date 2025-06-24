using FieldGraphX.Models;
using FlowGraphX;
using McTools.Xrm.Connection;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using XrmToolBox.Extensibility;
using Label = System.Windows.Forms.Label;

namespace FieldGraphX
{
    public partial class FieldGraphXControl : PluginControlBase
    {
        private Settings mySettings;
        private InfoLoader myInfoLoader;
        private Dictionary<FlowHierarchyNode, Point> panelPositions;

        public FieldGraphXControl()
        {
            InitializeComponent();
        }

        private void FieldGraphXControl_Load(object sender, EventArgs e)
        {
            //ShowInfoNotification("This is a notification that can lead to XrmToolBox repository", new Uri("https://github.com/MscrmTools/XrmToolBox"));

            // Loads or creates the settings for the plugin
            if (!SettingsManager.Instance.TryLoad(GetType(), out mySettings))
            {
                mySettings = new Settings();

                LogWarning("Settings not found => a new settings file has been created!");
            }
            else
            {
                LogInfo("Settings found and loaded");
            }

            myInfoLoader = new InfoLoader(Service);
            cmbEntities.DataSource = myInfoLoader.LoadEntities();
            if(cmbEntities.Items.Count > 0)
            {
                cmbEntities.SelectedIndex = 0;
            }
            if(cmbEntities.Text?.Trim()?.ToLower() != "")
            {
                cmbFields.DataSource = myInfoLoader.LoadFields(cmbEntities.Text.Trim().ToLower());
            }
            else
            {
                cmbFields.DataSource = new List<string>();
            }
        }

        private void tsbClose_Click(object sender, EventArgs e)
        {
            CloseTool();
        }

        /// <summary>
        /// This event occurs when the plugin is closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MyPluginControl_OnCloseTool(object sender, EventArgs e)
        {
            // Before leaving, save the settings
            SettingsManager.Instance.Save(GetType(), mySettings);
        }

        /// <summary>
        /// This event occurs when the connection has been updated in XrmToolBox
        /// </summary>
        public override void UpdateConnection(IOrganizationService newService, ConnectionDetail detail, string actionName, object parameter)
        {
            base.UpdateConnection(newService, detail, actionName, parameter);

            if (mySettings != null && detail != null)
            {
                mySettings.LastUsedOrganizationWebappUrl = detail.WebApplicationUrl;
                LogInfo("Connection has changed to: {0}", detail.WebApplicationUrl);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            flpResults.Controls.Clear(); // FlowLayoutPanel für Ergebnisse leeren
            string entity = cmbEntities.Text.Trim().ToLower();
            string field = cmbFields.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(entity) || string.IsNullOrEmpty(field))
            {
                MessageBox.Show("Bitte Entität und Feld angeben.");
                return;
            }

            string environmentUrl = mySettings.LastUsedOrganizationWebappUrl;

            WorkAsync(new WorkAsyncInfo
            {
                Message = "Flows werden analysiert...",
                Work = (w, ev) =>
                {
                    var analyzer = new FlowAnalyzer(Service);
                    var results = analyzer.AnalyzeFlows(entity, field,environmentUrl);
                    ev.Result = results;
                },
                PostWorkCallBack = ev =>
                {
                    var results = ev.Result as List<FlowUsage>;
                    var hierarchy = ev.Result as FlowHierarchy;
                    if (results != null && results.Count > 0)
                    {
                        foreach (var flow in results)
                        {
                            results.Sort((x, y) => string.Compare(x.FlowName, y.FlowName, StringComparison.OrdinalIgnoreCase));
                            // Erstelle eine Kachel für jeden Flow
                            var flowPanel = new Panel
                            {
                                BorderStyle = BorderStyle.FixedSingle,
                                Width = 350,
                                Height = 150,
                                Padding = new Padding(10)
                            };

                            var lblFlowName = new Label
                            {
                                Text = $"Flow: {flow.FlowName}",
                                Font = new Font("Arial", 10, FontStyle.Bold),
                                Dock = DockStyle.Top,
                                AutoSize = false
                            };

                            var lblTriggerType = new Label
                            {
                                Text = $"Trigger: {flow.Trigger.Name} auf Entity {flow.Trigger.Entity}",
                                Dock = DockStyle.Top
                            };

                            var lblFieldUsedAsTrigger = new Label
                            {
                                Text = $"Feld als Trigger verwendet: {flow.IsFieldUsedAsTrigger}",
                                Dock = DockStyle.Top
                            };

                            var lblFieldSet = new Label
                            {
                                Text = $"Feld wird gesetzt: {flow.IsFieldSet}",
                                Dock = DockStyle.Top
                            };

                            var btnOpenFlow = new Button
                            {
                                Text = "Flow öffnen",
                                Dock = DockStyle.Bottom,
                                Height = 30
                            };

                            btnOpenFlow.Click += (s, args) =>
                            {
                                try
                                {
                                    if (string.IsNullOrEmpty(flow.FlowUrl))
                                    {
                                        MessageBox.Show("Die Flow-URL ist leer oder ungültig.");
                                        return;
                                    }
                                    MessageBox.Show($"Öffne Flow: {flow.FlowName} ({flow.FlowUrl})");
                                    // Öffne die Flow-URL im Standard-Webbrowser
                                    var psi = new System.Diagnostics.ProcessStartInfo
                                    {
                                        FileName = flow.FlowUrl, // Die URL des Flows
                                        UseShellExecute = true // Shell-Execution aktivieren, um den Standardbrowser zu verwenden
                                    };
                                    System.Diagnostics.Process.Start(psi);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show($"Fehler beim Öffnen des Flows: {ex.Message}");
                                }
                            };

                            flowPanel.Controls.Add(btnOpenFlow);
                            flowPanel.Controls.Add(lblFieldSet);
                            flowPanel.Controls.Add(lblFieldUsedAsTrigger);
                            flowPanel.Controls.Add(lblTriggerType);
                            flowPanel.Controls.Add(lblFlowName);

                            flpResults.Controls.Add(flowPanel); // Kachel zum FlowLayoutPanel hinzufügen

                            var node = new TreeNode(flow.FlowName);
                            node.Nodes.Add("Trigger: " + flow.Trigger.Name);
                            node.Nodes.Add("Feld als Trigger verwendet: " + flow.IsFieldUsedAsTrigger);
                            node.Nodes.Add("Feld wird gesetzt: " + flow.IsFieldSet);
                            tvResults.Nodes.Add(node);
                            tvResults.ExpandAll();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Keine Flows gefunden.");
                    }
                    //if (hierarchy != null && hierarchy.RootNodes.Count > 0)
                    //{
                    //    VisualizeFlowHierarchy(hierarchy);
                    //}
                    //else
                    //{
                    //    MessageBox.Show("Keine Flows in der Hierarchie gefunden.");
                    //}
                }
            });
        }

        private void VisualizeFlowHierarchy(FlowHierarchy hierarchy)
        {
            flpResults.Controls.Clear(); // FlowLayoutPanel für Ergebnisse leeren
            flpResults.AutoScroll = true;

            panelPositions = new Dictionary<FlowHierarchyNode, Point>();

            void CreateFlowPanel(FlowHierarchyNode node, int level, int index)
            {
                // Panel für den aktuellen Flow erstellen
                var flowPanel = new Panel
                {
                    BorderStyle = BorderStyle.FixedSingle,
                    Width = 300,
                    Height = 120,
                    Padding = new Padding(10),
                    Location = new Point(50 + level * 350, 50 + index * 150) // Position basierend auf Level und Index
                };

                var lblFlowName = new Label
                {
                    Text = $"Flow: {node.Flow.FlowName}",
                    Font = new Font("Arial", 10, FontStyle.Bold),
                    Dock = DockStyle.Top,
                    AutoSize = false
                };

                var lblTriggerType = new Label
                {
                    Text = $"Trigger: {node.Flow.Trigger.Name}",
                    Dock = DockStyle.Top
                };

                var lblFieldUsedAsTrigger = new Label
                {
                    Text = $"Feld als Trigger verwendet: {node.Flow.IsFieldUsedAsTrigger}",
                    Dock = DockStyle.Top
                };

                var lblFieldSet = new Label
                {
                    Text = $"Feld wird gesetzt: {node.Flow.IsFieldSet}",
                    Dock = DockStyle.Top
                };

                flowPanel.Controls.Add(lblFlowName);
                flowPanel.Controls.Add(lblTriggerType);
                flowPanel.Controls.Add(lblFieldUsedAsTrigger);
                flowPanel.Controls.Add(lblFieldSet);

                flpResults.Controls.Add(flowPanel);
                panelPositions[node] = flowPanel.Location; // Position speichern

                // Kinderknoten verarbeiten
                for (int i = 0; i < node.ChildNodes.Count; i++)
                {
                    CreateFlowPanel(node.ChildNodes[i], level + 1, index + i);
                }
            }

            // Panels für die Root-Nodes erstellen
            for (int i = 0; i < hierarchy.RootNodes.Count; i++)
            {
                CreateFlowPanel(hierarchy.RootNodes[i], 0, i);
            }

            //// Pfeile zwischen den Panels zeichnen
            //flpResults.Paint += (s, e) =>
            //{
            //    foreach (var node in hierarchy.RootNodes)
            //    {
            //        DrawConnections(e.Graphics, node);
            //    }
            //};
        }

        private void DrawConnections(Graphics graphics, FlowHierarchyNode node)
        {
            if (node.ChildNodes.Count == 0) return;

            var startPoint = panelPositions[node];
            startPoint.Offset(150, 60); // Mitte des Panels

            foreach (var child in node.ChildNodes)
            {
                var endPoint = panelPositions[child];
                endPoint.Offset(0, 60); // Mitte des Panels

                // Pfeil zeichnen
                using (var pen = new Pen(Color.Black, 2))
                {
                    pen.EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
                    graphics.DrawLine(pen, startPoint, endPoint);
                }

                // Rekursiv für Kinderknoten
                DrawConnections(graphics, child);
            }
        }

        private void cmbEntities_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Setze die DataSource auf null, um die Items-Sammlung zurückzusetzen
            cmbFields.DataSource = null;

            // Lade die Felder basierend auf der ausgewählten Entität
            var fields = myInfoLoader.LoadFields(cmbEntities.Text.Trim().ToLower());

            // Weise die neue Datenquelle zu
            cmbFields.DataSource = fields;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            flpResults.Controls.Clear(); // FlowLayoutPanel für Ergebnisse leeren
            flpResults.AutoScroll = true;

            string entity = cmbEntities.Text.Trim().ToLower();
            string field = cmbFields.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(entity) || string.IsNullOrEmpty(field))
            {
                MessageBox.Show("Bitte Entität und Feld angeben.");
                return;
            }

            string environmentUrl = mySettings.LastUsedOrganizationWebappUrl;

            WorkAsync(new WorkAsyncInfo
            {
                Message = "Flows werden analysiert...",
                Work = (w, ev) =>
                {
                    var analyzer = new FlowAnalyzer(Service);
                    var hierarchy = analyzer.AnalyzeFlowsHierarchically(entity, field, environmentUrl); // Hierarchische Analyse
                    ev.Result = hierarchy;
                },
                PostWorkCallBack = ev =>
                {
                    var hierarchy = ev.Result as List<FlowUsage>;
                    if (hierarchy != null && hierarchy.Count > 0)
                    {
                        // Visualisiere die hierarchische Struktur der Flows
                        foreach (var flow in hierarchy)
                        {
                            var pos = DrawBox(flow);
                            foreach (var parent in flow.Parents)
                            {
                                var parentPos = DrawBox(parent);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Keine Flows gefunden.");
                    }
                }
            });
        }

        private Point DrawBox(FlowUsage flow)
        {
            
            // Panel für den aktuellen Flow erstellen
            var flowPanel = new Panel
            {
                BorderStyle = BorderStyle.FixedSingle,
                Width = 300,
                Height = 120,
                Padding = new Padding(10),
                Location = new Point(50 * 350, 50 * 150) // Position basierend auf Level und Index
            };

            var lblFlowName = new Label
            {
                Text = $"Flow: {flow.FlowName}",
                Font = new Font("Arial", 10, FontStyle.Bold),
                Dock = DockStyle.Top,
                AutoSize = false
            };

            var lblTriggerType = new Label
            {
                Text = $"Trigger: {flow.Trigger.Name}",
                Dock = DockStyle.Top
            };

            var lblFieldUsedAsTrigger = new Label
            {
                Text = $"Feld als Trigger verwendet: {flow.IsFieldUsedAsTrigger}",
                Dock = DockStyle.Top
            };

            var lblFieldSet = new Label
            {
                Text = $"Feld wird gesetzt: {flow.IsFieldSet}",
                Dock = DockStyle.Top
            };

            flowPanel.Controls.Add(lblFlowName);
            flowPanel.Controls.Add(lblTriggerType);
            flowPanel.Controls.Add(lblFieldUsedAsTrigger);
            flowPanel.Controls.Add(lblFieldSet);

            flpResults.Controls.Add(flowPanel);
            return flowPanel.Location; // Position speichern

            
        }
    }
}