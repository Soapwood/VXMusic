#if AIUNITY_CODE

using System;
using AiUnity.CLog.Core.Common;
using AiUnity.Common.Extensions;
using System.Collections.Generic;
using AiUnity.CLog.Core.Configuration;
using System.Reflection;
using System.ComponentModel;
using AiUnity.Common.InternalLog;
using System.Linq;
using AiUnity.Common.Log;

namespace AiUnity.CLog.Core.Loggers
{
    public abstract class ExternalLoggerFactoryAdapter : CacheLoggerFactoryAdapter
    {
        protected ILogManager nlogManager;

        public ExternalLoggerFactoryAdapter(Dictionary<string, string> properties, string assemblyName, string nameSpace) : base(properties)
        {
            IEnumerable<string> searchAssemblyNames = new List<string>() { "Assembly-CSharp", assemblyName };
            IEnumerable<Assembly> searchAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => searchAssemblyNames.Any(t => a.FullName.StartsWith(t)));
            IEnumerable<Type> logManagerTypes = searchAssemblies.SelectMany(a => a.GetTypes()).Where(t => typeof(ILogManager).IsAssignableFrom(t) && t.IsClass);
            Type nlogManagerType = logManagerTypes.FirstOrDefault(t => t.Namespace != null && t.Namespace.Contains(nameSpace));

            if (nlogManagerType != null) {
                //ILogManager nlogManager = (ILogManager)Activator.CreateInstance(nlogManagerType);
                PropertyInfo propertyInfo = nlogManagerType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                if (propertyInfo != null) {
                    nlogManager = propertyInfo.GetValue(null, null) as ILogManager;
                }
            }
        }

    }
}
#endif
