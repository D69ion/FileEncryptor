using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IDEAEncryprion
{
    public partial class ProgressBarForm : Form
    {
        public ProgressBarForm(long max, long min, int step)
        {
            InitializeComponent();
            progressBar1.Maximum = (int)max;
            progressBar1.Minimum = (int)min;
            progressBar1.Step = step;
        }

        public void Step()
        {
            progressBar1.Value += progressBar1.Step;
        }
    }
}
