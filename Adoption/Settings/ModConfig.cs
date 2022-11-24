using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using TaleWorlds.Library;

namespace Adoption.Settings
{
    internal sealed class ModSettingsConfig : ISettingsProvider
    {
        public static ModSettingsConfig? Instance { get; private set; }

        public ModSettingsConfig()
        {
            Instance = this;
        }

        public float AdoptionChance
        {
            get { return ModConfig.AdoptionChance; }
            set { ModConfig.AdoptionChance = value; }
        }

        public bool Debug
        {
            get { return ModConfig.Debug; }
            set { ModConfig.Debug = value; }
        }
    }

    internal static class ModConfig
    {
        private static float _adoptionChance = 0.05f;

        private static bool _debug = false;


        [ConfigPropertyUnbounded]
        public static float AdoptionChance
        {
            get
            {
                return _adoptionChance;
            }
            set
            {
                if (_adoptionChance != value)
                {
                    _adoptionChance = value;
                    Save();
                }
            }
        }

        [ConfigPropertyUnbounded]
        public static bool Debug
        {
            get
            {
                return _debug;
            }
            set
            {
                if (_debug != value)
                {
                    _debug = value;
                    Save();
                }
            }
        }


        public static void Initialize()
        {
            string text = ModUtilities.LoadConfigFile();
            if (string.IsNullOrEmpty(text))
            {
                Save();
            }
            else
            {
                bool flag = false;
                string[] array = text.Split(new char[]
                {
                    '\n'
                });
                for (int i = 0; i < array.Length; i++)
                {
                    string[] array2 = array[i].Split(new char[]
                    {
                        '='
                    });
                    PropertyInfo property = typeof(ModConfig).GetProperty(array2[0]);
                    if (property is null)
                    {
                        flag = true;
                    }
                    else
                    {
                        string text2 = array2[1];
                        try
                        {
                            if (property.PropertyType == typeof(string))
                            {
                                string value = Regex.Replace(text2, "\\r", "");
                                property.SetValue(null, value);
                            }
                            else if (property.PropertyType == typeof(float))
                            {
                                if (float.TryParse(text2, out float num))
                                {
                                    property.SetValue(null, num);
                                }
                                else
                                {
                                    flag = true;
                                }
                            }
                            else if (property.PropertyType == typeof(int))
                            {
                                if (int.TryParse(text2, out int num2))
                                {
                                    ConfigPropertyInt customAttribute = property.GetCustomAttribute<ConfigPropertyInt>();
                                    if (customAttribute is null || customAttribute.IsValidValue(num2))
                                    {
                                        property.SetValue(null, num2);
                                    }
                                    else
                                    {
                                        flag = true;
                                    }
                                }
                                else
                                {
                                    flag = true;
                                }
                            }
                            else if (property.PropertyType == typeof(bool))
                            {
                                if (bool.TryParse(text2, out bool flag2))
                                {
                                    property.SetValue(null, flag2);
                                }
                                else
                                {
                                    flag = true;
                                }
                            }
                            else
                            {
                                flag = true;
                            }
                        }
                        catch
                        {
                            flag = true;
                        }
                    }
                }
                if (flag)
                {
                    Save();
                }
            }
        }

        public static SaveResult Save()
        {
            Dictionary<PropertyInfo, object> dictionary = new();
            foreach (PropertyInfo propertyInfo in typeof(ModConfig).GetProperties())
            {
                if (propertyInfo.GetCustomAttribute<ConfigProperty>() is not null)
                {
                    dictionary.Add(propertyInfo, propertyInfo.GetValue(null, null));
                }
            }
            string text = "";
            foreach (KeyValuePair<PropertyInfo, object> keyValuePair in dictionary)
            {
                text = string.Concat(new string[]
                {
                    text,
                    keyValuePair.Key.Name,
                    "=",
                    keyValuePair.Value.ToString(),
                    "\n"
                });
            }
            SaveResult result = ModUtilities.SaveConfigFile(text);
            return result;
        }

        private interface IConfigPropertyBoundChecker<T>
        {
        }

        private abstract class ConfigProperty : Attribute
        {
        }

        private sealed class ConfigPropertyInt : ConfigProperty
        {
            public ConfigPropertyInt(int[] possibleValues, bool isRange = false)
            {
                _possibleValues = possibleValues;
                _isRange = isRange;
                bool isRange2 = _isRange;
            }

            public bool IsValidValue(int value)
            {
                if (_isRange)
                {
                    return value >= _possibleValues[0] && value <= _possibleValues[1];
                }
                int[] possibleValues = _possibleValues;
                for (int i = 0; i < possibleValues.Length; i++)
                {
                    if (possibleValues[i] == value)
                    {
                        return true;
                    }
                }
                return false;
            }

            private int[] _possibleValues;

            private bool _isRange;
        }

        private sealed class ConfigPropertyUnbounded : ConfigProperty
        {
        }
    }
}