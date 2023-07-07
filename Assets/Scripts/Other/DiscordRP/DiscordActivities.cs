using System;
using Discord;

namespace DiscordRP
{
    public static class DiscordActivities
    {
        // TODO: set up new activities matching the current game, go to discord's docs to see what u can do
        // These are the ones used for hold space to play, they can serve as a basic example
        public static readonly Activity MainMenuActivity = new Activity
        {
            State = "In the Main Menu",
            Assets =
        {
            LargeImage = "smile"
        },
            Instance = false
        };

        public static Activity StartGameActivity(int levelLoaded)
        {
            return new Activity
            {
                State = "Exploring Level " + levelLoaded.ToString(),
                Timestamps =
            {
                Start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            },
                Assets =
            {
                LargeImage = "hs2p_caves"
            },
                Instance = false
            };
        }
    }
}
