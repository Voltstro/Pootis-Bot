using System.Threading.Tasks;
using Discord.WebSocket;
using Pootis_Bot.Core;

namespace Pootis_Bot.Services
{
    public class VoteGivewayService
    {
        enum TimeType { Hour, Days, Seconds }


        public async Task StartVote(SocketGuild guild, ISocketMessageChannel channel, string time, string title, string description, string yesEmoji, string noEmoji)
        {
            TimeType timeType;

            if (time.EndsWith("h") || time.EndsWith("hrs") || time.EndsWith("hours"))
                timeType = TimeType.Hour;
            else if (time.EndsWith("s") || time.EndsWith("sec") || time.EndsWith("secs") || time.EndsWith("seconds"))
                timeType = TimeType.Seconds;
            else if (time.EndsWith("d") || time.EndsWith("days"))
                timeType = TimeType.Days;
            else
            {
                await channel.SendMessageAsync("Invaild time format, the time must end with either `h` for hours, `s` for seconds and `d` for days!");
                return;
            }

            Global.WriteMessage("The server {} has started vote that lasting {} seconds");

        }
    }
}
