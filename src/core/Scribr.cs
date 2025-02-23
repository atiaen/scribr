using rlImGui_cs;
using ImGuiNET;
using static Raylib_cs.Raylib;
using System.Numerics;
using Raylib_cs;
using Scriber;
using System.Text.Json;
using System.Collections;
using FFMpegCore;
using FFMpegCore.Pipes;
using System.Diagnostics;
using System.IO.Compression;


public class Scribr
{
    string folderLocation = "";

    Settings settings;

    //Queue Items are stored in the following format of {filePath}|{fileName}|{transcribingStatus e.g true or false}
    static List<string> items = new List<string>();
    public void Start()
    {
        string fileName = "settings.json";

        if (!File.Exists("settings.json"))
        {
            var newSettings = new Settings
            {
                folderLocation = "",
                currentModel = "vosk",
                voskModelType = ""
            };
            var options = new JsonSerializerOptions { WriteIndented = true };

            string jsonString = JsonSerializer.Serialize(newSettings, options);
            File.WriteAllText(fileName, jsonString);
        }
        else
        {
            string jsonString = File.ReadAllText(fileName);
            settings =
              JsonSerializer.Deserialize<Settings>(jsonString);
            folderLocation = settings.folderLocation;
        }



        SetConfigFlags(ConfigFlags.Msaa4xHint | ConfigFlags.VSyncHint);
        InitWindow(Program.screenWidth, Program.screenHeight, "Scribr");
        SetTargetFPS(144);

        rlImGui.Setup(true);
    }


