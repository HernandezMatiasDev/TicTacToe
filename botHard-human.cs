using System;

namespace TaTeTi_1._0
{
    public class BotHard : Bot
    {
        private Bot botExpert = new BotExpert();
        private Bot botMedium = new BotMedium();
        private Bot botEasy = new BotEasy();
        private Random rnd = new Random();

        public override byte[] playing(bool player)
        {
            // The idea behind this bot is that it plays well but can make mistakes
            // would be the bot most similar to a human
            // Modify the probabilities to adjust the bot
            // Statistics with these percentages (50, 40, 10)
            // bot BotHard (00,00%) vs BotExpert (38,10%)
            // bot BotHard (36,20%) vs BotMedium (20,20%)
            // bot BotHard (84,00%) vs BotEasy (9,70%)
            // It's much better than easy, a little better than normal, and much worse than expert.
            int probability = rnd.Next(0, 100);

            if (probability < 50)  // 50% botExpert
            {
                botExpert.GameState = this.GameState;
                return botExpert.playing(player);
            }
            else if (probability < 90) // 40 % //botMedium
            {
                botMedium.GameState = this.GameState;
                return botMedium.playing(player);
            }
            else // 10% // botEasy
            {
                botEasy.GameState = this.GameState;
                return botEasy.playing(player);
            }
        }
    }
}
