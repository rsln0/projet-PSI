using System;
using System.Collections.Generic;
using System.Linq;

namespace LivInParisApp
{
    public class Client
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string Adresse { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string MotDePasse { get; set; }
        public bool EstEntreprise { get; set; }
        public string NomEntreprise { get; set; }
        public string NomReferent { get; set; }
        public List<Commande> Commandes { get; set; }
        public double MontantAchats { get; set; }

        public Client()
        {
            Commandes = new List<Commande>();
            MontantAchats = 0;
        }

        public Client(int id, string nom, string prenom, string adresse, string telephone, string email, string motDePasse)
        {
            Id = id;
            Nom = nom;
            Prenom = prenom;
            Adresse = adresse;
            Telephone = telephone;
            Email = email;
            MotDePasse = motDePasse;
            EstEntreprise = false;
            Commandes = new List<Commande>();
            MontantAchats = 0;
        }

        public Client(int id, string nomEntreprise, string nomReferent, string adresse, string telephone, string email, string motDePasse, bool estEntreprise)
        {
            Id = id;
            NomEntreprise = nomEntreprise;
            NomReferent = nomReferent;
            Adresse = adresse;
            Telephone = telephone;
            Email = email;
            MotDePasse = motDePasse;
            EstEntreprise = estEntreprise;
            Commandes = new List<Commande>();
            MontantAchats = 0;
        }

        public void AjouterCommande(Commande commande)
        {
            Commandes.Add(commande);
            MontantAchats += commande.CalculerPrixTotal();
        }

        public override string ToString()
        {
            if (EstEntreprise)
            {
                return $"Entreprise: {NomEntreprise}, Référent: {NomReferent}, Adresse: {Adresse}, Achats: {MontantAchats:C}";
            }
            else
            {
                return $"{Prenom} {Nom}, Adresse: {Adresse}, Achats: {MontantAchats:C}";
            }
        }
    }

    public class Cuisinier
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string Adresse { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string MotDePasse { get; set; }
        public List<Plat> PlatsOfferts { get; set; }
        public List<Commande> CommandesLivrees { get; set; }

        public Cuisinier()
        {
            PlatsOfferts = new List<Plat>();
            CommandesLivrees = new List<Commande>();
        }

        public Cuisinier(int id, string nom, string prenom, string adresse, string telephone, string email, string motDePasse)
        {
            Id = id;
            Nom = nom;
            Prenom = prenom;
            Adresse = adresse;
            Telephone = telephone;
            Email = email;
            MotDePasse = motDePasse;
            PlatsOfferts = new List<Plat>();
            CommandesLivrees = new List<Commande>();
        }

        public void AjouterPlat(Plat plat)
        {
            PlatsOfferts.Add(plat);
        }

        public void AjouterCommande(Commande commande)
        {
            CommandesLivrees.Add(commande);
        }

        public Plat GetPlatDuJour()
        {
            if (PlatsOfferts.Count > 0)
            {
                // Simple logique pour choisir le plat du jour (dernier plat ajouté)
                return PlatsOfferts.LastOrDefault();
            }
            return null;
        }

        public List<Client> GetClientsServis()
        {
            HashSet<Client> clients = new HashSet<Client>();
            foreach (var commande in CommandesLivrees)
            {
                clients.Add(commande.Client);
            }
            return clients.ToList();
        }

        public List<Client> GetClientsServis(DateTime debut, DateTime fin)
        {
            HashSet<Client> clients = new HashSet<Client>();
            foreach (var commande in CommandesLivrees)
            {
                if (commande.DateCommande >= debut && commande.DateCommande <= fin)
                {
                    clients.Add(commande.Client);
                }
            }
            return clients.ToList();
        }

        public Dictionary<Plat, int> GetPlatParFrequence()
        {
            Dictionary<Plat, int> frequences = new Dictionary<Plat, int>();

            foreach (var commande in CommandesLivrees)
            {
                foreach (var ligne in commande.LignesCommande)
                {
                    if (frequences.ContainsKey(ligne.Plat))
                    {
                        frequences[ligne.Plat]++;
                    }
                    else
                    {
                        frequences[ligne.Plat] = 1;
                    }
                }
            }

            return frequences.OrderByDescending(x => x.Value)
                             .ToDictionary(x => x.Key, x => x.Value);
        }

        public override string ToString()
        {
            return $"{Prenom} {Nom}, Adresse: {Adresse}, Nb plats: {PlatsOfferts.Count}, Nb commandes: {CommandesLivrees.Count}";
        }
    }

