#if AIUNITY_CODE

using System;
using AiUnity.CLog.Core.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AiUnity.CLog.Core.Common
{
    public class StringFormatFormattedMessage
    {
        private volatile string cachedMessage;

        private readonly IFormatProvider FormatProvider;
        private readonly string Message;
        private readonly object[] Args;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringFormatFormattedMessage"/> class.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The args.</param>
       // [StringFormatMethod("message")]
        public StringFormatFormattedMessage(IFormatProvider formatProvider, string message, params object[] args)
        {
            FormatProvider = formatProvider;
            Message = message;
            Args = args;
        }

        /// <summary>
        /// Runs <see cref="string.Format(System.IFormatProvider, string, object[])"/> on supplied arguments.
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            if (cachedMessage == null && Message != null) {
                cachedMessage = string.Format(FormatProvider, Message, Args);
            }
            return cachedMessage;
        }
    }
}
#endif
