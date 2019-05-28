using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Consts
{
    using static Consts.Methods;
    using static Consts.References;

    public static class UserGame
    {
        public const int PLAYER_MAX_LIVES = 3;

        public const string PLAYER_LIVES = "PlayerLives";
        public const string PLAYER_READY = "IsPlayerReady";
        public const string PLAYER_LOADED_LEVEL = "PlayerLoadedLevel";
        public const float PLAYER_RESPAWN_TIME = 4.0f;
    }

    public static class References
    {
        public const string MY_CONST = "Test";
    }

    public static class Methods
    {
        public static readonly Action Noob = () => { };
    }
}