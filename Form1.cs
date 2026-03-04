using System;
using System.Drawing;
using System.Media;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TaTeTi_1._0
{
    public partial class Form1 : Form
    {
        private SoundPlayer soundPlayer;
        private Game game = new Game();
        private Bot bot = null;

        private Panel menuPanel;
        private Panel difficultyPanel;
        private Panel configPanel;
        private Panel gamePanel; 

        private Panel[,] panels; 
        private Label scoreLabel;

        private Panel victoryPanel;
        private Label victoryLabel;
        private bool isProcessingTurn = false;
        private bool gameOverPending = false;

        public Form1()
        {
            InitializeComponent();
            
            this.ClientSize = new Size(480, 480);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.CenterToScreen();
            this.BackgroundImage = Image.FromFile("Resources/ta te ti vacio.png");

            soundPlayer = new SoundPlayer("Resources/sound.wav");
            soundPlayer.PlayLooping();

            game.OnGameEnd += OnGameEnd;

            BuildInterface();
        }


        // UI construction
        private void BuildInterface()
        {
            menuPanel = CreateContainerPanel();
            difficultyPanel = CreateContainerPanel();
            configPanel = CreateContainerPanel();
            gamePanel = CreateContainerPanel();

            // Main Menu
            FlowLayoutPanel menuFlow = CreateFlowLayout();
            AddMenuButton("1 Player", (s, e) => SwitchPanel(difficultyPanel), menuFlow);
            AddMenuButton("2 Players", (s, e) => StartGame(null), menuFlow);
            AddMenuButton("Settings", (s, e) => SwitchPanel(configPanel), menuFlow);
            AddMenuButton("Exit", (s, e) => this.Close(), menuFlow);
            menuPanel.Controls.Add(menuFlow);

            // Difficulty Menu
            FlowLayoutPanel diffFlow = CreateFlowLayout();
            AddMenuButton("Very Easy", (s, e) => StartGame(new BotVeryEasy()), diffFlow);
            AddMenuButton("Easy", (s, e) => StartGame(new BotEasy()), diffFlow);
            AddMenuButton("Normal", (s, e) => StartGame(new BotMedium()), diffFlow);
            AddMenuButton("Hard - human", (s, e) => StartGame(new BotHard()), diffFlow);
            AddMenuButton("Hard - Default", (s, e) => StartGame(new BotHardDefault()), diffFlow);
            AddMenuButton("Expert", (s, e) => StartGame(new BotExpert()), diffFlow);
            AddMenuButton("Back", (s, e) => SwitchPanel(menuPanel), diffFlow);
            difficultyPanel.Controls.Add(diffFlow);

            // Settings Menu
            FlowLayoutPanel configFlow = CreateFlowLayout();
            CheckBox musicCheck = new CheckBox { Text = "Music (On/Off)", AutoSize = true, Checked = true, Font = new Font("Arial", 12), BackColor = Color.White };
            musicCheck.CheckedChanged += (s, e) => { if (musicCheck.Checked) soundPlayer.PlayLooping(); else soundPlayer.Stop(); };
            configFlow.Controls.Add(musicCheck);
            AddMenuButton("Back", (s, e) => SwitchPanel(menuPanel), configFlow);
            configPanel.Controls.Add(configFlow);

            BuildGameBoard();
            SwitchPanel(menuPanel);
        }

        private void BuildGameBoard()
        {
            Button backBtn = new Button { Text = "Back", Size = new Size(100, 50), Location = new Point(10, 10), BackColor = Color.White };
            backBtn.Click += (s, e) => SwitchPanel(menuPanel);
            gamePanel.Controls.Add(backBtn);

            scoreLabel = new Label { Text = "P1: 0   P2: 0", Location = new Point(340, 10), Size = new Size(200, 30), BackColor = Color.Transparent, Font = new Font("Arial", 14, FontStyle.Bold) };
            gamePanel.Controls.Add(scoreLabel);

            panels = new Panel[3, 3];
            byte panelSize = 135;
            byte margin = 25;

            for (byte row = 0; row < 3; row++)
            {
                for (byte col = 0; col < 3; col++)
                {
                    Panel p = new Panel { Size = new Size(panelSize, panelSize), BackColor = Color.Transparent };
                    p.Location = new Point((col * (panelSize + margin)) + 10, (row * (panelSize + margin)) + 10);

                    byte r = row, c = col;
                    p.Click += (s, e) => Panel_Click(r, c);

                    panels[row, col] = p;
                    gamePanel.Controls.Add(p);
                }
            }

            // Victory Screen overlay
            victoryPanel = new Panel
            {
                Size = new Size(300, 150),
                Location = new Point(90, 160),
                BackColor = Color.FromArgb(230, 0, 0, 0),
                Visible = false,
                Cursor = Cursors.Hand
            };

            victoryLabel = new Label
            {
                Text = "Round Over!\nClick to continue",
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 16, FontStyle.Bold)
            };
            victoryPanel.Controls.Add(victoryLabel);

            EventHandler onContinue = async (s, e) =>
            {
                victoryPanel.Visible = false;
                ClearVisualBoard();
                gameOverPending = false;
                isProcessingTurn = false;

                if (!game.PlayerInit && bot != null)
                {
                    isProcessingTurn = true;
                    await Task.Delay(400);
                    BotTurn();
                    isProcessingTurn = false;
                }
            };

            victoryPanel.Click += onContinue;
            victoryLabel.Click += onContinue;
            gamePanel.Controls.Add(victoryPanel);
        }

        // visual helpers
        private void SwitchPanel(Panel activePanel)
        {
            menuPanel.Visible = difficultyPanel.Visible = configPanel.Visible = gamePanel.Visible = false;
            activePanel.Visible = true;
        }

        private Panel CreateContainerPanel()
        {
            Panel p = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Visible = false };
            this.Controls.Add(p);
            return p;
        }

        private FlowLayoutPanel CreateFlowLayout()
        {
            return new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, AutoSize = true, Location = new Point(140, 25) };
        }

        private void AddMenuButton(string text, EventHandler onClick, FlowLayoutPanel container)
        {
            Button b = new Button { Text = text, Size = new Size(200, 50), Font = new Font("Arial", 14, FontStyle.Bold), BackColor = Color.White, FlatStyle = FlatStyle.Flat, Margin = new Padding(5) };
            b.Click += onClick;
            container.Controls.Add(b);
        }

        // game

        private void StartGame(Bot selectedBot)
        {
            bot = selectedBot;
            game.gameFullReset();
            UpdateScoreboard();
            ClearVisualBoard();
            gameOverPending = false;
            isProcessingTurn = false;
            victoryPanel.Visible = false;
            SwitchPanel(gamePanel);
        }

        private async void Panel_Click(byte row, byte col)
        {
            if (isProcessingTurn || gameOverPending) return; 

            if (game.GameState[row, col] != null) return;

            bool humanPiece = game.Player; 

            DrawPiece(row, col, humanPiece);

            game.newMove(row, col);

            if (gameOverPending) return;

            if (bot != null && !IsBoardEmpty(game.GameState))
            {
                isProcessingTurn = true; 
                
                await Task.Delay(600); 
                
                if (!gameOverPending) BotTurn();
                
                isProcessingTurn = false; 
            }
        }

        private void BotTurn()
        {
            bot.GameState = game.GameState;
            
            byte[] botMove = bot.playing(false); 

            if (botMove != null)
            {
                byte bRow = botMove[0];
                byte bCol = botMove[1];
                
                bool botPiece = game.Player; 

                DrawPiece(bRow, bCol, botPiece);
                
                game.newMove(bRow, bCol);
            }
        }

        private void DrawPiece(byte row, byte col, bool isPlayer1)
        {
            string piecePath = isPlayer1 ? "Resources/circle.png" : "Resources/cross.png";
            panels[row, col].BackgroundImage = Image.FromFile(piecePath);
            panels[row, col].BackgroundImageLayout = ImageLayout.Stretch;
        }

        private void OnGameEnd()
        {
            UpdateScoreboard();
            gameOverPending = true;
            victoryPanel.BringToFront(); 
            victoryPanel.Visible = true;
        }

        private void ClearVisualBoard()
        {
            foreach (var panel in panels)
            {
                panel.BackgroundImage = null;
            }
        }

        private void UpdateScoreboard()
        {
            scoreLabel.Text = $"P1: {game.Player1Score}   P2: {game.Player2Score}";
        }

        private bool IsBoardEmpty(bool?[,] board)
        {
            foreach (var cell in board) if (cell != null) return false;
            return true;
        }
    }
}