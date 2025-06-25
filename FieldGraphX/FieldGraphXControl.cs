using FieldGraphX.Models;
using FlowGraphX;
using McTools.Xrm.Connection;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using XrmToolBox.Extensibility;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using Label = System.Windows.Forms.Label;

namespace FieldGraphX
{
    public partial class FieldGraphXControl : PluginControlBase
    {
        private Settings mySettings;
        private InfoLoader myInfoLoader;
        private FlowVisualizationPanel visualizationPanel;

        // Dictionary zum Speichern der Flow-Positionen für das Zeichnen der Verbindungen
        private Dictionary<Guid, Rectangle> flowCardPositions = new Dictionary<Guid, Rectangle>();
        private List<FlowUsage> currentFlows = new List<FlowUsage>();
        private List<FlowUsage> allRelevantFlows = new List<FlowUsage>();

        // NEUE STRUKTUR: Gruppierung nach Feldern
        private Dictionary<string, List<FlowUsage>> fieldTriggerGroups = new Dictionary<string, List<FlowUsage>>();
        private Dictionary<string, List<FlowUsage>> fieldSetterGroups = new Dictionary<string, List<FlowUsage>>();

        public FieldGraphXControl()
        {
            InitializeComponent();
            InitializeVisualization();
        }

        private void InitializeVisualization()
        {
            visualizationPanel = new FlowVisualizationPanel();
            visualizationPanel.Dock = DockStyle.Fill;
            this.Controls.Add(visualizationPanel); // oder zu deinem Container
        }

        private void FieldGraphXControl_Load(object sender, EventArgs e)
        {
            //ShowInfoNotification("This is a notification that can lead to XrmToolBox repository", new Uri("https://github.com/MscrmTools/XrmToolBox"));

            // Loads or creates the settings for the plugin
            if (!SettingsManager.Instance.TryLoad(GetType(), out mySettings))
            {
                mySettings = new Settings();
                mySettings.LastUsedOrganizationWebappUrl = ConnectionDetail.WebApplicationUrl;
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


        /// <summary>
        /// This event occurs when the plugin is closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FieldGraphXControl_OnCloseTool(object sender, EventArgs e)
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
                MessageBox.Show("Please specify entity and field.");
                return;
            }

            string environmentUrl = mySettings.LastUsedOrganizationWebappUrl;

            WorkAsync(new WorkAsyncInfo
            {
                Message = "Flows are analyzed...",
                Work = (w, ev) =>
                {
                    var analyzer = new FlowAnalyzer(Service);
                    var results = analyzer.AnalyzeFlows(entity, field,"EnvID");
                    ev.Result = results;
                },
                PostWorkCallBack = ev =>
                {
                    var results = ev.Result as List<FlowUsage>;
                    MessageBox.Show($"Anzahl der Flows: {results.Count}");
                    if (results != null && results.Count > 0)
                    {
                        results.Sort((x, y) => string.Compare(x.FlowName, y.FlowName, StringComparison.OrdinalIgnoreCase));

                        foreach (var flow in results)
                        {
                            // Erstelle eine Kachel für jeden Flow
                            var flowPanel = new Panel
                            {
                                BorderStyle = BorderStyle.FixedSingle,
                                Width = 350,
                                Height = 150,
                                Padding = new Padding(5)
                            };

                            var lblFlowName = new Label
                            {
                                Text = $"Flow: {flow.FlowName}",
                                Font = new Font("Arial", 8, FontStyle.Bold),
                                Dock = DockStyle.Top,
                                AutoSize = false
                            };

                            var lblTriggerType = new Label
                            {
                                Text = $"Trigger: {flow.Trigger.Name} on Entity {flow.Trigger.Entity}",
                                Dock = DockStyle.Top
                            };

                            var lblFieldUsedAsTrigger = new Label
                            {
                                Text = $"Field used as trigger: {flow.IsFieldUsedAsTrigger}",
                                Dock = DockStyle.Top
                            };

                            var lblFieldSet = new Label
                            {
                                Text = $"Field is set: {flow.IsFieldSet}",
                                Dock = DockStyle.Top
                            };

                            var btnOpenFlow = new Button
                            {
                                Text = "Open flow",
                                Dock = DockStyle.Bottom,
                                Height = 30
                            };

                            btnOpenFlow.Click += (s, args) =>
                            {
                                try
                                {
                                    if (string.IsNullOrEmpty(flow.FlowUrl))
                                    {
                                        MessageBox.Show("The flow URL is empty or invalid.");
                                        return;
                                    }
                                    MessageBox.Show($"Open Flow: {flow.FlowName} ({flow.FlowUrl})");
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
                                    MessageBox.Show($"Error when opening the flow: {ex.Message}");
                                }
                            };

                            flowPanel.Controls.Add(btnOpenFlow);
                            flowPanel.Controls.Add(lblFieldSet);
                            flowPanel.Controls.Add(lblFieldUsedAsTrigger);
                            flowPanel.Controls.Add(lblTriggerType);
                            flowPanel.Controls.Add(lblFlowName);

                            flpResults.Controls.Add(flowPanel); // Kachel zum FlowLayoutPanel hinzufügen
                        }
                    }
                    else
                    {
                        MessageBox.Show("No flows found.");
                    }
                }
            });
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
                MessageBox.Show("Please specify entity and field.");
                return;
            }

