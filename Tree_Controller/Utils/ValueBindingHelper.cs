﻿// <copyright file="ValueBindingHelper.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>

namespace Tree_Controller.Utils
{
    using System;
    using Colossal.UI.Binding;

    public class ValueBindingHelper<T>
    {
        private readonly Action<T> _updateCallBack;

        public ValueBinding<T> Binding { get; }

        public T Value { get => Binding.value; set => Binding.Update(value); }

        public ValueBindingHelper(ValueBinding<T> binding, Action<T> updateCallBack = null)
        {
            Binding = binding;
            _updateCallBack = updateCallBack;
        }

        public void UpdateCallback(T value)
        {
            Binding.Update(value);
            _updateCallBack?.Invoke(value);
        }

        public static implicit operator T(ValueBindingHelper<T> helper)
        {
            return helper.Binding.value;
        }
    }
}