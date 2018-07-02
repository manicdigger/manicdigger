using NUnit.Framework;

namespace ManicDigger.Tests.UI
{
	[TestFixture()]
	public class TextBoxWidgetTests
	{
		private TextBoxWidget w;
		private MouseEventArgs mouseInside;
		private MouseEventArgs mouseOutside;
		private KeyEventArgs keyEventA;
		private KeyEventArgs keyEventB;
		private KeyPressEventArgs keyPressA;
		private KeyPressEventArgs keyPressB;

		[SetUp()]
		public void TextBoxWidgetTest()
		{
			w = new TextBoxWidget();
			w.SetX(5);
			w.SetY(5);
			w.SetSizeX(10);
			w.SetSizeY(10);
			mouseInside = new MouseEventArgs();
			mouseInside.SetX(10);
			mouseInside.SetY(10);
			mouseOutside = new MouseEventArgs();
			mouseOutside.SetX(20);
			mouseOutside.SetY(20);
			keyEventA = new KeyEventArgs();
			keyEventA.SetKeyCode(GlKeys.A);
			keyEventB = new KeyEventArgs();
			keyEventB.SetKeyCode(GlKeys.B);
			keyPressA = new KeyPressEventArgs();
			keyPressA.SetKeyChar('a');
			keyPressB = new KeyPressEventArgs();
			keyPressB.SetKeyChar('b');
		}

		[Test()]
		public void OnMouseDownTest()
		{
			w.OnMouseDown(null, mouseOutside);
			Assert.That(w.GetFocused(), Is.False);
			w.OnMouseDown(null, mouseInside);
			Assert.That(w.GetFocused(), Is.True);
			w.OnMouseMove(null, mouseInside);
			w.OnMouseDown(null, mouseOutside);
			Assert.That(w.GetFocused(), Is.False);
			w.OnMouseMove(null, mouseInside);
			w.OnMouseDown(null, mouseInside);
			Assert.That(w.GetFocused(), Is.True);
		}

		[Test()]
		public void OnKeyPressTest()
		{
			Assert.Inconclusive("Input testing not implemented due to missing GamePlatform interface");

			Assert.That(w.GetContent(), Is.EqualTo(""));
			w.OnKeyPress(null, keyPressA);
			Assert.That(w.GetContent(), Is.EqualTo(""));
			w.SetFocused(true);
			w.OnKeyPress(null, keyPressA);
			Assert.That(w.GetContent(), Is.EqualTo("a"));
			w.OnKeyPress(null, keyPressB);
			Assert.That(w.GetContent(), Is.EqualTo("ab"));
			w.SetFocused(true);
			w.OnKeyPress(null, keyPressB);
			Assert.That(w.GetContent(), Is.EqualTo("ab"));
		}

		[Test()]
		public void OnKeyDownTest()
		{
			Assert.Inconclusive("Input testing not implemented due to missing GamePlatform interface");
			// TODO: Deleting text and pasting clipboard contents
		}

		[Test()]
		public void GetEventResponseTest()
		{
			Assert.Inconclusive("Input testing not implemented due to missing GamePlatform interface");
			// TODO: Enter text and check output
		}

		[Test()]
		public void SetContentTest()
		{
			string val = "test";
			Assert.That(w.GetContent(), Is.EqualTo(""));
			w.SetContent(null, null);
			Assert.That(w.GetContent(), Is.EqualTo(""));
			w.SetContent(null, val);
			Assert.That(w.GetContent(), Is.EqualTo(val));
			w.SetContent(null, null);
			Assert.That(w.GetContent(), Is.EqualTo(""));
		}
	}
}