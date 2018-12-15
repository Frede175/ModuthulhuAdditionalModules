using System;
using System.Threading.Tasks;
using Discord;
using Lomztein.AdvDiscordCommands.Extensions;
using Lomztein.AdvDiscordCommands.Framework;
using Lomztein.AdvDiscordCommands.Framework.Categories;
using Lomztein.Moduthulhu.Modules.Command;

namespace Moduthulhu.Module.Remindme
{
    public class RemindMeCommand : ModuleCommand<RemindMeModule>
    {
        public RemindMeCommand()
        {
            CommandEnabled = true;
            Name = "remindme";
            Description = "Remind me in x amount of time";
            Category = StandardCategories.Utility;
            AvailableOnServer = true;
        }

        [Overload (typeof (void), "Remind me")]
        public async Task<Result> Execute(CommandMetadata data, int x, string type)
        {
            if (x <= 0 || x >= 1000) 
            {
                return new Result(null, "Failed to create reminder: Amount needs to bigger than 0 and less than 1000");
            }


            DateTime reminderTime = DateTime.Now;

            switch (type)
            {
                case "min":
                case "mins":
                    reminderTime = reminderTime.AddMinutes(x);
                    break;
                case "hour":
                case "hours":
                    reminderTime = reminderTime.AddHours(x);
                    break;
                case "day":
                case "days":
                    reminderTime = reminderTime.AddDays(x);
                    break;
                case "month":
                case "months":
                    reminderTime = reminderTime.AddMonths(x);
                    break;
                case "year":
                case "years":
                    reminderTime = reminderTime.AddYears(x);
                    break;
                default:
                    return new Result(null, "Failed to create reminder: Unkown time type");
            }

            await ParentModule.AddReminder(data.Message as IUserMessage, data.AuthorID, reminderTime);
            return new Result(null, $"Reminder set {reminderTime.ToShortDateString()} {reminderTime.ToShortTimeString()}");
        }

    }
}