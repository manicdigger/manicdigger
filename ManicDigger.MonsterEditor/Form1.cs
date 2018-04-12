using ManicDigger;
using ManicDigger.ClientNative;
using ManicDigger.Server;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ManicDigger.MonsterEditor
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		IGetFileStream getfile;
		private void Form1_Load(object sender, EventArgs e)
		{
			// load skin file
			string[] datapaths = new[] {
				Path.Combine(Path.Combine(Path.Combine("..", ".."), ".."), "data"),
				"data"
			};
			getfile = new GetFileStream(datapaths);
			richTextBox1.Text = new StreamReader(getfile.GetFile("player.txt")).ReadToEnd();

			// init UI
			RichTextBoxContextMenu(richTextBox1);
			RichTextBoxContextMenu(richTextBox2);
			UpdateLabels();

			// init 3D rendering
			the3d = new TextureLoader() { d_Config3d = config3d };
			glControl1.Paint += new PaintEventHandler(glControl1_Paint);
			glControl1.MouseWheel += new System.Windows.Forms.MouseEventHandler(glControl1_MouseWheel);
			loaded = true;
			GL.ClearColor(Color.SkyBlue);
			overheadcameraK.SetDistance(4);
			overheadcameraK.SetT((float)Math.PI);
			SetupViewport();
			Application.Idle += new EventHandler(Application_Idle);
			sw.Start();
		}

		private void RichTextBoxContextMenu(RichTextBox richTextBox)
		{
			ContextMenu cm = new ContextMenu();
			MenuItem mi = new MenuItem("Cut");
			mi.Click += (a, b) =>
			{
				richTextBox.Cut();
			};
			cm.MenuItems.Add(mi);

			mi = new MenuItem("Copy");
			mi.Click += (a, b) =>
			{
				richTextBox.Copy();
			};
			cm.MenuItems.Add(mi);

			mi = new MenuItem("Paste");
			mi.Click += (a, b) =>
			{
				richTextBox.Paste(DataFormats.GetFormat(DataFormats.UnicodeText));
			};
			cm.MenuItems.Add(mi);

			richTextBox.ContextMenu = cm;
		}

		private void UpdateLabels()
		{
			label1.Text = string.Format("Heading: {0} degrees.", HeadingDeg());
			label2.Text = string.Format("Pitch: {0} degrees.", PitchDeg());
		}

		private float HeadingDeg()
		{
			return -trackBar1.Value * 30;
		}

		private float PitchDeg()
		{
			return trackBar2.Value * 15;
		}

		Stopwatch sw = new Stopwatch();
		float dt;
		void Application_Idle(object sender, EventArgs e)
		{
			// Update dt as time since last call
			sw.Stop();
			double milliseconds = sw.Elapsed.TotalMilliseconds;
			dt = (float)(milliseconds / 1000);
			sw.Restart();

			// Invalidate the GLControl to force a redraw
			glControl1.Invalidate();
		}

		void glControl1_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (e.Delta != 0)
			{
				overheadcameraK.SetDistance(overheadcameraK.GetDistance() - 0.002f * e.Delta);
			}
		}

		void glControl1_Paint(object sender, PaintEventArgs e)
		{
			// Forward Paint event to Render()
			Render();
		}

		bool loaded;
		int playertexture = -1;
		bool modelLoaded;
		private void Render()
		{
			if (!loaded || !modelLoaded)
			{
				// Do not update if no model is currently loaded
				return;
			}

			// Clear buffers for new frame
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			// Initialize camera
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadIdentity();
			OverheadCamera();

			// Load model texture
			if (playertexture == -1)
			{
				LoadPlayerTexture(MyStream.ReadAllBytes(getfile.GetFile("mineplayer.png")));
			}

			// Draw grass and axis lines
			DrawGrass();
			DrawAxisLine(new Vector3(), HeadingDeg(), PitchDeg());

			// Try to render the model and display errors
			GL.Enable(EnableCap.Texture2D);
			bool exception = false;
			try
			{
				game.GLMatrixModeModelView();
				game.GLLoadMatrix(m);
				GL.BindTexture(TextureTarget.Texture2D, playertexture);
				game.GLRotate(HeadingDeg(), 0, 1, 0);
				d.Render(dt, PitchDeg(), 1);
			}
			catch (Exception ee)
			{
				if (richTextBox2Text != ee.ToString())
				{
					richTextBox2Text = ee.ToString();
					richTextBox2.Text = ee.ToString();
				}
				exception = true;
			}
			if (!exception)
			{
				richTextBox2.Text = "";
				richTextBox2Text = "";
				progressBar1.Value = (int)((d.GetAnimationFrame() / d.GetAnimationLength()) * progressBar1.Maximum);
			}

			// Swap buffers to draw changes
			glControl1.SwapBuffers();
		}
		string richTextBox2Text = "";

		private void DrawAxisLine(Vector3 v, float myheadingdeg, float mypitchdeg)
		{
			GL.Disable(EnableCap.Texture2D);
			GL.PushMatrix();
			GL.Translate(v);
			GL.Rotate(myheadingdeg, 0, 1, 0);
			GL.Rotate(mypitchdeg, 0, 0, 1);
			GL.Begin(PrimitiveType.Lines);
			GL.Color3(Color.Red);
			GL.Vertex3(0, 0.1, 0);
			GL.Vertex3(2, 0.1, 0);
			GL.Color3(Color.Green);
			GL.Vertex3(0, 0.1, 0);
			GL.Vertex3(0, 2, 0);
			GL.Color3(Color.Blue);
			GL.Vertex3(0, 0.1, 0);
			GL.Vertex3(0, 0.1, 2);
			GL.End();
			GL.Color3(Color.White);
			GL.PopMatrix();
		}

		TextureLoader the3d;
		int grasstexture = -1;
		private void DrawGrass()
		{
			if (grasstexture == -1)
			{
				grasstexture = LoadTexture(getfile.GetFile("grass_tiled.png"));
			}
			GL.BindTexture(TextureTarget.Texture2D, grasstexture);
			GL.Enable(EnableCap.Texture2D);
			GL.Color3(Color.White);
			GL.Begin(PrimitiveType.Quads);
			Rectangle r = new Rectangle(-10, -10, 20, 20);
			DrawWaterQuad(r.X, r.Y, r.Width, r.Height, 0);
			GL.End();
		}

		public int LoadTexture(Stream file)
		{
			using (file)
			{
				using (Bitmap bmp = new Bitmap(file))
				{
					return the3d.LoadTexture(bmp);
				}
			}
		}

		private void LoadPlayerTexture(byte[] file)
		{
			playertexture = the3d.LoadTexture(new Bitmap(new MemoryStream(file)));
		}

		void DrawWaterQuad(float x1, float y1, float width, float height, float z1)
		{
			RectangleF rect = new RectangleF(0, 0, 1 * width, 1 * height);
			float x2 = x1 + width;
			float y2 = y1 + height;
			GL.TexCoord2(rect.Right, rect.Bottom);
			GL.Vertex3(x2, z1, y2);
			GL.TexCoord2(rect.Right, rect.Top);
			GL.Vertex3(x2, z1, y1);
			GL.TexCoord2(rect.Left, rect.Top);
			GL.Vertex3(x1, z1, y1);
			GL.TexCoord2(rect.Left, rect.Bottom);
			GL.Vertex3(x1, z1, y2);
		}

		AnimatedModelRenderer d;
		AnimationState animstate = new AnimationState();
		Config3d config3d = new Config3d();
		Kamera overheadcameraK = new Kamera();
		Vector3 up = new Vector3(0f, 1f, 0f);
		private void SetupViewport()
		{
			float aspect_ratio = glControl1.Width / (float)glControl1.Height;
			Matrix4 perspective = Matrix4.CreatePerspectiveFieldOfView(fov, aspect_ratio, znear, zfar);
			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadMatrix(ref perspective);
			OverheadCamera();
		}

		GamePlatformNative platform = new GamePlatformNative();
		private void OverheadCamera()
		{
			GL.MatrixMode(MatrixMode.Modelview);

			Vector3Ref position = new Vector3Ref();
			overheadcameraK.GetPosition(platform, position);
			Vector3 position_ = new Vector3(position.GetX(), position.GetY(), position.GetZ());

			Vector3Ref center = new Vector3Ref();
			overheadcameraK.GetCenter(center);
			Vector3 center_ = new Vector3(center.GetX(), center.GetY(), center.GetZ());

			Matrix4 camera = Matrix4.LookAt(position_, center_, up);
			m = new float[] {
				camera.M11, camera.M12, camera.M13, camera.M14,
				camera.M21, camera.M22, camera.M23, camera.M24,
				camera.M31, camera.M32, camera.M33, camera.M34,
				camera.M41, camera.M42, camera.M43, camera.M44
			};
			GL.LoadMatrix(ref camera);
		}

		float[] m;
		float znear = 0.1f;
		float zfar { get { return ENABLE_ZFAR ? config3d.GetViewDistance() * 3f / 4 : 99999; } }
		bool ENABLE_ZFAR = false;
		public float fov = MathHelper.PiOver3;
		int oldmousex = 0;
		int oldmousey = 0;

		private void glControl1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			oldmousex = e.X;
			oldmousey = e.Y;
			down = true;
		}

		bool down = false;
		private void glControl1_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (down)
			{
				int deltax = e.X - oldmousex;
				int deltay = e.Y - oldmousey;
				oldmousex = e.X;
				oldmousey = e.Y;
				overheadcameraK.SetT(overheadcameraK.GetT() + (float)deltax * 0.05f);
				overheadcameraK.SetAngle(overheadcameraK.GetAngle() + (float)deltay * 1f);
				if (overheadcameraK.GetAngle() > 89)
				{
					overheadcameraK.SetAngle(89);
				}
				if (overheadcameraK.GetAngle() < -89)
				{
					overheadcameraK.SetAngle(-89);
				}
			}
		}

		private void glControl1_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			down = false;
		}

		private void trackBar1_Scroll(object sender, EventArgs e)
		{
			UpdateLabels();
		}

		private void trackBar2_Scroll(object sender, EventArgs e)
		{
			UpdateLabels();
		}

		private void richTextBox1_TextChanged(object sender, EventArgs e)
		{
			LoadModel();

			// update animation list
			int oldAnmimationIndex = listBox1.SelectedIndex;
			listBox1.Items.Clear();
			int animationCount = d.GetAnimationCount();
			for (int i = 0; i < animationCount; i++)
			{
				listBox1.Items.Add(d.GetAnimationName(i));
			}
			if (oldAnmimationIndex < listBox1.Items.Count)
			{
				// if animation still exists switch to the same as before the change
				listBox1.SelectedIndex = oldAnmimationIndex;
			}
		}

		Game game;
		void LoadModel()
		{
			game = new Game();
			game.SetPlatform(new GamePlatformNative());
			d = new AnimatedModelRenderer();
			AnimatedModel model = new AnimatedModel();
			try
			{
				model = AnimatedModelSerializer.Deserialize(game.GetPlatform(), richTextBox1.Text);
				modelLoaded = true;
			}
			catch (Exception ex)
			{
				modelLoaded = false;
				model = null;
				richTextBox2.Text = "Error in LoadModel():" + Environment.NewLine + ex.ToString();
			}
			if (model != null)
			{
				d.Start(game, model);
			}
		}

		private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			d.SetAnimationId(listBox1.SelectedIndex);
		}

		private void loadTextureToolStripMenuItem_Click(object sender, EventArgs e)
		{
			DialogResult result = openFileDialog1.ShowDialog();
			if (result == DialogResult.OK)
			{
				LoadPlayerTexture(File.ReadAllBytes(openFileDialog1.FileName));
			}
		}

		private void loadModelToolStripMenuItem_Click(object sender, EventArgs e)
		{
			DialogResult result = openFileDialog2.ShowDialog();
			if (result == DialogResult.OK)
			{
				richTextBox1.Text = File.ReadAllText(openFileDialog2.FileName);
			}
		}
	}

	public static class MyStream
	{
		public static byte[] ReadAllBytes(Stream stream)
		{
			return new BinaryReader(stream).ReadBytes((int)stream.Length);
		}
	}
}
