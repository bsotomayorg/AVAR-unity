using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
#if WINDOWS_UWP
using Windows.Storage;
using Windows.Storage.Streams;
#endif

public class OperationLog
{
    public enum OperationEvent
    {
        Configuration=-1,
        ChangeFocus=0,
        SelectExample=1,
        ExecuteScript=2,
        LaunchConfiguration=3,
        MoveObject=4,
        RotateObject=5
    }

    private static OperationLog instance;
    private bool isInited = false;
    
#if WINDOWS_UWP
    StorageFolder newFolder;
    StorageFile OperationFile;
    StorageFile PosFile;
#endif
    public static OperationLog Instance
    {
        get
        {
            if (instance == null)
                instance = new OperationLog();
            return instance;
        }
    }

    public async void submitConfiguration(string userName)
    {
        string tempText = string.Empty;
        var timeNow = System.DateTime.Now;
        tempText = "[" + timeNow.Day.ToString("d2") + timeNow.Month.ToString("d2") + timeNow.Year.ToString("d2") + " " + timeNow.Hour.ToString("d2") + timeNow.Minute.ToString("d2") + timeNow.Second.ToString("d2") + "] "+userName;

#if WINDOWS_UWP
        newFolder = await KnownFolders.DocumentsLibrary.CreateFolderAsync("AVAR",CreationCollisionOption.OpenIfExists);
        OperationFile = await newFolder.CreateFileAsync(tempText+"_operation.txt", CreationCollisionOption.OpenIfExists);
        PosFile=await newFolder.CreateFileAsync(tempText+"_position.txt", CreationCollisionOption.OpenIfExists);
#endif

        isInited = true;
    }


    public async void AddToPosLog(Vector3 pos, Vector3 rot, string name, Vector3 objPos)
    {
        if (!isInited)
            return;
        var timeNow = System.DateTime.Now;
        string textLog = string.Empty;
        textLog = "[" + timeNow.Day + "/" + timeNow.Month + "/" + timeNow.Year + " " + timeNow.Hour.ToString("d2") + ":" + timeNow.Minute.ToString("d2") + ":" + timeNow.Second.ToString("d2") + "] "
            + pos + " " + rot + name + objPos+"\n";
#if WINDOWS_UWP
        await FileIO.AppendTextAsync(PosFile, textLog, Windows.Storage.Streams.UnicodeEncoding.Utf8);
#endif
    }

    public async void AddToOperationLog(OperationEvent EventType,string newText)
    {

        if (!isInited)
            return;
        string textLog = string.Empty;

        var timeNow = System.DateTime.Now;
        textLog = "\n[" + timeNow.Day + "/" + timeNow.Month + "/" + timeNow.Year + " " + timeNow.Hour.ToString("d2") + ":" + timeNow.Minute.ToString("d2") + ":" + timeNow.Second.ToString("d2") + "] " + EventType.ToString() + ": " + newText+ "\n";
        Debug.Log(textLog);
#if WINDOWS_UWP
        await FileIO.AppendTextAsync(OperationFile, textLog, Windows.Storage.Streams.UnicodeEncoding.Utf8);
#endif
    }
}
