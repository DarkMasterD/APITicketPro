using System.Reflection;
using System.Runtime.Loader;

namespace APITicketPro.Helpers
{
    public class CustomAssemblyLoadContext : AssemblyLoadContext
    {
        public IntPtr LoadUnmanagedLibrary(string path)
        {
            return LoadUnmanagedDllFromPath(path);
        }

        protected override Assembly Load(AssemblyName assemblyName) => null;
    }
}
