using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace zCryptCore.Classes
{
    public class Splitter
    {
        private const int BUFFER_SIZE = 8 * 1024 * 1024;

        #region Split

        //Fonction qui split un fichier
        public static void Split(int splitSize, string sourceFile, string destPath)
        {
            try
            {
                if (File.Exists(sourceFile) == false)
                {
                    Log.Display("ERROR: " + sourceFile + " not found", Log.ColorError);
                    return;
                }
                if (Directory.Exists(destPath) == false)
                {
                    Directory.CreateDirectory(destPath);
                }

                Log.Display("File " + sourceFile + " will be splitted in "+ splitSize + "Mb junkfiles into "+ destPath, Log.ColorInfo);
                FileInfo finfo = new FileInfo(sourceFile);
                using (BinaryReader br = new BinaryReader(new FileStream(finfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    byte[] b = new byte[BUFFER_SIZE];
                    int n = br.Read(b, 0, BUFFER_SIZE);
                    int nbJunk = 0;
                    BinaryWriter bw = null;
                    bool bwNeedOpen = true;
                    int junkWriteBytes = 0;
                    int maxWriteBytesPerJunk = splitSize*1024*1024;
                    while (n > 0)
                    {
                        if (bwNeedOpen == true)
                        {
                            bwNeedOpen = false;
                            string junkPath = Path.Combine(destPath, finfo.Name + "_" + nbJunk.ToString("0000"));
                            nbJunk += 1;
                            if (bw != null)
                            {
                                bw.Flush();
                                bw.Dispose();
                            }
                            bw = new BinaryWriter(new FileStream(junkPath, FileMode.Create, FileAccess.Write, FileShare.None));
                            Log.Display("Junk " + junkPath + " created", Log.ColorFile);
                        }
                        if (n < BUFFER_SIZE)
                        {
                            Array.Resize(ref b, n);
                        }
                        bw.Write(b);
                        bw.Flush();

                        junkWriteBytes += n;
                        if (junkWriteBytes > maxWriteBytesPerJunk)
                        {
                            bwNeedOpen = true;
                            junkWriteBytes = 0;
                        }

                        Array.Resize(ref b, BUFFER_SIZE);
                        n = br.Read(b, 0, BUFFER_SIZE);
                    }
                    if (bw != null)
                    {
                        bw.Flush();
                        bw.Dispose();
                    }
                    Log.Display("File " + sourceFile + " splitted into " + nbJunk + " junkfile(s)", Log.ColorInfo);
                }
            }
            catch (Exception ex)
            {
                Log.E("Splitter.Split", ex.Message, ex.StackTrace);
                //throw;
            }
        }

        #endregion

        #region Assemble
        //Fonction qui assemble un fichier
        public static void Assemble(string sourceFile, string destPath)
        {
            try
            {
                if (File.Exists(sourceFile) == false)
                {
                    Log.Display("ERROR: " + sourceFile + " not found", Log.ColorError);
                    return;
                }
                if (Directory.Exists(destPath) == false)
                {
                    Directory.CreateDirectory(destPath);
                }

                FileInfo finfo = new FileInfo(sourceFile);
                string destFile = Path.Combine(destPath, finfo.Name.Substring(0, finfo.Name.Length - 5));
                Log.Display("File " + sourceFile + " will be assembled into " + destFile, Log.ColorInfo);

                using (BinaryWriter bw = new BinaryWriter(new FileStream(destFile, FileMode.Create, FileAccess.Write, FileShare.None)))
                {
                    string prefix = finfo.Name.Substring(0, finfo.Name.Length - 4);
                    FileInfo[] files = finfo.Directory.GetFiles(prefix + "*").OrderBy(x => x.FullName).ToArray();

                    foreach (FileInfo fi in files)
                    {
                        using (BinaryReader br = new BinaryReader(new FileStream(fi.FullName, FileMode.Open, FileAccess.Read, FileShare.Read)))
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
                            Log.Display("File " + fi.FullName + " assembled", Log.ColorFile);
                        }
                    }
                }
                Log.Display("File " + sourceFile + " assembled into " + destFile, Log.ColorInfo);
            }
            catch (Exception ex)
            {
                Log.E("Splitter.Split", ex.Message, ex.StackTrace);
                //throw;
            }
        }
        #endregion
    }
}
