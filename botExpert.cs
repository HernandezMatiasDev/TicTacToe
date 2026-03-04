using System;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

namespace TaTeTi_1._0
{
    public class BotExpert : BotEasy
    {

        Random rnd = new Random();

        // dictionaries to normalize the board
        // to deduce the possibilities
        private static readonly Dictionary<(byte, byte), byte> CornerRotations = new()
        {
            { (0, 0), 0 },
            { (2, 0), 1 },
            { (2, 2), 2 },
            { (0, 2), 3 }
        };
        private static readonly Dictionary<(byte, byte), byte> EdgeRotations = new()
        {
            { (0, 1), 0 },
            { (1, 0), 1 },
            { (2, 1), 2 },
            { (1, 2), 3 }
        };
        
        public override byte[] playing(bool player)
        {

            // The code is divided into two
            // The bot starts and the enemy starts

            // Count how many tokens each player has.
            var (playerCount, enemyCount) = CurrentTurn(player);

            if (playerCount == enemyCount) // Since they are the same, it means the bot starts.
            {
                switch (playerCount)
                {
                    case 0: // turn 1
                    {
                        return TurnOneAttacker(player);
                    }
                    case 1: // turn 2
                    {
                        return TurnTwoAttacker(player);
                    }
                    default: // rest of turns
                    {
                        return defaultTurn(player);
                    }
                }
                
            }
            else // the enemy begins
            {
                switch (playerCount)
                {
                    case 0: // turn 1
                    {
                        return TurnOneDefense(player);
                    }
                    default: // rest of turns
                    {
                        return defaultTurn(player);
                    }
                }
            }
        }


