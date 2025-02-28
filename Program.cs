using System;
using System.Collections.Generic;

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
        private Dictionary<string, Noeud> noeuds;

        public Graphe()
        {
            noeuds = new Dictionary<string, Noeud>();
        }

        public void AjouterNoeud(string nom)
        {
            if (!noeuds.ContainsKey(nom))
                noeuds[nom] = new Noeud(nom);
        }

        public void AjouterLien(string source, string cible, int poids)
        {
            if (noeuds.ContainsKey(source) && noeuds.ContainsKey(cible))
            {
                noeuds[source].Liens.Add(new Lien(noeuds[cible], poids));
            }
            else
            {
                Console.WriteLine("Erreur : Un des nœuds spécifiés n'existe pas.");
            }
        }

        // Parcours en largeur (BFS) en utilisant une List pour simuler à la fois la file d'attente et l'ensemble des visites
        public void ParcoursLargeur(string depart)
        {
            if (!noeuds.ContainsKey(depart))
            {
                Console.WriteLine($"Le nœud '{depart}' n'existe pas dans le graphe.");
                return;
            }

            List<Noeud> file = new List<Noeud>();         // Simule la file d'attente (FIFO)
            List<string> visites = new List<string>();      // Simule un ensemble pour stocker les nœuds visités

            file.Add(noeuds[depart]);
            visites.Add(depart);

            while (file.Count > 0)
            {
                // Récupérer et retirer le premier élément de la "file"
                Noeud courant = file[0];
                file.RemoveAt(0);
                Console.WriteLine($"Visite : {courant.Nom}");

                foreach (var lien in courant.Liens)
                {
                    // Vérifier dans la List si le nœud a déjà été visité
                    if (!visites.Contains(lien.Cible.Nom))
                    {
                        visites.Add(lien.Cible.Nom);
                        file.Add(lien.Cible);
                    }
                }
            }
        }
    }

    class Program
    {
        static void Main()
        {
            Graphe g = new Graphe();
            g.AjouterNoeud("Client");
            g.AjouterNoeud("Cuisinier");
            g.AjouterNoeud("Commande");
            g.AjouterLien("Client", "Commande", 1);
            g.AjouterLien("Commande", "Cuisinier", 1);

            Console.WriteLine("Parcours en largeur depuis 'Client':");
            g.ParcoursLargeur("Client");

            Console.WriteLine("Appuyez sur une touche pour fermer...");
            Console.ReadKey();
        }
    }
}
