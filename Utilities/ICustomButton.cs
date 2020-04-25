﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static UnityEngine.UI.Button;

namespace YourCommonTools
{
    public interface ICustomButton
    {
        ButtonClickedEvent GetOnClick();
        bool RunOnClick();
    }
}