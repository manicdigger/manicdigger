using NUnit.Framework;

namespace ManicDigger.Tests.UI
{
	[TestFixture()]
	public class CheckBoxWidgetTests
	{
		private CheckBoxWidget w;

		[SetUp]
		public void CheckBoxWidgetTest()
		{
			w = new CheckBoxWidget();
		}

		[Test()]
		public void OnMouseDownTest()
		{
			w.SetX(5);
			w.SetY(5);
			w.SetSizeX(10);
			w.SetSizeY(10);
			MouseEventArgs mouseInside = new MouseEventArgs();
			mouseInside.SetX(10);
			mouseInside.SetY(10);
			MouseEventArgs mouseOutside = new MouseEventArgs();
			mouseOutside.SetX(20);
			mouseOutside.SetY(20);

			w.OnMouseDown(null, mouseOutside);
			Assert.That(w.GetChecked(), Is.EqualTo(false));
			w.OnMouseDown(null, mouseInside);
			Assert.That(w.GetChecked(), Is.EqualTo(true));
			w.OnMouseDown(null, mouseOutside);
			Assert.That(w.GetChecked(), Is.EqualTo(true));
			w.OnMouseDown(null, mouseInside);
			Assert.That(w.GetChecked(), Is.EqualTo(false));
		}

		[Test()]
		public void SetCheckedTest()
		{
			Assert.That(w.GetChecked(), Is.EqualTo(false));
			w.SetChecked(true);
			Assert.That(w.GetChecked(), Is.EqualTo(true));
			w.SetChecked(false);
			Assert.That(w.GetChecked(), Is.EqualTo(false));
		}
	}
}