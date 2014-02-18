namespace Cafe.DailyReports
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading;

    using Nancy.Helpers;

    using log4net.Appender;
    using log4net.Core;

    public sealed class HipChatLogAppender : AppenderSkeleton
    {
        #region Static Fields

        /// <summary>
        /// Map of message levels to colors
        /// </summary>
        private static readonly Dictionary<string, string> ColorMap = new Dictionary<string, string>
            { { "DEBUG", "gray" }, { "INFO", "gray" }, { "WARN", "yellow" }, { "ERROR", "red" }, { "FATAL", "red" }, };

        /// <summary>
        /// Map of message levels to emoticons
        /// </summary>
        private static readonly Dictionary<string, string> EmoticonMap = new Dictionary<string, string>
            {
                { "DEBUG", "(content)" },
                { "INFO", "(wat)" },
                { "WARN", "(ohcrap)" },
                { "ERROR", "(omg)" },
                { "FATAL", "(boom)" }
            };

        #endregion

        #region Constructors and Destructors

        public HipChatLogAppender()
        {
            this.UseEmoticons = true;
            this.Synchronous = true;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the auth token.
        /// </summary>
        /// <value>
        /// The auth token.
        /// </value>
        public string AuthToken { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not this message should trigger a notification.
        /// </summary>
        public bool Notify { get; set; }

        /// <summary>
        /// Gets or sets the room id.
        /// </summary>
        /// <value>
        /// The room id.
        /// </value>
        public string RoomId { get; set; }

        /// <summary>
        /// Gets or sets the name of the sender.
        /// </summary>
        /// <value>
        /// The name of the sender.
        /// </value>
        public string SenderName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="HipChatLogAppender"/> is synchronous.
        /// </summary>
        /// <value>
        ///   <c>true</c> if synchronous; otherwise, <c>false</c>.
        /// </value>
        public bool Synchronous { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this appender should use emoticons.
        /// </summary>
        /// <value>
        ///   <c>true</c> if we should use emoticons; otherwise, <c>false</c>.
        /// </value>
        public bool UseEmoticons { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Writes the logging event out to hipchat with the specified details.
        /// </summary>
        /// <param name="loggingEvent">The event to append.</param>
        protected override void Append(LoggingEvent loggingEvent)
        {
            try
            {
                string content = this.GetMessage(loggingEvent);
                string color = GetBackgroundColor(loggingEvent.Level.Name);
                var message = new HipChatMessage(content, color);

                if (this.Synchronous)
                {
                    SendMessageRequest(
                        message.Content, this.AuthToken, this.RoomId, this.SenderName, message.Color, this.Notify);
                }
                else
                {
                    var thread = new Thread(this.SendMessage);
                    thread.IsBackground = true;
                    thread.Start(message);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Gets the color of the background for the message.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <returns>
        /// The color to use for the message
        /// </returns>
        private static string GetBackgroundColor(string level)
        {
            string color;
            ColorMap.TryGetValue(level, out color);
            return color;
        }

        /// <summary>
        /// Sends a message to a room.
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <param name="token">The hipchat token.</param>
        /// <param name="roomName">Name of the room to send to.</param>
        /// <param name="from">The Name to use when sending the message.</param>
        /// <param name="color">The color.</param>
        /// <param name="notify">if set to <c>true</c> notify.</param>
        private static void SendMessageRequest(
            string message, string token, string roomName, string from, string color, bool notify)
        {
            try
            {
                if (from.Length > 15)
                {
                    from = from.Substring(0, 15);
                }

                if (message.Length > 5000)
                {
                    message = message.Substring(0, 5000);
                }

                string url = string.Format(
                    "https://api.hipchat.com/v1/rooms/message?auth_token={0}", Uri.EscapeDataString(token));

                //string encodedMessage = HttpUtility.UrlEncode(message);

                string content =
                    string.Format(
                        "room_id={0}&notify={1}&from={2}&message={3}&color={4}&message_format={5}",
                        HttpUtility.UrlEncode(roomName),
                        notify ? '1' : '0',
                        HttpUtility.UrlEncode(from),
                        message,
                        color,
                        "text");

                Trace.WriteLine(url);
                Trace.WriteLine(content);

                WebRequest request = WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                byte[] contentBytes = Encoding.UTF8.GetBytes(content);
                request.ContentLength = contentBytes.Length;

                Stream stream = request.GetRequestStream();

                stream.Write(contentBytes, 0, contentBytes.Length);
                stream.Flush();
                stream.Flush();

                using (request.GetResponse())
                {
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Prepends the message with the emoticon if needed.
        /// </summary>
        /// <param name="loggingEvent">The logging event.</param>
        /// <returns>
        /// The message with the emoticon appended
        /// </returns>
        private string GetMessage(LoggingEvent loggingEvent)
        {
            string message = this.RenderLoggingEvent(loggingEvent);
            if (this.UseEmoticons == false)
            {
                return message;
            }

            string icon;
            if (EmoticonMap.TryGetValue(loggingEvent.Level.Name, out icon))
            {
                return string.Format("{0} - {1}", icon, message);
            }

            return message;
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="parameters">The thread parameters.</param>
        private void SendMessage(object parameters)
        {
            try
            {
                var message = (HipChatMessage)parameters;
                SendMessageRequest(
                    message.Content, this.AuthToken, this.RoomId, this.SenderName, message.Color, this.Notify);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        #endregion

        /// <summary>
        /// Class to contain arguments to pass to a background worker
        /// </summary>
        private sealed class HipChatMessage
        {
            #region Constructors and Destructors

            /// <summary>
            /// Initializes a new instance of the <see cref="HipChatMessage"/> class.
            /// </summary>
            /// <param name="content">The content.</param>
            /// <param name="color">The color.</param>
            public HipChatMessage(string content, string color)
            {
                this.Content = content;
                this.Color = color;
            }

            #endregion

            #region Public Properties

            /// <summary>
            /// Gets or sets the color.
            /// </summary>
            /// <value>
            /// The color.
            /// </value>
            public string Color { get; set; }

            /// <summary>
            /// Gets or sets the message content.
            /// </summary>
            /// <value>
            /// The content.
            /// </value>
            public string Content { get; set; }

            #endregion
        }
    }
}