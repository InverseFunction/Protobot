using System;
using System.Threading.Tasks;
using System.Web;
using System.Net;
using System.Text;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.ComponentModel;
using SteamKit2;
using SteamTrade;
using SteamKit2.Internal;
using SteamKit2.GC;

namespace TFBot
{
    public enum Permissions
    {
        DEFAULT,
        ADMIN,
        OWNER
    }
    public class Bot
    {

        #region SteamStuff
        public SteamClient SteamClient;
        public SteamUser SteamUser;
        public SteamFriends SteamFriends;
        public SteamGameCoordinator SteamGameCoordinator;
        public String User,Pass;
        public SteamUser.LogOnDetails details;
        #endregion
        #region GlobalStuff
        public bool bIsRunning = false;
        public bool bLoaded = false;
        public bool bLoggedOn = false;
        public bool bSteamGuard = false;
        public String LoginKey = "";
        public String sessionId;
        public String token;
        public String DisplayName = "Protobot";
        public List<SteamID> Admins = new List<SteamID>();
        public SteamID Owner;
        public Dictionary<ulong, User> users = new Dictionary<ulong, User>();
        public int CurrentGame = 0;
        #endregion

        public Log log;

        public Bot(String user, String pass, String authcode="")
        {
            log = new Log();
            this.User = user;
            this.Pass = pass;

            if(!authcode.Equals(""))
            {
                details = new SteamUser.LogOnDetails
                {
                    Username = this.User,
                    Password = this.Pass,
                    AuthCode = authcode
                };
            }
            else
            {
                details = new SteamUser.LogOnDetails
                {
                    Username = this.User,
                    Password = this.Pass
                };

            }

            Admins.Add(new SteamID(55131436, EUniverse.Public, EAccountType.Individual));
            Admins.Add(new SteamID(49739418, EUniverse.Public, EAccountType.Individual));

            Owner = new SteamID(151771732, EUniverse.Public, EAccountType.Individual);

            SteamClient = new SteamClient();
            SteamUser = SteamClient.GetHandler<SteamUser>();
            SteamFriends = SteamClient.GetHandler<SteamFriends>();
            SteamGameCoordinator = SteamClient.GetHandler<SteamGameCoordinator>();
            
           
        }

        #region ConditionChecks
        public bool IsRunning()
        {
            return bIsRunning;
        }
        public bool IsLoggedOn()
        {
            return bLoggedOn;
        }
        public bool IsLoaded()
        {
            return bLoaded;
        }
        public bool SteamGuard()
        {
            return bSteamGuard;
        }
        #endregion
        #region BotControl
        public bool Start()
        {
            //Connect to Steam Servers
            log.info("Connecting to Steam...");
            SteamClient.Connect();

            //Set Running to true, load bot
            log.success("Done!...Loading bot");
            bLoaded = true;
            bIsRunning = true;

            return true;
        }
        public void Stop()
        {
            //Terminate Thread
            log.error("Shutting down bot..");
            SteamClient.Disconnect();
            bIsRunning = false;
            bLoaded = false;
        }
        #endregion
        public void Tick()
        {
            var callback = SteamClient.WaitForCallback(true);

            #region login
            callback.Handle<SteamClient.ConnectedCallback>(c => 
            {
                if ( c.Result != EResult.OK )
                {
                    log.error(String.Format("Error connecting to Steam: {0}", c.Result));
                    bIsRunning = false;
                    return;
                }
                log.success(String.Format("Connected to Steam! Logging in User:{0}", User));
                BotLogOn();
            });
            callback.Handle<SteamClient.DisconnectedCallback>(c =>
            {
                log.info("Disconnected from steam");
                bIsRunning = false;
            });
            callback.Handle<SteamUser.LoggedOnCallback>(c => 
            {
                if(c.Result != EResult.OK)
                {
                    if(c.Result == EResult.AccountLogonDenied)
                    {
                        bSteamGuard = true;
                        if(c.Result == EResult.InvalidLoginAuthCode)
                        {
                            log.error("Invalid SteamGuard Code");
                            return;
                        }
                        
                    }
                    log.error(String.Format("Unable to connect to Steam: {0} / {1}", c.Result, c.ExtendedResult));

                    if(c.Result == EResult.OK)
                    {
                        LoginKey = c.WebAPIUserNonce;
                        log.success(String.Format("Logged Onto Steam Network! {0}", LoginKey));
                    }
                    bIsRunning = false;
                    return;
                }
            });
            callback.Handle<SteamUser.LoginKeyCallback>(c =>
            {

                if(SteamUser.SteamID != Owner)
                {
                    SteamFriends.SetPersonaName(DisplayName);
                }
                else
                {
                    SteamFriends.SetPersonaName("TTHKProtocol");
                }
                SteamFriends.SetPersonaState(EPersonaState.Online);
                
                log.success(String.Format("Bot {0} Has been COMPLETELY logged in!", User));
                bLoggedOn = true;
            });
            callback.Handle<SteamUser.LoggedOffCallback>(c => 
            {
                log.info(String.Format("Logged off of Steam: {0}", c.Result));
                bLoggedOn = false;
            });
            callback.Handle<SteamUser.UpdateMachineAuthCallback>(
                jobCallback => OnUpdateMachineAuthCallback(jobCallback, jobCallback.JobID)
             );
            #endregion
            #region Friends
            callback.Handle<SteamFriends.FriendMsgCallback>(c => 
            {
                EChatEntryType type = c.EntryType;
                if(c.EntryType == EChatEntryType.ChatMsg)
                {
                    if (SteamUser.SteamID != Owner)
                    {
                        log.info(String.Format("Chat Message Recieved From {0}: {1}", SteamFriends.GetFriendPersonaName(c.Sender), c.Message));
                        GetUser(c.Sender).OnMessage(type, c.Message);
                    }
                    if(SteamUser.SteamID == Owner)
                    {
                        log.info(String.Format("Away Mode Message Recieved From {0}: {1}", SteamFriends.GetFriendPersonaName(c.Sender), c.Message));
                        AwayMode(c.Sender, type, c.Message);
                    }
                }
            });
            #endregion
        }
        void BotLogOn()
        {  
            Directory.CreateDirectory(System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "sentryfiles"));
            FileInfo fi = new FileInfo(System.IO.Path.Combine("sentryfiles",String.Format("{0}.sentryfile", details.Username)));

