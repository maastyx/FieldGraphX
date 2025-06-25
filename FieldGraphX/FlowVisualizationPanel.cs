using FieldGraphX.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

public class FlowVisualizationPanel : Panel
{
    private List<FlowUsage> flows;
    private Dictionary<Guid, FlowCardControl> flowCards;
    private List<ConnectionLine> connections;

    public FlowVisualizationPanel()
    {
        this.AutoScroll = true;
        this.BackColor = Color.White;
        this.flowCards = new Dictionary<Guid, FlowCardControl>();
        this.connections = new List<ConnectionLine>();
        this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
    }

    public void VisualizeHierarchy(List<FlowUsage> hierarchy)
    {
        this.flows = hierarchy;
        this.Controls.Clear();
        this.flowCards.Clear();
        this.connections.Clear();

        // Erstelle Flow-Karten
        CreateFlowCards();

        // Berechne Layout
        CalculateLayout();

        // Erstelle Verbindungen
        CreateConnections();

        this.Invalidate();
    }

    private void CreateFlowCards()
    {
        foreach (var flow in flows)
        {
            var card = new FlowCardControl(flow);
            flowCards[flow.FlowID] = card;
            this.Controls.Add(card);
        }
    }

    private void CalculateLayout()
    {
        const int cardWidth = 200;
        const int cardHeight = 120;
        const int horizontalSpacing = 50;
        const int verticalSpacing = 50;
        const int startX = 20;
        const int startY = 20;

        // Gruppiere Flows nach Hierarchie-Ebenen
        var levels = new Dictionary<int, List<FlowUsage>>();
        var processedFlows = new HashSet<Guid>();

        // Level 0: Root-Flows (keine Parents)
        var rootFlows = flows.Where(f => f.Parents == null || f.Parents.Count == 0).ToList();
        levels[0] = rootFlows;

        // Weitere Levels basierend auf Parent-Beziehungen
        int currentLevel = 0;
        while (levels.ContainsKey(currentLevel))
        {
            var currentLevelFlows = levels[currentLevel];
            var nextLevelFlows = new List<FlowUsage>();

            foreach (var parentFlow in currentLevelFlows)
            {
                processedFlows.Add(parentFlow.FlowID);

                // Finde Child-Flows
                var childFlows = flows.Where(f =>
                    f.Parents != null &&
                    f.Parents.Any(p => p.FlowID == parentFlow.FlowID) &&
                    !processedFlows.Contains(f.FlowID)).ToList();

                nextLevelFlows.AddRange(childFlows);
            }

            if (nextLevelFlows.Count > 0)
            {
                levels[currentLevel + 1] = nextLevelFlows.Distinct().ToList();
            }

            currentLevel++;
        }

        // Wenn keine Levels gefunden wurden, alle Flows auf Level 0 setzen
        if (levels.Count == 0 || levels[0].Count == 0)
        {
            levels[0] = flows.ToList();
        }

        // Positioniere die Karten
        int maxWidth = 0;
        int maxHeight = 0;

        foreach (var level in levels)
        {
            int levelIndex = level.Key;
            var levelFlows = level.Value;

            if (levelFlows.Count == 0) continue;

            int y = startY + levelIndex * (cardHeight + verticalSpacing);
            int totalWidth = levelFlows.Count * cardWidth + (levelFlows.Count - 1) * horizontalSpacing;
            int startXForLevel = startX;

            for (int i = 0; i < levelFlows.Count; i++)
            {
                var flow = levelFlows[i];
                if (flowCards.ContainsKey(flow.FlowID))
                {
                    var card = flowCards[flow.FlowID];
                    int x = startXForLevel + i * (cardWidth + horizontalSpacing);

                    card.Location = new Point(x, y);
                    card.Size = new Size(cardWidth, cardHeight);
                    card.Visible = true;

                    maxWidth = Math.Max(maxWidth, x + cardWidth);
                    maxHeight = Math.Max(maxHeight, y + cardHeight);
                }
            }
        }

        // Setze die Scroll-Größe
        this.AutoScrollMinSize = new Size(maxWidth + 40, maxHeight + 40);
    }