            WorkAsync(new WorkAsyncInfo
            {
                Message = "Flows are analyzed...",
                Work = (w, ev) =>
                {
                    var analyzer = new FlowAnalyzer(Service);
                    var hierarchy = analyzer.AnalyzeFlowsHierarchically(entity, field, "EnvID"); // Hierarchische Analyse
                    ev.Result = hierarchy;
                },
                PostWorkCallBack = ev =>
                {
                    var hierarchy = ev.Result as List<FlowUsage>;
                    //MessageBox.Show($"Anzahl der Flows: {hierarchy.Count + hierarchy.Sum(f => f.Parents.Count)}");
                    if (hierarchy != null && hierarchy.Count > 0)
                    {
                        if (checkBox1.Checked) // Wenn die Checkbox aktiviert ist, einfache Visualisierung verwenden
                            VisualizeFlowHierarchyWithArrows(hierarchy);
                        else // Ansonsten die komplexe Visualisierung verwenden (FlowVisualizationPanel
                            MessageBox.Show("Not implementet");//VisualizeFlowHierarchy(hierarchy); // Hier die Methode aufrufen
                    }
                    else
                    {
                        MessageBox.Show("No flows found.");
                    }
                }
            });
        }



        public void VisualizeFlowHierarchyWithArrows(List<FlowUsage> hierarchy)
        {
            // Speichere die aktuellen Flows für das Zeichnen der Verbindungen
            currentFlows = hierarchy ?? new List<FlowUsage>();

            // ERWEITERT: Sammle alle relevanten Flows inklusive aller Ancestors
            allRelevantFlows = CollectAllRelevantFlows(hierarchy);

            // Verwende das vorhandene flpResults Panel, aber konfiguriere es neu
            flpResults.Controls.Clear();
            flpResults.AutoScroll = true;
            flpResults.FlowDirection = FlowDirection.TopDown;
            flpResults.WrapContents = false;
            flpResults.BackColor = Color.White;

            // Entferne vorherige Paint-Handler
            flpResults.Paint -= DrawConnectionsOnPanel;
            flpResults.Paint += DrawConnectionsOnPanel;

            // Debug-Ausgabe
            System.Diagnostics.Debug.WriteLine($"Original Flows: {hierarchy.Count}, Alle relevanten Flows: {allRelevantFlows.Count}");

            if (allRelevantFlows.Count == 0)
            {
                var noDataLabel = new Label();
                noDataLabel.Text = "Keine Flows gefunden";
                noDataLabel.Size = new Size(200, 30);
                noDataLabel.Font = new Font("Arial", 12, FontStyle.Bold);
                noDataLabel.ForeColor = Color.Red;
                flpResults.Controls.Add(noDataLabel);
                return;
            }

            // Analysiere und sortiere die COMPLETE Hierarchie
            var hierarchyLevels = BuildCompleteHierarchy(allRelevantFlows);

            // Positioniere die Flow-Karten im FlowLayoutPanel
            PositionFlowCardsInFlowPanel(flpResults, hierarchyLevels);

            foreach (var level in hierarchyLevels)
            {
                System.Diagnostics.Debug.WriteLine($"Level {level.Key}: {level.Value.Count} Flows");
            }

            // Panel invalidieren, um Paint-Event auszulösen
            flpResults.Invalidate();
        }

