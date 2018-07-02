using NUnit.Framework;

namespace ManicDigger.Tests.UI
{
	[TestFixture()]
	public class ListWidgetTests
	{
		ListWidget w;
		MouseEventArgs args_pgUp;
		MouseEventArgs args_pgDown;
		MouseEventArgs args_outside;
		MouseEventArgs args_elem0;
		MouseWheelEventArgs wheelargs_down;
		MouseWheelEventArgs wheelargs_up;

		[SetUp]
		public void ListWidgetTest()
		{
			w = new ListWidget();
			w.SetSizeX(500);
			w.SetSizeY(300);

			args_pgUp = new MouseEventArgs();
			args_pgUp.SetX((int)w.GetSizeX() - 30);
			args_pgUp.SetY((int)w.GetSizeY() - 30);
			args_pgDown = new MouseEventArgs();
			args_pgDown.SetX((int)w.GetSizeX() - 30);
			args_pgDown.SetY(30);
			args_outside = new MouseEventArgs();
			args_outside.SetX(-10);
			args_outside.SetY(-10);
			args_elem0 = new MouseEventArgs();
			args_elem0.SetX(100);
			args_elem0.SetY(50);

			wheelargs_down = new MouseWheelEventArgs();
			wheelargs_down.SetDelta(-1);
			wheelargs_up = new MouseWheelEventArgs();
			wheelargs_up.SetDelta(1);

			for (int i = 0; i < 5; i++)
			{
				ListEntry e = new ListEntry();
				e.imageMain = string.Format("image{0}.png", i);
				e.imageStatusBottom = string.Format("image_bot{0}.png", i);
				e.imageStatusTop = string.Format("image_top{0}.png", i);
				e.textBottomLeft = string.Format("text_bl{0}", i);
				e.textBottomRight = string.Format("text_br{0}", i);
				e.textTopLeft = string.Format("text_tl{0}", i);
				e.textTopRight = string.Format("text_tr{0}", i);
				w.AddElement(e);
			}
		}

		[Test()]
		public void OnMouseDownTest()
		{
			// TODO: perform test
			Assert.Inconclusive("Test skipped. GamePlatform required!");

			Assert.That(w.GetFocused(), Is.False);
			w.OnMouseDown(null, args_elem0);
			Assert.That(w.GetFocused(), Is.True);
		}

		[Test()]
		public void OnMouseWheelTest()
		{
			// TODO: perform test
			Assert.Inconclusive("Test skipped. GamePlatform required!");

			Assert.That(w.GetPage(), Is.EqualTo(0));
			w.OnMouseWheel(null, wheelargs_up);
			Assert.That(w.GetPage(), Is.EqualTo(0));
			w.OnMouseWheel(null, wheelargs_down);
			Assert.That(w.GetPage(), Is.EqualTo(1));
			w.OnMouseWheel(null, wheelargs_down);
			Assert.That(w.GetPage(), Is.EqualTo(2));
			w.OnMouseWheel(null, wheelargs_down);
			Assert.That(w.GetPage(), Is.EqualTo(2));
			w.OnMouseWheel(null, wheelargs_up);
			Assert.That(w.GetPage(), Is.EqualTo(1));
		}

		[Test()]
		public void AddElementTest()
		{
			int lastCount, currentCount;
			do
			{
				lastCount = w.GetEntriesCount();
				w.AddElement(new ListEntry());
				currentCount = w.GetEntriesCount();
			}
			while (lastCount != currentCount);
			Assert.That(w.GetEntriesCount(), Is.EqualTo(1024));
		}

		[TestCase(0, true)]
		[TestCase(1, true)]
		[TestCase(2, true)]
		[TestCase(3, true)]
		[TestCase(4, true)]
		[TestCase(5, false)]
		[TestCase(-1, false)]
		public void GetElementTest(int elementId, bool expectValid)
		{
			ListEntry e = w.GetElement(elementId);

			if (expectValid)
			{
				Assert.That(e, Is.Not.Null);
				Assert.That(e.imageMain, Is.EqualTo(string.Format("image{0}.png", elementId)));
				Assert.That(e.imageStatusBottom, Is.EqualTo(string.Format("image_bot{0}.png", elementId)));
				Assert.That(e.imageStatusTop, Is.EqualTo(string.Format("image_top{0}.png", elementId)));
				Assert.That(e.textBottomLeft, Is.EqualTo(string.Format("text_bl{0}", elementId)));
				Assert.That(e.textBottomRight, Is.EqualTo(string.Format("text_br{0}", elementId)));
				Assert.That(e.textTopLeft, Is.EqualTo(string.Format("text_tl{0}", elementId)));
				Assert.That(e.textTopRight, Is.EqualTo(string.Format("text_tr{0}", elementId)));
			}
			else
			{
				Assert.That(e, Is.Null);
			}
		}

		[Test()]
		public void ClearTest()
		{
			Assert.That(w.GetEntriesCount(), Is.EqualTo(5));
			w.Clear();
			Assert.That(w.GetEntriesCount(), Is.EqualTo(0));
			Assert.That(w.GetElement(0), Is.Null);
		}

		[Test()]
		public void GetIndexSelectedTest()
		{
			// TODO: perform test
			Assert.Inconclusive("Test skipped. GamePlatform required!");

			Assert.That(w.GetIndexSelected(), Is.EqualTo(-1));
			w.OnMouseDown(null, args_elem0);
			Assert.That(w.GetIndexSelected(), Is.EqualTo(0));
			w.OnMouseDown(null, args_pgUp);
			Assert.That(w.GetIndexSelected(), Is.EqualTo(0));
			w.OnMouseDown(null, args_elem0);
			Assert.That(w.GetIndexSelected(), Is.EqualTo(2));
		}
	}
}