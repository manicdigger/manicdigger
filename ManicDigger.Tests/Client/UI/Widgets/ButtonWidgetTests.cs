using NUnit.Framework;

namespace ManicDigger.Tests.UI
{
	[TestFixture()]
	public class ButtonWidgetTests
	{
		private ButtonWidget w;
		private MouseEventArgs mouseInside;
		private MouseEventArgs mouseOutside;

		[SetUp]
		public void ButtonWidgetTest()
		{
			w = new ButtonWidget();
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
		}

		[Test()]
		public void OnMouseDownTest()
		{
			w.OnMouseDown(null, mouseOutside);
			Assert.That(w.GetState(), Is.EqualTo(ButtonState.Normal));
			w.OnMouseDown(null, mouseInside);
			Assert.That(w.GetState(), Is.EqualTo(ButtonState.Pressed));
			w.OnMouseMove(null, mouseInside);
			w.OnMouseDown(null, mouseOutside);
			Assert.That(w.GetState(), Is.EqualTo(ButtonState.Normal));
			w.OnMouseMove(null, mouseInside);
			w.OnMouseDown(null, mouseInside);
			Assert.That(w.GetState(), Is.EqualTo(ButtonState.Pressed));
		}

		[Test()]
		public void OnMouseUpTest()
		{
			w.OnMouseUp(null, mouseOutside);
			Assert.That(w.GetState(), Is.EqualTo(ButtonState.Normal));
			w.OnMouseUp(null, mouseInside);
			Assert.That(w.GetState(), Is.EqualTo(ButtonState.Hover));
			w.OnMouseDown(null, mouseInside);
			w.OnMouseUp(null, mouseOutside);
			Assert.That(w.GetState(), Is.EqualTo(ButtonState.Normal));
			w.OnMouseDown(null, mouseInside);
			w.OnMouseUp(null, mouseInside);
			Assert.That(w.GetState(), Is.EqualTo(ButtonState.Hover));
		}

		[Test()]
		public void OnMouseMoveTest()
		{
			Assert.That(w.GetState(), Is.EqualTo(ButtonState.Normal));
			w.OnMouseMove(null, mouseOutside);
			Assert.That(w.GetState(), Is.EqualTo(ButtonState.Normal));
			w.OnMouseMove(null, mouseInside);
			Assert.That(w.GetState(), Is.EqualTo(ButtonState.Hover));
			w.OnMouseMove(null, mouseOutside);
			Assert.That(w.GetState(), Is.EqualTo(ButtonState.Normal));
		}

		[Test()]
		public void SetStateTest()
		{
			Assert.That(w.GetState(), Is.EqualTo(ButtonState.Normal));
			w.SetState(ButtonState.Normal);
			Assert.That(w.GetState(), Is.EqualTo(ButtonState.Normal));
			w.SetState(ButtonState.Hover);
			Assert.That(w.GetState(), Is.EqualTo(ButtonState.Hover));
			w.SetState(ButtonState.Pressed);
			Assert.That(w.GetState(), Is.EqualTo(ButtonState.Pressed));
		}
	}
}
