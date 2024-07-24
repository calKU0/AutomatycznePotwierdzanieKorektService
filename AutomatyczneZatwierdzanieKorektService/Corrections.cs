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
            int openResult;
            try
            {
                foreach (DataRow row in dt.Rows)
                {
                    openResult = XLApi.OpenDocument(Convert.ToInt32(row["TrN_GIDNumer"].ToString()), Convert.ToInt32(row["TrN_GIDTyp"].ToString()));
                    if (openResult == 0)
                    {
                        count += XLApi.CloseDocument();
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
