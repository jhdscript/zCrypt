using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace zCryptCore.Classes
{
    public class Crypter
    {

        #region Properties et Const
        public const string DEFAUT_KEY = "mysecretkey";
        private static byte[] FILE_BYTE_HEADER = { 7, 7, 7, 77, 77, 77, 7, 7, 7, 77, 77, 77, 7, 7, 7};
        private const string FILE_EXTENSION = ".zc";
        private const byte TYPE_DIR = 0;
        private const byte TYPE_FILE = 1;
        private const int BUFFER_SIZE = 8*1024*1024;
        private const string NEW_LINE = "\r\n";

        private static byte[] Key = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(DEFAUT_KEY));
        private static byte[] Iv = {80, 142, 193, 54, 216, 28, 190, 123, 101, 250, 93, 216, 214, 7, 229, 80};
        #endregion

        #region Crypt
        //Fonction qui lance le cryptage et qui retourne le path du fichier créé
        public static string Crypt(string sourcePath, string destPath)
        {
            string fileToCreate = "";
            try
            {
                bool isFile = false;
                if (Directory.Exists(sourcePath)){}
                else if (File.Exists(sourcePath))
                {
                    isFile = true;
                }
                else
                {
                    SourceNotFound(sourcePath);
                    return "";
                }

                if (Directory.Exists(destPath) == false)
                {
                    Directory.CreateDirectory(destPath);
                }

                fileToCreate = Path.Combine(destPath, GenerateMd5(sourcePath) + FILE_EXTENSION);
                if (File.Exists(fileToCreate))
                {
                    File.Delete(fileToCreate);
                }
                Log.Display("Generated file: " + fileToCreate, Log.ColorHelp);
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = Key;
                    aesAlg.IV = Iv;
                    using (ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(new FileStream(fileToCreate, FileMode.Create, FileAccess.Write, FileShare.None), encryptor, CryptoStreamMode.Write))
                        {
                            using (BinaryWriter bwEncrypt = new BinaryWriter(csEncrypt))
                            {
                                if (isFile)
                                {
                                    FileInfo finfo = new FileInfo(sourcePath);
                                    string arbo = "[FILE][" + ConvertToHumanReadable(finfo.Length).PadLeft(16) + "] " + GetCleanName(sourcePath, finfo.FullName, finfo.Name) + NEW_LINE;
                                    CryptGeneralHeader(bwEncrypt, finfo.Name, arbo);
                                    CryptFile(bwEncrypt, sourcePath, sourcePath);
                                }
                                else
                                {
                                    DirectoryInfo dinfo = new DirectoryInfo(sourcePath);
                                    string dparent = dinfo.Parent.FullName;
                                    dparent = Program.SuffixPath(dparent);
                                    string arbo = GenerateArborescence(dinfo);
                                    CryptGeneralHeader(bwEncrypt, dinfo.Name, arbo);
                                    CryptDir(bwEncrypt, dparent, sourcePath);
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Log.E("Crypter.Crypt", ex.Message, ex.StackTrace);
                //throw;
            }
            return fileToCreate;
        }

        //Fonction qui crypte un dossier
        private static void CryptDir(BinaryWriter bw, string originalSourcePath, string dirPath)
        {
            try
            {
                DirectoryInfo dinfo = new DirectoryInfo(dirPath);
                string cleanPath = GetCleanName(originalSourcePath, dirPath, dinfo.Name);
                if (cleanPath != null)
                {
                    CryptHeaderForFileAndDir(bw, TYPE_DIR, cleanPath, 0);
                }
                else
                {
                    cleanPath = dinfo.Name;
                }
                FileInfo[] files = dinfo.GetFiles();
                DirectoryInfo[] dirs = dinfo.GetDirectories();
                Log.Display("[DIR ][D: " + dirs.Length.ToString().PadLeft(4) + "][F: " + files.Length.ToString().PadLeft(4) + "] " + cleanPath, Log.ColorDir);

                foreach (FileInfo fi in files)
                {
                    CryptFile(bw, originalSourcePath, fi.FullName);
                }
                foreach (DirectoryInfo di in dirs)
                {
                    CryptDir(bw, originalSourcePath, di.FullName);
                }

            }
            catch (Exception ex)
            {
                Log.E("Crypter.CryptDir", ex.Message, ex.StackTrace);
                //throw;
            }
        }


        //Fonction qui crypte un fichier
        private static void CryptFile(BinaryWriter bw, string originalSourcePath, string filePath)
        {
            try
            {
                FileInfo finfo = new FileInfo(filePath);
                string cleanPath = GetCleanName(originalSourcePath, filePath, finfo.Name);
                Log.Display("[FILE][" + ConvertToHumanReadable(finfo.Length).PadLeft(16) + "] " + cleanPath, Log.ColorFile);
                CryptHeaderForFileAndDir(bw, TYPE_FILE, cleanPath, finfo.Length);

                using (BinaryReader br = new BinaryReader(new FileStream(finfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    byte[] b = new byte[BUFFER_SIZE];
                    int n = br.Read(b, 0, BUFFER_SIZE);
                    while (n > 0)
                    {
                        if (n < BUFFER_SIZE)
                        {
                            Array.Resize(ref b, n);
                        }
                        bw.Write(b);
                        bw.Flush();
                        Array.Resize(ref b, BUFFER_SIZE);
                        n = br.Read(b, 0, BUFFER_SIZE);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.E("Crypter.CryptFile", ex.Message, ex.StackTrace);
                //throw;
            }
        }

        //Fonction qui écrit le header de chaque fichier ou dossier
        private static void CryptHeaderForFileAndDir(BinaryWriter bw, byte type, string path, long length)
        {
            try
            {
                bw.Write(type);
                bw.Write(path);
                bw.Write(length);
            }
            catch (Exception ex)
            {
                Log.E("Crypter.CryptHeader", ex.Message, ex.StackTrace);
                //throw;
            }
        }

        //Fonction qui écrit le header principal d'un fichier zCrypt
        private static void CryptGeneralHeader(BinaryWriter bw, string originalName, string arbo)
        {
            try
            {
                bw.Write(FILE_BYTE_HEADER);
                bw.Write(originalName);
                bw.Write(arbo);
            }
            catch (Exception ex)
            {
                Log.E("Crypter.CryptGeneralHeader", ex.Message, ex.StackTrace);
                //throw;
            }
        }

        #endregion

        #region Decrypt

        //Fonction qui lance le decryptage
        public static void Decrypt(string sourceFile, string destPath)
        {
            try
            {
                if (File.Exists(sourceFile) == false)
                {
                    SourceNotFound(sourceFile);
                    return;
                }
                if (Directory.Exists(destPath) == false)
                {
                    Directory.CreateDirectory(destPath);
                }
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = Key;
                    aesAlg.IV = Iv;
                    using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read), decryptor, CryptoStreamMode.Read))
                        {
                            using (BinaryReader brDecrypt = new BinaryReader(csDecrypt))
                            {
                                string[] lines = DecryptGeneralHeader(brDecrypt);
                                if (lines != null && lines.Length > 0)
                                {
                                    bool decryptContinue = true;
                                    while (decryptContinue)
                                    {
                                        decryptContinue = DecryptFileAndDir(brDecrypt, destPath);
                                    }
                                }
                                else
                                {
                                    Log.Display("ERROR: " + sourceFile + " is not a valid zCrypt file or password is erronous", Log.ColorError);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.E("Crypter.Decrypt", ex.Message, ex.StackTrace);
                //throw;
            }
        }

        //Fonction qui decrypte un fichier ou un dossier
        private static bool DecryptFileAndDir(BinaryReader br, string destPath)
        {
            bool ret = true;
            try
            {
                //Header du fichier ou du dir
                byte type = br.ReadByte();
                string name = br.ReadString();
                long length = br.ReadInt64();

                string newPath = Path.Combine(destPath, name);
                if (type == TYPE_DIR)
                {
                    Log.Display("[DIR ][" + "".PadLeft(16) + "] " + name, Log.ColorDir);
                    if (Directory.Exists(newPath) == false)
                    {
                        Directory.CreateDirectory(newPath);
                    }
                }
                else
                {
                    Log.Display("[FILE][" + ConvertToHumanReadable(length).PadLeft(16) + "] " + name, Log.ColorFile);
                    using (BinaryWriter bw = new BinaryWriter(new FileStream(newPath, FileMode.Create, FileAccess.Write, FileShare.None)))
                    {
                        long bytesLeft = length;
                        int bufferSize = BUFFER_SIZE;
                        if (length < BUFFER_SIZE)
                        {
                            bufferSize = (int)length;
                        }
                        byte[] b = new byte[bufferSize];
                        int n = br.Read(b, 0, bufferSize);

                        while (n > 0)
                        {
                            bw.Write(b);
                            bw.Flush();
                            bytesLeft -= n;
                            if (bytesLeft > 0)
                            {
                                if (bytesLeft > BUFFER_SIZE)
                                {
                                    bufferSize = BUFFER_SIZE;
                                }
                                else
                                {
                                    bufferSize = (int) bytesLeft;
                                }
                                Array.Resize(ref b, bufferSize);
                                n = br.Read(b, 0, bufferSize);
                            }
                            else
                            {
                                n = 0;
                            }
                        }
                    }
                }

            }
            catch (EndOfStreamException)
            {
                ret = false;
            }
            catch (Exception ex)
            {
                ret = false;
                Log.E("Crypter.DecryptFileAndDir", ex.Message, ex.StackTrace);
                //throw;
            }
            return ret;
        }


       //Fonction qui decrypte le header general d'un fichier zync et qui retourne l'arborescence
        private static string[] DecryptGeneralHeader(BinaryReader br)
        {
            string[] ret = null;
            try
            {
                byte[] b = br.ReadBytes(FILE_BYTE_HEADER.Length);
                if (b.Length == FILE_BYTE_HEADER.Length)
                {
                    bool validHeader = true;
                    for (int i = 0; i < b.Length; i++)
                    {
                        if (b[i] != FILE_BYTE_HEADER[i])
                        {
                            validHeader = false;
                            break;
                        }
                    }
                    if (validHeader)
                    {
                        string originalName = br.ReadString();
                        string arborescence = br.ReadString(); //necessaire pour consommer l'arborescence
                        if (string.IsNullOrEmpty(originalName) == false)
                        {
                            ret = arborescence.Split(NEW_LINE.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        }
                        else
                        {
                            Log.Display("ERROR: Document name not found in zcrypt file", Log.ColorError);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.E("Crypter.DecryptGeneralHeader", ex.Message, ex.StackTrace);
                //throw;
            }
            return ret;
        }
        #endregion

        #region List
        //Fonction qui affiche les lignes d'une liste dans la console
        private static void ListDisplayLines(string[] lines)
        {
            try
            {
                if (lines != null && lines.Length > 0)
                {
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string l = lines[i];
                        string msg = "";
                        ConsoleColor color = Log.ColorInfo;

                        if (i == 0 && lines.Length == 1)
                        {
                            msg = l;
                            color = Log.ColorFile;
                        }
                        else if (i == 0)
                        {
                            msg = "* Nb Directories: " + l;
                            color = Log.ColorHelp;
                        }
                        else if (i == 1)
                        {
                            msg = "* Nb Files: " + l;
                            color = Log.ColorHelp;
                        }
                        else if (i == 2)
                        {
                            long totalSize = 0;
                            long.TryParse(l, out totalSize);
                            msg = "* Total Size: " + ConvertToHumanReadable(totalSize);
                            color = Log.ColorHelp;
                        }
                        else
                        {
                            if (l.StartsWith("[D"))
                            {
                                msg = l;
                                color = Log.ColorDir;
                            }
                            else if (l.StartsWith("[F"))
                            {
                                msg = l;
                                color = Log.ColorFile;
                            }
                            else
                            {
                                msg = l;
                            }
                        }
                        if (string.IsNullOrEmpty(msg) == false)
                        {
                            Log.Display(msg, color);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.E("Crypter.ListDisplayLines", ex.Message, ex.StackTrace);
                //throw;
            }
        }

        //Fonction qui écrit le resultat d'une liste dans un fichier
        private static void ListWriteFile(string[] lines, string destFile, bool append)
        {
            try
            {
                if (string.IsNullOrEmpty(destFile) == false)
                {
                    FileMode fm = FileMode.Create;
                    if (append)
                    {
                        fm = FileMode.Append;
                    }
                    using (StreamWriter sw = new StreamWriter(new FileStream(destFile, fm, FileAccess.Write, FileShare.None)))
                    {
                        foreach (string l in lines)
                        {
                            sw.WriteLine(l);
                        }
                        sw.WriteLine("");
                    }
                    //Log.Display("File listing exported to " + destFile, Log.ColorHelp);
                }
            }
            catch (Exception ex)
            {
                Log.Display("ERROR writing " + destFile, Log.ColorError);
                Log.E("Crypter.ListWriteFile", ex.Message, ex.StackTrace);
                //throw;
            }
        }

        //Fonction qui recupere la liste du contenu d'un fichier
        private static string[] ListGetLines(string sourceFile)
        {
            string[] lines = null;
            try
            {
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = Key;
                    aesAlg.IV = Iv;
                    using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read), decryptor, CryptoStreamMode.Read))
                        {
                            try
                            {
                                using (BinaryReader brDecrypt = new BinaryReader(csDecrypt))
                                {
                                    lines = DecryptGeneralHeader(brDecrypt);
                                }
                            }
                            catch (CryptographicException)
                            {
                                //ne rien faire
                                //throw;
                            }
                            catch (Exception)
                            {
                                //ne rien faire
                                //throw;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.E("Crypter.ListGetLines", ex.Message, ex.StackTrace);
                //throw;
            }
            return lines;
        }

        //Fonction qui traite la liste de lines lues du fichier crypté
        private static void ListProcessLines(string[] lines, string sourceFile, string destFile)
        {
            try
            {
                if (lines != null && lines.Length > 0)
                {
                    Log.Display("Crypted file " + sourceFile + " listing:", Log.ColorHelp);
                    ListDisplayLines(lines);
                    Log.Display("", Log.ColorInfo);
                    ListWriteFile(lines, destFile, false);
                }
                else
                {
                    Log.Display("ERROR: " + sourceFile + " is not a valid zCrypt file", Log.ColorError);
                }
            }
            catch (Exception ex)
            {
                Log.E("Crypter.ListProcessLines", ex.Message, ex.StackTrace);
                throw;
            }
        }

        //Fonction qui affiche le contenu d'un fichier zCrypt
        public static void List(string sourceFile, string destFile)
        {
            try
            {
                if (File.Exists(sourceFile) == false)
                {
                    Log.Display("ERROR: " + sourceFile + " not found", Log.ColorError);
                    return;
                }

                string[] lines = ListGetLines(sourceFile);
                ListProcessLines(lines, sourceFile, destFile);
            }
            catch (Exception ex)
            {
                Log.E("Crypter.List", ex.Message, ex.StackTrace);
                //throw;
            }
        }

        //Fonction qui affiche le contenu de tous les fichiers *.zc et *.zc_0000
        public static void ListAll(string sourcePath, string destFile)
        {
            try
            {
                if (Directory.Exists(sourcePath) == false)
                {
                    Log.Display("ERROR: " + sourcePath + " not found", Log.ColorError);
                    return;
                }

                DirectoryInfo dinfo = new DirectoryInfo(sourcePath);
                FileInfo[] files = (from x in dinfo.GetFiles() where x.Extension != null && x.Extension.ToLower().StartsWith(FILE_EXTENSION) select x).ToArray();

                List<string> allLines = new List<string>();
                foreach (FileInfo fi in files)
                {
                    string sourceFile = fi.FullName;
                    string[] lines = ListGetLines(sourceFile);
                    ListProcessLines(lines, sourceFile, destFile);
                }
            }
            catch (Exception ex)
            {
                Log.E("Crypter.ListAll", ex.Message, ex.StackTrace);
                //throw;
            }
        }
        #endregion

        #region Fonctions Diverses
        //Fonction qui affiche le message de source non trouvée
        private static void SourceNotFound(string sourcePath)
        {
            try
            {
                Log.Display("[ERROR] " + sourcePath + " not found", Log.ColorError);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, ex.StackTrace);
                //throw;
            }
        }

        //Fonction qui genere une arborescence
        private static string GenerateArborescence(DirectoryInfo dinfo)
        {
            string ret = "";
            try
            {
                int nbDirs = dinfo.GetDirectories("*", SearchOption.AllDirectories).Count();
                int nbFiles = dinfo.GetFiles("*", SearchOption.AllDirectories).Count();
                long totalSize = (from x in dinfo.GetFiles("*", SearchOption.AllDirectories) select x.Length).Sum();
                ret += nbDirs + NEW_LINE + nbFiles + NEW_LINE + totalSize + NEW_LINE;
                string parent = dinfo.Parent.FullName;
                parent = Program.SuffixPath(parent);
                ret += GenerateArborescenceRecursif(dinfo, parent);
            }
            catch (Exception ex)
            {
                Log.E("Crypter.GenerateArborescence", ex.Message, ex.StackTrace);
                //throw;
            }
            return ret;
        }
        private static string GenerateArborescenceRecursif(DirectoryInfo dinfo, string sourcePath)
        {
            string ret = "";
            try
            {
                DirectoryInfo[] dirs = dinfo.GetDirectories();
                FileInfo[] files = dinfo.GetFiles();
                ret += "[DIR ][D: " + dirs.Length.ToString().PadLeft(4) + "][F: " + files.Length.ToString().PadLeft(4) + "] " + GetCleanName(sourcePath, dinfo.FullName, dinfo.Name) + NEW_LINE;

                foreach (FileInfo fi in files)
                {
                    ret += "[FILE][" + ConvertToHumanReadable(fi.Length).PadLeft(16) + "] " + GetCleanName(sourcePath, fi.FullName, fi.Name) + NEW_LINE;
                }
                foreach (DirectoryInfo di in dirs)
                {
                    ret += GenerateArborescenceRecursif(di, sourcePath);
                }
            }
            catch (Exception ex)
            {
                Log.E("Crypter.GenerateArborescenceRecursif", ex.Message, ex.StackTrace);
                //throw;
            }
            return ret;
        }

        //Fonction qui duplique un string (remplace Strings.StrDup qui n'est pas dispo dans .net core)
        public static string StrDup(int loop, string s)
        {
            string ret = "";
            for (int i = 0; i < loop; i++)
            {
                ret += s;
            }
            return ret;
        }

        //Fonction qui genere un hash MD5
        private static string GenerateMd5(string input)
        {
            StringBuilder sBuilder = new StringBuilder();
            try
            {
                using (MD5 md5Hash = MD5.Create())
                {
                    byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
                    for (int i = 0; i < data.Length - 1; i++)
                    {
                        sBuilder.Append(data[i].ToString("x2"));
                    }
                }
            }
            catch (Exception ex)
            {
                Log.E("Crypter.GenerateMd5", ex.Message, ex.StackTrace);
                //throw;
            }
            return sBuilder.ToString().ToUpper();
        }

        //Fonction qui retourne un path en supprimer le prefix de source
        private static string GetCleanName(string originalSourcePath, string path, string defautValue)
        {
            string ret = path;
            try
            {
                ret = ret.Replace(originalSourcePath, "");
                if (string.IsNullOrEmpty(ret))
                {
                    ret = defautValue;
                }
            }
            catch (Exception ex)
            {
                Log.E("Crypter.GetCleanName", ex.Message, ex.StackTrace);
                //throw;
            }
            return ret;
        }

        //Fonction qui set la clé
        public static void SetKey(string newKey)
        {
            Key = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(newKey));
        }

        //Fonction qui convertit un nombre en format Human Readable
        private static string ConvertToHumanReadable(double value)
        {
            string[] units = {"", "K", "M", "G", "T", "P", "E", "Z", "Y"};
            double size = value;
            int i = 0;
            while (size >= 1024)
            {
                size /= 1024;
                i += 1;
            }
            return Math.Round(size, 2) + units[i] + "b";
        }
        #endregion
    }
}
