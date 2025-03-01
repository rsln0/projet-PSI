using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace GraphEtudiantSimple
{
    /// Classe représentant un sommet
    public class Sommet
    {
        /// Nom du sommet
        public string nom;
        /// Liste des connexions du sommet
        public List<Connexion> connexions;

        /// Constructeur
        public Sommet(string nomSommet)
        {
            nom = nomSommet;
            connexions = new List<Connexion>();
        }
    }

    /// Classe pour relier deux sommets
    public class Connexion
    {
        /// Sommet cible de la connexion
        public Sommet cible;

        /// Constructeur
        public Connexion(Sommet sommetCible)
        {
            cible = sommetCible;
        }
    }

    /// Classe du graphe
    public class Graphe
    {
        /// Dictionnaire des sommets
        public Dictionary<string, Sommet> sommets;

        /// Constructeur
        public Graphe()
        {
            sommets = new Dictionary<string, Sommet>();
        }

        /// Ajoute un sommet
        public void AjouterSommet(string nomSommet)
        {
            if (!sommets.ContainsKey(nomSommet))
                sommets[nomSommet] = new Sommet(nomSommet);
        }

        /// Ajoute une connexion dans les deux sens
        public void AjouterConnexion(string source, string destination)
        {
            if (sommets.ContainsKey(source) && sommets.ContainsKey(destination))
            {
                sommets[source].connexions.Add(new Connexion(sommets[destination]));
                sommets[destination].connexions.Add(new Connexion(sommets[source]));
            }
            else
                Console.WriteLine("Erreur : un des sommets n'existe pas.");
        }

        /// Affiche la liste d'adjacence
        public void AfficherListe()
        {
            Console.WriteLine("Liste d'adjacence :");
            List<string> liste = new List<string>(sommets.Keys);
            liste.Sort((a, b) => int.Parse(a).CompareTo(int.Parse(b)));
            foreach (string s in liste)
            {
                Console.Write(s + " -> ");
                foreach (var conn in sommets[s].connexions)
                    Console.Write(conn.cible.nom + " ");
                Console.WriteLine();
            }
        }

        /// Affiche la matrice d'adjacence
        public void AfficherMatrice()
        {
            Console.WriteLine("\nMatrice d'adjacence :");
            List<string> liste = new List<string>(sommets.Keys);
            liste.Sort((a, b) => int.Parse(a).CompareTo(int.Parse(b)));

            Console.Write("     ");
            foreach (string s in liste)
                Console.Write(s.PadLeft(5));
            Console.WriteLine();

            foreach (string s in liste)
            {
                Console.Write(s.PadLeft(5));
                foreach (string t in liste)
                {
                    bool trouve = sommets[s].connexions.Exists(c => c.cible.nom == t);
                    Console.Write(trouve ? "    1" : "    0");
                }
                Console.WriteLine();
            }
        }

        /// Parcours en largeur depuis un sommet
        public void ParcourLargeur(string depart)
        {
            if (!sommets.ContainsKey(depart))
            {
                Console.WriteLine("Le sommet " + depart + " n'existe pas.");
                return;
            }
            Queue<Sommet> file = new Queue<Sommet>();
            List<string> vus = new List<string>();

            file.Enqueue(sommets[depart]);
            vus.Add(depart);
            Console.WriteLine("Parcours en largeur :");
            while (file.Count > 0)
            {
                Sommet actuel = file.Dequeue();
                Console.Write(actuel.nom + " ");
                foreach (var conn in actuel.connexions)
                {
                    if (!vus.Contains(conn.cible.nom))
                    {
                        vus.Add(conn.cible.nom);
                        file.Enqueue(conn.cible);
                    }
                }
            }
            Console.WriteLine();
        }

        /// Parcours en profondeur depuis un sommet
        public void ParcourProfondeur(string depart)
        {
            if (!sommets.ContainsKey(depart))
            {
                Console.WriteLine("Le sommet " + depart + " n'existe pas.");
                return;
            }
            Stack<Sommet> pile = new Stack<Sommet>();
            List<string> vus = new List<string>();

            pile.Push(sommets[depart]);
            Console.WriteLine("Parcours en profondeur :");
            while (pile.Count > 0)
            {
                Sommet actuel = pile.Pop();
                if (!vus.Contains(actuel.nom))
                {
                    vus.Add(actuel.nom);
                    Console.Write(actuel.nom + " ");
                    foreach (var conn in actuel.connexions)
                        if (!vus.Contains(conn.cible.nom))
                            pile.Push(conn.cible);
                }
            }
            Console.WriteLine();
        }

        /// Crée une image du graphe et l'enregistre
        public void VisualiserGraphe(string fichier)
        {
            int largeur = 800, hauteur = 600;
            Bitmap bitmap = new Bitmap(largeur, hauteur);
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.White);
            Font font = new Font("Arial", 12);
            Brush brush = Brushes.Black;
            Random rnd = new Random();
            Dictionary<string, Point> positions = new Dictionary<string, Point>();

            foreach (var s in sommets.Keys)
                positions[s] = new Point(rnd.Next(50, largeur - 50), rnd.Next(50, hauteur - 50));

            foreach (var s in sommets.Values)
                foreach (var conn in s.connexions)
                    graphics.DrawLine(Pens.Black, positions[s.nom], positions[conn.cible.nom]);

            foreach (var s in sommets.Values)
            {
                Point pos = positions[s.nom];
                graphics.FillEllipse(Brushes.Blue, pos.X - 10, pos.Y - 10, 20, 20);
                graphics.DrawString(s.nom, font, brush, pos.X + 5, pos.Y + 5);
            }
            bitmap.Save(fichier, ImageFormat.Png);
            Console.WriteLine($"Graphe visualisé et enregistré sous {fichier}");
        }
    }

    /// Programme principal
    class Program
    {
        static void Main()
        {
            string cheminFichier = "soc-karate.mtx";
            Graphe monGraphe = new Graphe();
            List<Tuple<int, int>> listeConnexions = new List<Tuple<int, int>>();

            StreamReader fichier = new StreamReader(cheminFichier);
            string ligne;
            while ((ligne = fichier.ReadLine()) != null)
            {
                if (ligne.StartsWith("%") || ligne.Trim() == "")
                    continue;
                string[] morceaux = ligne.Split(' ');
                if (morceaux.Length == 2)
                {
                    int src = int.Parse(morceaux[0]);
                    int dest = int.Parse(morceaux[1]);
                    monGraphe.AjouterSommet(src.ToString());
                    monGraphe.AjouterSommet(dest.ToString());
                    listeConnexions.Add(new Tuple<int, int>(src, dest));
                }
            }
            fichier.Close();

            foreach (var tuple in listeConnexions)
                monGraphe.AjouterConnexion(tuple.Item1.ToString(), tuple.Item2.ToString());

            Console.WriteLine("Comment afficher le graphe ?");
            Console.WriteLine("1 - Liste d'adjacence");
            Console.WriteLine("2 - Matrice d'adjacence");
            Console.Write("Votre choix : ");
            string choixAffichage = Console.ReadLine();
            if (choixAffichage == "1")
                monGraphe.AfficherListe();
            else if (choixAffichage == "2")
                monGraphe.AfficherMatrice();
            else
                Console.WriteLine("Choix invalide, affichage en liste.");

            Console.WriteLine("\nChoisissez le type de parcours :");
            Console.WriteLine("1 - Parcours en largeur");
            Console.WriteLine("2 - Parcours en profondeur");
            Console.Write("Votre choix : ");
            string choixParcours = Console.ReadLine();
            Console.Write("Sommet de départ : ");
            string depart = Console.ReadLine();
            if (choixParcours == "1")
                monGraphe.ParcourLargeur(depart);
            else if (choixParcours == "2")
                monGraphe.ParcourProfondeur(depart);
            else
                Console.WriteLine("Choix invalide.");

            monGraphe.VisualiserGraphe("graphe.png");
            Console.WriteLine("Appuyez sur une touche pour fermer...");
            Console.ReadKey();
        }
    }
}
