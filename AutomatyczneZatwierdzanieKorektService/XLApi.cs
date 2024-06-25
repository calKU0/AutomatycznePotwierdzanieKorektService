using cdn_api;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomatyczneZatwierdzanieKorektService
{
    public class XLApi
    {
        public static readonly Int32 APIVersion = 20231;
        public readonly string database = ConfigurationManager.AppSettings["Database"];
        public static int IDSesjiXL;
        public XLApi()
        {
            
        }
        public int Login()
        {
            try
            {
                CheckApiVersion(APIVersion);

                XLLoginInfo_20231 loginInfo = new XLLoginInfo_20231();
                loginInfo.ProgramID = "Autoamtyczne Potwierdzanie Korekt";
                loginInfo.Winieta = -1;
                loginInfo.Wersja = APIVersion;
                loginInfo.Baza = database;

                int wynik_XLLogin = cdn_api.cdn_api.XLLogin(loginInfo, ref IDSesjiXL);

                if (wynik_XLLogin != 0)
                {
                    Log.Error($"Błąd logowania {wynik_XLLogin}");
                    return wynik_XLLogin;
                }

                if (loginInfo.Baza == "")
                {
                    Log.Error("Nie zalogowano do XL-a, program kończy swoje działanie");
                    return -1;
                }

                XLPolaczenieInfo_20231 polaczenieInfo = new XLPolaczenieInfo_20231();
                polaczenieInfo.Wersja = APIVersion;

                int wynik_XLPolaczenie = cdn_api.cdn_api.XLPolaczenie(polaczenieInfo);

                return wynik_XLPolaczenie;
            }
            catch (Exception ex)
            {
                Log.Error("Błąd logowania do XL-a, program kończy swoje działanie" + "\r" + ex.ToString());
                return -1;
            }
        }
        public int Logout()
        {
            try
            {
                return cdn_api.cdn_api.XLLogout(IDSesjiXL);
            }
            catch
            {
                Log.Error("Nie udało się wylogować");
                return -1;
            }
        }

        private void CheckApiVersion(Int32 APIVersion)
        {
            if (cdn_api.cdn_api.XLSprawdzWersje(ref APIVersion) != 0) // 0 api jest obsługiwane, -1 nie jest
            {
                Log.Error("Obecna wersja API nie jest obsługiwana przez obecną wersję XL-a");
            }
        }
    }
}
