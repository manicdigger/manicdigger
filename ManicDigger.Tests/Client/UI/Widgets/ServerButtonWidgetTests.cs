using NUnit.Framework;

namespace ManicDigger.Tests.UI
{
	[TestFixture()]
	public class ServerButtonWidgetTests
	{
		ServerButtonWidget w;

		[SetUp]
		public void ServerButtonWidgetTest()
		{
			w = new ServerButtonWidget();
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

			Assert.That(w.GetFocused(), Is.EqualTo(false));
			w.OnMouseDown(null, mouseOutside);
			Assert.That(w.GetFocused(), Is.EqualTo(false));
			w.OnMouseDown(null, mouseInside);
			Assert.That(w.GetFocused(), Is.EqualTo(true));
			w.OnMouseDown(null, mouseOutside);
			Assert.That(w.GetFocused(), Is.EqualTo(false));
		}

		[Test()]
		public void SetTextHeadingTest()
		{
			string val = "test";
			Assert.That(w.GetTextHeading(), Is.EqualTo(null));
			w.SetTextHeading(val);
			Assert.That(w.GetTextHeading(), Is.EqualTo(val));
			w.SetTextHeading("");
			Assert.That(w.GetTextHeading(), Is.EqualTo(""));
			w.SetTextHeading(null);
			Assert.That(w.GetTextHeading(), Is.EqualTo(null));
		}

		[Test()]
		public void SetTextGamemodeTest()
		{
			string val = "test";
			Assert.That(w.GetTextGamemode(), Is.EqualTo(null));
			w.SetTextGamemode(val);
			Assert.That(w.GetTextGamemode(), Is.EqualTo(val));
			w.SetTextGamemode("");
			Assert.That(w.GetTextGamemode(), Is.EqualTo(""));
			w.SetTextGamemode(null);
			Assert.That(w.GetTextGamemode(), Is.EqualTo(null));
		}

		[Test()]
		public void SetTextPlayercountTest()
		{
			string val = "test";
			Assert.That(w.GetTextPlayercount(), Is.EqualTo(null));
			w.SetTextPlayercount(val);
			Assert.That(w.GetTextPlayercount(), Is.EqualTo(val));
			w.SetTextPlayercount("");
			Assert.That(w.GetTextPlayercount(), Is.EqualTo(""));
			w.SetTextPlayercount(null);
			Assert.That(w.GetTextPlayercount(), Is.EqualTo(null));
		}

		[Test()]
		public void SetTextDescriptionTest()
		{
			string val = "test";
			Assert.That(w.GetTextDescription(), Is.EqualTo(null));
			w.SetTextDescription(val);
			Assert.That(w.GetTextDescription(), Is.EqualTo(val));
			w.SetTextDescription("");
			Assert.That(w.GetTextDescription(), Is.EqualTo(""));
			w.SetTextDescription(null);
			Assert.That(w.GetTextDescription(), Is.EqualTo(null));
		}

		[Test()]
		public void SetThumbnailTest()
		{
			const string defaultVal = "serverlist_entry_noimage.png";
			string val = "test";
			Assert.That(w.GetThumbnail(), Is.EqualTo(defaultVal));
			w.SetThumbnail(val);
			Assert.That(w.GetThumbnail(), Is.EqualTo(val));
			w.SetThumbnail("");
			Assert.That(w.GetThumbnail(), Is.EqualTo(defaultVal));
			w.SetThumbnail(null);
			Assert.That(w.GetThumbnail(), Is.EqualTo(defaultVal));
		}

		[Test()]
		public void SetErrorConnectTest()
		{
			Assert.That(w.GetErrorConnect(), Is.EqualTo(false));
			w.SetErrorConnect(true);
			Assert.That(w.GetErrorConnect(), Is.EqualTo(true));
			w.SetErrorConnect(false);
			Assert.That(w.GetErrorConnect(), Is.EqualTo(false));
		}

		[Test()]
		public void SetErrorVersionTest()
		{
			Assert.That(w.GetErrorVersion(), Is.EqualTo(false));
			w.SetErrorVersion(true);
			Assert.That(w.GetErrorVersion(), Is.EqualTo(true));
			w.SetErrorVersion(false);
			Assert.That(w.GetErrorVersion(), Is.EqualTo(false));
		}
	}
}