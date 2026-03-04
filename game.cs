using System;

namespace TaTeTi_1._0
{
    public class Game
    {   
        private bool?[,] game;

        private byte player1Score;
        private byte player2Score;

        private bool playerInit;
        private bool player;
        private int DrawCont;
        private bool isDraw;
        
        public event Action OnGameEnd;


        public bool?[,] GameState
        {
            get 
            { 
                return game; 
            }
        }

        public bool Player
        {
            get 
            { 
                return player; 
            }
        }

        public bool PlayerInit
        {
            get
            {
                return playerInit;
            }
        }

        public byte Player1Score
        {
            get
            {
                return player1Score;
            }
        }

        public byte Player2Score
        {
            get
            {
                return player2Score;
            }
        }


        public Game()
        {
            game = new bool?[3, 3];
            player = true;
            playerInit = true;
            player1Score = 0;
            player2Score = 0;
            DrawCont = 0;
            isDraw = false;
        }

        private bool availableSlot(byte row, byte col)
        {
            if (game[row, col] != null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        private bool checkline(bool? cell1, bool? cell2, bool? cell3)
        {
            if (cell1 != null && cell1 == cell2 && cell2 == cell3)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool isWin()
        {
            for(byte x=0; x<3; x++)
            {
                if (checkline(game[0,x],game[1,x],game[2,x]) || checkline(game[x,0],game[x,1],game[x,2]))
                {
                    return true;
                }
            }
            if (checkline(game[0,0],game[1,1],game[2,2]) || checkline(game[0,2],game[1,1],game[2,0]))
            {
                return true;
            }
            
            
            return false; 
        }


        public bool newMove(byte row, byte col) 
        {   
            if (DrawCont == 8)
            {
                isDraw = true;
            }
            DrawCont = DrawCont + 1;
            if (availableSlot(row,col))
            {
                game[row, col] = player;
                
                if (isWin())
                {
                    winer(player);
                    return false;
                }
                if (isDraw)
                {
                    draw();
                    return false;
                }
                
                player =  !player;
                return true;
            }
            return false;
        }
        private void gameReset() 
        {
            game = new bool?[3, 3];
        }

        public void gameFullReset() 
        {
            game = new bool?[3, 3];
            player = true;
            playerInit = true;
            player1Score = 0;
            player2Score = 0;
        }
    
        private void winer(bool player)
        {
            if (player)
            {
                player1Score += 1;
            }
            else
            {
                player2Score +=1;
            }

           
            playerInit = !playerInit;
            this.player = playerInit;
            DrawCont = 0;
            isDraw = false;
            gameReset();
            OnGameEnd?.Invoke();
        }

        private void draw()
        {
            playerInit = !playerInit;
            player = playerInit;
            DrawCont = 0;
            isDraw = false;
            gameReset();
            OnGameEnd?.Invoke();
        }

    }
}
