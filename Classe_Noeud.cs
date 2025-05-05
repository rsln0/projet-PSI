using System;
using System.Collections.Generic;

namespace LivInParisApp
{
    public class Noeud<T>
    {
        public T Data { get; set; }
        public string Nom { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string LigneMetro { get; set; }

        // Liste des voisins avec poids (temps de trajet)
        public Dictionary<Noeud<T>, double> Voisins { get; set; }

        public Noeud(T data, string nom, double latitude, double longitude, string ligneMetro)
        {
            Data = data;
            Nom = nom;
            Latitude = latitude;
            Longitude = longitude;
            LigneMetro = ligneMetro;
            Voisins = new Dictionary<Noeud<T>, double>();
        }

        public void AjouterVoisin(Noeud<T> voisin, double poids)
        {
            if (!Voisins.ContainsKey(voisin))
            {
                Voisins.Add(voisin, poids);
            }
            else
            {
                // Si le voisin existe déjà, mettre à jour le poids si nécessaire
                if (Voisins[voisin] > poids)
                {
                    Voisins[voisin] = poids;
                }
            }
        }

        public override string ToString()
        {
            return $"{Nom} (Ligne {LigneMetro})";
        }

        // Surcharge d'égalité pour comparer des noeuds
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            Noeud<T> other = (Noeud<T>)obj;
            return Nom.Equals(other.Nom) && LigneMetro == other.LigneMetro;
        }

        public override int GetHashCode()
        {
            return Nom.GetHashCode() ^ LigneMetro.GetHashCode();
        }
    }
}
