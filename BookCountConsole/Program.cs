using System;
using System.Data.SqlClient;

class Program
{
    static void Main(string[] args)
    {
        string connectionString = "Server=localhost,1433;Database=BulkyDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;";
        
        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) as BookCount FROM Products";
                
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    int count = (int)command.ExecuteScalar();
                    Console.WriteLine($"Veritabanında {count} kitap bulunmaktadır.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Hata oluştu: {ex.Message}");
        }
    }
} 