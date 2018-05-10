using System;
using System.Windows.Forms;

namespace FileEncryptor
{
    public partial class EncryptionForm : Form
    {
        MainForm mainForm;
        public EncryptionForm(MainForm form)
        {
            InitializeComponent();
            this.mainForm = form;            
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            if((checkBoxIDEA.Checked & checkBoxElgamal.Checked) || (checkBoxIDEA.Checked == false & checkBoxElgamal.Checked == false))
            {
                MessageBox.Show("Select 1 encryption method.");
                return;
            }

            if (checkBoxIDEA.Checked)
            {
                mainForm.EncryptionFlag = 1;
            }
            if (checkBoxElgamal.Checked)
            {
                mainForm.EncryptionFlag = 2;
            }
            this.DialogResult = DialogResult.OK;
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}
