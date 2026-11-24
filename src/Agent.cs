// Occhio di Lince - agente per il giocatore.
// Non contiene alcuna logica di controllo. Flusso:
//   1. verifica di avere i permessi di amministratore (altrimenti li richiede)
//   2. chiede il codice fornito dallo staff
//   3. mostra il consenso informato (S/N) - un rifiuto viene comunicato allo staff
//   4. scarica dal server il modulo di raccolta per quella sessione e lo esegue
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Web.Script.Serialization;

[assembly: AssemblyTitle("Occhio di Lince")]
[assembly: AssemblyProduct("Occhio di Lince")]
[assembly: AssemblyDescription("Strumento di diagnostica PC per lo staff")]
[assembly: AssemblyCompany("Occhio di Lince")]
[assembly: AssemblyVersion("1.1.0.0")]
[assembly: AssemblyFileVersion("1.1.0.0")]

static class Program
{
    const string Version = "1.1.0";
    static readonly JavaScriptSerializer Json = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
    static string Server;

    static int Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        try { Console.Title = "Occhio di Lince"; } catch { }
        ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072; // TLS 1.2
        Server = AgentConfig.ServerUrl.TrimEnd('/');

        // ---- 1. permessi di amministratore obbligatori
        if (!IsAdmin())
        {
            try
            {
                var psi = new ProcessStartInfo(Assembly.GetExecutingAssembly().Location) { UseShellExecute = true, Verb = "runas" };
                Process.Start(psi);
                return 0;
            }
            catch (Win32Exception)
            {
                Banner();
                Error("Il controllo richiede i permessi di amministratore.");
                Console.WriteLine("   Riapri Occhio.exe e clicca \"Si\" sulla richiesta di Windows.");
                Pause();
                return 5;
            }
        }

        Banner();

        // ---- 2. codice dello staff
        string code = null;
        Dictionary<string, object> session = null;
        for (int attempt = 1; attempt <= 5 && session == null; attempt++)
        {
            Prompt("   Inserisci il codice fornito dallo staff: ");
            code = (Console.ReadLine() ?? "").Trim();
            if (code.Length == 0) continue;
            session = Call("/api/agent/hello", new Dictionary<string, object> { { "code", code } });
        }
        if (session == null) { Pause(); return 1; }

        // ---- 3. consenso informato
        if (!AskConsent(session))
        {
            Call("/api/agent/refuse", new Dictionary<string, object> { { "code", code } });
            Console.WriteLine();
            Warn("Hai rifiutato il controllo. Lo staff e' stato informato del rifiuto.");
            Pause();
            return 4;
        }

        var claim = Call("/api/agent/claim", new Dictionary<string, object> {
            { "code", code }, { "consent", true }, { "admin", IsAdmin() }, { "agentVersion", Version }
        });
        if (claim == null || !(claim["script"] is string)) { Pause(); return 1; }

        // ---- 4. esecuzione del modulo di raccolta
        string dir = Path.Combine(Path.GetTempPath(), "OdL_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        string ps1 = Path.Combine(dir, "c.ps1");
        int exit = 1;
        try
        {
            File.WriteAllText(ps1, (string)claim["script"], new UTF8Encoding(true));
            var psi = new ProcessStartInfo("powershell.exe", "-NoProfile -ExecutionPolicy Bypass -File \"" + ps1 + "\"");
            psi.UseShellExecute = false;
            var p = Process.Start(psi);
            p.WaitForExit();
            exit = p.ExitCode;
        }
        catch (Exception ex) { Error("Errore durante il controllo: " + ex.Message); }
        finally
        {
            try { Directory.Delete(dir, true); } catch { }
        }
        Pause();
        return exit;
    }

    static bool IsAdmin()
    {
        return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
    }

