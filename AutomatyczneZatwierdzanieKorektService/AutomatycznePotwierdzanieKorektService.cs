using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serilog.Events;
using System.Timers;
using System.Data.SqlClient;
using System.Configuration;
using cdn_api;

namespace AutomatyczneZatwierdzanieKorektService
{
    public partial class AutomatycznePotwierdzanieKorektService : ServiceBase
    {
        public static int IDSesjiXL = 0;
        public static readonly Int32 APIVersion = 20231;
        private System.Timers.Timer timer = new System.Timers.Timer();
        private readonly string database = ConfigurationManager.AppSettings["Database"];
        private readonly int serviceEndHour = int.Parse(ConfigurationManager.AppSettings["ServiceEndHour"]);
        private readonly int serviceStartHour = int.Parse(ConfigurationManager.AppSettings["ServiceStartHour"]);
        private readonly int checkTimeMin = int.Parse(ConfigurationManager.AppSettings["CheckTimeMin"]);
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["GaskaConnectionString"].ConnectionString;
        private static AutoResetEvent autoResetEvent = new AutoResetEvent(false);
        private DataTable dtCorrections;
        private Thread threadConfirm;
        private Thread threadTimer;
        private Corrections corrections;
        private XLApi xlAPI;
        public AutomatycznePotwierdzanieKorektService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(Path.Combine(logDir, "log.txt"), rollingInterval: RollingInterval.Month)
                .CreateLogger();

            Log.Information("Uruchomienie usługi");

            try
            {
                xlAPI = new XLApi();
                if (xlAPI.Login() == 0)
                {
                    Log.Information("Zalogowano do XLa");
                }
                corrections = new Corrections(connectionString);

                threadTimer = new Thread(Timer);
                threadTimer.Start();
            }
            catch (Exception ex)
            {
                Log.Error("Błąd OnStart. " + ex.ToString());
                Stop();
            }
        }

        protected override void OnStop()
        {
            Log.Information("Zatrzymanie usługi");
            Log.CloseAndFlush();
            xlAPI.Logout();

            timer.Stop();
            if ((threadTimer.ThreadState & System.Threading.ThreadState.Running) == System.Threading.ThreadState.Running) { threadTimer.Abort(); }
        }

        public void OnTimer(object sender, ElapsedEventArgs args)
        {
            try
            {
                int aktualnaGodzina = int.Parse(DateTime.Now.Hour.ToString());
                string data = DateTime.Now.ToShortDateString();

                if (aktualnaGodzina >= serviceStartHour && aktualnaGodzina  <= serviceEndHour)
                {
                    threadConfirm = new Thread(Procces);
                    threadConfirm.Start();
                }
            }
            catch (Exception ex) { Log.Error(ex.ToString()); }
        }
        private void Timer()
        {
            try
            {
                timer.Interval = checkTimeMin * 60000;
                timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
                timer.Start();

                autoResetEvent.WaitOne(); // czekam na sygnał zatrzymania wątku
            }
            catch (ThreadAbortException) { }
            catch (Exception ex) { Log.Error(ex.ToString()); }
        }

        public void Procces()
        {
            try
            {
                dtCorrections = corrections.GetCorrections(); // Pobieramy korekty do potwierdzenia
                corrections.ConfirmCorrections(dtCorrections); // Potwierdzamy korekty
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }
    }
}
