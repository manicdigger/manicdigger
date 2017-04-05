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
mono CiTo.exe -D CITO -D JS -D JSTA -l js-ta -o cito/output/JsTa/ManicDigger.js $(ls ManicDiggerLib/Client/*.ci.cs) $(ls ManicDiggerLib/Client/Mods/*.ci.cs) $(ls ManicDiggerLib/Client/MainMenu/*.ci.cs) $(ls ManicDiggerLib/Client/Misc/*.ci.cs) $(ls ManicDiggerLib/Client/SimpleServer/*.ci.cs) $(ls ManicDiggerLib/Common/*.ci.cs) Packet.Serializer.ci.cs

# Copy skeleton files
cp -r cito/platform/JsTa/* cito/output/JsTa/
