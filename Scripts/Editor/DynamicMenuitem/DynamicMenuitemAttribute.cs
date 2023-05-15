using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace UnityGameFramework.Editor
{
    public class DynamicMenuItemAttribute : System.Attribute
    {
        public string ItemName { get; private set; }

        public int Priority { get; private set; }

        public DynamicMenuItemAttribute(string itemName, int priority)
        {
            ItemName = itemName;
            Priority = priority;
        }
    }

    public class BoolMenuItemAttribute : System.Attribute
    {
        public string Key { get; private set; }
        public bool DefaultValue { get; private set; }

        public BoolMenuItemAttribute(string key, bool defaultValue = false)
        {
            Key = key;
            DefaultValue = defaultValue;
        }
    }

    public class GroupMenuItemAttribute : System.Attribute
    {
        public string Key { get; private set; }
        public string DefaultValue { get; private set; }

        public string[] Values { get; private set; }

        public string[] Names { get; private set; }

        public GroupMenuItemAttribute(string key, string defaultValue, string[] values, string[] names = null)
        {
            Key = key;
            DefaultValue = defaultValue;

            if (names != null && values.Length != names.Length)
            {
                throw new Exception($"DynamicMenuItem: GroupMenuItem {key} keys/values count NOT equal.");
            }

            if (!values.Contains(defaultValue))
            {
                throw new Exception($"DynamicMenuItem: GroupMenuItem keys NOT contains {defaultValue}.");
            }

            Values = values;
            Names = names ?? values;
        }
    }
}