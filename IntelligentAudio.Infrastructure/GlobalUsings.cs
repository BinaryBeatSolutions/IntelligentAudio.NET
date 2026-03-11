global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Text;
global using System.Data.Common;
global using System.Threading.Tasks;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Hosting;
global using System.Collections.Concurrent;
global using System.Net;
global using System.Net.Sockets;
global using System.Buffers;
global using System.Threading.Channels;
global using System.Runtime.InteropServices;

global using static System.Runtime.InteropServices.JavaScript.JSType;

global using IntelligentAudio.Contracts.Models;
global using IntelligentAudio.Engine.Processors;
global using IntelligentAudio.Contracts;
global using IntelligentAudio.Infrastructure.Communication;
global using IntelligentAudio.Contracts.Events;
global using IntelligentAudio.Engine.Services;
global using IntelligentAudio.Contracts.Interfaces;
global using IntelligentAudio.Infrastructure.Extensions;
global using IntelligentAudio.Integrations.Common.Daw;
global using IntelligentAudio.Integrations.Common.Daw.Models;
global using IntelligentAudio.Integrations.Common.Daw.Interfaces;

global using IntelligentAudio.MusicTheory;
global using IntelligentAudio.MusicTheory.Models;
