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


public class Scribr
{
    string folderLocation = "";

    //Queue Items are stored in the following format of {filePath}|{fileName}|{transcribingStatus e.g true or false}
    static List<string> items = new List<string>();
    public void Start()
    {


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
            if (ImGui.Begin("Scribr", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoTitleBar))
            {
                ImGui.SetWindowSize(new Vector2(GetScreenWidth(), GetScreenHeight()));

            }

            //Top part where we show the users selected output folder
            ImGui.PushTextWrapPos(ImGui.GetCursorPos().X + 400);
            ImGui.Text("Output Folder: " + (string.IsNullOrWhiteSpace(folderLocation) ? "No Folder Selected Yet" : folderLocation));
            ImGui.PopTextWrapPos();

            ImGui.SameLine();
            if (ImGui.Button("Select an output folder."))
            {
                ImGui.OpenPopup("save-folder");

            }

            // ImGui.PushTextWrapPos(ImGui.GetCursorPos().X + 400);
            // ImGui.PopTextWrapPos();
            //ImGui.SameLine();

            //Button that opens the select audio file pop up/filepicker
            ImGui.Text("Add an audio File: ");
            if (ImGui.Button("Click to add a file"))
            {
                ImGui.OpenPopup("save-file");

            }

            //Draw list of add files for transcribing in a listbox
            if (ImGui.BeginListBox("##", new Vector2(GetScreenWidth() - 20, GetScreenHeight() - 125)))
            {
                for (int n = 0; n < items.Count; n++)
                {

                    //Here we just simply split the queue item string into separate variables for individual us

                    var fullText = items[n];
                    var all = Helpers.BreakdownQueueItem(fullText);
                    // var fileName = all[1];
                    // var filePath = all[0];
                    // var status = all[2];


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



            //Main area where the magic happens
            if (ImGui.Button("Start"))
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
                    // Console.WriteLine(r.IsCanceled);
                    // Console.WriteLine(r.IsCompleted);
                    // Console.WriteLine(r.IsCompletedSuccessfully);




                }

                // ThreadPool.QueueUserWorkItem(Worker, null);

                // var output = VoskHandler.ReadFile2(fileLocation);
                // var fileNameLocation = folderLocation + "/output.txt";

                // var finalText = Helpers.ReadJSONString(output, "text");
                // Helpers.WriteFileToPath(fileNameLocation, finalText);

            }

            ImGui.SameLine();

            if (ImGui.Button("Clear"))
            {
                items.Clear();
            }

            //Magic 2.0 where we show the modal/window for selecting an audio file for the queue
            var isOpen = true;
            if (ImGui.BeginPopupModal("save-file", ref isOpen, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove))
            {
                //Checkout /utils/FIlePicker to understand how this works
                var picker = FilePicker.GetFilePicker(this, Path.Combine(Environment.CurrentDirectory), ".wav|.mp4|.mp3|.m4a|.m3a|.ogg", false);
                if (picker.Draw())
                {
                    // Console.WriteLine(picker.SelectedFile);

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
                        // So why not just convert it myself into a folder users can see that's what the else statement is doing using ffmpeg installed on the machine

                        if (fileExtension == "wav")
                        {
                            var queueItem = $"{picker.SelectedFile}|{final}|{false}";
                            // Console.WriteLine(queueItem);
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
                                .OutputToPipe(new StreamPipeSink(audioOutputStream), options =>
                                    options
                                    .ForceFormat("wav"))
                                .ProcessAsynchronously();

                            var queueItem = $"conversions/{final}.wav|{final}|{false}";
                            items.Add(queueItem);
                            ImGui.OpenPopup("file-conversion-finished");
                            Console.WriteLine("Converted file");




                        }



                        ImGui.OpenPopup("item-added-popup");

                    }

                    // fileLocation = picker.SelectedFile;
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
                    FilePicker.RemoveFilePicker(this);
                }
                // Console.WriteLine(items);
                ImGui.EndPopup();
            }

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

            if (ImGui.BeginPopup("file-conversion-finished"))
            {
                ImGui.Text($"Converted file to wav");

                ImGui.EndPopup();
            }

            if (ImGui.BeginPopup("item-added-popup"))
            {
                ImGui.Text($"Added item to queue");

                ImGui.EndPopup();
            }
            ImGui.End();
            rlImGui.End();
            EndDrawing();
        }
    }

    static string Transcribe(string audioFileLocation)
    {
        return VoskHandler.ReadFile2(audioFileLocation);

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

    // static void Worker(object state)
    // {

    //     for (int i = 0; i < items.Count; i++)
    //     {
    //         var queueItem = Helpers.BreakdownQueueItem(items.ToArray()[i]);
    //         queueItem[2] = "true";
    //         var replaceValue = $"{queueItem[0]}|{queueItem[1]}|{queueItem[2]}";

    //         var output = VoskHandler.ReadFile2(queueItem[0]);
    //         var fileNameLocation = outputLocation + "/output.txt";

    //         var finalText = Helpers.ReadJSONString(output, "text");
    //         Helpers.WriteFileToPath(fileNameLocation, finalText);

    //     }
    // }

    public void StartAndRunQueue()
    {

    }


    public void Stop()
    {
        rlImGui.Shutdown();

        CloseWindow();

    }
}