using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Input;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Diagnostics;

namespace ManicDigger.Hud
{
    public interface IHUD
    {
        IServiceProvider ServiceProvider { get; }
        bool Enabled { get; set; }
        bool Visible { get; set; }
        int Width { get; set; }
        int Height { get; set; }

        void AddComponent(IHUDComponent component);
        IHUDComponent GetComponent(Type type);
        IHUDComponent GetComponent(string name);
        void RemoveComponent(IHUDComponent component);
        void LoadLayout(Stream stream);
        void SaveLayout(Stream stream);

        void Initialize();
        void OnResize(int width, int height);
        void OnKeyDown(KeyboardKeyEventArgs e);
        void OnKeyUp(KeyboardKeyEventArgs e);
        void Update(double delta);
        void Render();
    }
    public interface IHUDComponent
    {
        IHUD HUD { get; }
        string Name { get; }
        bool Enabled { get; set; }
        bool Visible { get; set; }
        float X { get; set; }
        float Y { get; set; }
        //Components with low orders will be drawn before components with high orders.
        int Order { get; set; }

        void OnAttach(IHUD hud);
        void OnDetach();
        void OnSaveLayout(IDictionary<string, string> data);
        void OnLoadLayout(IDictionary<string, string> data);
        void OnResize(int width, int height);
        void OnKeyDown(KeyboardKeyEventArgs e);
        void OnKeyUp(KeyboardKeyEventArgs e);

        void Initialize();
        void Update(double delta);
        void Render();
    }
    public interface ISizableHUDComponent
    {
        float Width { get; set; }
        float Height { get; set; }
        bool IsResizable { get; }
        bool IsMovable { get; }
    }
    public class HUDLayout
    {
        public List<HUDComponent> Components { get { return components; } set { components = value; } }
        List<HUDComponent> components = new List<HUDComponent>();
    }
    public class HUDComponent
    {
        public string Id { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public bool Visible { get; set; }
        public List<string> Data { get { return data; } set { data = value; } }
        List<string> data = new List<string>();
    }
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class GUIComponentAttribute : Attribute
    {
        public GUIComponentAttribute(string id)
        {
            this.Id = id;
        }
        //used in Xml layout.
        public string Id { get; private set; }
    }
    public class HUD : IHUD
    {
        private readonly string HUDFileName;
        private List<IHUDComponent> _components;
        private bool _isInitialized;

        public IServiceProvider ServiceProvider { get; private set; }

        private HUD()
        {
            HUDFileName = Path.Combine(Extensions.GetWorkingDirectory(Assembly.GetExecutingAssembly()), "data\\menus\\hud.xml");
            _components = new List<IHUDComponent>();
        }
        public HUD(IServiceProvider serviceProvider, int screenWidth, int screenHeight)
            : this()
        {
            ServiceProvider = serviceProvider;
            Width = screenWidth;
            Height = screenHeight;

            if (File.Exists(HUDFileName))
            {
                this.LoadLayout(File.Open(HUDFileName, FileMode.Open));
            }
            else
            {
                Trace.WriteLine("Could not find layout file 'data/menus/hud.xml'. HUD will be empty.");
            }
        }

