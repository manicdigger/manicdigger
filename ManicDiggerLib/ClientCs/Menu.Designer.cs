namespace GameModeFortress
{
    partial class Menu
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Menu));
            this.button_singleplayer = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.button_multiplayer = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // button_singleplayer
            // 
            this.button_singleplayer.BackColor = System.Drawing.Color.Transparent;
            this.button_singleplayer.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.button_singleplayer.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button_singleplayer.FlatAppearance.BorderSize = 0;
            this.button_singleplayer.FlatAppearance.CheckedBackColor = System.Drawing.Color.Transparent;
            this.button_singleplayer.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.button_singleplayer.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.button_singleplayer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_singleplayer.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_singleplayer.ForeColor = System.Drawing.SystemColors.Window;
            this.button_singleplayer.Location = new System.Drawing.Point(32, 350);
            this.button_singleplayer.Name = "button_singleplayer";
            this.button_singleplayer.Size = new System.Drawing.Size(256, 64);
            this.button_singleplayer.TabIndex = 10;
            this.button_singleplayer.Text = "Create or load singleplayer world...";
            this.button_singleplayer.UseVisualStyleBackColor = false;
            this.button_singleplayer.Click += new System.EventHandler(this.button_singleplayer_Click);
            this.button_singleplayer.MouseEnter += new System.EventHandler(this.button_singleplayer_MouseEnter);
            this.button_singleplayer.MouseLeave += new System.EventHandler(this.button_singleplayer_MouseLeave);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.CheckFileExists = false;
            this.openFileDialog1.DefaultExt = "mddbs";
            this.openFileDialog1.FileName = "Default";
            this.openFileDialog1.Filter = "Manic Digger Savegame|*.mddbs|All files|*.*";
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(735, 344);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 11;
            this.pictureBox1.TabStop = false;
            // 
            // button_multiplayer
            // 
            this.button_multiplayer.BackColor = System.Drawing.Color.Transparent;
            this.button_multiplayer.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.button_multiplayer.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button_multiplayer.FlatAppearance.BorderSize = 0;
            this.button_multiplayer.FlatAppearance.CheckedBackColor = System.Drawing.Color.Transparent;
            this.button_multiplayer.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.button_multiplayer.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.button_multiplayer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_multiplayer.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_multiplayer.ForeColor = System.Drawing.SystemColors.Window;
            this.button_multiplayer.Location = new System.Drawing.Point(447, 350);
            this.button_multiplayer.Name = "button_multiplayer";
            this.button_multiplayer.Size = new System.Drawing.Size(256, 64);
            this.button_multiplayer.TabIndex = 17;
            this.button_multiplayer.Text = "Browse multiplayer server list...";
            this.button_multiplayer.UseVisualStyleBackColor = false;
            this.button_multiplayer.Click += new System.EventHandler(this.button_multiplayer_Click);
            this.button_multiplayer.MouseEnter += new System.EventHandler(this.button_multiplayer_MouseEnter);
            this.button_multiplayer.MouseLeave += new System.EventHandler(this.button_multiplayer_MouseLeave);
            // 
            // Menu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(735, 446);
            this.Controls.Add(this.button_multiplayer);
            this.Controls.Add(this.button_singleplayer);
            this.Controls.Add(this.pictureBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Menu";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Manic Digger";
            this.Load += new System.EventHandler(this.Menu_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button button_multiplayer;
        private System.Windows.Forms.Button button_singleplayer;
    }
}