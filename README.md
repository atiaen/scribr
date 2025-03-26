# Scribr

## Overview

**Scribr** is a lightweight, open-source, cross-platform native desktop application for audio transcription. Built in C#, it leverages the power of **Alpha Cephei’s Vosk Toolkit** and **OpenAI’s Whisper** to provide fast and accurate transcription. Scribr is designed to be efficient, requiring minimal system resources while offering flexibility in model selection and language support.

## Features

- **Supports Multiple Audio Formats:** `.mp4`, `.mp3`, `.wav`, and `.ogg` (via bundled FFmpeg).
- **Offline and Privacy-Focused:** No cloud processing; all transcriptions run locally.
- **Multilingual Support:** Supports a variety of languages powered by Vosk and Whisper.
- **Lightweight and Efficient:** No Electron-based overhead, keeping system resource usage minimal.
- **Custom Model Support:** Easily import and switch between transcription models.

---

## Installation

### Download Prebuilt Binary

You can download the latest prebuilt binaries from the [Releases](https://github.com/atiaen/scribr/releases) page.

Alternatively, to build from source, follow the steps below.

---

## Building from Source

### Prerequisites

Regardless of platform, ensure you have the .NET SDK installed and set up on your machine. [.NET SDK](https://dotnet.microsoft.com/en-us/download)

### Clone the Repository

```sh
git clone https://github.com/atiaen/scribr.git
cd scribr
```

### Setup

1. **Create a `bin` folder** at the root of the project directory.
2. **Download FFmpeg binaries** from [ffbinaries](https://ffbinaries.com/downloads). Scribr uses version 3.2 for size efficiency, but you can use any version you prefer.
3. **Extract the FFmpeg binaries** into the `bin` folder.

### Build the Application

To build the application locally:

```sh
dotnet build
```

Or, to run the application without building:

```sh
dotnet restore
dotnet run Program.cs
```

### Packaging the Application

To package the application for distribution, run the `package.sh` script in the root directory:

```sh
./package.sh
```

This will create a `releases/` folder containing zip files for Windows, Linux, and macOS. Make sure to include the FFmpeg binaries in the zip folder before distributing.

**Windows users:** You can run the script using WSL or convert it to a PowerShell script if necessary. macOS compatibility is untested but should work.

---

## Usage
For detailed instructions on how to use Scribr, see the [Usage Guide](USAGE.md).
---

## Roadmap / Upcoming Features

- **Live microphone input for real-time transcription**
- **In-app text editing for transcriptions**
- **Basic audio editing using FFmpeg**
- **Better handling of video files**
- **Improved thread management for resource efficiency**
- **Support for more local English accents (e.g., Nigerian, Ghanaian) by including user-contributed models**

## Reporting Issues

If you encounter any bugs or have feature requests, please open an issue on the [GitHub Issues](https://github.com/atiaen/scribr/issues) page.

---

## Acknowledgments

- **[Alpha Cephei Vosk Toolkit](https://github.com/alphacep/vosk-api)**
- **[OpenAI Whisper](https://github.com/openai/whisper)**
- **[Raylib](https://www.raylib.com/)**
- **[Dear ImGui](https://github.com/ocornut/imgui)**
- **[Cothman Sans](https://github.com/sebsan/Cotham)**
---

### Stay Updated

Follow the repository for updates, and feel free to join the discussion!

---

Enjoy using Scribr!

