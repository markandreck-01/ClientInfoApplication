using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Data.SqlClient;

namespace ClientInfoApplication.Pages
{
    public class ClientsModel : PageModel
    {
        public List<ClientInfo> listClients = new List<ClientInfo>();
        public List<ContactInfo> listContacts = new List<ContactInfo>();
        public List<ContactLinkClients> listContactLinkClients = new List<ContactLinkClients>();

        public ContactLinkClients contactLinkClients = new ContactLinkClients();
        public ClientInfo clientInfo = new ClientInfo();

        public String errorMessage = "";
        public String successMessage = "";

        public bool IsTableEmpty { get; private set; }

        private static HashSet<int> generatedCodes = new HashSet<int>(); // To keep track of unique codes


        public void OnGet()
        {
            // check if the 'contacts_linked' database table is empty
            IsTableEmpty = CheckIfTableIsEmpty("clients_linked");

            // query entries in the 'clients' database table for the 'Link a contact to client(s)' form
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
                                Console.WriteLine("finished reading the table");
                                Console.WriteLine(listClients);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.ToString());
            }

            // query entries in the 'contacts' database table for the 'Link a contact to client(s)' form
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

            // query entries in the 'clients_linked' database table to display the 'List of Contacts and their linked Clients' table
            try
            {
                String connectionString = "Data Source=.\\sqlexpress;Initial Catalog=myDatabase;Integrated Security=True";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "SELECT * FROM clients_linked " +
                                 "ORDER BY contactSurname";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ContactLinkClients contactLinkClients = new ContactLinkClients();
                                contactLinkClients.contactId = "" + reader.GetInt32(0);
                                contactLinkClients.contactSurname = reader.GetString(1);
                                contactLinkClients.contactName = reader.GetString(2);
                                contactLinkClients.contactEmail = reader.GetString(3);
                                contactLinkClients.clientName = reader.GetString(4);
                                contactLinkClients.clientId = "" + reader.GetInt32(5);

                                listContactLinkClients.Add(contactLinkClients);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.ToString());
            }
        }

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
                errorMessage = ex.Message;
                return true; // Consider it empty if there's an error
            }
        }

        // handles a post from the New Client form
        public void OnPostCreateClient()
        {
            // capture inputs from the New Client form
            clientInfo.name = Request.Form["name"];

            // perform form input validation
            if (clientInfo.name.Length == 0)
            {
                errorMessage = "Please provide the client name";
                return;
            }

            // generate a client code using user input value
            clientInfo.clientCode = generateClientCode(clientInfo.name);

            //save the New Client entry into the 'clients' database table
            try
            {
                String connectionString = "Data Source=.\\sqlexpress;Initial Catalog=myDatabase;Integrated Security=True";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "INSERT INTO clients " +
                                 "(name, clientCode, linkedContacts) VALUES " +
                                 "(@name, @clientCode, @linkedContacts);";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@name", clientInfo.name);
                        command.Parameters.AddWithValue("@clientCode", clientInfo.clientCode);
                        command.Parameters.AddWithValue("@linkedContacts", 0);  // default linked contacts value

                        command.ExecuteNonQuery();
                        ;
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return;
            }

            // display success message
            successMessage = "New Client Successfully Added";

            // redirect to display updated clients table
            //Response.Redirect("/Index");
        }

        // handles a new link contact to client(s)
        public void OnPostLinkClients(int[] linkedClientIds)
        {
            contactLinkClients.contactId = Request.Form["selectedContactId"];

            // perform form input validation
            if (contactLinkClients.contactId.Count() == 0 || linkedClientIds.Count() == 0)
            {
                errorMessage = "Please select a Contact and at least 1 Client";
                return;
            }

            // extract corresponding info for the obtained linked client ids
            foreach (var clientId in linkedClientIds)
            {
                //obtain contact name, surname and email from the 'contacts' database table using the contactId
                try
                {
                    String connectionString = "Data Source=.\\sqlexpress;Initial Catalog=myDatabase;Integrated Security=True";
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        String sql = "SELECT name, surname, email FROM contacts " +
                                     "WHERE id=@contactId";
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@contactId", contactLinkClients.contactId);
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    contactLinkClients.contactName = reader.GetString(0);
                                    contactLinkClients.contactSurname = reader.GetString(1);
                                    contactLinkClients.contactEmail = reader.GetString(2);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.ToString());
                }

                //obtain client name from the 'clients' table using the clientId
                try
                {
                    String connectionString = "Data Source=.\\sqlexpress;Initial Catalog=myDatabase;Integrated Security=True";
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        String sql = "SELECT name FROM clients " +
                                     "WHERE id=@clientId";
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@clientId", clientId);
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    contactLinkClients.clientName = reader.GetString(0);
                                    contactLinkClients.clientId = "" + clientId;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.ToString());
                }

                // save new linked contact and client(s) to 'clients_linked' database table
                try
                {
                    String connectionString = "Data Source=.\\sqlexpress;Initial Catalog=myDatabase;Integrated Security=True";
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        String sql = "INSERT INTO clients_linked " +
                                     "(contactId, contactSurname, contactName, contactEmail, clientName, clientId) VALUES " +
                                     "(@contactId, @contactSurname, @contactName, @contactEmail, @clientName, @clientId);";

                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@contactId", contactLinkClients.contactId);
                            command.Parameters.AddWithValue("@contactSurname", contactLinkClients.contactSurname);
                            command.Parameters.AddWithValue("@contactName", contactLinkClients.contactName); 
                            command.Parameters.AddWithValue("@contactEmail", contactLinkClients.contactEmail);
                            command.Parameters.AddWithValue("@clientName", contactLinkClients.clientName);
                            command.Parameters.AddWithValue("@clientId", contactLinkClients.clientId);

                            command.ExecuteNonQuery();
                            ;
                        }
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    return;
                }
            }

            Console.WriteLine("List clients after link submission: ");
            Console.WriteLine(listContactLinkClients);


            //redirect to display updated 'List of Contacts' table
            Response.Redirect("/Clients");
        }

        private string generateClientCode(string name)
        {
            // Extract the first three uppercase letters from the name
            var letters = new string(name.ToUpper()
                .Where(char.IsLetter)
                .Distinct()
                .Take(3)
                .ToArray());

            // If less than 3 letters, pad with 'A-Z' letters
            while (letters.Length < 3)
            {
                if (letters.Length == 1)
                {
                    letters += "AB";
                }
                if (letters.Length == 2)
                {
                    letters += "A";
                }
            }

            // Generate 3 random digits
            int digits;
            Random random = new Random();
            do
            {
                digits = random.Next(0, 101); // Generates a number between 0 and 100
            } while (!generatedCodes.Add(digits)); // Continue until a unique number is found

            // Combine letters and digits
            return letters + digits.ToString("D3");
        }

    }
}
