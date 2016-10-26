using System;
using System.Collections.Generic;

using System.Text;

namespace Beetle.Express
{

     delegate void EventChannelConnectd(object sender, ChannelConnectEventArgs e);

     delegate void EventChannelDisposed(object sender, ChannelEventArgs e);

     delegate void EventChannelError(object sender, ErrorEventArgs e);
 
     delegate void EventChannelReceive(object sender, ChannelReceiveEventArgs e);

     delegate void EventChannelSend(object sender, ChannelSendEventArgs e);
}