        public bool Enabled { get; set; }
        public bool Visible { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public void AddComponent(IHUDComponent component)
        {
            if (!_components.Contains(component))
            {
                _components.Add(component);
                // sort the components via their Orders
                _components.Sort(CompareComponentOrder);
                // then attach it
                component.OnAttach(this);
            }
        }
        private int CompareComponentOrder(IHUDComponent left, IHUDComponent right)
        {
            if (left.Order < right.Order) { return left.Order - right.Order; }
            if (left.Order == right.Order) { return 0; }
            if (left.Order > right.Order) { return left.Order - right.Order; }
            return 0;
        }
        public IHUDComponent GetComponent(Type type)
        {
            return _components.Find(c => c.GetType() == type);
        }
        public IHUDComponent GetComponent(string name)
        {
            return _components.Find(c => string.Equals(c.Name, name, StringComparison.InvariantCultureIgnoreCase));
        }
        public void RemoveComponent(IHUDComponent component)
        {
            if (_components.Contains(component))
            {
                _components.Remove(component);
                component.OnDetach();
            }
        }
        public void LoadLayout(Stream stream)
        {
            HUDLayoutHelper.LoadLayout(stream, this, Width, Height);
        }
        public void SaveLayout(Stream stream)
        {
            HUDLayoutHelper.SaveLayout(stream, this);
        }
        public void Initialize()
        {
            for (int i = 0; i < _components.Count; i++)
            {
                _components[i].Initialize();
            }

            // enable the hud
            this.Enabled = true;
            this.Visible = true;
        }
        public void OnResize(int width, int height)
        {
            bool dimensionsChanged = false;
            if (width != this.Width)
            {
                dimensionsChanged = true;
                Width = width;
            }
            if (height != this.Height)
            {
                dimensionsChanged = true;
                Height = height;
            }

            if (dimensionsChanged)
            {
                // update from lowest order to highest
                for (int i = 0; i < _components.Count; i++)
                {
                    IHUDComponent c = _components[i];
                    c.OnResize(width, height);
                }
            }
        }
        public void OnKeyDown(KeyboardKeyEventArgs e)
        {
            // update from lowest order to highest
            for (int i = 0; i < _components.Count; i++)
            {
                IHUDComponent c = _components[i];
                c.OnKeyDown(e);
            }
        }
        public void OnKeyUp(KeyboardKeyEventArgs e)
        {
            // update from lowest order to highest
            for (int i = 0; i < _components.Count; i++)
            {
                IHUDComponent c = _components[i];
                c.OnKeyUp(e);
            }
        }
        public void Update(double delta)
        {
            if (!_isInitialized)
            {
                this.Initialize();
                _isInitialized = true;
            }
            if (!Enabled)
            {
                return;
            }

            // update from lowest order to highest
            for (int i = 0; i < _components.Count; i++)
            {
                IHUDComponent c = _components[i];
                if (c.Enabled)
                {
                    c.Update(delta);
                }
            }
        }
        public void Render()
        {
            if (!Enabled || !Visible || !_isInitialized)
            {
                return;
            }

            // render from lowest order to highest
            for (int i = 0; i < _components.Count; i++)
            {
                IHUDComponent c = _components[i];
                if (c.Visible)
                {
                    c.Render();
                }
            }
        }
    }
    public abstract class HUDComponentBase : IHUDComponent
    {
        public abstract string Name { get; }
        public IHUD HUD { get; private set; }
        public bool Enabled { get; set; }
        public bool Visible { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public int Order { get; set; }
        public virtual void OnAttach(IHUD hud)
        {
            this.HUD = hud;
        }
        public virtual void OnDetach()
        {
            this.HUD = null;
        }
        public virtual void OnSaveLayout(IDictionary<string, string> data) { }
        public virtual void OnLoadLayout(IDictionary<string, string> data) { }
        public virtual void OnResize(int width, int height) { }
        public virtual void OnKeyDown(KeyboardKeyEventArgs e) { }
        public virtual void OnKeyUp(KeyboardKeyEventArgs e) { }
        public virtual void Update(double delta) { }
        public virtual void Initialize() { }
        public abstract void Render();
    }
    /// <summary>
    /// Helper that loads and saves the HUD layout.
    /// </summary>
    static class HUDLayoutHelper
    {
        private static Dictionary<string, Type> _mapping;

