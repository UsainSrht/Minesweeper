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
        public int width;
        public int height;
        public int mines;

        public int minesToSweep;
        public int blocksToClear;
        public Form2(int width, int height, int mines)
        {
            InitializeComponent();
            this.width = width;
            this.height = height;
            this.mines = mines;
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
            Application.OpenForms[0].Show();
        }

        private async void createBlocks()
        {
            Random random = new Random();
            List<int> mineIndexes = new List<int>();
            for (int minesToPlant = mines; minesToPlant != 0; minesToPlant--)
            {
                int mineIndex = random.Next(width * height);
                while (mineIndexes.Contains(mineIndex))
                {
                    mineIndex = random.Next(width * height);
                }
                mineIndexes.Add(mineIndex);
            }
            for (int i = 0; i < width*height; i++)
            {
                Button dynamicButton = new Button();
                dynamicButton.Name = "dynamicButton" + i;
                dynamicButton.Size = new Size(20, 20);
                int x = i % height * 20;
                int y = i / width * 20;
                dynamicButton.Location = new Point(x, y);
                dynamicButton.BackColor = Color.Green;
                dynamicButton.Click += new System.EventHandler(this.Button_Click);
                Block block = new Block();
                block.index = i;
                block.isMine = mineIndexes.Contains(i);
                dynamicButton.Tag = block;
                Controls.Add(dynamicButton);
                await Task.Delay(1);
            }
        }

        private void Button_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            Block block = (Block)button.Tag;
            int index = block.index;
            openBlock(index);
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
                    button.BackColor = Color.Red;
                    //game finished...
                }
                else
                {
                    button.BackColor = SystemColors.Control;
                    checkNeighbors(index);
                }
            }
            
        }

        public void checkNeighbors(int index)
        {
            Button button = (Button)Controls.Find("dynamicButton" + index, false)[0];
            Block block = (Block)button.Tag;
            int x = index % height;
            int y = index / width;
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
