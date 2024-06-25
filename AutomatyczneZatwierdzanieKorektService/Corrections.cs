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

namespace AutomatyczneZatwierdzanieKorektService
{
    public class Corrections
    {
        private string connectionString;
        private int idPM;
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

                            if (Convert.ToBoolean(row["Czy generowac dok. magazynowe"]))
                            {
                                int result = GeneratePM(row);
                                if (result == 0)
                                {
                                    Log.Information($"Wygenerowano dokument magazynowy dla dokumentu {row["TrN_DokumentObcy"]}");
                                }
                                else
                                {
                                    Log.Warning($"Nie udało się wygenerować dokumentu magazynowego dla dokumentu {row["TrN_DokumentObcy"]}\nBłąd API: {result}");
                                }
                            }
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

        public int GeneratePM(DataRow row)
        {
            try
            {
                XLDokumentMagNagInfo_20231 PMInfo = new XLDokumentMagNagInfo_20231();
                PMInfo.Typ = 9; // PM
                PMInfo.Wersja = XLApi.APIVersion;
                PMInfo.ZrdNumer = Convert.ToInt32(row["TrN_GIDNumer"].ToString());
                PMInfo.ZrdTyp = Convert.ToInt32(row["TrN_GIDTyp"].ToString());
                PMInfo.ZrdLp = 1;
                PMInfo.ZrdFirma = 449892;

                return cdn_api.cdn_api.XLNowyDokumentMag(XLApi.IDSesjiXL, ref idPM, PMInfo);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Błąd przy generowaniu dok mag {ex}");
                Log.Error($"Błąd przy generowaniu dok Magazynowych dla dokumentu {row["TrN_DokumentObcy"]}\n{ex}");
                return 999999999;
            }
        }
    }
}