        // NEUE METHODE: Sammelt alle relevanten Flows inklusive aller Ancestors
        private List<FlowUsage> CollectAllRelevantFlows(List<FlowUsage> originalFlows)
        {
            var allFlows = new Dictionary<Guid, FlowUsage>();
            var toProcess = new Queue<FlowUsage>();
            var processed = new HashSet<Guid>();

            // Starte mit den ursprünglichen Flows
            foreach (var flow in originalFlows)
            {
                if (!allFlows.ContainsKey(flow.FlowID))
                {
                    allFlows[flow.FlowID] = flow;
                    toProcess.Enqueue(flow);
                }
            }

            // Verarbeite alle Ancestors rekursiv
            while (toProcess.Count > 0)
            {
                var currentFlow = toProcess.Dequeue();

                if (processed.Contains(currentFlow.FlowID))
                    continue;

                processed.Add(currentFlow.FlowID);

                // Füge alle Parents hinzu
                if (currentFlow.Parents != null)
                {
                    foreach (var parent in currentFlow.Parents)
                    {
                        if (!allFlows.ContainsKey(parent.FlowID))
                        {
                            allFlows[parent.FlowID] = parent;
                            toProcess.Enqueue(parent);
                        }
                    }
                }
            }

            return allFlows.Values.ToList();
        }

