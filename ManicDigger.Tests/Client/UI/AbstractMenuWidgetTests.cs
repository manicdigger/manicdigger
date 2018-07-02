using NUnit.Framework;

namespace ManicDigger.Tests.UI
{
	[TestFixture(typeof(AnimatedBackgroundWidget))]
	[TestFixture(typeof(ButtonWidget))]
	[TestFixture(typeof(CheckBoxWidget))]
	[TestFixture(typeof(ImageWidget))]
	[TestFixture(typeof(ListWidget))]
	[TestFixture(typeof(ProgressBarWidget))]
	[TestFixture(typeof(ServerButtonWidget))]
	[TestFixture(typeof(TextBoxWidget))]
	[TestFixture(typeof(TextWidget))]
	public class AbstractMenuWidgetTests<TWidget> where TWidget : AbstractMenuWidget, new()
	{
		private AbstractMenuWidget w;

		[SetUp]
		public void SetUp()
		{
			this.w = new TWidget();
		}

		[Test()]
		public void ConstructorTest()
		{
			Assert.That(w, Is.TypeOf(typeof(TWidget)));
			Assert.That(w, Is.InstanceOf(typeof(TWidget)));
			Assert.That(w, Is.InstanceOf(typeof(AbstractMenuWidget)));
			Assert.That(w.GetX(), Is.EqualTo(0));
			Assert.That(w.GetY(), Is.EqualTo(0));
			Assert.That(w.GetSizeX(), Is.EqualTo(0));
			Assert.That(w.GetSizeY(), Is.EqualTo(0));
			Assert.That(w.GetColor(), Is.EqualTo(-1));
			Assert.That(w.GetEventKeyPressed(), Is.EqualTo(false));
			Assert.That(w.GetEventName(), Is.EqualTo(null));
			Assert.That(w.GetEventResponse(), Is.AnyOf(null, ""));
			Assert.That(w.GetFocused(), Is.EqualTo(false));
			Assert.That(w.GetVisible(), Is.EqualTo(true));
		}

		[Test()]
		public void OnKeyPressTest()
		{
			KeyPressEventArgs args = new KeyPressEventArgs();
			args.SetHandled(false);
			args.SetKeyChar(1);

			// false after initialization
			Assert.That(w.GetEventKeyPressed(), Is.EqualTo(false));
			// false when no key is set
			w.OnKeyPress(null, args);
			Assert.That(w.GetEventKeyPressed(), Is.EqualTo(false));
			// false when wrong key is set
			w.SetEventKeyChar(2);
			Assert.That(w.GetEventKeyPressed(), Is.EqualTo(false));
			w.OnKeyPress(null, args);
			Assert.That(w.GetEventKeyPressed(), Is.EqualTo(false));
			// true when right key is set
			w.SetEventKeyChar(1);
			Assert.That(w.GetEventKeyPressed(), Is.EqualTo(false));
			w.OnKeyPress(null, args);
			Assert.That(w.GetEventKeyPressed(), Is.EqualTo(true));
			// true when event is repeated
			w.OnKeyPress(null, args);
			Assert.That(w.GetEventKeyPressed(), Is.EqualTo(true));
			// false when wrong key is set again
			w.SetEventKeyChar(2);
			Assert.That(w.GetEventKeyPressed(), Is.EqualTo(false));
			w.OnKeyPress(null, args);
			Assert.That(w.GetEventKeyPressed(), Is.EqualTo(false));
		}

		[Test()]
		public void OnKeyDownTest()
		{
			// no common logic yet
			Assert.Pass();
		}

		[Test()]
		public void OnMouseDownTest()
		{
			// no common logic yet
			Assert.Pass();
		}

		[Test()]
		public void OnMouseUpTest()
		{
			// no common logic yet
			Assert.Pass();
		}

		[Test()]
		public void OnMouseMoveTest()
		{
			// no common logic yet
			Assert.Pass();
		}

		[Test()]
		public void OnMouseWheelTest()
		{
			// no common logic yet
			Assert.Pass();
		}

