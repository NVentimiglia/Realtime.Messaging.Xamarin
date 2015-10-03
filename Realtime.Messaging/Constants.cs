using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealtimeFramework.Messaging {
    internal class Constants {
        /// <summary>
        /// Message maximum size in bytes
        /// </summary>
        /// <exclude/>
        internal const int MAX_MESSAGE_SIZE = 700;

        /// <summary>
        /// Channel maximum size in bytes
        /// </summary>
        /// <exclude/>
        internal const int MAX_CHANNEL_SIZE = 100;

        /// <summary>
        /// Connection Metadata maximum size in bytes
        /// </summary>
        /// <exclude/>
        internal const int MAX_CONNECTION_METADATA_SIZE = 256;


        internal const int HEARTBEAT_MAX_TIME = 60;
        internal const int HEARTBEAT_MIN_TIME = 10;
        internal const int HEARTBEAT_MAX_FAIL = 6;
        internal const int HEARTBEAT_MIN_FAIL = 1;


		internal const int  HTTPCLIENT_TIMEOUT = 5;


        #region Regex patterns (11)        
        internal const string OPERATION_PATTERN = @"^a\[""{""op"":""(?<op>[^\""]+)"",(?<args>.*)}""\]$";
        internal const string CLOSE_PATTERN = @"^c\[?(?<code>[^""]+),?""?(?<message>.*)""?\]?$";
        internal const string VALIDATED_PATTERN = @"^(""up"":){1}(?<up>.*)?,""set"":(?<set>.*)$";
        internal const string CHANNEL_PATTERN = @"^""ch"":""(?<channel>.*)""$";
        internal const string EXCEPTION_PATTERN = @"^""ex"":{(""op"":""(?<op>[^""]+)"",)?(""ch"":""(?<channel>.*)"",)?""ex"":""(?<error>.*)""}$";
        internal const string RECEIVED_PATTERN = @"^a\[""{""ch"":""(?<channel>.*)"",""m"":""(?<message>[\s\S]*)""}""\]$";
        internal const string MULTI_PART_MESSAGE_PATTERN = @"^(?<messageId>.[^_]*)_(?<messageCurrentPart>.[^-]*)-(?<messageTotalPart>.[^_]*)_(?<message>[\s\S]*)$";
        internal const string PERMISSIONS_PATTERN = @"""(?<key>[^""]+)"":{1}""(?<value>[^,""]+)"",?";
        internal const string CLUSTER_RESPONSE_PATTERN = @"var SOCKET_SERVER = ""(?<host>.*)"";";
        internal const int SERVER_HB_COUNT = 30;
        #endregion

    }
}
