using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace LivInParisApp
{
    public partial class MetroGUI : Form
    {
        private Graphe<string> metroGraph;
        private Dictionary<string, PointF> stationCoordinates;
        private Dictionary<int, Color> lineColors;
        private List<Noeud<string>> pathToDisplay;
        // Champ pour le panneau de la carte (mapPanel)
        private Panel mapPanel;
        // Bouton pour déclencher l'affichage du plus court chemin
        private Button btnTrajet;

        public MetroGUI()
        {
            InitializeComponent();

            // Initialisation du graphe et des structures de données
            metroGraph = new Graphe<string>(true);
            stationCoordinates = new Dictionary<string, PointF>();
            lineColors = new Dictionary<int, Color>();
            pathToDisplay = new List<Noeud<string>>();

            // Définir les couleurs des lignes de métro
            InitializeLineColors();

            // Charger les données du métro
            LoadMetroData();

            // Configuration de l'interface utilisateur
            SetupUI();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // MetroGUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 800);
            this.Name = "MetroGUI";
            this.Text = "Paris Metro Map";
            this.ResumeLayout(false);
        }

        private void InitializeLineColors()
        {
            lineColors.Add(1, Color.FromArgb(255, 255, 206, 0));    // Ligne 1 : Jaune
            lineColors.Add(2, Color.FromArgb(255, 0, 0, 255));      // Ligne 2 : Bleu
            lineColors.Add(3, Color.FromArgb(255, 152, 107, 42));   // Ligne 3 : Marron
            lineColors.Add(4, Color.FromArgb(255, 190, 50, 137));   // Ligne 4 : Pourpre
            lineColors.Add(5, Color.FromArgb(255, 255, 137, 0));    // Ligne 5 : Orange
            lineColors.Add(6, Color.FromArgb(255, 118, 180, 67));   // Ligne 6 : Vert clair
            lineColors.Add(7, Color.FromArgb(255, 230, 134, 161));  // Ligne 7 : Rose
            lineColors.Add(8, Color.FromArgb(255, 144, 89, 156));   // Ligne 8 : Lilas
            lineColors.Add(9, Color.FromArgb(255, 180, 186, 10));   // Ligne 9 : Vert-jaune
            lineColors.Add(10, Color.FromArgb(255, 223, 176, 117)); // Ligne 10 : Marron clair
            lineColors.Add(11, Color.FromArgb(255, 142, 85, 0));    // Ligne 11 : Marron
            lineColors.Add(12, Color.FromArgb(255, 0, 150, 67));    // Ligne 12 : Vert
            lineColors.Add(13, Color.FromArgb(255, 140, 200, 255)); // Ligne 13 : Bleu clair
            lineColors.Add(14, Color.FromArgb(255, 103, 46, 144));  // Ligne 14 : Pourpre
        }

        private void LoadMetroData()
        {
            try
            {
                LoadStationsData("C:/Users/ruben/OneDrive/Documents/noeuds projet PSI.csv");
                LoadConnectionsData("C:/Users/ruben/OneDrive/Documents/arcs projet PSI.csv");
                metroGraph.ChargerCorrespondances();
                Console.WriteLine("Données du métro chargées avec succès.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading metro data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadStationsData(string fileName)
        {
            if (!File.Exists(fileName))
            {
                MessageBox.Show($"Fichier non trouvé : {fileName}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string[] lines = File.ReadAllLines(fileName);
            bool isFirstLine = true;
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
                    // On évite les doublons dans le dictionnaire
                    if (stationCoordinates.ContainsKey(stationId))
                        continue;
                    string lineId = data[1].Trim();
                    string stationName = data[2].Trim();
                    double longitude = double.Parse(data[3], CultureInfo.InvariantCulture);
                    double latitude = double.Parse(data[4], CultureInfo.InvariantCulture);

                    Noeud<string> node = new Noeud<string>(stationId, stationName, longitude, latitude, lineId);
                    metroGraph.AjouterNoeud(node);
                    // Facteur d'espacement doublé (de 5000 à 10000)
                    float x = (float)((longitude - 2.225) * 10000);
                    float y = (float)((48.90 - latitude) * 10000);
                    stationCoordinates.Add(stationId, new PointF(x, y));
                }
            }
        }

        private void LoadConnectionsData(string fileName)
        {
            if (!File.Exists(fileName))
            {
                MessageBox.Show($"Fichier non trouvé : {fileName}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string[] lines = File.ReadAllLines(fileName);
            bool isFirstLine = true;
            // Création d'un dictionnaire utilisant l'ID de la station comme clé.
            Dictionary<string, Noeud<string>> stationsById = metroGraph.Noeuds.ToDictionary(n => n.Data);
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
                    string prevId = string.IsNullOrEmpty(data[2]) ? null : data[2].Trim();
                    string nextId = string.IsNullOrEmpty(data[3]) ? null : data[3].Trim();
                    double travelTime = double.Parse(data[4], CultureInfo.InvariantCulture);
                    if (stationsById.TryGetValue(stationId, out Noeud<string> currentStation))
                    {
                        if (prevId != null && travelTime > 0 && stationsById.TryGetValue(prevId, out Noeud<string> prevStation))
                        {
                            metroGraph.AjouterLien(currentStation, prevStation, travelTime);
                        }
                        if (nextId != null && travelTime > 0 && stationsById.TryGetValue(nextId, out Noeud<string> nextStation))
                        {
                            metroGraph.AjouterLien(currentStation, nextStation, travelTime);
                        }
                    }
                }
            }
        }

        private void MapPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Dessiner les arcs entre les stations
            DrawMetroLines(g);

            // Dessiner le plus court chemin (s'il existe)
            DrawShortestPath(g);

            // Dessiner les stations
            DrawStations(g);
        }

        // Nouvelle fonction pour dessiner les lignes de métro
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
                            if (node.LigneMetro == neighbor.Key.LigneMetro && int.TryParse(node.LigneMetro, out int lineNumber))
                            {
                                Color lineColor = lineColors.TryGetValue(lineNumber, out Color color) ? color : Color.Gray;
                                pen = new Pen(lineColor, 3);
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

        // Nouvelle fonction pour dessiner les stations
        private void DrawStations(Graphics g)
        {
            foreach (var kv in stationCoordinates)
            {
                PointF pos = kv.Value;
                using (SolidBrush brush = new SolidBrush(Color.Black))
                {
                    g.FillEllipse(brush, pos.X - 5, pos.Y - 5, 10, 10);
                }
            }
        }

        // Nouvelle fonction pour dessiner le plus court chemin
        private void DrawShortestPath(Graphics g)
        {
            if (pathToDisplay != null && pathToDisplay.Count > 1)
            {
                // Utiliser une épaisseur 1.5 fois plus grande que celle des lignes standard (1.5 * 3 = 4.5)
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

                    // Marquer les stations du chemin avec un contour rouge
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

        private void SetupUI()
        {
            // Création du panneau de la carte
            mapPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            mapPanel.Paint += MapPanel_Paint;
            this.Controls.Add(mapPanel);

            // Création du bouton pour afficher le plus court chemin
            btnTrajet = new Button
            {
                Text = "Trajet: Plus Court Chemin",
                AutoSize = true,
                Location = new Point(10, 10)
            };
            btnTrajet.Click += BtnTrajet_Click;
            this.Controls.Add(btnTrajet);
            btnTrajet.BringToFront();
        }

        private void BtnTrajet_Click(object sender, EventArgs e)
        {
            // Pour cet exemple, des IDs de station en dur sont utilisés.
            // Veuillez remplacer "ST001" et "ST002" par des IDs présents dans votre CSV.
            string sourceId = "ST001";
            string destinationId = "ST002";
            ComputeAndDisplayShortestPath(sourceId, destinationId);
        }

        // Calcule le plus court chemin avec l'algorithme de Dijkstra et actualise l'affichage.
        public void ComputeAndDisplayShortestPath(string sourceId, string destinationId)
        {
            Noeud<string> source = metroGraph.Noeuds.FirstOrDefault(n => n.Data.Equals(sourceId));
            Noeud<string> destination = metroGraph.Noeuds.FirstOrDefault(n => n.Data.Equals(destinationId));

            if (source == null || destination == null)
            {
                MessageBox.Show("Station source ou de destination non trouvée.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Calcul du plus court chemin avec Dijkstra
            List<Noeud<string>> shortestPath = metroGraph.Dijkstra(source, destination);

            // Appel de la fonction qui affiche le plus court chemin
            DisplayShortestPath(shortestPath);
        }

        // Nouvelle fonction pour afficher le plus court chemin
        public void DisplayShortestPath(List<Noeud<string>> path)
        {
            // Mise à jour du chemin à afficher
            pathToDisplay = path;

            // Si le chemin est vide ou ne contient qu'un seul nœud, afficher un message
            if (pathToDisplay == null || pathToDisplay.Count <= 1)
            {
                MessageBox.Show("Aucun chemin trouvé entre ces stations.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Calculer le temps total du trajet
            double totalTime = 0;
            for (int i = 0; i < pathToDisplay.Count - 1; i++)
            {
                Noeud<string> current = pathToDisplay[i];
                Noeud<string> next = pathToDisplay[i + 1];

                if (current.Voisins.TryGetValue(next, out double time))
                {
                    totalTime += time;
                }
            }

            // Afficher les informations du trajet
            string stationsCount = pathToDisplay.Count.ToString();
            string travelTime = totalTime.ToString("F1");

            MessageBox.Show($"Trajet de {pathToDisplay.First().Nom} à {pathToDisplay.Last().Nom}\n" +
                           $"Nombre de stations: {stationsCount}\n" +
                           $"Temps de trajet estimé: {travelTime} minutes",
                           "Détails du trajet", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Actualiser l'affichage
            mapPanel.Invalidate();
        }
    }
}
