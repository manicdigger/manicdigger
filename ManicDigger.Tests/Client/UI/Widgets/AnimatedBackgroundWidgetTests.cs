using NUnit.Framework;

namespace ManicDigger.Tests.UI
{
	[TestFixture()]
	public class AnimatedBackgroundWidgetTests
	{
		private AnimatedBackgroundWidget w;

		[SetUp]
		public void AnimatedBackgroundWidgetTest()
		{
			w = new AnimatedBackgroundWidget();
		}

		[Test()]
		public void InitTest()
		{
			w.Init(null, 10, 20);
			Assert.That(w.GetSizeX(), Is.EqualTo(10));
			Assert.That(w.GetSizeY(), Is.EqualTo(20));
		}
	}
}
