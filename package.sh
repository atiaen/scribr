echo "Building and packaging Scriber"
pwd

## declare an array variable
declare -a runtimes=("linux-x64" "win-x64" "osx-x64")

## now loop through the above array
for i in "${runtimes[@]}"
do
  dotnet publish -r "$i"
  cp -a fonts/ releases/"$i"/publish/
  (mkdir -p releases/"$i"/publish/bin/"$i"/; cp -a bin/"$i"/* releases/"$i"/publish/bin/"$i"/)
  (cd releases/"$i"/publish/; zip -r ../../../releases/Scribr_"$i".zip ./*)
done