using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using WebSocketSharp;
using UnityEngine;
using SIGVerse.Common;

namespace SIGVerse.RosBridge
{
	public interface IRosConnection
	{
		bool IsConnected();
	}
}

