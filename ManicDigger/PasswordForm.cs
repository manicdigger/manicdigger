using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GameModeFortress
{
    public partial class PasswordForm : Form
    {
        public PasswordForm()
        {
            InitializeComponent();
        }

        public string Password
        {
            get { return this.passwordTextBox.Text; }
            set { passwordTextBox.Text = value; }
        }
    }
}
