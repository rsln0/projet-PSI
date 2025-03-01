using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LivInParisGraph
{
    public class Noeud
    {
        public string Nom { get; set; }
        public List<Lien> Liens { get; set; }

        public Noeud(string nom)
        {
            Nom = nom;
            Liens = new List<Lien>();
        }
    }

    public class Lien
    {
        public Noeud Cible { get; set; }
        public int Poids { get; set; }

        public Lien(Noeud cible, int poids)
        {
            Cible = cible;
            Poids = poids;
        }
    }

    public class Graphe
    {
        public Dictionary<string, Noeud> Noeuds { get; private set; }

        public Graphe()
        {
            Noeuds = new Dictionary<string, Noeud>();
        }

        public void AjouterNoeud(string nom)
        {
            if (!Noeuds.ContainsKey(nom))
                Noeuds[nom] = new Noeud(nom);
        }

        public void AjouterLien(string source, string cible, int poids)
        {
            if (Noeuds.ContainsKey(source) && Noeuds.ContainsKey(cible))
            {
                Noeuds[source].Liens.Add(new Lien(Noeuds[cible], poids));
            }
            else
            {
                Console.WriteLine("Erreur : Un des nœuds spécifiés n'existe pas.");
            }
        }

        // Parcours en largeur (BFS)
        public void ParcoursLargeur(string depart)
        {
            if (!Noeuds.ContainsKey(depart))
            {
                Console.WriteLine($"Le nœud '{depart}' n'existe pas dans le graphe.");
                return;
            }

            List<Noeud> file = new List<Noeud>();         // File d'attente (FIFO)
            List<string> visites = new List<string>();      // Liste des nœuds visités

            file.Add(Noeuds[depart]);
            visites.Add(depart);

            while (file.Count > 0)
            {
                Noeud courant = file[0];
                file.RemoveAt(0);
                Console.Write(courant.Nom + " ");

                foreach (var lien in courant.Liens)
                {
                    if (!visites.Contains(lien.Cible.Nom))
                    {
                        visites.Add(lien.Cible.Nom);
                        file.Add(lien.Cible);
                    }
                }
            }
            Console.WriteLine();
        }

        // Parcours en profondeur (DFS)
        public void ParcoursProfondeur(string depart)
        {
            if (!Noeuds.ContainsKey(depart))
            {
                Console.WriteLine($"Le nœud '{depart}' n'existe pas dans le graphe.");
                return;
            }

            HashSet<string> visites = new HashSet<string>();
            DFS(Noeuds[depart], visites);
            Console.WriteLine();
        }

        private void DFS(Noeud courant, HashSet<string> visites)
        {
            Console.Write(courant.Nom + " ");
            visites.Add(courant.Nom);

            foreach (var lien in courant.Liens)
            {
                if (!visites.Contains(lien.Cible.Nom))
                {
                    DFS(lien.Cible, visites);
                }
            }
        }

        // Affichage de la liste d'adjacence
        public void AfficherListeAdjacence()
        {
            Console.WriteLine("Liste d'adjacence:");
            // Tri des nœuds selon leur nom (numérique si possible)
            var sortedKeys = Noeuds.Keys.ToList();
            sortedKeys.Sort((a, b) =>
            {
                if (int.TryParse(a, out int ia) && int.TryParse(b, out int ib))
                    return ia.CompareTo(ib);
                return a.CompareTo(b);
            });
            foreach (var key in sortedKeys)
            {
                Console.Write(key + " : ");
                foreach (var lien in Noeuds[key].Liens)
                {
                    Console.Write(lien.Cible.Nom + " ");
                }
                Console.WriteLine();
            }
        }

        // Affichage de la matrice d'adjacence
        public void AfficherMatriceAdjacence()
        {
            Console.WriteLine("Matrice d'adjacence:");
            var sortedKeys = Noeuds.Keys.ToList();
            sortedKeys.Sort((a, b) =>
            {
                if (int.TryParse(a, out int ia) && int.TryParse(b, out int ib))
                    return ia.CompareTo(ib);
                return a.CompareTo(b);
            });
            int n = sortedKeys.Count;

            // Création d'une correspondance entre nœuds et indices
            Dictionary<string, int> indexMap = new Dictionary<string, int>();
            for (int i = 0; i < n; i++)
            {
                indexMap[sortedKeys[i]] = i;
            }

            // Initialisation de la matrice
            int[,] matrix = new int[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    matrix[i, j] = 0;

            // Remplissage de la matrice
            foreach (var key in sortedKeys)
            {
                int i = indexMap[key];
                foreach (var lien in Noeuds[key].Liens)
                {
                    int j = indexMap[lien.Cible.Nom];
                    matrix[i, j] = lien.Poids;
                }
            }

            // Affichage de la matrice
            Console.Write("     ");
            foreach (var key in sortedKeys)
            {
                Console.Write(key.PadLeft(4));
            }
            Console.WriteLine();
            for (int i = 0; i < n; i++)
            {
                Console.Write(sortedKeys[i].PadLeft(4));
                for (int j = 0; j < n; j++)
                {
                    Console.Write(matrix[i, j].ToString().PadLeft(4));
                }
                Console.WriteLine();
            }
        }
    }

    class Program
    {
        static void Main()
        {
            string mtxFilePath = "soc-karate.mtx";

            Graphe g = new Graphe();

            // Lecture du fichier .mtx et instanciation du graphe
            // Chaque ligne (non commentée) contient deux nombres représentant une relation réciproque.
            using (StreamReader sr = new StreamReader(mtxFilePath))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith("%") || string.IsNullOrWhiteSpace(line))
                        continue;

                    string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                    {
                        int source = int.Parse(parts[0]);
                        int cible = int.Parse(parts[1]);
                        // Ajouter les nœuds s'ils n'existent pas déjà
                        g.AjouterNoeud(source.ToString());
                        g.AjouterNoeud(cible.ToString());
                        // Ajout du lien dans les deux sens pour refléter la réciprocité
                        g.AjouterLien(source.ToString(), cible.ToString(), 1);
                        g.AjouterLien(cible.ToString(), source.ToString(), 1);
                    }
                }
            }

            // Choix du mode de représentation
            Console.WriteLine("Choisissez le mode de représentation :");
            Console.WriteLine("1 : Liste d'adjacence");
            Console.WriteLine("2 : Matrice d'adjacence");
            Console.Write("Votre choix : ");
            string choixMode = Console.ReadLine();

            if (choixMode == "1")
            {
                g.AfficherListeAdjacence();
            }
            else if (choixMode == "2")
            {
                g.AfficherMatriceAdjacence();
            }
            else
            {
                Console.WriteLine("Mode non reconnu. Affichage par défaut (Liste d'adjacence) :");
                g.AfficherListeAdjacence();
            }

            // Demande du sommet de départ pour les parcours
            Console.Write("Entrez le sommet de départ pour le parcours : ");
            string depart = Console.ReadLine();

            Console.WriteLine("Parcours en largeur (BFS) :");
            g.ParcoursLargeur(depart);

            Console.WriteLine("Parcours en profondeur (DFS) :");
            g.ParcoursProfondeur(depart);

            Console.WriteLine("Appuyez sur une touche pour fermer...");
            Console.ReadKey();
        }
    }
}
