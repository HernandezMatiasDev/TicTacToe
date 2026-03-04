using System;

namespace TaTeTi_1._0
{
    public class BotMedium : BotEasy
    {

        public override byte[] playing(bool player)
        {
            byte[,] possibleLine = new byte[8, 3];
            byte[,] enemyPossibleLine = new byte[8, 3];

            byte nullPlays;


            possibleLine = lineFuncion(player);
            enemyPossibleLine = lineFuncion(!player);

            possibleLine = sortList(possibleLine);
            enemyPossibleLine = sortList(enemyPossibleLine);




            if (enemyPossibleLine[0, 2] == 2 && possibleLine[0, 2] != 2)
            {
                byte enemyNullPlays;

                enemyNullPlays = countNullsPlasy(enemyPossibleLine);

                return auxilarPlaying(enemyPossibleLine, enemyNullPlays);
            }

            nullPlays = countNullsPlasy(possibleLine);


            if (nullPlays == 8)
            {
                Bot bot = new BotVeryEasy();
                bot.GameState = this.GameState;
                return bot.playing(player);
            }



            return auxilarPlaying(possibleLine,nullPlays);
        }
        
        // Auxiliary function to avoid repeating code
        public  byte[] auxilarPlaying(byte[,] possibleLine, byte nullPlays)
        {
            byte[,] cleanPossibleLine = new byte[8 - nullPlays, 3];
            cleanPossibleLine = clearList(possibleLine, nullPlays);


            byte?[,] plays = new byte?[9, 2];
            plays = bestPlays(cleanPossibleLine);


            byte countPlaysNull = countNulls(plays);


            byte[,] clearPlays = new byte[9 - countPlaysNull, 2];
            clearPlays = clearNulls(plays, countPlaysNull);

            Random random = new Random();
            byte randomBestPlay;
            randomBestPlay = (byte)random.Next(0, 9 - countPlaysNull);

            byte[] palyReturn = new byte[2];
            palyReturn[0] = clearPlays[randomBestPlay, 0];
            palyReturn[1] = clearPlays[randomBestPlay, 1];


            return palyReturn;
        }
    }
}