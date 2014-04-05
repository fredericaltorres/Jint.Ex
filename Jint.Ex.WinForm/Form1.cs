using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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
            AsyncronousEngine.EmbedScriptAssemblies.Add(Assembly.GetExecutingAssembly());
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

        private void butSynchronousExecution_Click(object sender, EventArgs e)
        {
            AsyncronousEngine.RequestScriptFileExecution("Script1.js");
        }

        private void butASynchronousExecution_Click(object sender, EventArgs e)
        {
            AsyncronousEngine.RequestScriptFileExecution("Script2.js");
        }

        private void butTimer_Click(object sender, EventArgs e)
        {
            AsyncronousEngine.RequestScriptFileExecution("Script3.js");
        }

        private void butClearListBox_Click(object sender, EventArgs e)
        {
            this.lbOut.Items.Clear();
        }

        private void butClearEventQueue_Click(object sender, EventArgs e)
        {
            butClearListBox_Click(sender, e);
            AsyncronousEngine.RequestClearQueue();
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            AsyncronousEngine.Stop(() =>
            {
                Thread.Sleep(100);
                Application.DoEvents();
            });
        }

        private void butMultipleTimer_Click(object sender, EventArgs e)
        {
            AsyncronousEngine.RequestScriptFileExecution("Script.MultipleTimers.js");
        }
    }
}
