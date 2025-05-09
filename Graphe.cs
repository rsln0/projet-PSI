using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace LivInParisApp
{
    public class Graphe<T>
    {
        private Dictionary<int, double> _tempsChangementMapping = new Dictionary<int, double>();
        /// Dictionnaire pour stocker les stations uniques par nom
        private Dictionary<string, Noeud<T>> _stationsUniques = new Dictionary<string, Noeud<T>>();

        public List<Noeud<T>> Noeuds { get; private set; }
        public bool EstOriente { get; private set; }

        /// Dictionnaire pour suivre les lignes par station
        private Dictionary<string, HashSet<string>> _lignesParStation = new Dictionary<string, HashSet<string>>();

        public Graphe(bool estOriente = true)
        {
            Noeuds = new List<Noeud<T>>();
            EstOriente = estOriente;
        }

        public void AjouterNoeud(Noeud<T> noeud)
        {
            if (!Noeuds.Contains(noeud))
            {
                Noeuds.Add(noeud);
            }
        }

        public void AfficherGraphe()
        {
            Console.WriteLine("Contenu du graphe :");

            /// Trier les nœuds par ID pour l'affichage
            var noeudsTriesParId = Noeuds.OrderBy(n => n.LigneMetro).ToList();

            foreach (var noeud in noeudsTriesParId)
            {
                /// Afficher toutes les lignes associées à la station
                string lignes = GetLignesForStation(noeud.Nom);
                Console.WriteLine($"Station: {noeud.Nom} (Lignes: {lignes})");

                /// Trier les voisins par ID également
                var voisinsTriesParId = noeud.Voisins.OrderBy(v => v.Key.LigneMetro).ToList();

                foreach (var voisin in voisinsTriesParId)
                {
                    string typeConnection = "Liaison";
                    if (_lignesParStation.ContainsKey(noeud.Nom) && _lignesParStation.ContainsKey(voisin.Key.Nom))
                    {
                        /// Vérifier si les stations partagent une ligne commune
                        bool partageLigne = _lignesParStation[noeud.Nom].Intersect(_lignesParStation[voisin.Key.Nom]).Any();
                        typeConnection = partageLigne ? "Même ligne" : "Correspondance";
                    }
                    Console.WriteLine($"  -> Voisin: {voisin.Key.Nom}, Temps: {voisin.Value} min, Type: {typeConnection}");
                }

                Console.WriteLine(); /// Ligne vide pour améliorer la lisibilité
            }
        }

        private string GetLignesForStation(string nomStation)
        {
            if (_lignesParStation.ContainsKey(nomStation))
            {
                return string.Join(", ", _lignesParStation[nomStation]);
            }
            return string.Empty;
        }

        public void ChargerDonneesCSV(string cheminFichier)
        {
            try
            {
                string[] lignes = System.IO.File.ReadAllLines(cheminFichier);
                bool premiereLigne = true;
                Dictionary<int, int> idVersIdUnique = new Dictionary<int, int>();
                Dictionary<int, string> idVersNom = new Dictionary<int, string>();
                Dictionary<int, string> idVersLigne = new Dictionary<int, string>();
                _tempsChangementMapping.Clear();
                _stationsUniques.Clear();
                _lignesParStation.Clear();

                /// Première passe : créer les nœuds uniques par nom de station
                foreach (string ligne in lignes)
                {
                    if (premiereLigne)
                    {
                        premiereLigne = false;
                        continue; /// Ignorer l'en-tête
                    }

                    string[] donnees = ligne.Split(';');
                    if (donnees.Length >= 6)
                    {
                        int idStation = int.Parse(donnees[0]);
                        string nomStation = donnees[1];
                        string ligneMetro = donnees[2].Trim();
                        double tempsChangement = double.Parse(donnees[5], CultureInfo.InvariantCulture);

                        /// Enregistrer l'ID, le nom et la ligne pour une utilisation ultérieure
                        idVersNom[idStation] = nomStation;
                        idVersLigne[idStation] = ligneMetro;
                        _tempsChangementMapping[idStation] = tempsChangement;

                        /// Ajouter la ligne à la collection de lignes pour cette station
                        if (!_lignesParStation.ContainsKey(nomStation))
                        {
                            _lignesParStation[nomStation] = new HashSet<string>();
                        }
                        _lignesParStation[nomStation].Add(ligneMetro);

                        /// Créer ou récupérer le nœud unique pour cette station
                        if (!_stationsUniques.ContainsKey(nomStation))
                        {
                            /// Pour le premier nœud d'une station, utiliser sa ligne comme ligne principale
                            Noeud<T> nouveauNoeud = new Noeud<T>(default(T), nomStation, 0, 0, ligneMetro);
                            _stationsUniques.Add(nomStation, nouveauNoeud);
                            AjouterNoeud(nouveauNoeud);
                        }

                        /// Associer l'ID original au nœud unique
                        idVersIdUnique[idStation] = 0; /// Valeur provisoire, on utilisera le nom comme clé unique
                    }
                }

                /// Deuxième passe : créer les connexions entre les stations
                premiereLigne = true;
                foreach (string ligne in lignes)
                {
                    if (premiereLigne)
                    {
                        premiereLigne = false;
                        continue;
                    }

                    string[] donnees = ligne.Split(';');
                    if (donnees.Length >= 6)
                    {
                        int idStation = int.Parse(donnees[0]);
                        int? idPrecedent = string.IsNullOrEmpty(donnees[3]) ? (int?)null : int.Parse(donnees[3]);
                        int? idSuivant = string.IsNullOrEmpty(donnees[4]) ? (int?)null : int.Parse(donnees[4]);
                        double tempsEntreStations = double.Parse(donnees[4], CultureInfo.InvariantCulture);

                        string nomStationActuelle = idVersNom[idStation];
                        string ligneStationActuelle = idVersLigne[idStation];
                        Noeud<T> stationActuelle = _stationsUniques[nomStationActuelle];

                        /// Ajouter le lien vers la station précédente si elle existe
                        if (idPrecedent.HasValue && tempsEntreStations > 0)
                        {
                            if (idVersNom.ContainsKey(idPrecedent.Value))
                            {
                                string nomStationPrecedente = idVersNom[idPrecedent.Value];
                                Noeud<T> stationPrecedente = _stationsUniques[nomStationPrecedente];

                                /// Vérifier si les stations sont sur la même ligne
                                string ligneStationPrecedente = idVersLigne[idPrecedent.Value];
                                bool memeLigne = ligneStationActuelle == ligneStationPrecedente;

                                /// Ajouter le lien avec le temps approprié
                                AjouterLien(stationActuelle, stationPrecedente, tempsEntreStations);
                            }
                        }

                        /// Ajouter le lien vers la station suivante si elle existe
                        if (idSuivant.HasValue && tempsEntreStations > 0)
                        {
                            if (idVersNom.ContainsKey(idSuivant.Value))
                            {
                                string nomStationSuivante = idVersNom[idSuivant.Value];
                                Noeud<T> stationSuivante = _stationsUniques[nomStationSuivante];

                                /// Vérifier si les stations sont sur la même ligne
                                string ligneStationSuivante = idVersLigne[idSuivant.Value];
                                bool memeLigne = ligneStationActuelle == ligneStationSuivante;

                                /// Ajouter le lien avec le temps approprié
                                AjouterLien(stationActuelle, stationSuivante, tempsEntreStations);
                            }
                        }
                    }
                }

                /// On n'a plus besoin de créer explicitement les correspondances entre stations de même nom
                /// car nous avons maintenant une seule instance par station

                Console.WriteLine($"Chargement terminé. {_stationsUniques.Count} stations uniques créées avec leurs connexions.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement des données CSV: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        public void ChargerCorrespondances()
        {
            try
            {
                /// Cette méthode est maintenant simplifiée car nous avons déjà des stations uniques.
                /// Il suffit d'assurer que les connexions entre stations de différentes lignes
                /// aient le bon temps de correspondance.
                int correspondancesCreees = 0;

                /// Pour chaque station avec plusieurs lignes
                foreach (var paire in _lignesParStation.Where(p => p.Value.Count > 1))
                {
                    string nomStation = paire.Key;
                    var lignes = paire.Value.ToList();

                    if (_stationsUniques.ContainsKey(nomStation))
                    {
                        /// Nous n'avons qu'un seul nœud par station maintenant.
                        /// On vérifie et met à jour les connexions aux voisins.
                        Noeud<T> station = _stationsUniques[nomStation];

                        foreach (var voisin in station.Voisins.ToList())
                        {
                            string nomVoisin = voisin.Key.Nom;

                            /// Une correspondance existe si aucune ligne n'est commune entre les stations
                            bool estCorrespondance = !_lignesParStation[nomStation]
                                .Intersect(_lignesParStation[nomVoisin])
                                .Any();

                            if (estCorrespondance)
                            {
                                /// Utiliser le temps de correspondance moyen par défaut
                                double tempsCorrespondance = 3.0;

                                /// Mettre à jour le temps de la connexion si nécessaire
                                if (voisin.Value != tempsCorrespondance)
                                {
                                    station.AjouterVoisin(voisin.Key, tempsCorrespondance);
                                    correspondancesCreees++;
                                }
                            }
                        }
                    }
                }

                Console.WriteLine($"Total des temps de correspondances mis à jour: {correspondancesCreees}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la mise à jour des correspondances: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        public void AjouterLien(Noeud<T> source, Noeud<T> destination, double poids)
        {
            /// Vérifier que les deux noeuds existent dans le graphe
            if (!Noeuds.Contains(source))
                Noeuds.Add(source);

            if (!Noeuds.Contains(destination))
                Noeuds.Add(destination);

            /// Ajouter le lien dans le sens indiqué
            source.AjouterVoisin(destination, poids);

            /// Pour un graphe non orienté, ajouter aussi le lien inverse
            if (!EstOriente)
            {
                destination.AjouterVoisin(source, poids);
            }
        }

        /// Recherche du plus court chemin avec l'algorithme de Dijkstra
        public List<Noeud<T>> Dijkstra(Noeud<T> source, Noeud<T> destination)
        {
            if (!Noeuds.Contains(source) || !Noeuds.Contains(destination))
                return new List<Noeud<T>>();

            /// Initialisation des distances et prédécesseurs
            Dictionary<Noeud<T>, double> distances = new Dictionary<Noeud<T>, double>();
            Dictionary<Noeud<T>, Noeud<T>> predecesseurs = new Dictionary<Noeud<T>, Noeud<T>>();
            List<Noeud<T>> nonVisites = new List<Noeud<T>>();

            /// Tous les noeuds commencent avec une distance infinie, sauf la source
            foreach (Noeud<T> noeud in Noeuds)
            {
                distances[noeud] = noeud.Equals(source) ? 0 : double.MaxValue;
                nonVisites.Add(noeud);
            }

            /// Parcours principal de l'algorithme
            while (nonVisites.Count > 0)
            {
                Noeud<T> noeudActuel = null;
                double minDistance = double.MaxValue;

                /// Sélection du noeud non visité avec la distance minimale
                foreach (Noeud<T> noeud in nonVisites)
                {
                    if (distances[noeud] < minDistance)
                    {
                        minDistance = distances[noeud];
                        noeudActuel = noeud;
                    }
                }

                /// Si aucun noeud n'est trouvé ou si la destination est atteinte, on arrête la recherche
                if (noeudActuel == null || noeudActuel.Equals(destination))
                    break;

                /// Marquer le noeud actuel comme visité
                nonVisites.Remove(noeudActuel);

                /// Mise à jour des distances pour les voisins du noeud actuel
                foreach (var paire in noeudActuel.Voisins)
                {
                    Noeud<T> voisin = paire.Key;
                    double poids = paire.Value;
                    double distance = distances[noeudActuel] + poids;

                    /// Si une distance plus courte est trouvée, mise à jour et enregistrement du prédécesseur
                    if (distance < distances[voisin])
                    {
                        distances[voisin] = distance;
                        predecesseurs[voisin] = noeudActuel;
                    }
                }
            }

            /// Reconstruction du chemin à partir des prédécesseurs
            List<Noeud<T>> chemin = new List<Noeud<T>>();
            Noeud<T> noeudCourant = destination;
            if (predecesseurs.ContainsKey(destination))
            {
                while (noeudCourant != null)
                {
                    chemin.Insert(0, noeudCourant);
                    if (noeudCourant.Equals(source))
                        break;
                    noeudCourant = predecesseurs.ContainsKey(noeudCourant) ? predecesseurs[noeudCourant] : null;
                }
            }

            return chemin;
        }

        /// Recherche du plus court chemin avec l'algorithme de Bellman-Ford
        public List<Noeud<T>> BellmanFord(Noeud<T> source, Noeud<T> destination)
        {
            if (!Noeuds.Contains(source) || !Noeuds.Contains(destination))
                return new List<Noeud<T>>();

            /// Initialisation des distances et des prédécesseurs
            Dictionary<Noeud<T>, double> distances = new Dictionary<Noeud<T>, double>();
            Dictionary<Noeud<T>, Noeud<T>> predecesseurs = new Dictionary<Noeud<T>, Noeud<T>>();

            /// Distance de départ zéro pour la source, infini pour le reste
            foreach (Noeud<T> noeud in Noeuds)
            {
                distances[noeud] = noeud.Equals(source) ? 0 : double.MaxValue;
            }

            /// Relaxation : itérer V-1 fois sur tous les arcs
            for (int i = 0; i < Noeuds.Count - 1; i++)
            {
                foreach (Noeud<T> noeud in Noeuds)
                {
                    foreach (var paire in noeud.Voisins)
                    {
                        Noeud<T> voisin = paire.Key;
                        double poids = paire.Value;

                        if (distances[noeud] != double.MaxValue && distances[noeud] + poids < distances[voisin])
                        {
                            /// Mise à jour de la distance et du prédécesseur
                            distances[voisin] = distances[noeud] + poids;
                            predecesseurs[voisin] = noeud;
                        }
                    }
                }
            }

            /// Vérification d'un cycle de poids négatif
            foreach (Noeud<T> noeud in Noeuds)
            {
                foreach (var paire in noeud.Voisins)
                {
                    Noeud<T> voisin = paire.Key;
                    double poids = paire.Value;

                    if (distances[noeud] != double.MaxValue && distances[noeud] + poids < distances[voisin])
                    {
                        Console.WriteLine("Attention: Le graphe contient un cycle de poids négatif.");
                        return new List<Noeud<T>>();
                    }
                }
            }

            /// Reconstruction du chemin à partir des prédécesseurs
            List<Noeud<T>> chemin = new List<Noeud<T>>();
            Noeud<T> noeudCourant = destination;
            if (predecesseurs.ContainsKey(destination))
            {
                while (noeudCourant != null)
                {
                    chemin.Insert(0, noeudCourant);
                    if (noeudCourant.Equals(source))
                        break;
                    noeudCourant = predecesseurs.ContainsKey(noeudCourant) ? predecesseurs[noeudCourant] : null;
                }
            }

            return chemin;
        }

        /// Recherche du plus court chemin avec l'algorithme de Floyd-Warshall
        public List<Noeud<T>> FloydWarshall(Noeud<T> source, Noeud<T> destination)
        {
            if (!Noeuds.Contains(source) || !Noeuds.Contains(destination))
                return new List<Noeud<T>>();

            int n = Noeuds.Count;
            double[,] dist = new double[n, n];
            int[,] next = new int[n, n];

            /// Initialisation de la matrice de distances et de la matrice "next"
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    dist[i, j] = double.MaxValue;
                    next[i, j] = -1;
                }
            }

            /// Initialisation des distances directes entre les noeuds et des successeurs immédiats
            for (int i = 0; i < n; i++)
            {
                dist[i, i] = 0;
                foreach (var paire in Noeuds[i].Voisins)
                {
                    int j = Noeuds.IndexOf(paire.Key);
                    if (j != -1)
                    {
                        dist[i, j] = paire.Value;
                        next[i, j] = j;
                    }
                }
            }

            /// Itération pour trouver les chemins les plus courts entre toutes les paires de noeuds
            for (int k = 0; k < n; k++)
            {
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (dist[i, k] != double.MaxValue && dist[k, j] != double.MaxValue)
                        {
                            if (dist[i, j] > dist[i, k] + dist[k, j])
                            {
                                /// Mise à jour de la distance et de la matrice "next" pour le chemin
                                dist[i, j] = dist[i, k] + dist[k, j];
                                next[i, j] = next[i, k];
                            }
                        }
                    }
                }
            }

            /// Reconstruction du chemin de la source à la destination
            List<Noeud<T>> chemin = new List<Noeud<T>>();
            int sourceIndex = Noeuds.IndexOf(source);
            int destIndex = Noeuds.IndexOf(destination);

            /// Si aucun chemin n'existe
            if (next[sourceIndex, destIndex] == -1)
                return chemin;

            chemin.Add(source);
            /// Suivre la chaîne de "next" jusqu'à la destination
            while (sourceIndex != destIndex)
            {
                sourceIndex = next[sourceIndex, destIndex];
                if (sourceIndex == -1)
                    return new List<Noeud<T>>();
                chemin.Add(Noeuds[sourceIndex]);
            }

            return chemin;
        }

        /// Comparer les résultats des 3 algorithmes
        public void ComparerAlgorithmes(Noeud<T> source, Noeud<T> destination)
        {
            Console.WriteLine($"Comparaison des algorithmes pour le trajet de {source.Nom} à {destination.Nom}");

            var startDijkstra = DateTime.Now;
            var cheminDijkstra = Dijkstra(source, destination);
            var tempsDijkstra = (DateTime.Now - startDijkstra).TotalMilliseconds;

            var startBellman = DateTime.Now;
            var cheminBellman = BellmanFord(source, destination);
            var tempsBellman = (DateTime.Now - startBellman).TotalMilliseconds;

            var startFloyd = DateTime.Now;
            var cheminFloyd = FloydWarshall(source, destination);
            var tempsFloyd = (DateTime.Now - startFloyd).TotalMilliseconds;

            Console.WriteLine("\nRésultats Dijkstra:");
            AfficherChemin(cheminDijkstra);
            Console.WriteLine($"Temps d'exécution: {tempsDijkstra} ms");

            Console.WriteLine("\nRésultats Bellman-Ford:");
            AfficherChemin(cheminBellman);
            Console.WriteLine($"Temps d'exécution: {tempsBellman} ms");

            Console.WriteLine("\nRésultats Floyd-Warshall:");
            AfficherChemin(cheminFloyd);
            Console.WriteLine($"Temps d'exécution: {tempsFloyd} ms");

            Console.WriteLine("\nComparaison des performances:");
            Console.WriteLine($"Dijkstra: {tempsDijkstra} ms - Complexité O(E + V log V)");
            Console.WriteLine($"Bellman-Ford: {tempsBellman} ms - Complexité O(V*E)");
            Console.WriteLine($"Floyd-Warshall: {tempsFloyd} ms - Complexité O(V^3)");

            Console.WriteLine("\nConculsion:");
            Console.WriteLine("- Dijkstra est généralement le plus rapide pour les graphes sans poids négatifs.");
            Console.WriteLine("- Bellman-Ford peut gérer les poids négatifs mais est plus lent que Dijkstra.");
            Console.WriteLine("- Floyd-Warshall calcule tous les chemins les plus courts entre toutes les paires de sommets, ce qui le rend utile pour des requêtes multiples mais plus lent pour une seule requête.");
        }

        private void AfficherChemin(List<Noeud<T>> chemin)
        {
            if (chemin.Count == 0)
            {
                Console.WriteLine("Aucun chemin trouvé.");
                return;
            }

            double tempsTotal = 0;
            Console.WriteLine($"Chemin trouvé ({chemin.Count} stations):");

            for (int i = 0; i < chemin.Count; i++)
            {
                /// Afficher la station avec toutes ses lignes associées
                string lignes = GetLignesForStation(chemin[i].Nom);
                Console.WriteLine($"{i + 1}. {chemin[i].Nom} (Lignes: {lignes})");

                if (i < chemin.Count - 1)
                {
                    double tempsTroncon = chemin[i].Voisins[chemin[i + 1]];
                    tempsTotal += tempsTroncon;

                    /// Vérification si le segment entre deux stations est une correspondance
                    bool estCorrespondance = false;
                    if (_lignesParStation.ContainsKey(chemin[i].Nom) && _lignesParStation.ContainsKey(chemin[i + 1].Nom))
                    {
                        estCorrespondance = !_lignesParStation[chemin[i].Nom]
                            .Intersect(_lignesParStation[chemin[i + 1].Nom])
                            .Any();
                    }

                    if (estCorrespondance)
                    {
                        Console.WriteLine($"   Correspondance: entre lignes ({tempsTroncon} min)");
                    }
                    else
                    {
                        Console.WriteLine($"   → ({tempsTroncon} min)");
                    }
                }
            }

            Console.WriteLine($"Temps total estimé: {tempsTotal} minutes");
        }
    }
}