        // Count how many tokens each player has.
        private (byte, byte) CurrentTurn(bool player)
        {
            byte playerCount = 0;
            byte enemyCount = 0;
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    if (GameState[x, y] == player)
                    {
                        playerCount += 1;
                    }
                    else if (GameState[x, y] == !player)
                    {
                        enemyCount += 1;
                    }
                }
            }
            // It returns 2 bytes, first the bot and then the enemy
            return (playerCount, enemyCount);
        }


        // At this point the board is definitely empty.
        // Choose whether to start in a corner, in the center, or on an edge.
        // Prioritize the corner, as it is the strongest point
        private byte[] TurnOneAttacker(bool player)
        {
            int probability = rnd.Next(0, 100);

            if (probability < 60) // 60%  //corner 
            {
                int index = rnd.Next(CornerRotations.Count);
                var randomCorner = CornerRotations.Keys.ElementAt(index);
                return new byte[] { randomCorner.Item1, randomCorner.Item2 };
            }
            else if (probability < 80) // 20 % // center
            {
                return new byte[] { 1, 1 };
            }
            else // 20% // edge
            {
                int index = rnd.Next(EdgeRotations.Count);
                var randomCorner = EdgeRotations.Keys.ElementAt(index);
                return new byte[] { randomCorner.Item1, randomCorner.Item2 };
            }
        }


        private byte[] TurnTwoAttacker(bool player)
        {
            // I encapsulated it because it's only useful when there is at most one token for each player.

            (byte x, byte y) tokenPosition(bool token) // pass it the type of token you want to search for
            {
                for (byte i = 0; i < 3; i++)
                {
                    for (byte z = 0; z < 3; z++)
                    {
                        if (GameState[i, z] == token)
                        {
                            return (i, z);
                        }
                    }
                }
                return (0, 0); // This is just to get C# to compile
            }

            // At this point, there is definitely one token for each player.
            // saves the position of each token
            var (playerX, playerY) = tokenPosition(player);
            var (enemyX, enemyY) = tokenPosition(!player);


            byte relativePlayX = 0; // We set it to 0 so that C# compiles
            byte relativePlayY = 0;
            byte n = 0;

            // divide according to corner, edge or center
            if (CornerRotations.ContainsKey((playerX, playerY))) // corner
            {
                // If the enemy is also in a corner, we return any other corner.
                if (CornerRotations.ContainsKey((enemyX, enemyY)))
                {
                    List<(byte, byte)> availableCorners = new() { (0, 0), (2, 0), (2, 2), (0, 2) };

                    availableCorners.Remove((playerX, playerY));
                    availableCorners.Remove((enemyX, enemyY));

                    int index = rnd.Next(availableCorners.Count);
                    return new byte[] { availableCorners[index].Item1, availableCorners[index].Item2 };
                }

                // we normalize the coordinates
                n = CornerRotations[(playerX, playerY)];

                var (relativePlayerX, relativeplayerY) = RotationCoordinates(playerX, playerY, n);
                var (relativeEnemyX, relativeEnemyY) = RotationCoordinates(enemyX, enemyY, n);
                
                // From now on we will only use relative positions
                // We take into account that the relative token is always at 00


                // If the enemy is in the center, the corner opposite the starting one
                if ((relativeEnemyX, relativeEnemyY) == (1, 1))
                {
                    relativePlayX = 2;
                    relativePlayY = 2;

                }
                // If the enemy is below us, right corner
                else if ((relativeEnemyX, relativeEnemyY) == (1, 0))
                {
                    relativePlayX = 0;
                    relativePlayY = 2;
                }
                // If the enemy is next to us, the corner below ours.
                else if ((relativeEnemyX, relativeEnemyY) == (0, 1))
                {
                    relativePlayX = 2;
                    relativePlayY = 0;
                }
                // At this point, only the two edges furthest from 00 remain
                // any corner except the opposite one
                else
                {
                    (relativePlayX, relativePlayY) = rnd.Next(2) == 0 ? ((byte)0, (byte)2) : ((byte)2, (byte)0);
                }

            }
            else if (EdgeRotations.ContainsKey((playerX, playerY)))// edge
            {
                // we normalize the coordinates
                n = EdgeRotations[(playerX, playerY)];
                var (relativePlayerX, relativeplayerY) = RotationCoordinates(playerX, playerY, n);
                var (relativeEnemyX, relativeEnemyY) = RotationCoordinates(enemyX, enemyY, n);

                // From now on we will only use relative positions
                // We take into account that the relative token is always at 01

                // If the enemy has the center
                // We place a side edge
                // Only possible victory if they place it in the opposite corner // for (1,1)

                // Highly probable to win (2,1) (chaotic game)
                if ((relativeEnemyX, relativeEnemyY) == (1, 1) || (relativeEnemyX, relativeEnemyY) == (2, 1))
                {
                    (relativePlayX, relativePlayY) = rnd.Next(2) == 0 ? ((byte)1, (byte)0) : ((byte)1, (byte)2);
                }
                // You can only win if the opponent doesn't put it in the center
                else if ((relativeEnemyX, relativeEnemyY) == (0, 0))
                {
                    (relativePlayX, relativePlayY) = (1, 0);
                }
                // You can only win if the opponent doesn't put it in the center
                else if ((relativeEnemyX, relativeEnemyY) == (0, 2))
                {
                    (relativePlayX, relativePlayY) = (1, 2);
                }
                // From now on, victories are guaranteed
                // Place your piece on top, otherwise you'll surely lose
                else if ((relativeEnemyX, relativeEnemyY) == (2, 0))
                {
                    (relativePlayX, relativePlayY) = (0, 0);

                }
                // Place your piece on top, otherwise you'll surely lose
                else if ((relativeEnemyX, relativeEnemyY) == (2, 2))
                {
                    (relativePlayX, relativePlayY) = (0, 2);

                }
                else if ((relativeEnemyX, relativeEnemyY) == (1, 0))
                {
                    (relativePlayX, relativePlayY) = rnd.Next(2) == 0 ? ((byte)1, (byte)1) : ((byte)0, (byte)0);


                }
                else if ((relativeEnemyX, relativeEnemyY) == (1, 2))
                {
                    (relativePlayX, relativePlayY) = rnd.Next(2) == 0 ? ((byte)1, (byte)1) : ((byte)0, (byte)2);
                }
            }
            else // center
            {
                // If the enemy is on an edge, we win with any corner
                if (EdgeRotations.ContainsKey((enemyX, enemyY)))
                {
                    int index = rnd.Next(CornerRotations.Count);
                    var randomCorner = CornerRotations.Keys.ElementAt(index);
                    return new byte[] { randomCorner.Item1, randomCorner.Item2 };
                }
               // If we don't enter the if statement, it means the enemy put it in a corner

                n = CornerRotations[(enemyX, enemyY)];
                // Calculate n to place in the corner opposite the enemy
                (relativePlayX, relativePlayY) = (2, 2);
            }
            byte PlayX;
            byte PlayY;

            (PlayX, PlayY) = RotationCoordinates(relativePlayX, relativePlayY, (byte)(4 - n));

            // We return the best move
            return new byte[] { PlayX, PlayY };
        }


        private (byte x, byte y) RotationCoordinates(byte x, byte y, byte n)
        {

            for (int _ = 0; _ < n % 4; _++)
            {
                (x, y) = (y, (byte)(2 - x));
            }
            return (x, y);
        }

       
        protected byte[] defaultTurn(bool player)
        {
            // creates the lists that contain the possible lines
            byte[,] possibleLine = new byte[8, 3];
            byte[,] enemyPossibleLine = new byte[8, 3];

            
            // variables to use later
            byte randomBestPlay;
            byte[] palyReturn = new byte[2];

            // saves the possible lines of both the bot and the enemy
            possibleLine = lineFuncion(player);
            enemyPossibleLine = lineFuncion(!player);

            // Sort the lists from highest to lowest according to the number of tokens
            possibleLine = sortList(possibleLine);
            enemyPossibleLine = sortList(enemyPossibleLine);

            // calculate how many of the lines are invalid
            byte nullPlays;
            byte enemyNullPlays;

            nullPlays = countNullsPlasy(possibleLine);
            enemyNullPlays = countNullsPlasy(enemyPossibleLine);

            if (possibleLine[0, 2] == 2) // if we have a row with 2 tokens we win
            {
                return _defaultTurn(possibleLine, nullPlays);
            }
            else if (enemyPossibleLine[0, 2] == 2) // If the enemy has a row with 2 tokens, we prevent them from winning
            {
                return _defaultTurn(enemyPossibleLine, enemyNullPlays);
            }
            // If the previous "if" statements were not entered, it's because nobody is there to win.

            if (nullPlays == 8) // If nullPlays is 8, it means there are no possible lines.
            {
                // In that case, play at covering enemy lines.

                if (enemyNullPlays == 8) // If there are no enemy lines, random location
                {
                    Bot bot = new BotVeryEasy();
                    bot.GameState = this.GameState;
                    return bot.playing(player);
                }
                return _defaultTurn(enemyPossibleLine, enemyNullPlays);
            }

            // While it doesn't make sense for possibleLine[0, 2] to be 0 in this bot
            // Even though it's slightly less efficient, 
            // I'm leaving this if statement in so I can use this function outside of this bot in other contexts
            if (possibleLine[0, 2] == 0)
            {
                return _defaultTurn(possibleLine, nullPlays);
            }
            
            // Since I don't enter _defaultTurn
            // we removed the invalid lines
            byte[,] cleanPossibleLine = new byte[8 - nullPlays, 3];
            cleanPossibleLine = clearList(possibleLine, nullPlays);

            // attempts a double attack
            byte[,] doublePlays = DoubleAttack(cleanPossibleLine, cleanPossibleLine);

            // If the list is of length 0, it is because there are no double attacks.
            // If they exist, we play the double attack
            if (doublePlays.GetLength(0) != 0)
            {

                randomBestPlay = (byte)rnd.Next(0, doublePlays.GetLength(0));

                palyReturn[0] = doublePlays[randomBestPlay, 0];
                palyReturn[1] = doublePlays[randomBestPlay, 1];
                return palyReturn;
            }
            // since none of the previous if statements entered
            // will try to make a straight line, so that it benefits the enemy as little as possible

            
            
            // We calculate the best plays
            // This will only return plays that have the same value
            // Therefore, any of the plays it returns are equally good
            byte?[,] plays = new byte?[9, 2];
            plays = bestPlays(cleanPossibleLine);

            // We clean the list so that only the plays remain
            byte countPlaysNull = countNulls(plays);

            byte[,] clearPlays = new byte[9 - countPlaysNull, 2];
            clearPlays = clearNulls(plays, countPlaysNull);


            // We remove the enemy's invalid lines
            byte[,] EnemyCleanPossibleLine = new byte[8 - enemyNullPlays, 3];
            EnemyCleanPossibleLine = clearList(enemyPossibleLine, enemyNullPlays);

            // returns all plays that benefit the enemy except the enemy               
            byte[,] bestOptimalPlays = optimalPlay(clearPlays, player, EnemyCleanPossibleLine);

            // Choose a random move from among the best moves
            randomBestPlay = (byte)rnd.Next(0, bestOptimalPlays.GetLength(0));


            palyReturn[0] = bestOptimalPlays[randomBestPlay, 0];
            palyReturn[1] = bestOptimalPlays[randomBestPlay, 1];
            return palyReturn;
        }

        // Auxiliary function to avoid repeating code
        private byte[] _defaultTurn(byte[,] possibleLine, byte nullPlays)
        {
            //removed the invalid lines
            byte[,] cleanPossibleLine = new byte[8 - nullPlays, 3];
            cleanPossibleLine = clearList(possibleLine, nullPlays);

            // calculate the best plays
            // This will only return plays that have the same value
            // Therefore, any of the plays it returns are equally good
            byte?[,] plays = new byte?[9, 2];
            plays = bestPlays(cleanPossibleLine);


            // clean the list so that only the plays remain
            byte countPlaysNull = countNulls(plays);

            byte[,] clearPlays = new byte[9 - countPlaysNull, 2];
            clearPlays = clearNulls(plays, countPlaysNull);

            // We return a random play
            byte randomBestPlay;
            randomBestPlay = (byte)rnd.Next(0, 9 - countPlaysNull);

            byte[] palyReturn = new byte[2];
            palyReturn[0] = clearPlays[randomBestPlay, 0];
            palyReturn[1] = clearPlays[randomBestPlay, 1];


            return palyReturn;
        }


        // It returns a list of the best plays
        private byte[,] optimalPlay(byte[,] plays, bool player, byte[,] enemyPossibleLine)
        {
            byte[] playsValue = new byte[plays.GetLength(0)];

            // At this point, we definitely have a piece on the board.
            // It is absolutely possible to create a line "if (nullPlays == 8)"
            // There is no way to place another piece in line without generating a "possibleLine[0, 2] == 0" attack
            
            // The bot needs to know if placing that piece causes the enemy to defend themselves.
            // earn a useful token
            // We want to place our token in the place that least benefits defending against the enemy

            // because the enemy has to defend itself no matter what, and because of how everything is programmed so far
            // I have no way of knowing which place completes my line
            // And we need to know what that place is to see how much it benefits the enemy to put it there.
            // to find out which square completes the line
            // I pretend to make the move
            // Like a normal bot, it will defend itself against an attack no matter what
            // The place where I put the normal bot will be the place that completes my line
            // Therefore, by knowing the location, I can see how much it benefits my enemy

            for (int i = 0; i < plays.GetLength(0); i++)
            {
                bool?[,] AuxiliarGame = (bool?[,])game.Clone();
                AuxiliarGame[plays[i, 0], plays[i, 1]] = player;
                Bot mediumBot = new BotMedium();
                mediumBot.GameState = AuxiliarGame;
                byte[] playBot = mediumBot.playing(!player);
                // Currently, playBot is the token we need to win.
                // Now he notices when it benefits the enemy to place a token there

                byte countEnemyLines = 0;// The higher this variable, the more help it helps.
                for (int z = 0; z < enemyPossibleLine.GetLength(0); z++)
                {
                    
                    if (enemyPossibleLine[z, 2] == 1) // If it's a line with 0 tokens, we're not interested.
                    {
                        if (enemyPossibleLine[z, 1] == COL)
                        {
                            if (playBot[1] == enemyPossibleLine[z, 0])
                            {
                                countEnemyLines += 1;
                            }
                        }
                        else if (enemyPossibleLine[z, 1] == ROW)
                        {
                            if (playBot[0] == enemyPossibleLine[z, 0])
                            {
                                countEnemyLines += 1;
                            }
                        }
                        else
                        {
                            if (enemyPossibleLine[z, 0] == UPWARD)
                            {
                                if (playBot[0] + playBot[1] == 2)
                                {
                                    countEnemyLines += 1;
                                }
                            }
                            else
                            {
                                if (playBot[0] == playBot[1])
                                {
                                    countEnemyLines += 1;
                                }
                            }
                        }
                    }

                }
                // Now that we know how much it helps, we store the value in a list
                playsValue[i] = countEnemyLines;
            }

           // Now that we have the list with the value of each play, we order it.
            (plays, playsValue) = sortDubleList(plays, playsValue);

            // We only return the plays that benefit the enemy the least.
            byte auxilarCount = 0;
            while (auxilarCount < playsValue.Length && playsValue[0] == playsValue[auxilarCount])
            {
                auxilarCount++;
            }
            byte[,] bestOptimalPlays = new byte[auxilarCount, 2];

            for(int i = 0; i< auxilarCount;i++)
            {
                bestOptimalPlays[i, 0] = plays[i, 0];
                bestOptimalPlays[i, 1] = plays[i, 1];
            }
            return bestOptimalPlays;
        }
        
        //Order lists 1 and 2 based on list 2 from smallest to largest
        protected (byte[,], byte[]) sortDubleList(byte[,] plays,byte[] playsValue) 
        {
            bool ordinate = true;

            for(byte x = 0; x < playsValue.GetLength(0); x++)
            {
                ordinate = true;
                for(byte y = 0; y < playsValue.GetLength(0) - x - 1; y++)
                {
                    if (playsValue[y] > playsValue[y + 1])
                    {
                        ordinate = false;
                        playsValue[y] = (byte)(playsValue[y + 1] + playsValue[y]);
                        playsValue[y + 1] = (byte)(playsValue[y] - playsValue[y + 1]);
                        playsValue[y] = (byte)(playsValue[y] - playsValue[y + 1]);
                        for (byte i = 0; i < 2; i++)
                        {
                            plays[y + 1, i] = (byte)(plays[y, i] + plays[y + 1, i]);
                            plays[y, i] = (byte)(plays[y + 1, i] - plays[y, i]);
                            plays[y + 1, i] = (byte)(plays[y + 1, i] - plays[y, i]);
                        }
                    }
                }
                if (ordinate)
                {
                    return (plays, playsValue);
                }
            }
            return (plays, playsValue);
        }



        // This function checks if there are points where lists a and b converge.
        // If you want to compare within the same list, run that list twice.
        private byte[,] DoubleAttack(byte[,] possibleLineA, byte[,] possibleLineB)
        {

            // To compare, first check if it's the same location in memory; if not, check if they are exactly the same.           
            // I do that because I'm not 100% sure how C# handles parameters
            bool equalArray = Object.ReferenceEquals(possibleLineA, possibleLineB) || possibleLineA.Cast<byte>().SequenceEqual(possibleLineB.Cast<byte>());

            byte[,] palys = new byte[8, 2]; // It's bigger than necessary
            byte count = 0;

            // iterate the list a
            for (int i = 0; i < possibleLineA.GetLength(0); i++)
            {
                if (possibleLineA[i, 2] == 1)  // Compare with 1 because 2 is not possible and 0 is not of interest 
                {

                    // compares if sets a and b are equal; if so, z is equal to i + 1 (compares within the same list), otherwise it is 0 (compares between two lists)
                    for (int z = (equalArray) ? i + 1 : 0; z < possibleLineB.GetLength(0); z++)
                    {
                        if (possibleLineB[z, 2] == 1) // Compare with 1 because 2 is not possible and 0 is not of interest 
                        {
                            if (possibleLineB[z, 1] != possibleLineA[i, 1])
                            {
                                var play = (0, 0);

                                if (possibleLineB[z, 1] is COL or ROW && possibleLineA[i, 1] is COL or ROW)
                                {
                                    play = (possibleLineB[z, 1] == ROW) ? (possibleLineB[z, 0], possibleLineA[i, 0]) : (possibleLineA[i, 0], possibleLineB[z, 0]);
                                }
                                else if ((possibleLineB[z, 1] == DIAGONAL && possibleLineB[z, 0] == DOWNWARD) || (possibleLineA[i, 1] == DIAGONAL && possibleLineA[i, 0] == DOWNWARD)) //si es diagonal decendente 
                                {
                                    if (possibleLineB[z, 1] == DIAGONAL && possibleLineA[i, 1] == DIAGONAL) // if both are diagonals
                                    {
                                        play = (1, 1);
                                    }
                                    else // then it's a descending diagonal, whether column or row it's the same
                                    {
                                        play = (possibleLineB[z, 1] == DIAGONAL) ? (possibleLineA[i, 0], possibleLineA[i, 0]) : (possibleLineB[z, 0], possibleLineB[z, 0]);
                                    }
                                }
                                else // if it is diagonal and ascending
                                {
                                    // if either one is a row
                                    if ((possibleLineB[z, 1] == ROW) || (possibleLineA[i, 1] == ROW))
                                    {
                                        play = (possibleLineB[z, 1] == ROW) ? (possibleLineB[z, 0], 2 - possibleLineB[z, 0]) : (possibleLineA[i, 0], 2 - possibleLineA[i, 0]);
                                    }
                                    //One of them is definitely a column and the other is an ascending diagonal.                                   
                                    else
                                    {
                                        play = (possibleLineB[z, 1] == COL) ? (2 - possibleLineB[z, 0], possibleLineB[z, 0]) : (2 - possibleLineA[i, 0], possibleLineA[i, 0]);

                                    }
                                }
                                if (GameState[play.Item1, play.Item2] == null) // We make sure the place is free
                                {
                                    palys[count, 0] = (byte)play.Item1;
                                    palys[count, 1] = (byte)play.Item2;
                                    count++;
                                }

                            }
                        }
                    }
                }
            }

            byte[,] cleanPlays = new byte[count, 2];
            for (int j = 0; j < count; j++)
            {
                cleanPlays[j, 0] = palys[j, 0];
                cleanPlays[j, 1] = palys[j, 1];
            }
            return cleanPlays; // We return the plays without the 00s, which are noise
        }
        

        private byte[] TurnOneDefense(bool player)
        {
            // I encapsulated it because it's only useful when there's at most one token per player.
            // It's the same as in TurnTwoAttacker
            (byte x, byte y) tokenPosition(bool token) // pasarle el tipo de ficha que quieras buscar
            {
                for (byte i = 0; i < 3; i++)
                {
                    for (byte z = 0; z < 3; z++)
                    {
                        if (GameState[i, z] == token)
                        {
                            return (i, z);
                        }
                    }
                }
                return (0, 0); // This is just to get C# to compile
            }

            // At this point, there should only be one enemy token.
            var (enemyX, enemyY) = tokenPosition(!player);

            if ((enemyX, enemyY) == (1, 1))// if the enemy token is in the center
            {
                // place in any corner
                int index = rnd.Next(CornerRotations.Count);
                var randomCorner = CornerRotations.Keys.ElementAt(index);
                return new byte[] { randomCorner.Item1, randomCorner.Item2 };
            }
            else if (CornerRotations.ContainsKey((enemyX, enemyY))) // if the enemy token is in the corner
            {
                return new byte[] { 1, 1 }; // place in the center
            }
            else // if the enemy token is in the edge
            {
                // we calculate the relative position
                byte n = EdgeRotations[(enemyX, enemyY)];
                var (relativeEnemyX, relativeEnemyY) = RotationCoordinates(enemyX, enemyY, n);
                int index = rnd.Next(3);

                switch (index)
                {
                    case 0:
                        {
                            return new byte[] { 1, 1 }; // place in the center
                        }
                    case 1:
                        {
                            var (PlayX, playY) = RotationCoordinates(0, 0,(byte)(4 - n)); // place on the left side
                            return new byte[] { PlayX, playY };
                        }
                    case 2:
                        {
                            var (PlayX, playY) = RotationCoordinates(0, 2,(byte)(4 - n)); // place on the right side

                            return new byte[] { PlayX, playY };
                        }
                }

            }
            return  new byte[] {0, 0}; // This is just so C# compiles

        }

    }
}