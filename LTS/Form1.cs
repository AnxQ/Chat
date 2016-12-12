using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LTS
{
    public partial class Form1 : Form
    {
        //用户名将会从指定文件读取
        public string name = "Lan";

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string time = DateTime.Now.ToString();
            if (textBox1.Text == "")
            {
                ;
            }
            else if (textBox1.Text == "/cls")
                textBox2.Text = "";
            else
                textBox2.Text = textBox2.Text+time+"\r\n"+name+">>>"+textBox1.Text + "\r\n";
            textBox2.Focus();//将光标聚焦到textbox2
            textBox2.Select(textBox2.TextLength, 0);//光标定位到文本最后
            textBox2.ScrollToCaret();//滚动到光标处
            textBox1.Select(textBox1.TextLength, 0);
            textBox1.Focus();
            textBox1.Text = null;

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            FormBorderStyle = FormBorderStyle.FixedSingle;
            StartPosition = FormStartPosition.Manual;
            Location = new Point(0, 0);
            AcceptButton = button1;
            textBox2.ReadOnly = true;
            textBox2.ScrollBars = ScrollBars.Vertical;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            
        }
    }
}


//未完成的问题：
//textbox2的文字可滚动性   have finished
//第一次输入所造成的空行   have finished
//滚动后停在最底层         have finished
