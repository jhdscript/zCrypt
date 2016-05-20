using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using zCryptCore.Classes;

namespace zCryptCore
{
    public class Program
    {
        private const string ACTION_CRYPT = "crypt";
        private const string ACTION_DECRYPT = "decrypt";
        private const string ACTION_SPLIT = "split";
        private const string ACTION_ASSEMBLE = "assemble";
        private const string ACTION_LIST = "list";
        private const string ACTION_LISTALL = "listall";

        public static void Main(string[] args)
        {
            DisplayHeader();
            Stopwatch stw = new Stopwatch();
            stw.Start();

            if (args == null || args.Length < 2)
            {
                DisplayHelp();
            }
            else
            {
                string action = args[0].ToLower();

                switch (action)
                {
                    case ACTION_SPLIT:
                        if (args.Length < 4)
                        {
                            DisplayHelp();
                        }
                        else
                        {
                            string sourcePath = args[2];
                            string destPath = SuffixPath(args[3]);
                            int splitSizeInMb = 1024;
                            int.TryParse(args[1], out splitSizeInMb);
                            Splitter.Split(splitSizeInMb, sourcePath, destPath);
                        }
                        break;

                    case ACTION_ASSEMBLE:
                        if (args.Length < 3)
                        {
                            DisplayHelp();
                        }
                        else
                        {
                            string sourcePath = args[1];
                            string destPath = SuffixPath(args[2]);
                            Splitter.Assemble(sourcePath, destPath);
                        }
                        break;

                    case ACTION_CRYPT:
                        if (args.Length < 4)
                        {
                            DisplayHelp();
                        }
                        else
                        {
                            string sourcePath = args[2];
                            string destPath = args[3];
                            SetEncryptionKey(args[1]);
                            DisplayInfo(action, sourcePath, destPath);
                            if (Directory.Exists(sourcePath))
                            {
                                sourcePath = SuffixPath(sourcePath);
                            }
                            Crypter.Crypt(sourcePath, destPath);
                        }
                        break;

                    case ACTION_DECRYPT:
                        if (args.Length < 4)
                        {
                            DisplayHelp();
                        }
                        else
                        {
                            string sourcePath = args[2];
                            string destPath = SuffixPath(args[3]);
                            SetEncryptionKey(args[1]);
                            DisplayInfo(action, sourcePath, destPath);
                            Crypter.Decrypt(sourcePath, destPath);
                        }
                        break;

                    case ACTION_LIST:
                        if (args.Length < 3)
                        {
                            DisplayHelp();
                        }
                        else
                        {
                            string sourcePath = args[2];
                            string destPath = null;
                            if (args.Length == 4)
                            {
                                destPath = args[3];
                            }
                            SetEncryptionKey(args[1]);
                            DisplayInfo(action, sourcePath, destPath);
                            Crypter.List(sourcePath, destPath);
                        }
                        break;

                    case ACTION_LISTALL:
                        if (args.Length < 3)
                        {
                            DisplayHelp();
                        }
                        else
                        {
                            string sourcePath = args[2];
                            string destPath = null;
                            if (args.Length == 4)
                            {
                                destPath = args[3];
                            }
                            SetEncryptionKey(args[1]);
                            DisplayInfo(action, sourcePath, destPath);
                            Crypter.ListAll(sourcePath, destPath);
                        }
                        break;

                    default:
                        DisplayHelp();
                        break;
                }

            }

            stw.Stop();
            DisplayFinish(stw.Elapsed);
            Console.ReadLine();
        }

        #region Fonctions Communes
        //Fonction qui ajoute un suffixe au path si necessaire
        public static string SuffixPath(string path)
        {
            string suff = "/";
            if (System.Reflection.Assembly.GetEntryAssembly().Location.Contains(suff) == false)
            {
                suff = "\\";
            }
            if (path.EndsWith(suff) == false)
            {
                path += suff;
            }
            return path;
        }

        //Fonction qui set la clé d'encryption
        private static void SetEncryptionKey(string encryptKey)
        {
            try
            {
                while (encryptKey.Length < 8)
                {
                    encryptKey += Crypter.DEFAUT_KEY;
                }
                Crypter.SetKey(encryptKey);
            }
            catch (Exception ex)
            {
                Log.E("Program.SetEncryptionKey", ex.Message, ex.StackTrace);
                //throw;
            }
        }
        #endregion

        #region Display



