using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamKit2;

namespace TFBot
{
    public class User
    {
        public String[] Commands = new String[] 
        {
            "Help",
            "Friends",
            "User"
        };
        public String[] AdminCommands = new String[]
        {
            "Play"
        };
        public SteamID sid;
        public Permissions perms;
        public Bot Bot;

        public User(Bot bot, SteamID sid, Permissions perm)
        {
            this.sid = sid;
            this.perms = perm;
            this.Bot = bot;
        }

        public void OnMessage(EChatEntryType type, String message)
        {
            Console.Beep();
            for(int i=0;i<Commands.Length;i++)
            {
                if (message.Equals(Commands[i], StringComparison.OrdinalIgnoreCase))
                {
                    Bot.ExecuteCommand(Commands[i], this.sid);
                    return;
                }
            }
            for (int i = 0; i < AdminCommands.Length;i++ )
            {
                if (message.Equals(AdminCommands[i], StringComparison.OrdinalIgnoreCase) && i <= AdminCommands.Length)
                {
                    if (perms == Permissions.ADMIN || perms == Permissions.OWNER)
                    {
                        Bot.ExecuteCommand(AdminCommands[i], this.sid);
                        return;
                    }
                    else
                    {
                        Bot.SteamFriends.SendChatMessage(this.sid, type, "Insufficient Permissions!");
                        return;
                    }
                }
            }
            Bot.SteamFriends.SendChatMessage(this.sid, type, String.Format("Invalid Command: '{0}'. Type 'help' to see a list of Commands!", message));

        }
        #region Commands
        public void PrintCommands()
        {
            Bot.SteamFriends.SendChatMessage(this.sid, EChatEntryType.ChatMsg, "Commands:");
            for(int i=0;i<Commands.Length;i++)
            {
                Bot.SteamFriends.SendChatMessage(this.sid, EChatEntryType.ChatMsg, Commands[i]);
            }
            for(int i=0;i<AdminCommands.Length;i++)if(perms == Permissions.ADMIN || perms == Permissions.OWNER)
            {
                Bot.SteamFriends.SendChatMessage(this.sid, EChatEntryType.ChatMsg, AdminCommands[i]);
            }
        }
        public void PrintFriends()
        {
            Bot.SteamFriends.SendChatMessage(this.sid, EChatEntryType.ChatMsg, "Friends:");
            {
                for(int i=0;i<Bot.SteamFriends.GetFriendCount();i++)
                {
                    Bot.SteamFriends.SendChatMessage(this.sid, EChatEntryType.ChatMsg, String.Format("{0}", (Bot.SteamFriends.GetFriendPersonaName(Bot.SteamFriends.GetFriendByIndex(i)))));
                }
            }
        }
        public void UserInfo()
        {
            Bot.SteamFriends.SendChatMessage(this.sid, EChatEntryType.ChatMsg, String.Format("**********User Information: {0}*********", Bot.SteamFriends.GetFriendPersonaName(sid)));
            Bot.SteamFriends.SendChatMessage(this.sid, EChatEntryType.ChatMsg, String.Format("SteamID: {0}", sid.ToString()));
            Bot.SteamFriends.SendChatMessage(this.sid, EChatEntryType.ChatMsg, String.Format("Permissions: {0}", perms));
            Bot.SteamFriends.SendChatMessage(this.sid, EChatEntryType.ChatMsg, String.Format("PersonaState: {0}", Bot.SteamFriends.GetFriendPersonaState(sid)));
            Bot.SteamFriends.SendChatMessage(this.sid, EChatEntryType.ChatMsg, String.Format("Game: {0}", Bot.SteamFriends.GetFriendGamePlayedName(sid)));

        }
        #endregion Commands
    }

}