    public void Run()
    {

        while (!WindowShouldClose())
        {
            BeginDrawing();

            ClearBackground(Color.DarkGray);
            rlImGui.Begin();

            //Imgui Window Constraints seperate from Raylib Window Size etc
            ImGui.SetNextWindowSizeConstraints(new Vector2(GetScreenWidth(), GetScreenHeight()), new Vector2(GetScreenWidth(), GetScreenHeight()));


            //Imgui window settings where we actually start to draw the window
            if (ImGui.Begin("Scribr", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.MenuBar))
            {
                ImGui.SetWindowSize(new Vector2(GetScreenWidth(), GetScreenHeight()));
                // Create a menu bar
                if (ImGui.BeginMenuBar())
                {
                    if (ImGui.MenuItem("Open output folder") && !string.IsNullOrWhiteSpace(folderLocation))
                    {
                        Process.Start(new ProcessStartInfo()
                        {
                            FileName = folderLocation,
                            UseShellExecute = true,
                            Verb = "open"
                        });
                    }


                    ImGui.EndMenuBar();
                }

            }

            // if (importOpen)
            // {
            //     ImGui.OpenPopup("import-model");

            // }


            //Top part where we show the users selected output folder
            ImGui.PushTextWrapPos(ImGui.GetCursorPos().X + 400);
            ImGui.Text("Output Folder: " + (string.IsNullOrWhiteSpace(folderLocation) ? "No Folder Selected Yet" : folderLocation));
            ImGui.PopTextWrapPos();
            if (ImGui.Button("Select an output folder."))
            {
                ImGui.OpenPopup("save-folder");

            }

            ImGui.Dummy(new Vector2(0.0f, 5.0f));
            ImGui.Separator();
            //Button that opens the select audio file pop up/filepicker
            ImGui.Text("Add an audio File: ");
            if (ImGui.Button("Click to add a file"))
            {
                if (string.IsNullOrWhiteSpace(folderLocation))
                {
                    ImGui.OpenPopup("no_output_folder");

                }
                else
                {
                    ImGui.OpenPopup("save-file");


                }

            }
            ImGui.Dummy(new Vector2(0.0f, 5.0f));
            ImGui.Separator();

            ImGui.Text("Add/Overwrite existing model");
            if (ImGui.Button("Click here to import a model"))
            {
                ImGui.OpenPopup("import-model");

            }

            if (ImGui.IsItemHovered(ImGuiHoveredFlags.ForTooltip))
                ImGui.SetTooltip("Importing a new vosk model will overwrite any existing model you have.\nThis action cannot be undone!");

            ImGui.Dummy(new Vector2(0.0f, 5.0f));
            ImGui.Separator();

            //Draw list of add files for transcribing in a listbox
            if (ImGui.BeginListBox("##", new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetContentRegionAvail().Y - 80)))
            {
                for (int n = 0; n < items.Count; n++)
                {

                    //Here we just simply split the queue item string into separate variables for individual us
                    var fullText = items[n];
                    var all = Helpers.BreakdownQueueItem(fullText);


                    ImGui.PushTextWrapPos(ImGui.GetCursorPos().X + 280);
                    ImGui.Text(all[1]);
                    ImGui.PopTextWrapPos();

                    ImGui.SameLine(0.0f, ImGui.GetStyle().ItemInnerSpacing.X + 40);

                    //Show the loading indicator if any of the statues change to true
                    if (all[2] == "true")
                    {
                        ImGui.Text($"Transcribing {"|/-\\"[(int)(ImGui.GetTime() / 0.05f) & 3]}");
                    }
                    else if (all[2] == "done")
                    {
                        ImGui.Text("Completed");
                    }
                    else
                    {
                        //Show some text if not
                        ImGui.PushTextWrapPos(ImGui.GetCursorPos().X + 130);
                        ImGui.Text("Currently not transcribing");
                        ImGui.PopTextWrapPos();

                    }
                }
                ImGui.EndListBox();
            }


            ImGui.Dummy(new Vector2(0.0f, 5.0f));
            ImGui.Separator();


            var beginDisabled = false;
            if (!Directory.Exists("model"))
            {
                ImGui.BeginDisabled();
                beginDisabled = true;
            }


            //Main area where the magic happens
            if (ImGui.Button("Start", new Vector2(ImGui.GetContentRegionAvail().X * 0.5f, 0.0f)))
            {

                if (items.Count <= 0)
                {
                    ImGui.OpenPopup("no_items_warning");

                }

                if (string.IsNullOrWhiteSpace(folderLocation))
                {
                    ImGui.OpenPopup("no_output_folder");

                }

                if (items.Count > 0 && !string.IsNullOrWhiteSpace(folderLocation))
                {
                    List<Task> tasks = new List<Task>();

                    for (int i = 0; i < items.Count; i++)
                    {
                        var queueItem = Helpers.BreakdownQueueItem(items[i]);
                        queueItem[2] = "true";
                        var replaceValue = $"{queueItem[0]}|{queueItem[1]}|{queueItem[2]}";

                        Directory.CreateDirectory($"{folderLocation}/{queueItem[1]}");
                        items[i] = replaceValue;

                        // Console.WriteLine($"{folderLocation}/{queueItem[1]}");

                        Task tk = Task
                        .Run(() =>
                        {
                            Console.WriteLine("Inside first task (path is): " + queueItem[0]);
                            var result = VoskHandler.ReadFile2(queueItem[0]);
                            // Console.WriteLine("Still inside first final result is: " + result);
                            return result;
                        })
                        .ContinueWith(val =>
                        {
                            Console.WriteLine("Inside second task (result is): " + val.Result);
                            Write(val.Result, $"{folderLocation}/{queueItem[1]}");
                        }, TaskContinuationOptions.OnlyOnRanToCompletion)
                        .ContinueWith((a) =>
                        {

                            for (int i = 0; i < items.Count; i++)
                            {
                                var queueItem = Helpers.BreakdownQueueItem(items[i]);
                                var replaceValue = $"{queueItem[0]}|{queueItem[1]}|done";
                                items[i] = replaceValue;

                            }

                        }, TaskContinuationOptions.OnlyOnRanToCompletion);

                        tasks.Add(tk);

                    }

                    try
                    {
                        Task.Run(async () => await RunAsyncTasks(tasks.ToArray()));
                    }
                    catch (TaskCanceledException e)
                    {
                        Console.WriteLine(e);

                    }


                }


            }
            if (beginDisabled)
            {
                if (ImGui.IsItemHovered(ImGuiHoveredFlags.ForTooltip))
                    ImGui.SetTooltip("You currently do not have any vosk models installed.\nPlease first install a model to continue");

                ImGui.EndDisabled();


            }



            ImGui.SameLine();

            if (ImGui.Button("Clear", new Vector2(ImGui.GetContentRegionAvail().X, 0.0f)))
            {
                items.Clear();
            }

            // Vector2 center = ImGui.GetMainViewport().GetCenter();
            // ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));

            // if (ImGui.BeginPopupModal("Import Warning", ref warningOpen, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.AlwaysAutoResize))
            // {
            //     ImGui.Text("Importing a new vosk model will overwrite any existing model you have.\n This action cannot be undone!");
            //     ImGui.Separator();

            //     //static int unused_i = 0;
            //     //ImGui.Combo("Combo", ref unused_i, "Delete\0Delete harder\0");

            //     // bool dontAskMeNextTime = false;
            //     // ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0, 0));
            //     // ImGui.Checkbox("Don't ask me next time", ref dontAskMeNextTime);
            //     // ImGui.PopStyleVar();

            //     if (ImGui.Button("Continue", new Vector2(ImGui.GetContentRegionAvail().X * 0.5f, 0)))
            //     {
            //         warningOpen = false;
            //         ImGui.CloseCurrentPopup();
            //         ImGui.OpenPopup("import-model");
            //         Console.WriteLine("Should open modal");

            //     }





            //     ImGui.SetItemDefaultFocus();
            //     ImGui.SameLine();

            //     if (ImGui.Button("Cancel", new Vector2(ImGui.GetContentRegionAvail().X, 0)))
            //     {
            //         warningOpen = false;
            //         ImGui.CloseCurrentPopup();
            //     }


            //     ImGui.EndPopup();

            // }

            var importOpen = true;

            if (ImGui.BeginPopupModal("import-model", ref importOpen, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove))
            {
                var picker = FilePicker.GetFilePicker(this, Path.Combine(Environment.CurrentDirectory), ".zip", false);
                if (picker.Draw())
                {
                    if (!Directory.Exists("model"))
                    {
                        // Directory.CreateDirectory("model");

                        ExtractModelFile(picker.SelectedFile);
                        // ZipFile.ExtractToDirectory(picker.SelectedFile, "model");

                    }
                    else
                    {
                        Helpers.ClearFolder("model");
                        ExtractModelFile(picker.SelectedFile);

                        // ZipFile.ExtractToDirectory(picker.SelectedFile, "model");

                    }
                    FilePicker.RemoveFilePicker(this);

                }
                ImGui.EndPopup();

            }

            //Magic 2.0 where we show the modal/window for selecting an audio file for the queue
            var isOpen = true;
            if (ImGui.BeginPopupModal("save-file", ref isOpen, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove))
            {
                //Checkout FIlePicker to understand how this works
                var picker = FilePicker.GetFilePicker(this, Path.Combine(Environment.CurrentDirectory), ".wav|.mp4|.mp3|.ogg", false);
                if (picker.Draw())
                {

                    //Where we add an item to the queue by deconstructing the full path,e.g getting the file name and path and adding a status
                    if (!items.Contains(picker.SelectedFile))
                    {
                        //You could probably shorten this section but I prefer it this way lol
                        //First step is to remove the . extension 
                        //Next is to attempt to get the last string from the full path so /some/thing/whatwewant
                        var firstSplit = picker.SelectedFile.Split(".")[0];
                        var fileExtension = picker.SelectedFile.Split(".")[1];
                        var secondSplit = firstSplit.Split("/");
                        var final = secondSplit[secondSplit.Count() - 1];

                        //What's happening here is basically file conversion since Vosk only uses .wav files I don't want to hassle too much about converting my files every time
                        // So why not just convert it myself into a folder you can see that's what the else statement is doing using ffmpeg installed on the machine
                        //EDIT: I will most likely include a binary of ffmpeg with each release as not to hassle users with this.
                        //EDIT 2: I will probably just convert files regardless of type as all files need to be mono and I do not want to have to actively monitor this at all times.
                        if (fileExtension == "wav")
                        {
                            var queueItem = $"{picker.SelectedFile}|{final}|{false}";
                            // var mediaInfo = FFProbe.Analyse(picker.SelectedFile);
                            // Console.WriteLine(mediaInfo.AudioStreams.Count);
                            items.Add(queueItem);
                        }
                        else
                        {
                            Console.WriteLine("Attempting to convert file");
                            var audioInputStream = File.Open(picker.SelectedFile, FileMode.Open);

                            Directory.CreateDirectory("conversions");
                            var audioOutputStream = File.Open($"conversions/{final}.wav", FileMode.OpenOrCreate);


                            FFMpegArguments
                                    .FromPipeInput(new StreamPipeSource(audioInputStream))
                                    .OutputToPipe(new StreamPipeSink(audioOutputStream),
                                    options =>
                                    options.WithCustomArgument("-ac 1").ForceFormat("wav")).ProcessAsynchronously();


                            var queueItem = $"conversions/{final}.wav|{final}|{false}";
                            items.Add(queueItem);

                            //Todo: Popup isn't popping. fix might be to change from - to _ instead
                            ImGui.OpenPopup("file_conversion_finished");

                            Console.WriteLine("Converted file");

                        }



                        ImGui.OpenPopup("item_added_popup");

                    }
                    FilePicker.RemoveFilePicker(this);
                }

                ImGui.EndPopup();
            }


            //Magic 3.0 where we show the modal/window for selecting an output folder.
            var folderOpen = true;
            if (ImGui.BeginPopupModal("save-folder", ref folderOpen, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove))
            {
                var folderPicker = FilePicker.GetFilePicker(this, Path.Combine(Environment.CurrentDirectory), null, true);
                // folderPicker = 
                if (folderPicker.Draw())
                {
                    // Console.WriteLine(folderPicker.SelectedFile);
                    folderLocation = folderPicker.SelectedFile;
                    settings.folderLocation = folderPicker.SelectedFile;

                    var options = new JsonSerializerOptions { WriteIndented = true };
                    string jsonString = JsonSerializer.Serialize(settings, options);
                    File.WriteAllText("settings.json", jsonString);


                    FilePicker.RemoveFilePicker(this);
                }
                // Console.WriteLine(items);
                ImGui.EndPopup();
            }

            var progressModal = true;




            DrawPopupMenus();

            ImGui.End();
            rlImGui.End();
            EndDrawing();
        }

    }

    static void Write(string output, string outputFileLocation)
    {
        var fileNameLocation = outputFileLocation + "/output.txt";
        var finalText = Helpers.ReadJSONString(output, "text");
        Helpers.WriteFileToPath(fileNameLocation, finalText);
    }

    static async Task RunAsyncTasks(Task[] tasks)
    {
        await Task.WhenAll(tasks);
    }

    void DrawPopupMenus()
    {
        if (ImGui.BeginPopup("no_items_warning"))
        {
            ImGui.Text("You do not have any audio files in the current queue!!");
            ImGui.EndPopup();
        }
        if (ImGui.BeginPopup("no_output_folder"))
        {
            ImGui.Text("You have not selected an output folder!!");
            ImGui.EndPopup();
        }

        if (ImGui.BeginPopup("file_conversion_finished"))
        {
            ImGui.Text($"Converted file to wav");

            ImGui.EndPopup();
        }

        if (ImGui.BeginPopup("item_added_popup"))
        {
            ImGui.Text($"Added item to queue");

            ImGui.EndPopup();
        }
    }

    void ExtractModelFile(string path)
    {
        using (ZipArchive archive = ZipFile.OpenRead(path))
        {
            ZipArchiveEntry entry = archive.Entries[0];
            var extractPath = Path.GetFullPath("model");
            entry.ExtractToFile(Path.Combine(extractPath, entry.Name));
            Console.WriteLine(entry.FullName);
            //entry.ExtractToFile(extractPath);

        }
    }

    public void Stop()
    {
        rlImGui.Shutdown();

        CloseWindow();

    }
}