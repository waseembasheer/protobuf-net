using System;
using System.Text;
using System.Reflection;

namespace ProtoBuf.Meta
{
    class TTDUtils
    {
        public static string ExtractGenericTypeName(Type type, RuntimeTypeModel model)
        {
            string typeName = type.Name;

            StringBuilder sb = new StringBuilder(typeName);
            int split = typeName.IndexOf('`');
            if (split >= 0) sb.Length = split;

            sb.Append('<');

            var genericArguments = type
#if WINRT || COREFX || PROFILE259
                    .GetTypeInfo().GenericTypeArguments
#else
                    .GetGenericArguments()
#endif
                ;
            for (int i = 0; i < genericArguments.Length; i++)
            {
                var arg = genericArguments[i];

                Type tmp = arg;
                int key = model.GetKey(ref tmp);
                MetaType mt;
                if (key >= 0 && (mt = model[tmp]) != null && !mt.HasSurrogate) // <=== need to exclude surrogate to avoid chance of infinite loop
                {

                    sb.Append(mt.GetSchemaTypeName());
                }
                else
                {
                    if (tmp
#if WINRT || COREFX || PROFILE259
                        .GetTypeInfo()
#endif
                        .IsGenericType)
                    {
                        // Nested generic type.
                        string result = TTDUtils.ExtractGenericTypeName(tmp, model);

                        sb.Append(result);
                    }
                    else
                    {
                        RuntimeTypeModel.CommonImports ci = RuntimeTypeModel.CommonImports.None;
                        sb.Append(model.GetSchemaTypeName(tmp, DataFormat.Default, false, false, ref ci));
                    }
                }

                if (i != (genericArguments.Length - 1))
                {
                    sb.Append(',');
                }
            }

            sb.Append('>');

            return sb.ToString();
        }

        public static bool IsTTDCollectionType(Type t)
        {
            return t
#if WINRT || COREFX || PROFILE259
            .GetTypeInfo()
#endif
            .IsGenericType
            && (IsQueueOrStack(t) || IsListHashSetOrDictionary(t) || IsTTDGenericCollectionWithBase(t));
        }

        public static bool IsNullableType(Type t)
        {
#if NO_GENERICS
            return false; // never a Nullable<T>, so always returns false
#else
            return Nullable.GetUnderlyingType(t) != null;
#endif
        }

        public static bool IsQueueOrStack(Type t)
        {
            string name = t.Name;
            return name != null && (
                       name.StartsWith("Queue") || 
                       name.StartsWith("Stack"));
        }

        public static bool IsListHashSetOrDictionary(Type t)
        {
            string name = t.Name;
            return name != null && (
                       name.StartsWith("List") ||
                       name.StartsWith("HashSet") || 
                       name.StartsWith("Dictionary") || 
                       name.StartsWith("IDictionary"));
        }

        public static bool IsTTDGenericCollectionWithBase(Type t)
        {
            string name = t.Name;
            return name != null && (
                       name.StartsWith("SerializableDefaultDictionary") ||
                       name.StartsWith("SerializableDictionary") ||
                       name.StartsWith("SerializableHashSet") ||
                       name.StartsWith("SortedKeyedSequence") ||
                       name.StartsWith("AdjustmentsCacheEntity") ||
                       name.StartsWith("KeyHashIndex") ||
                       name.StartsWith("PermissionSet")) &&
                   !name.EndsWith("Base");
        }
    }
}