    private void CreateConnections()
    {
        connections.Clear();

        foreach (var flow in flows)
        {
            if (flow.Parents != null && flowCards.ContainsKey(flow.FlowID))
            {
                var childCard = flowCards[flow.FlowID];

                foreach (var parent in flow.Parents)
                {
                    if (flowCards.ContainsKey(parent.FlowID))
                    {
                        var parentCard = flowCards[parent.FlowID];

                        var connection = new ConnectionLine
                        {
                            StartPoint = new Point(
                                parentCard.Location.X + parentCard.Width / 2,
                                parentCard.Location.Y + parentCard.Height
                            ),
                            EndPoint = new Point(
                                childCard.Location.X + childCard.Width / 2,
                                childCard.Location.Y
                            ),
                            ConnectionType = ConnectionType.ParentChild,
                            Color = Color.Blue
                        };

                        connections.Add(connection);
                    }
                }
            }

            // Trigger-Verbindungen
            if (flow.IsFieldUsedAsTrigger && flow.Trigger != null)
            {
                var triggerFlows = flows.Where(f =>
                    f.IsFieldSet &&
                    f.FlowID != flow.FlowID &&
                    flowCards.ContainsKey(f.FlowID)).ToList();

                if (flowCards.ContainsKey(flow.FlowID))
                {
                    var triggerCard = flowCards[flow.FlowID];

                    foreach (var setterFlow in triggerFlows)
                    {
                        var setterCard = flowCards[setterFlow.FlowID];

                        var connection = new ConnectionLine
                        {
                            StartPoint = new Point(
                                setterCard.Location.X + setterCard.Width,
                                setterCard.Location.Y + setterCard.Height / 2
                            ),
                            EndPoint = new Point(
                                triggerCard.Location.X,
                                triggerCard.Location.Y + triggerCard.Height / 2
                            ),
                            ConnectionType = ConnectionType.Trigger,
                            Color = Color.Red
                        };

                        connections.Add(connection);
                    }
                }
            }
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        // Berücksichtige AutoScroll-Offset
        var scrollOffset = this.AutoScrollPosition;
        e.Graphics.TranslateTransform(scrollOffset.X, scrollOffset.Y);

        // Zeichne Verbindungslinien
        using (var pen = new Pen(Color.Black, 2))
        {
            foreach (var connection in connections)
            {
                pen.Color = connection.Color;

                if (connection.ConnectionType == ConnectionType.Trigger)
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                }
                else
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
                }

                // Zeichne Linie
                e.Graphics.DrawLine(pen, connection.StartPoint, connection.EndPoint);

                // Zeichne Pfeilspitze
                DrawArrowHead(e.Graphics, pen, connection.StartPoint, connection.EndPoint);
            }
        }
    }

    private void DrawArrowHead(Graphics g, Pen pen, Point start, Point end)
    {
        const int arrowLength = 10;
        const double arrowAngle = Math.PI / 6; // 30 Grad

        double angle = Math.Atan2(end.Y - start.Y, end.X - start.X);

        Point arrowPoint1 = new Point(
            (int)(end.X - arrowLength * Math.Cos(angle - arrowAngle)),
            (int)(end.Y - arrowLength * Math.Sin(angle - arrowAngle))
        );

        Point arrowPoint2 = new Point(
            (int)(end.X - arrowLength * Math.Cos(angle + arrowAngle)),
            (int)(end.Y - arrowLength * Math.Sin(angle + arrowAngle))
        );

        g.DrawLine(pen, end, arrowPoint1);
        g.DrawLine(pen, end, arrowPoint2);
    }
}

public class FlowCardControl : Panel
{
    private FlowUsage flow;
    private Label lblFlowName;
    private Label lblTrigger;
    private Label lblStatus;

    public FlowCardControl(FlowUsage flow)
    {
        this.flow = flow;
        InitializeComponents();
        SetupLayout();
        UpdateContent();
    }

    private void InitializeComponents()
    {
        this.BorderStyle = BorderStyle.FixedSingle;
        this.BackColor = Color.LightGray;

        lblFlowName = new Label
        {
            Font = new Font("Arial", 9, FontStyle.Bold),
            ForeColor = Color.Black,
            TextAlign = ContentAlignment.TopCenter
        };

        lblTrigger = new Label
        {
            Font = new Font("Arial", 8),
            ForeColor = Color.DarkBlue
        };

        lblStatus = new Label
        {
            Font = new Font("Arial", 8),
            ForeColor = Color.DarkGreen
        };

        this.Controls.AddRange(new Control[] { lblFlowName, lblTrigger, lblStatus });
    }

    private void SetupLayout()
    {
        lblFlowName.Dock = DockStyle.Top;
        lblFlowName.Height = 30;

        lblTrigger.Dock = DockStyle.Top;
        lblTrigger.Height = 40;

        lblStatus.Dock = DockStyle.Fill;
    }

    private void UpdateContent()
    {
        lblFlowName.Text = flow.FlowName ?? "Unnamed Flow";

        if (flow.Trigger != null)
        {
            lblTrigger.Text = $"Trigger: {flow.Trigger.Entity}.{flow.Trigger.Field}";
        }
        else
        {
            lblTrigger.Text = "No Trigger";
        }

        var statusParts = new List<string>();
        if (flow.IsFieldUsedAsTrigger) statusParts.Add("Uses Field as Trigger");
        if (flow.IsFieldSet) statusParts.Add("Sets Field");
        if (flow.Parents?.Count > 0) statusParts.Add($"Has {flow.Parents.Count} Parent(s)");

        lblStatus.Text = statusParts.Count > 0 ? string.Join("\n", statusParts) : "No special status";

        // Farbe basierend auf Status
        if (flow.IsFieldUsedAsTrigger)
            this.BackColor = Color.LightCoral;
        else if (flow.IsFieldSet)
            this.BackColor = Color.LightGreen;
        else
            this.BackColor = Color.LightGray;
    }
}

public class ConnectionLine
{
    public Point StartPoint { get; set; }
    public Point EndPoint { get; set; }
    public ConnectionType ConnectionType { get; set; }
    public Color Color { get; set; }
}

public enum ConnectionType
{
    ParentChild,
    Trigger
}
