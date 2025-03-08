echo "Building and packaging Scriber"
pwd

## declare an runtimes array
declare -a runtimes=("linux-x64" "win-x64" "osx-x64")

## now loop through the above array
for i in "${runtimes[@]}"
do
  ##Actually build process
  dotnet publish -r "$i"
  ## Copy fonts folder
  cp -a fonts/ releases/"$i"/publish/

  ## Copy Imgui.ini
  cp imgui.ini releases/"$i"/publish/
  
  ## Make releases dir if not exists and copy ffmpeg binaries to respective runtime environment
  (mkdir -p releases/"$i"/publish/bin/"$i"/; cp -a bin/"$i"/* releases/"$i"/publish/bin/"$i"/)
  ## Need to delete/rm all runtimes that are not the current one from releases/i/runtimes to reduce binary size to <= 80mb
  if [ "$i" = "osx-x64" ]
  then
    (mv releases/"$i"/publish/runtimes/macos-x64 ..; rm -rf releases/"$i"/publish/runtimes/*; mv ../macos-x64 releases/"$i"/publish/runtimes/)
  else
    (mv releases/"$i"/publish/runtimes/"$i" ..; rm -rf releases/"$i"/publish/runtimes/*; mv ../"$i" releases/"$i"/publish/runtimes/)
  fi
  ##Change directory into publish folder and zip it
  (cd releases/"$i"/publish/; zip -r ../../../releases/Scribr_"$i".zip ./*)
done