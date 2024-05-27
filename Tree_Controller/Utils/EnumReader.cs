// <copyright file="EnumReader.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>

namespace Tree_Controller.Utils
{
    using Colossal.UI.Binding;

    public class EnumReader<T> : IReader<T>
    {
        public void Read(IJsonReader reader, out T value)
        {
            reader.Read(out int value2);
            value = (T)(object)value2;
        }
    }
}
