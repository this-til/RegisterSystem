using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace RegisterSystem;

public class Util {
    public static bool isEffective(Type type) {
        if (type.IsAbstract) {
            return false;
        }
        if (type.GetCustomAttribute<ObsoleteAttribute>() is not null) {
            return false;
        }
        return true;
    }

    public static bool isEffective(FieldInfo memberInfo) {
        if (memberInfo.GetCustomAttribute<ObsoleteAttribute>() is not null) {
            return false;
        }
        if (memberInfo.GetCustomAttribute<IgnoreRegisterAttribute>() is not null) {
            return false;
        }
        return true;
    }
    
    public static bool isEffective(PropertyInfo memberInfo) {
        if (memberInfo.GetCustomAttribute<ObsoleteAttribute>() is not null) {
            return false;
        }
        if (memberInfo.GetCustomAttribute<IgnoreRegisterAttribute>() is not null) {
            return false;
        }
        return true;
    }

    public static String ofPath(Type type) {
        StringBuilder stringBuilder = new StringBuilder();
        String className = type.Name;

        String[] cell = className.Split("_");
        for (int i = 0; i < cell.Length; i++) {
            String stringCell = cell[i];
            if (string.IsNullOrEmpty(stringCell)) {
                continue;
            }
            if (i != 0) {
                stringBuilder.Append('_');
            }

            char[] chars = stringCell.ToCharArray();
            bool isOldUpperCase = false;
            for (int ii = 0; ii < chars.Length; ii++) {
                char c = chars[ii];
                if (char.IsUpper(c)) {
                    if (ii != 0 && !isOldUpperCase) {
                        stringBuilder.Append('_');
                    }
                    isOldUpperCase = true;
                    stringBuilder.Append(upperToLower(c));
                    continue;
                }
                isOldUpperCase = false;
                stringBuilder.Append(c);
            }
        }
        return stringBuilder.ToString();
    }

    public static String ofCompleteName(RegisterManage registerManage) {
        if (registerManage.getBasicsRegisterManage() is null) {
            return registerManage.getName();
        }
        List<string> list = new List<string>(2);
        RegisterManage? basics = registerManage;
        while (basics is not null) {
            list.Add(basics.getName());
            basics = basics.getBasicsRegisterManage();
        }
        list.Reverse();
        return string.Join('/', list);
    }

    public static char upperToLower(char c) {
        if (c > 64 && c < 91) {
            c = (char)(c + 32);
        }
        return c;
    }
}