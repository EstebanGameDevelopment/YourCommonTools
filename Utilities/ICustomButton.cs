using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace YourCommonTools
{
    public interface ICustomButton
    {
        UnityEngine.UI.Button.ButtonClickedEvent GetOnClick();
        bool RunOnClick();
    }
}
