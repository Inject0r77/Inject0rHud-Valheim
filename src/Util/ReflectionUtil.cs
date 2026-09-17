using System;
using System.Reflection;

namespace Inject0rHUD.Util
{
    internal static class ReflectionUtil
    {
        private const BindingFlags InstanceFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private const BindingFlags StaticFlags =
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        internal static FieldInfo FindInstanceField(Type type, string name)
        {
            for (Type t = type; t != null; t = t.BaseType)
            {
                FieldInfo f = t.GetField(name, InstanceFlags);
                if (f != null) return f;
            }
            return null;
        }

        internal static FieldInfo FindStaticField(Type type, string name)
        {
            for (Type t = type; t != null; t = t.BaseType)
            {
                FieldInfo f = t.GetField(name, StaticFlags);
                if (f != null) return f;
            }
            return null;
        }

        internal static MethodInfo FindMethod(Type type, string name, params Type[] parameterTypes)
        {
            for (Type t = type; t != null; t = t.BaseType)
            {
                MethodInfo m = t.GetMethod(name,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null, parameterTypes, null);
                if (m != null) return m;
            }
            return null;
        }

        internal static float ReadFloat(object instance, string fieldName, float fallback = 0f)
        {
            if (instance == null) return fallback;
            FieldInfo f = FindInstanceField(instance.GetType(), fieldName);
            if (f == null) return fallback;

            object v = f.GetValue(instance);
            return v is float ? (float)v : fallback;
        }

        internal static string ReadString(object instance, string fieldName, string fallback = "")
        {
            if (instance == null) return fallback;
            FieldInfo f = FindInstanceField(instance.GetType(), fieldName);
            if (f == null) return fallback;

            object v = f.GetValue(instance);
            return v as string ?? fallback;
        }
    }
}
