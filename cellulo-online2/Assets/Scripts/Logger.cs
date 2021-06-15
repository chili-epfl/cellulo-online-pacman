using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Photon.Pun;

public class Logger : MonoBehaviour
{
    private  string LOGFILE_DIRECTORY = "./Logs/";
    // private  string logfile_name = "Log.csv"

    private string logFilePath;
    public bool activeLogging = true;

    void Start()
    {
        if(this.activeLogging)
        {
            // check if directory exists (and create it if not)
            if(!Directory.Exists(LOGFILE_DIRECTORY)) Directory.CreateDirectory(LOGFILE_DIRECTORY);

            var hostName = PhotonNetwork.LocalPlayer.NickName;
            var time = System.DateTime.Now.ToString(CultureInfo.InvariantCulture);

            // logfile_name = "logs_"+hostName+"_"+time+".csv";
            var logfile_name = "logs.csv";
            this.logFilePath = LOGFILE_DIRECTORY + logfile_name;

            var fileExistedBefore = File.Exists(this.logFilePath);

            if (!fileExistedBefore)
            {
                File.Create(this.logFilePath).Dispose();
                writeMessageToLog("Time\tPacManMode\tHostPlayName\tRemotePlayerName\tTimeToWin\tTimesCaught");
                Debug.Log("LogFile created at " + this.logFilePath);
            }

            // if(File.Exists(this.logFilePath))
            // else Debug.LogError("Error creating LogFile");

            // writeMessageToLog("GameTime\tlocalX\tlocalY\tremoteX\tremoteY");
        }
    }

    /// <summary>
    /// Writes the message to the log file on disk.
    /// </summary>
    /// <param name="message">string representing the message to be written.</param>

    public void writeMessageToLog(string message)
    {
        if(this.activeLogging)
        {
            if(File.Exists(this.logFilePath))
            {
                TextWriter tw = new StreamWriter(this.logFilePath, true);
                tw.WriteLine(message);
                tw.Close();
            }
        }
    }

    public void writeGameSummaryToLog(PacManController.PacManMode pacManMode, float gameTime, int timesCaught)
    {
        var time = System.DateTime.Now.ToString(CultureInfo.InvariantCulture);
        var (localPlayerName, remotePlayerName) = getPlayerNames();
        writeMessageToLog($"{time}\t{pacManMode.ToString()}\t{localPlayerName}\t{remotePlayerName}\t{gameTime}\t{timesCaught}");
    }

    // public void writePositionToLog(float gameTime, string x, string y)
    // {
    //     writeMessageToLog(gameTime+"s" + "\t" + x + "\t" + y);
    // }

    private (string, string) getPlayerNames()
    {
        var localPlayerName = PhotonNetwork.LocalPlayer.NickName;
        var remotePlayerName = "error";
        foreach (var player in PhotonNetwork.PlayerListOthers)
        {
            remotePlayerName = player.NickName;
        }

        return (localPlayerName, remotePlayerName);
    }
}