        static HUDLayoutHelper()
        {
            _mapping = new Dictionary<string, Type>();
            foreach (var type in TypeManager.Instance.FindImplementers(typeof(IHUDComponent), false))
            {
                GUIComponentAttribute[] attribs = (GUIComponentAttribute[])type.GetCustomAttributes(typeof(GUIComponentAttribute), false);
                if (attribs.Length != 1)
                {
                    // invalid type usage
                    continue;
                }

                if (!_mapping.ContainsKey(attribs[0].Id))
                {
                    _mapping.Add(attribs[0].Id, type);
                }
                else
                {
                    // invalid Id usage!
                }
            }
        }
        public static void LoadLayout(Stream stream, IHUD hud, int screenWidth, int screenHeight)
        {
            HUDLayout layout = (HUDLayout)Serializers.XmlDeserialize(stream, typeof(HUDLayout));
            foreach (HUDComponent clayout in layout.Components)
            {
                Dictionary<string, string> data = new Dictionary<string, string>(clayout.Data.Count);
                foreach (string dataItem in clayout.Data.FindAll(di => !string.IsNullOrEmpty(di)))
                {

                    string[] tokens = dataItem.Split('=');
                    if (!data.ContainsKey(tokens[0]))
                    {
                        data.Add(tokens[0], tokens[1]);
                    }
                }

                // parse the element
                // if the element doesn't exist quit right here
                if (!_mapping.ContainsKey(clayout.Id))
                {
                    // error, id not found
                    System.Diagnostics.Trace.TraceWarning("No IHUDComponent found with id {0}! Ignoring this entry.", clayout.Id);
                    continue;
                }

                // create this hud component and add it
                IHUDComponent component = (IHUDComponent)Activator.CreateInstance(_mapping[clayout.Id], false);
                component.X = GetAbsX(screenWidth, clayout.X);
                component.Y = GetAbsY(screenHeight, clayout.Y);
                component.Enabled = true;
                component.Visible = clayout.Visible;
                component.OnLoadLayout(data);
                hud.AddComponent(component);
            }
        }
        public static void SaveLayout(Stream stream, IHUD hud)
        {

            //XDocument document = new XDocument();
            //document.Add(new XElement("Layout"));

            ////foreach(IHUDComponent component in hud.

            //// currently only components are supported... more may be to come
            //foreach (var componentE in document.Root.Elements("Component"))
            //{
            //    string id = null;
            //    float posX = 0;
            //    float posY = 0;
            //    bool visible = true;
            //    Dictionary<string, string> componentData = new Dictionary<string, string>(4);

            //    foreach (var attribute in componentE.Attributes())
            //    {
            //        switch (attribute.Name.LocalName)
            //        {
            //            case "Id":
            //                id = attribute.Value;
            //                break;
            //            case "X":
            //                // CultureInfo.InvariantCulture to ensure that a ',' stays a ',' and not a '.' (german: '.' vs. english: ',' and vice versa)!
            //                float.TryParse(attribute.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out posX);
            //                break;
            //            case "Y":
            //                // CultureInfo.InvariantCulture to ensure that a ',' stays a ',' and not a '.' (german: '.' vs. english: ',' and vice versa)!
            //                float.TryParse(attribute.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out posY);
            //                break;
            //            case "Visible":
            //                bool.TryParse(attribute.Value, out visible);
            //                break;
            //            default:
            //                // unrecognized attributes are put into the data dictionary
            //                componentData.Add(attribute.Name.LocalName, attribute.Value);
            //                break;
            //        }
            //    }

            //    // parse the element
            //    // if the element doesn't exist quit right here
            //    if (!_mapping.ContainsKey(id))
            //    {
            //        // error, id not found
            //        System.Diagnostics.Trace.WriteLine("Error: No IHudComponent found with id "   id   "!");
            //        continue;
            //    }

            //    // create this hud component and add it
            //    IHUDComponent component = (IHUDComponent)Activator.CreateInstance(_mapping[id], false);
            //    // TODO: these values need to be put into percentual coordinates
            //    component.X = posX;
            //    component.Y = posY;
            //    component.Visible = visible;
            //    component.OnLoadLayout(componentData);
            //    hud.AddComponent(component);
            //}
        }
        public static float GetAbsX(int screenWidth, float percentualX)
        {
            return (float)((percentualX / 100) * screenWidth);
        }
        public static float GetAbsY(int screenHeight, float percentualY)
        {
            return (float)((percentualY / 100) * screenHeight);
        }
        public static float GetRelX(int screenWidth, float absoluteX)
        {
            return (float)((absoluteX * 100) / screenWidth);
        }
        public static float GetRelY(int screenHeight, float absoluteY)
        {
            return (float)((absoluteY * 100) / screenHeight);
        }
    }
}
