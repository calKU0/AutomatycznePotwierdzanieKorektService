using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomatyczneZatwierdzanieKorektService
{
    public class Corrections
    {
        private string connectionString;
        public Corrections(string connectionString)
        {
            this.connectionString = connectionString;
        }
        public DataTable GetCorrections()
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand("dbo.GaskaAutomatycznePotwierdzanieKorekt", connection) { CommandType = CommandType.StoredProcedure })
                {
                    connection.Open();

                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    adapter.Fill(dt);
                }
                return dt;
            }
            catch (Exception ex)
            {
                Log.Error("Błąd w pobieraniu korekt " + ex);
                return dt;
            }
        }
        public int ConfirmCorrections(DataTable dt)
        {
            int count = 0;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    foreach (DataRow row in dt.Rows)
                    {
                        string query = "UPDATE cdn.TraNag SET Trn_Stan = 3, TrN_OpeNumerZ = 589, TrN_OpeTypZ = 128, TrN_OpeFirmaZ = 449892, TrN_OpeLpZ = 0 WHERE Trn_GidNumer = @GidNumer and Trn_GidTyp = @GidTyp";
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@GidNumer", row["TrN_GIDNumer"]);
                            command.Parameters.AddWithValue("@GidTyp", row["TrN_GIDTyp"]);
                            int rowsAffected = command.ExecuteNonQuery();
                            if (rowsAffected < 0) 
                            { 
                                rowsAffected = 0;
                                Log.Warning("Nie udało się potwierdzić korekty " + row["TrN_DokumentObcy"]);
                            }
                            count += rowsAffected;
                        };
                    }
                    return count;
                }
                catch (Exception ex)
                {
                    Log.Error("Błąd w potwierdzaniu korekt " + ex);
                    return count;
                }
            }
        }
    }
}
