using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Jint.Ex.WinForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AsyncronousEngine.EmbedScriptAssembly = Assembly.GetExecutingAssembly();
            AsyncronousEngine.Engine.SetValue("setUserMessage", new Action<string>(__setUserMessage__));
        }

        delegate void valueDelegate(string value);

        private void __setUserMessage__(string s)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action<string>(__setUserMessage__), s);
                }
                else
                {
                    this.lbOut.Items.Add(s);
                    this.lbOut.SelectedIndex = this.lbOut.Items.Count - 1;
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                System.Diagnostics.Debugger.Break();
            }
        }

        private void butRunScrip1_Click(object sender, EventArgs e)
        {
            AsyncronousEngine.RequestExecution("Script1.js");
        }

        private void butRunScript2_Click(object sender, EventArgs e)
        {
            AsyncronousEngine.RequestExecution("Script2.js");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AsyncronousEngine.RequestExecution("Script3.js");
        }

        private void butClear_Click(object sender, EventArgs e)
        {
            this.lbOut.Items.Clear();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            butClear_Click(sender, e);
            AsyncronousEngine.ClearQueue();
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //AsyncronousEngine.Kill();
            AsyncronousEngine.Stop();
            this.Close();
        }
    }
}
