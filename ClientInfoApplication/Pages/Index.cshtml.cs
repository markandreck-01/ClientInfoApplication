using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using System.Net;

namespace ClientInfoApplication.Pages
{
    public class IndexModel : PageModel
    {
        public List<ClientInfo> listClients = new List<ClientInfo>();
        public List<ContactInfo> listContacts = new List<ContactInfo>();
        public List<String> listClientLinks = new List<String>();
        public List<String> listContactLinks = new List<String>();

        public bool IsTableEmptyClients { get; private set; }
        public bool IsTableEmptyContacts { get; private set; }

        public String NoOfContacts;     // no of contacts linked to each client in 'clients' table
        public String NoOfClients;      // no of clients linked to each contact in 'contacts' table

        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            // check if the database tables are empty
            IsTableEmptyClients = CheckIfTableIsEmpty("clients");
            IsTableEmptyContacts = CheckIfTableIsEmpty("contacts");

            // query entries in the 'clients' database table
            try
            {
                String connectionString = "Data Source=.\\sqlexpress;Initial Catalog=myDatabase;Integrated Security=True";
                
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "SELECT * FROM clients " +
                                 "ORDER BY name";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ClientInfo clientInfo = new ClientInfo();
                                clientInfo.id = "" + reader.GetInt32(0);
                                clientInfo.name = reader.GetString(1);
                                clientInfo.clientCode = reader.GetString(2);
                                clientInfo.linkedContacts = "" + reader.GetInt32(3);

                                listClients.Add(clientInfo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.ToString());
            }


            // query entries in the 'contacts' database table
            try
            {
                String connectionString = "Data Source=.\\sqlexpress;Initial Catalog=myDatabase;Integrated Security=True";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "SELECT * FROM contacts " +
                                 "ORDER BY surname";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ContactInfo contactInfo = new ContactInfo();
                                contactInfo.id = "" + reader.GetInt32(0);
                                contactInfo.name = reader.GetString(1);
                                contactInfo.surname = reader.GetString(2);
                                contactInfo.email = reader.GetString(3);
                                contactInfo.linkedClients = "" + reader.GetInt32(4);

                                listContacts.Add(contactInfo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.ToString());
            }

            // obtain all the available 'ids' in the 'clients' database table
            try
            {
                String connectionString = "Data Source=.\\sqlexpress;Initial Catalog=myDatabase;Integrated Security=True";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "SELECT id FROM clients ";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                String clientId;
                                clientId = "" + reader.GetInt32(0);

                                listClientLinks.Add(clientId);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.ToString());
            }

            
            foreach ( var item in listClientLinks )
            {
                // query the number of times a client appears in the 'clients_linked' database table
                try
                {
                    String connectionString = "Data Source=.\\sqlexpress;Initial Catalog=myDatabase;Integrated Security=True";
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        String sql = "SELECT COUNT(*) FROM clients_linked " +
                                     "WHERE clientId=@clientId";
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@clientId", item);
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    NoOfContacts = "" + reader.GetInt32(0);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.ToString());
                }

                // update the 'LinkedContacts' column in the 'clients' database table
                try
                {
                    String connectionString = "Data Source=.\\sqlexpress;Initial Catalog=myDatabase;Integrated Security=True";
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        String sql = "UPDATE clients " +
                                     "SET linkedContacts=@linkedContacts " +
                                     "WHERE id=@id;";

                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@linkedContacts", NoOfContacts);
                            command.Parameters.AddWithValue("@id", item);

                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.ToString());
                }
            }

            // obtain all the available 'ids' in the 'contacts' database table
            try
            {
                String connectionString = "Data Source=.\\sqlexpress;Initial Catalog=myDatabase;Integrated Security=True";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "SELECT id FROM contacts ";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                String contactId;
                                contactId = "" + reader.GetInt32(0);

                                listContactLinks.Add(contactId);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.ToString());
            }


            foreach (var item in listContactLinks)
            {
                // query the number of times a client appears in the 'contacts_linked' database table
                try
                {
                    String connectionString = "Data Source=.\\sqlexpress;Initial Catalog=myDatabase;Integrated Security=True";
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        String sql = "SELECT COUNT(*) FROM contacts_linked " +
                                     "WHERE contactId=@contactId";
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@contactId", item);
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    NoOfClients = "" + reader.GetInt32(0);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.ToString());
                }

                // update the 'LinkedClients' column in the 'contacts' database table
                try
                {
                    String connectionString = "Data Source=.\\sqlexpress;Initial Catalog=myDatabase;Integrated Security=True";
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        String sql = "UPDATE contacts " +
                                     "SET linkedClients=@linkedClients " +
                                     "WHERE id=@id;";

                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@linkedClients", NoOfClients);
                            command.Parameters.AddWithValue("@id", item);

                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.ToString());
                }
            }
        }

        // checks if a given table is empty
        private bool CheckIfTableIsEmpty(string tableName)
        {
            try
            {
                String connectionString = "Data Source=.\\sqlexpress;Initial Catalog=myDatabase;Integrated Security=True";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = $"SELECT COUNT(*) FROM {tableName}";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        int count = (int)command.ExecuteScalar();
                        return count == 0;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while checking if the table is empty.");
                return true; // Consider it empty if there's an error
            }
        }
    }

    public class ClientInfo
    {
        public String id;
        public String name;
        public String clientCode;
        public String linkedContacts;
    }

    public class ContactInfo
    {
        public String id;
        public String name;
        public String surname;
        public String email;
        public String linkedClients;
    }

    public class ContactLinkClients
    {
        public String contactId;
        public String contactSurname;
        public String contactName;
        public String contactEmail;
        public String clientName;
        public String clientId;
    }

    public class ClientLinkContacts
    {
        public String clientId;
        public String clientName;
        public String clientCode;        
        public String contactSurname;
        public String contactName;
        public String contactId;
    }
}
