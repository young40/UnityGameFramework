using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        public BoolMenuItemAttribute(string key)
        {
            Key = key;
        }
    }
}