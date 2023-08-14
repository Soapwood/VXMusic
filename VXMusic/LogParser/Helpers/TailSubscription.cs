﻿using System;
using System.IO;
using System.Text;
using System.Threading;

namespace VXMusic.LogParser.Helpers
{
    public class TailSubscription : IDisposable
    {
        private string _filePath { get; set; }
        public delegate void OnUpdate(string content);
        public delegate void OnFirstRead(string filePath, long fromByte);
        private OnUpdate updateFunc { get; set; }
        private OnFirstRead firstReadFunc { get; set; }
        private Timer timer { get; set; }
        private long lastSize { get; set; }

        private bool doneFirstRead { get; set; }

        public TailSubscription(string filePath, OnUpdate func, long dueTimeMilliseconds, long frequencyMilliseconds, OnFirstRead readFunc = null)
        {
            _filePath = filePath;
            updateFunc = func;
            firstReadFunc = readFunc;
            lastSize = new FileInfo(filePath).Length;
            timer = new Timer(new TimerCallback(ExecOnUpdate), null, dueTimeMilliseconds, frequencyMilliseconds);
        }

        private void ExecOnUpdate(object timerState)
        {
            if (!File.Exists(_filePath))
            {
                Dispose();
                return;
            }

            long size = new FileInfo(_filePath).Length;

            if (size > lastSize)
            {
                if (!doneFirstRead)
                {
                    doneFirstRead = true;
                    firstReadFunc(_filePath, lastSize);
                }

                using (FileStream fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        sr.BaseStream.Seek(lastSize, SeekOrigin.Begin);

                        StringBuilder outputContent = new StringBuilder();
                        string line = string.Empty;

                        while ((line = sr.ReadLine()) != null)
                            outputContent.Append(line + "\n");

                        lastSize = sr.BaseStream.Position;

                        updateFunc(outputContent.ToString());
                    }
                }
            }
        }

        public void Dispose()
        {
            timer.Dispose();
            updateFunc = null;
        }
    }
}
