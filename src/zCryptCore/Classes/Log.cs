using System;
using System.Diagnostics;

namespace zCryptCore.Classes
{
    //Classe de gestion des logs
    public class Log
    {
        public static ConsoleColor ColorDir = ConsoleColor.Cyan;
        public static ConsoleColor ColorFile = ConsoleColor.DarkCyan;
        public static ConsoleColor ColorError = ConsoleColor.Red;
        public static ConsoleColor ColorInfo = ConsoleColor.White;
        public static ConsoleColor ColorHelp = ConsoleColor.DarkYellow;

        //Fonction d'output dans la console
        public static void Display(string msg, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(msg);
            Console.ForegroundColor = ConsoleColor.White;
        }
                
        //Fonction de log de Debug
        public static void D(string fonction, string msg)
        {
            Debug.WriteLine(msg);
        }

        //Fonction de log de Warning
        public static void W(string fonction, string msg)
        {
            Debug.WriteLine(msg);
        }
            
        //Fonction de log d'information
        public static void I(string fonction, string msg)
        {
            Debug.WriteLine(msg);
        }
            
        //Fonction de log d'erreur
        public static void E(string fonction, string msg, string stack)
        {
            Debug.WriteLine(msg);
            Debug.WriteLine(stack);
        }
    }
}
