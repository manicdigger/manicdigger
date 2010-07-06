using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Remoting;
using System.Reflection;
using System.Security.Policy;
using System.Security;
using System.Security.Permissions;
using System.IO;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Diagnostics;

namespace GameModeFortress
{
    //http://msdn.microsoft.com/en-us/library/bb763046.aspx
    //The Sandboxer class needs to derive from MarshalByRefObject so that we can create it in another 
    // AppDomain and refer to it from the default AppDomain.
    class Sandboxer : MarshalByRefObject
    {
        //static string pathToUntrusted = @"..\..\..\UntrustedCode\bin\Debug";
        static string untrustedAssembly;// = "UntrustedCode";
        static string untrustedClass;// = "UntrustedCode.UntrustedClass";
        static string entryPoint;// = "IsFibonacci";
        private static Object[] parameters = { 45 };
        int tmpid;
        AppDomain newDomain;
        public void Main1(string program)
        {
            string pathToUntrusted = Path.Combine(Path.GetTempPath(), "untrusted");//AppDomain.CurrentDomain.BaseDirectory
            //Setting the AppDomainSetup. It is very important to set the ApplicationBase to a folder 
            //other than the one in which the sandboxer resides.
            AppDomainSetup adSetup = new AppDomainSetup();
            adSetup.ApplicationBase = Path.GetFullPath(pathToUntrusted);

            //Setting the permissions for the AppDomain. We give the permission to execute and to 
            //read/discover the location where the untrusted code is loaded.
            PermissionSet permSet = new PermissionSet(PermissionState.None);
            permSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));

            //We want the sandboxer assembly's strong name, so that we can add it to the full trust list.
            //StrongName fullTrustAssembly = typeof(Sandboxer).Assembly.Evidence.GetHostEvidence<StrongName>();
            
            tmpid = new Random().Next();

            //Now we have everything we need to create the AppDomain, so let's create it.
            newDomain = AppDomain.CreateDomain("Sandbox" + tmpid, null, adSetup, permSet,
                CreateStrongName(typeof(Sandboxer).Assembly));

            //Use CreateInstanceFrom to load an instance of the Sandboxer class into the
            //new AppDomain. 
            handle = Activator.CreateInstanceFrom(
                newDomain, typeof(Sandboxer).Assembly.ManifestModule.FullyQualifiedName,
                typeof(Sandboxer).FullName
                );
                        
            var param = new CompilerParameters();
            param.OutputAssembly = Path.Combine(pathToUntrusted, string.Format("Tmp{0}.dll", tmpid));
            param.GenerateExecutable = false;
            param.GenerateInMemory = false;
            //param.CompilerOptions = "/optimize";
            //param.IncludeDebugInformation = true;
            param.TempFiles.KeepFiles = true;
            // param.ReferencedAssemblies.Add("Scripting.dll");
            if (!Directory.Exists(pathToUntrusted))
            {
                Directory.CreateDirectory(pathToUntrusted);
            }
            if (File.Exists(param.OutputAssembly))
            {
                File.Delete(param.OutputAssembly);
            }

            string s = program;
            CodeDomProvider u = CSharpCodeProvider.CreateProvider(CSharpCodeProvider.GetLanguageFromExtension("cs"));
            CompilerResults r = u.CompileAssemblyFromSource(param, new string[] { s });
            if (r.Errors.HasErrors)
            {
                throw new Exception("Compilation error.");
            }
            //sandbox.Load(File.ReadAllBytes("Scripting.dll"));

            string loc = r.CompiledAssembly.Location;
            untrustedAssembly = loc;
        }
        ObjectHandle handle;
        bool loaded = false;

        Assembly assembly;
        object targetinstance;
        Dictionary<string, MethodInfo> target = new Dictionary<string, MethodInfo>();
        public object ExecuteUntrustedCode(string assemblyName, string typeName, string entryPoint, Object[] parameters)
        {
            if (targetinstance == null)
            {
                //Load the MethodInfo for a method in the new Assembly. This might be a method you know, or 
                //you can use Assembly.EntryPoint to get to the main function in an executable.
                assembly = Assembly.Load(assemblyName);//Assembly.Load(assemblyName,);
                Type type = assembly.GetType(typeName);
                targetinstance = Activator.CreateInstance(type);
            }
            if (!target.ContainsKey(entryPoint))
            {
                Type type = assembly.GetType(typeName);
                target[entryPoint] = type.GetMethod(entryPoint);
            }
            try
            {
                //Now invoke the method.
                //bool retVal = (bool)target.Invoke(Activator.CreateInstance(type), parameters);
                return target[entryPoint].Invoke(targetinstance, parameters);
            }
            catch (Exception ex)
            {
                // When we print informations from a SecurityException extra information can be printed if we are 
                //calling it with a full-trust stack.
                (new PermissionSet(PermissionState.Unrestricted)).Assert();
                Console.WriteLine("SecurityException caught:\n{0}", ex.ToString());
                CodeAccessPermission.RevertAssert();
                Console.ReadLine();
                throw new Exception();
            }
        }
        /// <summary>
        /// Create a StrongName that matches a specific assembly
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// if <paramref name="assembly"/> is null
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// if <paramref name="assembly"/> does not represent a strongly named assembly
        /// </exception>
        /// <param name="assembly">Assembly to create a StrongName for</param>
        /// <returns>A StrongName that matches the given assembly</returns>
        public static StrongName CreateStrongName(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            AssemblyName assemblyName = assembly.GetName();
            Debug.Assert(assemblyName != null, "Could not get assembly name");

            // get the public key blob
            byte[] publicKey = assemblyName.GetPublicKey();
            if (publicKey == null || publicKey.Length == 0)
                throw new InvalidOperationException("Assembly is not strongly named");

            StrongNamePublicKeyBlob keyBlob = new StrongNamePublicKeyBlob(publicKey);

            // and create the StrongName
            return new StrongName(keyBlob, assemblyName.Name, assemblyName.Version);
        }
        public object Call(string type, string method, object[] args)
        {
            untrustedClass = type;
            entryPoint = method;
            parameters = args;

            //Unwrap the new domain instance into a reference in this domain and use it to execute the 
            //untrusted code.
            Sandboxer newDomainInstance = (Sandboxer)handle.Unwrap();
            return newDomainInstance.ExecuteUntrustedCode("Tmp" + tmpid, untrustedClass, entryPoint, parameters);
            //return newDomainInstance.Call(args);
        }
        public void Dispose()
        {
            if (newDomain != null)
            {
                AppDomain.Unload(newDomain);
            }
        }
        //http://www.dotnet247.com/247reference/msgs/13/66416.aspx
        public override object InitializeLifetimeService()
        {
            //This is to insure that when created as a Singleton,
            //the instance never dies, no matter how long between client calls.
            return null;
        }
    }
}
