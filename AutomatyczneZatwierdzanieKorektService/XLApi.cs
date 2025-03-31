using cdn_api;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AutomatyczneZatwierdzanieKorektService
{
    public class XLApi
    {
        public static readonly Int32 APIVersion = 20231;
        private readonly string database = ConfigurationManager.AppSettings["Database"];
        private readonly string XLLogin = ConfigurationManager.AppSettings["XLLogin"];
        private readonly string XLHaslo = ConfigurationManager.AppSettings["XLPassword"];
        public static int IDSesjiXL;
        private int IdDoc;
        public XLApi()
        {
            
        }
        public int Login()
        {
            try
            {
                CheckApiVersion(APIVersion);

                XLLoginInfo_20231 loginInfo = new XLLoginInfo_20231();
                loginInfo.ProgramID = "Automatyczne Potwierdzanie Korekt";
                loginInfo.Winieta = -1;
                loginInfo.Wersja = APIVersion;
                loginInfo.Baza = database;
                loginInfo.OpeIdent = XLLogin;
                loginInfo.OpeHaslo = XLHaslo;
                loginInfo.TrybWsadowy = 1;
                loginInfo.TrybNaprawy = 0;

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

        public int CloseDocument()
        {
            AutomatycznePotwierdzanieKorektService.AttachThreadToClarion(1);

            int result = -1;
            try
            {
                XLZamkniecieDokumentuInfo_20231 xlZamDoc = new XLZamkniecieDokumentuInfo_20231();
                xlZamDoc.Wersja = APIVersion;
                xlZamDoc.Tryb = 1; // Confirmation          

                result = cdn_api.cdn_api.XLZamknijDokument(IdDoc, xlZamDoc);

                if (result != 0)
                {
                    Log.Error($"Błąd API przy zamykaniu dokumentu{Environment.NewLine}Kod błędu: {result}");
                }
                else
                {
                    Log.Information($"Zamykam dokument GIDNumer: {IdDoc}");
                }
                result = result == 0 ? 1 : result;
                return result;
            }
            catch(Exception ex)
            {
                Log.Error($"Błąd podczas zamykania dokumentu: {IdDoc}{Environment.NewLine}{ex}");
                return result;
            }
        }

        public int OpenDocument(int gidNumber, int gidTyp)
        {
            AutomatycznePotwierdzanieKorektService.AttachThreadToClarion(1);

            int result = -1;
            try
            {
                XLOtwarcieNagInfo_20231 xlOtwDoc = new XLOtwarcieNagInfo_20231();
                xlOtwDoc.Wersja = APIVersion;
                xlOtwDoc.GIDNumer = gidNumber;
                xlOtwDoc.GIDTyp = gidTyp;
                xlOtwDoc.GIDFirma = 449892;
                xlOtwDoc.GIDLp = 0;
                xlOtwDoc.Tryb = 2;
                xlOtwDoc.TrybNaprawy = 2;

                result = cdn_api.cdn_api.XLOtworzDokument(IDSesjiXL, ref IdDoc, xlOtwDoc);

                if (result != 0)
                {
                    Log.Error($"Błąd API przy otwieraniu dokumentu{Environment.NewLine}Kod błędu: {result}");
                }
                else
                {
                    Log.Information($"Otwieram dokument GIDNumer: {IdDoc}");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                Log.Error($"Błąd podczas otwierania dokumentu: {IdDoc}{Environment.NewLine}{ex}");
                return result;
            }
        }

        public int CreateAWD(int RLSgidNumer, int RLSgidTyp)
        {
            AutomatycznePotwierdzanieKorektService.AttachThreadToClarion(1);

            int result = -1;
            try
            {
                XLDokumentMagNagInfo_20231 xlDokMag = new XLDokumentMagNagInfo_20231();
                xlDokMag.Wersja = APIVersion;
                xlDokMag.ZrdNumer = RLSgidNumer;
                xlDokMag.ZrdTyp = RLSgidTyp;
                xlDokMag.ZrdFirma = 449892;
                xlDokMag.ZrdLp = 0;
                xlDokMag.Tryb = 2;
                xlDokMag.Typ = 1093;

                result = cdn_api.cdn_api.XLNowyDokumentMag(IDSesjiXL, ref IdDoc, xlDokMag);

                if (result != 0)
                {
                    Log.Error($"Błąd API przy otwieraniu dokumentu{Environment.NewLine}Kod błędu: {result}");
                }
                else
                {
                    Log.Information($"Otwieram dokument GIDNumer: {IdDoc}");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                Log.Error($"Błąd podczas otwierania dokumentu: {IdDoc}{Environment.NewLine}{ex}");
                return result;
            }
        }

        public int AddElements(int twrGid)
        {
            AutomatycznePotwierdzanieKorektService.AttachThreadToClarion(1);

            int result = -1;
            try
            {
                XLDokumentMagElemInfo_20231 xlDokMag = new XLDokumentMagElemInfo_20231();
                xlDokMag.Wersja = APIVersion;
                xlDokMag.Ilosc = "1";
                xlDokMag.TwrNumer = twrGid;

                result = cdn_api.cdn_api.XLDodajPozycjeMag(IdDoc, xlDokMag);

                if (result != 0)
                {
                    Log.Error($"Błąd API przy otwieraniu dokumentu{Environment.NewLine}Kod błędu: {result}");
                }
                else
                {
                    Log.Information($"Otwieram dokument GIDNumer: {IdDoc}");
                }

                return result;
            }
            catch (Exception ex)
            {
                Log.Error($"Błąd podczas otwierania dokumentu: {IdDoc}{Environment.NewLine}{ex}");
                return result;
            }
        }
    }
}
