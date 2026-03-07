using System;

namespace TaTeTi_1._0
{
    public class BotVeryEasy : Bot
    {
        Random random = new Random();
        public override byte[] playing(bool player)
        {
            // returns a valid random location
            byte row, col;

            do
            {
                row = (byte)random.Next(0, 3);
                col = (byte)random.Next(0, 3);
            } while (GameState[row, col] != null);


            return new byte[] { row, col };
        }
    }
}   