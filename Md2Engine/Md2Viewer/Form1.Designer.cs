namespace Md2Viewer
{
    partial class Form1
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.label1 = new System.Windows.Forms.Label();
            this.tim = new System.Windows.Forms.Timer(this.components);
            this.anim = new System.Windows.Forms.ListBox();
            this.label7 = new System.Windows.Forms.Label();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.ts_wire = new System.Windows.Forms.ToolStripButton();
            this.ts_fill = new System.Windows.Forms.ToolStripButton();
            this.ts_text = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ts_light = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.rleft = new System.Windows.Forms.ToolStripButton();
            this.rright = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.ts_minus = new System.Windows.Forms.ToolStripButton();
            this.ts_plus = new System.Windows.Forms.ToolStripButton();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(544, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Fill mode";
            // 
            // tim
            // 
            this.tim.Interval = 60;
            this.tim.Tick += new System.EventHandler(this.tim_Tick);
            // 
            // anim
            // 
            this.anim.FormattingEnabled = true;
            this.anim.Location = new System.Drawing.Point(518, 56);
            this.anim.Name = "anim";
            this.anim.Size = new System.Drawing.Size(146, 459);
            this.anim.TabIndex = 14;
            this.anim.SelectedIndexChanged += new System.EventHandler(this.anim_SelectedIndexChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(515, 40);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(58, 13);
            this.label7.TabIndex = 16;
            this.label7.Text = "Animations";
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ts_wire,
            this.ts_fill,
            this.ts_text,
            this.toolStripSeparator1,
            this.ts_light,
            this.toolStripSeparator2,
            this.toolStripLabel1,
            this.rleft,
            this.rright,
            this.toolStripSeparator3,
            this.toolStripLabel2,
            this.ts_minus,
            this.ts_plus});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(676, 25);
            this.toolStrip1.TabIndex = 18;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // ts_wire
            // 
            this.ts_wire.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ts_wire.Image = ((System.Drawing.Image)(resources.GetObject("ts_wire.Image")));
            this.ts_wire.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.ts_wire.Name = "ts_wire";
            this.ts_wire.Size = new System.Drawing.Size(23, 22);
            this.ts_wire.Text = "Wireframe";
            this.ts_wire.Click += new System.EventHandler(this.ts_wire_Click);
            // 
            // ts_fill
            // 
            this.ts_fill.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ts_fill.Image = ((System.Drawing.Image)(resources.GetObject("ts_fill.Image")));
            this.ts_fill.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.ts_fill.Name = "ts_fill";
            this.ts_fill.Size = new System.Drawing.Size(23, 22);
            this.ts_fill.Text = "Filled";
            this.ts_fill.Click += new System.EventHandler(this.ts_fill_Click);
            // 
            // ts_text
            // 
            this.ts_text.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ts_text.Image = ((System.Drawing.Image)(resources.GetObject("ts_text.Image")));
            this.ts_text.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.ts_text.Name = "ts_text";
            this.ts_text.Size = new System.Drawing.Size(23, 22);
            this.ts_text.Text = "Textured";
            this.ts_text.Click += new System.EventHandler(this.ts_text_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // ts_light
            // 
            this.ts_light.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ts_light.Image = ((System.Drawing.Image)(resources.GetObject("ts_light.Image")));
            this.ts_light.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ts_light.Name = "ts_light";
            this.ts_light.Size = new System.Drawing.Size(23, 22);
            this.ts_light.Text = "Light";
            this.ts_light.Click += new System.EventHandler(this.ts_light_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(48, 22);
            this.toolStripLabel1.Text = "Rotation";
            // 
            // rleft
            // 
            this.rleft.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.rleft.Image = ((System.Drawing.Image)(resources.GetObject("rleft.Image")));
            this.rleft.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.rleft.Name = "rleft";
            this.rleft.Size = new System.Drawing.Size(23, 22);
            this.rleft.Text = "Rotate Left";
            this.rleft.MouseDown += new System.Windows.Forms.MouseEventHandler(this.rleft_Click);
            this.rleft.Click += new System.EventHandler(this.rleft_Click);
            // 
            // rright
            // 
            this.rright.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.rright.Image = ((System.Drawing.Image)(resources.GetObject("rright.Image")));
            this.rright.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.rright.Name = "rright";
            this.rright.Size = new System.Drawing.Size(23, 22);
            this.rright.Text = "Rotate Rigth";
            this.rright.MouseDown += new System.Windows.Forms.MouseEventHandler(this.rright_Click);
            this.rright.Click += new System.EventHandler(this.rright_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // ts_minus
            // 
            this.ts_minus.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ts_minus.Image = ((System.Drawing.Image)(resources.GetObject("ts_minus.Image")));
            this.ts_minus.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.ts_minus.Name = "ts_minus";
            this.ts_minus.Size = new System.Drawing.Size(23, 22);
            this.ts_minus.Text = "Zoom out";
            this.ts_minus.Click += new System.EventHandler(this.ts_minus_Click);
            // 
            // ts_plus
            // 
            this.ts_plus.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ts_plus.Image = ((System.Drawing.Image)(resources.GetObject("ts_plus.Image")));
            this.ts_plus.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.ts_plus.Name = "ts_plus";
            this.ts_plus.Size = new System.Drawing.Size(23, 22);
            this.ts_plus.Text = "Zoom in";
            this.ts_plus.Click += new System.EventHandler(this.ts_plus_Click);
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(33, 22);
            this.toolStripLabel2.Text = "Zoom";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(676, 525);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.anim);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Viewer";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Timer tim;
        private System.Windows.Forms.ListBox anim;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton ts_wire;
        private System.Windows.Forms.ToolStripButton ts_fill;
        private System.Windows.Forms.ToolStripButton ts_text;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton ts_light;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripButton rleft;
        private System.Windows.Forms.ToolStripButton rright;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripButton ts_minus;
        private System.Windows.Forms.ToolStripButton ts_plus;

    }
}

