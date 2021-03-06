using System;
using System.Linq;
using UnityEngine;

public static class Globals
{
       //---------------------------------------------------------------------
       // Debug Controls
       public static readonly bool DebugShowPing = true;

       // This boolean enables the shortcut for filling room settings. In the main menu.
       // Press Ctrl-D to fill name and room name and create room.
       // Press Ctrl-Shift D to fill name and room name and join room.
       public static readonly bool DebugCreateJoinRoomShortcut = true;

       public static readonly bool DebugAllowLocalControlOfPlayer = true;

       // When true allows controlling the remote player from the local computer while holding shift
       public static readonly bool DebugShiftToControlPlayer2 = false;

       // Will just set the create room settings to the values set just below.
       public static readonly bool DebugFillRoomSettings = false;
       public const GameMode DebugGamemodeSelection = GameMode.PacmanCoopVirtual;
       public const int DebugMapSelection = 2; // Valid maps are 1, 2

       //---------------------------------------------------------------------

       // Keys used in PUN Room Custom Properties
       public const string CustomPropertiesGamemodeKey = "gm";
       public const string CustomPropertiesMapKey = "map";

       //---------------------------------------------------------------------

       /// <summary>
       /// If Cellulo library is adapted to run on more Operating Systems they can
       /// be added here!
       /// </summary>
       private static readonly RuntimePlatform[] CelluloCompatiblePlatforms =
              {RuntimePlatform.LinuxEditor, RuntimePlatform.LinuxPlayer};

       public static bool IsPlatformCelluloCompatible()
       {
              return CelluloCompatiblePlatforms.Contains(Application.platform);
       }

       public enum GameMode
       {
              None,
              Pacman,
              PacmanVirutal,
              PacmanCoop,
              PacmanCoopVirtual,
              SpaceInvaders,
              DebugPun,
              DebugCellulo
       }

       [Flags]
       public enum DisallowMovementDirs
       {
              None    = 0b0000,
              Right   = 0b0001,
              Up      = 0b0010,
              Left    = 0b0100,
              Down    = 0b1000
       }

       public static DisallowMovementDirs OppositeDir(DisallowMovementDirs dir)
       {
              if ((dir & DisallowMovementDirs.Right) == DisallowMovementDirs.Right)
                     return DisallowMovementDirs.Left;

              if ((dir & DisallowMovementDirs.Up) == DisallowMovementDirs.Up)
                     return DisallowMovementDirs.Down;

              if ((dir & DisallowMovementDirs.Left) == DisallowMovementDirs.Left)
                     return DisallowMovementDirs.Right;

              if ((dir & DisallowMovementDirs.Down) == DisallowMovementDirs.Down)
                     return DisallowMovementDirs.Up;

              return DisallowMovementDirs.None;
       }

       //========================================================================
       // Coordinate Rescaling Stuff
       //     No longer needed since game map scale is now 1:1 with game

       // private const float real_map_x_min = 0f;
       // private const float real_map_x_max = 620f;
       // private const float real_map_y_min = 0f;
       // private const float real_map_y_max = 420f;

       // Old Rescaling Bounds
       // private const float game_map_x_max = 7f;
       // private const float game_map_x_min = -7f;
       // private const float game_map_y_max = 4.5f;
       // private const float game_map_y_min = -4.5f;

       // private const float game_map_x_max = real_map_x_max;
       // private const float game_map_x_min = real_map_x_min;
       // private const float game_map_y_max = real_map_y_max;
       // private const float game_map_y_min = real_map_y_min;

       /// <summary>
       /// Since the need for rescaling has been removed the function isn't really needed
       /// anymore. It been left here just in case.
       /// </summary>
       /// <param name="real_x"></param>
       /// <param name="real_y"></param>
       /// <returns></returns>
       public static Vector2 MapCoordsToGameCoords(float real_x, float real_y)
       {
              // float game_x = Rescale(real_x, real_map_x_min, real_map_x_max, game_map_x_min, game_map_x_max);
              // float game_y = -Rescale(real_y, real_map_y_min, real_map_y_max, game_map_y_min, game_map_y_max);

              return new Vector2(real_x, real_y);
       }


       // Formula: https://stats.stackexchange.com/questions/281162/scale-a-number-between-a-range/281164
       public static float Rescale(float m, float rMin, float rMax, float tMin, float tMax)
       {
              return ((m - rMin) / (rMax - rMin) * (tMax - tMin)) + tMin;
       }


       //========================================================================
       // Misc
       public static string FormatTime( float time )
       {
              int minutes = (int) time / 60 ;
              int seconds = (int) time - 60 * minutes;
              int milliseconds = (int) (1000 * (time - minutes * 60 - seconds));
              return $"{minutes:00}:{seconds:00}:{milliseconds:000}";
       }
}
