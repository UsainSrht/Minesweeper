using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Minesweeper
{
    public partial class Form2 : Form
    {
        public int buttonWidth = 25;
        public int buttonHeight = 25;

        public Color colorUnOpened = Color.Green;
        public Color colorOpened = SystemColors.Control;
        public Color colorFlagged = Color.Orange;
        public Color colorMine = Color.Red;

        public int width;
        public int height;
        public int mines;

        public int blocksToClear;

        public List<int> mineIndexes;

        public bool appClosing = false;
        public bool areMinesPlanted = false;

        public string title = "({0}x{1}) {2} mines {3}";

        public DateTime timerStartDate;

		public Form2(int width, int height, int mines)
        {
            InitializeComponent();
            this.width = width;
            this.height = height;
            this.mines = mines;
            this.Size = new Size(((width * buttonWidth) + 16), ((height * buttonHeight) + 40));
        }


        private void Form2_Load(object sender, EventArgs e)
        {
            this.Text = String.Format(title, width, height, mines, "00:00");
        }

        private void Form2_Shown(object sender, EventArgs e)
        {
            createBlocks();
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && !appClosing)
            {
                Application.OpenForms[0].Show();
            }
            
        }

        public void startTimer()
        {
            timer1.Start();
            timerStartDate = DateTime.Now;
        }

		private void timer1_Tick(object sender, EventArgs e)
		{
			TimeSpan diff = DateTime.Now.Subtract(timerStartDate);
            string countdown = diff.ToString("mm':'ss");
			Text = String.Format(title, width, height, mines, countdown);
		}

        private async void createBlocks()
        {
            blocksToClear = width * height - mines;
            

            int waitms = 1000 / (width * height);

            for (int i = 0; i < width*height; i++)
            {
                Button dynamicButton = new Button();
                dynamicButton.Name = "dynamicButton" + i;
                dynamicButton.Size = new Size(buttonWidth, buttonHeight);
                int x = ((i % width) * buttonWidth);
                int y = ((i / width) * buttonHeight);
				dynamicButton.Location = new Point(x, y);
                dynamicButton.BackColor = colorUnOpened;
                dynamicButton.MouseDown += new MouseEventHandler(this.Button_MouseClick);
                Block block = new Block();
                block.index = i;
                block.isMine = false;
                dynamicButton.Tag = block;
                Controls.Add(dynamicButton);
                await Task.Run(() => new System.Threading.ManualResetEvent(false).WaitOne(waitms));
            }
        }

        public List<int> generateMines()
        {
			Random random = new Random();
			List<int> generatedMines = new List<int>();
			for (int minesToPlant = mines; minesToPlant != 0; minesToPlant--)
			{
				int mineIndex = random.Next(width * height);
				while (generatedMines.Contains(mineIndex))
				{
					mineIndex = random.Next(width * height);
				}
				generatedMines.Add(mineIndex);
			}
            return generatedMines;
		}

        public void generateMines(int indexToAvoid)
        {
			List<int> clickEnvironment = new List<int>();
			clickEnvironment.Add(indexToAvoid);
			clickEnvironment.Add(indexToAvoid + 1); //right
			clickEnvironment.Add(indexToAvoid - 1); //left
			clickEnvironment.Add(indexToAvoid - width - 1); //upleft
			clickEnvironment.Add(indexToAvoid - width); //up
			clickEnvironment.Add(indexToAvoid - width + 1); //upright
			clickEnvironment.Add(indexToAvoid + width - 1); //downleft
			clickEnvironment.Add(indexToAvoid + width); //down
			clickEnvironment.Add(indexToAvoid + width + 1); //downright

            List<int> generatedMines;
			generatedMines = generateMines();

			while (generatedMines.Intersect(clickEnvironment).Count() > 0)
            {
                generatedMines = generateMines();
			}
            mineIndexes = generatedMines;
            for (int index = 0; index < mineIndexes.Count; index++)
            {
                int buttonIndex = mineIndexes[index];
				Button button = (Button)Controls.Find("dynamicButton" + buttonIndex, false)[0];
                Block block = (Block) button.Tag;
                block.isMine = true;
			}
            areMinesPlanted = true;
		}

        private void Button_MouseClick(object sender, MouseEventArgs e)
        {

			Button button = (Button)sender;
			Block block = (Block)button.Tag;

            if (e.Button == MouseButtons.Right)
            {
                if (block.isTagged)
                {
                    button.BackColor = colorUnOpened;
                    block.isTagged = false;
                }
                else
                {
                    button.BackColor = colorFlagged;
                    block.isTagged = true;
                }
                
            }
            else if (!block.isTagged)
            {
				
				int index = block.index;
				if (!areMinesPlanted)
				{
					generateMines(index);
                    startTimer();
				}

				openBlock(index);
            }
            
        }

        public void openBlock(int index)
        {
            Button button = (Button) Controls.Find("dynamicButton" + index, false)[0];
            if (button.Enabled)
            {
                button.Enabled = false;
                Block block = (Block)button.Tag;
                bool isMine = block.isMine;
                if (isMine)
                {
                    button.BackColor = colorMine;
					timer1.Stop();
					var result = MessageBox.Show("Better luck next time.", "You failed!", MessageBoxButtons.CancelTryContinue, MessageBoxIcon.Question);
                    if (result == DialogResult.Cancel)
                    {
                        revealAllMines();
                    }
                    else if (result == DialogResult.TryAgain)
                    {
                        newGame();

					}
                    else if (result == DialogResult.Continue)
                    {
						timer1.Start();
					}
                }
                else
                {
                    button.BackColor = SystemColors.Control;
                    blocksToClear--;
                    if (blocksToClear == 0)
                    {
						timer1.Start();
						var result = MessageBox.Show("You successfully finished the game.\n\nWould you like to start a new game?", "Congratulations!", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
						if (result == DialogResult.Yes)
                        {
							newGame();
						}
					}
                    checkNeighbors(index);
                }
            }
            
        }

        public void newGame()
        {
			Form newForm = new Form2(width, height, mines);
			newForm.Show();
			appClosing = true;
			this.Close();
		}

        public void revealAllMines()
        {
            mineIndexes.ForEach(index =>
            {
				Button button = (Button)Controls.Find("dynamicButton" + index, false)[0];
                button.BackColor = colorMine;
			});
        }

        public void checkNeighbors(int index)
        {
            Button button = (Button)Controls.Find("dynamicButton" + index, false)[0];
            Block block = (Block)button.Tag;
            int x = index % width;
            int y = (index / width);
            List<int> neighbors = new List<int>();
            if (y > 0)
            {
                if (x > 0)
                {
                    neighbors.Add((y-1) * width + (x-1));
                }
                neighbors.Add((y-1) * width + x);
                if (x < width-1)
                {
                    neighbors.Add((y-1) * width + (x+1));
                }
            }

            if (x > 0)
            {
                neighbors.Add(y * width + (x - 1));
            }

            if (x < width - 1)
            {
                neighbors.Add(y * width + (x + 1));
            }

            if (y < height - 1)
            {
                if (x > 0)
                {
                    neighbors.Add((y + 1) * width + (x - 1));
                }
                neighbors.Add((y + 1) * width + x);
                if (x < width - 1)
                {
                    neighbors.Add((y + 1) * width + (x + 1));
                }
            }

            int neighborMineCount = 0;

            for (int i = 0; i < neighbors.Count; i++)
            {
                int neighbor = neighbors[i];
                Button neighborButton = (Button)Controls.Find("dynamicButton" + neighbor, false)[0];
                Block neighborBlock = (Block)neighborButton.Tag;
				if (neighborBlock.isMine)
                {
                    neighborMineCount++;
                }
                
            }

            if (neighborMineCount != 0)
            {
                button.Text = neighborMineCount.ToString();
            }
            else 
            {
                for (int i = 0; i < neighbors.Count; i++)
                {
                    openBlock(neighbors[i]);
                }
            }
            
        }
	}
}
