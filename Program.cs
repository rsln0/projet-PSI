using System;
using System.Collections.Generic;
using System.IO;

namespace GraphEtudiantSimple
{
    /// Classe représentant un sommet du graphe
    public class Sommet
    {
        public string nom;
        public List<Connexion> connexions;

        public Sommet(string nomSommet)
        {
            nom = nomSommet;
            connexions = new List<Connexion>();
        }
    }

    /// Classe représentant une connexion entre deux sommets
    public class Connexion
    {
        public Sommet cible;

        public Connexion(Sommet sommetCible)
        {
            cible = sommetCible;
        }
    }

    /// Classe représentant le graphe
    public class Graphe
    {
        public Dictionary<string, Sommet> sommets;

        public Graphe()
        {
            sommets = new Dictionary<string, Sommet>();
        }

        public void AjouterSommet(string nomSommet)
        {
            if (!sommets.ContainsKey(nomSommet))
            {
                sommets[nomSommet] = new Sommet(nomSommet);
            }
        }

        public void AjouterConnexion(string source, string destination)
        {
            if (sommets.ContainsKey(source) && sommets.ContainsKey(destination))
            {
                /// Ajout de la connexion dans les deux sens (graphe non orienté)
                sommets[source].connexions.Add(new Connexion(sommets[destination]));
                sommets[destination].connexions.Add(new Connexion(sommets[source]));
            }
            else
            {
                Console.WriteLine("Erreur : un des sommets n'existe pas.");
            }
        }

        public void AfficherListe()
        {
            Console.WriteLine("Liste d'adjacence :");
            /// Récupérer les noms des sommets dans liste
            List<string> liste = new List<string>();
            foreach (string cle in sommets.Keys)
            {
                liste.Add(cle);
            }
            /// Tri  
            for (int i = 0; i < liste.Count - 1; i++)
            {
                for (int j = i + 1; j < liste.Count; j++)
                {
                    if (int.Parse(liste[i]) > int.Parse(liste[j]))
                    {
                        string a = liste[i];
                        liste[i] = liste[j];
                        liste[j] = a;
                    }
                }
            }

            /// Affichage de la liste d'adjacence
            for (int i = 0; i < liste.Count; i++)
            {
                string nomSommet = liste[i];
                Console.Write(nomSommet + " -> ");
                for (int j = 0; j < sommets[nomSommet].connexions.Count; j++)
                {
                    Console.Write(sommets[nomSommet].connexions[j].cible.nom + " ");
                }
                Console.WriteLine();
            }
        }

        public void AfficherMatrice()
        {
            Console.WriteLine("\nMatrice d'adjacence :");
            /// Récupérer les noms des sommets dans une liste
            List<string> liste = new List<string>();
            foreach (string cle in sommets.Keys)
            {
                liste.Add(cle);
            }
            /// Tri 
            for (int i = 0; i < liste.Count - 1; i++)
            {
                for (int j = i + 1; j < liste.Count; j++)
                {
                    if (int.Parse(liste[i]) > int.Parse(liste[j]))
                    {
                        string temp = liste[i];
                        liste[i] = liste[j];
                        liste[j] = temp;
                    }
                }
            }

            /// Affichage de la matrice
            Console.Write("     ");
            for (int i = 0; i < liste.Count; i++)
            {
                Console.Write(liste[i].PadLeft(5));
            }
            Console.WriteLine();

            /// Affichage de chaque ligne de la matrice
            for (int i = 0; i < liste.Count; i++)
            {
                Console.Write(liste[i].PadLeft(5));
                for (int j = 0; j < liste.Count; j++)
                {
                    bool trouve = false;
                    for (int k = 0; k < sommets[liste[i]].connexions.Count; k++)
                    {
                        if (sommets[liste[i]].connexions[k].cible.nom == liste[j])
                        {
                            trouve = true;
                            break;
                        }
                    }
                    if (trouve)
                        Console.Write("    1");
                    else
                        Console.Write("    0");
                }
                Console.WriteLine();
            }
        }

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

                for (int i = 0; i < actuel.connexions.Count; i++)
                {
                    string nomCible = actuel.connexions[i].cible.nom;
                    if (!vus.Contains(nomCible))
                    {
                        vus.Add(nomCible);
                        file.Enqueue(actuel.connexions[i].cible);
                    }
                }
            }
            Console.WriteLine();
        }

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

                    for (int i = 0; i < actuel.connexions.Count; i++)
                    {
                        string nomCible = actuel.connexions[i].cible.nom;
                        if (!vus.Contains(nomCible))
                        {
                            pile.Push(actuel.connexions[i].cible);
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
            string cheminFichier = "soc-karate.mtx";
            Graphe monGraphe = new Graphe();
            List<Tuple<int, int>> listeConnexions = new List<Tuple<int, int>>();

            /// Lecture du fichier
            StreamReader fichier = new StreamReader(cheminFichier);
            string ligne;
            while ((ligne = fichier.ReadLine()) != null)
            {
                if (ligne.StartsWith("%") || ligne.Trim() == "")
                {
                    continue;
                }
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

            /// Ajout des connexions
            for (int i = 0; i < listeConnexions.Count; i++)
            {
                monGraphe.AjouterConnexion(listeConnexions[i].Item1.ToString(), listeConnexions[i].Item2.ToString());
            }

            Console.WriteLine("Comment voulez-vous afficher le graphe ?");
            Console.WriteLine("1 - Liste d'adjacence");
            Console.WriteLine("2 - Matrice d'adjacence");
            Console.Write("Votre choix : ");
            string choixAffichage = Console.ReadLine();
            if (choixAffichage == "1")
                monGraphe.AfficherListe();
            else if (choixAffichage == "2")
                monGraphe.AfficherMatrice();
            else
                Console.WriteLine("Choix invalide, affichage par défaut en liste d'adjacence.");

            Console.WriteLine("\nQuel type de parcours souhaitez-vous ?");
            Console.WriteLine("1 - Parcours en largeur");
            Console.WriteLine("2 - Parcours en profondeur");
            Console.Write("Votre choix : ");
            string choixParcours = Console.ReadLine();
            Console.Write("Entrez le sommet de départ : ");
            string depart = Console.ReadLine();
            if (choixParcours == "1")
                monGraphe.ParcourLargeur(depart);
            else if (choixParcours == "2")
                monGraphe.ParcourProfondeur(depart);
            else
                Console.WriteLine("Choix invalide.");

            Console.WriteLine("Appuyez sur une touche pour fermer...");
            Console.ReadKey();
        }
    }
}
