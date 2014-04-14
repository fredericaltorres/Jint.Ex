using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace Jint.Ex.WinForm
{
    public partial class Form1 : Form
    {
        private AsyncronousEngine _asyncronousEngine;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.InitializeAsyncronousEngine();
        }

        private void InitializeAsyncronousEngine()
        {
            _asyncronousEngine = new AsyncronousEngine();
            _asyncronousEngine.EmbedScriptAssemblies.Add(Assembly.GetExecutingAssembly());
            // Expose to the JavaScript world the function setUserMessage which add a a message to the UI
            _asyncronousEngine.Engine.SetValue("setUserMessage", new Action<string, bool>(__setUserMessage__));
        }

        private void __setUserMessage__(string s, bool replace)
        {
            try
            {
                // When the method is called by the JavaScript engine it will be called from different thread
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action<string, bool>(__setUserMessage__), s, replace);
                }
                else
                {
                    if(replace)
                        this.lbOut.Items.Clear();
                    this.lbOut.Items.Add(s);
                    this.lbOut.SelectedIndex = this.lbOut.Items.Count - 1;
                }
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                Debugger.Break();
            }
        }

        private void butSynchronousExecution_Click(object sender, EventArgs e)
        {
            _asyncronousEngine.RequestFileExecution("SynchronousExecution.js");
        }

        private void butASynchronousExecution_Click(object sender, EventArgs e)
        {
            _asyncronousEngine.RequestFileExecution("AsynchronousExecution.js");
        }

        private void butTimer_Click(object sender, EventArgs e)
        {
            _asyncronousEngine.RequestFileExecution("Timer.js");
        }

        private void butMultipleTimer_Click(object sender, EventArgs e)
        {
            _asyncronousEngine.RequestFileExecution("MultipleTimers.js");
        }

        private void butClearListBox_Click(object sender, EventArgs e)
        {
            this.lbOut.Items.Clear();
        }

        private void butClearEventQueue_Click(object sender, EventArgs e)
        {
            butClearListBox_Click(sender, e);
            _asyncronousEngine.RequestClearQueue();
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // How to correctly request stopping the AsyncronousEngine event loop
            _asyncronousEngine.Stop(() =>
            {
                Thread.Sleep(100);
                Application.DoEvents();
            });
        }
    }
}