        //Fonction qui affiche le message a la fin du traitement
        private static void DisplayFinish(TimeSpan time)
        {
            try
            {
                string str = "Process finished in " + time.ToString() + ". Press Enter to exit.";
                int bufWidth = Console.BufferWidth - 1;
                string sep = Crypter.StrDup(bufWidth, "=");
                Log.Display(sep, Log.ColorInfo);
                Log.Display("= " + str.PadRight(bufWidth - 4) + " =", Log.ColorInfo);
                Log.Display(sep, Log.ColorInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, ex.StackTrace);
                //throw;
            }
        }


        //Fonction qui affiche les informations du traitement en cours
        private static void DisplayInfo(string action, string sourcePath, string destPath)
        {
            try
            {
                string str = action + " start at " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                int bufWidth = Console.BufferWidth - 1;
                string sep = Crypter.StrDup(bufWidth, "=");
                Log.Display("= " + str.PadRight(bufWidth - 4) + " =", Log.ColorInfo);
                Log.Display("= " + ("Source: " + sourcePath).PadRight(bufWidth - 4) + " =", Log.ColorInfo);
                if (string.IsNullOrEmpty(destPath) == false)
                {
                    Log.Display("= " + ("Dest  : " + destPath).PadRight(bufWidth - 4) + " =", Log.ColorInfo);
                }
                Log.Display(sep, Log.ColorInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, ex.StackTrace);
                //throw;
            }
        }

        //Fonction qui affiche le header de la console
        private static void DisplayHeader()
        {
            try
            {
                int bufWidth = Console.BufferWidth - 1;
                string sep = Crypter.StrDup(bufWidth, "=");
                Log.Display(sep, Log.ColorInfo);
                Log.Display("= " + "zCrypt, secure your data quickly !!! www.zem.fr".PadRight(bufWidth - 4) + " =", Log.ColorInfo);
                Log.Display(sep, Log.ColorInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, ex.StackTrace);
                //throw;
            }
        }

        //Fonction qui affiche l'aide
        private static void DisplayHelp()
        {
            try
            {
                Log.Display("zCrypt allows you to encrypt files and directories into a crypted single file", Log.ColorInfo);
                Log.Display("It uses AES encryption to protect your files", Log.ColorInfo);
                Log.Display("", Log.ColorHelp);
                Log.Display("Syntax to crypt:", Log.ColorHelp);
                Log.Display("zCrypt.exe " + ACTION_CRYPT + " \"encryptionPassword\" \"sourceDirOrFile\" \"outputPath\"", Log.ColorHelp);
                Log.Display("", Log.ColorHelp);
                Log.Display("Syntax to decrypt:", Log.ColorHelp);
                Log.Display("zCrypt.exe " + ACTION_DECRYPT + " \"encryptionPassword\" \"cryptedFilePath\" \"outputPath\"", Log.ColorHelp);
                Log.Display("", Log.ColorHelp);
                Log.Display("Syntax to list directories and files into a crypted file:", Log.ColorHelp);
                Log.Display("zCrypt.exe " + ACTION_LIST + " \"encryptionPassword\" \"cryptedFilePath\" [\"outputFile\"]", Log.ColorHelp);
                Log.Display("", Log.ColorHelp);
                Log.Display("Syntax to list directories and files into all crypted files (*.zc and *.zc_0000):", Log.ColorHelp);
                Log.Display("zCrypt.exe " + ACTION_LISTALL + " \"encryptionPassword\" \"sourceDirOrFile\" [\"outputFile\"]", Log.ColorHelp);
                Log.Display("", Log.ColorHelp);
                Log.Display("Syntax to split a file into junkfiles:", Log.ColorHelp);
                Log.Display("zCrypt.exe " + ACTION_SPLIT + " sizeInMb \"sourceFile\" \"outputPath\"", Log.ColorHelp);
                Log.Display("", Log.ColorHelp);
                Log.Display("Syntax to assemble junkfiles into a single file:", Log.ColorHelp);
                Log.Display("zCrypt.exe " + ACTION_ASSEMBLE + " \"splitFile\" \"outputPath\"", Log.ColorHelp);
                Log.Display("", Log.ColorHelp);
                Log.Display("Parameters:", Log.ColorHelp);
                Log.Display("- sourceDirOrFile: source directory or file to encrypt or split", Log.ColorHelp);
                Log.Display("- outputPath: output directory to store generated files (crypted, decrypted, splitted or assemble)", Log.ColorHelp);
                Log.Display("- outputPath: output file to store command result", Log.ColorHelp);
                Log.Display("- cryptedFilePath: path to the file you want to decrypt", Log.ColorHelp);
                Log.Display("- encryptionPassword: password used to protect your files (at least 8 chars)", Log.ColorHelp);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, ex.StackTrace);
                //throw;
            }
        }
        #endregion
    }
}
