namespace FileEncryptor
{
    partial class EncryptionForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.checkBoxIDEA = new System.Windows.Forms.CheckBox();
            this.checkBoxElgamal = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(42, 71);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 6;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(123, 71);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 7;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(25, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(173, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Please, choose encryption method.";
            // 
            // checkBoxIDEA
            // 
            this.checkBoxIDEA.AutoSize = true;
            this.checkBoxIDEA.Location = new System.Drawing.Point(12, 25);
            this.checkBoxIDEA.Name = "checkBoxIDEA";
            this.checkBoxIDEA.Size = new System.Drawing.Size(103, 17);
            this.checkBoxIDEA.TabIndex = 11;
            this.checkBoxIDEA.Text = "IDEA encryption";
            this.checkBoxIDEA.UseVisualStyleBackColor = true;
            // 
            // checkBoxElgamal
            // 
            this.checkBoxElgamal.AutoSize = true;
            this.checkBoxElgamal.Location = new System.Drawing.Point(12, 48);
            this.checkBoxElgamal.Name = "checkBoxElgamal";
            this.checkBoxElgamal.Size = new System.Drawing.Size(115, 17);
            this.checkBoxElgamal.TabIndex = 12;
            this.checkBoxElgamal.Text = "Elgamal encryption";
            this.checkBoxElgamal.UseVisualStyleBackColor = true;
            // 
            // EncryptionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(224, 102);
            this.Controls.Add(this.checkBoxElgamal);
            this.Controls.Add(this.checkBoxIDEA);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "EncryptionForm";
            this.Text = "EncryptionForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox checkBoxIDEA;
        private System.Windows.Forms.CheckBox checkBoxElgamal;
    }
}