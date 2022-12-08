using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            this.Text = width + "x" + height + " (" + mines + " mines)";
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

        private async void createBlocks()
        {
            blocksToClear = width * height - mines;
            Random random = new Random();
            mineIndexes = new List<int>();
            for (int minesToPlant = mines; minesToPlant != 0; minesToPlant--)
            {
                int mineIndex = random.Next(width * height);
                while (mineIndexes.Contains(mineIndex))
                {
                    mineIndex = random.Next(width * height);
                }
                mineIndexes.Add(mineIndex);
            }

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
                block.isMine = mineIndexes.Contains(i);
                dynamicButton.Tag = block;
                Controls.Add(dynamicButton);
                await Task.Run(() => new System.Threading.ManualResetEvent(false).WaitOne(waitms));
            }
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
                    var result = MessageBox.Show("Better luck next time.", "You failed!", MessageBoxButtons.CancelTryContinue, MessageBoxIcon.Question);
                    if (result == DialogResult.Cancel)
                    {
                        revealAllMines();
					}
                    else if (result == DialogResult.TryAgain)
                    {
						Form newForm = new Form2(width, height, mines);
						newForm.Show();
                        appClosing = true;
                        this.Close();
					}
                }
                else
                {
                    button.BackColor = SystemColors.Control;
                    blocksToClear--;
                    if (blocksToClear == 0)
                    {
                        MessageBox.Show("successful!");
                        //game finished...
                    }
                    checkNeighbors(index);
                }
            }
            
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