        private Dictionary<int, List<FlowUsage>> BuildCompleteHierarchy(List<FlowUsage> flows)
        {
            var result = new Dictionary<int, List<FlowUsage>>();
            var flowLevels = new Dictionary<Guid, int>(); // FlowID -> Level
            var processedFlows = new HashSet<Guid>();

            // Erstelle eine Lookup-Map für schnelleren Zugriff
            var flowLookup = flows.ToDictionary(f => f.FlowID, f => f);

            // ERWEITERT: Berechne für jeden Flow die maximale Tiefe seiner Ancestry
            var flowDepths = CalculateFlowDepths(flows, flowLookup);

            // Level 0: Root Flows (keine Parents oder Parents nicht in der aktuellen Liste)
            var rootFlows = flows.Where(f =>
                f.Parents == null ||
                f.Parents.Count == 0 ||
                !f.Parents.Any(p => flowLookup.ContainsKey(p.FlowID))
            ).ToList();

            if (rootFlows.Count > 0)
            {
                result[0] = rootFlows;
                foreach (var flow in rootFlows)
                {
                    flowLevels[flow.FlowID] = 0;
                    processedFlows.Add(flow.FlowID);
                }
            }

            // Weitere Levels aufbauen - ERWEITERT für tiefe Hierarchien
            int level = 1;
            bool foundFlowsInLevel = true;

            while (foundFlowsInLevel && level < 50) // Erhöht für tiefe Hierarchien
            {
                foundFlowsInLevel = false;
                var levelFlows = new List<FlowUsage>();

                foreach (var flow in flows.Where(f => !processedFlows.Contains(f.FlowID)))
                {
                    if (flow.Parents != null && flow.Parents.Count > 0)
                    {
                        // Prüfe ob ALLE verfügbaren Parents bereits verarbeitet wurden
                        var availableParents = flow.Parents.Where(p => flowLookup.ContainsKey(p.FlowID)).ToList();
                        var processedParents = availableParents.Where(p => flowLevels.ContainsKey(p.FlowID)).ToList();

                        // Flow kann platziert werden wenn mindestens ein Parent verarbeitet wurde
                        // ABER: Das Level wird basierend auf dem tiefsten Parent bestimmt
                        if (processedParents.Count > 0)
                        {
                            var maxParentLevel = processedParents.Max(p => flowLevels[p.FlowID]);
                            var newLevel = maxParentLevel + 1;

                            // Stelle sicher, dass das aktuelle Level korrekt ist
                            if (newLevel >= level)
                            {
                                levelFlows.Add(flow);
                                flowLevels[flow.FlowID] = newLevel;
                                processedFlows.Add(flow.FlowID);
                                foundFlowsInLevel = true;
                            }
                        }
                    }
                }

                if (levelFlows.Count > 0)
                {
                    // Sortiere Flows nach ihrer berechneten Ebene
                    var sortedLevelFlows = levelFlows.GroupBy(f => flowLevels[f.FlowID]);

                    foreach (var group in sortedLevelFlows)
                    {
                        if (!result.ContainsKey(group.Key))
                            result[group.Key] = new List<FlowUsage>();
                        result[group.Key].AddRange(group.ToList());
                    }
                }

                level++;
            }

            // Alle übrigen Flows (Zyklen oder isolierte Flows)
            var remainingFlows = flows.Where(f => !processedFlows.Contains(f.FlowID)).ToList();
            if (remainingFlows.Count > 0)
            {
                var maxLevel = result.Keys.Count > 0 ? result.Keys.Max() + 1 : 0;
                result[maxLevel] = remainingFlows;
                foreach (var flow in remainingFlows)
                {
                    flowLevels[flow.FlowID] = maxLevel;
                }
            }

            return result;
        }

        // ERWEITERTE METHODE: Berechnet die Tiefe jedes Flows in der Hierarchie
        private Dictionary<Guid, int> CalculateFlowDepths(List<FlowUsage> flows, Dictionary<Guid, FlowUsage> flowLookup)
        {
            var depths = new Dictionary<Guid, int>();
            var calculating = new HashSet<Guid>(); // Verhindert Zyklen

            foreach (var flow in flows)
            {
                CalculateDepthRecursive(flow.FlowID, flows, flowLookup, depths, calculating);
            }

            return depths;
        }

        private int CalculateDepthRecursive(Guid flowId, List<FlowUsage> allFlows, Dictionary<Guid, FlowUsage> flowLookup, Dictionary<Guid, int> depths, HashSet<Guid> calculating)
        {
            // Bereits berechnet
            if (depths.ContainsKey(flowId))
                return depths[flowId];

            // Zyklus erkannt
            if (calculating.Contains(flowId))
                return 0;

            // Flow nicht in der aktuellen Liste
            if (!flowLookup.ContainsKey(flowId))
                return 0;

            calculating.Add(flowId);

            var flow = flowLookup[flowId];
            int maxParentDepth = 0;

            if (flow.Parents != null && flow.Parents.Count > 0)
            {
                foreach (var parent in flow.Parents)
                {
                    var parentDepth = CalculateDepthRecursive(parent.FlowID, allFlows, flowLookup, depths, calculating);
                    maxParentDepth = Math.Max(maxParentDepth, parentDepth);
                }
            }

            calculating.Remove(flowId);
            depths[flowId] = maxParentDepth + 1;

            return depths[flowId];
        }

