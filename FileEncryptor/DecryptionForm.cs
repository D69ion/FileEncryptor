using System;
using System.IO;
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
    public partial class DecryptionForm : Form
    {
        MainForm mainForm;
        public DecryptionForm(MainForm form)
        {
            InitializeComponent();
            textBoxKeyFile.ReadOnly = true;
            this.mainForm = form;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (textBoxKeyFile.Text.Length > 0)
            {
                mainForm.KeyFilePath = textBoxKeyFile.Text;
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("No encryption key file selected.");
                return;

            }
            this.DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            mainForm.KeyFilePath = "";
            this.DialogResult = DialogResult.Cancel;
        }

        private void buttonSelectFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Key files(*.key)|*.key";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                FileInfo info = new FileInfo(fileDialog.FileName);
                textBoxKeyFile.Text = info.FullName;
                mainForm.textBoxLog.Text += "Selected key file: " + info.Name.ToString() + "\r\n";
                //mainForm.KeyFilePath = info.FullName;
            }
        }
    }
}
