using System;
using Lomztein.Moduthulhu.Core.Module.Framework;
using Lomztein.Moduthulhu.Core.Clock;
using Lomztein.Moduthulhu.Core.Extensions;
using Lomztein.Moduthulhu.Core.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.Moduthulhu.Modules.Command;
using Lomztein.Moduthulhu.Cross;
using Lomztein.Moduthulhu.Core.Configuration;
using Discord.WebSocket;
using Lomztein.AdvDiscordCommands.Extensions;

namespace Moduthulhu.Module.Remindme
{
    public class RemindMeModule : ModuleBase
    {

        public RemindMeModule() {}

        public RemindMeModule(Dictionary<ulong, Reminder> reminders) 
        {
            this.reminders = reminders;
        }


        public override string Name => "Remind Me";

        public override string Description => "Remind me feature, that allows users to get reminded in x amount of time";

        public override string Author => "Frederik Rosenberg";

        public override bool Multiserver => true;
        private const string path = "Reminder";

        private const string emojiId = "\u23F0";

        private RemindMeCommand command;

        private Dictionary<ulong, Reminder> reminders;

        private void SaveData() 
        {
            DataSerialization.SerializeData(reminders, path);
        }
        
        public override void PreInitialize() 
        {
            reminders = DataSerialization.DeserializeData<Dictionary<ulong, Reminder>>(path);
            if (reminders == null) reminders = new Dictionary<ulong, Reminder>();
        }

        public override void Initialize()
        {
            command = new RemindMeCommand() {ParentModule = this };
            ParentContainer.GetModule<CommandRootModule>().AddCommands(command);
            this.GetClock().OnMinutePassed += Update;
            ParentShard.ReactionAdded += ReactionAdded;
            ParentShard.ReactionRemoved += ReactionRemoved;
        }

        private Task ReactionAdded(Cacheable<IUserMessage, ulong> messageCache, ISocketMessageChannel channel, SocketReaction reaction)
        {
            ulong messageId = reaction.MessageId;
            ulong userId = reaction.UserId;
            if (reaction.User.Value.IsBot) return Task.CompletedTask;
            if (reminders.TryGetValue(messageId, out var reminder)) 
            {
                if (!reminder.Users.Contains(userId)) 
                {
                    reminder.Users.Add(userId);
                    reminders[messageId] = reminder;
                    SaveData();
                }
            }
            return Task.CompletedTask;
        }

        private Task ReactionRemoved(Cacheable<IUserMessage, ulong> messageCache, ISocketMessageChannel channel, SocketReaction reaction)
        {
            ulong messageId = reaction.MessageId;
            ulong userId = reaction.UserId;
            if (reaction.User.Value.IsBot) return Task.CompletedTask;
            if (reminders.TryGetValue(messageId, out var reminder)) 
            {
                if (reminder.Users.Contains(userId)) 
                {
                    reminder.Users.Remove(userId);
                    reminders[messageId] = reminder;
                    SaveData();
                }
            }
            return Task.CompletedTask;
        }

        public async Task AddReminder(IUserMessage message, ulong userId, DateTime reminderTime) 
        {
            var reminder = new Reminder(message.GetGuild().Id, message.Channel.Id, message.Id, reminderTime);
            reminder.Users.Add(userId);
            reminders.Add(message.Id, reminder);
            SaveData();

            var emoji = new Emoji(emojiId);

            await message.AddReactionAsync(emoji);
        }
        
        private Task Update(DateTime now, DateTime last) 
        {

            List<ulong> removes = new List<ulong>();

            foreach (var reminder in reminders)
            {
                if (reminder.Value.Remind <= now) 
                {
                    Task.Run(() => SendRemindMessageAsync(reminder.Value));
                    removes.Add(reminder.Key);
                }
            }


            foreach (var remove in removes) 
            {
                reminders.Remove(remove);
            }

            if (removes.Any()) 
            {
                SaveData();
            }
            

            return Task.CompletedTask;
        }

        private async void SendRemindMessageAsync(Reminder reminder) 
        {
            var users = reminder.Users.Select(id => ParentShard.GetUser(reminder.Server, id));

            var channel = ParentShard.GetTextChannel(reminder.Server, reminder.Channel);

            //Can't send the message if the channel has been deleted
            if (channel == null) return;

            var stringBuilder = new StringBuilder();

            foreach (var user in users) 
            {
                stringBuilder.Append(user.Mention).Append(" ");
            }
            

            var builder = new EmbedBuilder();
            builder.WithDescription($"Reminder for [Message]({reminder.Link})");
            

            await channel.SendMessageAsync(stringBuilder.ToString(), embed: builder.Build());
        }

        public override void Shutdown()
        {
            this.GetClock().OnMinutePassed -= Update;
            ParentShard.ReactionAdded -= ReactionAdded;
            ParentShard.ReactionRemoved -= ReactionRemoved;
        }
    }
}
