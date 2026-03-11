using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace IntelligentAudio.API;

public sealed class SessionManager
{
    // Latency Analysis: Snabb lookup för att veta vilken port vi ska skicka OSC till
    private readonly ConcurrentDictionary<string, int> _sessionPorts = new();

    public void Register(string sid, int port) => _sessionPorts[sid] = port;
    public int GetPort(string sid) => _sessionPorts.GetValueOrDefault(sid, 9005); // Defaultar till 9005
}
