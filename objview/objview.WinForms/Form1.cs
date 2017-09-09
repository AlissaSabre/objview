using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace objview.WinForms
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void glControl1_ContextCreated(object sender, OpenGL.GlControlEventArgs e)
        {
            GlMain.Initialize();
            GlMain.Resize(glControl1.Width, glControl1.Height);
        }

        private void glControl1_ContextDestroying(object sender, OpenGL.GlControlEventArgs e)
        {
            GlMain.Destroy();
        }

        private void glControl1_SizeChanged(object sender, EventArgs e)
        {
            GlMain.Resize(glControl1.Width, glControl1.Height);
            glControl1.Invalidate();
        }

        private void glControl1_Render(object sender, OpenGL.GlControlEventArgs e)
        {
            GlMain.Draw();
        }

        private void glControl1_MouseDown(object sender, MouseEventArgs e)
        {

        }

        private void glControl1_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void glControl1_MouseUp(object sender, MouseEventArgs e)
        {

        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK == openFileDialog1.ShowDialog())
            {
                var mesh = ObjMesh.FromFile(openFileDialog1.FileName);
                GlMain.Mesh = mesh;
                glControl1.Invalidate();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

    }
}
