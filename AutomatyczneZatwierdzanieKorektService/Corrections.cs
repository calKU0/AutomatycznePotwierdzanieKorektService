using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cdn_api;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AutomatyczneZatwierdzanieKorektService
{
    public class Corrections
    {
        private readonly string connectionString;
        private readonly XLApi XLApi;
        public Corrections(string connectionString)
        {
            this.connectionString = connectionString;
            XLApi = new XLApi();
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
            try
            {
                string query = @"UPDATE CDN.TraNag 
SET TrN_VatTyp = 2, 
TrN_VatNumer = 434, 
TrN_VatKorekta = 1, 
TrN_Stan = 3, 
TrN_OpeTypZ = 128, 
TrN_OpeFirmaZ = 449892, 
TrN_OpeNumerZ = 423, 
TrN_OpeNumerM = 423, 
TrN_WsSCHTyp = 449, 
TrN_WsDziennik = '', 
TrN_DokTypJPK = '' 
WHERE TrN_GIDTyp = @trnGidTyp AND TrN_GIDNumer = @trnGidNumer";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    foreach (DataRow row in dt.Rows)
                    {
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.CommandType = CommandType.Text;
                            command.Parameters.AddWithValue("@trnGidTyp", row["TrN_GIDTyp"]);
                            command.Parameters.AddWithValue("@trnGidNumer", row["TrN_GIDNumer"]);

                            count = command.ExecuteNonQuery();
                        }
                    }
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
