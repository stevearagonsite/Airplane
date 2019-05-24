using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Consts
{
    using static Consts.Methods;
    using static Consts.References;

    public static class References
    {
        public const string MY_CONST = "Test";
    }

    public static class Methods
    {
        public static readonly Action noob = () => { };
    }
}