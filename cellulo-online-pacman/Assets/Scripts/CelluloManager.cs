using System;
using Photon.Pun;
using UnityEngine;

public static class CelluloManager
{
    [NonSerialized] public static bool IsCelluloHost = false;

    //========================================================================
    // Setup and Teardown

    public static void TryInitialize()
    {
        if (Globals.IsPlatformCelluloCompatible())
        {
            Cellulo.initialize();
            IsCelluloHost = true;
            Debug.Log("Cellulo Initialized!");
        }
        else
        {
            Debug.Log("Platform incompatible with Cellulo");
        }
    }

    public static void TryDeinitialize()
    {
        if (IsCelluloHost)
        {
            Cellulo.deinitialize();
        }
    }

    //========================================================================

    public static Cellulo GetCellulo()
    {
        Cellulo cellulo;

        if (Cellulo.robotsRemaining() >= 1)
        {
            cellulo = new Cellulo();
        }
        else
        {
            throw new Exception("No cellulo robot(s) found to connect to");
        }

        return cellulo;
    }
}
