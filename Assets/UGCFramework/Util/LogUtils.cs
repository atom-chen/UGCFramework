﻿using System;
using System.IO;
using System.Text;
using UnityEngine;

public partial class LogUtils
{
    public static string LogErrorRootPath = Application.persistentDataPath + "/LogError/";
    static string logErrorPath;
    static StringBuilder allLogStr = new StringBuilder();
    static StreamWriter logWrite;

    /// <summary>
    /// 封装系统Debug.Log
    /// </summary>
    /// <param name="log">输出的日志内容</param>
    /// <param name="isAppend">是否对日志进行保留拼接</param>
    public static void Log(object log, bool isAppend = true)
    {
        if (EternalGameObject.Instance.isDebugLog)
            Debug.Log(log);

        if (isAppend)
            AppendLog(log);
    }

    public static void LogError(object log, bool containStackTrace = true)
    {
        string messageStr = log.ToString();
        if (EternalGameObject.Instance.isDebugLog)
            Log(log);
        if (containStackTrace)
            LogToFile(messageStr, new System.Diagnostics.StackTrace().ToString(), LogType.Error);
        else
            LogToFile(messageStr, null, LogType.Error);
    }

    static void AppendLog(object log)
    {
        try
        {
            if (allLogStr.Length > 1048576)
                allLogStr.Clear();
            allLogStr.AppendLine();
            allLogStr.Append(string.Format("{0}   {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), log));
            allLogStr.AppendLine();
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    public static void LogToFile(string condition, string stackTrace, LogType type)
    {
        switch (type)
        {
            case LogType.Exception:
            case LogType.Error:
                RefreshOrCreateFile();
                if (logWrite == null || !string.IsNullOrEmpty(stackTrace))
                {
                    logWrite.Close();
                    File.Delete(logErrorPath);
                    return;
                }
                logWrite.WriteLine();
                logWrite.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 错误类型:" + type);
                logWrite.WriteLine(condition);
                if (!string.IsNullOrEmpty(stackTrace))
                    logWrite.WriteLine(stackTrace);
                //logWrite.WriteLine(type);
                logWrite.Flush();
                break;
            case LogType.Assert:
            case LogType.Log:
            case LogType.Warning:
                break;
        }
    }

    static void RefreshOrCreateFile()
    {
        if (string.IsNullOrEmpty(logErrorPath))
        {
            logErrorPath = LogErrorRootPath + UserInfoModel.Instance.userId + "_" + MiscUtils.GetTimeStampByDateTime(DateTime.Now) + ".log";
            MiscUtils.CreateTextFile(logErrorPath, "");
            logWrite = new StreamWriter(logErrorPath) { AutoFlush = false };
        }
        else
        {
            if (!File.Exists(logErrorPath))
                return;
            int index = logErrorPath.IndexOf("/_");
            if (index > 0)
            {
                string newName = logErrorPath.Insert(index + 1, UserInfoModel.Instance.userId);
                if (newName != logErrorPath)
                {
                    logWrite.Close();
                    StreamReader sr = new StreamReader(logErrorPath);
                    string fileContent = sr.ReadToEnd();
                    sr.Close();
                    File.Delete(logErrorPath);

                    MiscUtils.CreateTextFile(newName, "");
                    logWrite = new StreamWriter(newName) { AutoFlush = false };
                    logWrite.Write(fileContent);
                    logWrite.Flush();
                }
            }
        }
    }
}