        private void PositionFlowCardsInFlowPanel(FlowLayoutPanel container, Dictionary<int, List<FlowUsage>> hierarchyLevels)
        {
            container.Controls.Clear();

            const int cardWidth = 300; // Etwas breiter für bessere Lesbarkeit
            const int cardHeight = 180; // Höher für mehr Informationen

            // Dictionary zum Speichern der Positionen für das Zeichnen der Verbindungen
            flowCardPositions = new Dictionary<Guid, Rectangle>();

            foreach (var level in hierarchyLevels.OrderBy(l => l.Key))
            {
                // Level-Header mit erweiterten Informationen
                var levelHeader = new Label();
                var originalFlowsInLevel = level.Value.Where(f => currentFlows.Any(cf => cf.FlowID == f.FlowID)).Count();
                var ancestorFlowsInLevel = level.Value.Count - originalFlowsInLevel;

                if (level.Key == 0)
                {
                    levelHeader.Text = $"🌱 Root Flows - Level {level.Key} ({level.Value.Count} flows)";
                    levelHeader.BackColor = Color.LightGreen;
                }
                else
                {
                    levelHeader.Text = $"🔗 Level {level.Key} - {level.Value.Count} flows ({originalFlowsInLevel} original, {ancestorFlowsInLevel} ancestors)";
                    levelHeader.BackColor = Color.LightBlue;
                }

                levelHeader.Font = new Font("Arial", 10, FontStyle.Bold);
                levelHeader.Size = new Size(container.Width - 40, 35);
                levelHeader.TextAlign = ContentAlignment.MiddleLeft;
                levelHeader.BorderStyle = BorderStyle.FixedSingle;
                levelHeader.Margin = new Padding(5);
                container.Controls.Add(levelHeader);

                // Flow-Karten in diesem Level hinzufügen
                foreach (var flow in level.Value)
                {
                    var flowCard = CreateEnhancedFlowCard(flow);
                    flowCard.Size = new Size(cardWidth, cardHeight);

                    // Speichere die Position für Verbindungslinien
                    flowCard.LocationChanged += (sender, e) => {
                        if (sender is Panel card && card.Tag is FlowUsage f)
                        {
                            flowCardPositions[f.FlowID] = new Rectangle(card.Location, card.Size);
                            container.Invalidate(); // Neu zeichnen der Verbindungen
                        }
                    };

                    container.Controls.Add(flowCard);
                }

                // Trennlinie zwischen Leveln
                if (level.Key < hierarchyLevels.Keys.Max())
                {
                    var separator = new Label();
                    separator.Text = "";
                    separator.Size = new Size(container.Width - 40, 3);
                    separator.BackColor = Color.Gray;
                    separator.Margin = new Padding(5, 10, 5, 10);
                    container.Controls.Add(separator);
                }
            }

            // Warte kurz und aktualisiere dann die Positionen
            Timer positionTimer = new Timer();
            positionTimer.Interval = 100;
            positionTimer.Tick += (sender, e) => {
                UpdateFlowCardPositions(container);
                positionTimer.Stop();
                positionTimer.Dispose();
            };
            positionTimer.Start();
        }

        private void UpdateFlowCardPositions(FlowLayoutPanel container)
        {
            foreach (Control control in container.Controls)
            {
                if (control.Tag is FlowUsage flow)
                {
                    flowCardPositions[flow.FlowID] = new Rectangle(control.Location, control.Size);
                }
            }
            container.Invalidate();
        }

        private void DrawConnectionsOnPanel(object sender, PaintEventArgs e)
        {
            DrawConnections(e.Graphics, sender as Control);
        }

