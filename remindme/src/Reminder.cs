using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Moduthulhu.Module.Remindme
{
    public class Reminder
    {

        public Reminder(ulong server, ulong channel, ulong message, DateTime remind)
        {
            Server = server;
            Channel = channel;
            Message = message;
            Remind = remind;
            Users = new List<ulong>();

        }

        public ulong Server { get; }
        public ulong Channel { get; }
        public ulong Message { get; }

        public DateTime Remind {get;}

        public List<ulong> Users {get; set;}

        [JsonIgnore]
        public string Link => $"https://discordapp.com/channels/{Server}/{Channel}/{Message}";

        



        
    }
}