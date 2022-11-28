using System.Runtime.CompilerServices;

namespace Minesweeper
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 3)
            {
                panel1.Visible = true;
                button1.Enabled = false;
            }
            else
            {
                button1.Enabled = true;
                panel1.Visible = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int width;
            int height;
            int mines;
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    width = 8;
                    height = 8;
                    mines = 10;
                    break;
                case 1:
                    width = 16;
                    height = 16;
                    mines = 40;
                    break;
                case 2:
                    width = 16;
                    height = 30;
                    mines = 99;
                    break;
                default:
                    width = (int)numericUpDown1.Value;
                    height = (int)numericUpDown2.Value;
                    mines = (int)numericUpDown3.Value;
                    break;
            }
            Form theGame = new Form2(width, height, mines);
            this.Hide();
            theGame.ShowDialog();
            
        }

        private void numericUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDown1.Value > 0 && numericUpDown2.Value > 0 && numericUpDown3.Value > 0)
            {
                button1.Enabled = true;
            }
            else
            {
                button1.Enabled = false;
            }
        }
    }
}