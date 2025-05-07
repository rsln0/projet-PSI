using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using MySql.Data.MySqlClient;
using System.Text.Json;
using System.Xml.Serialization;
using System.Windows.Forms; // N'oubliez pas cette référence !
using PSI; // Pour accéder à Metro_GUI

namespace LivInParisApp
{
    class Program
    {
        private static string connectionString = "Server=localhost;Database=LivInParis;Uid=root;Pwd=zigwy2306;";
        private static MySqlConnection connection;

        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                connection = new MySqlConnection(connectionString);
                connection.Open();
                Console.WriteLine("Connexion réussie à la base de données LivInParis");

                // Menu principal
                bool exit = false;
                while (!exit)
                {
                    Console.Clear();
                    Console.WriteLine("===== Liv'In Paris - Système de partage de repas =====");
                    Console.WriteLine("1. Module Client");
                    Console.WriteLine("2. Module Cuisinier");
                    Console.WriteLine("3. Module Commande");
                    Console.WriteLine("4. Module Trajet (Interface Graphique)");
                    Console.WriteLine("5. Quitter");
                    Console.Write("Votre choix : ");

                    string choice = Console.ReadLine();
                    Console.WriteLine();

                    switch (choice)
                    {
                        case "1":
                            ModuleClient();
                            break;
                        case "2":
                            ModuleCuisinier();
                            break;
                        case "3":
                            ModuleCommande();
                            break;
                        case "4":
                            ModuleTrajet();
                            break;
                        case "5":
                            exit = true;
                            break;
                        default:
                            Console.WriteLine("Option invalide. Veuillez réessayer.");
                            break;
                    }
                    if (!exit)
                    {
                        Console.WriteLine("\nAppuyez sur une touche pour continuer...");
                        Console.ReadKey();
                    }
                }

