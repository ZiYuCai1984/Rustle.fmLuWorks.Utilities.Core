using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Win32;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedType.Global

namespace Rustle.fmLuWorks.Utilities.Core
{
    public static class RegistryHelper
    {
        private static RegistryKey OpenBaseKey(
            RegistryHive registryHive = RegistryHive.CurrentUser)
        {
            var baseKey = RegistryKey.OpenBaseKey(
                registryHive,
                Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32);

            Debug.Assert(baseKey != null);
            return baseKey;
        }

        private static void RemoveKey(
            string keyPath,
            RegistryHive registryHive = RegistryHive.CurrentUser)
        {
            using var baseKey = OpenBaseKey(registryHive);
            baseKey.DeleteSubKeyTree(keyPath);
        }

        /// <summary>
        ///     Set value to registry
        /// </summary>
        /// <param name="keyPath">key path</param>
        /// <param name="name">name of sub key</param>
        /// <param name="value">value</param>
        /// <param name="kind">RegistryValueKind</param>
        public static void SetValue(
            string keyPath,
            string name,
            object value,
            RegistryValueKind kind = RegistryValueKind.String)
        {
            using var key = EnsureKeyExist(keyPath);
            key.SetValue(name, value, kind);
        }

        public static string[] GetChildKeyNames(string keyPath, RegistryHive registryHive)
        {
            using var baseKey = OpenBaseKey(registryHive);

            using var key = baseKey.OpenSubKey(keyPath);
            return key.SubKeyCount == 0
                ? new string[0]
                : key.GetSubKeyNames();
        }

        /// <summary>
        ///     Get boolean value from registry
        /// </summary>
        /// <param name="keyPath">key path</param>
        /// <param name="name">name of sub key</param>
        /// <param name="defaultValue">defaultValue</param>
        /// <param name="createIfNotExist">create key and sub key if not exist</param>
        /// <returns>boolean value</returns>
        public static bool? GetBooleanValue(
            string keyPath,
            string name,
            string defaultValue = "",
            bool createIfNotExist = true)
        {
            var value = GetValue(keyPath, name, defaultValue, createIfNotExist);
            if (value == null)
            {
                return null;
            }

            if (bool.TryParse(value, out var result))
            {
                return result;
            }

            return null;
        }

        public static string? GetValue(
            string keyPath,
            string name,
            string defaultValue = "",
            bool createIfNotExist = true,
            RegistryHive registryHive = RegistryHive.CurrentUser)
        {
            if (createIfNotExist)
            {
                if (!string.IsNullOrEmpty(defaultValue))
                {
                    using var key = EnsureKeyExist(keyPath);
                    return EnsureItemExist(key, name, defaultValue);
                }
                else
                {
                    using var baseKey = OpenBaseKey(registryHive);
                    var key = baseKey.OpenSubKey(keyPath);
                    return key?.GetValue(name)?.ToString();
                }
            }

            {
                using var baseKey = OpenBaseKey(registryHive);
                var key = baseKey.OpenSubKey(keyPath);
                if (key == null)
                {
                    return defaultValue;
                }

                var value = key.GetValue(name);

                return value == null ? defaultValue : value.ToString();
            }
        }

        private static RegistryKey EnsureKeyExist(
            string keyPath,
            RegistryHive registryHive = RegistryHive.CurrentUser)
        {
            using var baseKey = OpenBaseKey(registryHive);
            var key = baseKey.OpenSubKey(keyPath, true);

            if (key != null)
            {
                return key;
            }

            key = CreateKey(baseKey, keyPath);

            if (key == null)
            {
                throw new InvalidOperationException($"create {keyPath} fail ");
            }

            return key;
        }

        private static string EnsureItemExist(
            RegistryKey key,
            string name,
            string defaultValue = "",
            RegistryValueKind kind = RegistryValueKind.String)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (!key.GetValueNames().Contains(name))
            {
                key.SetValue(name, defaultValue, kind);
            }

            var value = key.GetValue(name);
            if (value == null)
            {
                throw new NullReferenceException(nameof(value));
            }

            return value.ToString()!;
        }


        private static RegistryKey CreateKey(RegistryKey key, string subKeyName, int index = 0)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            while (true)
            {
                var strs = subKeyName.MergeSpace().Split('\\');
                if (index == strs.Length)
                {
                    if (key != null)
                    {
                        return key;
                    }
                }

                var s = strs[index];

                if (key != null)
                {
                    var subKeys = key.GetSubKeyNames();

                    if (subKeys.Any(k => string.Equals(k, s, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        var tempKey = key.OpenSubKey(s, true);
                        key.Close();
                        key = tempKey;
                        index = ++index;
                        continue;
                    }
                }

                if (key != null)
                {
                    var tempKey2 = key.CreateSubKey(s, true);
                    key.Close();
                    key = tempKey2;
                }

                index = ++index;
            }
        }

        private static string MergeSpace(this string str)
        {
            while (str.Contains("  "))
            {
                str = str.Replace("  ", " ");
            }

            return str;
        }
    }
}