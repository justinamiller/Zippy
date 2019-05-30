﻿using System;
using System.Collections.Generic;
using System.Text;
using static JsonSerializer.Utility.ConvertUtils;

namespace JsonSerializer.Internal
{
    internal interface IValue
    {
        string Name { get; }
        Utility.ConvertUtils.TypeCode Code { get; }
        object GetValue(object instance);
    }
}