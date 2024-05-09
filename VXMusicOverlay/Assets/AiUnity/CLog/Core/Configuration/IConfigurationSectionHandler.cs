#if AIUNITY_CODE

using System;
using AiUnity.CLog.Core.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace AiUnity.CLog.Core.Configuration
{
    public interface IConfigurationSectionHandler
    {
        object Create(object parent, object configContext, XmlNode section);
    }
}
#endif
