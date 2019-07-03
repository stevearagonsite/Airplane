using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Consts
{
    public static class UserGame
    {
        public const int PLAYER_MAX_LIVES = 3;

        public const string PLAYER_LIVES = "PlayerLives";
        public const string PLAYER_ENTRY = "PlayerEntry";
        public const string PLAYER_READY = "IsPlayerReady";
        public const string PLAYER_LOADED_LEVEL = "PlayerLoadedLevel";
        public const float PLAYER_RESPAWN_TIME = 3.0f;
        public const float PLAYER_FORCE_TO_EXPLOTION = 10000;
    }
    
    public static class Layers
    {
        public const int TERRAIN_NUM_LAYER = 8;
        public const string TERRAIN_LABEL_LAYER = "Terrain";
        public const int PLAYERS_NUM_LAYER  = 9;
        public const string PLAYERS_LABEL_LAYER  = "Players";
        public const int TRIGGERS_NUM_LAYER  = 10;
        public const string TRIGGERS_LABEL_LAYER  = "Triggers";
        public const int WALLS_NUM_LAYER  = 11;
        public const string WALLS_LABEL_LAYER  = "Walls";
        public const int WINNER_NUM_LAYER  = 12;
        public const string WINNER_LABEL_LAYER  = "Winner";
    }

    public static class Methods
    {
        public static readonly Action Noob = () => { };
    }
}