                connection.Close();
                Console.WriteLine("Au revoir !");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur : {ex.Message}");
            }
        }

        #region Module Client
        private static void ModuleClient()
        {
            bool retour = false;
            while (!retour)
            {
                Console.Clear();
                Console.WriteLine("===== MODULE CLIENT =====");
                Console.WriteLine("1. Ajouter un client");
                Console.WriteLine("2. Modifier un client");
                Console.WriteLine("3. Supprimer un client");
                Console.WriteLine("4. Afficher tous les clients");
                Console.WriteLine("5. Afficher les clients par rue");
                Console.WriteLine("6. Retour au menu principal");
                Console.Write("Votre choix : ");

                string choice = Console.ReadLine();
                Console.WriteLine();

                switch (choice)
                {
                    case "1":
                        AjouterClient();
                        break;
                    case "2":
                        ModifierClient();
                        break;
                    case "3":
                        SupprimerClient();
                        break;
                    case "4":
                        AfficherClientsParOrdreAlphabetique();
                        break;
                    case "5":
                        AfficherClientsParRue();
                        break;
                    case "6":
                        retour = true;
                        break;
                    default:
                        Console.WriteLine("Option invalide. Veuillez réessayer.");
                        break;
                }

                if (!retour)
                {
                    Console.WriteLine("\nAppuyez sur une touche pour continuer...");
                    Console.ReadKey();
                }
            }
        }

        private static void AjouterClient()
        {
            Console.WriteLine("=== Ajout d'un nouveau client ===");

            Console.Write("Nom : ");
            string nom = Console.ReadLine();

            Console.Write("Prénom : ");
            string prenom = Console.ReadLine();

            Console.Write("Adresse : ");
            string adresse = Console.ReadLine();

            Console.Write("Téléphone : ");
            string telephone = Console.ReadLine();

            Console.Write("Email : ");
            string email = Console.ReadLine();

            try
            {
                string query = "INSERT INTO Client (nom, prenom, adresse, telephone, email) VALUES (@nom, @prenom, @adresse, @telephone, @email)";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@nom", nom);
                cmd.Parameters.AddWithValue("@prenom", prenom);
                cmd.Parameters.AddWithValue("@adresse", adresse);
                cmd.Parameters.AddWithValue("@telephone", telephone);
                cmd.Parameters.AddWithValue("@email", email);

                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    Console.WriteLine("Client ajouté avec succès !");
                }
                else
                {
                    Console.WriteLine("Erreur lors de l'ajout du client.");
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    Console.WriteLine("Erreur: Cette adresse email est déjà utilisée.");
                }
                else
                {
                    Console.WriteLine($"Erreur MySQL: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
            }
        }

        private static void ModifierClient()
        {
            Console.WriteLine("=== Modification d'un client ===");

            Console.Write("Entrez l'ID du client à modifier : ");
            if (!int.TryParse(Console.ReadLine(), out int idClient))
            {
                Console.WriteLine("ID invalide.");
                return;
            }

            try
            {
                string checkQuery = "SELECT * FROM Client WHERE id_client = @id";
                MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
                checkCmd.Parameters.AddWithValue("@id", idClient);

                using (MySqlDataReader reader = checkCmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        Console.WriteLine("Client non trouvé.");
                        return;
                    }
                    Console.WriteLine($"Modification du client: {reader["prenom"]} {reader["nom"]}");
                }

                Console.WriteLine("Entrez les nouvelles informations (laissez vide pour conserver la valeur actuelle):");

                string currentQuery = "SELECT * FROM Client WHERE id_client = @id";
                MySqlCommand currentCmd = new MySqlCommand(currentQuery, connection);
                currentCmd.Parameters.AddWithValue("@id", idClient);

                string currentNom = "", currentPrenom = "", currentAdresse = "", currentTelephone = "", currentEmail = "";
                using (MySqlDataReader reader = currentCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        currentNom = reader["nom"].ToString();
                        currentPrenom = reader["prenom"].ToString();
                        currentAdresse = reader["adresse"].ToString();
                        currentTelephone = reader["telephone"].ToString();
                        currentEmail = reader["email"].ToString();
                    }
                }

                Console.Write($"Nom [{currentNom}] : ");
                string nom = Console.ReadLine();
                nom = string.IsNullOrWhiteSpace(nom) ? currentNom : nom;

                Console.Write($"Prénom [{currentPrenom}] : ");
                string prenom = Console.ReadLine();
                prenom = string.IsNullOrWhiteSpace(prenom) ? currentPrenom : prenom;

                Console.Write($"Adresse [{currentAdresse}] : ");
                string adresse = Console.ReadLine();
                adresse = string.IsNullOrWhiteSpace(adresse) ? currentAdresse : adresse;

                Console.Write($"Téléphone [{currentTelephone}] : ");
                string telephone = Console.ReadLine();
                telephone = string.IsNullOrWhiteSpace(telephone) ? currentTelephone : telephone;

                Console.Write($"Email [{currentEmail}] : ");
                string email = Console.ReadLine();
                email = string.IsNullOrWhiteSpace(email) ? currentEmail : email;

                string updateQuery = "UPDATE Client SET nom = @nom, prenom = @prenom, adresse = @adresse, telephone = @telephone, email = @email WHERE id_client = @id";
                MySqlCommand updateCmd = new MySqlCommand(updateQuery, connection);
                updateCmd.Parameters.AddWithValue("@nom", nom);
                updateCmd.Parameters.AddWithValue("@prenom", prenom);
                updateCmd.Parameters.AddWithValue("@adresse", adresse);
                updateCmd.Parameters.AddWithValue("@telephone", telephone);
                updateCmd.Parameters.AddWithValue("@email", email);
                updateCmd.Parameters.AddWithValue("@id", idClient);

                int rowsAffected = updateCmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    Console.WriteLine("Client modifié avec succès !");
                }
                else
                {
                    Console.WriteLine("Aucune modification n'a été effectuée.");
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    Console.WriteLine("Erreur: Cette adresse email est déjà utilisée.");
                }
                else
                {
                    Console.WriteLine($"Erreur MySQL: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
            }
        }

        private static void SupprimerClient()
        {
            Console.WriteLine("=== Suppression d'un client ===");

            Console.Write("Entrez l'ID du client à supprimer : ");
            if (!int.TryParse(Console.ReadLine(), out int idClient))
            {
                Console.WriteLine("ID invalide.");
                return;
            }

            try
            {
                string checkQuery = "SELECT * FROM Client WHERE id_client = @id";
                MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
                checkCmd.Parameters.AddWithValue("@id", idClient);

                using (MySqlDataReader reader = checkCmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        Console.WriteLine("Client non trouvé.");
                        return;
                    }
                    Console.WriteLine($"Vous êtes sur le point de supprimer le client: {reader["prenom"]} {reader["nom"]}");
                }

                Console.Write("Êtes-vous sûr ? (O/N) : ");
                string confirmation = Console.ReadLine().ToUpper();
                if (confirmation != "O" && confirmation != "OUI")
                {
                    Console.WriteLine("Suppression annulée.");
                    return;
                }

                string deleteQuery = "DELETE FROM Client WHERE id_client = @id";
                MySqlCommand deleteCmd = new MySqlCommand(deleteQuery, connection);
                deleteCmd.Parameters.AddWithValue("@id", idClient);

                int rowsAffected = deleteCmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    Console.WriteLine("Client supprimé avec succès !");
                }
                else
                {
                    Console.WriteLine("Aucun client n'a été supprimé.");
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1451)
                {
                    Console.WriteLine("Erreur: Ce client ne peut pas être supprimé car il est lié à des commandes.");
                }
                else
                {
                    Console.WriteLine($"Erreur MySQL: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
            }
        }

        private static void AfficherClientsParOrdreAlphabetique()
        {
            Console.WriteLine("=== Liste des clients par ordre alphabétique ===");

            try
            {
                string query = "SELECT id_client, nom, prenom, adresse, telephone, email FROM Client ORDER BY nom, prenom";
                MySqlCommand cmd = new MySqlCommand(query, connection);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        Console.WriteLine("Aucun client trouvé.");
                        return;
                    }

                    const int idWidth = 5;
                    const int nomWidth = 15;
                    const int prenomWidth = 15;
                    const int adresseWidth = 30;
                    const int telWidth = 15;
                    const int emailWidth = 30;

                    Console.WriteLine($"{"ID".PadRight(idWidth)}{"Nom".PadRight(nomWidth)}{"Prénom".PadRight(prenomWidth)}{"Adresse".PadRight(adresseWidth)}{"Téléphone".PadRight(telWidth)}{"Email".PadRight(emailWidth)}");
                    Console.WriteLine(new string('-', idWidth + nomWidth + prenomWidth + adresseWidth + telWidth + emailWidth));

                    while (reader.Read())
                    {
                        string adresse = reader["adresse"].ToString();
                        if (adresse.Length > adresseWidth - 3)
                        {
                            adresse = adresse.Substring(0, adresseWidth - 3) + "...";
                        }

                        string email = reader["email"].ToString();
                        if (email.Length > emailWidth - 3)
                        {
                            email = email.Substring(0, emailWidth - 3) + "...";
                        }

                        Console.WriteLine(
                            $"{reader["id_client"].ToString().PadRight(idWidth)}" +
                            $"{reader["nom"].ToString().PadRight(nomWidth)}" +
                            $"{reader["prenom"].ToString().PadRight(prenomWidth)}" +
                            $"{adresse.PadRight(adresseWidth)}" +
                            $"{reader["telephone"].ToString().PadRight(telWidth)}" +
                            $"{email}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
            }
        }

        private static void AfficherClientsParRue()
        {
            Console.WriteLine("=== Liste des clients par rue ===");

            try
            {
                string query = "SELECT id_client, nom, prenom, adresse, telephone, email FROM Client ORDER BY adresse";
                MySqlCommand cmd = new MySqlCommand(query, connection);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        Console.WriteLine("Aucun client trouvé.");
                        return;
                    }

                    const int idWidth = 5;
                    const int nomWidth = 15;
                    const int prenomWidth = 15;
                    const int adresseWidth = 30;
                    const int telWidth = 15;
                    const int emailWidth = 30;

                    Console.WriteLine($"{"ID".PadRight(idWidth)}{"Nom".PadRight(nomWidth)}{"Prénom".PadRight(prenomWidth)}{"Adresse".PadRight(adresseWidth)}{"Téléphone".PadRight(telWidth)}{"Email".PadRight(emailWidth)}");
                    Console.WriteLine(new string('-', idWidth + nomWidth + prenomWidth + adresseWidth + telWidth + emailWidth));

                    while (reader.Read())
                    {
                        string adresse = reader["adresse"].ToString();
                        if (adresse.Length > adresseWidth - 3)
                        {
                            adresse = adresse.Substring(0, adresseWidth - 3) + "...";
                        }

                        string email = reader["email"].ToString();
                        if (email.Length > emailWidth - 3)
                        {
                            email = email.Substring(0, emailWidth - 3) + "...";
                        }

                        Console.WriteLine(
                            $"{reader["id_client"].ToString().PadRight(idWidth)}" +
                            $"{reader["nom"].ToString().PadRight(nomWidth)}" +
                            $"{reader["prenom"].ToString().PadRight(prenomWidth)}" +
                            $"{adresse.PadRight(adresseWidth)}" +
                            $"{reader["telephone"].ToString().PadRight(telWidth)}" +
                            $"{email}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
            }
        }
        #endregion

        #region Module Cuisinier
        private static void ModuleCuisinier()
        {
            bool retour = false;
            while (!retour)
            {
                Console.Clear();
                Console.WriteLine("===== MODULE CUISINIER =====");
                Console.WriteLine("1. Ajouter un cuisinier");
                Console.WriteLine("2. Modifier un cuisinier");
                Console.WriteLine("3. Supprimer un cuisinier");
                Console.WriteLine("4. Retour au menu principal");
                Console.Write("Votre choix : ");

                string choice = Console.ReadLine();
                Console.WriteLine();

                switch (choice)
                {
                    case "1":
                        AjouterCuisinier();
                        break;
                    case "2":
                        ModifierCuisinier();
                        break;
                    case "3":
                        SupprimerCuisinier();
                        break;
                    case "4":
                        retour = true;
                        break;
                    default:
                        Console.WriteLine("Option invalide. Veuillez réessayer.");
                        break;
                }

                if (!retour)
                {
                    Console.WriteLine("\nAppuyez sur une touche pour continuer...");
                    Console.ReadKey();
                }
            }
        }

        private static void AjouterCuisinier()
        {
            Console.WriteLine("=== Ajout d'un nouveau cuisinier ===");

            Console.Write("Nom : ");
            string nom = Console.ReadLine();

            Console.Write("Prénom : ");
            string prenom = Console.ReadLine();

            Console.Write("Adresse : ");
            string adresse = Console.ReadLine();

            Console.Write("Téléphone : ");
            string telephone = Console.ReadLine();

            Console.Write("Email : ");
            string email = Console.ReadLine();

            try
            {
                string query = "INSERT INTO Cuisinier (nom, prenom, adresse, telephone, email) VALUES (@nom, @prenom, @adresse, @telephone, @email)";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@nom", nom);
                cmd.Parameters.AddWithValue("@prenom", prenom);
                cmd.Parameters.AddWithValue("@adresse", adresse);
                cmd.Parameters.AddWithValue("@telephone", telephone);
                cmd.Parameters.AddWithValue("@email", email);

                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    Console.WriteLine("Cuisinier ajouté avec succès !");
                }
                else
                {
                    Console.WriteLine("Erreur lors de l'ajout du cuisinier.");
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    Console.WriteLine("Erreur: Cette adresse email est déjà utilisée.");
                }
                else
                {
                    Console.WriteLine($"Erreur MySQL: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
            }
        }

        private static void ModifierCuisinier()
        {
            Console.WriteLine("=== Modification d'un cuisinier ===");

            Console.Write("Entrez l'ID du cuisinier à modifier : ");
            if (!int.TryParse(Console.ReadLine(), out int idCuisinier))
            {
                Console.WriteLine("ID invalide.");
                return;
            }

            try
            {
                string checkQuery = "SELECT * FROM Cuisinier WHERE id_cuisinier = @id";
                MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
                checkCmd.Parameters.AddWithValue("@id", idCuisinier);

                using (MySqlDataReader reader = checkCmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        Console.WriteLine("Cuisinier non trouvé.");
                        return;
                    }
                    Console.WriteLine($"Modification du cuisinier: {reader["prenom"]} {reader["nom"]}");
                }

                Console.WriteLine("Entrez les nouvelles informations (laissez vide pour conserver la valeur actuelle):");

                string currentQuery = "SELECT * FROM Cuisinier WHERE id_cuisinier = @id";
                MySqlCommand currentCmd = new MySqlCommand(currentQuery, connection);
                currentCmd.Parameters.AddWithValue("@id", idCuisinier);

                string currentNom = "", currentPrenom = "", currentAdresse = "", currentTelephone = "", currentEmail = "";
                using (MySqlDataReader reader = currentCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        currentNom = reader["nom"].ToString();
                        currentPrenom = reader["prenom"].ToString();
                        currentAdresse = reader["adresse"].ToString();
                        currentTelephone = reader["telephone"].ToString();
                        currentEmail = reader["email"].ToString();
                    }
                }

                Console.Write($"Nom [{currentNom}] : ");
                string nom = Console.ReadLine();
                nom = string.IsNullOrWhiteSpace(nom) ? currentNom : nom;

                Console.Write($"Prénom [{currentPrenom}] : ");
                string prenom = Console.ReadLine();
                prenom = string.IsNullOrWhiteSpace(prenom) ? currentPrenom : prenom;

                Console.Write($"Adresse [{currentAdresse}] : ");
                string adresse = Console.ReadLine();
                adresse = string.IsNullOrWhiteSpace(adresse) ? currentAdresse : adresse;

                Console.Write($"Téléphone [{currentTelephone}] : ");
                string telephone = Console.ReadLine();
                telephone = string.IsNullOrWhiteSpace(telephone) ? currentTelephone : telephone;

                Console.Write($"Email [{currentEmail}] : ");
                string email = Console.ReadLine();
                email = string.IsNullOrWhiteSpace(email) ? currentEmail : email;

                string updateQuery = "UPDATE Cuisinier SET nom = @nom, prenom = @prenom, adresse = @adresse, telephone = @telephone, email = @email WHERE id_cuisinier = @id";
                MySqlCommand updateCmd = new MySqlCommand(updateQuery, connection);
                updateCmd.Parameters.AddWithValue("@nom", nom);
                updateCmd.Parameters.AddWithValue("@prenom", prenom);
                updateCmd.Parameters.AddWithValue("@adresse", adresse);
                updateCmd.Parameters.AddWithValue("@telephone", telephone);
                updateCmd.Parameters.AddWithValue("@email", email);
                updateCmd.Parameters.AddWithValue("@id", idCuisinier);

                int rowsAffected = updateCmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    Console.WriteLine("Cuisinier modifié avec succès !");
                }
                else
                {
                    Console.WriteLine("Aucune modification n'a été effectuée.");
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    Console.WriteLine("Erreur: Cette adresse email est déjà utilisée.");
                }
                else
                {
                    Console.WriteLine($"Erreur MySQL: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
            }
        }

        private static void SupprimerCuisinier()
        {
            Console.WriteLine("=== Suppression d'un cuisinier ===");

            Console.Write("Entrez l'ID du cuisinier à supprimer : ");
            if (!int.TryParse(Console.ReadLine(), out int idCuisinier))
            {
                Console.WriteLine("ID invalide.");
                return;
            }

            try
            {
                string checkQuery = "SELECT * FROM Cuisinier WHERE id_cuisinier = @id";
                MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
                checkCmd.Parameters.AddWithValue("@id", idCuisinier);

                using (MySqlDataReader reader = checkCmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        Console.WriteLine("Cuisinier non trouvé.");
                        return;
                    }
                    Console.WriteLine($"Vous êtes sur le point de supprimer le cuisinier: {reader["prenom"]} {reader["nom"]}");
                }

                Console.Write("Êtes-vous sûr ? (O/N) : ");
                string confirmation = Console.ReadLine().ToUpper();
                if (confirmation != "O" && confirmation != "OUI")
                {
                    Console.WriteLine("Suppression annulée.");
                    return;
                }

                string deleteQuery = "DELETE FROM Cuisinier WHERE id_cuisinier = @id";
                MySqlCommand deleteCmd = new MySqlCommand(deleteQuery, connection);
                deleteCmd.Parameters.AddWithValue("@id", idCuisinier);

                int rowsAffected = deleteCmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    Console.WriteLine("Cuisinier supprimé avec succès !");
                }
                else
                {
                    Console.WriteLine("Aucun cuisinier n'a été supprimé.");
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1451)
                {
                    Console.WriteLine("Erreur: Ce cuisinier ne peut pas être supprimé car il est lié à des commandes ou des plats.");
                }
                else
                {
                    Console.WriteLine($"Erreur MySQL: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
            }
        }
        #endregion

        #region Module Commande
        private static void ModuleCommande()
        {
            bool retour = false;
            while (!retour)
            {
                Console.Clear();
                Console.WriteLine("===== MODULE COMMANDE =====");
                Console.WriteLine("1. Afficher toutes les commandes");
                Console.WriteLine("2. Afficher les commandes d'un client");
                Console.WriteLine("3. Afficher les commandes d'un cuisinier");
                Console.WriteLine("4. Afficher le détail d'une commande");
                Console.WriteLine("5. Retour au menu principal");
                Console.Write("Votre choix : ");
                string choice = Console.ReadLine();
                Console.WriteLine();

                switch (choice)
                {
                    case "1":
                        AfficherToutesCommandes();
                        break;
                    case "2":
                        AfficherCommandesClient();
                        break;
                    case "3":
                        AfficherCommandesCuisinier();
                        break;
                    case "4":
                        AfficherDetailCommande();
                        break;
                    case "5":
                        retour = true;
                        break;
                    default:
                        Console.WriteLine("Option invalide. Veuillez réessayer.");
                        break;
                }
                if (!retour)
                {
                    Console.WriteLine("\nAppuyez sur une touche pour continuer...");
                    Console.ReadKey();
                }
            }
        }

        private static void AfficherToutesCommandes()
        {
            Console.WriteLine("=== Liste de toutes les commandes ===");

            try
            {
                string query = @"
    SELECT c.id_commande, DATE_FORMAT(c.date_commande, '%d/%m/%Y') AS date_formattee, 
           cl.id_client, CONCAT(cl.prenom, ' ', cl.nom) AS nom_client,
           cu.id_cuisinier, CONCAT(cu.prenom, ' ', cu.nom) AS nom_cuisinier,
           (SELECT SUM(p.prix * dc.quantite) 
            FROM Details_Commande dc 
            JOIN Plat p ON dc.id_plat = p.id_plat 
            WHERE dc.id_commande = c.id_commande) AS prix_total_calcule
    FROM Commande c
    JOIN Client cl ON c.id_client = cl.id_client
    JOIN Cuisinier cu ON c.id_cuisinier = cu.id_cuisinier
    ORDER BY c.date_commande DESC";

                MySqlCommand cmd = new MySqlCommand(query, connection);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        Console.WriteLine("Aucune commande trouvée.");
                        return;
                    }

                    const int idWidth = 5;
                    const int dateWidth = 12;
                    const int idClientWidth = 10;
                    const int clientWidth = 25;
                    const int cuisinierWidth = 25;
                    const int prixWidth = 15;

                    Console.WriteLine($"{"ID".PadRight(idWidth)} | {"Date".PadRight(dateWidth)} | {"ID Client".PadRight(idClientWidth)} | {"Client".PadRight(clientWidth)} | {"Cuisinier".PadRight(cuisinierWidth)} | {"Prix total".PadRight(prixWidth)}");
                    Console.WriteLine(new string('-', idWidth + dateWidth + idClientWidth + clientWidth + cuisinierWidth + prixWidth + 10));

                    while (reader.Read())
                    {
                        string id = reader["id_commande"].ToString();
                        string date = reader["date_formattee"].ToString();
                        string idClient = reader["id_client"].ToString();
                        string client = reader["nom_client"].ToString();
                        string cuisinier = reader["nom_cuisinier"].ToString();
                        string prix = ((decimal)reader["prix_total_calcule"]).ToString("C");

                        if (client.Length > clientWidth - 3)
                            client = client.Substring(0, clientWidth - 3) + "...";

                        if (cuisinier.Length > cuisinierWidth - 3)
                            cuisinier = cuisinier.Substring(0, cuisinierWidth - 3) + "...";

                        Console.WriteLine(
                            $"{id.PadRight(idWidth)} | " +
                            $"{date.PadRight(dateWidth)} | " +
                            $"{idClient.PadRight(idClientWidth)} | " +
                            $"{client.PadRight(clientWidth)} | " +
                            $"{cuisinier.PadRight(cuisinierWidth)} | " +
                            $"{prix}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
            }
        }

        private static void AfficherCommandesClient()
        {
            Console.WriteLine("=== Affichage des commandes d'un client ===");

            int idClient = SelectionnerClient();
            if (idClient == -1) return;

            try
            {
                string queryNomClient = "SELECT CONCAT(prenom, ' ', nom) AS nom_complet FROM Client WHERE id_client = @idClient";
                MySqlCommand cmdNomClient = new MySqlCommand(queryNomClient, connection);
                cmdNomClient.Parameters.AddWithValue("@idClient", idClient);
                string nomClient = (string)cmdNomClient.ExecuteScalar();

                Console.WriteLine($"\n=== Commandes de {nomClient} (ID: {idClient}) ===");

                string queryCommandes = @"
    SELECT c.id_commande, DATE_FORMAT(c.date_commande, '%d/%m/%Y') AS date_formattee, 
           CONCAT(cu.prenom, ' ', cu.nom) AS nom_cuisinier,
           (SELECT SUM(p.prix * dc.quantite) 
            FROM Details_Commande dc 
            JOIN Plat p ON dc.id_plat = p.id_plat 
            WHERE dc.id_commande = c.id_commande) AS prix_total_calcule
    FROM Commande c
    JOIN Cuisinier cu ON c.id_cuisinier = cu.id_cuisinier
    WHERE c.id_client = @idClient
    ORDER BY c.date_commande DESC";

                MySqlCommand cmdCommandes = new MySqlCommand(queryCommandes, connection);
                cmdCommandes.Parameters.AddWithValue("@idClient", idClient);

                List<int> commandeIds = new List<int>();

                using (MySqlDataReader reader = cmdCommandes.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        Console.WriteLine("Aucune commande trouvée pour ce client.");
                        return;
                    }

                    const int idWidth = 5;
                    const int dateWidth = 12;
                    const int cuisinierWidth = 30;
                    const int prixWidth = 15;

                    Console.WriteLine($"{"ID".PadRight(idWidth)} | {"Date".PadRight(dateWidth)} | {"Cuisinier".PadRight(cuisinierWidth)} | {"Prix total".PadRight(prixWidth)}");
                    Console.WriteLine(new string('-', idWidth + dateWidth + cuisinierWidth + prixWidth + 6));

                    while (reader.Read())
                    {
                        int idCommande = (int)reader["id_commande"];
                        commandeIds.Add(idCommande);

                        string id = idCommande.ToString();
                        string date = reader["date_formattee"].ToString();
                        string cuisinier = reader["nom_cuisinier"].ToString();
                        string prix = ((decimal)reader["prix_total_calcule"]).ToString("C");

                        if (cuisinier.Length > cuisinierWidth - 3)
                            cuisinier = cuisinier.Substring(0, cuisinierWidth - 3) + "...";

                        Console.WriteLine(
                            $"{id.PadRight(idWidth)} | " +
                            $"{date.PadRight(dateWidth)} | " +
                            $"{cuisinier.PadRight(cuisinierWidth)} | " +
                            $"{prix}");
                    }
                }

                if (commandeIds.Count > 0)
                {
                    Console.Write("\nVoulez-vous voir le détail d'une commande de ce client ? (O/N) : ");
                    string reponse = Console.ReadLine().ToUpper();
                    if (reponse == "O" || reponse == "OUI")
                    {
                        Console.Write($"Entrez l'ID de la commande à afficher ({string.Join(", ", commandeIds)}) : ");
                        if (int.TryParse(Console.ReadLine(), out int idCommande) && commandeIds.Contains(idCommande))
                        {
                            AfficherDetailCommandeSpecifique(idCommande);
                        }
                        else
                        {
                            Console.WriteLine("ID de commande invalide ou ne correspond pas à ce client.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
            }
        }

        private static void AfficherCommandesCuisinier()
        {
            Console.WriteLine("=== Affichage des commandes d'un cuisinier ===");

            int idCuisinier = SelectionnerCuisinier();
            if (idCuisinier == -1) return;

            try
            {
                string queryNomCuisinier = "SELECT CONCAT(prenom, ' ', nom) AS nom_complet FROM Cuisinier WHERE id_cuisinier = @idCuisinier";
                MySqlCommand cmdNomCuisinier = new MySqlCommand(queryNomCuisinier, connection);
                cmdNomCuisinier.Parameters.AddWithValue("@idCuisinier", idCuisinier);
                string nomCuisinier = (string)cmdNomCuisinier.ExecuteScalar();

                Console.WriteLine($"\n=== Commandes préparées par {nomCuisinier} (ID: {idCuisinier}) ===");

                string queryCommandes = @"
    SELECT c.id_commande, DATE_FORMAT(c.date_commande, '%d/%m/%Y') AS date_formattee, 
           cl.id_client, CONCAT(cl.prenom, ' ', cl.nom) AS nom_client,
           (SELECT SUM(p.prix * dc.quantite) 
            FROM Details_Commande dc 
            JOIN Plat p ON dc.id_plat = p.id_plat 
            WHERE dc.id_commande = c.id_commande) AS prix_total_calcule
    FROM Commande c
    JOIN Client cl ON c.id_client = cl.id_client
    WHERE c.id_cuisinier = @idCuisinier
    ORDER BY c.date_commande DESC";

                MySqlCommand cmdCommandes = new MySqlCommand(queryCommandes, connection);
                cmdCommandes.Parameters.AddWithValue("@idCuisinier", idCuisinier);

                List<int> commandeIds = new List<int>();

                using (MySqlDataReader reader = cmdCommandes.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        Console.WriteLine("Aucune commande trouvée pour ce cuisinier.");
                        return;
                    }

                    const int idWidth = 5;
                    const int dateWidth = 12;
                    const int idClientWidth = 10;
                    const int clientWidth = 30;
                    const int prixWidth = 15;

                    Console.WriteLine($"{"ID".PadRight(idWidth)} | {"Date".PadRight(dateWidth)} | {"ID Client".PadRight(idClientWidth)} | {"Client".PadRight(clientWidth)} | {"Prix total".PadRight(prixWidth)}");
                    Console.WriteLine(new string('-', idWidth + dateWidth + idClientWidth + clientWidth + prixWidth + 8));

                    while (reader.Read())
                    {
                        int idCommande = (int)reader["id_commande"];
                        commandeIds.Add(idCommande);

                        string id = idCommande.ToString();
                        string date = reader["date_formattee"].ToString();
                        string idClient = reader["id_client"].ToString();
                        string client = reader["nom_client"].ToString();
                        string prix = ((decimal)reader["prix_total_calcule"]).ToString("C");

                        if (client.Length > clientWidth - 3)
                            client = client.Substring(0, clientWidth - 3) + "...";

                        Console.WriteLine(
                            $"{id.PadRight(idWidth)} | " +
                            $"{date.PadRight(dateWidth)} | " +
                            $"{idClient.PadRight(idClientWidth)} | " +
                            $"{client.PadRight(clientWidth)} | " +
                            $"{prix}");
                    }
                }

                if (commandeIds.Count > 0)
                {
                    Console.Write("\nVoulez-vous voir le détail d'une commande de ce cuisinier ? (O/N) : ");
                    string reponse = Console.ReadLine().ToUpper();
                    if (reponse == "O" || reponse == "OUI")
                    {
                        Console.Write($"Entrez l'ID de la commande à afficher ({string.Join(", ", commandeIds)}) : ");
                        if (int.TryParse(Console.ReadLine(), out int idCommande) && commandeIds.Contains(idCommande))
                        {
                            AfficherDetailCommandeSpecifique(idCommande);
                        }
                        else
                        {
                            Console.WriteLine("ID de commande invalide ou ne correspond pas à ce cuisinier.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
            }
        }

        private static void AfficherDetailCommande()
        {
            Console.WriteLine("=== Affichage du détail d'une commande ===");

            Console.Write("Entrez l'ID de la commande : ");
            if (!int.TryParse(Console.ReadLine(), out int idCommande))
            {
                Console.WriteLine("ID invalide.");
                return;
            }

            try
            {
                string queryCommande = @"
            SELECT c.id_commande, c.date_commande, c.prix_total, 
                   cl.id_client, CONCAT(cl.prenom, ' ', cl.nom) AS nom_client,
                   cl.email AS email_client, cl.telephone AS tel_client,
                   cu.id_cuisinier, CONCAT(cu.prenom, ' ', cu.nom) AS nom_cuisinier
            FROM Commande c
            JOIN Client cl ON c.id_client = cl.id_client
            JOIN Cuisinier cu ON c.id_cuisinier = cu.id_cuisinier
            WHERE c.id_commande = @idCommande";

                MySqlCommand cmdCommande = new MySqlCommand(queryCommande, connection);
                cmdCommande.Parameters.AddWithValue("@idCommande", idCommande);

                using (MySqlDataReader reader = cmdCommande.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        Console.WriteLine("Commande non trouvée.");
                        return;
                    }

                    Console.WriteLine("\n=== DÉTAILS DE LA COMMANDE ===");
                    Console.WriteLine($"Commande #{reader["id_commande"]}");
                    Console.WriteLine($"Date: {((DateTime)reader["date_commande"]).ToString("dd/MM/yyyy")}");
                    Console.WriteLine("\n--- Informations client ---");
                    Console.WriteLine($"Client: {reader["nom_client"]} (ID: {reader["id_client"]})");
                    Console.WriteLine($"Email: {reader["email_client"]}");
                    Console.WriteLine($"Téléphone: {reader["tel_client"]}");
                    Console.WriteLine("\n--- Informations cuisinier ---");
                    Console.WriteLine($"Cuisinier: {reader["nom_cuisinier"]} (ID: {reader["id_cuisinier"]})");
                }

                Console.WriteLine("\n--- Plats commandés ---");
                string queryPlats = @"
            SELECT p.nom_plat, p.type_plat, dc.quantite, p.prix, 
                   (p.prix * dc.quantite) AS sous_total
            FROM Details_Commande dc
            JOIN Plat p ON dc.id_plat = p.id_plat
            WHERE dc.id_commande = @idCommande
            ORDER BY p.type_plat, p.nom_plat";

                MySqlCommand cmdPlats = new MySqlCommand(queryPlats, connection);
                cmdPlats.Parameters.AddWithValue("@idCommande", idCommande);

                decimal prixTotal = 0;
                using (MySqlDataReader reader = cmdPlats.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        Console.WriteLine("Aucun plat dans cette commande.");
                        return;
                    }

                    const int nomPlatWidth = 30;
                    const int typePlatWidth = 15;
                    const int quantiteWidth = 10;
                    const int prixUnitWidth = 15;
                    const int sousTotalWidth = 15;

                    Console.WriteLine($"{"Plat".PadRight(nomPlatWidth)} | {"Type".PadRight(typePlatWidth)} | {"Quantité".PadRight(quantiteWidth)} | {"Prix unitaire".PadRight(prixUnitWidth)} | {"Sous-total".PadRight(sousTotalWidth)}");
                    Console.WriteLine(new string('-', nomPlatWidth + typePlatWidth + quantiteWidth + prixUnitWidth + sousTotalWidth + 8));

                    while (reader.Read())
                    {
                        decimal sousTotal = (decimal)reader["sous_total"];
                        string nomPlat = reader["nom_plat"].ToString();
                        string typePlat = reader["type_plat"].ToString();
                        string quantite = reader["quantite"].ToString();
                        string prixUnit = ((decimal)reader["prix"]).ToString("C");
                        string sousTotal_str = sousTotal.ToString("C");

                        if (nomPlat.Length > nomPlatWidth - 3)
                            nomPlat = nomPlat.Substring(0, nomPlatWidth - 3) + "...";

                        Console.WriteLine(
                            $"{nomPlat.PadRight(nomPlatWidth)} | " +
                            $"{typePlat.PadRight(typePlatWidth)} | " +
                            $"{quantite.PadRight(quantiteWidth)} | " +
                            $"{prixUnit.PadRight(prixUnitWidth)} | " +
                            $"{sousTotal_str}");

                        prixTotal += sousTotal;
                    }
                }

                Console.WriteLine(new string('-', 90));
                Console.WriteLine($"{"PRIX TOTAL:".PadRight(73)} {prixTotal:C}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
            }
        }

        private static void AfficherDetailCommandeSpecifique(int idCommande)
        {
            try
            {
                string queryCommande = @"
            SELECT c.id_commande, c.date_commande, c.prix_total, 
                   cl.id_client, CONCAT(cl.prenom, ' ', cl.nom) AS nom_client,
                   cl.email AS email_client, cl.telephone AS tel_client,
                   cu.id_cuisinier, CONCAT(cu.prenom, ' ', cu.nom) AS nom_cuisinier
            FROM Commande c
            JOIN Client cl ON c.id_client = cl.id_client
            JOIN Cuisinier cu ON c.id_cuisinier = cu.id_cuisinier
            WHERE c.id_commande = @idCommande";

                MySqlCommand cmdCommande = new MySqlCommand(queryCommande, connection);
                cmdCommande.Parameters.AddWithValue("@idCommande", idCommande);

                using (MySqlDataReader reader = cmdCommande.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        Console.WriteLine("Commande non trouvée.");
                        return;
                    }

                    Console.WriteLine("\n=== DÉTAILS DE LA COMMANDE ===");
                    Console.WriteLine($"Commande #{reader["id_commande"]}");
                    Console.WriteLine($"Date: {((DateTime)reader["date_commande"]).ToString("dd/MM/yyyy")}");
                    Console.WriteLine("\n--- Informations client ---");
                    Console.WriteLine($"Client: {reader["nom_client"]} (ID: {reader["id_client"]})");
                    Console.WriteLine($"Email: {reader["email_client"]}");
                    Console.WriteLine($"Téléphone: {reader["tel_client"]}");
                    Console.WriteLine("\n--- Informations cuisinier ---");
                    Console.WriteLine($"Cuisinier: {reader["nom_cuisinier"]} (ID: {reader["id_cuisinier"]})");
                }

                Console.WriteLine("\n--- Plats commandés ---");
                string queryPlats = @"
            SELECT p.nom_plat, p.type_plat, dc.quantite, p.prix, 
                   (p.prix * dc.quantite) AS sous_total
            FROM Details_Commande dc
            JOIN Plat p ON dc.id_plat = p.id_plat
            WHERE dc.id_commande = @idCommande
            ORDER BY p.type_plat, p.nom_plat";

                MySqlCommand cmdPlats = new MySqlCommand(queryPlats, connection);
                cmdPlats.Parameters.AddWithValue("@idCommande", idCommande);

                decimal prixTotal = 0;
                using (MySqlDataReader reader = cmdPlats.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        Console.WriteLine("Aucun plat dans cette commande.");
                        return;
                    }

                    const int nomPlatWidth = 30;
                    const int typePlatWidth = 15;
                    const int quantiteWidth = 10;
                    const int prixUnitWidth = 15;
                    const int sousTotalWidth = 15;

                    Console.WriteLine($"{"Plat".PadRight(nomPlatWidth)} | {"Type".PadRight(typePlatWidth)} | {"Quantité".PadRight(quantiteWidth)} | {"Prix unitaire".PadRight(prixUnitWidth)} | {"Sous-total".PadRight(sousTotalWidth)}");
                    Console.WriteLine(new string('-', nomPlatWidth + typePlatWidth + quantiteWidth + prixUnitWidth + sousTotalWidth + 8));

                    while (reader.Read())
                    {
                        decimal sousTotal = (decimal)reader["sous_total"];
                        string nomPlat = reader["nom_plat"].ToString();
                        string typePlat = reader["type_plat"].ToString();
                        string quantite = reader["quantite"].ToString();
                        string prixUnit = ((decimal)reader["prix"]).ToString("C");
                        string sousTotal_str = sousTotal.ToString("C");

                        if (nomPlat.Length > nomPlatWidth - 3)
                            nomPlat = nomPlat.Substring(0, nomPlatWidth - 3) + "...";

                        Console.WriteLine(
                            $"{nomPlat.PadRight(nomPlatWidth)} | " +
                            $"{typePlat.PadRight(typePlatWidth)} | " +
                            $"{quantite.PadRight(quantiteWidth)} | " +
                            $"{prixUnit.PadRight(prixUnitWidth)} | " +
                            $"{sousTotal_str}");

                        prixTotal += sousTotal;
                    }
                }

                Console.WriteLine(new string('-', 90));
                Console.WriteLine($"{"PRIX TOTAL:".PadRight(73)} {prixTotal:C}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
            }
        }
        #endregion

        #region Module Trajet
        private static void ModuleTrajet()
        {
            Console.Clear();
            Console.WriteLine("===== MODULE TRAJET =====");

            try
            {
                var graphe = new LivInParisApp.Graphe<string>(estOriente: false);

                string cheminFichier = "C:/Users/ruben/OneDrive/Documents/arcs projet PSI.csv"; // Remplacez par le chemin réel
                graphe.ChargerDonneesCSV(cheminFichier);

                Console.WriteLine("Choisissez une action :");
                Console.WriteLine("1. Lancer l'interface graphique (Metro_GUI)");
                Console.WriteLine("2. Comparer les algorithmes du plus court chemin");
                Console.Write("Votre choix : ");
                string subChoice = Console.ReadLine();
                Console.WriteLine();

                switch (subChoice)
                {
                    case "1":
                        Application.Run(new Metro_GUI());
                        break;
                    case "2":
                        ComparerCheminsAvecInput(graphe);
                        break;
                    default:
                        Console.WriteLine("Option invalide.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur dans le module Trajet : " + ex.Message);
            }

            Console.WriteLine("\nAppuyez sur une touche pour continuer...");
            Console.ReadKey();
        }

        private static void ComparerCheminsAvecInput(LivInParisApp.Graphe<string> graphe)
        {
            Console.Write("Entrez la station de départ (par défaut 'Porte Maillot') : ");
            string departInput = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(departInput))
                departInput = "Porte Maillot";

            Console.Write("Entrez la station d'arrivée (par défaut 'Argentine') : ");
            string arriveeInput = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(arriveeInput))
                arriveeInput = "Argentine";

            var source = graphe.Noeuds.FirstOrDefault(n => n.Nom.Equals(departInput, StringComparison.InvariantCultureIgnoreCase));
            var destination = graphe.Noeuds.FirstOrDefault(n => n.Nom.Equals(arriveeInput, StringComparison.InvariantCultureIgnoreCase));

            if (source != null && destination != null)
            {
                graphe.ComparerAlgorithmes(source, destination);
            }
            else
            {
                Console.WriteLine("Les stations de départ ou d'arrivée sont introuvables.");
            }
        }
        #endregion

        // Méthodes auxiliaires pour sélectionner un client ou un cuisinier
        private static int SelectionnerClient()
        {
            Console.Write("Entrez l'ID du client : ");
            if (!int.TryParse(Console.ReadLine(), out int idClient))
            {
                Console.WriteLine("ID invalide.");
                return -1;
            }
            return idClient;
        }

        private static int SelectionnerCuisinier()
        {
            Console.Write("Entrez l'ID du cuisinier : ");
            if (!int.TryParse(Console.ReadLine(), out int idCuisinier))
            {
                Console.WriteLine("ID invalide.");
                return -1;
            }
            return idCuisinier;
        }
    }
}
