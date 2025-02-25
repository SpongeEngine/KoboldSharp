# KoboldSharp
[![NuGet](https://img.shields.io/nuget/v/SpongeEngine.KoboldSharp.svg)](https://www.nuget.org/packages/SpongeEngine.KoboldSharp)
[![NuGet Downloads](https://img.shields.io/nuget/dt/SpongeEngine.KoboldSharp.svg)](https://www.nuget.org/packages/SpongeEngine.KoboldSharp)
[![Run Tests](https://github.com/SpongeEngine/KoboldSharp/actions/workflows/run-tests.yml/badge.svg)](https://github.com/SpongeEngine/KoboldSharp/actions/workflows/run-tests.yml)
[![License](https://img.shields.io/github/license/SpongeEngine/KoboldSharp)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-6.0%20%7C%207.0%20%7C%208.0%2B-512BD4)](https://dotnet.microsoft.com/download)

The C# client for KoboldCpp.

## Features
- Complete support for KoboldCpp's native API
- Streaming text generation
- Built-in error handling and logging
- Cross-platform compatibility
- Full async/await support
- Support for Stable Diffusion image generation
- Support for Whisper audio transcription
- Support for WebSearch integration
- Support for multiplayer features

📦 [View Package on NuGet](https://www.nuget.org/packages/SpongeEngine.KoboldSharp)

## Installation
Install via NuGet:
```bash
dotnet add package SpongeEngine.KoboldSharp
```

## Quick Start

### Basic Usage
```csharp
using SpongeEngine.KoboldSharp;

// Configure the client
var options = new KoboldSharpClientOptions
{
    HttpClient = new HttpClient
    {
        BaseAddress = new Uri("http://localhost:5001")
    },
    // Optional client-side settings
    MultiplayerEnabled = false,
    WebSearchEnabled = false
};

// Create client instance
using var client = new KoboldSharpClient(options);

// Generate completion
var request = new KoboldSharpRequest
{
    Prompt = "Write a short story about a robot:",
    MaxLength = 200,
    Temperature = 0.7f,
    TopP = 0.9f
};

var response = await client.GenerateAsync(request);
Console.WriteLine(response.Results[0].Text);

// Stream completion
await foreach (var token in client.GenerateStreamAsync(request))
{
    Console.Write(token);
}
```

## Configuration

### Client Options
```csharp
var options = new KoboldSharpClientOptions
{
    // Base configuration
    HttpClient = new HttpClient
    {
        BaseAddress = new Uri("http://localhost:5001")
    },
    JsonSerializerOptions = new JsonSerializerOptions(), // Optional JSON settings
    Logger = loggerInstance,                             // Optional ILogger
    
    // Optional features
    MultiplayerEnabled = false,           // Enable multiplayer support
    WebSearchEnabled = false,             // Enable web search integration
    
    // Stable Diffusion settings (if using image generation)
    StableDiffusionModelPath = "path/to/model.safetensors",
    StableDiffusionVaePath = "path/to/vae.safetensors",
    StableDiffusionUseQuantization = false,
    StableDiffusionMaxResolution = 512,
    StableDiffusionThreads = -1,
};
```

### Generation Parameters
```csharp
var request = new KoboldSharpRequest
{
    // Basic Parameters
    Prompt = "Your prompt here",
    MaxLength = 200,                     // Maximum tokens to generate
    MaxContextLength = 2048,             // Maximum context length
    Temperature = 0.7f,                  // Randomness (0.0-1.0)
    
    // Sampling Parameters
    TopP = 0.9f,                        // Nucleus sampling threshold
    TopK = 40,                          // Top-K sampling
    TopA = 0.0f,                        // Top-A sampling
    MinP = 0.0f,                        // Minimum P sampling
    Typical = 1.0f,                     // Typical sampling
    Tfs = 1.0f,                         // Tail-free sampling
    
    // Repetition Control
    RepetitionPenalty = 1.1f,           // Base repetition penalty
    RepetitionPenaltyRange = 320,       // How far back to apply penalty
    RepetitionPenaltySlope = 1.0f,      // Penalty application slope
    PresencePenalty = 0.0f,             // Presence penalty
    
    // Mirostat Parameters
    MirostatMode = 0,                   // Mirostat sampling mode (0, 1, 2)
    MirostatTau = 5.0f,                 // Target entropy
    MirostatEta = 0.1f,                 // Learning rate

    // Advanced Control
    Seed = -1,                          // RNG seed (-1 for random)
    StopSequences = new List<string>(),  // Stop generation sequences
    Stream = false,                      // Enable streaming
    TrimStop = true,                    // Trim stop sequences
    Grammar = null,                     // Optional grammar constraints
    GrammarRetainState = false,         // Retain grammar state
    Memory = null,                      // Optional context memory
    BannedTokens = null,                // Tokens to never generate
    LogitBias = null,                   // Token generation biases
    
    // Special Features
    Images = null,                      // Images for multimodal models
    AllowEosToken = true,               // Allow end of sequence token
    BypassEosToken = false,             // Bypass EOS token
    RenderSpecial = false,              // Render special tokens
    
    // Dynamic Temperature
    DynamicTemperatureRange = 0.0f,     // Dynamic temperature range
    DynamicTemperatureExponent = 1.0f,  // Dynamic temperature exponent
    SmoothingFactor = 0.0f              // Output smoothing factor
};
```

### Additional Features

#### Image Generation with Stable Diffusion
```csharp
// Text to image
var txt2imgRequest = new StableDiffusionGenerationRequest
{
    Prompt = "A beautiful mountain landscape",
    NegativePrompt = "blur, haze",
    Width = 512,
    Height = 512,
    Steps = 20,
    CfgScale = 7.0f,
    SamplerName = "euler_a"
};

var txt2imgResponse = await client.TextToImageAsync(txt2imgRequest);

// Image to image
var img2imgRequest = new StableDiffusionImageToImageRequest
{
    InitImages = new List<string> { Convert.ToBase64String(File.ReadAllBytes("input.png")) },
    Prompt = "Convert to oil painting",
    DenoisingStrength = 0.75f
};

var img2imgResponse = await client.ImageToImageAsync(img2imgRequest);
```

#### Audio Transcription with Whisper
```csharp
var transcribeRequest = new WhisperRequest
{
    AudioData = Convert.ToBase64String(File.ReadAllBytes("audio.mp3")),
    Prompt = "Optional transcription prompt",
    SuppressNonSpeech = true
};

var transcription = await client.TranscribeAudioAsync(transcribeRequest);
```

## Error Handling
```csharp
try
{
    var response = await client.GenerateAsync(request);
}
catch (LlmSharpException ex)
{
    Console.WriteLine($"Generation error: {ex.Message}");
    if (ex.StatusCode.HasValue)
    {
        Console.WriteLine($"Status code: {ex.StatusCode}");
    }
    if (ex.ResponseContent != null)
    {
        Console.WriteLine($"Response content: {ex.ResponseContent}");
    }
}
```

## Logging
The client supports Microsoft.Extensions.Logging:

```csharp
ILogger logger = LoggerFactory
    .Create(builder => builder
        .AddConsole()
        .SetMinimumLevel(LogLevel.Debug))
    .CreateLogger<KoboldSharpClient>();

var options = new KoboldSharpClientOptions
{
    HttpClient = new HttpClient
    {
        BaseAddress = new Uri("http://localhost:5001")
    },
    Logger = logger
};
var client = new KoboldSharpClient(options);
```

## Testing
To run the tests:
```bash
dotnet test
```

Configure test environment:
```bash
export KOBOLDCPP_BASE_URL="http://localhost:5001"
```

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing
Contributions are welcome! Please feel free to submit a Pull Request.

## Support
For issues and feature requests, please use the [GitHub issues page](https://github.com/SpongeEngine/KoboldSharp/issues).
