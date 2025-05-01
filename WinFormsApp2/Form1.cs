using System;
using System.Drawing;
using System.Windows.Forms;
using System.Timers;

namespace WinFormsApp2
{
    public partial class Form1 : Form
    {
        private Random random = new Random();
        private int score = 0;
        private int timeLeft = 60;
        private System.Timers.Timer gameTimer;
        private Point targetPosition;
        private int targetSize = 30;
        private bool isGameRunning = false;
        private DifficultyLevel currentDifficulty = DifficultyLevel.Normal;
        private TargetShape currentShape = TargetShape.Circle;
        private int gameTime = 60;

        private enum DifficultyLevel
        {
            Easy,
            Normal,
            Hard
        }

        private enum TargetShape
        {
            Circle,
            Square,
            Triangle
        }

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.BackColor = Color.Black;
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Cursor = Cursors.Cross;

            gameTimer = new System.Timers.Timer(1000);
            gameTimer.Elapsed += GameTimer_Elapsed;

            // Menü oluştur
            CreateMenu();
        }

        private void CreateMenu()
        {
            MenuStrip menuStrip = new MenuStrip();
            ToolStripMenuItem difficultyMenu = new ToolStripMenuItem("Zorluk");
            ToolStripMenuItem shapeMenu = new ToolStripMenuItem("Hedef Şekli");
            ToolStripMenuItem timeMenu = new ToolStripMenuItem("Süre");
            ToolStripMenuItem exitMenu = new ToolStripMenuItem("Çıkış");

            // Zorluk seviyeleri
            difficultyMenu.DropDownItems.Add("Kolay", null, (s, e) => SetDifficulty(DifficultyLevel.Easy));
            difficultyMenu.DropDownItems.Add("Normal", null, (s, e) => SetDifficulty(DifficultyLevel.Normal));
            difficultyMenu.DropDownItems.Add("Zor", null, (s, e) => SetDifficulty(DifficultyLevel.Hard));

            // Hedef şekilleri
            shapeMenu.DropDownItems.Add("Daire", null, (s, e) => SetShape(TargetShape.Circle));
            shapeMenu.DropDownItems.Add("Kare", null, (s, e) => SetShape(TargetShape.Square));
            shapeMenu.DropDownItems.Add("Üçgen", null, (s, e) => SetShape(TargetShape.Triangle));

            // Süre seçenekleri
            timeMenu.DropDownItems.Add("30 Saniye", null, (s, e) => SetTime(30));
            timeMenu.DropDownItems.Add("60 Saniye", null, (s, e) => SetTime(60));
            timeMenu.DropDownItems.Add("90 Saniye", null, (s, e) => SetTime(90));

            // Çıkış menüsü
            exitMenu.Click += (s, e) => Application.Exit();

            menuStrip.Items.Add(difficultyMenu);
            menuStrip.Items.Add(shapeMenu);
            menuStrip.Items.Add(timeMenu);
            menuStrip.Items.Add(exitMenu);

            this.Controls.Add(menuStrip);
            this.MainMenuStrip = menuStrip;
        }

        private void SetDifficulty(DifficultyLevel level)
        {
            currentDifficulty = level;
            switch (level)
            {
                case DifficultyLevel.Easy:
                    targetSize = 40;
                    break;
                case DifficultyLevel.Normal:
                    targetSize = 30;
                    break;
                case DifficultyLevel.Hard:
                    targetSize = 20;
                    break;
            }
            ResetGame();
        }

        private void SetShape(TargetShape shape)
        {
            currentShape = shape;
            ResetGame();
        }

        private void SetTime(int seconds)
        {
            gameTime = seconds;
            timeLeft = seconds;
            ResetGame();
        }

        private void ResetGame()
        {
            score = 0;
            timeLeft = gameTime;
            isGameRunning = false;
            gameTimer.Stop();
            this.Invalidate();
        }

        private void GameTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (timeLeft > 0)
            {
                timeLeft--;
                this.Invoke((MethodInvoker)delegate
                {
                    this.Invalidate();
                });
            }
            else
            {
                gameTimer.Stop();
                isGameRunning = false;
                this.Invoke((MethodInvoker)delegate
                {
                    MessageBox.Show($"Oyun Bitti! Skorunuz: {score}", "Aim Training");
                    ResetGame();
                });
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            // Menü çubuğunun yüksekliğini al
            int menuHeight = this.MainMenuStrip?.Height ?? 0;

            // Skor, süre ve zorluk gösterimi
            string scoreText = $"Skor: {score}";
            string timeText = $"Süre: {timeLeft}";
            string difficultyText = $"Zorluk: {currentDifficulty}";
            using (Font font = new Font("Arial", 20))
            {
                g.DrawString(scoreText, font, Brushes.White, 10, menuHeight + 10);
                g.DrawString(timeText, font, Brushes.White, this.Width - 150, menuHeight + 10);
                g.DrawString(difficultyText, font, Brushes.White, this.Width / 2 - 100, menuHeight + 10);
            }

            // Hedef çizimi
            if (isGameRunning)
            {
                switch (currentShape)
                {
                    case TargetShape.Circle:
                        g.FillEllipse(Brushes.Red, targetPosition.X - targetSize / 2,
                            targetPosition.Y - targetSize / 2, targetSize, targetSize);
                        break;
                    case TargetShape.Square:
                        g.FillRectangle(Brushes.Red, targetPosition.X - targetSize / 2,
                            targetPosition.Y - targetSize / 2, targetSize, targetSize);
                        break;
                    case TargetShape.Triangle:
                        Point[] points = new Point[]
                        {
                            new Point(targetPosition.X, targetPosition.Y - targetSize / 2),
                            new Point(targetPosition.X - targetSize / 2, targetPosition.Y + targetSize / 2),
                            new Point(targetPosition.X + targetSize / 2, targetPosition.Y + targetSize / 2)
                        };
                        g.FillPolygon(Brushes.Red, points);
                        break;
                }
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (!isGameRunning)
            {
                isGameRunning = true;
                gameTimer.Start();
                GenerateNewTarget();
            }
            else
            {
                double distance = Math.Sqrt(Math.Pow(e.X - targetPosition.X, 2) +
                    Math.Pow(e.Y - targetPosition.Y, 2));

                if (distance <= targetSize / 2)
                {
                    score++;
                    GenerateNewTarget();
                }
            }
            this.Invalidate();
        }

        private void GenerateNewTarget()
        {
            int margin = 50;
            targetPosition = new Point(
                random.Next(margin, this.Width - margin),
                random.Next(margin, this.Height - margin)
            );
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Escape)
            {
                Application.Exit();
            }
        }
    }
}
