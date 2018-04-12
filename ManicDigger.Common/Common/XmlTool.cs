using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

namespace ManicDigger.Common
{
	/// <summary>
	/// Provides basic XML operations
	/// </summary>
	public class XmlTool
	{
		public static string XmlVal(XmlDocument d, string path)
		{
			XPathNavigator navigator = d.CreateNavigator();
			XPathNodeIterator iterator = navigator.Select(path);
			foreach (XPathNavigator n in iterator)
			{
				return n.Value;
			}
			return null;
		}
		public static IEnumerable<string> XmlVals(XmlDocument d, string path)
		{
			XPathNavigator navigator = d.CreateNavigator();
			XPathNodeIterator iterator = navigator.Select(path);
			foreach (XPathNavigator n in iterator)
			{
				yield return n.Value;
			}
		}
		public static string X(string name, string value)
		{
			return string.Format("<{0}>{1}</{0}>", name, value);
		}
	}
}