    static bool AskConsent(Dictionary<string, object> s)
    {
        Console.WriteLine();
        Line();
        Title("   RICHIESTA DI CONTROLLO PC");
        Line();
        Console.WriteLine("   Richiesto da : " + s["staff"]);
        Console.WriteLine("   Giocatore    : " + s["player"]);
        Console.WriteLine();
        Console.WriteLine("   Se accetti, Occhio di Lince leggera' con permessi di amministratore:");
        Console.WriteLine("    - programmi avviati di recente e processi in esecuzione");
        Console.WriteLine("    - cartelle di FiveM e GTA V, file scaricati, cestino, file recenti");
        Console.WriteLine("    - servizi e registri di Windows, Windows Defender, dispositivi USB");
        Console.WriteLine("    - cronologia del browser e cache DNS (solo voci legate ai cheat)");
        Console.WriteLine("    - download di programmi/archivi degli ultimi 30 giorni");
        Console.WriteLine("    - programmi ad avvio automatico e cartelle aperte di recente");
        Console.WriteLine("    - impronta hardware anonima (hash, nessun seriale in chiaro)");
        Console.WriteLine("   L'impronta (hash) dei file sospetti puo' essere verificata su");
        Console.WriteLine("   VirusTotal/MalwareBazaar: i file NON vengono mai inviati.");
        Console.WriteLine();
        Console.WriteLine("   Il programma NON modifica, NON cancella e NON installa nulla.");
        Console.WriteLine("   I dati vengono inviati in modo protetto solo allo staff del server.");
        Line();
        while (true)
        {
            Prompt("   Accetti il controllo? (S = si / N = no): ");
            string a = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
            if (a == "S" || a == "SI") return true;
            if (a == "N" || a == "NO")
            {
                Prompt("   Confermi il RIFIUTO? Lo staff ne sara' informato. (S/N): ");
                string b = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (b == "S" || b == "SI") return false;
            }
        }
    }

    // chiamata JSON al server; in caso di errore mostra il messaggio e restituisce null
    static Dictionary<string, object> Call(string path, Dictionary<string, object> body)
    {
        try
        {
            var req = (HttpWebRequest)WebRequest.Create(Server + path);
            req.Method = "POST";
            req.ContentType = "application/json";
            req.UserAgent = "OcchioDiLince-Agent/" + Version;
            req.Timeout = 30000;
            byte[] bytes = Encoding.UTF8.GetBytes(Json.Serialize(body));
            req.ContentLength = bytes.Length;
            using (var st = req.GetRequestStream()) st.Write(bytes, 0, bytes.Length);
            using (var r = (HttpWebResponse)req.GetResponse())
            using (var sr = new StreamReader(r.GetResponseStream(), Encoding.UTF8))
                return Json.Deserialize<Dictionary<string, object>>(sr.ReadToEnd());
        }
        catch (WebException ex) { Error(ReadError(ex)); }
        catch (Exception ex) { Error("Errore: " + ex.Message); }
        return null;
    }

    static string ReadError(WebException ex)
    {
        try
        {
            if (ex.Response != null)
                using (var sr = new StreamReader(ex.Response.GetResponseStream(), Encoding.UTF8))
                {
                    var d = Json.Deserialize<Dictionary<string, object>>(sr.ReadToEnd());
                    if (d != null && d.ContainsKey("error")) return d["error"].ToString();
                }
        }
        catch { }
        return "Impossibile contattare il server dello staff. Controlla la connessione e riprova.";
    }

    static void Banner()
    {
        Console.WriteLine();
        Line();
        Title("                      OCCHIO DI LINCE");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("            Strumento di diagnostica PC per lo staff");
        Console.ResetColor();
        Line();
        Console.WriteLine("   Versione " + Version);
        Console.WriteLine();
    }

    static void Line() { Console.ForegroundColor = ConsoleColor.DarkYellow; Console.WriteLine("   ========================================================="); Console.ResetColor(); }
    static void Title(string t) { Console.ForegroundColor = ConsoleColor.Yellow; Console.WriteLine(t); Console.ResetColor(); }
    static void Prompt(string t) { Console.ForegroundColor = ConsoleColor.White; Console.Write(t); Console.ResetColor(); }
    static void Warn(string t) { Console.ForegroundColor = ConsoleColor.Yellow; Console.WriteLine("   " + t); Console.ResetColor(); }
    static void Error(string t) { Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine("   " + t); Console.ResetColor(); Console.WriteLine(); }
    static void Pause() { Console.WriteLine(); Console.Write("   Premi INVIO per chiudere..."); Console.ReadLine(); }
}
