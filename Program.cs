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

        public Lien(Noeud cible)
        {
            Cible = cible;
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

        public void AjouterLien(string source, string cible)
        {
            if (Noeuds.ContainsKey(source) && Noeuds.ContainsKey(cible))
            {
                Noeuds[source].Liens.Add(new Lien(Noeuds[cible]));
                Noeuds[cible].Liens.Add(new Lien(Noeuds[source]));
            }
            else
            {
                Console.WriteLine("Erreur : Un des nœuds spécifiés n'existe pas.");
            }
        }

        public void AfficherListeAdjacence()
        {
            Console.WriteLine("Liste d'adjacence :");
            foreach (var noeud in Noeuds.OrderBy(n => int.Parse(n.Key)))
            {
                Console.Write(noeud.Key + " -> ");
                foreach (var lien in noeud.Value.Liens)
                {
                    Console.Write(lien.Cible.Nom + " ");
                }
                Console.WriteLine();
            }
        }

        public void AfficherMatriceAdjacence()
        {
            Console.WriteLine("\nMatrice d'adjacence :");

            var noeudsTries = Noeuds.Keys.Select(int.Parse).OrderBy(x => x).Select(x => x.ToString()).ToList();

            Console.Write("    ");
            foreach (var nom in noeudsTries)
            {
                Console.Write(nom.PadLeft(3) + " ");
            }
            Console.WriteLine();

            foreach (var ligne in noeudsTries)
            {
                Console.Write(ligne.PadLeft(3) + " ");
                foreach (var colonne in noeudsTries)
                {
                    if (Noeuds[ligne].Liens.Any(l => l.Cible.Nom == colonne))
                        Console.Write(" 1 ");
                    else
                        Console.Write(" 0 ");
                }
                Console.WriteLine();
            }
        }

        public void ParcoursLargeur(string depart)
        {
            if (!Noeuds.ContainsKey(depart))
            {
                Console.WriteLine($"Le nœud '{depart}' n'existe pas.");
                return;
            }

            Queue<Noeud> file = new Queue<Noeud>();
            HashSet<string> visites = new HashSet<string>();

            file.Enqueue(Noeuds[depart]);
            visites.Add(depart);

            Console.WriteLine("Parcours en largeur :");
            while (file.Count > 0)
            {
                Noeud courant = file.Dequeue();
                Console.Write(courant.Nom + " ");

                foreach (var lien in courant.Liens)
                {
                    if (!visites.Contains(lien.Cible.Nom))
                    {
                        visites.Add(lien.Cible.Nom);
                        file.Enqueue(lien.Cible);
                    }
                }
            }
            Console.WriteLine();
        }

        public void ParcoursProfondeur(string depart)
        {
            if (!Noeuds.ContainsKey(depart))
            {
                Console.WriteLine($"Le nœud '{depart}' n'existe pas.");
                return;
            }

            Stack<Noeud> pile = new Stack<Noeud>();
            HashSet<string> visites = new HashSet<string>();

            pile.Push(Noeuds[depart]);

            Console.WriteLine("Parcours en profondeur :");
            while (pile.Count > 0)
            {
                Noeud courant = pile.Pop();
                if (!visites.Contains(courant.Nom))
                {
                    visites.Add(courant.Nom);
                    Console.Write(courant.Nom + " ");

                    foreach (var lien in courant.Liens)
                    {
                        if (!visites.Contains(lien.Cible.Nom))
                        {
                            pile.Push(lien.Cible);
                        }
                    }
                }
            }
            Console.WriteLine();
        }
    }

    class Program
    {
        static void Main()
        {
            string mtxFilePath = "soc-karate.mtx";
            Graphe g = new Graphe();
            List<(int, int)> liens = new List<(int, int)>();

            using (StreamReader sr = new StreamReader(mtxFilePath))
            {
                string? line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith("%") || string.IsNullOrWhiteSpace(line))
                        continue;

                    string[] parts = line.Split(' ');
                    if (parts.Length == 2)
                    {
                        int source = int.Parse(parts[0]);
                        int cible = int.Parse(parts[1]);
                        g.AjouterNoeud(source.ToString());
                        g.AjouterNoeud(cible.ToString());
                        liens.Add((source, cible));
                    }
                }
            }

            foreach (var (source, cible) in liens)
            {
                g.AjouterLien(source.ToString(), cible.ToString());
            }

            Console.WriteLine("Comment voulez-vous représenter le graphe ?");
            Console.WriteLine("1 - Liste d'adjacence");
            Console.WriteLine("2 - Matrice d'adjacence");
            Console.Write("Votre choix : ");
            string choixAffichage = Console.ReadLine();

            if (choixAffichage == "1")
                g.AfficherListeAdjacence();
            else if (choixAffichage == "2")
                g.AfficherMatriceAdjacence();
            else
                Console.WriteLine("Choix invalide, affichage par défaut en liste d'adjacence.");

            Console.WriteLine("\nChoisissez un type de parcours :");
            Console.WriteLine("1 - Parcours en largeur");
            Console.WriteLine("2 - Parcours en profondeur");
            Console.Write("Votre choix : ");
            string choix = Console.ReadLine();

            Console.Write("Entrez le nœud de départ : ");
            string depart = Console.ReadLine();

            if (choix == "1")
                g.ParcoursLargeur(depart);
            else if (choix == "2")
                g.ParcoursProfondeur(depart);
            else
                Console.WriteLine("Choix invalide.");

            Console.WriteLine("Appuyez sur une touche pour fermer...");
            Console.ReadKey();
        }
    }
}

/// MANQUE : aligner la matrice, interface graphique
