using System;
using System.Configuration;
using System.Linq;

namespace TaTeTi_1._0
{
        public class BotEasy : Bot
    {   
        protected const byte COL = 2, ROW = 1, DIAGONAL = 3, UPWARD = 1, DOWNWARD = 2;


        public override byte[] playing(bool player)
        {
            // creates the list that contain the possible lines
            byte[,] possibleLine = new byte[8, 3];
            byte nullPlays;

            //  saves possible lines
            possibleLine = lineFuncion(player);

            // Sort the list from highest to lowest according to the number of tokens
            possibleLine = sortList(possibleLine);

            //Calculate how many lines are invalid
            nullPlays = countNullsPlasy(possibleLine);

            // If there are 8 invalid lines, it means there are no lines left.
            // in that case, play randomly
            if (nullPlays == 8)
            {
                Bot bot = new BotVeryEasy();
                bot.GameState = this.GameState;
                return bot.playing(player);
            }

            // removed the invalid lines
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

            // return a random play
            Random random = new Random();
            byte randomBestPlay;
            randomBestPlay = (byte)random.Next(0, 9 - countPlaysNull);

            byte[] palyReturn = new byte[2];
            palyReturn[0] = clearPlays[randomBestPlay, 0];
            palyReturn[1] = clearPlays[randomBestPlay, 1];


            return palyReturn;
        }

        // returns how many invalid plays
        protected byte countNulls(byte?[,] plays)
        {
            byte count = 0;
            for (byte i = 0; i < plays.GetLength(0); i++)
            {
                if (plays[i, 0] == null || plays[i, 1] == null)
                {
                    count += 1;
                }
            }
            return count;
        }
        
        // Filter the plays by removing those that contain null values.
        protected byte[,] clearNulls(byte?[,] plays, byte count)
        {
            byte[,] clearPlays = new byte[9 - count, 2];
            byte AuxiliaryCount = 0;

            for (byte i = 0; i < plays.GetLength(0); i++)
            {
                if (plays[i, 0] != null && plays[i, 1] != null)
                {
                    clearPlays[AuxiliaryCount, 0] = (byte)(plays[i, 0]);
                    clearPlays[AuxiliaryCount, 1] = (byte)(plays[i, 1]);
                    AuxiliaryCount += 1;
                }
            }
            return clearPlays;
        }
        
        
        // This will only return plays that have the same value
        // Therefore, any of the plays it returns are equally good
        protected byte?[,] bestPlays(byte[,] lines)
        {      
            // Initialization of matrices and variables
            // listBestPlays saves the optimal plays    
            byte?[,] listBestPlays = new byte?[9,2];
            byte?[,] listCoordinatePositions = new byte?[3, 2];
            byte[] listAuxiliary = new byte[2];

            byte count = 0;
            byte TokenCount = 0;

            // Flags and counters to know how many lines have 2, 1 or 0 tiles
            bool twoToken = false;
            byte twoTokenCount = 0;
            bool oneToken = false;
            byte oneTokenCount = 0;
            bool zeroToken = false;
            byte zerTokenCount = 0;

            // traverse the lines and group by the number of tokens
            // líneas[i, 2] es la cantidad de token en esa línea
            for (byte i = 0; i < lines.GetLength(0); i++)
            {
                if (lines[i, 2] == 2)
                {
                    twoToken = true;
                    twoTokenCount += 1;
                }
                else if (lines[i, 2] == 1)
                {
                    oneToken = true;
                    oneTokenCount += 1;
                }
                else if (lines[i, 2] == 0)
                {
                    zeroToken = true;
                    zerTokenCount += 1;
                }
            }
            
            // Lines are prioritized according to the number of tokens: 2>1>0
            // TokenCount stores the number of lines that have the highest priority found.
            if (twoToken)
            {
                TokenCount = twoTokenCount;
            }
            else if (oneToken)
            {
                TokenCount = oneTokenCount;
            }
            else if (zeroToken)
            {
                TokenCount = zerTokenCount;
            }


            // Since the list is ordered by the number of tokens, we only go up to TokenCount
            for (byte i = 0; i < TokenCount; i++)
            {
                // extracts the line identifiers
                listAuxiliary[0] = lines[i, 0];
                listAuxiliary[1] = lines[i, 1];

                // The empty cells of that line are obtained
                listCoordinatePositions = cordenatePositions(listAuxiliary);

                for (byte x = 0; x < 3; x++)
                {
                    // verify that the returned coordinate is not null
                    // and that it has not been previously saved in listBestPlays
                    if (!savedMove(listBestPlays, listCoordinatePositions[x, 0], listCoordinatePositions[x, 1]))
                    {
                        listBestPlays[count, 0] = listCoordinatePositions[x, 0];
                        listBestPlays[count, 1] = listCoordinatePositions[x, 1];
                        count += 1;
                    }
                }

            }    

            // the best possible plays              
            return listBestPlays;
        }
        protected bool savedMove(byte?[,] matriz, byte? valor1, byte? valor2)
        {
            return Enumerable.Range(0, matriz.GetLength(0)).Any(i => matriz[i, 0] == valor1 && matriz[i, 1] == valor2);
        }


