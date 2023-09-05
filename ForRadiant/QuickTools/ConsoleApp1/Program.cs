using System;
using System.Management;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;

class ProcessMonitor
{
    public static void Main()
    {
        Dictionary<string, Process> newProcs = new Dictionary<string, Process>();
        while (true)
        {
            foreach (Process process in Process.GetProcesses())
            {
                if (process.ProcessName.CompareTo("notepad") == 0)
                {
                    if (!searchForProcess(newProcs, process.Id))
                    {
                        ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id);
                        foreach (ManagementObject @object in searcher.Get())
                        {
                            Console.Write("Adding new Process: ");
                            Console.WriteLine(@object["CommandLine"] + " ");
                            newProcs.Add(@object["CommandLine"] + " ", process);
                        }
                    }
                }
            }
            checkProcesses(newProcs);
        }
    }

    private static bool searchForProcess(Dictionary<string, Process> newProcs, int newKey)
    {
        foreach (Process process in newProcs.Values)
        {
            if (process.Id == newKey)
                return true;
        }

        return false;
    }
    private static void checkProcesses(Dictionary<string, Process> newProcs)
    {
        foreach (string currProc in newProcs.Keys)
        {
            bool processExists = false;
            foreach (Process process in Process.GetProcesses())
            {
                if (process.Id == newProcs[currProc].Id)
                {
                    processExists = true;
                    break;
                }

            }
            if (!processExists)
            {
                Console.Write("Process Finished: ");
                Console.WriteLine(currProc);
                newProcs.Remove(currProc);
                if (newProcs.Count == 0)
                    break;
            }
        }
    }
}