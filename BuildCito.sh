# Generate ProtoBuf files
mono CodeGenerator.exe Packet.proto

# Clean cito output directory
rm -r cito/output

# Create output directories
mkdir cito/output
mkdir cito/output/JsTa

# Compile JavaScript files
mono CitoAssets.exe data Assets.ci.cs
mono CiTo.exe -D CITO -D JS -D JSTA -l js-ta -o cito/output/JsTa/Assets.js Assets.ci.cs
mono CiTo.exe -D CITO -D JS -D JSTA -l js-ta -o cito/output/JsTa/ManicDigger.js $(ls ManicDigger.Common/Client/*.ci.cs) $(ls ManicDigger.Common/Client/Mods/*.ci.cs) $(ls ManicDigger.Common/Client/MainMenu/*.ci.cs) $(ls ManicDigger.Common/Client/Misc/*.ci.cs) $(ls ManicDigger.Common/Client/SimpleServer/*.ci.cs) $(ls ManicDigger.Common/Client/UI/*.ci.cs) $(ls ManicDigger.Common/Client/UI/Screens/*.ci.cs) $(ls ManicDigger.Common/Client/UI/Widgets/*.ci.cs) $(ls ManicDigger.Common/Common/*.ci.cs) Packet.Serializer.ci.cs

# Copy skeleton files
cp -r cito/platform/JsTa/* cito/output/JsTa/
