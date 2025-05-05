using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace LivInParisApp
{
    public partial class Metro_GUI : Form
    {
        private Graphe<string> metroGraph;
        private Dictionary<string, PointF> stationCoordinates;
        private Dictionary<int, Color> lineColors;
        private List<Noeud<string>> pathToDisplay;
        private Panel mapPanel = null!;
        private Panel controlPanel = null!;
        private ComboBox cbSource = null!;
        private ComboBox cbDestination = null!;
        private Button btnTrajet = null!;
        private Button btnColoration = null!;

        // Variables pour la coloration de graphe
        private Dictionary<string, Color> nodeColors;
        private bool colorationActive = false;
        private System.Windows.Forms.Timer colorationTimer = null!;
        private int currentColorationStep = 0;
        private List<Tuple<Noeud<string>, Color>> colorationSteps;

        // Dictionnaire pour stocker les lignes par station
        private Dictionary<string, HashSet<string>> lignesParStation = new Dictionary<string, HashSet<string>>();

        // Buffer pour le dessin fluide
        private BufferedGraphicsContext bufferedGraphicsContext;
        private BufferedGraphics? bufferedGraphics;

        public Metro_GUI()
        {
            InitializeComponent();

            metroGraph = new Graphe<string>(false); // Graphe non orienté
            stationCoordinates = new Dictionary<string, PointF>();
            lineColors = new Dictionary<int, Color>();
            pathToDisplay = new List<Noeud<string>>();
            nodeColors = new Dictionary<string, Color>();
            colorationSteps = new List<Tuple<Noeud<string>, Color>>();

            bufferedGraphicsContext = BufferedGraphicsManager.Current;
            ResizeRedraw = true;

            InitializeLineColors();
            LoadMetroData();
            SetupUI();
            SetupColorationTimer();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // Metro_GUI
            // 
            this.AutoScaleDimensions = new SizeF(8F, 16F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1200, 800);
            this.Name = "Metro_GUI";
            this.Text = "Paris Metro Map";
            this.ResumeLayout(false);
        }

        private void InitializeLineColors()
        {
            lineColors.Add(1, Color.FromArgb(255, 255, 206, 0));
            lineColors.Add(2, Color.FromArgb(255, 0, 0, 255));
            lineColors.Add(3, Color.FromArgb(255, 152, 107, 42));
            lineColors.Add(4, Color.FromArgb(255, 190, 50, 137));
            lineColors.Add(5, Color.FromArgb(255, 255, 137, 0));
            lineColors.Add(6, Color.FromArgb(255, 118, 180, 67));
            lineColors.Add(7, Color.FromArgb(255, 230, 134, 161));
            lineColors.Add(8, Color.FromArgb(255, 144, 89, 156));
            lineColors.Add(9, Color.FromArgb(255, 180, 186, 10));
            lineColors.Add(10, Color.FromArgb(255, 223, 176, 117));
            lineColors.Add(11, Color.FromArgb(255, 142, 85, 0));
            lineColors.Add(12, Color.FromArgb(255, 0, 150, 67));
            lineColors.Add(13, Color.FromArgb(255, 140, 200, 255));
            lineColors.Add(14, Color.FromArgb(255, 103, 46, 144));
        }

        private void LoadMetroData()
        {
            LoadStationsData("C:/Users/ruben/OneDrive/Documents/noeuds projet PSI.csv");
            LoadConnectionsData("C:/Users/ruben/OneDrive/Documents/arcs projet PSI.csv");
            // Ne plus appeler metroGraph.ChargerCorrespondances() car nous gérons les correspondances nous-mêmes
        }

        private void LoadStationsData(string fileName)
        {
            if (!File.Exists(fileName))
            {
                MessageBox.Show($"Fichier non trouvé : {fileName}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string[] lines = File.ReadAllLines(fileName);
            bool isFirstLine = true;

            // Dictionnaire pour stocker les IDs de station par nom
            Dictionary<string, List<string>> stationIdsByName = new Dictionary<string, List<string>>();

            foreach (string line in lines)
            {
                if (isFirstLine)
                {
                    isFirstLine = false;
                    continue;
                }

                string[] data = line.Split(';');
                if (data.Length >= 7)
                {
                    string stationId = data[0].Trim();
                    string lineId = data[1].Trim();
                    string stationName = data[2].Trim();
                    double longitude = double.Parse(data[3], CultureInfo.InvariantCulture);
                    double latitude = double.Parse(data[4], CultureInfo.InvariantCulture);

                    if (!lignesParStation.ContainsKey(stationName))
                        lignesParStation[stationName] = new HashSet<string>();
                    lignesParStation[stationName].Add(lineId);

                    // Stocker l'ID de la station avec son nom pour les correspondances ultérieures
                    if (!stationIdsByName.ContainsKey(stationName))
                        stationIdsByName[stationName] = new List<string>();
                    stationIdsByName[stationName].Add(stationId);

                    // Créer le nœud dans le graphe
                    Noeud<string> node = new Noeud<string>(stationId, stationName, longitude, latitude, lineId);
                    metroGraph.AjouterNoeud(node);

                    float x = (float)((longitude - 2.225) * 10000);
                    float y = (float)((48.90 - latitude) * 10000);
                    stationCoordinates.Add(stationId, new PointF(x, y));
                    nodeColors.Add(stationId, Color.Black);
                }
            }

            // Après avoir chargé toutes les stations, créer les correspondances
            CreateTransferConnections(stationIdsByName);
        }

        private void CreateTransferConnections(Dictionary<string, List<string>> stationIdsByName)
        {
            // Pour chaque station qui apparaît sur plusieurs lignes
            foreach (var entry in stationIdsByName.Where(e => e.Value.Count > 1))
            {
                List<string> stationIds = entry.Value;

                // Créer des correspondances entre toutes les paires de lignes
                for (int i = 0; i < stationIds.Count; i++)
                {
                    for (int j = i + 1; j < stationIds.Count; j++)
                    {
                        string id1 = stationIds[i];
                        string id2 = stationIds[j];

                        // Trouver les nœuds correspondants
                        Noeud<string>? node1 = metroGraph.Noeuds.FirstOrDefault(n => n.Data == id1);
                        Noeud<string>? node2 = metroGraph.Noeuds.FirstOrDefault(n => n.Data == id2);

                        if (node1 != null && node2 != null)
                        {
                            // Temps de correspondance standard (à ajuster selon vos besoins)
                            double transferTime = 3.0;

                            // Ajouter la correspondance dans les deux sens
                            metroGraph.AjouterLien(node1, node2, transferTime);
                            metroGraph.AjouterLien(node2, node1, transferTime);
                        }
                    }
                }
            }
        }

        // Méthode LoadConnectionsData corrigée
        private void LoadConnectionsData(string fileName)
        {
            if (!File.Exists(fileName))
            {
                MessageBox.Show($"Fichier non trouvé : {fileName}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string[] lines = File.ReadAllLines(fileName);
            bool isFirstLine = true;

            Dictionary<string, Noeud<string>> stationsById = new Dictionary<string, Noeud<string>>();
            foreach (var node in metroGraph.Noeuds)
            {
                stationsById[node.Data] = node;
            }

            // HashSet pour éviter les doublons de connexion
            HashSet<Tuple<string, string>> connections = new HashSet<Tuple<string, string>>();

            foreach (string line in lines)
            {
                if (isFirstLine)
                {
                    isFirstLine = false;
                    continue;
                }

                string[] data = line.Split(';');
                if (data.Length >= 6)
                {
                    string stationId = data[0].Trim();
                    string? prevId = string.IsNullOrEmpty(data[2]) ? null : data[2].Trim();
                    string? nextId = string.IsNullOrEmpty(data[3]) ? null : data[3].Trim();
                    if (!double.TryParse(data[4].Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double travelTime))
                        continue;

                    if (stationsById.TryGetValue(stationId, out Noeud<string> currentStation))
                    {
                        // Connexion avec la station précédente
                        if (!string.IsNullOrEmpty(prevId) && travelTime > 0 &&
                            stationsById.TryGetValue(prevId, out Noeud<string>? prevStation))
                        {
                            string id1 = string.Compare(currentStation.Data, prevStation.Data) < 0 ? currentStation.Data : prevStation.Data;
                            string id2 = string.Compare(currentStation.Data, prevStation.Data) < 0 ? prevStation.Data : currentStation.Data;
                            var connection = Tuple.Create(id1, id2);

                            if (!connections.Contains(connection))
                            {
                                connections.Add(connection);
                                metroGraph.AjouterLien(currentStation, prevStation, travelTime);
                            }
                        }

                        // Connexion avec la station suivante
                        if (!string.IsNullOrEmpty(nextId) && travelTime > 0 &&
                            stationsById.TryGetValue(nextId, out Noeud<string>? nextStation))
                        {
                            string id1 = string.Compare(currentStation.Data, nextStation.Data) < 0 ? currentStation.Data : nextStation.Data;
                            string id2 = string.Compare(currentStation.Data, nextStation.Data) < 0 ? nextStation.Data : currentStation.Data;
                            var connection = Tuple.Create(id1, id2);

                            if (!connections.Contains(connection))
                            {
                                connections.Add(connection);
                                metroGraph.AjouterLien(currentStation, nextStation, travelTime);
                            }
                        }
                    }
                }
            }
        }

        private void SetupUI()
        {
            mapPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            mapPanel.Paint += MapPanel_Paint;
            mapPanel.Resize += MapPanel_Resize;
            this.Controls.Add(mapPanel);

            controlPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.LightGray
            };
            this.Controls.Add(controlPanel);

            Label lblSource = new Label
            {
                Text = "Départ:",
                Location = new Point(10, 15),
                AutoSize = true
            };
            controlPanel.Controls.Add(lblSource);

            cbSource = new ComboBox
            {
                Location = new Point(70, 10),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            controlPanel.Controls.Add(cbSource);

            Label lblDestination = new Label
            {
                Text = "Arrivée:",
                Location = new Point(290, 15),
                AutoSize = true
            };
            controlPanel.Controls.Add(lblDestination);

            cbDestination = new ComboBox
            {
                Location = new Point(360, 10),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            controlPanel.Controls.Add(cbDestination);

            var stations = metroGraph.Noeuds.OrderBy(n => n.Nom);
            foreach (var node in stations)
            {
                string stationLignes = GetLignesForStation(node.Nom);
                string stationInfo = $"{node.Nom} (Lignes: {stationLignes})";
                var item = new KeyValuePair<string, string>(node.Data, stationInfo);
                cbSource.Items.Add(item);
                cbDestination.Items.Add(item);
            }

            cbSource.DisplayMember = "Value";
            cbSource.ValueMember = "Key";
            cbDestination.DisplayMember = "Value";
            cbDestination.ValueMember = "Key";

            if (cbSource.Items.Count > 0)
                cbSource.SelectedIndex = 0;
            if (cbDestination.Items.Count > 0)
                cbDestination.SelectedIndex = (cbDestination.Items.Count > 1) ? 1 : 0;

            btnTrajet = new Button
            {
                Text = "Calculer Trajet",
                AutoSize = true,
                Location = new Point(580, 15)
            };
            btnTrajet.Click += BtnTrajet_Click;
            controlPanel.Controls.Add(btnTrajet);

            btnColoration = new Button
            {
                Text = "Coloration de graphe",
                AutoSize = true,
                Location = new Point(740, 15)
            };
            btnColoration.Click += BtnColoration_Click;
            controlPanel.Controls.Add(btnColoration);

            InitializeBufferedGraphics();
        }

        private string GetLignesForStation(string stationName)
        {
            if (lignesParStation.ContainsKey(stationName))
                return string.Join(", ", lignesParStation[stationName]);
            return string.Empty;
        }

        private void InitializeBufferedGraphics()
        {
            if (bufferedGraphicsContext == null || mapPanel == null)
                return;

            if (mapPanel.Width > 0 && mapPanel.Height > 0)
            {
                try
                {
                    if (bufferedGraphics != null)
                        bufferedGraphics.Dispose();
                    bufferedGraphics = bufferedGraphicsContext.Allocate(
                        mapPanel.CreateGraphics(),
                        new Rectangle(0, 0, mapPanel.Width, mapPanel.Height));
                    RenderMap(bufferedGraphics.Graphics);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'initialisation du buffer graphique: {ex.Message}",
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void MapPanel_Resize(object? sender, EventArgs e)
        {
            InitializeBufferedGraphics();
        }

        private void SetupColorationTimer()
        {
            colorationTimer = new System.Windows.Forms.Timer();
            colorationTimer.Interval = 100;
            colorationTimer.Tick += ColorationTimer_Tick;
        }

        private void ResetNodeColors()
        {
            foreach (var key in nodeColors.Keys.ToList())
            {
                nodeColors[key] = Color.Black;
            }
            RenderAndRefresh();
        }

        private void ApplyWelshPowellColoration()
        {
            ResetNodeColors();
            colorationSteps.Clear();

            // Construire le graphe de conflit basé sur les noms de stations
            Dictionary<string, HashSet<string>> conflictGraph = new Dictionary<string, HashSet<string>>();
            Dictionary<string, string> idToName = new Dictionary<string, string>();
            Dictionary<string, string> nameToId = new Dictionary<string, string>();

            // Associer chaque ID à un nom de station et vice versa
            foreach (var node in metroGraph.Noeuds)
            {
                idToName[node.Data] = node.Nom;
                if (!nameToId.ContainsKey(node.Nom))
                    nameToId[node.Nom] = node.Data;
                if (!conflictGraph.ContainsKey(node.Nom))
                    conflictGraph[node.Nom] = new HashSet<string>();
            }

            // Construire le graphe de conflit basé sur les noms
            foreach (var node in metroGraph.Noeuds)
            {
                foreach (var neighbor in node.Voisins.Keys)
                {
                    conflictGraph[node.Nom].Add(neighbor.Nom);
                }
            }

            // Trier les stations par degré décroissant
            var sortedStations = conflictGraph.Keys
                .OrderByDescending(name => conflictGraph[name].Count)
                .ToList();

            List<Color> availableColors = new List<Color>
            {
                Color.Red, Color.Blue, Color.Green, Color.Orange,
                Color.Purple, Color.Brown, Color.Cyan, Color.Magenta,
                Color.Pink, Color.LimeGreen, Color.Indigo, Color.Gold,
                Color.Teal, Color.Crimson, Color.DarkOliveGreen,
                Color.SteelBlue, Color.Maroon, Color.ForestGreen, Color.Sienna,
                Color.Orchid, Color.RoyalBlue, Color.SeaGreen, Color.Chocolate,
                Color.DarkViolet, Color.DodgerBlue, Color.OliveDrab, Color.Firebrick
            };

            // Colorier le graphe en fonction des noms
            Dictionary<string, Color> stationColors = new Dictionary<string, Color>();
            foreach (var stationName in sortedStations)
            {
                if (stationColors.ContainsKey(stationName))
                    continue;

                HashSet<Color> usedColors = new HashSet<Color>();
                foreach (string neighborName in conflictGraph[stationName])
                {
                    if (stationColors.TryGetValue(neighborName, out Color color))
                        usedColors.Add(color);
                }

                Color selectedColor = availableColors.FirstOrDefault(c => !usedColors.Contains(c));
                if (selectedColor.IsEmpty)
                    selectedColor = availableColors[0];

                stationColors[stationName] = selectedColor;

                // Affecter la même couleur aux stations compatibles
                foreach (var otherStationName in sortedStations)
                {
                    if (stationColors.ContainsKey(otherStationName))
                        continue;

                    if (!conflictGraph[stationName].Contains(otherStationName) &&
                        !conflictGraph[otherStationName].Contains(stationName))
                    {
                        bool canUseColor = true;
                        foreach (string neighborName in conflictGraph[otherStationName])
                        {
                            if (stationColors.TryGetValue(neighborName, out Color color) &&
                                color.ToArgb() == selectedColor.ToArgb())
                            {
                                canUseColor = false;
                                break;
                            }
                        }

                        if (canUseColor)
                            stationColors[otherStationName] = selectedColor;
                    }
                }
            }

            // Appliquer les couleurs aux nœuds du graphe en utilisant leur nom
            foreach (var node in metroGraph.Noeuds)
            {
                if (stationColors.TryGetValue(node.Nom, out Color color))
                {
                    colorationSteps.Add(new Tuple<Noeud<string>, Color>(node, color));
                }
            }
            currentColorationStep = 0;
        }

        private void ColorationTimer_Tick(object? sender, EventArgs e)
        {
            int nodesToColorPerTick = 5;
            bool colorationChanged = false;
            for (int i = 0; i < nodesToColorPerTick && currentColorationStep < colorationSteps.Count; i++)
            {
                var step = colorationSteps[currentColorationStep];
                nodeColors[step.Item1.Data] = step.Item2;
                currentColorationStep++;
                colorationChanged = true;
            }
            if (colorationChanged)
                RenderAndRefresh();
            if (currentColorationStep >= colorationSteps.Count)
                colorationTimer.Stop();
        }

        private void RenderAndRefresh()
        {
            if (bufferedGraphics == null)
            {
                try
                {
                    InitializeBufferedGraphics();
                    if (bufferedGraphics == null)
                    {
                        mapPanel.Invalidate();
                        return;
                    }
                }
                catch
                {
                    mapPanel.Invalidate();
                    return;
                }
            }
            try
            {
                RenderMap(bufferedGraphics.Graphics);
                bufferedGraphics.Render();
            }
            catch (Exception)
            {
                mapPanel.Invalidate();
            }
        }

        private void MapPanel_Paint(object? sender, PaintEventArgs e)
        {
            try
            {
                if (bufferedGraphics != null)
                    bufferedGraphics.Render(e.Graphics);
                else
                {
                    RenderMap(e.Graphics);
                    InitializeBufferedGraphics();
                }
            }
            catch (Exception)
            {
                RenderMap(e.Graphics);
            }
        }

        private void RenderMap(Graphics g)
        {
            g.Clear(Color.White);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            DrawMetroLines(g);
            DrawShortestPath(g);
            DrawStations(g);
        }

        private void DrawMetroLines(Graphics g)
        {
            foreach (var node in metroGraph.Noeuds)
            {
                if (stationCoordinates.TryGetValue(node.Data, out PointF nodePos))
                {
                    foreach (var neighbor in node.Voisins)
                    {
                        if (stationCoordinates.TryGetValue(neighbor.Key.Data, out PointF neighborPos))
                        {
                            Pen pen;
                            bool partageLigne = false;
                            if (lignesParStation.ContainsKey(node.Nom) && lignesParStation.ContainsKey(neighbor.Key.Nom))
                                partageLigne = lignesParStation[node.Nom].Intersect(lignesParStation[neighbor.Key.Nom]).Any();

                            if (partageLigne)
                            {
                                string? ligneCommuneStr = lignesParStation[node.Nom].Intersect(lignesParStation[neighbor.Key.Nom]).FirstOrDefault();
                                if (int.TryParse(ligneCommuneStr, out int ligneCommune) && lineColors.TryGetValue(ligneCommune, out Color color))
                                    pen = new Pen(color, 3);
                                else
                                    pen = new Pen(Color.Gray, 3);
                            }
                            else
                            {
                                pen = new Pen(Color.Black, 1);
                                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                            }

                            g.DrawLine(pen, nodePos, neighborPos);
                            pen.Dispose();
                        }
                    }
                }
            }
        }

        private void DrawStations(Graphics g)
        {
            foreach (var kv in stationCoordinates)
            {
                string stationId = kv.Key;
                PointF pos = kv.Value;
                Color fillColor = nodeColors.ContainsKey(stationId) ? nodeColors[stationId] : Color.Black;
                using (SolidBrush brush = new SolidBrush(fillColor))
                {
                    g.FillEllipse(brush, pos.X - 5, pos.Y - 5, 10, 10);
                }
                g.DrawEllipse(Pens.Black, pos.X - 5, pos.Y - 5, 10, 10);
            }
        }

        private void DrawShortestPath(Graphics g)
        {
            if (pathToDisplay != null && pathToDisplay.Count > 1)
            {
                using (Pen redPen = new Pen(Color.Red, 4.5f))
                {
                    for (int i = 0; i < pathToDisplay.Count - 1; i++)
                    {
                        string startKey = pathToDisplay[i].Data;
                        string endKey = pathToDisplay[i + 1].Data;
                        if (stationCoordinates.TryGetValue(startKey, out PointF startPos) &&
                            stationCoordinates.TryGetValue(endKey, out PointF endPos))
                        {
                            g.DrawLine(redPen, startPos, endPos);
                        }
                    }
                    foreach (var node in pathToDisplay)
                    {
                        if (stationCoordinates.TryGetValue(node.Data, out PointF pos))
                        {
                            g.DrawEllipse(redPen, pos.X - 6, pos.Y - 6, 12, 12);
                        }
                    }
                }
            }
        }

        private void BtnTrajet_Click(object? sender, EventArgs e)
        {
            if (cbSource.SelectedItem is KeyValuePair<string, string> sourceItem &&
                cbDestination.SelectedItem is KeyValuePair<string, string> destItem)
            {
                string sourceId = sourceItem.Key;
                string destinationId = destItem.Key;
                if (sourceId.Equals(destinationId))
                {
                    MessageBox.Show("Veuillez sélectionner des stations différentes.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                ComputeAndDisplayShortestPath(sourceId, destinationId);
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner les stations de départ et d'arrivée.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnColoration_Click(object? sender, EventArgs e)
        {
            if (!colorationActive)
            {
                ApplyWelshPowellColoration();
                colorationActive = true;
                colorationTimer.Start();
            }
            else
            {
                colorationTimer.Stop();
                ResetNodeColors();
                colorationActive = false;
            }
        }

        public void ComputeAndDisplayShortestPath(string sourceId, string destinationId)
        {
            Noeud<string>? source = metroGraph.Noeuds.FirstOrDefault(n => n.Data.Equals(sourceId));
            Noeud<string>? destination = metroGraph.Noeuds.FirstOrDefault(n => n.Data.Equals(destinationId));
            if (source == null || destination == null)
            {
                MessageBox.Show("Station source ou de destination non trouvée.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            List<Noeud<string>> shortestPath = metroGraph.Dijkstra(source, destination);
            DisplayShortestPath(shortestPath);
        }

        public void DisplayShortestPath(List<Noeud<string>> path)
        {
            pathToDisplay = path;
            if (pathToDisplay == null || pathToDisplay.Count <= 1)
            {
                MessageBox.Show("Aucun chemin trouvé entre ces stations.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            double totalTime = 0;
            string detailsTrajet = "";
            for (int i = 0; i < pathToDisplay.Count - 1; i++)
            {
                Noeud<string> current = pathToDisplay[i];
                Noeud<string> next = pathToDisplay[i + 1];
                if (current.Voisins.TryGetValue(next, out double time))
                {
                    totalTime += time;
                    bool estCorrespondance = false;
                    if (lignesParStation.ContainsKey(current.Nom) && lignesParStation.ContainsKey(next.Nom))
                        estCorrespondance = !lignesParStation[current.Nom].Intersect(lignesParStation[next.Nom]).Any();
                    string typeConnection = estCorrespondance ? "Correspondance" : "Même ligne";
                    detailsTrajet += $"{i + 1}. {current.Nom} ({GetLignesForStation(current.Nom)}) -> {next.Nom} ({GetLignesForStation(next.Nom)}): {time:F1} min ({typeConnection})\n";
                }
            }
            string details = $"Trajet de {pathToDisplay.First().Nom} à {pathToDisplay.Last().Nom}\n" +
                             $"Nombre de stations: {pathToDisplay.Count}\n" +
                             $"Temps de trajet estimé: {totalTime:F1} minutes\n\n" +
                             $"Détails du trajet:\n{detailsTrajet}";
            MessageBox.Show(details, "Détails du trajet", MessageBoxButtons.OK, MessageBoxIcon.Information);
            RenderAndRefresh();
        }
    }
}
