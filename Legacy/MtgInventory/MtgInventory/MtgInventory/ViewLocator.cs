// Copyright (c) The Avalonia Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using MtgInventory.ViewModels;

namespace MtgInventory
{
    public class ViewLocator : IDataTemplate
    {
        public bool SupportsRecycling => false;

        public IControl? Build(object param)
        {
            var name = param?.GetType()?.FullName?.Replace("ViewModel", "View") ?? "";
            var type = Type.GetType(name);

            if (type != null)
            {
                return Activator.CreateInstance(type) as Control;
            }

            return new TextBlock { Text = "Not Found: " + name };
        }

        public bool Match(object data)
        {
            return data is ViewModelBase;
        }
    }
}