            if (fi.Exists && fi.Length > 0)
            {
                details.SentryFileHash = SHAHash(File.ReadAllBytes(fi.FullName));
            }
            else
            {
                details.SentryFileHash = null;
            }
            SteamUser.LogOn(details);
        }
        #region SteamAuthentication
        static byte[] SHAHash(byte[] input)
        {
            SHA1Managed sha = new SHA1Managed();

            byte[] output = sha.ComputeHash(input);

            sha.Clear();

            return output;
        }
        void OnUpdateMachineAuthCallback(SteamUser.UpdateMachineAuthCallback machineAuth, JobID jobId)
        {
            byte[] hash = SHAHash(machineAuth.Data);

            Directory.CreateDirectory(System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "sentryfiles"));

            File.WriteAllBytes(System.IO.Path.Combine("sentryfiles", String.Format("{0}.sentryfile", details.Username)), machineAuth.Data);

            var authResponse = new SteamUser.MachineAuthDetails
            {
                BytesWritten = machineAuth.BytesToWrite,
                FileName = machineAuth.FileName,
                FileSize = machineAuth.BytesToWrite,
                Offset = machineAuth.Offset,
                SentryFileHash = hash,
                OneTimePassword = machineAuth.OneTimePassword,

                LastError = 0,
                Result = EResult.OK,

                JobID = jobId,
            };

            SteamUser.SendMachineAuthResponse(authResponse);
        }
        #endregion
        #region BotEvents
        public void SetGamePlaying(int id)
        {
            var Game = new SteamKit2.ClientMsgProtobuf<CMsgClientGamesPlayed>(EMsg.ClientGamesPlayed);
            Game.Body.games_played.Add(new CMsgClientGamesPlayed.GamePlayed 
            {
                game_id = new GameID(id),
            });
            SteamClient.Send(Game);

            CurrentGame = id;
        }
        #endregion
        #region BotCommands
        public void ExecuteCommand(String command, SteamID sid)
        {
            if(command.Equals("Help"))
            {
                GetUser(sid).PrintCommands();
            }
            if(command.Equals("Friends"))
            {
                GetUser(sid).PrintFriends();
            }
            if(command.Equals("User"))
            {
                GetUser(sid).UserInfo();
            }
        }

        #endregion
        #region UserInfo
        User GetUser(SteamID sid)
        {
            if(!users.ContainsKey(sid.ConvertToUInt64()))
            {
                users[sid.ConvertToUInt64()] = new User(this, sid, GetPermissions(sid));
            }
            return users[sid.ConvertToUInt64()];
        }
        Permissions GetPermissions(SteamID sid)
        {
            if(Admins.Contains(sid))
            {
                return Permissions.ADMIN;
            }
            if(sid == Owner)
            {
                return Permissions.OWNER;
            }
            return Permissions.DEFAULT;
        }
        public void AwayMode(SteamID id, EChatEntryType type, String message)
        {
            if(id == Admins.Find(x => x.AccountID == 55131436))
            {
                SteamFriends.SendChatMessage(id, type, "Hey Honey..I'm not here right now, Leave a message if you'd like! I love reading them.");
            }
            else
            {
                SteamFriends.SendChatMessage(id, type, "PROTOBOT DEV RESPONSE");
            }
        }
        #endregion
    }
}