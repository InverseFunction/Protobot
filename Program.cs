using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFBot
{
    class Program
    {
        public static Log mLog;
        public static Bot Bot;
        public static String user, pass;
        static void Main(string[] args)
        {
            mLog = new Log();
            if (args.Length < 2)
            {
                mLog.error("PROTOBOT: Required Arguements: Username Password");
                return;
            }
            user = args[0];
            pass = args[1];

            Bot = new Bot(user, pass);

            if (!Bot.IsLoaded())
            {
                Bot.Start();
            }
            while (Bot.IsRunning())
            {
                Bot.Tick();
                if (Bot.SteamGuard())
                {
                    AuthBot();
                }
            }

        }
        static void AuthBot()
        {
            if (Bot.IsRunning())
            {
                Bot.bIsRunning = false;
            }
            if (Bot != null)
            {
                mLog.info("Test");
                Bot = null;
            }
            mLog.info("Please enter your SteamGuard Auth info!");
            String temp = Console.ReadLine();
            Bot = new Bot(user, pass, temp);
            Bot.Start();
            return;
        }
    }
}
