using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Md2Viewer
{
    public partial class Form1 : Form
    {
        private OGLView oglView;

        public static Thread thrOpenGL;

        public Form1()
        {
            InitializeComponent();

            oglView = new OGLView();
            oglView.Parent = this;
            oglView.Location = new System.Drawing.Point(10, 10);
            oglView.Size = new System.Drawing.Size(500, 500);
            oglView.Dock = DockStyle.None;

            oglView.drawFilled();
            oglView.drawSmooth();
            oglView.drawTextured(true);
            oglView.drawLights(true);

            oglView.setColor(1, 1, 1);
            oglView.setSca(0.08f);
            oglView.setRotz(-45);

            oglView.setAnim(oglView.m1.animationPool[0].getName());

            ts_fill.Checked = true;
            ts_wire.Checked = false;
            ts_text.Checked = true;
            ts_light.Checked = true;

            tim.Enabled = true;
        }

        private void tim_Tick(object sender, EventArgs e)
        {
            oglView.Refresh();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            tim.Enabled = true;
            for (int i = 0; i < oglView.m1.animationPool.Count; i++)
            {
                anim.Items.Add(oglView.m1.animationPool[i].getName());
            }
        }

        private void anim_SelectedIndexChanged(object sender, EventArgs e)
        {
            oglView.setAnim(anim.Text);
        }

        private void ts_wire_Click(object sender, EventArgs e)
        {
            oglView.drawWireframe();
            ts_wire.Checked = true;
            ts_fill.Checked = false;
        }

        private void ts_fill_Click(object sender, EventArgs e)
        {
            oglView.drawFilled();
            ts_wire.Checked = false;
            ts_fill.Checked = true;
        }

        private void ts_text_Click(object sender, EventArgs e)
        {
            if (!ts_text.Checked)
            {
                oglView.drawTextured(true);
                ts_text.Checked = true;
            }
            else
            {
                oglView.drawTextured(false);
                ts_text.Checked = false;
            }
        }

        private void ts_light_Click(object sender, EventArgs e)
        {
            if (!ts_light.Checked)
            {
                oglView.drawLights(true);
                ts_light.Checked = true;
            }
            else
            {
                oglView.drawLights(false);
                ts_light.Checked = false;
            }
        }

        private void rleft_Click(object sender, EventArgs e)
        {
            oglView.setRotz(oglView.getRotz() - 15);
        }

        private void rright_Click(object sender, EventArgs e)
        {
            oglView.setRotz(oglView.getRotz() + 15);
        }

        private void rleft_Click(object sender, MouseEventArgs e)
        {
            oglView.setRotz(oglView.getRotz() - 5);
        }

        private void rright_Click(object sender, MouseEventArgs e)
        {
            oglView.setRotz(oglView.getRotz() + 5);
        }

        private void ts_minus_Click(object sender, EventArgs e)
        {
            oglView.setSca(oglView.getSca() - 0.01f);
        }

        private void ts_plus_Click(object sender, EventArgs e)
        {
            oglView.setSca(oglView.getSca() + 0.01f);
        }
    }
}