        // Searches for and returns the coordinates of empty cells in a specific line (row, column, or diagonal)
        protected byte?[,] cordenatePositions(byte[] line)
        {
            // This matrix stores the coordinates found.
            byte?[,] listCordenatePositions = new byte?[3, 2];
            byte count = 0;

            // if the line is a row
            if (line[1] == ROW)
            {
                for (byte i = 0; i < 3; i++)
                {
                    if (GameState[line[0], i] == null)
                    {
                        listCordenatePositions[count, 0] = line[0];
                        listCordenatePositions[count, 1] = i;
                        count += 1;
                    }
                }
            }
            // if the line is a COL
            else if (line[1] == COL)
            {
                for (byte i = 0; i < 3; i++)
                {
                    if (GameState[i, line[0]] == null)
                    {
                        listCordenatePositions[count, 0] = i;
                        listCordenatePositions[count, 1] = line[0];
                        count += 1;
                    }
                }
            }
            // if the line is a DIAGONAL
            else if (line[1] == DIAGONAL)
            {
                if (line[0] == DOWNWARD)
                {
                    for (byte i = 0; i < 3; i++)
                    {
                        if (GameState[i, i] == null)
                        {
                            listCordenatePositions[count, 0] = i;
                            listCordenatePositions[count, 1] = i;
                            count += 1;
                        }
                    }
                }
                else if (line[0] == UPWARD)
                {
                    for (byte i = 0, z = 2; i < 3; i++, z--)
                    {
                        if (GameState[i, z] == null)
                        {
                            listCordenatePositions[count, 0] = i;
                            listCordenatePositions[count, 1] = z;
                            count += 1;
                        }
                    }
                }
            }
            // Returns the coordinates of the empty spaces found on that line
            return listCordenatePositions;

        }
        
        
        // Sort the list from highest to lowest according to the number of tokens
        protected byte[,] sortList(byte[,] lines) 
        {
            bool ordinate = true;

            for(byte x = 0; x < lines.GetLength(0); x++)
            {
                ordinate = true;
                for(byte y = 0; y < lines.GetLength(0) - x - 1; y++)
                {
                    if (lines[y, 2] < lines[y + 1, 2])
                    {   
                        ordinate = false;
                        for (byte i = 0; i < 3; i++)
                        {
                            lines[y + 1, i] = (byte)(lines[y, i] + lines[y + 1, i]);
                            lines[y, i] = (byte)(lines[y + 1, i] - lines[y, i]);
                            lines[y + 1, i] = (byte)(lines[y + 1, i] - lines[y, i]);
                        }
                    }
                }
                if (ordinate)
                {
                    return lines;
                }
            }
            return lines;
        }

