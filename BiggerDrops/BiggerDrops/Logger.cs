using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace BiggerDrops {
  public enum LogFileType {
    Main
  }
  public class LogFile {
    private string m_logfile;
    private Mutex mutex;
    private StringBuilder m_cache = null;
    private StreamWriter m_fs = null;
    private bool enabled;
    public LogFile(string name, bool enabled) {
      try {
        this.mutex = new Mutex();
        this.enabled = enabled;
        this.m_cache = new StringBuilder();
        this.m_logfile = Path.Combine(Logger.BaseDirectory, name);
        File.Delete(this.m_logfile);
        this.m_fs = new StreamWriter(this.m_logfile);
        this.m_fs.AutoFlush = true;
      } catch (Exception) {

      }
    }
    public void flush() {
      if (this.mutex.WaitOne(1000)) {
        this.m_fs.Write(this.m_cache.ToString());
        this.m_fs.Flush();
        this.m_cache.Length = 0;
        this.mutex.ReleaseMutex();
      }
    }
    public void W(string line, bool isCritical = false) {
      if ((this.enabled) || (isCritical)) {
        if (this.mutex.WaitOne(1000)) {
          m_cache.Append(line);
          this.mutex.ReleaseMutex();
        }
        if (isCritical) { this.flush(); };
        if (m_logfile.Length > Logger.flushBufferLength) { this.flush(); };
      }
    }
    public void WL(string line, bool isCritical = false) {
      line += "\n"; this.W(line, isCritical);
    }
    public void W(int initiation, string line, bool isCritical = false) {
      string init = new string(' ', initiation);
      line = init + line; this.W(line, isCritical);
    }
    public void WL(int initiation, string line, bool isCritical = false) {
      string init = new string(' ', initiation);
      line = init + line; this.WL(line, isCritical);
    }
    public void TW(int initiation, string line, bool isCritical = false) {
      string init = new string(' ', initiation);
      line = "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "]" + init + line;
      this.W(line, isCritical);
    }
    public void TWL(int initiation, string line, bool isCritical = false) {
      string init = new string(' ', initiation);
      line = "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "]" + init + line;
      this.WL(line, isCritical);
    }
  }
  public static class Logger {
    private static Dictionary<LogFileType, LogFile> logs = new Dictionary<LogFileType, LogFile>();
    //private static string m_assemblyFile;
    public static string BaseDirectory;
    public static readonly int flushBufferLength = 16 * 1024;
    public static bool flushThreadActive = true;
    public static Thread flushThread = new Thread(flushThreadProc);
    public static void flushThreadProc() {
      while (Logger.flushThreadActive == true) {
        Thread.Sleep(30 * 1000);
        Logger.LogWrite("Log flushing\n");
        Logger.flush();
      }
    }
    public static void flush() {
      foreach (var log in Logger.logs) { log.Value.flush(); }
    }
    public static void LogWrite(string line, bool isCritical = false) {
      if (Logger.logs.ContainsKey(LogFileType.Main) == false) { return; }
      Logger.logs[LogFileType.Main].W(line, isCritical);
    }
    public static LogFile M { get { return Logger.logs[LogFileType.Main]; } }
    public static void InitLog() {
      //LogFile file = new LogFile("CAC_main_log.txt", CustomAmmoCategories.Settings.debugLog);
      Logger.logs.Add(LogFileType.Main, new LogFile("ELog.txt", BiggerDrops.settings.debugLog));
      //Log.logs.Add(LogFileType.Main, null);
      Logger.flushThread.Start();
    }

    static string filePath = $"{BiggerDrops.ModDirectory}/Log.txt";
    public static void LogError(Exception ex) {
      Logger.M.WL("Message :" + ex.Message + "<br/>" + Environment.NewLine + "StackTrace :" + ex.StackTrace + "" + Environment.NewLine + "Date :" + DateTime.Now.ToString(), true);
      Logger.M.WL(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine, true);
      /*(new FileInfo(filePath)).Directory.Create();
      using (StreamWriter writer = new StreamWriter(filePath, true)) {
        writer.WriteLine("Message :" + ex.Message + "<br/>" + Environment.NewLine + "StackTrace :" + ex.StackTrace +
           "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
        writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
      }
      Logger.M.WL("Message :" + ex.Message + "<br/>" + Environment.NewLine + "StackTrace :" + ex.StackTrace + "" + Environment.NewLine + "Date :" + DateTime.Now.ToString(),true);
      Logger.M.WL(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine, true);*/
    }

    public static void LogLine(String line) {
      Logger.M.WL(line + Environment.NewLine + "Date :" + DateTime.Now.ToString(), true);
      Logger.M.WL(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine, true);
      /*(new FileInfo(filePath)).Directory.Create();
      using (StreamWriter writer = new StreamWriter(filePath, true)) {
        writer.WriteLine(line + Environment.NewLine + "Date :" + DateTime.Now.ToString());
        writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
      }*/
    }
  }
}