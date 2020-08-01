using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Win32;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedType.Global

namespace Rustle.fmLuWorks.Utilities.Core
{
    public static class ReflectionHelper
    {
        private static void SetGetOnlyProperty(
            object obj,
            string propertyName,
            object newValue)
        {
            var type = obj.GetType();
            var field = type
                .GetRuntimeFields()
                .FirstOrDefault(
                    a => Regex.IsMatch(
                        a.Name,
                        $"\\A<{propertyName}>k__BackingField\\Z"));
            Debug.Assert(field != null);
            field.SetValue(obj, newValue);
        }

        public static IEnumerable<T> GetPrivateInstanceValues<T>(
        this object obj,
           Predicate<T>? predicate = null)
        {
            return from v in (from p in
                        obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic)
                              where p.PropertyType == typeof(T)
                              select (T)p.GetValue(obj, null)).ToArray()
                   where predicate == null || predicate.Invoke(v)
                   select v;
        }


        public static T GetPrivateInstanceValue<T>(
            this object obj,
            Predicate<T>? predicate = null)
        {
            return obj.GetPrivateInstanceValues(predicate).FirstOrDefault();
        }
    }
}