        //determines how many lines are not available
        protected byte countNullsPlasy(byte[,] lines)
        {
            byte count = 0;
            for (byte i = 0; i < lines.GetLength(0); i++)
            {
                if (lines[i, 1] == 0) //since place 1 can never be 0, if it is 0 it is because it is not a possible line
                {
                    count += 1;
                }
            }
            return count;
        }

        // Returns a new array with the first (8 - count) rows of the original.
        protected byte[,] clearList(byte[,] lines, byte count)
        {

            byte[,] newListClear = new byte[8 - count, 3];
            for (byte i = 0; i < newListClear.GetLength(0); i++)
            {
                for (byte x = 0; x < 3; x++)
                {
                    newListClear[i, x] = lines[i, x];
                }
            }
            return newListClear;
        }
        
        // This function checks that there are no enemy pieces in any of the 3 cells        
        // is only useful if you pass it a line
        protected bool checkline(bool? cell1, bool? cell2, bool? cell3, bool player)
        {
            if (cell1 != !player && cell2 != !player && cell3 != !player)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // This function checks if straight lines are available and which ones they are.
        //returns a list of possible lines
        // the list is from 8 , 3
        // in position 0 stores which row, column or diagonal it is
        // in position 1 it stores whether it is a row, column or diagonal
        // In position 2, store how many tokens we have in that row
        // Row, column, and diagonal values ​​are stored as constants   
        protected byte[,] lineFuncion(bool player) 

        {       
            byte[,] possibleLine = new byte[8,3];

            byte cont = 0;
            byte playerQuantity;

            for (byte x = 0; x < 3; x++)
            {
                // Check for possible vertical lines
                if (checkline(GameState[0, x], GameState[1, x], GameState[2, x], player))
                {
                    playerQuantity = 0;

                    // count how many pieces you have on that line
                    for (byte i = 0; i < 3; i++) 
                    {
                        if (GameState[i, x] == player)
                        {
                            playerQuantity += 1;
                        }
                    }

                    possibleLine[cont, 0] = x;
                    possibleLine[cont, 1] = COL;
                    possibleLine[cont, 2] = playerQuantity;
                    cont += 1;
                }
                //Check for possible horizontal lines
                if (checkline(GameState[x, 0], GameState[x, 1], GameState[x, 2], player)) 
                {
                    playerQuantity = 0;

                    for (byte i = 0; i < 3; i++)
                    {
                        if (GameState[x, i] == player)
                        {
                            playerQuantity += 1;
                        }
                    }

                    possibleLine[cont, 0] = x;
                    possibleLine[cont, 1] = ROW;
                    possibleLine[cont, 2] = playerQuantity;

                    cont += 1;
                }
            }
            //Check for possible diagonals
            if (checkline(GameState[0,0],GameState[1,1],GameState[2,2], player)) 
            {
                playerQuantity = 0;

                for (byte i=0; i<3; i++)
                    {
                        if (GameState[i,i] == player)
                        {
                            playerQuantity += 1;
                        }
                    }

                possibleLine[cont, 0] = DOWNWARD;
                possibleLine[cont, 1] = DIAGONAL;
                possibleLine[cont, 2] = playerQuantity;
                cont+= 1;
            }
            //Check for possible diagonals
            if (checkline(GameState[0,2],GameState[1,1],GameState[2,0], player))
            {
                playerQuantity = 0;

                for (byte i = 0, z = 2; i < 3 ; i++, z--)
                    {
                        if (GameState[i,z] == player)
                        {
                            playerQuantity += 1;
                        }
                    }

                possibleLine[cont, 0] = UPWARD;
                possibleLine[cont, 1] = DIAGONAL;
                possibleLine[cont, 2] = playerQuantity;
                cont+= 1;
            }
            
            return possibleLine;
        }
    }

}