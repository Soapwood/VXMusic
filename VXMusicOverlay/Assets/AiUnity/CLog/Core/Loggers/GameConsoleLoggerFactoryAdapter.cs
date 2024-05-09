// Copyright (C) 2014 Extesla, LLC.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#if AIUNITY_CODE

using System;
using AiUnity.CLog.Core.Common;
using System.Collections.Generic;
using System.ComponentModel;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Attributes;
using AiUnity.Common.Log;

namespace AiUnity.CLog.Core.Loggers
{
    [Adapter("GameConsole", typeof(GameConsoleLogger))]
    public class GameConsoleLoggerFactoryAdapter : UnityConsoleLoggerFactoryAdapter, IGameConsoleLoggerSettings
    {
        #region Properties
        // Internal logger singleton
        private static IInternalLogger Logger { get { return CLogInternalLogger.Instance; } }

        [RequiredParameter]
        [DefaultValue(true)]
        [Display("Start console", "Make Game Console Log window active at startup.", false)]
        public bool ConsoleActive { get; set; }

        [RequiredParameter]
        [DefaultValue(8)]
        [Display("Font size", "Font size for game console", false)]
        public int FontSize { get; set; }

        [RequiredParameter]
        [DefaultValue(true)]
        //[Display("Enable Icon", "Display shortcut icon when NLOG Console minimized.  Alternatively NLOG Console can be restored with a gesture (TBD).", false)]
        public bool IconEnable { get; set; }

        [RequiredParameter]
        [DefaultValue(LogLevels.Assert | LogLevels.Fatal | LogLevels.Error | LogLevels.Warn)]
        [Display("Filter levels", "Starting log level filter that is runtime adjustable.", false)]
        public LogLevels LogLevelsFilter { get; set; }

        private IGameConsoleController GameConsoleController { get; set; }
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleOutLoggerFactoryAdapter"/> class.
        /// </summary>
        /// <param name="properties">Configuration data (XML) Used to configure logger properties.</param>
        public GameConsoleLoggerFactoryAdapter(Dictionary<string, string> properties = null) : base(properties)
        {
        }

        /// <summary>
        /// Creates a new <see cref="UnityConsoleLogger"/> instance.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override CLogger CreateLogger(string name, UnityEngine.Object context, IFormatProvider formatProvider)
        {
            return new GameConsoleLogger(name, this, context, formatProvider);
        }

    }
}

#endif