    public enum TypePlat
    {
        Entree,
        PlatPrincipal,
        Dessert
    }

    public class Plat
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public TypePlat Type { get; set; }
        public int NbPersonnes { get; set; }
        public DateTime DateFabrication { get; set; }
        public DateTime DatePeremption { get; set; }
        public double PrixParPersonne { get; set; }
        public string Nationalite { get; set; }
        public string RegimeAlimentaire { get; set; }
        public List<string> Ingredients { get; set; }
        public string PhotoUrl { get; set; }
        public Cuisinier Cuisinier { get; set; }
        public Plat RecetteOrigine { get; set; }

        public Plat()
        {
            Ingredients = new List<string>();
        }

        public Plat(int id, string nom, TypePlat type, int nbPersonnes, DateTime dateFabrication,
                    DateTime datePeremption, double prixParPersonne, string nationalite,
                    string regimeAlimentaire, Cuisinier cuisinier)
        {
            Id = id;
            Nom = nom;
            Type = type;
            NbPersonnes = nbPersonnes;
            DateFabrication = dateFabrication;
            DatePeremption = datePeremption;
            PrixParPersonne = prixParPersonne;
            Nationalite = nationalite;
            RegimeAlimentaire = regimeAlimentaire;
            Ingredients = new List<string>();
            Cuisinier = cuisinier;
        }

        public void AjouterIngredient(string ingredient)
        {
            Ingredients.Add(ingredient);
        }

        public bool EstPerime()
        {
            return DateTime.Now > DatePeremption;
        }

        public double CalculerPrixTotal()
        {
            return PrixParPersonne * NbPersonnes;
        }

        public override string ToString()
        {
            return $"{Nom} ({Type}), {NbPersonnes} pers., {PrixParPersonne:C}/pers., {Nationalite}, {RegimeAlimentaire}";
        }
    }

    public class LigneCommande
    {
        public int Id { get; set; }
        public Plat Plat { get; set; }
        public int Quantite { get; set; }
        public DateTime DateLivraison { get; set; }
        public string AdresseLivraison { get; set; }
        public bool EstLivree { get; set; }

        public LigneCommande()
        {
        }

        public LigneCommande(int id, Plat plat, int quantite, DateTime dateLivraison, string adresseLivraison)
        {
            Id = id;
            Plat = plat;
            Quantite = quantite;
            DateLivraison = dateLivraison;
            AdresseLivraison = adresseLivraison;
            EstLivree = false;
        }

        public double CalculerPrix()
        {
            return Plat.PrixParPersonne * Quantite;
        }

        public override string ToString()
        {
            return $"{Plat.Nom}, Qté: {Quantite}, Prix: {CalculerPrix():C}, Livraison: {DateLivraison.ToShortDateString()}, {(EstLivree ? "Livrée" : "En attente")}";
        }
    }

    public class Commande
    {
        public int Id { get; set; }
        public Client Client { get; set; }
        public Cuisinier Cuisinier { get; set; }
        public DateTime DateCommande { get; set; }
        public List<LigneCommande> LignesCommande { get; set; }
        public bool EstPayee { get; set; }
        public string CheminLivraison { get; set; }

        public Commande()
        {
            LignesCommande = new List<LigneCommande>();
            DateCommande = DateTime.Now;
        }

        public Commande(int id, Client client, Cuisinier cuisinier)
        {
            Id = id;
            Client = client;
            Cuisinier = cuisinier;
            DateCommande = DateTime.Now;
            LignesCommande = new List<LigneCommande>();
            EstPayee = false;
        }

        public void AjouterLigneCommande(LigneCommande ligne)
        {
            LignesCommande.Add(ligne);
        }

        public double CalculerPrixTotal()
        {
            return LignesCommande.Sum(l => l.CalculerPrix());
        }

        public bool EstTotalementLivree()
        {
            return LignesCommande.All(l => l.EstLivree);
        }

        public override string ToString()
        {
            return $"Commande #{Id}, Client: {Client.Nom}, Cuisinier: {Cuisinier.Nom}, Date: {DateCommande.ToShortDateString()}, Prix: {CalculerPrixTotal():C}, Statut: {(EstPayee ? "Payée" : "Non payée")}";
        }
    }
}
