﻿using System;

namespace MariEngine.Utils;

public static class TypeUtils
{
    public static bool IsAssignableToGenericType(Type givenType, Type genericType)
    {
        var interfaceTypes = givenType.GetInterfaces();

        foreach (var it in interfaceTypes)
        {
            if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                return true;
        }

        if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
            return true;

        var baseType = givenType.BaseType;
        if (baseType == null) return false;

        return IsAssignableToGenericType(baseType, genericType);
    }
}