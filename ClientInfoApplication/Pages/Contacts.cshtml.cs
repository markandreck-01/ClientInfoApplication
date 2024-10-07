using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;

namespace ClientInfoApplication.Pages
{
    public class ContactsModel : PageModel
    {
        public List<ClientInfo> listClients = new List<ClientInfo>();
        public List<ContactInfo> listContacts = new List<ContactInfo>();
        public List<ClientLinkContacts> listClientLinkContacts = new List<ClientLinkContacts>();

        public ClientLinkContacts clientLinkContacts = new ClientLinkContacts();
        public ContactInfo contactInfo = new ContactInfo();

        public String errorMessage = "";
        public String successMessage = "";

        public bool IsTableEmpty { get; private set; }

        public void OnGet()
        {
            // check if the 'clients_linked' database table is empty
            IsTableEmpty = CheckIfTableIsEmpty("contacts_linked");

            // query entries in the 'clients' database table for the 'Link a client to contact(s)' form
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
                                clientInfo.linkedContacts = "" + reader.GetInt32(0);

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

            // query entries in the 'contacts' database table for the 'Link a client to contact(s)' form
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

            // query entries in the 'contacts_linked' database table to display the 'List of Clients and their linked Contacts' table
            try
            {
                String connectionString = "Data Source=.\\sqlexpress;Initial Catalog=myDatabase;Integrated Security=True";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "SELECT * FROM contacts_linked " +
                                 "ORDER BY clientName";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ClientLinkContacts clientLinkContacts = new ClientLinkContacts();
                                clientLinkContacts.clientId = "" + reader.GetInt32(0);
                                clientLinkContacts.clientName = reader.GetString(1);
                                clientLinkContacts.clientCode = reader.GetString(2);
                                clientLinkContacts.contactSurname = reader.GetString(3);
                                clientLinkContacts.contactName = reader.GetString(4);
                                clientLinkContacts.contactId = "" + reader.GetInt32(5);

                                listClientLinkContacts.Add(clientLinkContacts);
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

        // handles a post from the New Contact form
        public void OnPostCreateContact()
        {
            // capture inputs from the New Contact form
            contactInfo.name = Request.Form["name"];
            contactInfo.surname = Request.Form["surname"];
            contactInfo.email = Request.Form["email"];

            // perform form input validation
            if (contactInfo.name.Length == 0 || contactInfo.surname.Length == 0 || contactInfo.email.Length == 0)
            {
                errorMessage = "Please fill in all the fields";
                return;
            }

            //save the New Contact entry into the 'contacts' database table
            try
            {
                String connectionString = "Data Source=.\\sqlexpress;Initial Catalog=myDatabase;Integrated Security=True";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "INSERT INTO contacts " +
                                 "(name, surname, email, linkedClients) VALUES " +
                                 "(@name, @surname, @email, @linkedClients);";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@name", contactInfo.name);
                        command.Parameters.AddWithValue("@surname", contactInfo.surname);
                        command.Parameters.AddWithValue("@email", contactInfo.email);
                        command.Parameters.AddWithValue("@linkedClients", 0);   // default linked clients value

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

            // clear form fields
            contactInfo.name = "";
            contactInfo.surname = "";
            contactInfo.email = "";
            successMessage = "New Client Successfully Added";

            //redirect to display updated contacts table
            Response.Redirect("/Index");
        }

        // handles a new link client to contact(s)
        public void OnPostLinkContacts(int[] linkedContactIds)
        {
            clientLinkContacts.clientId = Request.Form["selectedClientId"];

            // perform form input validation
            if ( clientLinkContacts.clientId.Count() == 0 || linkedContactIds.Count() == 0)
            {
                errorMessage = "Please select a Client and at least 1 Contact";
                return;
            }

            // extract corresponding info for the obtained linked contact ids
            foreach (var contact in linkedContactIds)
            {
                //obtain client name and client code from the 'clients' table using the clientId
                try
                {
                    String connectionString = "Data Source=.\\sqlexpress;Initial Catalog=myDatabase;Integrated Security=True";
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        String sql = "SELECT name, clientCode FROM clients " +
                                     "WHERE id=@clientId";
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@clientId", clientLinkContacts.clientId);
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    clientLinkContacts.clientName = reader.GetString(0);
                                    clientLinkContacts.clientCode = reader.GetString(1);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.ToString());
                }

                //obtain contact surname and name from the 'contacts' table using the contactId
                try
                {
                    String connectionString = "Data Source=.\\sqlexpress;Initial Catalog=myDatabase;Integrated Security=True";
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        String sql = "SELECT surname, name FROM contacts " +
                                     "WHERE id=@contact";
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@contact", contact);
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    clientLinkContacts.contactSurname = reader.GetString(0);
                                    clientLinkContacts.contactName = reader.GetString(1);
                                    clientLinkContacts.contactId = "" + contact;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.ToString());
                }
                
                // save new linked client and contact(s) to 'contacts_linked' database table
                try
                {
                    String connectionString = "Data Source=.\\sqlexpress;Initial Catalog=myDatabase;Integrated Security=True";
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        String sql = "INSERT INTO contacts_linked " +
                                     "(clientId, clientName, clientCode, contactSurname, contactName, contactId) VALUES " +
                                     "(@clientId, @clientName, @clientCode, @contactSurname, @contactName, @contactId);";

                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@clientId", clientLinkContacts.clientId);
                            command.Parameters.AddWithValue("@clientName", clientLinkContacts.clientName);
                            command.Parameters.AddWithValue("@clientCode", clientLinkContacts.clientCode);
                            command.Parameters.AddWithValue("@contactSurname", clientLinkContacts.contactSurname);
                            command.Parameters.AddWithValue("@contactName", clientLinkContacts.contactName);
                            command.Parameters.AddWithValue("@contactId", clientLinkContacts.contactId);

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

            // clear form fields
            //clientLinkContacts.contactId = "";
            //clientLinkContacts.clientId = "";
            //contactInfo.email = "";
            successMessage = "Client successfully linked to Contact(s)";

            ////redirect to display updated contacts table
            Response.Redirect("/Contacts");
        }
    }
}
