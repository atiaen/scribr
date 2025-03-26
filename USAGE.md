# Scribr Usage Guide

## Getting Started  

When you run Scribr, you'll see two main tabs:  

- **Workspace** – This is where you load audio files and start transcriptions.  
- **Settings** – Configure output directories and select models.  

### Initial Setup  

1. **Set an Output Folder**  
   - Navigate to the **Settings** tab.  
   - Click the button to select an **output folder**.  
   - This folder is crucial, as Scribr stores all converted and transcribed files here.  

2. **Select a Transcription Model**  
   - Choose between **Vosk** or **Whisper** from the dropdown menu.  

### Using Vosk  

- Vosk models need to be downloaded manually.  
- Click **"Get Vosk Model"** in the top menu to download the model to your output folder.  
- Once downloaded, click **"Add/Overwrite Model"** and select the model from the output folder.  
- Scribr will import the model into a new folder named **`model/`** inside your Scribr directory.  

### Using Whisper  

- Whisper model downloads are handled automatically.  
- Clicking **"Get Whisper Model"** will download it into a folder called **`bin/`**.  
- If you need to change the model, download a compatible model from [GGML Whisper Models](https://ggml.ggerganov.com), rename it to `model`, and replace the existing `model.bin` file inside the `bin/` folder.  

**Note:** You don’t need to restart Scribr when changing models just ensure the files are named correctly.  

---

## Transcribing Audio  

1. **Add an Audio File**  
   - Go to the **Workspace** tab.  
   - Click the button labeled **"Add an audio file"** to open a file dialog.  
   - Select a supported file format (`.mp4`, `.mp3`, `.wav`, `.ogg`).  

2. **Start Transcription**  
   - Click the **"Start"** button at the bottom.  
   - A loading message will appear while processing.  
   - Once completed, the text will change to "Completed".  

3. **Accessing the Output**  
   - Click **"Open Output Folder"** (top menu) to navigate to your output directory.  
   - Scribr creates a subfolder named after the transcribed file.  
   - Inside, you’ll find a text file named **`output.txt`** containing the transcription.  

---

## Additional Notes  

- You can switch between **Vosk** and **Whisper** at any time without restarting the application.  
- All transcriptions and converted files are stored in the output folder you specify.  
- If you run into issues, ensure that models are correctly placed and named as required.  

---