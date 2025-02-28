using System;
using System.Collections.Generic;

namespace LivInParisGraph
{
    class Noeud
    {
        public string Nom;
        public List<Lien> Liens;

        public Noeud(string nom)
        {
            Nom = nom;
            Liens = new List<Lien>();
        }
    }

    class Lien
    {
        public Noeud Cible;
        public int Poids;

        public Lien(Noeud cible, int poids)
        {
            Cible = cible;
            Poids = poids;
        }
    }

    class Graphe
    {
        private List<Noeud> noeuds = new List<Noeud>();

        public void AjouterNoeud(string nom)
        {
            // Vérifie si le noeud existe déjà avec une boucle simple
            foreach (Noeud n in noeuds)
            {
                if (n.Nom == nom)
                {
                    return; // On ne l'ajoute pas s'il existe déjà
                }
            }
            noeuds.Add(new Noeud(nom));
        }

        public void AjouterLien(string source, string cible, int poids)
        {
            Noeud noeudSource = null;
            Noeud noeudCible = null;

            // Recherche des noeuds dans la liste
            foreach (Noeud n in noeuds)
            {
                if (n.Nom == source)
                    noeudSource = n;
                if (n.Nom == cible)
                    noeudCible = n;
            }

            // Ajoute le lien si les deux nœuds existent
            if (noeudSource != null && noeudCible != null)
            {
                noeudSource.Liens.Add(new Lien(noeudCible, poids));
            }
            else
            {
                Console.WriteLine("Erreur : Un des nœuds n'existe pas.");
            }
        }

        public void ParcoursLargeur(string depart)
        {
            Noeud noeudDepart = null;

            // Recherche du noeud de départ
            foreach (Noeud n in noeuds)
            {
                if (n.Nom == depart)
                {
                    noeudDepart = n;
                    break;
                }
            }

            if (noeudDepart == null)
            {
                Console.WriteLine("Le nœud de départ n'existe pas.");
                return;
            }

            List<Noeud> file = new List<Noeud>(); // Simule la file d'attente
            List<string> visites = new List<string>(); // Simule la liste des noeuds visités

            file.Add(noeudDepart);
            visites.Add(noeudDepart.Nom);

            while (file.Count > 0)
            {
                Noeud courant = file[0];
                file.RemoveAt(0);
                Console.WriteLine("Visite : " + courant.Nom);

                foreach (Lien lien in courant.Liens)
                {
                    // Vérifie si le nœud a déjà été visité
                    bool dejaVisite = false;
                    foreach (string v in visites)
                    {
                        if (v == lien.Cible.Nom)
                        {
                            dejaVisite = true;
                            break;
                        }
                    }

                    if (!dejaVisite)
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

            Console.WriteLine("Parcours en largeur depuis 'Client' :");
            g.ParcoursLargeur("Client");

            Console.WriteLine("Appuyez sur une touche pour fermer...");
            Console.ReadKey();
        }
    }
}