        private void DrawConnections(Graphics g, Control container)
        {
            if (flowCardPositions == null || flowCardPositions.Count == 0 || allRelevantFlows == null)
                return;

            var pen = new Pen(Color.DarkBlue, 2);
            var arrowPen = new Pen(Color.DarkRed, 2);

            // Erstelle Lookup für schnelleren Zugriff - VERWENDE allRelevantFlows
            var flowLookup = allRelevantFlows.ToDictionary(f => f.FlowID, f => f);

            foreach (var flow in allRelevantFlows)
            {
                if (flow.Parents == null || flow.Parents.Count == 0)
                    continue;

                if (!flowCardPositions.ContainsKey(flow.FlowID))
                    continue;

                var childRect = flowCardPositions[flow.FlowID];

                foreach (var parent in flow.Parents)
                {
                    if (flowCardPositions.ContainsKey(parent.FlowID))
                    {
                        var parentRect = flowCardPositions[parent.FlowID];

                        // Berechne Verbindungspunkte
                        Point parentPoint = new Point(
                            parentRect.X + parentRect.Width / 2,
                            parentRect.Y + parentRect.Height - 5
                        );

                        Point childPoint = new Point(
                            childRect.X + childRect.Width / 2,
                            childRect.Y + 5
                        );

                        // Zeichne Verbindungslinie mit Kurve
                        DrawCurvedConnection(g, pen, parentPoint, childPoint);

                        // Zeichne Pfeilspitze
                        DrawArrowHead(g, arrowPen, parentPoint, childPoint);
                    }
                }
            }

            pen.Dispose();
            arrowPen.Dispose();
        }

        private void DrawCurvedConnection(Graphics g, Pen pen, Point start, Point end)
        {
            // Erstelle eine geschwungene Linie zwischen den Punkten
            int midY = (start.Y + end.Y) / 2;

            Point[] points = {
        start,
        new Point(start.X, midY),
        new Point(end.X, midY),
        end
    };

            if (points.Length >= 2)
            {
                // Zeichne eine einfache Linie, falls Kurven-Zeichnung Probleme macht
                g.DrawLine(pen, start, end);
            }
        }

        private void DrawArrowHead(Graphics g, Pen pen, Point start, Point end)
        {
            const double arrowLength = 15;
            const double arrowAngle = Math.PI / 6; // 30 Grad

            double angle = Math.Atan2(end.Y - start.Y, end.X - start.X);

            // Pfeilspitze Punkte berechnen
            Point arrowPoint1 = new Point(
                (int)(end.X - arrowLength * Math.Cos(angle - arrowAngle)),
                (int)(end.Y - arrowLength * Math.Sin(angle - arrowAngle))
            );

            Point arrowPoint2 = new Point(
                (int)(end.X - arrowLength * Math.Cos(angle + arrowAngle)),
                (int)(end.Y - arrowLength * Math.Sin(angle + arrowAngle))
            );

            // Zeichne Pfeilspitze
            g.DrawLine(pen, end, arrowPoint1);
            g.DrawLine(pen, end, arrowPoint2);
        }

