using System;
using System.IO;
using System.Text;
using InfrastructureTycoon.Content;

namespace InfrastructureTycoon.Simulation
{
    public sealed class SessionSaveSlot
    {
        public string FilePath { get; }
        public SessionSaveSlot(string path) { FilePath = Path.GetFullPath(path); }
        public bool TrySave(CompanySession session, string gameVersion, out string reason)
        {
            string temporary = FilePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                string json = SessionSaveJson.ToJson(SessionSaveJson.Capture(session, gameVersion));
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    var bytes = new UTF8Encoding(false).GetBytes(json);
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush(true);
                }
                // Never delete the old slot to make room: replacement must succeed atomically.
                if (File.Exists(FilePath)) File.Replace(temporary, FilePath, null);
                else File.Move(temporary, FilePath);
                reason = string.Empty;
                return true;
            }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException)
            {
                reason = "Could not write the save file.";
                return false;
            }
            catch (InvalidOperationException)
            {
                reason = "Save is unavailable during a simulation update.";
                return false;
            }
            finally
            {
                try { if (File.Exists(temporary)) File.Delete(temporary); }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
        }

        public bool TryLoad(HostingCatalog catalog, out CompanySession session, out string reason)
        {
            session = null;
            try
            {
                string json = File.ReadAllText(FilePath, Encoding.UTF8);
                if (!SessionSaveJson.TryLoad(json, catalog, out session, out _))
                {
                    reason = "Save data is invalid or unsupported.";
                    return false;
                }
                reason = string.Empty;
                return true;
            }
            catch (Exception error) when (error is FileNotFoundException || error is DirectoryNotFoundException)
            {
                reason = "No save found.";
                return false;
            }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException)
            {
                reason = "Could not read the save file.";
                return false;
            }
        }
    }
}
