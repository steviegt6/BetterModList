﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IL.Terraria.ID;

namespace BetterModList.Common.Utilities
{
    /// <summary>
    ///     Supplier of a reflection cache system, as well as many utilities.
    /// </summary>
    public static class ReflectionUtilities
    {
        #region Cache

        public enum ReflectionType
        {
            Field,
            Property,
            Type,
            Constructor
        }

        private static Dictionary<ReflectionType, Dictionary<string, object>> ReflectionCache =>
            new Dictionary<ReflectionType, Dictionary<string, object>>
            {
                {ReflectionType.Field, new Dictionary<string, object>()},
                {ReflectionType.Property, new Dictionary<string, object>()},
                {ReflectionType.Type, new Dictionary<string, object>()},
                {ReflectionType.Constructor, new Dictionary<string, object>()}
            };

        public static BindingFlags UniversalFlags => BindingFlags.Public | BindingFlags.NonPublic |
                                                     BindingFlags.Instance | BindingFlags.Static;

        public static Type GetCachedType(this Assembly assembly, string typeName) =>
            RetrieveFromCache(ReflectionType.Type, typeName, () => assembly.GetType(typeName));

        public static FieldInfo GetCachedField(this Type type, string fieldName) =>
            RetrieveFromCache(ReflectionType.Field, GetFieldNameForCache(type, fieldName),
                () => type.GetField(fieldName, UniversalFlags));

        public static PropertyInfo GetCachedProperty(this Type type, string propertyName) =>
            RetrieveFromCache(ReflectionType.Property, GetPropertyNameForCache(type, propertyName),
                () => type.GetProperty(propertyName, UniversalFlags));

        public static ConstructorInfo GetCachedConstructor(this Type type, params Type[] types) => RetrieveFromCache(
            ReflectionType.Constructor, GetConstructorNameForCache(type, types),
            () => type.GetConstructor(UniversalFlags, null, types, null));

        public static string GetFieldNameForCache(Type type, string fieldName)
        {
            string assemblyName = type.Assembly.GetName().Name;
            string typeName = type.Name;
            return $"{assemblyName}.{typeName}.{fieldName}";
        }

        public static string GetPropertyNameForCache(Type type, string property)
        {
            string assemblyName = type.Assembly.GetName().Name;
            string typeName = type.Name;
            return $"{assemblyName}.{typeName}.{property}";
        }

        public static string GetConstructorNameForCache(Type type, params Type[] types)
        {
            string assemblyName = type.Assembly.GetName().Name;
            string typeName = type.Name;
            List<string> typeNames = types.Select(cType => cType.Name).ToList();
            return $"{assemblyName}.{typeNames}{{{string.Join(",", typeNames)}}}";
        }

        private static TReturn RetrieveFromCache<TReturn>(ReflectionType refType, string key, Func<TReturn> fallback)
        {
            if (ReflectionCache[refType].ContainsKey(key))
                return (TReturn) ReflectionCache[refType][key];

            TReturn value = fallback();
            ReflectionCache[refType].Add(key, value);
            return value;
        }

        #endregion

        public static TFieldType GetFieldValue<TType, TFieldType>(this TType obj, string field) =>
            (TFieldType) typeof(TType).GetCachedField(field)?.GetValue(obj);

        public static void SetFieldValue<TType, TFieldType>(this TType obj, string field, TFieldType value) =>
            typeof(TType).GetCachedField(field)?.SetValue(obj, value);

        public static TFieldType GetPropertyValue<TType, TFieldType>(this TType obj, string property) =>
            (TFieldType) typeof(TType).GetCachedProperty(property)?.GetValue(obj);

        public static void SetPropertyValue<TType, TFieldType>(this TType obj, string property, TFieldType value) =>
            typeof(TType).GetCachedProperty(property)?.SetValue(obj, value);

        public static FieldInfo GetCachedField<TType>(string fieldName) => typeof(TType).GetCachedField(fieldName);

        public static PropertyInfo GetCachedProperty<TType>(string propertyName) =>
            typeof(TType).GetCachedProperty(propertyName);
    }
}