		[Test()]
		public void IsCursorInsideTest()
		{
			MouseEventArgs args = new MouseEventArgs();
			MouseEventArgs args2 = new MouseEventArgs();
			Assert.That(w.IsCursorInside(args), Is.EqualTo(true));
			args.SetX(10);
			args.SetY(20);
			args2.SetX(-1);
			args2.SetY(-1);
			Assert.That(w.IsCursorInside(args), Is.EqualTo(false));
			Assert.That(w.IsCursorInside(args2), Is.EqualTo(false));
			w.SetSizeX(5);
			w.SetSizeY(5);
			Assert.That(w.IsCursorInside(args), Is.EqualTo(false));
			Assert.That(w.IsCursorInside(args2), Is.EqualTo(false));
			w.SetSizeX(10);
			w.SetSizeY(5);
			Assert.That(w.IsCursorInside(args), Is.EqualTo(false));
			Assert.That(w.IsCursorInside(args2), Is.EqualTo(false));
			w.SetSizeX(5);
			w.SetSizeY(20);
			Assert.That(w.IsCursorInside(args), Is.EqualTo(false));
			Assert.That(w.IsCursorInside(args2), Is.EqualTo(false));
			w.SetSizeX(10);
			w.SetSizeY(20);
			Assert.That(w.IsCursorInside(args), Is.EqualTo(true));
			Assert.That(w.IsCursorInside(args2), Is.EqualTo(false));
		}

		[Test()]
		public void FocusedTest()
		{
			Assert.That(w.GetFocused(), Is.EqualTo(false));
			w.SetFocused(true);
			if (w.GetFocusable())
			{
				Assert.That(w.GetFocused(), Is.EqualTo(true));
			}
			else
			{
				Assert.That(w.GetFocused(), Is.EqualTo(false));
			}
			w.SetFocused(false);
			Assert.That(w.GetFocused(), Is.EqualTo(false));
		}

		[Test()]
		public void VisibleTest()
		{
			Assert.That(w.GetVisible(), Is.EqualTo(true));
			w.SetVisible(false);
			Assert.That(w.GetVisible(), Is.EqualTo(false));
			w.SetVisible(true);
			Assert.That(w.GetVisible(), Is.EqualTo(true));
		}

		[Test()]
		public void ClickableTest()
		{
			if (w.GetClickable())
			{
				w.SetClickable(true);
				Assert.That(w.GetClickable(), Is.EqualTo(true));
			}
			else
			{
				w.SetClickable(true);
				Assert.That(w.GetClickable(), Is.EqualTo(true));
			}
			w.SetClickable(false);
			Assert.That(w.GetClickable(), Is.EqualTo(false));
		}

		[Test()]
		public void FocusableTest()
		{
			w.SetFocused(true);
			if (w.GetFocusable())
			{
				Assert.That(w.GetFocused(), Is.EqualTo(true));
			}
			else
			{
				Assert.That(w.GetFocused(), Is.EqualTo(false));
			}
			w.SetFocusable(false);
			Assert.That(w.GetFocusable(), Is.EqualTo(false));
			Assert.That(w.GetFocused(), Is.EqualTo(false));
		}

		[Test()]
		public void HasBeenClickedTest()
		{
			MouseEventArgs args = new MouseEventArgs();
			MouseEventArgs args2 = new MouseEventArgs();
			Assert.That(w.HasBeenClicked(args), Is.EqualTo(w.GetClickable()));
			args.SetX(10);
			args.SetY(20);
			args2.SetX(-1);
			args2.SetY(-1);
			Assert.That(w.HasBeenClicked(args), Is.EqualTo(false));
			Assert.That(w.HasBeenClicked(args2), Is.EqualTo(false));
			w.SetSizeX(5);
			w.SetSizeY(5);
			Assert.That(w.HasBeenClicked(args), Is.EqualTo(false));
			Assert.That(w.HasBeenClicked(args2), Is.EqualTo(false));
			w.SetSizeX(10);
			w.SetSizeY(5);
			Assert.That(w.HasBeenClicked(args), Is.EqualTo(false));
			Assert.That(w.HasBeenClicked(args2), Is.EqualTo(false));
			w.SetSizeX(5);
			w.SetSizeY(20);
			Assert.That(w.HasBeenClicked(args), Is.EqualTo(false));
			Assert.That(w.HasBeenClicked(args2), Is.EqualTo(false));
			w.SetSizeX(10);
			w.SetSizeY(20);
			Assert.That(w.HasBeenClicked(args), Is.EqualTo(w.GetClickable()));
			Assert.That(w.HasBeenClicked(args2), Is.EqualTo(false));
		}

		[Test()]
		public void SetXTest()
		{
			int val = 5;
			Assert.That(w.GetX(), Is.EqualTo(0));
			w.SetX(val);
			Assert.That(w.GetX(), Is.EqualTo(val));
			w.SetX(-val);
			Assert.That(w.GetX(), Is.EqualTo(-val));
		}

		[Test()]
		public void SetYTest()
		{
			int val = 5;
			Assert.That(w.GetY(), Is.EqualTo(0));
			w.SetY(val);
			Assert.That(w.GetY(), Is.EqualTo(val));
			w.SetY(-val);
			Assert.That(w.GetY(), Is.EqualTo(-val));
		}

