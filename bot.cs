using System;


namespace TaTeTi_1._0
{
    public abstract class Bot
    {
        // Declare the array without initializing it
        protected bool?[,] game;
        // Pass the complete game state and not just the modification.
        public bool?[,] GameState
        {
            get 
            { 
                return game; 
            }
            set 
            { 
                if (value.GetLength(0) == 3 && value.GetLength(1) == 3) // Validar que la matriz sea 3x3
                {
                    game = value; 
                }
                else
                {
                    throw new ArgumentException("Error, la matriz debe ser de 3x3.");
                }
            } 
        }


        // Constructor that receives the Game instance
        public Bot()
        {
            game = new bool?[3, 3];
        }



        // Abstract method that must be implemented by child classes
        // This is going to be the main action
        public abstract byte[] playing(bool player);
    }
}
