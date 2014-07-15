using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFBot
{
    public class Log
    {
        ConsoleColor SuccessColor;
        ConsoleColor ErrorColor;
        ConsoleColor InfoColor;
        ConsoleColor DefaultColor;
        public Log()
        {
            Console.Title = "Protobot";
            SuccessColor = ConsoleColor.DarkGreen;
            ErrorColor = ConsoleColor.DarkRed;
            InfoColor = ConsoleColor.DarkYellow;
            DefaultColor = ConsoleColor.White;
        }

        public void info(String msg)
        {
            Console.ForegroundColor = InfoColor;
            Console.WriteLine(msg);
            Console.ForegroundColor = DefaultColor;
        }
        public void error(String msg)
        {
            Console.ForegroundColor = ErrorColor;
            Console.WriteLine(msg);
            Console.ForegroundColor = DefaultColor;
        }
        public void success(String msg)
        {
            Console.ForegroundColor = SuccessColor;
            Console.WriteLine(msg);
            Console.ForegroundColor = DefaultColor;
        }

    }
}