        private Panel CreateEnhancedFlowCard(FlowUsage flow)
        {
            var card = new Panel();
            card.Size = new Size(300, 180); // Größere Karte für mehr Informationen
            card.BorderStyle = BorderStyle.FixedSingle;
            card.Margin = new Padding(5);
            card.Tag = flow; // Wichtig für das Zeichnen der Verbindungen

            // ERWEITERTE Hintergrundfarbe basierend auf Status und Herkunft
            bool isOriginalFlow = currentFlows.Any(cf => cf.FlowID == flow.FlowID);
            bool isAncestorFlow = !isOriginalFlow;

            if (isAncestorFlow)
            {
                card.BackColor = Color.FromArgb(240, 240, 240); // Sehr helles Grau für Ancestors
                card.BorderStyle = BorderStyle.Fixed3D; // 3D-Rahmen für Ancestors
            }
            else if (flow.IsFieldUsedAsTrigger)
                card.BackColor = Color.FromArgb(255, 182, 193); // Light Pink
            else if (flow.IsFieldSet)
                card.BackColor = Color.FromArgb(144, 238, 144); // Light Green
            else
                card.BackColor = Color.FromArgb(211, 211, 211); // Light Gray

            // Flow Name mit Herkunfts-Indikator
            var lblName = new Label();
            var namePrefix = isAncestorFlow ? "👤 " : "🎯 "; // Ancestor vs Original
            lblName.Text = namePrefix + (flow.FlowName ?? "Unnamed Flow");
            lblName.Font = new Font("Arial", 9, FontStyle.Bold);
            lblName.Location = new Point(5, 5);
            lblName.Size = new Size(290, 20);
            lblName.TextAlign = ContentAlignment.TopCenter;
            lblName.BackColor = Color.Transparent;
            card.Controls.Add(lblName);

            // Trigger Info
            var lblTrigger = new Label();
            if (flow.Trigger != null)
            {
                lblTrigger.Text = $"🎯 Trigger: {flow.Trigger.Entity}.{flow.Trigger.Field}";
            }
            else
            {
                lblTrigger.Text = "❌ No Trigger";
            }
            lblTrigger.Font = new Font("Arial", 8);
            lblTrigger.Location = new Point(5, 28);
            lblTrigger.Size = new Size(290, 18);
            lblTrigger.ForeColor = Color.DarkBlue;
            lblTrigger.BackColor = Color.Transparent;
            card.Controls.Add(lblTrigger);

            // ERWEITERTE Parent-Hierarchie Info
            var lblParentInfo = new Label();
            lblParentInfo.Text = BuildSimplifiedParentInfo(flow);
            lblParentInfo.Font = new Font("Arial", 7);
            lblParentInfo.Location = new Point(5, 48);
            lblParentInfo.Size = new Size(290, 35);
            lblParentInfo.ForeColor = Color.DarkMagenta;
            lblParentInfo.BackColor = Color.Transparent;
            card.Controls.Add(lblParentInfo);

            // Flow-Typ und Status Info
            var lblStatus = new Label();
            var statusParts = new List<string>();

            if (isAncestorFlow)
                statusParts.Add("👤 Ancestor Flow");
            else
                statusParts.Add("🎯 Original Flow");

            if (flow.IsFieldUsedAsTrigger) statusParts.Add("🔵 Uses Field as Trigger");
            if (flow.IsFieldSet) statusParts.Add("🟢 Sets Field");

            // Zähle direkte Children
            var directChildren = CountDirectChildren(flow);
            if (directChildren > 0)
                statusParts.Add($"👶 {directChildren} Direct Children");

            lblStatus.Text = string.Join("\n", statusParts);
            lblStatus.Font = new Font("Arial", 7);
            lblStatus.Location = new Point(5, 87);
            lblStatus.Size = new Size(290, 40);
            lblStatus.ForeColor = Color.DarkGreen;
            lblStatus.BackColor = Color.Transparent;
            card.Controls.Add(lblStatus);

            // KORRIGIERTE Button-Positionierung
            var btnOpen = new Button();
            btnOpen.Text = "Open Flow";
            btnOpen.Size = new Size(90, 28);
            btnOpen.Location = new Point(105, 145); // Zentriert horizontal, am unteren Rand
            btnOpen.BackColor = Color.LightSkyBlue;
            btnOpen.FlatStyle = FlatStyle.Flat;
            btnOpen.Click += (sender, e) => {
                if (!string.IsNullOrEmpty(flow.FlowUrl))
                {
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = flow.FlowUrl,
                            UseShellExecute = true
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Could not open flow: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("No URL available for this flow.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };
            card.Controls.Add(btnOpen);

            return card;
        }

        // VEREINFACHTE METHODE: Erstellt kompakte Parent-Info
        private string BuildSimplifiedParentInfo(FlowUsage flow)
        {
            if (flow.Parents == null || flow.Parents.Count == 0)
                return "🌱 Root Flow (No Parents)";

            var parentNames = flow.Parents.Take(2).Select(p => p.FlowName ?? "Unknown").ToList();
            var result = $"📋 Parents: {string.Join(", ", parentNames)}";

            if (flow.Parents.Count > 2)
                result += $" (+{flow.Parents.Count - 2} more)";

            return result;
        }

        // NEUE METHODE: Zählt direkte Children
        private int CountDirectChildren(FlowUsage flow)
        {
            return allRelevantFlows.Count(f =>
                f.Parents != null && f.Parents.Any(p => p.FlowID == flow.FlowID));
        }
    }
}