		[Test()]
		public void SetSizeXTest()
		{
			int val = 5;
			Assert.That(w.GetSizeX(), Is.EqualTo(0));
			w.SetSizeX(val);
			Assert.That(w.GetSizeX(), Is.EqualTo(val));
			w.SetSizeX(-val);
			Assert.That(w.GetSizeX(), Is.EqualTo(-val));
		}

		[Test()]
		public void SetSizeYTest()
		{
			int val = 5;
			Assert.That(w.GetSizeY(), Is.EqualTo(0));
			w.SetSizeY(val);
			Assert.That(w.GetSizeY(), Is.EqualTo(val));
			w.SetSizeY(-val);
			Assert.That(w.GetSizeY(), Is.EqualTo(-val));
		}

		[Test()]
		public void SetColorTest()
		{
			int col = ColorCi.FromArgb(0, 0, 0, 0);
			Assert.That(w.GetColor(), Is.EqualTo(ColorCi.FromArgb(255, 255, 255, 255)));
			w.SetColor(col);
			Assert.That(w.GetSizeY(), Is.EqualTo(col));
		}

		[Test()]
		public void SetEventKeyCharTest()
		{
			KeyPressEventArgs keyPressA = new KeyPressEventArgs();
			keyPressA.SetKeyChar('a');
			KeyPressEventArgs keyPressB = new KeyPressEventArgs();
			keyPressB.SetKeyChar('b');

			Assert.That(w.GetEventKeyPressed(), Is.False);
			w.SetEventKeyChar('a');
			Assert.That(w.GetEventKeyPressed(), Is.False);
			w.OnKeyPress(null, keyPressA);
			Assert.That(w.GetEventKeyPressed(), Is.True);
			w.SetEventKeyChar('b');
			Assert.That(w.GetEventKeyPressed(), Is.False);
			w.OnKeyPress(null, keyPressB);
			Assert.That(w.GetEventKeyPressed(), Is.True);
			w.OnKeyPress(null, keyPressA);
			Assert.That(w.GetEventKeyPressed(), Is.False);
		}

		[Test()]
		public void SetEventNameTest()
		{
			string val = "test";
			Assert.That(w.GetEventName(), Is.EqualTo(null));
			w.SetEventName(val);
			Assert.That(w.GetEventName(), Is.EqualTo(val));
			w.SetEventName(null);
			Assert.That(w.GetEventName(), Is.EqualTo(null));
		}

		[Test()]
		public void GetEventResponseTest()
		{
			Assert.That(w.GetEventResponse(), Is.AnyOf(null, ""));
		}

		[Test()]
		public void SetNextWidgetTest()
		{
			AbstractMenuWidget val = new ImageWidget();
			Assert.That(w.GetNextWidget(), Is.Null);
			w.SetNextWidget(val);
			Assert.That(w.GetNextWidget(), Is.EqualTo(val));
			w.SetNextWidget(null);
			Assert.That(w.GetNextWidget(), Is.Null);
		}

		[Test()]
		public void NextWidgetKeyDownTest()
		{
			KeyEventArgs args = new KeyEventArgs();
			args.SetHandled(false);
			args.SetKeyCode(GlKeys.Tab);

			// using a TextBoxWidget as it has focus properties set by default
			AbstractMenuWidget val = new TextBoxWidget();

			// no action when not focused
			w.OnKeyDown(null, args);
			Assert.That(w.GetFocused(), Is.False);
			Assert.That(val.GetFocused(), Is.False);
			Assert.That(args.GetHandled(), Is.False);

			if (!w.GetFocusable()) { return; }
			w.SetFocused(true);

			// no action when no target is set
			w.OnKeyDown(null, args);
			Assert.That(w.GetFocused(), Is.True);
			Assert.That(val.GetFocused(), Is.False);
			Assert.That(args.GetHandled(), Is.False);

			w.SetNextWidget(val);

			// no action when wrong key is pressed
			args.SetKeyCode(GlKeys.Space);
			w.OnKeyDown(null, args);
			Assert.That(w.GetFocused(), Is.True);
			Assert.That(val.GetFocused(), Is.False);
			Assert.That(args.GetHandled(), Is.False);

			// focused widget is changed and event set handled
			args.SetKeyCode(GlKeys.Tab);
			w.OnKeyDown(null, args);
			Assert.That(w.GetFocused(), Is.False);
			Assert.That(val.GetFocused(), Is.True);
			Assert.That(args.GetHandled(), Is.True);
		}
	}
}
