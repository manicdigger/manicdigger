using NUnit.Framework;

namespace ManicDigger.Tests.UI
{
	[TestFixture()]
	public class TextWidgetTests
	{
		TextWidget w;

		[SetUp()]
		public void TextWidgetTest()
		{
			w = new TextWidget();
		}

		[Test()]
		public void SetAlignmentTest()
		{
			Assert.That(w.GetAlignment(), Is.EqualTo(TextAlign.Left));
			w.SetAlignment(TextAlign.Center);
			Assert.That(w.GetAlignment(), Is.EqualTo(TextAlign.Center));
			w.SetAlignment(TextAlign.Right);
			Assert.That(w.GetAlignment(), Is.EqualTo(TextAlign.Right));
			w.SetAlignment(TextAlign.Left);
			Assert.That(w.GetAlignment(), Is.EqualTo(TextAlign.Left));
		}

		[Test()]
		public void SetBaselineTest()
		{
			Assert.That(w.GetBaseline(), Is.EqualTo(TextBaseline.Top));
			w.SetBaseline(TextBaseline.Middle);
			Assert.That(w.GetBaseline(), Is.EqualTo(TextBaseline.Middle));
			w.SetBaseline(TextBaseline.Bottom);
			Assert.That(w.GetBaseline(), Is.EqualTo(TextBaseline.Bottom));
			w.SetBaseline(TextBaseline.Top);
			Assert.That(w.GetBaseline(), Is.EqualTo(TextBaseline.Top));
		}

		[Test()]
		public void SetFontTest()
		{
			FontCi val = new FontCi();
			Assert.That(w.GetFont(), Is.EqualTo(null));
			w.SetFont(null);
			Assert.That(w.GetFont(), Is.EqualTo(null));
			w.SetFont(val);
			Assert.That(w.GetFont(), Is.EqualTo(val));
			w.SetFont(null);
			Assert.That(w.GetFont(), Is.EqualTo(val));
		}

		[Test()]
		public void SetXTest()
		{
			int val = -10;
			Assert.That(w.GetX(), Is.EqualTo(0));
			w.SetX(val);
			Assert.That(w.GetX(), Is.EqualTo(val));
			w.SetX(0);
			Assert.That(w.GetX(), Is.EqualTo(0));
		}

		[Test()]
		public void SetYTest()
		{
			int val = -10;
			Assert.That(w.GetY(), Is.EqualTo(0));
			w.SetY(val);
			Assert.That(w.GetY(), Is.EqualTo(val));
			w.SetY(0);
			Assert.That(w.GetY(), Is.EqualTo(0));
		}

		[Test()]
		public void SetTextTest()
		{
			string val = "test";
			Assert.That(w.GetEventName(), Is.EqualTo(null));
			w.SetEventName(val);
			Assert.That(w.GetEventName(), Is.EqualTo(val));
			w.SetEventName(null);
			Assert.That(w.GetEventName(), Is.EqualTo(null));
		}
	}
}