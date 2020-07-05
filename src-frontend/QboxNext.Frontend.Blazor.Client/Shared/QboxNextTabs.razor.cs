using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QboxNext.Frontend.Blazor.Client.Shared
{
    public partial class QboxNextTabs
    {
        string selectedTab = "profile";

        private void OnSelectedTabChanged(string name)
        {
            selectedTab = name;
        }
    }
}
