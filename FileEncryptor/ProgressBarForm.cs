using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileEncryptor
{
    public partial class ProgressBarForm : Form
    {
        MainForm mainForm;
        public ProgressBarForm(MainForm mainForm)
        {
            InitializeComponent();
            this.mainForm = mainForm;
        }
    }
}
