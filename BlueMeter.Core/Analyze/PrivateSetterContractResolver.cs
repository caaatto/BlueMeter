using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BlueMeter.Core.Analyze
{
    internal class PrivateSetterContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization ms)
        {
            var prop = base.CreateProperty(member, ms);
            if (!prop.Writable)
            {
                var pi = member as PropertyInfo;
                if (pi?.GetSetMethod(true) != null)
                {
                    prop.Writable = true;
                }
            }
            return prop;
        }
    }
}
